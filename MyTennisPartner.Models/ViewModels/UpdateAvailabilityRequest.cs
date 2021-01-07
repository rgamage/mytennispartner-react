using MyTennisPartner.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Models.ViewModels
{
    public class UpdateAvailabilityRequest
    {
        public UpdateAvailabilityRequest()
        {
            InviteMemberIds = new List<int>();
        }

        [Required]
        public int MatchId { get; set; }

        /// <summary>
        /// memberId of referring member (member who has just cancelled, for example), and is referring this match to someone else
        /// </summary>
        [Required]
        public int MemberId { get; set; }

        [Required]
        public int LeagueId { get; set; }

        [Required]
        public Availability Value { get; set; }

        public MatchDeclineAction Action { get; set; }

        public bool IsInLineup { get; set; }

        /// <summary>
        /// for the case of a member responding to a match opportunity, or changing their availability,
        /// this is the id of the responding member
        /// </summary>
        public int RespondingMemberId { get; set; }

        /// <summary>
        /// list of memberIds which will receive a match invitation, in the case a user is declining a match
        /// </summary>
        public List<int> InviteMemberIds { get; }
    }
}
