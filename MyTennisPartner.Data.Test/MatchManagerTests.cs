using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.Enums;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Models.ViewModels;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Models.Exceptions;

namespace MyTennisPartner.Data.Test
{
    [TestClass]
    public class MatchManagerTests: ManagerTestBase, IDisposable
    {
        private MatchManager _matchManager;
        private readonly ILogger<MatchManager> logger;

#pragma warning disable CS0162
        public MatchManagerTests(): base()
        {
            logger = new Moq.Mock<ILogger<MatchManager>>().Object;
        }

        [TestMethod]
        public async Task CanUpdateMatchLine()
        {
            // seed some data
            Seed(SeedOptions.WithPlayersInLineup);
            int matchId = Match1.MatchId;
            Assert.AreNotEqual(0, Match1.MatchId);

            // now update a line
            Assert.AreNotEqual("99", Line1.CourtNumber);
            Line1.CourtNumber = "99";

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                await _matchManager.UpdateMatchAsync(Match1);
            }

            var notification = _matchManager.NotificationEvents
                .Where(n => n.EventType == NotificationEventType.CourtChange)
                .Where(n => n.LeagueMemberIds.Contains(LeagueMember1.LeagueMemberId))
                .FirstOrDefault();
            Assert.IsNotNull(notification);

            using (var context = new TennisContext(Options))
            {
                var match2 = context.Matches.Include(m => m.Lines).Where(m => m.MatchId == matchId).FirstOrDefault();
                Assert.AreEqual("99", match2.Lines.First().CourtNumber);
            }

            using (var context = new TennisContext(Options))
            {
                // now change various propertie of the match, check for notifications
                _matchManager = new MatchManager(context, logger);

                // change start time
                Match1.StartTime = DateTime.UtcNow + TimeSpan.FromDays(1);
                await _matchManager.UpdateMatchAsync(Match1);

                notification = _matchManager.NotificationEvents
                    .Where(n => n.EventType == NotificationEventType.MatchChanged)
                    .Where(n => n.LeagueMemberIds.Contains(Player1.LeagueMemberId))
                    .FirstOrDefault();
                Assert.IsNotNull(notification);

                // change end time
                _matchManager.NotificationEvents.Clear();
                Match1.EndTime = DateTime.UtcNow + TimeSpan.FromDays(2);
                await _matchManager.UpdateMatchAsync(Match1);

                notification = _matchManager.NotificationEvents
                    .Where(n => n.EventType == NotificationEventType.MatchChanged)
                    .Where(n => n.LeagueMemberIds.Contains(Player1.LeagueMemberId))
                    .FirstOrDefault();
                Assert.IsNotNull(notification);

                // change warm-up time
                _matchManager.NotificationEvents.Clear();
                Match1.WarmupTime = DateTime.UtcNow + TimeSpan.FromDays(2);
                await _matchManager.UpdateMatchAsync(Match1);

                notification = _matchManager.NotificationEvents
                    .Where(n => n.EventType == NotificationEventType.MatchChanged)
                    .Where(n => n.LeagueMemberIds.Contains(Player1.LeagueMemberId))
                    .FirstOrDefault();
                Assert.IsNotNull(notification);

                // change venue
                var venue2 = new Venue { Name = "new venue" };
                context.Venues.Add(venue2);
                context.SaveChanges();

                _matchManager.NotificationEvents.Clear();
                Match1.MatchVenue = venue2;
                Match1.MatchVenueVenueId = venue2.VenueId;
                await _matchManager.UpdateMatchAsync(Match1);

                notification = _matchManager.NotificationEvents
                    .Where(n => n.EventType == NotificationEventType.MatchChanged)
                    .Where(n => n.LeagueMemberIds.Contains(Player1.LeagueMemberId))
                    .FirstOrDefault();
                Assert.IsNotNull(notification);

                // change home/away status
                _matchManager.NotificationEvents.Clear();
                Match1.HomeMatch = true;
                await _matchManager.UpdateMatchAsync(Match1);

                notification = _matchManager.NotificationEvents
                    .Where(n => n.EventType == NotificationEventType.MatchChanged)
                    .Where(n => n.LeagueMemberIds.Contains(Player1.LeagueMemberId))
                    .FirstOrDefault();
                Assert.IsNotNull(notification);
            }
        }

        [TestMethod]
        public async Task CanUpdateLineMemberAvailability()
        {
            Seed(SeedOptions.WithPlayers);
            int player1Id = Player1.Id;
            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);

                // add player to line-up, by setting the player's line Id
                Player1.LineId = Line1.LineId;
                await _matchManager.UpdateMatchAsync(Match1);
            }
            using (var context = new TennisContext(Options))
            {
                // verify player1 is now in the line-up under line1
                var line1Players = context.Players
                    .Where(p => p.MatchId == Match1.MatchId && p.LineId == Line1.LineId);
                Assert.IsNotNull(line1Players);
                Assert.AreEqual(1, line1Players.Count());
                Assert.AreEqual(player1Id, line1Players.First().Id);
            }

            // now update a linemember's availability
            Assert.AreEqual(Availability.Unknown, Player1.Availability);
            Assert.IsNotNull(Player1);
            Player1.Availability = Availability.Unavailable;
            Assert.AreEqual(Availability.Unavailable, Player1.Availability);

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                await _matchManager.UpdateMatchAsync(Match1);
            }

            using (var context = new TennisContext(Options))
            {
                var line1Player1 = context.Players
                    .Where(p => p.LineId == Line1.LineId)
                    .First();

                Assert.AreEqual(Player1.Id, line1Player1.Id);
                Assert.AreEqual(Availability.Unavailable, line1Player1.Availability);
            }
        }

        [TestMethod]
        public async Task CanDeleteLineFromMatch()
        {
            Seed(SeedOptions.WithPlayers);
            int player1Id = Player1.Id;
            int matchId = Match1.MatchId;
            using (var context = new TennisContext(Options))
            {
                // set player1 to confirmed avail
                Player1.Availability = Availability.Confirmed;
                context.Players.Update(Player1);
                context.SaveChanges();

                _matchManager = new MatchManager(context, logger);

                // confirm no players in line-up
                var linePlayers = context.Players.Where(p => p.LineId > 0).ToList();
                Assert.AreEqual(0, linePlayers.Count);

                // add player to line-up, by setting the player's line Id
                Player1.LineId = Line1.LineId;
                await _matchManager.UpdateMatchAsync(Match1);
            }
            using (var context = new TennisContext(Options)) {
                // confirm we now have a player in the lineup
                var p1 = context.Players.Where(p => p.LineId == Line1.LineId).ToList();
                Assert.IsNotNull(p1);
                Assert.AreEqual(1, p1.Count);
            }

            // now delete a line from the match
            Match1.Lines.Remove(Line1);
            Line1.Match = null;
            Line1.MatchId = 0;

            // send this match to the matchManager to update db
            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                await _matchManager.UpdateMatchAsync(Match1);
            }

            using (var context = new TennisContext(Options))
            {
                // confirm there is now no line for this match
                var matchA = context.Matches
                    .Include(m => m.Lines)
                    .Where(m => m.MatchId == matchId)
                    .FirstOrDefault();
                Assert.AreEqual(0, matchA.Lines.Count);
            }
        }

        [TestMethod]
        public async Task CanAddLineToMatch()
        {
            Seed();

            // getNewMatch returns a new match with one empty line added, but we are going to add another one
            var line2 = new Line
            {
                MatchId = Match1.MatchId,
                CourtNumber = "2"
            };

            Match1.Lines.Add(line2);
            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var m = await _matchManager.UpdateMatchAsync(Match1);
            }

            using (var context = new TennisContext(Options))
            {
                var match2 = context.Matches
                    .Include(m => m.Lines)
                    .Where(m => m.MatchId == Match1.MatchId)
                    .FirstOrDefault();
                var expectedNumberOfLines = 2;  // one from seeding, plus one added in this test = 2 expected
                Assert.AreEqual(expectedNumberOfLines, match2.Lines.Count);
                Assert.AreEqual(line2.CourtNumber, Match1.Lines.Last().CourtNumber);
            }

        }

        [TestMethod]
        public async Task CanUpdateMatchWithNoLines()
        {

            Seed();

            // remove line
            using (var context = new TennisContext(Options))
            {
                context.Lines.Remove(Line1);
                context.SaveChanges();
            }

            // confirm properties of match 
            Assert.AreNotEqual(PlayFormat.GroupLesson, Match1.Format);
            Assert.IsFalse(Match1.HomeMatch);

            // now make some changes to the match, see if it gets updated properly
            Match1.Format = PlayFormat.GroupLesson;
            Match1.HomeMatch = true;

            using (var context = new TennisContext(Options))
            {
                // save these changes
                _matchManager = new MatchManager(context, logger);
                await _matchManager.UpdateMatchAsync(Match1);
            }

            using (var context = new TennisContext(Options))
            {
                // now fetch the same match, see if properties have been updated
                var matchUpdated = await context.Matches.FindAsync(Match1.MatchId);
                Assert.AreEqual(PlayFormat.GroupLesson, matchUpdated.Format);
                Assert.IsTrue(matchUpdated.HomeMatch);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Could not find league")]
        public void CanGetNextMatchTimeInvalidLeague()
        {
            var league = new League
            {
                Name = "Test League",
                MeetingFrequency = Frequency.Weekly,
                MeetingDay = DayOfWeek.Friday,
                MatchStartTime = new TimeSpan(10, 0, 0) // 10am
            };
            using (var context = new TennisContext(Options))
            {
                // we have not saved our new league to the db, so it won't be found by the match manager.  We expect an exception in this case
                _matchManager = new MatchManager(context, logger);
                // set reference time to a Wednesday.  Next match slot should be on Friday of this same week (4/27)
                var refTime = new DateTime(2018, 4, 27, 11, 0, 0, DateTimeKind.Utc);
                var startTime = _matchManager.GetNextAvailableMatchTime(league.LeagueId, refTime);
            }

        }

        [TestMethod]
        public void CanGetNextMatchTimeThisWeek()
        {
            var league = new League
            {
                Name = "Test League",
                MeetingFrequency = Frequency.Weekly,
                MeetingDay = DayOfWeek.Friday,
                MatchStartTime = new TimeSpan(10, 0, 0) // 10am
            };
            using (var context = new TennisContext(Options))
            {
                context.Leagues.Add(league);
                context.SaveChanges();

                // ensure we got an id assigned to this league
                Assert.AreNotEqual(0, league.LeagueId);
                _matchManager = new MatchManager(context, logger);
                // set reference time to a Wednesday.  Next match slot should be on Friday of this same week (4/27)
                var refTime = new DateTime(2018, 4, 27, 11, 0, 0, DateTimeKind.Utc);
                var startTime = _matchManager.GetNextAvailableMatchTime(league.LeagueId, refTime);
                Assert.AreEqual(DayOfWeek.Friday, startTime.DayOfWeek);
                Assert.AreEqual(league.MatchStartTime.Hours, startTime.Hour);
                Assert.AreEqual(league.MatchStartTime.Minutes, startTime.Minute);
                Assert.AreEqual(4, startTime.Day);
            }
        }

        [DataRow(4, 27, 11, Frequency.Weekly, 5, 4)]
        [DataRow(4, 27, 9, Frequency.Weekly, 4, 27)]
        [DataRow(4, 28, 9, Frequency.Weekly, 5, 4)]
        [DataRow(5, 6, 9, Frequency.Weekly, 5, 11)]
        [DataRow(4, 27, 11, Frequency.BiWeekly, 5, 4)]
        [DataRow(4, 27, 9, Frequency.BiWeekly, 4, 27)]
        [DataRow(4, 28, 9, Frequency.BiWeekly, 5, 4)]
        [DataRow(5, 6, 9, Frequency.BiWeekly, 5, 11)]
        [DataRow(4, 27, 11, Frequency.TriWeekly, 5, 4)]
        [DataRow(4, 27, 9, Frequency.TriWeekly, 4, 27)]
        [DataRow(4, 28, 9, Frequency.TriWeekly, 5, 4)]
        [DataRow(5, 6, 9, Frequency.TriWeekly, 5, 11)]
        [DataRow(4, 27, 11, Frequency.AdHoc, 5, 4)]
        [DataRow(4, 27, 9, Frequency.AdHoc, 4, 27)]
        [DataRow(4, 28, 9, Frequency.AdHoc, 5, 4)]
        [DataRow(5, 6, 9, Frequency.AdHoc, 5, 11)]
        [DataTestMethod]
        public void CanGetNextMatchTime(int monthToday, int dayToday, int hourToday, Frequency leagueFrequency, int matchMonth, int matchDay)
        {
            var league = new League
            {
                Name = "Test League",
                MeetingFrequency = leagueFrequency,
                MeetingDay = DayOfWeek.Friday,
                MatchStartTime = new TimeSpan(10, 0, 0) // 10am
            };
            using (var context = new TennisContext(Options))
            {
                context.Leagues.Add(league);
                context.SaveChanges();

                // ensure we got an id assigned to this league
                Assert.AreNotEqual(0, league.LeagueId);
                _matchManager = new MatchManager(context, logger);
                // set reference time to a Wednesday.  Next match slot should be on Friday of this same week (4/27)
                var refTime = new DateTime(2018, monthToday, dayToday, hourToday, 0, 0, DateTimeKind.Utc);
                var startTime = _matchManager.GetNextAvailableMatchTime(league.LeagueId, refTime);
                if (league.MeetingFrequency != Frequency.AdHoc)
                {
                    Assert.AreEqual(league.MeetingDay, startTime.DayOfWeek);
                }
                Assert.AreEqual(league.MatchStartTime.Hours, startTime.Hour);
                Assert.AreEqual(league.MatchStartTime.Minutes, startTime.Minute);
                Assert.AreEqual(matchMonth, startTime.Month);
                Assert.AreEqual(matchDay, startTime.Day);
            }
        }

        [TestMethod]
        public void CanGetNextMatchTimeWithMatches()
        {
            var league = new League
            {
                Name = "Test League",
                MeetingFrequency = Frequency.Weekly,
                MeetingDay = DayOfWeek.Friday,
                MatchStartTime = new TimeSpan(10, 0, 0) // 10am
            };
            DateTime startTime;
            DateTime refTime;
            Match match;
            using (var context = new TennisContext(Options))
            {
                var venue = new Venue
                {
                    Name = "test vewnue"
                };
                context.Venues.Add(venue);
                context.SaveChanges();
                match = new Match
                {
                    StartTime = new DateTime(2018, 4, 20, 10, 0, 0, DateTimeKind.Utc),
                    LeagueId = league.LeagueId,
                    MatchVenue = venue,
                    MatchVenueVenueId = venue.VenueId
                };
                league.Matches.Add(match);
                context.Leagues.Add(league);
                context.SaveChanges();

                // ensure we got an id assigned to this league
                Assert.AreNotEqual(0, league.LeagueId);
                _matchManager = new MatchManager(context, logger);
                // set reference time to a Wednesday.  Next match slot should be on Friday of this same week (4/27)
                refTime = new DateTime(2018, 4, 25, 9, 0, 0, DateTimeKind.Utc);
                startTime = _matchManager.GetNextAvailableMatchTime(league.LeagueId, refTime);
            }

            using (var context = new TennisContext(Options)) {
                _matchManager = new MatchManager(context, logger);
                if (league.MeetingFrequency != Frequency.AdHoc)
                {
                    Assert.AreEqual(league.MeetingDay, startTime.DayOfWeek);
                }
                Assert.AreEqual(league.MatchStartTime.Hours, startTime.Hour);
                Assert.AreEqual(league.MatchStartTime.Minutes, startTime.Minute);
                Assert.AreEqual(4, startTime.Month);
                Assert.AreEqual(27, startTime.Day);

                league.MeetingFrequency = Frequency.BiWeekly;
                context.Leagues.Update(league);
                startTime = _matchManager.GetNextAvailableMatchTime(league.LeagueId, refTime);
                Assert.AreEqual(5, startTime.Month);
                Assert.AreEqual(4, startTime.Day);

                league.MeetingFrequency = Frequency.TriWeekly;
                startTime = _matchManager.GetNextAvailableMatchTime(league.LeagueId, refTime);
                Assert.AreEqual(5, startTime.Month);
                Assert.AreEqual(11, startTime.Day);

                // test if existing match is in future
                match.StartTime = new DateTime(2018, 5, 4, 10, 0, 0, DateTimeKind.Utc);
                startTime = _matchManager.GetNextAvailableMatchTime(league.LeagueId, refTime);
                Assert.AreEqual(5, startTime.Month);
                Assert.AreEqual(25, startTime.Day);
            }
        }

        [TestMethod]  
        public async Task CanGetUnansweredAvailabilities()
        {
            Seed(SeedOptions.WithPlayers);
            LeagueMember1.IsSubstitute = false;
            LeagueMember2.IsSubstitute = true;

            using (var context = new TennisContext(Options))
            {
                var mgr = new MatchManager(context, logger);

                // we expect 1 result for player 1, because there is no Player yet existing for this league member for this match
                // (they have not responded)
                var matchAvails = await mgr.GetUnansweredAvailabilities(Member1.MemberId);
                Assert.AreEqual(1, matchAvails.Count);

                // we expect one result for player 2, because there is a Player, but it's set to unknown, which is 
                // effectively the same as 'unanswered'
                matchAvails = await mgr.GetUnansweredAvailabilities(Member2.MemberId);
                Assert.AreEqual(1, matchAvails.Count);

                // now 'answer' the availability for player 2 by setting a value != unknown
                Player2.Availability = Availability.Confirmed;
                context.Players.Update(Player2);
                context.SaveChanges();
                // now we should expect 1 result for member 2, but it should show him as confirmed
                matchAvails = await mgr.GetUnansweredAvailabilities(Member2.MemberId);
                Assert.AreEqual(1, matchAvails.Count);
                var avail = matchAvails[0].Matches[0].Availability;
                Assert.AreEqual(Availability.Confirmed, avail);
            }
        }

        [TestMethod]
        public async Task CanUpdateMatchWithAddedAvailability()
        {          
            Seed();

            // save ids for later
            var matchId = Match1.MatchId;
            Match updatedMatch;
            using (var context = new TennisContext(Options))
            {
                var player = new Player
                {
                    LeagueId = League1.LeagueId,
                    LeagueMemberId = LeagueMember1.LeagueMemberId,
                    MemberId = Member1.MemberId
                };

                Match1.Players.Add(player);
                var mgr = new MatchManager(context, logger);
                updatedMatch = await mgr.UpdateMatchAsync(Match1);
            }

            // now check the availability is there
            using (var context = new TennisContext(Options))
            {
                var checkPlayers = context.Players.FirstOrDefault(p => p.MatchId == Match1.MatchId && p.MemberId == Member1.MemberId);
                Assert.IsNotNull(checkPlayers);                
            }

            // test changing player avail using updateMatchAsync
            using (var context = new TennisContext(Options))
            {
                var player1 = updatedMatch.Players.First();
                player1.Availability = Availability.Unavailable;
                _matchManager = new MatchManager(context, logger);
                var x = await _matchManager.UpdateMatchAsync(updatedMatch);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var x = await _matchManager.DeleteMatch(updatedMatch.MatchId);
            }

        }

        [TestMethod]
        public async Task CanAddAndRemovePlayers()
        {
            Seed();
            int player1Id;
            using (var context = new TennisContext(Options))
            {

                var player1 = new Player
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    LeagueMemberId = LeagueMember1.LeagueMemberId,
                    MemberId = Member1.MemberId,
                    Availability = Availability.Confirmed
                };

                var player2 = new Player
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    LeagueMemberId = LeagueMember2.LeagueMemberId,
                    MemberId = Member2.MemberId,
                    Availability = Availability.Unknown
                };
                Match1.Players.Add(player1);
                context.Players.Add(player1);
                context.Matches.Update(Match1);
                await context.SaveChangesAsync();

                // add new match players
                Match1.Players.Add(player2);
                context.Matches.Update(Match1);
                await context.SaveChangesAsync();

                // add new player to match
                player1Id = player1.Id;
                Assert.AreNotEqual(0, player1Id);
                Assert.AreNotEqual(0, player1.MatchId);
                Assert.IsNotNull(player1.MatchId);

                // can remove player
                context.Players.Remove(player1);
                await context.SaveChangesAsync();
                var p1 = context.Players.Find(player1.Id);
                Assert.IsNull(p1);
            }
        }

        [TestMethod]
        public async Task CanAddAndRemovePlayersFromMatch()
        {           
            Seed(SeedOptions.WithPlayers);

            using (var context = new TennisContext(Options))
            {
                // confirm there are two players
                Assert.AreEqual(2, Match1.Players.Count());
                
                // test if we can delete all players
                context.Players.Remove(Player1);
                context.Players.Remove(Player2);
                await context.SaveChangesAsync();

                // confirm there are now no players
                Assert.AreEqual(0, Match1.Players.Count());
                context.Matches.Update(Match1);
                Assert.AreEqual(0, Match1.Players.Count());
                await context.SaveChangesAsync();
                Assert.AreEqual(0, Match1.Players.Count());
            }
        }

        [TestMethod]
        public async Task CanAddUnavailablePlayerToMatch()
        {
            Seed(SeedOptions.WithPlayers);
            using (var context = new TennisContext(Options))
            {
                var p1 = Match1.Players.First();
                p1.Availability = Availability.Unavailable;
                p1.LineId = Line1.LineId;
                var mgr = new MatchManager(context, logger);
                await mgr.UpdateMatchAsync(Match1);
            }
        }

        [TestMethod]
        public async Task CanAddAndRemovePlayersFromLineup()
        {
            Seed(SeedOptions.WithPlayers);
 
            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                // add player to line-up, by setting the player's line Id
                Player1.LineId = Line1.LineId;
                await _matchManager.UpdateMatchAsync(Match1);
            }
            using (var context = new TennisContext(Options))
            {
                // confirm player is in lineup
                var playerList = context.Players.Where(p => p.LineId == Line1.LineId).ToList();
                Assert.AreEqual(1, playerList.Count);
            }

            // check if correect notification was registered for this event, for this member
            var notification = _matchManager.NotificationEvents
                .Where(n => n.EventType == NotificationEventType.AddedToMatch)
                .Where(n => n.LeagueMemberIds.Contains(LeagueMember1.LeagueMemberId))
                .FirstOrDefault();
            Assert.IsNotNull(notification);

            // delete a player from the line-up
            Player1.LineId = null;

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var newMatch = await _matchManager.UpdateMatchAsync(Match1);
            }
            using (var context = new TennisContext(Options))
            {
                // test if player is no longer in line-up
                var playerList = context.Players.Where(p => p.LineId == Line1.LineId).ToList();
                Assert.AreEqual(0, playerList.Count);
            }

            // check notification is registered
            notification = _matchManager.NotificationEvents
                .Where(n => n.EventType == NotificationEventType.RemovedFromMatch)
                .Where(n => n.MemberIds.Contains(LeagueMember1.MemberId))
                .FirstOrDefault();
            Assert.IsNotNull(notification);

            // clear player list for this match with empty list
            Match1.Players.Clear();
            Match1.League = null;

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var match2 = await _matchManager.UpdateMatchAsync(Match1);
            }
            using (var context = new TennisContext(Options))
            {
                var match3 = context.Matches.Include(m => m.Players).First();
                Assert.AreEqual(0, match3.Players.Count());
            }
        }

        /// <summary>
        /// test that player 2 can take player 1's spot in the line-up
        /// </summary>
        /// <returns></returns>
        [TestMethod]
         public async Task CanUpdateMatchPlayer2()
        {
            Seed(SeedOptions.WithPlayers);

            // only add player 1 to the line-up
            Player1.LineId = Line1.LineId;

            using (var context = new TennisContext(Options))
            {
                context.Players.Update(Player1);
                context.SaveChanges();
            }

            // save ids for later
            var matchId = Match1.MatchId;
            using (var context = new TennisContext(Options))
            {
                // verify that member 1 is in the match
                var checkLeagueMember1AddedToMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember1AddedToMatch);

                // verify member 2 is NOT in match
                var checkLeagueMember2NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId == null);
                Assert.IsNotNull(checkLeagueMember2NotInMatch);
            }
            
            using (var context = new TennisContext(Options))
            {
                var mgr = new MatchManager(context, logger);
                await mgr.UpdateMatchPlayer("2", matchId, League1.LeagueId, Availability.Confirmed, LeagueMember1.MemberId);
            }

            // now check the availability is there for player 2, who should have taken player 1's place
            using (var context = new TennisContext(Options))
            {
                // now verify member 1 is NOT in the match, and member 2 is in the match
                var checkLeagueMember1NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId == null);
                Assert.IsNotNull(checkLeagueMember1NotInMatch);

                // verify member 2 is in match
                var checkLeagueMember2InMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember2InMatch);

                var player = context.Players.FirstOrDefault(p => p.MatchId == Match1.MatchId && p.MemberId == Member2.MemberId);
                Assert.IsNotNull(player);
                Assert.AreEqual(Availability.Confirmed, player.Availability);
            }
        }

        [TestMethod]
        public async Task CanUpdateMatchPlayerNullPlayer()
        {
            Seed(SeedOptions.WithPlayers);

            // only add player 1 to the line-up
            Player1.LineId = Line1.LineId;

            using (var context = new TennisContext(Options))
            {
                context.Players.Update(Player1);
                context.Players.Remove(Player2);  // remove player2 - simulate no player yet
                context.SaveChanges();
            }

            // save ids for later
            var matchId = Match1.MatchId;
            using (var context = new TennisContext(Options))
            {
                // verify that member 1 is in the match
                var checkLeagueMember1AddedToMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember1AddedToMatch);

                // verify member 2 is NOT in match
                var checkLeagueMember2NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId == Line1.LineId);
                Assert.IsNull(checkLeagueMember2NotInMatch);

                var mgr = new MatchManager(context, logger);
                await mgr.UpdateMatchPlayer("2", matchId, League1.LeagueId, Availability.Confirmed, LeagueMember1.MemberId);
            }

            // now check the availability is there for player 2, who should have taken player 1's place
            using (var context = new TennisContext(Options))
            {
                // now verify member 1 is NOT in the match, and member 2 is in the match
                var checkLeagueMember1NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId == null);
                Assert.IsNotNull(checkLeagueMember1NotInMatch);

                // verify member 2 is in match
                var checkLeagueMember2InMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember2InMatch);

                var player = context.Players.FirstOrDefault(p => p.MatchId == Match1.MatchId && p.MemberId == Member2.MemberId);
                Assert.IsNotNull(player);
                Assert.AreEqual(Availability.Confirmed, player.Availability);
            }
        }

        /// <summary>
        /// responder and referring member are the same, so cannot fill the sub spot
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CannotUpdateMatchPlayerSameResponder()
        {
            Seed(SeedOptions.WithPlayers);

            // only add player 1 to the line-up
            Player1.LineId = Line1.LineId;

            using (var context = new TennisContext(Options))
            {
                context.Players.Update(Player1);
                context.SaveChanges();
            }

            // save ids for later
            var matchId = Match1.MatchId;
            using (var context = new TennisContext(Options))
            {
                // verify that member 1 is in the match
                var checkLeagueMember1AddedToMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember1AddedToMatch);

                // verify member 2 is NOT in match
                var checkLeagueMember2NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId == null);
                Assert.IsNotNull(checkLeagueMember2NotInMatch);

                var mgr = new MatchManager(context, logger);
                try
                {
                    await mgr.UpdateMatchPlayer("1", matchId, League1.LeagueId, Availability.Confirmed, LeagueMember1.MemberId);
                    Assert.AreEqual(1, 2);  // we should not hit this line - if we do, something went wrong
                }
                catch(BadRequestException ex)
                {
                    Assert.AreEqual(typeof(BadRequestException), ex.GetType());
                }
            }
        }

        /// <summary>
        /// invalid match id sent to method
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CannotUpdateMatchPlayerInvalidMatch()
        {
            Seed(SeedOptions.WithPlayers);

            // only add player 1 to the line-up
            Player1.LineId = Line1.LineId;

            using (var context = new TennisContext(Options))
            {
                context.Players.Update(Player1);
                context.SaveChanges();
            }

            // save ids for later
            var matchId = Match1.MatchId;
            using (var context = new TennisContext(Options))
            {
                // verify that member 1 is in the match
                var checkLeagueMember1AddedToMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember1AddedToMatch);

                // verify member 2 is NOT in match
                var checkLeagueMember2NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId == null);
                Assert.IsNotNull(checkLeagueMember2NotInMatch);

                var mgr = new MatchManager(context, logger);
                try
                {
                    await mgr.UpdateMatchPlayer("2", 42, League1.LeagueId, Availability.Confirmed, LeagueMember1.MemberId);
                    Assert.AreEqual(1, 2);  // we should not hit this line - if we do, something went wrong
                }
                catch (NotFoundException ex)
                {
                    Assert.AreEqual(typeof(NotFoundException), ex.GetType());
                }
            }
        }

        /// <summary>
        /// player is responding after slot has already been taken
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CannotUpdateMatchPlayerSlotNotAvailable()
        {
            Seed(SeedOptions.WithPlayers);

            // only add player 1 to the line-up
            Player1.LineId = Line1.LineId;
            Player1.Availability = Availability.Confirmed;
            Player2.Availability = Availability.Confirmed;

            using (var context = new TennisContext(Options))
            {
                context.Players.Update(Player1);
                context.Players.Update(Player2);
                context.SaveChanges();
            }

            // save ids for later
            var matchId = Match1.MatchId;
            using (var context = new TennisContext(Options))
            {
                // verify that member 1 is in the match
                var checkLeagueMember1AddedToMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember1AddedToMatch);

                // verify member 2 is NOT in match
                var checkLeagueMember2NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId == null);
                Assert.IsNotNull(checkLeagueMember2NotInMatch);

                var mgr = new MatchManager(context, logger);
                try
                {
                    await mgr.UpdateMatchPlayer("2", matchId, League1.LeagueId, Availability.Confirmed, LeagueMember1.MemberId);
                    Assert.AreEqual(1, 2);  // we should not hit this line - if we do, something went wrong
                }
                catch (NotFoundException ex)
                {
                    Assert.AreEqual(typeof(NotFoundException), ex.GetType());
                }
            }
        }

        /// <summary>
        /// if responding player is already in line-up then they cannot take the place of another player
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CannotUpdateMatchPlayerAlreadyInLineup()
        {
            Seed(SeedOptions.WithPlayers);

            // add player 1 and player 2 to the line-up
            Player1.LineId = Line1.LineId;
            Player2.LineId = Line1.LineId;
            //Player1.Availability = Availability.Confirmed;
            Player2.Availability = Availability.Confirmed;

            using (var context = new TennisContext(Options))
            {
                context.Players.Update(Player1);
                context.Players.Update(Player2);
                context.SaveChanges();
            }

            // save ids for later
            var matchId = Match1.MatchId;
            using (var context = new TennisContext(Options))
            {
                // verify that member 1 is in the match
                var checkLeagueMember1AddedToMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember1AddedToMatch);

                // verify member 2 is in match
                var checkLeagueMember2NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember2NotInMatch);

                var mgr = new MatchManager(context, logger);
                try
                {
                    await mgr.UpdateMatchPlayer("2", matchId, League1.LeagueId, Availability.Confirmed, LeagueMember1.MemberId);
                    Assert.AreEqual(1, 2);  // we should not hit this line - if we do, something went wrong
                }
                catch (BadRequestException ex)
                {
                    Assert.AreEqual(typeof(BadRequestException), ex.GetType());
                }
            }
        }

        /// <summary>
        /// referring player is no longer in line-up - slot has already been taken
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CannotUpdateMatchPlayerReferrerNotInLineup()
        {
            Seed(SeedOptions.WithPlayers);

            // don't add either player to lineup

            // save ids for later
            var matchId = Match1.MatchId;
            using (var context = new TennisContext(Options))
            {
                var mgr = new MatchManager(context, logger);
                try
                {
                    await mgr.UpdateMatchPlayer("2", matchId, League1.LeagueId, Availability.Confirmed, LeagueMember1.MemberId);
                    Assert.AreEqual(1, 2);  // we should not hit this line - if we do, something went wrong
                }
                catch (NotFoundException ex)
                {
                    Assert.AreEqual(typeof(NotFoundException), ex.GetType());
                }
            }
        }
        /// <summary>
        /// test if we can substitute one player for another in a match
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanUpdateMatchPlayerResponderConfirmed()
        {         
            Seed(SeedOptions.WithPlayers);

            // only add player 1 to the line-up
            Player1.LineId = Line1.LineId;
            // start with player 2 being available
            Player2.Availability = Availability.Confirmed;

            using (var context = new TennisContext(Options))
            {
                context.Players.Update(Player1);
                context.Players.Update(Player2);
                context.SaveChanges();
            }

            // save ids for later
            var matchId = Match1.MatchId;
            using (var context = new TennisContext(Options))
            {
                // verify that member 1 is in the match
                var checkLeagueMember1AddedToMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember1AddedToMatch);

                // verify member 2 is NOT in match
                var checkLeagueMember2NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId == null);
                Assert.IsNotNull(checkLeagueMember2NotInMatch);

                var mgr = new MatchManager(context, logger);
                await mgr.UpdateMatchPlayer("2", matchId, League1.LeagueId, Availability.Confirmed, LeagueMember1.MemberId);
            }

            // now check the availability is there for player 2, who should have taken player 1's place
            using (var context = new TennisContext(Options))
            {
                // now verify member 1 is NOT in the match, and member 2 is in the match
                var checkLeagueMember1NotInMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member1.MemberId
                        && p.LineId == null);
                Assert.IsNotNull(checkLeagueMember1NotInMatch);

                // verify member 2 is in match
                var checkLeagueMember2InMatch = context.Players
                    .FirstOrDefault(p => p.MatchId == matchId
                        && p.MemberId == Member2.MemberId
                        && p.LineId >= 0);
                Assert.IsNotNull(checkLeagueMember2InMatch);

                var player = context.Players.FirstOrDefault(p => p.MatchId == Match1.MatchId && p.MemberId == Member2.MemberId);
                Assert.IsNotNull(player);
                Assert.AreEqual(Availability.Confirmed, player.Availability);
            }
        }

        [TestMethod]
        public async Task CanUpdateAvailability()
        {
            Seed(SeedOptions.WithPlayers);
            
            // set league to auto-add
            League1.AutoAddToLineup = true;

            // set to singles, so two players will trigger an auto-add
            Match1.Format = PlayFormat.MensSingles;

            using (var context = new TennisContext(Options))
            {
                context.Leagues.Update(League1);
                context.Matches.Update(Match1);
                context.SaveChanges();
            }

            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    MemberId = Member1.MemberId,
                    Value = Availability.Confirmed
                };
                _matchManager = new MatchManager(context, logger);
                var pResponse1 = await _matchManager.UpdateAvailability(req);
                // confirm first player was not added to line-up
                Assert.IsNull(pResponse1.LineId);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                // add second player, should add both to line-up because we're playing singles
                req.MemberId = Member2.MemberId;
                var pResponse2 = await _matchManager.UpdateAvailability(req);
                Assert.IsNotNull(pResponse2.LineId);
            }
        }

        /// <summary>
        /// test that system waits until all the regular players have responded, before adding players to line-up
        /// e.g. Reg1 ?, Reg2 Y, Sub1 Y -> No players will be added, waiting for Reg1 to respond
        /// then, Reg1 -> Y, add Reg1 and Reg2 to match
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanUpdateAvailabilityWaitForRegularConfirm()
        {
            Seed(SeedOptions.WithPlayers);

            var member3 = new Member
            {
                FirstName = "Third",
                ZipCode = "12345",
                UserId = "3",
                PhoneNumber = "18005551212"
            };

            var leagueMember3 = new LeagueMember
            {
                League = League1,
                Member = member3,
                IsSubstitute = false
            };

            // add third league member (regular)
            League1.LeagueMembers.Add(leagueMember3);

            // set league to auto-add
            League1.AutoAddToLineup = true;

            // set to singles, so two players will trigger an auto-add
            Match1.Format = PlayFormat.MensSingles;

            // set sub player to confirmed
            Player2.Availability = Availability.Confirmed;

            using (var context = new TennisContext(Options))
            {
                context.Members.Add(member3);
                //context.LeagueMembers.Add(leagueMember3);
                context.Leagues.Update(League1);
                context.Matches.Update(Match1);
                context.SaveChanges();
            }

            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    MemberId = Member1.MemberId,
                    Value = Availability.Confirmed
                };
                _matchManager = new MatchManager(context, logger);
                var pResponse1 = await _matchManager.UpdateAvailability(req);
                // confirm first player was not added to line-up
                Assert.IsNull(pResponse1.LineId);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                // add third player, should add both regulars (members 1,3) to line-up because we're playing singles
                req.MemberId = member3.MemberId;
                var pResponse2 = await _matchManager.UpdateAvailability(req);
                Assert.IsNotNull(pResponse2.LineId);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var p1 = context.Players.Where(p => p.Member.UserId == "1").FirstOrDefault();
                var p2 = context.Players.Where(p => p.Member.UserId == "2").FirstOrDefault();
                var p3 = context.Players.Where(p => p.Member.UserId == "3").FirstOrDefault();

                // confirm that players 1 and 3 are in line-up, player 2 (sub) is not
                Assert.IsNotNull(p1.LineId);
                Assert.IsNull(p2.LineId);
                Assert.IsNotNull(p3.LineId);
            }
        }

        /// <summary>
        /// test that system waits until all the regular players have responded, before adding players to line-up
        /// e.g. Reg1 ?, Reg2 Y, Sub1 Y -> No players will be added, waiting for Reg1 to respond
        /// then, Reg1 -> N, add Sub1 and Reg3 to match
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanUpdateAvailabilityWaitForRegularDecline()
        {
            Seed(SeedOptions.WithPlayers);

            var member3 = new Member
            {
                FirstName = "Third",
                ZipCode = "12345",
                UserId = "3",
                PhoneNumber = "18005551212"
            };

            var leagueMember3 = new LeagueMember
            {
                League = League1,
                Member = member3,
                IsSubstitute = false
            };

            // add third league member (regular)
            League1.LeagueMembers.Add(leagueMember3);

            // set league to auto-add
            League1.AutoAddToLineup = true;

            // set to singles, so two players will trigger an auto-add
            Match1.Format = PlayFormat.MensSingles;

            // set sub player to confirmed
            Player2.Availability = Availability.Confirmed;

            using (var context = new TennisContext(Options))
            {
                context.Members.Add(member3);
                //context.LeagueMembers.Add(leagueMember3);
                context.Leagues.Update(League1);
                context.Matches.Update(Match1);
                context.SaveChanges();
            }

            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    MemberId = Member1.MemberId,
                    Value = Availability.Confirmed
                };
                _matchManager = new MatchManager(context, logger);
                // set player 1 avail to Confirmed
                var pResponse1 = await _matchManager.UpdateAvailability(req);
                // confirm first player was not added to line-up, because we have 1 reg & 1 sub, but still waiting for other reg player to respond yes or no
                Assert.IsNull(pResponse1.LineId);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                // third player (regular) declines, should add regular (member 1) and sub (member 2) to line-up because we're playing singles
                req.MemberId = member3.MemberId;
                req.Value = Availability.Unavailable;
                var pResponse2 = await _matchManager.UpdateAvailability(req);
                Assert.IsNull(pResponse2.LineId);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var p1 = context.Players.Where(p => p.Member.UserId == "1").FirstOrDefault();
                var p2 = context.Players.Where(p => p.Member.UserId == "2").FirstOrDefault();
                var p3 = context.Players.Where(p => p.Member.UserId == "3").FirstOrDefault();

                // confirm that players 1 and 2 (sub) are in line-up, player 3 (reg) is not
                Assert.IsNotNull(p1.LineId);
                Assert.IsNotNull(p2.LineId);
                Assert.IsNull(p3.LineId);
            }
        }

        [ExpectedException(typeof(NotFoundException))]
        [TestMethod]
        public async Task CanUpdateAvailabilityInvalidLeague()
        {
            Seed();
            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = -1,
                    MatchId = Match1.MatchId,
                    MemberId = Member1.MemberId,
                    Value = Availability.Confirmed
                };
                _matchManager = new MatchManager(context, logger);
                _ = await _matchManager.UpdateAvailability(req);
                // expect exception here
            }
        }

        [ExpectedException(typeof(NotFoundException))]
        [TestMethod]
        public async Task CanUpdateAvailabilityInvalidMatch()
        {
            Seed();
            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = Match1.LeagueId,
                    MatchId = -1,
                    MemberId = Member1.MemberId,
                    Value = Availability.Confirmed
                };
                _matchManager = new MatchManager(context, logger);
                _ = await _matchManager.UpdateAvailability(req);
                // expect exception here
            }
        }

        [ExpectedException(typeof(NotFoundException))]
        [TestMethod]
        public async Task CanUpdateAvailabilityInvalidLeagueMember()
        {
            Seed();
            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = Match1.LeagueId,
                    MatchId = Match1.MatchId,
                    MemberId = -1,
                    Value = Availability.Confirmed
                };
                _matchManager = new MatchManager(context, logger);
                _ = await _matchManager.UpdateAvailability(req);
                // expect exception here
            }
        }

        /// <summary>
        /// update avail when players don't exist
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanUpdateAvailabilityNullPlayers()
        {
            Seed();

            // set league to auto-add
            League1.AutoAddToLineup = true;

            // set to singles, so two players will trigger an auto-add
            Match1.Format = PlayFormat.MensSingles;

            using (var context = new TennisContext(Options))
            {
                context.Leagues.Update(League1);
                context.Matches.Update(Match1);
                context.SaveChanges();
            }

            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    MemberId = Member1.MemberId,
                    Value = Availability.Confirmed
                };
                _matchManager = new MatchManager(context, logger);
                var pResponse1 = await _matchManager.UpdateAvailability(req);
                // confirm first player was not added to line-up
                Assert.IsNull(pResponse1.LineId);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                // add second player, should add both to line-up because we're playing singles
                req.MemberId = Member2.MemberId;
                var pResponse2 = await _matchManager.UpdateAvailability(req);
                Assert.IsNotNull(pResponse2.LineId);
            }
        }

        [TestMethod]
        public async Task CanUpdateAvailabilityNoLines()
        {
            Seed();

            // set league to auto-add
            League1.AutoAddToLineup = true;

            // set to singles, so two players will trigger an auto-add
            Match1.Format = PlayFormat.MensSingles;


            using (var context = new TennisContext(Options))
            {
                context.Leagues.Update(League1);
                context.Matches.Update(Match1);
                // remove line, to test case where there are no lines created yet
                context.Lines.Remove(Line1);
                context.SaveChanges();
            }

            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    MemberId = Member1.MemberId,
                    Value = Availability.Confirmed
                };
                _matchManager = new MatchManager(context, logger);
                var pResponse1 = await _matchManager.UpdateAvailability(req);
                // confirm first player was not added to line-up
                Assert.IsNull(pResponse1.LineId);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                // add second player, should add both to line-up because we're playing singles
                req.MemberId = Member2.MemberId;
                var pResponse2 = await _matchManager.UpdateAvailability(req);
                Assert.IsNotNull(pResponse2.LineId);
            }
        }

        [TestMethod]
        public async Task CanUpdateAvailabilityNullPlayersUndecidedRegular()
        {
            Seed();

            // set league to auto-add
            League1.AutoAddToLineup = true;

            // set to singles, so two players will trigger an auto-add
            Match1.Format = PlayFormat.MensSingles;

            using (var context = new TennisContext(Options))
            {
                context.Leagues.Update(League1);
                context.Matches.Update(Match1);
                context.SaveChanges();
            }

            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    MemberId = Member2.MemberId,
                    Value = Availability.Confirmed
                };
                _matchManager = new MatchManager(context, logger);
                var pResponse1 = await _matchManager.UpdateAvailability(req);
                // confirm first player was not added to line-up
                Assert.IsNull(pResponse1.LineId);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                // add second player, should add both to line-up because we're playing singles
                req.MemberId = Member1.MemberId;
                var pResponse2 = await _matchManager.UpdateAvailability(req);
                Assert.IsNotNull(pResponse2.LineId);
            }
        }

        [TestMethod]
        public async Task CanUpdateAvailabilityNotifySubs()
        {
            Seed(SeedOptions.WithPlayersInLineup);

            // set league to auto-add
            League1.AutoAddToLineup = true;

            // set to singles, so two players will trigger an auto-add
            Match1.Format = PlayFormat.MensSingles;

            // remove player2 from lineup
            Player2.LineId = null;

            using (var context = new TennisContext(Options))
            {
                context.Leagues.Update(League1);
                context.Matches.Update(Match1);
                context.Players.Update(Player2);
                context.SaveChanges();
            }

            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    MemberId = Member1.MemberId,
                    Value = Availability.Unavailable,
                    Action = MatchDeclineAction.InviteAll
                };
                _matchManager = new MatchManager(context, logger);
                var pResponse1 = await _matchManager.UpdateAvailability(req);
                // confirm first player is now unavailable
                Assert.AreEqual(Availability.Unavailable, pResponse1.Availability);

                // confirm we will be notifying player2 of the sub opportunity
                var notification = _matchManager.NotificationEvents
                    .Where(n => n.EventType == NotificationEventType.SubForMatchOpening)
                    .Where(n => n.MemberIds.Contains(Player2.MemberId))
                    .FirstOrDefault();
                Assert.IsNotNull(notification);            
            }
        }

        [TestMethod]
        public void CanSeedPlayersInLineup()
        {
            Seed(SeedOptions.WithPlayersInLineup);
            var playersInLineup = Match1.Players.Where(p => p.LineId == Line1.LineId).Count();
            Assert.AreEqual(2, playersInLineup);
        }

        /// <summary>
        /// when adding player to a new line with no id yet, player.lineId is 0 or null,
        /// which may cause issue on back end insert/update methods
        /// </summary>
        [TestMethod]
        public async Task CanAddPlayersToNewLine()
        {
            // get basic seed data, with one line and two players, none in lineup
            Seed(SeedOptions.WithPlayers);

            var line2 = new Line
            {
                CourtNumber = "2",
                Guid = "abc"
            };

            // associate player with new line
            Player1.LineId = line2.LineId;
            Player1.Guid = "abc";   // this guid links the player to the line, since the line does not yet have an id

            // add line to the match
            Match1.Lines.Add(line2);

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                Match1 = await _matchManager.UpdateMatchAsync(Match1);
            }

            using (var context = new TennisContext(Options))
            {
                var match2 = context.Matches
                    .Include(m => m.Lines)
                    .First();

                // verify we have added a second line to the match
                Assert.AreEqual(2, match2.Lines.Count);

                // verify the second line has player1 as a player
                var line2Check = match2.Lines.Where(l => l.CourtNumber == "2").FirstOrDefault();
                Assert.IsNotNull(line2Check);
                var line2Players = context.Players.Where(p => p.LineId == line2Check.LineId).ToList();
                Assert.AreEqual(1, line2Players.Count);
                Assert.AreEqual(Player1.Id, line2Players.First().Id);

                // verify first line has no players
                var line1Check = match2.Lines.Where(l => l.CourtNumber == "1").FirstOrDefault();
                Assert.IsNotNull(line1Check);
                var line1Players = context.Players.Where(p => p.LineId == line1Check.LineId);
                Assert.AreEqual(0, line1Players.Count());
            }
        }

        /// <summary>
        /// test if we can move an existing player from an existing line to a new line
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanMovePlayersToNewLine()
        {
            // get basic seed data, with one line and two players, both in lineup
            Seed(SeedOptions.WithPlayersInLineup);

            var line2 = new Line
            { 
                CourtNumber = "2",
                Guid = "abc"
            };

            // associate player1 with a new line
            Player1.LineId = line2.LineId;
            Player1.Guid = "abc";   // this guid links the player to the line, since the line does not yet have an id

            // add line to the match
            Match1.Lines.Add(line2);

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                Match1 = await _matchManager.UpdateMatchAsync(Match1);
            }
            using (var context = new TennisContext(Options))
            {
                var match = context.Matches
                    .Include(m => m.Lines)
                    .Include(m => m.Players)
                    .First();

                // verify we have added a second line to the match
                Assert.AreEqual(2, match.Lines.Count);

                // verify the second line has player1 as a player
                var line2Check = match.Lines.Where(l => l.CourtNumber == "2").FirstOrDefault();
                Assert.IsNotNull(line2Check);
                var line2Player = match.Players.Where(p => p.LineId == line2.LineId).First();
                Assert.AreEqual(Player1.Id, line2Player.Id);

                // verify first line has only player2 as a player
                var line1Check = match.Lines.Where(l => l.CourtNumber == "1").FirstOrDefault();
                Assert.IsNotNull(line1Check);
                var line1Players = match.Players.Where(p => p.LineId == Line1.LineId);
                Assert.AreEqual(1, line1Players.Count());
                var line1Player = line1Players.First();
                Assert.AreEqual(Player2.Id, line1Player.Id);

                // verify player1 will NOT be sent a 'removed from match' notification
                // because he was removed from one line BUT, he was also added to another line
                var notification = _matchManager.NotificationEvents
                    .Where(n => n.EventType == NotificationEventType.RemovedFromMatch)
                    .Where(n => n.MemberIds.Contains(Player1.MemberId))
                    .FirstOrDefault();
                Assert.IsNull(notification);
            }
        }

        /// <summary>
        /// test if we can move an existing player from an existing line to a new line
        /// and delete the first line in the same transaction
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanMovePlayersToNewLineWithOldLineDelete()
        {
            // get basic seed data, with one line and two players, both in lineup
            Seed(SeedOptions.WithPlayersInLineup);

            var line2 = new Line
            {
                CourtNumber = "2",
                Guid = "abc"
            };

            // associate players with a new line
            Player1.LineId = line2.LineId;
            Player1.Guid = line2.Guid;      // this guid links the player to the line, since the line does not yet have an id
            Player2.LineId = line2.LineId;
            Player2.Guid = line2.Guid;

            // add new line to the match
            Match1.Lines.Add(line2);

            // remove old line from match
            Match1.Lines.Remove(Line1);
            Line1.Match = null;
            Line1.MatchId = 0;

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                Match1 = await _matchManager.UpdateMatchAsync(Match1);
            }
            using (var context = new TennisContext(Options))
            {
                Match1 = context.Matches
                    .Include(m => m.Lines)
                    .First();

                // verify we have added a second line to the match
                Assert.AreEqual(1, Match1.Lines.Count);
                // verify this is the line we added
                Assert.AreEqual("2", Match1.Lines.First().CourtNumber);

                // verify the second line has player1 as a player
                var line2Check = Match1.Lines.Where(l => l.CourtNumber == "2").FirstOrDefault();
                Assert.IsNotNull(line2Check);
                Assert.AreEqual(line2Check.LineId, Player1.LineId);

                // verify first line has been removed
                var line1Check = Match1.Lines.Where(l => l.CourtNumber == "1").FirstOrDefault();
                Assert.IsNull(line1Check);
            }
        }

        [TestMethod]
        public async Task CanUpdateNewMatch()
        {
            League1 = new League
            {
                Name = "My Test League"
            };

            Venue1 = new Venue
            {
                Name = "My test venue"
            };

            using (var context = new TennisContext(Options))
            {
                context.Venues.Add(Venue1);
                context.Leagues.Add(League1);
                await context.SaveChangesAsync();
            }

            Match1 = new Match
            {
                MatchId = 0,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow + TimeSpan.FromHours(1.5),
                League = League1,
                MatchVenue = Venue1
            };

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                await _matchManager.UpdateMatchAsync(Match1);
            }
            using (var context = new TennisContext(Options))
            {
                Match1 = context.Matches.First();
                Assert.IsNotNull(Match1.MatchId);
                Assert.AreNotEqual(0, Match1.MatchId);
            }
         }

        [TestMethod]
        public async Task CanUpdateNewMatchWithLine()
        {
            League1 = new League
            {
                Name = "My Test League"
            };

            Venue1 = new Venue
            {
                Name = "My test venue"
            };

            var line = new Line
            {
                CourtNumber = "1"
            };

            Member1 = new Member
            {
                FirstName = "Randy",
                ZipCode = "12345",
                UserId = "1",
                PhoneNumber = "18005551212"
            };

            LeagueMember1 = new LeagueMember
            {
                League = League1,
                Member = Member1,
                IsSubstitute = false
            };

            using (var context = new TennisContext(Options))
            {
                context.Venues.Add(Venue1);
                context.Leagues.Add(League1);
                context.Members.Add(Member1);
                context.LeagueMembers.Add(LeagueMember1);
                await context.SaveChangesAsync();
            }

            Match1 = new Match
            {
                MatchId = 0,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow + TimeSpan.FromHours(1.5),
                League = League1,
                MatchVenue = Venue1
            };

            Match1.Lines.Add(line);

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                await _matchManager.UpdateMatchAsync(Match1);

                var notification = _matchManager.NotificationEvents
                    .Where(n => n.EventType == NotificationEventType.MatchAdded)
                    .FirstOrDefault();
                Assert.IsNotNull(notification);
            }

            using (var context = new TennisContext(Options))
            {
                Match1 = context.Matches.First();
                Assert.IsNotNull(Match1.MatchId);
                Assert.AreNotEqual(0, Match1.MatchId);
            }
        }

        [TestMethod]
        public async Task CanCancelMatch()
        {
            Seed(SeedOptions.WithPlayersInLineup);
            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var match = await _matchManager.DeleteMatch(Match1.MatchId);

                // ensure we have requested a notification of this event, to all the players in the line-up
                var notification = _matchManager.NotificationEvents
                    .Where(n => n.EventType == NotificationEventType.MatchCancelled)
                    .Where(n => n.LeagueMemberIds.Contains(Player1.LeagueMemberId))
                    .Where(n => n.LeagueMemberIds.Contains(Player2.LeagueMemberId))
                    .FirstOrDefault();
                Assert.IsNotNull(notification);
            }

            using (var context = new TennisContext(Options))
            {
                // ensure match was deleted
                _matchManager = new MatchManager(context, logger);
                var match = context.Matches.FirstOrDefault();
                Assert.IsNull(match);

            }
        }

        [TestMethod]
        public async Task CanGetAvailabilityGrid()
        {
            Seed(SeedOptions.WithPlayersInLineup);
            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var result = await _matchManager.GetLeagueAvailabilityGrid(League1.LeagueId);
            }
        }

        [TestMethod]
        public async Task CanGetAvailabilityGridNoLines()
        {
            Seed();
            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var result = await _matchManager.GetLeagueAvailabilityGrid(League1.LeagueId);
            }
        }

        /// <summary>
        /// this is to address a bug whereby after adding people to a match, deleting one, then setting that one's avail to unknown would
        /// then auto-add them back to the match.  Setting someone's avail to Unknown should never auto-add them to a match
        /// Related: Also, setting them to Avail added them to the match, which it shouldn't unless all other regulars have responded
        /// I believe both of these issues only occur when the user already has been in the line-up (a Player object has been created for them).
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanUpdateAvailabilityWithoutAddingToMatch()
        {
            Seed(SeedOptions.WithPlayersInLineup);

            // set league to auto-add
            League1.AutoAddToLineup = true;

            // set to singles, so two players will trigger an auto-add
            Match1.Format = PlayFormat.MensSingles;

            using (var context = new TennisContext(Options))
            {
                context.Leagues.Update(League1);
                context.Matches.Update(Match1);
                context.SaveChanges();
            }

            using (var context = new TennisContext(Options))
            {
                // remove player1 from line-up, set to available
                _matchManager = new MatchManager(context, logger);
                Player1.LineId = null;
                Player1.Availability = Availability.Confirmed;
                context.Players.Update(Player1);
                context.SaveChanges();
                Assert.IsNull(Player1.LineId);
            }

            UpdateAvailabilityRequest req;
            using (var context = new TennisContext(Options))
            {
                req = new UpdateAvailabilityRequest
                {
                    LeagueId = League1.LeagueId,
                    MatchId = Match1.MatchId,
                    MemberId = Member1.MemberId,
                    Value = Availability.Unknown,
                    Action = MatchDeclineAction.DoNothing
                };
                _matchManager = new MatchManager(context, logger);
                var pResponse1 = await _matchManager.UpdateAvailability(req);
            }

            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var p1 = context.Players.Where(p => p.Member.UserId == "1").FirstOrDefault();
                var p2 = context.Players.Where(p => p.Member.UserId == "2").FirstOrDefault();

                // confirm that player 2 is in line-up, player 1 is not
                Assert.IsNull(p1.LineId);
                Assert.IsNotNull(p2.LineId);
            }
        }

        [TestMethod]
        public async Task CanGetProspectiveMatches1()
        {
            Seed(SeedOptions.WithPlayers);
            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var p = await _matchManager.GetProspectiveMatches(Member1.MemberId);
                // we expect one match, because player is not in the line-up
                Assert.AreEqual(1, p.Matches.Count);
            }
        }

        [TestMethod]
        public async Task CanGetProspectiveMatches2()
        {
            Seed(SeedOptions.WithPlayersInLineup);
            using (var context = new TennisContext(Options))
            {
                _matchManager = new MatchManager(context, logger);
                var p = await _matchManager.GetProspectiveMatches(Member1.MemberId);
                // we expect no matches, because player is in the line-up
                Assert.AreEqual(0, p.Matches.Count);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_matchManager != null) _matchManager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MatchManagerTests()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
