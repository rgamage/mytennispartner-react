using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.Enums;
using MyTennisPartner.Web.Exceptions;
using MyTennisPartner.Web.Managers;
using MyTennisPartner.Web.Services;
using MyTennisPartner.Web.Services.Reservations;

namespace MyTennisPartner.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/Reservation")]
    //[Authorize]
    public class ReservationController : BaseController
    {

        #region members
        private readonly CourtReservationServiceSpareTime service;
        private readonly TennisContext database;
        private readonly ReservationManager _reservationManager;
        #endregion

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        public ReservationController(
            CourtReservationServiceSpareTime service, 
            ILogger<ReservationController> logger, 
            NotificationService notificationService,
            TennisContext context, 
            ReservationManager reservationManager
            ) : base(logger, notificationService) {

            this.service = service;
            database = context;
            _reservationManager = reservationManager;
        }

        #endregion

        #region post
        // POST: api/Reservation/login-test
        [HttpPost("login-test")]
        [ApiValidationFilter]
        public IActionResult LoginTest([FromBody]Credentials creds)
        {
            if (creds is null) return ApiBad("credentials", "credentials cannot be null");

            service.SetHost(creds.Host);
            var result = service.LoginTest(creds.Username, creds.Password);
            return ApiOk(result);
        }

        /// <summary>
        /// scan all SpareTime reservation sites to test user's ability to log in at each
        /// </summary>
        /// <param name="creds"></param>
        /// <returns></returns>
        [HttpPost("TestSparetimeLogins")]
        [ApiValidationFilter]
        public IActionResult TestSparetimeLogins([FromBody]Credentials creds)
        {
            if (creds is null) return ApiBad("credentials", "credentials cannot be null");
            var results = new List<ReservationServiceLoginResult>();

            var spareTimeVenues = database.Venues
                .Include(v => v.ReservationSystem)
                .Where(v => v.ReservationSystem.CourtReservationProvider == CourtReservationProvider.TennisBookings)
                .OrderBy(v => v.Name)
                .ToList();

            foreach(var venue in spareTimeVenues)
            {
                service.SetHost(venue.ReservationSystem.HostName);
                var result = new ReservationServiceLoginResult
                {
                    VenueId = venue.VenueId,
                    VenueName = venue.Name,
                    MemberNumber = service.LoginTest(creds.Username, creds.Password)
                };
                results.Add(result);
            }

            return ApiOk(results);
        }

        /// <summary>
        /// triggers the auto reserve of courts, that normally runs at 5am daily
        /// </summary>
        /// <returns></returns>
        [HttpPost("AutoReserve")]
        public async Task<IActionResult> AutoReserveCourtsAsync()
        {
            var success = await _reservationManager.ReserveAllCourts();
            if (success)
            {
                return Ok();
            }
            return ApiFailed("Failed to reserve courts");
        }

        [HttpPost("UpdateTargetCourts")]
        public async Task<IActionResult> UpdateTargetCourts(int matchId = 0)
        {
            await _reservationManager.UpdateTargetCourts(matchId);
            var courtsAvailable = _reservationManager.TargetCourtInfoList.Any() ? _reservationManager.TargetCourtInfoList[0].CourtsAvailable : "";
            return Ok(courtsAvailable);
        }

        #endregion
    }

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

    /// <summary>
    /// class to hold court reservation credentials passed from client
    /// </summary>
    public class Credentials
    {
        [Required]
        public string Host { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}