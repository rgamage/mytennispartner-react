using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using MyTennisPartner.Models.Enums;
using MyTennisPartner.Web.Services.Reservations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Models.CourtReservations.TennisBookings;
using MyTennisPartner.Models.CourtReservations;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Models.ViewModels;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Utilities;

namespace MyTennisPartner.Web.Managers
{
    /// <summary>
    /// manager to handle court reservations
    /// </summary>
    public class ReservationManager
    {
        private readonly ILogger _logger;
        private readonly TennisContext _context;
        private readonly IServiceProvider Service;
        private readonly LineManager _lineManager;
        private ICourtReservationService courtReservationService;

        /// <summary>
        /// list of target courts, accessible externally to allow callers to see which courts the algorithm would target
        /// </summary>
        public List<TargetCourtInfo> TargetCourtInfoList { get; }

        public ReservationManager(TennisContext context, ILogger<ReservationManager> logger, IServiceProvider service, LineManager lineManager) {
            _logger = logger;
            _context = context;
            _lineManager = lineManager;
            Service = service;
            TargetCourtInfoList = new List<TargetCourtInfo>();
        }

        /// <summary>
        /// reserves courts for all eligible matches
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReserveAllCourts(bool bookCourts=true)
        {
            _logger.LogInformation("ReserveAllCourts starting");
            var matches = GetUpcomingAutoReserveMatches().ToList();
            _logger.LogInformation($"ReserveAllCourts found {matches.Count} match(es), calling ReserveCourts");
            var result = await ReserveCourts(matches, bookCourts);
            _logger.LogInformation($"ReserveAllCourts result = {result}");
            return result;
        }

        /// <summary>
        /// reserves courts for a single, specific match
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public async Task<bool> ReserveCourtsSingleMatch(int matchId)
        {
            _logger.LogInformation("ReserveCourtsSingleMatch starting");
            var matches = await GetUpcomingAutoReserveMatches()
                .Where(m => m.MatchId == matchId)
                .ToListAsync();
            var result = await ReserveCourts(matches);
            return result;
        }

        /// <summary>
        /// method to update target courts for upcoming matches
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateTargetCourts(int matchId = 0)
        {
            _logger.LogInformation("UpdateTargetCourts starting");
            List<Match> matches;
            if (matchId == 0)
            {
                // all upcoming matches in next week
                matches = await GetUpcomingAutoReserveMatches(7)  // check matches upcoming in the next week (7 days)
                    .ToListAsync();
            }
            else
            {
                // specific match
                matches = await _context.Matches.Where(m => m.MatchId == matchId)
                // include the reservation info in our results
                    .Include(m => m.MatchVenue).ThenInclude(venue => venue.ReservationSystem)
                    .Include(m => m.Lines)
                    .ToListAsync();
            }
            var result = await ReserveCourts(matches, false);   // does not actually reserve courts, due to false param
            // the above call to ReserveCourts also populates the TargetCourtInfoList
            foreach (var tgi in TargetCourtInfoList)
            {
                await _lineManager.UpdateTargetCourts(tgi.MatchId, tgi.TargetCourts, tgi.CourtsAvailable);
            }
            return result;
        }

        /// <summary>
        /// fetch IQueryable collection of matches that are eligible for auto-reservation of courts
        /// </summary>
        /// <returns></returns>
        public IQueryable<Match> GetUpcomingAutoReserveMatches(int maxDaysInAdvance = 2)
        {
            // wait time on reservation day before we can make reservations (e.g. for Spare Time, this is 5 hrs, because we can't reserve until 5am if we want a day two days in future)
            var reservationDayWaitTime = TimeSpan.FromHours(5);

            // define maximum days in advance we can reserve courts

            // calculate latest allowable reservation date, based on rules above
            var utcNow = DateTime.UtcNow;
            _logger.LogInformation($"GetUpcomingAutoReserveMatches: utcNow = {utcNow}");
            _logger.LogInformation($"AppTimeZoneInfo = {ApplicationConstants.AppTimeZoneInfo}");

            var dateLocal = TimeZoneInfo.ConvertTimeFromUtc(utcNow, ApplicationConstants.AppTimeZoneInfo);
            _logger.LogInformation($"Now local time = {dateLocal}");
            var d = dateLocal - reservationDayWaitTime;
            var latestReservationDateLocal = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0) + TimeSpan.FromDays(maxDaysInAdvance + 1);
            _logger.LogInformation($"Latest reservation date = {latestReservationDateLocal}");
            var offset = (utcNow - dateLocal).Hours;
            _logger.LogInformation($"offset = {offset}");
            var latestUtcDate = new DateTime(d.Year, d.Month, d.Day, offset, 0, 0) + TimeSpan.FromDays(maxDaysInAdvance + 1);
            _logger.LogInformation($"Latest UTC date = {latestUtcDate}");

            var matches = _context.Matches
                // where match is in the future
                .Where(m => m.StartTime > utcNow)
                // but not too far in the future 
                .Where(m => m.StartTime <= latestUtcDate)
                // where user has requested auto-reserve
                .Where(m => m.AutoReserveCourts)
                // where venue has a reservation system
                .Where(m => m.MatchVenue.ReservationSystem != null)
                // where there are any courts that have not already been reserved
                .Where(m => m.Lines.Any(l => !l.IsReserved))
                // group by venue, then by start time
                .OrderBy(m => m.MatchVenue.VenueId)
                .ThenBy(m => m.StartTime)
                // include the reservation info in our results
                .Include(m => m.MatchVenue).ThenInclude(venue => venue.ReservationSystem)
                .Include(m => m.Lines);

            return matches;
        }

        /// <summary>
        /// method to reserve courts for any upcoming matches that meet the criteria
        /// pass in matches collection, which must include match.venue.reservationsystem and match.lines
        /// </summary>
        /// <param name="matches"></param>
        /// <returns></returns>
        public async Task<bool> ReserveCourts(IEnumerable<Match> matches, bool bookCourts = true)
        {
            TargetCourtInfoList.Clear();
            _logger.LogInformation($"Starting ReserveCourts method, bookCourts = {bookCourts}");
            if (matches == null || !matches.Any())
            {
                // nothing to do, no matches in collection
                return false;
            }

            // note: matches collection must include match.venue.reservationsystem and match.lines
            foreach(var match in matches)
            {
                _logger.LogInformation($"Processing match {match.MatchId}");

                var tgi = new TargetCourtInfo(match.MatchId);

                if (match.MatchVenue.ReservationSystem == null)
                {
                    break;
                }
                switch(match.MatchVenue.ReservationSystem.CourtReservationProvider)
                {
                    case CourtReservationProvider.TennisBookings:
                        courtReservationService =
                            Service
                                .GetRequiredService(typeof(CourtReservationServiceSpareTime)) as CourtReservationServiceSpareTime;
                        break;
                    case CourtReservationProvider.LifetimeFitness:
                        courtReservationService =
                            Service
                                .GetRequiredService(typeof(CourtReservationServiceLifetime)) as CourtReservationServiceLifetime;
                        break;
                }

                _logger.LogInformation("Getting credentials of players");

                List<CredentialDto> credentials;

                if (!bookCourts)
                {
                    // get credentials of all players on roster
                    credentials = await _context.LeagueMembers
                        .Where(lm => lm.LeagueId == match.LeagueId)
                        .Where(lm => lm.Member.SpareTimeMemberNumber > 0)
                        .Where(lm => lm.Member.AutoReserveVenues != null)
                        .Where(lm => lm.Member.AutoReserveVenues.Contains($"[{match.MatchVenueVenueId}]"))
                        .Select(lm => new CredentialDto
                        {
                            MemberId = lm.MemberId,
                            FirstName = lm.Member.FirstName,
                            LastName = lm.Member.LastName,
                            Username = lm.Member.SpareTimeUsername,
                            Password = lm.Member.SpareTimePassword,
                            MemberNumber = lm.Member.SpareTimeMemberNumber
                        })
                        .OrderByDescending(c => c.MemberId)
                        .Take(1)  // we only need one credential to check court avail
                        .ToListAsync();
                }
                else
                {
                    // get the login credentials of players that are in the line-up and that have credentials for that particular venue
                    credentials = await _context.Players
                        .Where(p => p.MatchId == match.MatchId)
                        .Where(p => p.Member.SpareTimeMemberNumber > 0)
                        .Where(p => p.LineId > 0 || !bookCourts)  // is in the line-up, or we are not really booking the courts (this means anyone on the roster can be used for credentials, because we are just checking availability)
                        .Where(p => p.Member.AutoReserveVenues != null)
                        .Where(p => p.Member.AutoReserveVenues.Contains($"[{match.MatchVenueVenueId}]"))
                        .Select(p => new CredentialDto
                        {
                            MemberId = p.MemberId,
                            FirstName = p.Member.FirstName,
                            LastName = p.Member.LastName,
                            Username = p.Member.SpareTimeUsername,
                            Password = p.Member.SpareTimePassword,
                            MemberNumber = p.Member.SpareTimeMemberNumber
                        })
                        .ToListAsync();
                }

                _logger.LogInformation($"Found {credentials.Count} valid credential(s)");

                var playerLines = await _lineManager.GetLinesByMatchAsync(match.MatchId);
                var unreservedLines = playerLines.Lines.Where(l => !l.IsReserved).ToList();

                // limit to courts that have not already been reserved
                playerLines.Lines.Clear();
                playerLines.Lines.AddRange(unreservedLines);

                foreach (var line in playerLines.Lines)
                {
                    // players property comes un-populated, so populate with proper players, so they're easier to access later
                    line.Players.AddRange(playerLines.Players.Where(p => p.LineId == line.LineId).ToList());
                }

                var lines = match.Lines.Where(l => !l.IsReserved).ToList();
                var courtsNeeded = lines.Count;

                // disabled this check for now - it interferes with the updateTargetCourts feature, and is not needed anyway,
                // as later we will check if each court has one credential
                //if (courtsNeeded > credentials.Count)
                //{
                //    // there are more courts than registered members, so we cannot reserve the courts
                //    var message = "Auto Court Reservation failed: Not enough club members have credentials to reserve the courts";
                //    _logger.LogError(message);
                //    throw new Exception(message);
                //}

                if (!credentials.Any())
                {
                    // there are more courts than registered members, so we cannot reserve the courts
                    var message = $"Auto Court Reservation failed: No members in this {ApplicationConstants.league} have credentials to reserve the courts";
                    _logger.LogError(message);
                    throw new Exception(message);
                }

                var requestedDuration = (match.EndTime - match.StartTime).TotalMinutes;
                var maxDuration = 90; // todo: get this from a global setting, or venue-specific limit setting?
                if (requestedDuration > maxDuration) 
                {
                    // trying to reserve court for too long duration
                    var message = $"Auto Court Reservation failed: Requested courts for {requestedDuration} minutes, max allowed is {maxDuration}";
                    _logger.LogError(message);
                    throw new Exception(message);
                }

                // get viewmodel to translate start, end times to local time
                var matchViewModel = ModelMapper.Map<MatchSummaryViewModel>(match);

                // use one of the players' credentials to get list of all courts available
                courtReservationService.SetHost(match.MatchVenue.ReservationSystem.HostName);
                var courtBookings = courtReservationService.GetCourtAvailability(credentials[0].Username, credentials[0].Password, match.StartTime, !bookCourts);
                var courtsAvailable = courtBookings.CourtsAvailable(matchViewModel.StartTimeLocal, matchViewModel.EndTimeLocal);
                var freeCourts = courtsAvailable.Count;
                tgi.CourtsAvailable = string.Join(",", courtsAvailable);

                _logger.LogInformation($"Found {freeCourts} free court(s), {courtsNeeded} needed");

                if (freeCourts < courtsNeeded)
                {
                    var message = $"Auto Court Reservation failed: There are not enough free courts for this match.  Need {courtsNeeded} court{(courtsNeeded > 1 ? "s" : "")}, only {freeCourts} {(freeCourts > 1 ? "are" : "is")} available.";
                    _logger.LogError(message);
                    throw new Exception(message);
                }

                _logger.LogInformation($"Found the following total courts available: {string.Join(",", courtsAvailable)}");

                // as a baseline, just target the first n courts in the list for our reservation
                var targetCourts = courtsAvailable.Take(courtsNeeded).ToList();

                _logger.LogInformation($"Target courts are the following: {string.Join(",", targetCourts)}");

                // the target courts may not be neighboring courts, so...
                // identify a group of free courts that are neighboring courts if possible (consecutively numbered courts)
                var i = 0;
                var neighboringTargetCourts = new List<string>();
                while (i < freeCourts)
                {
                    neighboringTargetCourts.Add(courtsAvailable[i]);
                    if (StringHelper.ConsecutiveNumbers(neighboringTargetCourts))
                    {
                        if (neighboringTargetCourts.Count == targetCourts.Count)
                        {
                            // we have found enough courts that are neighbors, so use these as our target courts
                            targetCourts = neighboringTargetCourts;
                            _logger.LogInformation($"Set target courts to neighboring courts: {string.Join(",", targetCourts)}");
                            break;
                        }
                        i++;
                    }
                    else
                    {
                        // we have skipped a number, so start again with a fresh list
                        neighboringTargetCourts.Clear();
                    }
                }

                // check if all the court numbers already specified by users are available.  If so, target those courts.
                // these will override any of the above target courts, because we assume they are the users' first choices
                if (lines.All(l => courtsAvailable.Any(c => l.CourtNumber == c)))
                {
                    targetCourts = lines
                        // sort by court number so we keep the same order of lineup vs. requested court number
                        // the lines we fetched earlier are ordered by court number, so they will align
                        .OrderBy(l => StringHelper.ZeroPadLeft(l.CourtNumber))
                        .Select(l => l.CourtNumber)
                        .ToList();
                        _logger.LogInformation($"Set target courts to user-specified courts: {string.Join(",", targetCourts)}");
                }

                // set public property for this match
                tgi.TargetCourts.AddRange(targetCourts);
                TargetCourtInfoList.Add(tgi);

                // bail out now if we are not actually booking courts - use this option if you just wanted to see which target courts would be selected
                if (!bookCourts) continue;

                // join players with credentials, and figure out how many distinct lines (courts) we end up with
                var cred = (from player in playerLines.Lines.SelectMany(l => l.Players)
                            join credential in credentials on player.MemberId equals credential.MemberId
                            select player.LineId)
                           .Distinct();

                if (cred.Count() < courtsNeeded)
                {
                    // not every court has a registered member, so we cannot reserve the courts
                    var message = "Auto Court Reservation failed: Every court needs a member with credentials to reserve the courts";
                    _logger.LogError(message);
                    throw new Exception(message);
                }

                // now iterate through the lines (courts) that have not already been reserved, and reserve the target courts for each one
                // note - use case where some courts have been reserved previously, some have not, may result in courts not all together
                foreach (var line in playerLines.Lines)
                {
                    var players = line.Players;
                    var playingMemberIds = players.Select(p => p.MemberId);
                    var credential = credentials.Where(c => playingMemberIds.Contains(c.MemberId)).FirstOrDefault();
                    if (credential == null)
                    {
                        var message = $"Auto Court Reservation failed: Court {line.CourtNumber} does not have a player with login credentials to book the court";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }

                    // figure out which player will be the one reserving the court.  Put him at the top of the list
                    var reservingPlayer = players.FirstOrDefault(p => p.MemberId == credential.MemberId);
                    //players.Remove(reservingPlayer);
                    //players.Insert(0, reservingPlayer);
                    // instead of adding all players, try just adding the one reserving the court
                    players = new List<PlayerViewModel> { reservingPlayer };

                    // get the next target court
                    var courtNumberString = targetCourts[0];
                    targetCourts.RemoveAt(0);

                    var success = int.TryParse(courtNumberString, out int courtNumber);
                    if (!success)
                    {
                        var message = $"Auto Court Reservation failed: Court number is invalid: [{line.CourtNumber}]";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }

                    var members = players
                          .Select(p => new ReservationMember
                          {
                              FirstName = p.FirstName,
                              LastName = p.LastName,
                              MemberNumber = p.SpareTimeMemberNumber ?? 0
                          })
                          .ToList();

                    var reservationDetails = new ReservationDetails(
                        members,
                        new TimeSlot
                        {
                            StartTime = matchViewModel.StartTimeLocal,
                            EndTime = matchViewModel.EndTimeLocal
                        },
                        match.Format.IsDoubles(),
                        courtNumber
                        );
                    
                    var reserved = courtReservationService.ReserveCourts(credential.Username, credential.Password, reservationDetails);
                    if (reserved)
                    {
                        _logger.LogInformation($"successfully reserved court {reservationDetails.CourtNumber} for match {line.MatchId}");
                        // update database to reflect we have reserved these courts
                        var updatedLine = lines.First(l => l.LineId == line.LineId);
                        updatedLine.IsReserved = true;
                        updatedLine.CourtNumber = reservationDetails.CourtNumber.ToString();
                    }
                    else
                    {
                        _logger.LogError($"Failed to reserve court {reservationDetails.CourtNumber} for match {line.MatchId}");
                    }
                }
                // save court updates to database
                _context.Lines.UpdateRange(lines);
                await _context.SaveChangesAsync();
            }
            return true;
        }
    }
}
