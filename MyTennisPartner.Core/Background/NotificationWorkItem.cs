using MyTennisPartner.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Core.Background
{
    /// <summary>
    /// class to hold all necessary details of a notification to user
    /// </summary>
    public class NotificationWorkItem
    {
        /// <summary>
        /// web application uri (e.g. 'https://mytennispartner.com')
        /// </summary>
        public string WebAppUri { get; set; }

        /// <summary>
        /// list of events to notify
        /// </summary>
        public IEnumerable<NotificationEvent> Events { get; set; }
    }
}
