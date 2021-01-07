using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Data.Test
{
    [TestClass]
    public class MemberManagerTests: ManagerTestBase, IDisposable
    {
        private MemberManager _memberManager;
        private readonly ILogger<MemberManager> logger;

        public MemberManagerTests()
        { 
            logger = new Moq.Mock<ILogger<MemberManager>>().Object;
        }

        [TestMethod]
         public async Task CanGetSubPickList()
        {
            // test with no players in line-up
            Seed(SeedOptions.WithPlayers);
            using (var context = new TennisContext(Options))
            {
                // confirm we get both members returned, since they are not in the line-up
                _memberManager = new MemberManager(context, logger);
                var results = await _memberManager.GetSubPickList(Match1.MatchId);
                Assert.AreEqual(2, results.Count());

                // now check sort order
                Player1.Availability = Availability.Unknown;
                Player2.Availability = Availability.Confirmed;
                context.Players.Update(Player1); 
                context.Players.Update(Player2);
                context.SaveChanges();
                results = await _memberManager.GetSubPickList(Match1.MatchId);
                // confirm the player that is confirmed available is at the top of the list
                Assert.AreEqual(Player2.MemberId, results.First().MemberId);

                // switch availabilities, check sort order again
                Player1.Availability = Availability.Confirmed;
                Player2.Availability = Availability.Unknown;
                context.Players.Update(Player1);
                context.Players.Update(Player2);
                context.SaveChanges();
                results = await _memberManager.GetSubPickList(Match1.MatchId);
                // confirm the player that is confirmed available is at the top of the list
                Assert.AreEqual(Player1.MemberId, results.First().MemberId);
            }

            // now test with all players in the line-up - we should get no results
            Seed(SeedOptions.WithPlayersInLineup);
            using (var context = new TennisContext(Options))
            {
                _memberManager = new MemberManager(context, logger);
                var results = await _memberManager.GetSubPickList(Match1.MatchId);
                Assert.AreEqual(0, results.Count());
            }
        }

        /// <summary>
        /// ensure exception is thrown when passing invalid matchId to GetSubPickList
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.Exception))]
        public async Task CannotGetSubPickListWithInvalidMatch()
        {
            Seed(SeedOptions.WithPlayersInLineup);
            using (var context = new TennisContext(Options))
            {
                _memberManager = new MemberManager(context, logger);
                // test with invalid match Id
                var invalidMatchNumber = 1234567;
                var errorResult = await _memberManager.GetSubPickList(invalidMatchNumber);
                // we expect an exception to be thrown as a result of the above method call
                // Assert - expects exception (see annotation at top of this test method)
            }
        }

        /// <summary>
        /// create a new member
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanCreateNewMember()
        {
            Seed();
            using (var context = new TennisContext(Options))
            {
                _memberManager = new MemberManager(context, logger);
                var member3 = new Member
                {
                    FirstName = "Member3First",
                    //LastName = "Member3Last",
                    ZipCode = "12345",
                    UserId = "2",
                    PhoneNumber = "18005551212"
                };
                await _memberManager.CreateMemberAsync(member3);
                Assert.IsTrue(member3.MemberId > 0);
            }
        }

        /// <summary>
        /// create a new member with an existing home venue
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanCreateNewMemberWithVenue()
        {
            Seed();
            using (var context = new TennisContext(Options))
            {
                _memberManager = new MemberManager(context, logger);
                var member3 = new Member
                {
                    FirstName = "Member3First",
                    LastName = "Member3Last",
                    ZipCode = "12345",
                    UserId = "2",
                    PhoneNumber = "18005551212", 
                    HomeVenueVenueId = Venue1.VenueId,
                    HomeVenue = Venue1
                };
                await _memberManager.CreateMemberAsync(member3);
                Assert.IsTrue(member3.MemberId > 0);
            }
        }

        /// <summary>
        /// test to see if we can get members with images
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanGetMembersWithImages()
        {
            Seed();
            using (var context = new TennisContext(Options))
            {
                var mgr = new MemberManager(context, logger);
                var members = await mgr.GetMembersWithImagesAsync();
                Assert.AreNotEqual(0, members.Count());
            }
        }

        [TestMethod]
        public void CanMapModelInSql()
        {
            var member = new Member
            {
                FirstName = "Randy",
                LastName = "Gamage",
                ZipCode = "95959",
                PhoneNumber = "555-1212",
                Gender = Gender.Male
            };

            using (var context = new TennisContext(Options))
            {
                context.Members.Add(member);
                context.SaveChanges();
            }

            using (var context = new TennisContext(Options))
            {
                var mgr = new MemberManager(context, logger);
                var members = mgr.GetMembersTest();
                Assert.AreNotEqual(0, members.Count);
            }
        }

        [TestMethod]
        public async Task CanGetOrderedSubList()
        {
            Seed(SeedOptions.WithPlayers);
            var matchTime = DateTime.UtcNow + TimeSpan.FromDays(8);  // change this date to test different point values
            var Match2 = new Match
            {
                StartTime = new DateTime(matchTime.Year, matchTime.Month, matchTime.Day, matchTime.Hour, 0, 0),
                EndTime = new DateTime(matchTime.Year, matchTime.Month, matchTime.Day, matchTime.Hour, 0, 0) + TimeSpan.FromHours(1.5),
                MatchVenueVenueId = Venue1.VenueId,
                LeagueId = League1.LeagueId
            };

            var Line2 = new Line
            {
                CourtNumber = "1"
            };

            var Player1future1 = new Player
            {
                LeagueId = League1.LeagueId,
                LeagueMemberId = LeagueMember1.LeagueMemberId,
                MemberId = Member1.MemberId,
                Availability = Availability.Unknown
            };

            using (var context = new TennisContext(Options))
            {
                context.SaveChanges();
                context.Matches.Add(Match2);
                context.SaveChanges();
                Line2.MatchId = Match2.MatchId;
                context.Lines.Add(Line2);
                Player1future1.LineId = Line2.LineId;
                Player1future1.MatchId = Match2.MatchId;
                context.Players.Add(Player1future1);
                context.SaveChanges();

                var mgr = new MemberManager(context, logger);
                var subList = await mgr.GetOrderedSubList(Match1.MatchId);
                Assert.IsTrue(subList.Any());
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
                    if (_memberManager != null) _memberManager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MemberManagerTests()
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
