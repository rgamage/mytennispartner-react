using MyTennisPartner.Data.Models;

namespace MyTennisPartner.Data.DataTransferObjects
{
    public class LeagueSummary: League
    {
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
        /// number of total matches (past and scheduled)
        /// </summary>
        public int TotalMatchCount { get; set; }
    }

}

