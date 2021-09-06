using BPMListener.Example.Events;
using BPMListener.Example.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BPMListener.Example.Execution
{
    public class Processor
    {
        public event EventHandler<TaskStartedEventArgs> TaskStarted;
        public event EventHandler<TaskExitedEventArgs> TaskExited;

        private readonly TaskConfig _taskConfig;
        private readonly ILogger _logger;

        public Processor(TaskConfig taskConfig, Func<string, ILogger> loggerFactory)
        {
            _taskConfig = taskConfig;
            _logger = loggerFactory.Invoke(nameof(Processor));
        }

        public void Run(ExternalTask task, string workerId)
        {
            var t = new TaskFactory().StartNew(() =>
            {
                _logger.LogInformation($"Run task {_taskConfig.Topic} with workerId {workerId}");
                task.WorkerId = workerId;                
                StartProcess(task);
            }, TaskCreationOptions.LongRunning);
            TaskStarted?.Invoke(this, new TaskStartedEventArgs(new TaskIdentifier(task.Id, workerId, task.ProcessInstanceId, task.ProcessDefinitionId)));
            t.Start();
            
        }

        private void StartProcess(ExternalTask task)
        { 
            var info = new System.Diagnostics.ProcessStartInfo()
            {
                Arguments = _taskConfig.Arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = _taskConfig.Executable
            };

            var p = new System.Diagnostics.Process()
            {
                StartInfo = info
            };

            p.OutputDataReceived += (o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    _logger.LogInformation(e.Data);
                }
            };
            p.ErrorDataReceived += (o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    _logger.LogWarning(e.Data);
                }
            };

            var code = -1;

            try
            {
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                _logger.LogError($"$Failed to start process [{info.FileName}]", ex);
            }
            finally
            {
                code = p?.ExitCode ?? -1;
                p?.Close();
            }            

            var m = $"External task {_taskConfig.Topic} exited with code {code}";            
            TaskExited?.Invoke(this, new TaskExitedEventArgs(code, m, task));         
        }
    }
}
