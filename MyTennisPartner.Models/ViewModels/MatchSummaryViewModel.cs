using MyTennisPartner.Models.Enums;
using MyTennisPartner.Models.Utilities;
using System;
using System.Collections.Generic;

namespace MyTennisPartner.Models.ViewModels
{
    /// <summary>
    /// class to hold basic sumamry info about match
    /// </summary>
    public class MatchSummaryViewModel
    {
        public int LeagueId { get; set; }
        public int MatchId { get; set; }
        public string LeagueName { get; set; }
        public string VenueName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime WarmupTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTimeLocal { get { return TimeZoneInfo.ConvertTimeFromUtc(StartTime, DataConstants.AppTimeZoneInfo); }}
        public DateTime WarmupTimeLocal { get { return TimeZoneInfo.ConvertTimeFromUtc(WarmupTime, DataConstants.AppTimeZoneInfo); } }
        public DateTime EndTimeLocal { get { return TimeZoneInfo.ConvertTimeFromUtc(EndTime, DataConstants.AppTimeZoneInfo); } }
        public int TotalAvailable { get; set; }
        public Availability Availability { get; set; }

        /// <summary>
        /// short summary of match, like "8/12/2019, 6pm, Beer Dogs"
        /// </summary>
        public string ShortSummary {  get
            {
                // return short summary of match details, like "8/12/2019, 6pm, Beer Dogs"
                return $"{StartTimeLocal.ToShortDateString()}, {StartTimeLocal.ToShortTimeString()}, {LeagueName}";
            } 
        }
        /// <summary>
        /// how many players do we expect per line (based on the play format of the league)
        /// </summary>
        public int ExpectedPlayersPerLine
        {
            get
            {
                return LeagueHelper.FullCourtSize(Format);
            }
        }

        /// <summary>
        /// type of event (singles, doubles, lesson, etc)
        /// </summary>
        public PlayFormat Format { get; set; }

    }
}
