using System;
using MyTennisPartner.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Models.ViewModels
{
    public class LeagueSummaryViewModel
    {
        [Required]
        public int LeagueId { get; set; }

        /// <summary>
        /// short name of the league
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// extended name/description of the league
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// details about league rules, guidelines, frequency, etc.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// owner name, for display on client, and for passing back to server if updated
        /// </summary>
        public MemberNameViewModel Owner { get; set; }

        /// <summary>
        /// minimum age for the league (e.g. 40 & over league)
        /// </summary>
        public int MinimumAge { get; set; }

        /// <summary>
        /// minimum skill ranking to join this league
        /// </summary>
        public string MinimumRanking { get; set; }

        /// <summary>
        /// maximum skill ranking to join this league
        /// </summary>
        public string MaximumRanking { get; set; }

        /// <summary>
        /// default number of lines per match.  Lines can be added/deleted later per match, but this is the default
        /// </summary>
        public int DefaultNumberOfLines { get; set; }

        /// <summary>
        /// RotatePartners - true if we want to mix up partners with each rotation
        /// </summary>
        public bool RotatePartners { get; set; }

        /// <summary>
        /// default number of rotations per line.  Rotations can be added/deleted later per match, but this is the default
        /// </summary>
        public int DefaultNumberOfRotations { get; set; }

        /// <summary>
        /// default tennis format (mens singles, mixed doubles, etc)
        /// </summary>
        public PlayFormat DefaultFormat { get; set; }

        /// <summary>
        /// true if league keeps track of scores
        /// </summary>
        public bool ScoreTracking { get; set; }

        /// <summary>
        /// true if this league is not a real league, but a template from which new leagues are created
        /// </summary>
        public bool IsTemplate { get; set; }

        /// <summary>
        /// how frequently does the league meet
        /// </summary>
        public Frequency MeetingFrequency { get; set; }

        /// <summary>
        /// day of week that the league meets
        /// </summary>
        public DayOfWeek MeetingDay { get; set; }

        /// <summary>
        /// match start time, in text 24-hr format, e.g. "18:00" = 6pm
        /// </summary>
        public string MatchStartTime { get; set; }

        /// <summary>
        /// match start time, in local time
        /// </summary>
        public string MatchStartTimeLocal { get; set; }

        /// <summary>
        /// amount of time to allow for warm-up before matches, e.g. 30 minutes
        /// </summary>
        public int WarmupTimeMinutes { get; set; }

        /// <summary>
        /// home venue for this league
        /// </summary>
        public VenueViewModel HomeVenue { get; set; }

        /// <summary>
        /// maximum number of regular (non-sub) members
        /// </summary>
        public int MaxNumberRegularMembers { get; set; }

        /// <summary>
        /// number of matches in a session
        /// </summary>
        public int NumberMatchesPerSession { get; set; }

        // calculated fields are below this line - for UI / display purposes only

        /// <summary>
        /// number of regular members
        /// </summary>
        public int RegularMemberCount { get; set; }

        /// <summary>
        /// number of sub members
        /// </summary>
        public int SubMemberCount { get; set; }

        /// <summary>
        /// number of upcoming (scheduled) matches
        /// </summary>
        public int UpcomingMatchCount { get; set; }

        /// <summary>
        /// total number of matches
        /// </summary>
        public int TotalMatchCount { get; set; }

        /// <summary>
        /// set all default values.  Use when creating a new league
        /// </summary>
        public void SetDefaultValues()
        {
            Name = "Name";
            Description = "My Description";
            Details = "My Details";
            MinimumAge = 18;
            MinimumRanking = "3.0";
            MaximumRanking = "4.0";
            DefaultNumberOfLines = 3;
            RotatePartners = false;
            DefaultNumberOfRotations = 0;
            DefaultFormat = PlayFormat.MensDoubles;
            ScoreTracking = false;
            IsTemplate = false;
            MeetingFrequency = Frequency.Weekly;
            MeetingDay = DayOfWeek.Sunday;
            MatchStartTime = "10:00";
            WarmupTimeMinutes = 30;
            MaxNumberRegularMembers = 16;
            AutoAddToLineup = false;
            MarkNewCourtsReserved = false;
            MarkNewPlayersConfirmed = false;
        }

        /// <summary>
        /// used when fetching this league, if the member fetching it is a captain, this should be true
        /// </summary>
        public bool IsCaptain { get; set; }

        /// <summary>
        /// true if members get added to line-up when responding as avail to new matches
        /// </summary>
        public bool AutoAddToLineup { get; set; }

        /// <summary>
        /// if true, new matches created for this league will be set with this flag on
        /// </summary>
        public bool AutoReserveCourts { get; set; }

        /// <summary>
        /// mark any new courts as 'reserved' - use when there is no need for an extra step to reserve courts
        /// because they are already reserved ahead of time, or there is no reservation system (i.e. public courts, etc.)
        /// </summary>
        public bool MarkNewCourtsReserved { get; set; }

        /// <summary>
        /// mark any players added to a match as 'confirmed', so they don't get an e-mail asking them to confirm
        /// </summary>
        public bool MarkNewPlayersConfirmed { get; set; }

    }
}
