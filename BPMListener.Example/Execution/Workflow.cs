using BPMListener.Example.Events;
using BPMListener.Example.Exceptions;
using BPMListener.Example.Models;
using BPMListener.Example.Requests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMListener.Example.Execution
{
    public class Workflow
    {
        private readonly IEnumerable<TaskConfig> _taskMap;
        private readonly Watchdog _watchdog;
        private readonly int _defaultLockDuration;
        private readonly Func<string, ILogger> _loggerFactory;
        private readonly ILogger _logger;

        public Workflow(IEnumerable<TaskConfig> taskMap, Watchdog watchdog, int defaultLockDuration, Func<string, ILogger> loggerFactory)
        {
            _taskMap = taskMap;
            _watchdog = watchdog;
            _defaultLockDuration = defaultLockDuration;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.Invoke(nameof(Workflow));
        }

        public void BeginWorkflow()
        {
            Task.Run(async () => await BeginWorkflowAsync());
        }

        private async Task BeginWorkflowAsync()
        {
            _logger.LogInformation("Begin Workflow");
            var tasks = Next();
            await foreach (var task in tasks)
            {
                if(task.WorkerId == null)
                {
                    var workerId = Guid.NewGuid().ToString();
                    var topicName = task.TopicName;                    
                    _logger.LogInformation($"Fetch task {topicName}");
                    try
                    {
                        await FetchAndLockAsync(workerId, topicName, _defaultLockDuration);                        
                    }
                    catch (ExecutionException ex)
                    {
                        _logger.LogError($"Failed to fetch task {topicName}", ex);
                        continue;
                    }
                    var config = _taskMap.Single(task => task.Topic == topicName);
                    var processor = new Processor(config, _loggerFactory);
                    processor.TaskStarted += OnTaskStarted;
                    processor.TaskExited += OnTaskExcited;
                    processor.Run(task, workerId);
                }
                else
                {
                    _logger.LogInformation($"Skip already running task {task.TopicName} with workerId {task.WorkerId}");
                }                
            }
            _logger.LogInformation("End Workflow");
        }

        private void OnTaskStarted(object sender, TaskStartedEventArgs e)
        {            
            _watchdog.Add(e.TaskIdentifier);
        }

        private void OnTaskExcited(object sender, TaskExitedEventArgs e)
        {
            if(sender is Processor processor)
            {
                processor.TaskExited -= OnTaskExcited;
                processor.TaskStarted -= OnTaskStarted;
                _watchdog.Remove(new TaskIdentifier(e.Task.Id, e.Task.WorkerId, e.Task.ProcessInstanceId, e.Task.ProcessDefinitionId));
            }
            Task.Run(async() => {
                try
                {
                    await CompleteAsync(e.ExitCode, e.Message, e.Task);
                }
                catch (ExecutionException ex)
                {
                    _logger.LogError($"Failed to complete task {e.Task.TopicName}", ex);
                }
            });
        }

        private async IAsyncEnumerable<ExternalTask> Next()
        {
            var request = new ExternalTaskRequest();
            var result = await request.GetAsync();
            foreach (var task in result)
            {
                yield return task;
            }
        }

        private async Task<ExternalTask> FetchAndLockAsync(string workerId, string topic, int lockDuration)
        {
            try
            {
                var request = new FetchAndLockRequest();
                var result = await request.PostAsync(workerId, topic, lockDuration);
                return result.SingleOrDefault();
            }
            catch (Exception ex)
            {
                throw new ExecutionException("Execution failed", ex);
            }
        }

        private async Task CompleteAsync(int exitCode, string message, ExternalTask task)
        {
            var dic = new Dictionary<string, Variable>();
            if(exitCode != 0)
            {
                dic.Add("errorCode", new Variable()
                {
                    Value = exitCode
                });
                dic.Add("errorMessage", new Variable()
                {
                    Value = message
                });
                dic.Add("hasError", new Variable()
                {
                    Value = exitCode != 0 ? "true" : "false"
                });
            }
            try
            {
                var request = new CompleteRequest();
                await request.PostAsync(task, dic);                
            }
            catch (Exception ex)
            {
                throw new ExecutionException($"Completion of task {task.TopicName} with workerId {task.WorkerId} failed with exit code {exitCode}", ex);
            }
        }
    }
}
