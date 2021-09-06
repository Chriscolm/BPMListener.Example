using BPMListener.Example.Models;
using System;

namespace BPMListener.Example.Events
{
    public class TaskExitedEventArgs : EventArgs
    {
        public int ExitCode { get; }
        public string Message { get; }
        public ExternalTask Task { get; }

        public TaskExitedEventArgs(int exitCode, string message, ExternalTask task)
        {
            ExitCode = exitCode;
            Message = message;
            Task = task;
        }
    }
}
