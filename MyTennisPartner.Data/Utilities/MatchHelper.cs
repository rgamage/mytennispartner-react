using MyTennisPartner.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Data.Utilities
{
    public static class MatchHelper
    {
        /// <summary>
        /// return true if basic match properties are true
        /// </summary>
        /// <param name="match1"></param>
        /// <param name="match2"></param>
        /// <returns></returns>
        public static bool MatchPropertiesAreEqual(Match match1, Match match2)
        {
            var result = true;
            if (match1 is null && match2 is null) return true;
            if (match1 is null || match2 is null) return false;
            if (match1.StartTime != match2.StartTime) result = false;
            if (match1.EndTime != match2.EndTime) result = false;
            if (match1.WarmupTime != match2.WarmupTime) result = false;
            if (match1.MatchVenueVenueId != match2.MatchVenueVenueId) result = false;
            if (match1.HomeMatch != match2.HomeMatch) result = false;

            return result;
        }

    }
}
