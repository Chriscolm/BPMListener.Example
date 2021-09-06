using BPMListener.Example.Execution;
using System;

namespace BPMListener.Example.Events
{
    public class TaskStartedEventArgs : EventArgs
    {
        public TaskIdentifier TaskIdentifier { get; }

        public TaskStartedEventArgs(TaskIdentifier taskIdentifier)
        {
            TaskIdentifier = taskIdentifier;
        }        
    }
}
