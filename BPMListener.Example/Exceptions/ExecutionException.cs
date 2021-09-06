using System;

namespace BPMListener.Example.Exceptions
{
    [Serializable]
    public class ExecutionException : Exception
    {
        public ExecutionException() { }
        public ExecutionException(string message) : base(message) { }
        public ExecutionException(string message, Exception inner) : base(message, inner) { }
        protected ExecutionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
