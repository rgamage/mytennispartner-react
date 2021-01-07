using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyTennisPartner.Models.Enums;

namespace MyTennisPartner.Data.Utilities
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// given a frequency of matches, return how many days to add to a date to shift to next match
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static int GetDaysFromFrequency(Frequency frequency)
        {
            switch (frequency)
            {
                case Frequency.Weekly:
                    return 7;
                case Frequency.BiWeekly:
                    return 14;
                case Frequency.TriWeekly:
                    return 21;
                case Frequency.AdHoc:
                    return 1;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// given a start date and frequency, return next date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static DateTime GetDateFromFrequency(DateTime startDate, Frequency frequency)
        {
            switch (frequency)
            {
                case Frequency.Weekly:
                    return startDate + TimeSpan.FromDays(7);
                case Frequency.BiWeekly:
                    return startDate + TimeSpan.FromDays(14);
                case Frequency.TriWeekly:
                    return startDate + TimeSpan.FromDays(21);
                default:
                    return startDate;
            }
        }

        /// <summary>
        /// method to test if a utc date is in future
        /// </summary>
        /// <param name="utcDate"></param>
        /// <returns></returns>
        public static bool IsInFuture(this DateTime utcDate)
        {
            var utcNow = DateTime.UtcNow;
            if ((utcDate - utcNow).TotalMinutes > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// gets local (to application users' time zone) date/time
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLocalDateTime()
        {
            var dateLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DataConstants.AppTimeZoneInfo);
            return dateLocal;
        }

        /// <summary>
        /// converts utc time string to local display format
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToDisplayTimeFromUtc(string utcTimeString)
        {
            var localTimeString = UtcToLocalTimeString(utcTimeString);
            return ToDisplayTime(localTimeString);
        }

        /// <summary>
        /// converts time string, like "18:00" to display time, like "6:00pm"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToDisplayTime(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "unknown time";
            }

            var strArray = input.Split(new char[] { ':' });
            if (strArray.Length < 2)
            {
                return "unknown time";
            }

            var hrs = strArray[0];
            var minutes = strArray[1];
            var displayHours = ((int.Parse(hrs)-1) % 12) + 1;
            var ampm = int.Parse(hrs) < 12 ? "AM" : "PM";
            var result = $"{displayHours}:{minutes} {ampm}";
            return result;
        }

        /// <summary>
        /// converts utc time string, like "17:00" to time string in local time, like "09:00" if we are in PST (UTC-8)
        /// </summary>
        /// <param name="utcTimeString"></param>
        /// <returns></returns>
        public static string UtcToLocalTimeString(string utcTimeString)
        {
            if (string.IsNullOrEmpty(utcTimeString)) return "";
            if (!utcTimeString.Contains(":"))
            {
                // invalid input, return input string
                return utcTimeString;
            }
            var UtcOffsetMinutes = (int)DataConstants.AppTimeZoneInfo.BaseUtcOffset.TotalMinutes;
            var hrUtc = int.Parse(utcTimeString.Split(':')[0]);
            var minUtc = int.Parse(utcTimeString.Split(':')[1]);
            var totalMinutesUtc = 60 * hrUtc + minUtc;
            
            // are we in DST?  If so, correct for it
            var now = DateTime.Now;
            var DstOffsetMinutes = now.IsDaylightSavingTime() ? 60 : 0;

            var totalMinutesLocal = totalMinutesUtc + UtcOffsetMinutes + DstOffsetMinutes;
            if (totalMinutesLocal < 0)
            {
                totalMinutesLocal += 24 * 60;
            }
            var hrsLocal = (int)Math.Floor((double)totalMinutesLocal / 60) % 24;
            var minLocal = totalMinutesLocal % 60;
            var localTimeString = $"{hrsLocal:D2}:{minLocal:D2}";
            return localTimeString;
        }

        public static string LocalToUtcTimeString(string localTimeString)
        {
            if (string.IsNullOrEmpty(localTimeString)) return "";
            if (!localTimeString.Contains(":"))
            {
                // invalid input, return input string
                return localTimeString;
            }
            var UtcOffsetMinutes = (int)DataConstants.AppTimeZoneInfo.BaseUtcOffset.TotalMinutes;
            var hrLocal = int.Parse(localTimeString.Split(':')[0]);
            var minLocal = int.Parse(localTimeString.Split(':')[1]);
            var totalMinutesLocal = 60 * hrLocal + minLocal;

            // are we in DST?  If so, correct for it
            var now = DateTime.Now;
            var DstOffsetMinutes = now.IsDaylightSavingTime() ? 60 : 0;

            var totalMinutesUtc = totalMinutesLocal - UtcOffsetMinutes - DstOffsetMinutes;
            if (totalMinutesUtc < 0)
            {
                totalMinutesUtc += 24 * 60;
            }
            var hrsUtc = (int)Math.Floor((double)totalMinutesUtc / 60) % 24;
            var minUtc = totalMinutesUtc % 60;
            var utcTimeString = $"{hrsUtc:D2}:{minUtc:D2}";
            return utcTimeString;
        }
    }
}
