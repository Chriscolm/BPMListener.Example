using BPMListener.Example.Events;
using BPMListener.Example.Exceptions;
using BPMListener.Example.Requests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMListener.Example.Execution
{
    public class Watchdog
    {
        private readonly HashSet<Watcher> _watchers;
        private readonly ExtendLockRequest _extendLockRequest;
        private readonly double _timerIntervalInMs;
        private readonly int _lockExtentInMs;
        private readonly ILogger _logger;

        public Watchdog(ExtendLockRequest extendLockRequest, int lockExtentInMs, double timerIntervalInMs, Func<string, ILogger> loggerFactory)
        {
            _extendLockRequest = extendLockRequest;
            _watchers = new();
            _timerIntervalInMs = timerIntervalInMs;
            _lockExtentInMs = lockExtentInMs;
            _logger = loggerFactory.Invoke(nameof(Workflow));
        }

        public void Add(TaskIdentifier taskIdentifier)
        {
            var watcher = new Watcher(taskIdentifier, _lockExtentInMs, _timerIntervalInMs);
            watcher.LockDurationExpiring += async (o, e) => await OnLockDurationExpiring(o, e);
            _watchers.Add(watcher);
        }

        public void Remove(TaskIdentifier taskIdentifier)
        {
            var w = _watchers.SingleOrDefault(p => p.TaskIdentifier.Equals(taskIdentifier));
            if(w != null)
            {
                w.LockDurationExpiring -= async (o, e) => await OnLockDurationExpiring(o, e);
                w.Stop();
                _watchers.Remove(w);
            }            
        }

        private async Task OnLockDurationExpiring(object sender, LockDurationExpiringEventArgs e)
        {
            try
            {
                _logger.LogInformation("Try extend lock duration");
                await _extendLockRequest.PostAsync(e.TaskIdentifier.TaskId, e.TaskIdentifier.WorkerId, e.LockExtentInMs);
                _logger.LogInformation($"Extended lock duration by {e.LockExtentInMs}");
            }
            catch (ExecutionException ex)
            {
                _logger.LogError("Failed to extend lock", ex);
            }
        }
    }
}
