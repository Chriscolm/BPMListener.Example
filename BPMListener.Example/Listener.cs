using BPMListener.Example.Execution;
using BPMListener.Example.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BPMListener.Example
{
    public class Listener : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly IEnumerable<TaskConfig> _taskMap;
        private readonly Watchdog _watchdog;
        private readonly int _defaultLockDuration;
        private readonly ILogger _logger;
        private readonly Func<string, ILogger> _loggerFactory;
        private bool _doProcessing;
        private bool _isDisposed;

        public Listener(IEnumerable<TaskConfig> taskMap, Watchdog watchdog, int defaultLockDuration, Func<string, ILogger> loggerFactory)
        {
            _taskMap = taskMap;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8787/wakeup/");
            _listener.Prefixes.Add("http://localhost:8787/message/");
            _watchdog = watchdog;
            _defaultLockDuration = defaultLockDuration;
            _logger = loggerFactory.Invoke(nameof(Listener));
            _loggerFactory = loggerFactory;
        }        

        internal async Task ListenAsync()
        {
            _listener.Start();
            _doProcessing = true;
            while (_doProcessing)
            {
                var context = await _listener.GetContextAsync();
                var code = await ProcessRequestAsync(context);
                _logger.LogInformation($"Processed request with status code {code}");
            }            
        }

        internal async Task<int> ProcessRequestAsync(HttpListenerContext context)
        {
            _logger.LogDebug($"Received Request - {context.Request.Url}");
            
            var code = 404;
            var path = context.Request.Url.LocalPath.ToLowerInvariant();
            if(path == "/wakeup/")
            {
                code = 204;
                var wf = new Workflow(_taskMap, _watchdog, _defaultLockDuration, _loggerFactory);
                wf.BeginWorkflow();
            }
            if (path == "/message/")
            {
                code = 204;
                var buffer = new byte[context.Request.ContentLength64];
                await context.Request.InputStream.ReadAsync(buffer);
                var s = Encoding.UTF8.GetString(buffer);
                _logger.LogWarning(s);
            }
            using var response = context.Response;
            response.StatusCode = code;
            return code;
        }

        internal void Stop()
        {
            _doProcessing = false;
            _listener.Stop();
        }

        private void Dispose(bool isDisposing)
        {
            if(isDisposing && !_isDisposed)
            {
                _isDisposed = true;
                Stop();
                if (_listener is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
