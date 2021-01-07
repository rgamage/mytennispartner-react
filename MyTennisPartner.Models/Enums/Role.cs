using System.ComponentModel;

namespace MyTennisPartner.Models.Enums
{
    public enum Role
    {
        [Description("Player")]
        Player,

        [Description("Pro/Instructor")]
        Pro,

        [Description("Club Manager")]
        VenueAdmin,

        [Description("Admin")]
        Admin
    }
}
