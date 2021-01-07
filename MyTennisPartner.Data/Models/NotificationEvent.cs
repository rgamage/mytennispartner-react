using MyTennisPartner.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Data.Models
{
    public enum NotificationEventType
    {
        AddedToMatch,
        RemovedFromMatch,
        SubForMatchOpening,
        CourtChange,
        MatchCancelled,
        MatchChanged,
        MatchAdded,
        MatchAddedReminder,
        AddedToLeague,
        PlayerResponded,
        CourtAutoAdded
    }

    /// <summary>
    /// class to hold details of a notification event that is found, usually by a data manager
    /// the details of this event are stored in this class and passed to a notification manager to
    /// send emails/texts as needed to users
    /// </summary>
    public class NotificationEvent
    {
        public NotificationEvent()
        {
            MemberIds = new List<int>();
            LeagueMemberIds = new List<int>();
        }

        public override string ToString()
        {
            return EventType.ToString();
        }

        public NotificationEventType EventType { get; set; }

        /// <summary>
        /// match Id
        /// </summary>
        public int MatchId { get; set; }

        /// <summary>
        /// league Id
        /// </summary>
        public int LeagueId { get; set; }

        /// <summary>
        /// optional additional description/info about this event, for use in body of email to user
        /// </summary>
        public string EventDescription { get; set; }

        /// <summary>
        /// members affected by the event
        /// </summary>
        public List<int> MemberIds { get; }

        /// <summary>
        /// leagueMemberIds. in some cases the memberIds are not readily available.  In those cases, use leagueMemberId instead
        /// </summary>
        public List<int> LeagueMemberIds { get; }

        /// <summary>
        /// in the case of a player that is declining a match, they are the referring member
        /// </summary>
        public int ReferringMemberId { get; set; }

        /// <summary>
        /// holds all info about the match in question
        /// </summary>
        public MatchSummaryViewModel MatchSummary { get; set; }
    }
}
