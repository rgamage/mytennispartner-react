using MyTennisPartner.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Web.Background
{
    /// <summary>
    /// class to hold all necessary details of a notification to user
    /// </summary>
    public class NotificationWorkItem
    {
        /// <summary>
        /// web request scheme (e.g. 'https://')
        /// </summary>
        public string RequestScheme { get; set; }

        /// <summary>
        /// web request host (e.g. 'dev.mytennispartner')
        /// </summary>
        public string RequestHost { get; set; }

        /// <summary>
        /// list of events to notify
        /// </summary>
        public IEnumerable<NotificationEvent> Events { get; set; }
    }
}
