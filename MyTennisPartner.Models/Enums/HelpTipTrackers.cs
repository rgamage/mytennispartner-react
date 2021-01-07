using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.Enums
{
    /// <summary>
    /// flags to track which help tips user has dismissed
    /// </summary>
    [Flags]
    public enum HelpTipTrackers
    {
        None = 0,
        DashboardMatchOpportunityExpand = 1 << 0,
        DashboardCommittedMatchDecline = 1 << 1
    }
}
