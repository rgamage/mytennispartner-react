using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using RestSharp;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Models.CourtReservations;
using MyTennisPartner.Models.CourtReservations.TennisBookings;
using MyTennisPartner.Core;
using MyTennisPartner.Utilities;

namespace MyTennisPartner.Core.Services.Reservations
{

    // service to manage reservation of courts, check court availability, etc.
    public partial class CourtReservationServiceSpareTime : ICourtReservationService
    {
        private readonly ILogger<CourtReservationServiceSpareTime> logger;

        const string LoginSubmitUrl = "LoginX.aspx/MyCallBack?p=LoginUser";
        const string LoginStringTemplate = @"checkjs÷SCRIPTS÷checkjs÷hidden¥miscinfo÷d=1920x1200;wo=none;mobo=0÷miscinfo÷hidden¥origurl÷https://{host}/LoginX.aspx÷origurl÷hidden¥loreturnurl÷https://{host}/LoginX.aspx÷loreturnurl÷hidden¥myqs÷mysecrethere÷myqs÷hidden¥setpfromc÷÷setpfromc÷hidden¥txtusername÷{username}÷txtusername÷text¥txtpassword÷{password}÷txtpassword÷password¥chkautologin÷false÷chkautologin÷checkbox¥junkforformatting÷false÷÷checkbox¥chkusermobo÷false÷chkusermobo÷checkbox";
        const string GridSubUrlTemplate = "gridsub.aspx?reqresourceIDs={courtResourceIds}&reqdates={reqDates}&gsx=0&gsy=0";
        const string CheckRulesUrl = "ViewSchedules.aspx/MyCallBack?p=CheckRules";
        const string ConfirmingUrl = "Confirming.aspx";
        const string BookCourtUrl = "Confirming.aspx/MyCallBack?p=Save";

        /// <summary>
        /// login string, calculated based on login string template and host name
        /// </summary>
        private string LoginString;

        /// <summary>
        /// base id number for court Ids, different for each club
        /// </summary>
        private int courtBaseId;

        /// <summary>
        /// rest sharp client
        /// </summary>
        private readonly RestClientService restClientService;

        private string hostName;

        /// <summary>
        /// constructor
        /// </summary>
        public CourtReservationServiceSpareTime(CourtReservationClient restClient, ILogger<CourtReservationServiceSpareTime> logger)
        {
            this.logger = logger;
            restClientService = new RestClientService(restClient, logger);
        }

        /// <summary>
        /// set host url.  This method MUST be called before any request methods, as we need to know the host url
        /// </summary>
        /// <param name="hostname"></param>
        public void SetHost(string hostname)
        {
            // set url-specific info, based on venue's reservation system url
            restClientService.SetHost(hostname);
            //restClientService.AddHeader("Referer", $"https://{hostname}/Default.aspx?open=1");
            LoginString = LoginStringTemplate.Replace("{host}", hostname);
            hostName = hostname;
        }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// court resource names depend upon the club, so we need this decoder method
        /// </summary>
        /// <param name="hostname"></param>
        private string GetCourtResourceIds(string hostname)
        {
            return hostname switch
            {
                "goldriver.tennisbookings.com"    => "1067469600|1067469601",
                "riodeloro.tennisbookings.com"    => "1067471017|1067471018|1067471019|1067471020|1067471200",
                "broadstone.tennisbookings.com"   => "1067471121|1067471122",
                "lagunacreek.tennisbookings.com"  => "1067471158|1067471161|1067471159|1067471160",
                "johnsonranch.tennisbookings.com" => "1067471130|1067471131|1067471132|1067471133",
                "natomas.tennisbookings.com"      => "1067471149|1067471150",
                "diamondhills.tennisbookings.com" => "1067471167|1067471168",
                _                                 => "",
            };
        }

        /// <summary>
        /// reserve courts
        /// </summary>
        /// <returns></returns>
        public bool ReserveCourts(string username, string password, ReservationDetails reservation)
        {
            // example post for Randy Gamage reserving, with Guests Pat, Dave, Joe (each checked as Guests), for court 20, Sunday 3/17, 8:00-8:30pm, with e-mail conf requested for Randy, no e-mail reminder selected, no e-mail to other players, and do not display my name on the reservation.  Selected Doubles.
            // var postData =  {"Action":"Save","SerializedString":"txtflagstring÷3271|19263||÷txtflagstring÷hidden¥dbg÷÷dbg÷hidden¥txtplayers÷564705|||N||Pat||Y||Joe||Y||Dave||Y|÷txtplayers÷hidden¥n1÷Randy Gamage÷÷text¥n2÷Pat÷÷text¥y2÷true÷÷checkbox¥n3÷Joe÷÷text¥y3÷true÷÷checkbox¥n4÷Dave÷÷text¥y4÷true÷÷checkbox¥txtmessage÷÷txtmessage÷textarea¥rb19263÷true÷fs3271÷radio¥rb19271÷false÷fs3271÷radio¥rb-5÷false÷fs3271÷radio¥chkpublic÷false÷chkpublic÷checkbox¥chkemailconfirmation÷true÷chkemailconfirmation÷checkbox¥chkemailconfirmationtoothers÷false÷chkemailconfirmationtoothers÷checkbox¥chkemailreminder÷true÷chkemailreminder÷checkbox¥cbohours÷12÷cbohours÷select-one","p1":"","p2":"","p3":"","p4":"","p5":""}
            // similar to above, but just me and Larry Miller (not as Guest), and at 7:00-7:30pm, Singles
            // var postData = {"Action":"Save","SerializedString":"txtflagstring÷3271|19271||÷txtflagstring÷hidden¥dbg÷÷dbg÷hidden¥txtplayers÷564705|||N|563782|||N|÷txtplayers÷hidden¥n1÷Randy Gamage÷÷text¥n2÷Larry Miller÷÷text¥y2÷false÷÷checkbox¥n3÷÷÷text¥y3÷false÷÷checkbox¥n4÷÷÷text¥y4÷false÷÷checkbox¥txtmessage÷÷txtmessage÷textarea¥rb19263÷false÷fs3271÷radio¥rb19271÷true÷fs3271÷radio¥rb-5÷false÷fs3271÷radio¥chkpublic÷false÷chkpublic÷checkbox¥chkemailconfirmation÷false÷chkemailconfirmation÷checkbox¥chkemailconfirmationtoothers÷false÷chkemailconfirmationtoothers÷checkbox¥chkemailreminder÷false÷chkemailreminder÷checkbox","p1":"","p2":"","p3":"","p4":"","p5":""}
            // do work here
            // to find member number, search for: memnum="564705" in /Confirming.aspx that is returned after clicking on the Book link
            // to fetch this Confirming.aspx, send POST request to /ViewSchedules.aspx/MyCallBack?p=CheckRules, with this content: {"Action":"CheckRules","SerializedString":"","p1":"d=03172019&ts=5072|1900|1930[","p2":"","p3":"","p4":"","p5":""}
            // the response is this simple json: {"d":"cb_goConfirming()"}
            // then fetch /Confirming.aspx, which contains the html with memnum="NNNNN"
            // question: can we just GET this Confirming.aspx directly, or do we need to first call /ViewSchedules?
            // upon entering their SP creds into MTP, we should fetch this member number and save it in our db, so don't have to fetch it again
            // also, if they don't click the "Test" button on the member profile page after entering their creds, then we should test it for them, to get this info
            // or perhaps if the creds are new/changed, don't allow saving of the profile until this new info is tested
            // in db, we should record that it has been tested successfully or not, and their member number
            var success = Login(username, password);
            if (!success)
            {
                var message = "ReserveCourts - failed to log in to reservation provider";
                throw new Exception(message);
            }
            if (reservation is null)
            {
                throw new Exception("Reserve Courts failed: Reservation is null");
            }
            var tomorrow = DateTime.UtcNow + TimeSpan.FromDays(1);
            var availTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 10, 0, 0);
            GetCourtAvailability(username, password, availTime);
            var checkRulesPost = new CheckRulesPost(courtBaseId, reservation.CourtNumber, reservation.TimeSlot.StartTime, reservation.TimeSlot.EndTime);
            var checkRulesResponse = restClientService.Post(CheckRulesUrl, checkRulesPost);
            if (checkRulesResponse.ErrorException != null)
            {
                var message = $"ReserveCourts - failed to post to CheckRules url: {checkRulesResponse.ErrorException.Message} - {checkRulesResponse.ErrorException.InnerException}";
                logger.LogError(message);
            }

            restClientService.Get(ConfirmingUrl);
            //var confirmingPage = confirmingResponse.Content;

            var post = new BookCourtPost(reservation);
            var response = restClientService.Post(BookCourtUrl, post);
            if (response.ErrorException != null)
            {
                var message = $"ReserveCourts - failed to book court {reservation.CourtNumber}: {response.ErrorException.Message} - {response.ErrorException.InnerException}";
                logger.LogError(message);
                return false;
            }

            if (response.Content.Contains("cb_conf("))
            {
                // success!
                logger.LogInformation($"Successfully reserved court {reservation.CourtNumber}");
                return true;
            }

            // we failed somehow.  Look for a warning message in the response
            Regex regex = new Regex(@"cb_warn\(\\""(?<warningMessage>.+)\\""");
            Match match = regex.Match(response.Content);
            var warningMessage = match.Groups["warningMessage"].Value;
            var errorMessage = !string.IsNullOrEmpty(warningMessage) ? warningMessage : response.Content;
            logger.LogError("Failed to reserve courts - error response message below");
            logger.LogError(errorMessage);

            return false;
        }

        /// <summary>
        /// method to test login to reservation system
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public int LoginTest(string username, string password)
        {
            var tomorrow = DateTime.UtcNow + TimeSpan.FromDays(1);
            var availTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 10, 0, 0, DateTimeKind.Utc);

            try
            {
                // this is necessary for the login test, because we need to parse the base court id
                GetCourtAvailability(username, password, availTime);
            }
            catch
            {
                // login failed, silently return 0 for member number
                return 0;
            }

            // set our reservation time to tomorrow, 8am - 8:30am
            var t = DateTime.Today;
            var startTime = t.AddDays(1).AddHours(8);
            var endTime = startTime + TimeSpan.FromHours(0.5);

            // try to get member number
            var checkRulesPost = new CheckRulesPost(courtBaseId, 1, startTime, endTime);
            var checkRulesResponse = restClientService.Post(CheckRulesUrl, checkRulesPost);
            if (checkRulesResponse.ErrorException != null)
            {
                var message = $"LoginTest - failed to post to CheckRules url: {checkRulesResponse.ErrorException.Message} - {checkRulesResponse.ErrorException.InnerException}";
                logger.LogError(message);
            }
            var confirmingResponse = restClientService.Get(ConfirmingUrl);
            var confirmingPage = confirmingResponse.Content;
            // look for the string memnum="12345" in the response, to find out our member's member number
            var memberNumber = ParseMemberNumber(confirmingPage);
            return memberNumber;
        }

        /// <summary>
        /// parse member data from html page
        /// </summary>
        /// <param name="page"></param> 
        /// <returns></returns>
        private int ParseMemberNumber(string page)
        {
            Regex regex = new Regex("memnum=\"(?<memberNumber>[0-9]+)\"");
            Match match = regex.Match(page);
            _ = int.TryParse(match.Groups["memberNumber"].Value, out int memberNumber);
            return memberNumber;
        }

        public bool Login(string username, string password)
        {
            // send login request, get auth cookies
            var loginPost = new LoginPost(username, password, LoginString);
            var response = restClientService.Post(LoginSubmitUrl, loginPost);

            if (response.ErrorException != null)
            {
                var message = $"Login - failed to log in to court reservation provider: {response.ErrorException.Message} - {response.ErrorException.InnerException}";
                logger.LogError(message);
                return false;
            }
            var data = response.Content;
            if (data.Contains("Error") || data.Contains("error") || data.Contains("invalid login") || data.Contains("cb_nomatch()"))
            {
                var message = "Login - failed to log in to court reservation provider.  Received error page from provider.";
                logger.LogError(message);
                return false;
            }
            return true;
        }

        public CourtOpeningCollection GetCourtAvailability(string username, string password, DateTime dateUtc, bool includeFutureDates = false)
        {
            if (string.IsNullOrEmpty(restClientService.BaseUrl?.ToString()))
            {
                var message = "GetCourtAvailability - no host specified.  Need to call SetHost to set reservation service host.";
                logger.LogError(message);
                throw new Exception(message);
            }
            // clear cookies
            restClientService.ClearCookies();

            var success = Login(username, password);
            if (!success)
            {
                var message = "GetCourtAvailability - failed to log in to reservation provider";
                throw new Exception(message);
            }

            // build the date code string for the court reservation API
            var dateLocal = TimeZoneInfo.ConvertTimeFromUtc(dateUtc, ApplicationConstants.AppTimeZoneInfo);
            var refDate = new DateTime(2005, 1, 1);
            var offsetDays = (dateLocal - refDate).Days;
            var dateCode = $"d{offsetDays}";

            //var gridRequest = new RestRequest(gridUrl, Method.GET);
            //gridRequest.AddHeader("Referer", courtReservationGridReferralUrl);
            //// get reservation grid to see availability
            //var response2 = client.Execute(gridRequest);
            var gridUrlBase = GridSubUrlTemplate
                .Replace("{courtResourceIds}", GetCourtResourceIds(hostName))
                .Replace("{reqDates}", dateCode);
            var gridUrl = gridUrlBase;
            logger.LogInformation("fetching court grid");
            var response2 = restClientService.Get(gridUrl);

            logger.LogInformation($"got response statusCode = {response2.StatusCode}:{response2.StatusDescription} - content = {response2.Content}");
            if (response2.ErrorException != null)
            {
                var message = $"GetCourtAvailability - failed to fetch court reservation grid from provider - {response2.ErrorMessage}";
                logger.LogError(message);
                throw new Exception(message);
            }
            var data2 = response2.Content;
            if (data2.Contains("Error") || data2.Contains("error"))
            {
                var message = $"GetCourtAvailability - failed to fetch court reservation grid from provider";
                logger.LogError(message);
                throw new Exception(message);
            }

            var reservations = ParseReservationGrid(data2, dateLocal, includeFutureDates);
            logger.LogInformation("GetCourtAvailability - successfully retrieved court reservations grid");

            return reservations;
        }



        /// <summary>
        /// parse the court reservation grid result (HTML)
        /// </summary>
        private CourtOpeningCollection ParseReservationGrid(string html, DateTime dateLocal, bool includeFutureDates)
        {
            /*
             * The main reservation table is a <table> element with id = "TT"
             * this table has 36 rows - <tr> elements - with ids = "TT1" thru "TT36", representing times from 5am to 11pm (18hrs) in 1/2hr increments = 36 slots
             * each row has 25 columns, with ids like "r1c1" (row 1, col 1).  The rN is time slot number, and the cN is court N. 
             * each column (<td> element) has a "rowspan" telling how many time slots that reservation spans
             * empty space on the grid is filled by <td> elements with no Id, and class could be ="rhspacer" - we can ignore these elements
             * 
             * Algorithm:
             * 1) Parse html into collection of rows, each row having a collection of columns with ids parsed out and rowspans parsed out
             * 2) Iterate through this collection to fill an object holding courts, each court having a collection of booked time slots
             * 3) Eventually we want to pass a time period and number of courts required, and get back a collection of free courts that match the requirement,
             *    with preferences based on adjacent location, lighted in evening, avoid bad courts, etc.
             * NOTE - Idea for improving / simplifying the algorithm:
             *  1) Parse the collection of HtmlNode objects into a class with nested structure mimicking the table/row/colums
             *  2) Use LINQ to find all cols with class "x" or "f" for busy or free, along with all needed meta-data for that timeslot (court#, etc.)
             *  3) Profit!
             */

            logger.LogInformation($"Starting ParseReservationGrid, dateLocal = {dateLocal}, dateLocal.Date = {dateLocal.Date}");
            bool parseSuccess;
            var collection = new CourtOpeningCollection
            {
                Date = dateLocal
            };

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // try to parse the base court id for this club
            var tableNodes = htmlDoc?.DocumentNode?.SelectNodes("//table");
            if (tableNodes == null)
            {
                logger.LogError("Unable to parse reservation grid - could not get tableNodes from html.");
                return collection;
            }

            var headersTable = tableNodes
                .Where(n => n.Attributes["id"] != null && n.Attributes["id"].Value == "Headers")
                .FirstOrDefault();
            var headersTableHtml = headersTable.InnerHtml;
            var headerDoc = new HtmlDocument();
            headerDoc.LoadHtml(headersTableHtml);
            var headerRow = headerDoc.DocumentNode.SelectNodes("//tr")
                .FirstOrDefault();
            var headerCell = headerRow.SelectNodes("//td")
                .FirstOrDefault();
            var firstCourtIdString = headerCell.Attributes["pass"]?.Value;
            var firstCourtSubString = firstCourtIdString.Substring(0, firstCourtIdString.IndexOf("|"));
            parseSuccess = int.TryParse(firstCourtSubString, out int firstCourtId);
            if (!parseSuccess)
            {
                logger.LogError("ParseReservationGrid ERROR - unable to parse firstCourtIdSubString as int");
                return collection;
            }
            courtBaseId = firstCourtId - 1;
            // done parsing base court id

            var gridTable = tableNodes
               .Where(n => n.Attributes["id"] != null && n.Attributes["id"].Value == "TT")
               .FirstOrDefault();

            if (gridTable == null) return collection;
            var rows = gridTable.SelectNodes("//tr").ToList();

            // start time of the grid varies by day (e.g. 5am on weekdays, 7am on weekends, etc.)
            // so we need to parse the first row's start time
            var startTimeText = rows[0].Attributes["mytag"]?.Value;
            var startHourText = startTimeText.Split(':')?[0];
            var startMinuteText = startTimeText.Split(':')?[1];
            parseSuccess = int.TryParse(startHourText, out int startHour);
            if (!parseSuccess)
            {
                logger.LogError("ParseReservationGrid ERROR - unable to parse startHourText as int");
                return collection;
            }
            parseSuccess = int.TryParse(startMinuteText, out int startMinute);
            if (!parseSuccess)
            {
                logger.LogError("ParseReservationGrid ERROR - unable to parse startMinuteText as int");
                return collection;
            }
            var startTimeOffset = startHour + startMinute / 60;
            foreach (var row in rows)
            {
                var rowid = row.Attributes["id"]?.Value;
                if (rowid == null || !rowid.StartsWith("TT")) continue;
                var slotNumber = int.Parse(rowid.Substring(2));
                var startTime = dateLocal.Date + TimeSpan.FromHours(0.5 * (slotNumber - 1) + startTimeOffset);
                var a = row.InnerHtml;
                var h = new HtmlDocument();
                h.LoadHtml(a);
                var cols = h.DocumentNode.SelectNodes("//td")
                            .Where(n => n.Attributes["id"] != null && n.Attributes["id"].Value.StartsWith("r"))
                            .ToList();
                foreach (var col in cols)
                {
                    var colid = col.Attributes["id"]?.Value;
                    if (colid == null) continue;

                    Regex regex = new Regex("r(?<row>[0-9]+)c(?<col>[0-9]+)");
                    Match match = regex.Match(colid);
                    if (!match.Success) continue;

                    var rowSlot = match.Groups["row"].Value;
                    var colSlot = match.Groups["col"].Value;
                    var courtNumber = colSlot.ToString();
                    var description = col.InnerHtml;

                    var courtSchedule = collection.CourtSchedules
                                            .FirstOrDefault(c => c.CourtNumber == courtNumber);
                    if (courtSchedule == null)
                    {
                        courtSchedule = new CourtSchedule
                        {
                            CourtNumber = courtNumber
                        };
                        collection.CourtSchedules.Add(courtSchedule);
                    }


                    var rowSpan = col.Attributes["rowspan"]?.Value;
                    parseSuccess = int.TryParse(rowSpan, out int rowSpanInt);
                    if (!parseSuccess)
                    {
                        if (col.HasClass("x"))
                        {
                            // class 'x' means busy, so same as rowspan = 1
                            rowSpanInt = 1;
                        }
                        else
                        {
                            // this slot must be free (class 'f'), so move on to next column
                            continue;
                        }
                    }
                    if (rowSpan == null) continue;
                    var endTime = startTime + TimeSpan.FromHours(rowSpanInt * 0.5);

                    var reservation = new TimeSlot
                    {
                        StartTime = startTime,
                        EndTime = endTime
                    };

                    // if we are checking future availability, then we want to ignore the slots that are showing 'not bookable'
                    if (!includeFutureDates || description?.Contains("Date not yet Bookable") == false)
                    {
                        courtSchedule.BookedTimeSlots.Add(reservation);
                    }
                }
            }
            return collection;
        }
    }
}
