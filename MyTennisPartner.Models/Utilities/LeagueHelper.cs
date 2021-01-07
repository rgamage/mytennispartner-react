using MyTennisPartner.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.Utilities
{
    /// <summary>
    /// helper class for various needs
    /// </summary>
    public static class LeagueHelper
    {
        /// <summary>
        /// given a format of play, return size of full court (4 players for doubles, 2 for singles, etc.)
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static int FullCourtSize(PlayFormat format)
        {
            switch (format)
            {
                case PlayFormat.MensSingles:
                case PlayFormat.WomensSingles:
                case PlayFormat.SinglesPractice:
                    return 2;
                case PlayFormat.MensDoubles:
                case PlayFormat.MixedDoubles:
                case PlayFormat.WomensDoubles:
                    return 4;
                case PlayFormat.PrivateLesson:
                    return 1;
                case PlayFormat.GroupLesson:
                    return 10;
                default:
                    return 10;
            }
        }
    }
}
