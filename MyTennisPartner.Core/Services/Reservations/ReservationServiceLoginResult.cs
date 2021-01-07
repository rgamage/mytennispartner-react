using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Core.Services.Reservations
{
    /// <summary>
    /// class to hold results of a login test for multiple court reservation providers
    /// </summary>
    public class ReservationServiceLoginResult
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public int MemberNumber { get; set; }
        public bool LoginSuccess => MemberNumber > 0;
    }
}
