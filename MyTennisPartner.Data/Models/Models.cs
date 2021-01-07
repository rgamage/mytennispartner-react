using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyTennisPartner.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MyTennisPartner.Models.ViewModels;

namespace MyTennisPartner.Data.Models
{

    public class MemberRole
    {
        [Key]
        public int MemberRoleId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public Role Role { get; set; }
    }

    /// <summary>
    /// member profile info
    /// </summary>
    public class Member
    {
        public Member()
        {
            CreateDate = DateTime.UtcNow;
            LeagueMembers = new HashSet<LeagueMember>();
            PlayerPreferences = new HashSet<PlayerPreference>();
            UserPreferenceFlags =
                UserPreferenceFlags.ShowDeclinedMatchesOnDashboard;
            MemberRoleFlags = MemberRoleFlags.Player;
        }

        [Key]
        public int MemberId { get; set; }

        /// <summary>
        /// reference to Application User
        /// </summary>
        //[Required]
        public string UserId { get; set; }

        /// <summary>
        /// first name for public profile
        /// </summary>
        //[Required]
        public string FirstName { get; set; }

        /// <summary>
        /// last name for public profile
        /// </summary>
        //[Required]
        public string LastName { get; set; }

        /// <summary>
        /// gender of Member
        /// </summary>
        [EnumDataType(typeof(Gender), ErrorMessage = "Please select a gender")]
        public Gender Gender { get; set; }

        /// <summary>
        /// Member's home base venue (primary club, etc.)
        /// </summary>
        public Venue HomeVenue { get; set; }

        /// <summary>
        /// zip code to use for finding nearby Members
        /// </summary>
        [StringLength(5, MinimumLength = 5)]
        [Required]
        public string ZipCode { get; set; }

        /// <summary>
        /// skill ranking, typically USTA rating
        /// </summary>
        //[Required]
        public string SkillRanking { get; set; }

        /// <summary>
        /// birth year for use in calculating age
        /// </summary>
        [Range(1917, 2017)]
        public int BirthYear { get; set; }

        /// <summary>
        /// profile image
        /// </summary>
        [JsonIgnore]  // do not serialize the image, for example when returning member from an API method
        public MemberImage Image { get; set; }

        /// <summary>
        /// ref id of home venue
        /// </summary>
        public int? HomeVenueVenueId { get; set; }

        /// <summary>
        /// notify when player is added or removed from a match
        /// </summary>
        public bool NotifyAddOrRemoveMeFromMatch { get; set; }

        /// <summary>
        /// notify when any key match details change (venue, time/date)
        /// </summary>
        public bool NotifyMatchDetailsChangeOrCancelled { get; set; }

        /// <summary>
        /// notify if player is a sub and there is a match opening (another player cannot play)
        /// </summary>
        public bool NotifySubForMatchOpening { get; set; }

        /// <summary>
        /// notify player when court assignments have changed for player
        /// </summary>
        public bool NotifyCourtChange { get; set; }

        /// <summary>
        /// notify when new match added to a league of which you are a member
        /// </summary>
        public bool NotifyMatchAdded { get; set; }

        /// <summary>
        /// phone number for calling, text messages, etc.
        /// </summary>
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// username for spare time court reservation system
        /// </summary>
        public string SpareTimeUsername { get; set; }

        /// <summary>
        /// password for spare time court reservation system
        /// </summary>
        public string SpareTimePassword { get; set; }

        /// <summary>
        /// spare time court reservation system member number
        /// </summary>
        public int? SpareTimeMemberNumber { get; set; }

        /// <summary>
        /// comma-separated list of Venue Ids for which the member has auto-reserve capability
        /// </summary>
        public string AutoReserveVenues { get; set; }

        /// <summary>
        /// date record was created
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// tracking of which help tips user has dismissed
        /// </summary>
        public HelpTipTrackers HelpTipTrackers { get; set; }

        /// <summary>
        /// flags for user preferences
        /// </summary>
        public UserPreferenceFlags UserPreferenceFlags { get; set; }

        /// <summary>
        /// email address, kept in sync with user db email field
        /// user should only be able to edit from accounts page / user db, then
        /// app needs to ensure this updates member db as well
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// member roles
        /// </summary>
        public MemberRoleFlags MemberRoleFlags { get; set; }

        /// <summary>
        /// preference flags for email notifications
        /// </summary>
        public EmailNotificationFlags EmailNotificationFlags { get; set; }

        /// <summary>
        /// preference flags for text (SMS) notifications
        /// </summary>
        public TextNotificationFlags TextNotificationFlags { get; set; }

        /// <summary>
        /// reference to join table for league Members
        /// </summary>
        public ICollection<LeagueMember> LeagueMembers { get; }

        /// <summary>
        /// Member's tennis PlayerPreferences (what formats to they like to play?)
        /// </summary>
        public ICollection<PlayerPreference> PlayerPreferences { get; }
        
        /// <summary>
        /// court reservation credentials
        /// </summary>
        //public ICollection<ReservationCredentials> ReservationCredentials { get; set; }

    }

    /// <summary>
    /// court reservation credentials for a given court reservation system/provider
    /// </summary>
    //public class ReservationCredentials
    //{
    //    public int Id { get; set; }
    //    public int MemberId { get; set; }
    //    public CourtReservationProvider CourtReservationProvider { get; set; }
    //    public string Username { get; set; }
    //    public string Password { get; set; }
    //}
     
    /// <summary>
    /// Member's PlayerPreferences in terms of formats of play
    /// </summary>
    public class PlayerPreference
    {
        [Key]
        public int PlayerPreferenceId { get; set; }
        public PlayFormat Format { get; set; }
        public int MemberId { get; set; }
    }

    /// <summary>
    /// court reservation system details
    /// </summary>
    public class ReservationSystem
    {
        public int Id { get; set; }
        public CourtReservationProvider CourtReservationProvider { get; set; }
        public string HostName { get; set; }
        public int EarliestCourtHour { get; set; }
        public int EarliestCourtMinute { get; set; }
        public int LatestCourtHour { get; set; }
        public int LatestCourtMinute { get; set; }
        public int VenueId { get; set; }

        /// <summary>
        /// how many days ahead can you reserve courts?
        /// </summary>
        public int MaxDaysAhead { get; set; }
    }

    /// <summary>
    /// class to hold match location
    /// </summary>
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        // name of venue
        public string Name { get; set; }

        public Address VenueAddress { get; set; }

        // contact person that actively manages the tennis (courts, etc.) for venue
        public Contact VenueContact { get; set; }

        public ReservationSystem ReservationSystem { get; set; }
    }

    /// <summary>
    /// generic address class - look into copying standard practices for address field behaviors, data types, validation, etc.
    /// </summary>
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        [StringLength(50)]
        public string Street1 { get; set; }

        [StringLength(50)]
        public string Street2 { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(2)]
        public string State { get; set; }

        [StringLength(10)]
        public string Zip { get; set; }

        // foreign key
        public int VenueId { get; set; }
    }

    /// <summary>
    /// personal contact info
    /// </summary>
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [StringLength(30)]
        public string FirstName { get; set; }

        [StringLength(30)]
        public string LastName { get; set; }

        [StringLength(30)]
        [Phone]
        public string Phone1 { get; set; }

        [StringLength(30)]
        [Phone]
        public string Phone2 { get; set; }

        [StringLength(50)]
        [EmailAddress]
        public string Email { get; set; }

        // foreign key
        public int VenueId { get; set; }
    }


    public class League
    {
        public League()
        {
            CreateDate = DateTime.UtcNow;
            Matches = new HashSet<Match>();
            LeagueMembers = new HashSet<LeagueMember>();
        }

        [Key]
        public int LeagueId { get; set; }

        /// <summary>
        /// short name of the league
        /// </summary>
        [StringLength(52)]
        public string Name { get; set; }

        /// <summary>
        /// extended name/description of the league
        /// </summary>
        [StringLength(100)]
        public string Description { get; set; }

        /// <summary>
        /// details about league rules, guidelines, frequency, etc.
        /// </summary>
        [StringLength(1000)]
        public string Details { get; set; }

        /// <summary>
        /// organizer, captain, pro, or owner of this league
        /// </summary>
        public Member Owner { get; set; }

        /// <summary>
        /// reference to owner member id
        /// </summary>
        public int? OwnerMemberId { get; set; }

        /// <summary>
        /// minimum age for the league (e.g. 40 & over league)
        /// </summary>
        [Range(5, 110)]
        public int MinimumAge { get; set; }

        /// <summary>
        /// minimum skill ranking to join this league
        /// </summary>
        [Display(Name="Minimum NTRP Member ranking")]
        public string MinimumRanking { get; set; }

        /// <summary>
        /// maximum skill ranking to join this league
        /// </summary>
        [Display(Name = "Maximum NTRP Member ranking")]
        public string MaximumRanking { get; set; }

        /// <summary>
        /// IsCompetitiveLeague is true if matches are played against other leagues
        /// If false, then league is internal (they play each other rather than compete against other leagues)
        /// </summary>
        //[Display(Name="This a competitive league (we play against other leagues)")]
        //public bool IsCompetitiveLeague { get; set; }

        /// <summary>
        /// default number of lines per match.  Lines can be added/deleted later per match, but this is the default
        /// </summary>
        [Display(Name = "Default number of lines or courts per match")]
        public int DefaultNumberOfLines { get; set; }

        /// <summary>
        /// RotatePartners - true if we want to mix up partners with each rotation
        /// </summary>
        [Display(Name="Each line has multiple rotations")]
        public bool RotatePartners { get; set; }

        /// <summary>
        /// default tennis format (mens singles, mixed doubles, etc)
        /// </summary>
        [Display(Name="Default format of play")]
        public PlayFormat DefaultFormat { get; set; }

        /// <summary>
        /// true if league keeps track of scores
        /// </summary>
        [Display(Name="Track our scores")]
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
        /// time of day matches usually start for this league (timespan since midnight)
        /// </summary>
        public TimeSpan MatchStartTime { get; set; }

        /// <summary>
        /// amount of time in minutes to allow for warm-up before matches, e.g. 30 minutes
        /// </summary>
        public int WarmupTimeMinutes { get; set; }

        /// <summary>
        /// venue where most or all matches are played for this league
        /// </summary>
        public Venue HomeVenue { get; set; }

        /// <summary>
        /// reference to home venue
        /// </summary>
        public int? HomeVenueVenueId { get; set; }

        /// <summary>
        /// maximum number of regular (non-sub) members
        /// </summary>
        public int MaxNumberRegularMembers { get; set; }

        /// <summary>
        /// number of matches in a session
        /// </summary>
        public int NumberMatchesPerSession { get; set; }

        /// <summary>
        /// if true, when user responds to avail for a new match, if avail, auto-add user to line-up as Confirmed
        /// </summary>
        public bool AutoAddToLineup { get; set; }

        /// <summary>
        /// if true, new matches will default to auto-reserve courts
        /// </summary>
        public bool AutoReserveCourts { get; set; }

        /// <summary>
        /// create date of league
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// mark any new courts as 'reserved' - use when there is no need for an extra step to reserve courts
        /// because they are already reserved ahead of time, or there is no reservation system (i.e. public courts, etc.)
        /// </summary>
        public bool MarkNewCourtsReserved { get; set; }

        /// <summary>
        /// mark any players added to a match as 'confirmed', so they don't get an e-mail asking them to confirm
        /// </summary>
        public bool MarkNewPlayersConfirmed { get; set; }

        /// <summary>
        /// collection of Members in this league
        /// </summary>
        public ICollection<LeagueMember> LeagueMembers { get; }

        /// <summary>
        /// collection of matches
        /// </summary>
        public ICollection<Match> Matches { get; }

        /// <summary>
        /// method to clone ourselves
        /// </summary>
        /// <returns></returns>
        public League Clone() { return (League)MemberwiseClone(); }
    }

    /// <summary>
    /// class for individual league session or season (collection of matches)
    /// </summary>
    public class Session
    {
        /// <summary>
        /// primary key
        /// </summary>
        [Key]
        public int SessionId { get; set; }

        /// <summary>
        /// ref to league
        /// </summary>
        public int LeagueId { get; set; }

        /// <summary>
        /// user-entered name of session
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// start date of session - should be set to 0:00 time of day (beginning of day)
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// end date of session - should be set to 23:59:59 time of day (end of day)
        /// </summary>
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// class to hold match info
    /// Note: we need context-sensitive names for matches and other items.  For example, lessons would be called lesson or clinic, not match
    /// This context-sensitve naming could be handled as part of the localization and sport-specific terminology scheme
    /// </summary>
    public class Match
    {
        public Match()
        {
            CreateDate = DateTime.UtcNow;
            Lines = new HashSet<Line>();
            Players = new HashSet<Player>();
        }

        [Key]
        public int MatchId { get; set; }

        /// <summary>
        /// session id, if exists
        /// </summary>
        public int? SessionId { get; set; }

        /// <summary>
        /// league id
        /// </summary>
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
        /// expected end time, used mainly for lessons.  For matches, this could be an estimate (but will be needed for court reservations)
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// location of match
        /// </summary>
        public Venue MatchVenue { get; set; }

        /// <summary>
        /// foreign key - reference to match venue id
        /// </summary>
        public int? MatchVenueVenueId { get; set; }

        /// <summary>
        /// home/away indicator
        /// </summary>
        public bool HomeMatch { get; set; }

        /// <summary>
        /// type of event (singles, doubles, lesson, etc)
        /// </summary>
        public PlayFormat Format { get; set; }

        /// <summary>
        /// if true, and if enough players have entered their credentials, then auto-reserve the courts for upcoming matches
        /// </summary>
        public bool AutoReserveCourts { get; set; }

        /// <summary>
        /// create date of match
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// match lines, e.g. a match could have 2 singles and 3 doubles lines, or 4 indoor court lines, etc.
        /// </summary>
        public ICollection<Line> Lines { get; }

        /// <summary>
        /// navigation property to parent league
        /// </summary>
        public League League {get; set; }

        /// <summary>
        /// players
        /// </summary>
        public ICollection<Player> Players { get; }
    }

    /// <summary>
    /// each match has a collection of lines (e.g. indoor could have 8 lines, social league could have 1-3 lines, etc.)
    /// Lines are simultaneous competitions during a given match
    /// </summary>
    public class Line
    {
        [Key]
        public int LineId { get; set; }

        /// <summary>
        /// court number
        /// </summary>
        public string CourtNumber { get; set; }

        /// <summary>
        /// each line could have a different format (e.g. singles, doubles, etc.)
        /// </summary>
        public PlayFormat Format { get; set; }

        /// <summary>
        /// true if court has been reserved 
        /// </summary>
        public bool IsReserved { get; set; }

        /// <summary>
        /// link to match parent for this line
        /// </summary>
        public Match Match { get; set; }

        /// <summary>
        /// id of parent match
        /// </summary>
        public int MatchId { get; set; }

        /// <summary>
        /// used only for client mapping of new lines to players, when adding new lines and associating
        /// players all in one transaction.  This field is not saved to the database (not mapped in EF)
        /// </summary>
        [NotMapped]
#pragma warning disable CA1720 // Identifier contains type name
        public string Guid { get; set; }
#pragma warning restore CA1720 // Identifier contains type name

        /// <summary>
        /// true if user has overridden the court number (for use when auto-reserve system is managing court number)
        /// </summary>
        public bool CourtNumberOverridden { get; set; }

        /// <summary>
        /// list of courts available, according to reservation system
        /// </summary>
        public string CourtsAvailable { get; set; }
    }

    /// <summary>
    /// LeagueUsers (Members) is a join entity of leagues and users
    /// </summary>
    public class LeagueMember
    {
        public LeagueMember()
        {
            Players = new HashSet<Player>();
        }

        [Key]
        public int LeagueMemberId { get; set; }

        /// <summary>
        /// foreign key to league
        /// </summary>
        public int LeagueId { get; set; }

        /// <summary>
        /// foreign key to Member
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// true if this Member is not a regular league member, but part of a sub pool for the league
        /// </summary>
        public bool IsSubstitute { get; set; }

        /// <summary>
        /// true if captain or co-captain.  there can be multiple captains in a league
        /// </summary>
        public bool IsCaptain { get; set; }

        // references
        public League League { get; set; }
        public Member Member { get; set; }

        /// <summary>
        /// players
        /// </summary>
        public ICollection<Player> Players { get; }
    }

    /// <summary>
    /// member image for member profile pics
    /// </summary>
    public class MemberImage
    {
        [Key]
        public int ImageId { get; set; }
        public byte[] ImageBytes { get; set; }
        public int MemberId { get; set; }
    }


    public class Player
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int LeagueId { get; set; }
        public int? LineId { get; set; }
        public int MatchId { get; set; }
        public int LeagueMemberId { get; set; }
        public Member Member { get; set; }
        public League League { get; set; }
        //public Line Line { get; set; }
        public Match Match { get; set; }
        public LeagueMember LeagueMember { get; set; }
        public Availability Availability { get; set; }
        public bool IsSubstitute { get; set; }
        public bool IsHomePlayer { get; set; }
        public DateTime ModifiedDate { get; set; }

        [NotMapped]
        public bool IsInLineup
        {
            get
            {
                // if we are linked to a line, then we are in a line-up
                // note: we consider value of "0" to be in line-up, because it could be a new line, that does
                // not yet have an Id.  If there is no line, it should be null
                return LineId >= 0;
            }
        }

        /// <summary>
        /// used only for client mapping of new lines to players, when adding new lines and associating
        /// players all in one transaction.  This field is not saved to the database (not mapped in EF)
        /// </summary>
        [NotMapped]
#pragma warning disable CA1720 // Identifier contains type name
        public string Guid { get; set; }
#pragma warning restore CA1720 // Identifier contains type name
    }
}
