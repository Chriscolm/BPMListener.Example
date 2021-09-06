using BPMListener.Example.Execution;
using System;

namespace BPMListener.Example.Events
{
    public class LockDurationExpiringEventArgs : EventArgs
    {
        public int LockExtentInMs { get; }
        public TaskIdentifier TaskIdentifier { get; }
        
        public LockDurationExpiringEventArgs(TaskIdentifier taskIdentifier, int lockExtentInMs)
        {
            LockExtentInMs = lockExtentInMs;
            TaskIdentifier = taskIdentifier;
        }
    }
}
