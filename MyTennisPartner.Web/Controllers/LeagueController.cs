using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Models.ViewModels;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MyTennisPartner.Web.Models;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Web.Exceptions;
using MyTennisPartner.Web.Services;
using MyTennisPartner.Models.Users;

namespace MyTennisPartner.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/League")]
    [Authorize]
    public class LeagueController : BaseController
    {
        private readonly LeagueManager _leagueManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeagueController(TennisContext context, LeagueManager leagueManager, UserManager<ApplicationUser> userManager, ILogger<LeagueController> logger, NotificationService notificationService)
            : base(logger, notificationService, context)
        {
            Logger.LogInformation("league controller constructor running");
            _leagueManager = leagueManager;
            _userManager = userManager;
        }

        // GET: api/League/Search
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string search, int numResults = 20)
        {
            var leagues = await _leagueManager.SearchLeaguesAsync(search, numResults);
            var leagueViewModels = ModelMapper.Map<IEnumerable<LeagueSearchViewModel>>(leagues);
            return ApiOk(leagueViewModels);
        }

        // GET: api/League/ByMember
        [HttpGet("ByMember")]
        public async Task<IActionResult> GetByMember(int memberId, int pageSize = 20)
        {
            Logger.LogInformation($"Getting list of leagues by member {memberId}");
            var leagueSearchViewModels = await _leagueManager.GetLeagueSearchByMemberAsync(memberId, pageSize);
            return ApiOk(leagueSearchViewModels);
        }

        // GET: api/League/Summary
        [HttpGet("Summary/{id}")]
        public async Task<IActionResult> Summary([FromRoute]int id)
        {
            var userName = _userManager.GetUserName(User);
            var appUser = await _userManager.FindByNameAsync(userName);
            if (appUser == null)
            {
                return ApiNotFound("Could not find user - may not be logged in");
            }
            var member = await Context.Members
                .Where(m => m.UserId == appUser.Id)
                .FirstOrDefaultAsync();

            var leagueSummaryViewModel = await _leagueManager.GetLeagueSummaryViewModelAsync(id, member?.MemberId ?? 0);

            return ApiOk(leagueSummaryViewModel);
        }

        /// <summary>
        /// method to determine if member can edit the referenced league
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="matchId"></param>
        /// <returns></returns>
        [HttpGet("GetLeagueEditAbility/{leagueId}/{memberId}")]
        public async Task<IActionResult> GetLeagueEditAbility([FromRoute]int leagueId, [FromRoute]int memberId)
        {
            var canEdit = await _leagueManager.IsLeagueOwnerOrCaptainAsync(leagueId, memberId);
            return ApiOk(canEdit);
        }

        // GET: api/League
        [HttpGet]
        public async Task<IActionResult> GetLeagues()
        {
            var leagues = await _leagueManager.GetAllLeaguesAsync();
            return ApiOk(leagues);
        }


        // GET: api/League/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeague([FromRoute] int id)
        {
            var league = await _leagueManager.GetLeagueAsync(id);
            return ApiOk(league);
        }

        // PUT: api/League/5
        [HttpPut("{id}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutLeague([FromRoute]int id, [FromBody] LeagueSummaryViewModel leagueSummaryViewModel)
        {
            if (leagueSummaryViewModel is null)
            {
                return ApiBad("leagueSummaryViewModel", "cannot be null");
            }
            if (id != leagueSummaryViewModel.LeagueId)
            {
                return ApiBad("leagueId", $"League Id in path ({id}) does not match League in body ({leagueSummaryViewModel.LeagueId})");
            }

            var updatedLeague = await _leagueManager.UpdateLeagueAsync(leagueSummaryViewModel);
            return ApiOk(updatedLeague);
        }

        // POST: api/League
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostLeague([FromBody] LeagueSummaryViewModel leagueSummaryViewModel)
        {
            var league = await _leagueManager.CreateLeagueAsync(leagueSummaryViewModel);

            return ApiOk(new { leagueId = league.LeagueId });
        }

        // DELETE: api/League/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeague([FromRoute] int id)
        {
            var league = await _leagueManager.RemoveLeagueAsync(id);
            return ApiOk(league);
        }

    }
}