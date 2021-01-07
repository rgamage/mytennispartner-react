using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Data.DataTransferObjects;
using MyTennisPartner.Models.Exceptions;
using MyTennisPartner.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyTennisPartner.Models.Enums;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using System.Threading;

namespace MyTennisPartner.Data.Managers
{
    public class MemberManager : ManagerBase
    {

        #region constructor
        public MemberManager(TennisContext context, ILogger<MemberManager> logger) : base(context, logger)
        {
        }
        #endregion


        #region create
        /// <summary>
        /// create member
        /// </summary>
        /// <param name="memberViewModel"></param>
        /// <returns></returns>
        public async Task<Member> CreateMemberAsync(Member member)
        {
            if (member is null) return null;
            member.Image = new MemberImage
            {
                MemberId = member.MemberId
            };
            if (member.HomeVenueVenueId > 0)
            {
                // this is to avoid a venue insert error (we don't want to add a new venue, just associate an existing one with this new member)
                //Context.Entry(member.HomeVenue).State = EntityState.Detached;
                member.HomeVenue = null;
                //Context.Entry(member.HomeVenueVenueId).State = EntityState.Modified;
            }
            Context.Members.Add(member);
            //Context.Entry(member.HomeVenueVenueId).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            // now load the venue, so we return the member with its venue intact
            Context.Entry(member)
               .Reference(m => m.HomeVenue)
                .Load();

            return member;
        }

        public async Task<LeagueMember> AddLeagueMemberAsync(int leagueId, int memberId, bool isSub = false)
        {
            var existingLeagueMember = Context.LeagueMembers
                .Where(m => m.LeagueId == leagueId && m.MemberId == memberId)
                .FirstOrDefault();

            if (existingLeagueMember != null)
            {
                throw new BadRequestException("Unable to create new league member: already exists in database");
            }

            var existingLeague = Context.Leagues.Where(l => l.LeagueId == leagueId).FirstOrDefault();
            if (existingLeague == null)
            {
                throw new BadRequestException($"ERROR adding member to league: League {leagueId} doesn't exist");
            }

            var existingMember = Context.Members.Where(m => m.MemberId == memberId).FirstOrDefault();
            if (existingMember == null)
            {
                throw new BadRequestException($"ERROR adding member to league: Member {memberId} doesn't exist");
            }

            var newLeagueMember = new LeagueMember
            {
                LeagueId = leagueId,
                MemberId = memberId,
                IsSubstitute = isSub
            };

            Context.LeagueMembers.Add(newLeagueMember);
            await Context.SaveChangesAsync();

            var notificationEvent = new NotificationEvent
            {
                EventDescription = $"Added member to roster",
                EventType = NotificationEventType.AddedToLeague,
                LeagueId = leagueId
            };
            notificationEvent.MemberIds.Add(memberId);

            NotificationEvents.Add(notificationEvent);
            return newLeagueMember;
        }
        #endregion

        #region read
        /// <summary>
        /// get all members
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Member>> GetMembersAsync()
        {
            return await Context.Members.ToListAsync();
        }

        public async Task<List<MemberViewModel>> GetMemberViewModelsAsync()
        {
            var members = await Context.Members
                .Include(m => m.HomeVenue)
                .ProjectTo<MemberViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .ToListAsync();

            members = members.OrderBy(m => m.LastName).ToList();
            return members;
        }

        /// <summary>
        /// bulk update of members
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public async Task UpdateMembers(List<Member> members)
        {
            Context.Members.UpdateRange(members);
            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// get all members, with their profile images attached to viewmodel
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MemberViewModel>> GetMembersWithImagesAsync()
        {
            var members = await (from member in Context.Members
                                 join memberImage in Context.MemberImages on member.MemberId equals memberImage.MemberId into images
                                 from image in images.DefaultIfEmpty()
                                 select new
                                 {
                                     member,
                                     image = image.ImageBytes
                                 })
                          .ToListAsync();
            var results = new List<MemberViewModel>();

            foreach (var memberResult in members)
            {
                var result = ModelMapper.Map<MemberViewModel>(memberResult.member);
                result.SetProfileImage(memberResult.image);
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// get member by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Member> GetMemberAsync(int id)
        {
            var member = await Context.Members
                .Include(m => m.HomeVenue)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            return member;
        }

        public Member GetMember(int id)
        {
            //var member = await Context.Members.FindAsync(id);
            var member = Context.Members.FirstOrDefault(m => m.MemberId == id);
            return member;
        }

        /// <summary>
        /// method to get member profile image
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<byte[]> GetMemberImageByMember(int memberId)
        {
            var image = await Context.MemberImages.FirstOrDefaultAsync(m => m.MemberId == memberId);
            return image?.ImageBytes ?? new byte[0];
        }

        /// <summary>
        /// get member by userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Member> GetMemberByUserAsync(string userId)
        {
            var member = await Context.Members
                .Include(m => m.PlayerPreferences)
                .Include(m => m.HomeVenue)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            return member;
        }

        public async Task<Member> GetMemberByUserAsync2(string userId)
        {
            // todo: understand why this method works with includes commented out, but doesn't work 
            // with them enabled.  This only fails on my home PC, query succeeds on work PC.  ?? EF Framework install issue?
            var member = await Context.Members
                //.Include(m => m.MemberRoles)
                //.Include(m => m.PlayerPreferences)
                .Include(m => m.HomeVenue).ThenInclude(v => v.VenueAddress)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            return member;
        }

        /// <summary>
        /// search members, use for type-ahead, auto-complete, etc.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="leagueId"></param>
        /// <param name="excludeMatchId"></param>
        /// <param name="numResults"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MemberViewModel>> SearchAsync(string search, Gender gender, int leagueId = 0, int excludeLeagueId = 0, int numResults = 10)
        {
            var members = await Context.Members
                // search by partial first/last name 
                .Where(m => m.FirstName.StartsWith(search) || m.LastName.StartsWith(search) || string.IsNullOrEmpty(search))
                // only players in the specified league
                .Where(m => leagueId == 0 || Context.LeagueMembers.Any(lm => lm.LeagueId == leagueId && lm.MemberId == m.MemberId))
                // exclude players in the specified league
                .Where(m => excludeLeagueId == 0 || !Context.LeagueMembers.Any(lm => lm.LeagueId == excludeLeagueId && lm.MemberId == m.MemberId))
                // only of specified gender
                .Where(m => m.Gender == gender || gender == Gender.Unknown)
                // order by last name
                .OrderBy(m => m.LastName)
                .Take(numResults)
                .ProjectTo<MemberViewModel>(ModelMapper.Mapper.ConfigurationProvider)
                .ToListAsync();

            return members;
        }

        /// <summary>
        /// fetch player picklist for a given match.  Exclude players already in the lineup
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="matchId"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        //public async Task<IEnumerable<MemberViewModel>> GetPlayerPickList_OLD(int leagueId, int matchId, IEnumerable<LineViewModel> lines)
        //{
        //    // get list of league members who are already in line-up
        //    var lmIds = lines.SelectMany(l => l.Players).Select(p => p.LeagueMemberId).ToList();

        //    // get list of league members, excluding those already in line-up
        //    var members = await _context.LeagueMembers
        //        .Where(lm => lm.LeagueId == leagueId)
        //        .Where(lm => !lmIds.Contains(lm.LeagueMemberId))  // exlude existing line-up
        //        .Select(lm => new MemberViewModel
        //        {
        //            MemberId = lm.MemberId,
        //            UserId = lm.Member.UserId,
        //            FirstName = lm.Member.FirstName,
        //            LastName = lm.Member.LastName,
        //            HomeVenueName = lm.Member.HomeVenue.Name,
        //            IsSubstitute = lm.IsSubstitute,
        //            Gender = lm.Member.Gender,
        //            IsCaptain = lm.IsCaptain,
        //            LeagueMemberId = lm.LeagueMemberId,
        //            Availability = lm.Players.Where(p => p.MatchId == matchId).Any() ?
        //                         lm.Players.FirstOrDefault(p => p.MatchId == matchId).Availability :
        //                         Availability.Unknown,
        //            PlayerId = lm.Players.Where(p => p.MatchId == matchId).Any() ?
        //                         lm.Players.FirstOrDefault(p => p.MatchId == matchId).Id : 0
        //        })
        //        .ToListAsync();
        //    return members;
        //}

        /// <summary>
        /// get player picklist, for use in selecting players to add to a match
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="matchId"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PlayerViewModel>> GetPlayerPickList(int leagueId, int matchId, IEnumerable<PlayerViewModel> existingPlayers)
        {
            // get list of league members who are already in line-up
            var lmIds = existingPlayers?.Select(p => p.LeagueMemberId)?.ToList() ?? new List<int>();

            var players = await (from lm in Context.LeagueMembers
                .Where(lm => lm.LeagueId == leagueId)
                .Where(lm => !lmIds.Contains(lm.LeagueMemberId))  // exlude existing line-up
                                 join p in Context.Players
                                    .Where(p => p.MatchId == matchId) on lm.LeagueMemberId equals p.LeagueMemberId into matchPlayers
                                 from p in matchPlayers.DefaultIfEmpty()
                                 select new PlayerViewModel
                                 {
                                     Id = p != null ? p.Id : 0,
                                     Availability = p != null ? p.Availability : Availability.Unknown,
                                     ModifiedDate = p != null ? p.ModifiedDate : DateTime.Today,
                                     MemberId = lm.MemberId,
                                     LeagueId = lm.LeagueId,
                                     LeagueMemberId = lm.LeagueMemberId,
                                     MatchId = matchId,
                                     FirstName = lm.Member.FirstName,
                                     LastName = lm.Member.LastName,
                                     HomeVenueName = lm.Member.HomeVenue.Name,
                                     IsSubstitute = lm.IsSubstitute,
                                     Gender = lm.Member.Gender,
                                     IsCaptain = lm.IsCaptain
                                 })
                    .ToListAsync();
            
            players = players.OrderBy(p => p.IsSubstitute).ThenBy(p => p.AvailabilitySort).ThenBy(p => p.FirstName).ToList();

            return players;
        }

        public async Task<List<MemberNameViewModel>> GetOrderedSubList(int matchId)
        {
            // get list of players not in line-up, and who are not un-available, with initial sort
            var playerList = (await GetSubPickList(matchId)).ToList();

            // now add an additional sort algorithm, based on when they responded, points, etc.
            var lmids = playerList.Select(l => l.LeagueMemberId).ToList();

            var leagueId = await Context.Matches
                .Where(m => m.MatchId == matchId)
                .Select(m => m.LeagueId)
                .FirstOrDefaultAsync();

            // tuning of this algorithm can be done by adjusting these values
            var futureMatchPoints = 10;
            var recentMatchPoints = 5;
            var distantMatchPoints = 1;
            double recentTimeDays = 21;      // 3 weeks
            double distantTimeDays = 13 * 7; // 3 months
            var recentDate = DateTime.UtcNow - TimeSpan.FromDays(recentTimeDays);
            var distantDate = DateTime.UtcNow - TimeSpan.FromDays(distantTimeDays);

            var futurePlayerPoints = Context.Players
                .Where(p => lmids.Contains(p.LeagueMemberId))       // limit to these players
                .Where(p => p.LeagueId == leagueId)                 // in this league
                .Where(p => p.LineId != null)                       // that are in the line-up
                .Where(p => p.Match.StartTime > DateTime.UtcNow)    // in this time window
                .GroupBy(p => p.LeagueMemberId)                     // we want per-player information (how many matches per player?)
                .Select(p => new { LeagueMemberId = p.Key, Points = p.Count() * futureMatchPoints })
                .ToList();

            // assign points for each player who has a match in the recent past
            var recentPlayerPoints = Context.Players
                .Where(p => lmids.Contains(p.LeagueMemberId))       // limit to these players
                .Where(p => p.LeagueId == leagueId)                 // in this league
                .Where(p => p.LineId != null)                       // that are in the line-up
                .Where(p => p.Match.StartTime < DateTime.UtcNow)    // in this time window
                .Where(p => p.Match.StartTime > recentDate)         // in this time window
                .GroupBy(p => p.LeagueMemberId)                     // we want per-player information (how many matches per player?)
                .Select(p => new { LeagueMemberId = p.Key, Points = p.Count() * recentMatchPoints })
                .ToList();

            // assign points for each player who has a match in the distant past
            var distantPlayerPoints = Context.Players
                .Where(p => lmids.Contains(p.LeagueMemberId))       // limit to these players
                .Where(p => p.LeagueId == leagueId)                 // in this league
                .Where(p => p.LineId != null)                       // that are in the line-up
                .Where(p => p.Match.StartTime <= recentDate)        // in this time window
                .Where(p => p.Match.StartTime > distantDate)        // in this time window
                .GroupBy(p => p.LeagueMemberId)                     // we want per-player information (how many matches per player?)
                .Select(p => new { LeagueMemberId = p.Key, Points = p.Count() * distantMatchPoints })
                .ToList();

            foreach (var player in playerList)
            {
                player.Points =
                                futurePlayerPoints.FirstOrDefault(p => p.LeagueMemberId == player.LeagueMemberId)?.Points ?? 0
                              + recentPlayerPoints.FirstOrDefault(p => p.LeagueMemberId == player.LeagueMemberId)?.Points ?? 0
                              + distantPlayerPoints.FirstOrDefault(p => p.LeagueMemberId == player.LeagueMemberId)?.Points ?? 0;
                              
            }

            playerList = playerList
                .OrderByDescending(p => p.Availability)  // available players first
                .ThenBy(p => p.IsSubstitute)  // regulars first
                .ThenBy(p => p.Points) // low points first
                .ThenBy(p => p.ResponseDate)  // first responders first
                .ToList();

            return playerList;
        }

        /// <summary>
        /// get pick list for selection for sub-notification, when user declines a match
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MemberNameViewModel>> GetSubPickList(int matchId)
        {
            // get list of league members that are already in the line-up for this match
            var lmIds = Context.Players
                .Where(p => p.MatchId == matchId)
                .Where(p => p.LineId >= 0)
                .Select(p => p.LeagueMemberId)
                .ToList();

            var leagueId = await Context.Matches
                .Where(m => m.MatchId == matchId)
                .Select(m => m.LeagueId)
                .FirstOrDefaultAsync();

            if (leagueId == 0)
            {
                throw new Exception($"ERROR in GetSubPickList - cannot find match {matchId}");
            }

            // get list of league members in this league, except those already in line-up
            var members = await Context.LeagueMembers
                .Where(lm => lm.LeagueId == leagueId)
                .Where(lm => !lmIds.Contains(lm.LeagueMemberId))
                .Select(lm => new MemberNameViewModel
                {
                    MemberId = lm.MemberId,
                    UserId = lm.Member.UserId,
                    FirstName = lm.Member.FirstName,
                    LastName = lm.Member.LastName,
                    HomeVenueName = lm.Member.HomeVenue.Name,
                    IsSubstitute = lm.IsSubstitute,
                    Gender = lm.Member.Gender,
                    IsCaptain = lm.IsCaptain,
                    LeagueMemberId = lm.LeagueMemberId,
                    Availability = lm.Players.Where(p => p.MatchId == matchId).Any() ?
                                 lm.Players.FirstOrDefault(p => p.MatchId == matchId).Availability :
                                 Availability.Unknown,
                    ResponseDate = lm.Players.Where(p => p.MatchId == matchId).Any() ?
                                 lm.Players.FirstOrDefault(p => p.MatchId == matchId).ModifiedDate :
                                 DateTime.UtcNow
                })
                .Where(m => m.Availability != Availability.Unavailable)  // we don't want members who have said they are un-available
                .OrderByDescending(m => m.Availability)  // confirmed members on top
                .ThenBy(m => m.IsSubstitute)  // non-subs on top
                .ToListAsync();
            return members;
        }

        /// <summary>
        /// get members that are in a given league
        /// </summary>
        /// <param name="leagueId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MemberViewModel>> GetMembersByLeagueAsync(int leagueId)
        {
            var memberCompositeList = await Context.LeagueMembers
                .Where(lm => lm.LeagueId == leagueId)
                //.Where(lm => memberId == 0 || lm.MemberId == memberId)
                .Select(lm => new LeagueMemberComposite
                {
                    Member = lm.Member,
                    Venue = lm.Member.HomeVenue,
                    LeagueMember = lm
                })
                .OrderBy(m => m.Member.LastName)
                .ToListAsync();

            var memberList = new List<MemberViewModel>();
            foreach (var result in memberCompositeList)
            {
                var memberViewModel = ModelMapper.Map<MemberViewModel>(result.Member);
                memberViewModel.HomeVenue = ModelMapper.Map<VenueViewModel>(result.Venue);
                memberViewModel.IsCaptain = result.LeagueMember.IsCaptain;
                memberViewModel.IsSubstitute = result.LeagueMember.IsSubstitute;
                memberViewModel.LeagueId = result.LeagueMember.LeagueId;
                memberViewModel.LeagueMemberId = result.LeagueMember.LeagueMemberId;
                memberList.Add(memberViewModel);
            }

            return memberList;
        }

        public List<MemberViewModel> GetMembersTest()
        {
            var q = Context.Members
                .Where(m => m.FirstName.StartsWith("R"))
                .Where(mvm => mvm.Gender == Gender.Male);

            var r = q.ProjectTo<MemberViewModel>(ModelMapper.Mapper.ConfigurationProvider);

            return r.ToList();
        }

        #endregion

        #region update
        public async Task<Member> UpdateMember(Member member)
        {
            if (member is null)
            {
                throw new BadRequestException("Member to update is null");
            }
            Context.Members.Update(member);
            await Context.SaveChangesAsync();
            return member;
        }

        /// <summary>
        /// update a member
        /// </summary>
        /// <param name="memberViewModel"></param>
        /// <returns></returns>
        public async Task<Member> UpdateMemberAsync(Member member)
        {
            if (member is null)
            {
                throw new BadRequestException("Member to update is null");
            }
            Context.Members.Update(member);
            UpdatePlayerPreferences(member.MemberId, member.PlayerPreferences.ToList());

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(member.MemberId))
                {
                    throw new NotFoundException($"Update member failed: memberId ({member.MemberId}) does not exist");
                }
                else
                {
                    throw;
                }
            }
            return member;
        }

        /// <summary>
        /// update help tip tracking for specific member
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="tracker"></param>
        public async Task EnableHelpTipTrackers(int memberId, HelpTipTrackers tracker)
        {
            var member = await Context.Members.FindAsync(memberId);
            if (member != null)
            {
                // to enable the specified flags, logical-OR it with the current flags
                member.HelpTipTrackers |= tracker;
                Context.Members.Update(member);
                await Context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// disable specific help tip tracking for a given member
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="tracker"></param>
        /// <returns></returns>
        public async Task DisableHelpTipTrackers(int memberId, HelpTipTrackers tracker)
        {
            var member = await Context.Members.FindAsync(memberId);
            if (member != null)
            {
                // to disable the specified flags, invert the input and logical-AND it with the current flags
                var invertedFlags = ~((int)tracker);
                var currentFlags = (int)member.HelpTipTrackers;
                var newFlags = currentFlags &= invertedFlags;
                member.HelpTipTrackers = (HelpTipTrackers)newFlags;
                Context.Members.Update(member);
                await Context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// enable specific user pref flags for a given member
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public async Task EnableUserPreferenceFlags(int memberId, UserPreferenceFlags flags)
        {
            var member = await Context.Members.FindAsync(memberId);
            if (member != null)
            {
                // to enable the specified flags, logical-OR it with the current flags
                member.UserPreferenceFlags |= flags;
                Context.Members.Update(member);
                await Context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// disable user pref flags for a specific member
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public async Task DisableUserPreferenceFlags(int memberId, UserPreferenceFlags flags)
        {
            var member = await Context.Members.FindAsync(memberId);
            if (member != null)
            {
                // to disable the specified flags, invert the input and logical-AND it with the current flags
                var invertedFlags = ~((int)flags);
                var currentFlags = (int)member.HelpTipTrackers;
                var newFlags = currentFlags &= invertedFlags;
                member.UserPreferenceFlags = (UserPreferenceFlags)newFlags;
                Context.Members.Update(member);
                await Context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// update player prefs
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="preferences"></param>
        private void UpdatePlayerPreferences(int memberId, List<PlayerPreference> preferences)
        {
            var preferencesToDelete = Context.PlayerPreferences.Where(m => m.MemberId == memberId);
            Context.PlayerPreferences.RemoveRange(preferencesToDelete);
            foreach (var pref in preferences)
            {
                pref.PlayerPreferenceId = 0;
                pref.MemberId = memberId;
            }
            Context.PlayerPreferences.AddRange(preferences);
        }

        public async Task<LeagueMember> UpdateLeagueMemberAsync(MemberViewModel memberViewModel)
        {
            if (memberViewModel is null)
            {
                throw new BadRequestException("League member to update is null");
            }
            var leagueMember = Context.LeagueMembers
                .Where(m => m.LeagueId == memberViewModel.LeagueId)
                .Where(m => m.MemberId == memberViewModel.MemberId)
                .Include(m => m.Players)
                .FirstOrDefault();

            if (leagueMember == null)
            {
                throw new NotFoundException("Cannot find referenced LeagueMember to update");
            }

            leagueMember.IsCaptain = memberViewModel.IsCaptain;
            leagueMember.IsSubstitute = memberViewModel.IsSubstitute;
            Context.LeagueMembers.Update(leagueMember);

            // now update all associated Players, in case the IsSubstitue flag has been changed for them
            var players = leagueMember.Players;
            foreach (var p in players)
            {
                p.IsSubstitute = leagueMember.IsSubstitute;
            }

            Context.Players.UpdateRange(players);

            // save changes
            await Context.SaveChangesAsync();
            return leagueMember;
        }

        /// <summary>
        /// update member image
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public async Task UpdateMemberImageAsync(int memberId, byte[] imageData)
        {
            var memberImage = await Context.MemberImages.FirstOrDefaultAsync(i => i.MemberId == memberId);

            if (memberImage != null)
            {
                memberImage.ImageBytes = imageData;
                Context.MemberImages.Update(memberImage);
                await Context.SaveChangesAsync();
            }
            else
            {
                memberImage = new MemberImage
                {
                    MemberId = memberId,
                    ImageBytes = imageData
                };
                Context.MemberImages.Add(memberImage);
                await Context.SaveChangesAsync();
            }
        }

        #endregion

        #region delete
        /// <summary>
        /// delete member
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Member> DeleteMemberAsync(int id)
        {
            var member = await Context.Members.SingleOrDefaultAsync(m => m.MemberId == id);
            if (!MemberExists(id))
            {
                throw new NotFoundException($"DeleteMember: member id {id} not found");
            }

            Context.Members.Remove(member);
            await Context.SaveChangesAsync();
            return member;
        }

        public async Task<LeagueMember> DeleteLeagueMemberAsync(int leagueId, int memberId)
        {
            var leagueMember = await Context.LeagueMembers
                .Include(lm => lm.Players)
                .SingleOrDefaultAsync(m => m.LeagueId == leagueId && m.MemberId == memberId);
            if (leagueMember == null)
            {
                throw new NotFoundException($"No league member found for leagueId={leagueId} memberId={memberId}");
            }

            if (leagueMember.Players != null)
            {
                Context.Players.RemoveRange(leagueMember.Players);
            }

            Context.LeagueMembers.Remove(leagueMember);
            await Context.SaveChangesAsync();
            return leagueMember;
        }
        #endregion

        #region helpers

        /// <summary>
        /// check if member exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool MemberExists(int id)
        {
            return Context.Members.Any(e => e.MemberId == id);
        }

        #endregion
    }
}
