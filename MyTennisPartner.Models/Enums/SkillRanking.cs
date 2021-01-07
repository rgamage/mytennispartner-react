using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.Enums
{
    /// <summary>
    /// class to hold skill rankings
    /// </summary>
    public static class SkillRanking
    {
        /// <summary>
        /// return list of available skill rankings as strings
        /// </summary>
        /// <returns></returns>
        public static List<string> GetValues()
        {
            return new List<string>
            {
                "2.5",
                "3.0",
                "3.5",
                "4.0",
                "4.5",
                "5.0",
                "5.5"
            };
        }
    }
}
