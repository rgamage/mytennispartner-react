using MyTennisPartner.Data.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Models.ViewModels;
using MyTennisPartner.Models.Exceptions;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Data.Context;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyTennisPartner.Models.Utilities;
using AutoMapper.QueryableExtensions;
using System;
using MyTennisPartner.Models.Enums;

namespace MyTennisPartner.Data.Managers
{
    /// <summary>
    /// class to manage data access for Lines
    /// </summary>
    public class LineManager : ManagerBase
    {
        private readonly MemberManager memberManager;
        #region constructor
        public LineManager(TennisContext context, ILogger<LineManager> logger, MemberManager memberManager) : base(context, logger) {
            this.memberManager = memberManager;
        }
        #endregion

        /// <summary>
        /// get all lines
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Line>> GetAllLinesAsync()
        {
            var lines = await Context.Lines.ToListAsync();
            return lines;
        }

        /// <summary>
        /// get line
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Line> GetLineAsync(int id)
        {
            var line = await Context.Lines.FindAsync(id);
            if (line == null)
            {
                throw new NotFoundException($"Line not found: id = {id}");
            }
            return line;
        }

        /// <summary>
        /// get lines by match
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<LinesWithAvailabilityViewModel> GetLinesByMatchAsync(int matchId, bool withSubs = true)
        {
            var match = Context.Matches.FirstOrDefault(m => m.MatchId == matchId);
            if (match == null)
            {
                throw new Exception($"Match not found for matchId = {matchId}");
            }

            var lines = await Context.Lines
                .Where(l => l.MatchId == matchId)
                .ProjectTo<LineViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .ToListAsync();

            if (lines.Any())
            {
                lines = lines
                    .OrderBy(l => StringHelper.ZeroPadLeft(l.CourtNumber))
                    .ToList();
            }

            var players = await Context.Players
                .Where(p => p.MatchId == matchId)
                .ProjectTo<PlayerViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .ToListAsync();

            if (match.AutoReserveCourts)
            {
                foreach (var line in lines)
                {
                    // warn if there are no players in this line who can reserve a court, and the court is not already reserved, and the court is not empty
                    line.LineWarning = !players.Any(p => p.LineId == line.LineId && p.CanReserveCourt) && !line.IsReserved && players.Any(p => p.LineId == line.LineId);
                }
            }

            var subs = new List<MemberNameViewModel>();
            if (withSubs)
            {
                subs = await memberManager.GetOrderedSubList(match.MatchId);
                if (subs.Any(s => s.Availability == Availability.Confirmed))
                {
                    // if we have one or more confirmed players on the sub list, then let's just limit the list to them,
                    // so we don't clutter the list with all the people who have not bothered to respond
                    subs = subs.Where(s => s.Availability == Availability.Confirmed)
                        .ToList();
                }
            }

            var lwa = new LinesWithAvailabilityViewModel
            (
                lines,
                players,
                subs
            );

            return lwa;
        }

        /// <summary>
        /// Update a line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public async Task<Line> UpdateLineAsync(Line line)
        {
            Context.Lines.Update(line);
            await Context.SaveChangesAsync();
            return line;
        }

        /// <summary>
        /// create new line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public async Task<Line> CreateLineAsync(Line line)
        {
            Context.Lines.Add(line);
            await Context.SaveChangesAsync();
            return line;
        }

        /// <summary>
        /// remove a line from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Line> RemoveLineAsync(int id)
        {
            var existingLine = Context.Lines.Find(id);
            Context.Lines.Remove(existingLine);
            await Context.SaveChangesAsync();
            return existingLine;
        }

        /// <summary>
        /// sets court numbers according to target courts identified by auto-reserve algorithm, so players can see 
        /// which courts the system will try to reserve
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="targetCourts"></param>
        /// <returns></returns>
        public async Task<bool> UpdateTargetCourts(int matchId, List<string> targetCourts, string courtsAvailable)
        {
            if (targetCourts is null) return false;
            var lines = await Context.Lines.Where(l => l.MatchId == matchId).ToListAsync();
            if (lines.Count > targetCourts.Count) return false;

            lines = lines
                .OrderBy(l => StringHelper.ZeroPadLeft(l.CourtNumber))
                .ToList();

            var i = 0;
            foreach(var line in lines)
            {
                if (!line.IsReserved && !line.CourtNumberOverridden)
                {
                    line.CourtNumber = targetCourts[i];
                }
                line.CourtsAvailable = courtsAvailable;
                i++;
            }
            Context.Lines.UpdateRange(lines);
            await Context.SaveChangesAsync();
            return true;
        }
    }
}
