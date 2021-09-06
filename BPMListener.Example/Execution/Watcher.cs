using BPMListener.Example.Events;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace BPMListener.Example.Execution
{
    public class Watcher
    {
        public event EventHandler<LockDurationExpiringEventArgs> LockDurationExpiring;
        
        private readonly Timer _timer;        
        private readonly int _lockExtentInMs;

        public TaskIdentifier TaskIdentifier;

        public Watcher(TaskIdentifier taskIdentifier, int lockExtentInMs, double timerIntervalInMs)
        {
            _timer = new Timer(timerIntervalInMs)
            {
                AutoReset = true,
                Enabled = true
            };
            _timer.Elapsed += async(o,e) => await OnTimerElapsed(o,e);
            TaskIdentifier = taskIdentifier;
            _lockExtentInMs = lockExtentInMs;
        }

        private async Task OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await Task.Run(() => LockDurationExpiring?.Invoke(this, new LockDurationExpiringEventArgs(TaskIdentifier, _lockExtentInMs)));
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
