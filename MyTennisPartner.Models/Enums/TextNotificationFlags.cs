using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MyTennisPartner.Models.Enums
{
    [Flags]
    public enum TextNotificationFlags
    {
        [Description("Added or Removed From Match")]
        AddRemoveFromMatch = 1,
        [Description("Court Change")]
        CourtChange = 2,
        [Description("Match Time/Location Changed or Cancelled")]
        MatchChangeOrCancel = 4,
        [Description("Sub Needed")]
        SubNeeded = 8,
        [Description("New Match Added")]
        MatchAdded = 16,
        [Description("Player Responded to Match")]
        PlayerResponded = 32,
        [Description("Urgent Alert")]
        UrgentAlert = 64,
        [Description("Default Notifications")]
        DefaultNotifications = SubNeeded | UrgentAlert
    }
}
