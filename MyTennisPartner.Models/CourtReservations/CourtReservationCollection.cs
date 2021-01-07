using MyTennisPartner.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyTennisPartner.Models.CourtReservations
{
    public class CourtOpeningCollection
    {
        public CourtOpeningCollection()
        {
            CourtSchedules = new List<CourtSchedule>();
        }

        public List<CourtSchedule> CourtSchedules { get; }

        public DateTime Date { get; set; }

        /// <summary>
        /// given a start and end time, return all courts that are available for that time slot
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<string> CourtsAvailable(DateTime startTime, DateTime endTime)
        {
            var utcNow = DateTime.UtcNow;
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, ApplicationConstants.AppTimeZoneInfo);
            if (startTime < localNow)
            {
                // cannot reserve courts in the past, so return an empty list of courts available
                return new List<string>();
            }

            // todo: check for < earliest hour, > latest hour (hours of operation)

            var courts = CourtSchedules
                .Where(c => !c.BookedTimeSlots.Any(r => r.StartTime < endTime && r.EndTime > startTime))
                .OrderBy(c => MyTennisPartner.Models.Utilities.StringHelper.ZeroPadLeft(c.CourtNumber))
                .Select(c => c.CourtNumber)
                .ToList();

            return courts;
        }
    }
}
