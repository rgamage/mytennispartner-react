using MyTennisPartner.Models.Enums;
using System;

namespace MyTennisPartner.Models.ViewModels
{
    public class PlayerViewModel
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int LeagueId { get; set; }
        public int? LineId { get; set; }
        public int MatchId { get; set; }
        public int LeagueMemberId { get; set; }
        public Availability Availability { get; set; }
        public bool IsSubstitute { get; set; }
        public bool IsHomePlayer { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public bool IsCaptain { get; set; }
        public string HomeVenueName { get; set; }
        public int? SpareTimeMemberNumber { get; set; }
        public bool IsInLineup
        {
            get
            {
                // if we are linked to a line, then we are in a line-up
                return LineId > 0 || !string.IsNullOrEmpty(Guid);
            }
        }

        /// <summary>
        /// used to associate a player with a line, before the line has an id
        /// </summary>
        public string Guid { get; set; }

        public string AutoReserveVenues { get; set; }
        public bool CanReserveCourt { get; set; }

        /// <summary>
        /// player initials, e.g. JS for John Smith
        /// </summary>
        public string PlayerInitials { get
            {
                var first = FirstName?.Substring(0,1) ?? "?";
                var last = LastName?.Substring(0,1) ?? "?";
                return $"{first}{last}";
            } }

        public string FullName { get
            {
                return $"{FirstName} {LastName}";
            } }

        /// <summary>
        /// for UI, tracking of selections
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// for sorting by availability, in a way that makes sense (most available to least available)
        /// </summary>
        public int AvailabilitySort
        {
            get
            {
                switch (Availability)
                {
                    case Availability.Confirmed: return 0;
                    case Availability.Unknown: return 1;
                    case Availability.Unavailable: return 2;
                    default: return 3;
                }
            }
        }
    }
}
