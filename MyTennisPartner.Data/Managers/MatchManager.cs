using MyTennisPartner.Data.Context;
using System;
using System.Linq;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Data.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MyTennisPartner.Models.ViewModels;
using MyTennisPartner.Models.Exceptions;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Models.Enums;
using AutoMapper.QueryableExtensions;
using MyTennisPartner.Models.Utilities;

namespace MyTennisPartner.Data.Managers
{

    public class MatchManager : ManagerBase
    {
        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public MatchManager(TennisContext context, ILogger<MatchManager> logger) : base(context, logger) {
        }
        #endregion

        #region sessions
        //public void ScheduleSession(int leagueId, DateTime startDate, DateTime endDate)
        //{
        //    var league = _context.Leagues
        //        .Where(l => l.LeagueId == leagueId)
        //        .FirstOrDefault();

        //    var frequency = league.MeetingFrequency;
        //    var numberOfMatches = league.NumberMatchesPerSession;

        //    // note - incomplete
        //}
        #endregion

        #region helpers
        /// <summary>
        /// given a notification event, adds basic info about a match to the description field
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public void AddMatchSummaryDescription(ref NotificationEvent evt, int matchId)
        {
            if (evt is null) return;
            var match = GetMatchById(matchId);
            if (match == null) return;

            var matchInfo = ModelMapper.Map<MatchSummaryViewModel>(match);
            evt.MatchSummary = matchInfo;
            var description = $"<h3>{matchInfo.LeagueName}</h3><div>{matchInfo.StartTimeLocal.ToLongDateString()}</div><div>{matchInfo.StartTimeLocal.ToShortTimeString()}</div><div>At {matchInfo.VenueName}</div>";
            evt.EventDescription = description;
        }

        /// <summary>
        /// get match by id, including venue and league
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        private Match GetMatchById(int matchId)
        {
            var match = Context.Matches
                .Include(m => m.MatchVenue)
                .Include(m => m.League)
                .Where(m => m.MatchId == matchId)
                .FirstOrDefault();

            return match;
        }

        /// <summary>
        /// given a league, return next available time (in UTC) for a match, based on league's settings and any currently scheduled matches
        /// </summary>
        /// <param name="leagueId"></param>
        /// <returns></returns>
        public DateTime GetNextAvailableMatchTime(int leagueId, DateTime refTime)
        {
            var league = Context.Leagues
                .Include(l => l.Matches)
                .FirstOrDefault(l => l.LeagueId == leagueId);

            if (league == null)
            {
                throw new Exception("Could not find league");
            }
            var startHours = league.MatchStartTime.Hours;
            var startMinutes = league.MatchStartTime.Minutes;
            var dayOfWeek = (int)league.MeetingDay;

            var latestMatch = Context.Matches.Where(m => m.LeagueId == leagueId)
                    .OrderByDescending(m => m.StartTime)
                    .FirstOrDefault();

            var hasMatches = (latestMatch != null);

            if (hasMatches)
            {
                var nextMatchTime = DateTimeHelper.GetDateFromFrequency(latestMatch.StartTime, league.MeetingFrequency);
                if (nextMatchTime.IsInFuture())
                {
                    // if we already have match booked, and the next interval is in the future, then simply book the next slot after that match
                    return nextMatchTime;
                }
            }

            var startTime = new DateTime(refTime.Year, refTime.Month, refTime.Day, startHours, startMinutes, 0, DateTimeKind.Utc);
            var refTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(refTime, Utilities.DataConstants.AppTimeZoneInfo);
            if (league.MeetingDay >= refTime.DayOfWeek)
            {
                // next avail match day is today or later this week
                startTime = startTime.AddDays(league.MeetingDay - refTimeLocal.DayOfWeek);
            }
            else
            {
                // next avail match day is next week, so add appropriate days
                startTime = startTime.AddDays(7 - (refTimeLocal.DayOfWeek - league.MeetingDay));
            }
            // for the case where match is today, check if match is in future.  If not, add a week
            if ((startTime - refTime).TotalMinutes < 0)
            {
                // we missed our time slot today, so push out another week (or day in case of ad hoc freq)
                startTime = startTime.AddDays(7);
            }

            if (hasMatches)
            {
                // check if recent match and this new match are too close together.  If so, push it out
                while ((startTime - latestMatch.StartTime).TotalDays < DateTimeHelper.GetDaysFromFrequency(league.MeetingFrequency))
                {
                    startTime = startTime.AddDays(7);
                }
            }

            return startTime;
        }
        #endregion

        #region eventHandlers
        /// <summary>
        /// handle deleted line members event
        /// </summary>
        /// <param name="memberIds"></param>
        /// <param name="matchId"></param>
        /// <param name="LeagueId"></param>
        private void HandleDeletedLineMembersEvent(IEnumerable<int> memberIds, int matchId, int leagueId)
        {
            if (memberIds == null || !memberIds.Any()) return;

            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.RemovedFromMatch,
                MatchId = matchId,
                LeagueId = leagueId
            };
            myEvent.MemberIds.AddRange(memberIds);
            AddMatchSummaryDescription(ref myEvent, matchId);

            NotificationEvents.Add(myEvent);
        }

        /// <summary>
        /// handle match added notification
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="matchId"></param>
        private void HandleMatchAddedNotification(int leagueId, int matchId)
        {
            var addedLineMembers = NotificationEvents
                .Where(e => e.EventType == NotificationEventType.AddedToMatch)
                .SelectMany(e => e.LeagueMemberIds)
                .ToList();

            var leagueMemberIds = Context.LeagueMembers
                .Where(lm => lm.LeagueId == leagueId)
                .Select(lm => lm.LeagueMemberId)
                .ToList();

            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.MatchAdded,
                MatchId = matchId,
                LeagueId = leagueId
            };
            // exclude those members who are already receiving a notification about being added to the lineup
            myEvent.LeagueMemberIds.AddRange(leagueMemberIds.Except(addedLineMembers).ToList());
            AddMatchSummaryDescription(ref myEvent, matchId);

            NotificationEvents.Add(myEvent);
        }

        /// <summary>
        /// create notification for matches that have been added in past, but users have not responded with their availability
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="matchId"></param>
        private void HandleMatchAddedReminderNotification(int leagueId, int matchId)
        {
            // get all leagueMembers
            var leagueMemberIds = Context.LeagueMembers
                .Where(lm => lm.LeagueId == leagueId)
                .Select(lm => lm.LeagueMemberId)
                .ToList();

            // get just the ones that have responded with yes or no
            var respondingLeagueMemberIds = Context.Players
                .Where(p => p.MatchId == matchId)
                .Where(p => p.Availability != Availability.Unknown)
                .Select(p => p.LeagueMemberId)
                .ToList();

            // get just the members who have not responded
            var unansweredLeagueMemberIds = leagueMemberIds.Except(respondingLeagueMemberIds)
                .ToList();

            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.MatchAddedReminder,
                MatchId = matchId,
                LeagueId = leagueId 
            };
            myEvent.LeagueMemberIds.AddRange(unansweredLeagueMemberIds);
            AddMatchSummaryDescription(ref myEvent, matchId);

            NotificationEvents.Add(myEvent);
        }

        /// <summary>
        /// create a notification for when a player responds to a match, to notify all other league members (except those that have already declined the match)
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="matchId"></param>
        /// <param name="respondingMemberId"></param>
        private void HandlePlayerRespondedNotification(int leagueId, int matchId, int respondingMemberId)
        {
            // get all leagueMembers
            var leagueMemberIds = Context.LeagueMembers
                .Where(lm => lm.LeagueId == leagueId)
                .Select(lm => lm.LeagueMemberId)
                .ToList();

            // get just the ones that have responded with a No, or the responding member (because we want to exclude both decliners and the responding member)
            var decliningLeagueMemberIds = Context.Players
                .Where(p => p.MatchId == matchId)
                .Where(p => p.Availability == Availability.Unavailable || p.MemberId == respondingMemberId)
                .Select(p => p.LeagueMemberId)
                .ToList();

            // get just the members who have not responded, or responded with a Yes
            var affectedLeagueMemberIds = leagueMemberIds
                .Except(decliningLeagueMemberIds)
                .ToList();

            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.PlayerResponded,
                MatchId = matchId,
                LeagueId = leagueId,
                ReferringMemberId = respondingMemberId  // the responder now becomes the referrer
            };
            myEvent.LeagueMemberIds.AddRange(affectedLeagueMemberIds);

            AddMatchSummaryDescription(ref myEvent, matchId);

            NotificationEvents.Add(myEvent);
        }

        /// <summary>
        /// handle availability update event
        /// </summary>
        /// <param name="request"></param>
        private void HandleAvailabilityUpdateNotification(UpdateAvailabilityRequest request)
        {
            var memberIds = new List<int>();

            switch (request.Action)
            {
                case MatchDeclineAction.DoNothing:
                    // do not invite anyone to this match
                    return;

                case MatchDeclineAction.InviteSome:
                    // invite some players, as defined by (passed in from) the client
                    memberIds = request.InviteMemberIds;
                    break;

                case MatchDeclineAction.InviteAll:
                    // invite all players, subs and non-subs, who are not in line-up, and who have not declared they are unavailable
                    // NOTE - this assumes there are players for every league member, for this match!
                    memberIds = Context.Players
                        .Where(p => p.MatchId == request.MatchId)
                        .Where(p => p.Availability != Availability.Unavailable)
                        .Where(p => p.LineId == null)
                        .Select(p => p.MemberId)
                        .ToList();
                    break;

                default: return;
            }

            // TODO: If action = Invite All, and there are no members in the list (nobody is available),
            // perhaps we should send a different notification, to the referring member, to tell them
            // that nobody was available, and that no invitations were sent

            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.SubForMatchOpening,
                MatchId = request.MatchId,
                LeagueId = request.LeagueId,
                ReferringMemberId = request.MemberId
            };
            myEvent.MemberIds.AddRange(memberIds);
            AddMatchSummaryDescription(ref myEvent, request.MatchId);

            NotificationEvents.Add(myEvent);
        }

        /// <summary>
        /// handle added line members event
        /// </summary>
        /// <param name="lineMembers"></param>
        /// <param name="matchId"></param>
        /// <param name="LeagueId"></param>
        private void HandleAddedPlayersEvent(IEnumerable<Player> players, int matchId, int leagueId)
        {
            if (players == null || !players.Any()) return;

            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.AddedToMatch,
                MatchId = matchId,
                LeagueId = leagueId
            };
            AddMatchSummaryDescription(ref myEvent, matchId);

            foreach (var player in players.Where(p => p.IsInLineup))
            {
                myEvent.LeagueMemberIds.Add(player.LeagueMemberId);
            }

            NotificationEvents.Add(myEvent);
        }

        /// <summary>
        /// handle event where courts have changed (add/delete court or court names changed)
        /// </summary>
        private void HandleCourtChangeEvent(IEnumerable<int> leagueMemberIds, int matchId, int leagueId)
        {
            if (leagueMemberIds == null || !leagueMemberIds.Any()) return;
            
            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.CourtChange,
                MatchId = matchId,
                LeagueId = leagueId
            };
            myEvent.LeagueMemberIds.AddRange(leagueMemberIds);
            AddMatchSummaryDescription(ref myEvent, matchId);

            NotificationEvents.Add(myEvent);
        }

        /// <summary>
        /// match details have changed
        /// </summary>
        /// <param name="leagueMemberIds"></param>
        /// <param name="matchId"></param>
        /// <param name="leagueId"></param>
        private void HandleMatchChangeEvent(IEnumerable<int> leagueMemberIds, int matchId, int leagueId)
        {
            if (leagueMemberIds == null || !leagueMemberIds.Any()) return;
            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.MatchChanged,
                MatchId = matchId,
                LeagueId = leagueId
            };
            myEvent.LeagueMemberIds.AddRange(leagueMemberIds);
            AddMatchSummaryDescription(ref myEvent, matchId);

            NotificationEvents.Add(myEvent);
        }

        /// <summary>
        /// handle match cancelled event
        /// </summary>
        /// <param name="leagueMemberIds"></param>
        /// <param name="matchId"></param>
        /// <param name="leagueId"></param>
        private void HandleMatchCancelled(List<int> leagueMemberIds, int matchId, int leagueId)
        {
            if (leagueMemberIds == null || !leagueMemberIds.Any()) return;

            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.MatchCancelled,
                MatchId = matchId,
                LeagueId = leagueId
            };
            myEvent.LeagueMemberIds.AddRange(leagueMemberIds);
            AddMatchSummaryDescription(ref myEvent, matchId);

            NotificationEvents.Add(myEvent);
        }

        /// <summary>
        /// handle event when court / line was auto-added because enough players were available and auto-add feature was enabled for that league
        /// </summary>
        /// <param name="players"></param>
        /// <param name="leagueId"></param>
        /// <param name="matchId"></param>
        private void HandleCourtAutoAddedEvent(IEnumerable<PlayerViewModel> players, int matchId, int leagueId) {
            if (players == null || !players.Any()) return;

            var myEvent = new NotificationEvent
            {
                EventType = NotificationEventType.CourtAutoAdded,
                MatchId = matchId,
                LeagueId = leagueId
            };
            AddMatchSummaryDescription(ref myEvent, matchId);

            foreach (var player in players)
            {
                myEvent.LeagueMemberIds.Add(player.LeagueMemberId);
            }

            NotificationEvents.Add(myEvent);

        }

        #endregion

        #region get

        /// <summary>
        /// get matches by member, with avail for the requesting member
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="showPast"></param>
        /// <param name="showFuture"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<List<MatchViewModel>> GetMatchesByMember(int memberId, bool showPast = false, bool showFuture = true, int page = 1, int pageSize = 5)
        {
            var skip = (page < 1 ? 0 : page - 1) * pageSize;

            var member = Context.Members
                .FirstOrDefault(m => m.MemberId == memberId);
            if (member == null)
            {
                throw new NotFoundException($"Member not found: memberId={memberId}");
            }

            IQueryable<Match> matches;

            if (member.MemberRoleFlags.HasFlag(MemberRoleFlags.Player))
            {
                // find all matches for which this member is in the line-up
                //matches = _context.Matches
                //    .Where(m => m.Lines.SelectMany(l => l.GetPlayers()).Any(p => p.MemberId == memberId));
                matches = Context.Players
                    .Where(p => p.LineId > 0 && p.MemberId == memberId)
                    .Select(p => p.Match);
            }
            else
            {
                // member is not a player, but a pro or venue manager, so list matches at their venue
                matches = Context.Matches
                    .Include(m => m.Players)
                    .Where(m => m.MatchVenueVenueId == member.HomeVenueVenueId);
            }

            // filter by time
            if (showFuture ^ showPast)
            {
                if (showFuture)
                { 
                    matches = matches
                        .Where(m => m.EndTime >= DateTime.UtcNow)
                        .OrderBy(m => m.StartTime);
                }

                if (showPast)
                {
                    matches = matches
                        .Where(m => m.EndTime < DateTime.UtcNow)
                        .OrderByDescending(m => m.StartTime);
                }
            }

            // filter by page, fetch all necessary related info
            var matchViewModels = await matches
                .Skip(skip)
                .Take(pageSize)
                .ProjectTo<MatchViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .ToListAsync();

            // for some unknown reason, this stopped working, need to fetch avail flag manually, rather than projectTo mapping
            //// map player availability to view model
            //foreach (var match in matchViewModels)
            //{
            //    match.PlayerAvailability = match.Players
            //        .Where(p => p.MemberId == memberId)
            //        .FirstOrDefault()?.Availability ?? Availability.Unknown;

            //    // save a little bandwidth, don't return all these records to the client as they are not needed there
            //    match.Players.Clear();
            //}

            // manual method
            foreach(var match in matchViewModels)
            {
                var avail = await Context.Players
                    .Where(p => p.MemberId == memberId)
                    .Where(p => p.MatchId == match.MatchId)
                    .FirstOrDefaultAsync();
                match.PlayerAvailability = avail?.Availability ?? Availability.Unknown;
            }

            return matchViewModels;
        }

        /// <summary>
        /// get match by id
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public async Task<MatchViewModel> GetMatchAsync(int matchId)
        {
            var match = await Context.Matches
                .Where(m => m.MatchId == matchId)
                .ProjectTo<MatchViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return match;
        }

        /// <summary>
        /// get match by member
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<MatchViewModel> GetMatchByMemberAsync(int matchId, int memberId)
        {
            var match = await Context.Matches
                .Where(m => m.MatchId == matchId)
                .ProjectTo<MatchViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (match != null)
            {
                // set edit flag for this match, if the calling member is captain or owner of the league associated with this match
                var canEdit = await Context.LeagueMembers
                                .Where(lm => lm.LeagueId == match.LeagueId)
                                .Where(l => l.MemberId == memberId)
                                .Where(l => l.IsCaptain || (l.League.OwnerMemberId == memberId))
                                .AnyAsync();

                match.CanEdit = canEdit;
            }
            return match;
        }

        /// <summary>
        /// method to check if a member can edit a match
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<bool> CanEditMatch(int leagueId, int memberId)
        {
            var isCaptain = await Context.LeagueMembers
                            .Where(l => l.LeagueId == leagueId)
                            .Where(l => l.IsCaptain)
                            .Where(l => l.MemberId == memberId)
                            .AnyAsync();

            var isOwner = await Context.Leagues.Where(l => l.LeagueId == leagueId)
                            .Where(l => l.OwnerMemberId == memberId)
                            .AnyAsync();

            // todo: add another check if member is the owner of the match (in future when matches may not be associated with leagues)

            // can edit if member is either captain or owner
            return isCaptain || isOwner;
        }

        /// <summary>
        /// get matches by League
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="showPast"></param>
        /// <param name="showFuture"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<List<MatchViewModel>> GetMatchesByLeague(int leagueId, bool showPast = false, bool showFuture = true, int page = 1, int pageSize = 20)
        {
            var skip = (page < 1 ? 0 : page - 1) * pageSize;

            var league = Context.Leagues.Find(leagueId);
            if (league == null)
            {
                throw new NotFoundException($"League not found: id={leagueId}");
            }

            // filter by league
            var matches = Context.Matches
                .Where(m => m.LeagueId == leagueId);

            // filter by time
            if (showFuture ^ showPast)
            {
                if (showFuture)
                {
                    matches = matches
                        .Where(m => m.EndTime >= DateTime.UtcNow)
                        .OrderBy(m => m.StartTime);
                }

                if (showPast)
                {
                    matches = matches
                        .Where(m => m.EndTime < DateTime.UtcNow)
                        .OrderByDescending(m => m.StartTime);
                }
            }

            // filter by page, project to viewmodel
            var matchViewModels = await matches
                .Skip(skip)
                .Take(pageSize)
                .ProjectTo<MatchViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .ToListAsync();

            // save some bandwidth - no need to send all the players to the client
            //foreach(MatchViewModel m in matchViewModels)
            //{
            //    m.Players.Clear();
            //}

            return matchViewModels;
        }

        /// <summary>
        /// search for matches
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="showPast"></param>
        /// <param name="showFuture"></param>
        /// <param name="matchId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<List<MatchViewModel>> SearchMatchesAsync(int matchId, int leagueId, bool showPast = false, bool showFuture = true, int page = 1, int pageSize = 20)
        {
            var skip = (page < 1 ? 0 : page - 1) * pageSize;

            var matches = Context.Matches.AsQueryable();

            if (leagueId > 0)
            {
                // filter by league
                var league = Context.Leagues.Find(leagueId);
                matches = matches
                    .Where(m => m.LeagueId == leagueId);
            }

            if (matchId > 0)
            {
                // filter by match
                matches = matches.Where(m => m.MatchId == matchId);
            }

            // filter by time
            if (showFuture ^ showPast)
            {
                if (showFuture)
                {
                    matches = matches
                        .Where(m => m.EndTime >= DateTime.UtcNow)
                        .OrderBy(m => m.StartTime);
                }

                if (showPast)
                {
                    matches = matches
                        .Where(m => m.EndTime < DateTime.UtcNow)
                        .OrderByDescending(m => m.StartTime);
                }
            }

            // filter by page, project to viewmodel
            var matchViewModels = await matches
                .Skip(skip)
                .Take(pageSize)
                .ProjectTo<MatchViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .ToListAsync();

            return matchViewModels;
        }

        public async Task<MatchDetailsViewModel> GetProspectiveMatches(int memberId)
        {
            var now = DateTime.UtcNow;

            // find all future matches of leagues this member is a part of, but where they are NOT in the line-up ('prospective' matches)
            var matches = (await (from league in Context.Leagues.Where(l => l.LeagueMembers.Any(lm => lm.MemberId == memberId))
                                  join match in Context.Matches
                                        .Where(m => m.StartTime > now)  // future matches
                                        .Where(m => !m.Players.Any(p => p.MemberId == memberId && p.MatchId == m.MatchId && p.LineId != null))  // only matches for which this member is not in the line-up
                                    on league.LeagueId equals match.LeagueId
                                  select match)
                             .Include(m => m.League)
                             .ProjectTo<MatchSummaryViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                             .ToListAsync())
                             .OrderBy(m => m.StartTimeLocal)
                             .ToList();

            var matchIds = matches.Select(m => m.MatchId).ToList();
            var lines = await Context.Lines.Where(l => matchIds.Contains(l.MatchId))
                            .Select(l => ModelMapper.Map<LineViewModel>(l))
                            .ToListAsync();

            // order by court number
            lines = lines.OrderBy(l => StringHelper.ZeroPadLeft(l.CourtNumber)).ToList();

            var players = GetPlayersByMatch(matchIds).ToList();

            // map player availability to view model
            foreach (var match in matches)
            {
                match.Availability = players
                    .Where(p => p.MemberId == memberId && p.MatchId == match.MatchId)
                    .FirstOrDefault()?.Availability ?? Availability.Unknown;
            }

            var matchDetails = new MatchDetailsViewModel
            (
                matches: matches,
                lines: lines,
                players: players
            );

            return matchDetails;
        }

        /// <summary>
        /// alternate signature for GetPlayersByMatch, for only one match
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public IEnumerable<PlayerViewModel> GetPlayersByMatch(int matchId)
        {
            return GetPlayersByMatch(new List<int> { matchId });
        }

        /// <summary>
        /// given a list of matches, return all players
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public IEnumerable<PlayerViewModel> GetPlayersByMatch(List<int> matchIds)
        {            
            var playerResults = (
                from myMatch in Context.Matches
                    .Where(m => matchIds.Contains(m.MatchId))
                join league in Context.Leagues on myMatch.LeagueId equals league.LeagueId
                join lm in Context.LeagueMembers on league.LeagueId equals lm.LeagueId
                join player in Context.Players on new { myMatch.MatchId, lm.LeagueMemberId } equals new { player.MatchId, player.LeagueMemberId } into players
                from player in players.DefaultIfEmpty()  // if player doesn't exist, map to null (outer join), and create one in code below
                select ModelMapper.Map<PlayerViewModel>(new Player
                {
                    LeagueMemberId = lm.LeagueMemberId,
                    Availability = player == null ? Availability.Unknown : player.Availability,
                    MatchId = myMatch.MatchId,
                    MemberId = lm.MemberId,
                    LeagueId = lm.LeagueId,
                    ModifiedDate = player == null ? DateTime.UtcNow : player.ModifiedDate,
                    IsSubstitute = player == null ? lm.IsSubstitute : player.IsSubstitute,
                    Member = lm.Member,
#pragma warning disable IDE0031 // Use null propagation
                    LineId = player == null ? null : player.LineId
#pragma warning restore IDE0031 // Use null propagation
                })
            );

            return playerResults;
        }
        #endregion

        #region update

        /// <summary>
        /// updates match
        /// </summary>
        /// <param name="inputMatch"></param>
        /// <returns></returns>
        public async Task<Match> UpdateMatchAsync(Match inputMatch)
        {
            if (inputMatch is null) return null;

            var existingMatch = await Context.Matches.AsNoTracking()
                .Include(m => m.Lines).AsNoTracking()
                .Include(m => m.Players).AsNoTracking()
                .Where(m => m.MatchId == inputMatch.MatchId)
                .FirstOrDefaultAsync();

            // test for match properties that may have changed
            var hasMatchPropertiesChanged = false;
            if (existingMatch != null && !MatchHelper.MatchPropertiesAreEqual(inputMatch, existingMatch))
            {
                hasMatchPropertiesChanged = true;
            }

            //ModelMapper.Map(inputMatch, existingMatch);

            var matchIsInFuture = inputMatch.EndTime > DateTime.UtcNow;

            var matchAdded = existingMatch == null;

            // test for court changes
            var hasCourtChanges = false;
            if (inputMatch.Lines != null)
            {
                foreach (var line in inputMatch.Lines)
                {
                    var existingLine = existingMatch?.Lines?.Where(l => l.LineId == line.LineId).FirstOrDefault();
                    if (existingLine != null)
                    {
                        // if court number has changed
                        if (existingLine.CourtNumber != line.CourtNumber)
                        {
                            // suppress court change notification if court will be auto-reserved and has not yet been reserved,
                            // because it means the captain is just changing his preference on courts, and players should not
                            // be alerted about this until the courts have actually been reserved
                            if (!(inputMatch.AutoReserveCourts && !line.IsReserved))
                            {
                                hasCourtChanges = true;
                            }
                        }
                    }
                }
            }

            // test for players added
            var addedPlayers = new List<Player>();
            if (inputMatch.Players != null)
            {
                addedPlayers = inputMatch.Players
                    .Where(p => p != null && p.IsInLineup)
                    .Where(p => !Context.Players.Any(ep => ep.MatchId == inputMatch.MatchId && ep.LineId >= 0 && p.Id == ep.Id) || (p.Id == 0 && p.LineId >= 0))
                    .ToList();
            }
            //if (inputMatch.Players != null)
            //{
            //    var addedPlayerIds = inputMatch.Players.Where(p => p.LineId > 0).Select(p => p.Id).Except(existingMatch.Players.Where(p => p.LineId > 0).Select(p => p.Id)).ToList();
            //    addedPlayers = inputMatch.Players.Where(p => addedPlayerIds.Contains(p.Id) || (p.Id == 0 && p.LineId > 0)).ToList();
            //}

            var deletedMemberIds = new List<int>();

            if (existingMatch?.Players != null)
            {
                var deletedPlayers = existingMatch.Players
                    .Where(ep => ep.IsInLineup)
                    .Where(ep => !inputMatch.Players?.Any(ip => ip.Id == ep.Id && ip.IsInLineup) ?? false)
                    .ToList();

                deletedMemberIds = deletedPlayers.Select(p => p.MemberId).ToList();
            }

             Context.Matches.Update(inputMatch);

            // a match has a collection of lines, so we need to handle the case where
            // we are deleting lines, by comparing existing lines with lines in inputMatch
            var inputLineIds = inputMatch.Lines?.Select(l => l.LineId)?.ToList() ?? new List<int>();
            var deletedLines = await Context.Matches
                .Where(m => m.MatchId == inputMatch.MatchId)
                .SelectMany(m => m.Lines)//.AsNoTracking()
                .Where(el => inputMatch.Lines == null || !inputLineIds.Contains(el.LineId))
                .ToListAsync();

            var deletedLineIds = deletedLines.Select(l => l.LineId).ToList();

            if (deletedLines.Any())
            {
                //var playersToRemoveFromLine = Context.Players
                //    .Where(p => deletedLineIds.Contains(p.LineId ?? 0))
                //    .AsNoTracking()
                //    .ToList();
                var playersToRemoveFromLine = inputMatch.Players
                    .Where(p => deletedLineIds.Contains(p.LineId ?? 0))
                    .ToList();

                if (playersToRemoveFromLine.Any())
                {
                    foreach (var player in playersToRemoveFromLine)
                    {
                        player.LineId = null;
                    }
                    Context.Players.UpdateRange(playersToRemoveFromLine);
                }

                Context.Lines.RemoveRange(deletedLines);
            }

            // do we need to delete any players?  check for players in db, but not in input match
            var inputPlayerIds = inputMatch.Players?.Select(p => p.Id)?.ToList() ?? new List<int>();
            var playersToDelete = Context.Players
                .Where(p => p.MatchId == inputMatch.MatchId)
                .Where(p => !inputPlayerIds.Any(id => id == p.Id))
                .AsNoTracking()
                .ToList();

            if (playersToDelete.Any())
            {
                // yes, delete these
                Context.Players.RemoveRange(playersToDelete);
            }

            await Context.SaveChangesAsync();

            // map new lines to players, using guid props of each
            if (inputMatch.Players != null)
            {
                foreach (var player in inputMatch.Players.Where(p => !string.IsNullOrEmpty(p.Guid) && (p.LineId == 0 || p.LineId == null)))
                {
                    var matchingLine = inputMatch.Lines.FirstOrDefault(l => l.Guid == player.Guid);
                    player.LineId = matchingLine.LineId;
                    Context.Players.Update(player);
                }
            }
            await Context.SaveChangesAsync();

            var affectedLeagueMemberIds = inputMatch.Players?.Where(p => p.IsInLineup).Select(p => p.LeagueMemberId).ToList();

            // only send notifications if match is in future
            if (matchIsInFuture)
            {
                // handle notifications of match changes
                if (hasMatchPropertiesChanged)
                {
                    if (inputMatch.Players != null)
                    {
                        HandleMatchChangeEvent(affectedLeagueMemberIds, inputMatch.MatchId, inputMatch.LeagueId);
                    }
                }

                // handle court change notifications
                if (hasCourtChanges)
                {
                    HandleCourtChangeEvent(affectedLeagueMemberIds, inputMatch.MatchId, inputMatch.LeagueId);
                }

                // handle deleted player notification
                if (deletedMemberIds.Any())
                {
                    HandleDeletedLineMembersEvent(deletedMemberIds, inputMatch.MatchId, inputMatch.LeagueId);
                }

                // handle added player notification
                if (addedPlayers.Any())
                {
                    HandleAddedPlayersEvent(addedPlayers, inputMatch.MatchId, inputMatch.LeagueId);
                }

                if (matchAdded)
                {
                    HandleMatchAddedNotification(inputMatch.LeagueId, inputMatch.MatchId);
                }
            }

            return inputMatch;
        }

        /// <summary>
        /// update availability for match player
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="availability"></param>
        /// <param name="referringMemberId"></param>
        /// <returns></returns>
        public async Task UpdateMatchPlayer(string userId, int matchId, int leagueId, Availability availability, int referringMemberId=0, int respondingMemberId=0)
        {

            var respondingLeagueMember = await Context.LeagueMembers
                .SingleOrDefaultAsync(m => (m.Member.UserId == userId || m.Member.MemberId == respondingMemberId) && m.LeagueId == leagueId);
           
            if (respondingLeagueMember.MemberId == referringMemberId)
            {
                throw new BadRequestException("Invalid request - referring member and responding member must be different (you cannot respond to your own sub request)");
            } 

            var respondingPlayer = await Context.Players
                .Where(p => p.LeagueMemberId == respondingLeagueMember.LeagueMemberId)
                .Where(p => p.MatchId == matchId)
                //.AsNoTracking()  // force latest info, without caching
                .FirstOrDefaultAsync();

            var respondingPlayerExists = respondingPlayer != null;

            var referringPlayer = await Context.Players
                .Where(p => p.MemberId == referringMemberId)
                .Where(p => p.MatchId == matchId)
                //.AsNoTracking()  // force latest info, without caching
                .FirstOrDefaultAsync();

            var match = Context.Matches
            //.AsNoTracking()
            .Where(m => m.MatchId == matchId)
            .Include(m => m.Players).ThenInclude(p => p.LeagueMember)//.AsNoTracking()
            .Include(m => m.Lines)//.AsNoTracking()  // force latest info, without caching
            .FirstOrDefault();

            if (match == null)
            {
                throw new NotFoundException($"Unable to find match id = {matchId}");
            }

            if (!respondingPlayerExists)
            {
                // create a new player
                respondingPlayer = new Player
                {
                    LeagueMemberId = respondingLeagueMember.LeagueMemberId,
                    MemberId = respondingLeagueMember.MemberId,
                    LeagueId = leagueId,
                    MatchId = match.MatchId,
                    IsSubstitute = respondingLeagueMember.IsSubstitute
                };
                match.Players.Add(respondingPlayer);
            }

            if (respondingPlayer.Availability == Availability.Confirmed && referringPlayer.Availability == Availability.Confirmed)
            {
                throw new NotFoundException("This match slot is not available.  The referring player is confirmed to be playing.  They may have changed their mind after requesting a sub.");
            }

            if (match.Players.Where(p => p.IsInLineup && p.LeagueMemberId == respondingLeagueMember.LeagueMemberId).Any())
            {
                if (availability == Availability.Confirmed && respondingPlayer.Availability == Availability.Confirmed)
                {
                    throw new BadRequestException("Unable to add you to this match - you are already in the line-up and confirmed!");
                }
                if (availability == Availability.Unavailable && respondingPlayer.Availability == Availability.Confirmed)
                {
                    throw new BadRequestException("Unable to respond to this match - you are already in the line-up and confirmed.  To remove yourself from an existing match, go to your home page and click on the match and change your availability.");
                }
            }

            var line = match.Lines?.Where(l => l.LineId == referringPlayer.LineId).FirstOrDefault();

            if (referringPlayer == null || line == null)
            {
                throw new NotFoundException("Unable to find line - someone may have taken your place in this match or this line was deleted");
            }

            if (availability == Availability.Confirmed)
            {
                // swap players in the line-up
                respondingPlayer.LineId = line.LineId;
                referringPlayer.LineId = null;
                respondingPlayer.IsSubstitute = true;  // mark player as sub, even if they are a regular member of league, because for this match, they are subbing
            }

            respondingPlayer.Availability = availability;
            respondingPlayer.ModifiedDate = DateTime.UtcNow;

            // add notification that a player has responded
            HandlePlayerRespondedNotification(match.LeagueId, match.MatchId, respondingPlayer.MemberId);

            await UpdateMatchAsync(match);

            return;
        }

        #endregion

        #region delete
        /// <summary>
        /// delete a match
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Match> DeleteMatch(int id)
        {
            var match = await Context.Matches
                .Include(m => m.Lines)
                .Include(m => m.Players)
                .SingleOrDefaultAsync(m => m.MatchId == id);
            if (match == null)
            {
                throw new NotFoundException($"ERROR - DeleteMatch: No match found with id={id}");
            }

            var leagueMemberIds = match.Players
                .Where(p => p.IsInLineup)
                .Select(p => p.LeagueMemberId)
                .ToList();

            if (match.EndTime > DateTime.UtcNow)
            {
                // only notify if this match is in the future
                HandleMatchCancelled(leagueMemberIds, match.MatchId, match.LeagueId);
            }

            // have to manually delete some items that are not cascade-delete enabled
            if (match.Players != null)
            {
                Context.Players.RemoveRange(match.Players);
            }
            if (match.Lines != null)
            {
                Context.Lines.RemoveRange(match.Lines);
            }

            // finally, remove match
            Context.Matches.Remove(match);

            await Context.SaveChangesAsync();
            return match;
        }
        #endregion

        #region getNewMatch

        /// <summary>
        /// get a new match for a given league, with all the correct defaults for that league
        /// </summary>
        /// <param name="leagueId"></param>
        /// <returns></returns>
        public async Task<MatchViewModel> GetNewMatch(int leagueId)
        {
            var league = await Context.Leagues
                .Include(l => l.HomeVenue)
                .FirstOrDefaultAsync(l => l.LeagueId == leagueId);

            if (league == null)
            {
                throw new BadRequestException("Unable to find league");
            }

            // to calculate the next logical start time for a new match, consider the league's frequency of matches, day they play, and 
            // any currently scheduled matches.  For example, if they play every wed night, and there is already match scheduled for next
            // wed, make this match for the following wed (e.g. next available time slot)

            var nextMatchStartTime = GetNextAvailableMatchTime(leagueId, DateTime.UtcNow);

            var match = new Match
            {
                MatchVenue = league.HomeVenue,
                MatchVenueVenueId = league.HomeVenue.VenueId,
                Format = league.DefaultFormat,
                HomeMatch = true,
                League = league,
                LeagueId = league.LeagueId,
                StartTime = nextMatchStartTime,
                EndTime = nextMatchStartTime.AddMinutes(90),
                WarmupTime = nextMatchStartTime.AddMinutes(-league.WarmupTimeMinutes),
                AutoReserveCourts = league.AutoReserveCourts                
            };
            var reservationSystem = await Context.ReservationSystems.FirstOrDefaultAsync(r =>r.VenueId == match.MatchVenueVenueId);
            var matchViewModel = ModelMapper.Map<MatchViewModel>(match);
            matchViewModel.MarkNewCourtsReserved = league.MarkNewCourtsReserved;
            matchViewModel.MarkNewCourtsReserved = league.MarkNewPlayersConfirmed;
            matchViewModel.VenueHasReservationSystem = reservationSystem != null;

            return matchViewModel;
        }
        #endregion

        #region availability
        /// <summary>
        /// update player availability
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Player> UpdateAvailability(UpdateAvailabilityRequest request)
        {
            if (request is null)
            {
                throw new BadRequestException("Update availability request is null");
            }
            var match = Context.Matches.Find(request.MatchId);
            if (match == null)
            {
                throw new NotFoundException("Unable to find match");
            }

            var existingPlayer = Context.Players
                .Where(p => p.MatchId == request.MatchId && p.MemberId == request.MemberId)
                .FirstOrDefault();

            // set flag if player was in a line-up - this is important later for notifications
            var playerInLineup = existingPlayer == null ? false : existingPlayer.LineId == null ? false : existingPlayer.IsInLineup;

            var league = Context.Leagues.Find(request.LeagueId);

            if (league == null)
            {
                throw new NotFoundException("Unable to find league");
            }

            var autoAdd = league.AutoAddToLineup;

            var leagueMember = await Context.LeagueMembers
                .FirstOrDefaultAsync(lm => lm.MemberId == request.MemberId && lm.LeagueId == request.LeagueId);

            if (leagueMember == null)
            {
                throw new NotFoundException("Unable to find LeagueMember");
            }

            var leagueMemberId = leagueMember.LeagueMemberId;

            // set flag as to whether the player is changing their availability, for notification purposes
            var hasChangedTheirAvailability = (existingPlayer?.Availability ?? Availability.Unknown) != request.Value;

            Player playerResult;

            Player newPlayer = null;
            if (existingPlayer == null)
            {
                // new record
                newPlayer = new Player
                {
                    LeagueId = request.LeagueId,
                    MemberId = request.MemberId,
                    MatchId = request.MatchId,
                    LeagueMemberId = leagueMemberId,
                    Availability = request.Value,
                    ModifiedDate = DateTime.UtcNow,
                    IsHomePlayer = true,
                    IsSubstitute = leagueMember.IsSubstitute
                };
                Context.Players.Add(newPlayer);
                playerResult = newPlayer;
            }
            else
            {
                // existing record
                existingPlayer.Availability = request.Value;
                existingPlayer.ModifiedDate = DateTime.Now;
                Context.Players.Update(existingPlayer);
                await Context.SaveChangesAsync();
                playerResult = existingPlayer; 
            }
            var playerToAdd = existingPlayer ?? newPlayer;
            var wasAddedToLineup = false;
            var availablePlayers = new List<Player>();
            var undecidedPlayers = new List<PlayerViewModel>();

            if (!playerInLineup && autoAdd) // && request.Value == Availability.Confirmed)
            {
                // player is not in line-up, and league settings allow auto-add to line-up
                // so let's add any available players to the line-up, or add others to the line-up if logic shows there are 
                // enough players to round out a court

                // check if we are filling out a court
                var courtSize = LeagueHelper.FullCourtSize(match.Format);

                var currentPlayers = Context.Players
                    .Where(p => p.MatchId == request.MatchId)
                    .Where(p => p.LineId >= 0)  // they are in the line-up already
                    .Where(p => p.Availability != Availability.Unavailable)
                    .Select(p => p.LeagueMemberId)
                    .ToList();

                availablePlayers = Context.Players
                    .Where(p => p.MatchId == request.MatchId)
                    .Where(p => p.LineId == null)
                    .Where(p => p.Availability == Availability.Confirmed)
                    .OrderBy(p => p.IsSubstitute)  // regulars get priority
                    .ThenBy(p => p.ModifiedDate)  // first to Confirm will get first slot
                    .ToList();

                undecidedPlayers = GetPlayersByMatch(match.MatchId)
                    // exclude the player we are adding
                    .Where(p => p.LeagueMemberId != playerToAdd.LeagueMemberId)
                    // include only those who are undecided (or have not responded)
                    .Where(p => p.Availability == Availability.Unknown)
                    .ToList();

                // if not already included, add current user's record to list of available players
                if (!availablePlayers.Any(p => p.Id == playerToAdd.Id) && playerToAdd.Availability == Availability.Confirmed)
                {
                    availablePlayers.Add(playerToAdd);
                }

                // if there are any regular players available, then exclude subs from the available list of players
                // in other words, focus this auto-add feature on regular members, until there are no more reg players available,
                // then open it up to the subs
                if (undecidedPlayers.Any(m => !m.IsSubstitute))
                {
                    // yes, there are still regulars that have not responded, so let's exclude the subs for now
                    availablePlayers = availablePlayers.Where(p => !p.IsSubstitute).ToList();
                }

                // re-order the list
                availablePlayers = availablePlayers
                    .OrderBy(p => p.IsSubstitute)  // regulars get priority
                    .ThenBy(p => p.ModifiedDate)  // first to Confirm will get first slot
                    .ToList();

                // will this new player round out a court?
                //var willRoundOutCourt = (currentPlayers.Count + availablePlayers.Count) % courtSize == 0;
                var currentCourtLoad = Math.Floor((decimal)currentPlayers.Count / courtSize);
                var possibleCourtLoad = Math.Floor((decimal)(currentPlayers.Count + availablePlayers.Count) / courtSize);
                var willRoundOutCourt = possibleCourtLoad > currentCourtLoad;

                if (willRoundOutCourt)
                {
                    var lines = (await Context.Lines
                    .Where(l => l.MatchId == request.MatchId)
                    .OrderBy(l => l.CourtNumber)
                    .ToListAsync())
                    .OrderBy(l => StringHelper.ZeroPadLeft(l.CourtNumber))
                    .ToList();

                    var playerCounts = (await Context.Lines
                        .Where(l => l.MatchId == request.MatchId)
                        .Select(l => new { l.LineId, l.CourtNumber, playerCount = l.Match.Players.Where(p => p.LineId == l.LineId).Count() })
                        .ToListAsync())
                        .OrderBy(l => StringHelper.ZeroPadLeft(l.CourtNumber))
                        .ToList();

                    var totalAvail = availablePlayers.Count;
                    var playerIndex = 0;
                    var lineIndex = 0;
                    var nextCourt = "1";
                    while (playerIndex < totalAvail)
                    {
                        var currentLine = lines.Count < lineIndex + 1 ? null : lines[lineIndex];
                        var currentCourt = currentLine == null ? nextCourt : currentLine.CourtNumber;
                        if (currentLine == null)
                        {
                            var remainingPlayersToAdd = totalAvail - playerIndex;
                            if (remainingPlayersToAdd >= courtSize)  // make sure we have enough players to round another full court
                            {
                                currentLine = new Line
                                {
                                    MatchId = request.MatchId,
                                    CourtNumber = nextCourt,
                                    Format = league.DefaultFormat
                                };
                                Context.Lines.Add(currentLine);
                                await Context.SaveChangesAsync();
                            }
                            else break;  // not enough players to add another court, get out of here
                        }
                        nextCourt = StringHelper.NextNumberAsString(currentCourt);
                        var currentLinePlayers = playerCounts.FirstOrDefault(p => p.LineId == currentLine.LineId)?.playerCount ?? 0;
                        var emptySlots = courtSize - currentLinePlayers;
                        for (int j = 0; j < emptySlots; j++)
                        {
                            availablePlayers[playerIndex].LineId = currentLine.LineId;
                            playerIndex++;
                            if (playerIndex >= totalAvail) break;
                        }
                        lineIndex++;
                        wasAddedToLineup = true;
                    }
                }
            }

            await Context.SaveChangesAsync();

            // in event that player is unable to play, handle notifications to subs
            if (request.Value == Availability.Unavailable && playerInLineup)
            {
                HandleAvailabilityUpdateNotification(request);
            }

            if (wasAddedToLineup)
            {
                HandleAddedPlayersEvent(availablePlayers, request.MatchId, league.LeagueId);
                HandleCourtAutoAddedEvent(undecidedPlayers, request.MatchId, league.LeagueId);
            }

            if (hasChangedTheirAvailability)
            {
                // add notificaation that a player has responded, only if they are changing their availability
                HandlePlayerRespondedNotification(match.LeagueId, match.MatchId, playerResult.MemberId);
            }

            return playerResult;
        }

        /// <summary>
        /// given a league id, return availability for all players, for all upcoming matches
        /// use to display grid in UI of every player's availability for future matches
        /// </summary>
        /// <param name="leagueId"></param>
        /// <returns></returns>
        public async Task<LeagueAvailabilityGridViewModel> GetLeagueAvailabilityGrid(int leagueId)
        {
            // fetch time into variable so it can be used in LinqSQL query without error
            var utcNow = DateTime.UtcNow;

            // get upcoming matches
            var upcomingMatches = await Context.Matches.AsNoTracking()
                .Where(m => m.EndTime > utcNow)
                .Where(m => m.LeagueId == leagueId)
                .OrderBy(m => m.StartTime)
                .ProjectTo<MatchSummaryViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .ToListAsync();

            // get league member names
            var memberNames = Context.LeagueMembers
                .Where(lm => lm.LeagueId == leagueId)
                .ProjectTo<MemberNameViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .ToList();

            // get player availability data
            var gridData = (from league in Context.Leagues.AsNoTracking().Where(l => l.LeagueId == leagueId)
                     join lm in Context.LeagueMembers on league.LeagueId equals lm.LeagueId
                     join match in Context.Matches.Where(m => m.EndTime > utcNow) on league.LeagueId equals match.LeagueId
                     join player in Context.Players on new { match.MatchId, lm.LeagueMemberId } equals new { player.MatchId, player.LeagueMemberId } into players
                     from player in players.DefaultIfEmpty()
                     select new
                     {
                         match.MatchId,
                         match.StartTime,
                         // if player exists, use it, else create new Player for this match/member
                         Player = ModelMapper.Map<PlayerViewModel>(player ?? new Player
                         {
                             LeagueMemberId = lm.LeagueMemberId,
                             Availability = Availability.Unknown,
                             MatchId = match.MatchId,
                             MemberId = lm.MemberId,
                             LeagueId = lm.LeagueId,
                             ModifiedDate = DateTime.UtcNow
                         })
                     })
                     .ToList();

            // get distinct list of member ids, in order
            var memberIds = memberNames
                .OrderBy(g => g.IsSubstitute)
                .ThenBy(g => g.FirstName)
                .Select(g => g.MemberId)
                .Distinct();

            // cycle through list and group by members (rows), attach list of players (cols) to each member,
            // ordered by match date (start time)
            var playerList = new List<LeagueAvailabilityViewModel>();
            foreach (var memberId in memberIds)
            {
                // member for this row of the avail grid
                var memberName = memberNames.First(g => g.MemberId == memberId);

                // list of availabilities for this member, for each match
                var leaguePlayers = gridData
                        .Where(g => g.Player.MemberId == memberId)
                        .OrderBy(g => g.StartTime)
                        .Select(g => g.Player).ToList();

                var lavm = new LeagueAvailabilityViewModel(memberName, leaguePlayers);
                playerList.Add(lavm);
            }

            foreach (var m in upcomingMatches)
            {
                m.TotalAvailable = playerList
                    .SelectMany(l => l.LeaguePlayers)
                    .Where(p => p.MatchId == m.MatchId)
                    .Sum(p => p.Availability == Availability.Confirmed ? 1 : 0);
            }

            // populate grid viewmodel
            var laGrid = new LeagueAvailabilityGridViewModel(playerList, upcomingMatches);

            return laGrid;
        }

        /// <summary>
        /// get list of matches that are scheduled, in leagues I am in, 
        /// and am not already in the lineup
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<List<MatchAvailabilityViewModel>> GetUnansweredAvailabilities(int memberId)
        {
            var leagues = await Context.Leagues
                .Where(l => l.LeagueMembers.Any(lm => lm.MemberId == memberId))
                .Select(l => ModelMapper.Map<LeagueSummaryViewModel>(l))
                .ToListAsync();

            var leagueIds = leagues.Select(l => l.LeagueId).ToList();

            var now = DateTime.UtcNow;

            var matchPlayers = await Context.Matches
                 .Where(m => leagueIds.Contains(m.LeagueId))
                 .Where(m => m.EndTime > now)
                 .Where(m => !m.Players.Where(p => p.MemberId == memberId).Any(p => p.LineId > 0))  // exclude matches where the member is in the line-up
                 .Select(m => new { Match = ModelMapper.Map<MatchSummaryViewModel>(m), Player = m.Players.FirstOrDefault(p => p.MemberId == memberId) })
                 .ToListAsync();

            var matchAvailabilities = new List<MatchAvailabilityViewModel>();

            // set availability in viewmodel, from player
            foreach(var mp in matchPlayers)
            {
                mp.Match.Availability = mp.Player?.Availability ?? Availability.Unknown;
            }

            foreach(var l in leagues)
            {
                if (matchPlayers.Where(mp => mp.Match.LeagueId == l.LeagueId).Any())
                {
                    var mavm = new MatchAvailabilityViewModel
                    {
                        League = l
                    };
                    var matches = matchPlayers
                        .Where(mp => mp.Match.LeagueId == l.LeagueId)
                        .Select(mp => mp.Match)
                        .OrderBy(m => m.StartTimeLocal)
                        .ToList();
                    mavm.Matches.AddRange(matches);                    
                    matchAvailabilities.Add(mavm);
                }
            }

            // order leagues by the ones with the first match at the top
            matchAvailabilities = matchAvailabilities
                .OrderBy(ma => ma.Matches.OrderBy(m => m.StartTimeLocal).FirstOrDefault()?.StartTimeLocal)
                .ToList();

            return matchAvailabilities;
        }
        #endregion

        #region reminders
        /// <summary>
        /// get matches that are upcoming, and are scheduled three days from now
        /// </summary>
        /// 
        public int NotifyUpcomingMatches(int daysAhead)
        {
            var utcNow = DateTime.UtcNow;
            var dateLocal = TimeZoneInfo.ConvertTimeFromUtc(utcNow, Utilities.DataConstants.AppTimeZoneInfo);
            var offset = (utcNow - dateLocal);
            var todayLocal = new DateTime(dateLocal.Year, dateLocal.Month, dateLocal.Day, 0, 0, 0);
            var minUtcDate = todayLocal + TimeSpan.FromDays(daysAhead) + offset;
            var maxUtcDate = todayLocal + TimeSpan.FromDays(daysAhead + 1) + offset;
            var matches = Context.Matches
                .Where(m => m.StartTime >= minUtcDate)
                .Where(m => m.StartTime < maxUtcDate)
                .Where(m => m.AutoReserveCourts)  // only send match reminder for matches where we are using court reservation system
                .Select(m => new { m.MatchId, m.LeagueId })
                .ToList();

            foreach (var match in matches)
            {
                HandleMatchAddedReminderNotification(match.LeagueId, match.MatchId);
            }

            // note - after calling this method, caller needs to add the NotificationEvents to a background notification queue to send them
            return matches.Count;
        }
        #endregion
    }
}
 