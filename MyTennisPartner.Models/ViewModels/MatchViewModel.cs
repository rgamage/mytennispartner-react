using MyTennisPartner.Models.Enums;
using MyTennisPartner.Models.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTennisPartner.Models.ViewModels
{
    public class MatchViewModel
    {
        public MatchViewModel()
        {
            Lines = new List<LineViewModel>();
            Players = new List<PlayerViewModel>();
        }

        [Required]
        public int MatchId { get; set; }

        /// <summary>
        /// session id, if exists
        /// </summary>
        public int? SessionId { get; set; }

        /// <summary>
        /// league id
        /// </summary>
        [Required]
        public int LeagueId { get; set; }

        /// <summary>
        /// start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// time to start warm-ups (needed for court reservations)
        /// </summary>
        public DateTime WarmupTime { get; set; }

        /// <summary>
        /// warm-up duration in minutes
        /// </summary>
        public int WarmUpDurationMinutes { get 
            {
                return (int)(StartTime - WarmupTime).TotalMinutes;
            }
            set
            {
                WarmupTime = StartTime - TimeSpan.FromMinutes(value);
            }
        }

        public int MatchDurationMinutes { 
            get
            {
                return (int)(EndTime - StartTime).TotalMinutes;
            }
            set
            {
                EndTime = StartTime + TimeSpan.FromMinutes(value);
            }
        }

        /// <summary>
        /// expected end time, used mainly for lessons.  For matches, this could be an estimate (but will be needed for court reservations)
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// location of match
        /// </summary>
        public VenueViewModel MatchVenue { get; set; }

        /// <summary>
        /// home/away indicator
        /// </summary>
        public bool HomeMatch { get; set; }

        /// <summary>
        /// type of event (singles, doubles, lesson, etc)
        /// </summary>
        public PlayFormat Format { get; set; }

        /// <summary>
        /// flag to tell UI whether to show the warmup time or not - don't show it if it's the same as the start time (no warm-up)
        /// </summary>
        public bool ShowWarmupTime { get { return !(StartTime == WarmupTime); } }

        /// <summary>
        /// css color string for specifying icon color
        /// </summary>
        public string VenueIconColor { get; set; }

        /// <summary>
        /// flag for UI to indicate user's availability for this match
        /// </summary>
        public Availability PlayerAvailability { get; set; }

        /// <summary>
        /// for UI display purposes, to allow display of league name in a list of matches
        /// </summary>
        public string LeagueName { get; set; }

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
        /// if true, then attempt to automatically reserve courts for upcoming matches
        /// </summary>
        public bool AutoReserveCourts { get; set; }

        /// <summary>
        /// if true, the venue supports a court reservation system
        /// </summary>
        public bool VenueHasReservationSystem { get; set; }

        /// <summary>
        /// new courts added will be marked as reserved.  pass this from league settings
        /// </summary>
        public bool MarkNewCourtsReserved { get; set; }

        /// <summary>
        /// mark new players as confirmed when adding them manually. pass this option from league settings
        /// </summary>
        public bool MarkNewPlayersConfirmed { get; set; }

        /// <summary>
        /// local start time
        /// </summary>
        public DateTime StartTimeLocal { get { return TimeZoneInfo.ConvertTimeFromUtc(StartTime, DataConstants.AppTimeZoneInfo); }
            set
            {
                if (value == null) return;

                // fetch these before changing the start/end times, because they will change when start time changes
                var matchDuration = MatchDurationMinutes;
                var warmDuration = WarmUpDurationMinutes;

                if (value.Kind == DateTimeKind.Utc)
                {
                    // time is already UTC, so just set it
                    StartTime = value;
                }
                else
                {
                    // time is localTime, so convert to UTC and set it
                    StartTime = TimeZoneInfo.ConvertTimeToUtc(value, DataConstants.AppTimeZoneInfo);
                }
                EndTime = StartTime + TimeSpan.FromMinutes(matchDuration);
                WarmupTime = StartTime - TimeSpan.FromMinutes(warmDuration);
            }
        }

        /// <summary>
        /// when fetching a match by user, tells front end if user is the captain of the league associated with this match
        /// used to permit editing of the match
        /// <summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// flag for UI display of line details, to show or not to show
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// match lines, e.g. a match could have 2 singles and 3 doubles lines, or 4 indoor court lines, etc.
        /// </summary>
        public List<LineViewModel> Lines { get; }
        public ICollection<PlayerViewModel> Players { get; }
    }
}
