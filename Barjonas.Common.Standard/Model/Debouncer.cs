using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Barjonas.Common.Model
{
    public class Debouncer
    {
        private readonly Timer _timer;
        private bool _isRunning = false;
        private bool _isDebouncing = false;
        public event EventHandler Execute;
        public Debouncer()
        {
            _timer = new Timer(TimerComplete, null, Timeout.Infinite, Timeout.Infinite);
        }

        public TimeSpan MinimumInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Raise the <see cref="Execute"/> event immediately unless less than the <see cref="MinimumInterval"/> has not passed since its last execution.
        /// In that case, ensure that the <see cref="Execute"/> event is fired immediately after the <see cref="MinimumInterval"/> has passed.
        /// </summary>
        public void TryExecute()
        {
            if (_isRunning)
            {
                _isDebouncing = true;
            }
            else
            {
                _isRunning = true;
                _timer.Change(MinimumInterval, Timeout.InfiniteTimeSpan);
                Execute?.Invoke(this, new EventArgs());
            }
        }

        private void TimerComplete(object state)
        {
            _isRunning = false;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            if (_isDebouncing)
            {
                _isDebouncing = false;
                Execute?.Invoke(this, new EventArgs());
            }
        }
    }
}
