using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Utilities
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
            if (string.IsNullOrEmpty(input)) return "000";
            var isNum = int.TryParse(input, out int result);
            if (isNum) return result.ToString("D3");
            else return input;
        }

        /// <summary>
        /// test if list of numbers (in string format) are consecutive
        /// </summary>
        /// <param name="numberList"></param>
        /// <returns></returns>
        public static bool ConsecutiveNumbers(List<string> numberList)
        {
            if (numberList is null) return false;
            if (numberList.Count < 2) return true;
            var success = int.TryParse(numberList[0], out int prevNumber);
            if (!success) return false;  // bad format of number string
            for (var i = 1; i < numberList.Count; i++)
            {
                success = int.TryParse(numberList[i], out int result);
                if (!success) return false;
                if (result != prevNumber + 1)
                {
                    return false;
                }
                prevNumber = result;
            }
            return true;
        }

        public static string RemoveSpaces(string inputString)
        {
            var result = inputString?.Replace(" ", "");
            return result;
        }

        public static string RemoveSpacesAndParens(string inputString)
        {
            var result = inputString?.Replace(" ", "");
            result = result?.Replace("(", "");
            result = result?.Replace(")", "");
            return result;
        }

        /// <summary>
        /// given a string representing an integer, return a string representing the next sequential integer
        /// e.g. given "2", return "3"
        /// if unable to parse input, return "1"
        /// </summary>
        /// <param name="numberString"></param>
        /// <returns></returns>
        public static string GetNextNumber(string numberString)
        {
            var success = Int32.TryParse(numberString, out int number);
            if (!success) return "1";
            var result = number++;
            return result.ToString();
        }
    }
}
