using MyTennisPartner.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.ViewModels
{
    /// <summary>
    /// court reservation system details
    /// </summary>
    public class ReservationSystemViewModel
    {
        public int Id { get; set; }
        public CourtReservationProvider CourtReservationProvider { get; set; }
        public string HostName { get; set; }
        public int EarliestCourtHour { get; set; }
        public int EarliestCourtMinute { get; set; }
        public int LatestCourtHour { get; set; }
        public int LatestCourtMinute { get; set; }
        public int VenueId { get; set; }

        /// <summary>
        /// how many days ahead can you reserve courts?
        /// </summary>
        public int MaxDaysAhead { get; set; }
    }
}
