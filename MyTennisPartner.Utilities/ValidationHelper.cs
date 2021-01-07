using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Utilities
{
    /// <summary>
    /// helper class for validations
    /// </summary>
    public static class ValidationHelper
    {
        public const int MaxAge = 110;
        public const int MinAge = 5;

        /// <summary>
        /// returns max acceptable birthyear value, depending on today's date
        /// </summary>
        /// <returns></returns>
        public static int GetMaxBirthyear()
        {
            var thisYear = DateTime.Today.Year;
            var maxYear = thisYear - MinAge;
            return maxYear;
        }

        /// <summary>
        /// returns min acceptable birthyear value, depending on today's date
        /// </summary>
        /// <returns></returns>
        public static int GetMinBirthyear()
        {
            var thisYear = DateTime.Today.Year;
            var minYear = thisYear - MaxAge;
            return minYear;
        }
    }
}
