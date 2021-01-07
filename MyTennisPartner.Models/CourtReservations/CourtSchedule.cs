using System.Collections.Generic;

namespace MyTennisPartner.Models.CourtReservations
{
    public class CourtSchedule
    {
        public CourtSchedule()
        {
            BookedTimeSlots = new List<TimeSlot>();
        }
        public string CourtNumber { get; set; }
        public List<TimeSlot> BookedTimeSlots { get; }

    }
}
