using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.Enums
{
    [Flags]
    public enum UserPreferenceFlags
    {
        None = 0,
        ShowDeclinedMatchesOnDashboard = 1 << 0
    }
}
