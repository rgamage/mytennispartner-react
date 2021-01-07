
using System;
using System.Collections.Generic;

namespace MyTennisPartner.Models.Utilities
{
    public static class StringHelper
    {
        /// <summary>
        /// given a string input that is usually a number, left pad with zeros for easier numeric sorting
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ZeroPadLeft(string input)
        {
            var isNum = int.TryParse(input, out int result);
            if (isNum) return result.ToString("D3");
            else return input;
        }

        /// <summary>
        /// returns next number as a string, given a string representing a number
        /// for example, given "3", returns "4"
        /// </summary>
        /// <param name="currentNumber"></param>
        /// <returns></returns>
        public static string NextNumberAsString(string currentNumber)
        {
            var success = int.TryParse(currentNumber, out int result);
            if (success)
            {
                return (result + 1).ToString();
            }
            else return "1";
        }

        public static string DayOfWeekString(int dayIndex)
        {
            switch(dayIndex)
            {
                case 0: return "Sunday";
                case 1: return "Monday";
                case 2: return "Tuesday";
                case 3: return "Wednesday";
                case 4: return "Thursday";
                case 5: return "Friday";
                case 6: return "Saturday";
                default: return "Uknown Day";
            }
        }

        /// <summary>
        /// given an enum type, return all string values of the enums
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> GetAllStrings<T>() where T : Enum
        {
            var result = new List<string>();
            foreach (T value in (T[])Enum.GetValues(typeof(T)))
            {
                result.Add(value.ToString());
            }
            result.Sort();
            return result;
        }
    }
}
