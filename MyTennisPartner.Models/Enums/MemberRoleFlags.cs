using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MyTennisPartner.Models.Enums
{
    [Flags]
    public enum MemberRoleFlags
    {
        [Description("Player")]
        Player = 1,

        [Description("Pro/Instructor")]
        Pro = 2,

        [Description("Club Manager")]
        VenueAdmin = 4
    }
}
