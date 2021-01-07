using System;
using MyTennisPartner.Models.CourtReservations;
using MyTennisPartner.Models.CourtReservations.TennisBookings;

namespace MyTennisPartner.Web.Services.Reservations
{
    public interface ICourtReservationService
    {
        CourtOpeningCollection GetCourtAvailability(string username, string password, DateTime dateUtc, bool includeFutureDates=false);
        bool Login(string username, string password);
        int LoginTest(string username, string password);
        bool ReserveCourts(string username, string password, ReservationDetails reservation);
        void SetHost(string hostname);
    }
}