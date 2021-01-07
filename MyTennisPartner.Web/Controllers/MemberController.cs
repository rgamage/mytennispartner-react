using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Web.Utilities;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.ViewModels;
using MyTennisPartner.Web.Data;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Web.Exceptions;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Models.Enums;
using MyTennisPartner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using MyTennisPartner.Web.Background;
using Microsoft.AspNetCore.Identity;
using MyTennisPartner.Models.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace MyTennisPartner.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/Member")]
    [Authorize]
    public class MemberController : BaseController
    {
        private readonly UserDbContext _userDbContext;
        private readonly MemberManager _memberManager;
        private readonly IBackgroundNotificationQueue _queue;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _environment;
        private readonly IEmailSender _emailSender;
        private readonly EmailSenderOptions _emailOptions;

        #region constructor
        public MemberController(TennisContext context, UserDbContext userDbContext, ILogger<MemberController> logger, MemberManager memberManager, NotificationService notificationService,
            IBackgroundNotificationQueue queue, UserManager<ApplicationUser> userManager, IHostingEnvironment environment, IEmailSender sender, IOptions<EmailSenderOptions> emailOptions) : base(logger, notificationService, context)
        {
            _emailOptions = emailOptions?.Value;
            _userDbContext = userDbContext;
            _memberManager = memberManager;
            _userManager = userManager;
            _environment = environment;
            _emailSender = sender;
            _queue = queue;
        }
        #endregion


        #region members
        /// <summary>
        /// search for members by first/last name
        /// optionally, limit results to members of a given league
        /// optionally, limit results to member that are NOT already in the line-up for a given match
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Search")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> Search(string search, Gender gender, int leagueId = 0, int excludeLeagueId = 0, int numResults = 10)
        {
            var members = await _memberManager.SearchAsync(search, gender, leagueId, excludeLeagueId, numResults);
            var memberNames = ModelMapper.Map<List<MemberNameViewModel>>(members);
            memberNames = memberNames
                .OrderBy(m => m.IsSubstitute)
                .ThenBy(m => m.FirstName)
                .ToList();

            return ApiOk(memberNames);
        }

        [ApiValidationFilter]
        [HttpPost]
        [Route("GetPlayerPickList")]
        //[ResponseCache(Duration =60)]
        public async Task<IActionResult> GetPlayerPickList([FromBody]MatchViewModel match)
        {
            if (match is null) return ApiBad("match", "Cannot be null");
            var matchId = match.MatchId;
            var leagueId = match.LeagueId;
            var playersInLineup = match.Players?.Where(p => p.LineId >= 0).ToList();
            var players = await _memberManager.GetPlayerPickList(leagueId, matchId, playersInLineup);
            players = players
                .OrderBy(m => m.IsSubstitute)
                .ThenBy(m => m.FirstName)
                .ToList();

            return ApiOk(players);
        }

        // GET: api/GetSubPickList
        [HttpGet]
        [Route("GetSubPickList")]
        public async Task<IActionResult> GetSubPickList([FromQuery]int matchId)
        {
            var members = await _memberManager.GetOrderedSubList(matchId);
            return ApiOk(members);
        }

        // GET: api/Member
        [HttpGet]
        public async Task<IActionResult> GetMembers()
        {
            var members = await _memberManager.GetMembersAsync();
            var memberViewModels = ModelMapper.Map<List<MemberViewModel>>(members)
                .OrderBy(m => m.LastName);
                
            return ApiOk(memberViewModels); 
        }

        // GET: api/Member/5
        [ApiValidationFilter]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember([FromRoute] int id)
        {
            var member = await _memberManager.GetMemberAsync(id);

            if (member == null)
            {
                return ApiNotFound($"No member found with id={id}");
            }

            return ApiOk(member);
        }

        // GET: api/Member/GetMemberByUser/5
        [ApiValidationFilter]
        [HttpGet("[action]/{userId}")]
        public async Task<IActionResult> GetMemberByUser([FromRoute] string userId)
        {
            var member = await _memberManager.GetMemberByUserAsync(userId);
            if (member == null)
            {
                //return ApiNotFound($"Member not found for UserId = {userId}");
                // don't want to return a 404 here, because that shows as a "Failed" request in metrics,
                // so instead just return an empty Ok
                return ApiOk();
            }

            var memberViewModel = ModelMapper.Map<MemberViewModel>(member);

            var user = await _userDbContext.Users.FindAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            memberViewModel.IsAdmin = isAdmin;

            return ApiOk(memberViewModel);
        }

        // PUT: api/Member/5
        [ApiValidationFilter]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember([FromRoute] int id, [FromBody] MemberViewModel memberViewModel)
        {
            if (memberViewModel is null)
            {
                return ApiBad("memberViewModel", "memberViewModel is null");
            }

            if (id != memberViewModel.MemberId)
            {
                return ApiBad("", $"MemberId in URL ({id}) does not match MemberId in model ({memberViewModel.MemberId})");
            }
            var member = ModelMapper.Map<Member>(memberViewModel);

            var memberUpdated = await _memberManager.UpdateMemberAsync(member);

            return ApiOk(memberUpdated);
        }

        // POST: api/Member
        [ApiValidationFilter]
        [HttpPost]
        public async Task<IActionResult> PostMember([FromBody] MemberViewModel memberViewModel)
        {
            //if (memberViewModel.BirthYear > ValidationHelper.GetMaxBirthyear() || memberViewModel.BirthYear < ValidationHelper.GetMinBirthyear())
            //{
            //    return ApiBad("birthYear", $"Birth year must be between {ValidationHelper.GetMinBirthyear()} and {ValidationHelper.GetMaxBirthyear()}.");
            //}

            if (memberViewModel != null)
            {
                var user = await _userManager.FindByIdAsync(memberViewModel.UserId);

                // keep email address fields in sync, always use user db as reference
                memberViewModel.Email = user.Email;

                var member = ModelMapper.Map<Member>(memberViewModel);

                member = await _memberManager.CreateMemberAsync(member);
                await SendEmailNoticeNewMember(user, _emailOptions.SiteAdminEmail);
                return ApiOk(member);
            }

            return ApiBad("member", "member viewmodel is null");
        }

        // DELETE: api/Member/5
        [ApiValidationFilter]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember([FromRoute] int id)
        {
            var member = await _memberManager.DeleteMemberAsync(id);
            return ApiOk(member);
        }

        [AllowAnonymous]
        [HttpGet("syncEmails")]
        public async Task<IActionResult> SyncEmails()
        {
            var members = (await _memberManager.GetMembersAsync()).ToList();
            var users = await _userManager.Users.ToListAsync();
            foreach(var member in members)
            {
                member.Email = users.FirstOrDefault(u => u.Id == member.UserId)?.Email;
            }
            await _memberManager.UpdateMembers(members);
            return ApiOk();
        }
        #endregion  


        #region leaguemembers
        // DELETE: api/leaguemembers/5/2
        [ApiValidationFilter]
        [HttpDelete("leaguemembers/{leagueId}/{memberId}")]
        public async Task<IActionResult> DeleteMember([FromRoute] int leagueId, [FromRoute] int memberId)
        {
            var leagueMember = await _memberManager.DeleteLeagueMemberAsync(leagueId, memberId);
            return ApiOk(leagueMember);
        }
        
        // GET: api/Member/5
        [ApiValidationFilter]
        [HttpGet("leaguemembers/{leagueId}")]
        public async Task<IActionResult> GetLeagueMembersByLeague([FromRoute]int leagueId)
        {
            var members = await _memberManager.GetMembersByLeagueAsync(leagueId);
            var uids = members.Select(m => m.UserId).ToList();

            return ApiOk(members);
        }

        /// <summary>
        /// method to update leaguemember
        /// </summary>
        /// <param name="memberViewModel"></param>
        /// <returns></returns>
        [ApiValidationFilter]
        [HttpPut("leaguemembers")]
        public async Task<IActionResult> UpdateLeagueMember([FromBody] MemberViewModel memberViewModel)
        {
            if (memberViewModel == null || memberViewModel.LeagueId == 0 || memberViewModel.MemberId == 0)
            {
                return ApiBad("", "Missing or invalid league or member ID");
            }
            var leagueMember = await _memberManager.UpdateLeagueMemberAsync(memberViewModel);
            return ApiOk(leagueMember);
        }

        /// <summary>
        /// method to add league member
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="memberId"></param>
        /// <param name="isSub"></param>
        /// <returns></returns>
        // POST: api/LeagueMembers/5/10
        [HttpPost("leaguemembers/{leagueId}/{memberId}")]
        public async Task<IActionResult> AddLeagueMember([FromRoute]int leagueId, [FromRoute]int memberId, [FromQuery]bool isSub=false)
        {
            var newLeagueMember = await _memberManager.AddLeagueMemberAsync(leagueId, memberId, isSub);
            QueueNotifications();
            return ApiOk(newLeagueMember);
        }

        #endregion


        #region helpers
        /// <summary>
        /// queue any notifications to the background notification service
        /// </summary>
        [NonAction]
        private void QueueNotifications()
        {
            var nwi = new NotificationWorkItem
            {
                Events = _memberManager.NotificationEvents,
                RequestHost = HttpContext.Request.Host.ToString(),
                RequestScheme = HttpContext.Request.Scheme
            };
            _queue.QueueBackgroundNotificationWorkItem(nwi);
        }

        [NonAction]
        private async Task SendEmailNoticeNewMember(ApplicationUser user, string emailAddress)
        {
            // Send email notification to site admin that a new user has registered
            await _emailSender.SendEmailAsync(emailAddress, $"New member profile created in {_environment.EnvironmentName}",
                $"A new member profile has been created on the website.  Name = {user.FirstName} {user.LastName}, Email = {user.Email}"
            );

            Logger.LogInformation($"Sent new member profile email (user id: {user.Id})");
        }

        #endregion
    }
}