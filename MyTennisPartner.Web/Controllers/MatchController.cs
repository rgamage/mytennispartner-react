using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.ViewModels;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.Utilities;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Web.Exceptions;
using MyTennisPartner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MyTennisPartner.Web.Models;
using AutoMapper.QueryableExtensions;
using System.Diagnostics;
using MyTennisPartner.Web.Background;
using System;
using MyTennisPartner.Web.Managers;
using MyTennisPartner.Models.Users;

namespace MyTennisPartner.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/Match")]
    [Authorize]
    public class MatchController : BaseController
    {
        private readonly MatchManager _matchManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBackgroundNotificationQueue _queue;
        private readonly ReservationManager _reservationManager;

        public MatchController(TennisContext context, MatchManager matchManager, ILogger<MatchController> logger,
            NotificationService notificationService, UserManager<ApplicationUser> userManager, IBackgroundNotificationQueue queue,
            ReservationManager reservationManager):
            base(logger, notificationService, context)
        {
            Logger.LogInformation("match controller constructor running");
            _matchManager = matchManager;
            _userManager = userManager;
            _queue = queue;
            _reservationManager = reservationManager;
        }

        /// <summary>
        /// Builds a new match object, based on a given league.  Returns the object, but does not create it in the database
        /// The properties of the new match will be appropriate for the specified league
        /// </summary>
        /// <param name="leagueId"></param>
        /// <returns></returns>
        [HttpGet("GetNew")]
        public async Task<IActionResult> GetNewMatch(int leagueId)
        {
            var match = await _matchManager.GetNewMatch(leagueId);
            var matchViewModel = ModelMapper.Map<MatchViewModel>(match);

            return ApiOk(matchViewModel);
        }

        // GET: api/Match/ByLeague
        [HttpGet("ByLeague")]
        public async Task<IActionResult> GetMatchesByLeague(int leagueId, bool showPast=false, bool showFuture=true, int page=1, int pageSize=20)
        {
            var matchViewModels = await _matchManager.GetMatchesByLeague(leagueId, showPast, showFuture, page, pageSize);
            return ApiOk(matchViewModels);
        }

        // GET: api/Match/ByMember
        [HttpGet("ByMember")]
        public async Task<IActionResult> GetMatchesByMember(int memberId, bool showPast = false, bool showFuture = true, int page = 1, int pageSize = 5)
        {
            Logger.LogInformation("Getting list of matches by member ");
            var matchViewModels = await _matchManager.GetMatchesByMember(memberId, showPast, showFuture, page, pageSize);
            return ApiOk(matchViewModels);
        }

        [HttpGet("Prospective")]
        public async Task<IActionResult> GetProspectiveMatches(int memberId)
        {
            Logger.LogInformation($"Getting prospective matches by member {memberId}");
            var t = Stopwatch.StartNew();
            var matches = await _matchManager.GetProspectiveMatches(memberId);
            t.Stop();
            Logger.LogInformation($"Finished get ProspectiveMatches in {t.ElapsedMilliseconds} ms");
            return ApiOk(matches);
        }

        // GET: api/Match/5
        [ApiValidationFilter]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMatch([FromRoute] int id)
        {
            var match = await Context.Matches
                .Where(m => m.MatchId == id)
                .ProjectTo<MatchViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (match == null)
            {
                return ApiNotFound($"No match found with id={id}");
            }

            return ApiOk(match);
        }

        // PUT: api/Match/5
        [ApiValidationFilter]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMatch([FromRoute] int id, [FromBody] MatchViewModel matchViewModel)
        {
            var stop = new Stopwatch();
            stop.Start();
            if (matchViewModel is null)
            {
                return ApiBad("match", "Match to update is null");
            }
            if (id != matchViewModel.MatchId)
            {
                return ApiNotFound($"No match found with id={id}");
            }

            var newMatch = ModelMapper.Map<Match>(matchViewModel);
            Logger.LogInformation($"PutMatch 1, elapsed = {stop.ElapsedMilliseconds} ms");
            try
            {
                newMatch = await _matchManager.UpdateMatchAsync(newMatch);
                Logger.LogInformation($"PutMatch 2, elapsed = {stop.ElapsedMilliseconds} ms");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MatchExists(id))
                {
                    return ApiNotFound($"No match found with id = {id}");
                }
                else
                {
                    throw;
                }
            }

            var mappedMatch = await Context.Matches
                .Where(m => m.MatchId == newMatch.MatchId)
                .ProjectTo<MatchViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            Logger.LogInformation($"PutMatch 3, elapsed = {stop.ElapsedMilliseconds} ms");

            // notify users of any events that were recorded during this operation
            // note - this is the synchronous way to do this, in the scope of this request, which has been 
            // commented out because it takes 3-5 seconds to execute
            //await _notificationService.NotifyAsync(_matchManager.NotificationEvents, HttpContext.Request.Scheme, HttpContext.Request.Host.ToString());

            // this is the background method of sending notifications.  Will execute in background, allowing
            // our web request to return promptly to client
            QueueNotifications();

            Logger.LogInformation($"PutMatch 4, elapsed = {stop.ElapsedMilliseconds} ms");

            return ApiOk(mappedMatch);
        }

        // POST: api/Match
        [ApiValidationFilter]
        [HttpPost]
        public async Task<IActionResult> PostMatch([FromBody] Match match)
        {
            Context.Matches.Add(match);
            await Context.SaveChangesAsync();

            return ApiOk(match);
        }

        /// <summary>
        /// delete match
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/Match/5
        [ApiValidationFilter]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMatch([FromRoute] int id)
        {
            var match = await _matchManager.DeleteMatch(id);

            // notify users of any events that were recorded during this operation
            QueueNotifications();

            return ApiOk(match);
        }

        private bool MatchExists(int id)
        {
            return Context.Matches.Any(e => e.MatchId == id);
        }

        /// <summary>
        /// set/clear isUnableToPlayMatch flag for this match, them member
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="memberId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ApiValidationFilter]
        [HttpPut("UpdateAvailability")]
        public async Task<IActionResult> UpdateAvailability([FromBody]UpdateAvailabilityRequest request)
        {
            var player = await _matchManager.UpdateAvailability(request);
            QueueNotifications();

            return ApiOk(player);
        }
        
        /// <summary>
        /// player can respond to match by submitting their availability
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="memberId"></param>
        /// <param name="leagueId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("RespondToMatch")]
        [ApiValidationFilter]
        public async Task<IActionResult> RespondToMatch([FromBody]UpdateAvailabilityRequest request)
        {
            var name = _userManager.GetUserName(User);
            if (name == null)
            {
                return ApiNotFound("User not found - member may not be logged in");
            }
            if (request == null)
            {
                return ApiBad("Request", "Availability request is null");
            }

            var appUser = await _userManager.FindByNameAsync(name);
            if (request.MemberId > 0)
            {
                // we have a referring member Id, so we are responding to a sub request
                await _matchManager.UpdateMatchPlayer(appUser.Id, request.MatchId, request.LeagueId, request.Value, request.MemberId);
            }
            else
            {
                // we have no referring member, so we are just responding to a new match, setting our availability
                var memberId = await Context
                    .Members
                    .Where(m => m.UserId == appUser.Id)
                    .Select(m => m.MemberId)
                    .FirstOrDefaultAsync();
                request.MemberId = memberId;

                var player = await _matchManager.UpdateAvailability(request);
            }

            // notify members if necessary
            QueueNotifications();

            return ApiOk();
        }

        /// <summary>
        /// for a given member, return upcoming matches for which he has not responded 
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [HttpGet("GetUnansweredAvailability")]
        public async Task<IActionResult> GetUnansweredAvailability(int memberId)
        {
            var avail = await _matchManager.GetUnansweredAvailabilities(memberId);
            return ApiOk(avail);
        }

        /// <summary>
        /// get league availability
        /// </summary>
        /// <param name="leagueId"></param>
        /// <returns></returns>
        [HttpGet("GetLeagueAvailability")]
        public async Task<IActionResult> GetLeagueAvailability(int leagueId)
        {
            var leagueAvail = await _matchManager.GetLeagueAvailabilityGrid(leagueId);
            return ApiOk(leagueAvail);
        }

        /// <summary>
        /// reserve courts for a given match
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        [HttpPost("ReserveCourtsForMatch")]
        public async Task<bool> ReserveCourts([FromQuery]int matchId)
        {
            var result = await _reservationManager.ReserveCourtsSingleMatch(matchId);
            return result;
        }

        /// <summary>
        /// queue any notifications to the background notification service
        /// </summary>
        [NonAction]
        private void QueueNotifications()
        {
            var nwi = new NotificationWorkItem
            {
                Events = _matchManager.NotificationEvents,
                RequestHost = HttpContext.Request.Host.ToString(),
                RequestScheme = HttpContext.Request.Scheme
            };
            _queue.QueueBackgroundNotificationWorkItem(nwi);
        }

    }
}