using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MyTennisPartner.Models.Enums
{
    [Flags]
    public enum EmailNotificationFlags
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
        [Description("All Notifications")]
        AllNotifications = 0x7FFFFFFF  // all flags set for Int32 enum
    }
}
