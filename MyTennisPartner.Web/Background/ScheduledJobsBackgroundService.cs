using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Web.Managers;
using MyTennisPartner.Models;
using NCrontab.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyTennisPartner.Utilities;

namespace MyTennisPartner.Web.Background
{
    /// <summary>
    /// queued hosted service class, taken from ms article here:
    /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2
    /// </summary>
    public class ScheduledJobsBackgroundService : BackgroundService
    {
        /// <summary>
        /// logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Services, used to create a scope
        /// </summary>
        private IServiceProvider Services { get; }

        /// <summary>
        /// The next run list
        /// </summary>
        private DateTime[] _nextRunList;

        /// <summary>
        /// The rma background service application settings
        /// </summary>
        private readonly ScheduledJobSetting[] _scheduledJobsSettings;

        /// <summary>
        /// The schedule job list
        /// </summary>
        private readonly CrontabSchedule[] _scheduleJobs;

        /// <summary>
        /// hosting environment
        /// </summary>
        private readonly IHostingEnvironment _environment;

        /// <summary>
        /// background notification queue
        /// </summary>
        private readonly IBackgroundNotificationQueue _queue;

        public ScheduledJobsBackgroundService(IOptions<ScheduledJobSettings> scheduledJobSettings,
            ILogger<ScheduledJobsBackgroundService> logger, IServiceProvider services, IHostingEnvironment environment,
            IBackgroundNotificationQueue queue)
        {
            _environment = environment;
            Services = services;
            _logger = logger;
            _queue = queue;

            _scheduledJobsSettings = scheduledJobSettings?.Value?.Jobs;
            if (_scheduledJobsSettings != null)
            {
                _scheduleJobs = _scheduledJobsSettings
                    .Select(j => CrontabSchedule.Parse(j.SchedulerRunPeriod))
                    .ToArray();
            }
        }

        protected async override Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            if (_scheduleJobs is null) return;

            var dateLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ApplicationConstants.AppTimeZoneInfo);
            _logger.LogInformation("Scheduled Job Background Service is starting.");

            _nextRunList = _scheduleJobs.Select(j => j.GetNextOccurrence(dateLocal)).ToArray();
            bool quitDueToException = false;
            while (!cancellationToken.IsCancellationRequested && !quitDueToException)
            {
                dateLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ApplicationConstants.AppTimeZoneInfo);
                try
                {
                    using (var scope = Services.CreateScope())
                    {

                        // create a new scope for each queued item, because we need a scope for all
                        // of our dependencies, like db context, etc. that are injected into the notification service
                        var matchManager =
                            scope.ServiceProvider
                                .GetRequiredService(typeof(MatchManager)) as MatchManager;

                        var reservationManager =
                            scope.ServiceProvider
                                .GetRequiredService(typeof(ReservationManager)) as ReservationManager;

                        var lineManager =
                            scope.ServiceProvider
                            .GetRequiredService(typeof(LineManager)) as LineManager;

                        var i = 0;
                        foreach (var runTime in _nextRunList)
                        {
                            if (dateLocal >= runTime)
                            {
                                _logger.LogInformation($"{DateTime.Now} Calling Job {_scheduledJobsSettings[i].Description} from background service");

                                // do some work here
                                switch (i)
                                {
                                    case 0:
                                        // job 1 - Daily Court Reservation Job
                                        // so we are not making actual court reservations in multiple environments, limit this to PROD
                                        if (_environment.IsProduction())
                                        {
                                            _logger.LogInformation($"{DateTime.Now} executing job 1... court reservations");
                                            try
                                            {
                                                var result = await reservationManager.ReserveAllCourts();
                                                _logger.LogInformation($"job 1 result = {result}");
                                            }
                                            catch (Exception ex)
                                            {
                                                var message = $"ERROR reserving courts - {ex.Message} - {ex.InnerException?.Message}";
                                                _logger.LogError(message);
                                            }
                                        }
                                        break;
                                    case 1:
                                        // job 2 - Added Match Reminder Job
                                        _logger.LogInformation($"{DateTime.Now} executing job 2...");
                                        // send reminder e-mails to league members of matches coming up that they have not yet responded to,
                                        // and they are not in the line-up,
                                        // where the members have enabled reminder notifications
                                        try
                                        {
                                            var numMatches = matchManager.NotifyUpcomingMatches(3);
                                            _logger.LogInformation($"Found {numMatches} matches coming up for add match reminder e-mails");
                                            var host = _environment.IsProduction() ? "mytennispartner.com" :
                                                       _environment.IsStaging() ? "uat.mytennispartner.com" :
                                                       _environment.IsDevelopment() || _environment.EnvironmentName == "LocalDevelopment" ? "dev.mytennispartner.com" : "unknown-host";
                                            var nwi = new NotificationWorkItem
                                            {
                                                Events = matchManager.NotificationEvents,
                                                RequestHost = host,
                                                RequestScheme = "https"
                                            };
                                            _queue.QueueBackgroundNotificationWorkItem(nwi);
                                        }
                                        catch (Exception ex)
                                        {
                                            var message = $"ERROR in background Notification job - {ex.Message} - {ex.InnerException?.Message}";
                                            _logger.LogError(message);
                                        }

                                        break;
                                    case 2:
                                        // job 3 - hourly job to update target courts for upcoming auto-reserve matches that have not yet been reserved
                                        _logger.LogInformation($"{DateTime.Now} executing job 3... target court update");
                                        try
                                        {
                                            var result = await reservationManager.UpdateTargetCourts();
                                            _logger.LogInformation($"job 3 result = {result}");
                                        }
                                        catch (Exception ex)
                                        {
                                            var message = $"ERROR updating target courts - {ex.Message} - {ex.InnerException?.Message}";
                                            _logger.LogError(message);
                                        }
                                        break;
                                }

                                _logger.LogInformation($"Job {i+1} finished");
                                dateLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ApplicationConstants.AppTimeZoneInfo);
                                _nextRunList[i] = _scheduleJobs[i].GetNextOccurrence(dateLocal);
                            }
                            i++;
                        }
                    }
                    //5 seconds delay
                    await Task.Delay(5000, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                       $"Error occurred executing scheduled jobs background service");
                    quitDueToException = true;
                }
            }

            _logger.LogInformation("Scheduled Jobs Background Service is stopping.");
        }
    }
}
