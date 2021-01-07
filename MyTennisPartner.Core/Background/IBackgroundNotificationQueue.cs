using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyTennisPartner.Core.Background
{
    /// <summary>
    /// background task queue interface, taken from this MS documentation article:
    /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2
    /// </summary>
    public interface IBackgroundNotificationQueue
    {
        void QueueBackgroundNotificationWorkItem(NotificationWorkItem workItem);

        Task<NotificationWorkItem> DequeueAsync();
    }
}
