using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Data.Test.Seeding;
using MyTennisPartner.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MyTennisPartner.Data.Test
{
    /// <summary>
    /// base class for manager tests
    /// </summary>
    [TestClass]
    public class ManagerTestBase
    {
        const string dbConnectionString = "Server=(localdb)\\mssqllocaldb;Database=tennis-test;Trusted_Connection=True;MultipleActiveResultSets=true";
        protected DbContextOptions<TennisContext> Options { get; }
        const bool useMemoryDb = false;
        protected League League1 { get; set; }
        protected Venue Venue1 { get; set; }
        protected Match Match1 { get; set; }
        protected Member Member1 { get; set; }
        protected Member Member2 { get; set; }
        protected LeagueMember LeagueMember1 { get; set; }
        protected LeagueMember LeagueMember2 { get; set; }
        protected Line Line1 { get; set; }
        protected Player Player1 { get; set; }
        protected Player Player2 { get; set; }

        public ManagerTestBase()
        {
            var connectionMemory = new SqliteConnection("DataSource=:memory:");
            connectionMemory.Open();

            var _optionsMemory = new DbContextOptionsBuilder<TennisContext>()
                .UseSqlite(connectionMemory)
                .ConfigureWarnings(warnings =>
                      warnings.Default(WarningBehavior.Ignore)
                        .Throw(RelationalEventId.QueryClientEvaluationWarning))
                .EnableDetailedErrors()
                .Options;

            var _optionsSql = new DbContextOptionsBuilder<TennisContext>()
                .UseSqlServer(dbConnectionString)
                .ConfigureWarnings(warnings =>
                      warnings.Default(WarningBehavior.Ignore)
                        // throw warnings as errors, for case where EF has to evaluate locally, rather in db
                        .Throw(RelationalEventId.QueryClientEvaluationWarning))
                .EnableDetailedErrors()
                // .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                // .EnableSensitiveDataLogging(true)
                .Options;

            Options = useMemoryDb ? _optionsMemory : _optionsSql;

#pragma warning disable CS0162 // Unreachable code detected
            using (var context = new TennisContext(Options))
            {
                if (useMemoryDb)
                {
                    context.Database.EnsureCreated();
                }
                else
                {
                    context.Database.Migrate();
                }
                DbInitializer.DeleteAllData(context);
            }
#pragma warning restore CS0162 // Unreachable code detected
        }

        /// <summary>
        /// method to run before every test
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            using (var context = new TennisContext(Options))
            {
                // clear db before every test
                DbInitializer.DeleteAllData(context);
            }
        }

        /// <summary>
        /// options to customize seeding of test data
        /// </summary>
        [Flags]
        public enum SeedOptions
        {
            Default = 0,
            WithPlayers = 1,
            WithPlayersInLineup = 2
        }

        protected void Seed(SeedOptions options = SeedOptions.Default)
        {
            Member1 = new Member
            {
                FirstName = "Randy",
                LastName = "Gamage",
                ZipCode = "12345",
                UserId = "1",
                PhoneNumber = "18005551212",
                SpareTimeMemberNumber = 564705,
                SpareTimeUsername = "rgamage",
                SpareTimePassword = "august07"
            };

            Member2 = new Member
            {
                FirstName = "Gary",
                ZipCode = "12345",
                UserId = "2",
                PhoneNumber = "18005551212"
            };

            League1 = new League
            {
                Name = "My Test League"
            };

            Venue1 = new Venue
            {
                Name = "My test venue"
            };

            var matchTime = DateTime.UtcNow + TimeSpan.FromDays(1);
            Match1 = new Match
            {
                StartTime = new DateTime(matchTime.Year, matchTime.Month, matchTime.Day, matchTime.Hour, 0, 0),
                EndTime   = new DateTime(matchTime.Year, matchTime.Month, matchTime.Day, matchTime.Hour, 0, 0) + TimeSpan.FromHours(1.5),
                League = League1,
                MatchVenue = Venue1
            };

            LeagueMember1 = new LeagueMember
            {
                League = League1,
                Member = Member1,
                IsSubstitute = false
            };

            LeagueMember2 = new LeagueMember
            {
                League = League1,
                Member = Member2,
                IsSubstitute = true
            };

            Line1 = new Line
            {
                CourtNumber = "1"
            };

            if (options.HasFlag(SeedOptions.WithPlayers) || options.HasFlag(SeedOptions.WithPlayersInLineup))
            {
                Player1 = new Player
                {
                    League = League1,
                    Match = Match1,
                    LeagueMember = LeagueMember1,
                    Member = Member1,
                    Availability = Availability.Unknown
                };

                Player2 = new Player
                {
                    League = League1,
                    Match = Match1,
                    LeagueMember = LeagueMember2,
                    Member = Member2,
                    Availability = Availability.Unknown,
                    IsSubstitute = true
                };
            }
            Match1.Lines.Add(Line1);

            using (var context = new TennisContext(Options))
            {
                context.Members.Add(Member1);
                context.Members.Add(Member2);
                context.Leagues.Add(League1);
                context.Matches.Add(Match1);
                context.LeagueMembers.Add(LeagueMember1);
                context.LeagueMembers.Add(LeagueMember2);            
                if (options.HasFlag(SeedOptions.WithPlayers))
                {
                    context.Players.Add(Player1);
                    context.Players.Add(Player2);
                }
                context.SaveChanges();
                if (options.HasFlag(SeedOptions.WithPlayersInLineup))
                {
                    Player1.LineId = Line1.LineId;
                    Player2.LineId = Line1.LineId;
                    context.Players.Update(Player1);
                    context.Players.Update(Player2);
                    context.SaveChanges();
                }
            }
        }
    }
}
