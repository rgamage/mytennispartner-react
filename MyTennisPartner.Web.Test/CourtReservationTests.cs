using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using MyTennisPartner.Models.CourtReservations.TennisBookings;
using System.Collections.Generic;
using MyTennisPartner.Models.CourtReservations;
using MyTennisPartner.Web.Services.Reservations;
using MyTennisPartner.Web.Managers;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Test;
using System.Threading.Tasks;
using MyTennisPartner.Models.Enums;
using MyTennisPartner.Data.Models;
using System.Text.RegularExpressions;
using System.Linq;
using MyTennisPartner.Utilities;

namespace MyTennisPartner.Web.Test
{
    [TestClass]
    public class CourtReservationTests : ManagerTestBase
    {
        private readonly ILogger<CourtReservationServiceSpareTime> logger1;
        private readonly ILogger<MemberManager> logger2;
        private readonly ILogger<LineManager> logger3;
        private readonly ILogger<ReservationManager> logger4;

        const string username = "rgamage";
        const string password = "august07";
        //const string username = "genenyep";
        //const string password = "Hawaii12";


        public CourtReservationTests()
        {
            logger1 = new Mock<ILogger<CourtReservationServiceSpareTime>>().Object;
            logger3 = new Mock<ILogger<LineManager>>().Object;
            logger2 = new Mock<ILogger<MemberManager>>().Object;
            logger4 = new Mock<ILogger<ReservationManager>>().Object;

            //var connectionMemory = new SqliteConnection("DataSource=:memory:");
            //connectionMemory.Open();

            //var _optionsMemory = new DbContextOptionsBuilder<TennisContext>()
            //    .UseSqlite(connectionMemory)
            //    .ConfigureWarnings(warnings =>
            //          warnings.Default(WarningBehavior.Ignore)
            //            .Throw(RelationalEventId.QueryClientEvaluationWarning))
            //    .EnableDetailedErrors()
            //    .Options;

            //options = _optionsMemory;
            //using (var context = new TennisContext(options))
            //{
            //    context.Database.EnsureCreated();
            //}
        }

        /// <summary>
        /// see if we can fetch the court reservation grid
        /// </summary>
        [TestMethod]
        public void CanGetCourtAvailabilityGoldRiver()
        {
            var client = new CourtReservationClient();
            var service = new CourtReservationServiceSpareTime(client, logger1);
            service.SetHost("goldriver.tennisbookings.com");
            var reservations = service.GetCourtAvailability(username, password, DateTime.UtcNow + TimeSpan.FromHours(24));
            Assert.IsNotNull(reservations);
            // <assert no exceptions thrown from above method call>
        }

        /// <summary>
        /// see if we can fetch the court reservation grid
        /// </summary>
        //[TestMethod]
        public void CanGetCourtAvailabilityBroadstone()
        {
            var client = new CourtReservationClient();
            var service = new CourtReservationServiceSpareTime(client, logger1);
            service.SetHost("broadstone.tennisbookings.com");
            var reservations = service.GetCourtAvailability(username, password, DateTime.UtcNow + TimeSpan.FromHours(24));
            Assert.IsNotNull(reservations);
            // <assert no exceptions thrown from above method call>
        }

        /// <summary>
        /// test login
        /// </summary>
        [TestMethod]
        public void CanTestLogin()
        {
            var client = new CourtReservationClient();
            var service = new CourtReservationServiceSpareTime(client, logger1);
            service.SetHost("goldriver.tennisbookings.com");
            var memberNumber = service.LoginTest(username, password);
            logger1.LogInformation("unit test logger experiment");
            Assert.IsTrue(memberNumber > 0);
        }

        /// <summary>
        /// test to see if we can reserve a court
        /// NOTE: leave this test commented out, as we do NOT want to repeatedly reserve courts on a regular basis
        /// </summary>
        //[TestMethod]
        public void CanReserveCourt()
        {
            var client = new CourtReservationClient();
            var service = new CourtReservationServiceSpareTime(client, logger1);
            service.SetHost("goldriver.tennisbookings.com");

            // set our reservation time to tomorrow, 9pm - 9:30pm
            var t = DateTime.Today;
            var startTime = t.AddDays(1).AddHours(21);  // tomorrow at 9pm
            var endTime = startTime + TimeSpan.FromHours(0.5);

            var reservationDetails = new ReservationDetails(
                members: new List<ReservationMember>()
                {
                    new ReservationMember
                    {
                        FirstName = "Randy",
                        LastName = "Gamage",
                        MemberNumber = 564705
                    }
                },
                timeslot: new TimeSlot { StartTime = startTime, EndTime = endTime },
                isDoubles: false,
                courtNumber: 19
            );

            var success = service.ReserveCourts(username, password, reservationDetails);
            Assert.IsTrue(success);
        }

        /// <summary>
        /// ensure we can create the check rules post body
        /// this is used just before booking a court, to set the court, start time, and end time
        /// </summary>
        [TestMethod]
        public void CanCreateCheckRulesPost()
        {
            var targetp1 = "d=03172019&ts=5071|1900|1930[";
            var startTime = new DateTime(2019, 3, 17, 19, 0, 0);
            var endTime = new DateTime(2019, 3, 17, 19, 30, 0);
            var post = new CheckRulesPost(5052, 19, startTime, endTime);
            Assert.AreEqual(targetp1, post.p1);
        }

        /// <summary>
        /// ensure we can create the proper POST payload to book a court
        /// </summary>
        [TestMethod]
        public void CanCreateBookCourtPostTwoMembers()
        {
            var serializedString = "txtflagstring÷3271|19271||÷txtflagstring÷hidden¥dbg÷÷dbg÷hidden¥txtplayers÷564705|||N|563782|||N|÷txtplayers÷hidden¥n1÷Randy Gamage÷÷text¥n2÷Larry Miller÷÷text¥y2÷false÷÷checkbox¥n3÷÷÷text¥y3÷false÷÷checkbox¥n4÷÷÷text¥y4÷false÷÷checkbox¥txtmessage÷÷txtmessage÷textarea¥rb19263÷false÷fs3271÷radio¥rb19271÷true÷fs3271÷radio¥rb-5÷false÷fs3271÷radio¥chkpublic÷false÷chkpublic÷checkbox¥chkemailconfirmation÷false÷chkemailconfirmation÷checkbox¥chkemailconfirmationtoothers÷false÷chkemailconfirmationtoothers÷checkbox¥chkemailreminder÷false÷chkemailreminder÷checkbox¥cbohours÷1÷cbohours÷select-one";
            var reservationDetails = new ReservationDetails(

                members: new List<ReservationMember>()
                {
                    new ReservationMember
                    {
                        FirstName = "Randy",
                        LastName = "Gamage",
                        MemberNumber = 564705
                    },
                    new ReservationMember
                    {
                        FirstName = "Larry",
                        LastName = "Miller",
                        MemberNumber = 563782
                    }
                },
                timeslot: new TimeSlot { StartTime = new DateTime(2019, 3, 17, 20, 0, 0), EndTime = new DateTime(2019, 3, 17, 20, 30, 0) },
                isDoubles: false,
                courtNumber: 1
            );
            var post = new BookCourtPost(reservationDetails);
            Assert.AreEqual(serializedString, post.SerializedString);
        }

        /// <summary>
        /// test court booking with one member
        /// </summary>
        [TestMethod]
        public void CanCreateBookCourtPostOneMember()
        {
            var serializedString = "txtflagstring÷3271|19271||÷txtflagstring÷hidden¥dbg÷÷dbg÷hidden¥txtplayers÷564705|||N|÷txtplayers÷hidden¥n1÷Randy Gamage÷÷text¥n2÷÷÷text¥y2÷false÷÷checkbox¥n3÷÷÷text¥y3÷false÷÷checkbox¥n4÷÷÷text¥y4÷false÷÷checkbox¥txtmessage÷÷txtmessage÷textarea¥rb19263÷false÷fs3271÷radio¥rb19271÷true÷fs3271÷radio¥rb-5÷false÷fs3271÷radio¥chkpublic÷false÷chkpublic÷checkbox¥chkemailconfirmation÷false÷chkemailconfirmation÷checkbox¥chkemailconfirmationtoothers÷false÷chkemailconfirmationtoothers÷checkbox¥chkemailreminder÷false÷chkemailreminder÷checkbox¥cbohours÷1÷cbohours÷select-one";
            var reservationDetails = new ReservationDetails(
                members: new List<ReservationMember>()
                {
                    new ReservationMember
                    {
                        FirstName = "Randy",
                        LastName = "Gamage",
                        MemberNumber = 564705
                    }
                },
                timeslot: new TimeSlot { StartTime = new DateTime(2019, 3, 17, 20, 0, 0), EndTime = new DateTime(2019, 3, 17, 20, 30, 0) },
                isDoubles: false,
                courtNumber: 1
            );
            var post = new BookCourtPost(reservationDetails);
            Assert.AreEqual(serializedString, post.SerializedString);
        }

        [TestMethod]
        public void CanCreateBookCourtPostOneMemberThreeGuests()
        {
            var serializedString = "txtflagstring÷3271|19263||÷txtflagstring÷hidden¥dbg÷÷dbg÷hidden¥txtplayers÷564705|||N||Larry||Y||Moe||Y||Curly||Y|÷txtplayers÷hidden¥n1÷Randy Gamage÷÷text¥n2÷Larry÷÷text¥y2÷true÷÷checkbox¥n3÷Moe÷÷text¥y3÷true÷÷checkbox¥n4÷Curly÷÷text¥y4÷true÷÷checkbox¥txtmessage÷÷txtmessage÷textarea¥rb19263÷true÷fs3271÷radio¥rb19271÷false÷fs3271÷radio¥rb-5÷false÷fs3271÷radio¥chkpublic÷false÷chkpublic÷checkbox¥chkemailconfirmation÷false÷chkemailconfirmation÷checkbox¥chkemailconfirmationtoothers÷false÷chkemailconfirmationtoothers÷checkbox¥chkemailreminder÷false÷chkemailreminder÷checkbox¥cbohours÷1÷cbohours÷select-one";
            var reservationDetails = new ReservationDetails(
                members: new List<ReservationMember>()
                {
                    new ReservationMember
                    {
                        FirstName = "Randy",
                        LastName = "Gamage",
                        MemberNumber = 564705
                    },
                    new ReservationMember
                    {
                        FirstName = "Larry",
                        LastName = "",
                        MemberNumber = 0
                    },
                    new ReservationMember
                    {
                        FirstName = "Moe",
                        LastName = "",
                        MemberNumber = 0
                    },
                    new ReservationMember
                    {
                        FirstName = "Curly",
                        LastName = "",
                        MemberNumber = 0
                    }
                },
                timeslot: new TimeSlot { StartTime = new DateTime(2019, 3, 17, 20, 0, 0), EndTime = new DateTime(2019, 3, 17, 20, 30, 0) },
                isDoubles: true,
                courtNumber: 1
            );
            var post = new BookCourtPost(reservationDetails);
            Assert.AreEqual(serializedString, post.SerializedString);
        }

        [TestMethod]
        public void CanDetectConsecutiveNumbers()
        {
            var numbers = new List<string> { "02", "03", "04" };
            var result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsTrue(result);

            numbers = new List<string> { "00", "01", "02" };
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsTrue(result);  

            numbers = new List<string> { "02", "04", "05" };
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsFalse(result);

            numbers = new List<string> { "06" };
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsTrue(result);

            numbers = new List<string>();
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsTrue(result);

            numbers = new List<string> { "05", "04", "03" };
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsFalse(result);

            numbers = new List<string> { "2", "3", "4" };
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsTrue(result);

            numbers = new List<string> { "0", "1", "2" };
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsTrue(result);

            numbers = new List<string> { "2", "4", "5" };
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsFalse(result);

            numbers = new List<string> { "6" };
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsTrue(result);

            numbers = new List<string>();
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsTrue(result);

            numbers = new List<string> { "5", "4", "3" };
            result = StringHelper.ConsecutiveNumbers(numbers);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// test if we can review upcoming matches, and if enabled, auto-reserve courts for them
        /// </summary>
        //[TestMethod]
        public async Task CanAutoReserveCourts()
        {
            var client = new CourtReservationClient();
            var service = new CourtReservationServiceSpareTime(client, logger1);
            service.SetHost("goldriver.tennisbookings.com");  

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(CourtReservationServiceSpareTime)))
                .Returns(service);
            using (var context = new TennisContext(Options))
            {
                Seed(SeedOptions.WithPlayersInLineup);
                Match1.AutoReserveCourts = true;
                Match1.MatchVenue.ReservationSystem = new ReservationSystem
                {
                    CourtReservationProvider = CourtReservationProvider.TennisBookings,
                    HostName = "goldriver.tennisbookings.com",
                    MaxDaysAhead = 2
                };
                Match1.Format = PlayFormat.MensSingles;
                context.Matches.Update(Match1);

                // set court number so we can check later if it was updated to reserved court number
                Line1.CourtNumber = "99";
                context.Lines.Update(Line1);

                context.SaveChanges();

                var memberManager = new MemberManager(context, logger2);
                var lineManager = new LineManager(context, logger3, memberManager);
                var reservationManager = new ReservationManager(context, logger4, serviceProvider.Object, lineManager);
                var result = await reservationManager.ReserveAllCourts();
                Assert.IsTrue(result);
            }
            using (var context = new TennisContext(Options))
            {
                var line = context.Lines.First(l => l.LineId == Line1.LineId);
                Assert.IsTrue(line.IsReserved);
                Assert.IsFalse(line.CourtNumber == "99");
            }
        }

        [TestMethod]
        public void CanParseWarningMessage()
        {
            var testString = "{\"d\":\"cb_warn(\\\"Booking failed: Bookings for North (Lighted) cannot be for longer than 90 minutes.\\\",1)\"}";
            Regex regex = new Regex(@"cb_warn\(\\""(?<warningMessage>.+)\\""");
            System.Text.RegularExpressions.Match match = regex.Match(testString);
            var warningMessage = match.Groups["warningMessage"].Value;
            Assert.IsTrue(!string.IsNullOrEmpty(warningMessage));
        }
    }
}
