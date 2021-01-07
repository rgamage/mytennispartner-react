using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyTennisPartner.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Models.ViewModels
{
    public class 
        MemberViewModel
    {
        public MemberViewModel()
        {
            PlayerPreferences = new List<PlayerPreferenceViewModel>();
            UserPreferenceFlags =
                UserPreferenceFlags.ShowDeclinedMatchesOnDashboard;
            MemberRoleFlags = MemberRoleFlags.Player;
        }

        /// <summary>
        /// byte array holding profile image data
        /// </summary>
        private byte[] imageData;

        /// <summary>
        /// first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// member Id
        /// </summary>
        [Required]
        public int MemberId { get; set; }

        /// <summary>
        /// reference to Application User
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// gender of Member
        /// </summary>
        [EnumDataType(typeof(Gender), ErrorMessage = "Please select a gender")]
        public Gender Gender { get; set; }

        /// <summary>
        /// Member's home base venue (primary club, etc.)
        /// </summary>
        [Required(ErrorMessage ="Home Venue is required.  If your home club is not listed, select the closest or most frequented club.")]
        public VenueViewModel HomeVenue { get; set; }

        /// <summary>
        /// simple string for home venue name
        /// </summary>
        public string HomeVenueName { get; set; }

        /// <summary>
        /// zip code to use for finding nearby Members
        /// </summary>
        [StringLength(5, MinimumLength = 5)]
        [Required]
        public string ZipCode { get; set; }

        /// <summary>
        /// skill ranking, typically USTA rating
        /// </summary>
        [Required]
        public string SkillRanking { get; set; }

        /// <summary>
        /// birth year for use in calculating age
        /// </summary>
        [Range(1917, 2017)]
        public int BirthYear { get; set; }

        /// <summary>
        /// from leagueMember table, true if member is a captain of that league
        /// </summary>
        public bool IsCaptain { get; set; }

        /// <summary>
        /// from leagueMember table, true if member is sub for that league
        /// </summary>
        public bool IsSubstitute { get; set; }

        /// <summary>
        /// from leagueMember table, id of associated league
        /// </summary>
        public int LeagueId { get; set; }

        /// <summary>
        /// true if user is admin user
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// notify when player is added or removed from a match
        /// </summary>
        public bool NotifyAddOrRemoveMeFromMatch
        {
            get { return EmailNotificationFlags.HasFlag(EmailNotificationFlags.AddRemoveFromMatch); }
            set
            {
                if (value)
                {
                    EmailNotificationFlags |= EmailNotificationFlags.AddRemoveFromMatch;
                }
                else
                {
                    EmailNotificationFlags &= ~EmailNotificationFlags.AddRemoveFromMatch;
                }
            }
        }

        /// <summary>
        /// notify when any key match details change (venue, time/date)
        /// </summary>
        public bool NotifyMatchDetailsChangeOrCancelled
        {
            get { return EmailNotificationFlags.HasFlag(EmailNotificationFlags.MatchChangeOrCancel); }
            set
            {
                if (value)
                {
                    EmailNotificationFlags |= EmailNotificationFlags.MatchChangeOrCancel;
                }
                else
                {
                    EmailNotificationFlags &= ~EmailNotificationFlags.MatchChangeOrCancel;
                }
            }
        }

        /// <summary>
        /// notify if player is a sub and there is a match opening (another player cannot play)
        /// </summary>
        public bool NotifySubForMatchOpening
        {
            get { return EmailNotificationFlags.HasFlag(EmailNotificationFlags.SubNeeded); }
            set
            {
                if (value)
                {
                    EmailNotificationFlags |= EmailNotificationFlags.SubNeeded;
                }
                else
                {
                    EmailNotificationFlags &= ~EmailNotificationFlags.SubNeeded;
                }
            }
        }

        /// <summary>
        /// notify player when court assignments have changed for player
        /// </summary>
        public bool NotifyCourtChange
        {
            get { return EmailNotificationFlags.HasFlag(EmailNotificationFlags.CourtChange); }
            set
            {
                if (value)
                {
                    EmailNotificationFlags |= EmailNotificationFlags.CourtChange;
                }
                else
                {
                    EmailNotificationFlags &= ~EmailNotificationFlags.CourtChange;
                }
            }
        }

        /// <summary>
        /// notify when match added to a league of which you are a member
        /// </summary>
        public bool NotifyMatchAdded { 
            get { return EmailNotificationFlags.HasFlag(EmailNotificationFlags.MatchAdded); } 
            set {
                if (value)
                {
                    EmailNotificationFlags |= EmailNotificationFlags.MatchAdded;
                }
                else
                {
                    EmailNotificationFlags &= ~EmailNotificationFlags.MatchAdded;
                }
            } 
        }

        /// <summary>
        /// notify every time a player responds to a match, only sent to league captains
        /// </summary>
        public bool NotifyPlayerResponded
        {
            get { return EmailNotificationFlags.HasFlag(EmailNotificationFlags.PlayerResponded); }
            set
            {
                if (value)
                {
                    EmailNotificationFlags |= EmailNotificationFlags.PlayerResponded;
                }
                else
                {
                    EmailNotificationFlags &= ~EmailNotificationFlags.PlayerResponded;
                }
            }
        }

        /// <summary>
        /// flags to track user preferences
        /// </summary>
        public UserPreferenceFlags UserPreferenceFlags { get; set; }

        /// <summary>
        /// flags to track help tips, can be dismissed by user one by one
        /// </summary>
        public HelpTipTrackers HelpTipTrackers { get; set; }

        /// <summary>
        /// list of player preferences
        /// </summary>
        public List<PlayerPreferenceViewModel> PlayerPreferences { get; }

        /// <summary>
        /// ref id from LeagueMember table
        /// </summary>
        public int LeagueMemberId { get; set; }

        /// <summary>
        /// phone number
        /// </summary>
        [Phone]
        [Required]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// user email address (stored in both AppUser db and memberdb)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// for use on client side to show availability for a given match
        /// </summary>
        public Availability Availability { get; set; }

        /// <summary>
        /// for use on client, when picking a league member as a player to add to a line-up
        /// </summary>
        public int? PlayerId { get; set; }

        /// <summary>
        /// credentials for court reservations
        /// </summary>
        public ReservationCredentialsViewModel ReservationCredentials { get; set; }

        /// <summary>
        /// username for sparetime court reservation system
        /// </summary>
        public string SpareTimeUsername { get; set; }

        /// <summary>
        /// password for sparetime court reservation system
        /// </summary>
        public string SpareTimePassword { get; set; }

        /// <summary>
        /// member number for spare time court reservation system
        /// </summary>
        public int? SpareTimeMemberNumber { get; set; }

        /// <summary>
        /// comma-separated list of Venue Ids for which the member has auto-reserve capability
        /// </summary>
        public string AutoReserveVenues { get; set; }
        
        /// <summary>
        /// member profile image, as a base64-encoded string, for display in an HTML page
        /// </summary>
        public string ProfileImageBase64 { get
            {
                // encode image data as base64
                if (imageData is null) return string.Empty;
                var base64 = Convert.ToBase64String(imageData);
                return string.Format("data:image/gif;base64,{0}", base64);
            }
        }

        /// <summary>
        /// set profile image
        /// </summary>
        /// <param name="imageData"></param>
        public void SetProfileImage(byte[] imageData)
        {
            this.imageData = imageData;
        }

        /// <summary>
        /// access image data
        /// </summary>
        /// <returns></returns>
        public byte[] GetImageData()
        {
            return imageData;
        }

        /// <summary>
        /// member role flags
        /// </summary>
        public MemberRoleFlags MemberRoleFlags { get; set; }

        /// <summary>
        /// preference flags for email notifications
        /// </summary>
        public EmailNotificationFlags EmailNotificationFlags { get; set; }

        /// <summary>
        /// for use in UI, to mark this member to be deleted
        /// </summary>
        public bool MarkToDelete { get; set; }
    }
}
