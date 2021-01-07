using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Data.Test.Seeding;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Models.Enums;
using MyTennisPartner.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyTennisPartner.Data.Test
{
    [TestClass]
    public class LeagueManagerTests: ManagerTestBase, IDisposable
    {
        private readonly ILogger<LeagueManager> logger;
        private LeagueManager _leagueManager;

#pragma warning disable CS0162
        public LeagueManagerTests()
        {
            logger = new Moq.Mock<ILogger<LeagueManager>>().Object;
        }

        [TestMethod]
        public async Task CanUpdateLeague()
        {
            League league;
            int leagueId;
            using (var context = new TennisContext(Options))
            {
                league = new League
                {
                    Name = "Test League Original"
                };
                context.Leagues.Add(league);
                context.SaveChanges();
                leagueId = league.LeagueId;
            }

            LeagueSummaryViewModel league2 = new LeagueSummaryViewModel
            {
                LeagueId = leagueId,
                Name = "Test League Revised"
            };
            League league3;
            using (var context = new TennisContext(Options))
            {
                _leagueManager = new LeagueManager(context, logger);
                league3 = await _leagueManager.UpdateLeagueAsync(league2);
            }

            Assert.AreEqual("Test League Revised", league3.Name);
        }

        [TestMethod]
        public async Task CanUpdateLeagueWithOwnerAndVenue()
        {
            League league;
            int leagueId;
            Member member;
            Venue venue;
            Address address;
            Contact contact;
            using (var context = new TennisContext(Options))
            {
                member = new Member
                {
                    FirstName = "John",
                    ZipCode = "12345",
                    PhoneNumber = "18005551212"
                };
                address = new Address
                {
                    City = "Gold River"
                };
                contact = new Contact
                {
                    FirstName = "Randy"
                };
                venue = new Venue
                {
                    Name = "My Venue",
                    VenueAddress = address,
                    VenueContact = contact
                };
                league = new League
                {
                    Name = "Test League Original",
                    Owner = member,
                    HomeVenue = venue
                };
                context.Leagues.Add(league);
                context.SaveChanges();
                leagueId = league.LeagueId;
            }

            LeagueSummaryViewModel league2 = new LeagueSummaryViewModel
            {
                LeagueId = leagueId,
                Name = "Test League Revised",
                Owner = ModelMapper.Map<MemberNameViewModel>(member),
                HomeVenue = ModelMapper.Map<VenueViewModel>(venue)
            };
            League league3;
            using (var context = new TennisContext(Options))
            {
                _leagueManager = new LeagueManager(context, logger);
                league3 = await _leagueManager.UpdateLeagueAsync(league2);
            }

            Assert.AreEqual("Test League Revised", league3.Name);
        }

        [ExpectedException(typeof(DbUpdateConcurrencyException))]
        [TestMethod]
        public void CannotCascadeDeletePlayer()
        {
            // check if cascade delete is working with Player entity
            var member = new Member
            {
                FirstName = "Randy",
                ZipCode = "12345",
                PhoneNumber = "18005551212"
            };

            var league = new League
            {
                Name = "My Test League"
            };

            var venue = new Venue
            {
                Name = "My test venue"
            };

            var match = new Match
            {
                StartTime = DateTime.Now,
                League = league,
                MatchVenue = venue
            };

            var leagueMember = new LeagueMember
            {
                League = league,
                Member = member
            };

            using (var context = new TennisContext(Options))
            {
                context.Members.Add(member);
                context.Leagues.Add(league);
                context.Matches.Add(match);
                context.LeagueMembers.Add(leagueMember);
                context.SaveChanges();
                var player = new Player
                {
                    LeagueId = league.LeagueId,
                    MatchId = match.MatchId,
                    LeagueMemberId = leagueMember.LeagueMemberId,
                    MemberId = member.MemberId,
                    Availability = Availability.Unknown,
                    IsSubstitute = false,
                    IsHomePlayer = false,
                    ModifiedDate = DateTime.Today
                };
                context.Players.Add(player);
            }
            Assert.AreNotEqual(0, member.MemberId);

            using (var context = new TennisContext(Options))
            {
                context.Members.Remove(member);
                context.SaveChanges();
            }
            // the above SaveChanges() should cause an UpdateConcurrency exception, as the Player entity has foreign keys and cannot be cascade-deleted
        }

        [TestMethod]
        public void CanInsertNewPlayer()
        {
            // check if we can insert a new Player record
            var member = new Member
            {
                FirstName = "Randy",
                ZipCode = "12345",
                PhoneNumber = "18005551212"
            };

            var league = new League
            {
                Name = "My Test League"
            };

            var venue = new Venue
            {
                Name = "My test venue"
            };

            var match = new Match
            {
                StartTime = DateTime.Now,
                League = league,
                MatchVenue = venue
            };

            var leagueMember = new LeagueMember
            {
                League = league,
                Member = member
            };

            using (var context = new TennisContext(Options))
            {
                context.Members.Add(member);
                context.Leagues.Add(league);
                context.Matches.Add(match);
                context.LeagueMembers.Add(leagueMember);
                context.SaveChanges();
            }
            Assert.AreNotEqual(0, member.MemberId);

            using (var context = new TennisContext(Options))
            {
                var player = new Player
                {
                    LeagueMemberId = leagueMember.LeagueMemberId,
                    MatchId = match.MatchId,
                    LeagueId = league.LeagueId,
                    MemberId = member.MemberId
                };
                context.Players.Add(player);
                context.SaveChanges();
                Assert.AreEqual(match.MatchId, player.MatchId);
                Assert.AreEqual(leagueMember.LeagueMemberId, player.LeagueMemberId);
            }
        }

        [TestMethod]
        public void CanUpdatePlayer()
        {
            // check if we can insert a new player record
            var member = new Member
            {
                FirstName = "Randy",
                ZipCode = "12345",
                PhoneNumber = "18005551212"
            };

            var league = new League
            {
                Name = "My Test League"
            };

            var venue = new Venue
            {
                Name = "My test venue"
            };

            var match = new Match
            {
                StartTime = DateTime.Now,
                League = league,
                MatchVenue = venue
            };

            var leagueMember = new LeagueMember
            {
                League = league,
                Member = member
            };

            var player = new Player
            {
                LeagueMember = leagueMember,
                Match = match,
                Member = member,
                League = league,
                Availability = Availability.Confirmed
            };

            using (var context = new TennisContext(Options))
            {
                context.Members.Add(member);
                context.Leagues.Add(league);
                context.Matches.Add(match);
                context.LeagueMembers.Add(leagueMember);
                context.SaveChanges();
            }
            Assert.AreNotEqual(0, member.MemberId);

            using (var context = new TennisContext(Options))
            {
                match.Players.Add(player);
                context.Matches.Update(match);
                context.Players.Add(player);
                context.SaveChanges();
                Assert.AreEqual(match.MatchId, player.MatchId);
                Assert.AreEqual(leagueMember.LeagueMemberId, player.LeagueMemberId);
            }

            // now try to update the availability
            using (var context = new TennisContext(Options))
            {
                player.Availability = Availability.Unavailable;
                context.Players.Update(player);
                context.SaveChanges();
            }
            using (var context = new TennisContext(Options))
            {
                var updatedPlayer = context.Players.Find(player.Id);
                Assert.AreEqual(Availability.Unavailable, updatedPlayer.Availability);
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
                    // TODO: dispose managed state (managed objects)
                    if (_leagueManager != null) _leagueManager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LeagueManagerTests()
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
