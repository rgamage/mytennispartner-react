using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyTennisPartner.Models.CourtReservations;
using MyTennisPartner.Models.CourtReservations.TennisBookings;

namespace MyTennisPartner.Web.Services.Reservations
{
    public class CourtReservationServiceLifetime : ICourtReservationService
    {
        public CourtOpeningCollection GetCourtAvailability(string username, string password, DateTime dateUtc, bool includeFutureDates)
        {
            throw new NotImplementedException();
        }

        public bool Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public int LoginTest(string username, string password)
        {
            throw new NotImplementedException();
        }

        public bool ReserveCourts(string username, string password, ReservationDetails reservation)
        {
            throw new NotImplementedException();
        }

        public void SetHost(string hostname)
        {
            throw new NotImplementedException();
        }
    }
}
