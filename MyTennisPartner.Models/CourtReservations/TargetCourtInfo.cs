using System.Collections.Generic;

namespace MyTennisPartner.Models.CourtReservations
{
    /// <summary>
    /// class to hold info for target courts - those courts that the algorithm has identified as candidates to reserve for a match
    /// </summary>
    public class TargetCourtInfo
    {
        public TargetCourtInfo(int matchId)
        {
            MatchId = matchId;
            TargetCourts = new List<string>();
        }

        public int MatchId { get; set; }
        public List<string> TargetCourts { get; }
        /// <summary>
        /// comma-separated list of courts available
        /// </summary>
        public string CourtsAvailable { get; set; }
    }
}
