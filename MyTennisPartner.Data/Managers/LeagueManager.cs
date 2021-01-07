using MyTennisPartner.Data.Context;
using System;
using System.Collections.Generic;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Data.DataTransferObjects;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Models.ViewModels;
using MyTennisPartner.Models.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MyTennisPartner.Data.Managers
{
    public class LeagueManager: ManagerBase
    {

        #region constructor
        public LeagueManager(TennisContext context, ILogger<LeagueManager> logger): base(context, logger) { }
        #endregion

        #region get
        /// <summary>
        /// get all leagues
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<League>> GetAllLeaguesAsync()
        {
            var leagues = await Context.Leagues.ToListAsync();
            return leagues;
        }

        public async Task<League> GetLeagueAsync(int id)
        {
            var league = await Context.Leagues.FirstOrDefaultAsync(l => l.LeagueId == id);

            if (league == null)
            {
                throw new NotFoundException($"No league found with id={id}");
            }

            return league;
        }

        /// <summary>
        /// search leagues by name , limit to number of results, include venue
        /// </summary>
        /// <param name="search"></param>
        /// <param name="numResults"></param>
        /// <returns></returns>
        public async Task<IEnumerable<League>> SearchLeaguesAsync(string search, int numResults)
        {
            var leagues = Context.Leagues
                .Include(l => l.HomeVenue)
                .Include(l => l.Owner)
                .Where(l => l.Name.Contains(search) || string.IsNullOrEmpty(search))
                .OrderBy(l => l.Name)
                .Take(numResults)
                .ToListAsync();

            return await leagues;
        }

        /// <summary>
        /// get leagues in which the specified member is a player, or owner of the league
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task <IEnumerable<League>> GetLeaguesByMemberAsync(int memberId, int pageSize)
        {
            var leagues = await Context.Leagues
                .Include(l => l.HomeVenue)
                .Include(l => l.Owner)
                .Where(l => l.LeagueMembers.Any(lm => lm.MemberId == memberId) || l.OwnerMemberId == memberId)
                .OrderBy(l => l.Name)
                .Take(pageSize)
                .ToListAsync();

            return leagues;
        }

        /// <summary>
        /// get league summary
        /// </summary>
        /// <param name="LeagueId"></param>
        /// <returns></returns>
        public LeagueSummary GetLeagueSummary(int LeagueId)
        {
            var leagueInfo = Context.Leagues
                .AsNoTracking()
                .Include(l => l.Owner)
                .Include(l => l.HomeVenue).ThenInclude(v => v.VenueAddress)
                .Include(l => l.HomeVenue).ThenInclude(v => v.VenueContact)
                .Where(l => l.LeagueId == LeagueId)
                .Select(l => new
                {
                    summary = l,
                    regs = l.LeagueMembers.Where(m => !m.IsSubstitute).Count(),
                    subs = l.LeagueMembers.Where(m => m.IsSubstitute).Count(),
                    upcoming = l.Matches.Where(m => m.StartTime >= DateTime.Today).Count(),
                    total = l.Matches.Count()
                })
                .FirstOrDefault();

            if (leagueInfo == null) return null;

            var leagueSummary = ModelMapper.Map<LeagueSummary>(leagueInfo.summary);
            leagueSummary.RegularMemberCount = leagueInfo.regs;
            leagueSummary.SubMemberCount = leagueInfo.subs;
            leagueSummary.UpcomingMatchCount = leagueInfo.upcoming;
            leagueSummary.TotalMatchCount = leagueInfo.total;

            return leagueSummary;
        }

        /// <summary>
        /// get league search by member
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<IEnumerable<LeagueSearchViewModel>> GetLeagueSearchByMemberAsync(int memberId, int pageSize)
        {
            var leagues = await GetLeaguesByMemberAsync(memberId, pageSize);

            var leagueViewModels = ModelMapper.Map<IEnumerable<LeagueSearchViewModel>>(leagues);

            // now we need to populate the IsCaptain field, if requester is a captain of this league
            var leagueIds = leagueViewModels.Select(l => l.LeagueId).ToList();
            var captainLeagueIds = await Context.LeagueMembers
                .Where(lm => leagueIds.Any(li => li == lm.LeagueId))
                .Where(lm => lm.MemberId == memberId)
                .Where(lm => lm.IsCaptain)
                .Select(lm => lm.LeagueId)
                .ToListAsync();

            foreach (var league in leagueViewModels.Where(l => captainLeagueIds.Any(lid => lid == l.LeagueId)))
            {
                league.IsCaptain = true;
            }

            foreach (var league in leagueViewModels)
            {
                league.IsOwner = league.OwnerMemberId == memberId;
            }

            return leagueViewModels;
        }

        /// <summary>
        /// get league summary viewmodel
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<LeagueSummaryViewModel> GetLeagueSummaryViewModelAsync(int leagueId, int memberId)
        {
            var leagueSummary = GetLeagueSummary(leagueId);

            if (leagueSummary == null)
            {
                throw new BadRequestException($"GetLeagueSummaryViewModel: League with leagueId = {leagueId} does not exist");
            }

            var leagueSummaryViewModel = ModelMapper.Map<LeagueSummaryViewModel>(leagueSummary);

            if (memberId != 0)
            {
                var leagueMember = await Context.LeagueMembers
                    .Where(lm => lm.LeagueId == leagueId)
                    .Where(lm => lm.MemberId == memberId)
                    .FirstOrDefaultAsync();

                leagueSummaryViewModel.IsCaptain = leagueMember?.IsCaptain ?? false;
            }

            return leagueSummaryViewModel;
        }

        /// <summary>
        /// method to determine if a given memberId is either the owner or captain of a given league
        /// used to determine if the member can edit the league
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<bool> IsLeagueOwnerOrCaptainAsync(int leagueId, int memberId)
        {
            var league = await Context.Leagues.FindAsync(leagueId);
            if (league == null)
            {
                throw new BadRequestException($"IsLeagueOwnerOrCaptain: Cannot find leagueId={leagueId}"); 
            }
            var member = await Context.Members.FindAsync(memberId);
            if (member == null)
            {
                throw new BadRequestException($"IsLeagueOwnerOrCaptain: Cannot find memberId={memberId}");
            }
            if (league.OwnerMemberId == memberId)
            {
                // member is the owner of the league
                return true;
            }

            var isCaptain = await Context.LeagueMembers
                .AnyAsync(lm => lm.LeagueId == leagueId
                             && lm.MemberId == memberId
                             && lm.IsCaptain);

            return isCaptain;
        }
        #endregion

        #region create
        /// <summary>
        /// create a new league, given a league summary viewmodel
        /// </summary>
        /// <param name="leagueSummaryViewModel"></param>
        /// <returns></returns>
        public async Task<League> CreateLeagueAsync(LeagueSummaryViewModel leagueSummaryViewModel)
        {
            var league = ModelMapper.Map<League>(leagueSummaryViewModel);
            Context.Leagues.Add(league);
            await Context.SaveChangesAsync();
            return league;
        }
        #endregion

        #region update
        /// <summary>
        /// update a league
        /// </summary>
        /// <param name="leagueSummaryViewModel"></param>
        /// <returns></returns>
        public async Task<League> UpdateLeagueAsync(LeagueSummaryViewModel leagueSummaryViewModel)
        {
            var updatedLeague = ModelMapper.Map<League>(leagueSummaryViewModel);
            Context.Leagues.Update(updatedLeague);
            await Context.SaveChangesAsync();
            return updatedLeague;
        }
        #endregion

        #region delete

        /// <summary>
        /// remove a league
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<League> RemoveLeagueAsync(int id)
        {
            var league = await Context.Leagues
                .Include(l => l.Matches)
                .ThenInclude(m => m.Lines)
                .Include(l => l.Matches)
                .ThenInclude(m => m.Players)
                .SingleOrDefaultAsync(l => l.LeagueId == id);
            if (league == null)
            {
                throw new NotFoundException($"Unable to delete league.  League with id={id} not found.");
            }

            // need to delete any matches associated with this league
            var matches = league.Matches;
            if (matches != null)
            {
                // remove lines
                Context.Lines.RemoveRange(matches.SelectMany(m => m.Lines));

                // remove players
                Context.Players.RemoveRange(matches.SelectMany(m => m.Players));

                // remove matches
                Context.Matches.RemoveRange(matches);
            }

            Context.Leagues.Remove(league);
            await Context.SaveChangesAsync();
            return league;
        }
        #endregion
    }
}
