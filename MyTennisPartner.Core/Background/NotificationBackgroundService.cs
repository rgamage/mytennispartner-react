using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Core.Services.Notifications;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyTennisPartner.Core.Background
{
    /// <summary>
    /// queued hosted service class, taken from ms article here:
    /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2
    /// </summary>
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        public IServiceProvider Services { get; }

        public NotificationBackgroundService(IBackgroundNotificationQueue notificationQueue,
            ILoggerFactory loggerFactory, IServiceProvider services)
        {
            Services = services;
            NotificationQueue = notificationQueue;
            _logger = loggerFactory.CreateLogger<NotificationBackgroundService>();
        }

        public IBackgroundNotificationQueue NotificationQueue { get; }

        // if the application wants to kick off a background notification task,
        // they need to inject an instance of a IBackgroundNotificationQueue into their class,
        // then simply add a notification work item (NotificationWorkItem) to the queue, like this:
        // _queue.QueueBackgroundNotificationWorkItem(nwi);

        protected async override Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification Background Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await NotificationQueue.DequeueAsync();

                try
                {
                    using var scope = Services.CreateScope();
                    // create a new scope for each queued item, because we need a scope for all
                    // of our dependencies, like db context, etc. that are injected into the notification service
                    var notificationService =
                        scope.ServiceProvider
                            .GetRequiredService(typeof(NotificationService)) as NotificationService;

                    _logger.LogInformation("Calling notification method from background service");
                    await notificationService.NotifyAsync(workItem.Events, workItem.WebAppUri);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                       $"Error occurred executing background notification.");
                }
            }

            _logger.LogInformation("Background Notification Service is stopping.");
        }
    }
}
