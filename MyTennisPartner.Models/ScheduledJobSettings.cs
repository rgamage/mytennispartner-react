using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Models
{
    public class ScheduledJobSetting
    {
        /// <summary>
        /// description of scheduled job
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// schedule run period, in crontab format
        /// Format:: Minute Hour Day(month) Month day(of week, Sun=0)
        /// </summary>
        public string SchedulerRunPeriod { get; set; }
    }

    /// <summary>
    /// container to hold scheduled jobs
    /// </summary>
    public class ScheduledJobSettings
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public ScheduledJobSetting[] Jobs { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
