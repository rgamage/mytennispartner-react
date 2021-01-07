
using MyTennisPartner.Models.Enums;
using System;

namespace MyTennisPartner.Models.ViewModels
{
    public class LeagueSearchViewModel
    {
        public int LeagueId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string VenueName { get; set; }
        public string VenueIconColor { get; set; }
        public Frequency MeetingFrequency { get; set; }
        public DayOfWeek MeetingDay { get; set; }
        public string MatchStartTime { get; set; }
        public bool IsCaptain { get; set; }
        public DateTime StartTimeLocal { get; set; }
        public int? OwnerMemberId { get; set; }
        public bool IsOwner { get; set; }
    }
}
