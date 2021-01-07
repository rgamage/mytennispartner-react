using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MyTennisPartner.Web.Background
{
    /// <summary>
    /// background task queue class, taken from this MS documentation article:
    /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2
    /// </summary>
    public class BackgroundNotificationQueue : IBackgroundNotificationQueue, IDisposable
    {
        private readonly ConcurrentQueue<NotificationWorkItem> _workItems = new ConcurrentQueue<NotificationWorkItem>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundNotificationWorkItem(NotificationWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<NotificationWorkItem> DequeueAsync()
        {
            await _signal.WaitAsync();
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }

        /// <summary>
        /// implement Dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// implement disposal method
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_signal != null)
                {
                    _signal.Dispose();
                    _signal = null;
                }
            }
        }
    }
}
