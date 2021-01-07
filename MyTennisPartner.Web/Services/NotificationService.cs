using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Utilities;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.Enums;
using MyTennisPartner.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTennisPartner.Web.Services
{
    /// <summary>
    /// class to handle notification of various events for users
    /// </summary>
    public class NotificationService
    {
        private readonly IEmailSender _emailSender;
        private readonly TennisContext _context;
        private readonly UserDbContext _userContext;
        private readonly ILogger _logger;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly string _envTag;
        private readonly string emailHeader;
        private string siteUrl;

        private enum IdNumbers
        {
            LeagueMembers,
            Members
        }

        public NotificationService(IEmailSender emailSender, TennisContext context, UserDbContext userContext, ILogger<TennisContext> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _userContext = userContext;
            _env = env;
            _envTag = _env == null ? "" :
                      _env.EnvironmentName == "LocalDevelopment" ? "-LocalDEV" :
                      _env.IsDevelopment() ? "-DEV" :
                      _env.IsStaging() ? "-UAT" : "";
             emailHeader = $"<div style=\"color:#f2f2f2; background-color:#408800; font-family: sans-serif; padding-top: 1px; padding-left: 10px; padding-bottom: 1px;\"><h2>My Tennis Partner{_envTag}</h2></div>";
        }

        /// <summary>
        /// notify users of various events
        /// </summary>
        /// <param name="events"></param>
        public async Task NotifyAsync(IEnumerable<NotificationEvent> events, string requestScheme, string requestHost)
        {
            if (events == null) return;

            siteUrl = $"{requestScheme}://{requestHost}";

            foreach (var ev in events)
            {
                _logger.LogInformation($"NotifyAsync: starting notification, of type ${ev.EventDescription}");
                switch (ev.EventType)
                {
                    case NotificationEventType.AddedToMatch:
                        await HandleAddedToMatch(ev);
                        break;
                    case NotificationEventType.RemovedFromMatch:
                        await HandleRemovedFromMatch(ev);
                        break;

                    case NotificationEventType.CourtChange:
                        await HandleCourtChange(ev);
                        break;

                    case NotificationEventType.MatchCancelled:
                        await HandleMatchCancelled(ev);
                        break;

                    case NotificationEventType.MatchChanged:
                        await HandleMatchChanged(ev);
                        break;

                    case NotificationEventType.SubForMatchOpening:
                        await HandleAvailabilityUpdate(ev);
                        break;

                    case NotificationEventType.MatchAdded:
                        await HandleMatchAdded(ev);
                        break;

                    case NotificationEventType.AddedToLeague:
                        await HandleAddedToLeague(ev);
                        break;

                    case NotificationEventType.MatchAddedReminder:
                        await HandleMatchAddedReminder(ev);
                        break;

                    case NotificationEventType.PlayerResponded:
                        // user feedback was: too many e-mails
                        // disable for now, until we add a member notification preference for this
                        //await HandlePlayerResponded(ev);
                        break;

                    case NotificationEventType.CourtAutoAdded:
                        await HandleCourtAutoAdded(ev);
                        break;
                }
            }
        }

        private async Task<List<string>> GetUsers(NotificationEvent notEvent, Func<IQueryable<Member>, IQueryable<Member>> preference)
        {
            var idType = notEvent.LeagueMemberIds.Any() ? IdNumbers.LeagueMembers
                : IdNumbers.Members;

            var ids = idType == IdNumbers.LeagueMembers ? notEvent.LeagueMemberIds
                    : notEvent.MemberIds;

            List<string> userIds = new List<string>();

            switch (idType)
            {
                case IdNumbers.LeagueMembers:
                    var members1 = _context.LeagueMembers
                    .Where(lm => ids.Contains(lm.LeagueMemberId))
                    .Select(lm => lm.Member);

                    // apply member preference filter to IQueryable
                    members1 = preference(members1);

                    // take userIds
                    userIds = await members1
                        .Select(m => m.UserId)
                        .ToListAsync();
                    break;

                case IdNumbers.Members:
                    var members2 = _context.Members
                        .Where(m => ids.Contains(m.MemberId));

                    // apply member prefs
                    members2 = preference(members2);

                    userIds = await members2
                        .Select(m => m.UserId)
                        .ToListAsync();
                    break;
            }
            return userIds;
        }

        private async Task<List<MailRecipient>> GetMailRecipientsFromUserIds(List<string> userIds)
        {
            // look up email addresses of all affected members
            var emails = await _userContext.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new MailRecipient { Email = u.Email, DisplayName = $"{u.FirstName} {u.LastName}" })
                .ToListAsync();

            return emails;
        }

        /// <summary>
        /// handle remove event
        /// </summary>
        /// <param name="notificationEvent"></param>
        /// <returns></returns>
        private async Task HandleRemovedFromMatch(NotificationEvent notificationEvent)
        {
            //var userIds = await GetUsersFromMemberIdsAddRemove(notificationEvent.MemberIds);
            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifyAddOrRemoveMeFromMatch));
            var emails = await GetMailRecipientsFromUserIds(userIds);

            if (!emails.Any()) return;

            var matchUrl = GetMatchUrl(notificationEvent);
            var subject = $"MTP{_envTag}: You've been removed from a match";
            var message = $"{emailHeader}{notificationEvent.EventDescription}You have been removed from a match.  Click link <a href=\"{matchUrl}\">HERE</a> to view match details";
            await SendEmail(emails, subject, message);
        }

        /// <summary>
        /// handle event where sub opportunity opens up 
        /// </summary>
        /// <param name="notificationEvent"></param>
        /// <returns></returns>
        private async Task HandleAvailabilityUpdate(NotificationEvent notificationEvent)
        {
            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifySubForMatchOpening));
            var emails = await GetMailRecipientsFromUserIds(userIds);

            if (!emails.Any()) return;

            var referringMember = _context.Members
                .Find(notificationEvent.ReferringMemberId);

            var matchUrl = GetMatchUrl(notificationEvent);
            var acceptPath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId={notificationEvent.ReferringMemberId}&availability={(int)Availability.Confirmed}";
            var declinePath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId={notificationEvent.ReferringMemberId}&availability={(int)Availability.Unavailable}";
            var acceptUrl = siteUrl + acceptPath;
            var declineUrl = siteUrl + declinePath;
            var subject = $"MTP{_envTag}: Sub opportunity - {notificationEvent.MatchSummary.ShortSummary}";
            var message = $"{emailHeader}{notificationEvent.EventDescription}<p>We need your help to fill in a spot for an upcoming match.  {referringMember.FirstName} {referringMember.LastName} is unable to play in this match.  ";
            message += $"Click link <a href=\"{siteUrl + "/dashboard"}\">HERE</a> to view this match opportunity on your dashboard.  ";
            message += "Click one of the links below to instantly respond to this request.</p>";
            message += $"<div>To accept this match, click here: <br /><br /></div><div><span style=\"background-color:#408800;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{acceptUrl}\">ACCEPT</a></span></div>";
            message += "<br />";
            message += $"<div>To decline this match, click here: <br /><br /></div><div><span style=\"background-color:red;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{declineUrl}\">DECLINE</a></span></div>";
            await SendEmail(emails, subject, message);
        }

        /// <summary>
        /// handle add event
        /// </summary>
        /// <param name="notificationEvent"></param>
        /// <returns></returns>
        private async Task HandleAddedToMatch(NotificationEvent notificationEvent)
        {
            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifyAddOrRemoveMeFromMatch));
            var emails = await GetMailRecipientsFromUserIds(userIds);

            if (!emails.Any()) return;

            var matchUrl = GetMatchUrl(notificationEvent);
            var subject = $"MTP{_envTag}: You've been added to a match";
            var message = $"{emailHeader}{notificationEvent.EventDescription}You have been added to a match.  Click link <a href=\"{siteUrl + "/dashboard"}\">HERE</a> to view your current match commitments, or to change your availability.<br />";
            message += BuildMatchSummaryHtml(notificationEvent.LeagueId, notificationEvent.MatchId);

            await SendEmail(emails, subject, message);
        }

        private async Task HandleCourtChange(NotificationEvent notificationEvent)
        {
            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifyCourtChange));
            var emails = await GetMailRecipientsFromUserIds(userIds);

            if (!emails.Any()) return;

            var matchUrl = GetMatchUrl(notificationEvent);
            var subject = $"MTP{_envTag}: Courts / line-ups have changed";
            var message = $"{emailHeader}{notificationEvent.EventDescription}Courts have changed on your match.  ";

            var matchSummary = BuildMatchSummaryHtml(notificationEvent.LeagueId, notificationEvent.MatchId);
            message += matchSummary;

            message += $"<br />Click link <a href=\"{siteUrl + "/dashboard"}\">HERE</a> to view your current matches"; 

            // todo - add line-up to e-mail message

            await SendEmail(emails, subject, message);

        }

        private async Task HandleMatchCancelled(NotificationEvent notificationEvent)
        {
            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifyMatchDetailsChangeOrCancelled));
            var emails = await GetMailRecipientsFromUserIds(userIds);

            if (!emails.Any()) return;

            var leaguePath = $"/leagues/schedule/{notificationEvent.LeagueId}";
            var leagueUrl = siteUrl + leaguePath;
            var subject = $"MTP{_envTag}: Match cancelled";
            var message = $"{emailHeader}{notificationEvent.EventDescription}<p>Your match has been cancelled.  Click link <a href=\"{leagueUrl}\">HERE</a> to view upcoming matches for this league.</p>";
            await SendEmail(emails, subject, message);
        }

        private async Task HandleMatchChanged(NotificationEvent notificationEvent)
        {
            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifyMatchDetailsChangeOrCancelled));
            var emails = await GetMailRecipientsFromUserIds(userIds);

            if (!emails.Any()) return;

            var matchUrl = GetMatchUrl(notificationEvent);
            var subject = $"MTP{_envTag}: Match changed";
            var message = $"{emailHeader}{notificationEvent.EventDescription}<p>Your match details (date, time, and/or venue) have changed.  Updated time/date and venue are shown above.</p><p>Click link <a href=\"{siteUrl + "/dashboard"}\">HERE</a> to view all the latest details for this match and others on your dashboard.</p>";
            await SendEmail(emails, subject, message);
        }

        private async Task HandleMatchAdded(NotificationEvent notificationEvent)
        {
            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifyMatchAdded));
            var emails = await GetMailRecipientsFromUserIds(userIds);
            if (!emails.Any()) return;

            var matchUrl = GetMatchUrl(notificationEvent);
            var subject = $"MTP{_envTag}: Match added - {notificationEvent.MatchSummary.ShortSummary} - set your availability";
            var message = $"{emailHeader}{notificationEvent.EventDescription}<p>A new match has been scheduled. The time/date and venue are shown above.</p><p>Click link <a href=\"{siteUrl + "/dashboard"}\">HERE</a> to view all the latest details for this match on your dashboard.  Click one of the links below to let us know if you are available for this match.</p>";
            var acceptPath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId=0&availability={(int)Availability.Confirmed}";
            var declinePath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId=0&availability={(int)Availability.Unavailable}";
            var acceptUrl = siteUrl + acceptPath;
            var declineUrl = siteUrl + declinePath;

            message += $"<div>If you are available for this match, click here: <br /><br /></div><div><span style=\"background-color:#408800;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{acceptUrl}\">I'm Available</a></span></div>";
            message += "<br />";
            message += $"<div>If you are not available for this match, click here: <br /><br /></div><div><span style=\"background-color:red;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{declineUrl}\">I'm NOT available</a></span></div>";
            await SendEmail(emails, subject, message);
        }

        /// <summary>
        /// build html message summarizing match status and respondents
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public string BuildMatchSummaryHtml(int leagueId, int matchId)
        {
            var leagueMemberPlayers = (from lm in _context.LeagueMembers
                                        .Where(lm => lm.LeagueId == leagueId)
                                       join p in _context.Players.Where(p => p.MatchId == matchId) on lm.MemberId equals p.MemberId into plm
                                       from p in plm.DefaultIfEmpty()
                                       select new
                                       {
                                           LeagueMember = lm,
                                           Player = p,
                                           IsInLineup = p == null ? false : p.LineId > 0,
                                           FullName = $"{lm.Member.FirstName} {lm.Member.LastName}",
                                           p.LineId
                                       })
        .OrderByDescending(player => player.Player.ModifiedDate)
        .ToList();

            var lines = _context.Lines
                           .Where(l => l.MatchId == matchId)
                           .ToList()
                           .OrderBy(l => l.CourtNumber);

            var sb = new StringBuilder();
            sb.Append("<p>Current status of players</p>");
            var inLineup = leagueMemberPlayers.Where(p => p.IsInLineup);
            if (inLineup.Any())
            {
                sb.Append("<h4>In line-up</h4>");
                foreach (var line in lines)
                {
                    sb.Append($"<strong>Court {(string.IsNullOrEmpty(line.CourtNumber) ? "?" : line.CourtNumber)}</strong>");
                    sb.Append("<ul>");
                    foreach (var p in inLineup.Where(p => p.LineId == line.LineId))
                    {
                        var sideNote = p.Player == null || p.Player.Availability == Availability.Unknown ?
                            " (has not confirmed)" :
                            p.Player?.Availability == Availability.Unavailable ? " (but is unavailable!)" : " (confirmed)";
                        sb.Append($"<li>{p.FullName}{sideNote}</li>");
                    }
                    sb.Append("</ul>");
                }
            }
            else
            {
                sb.Append("<h4>There is no line-up yet for this match</h4>");
            }
            var avail = leagueMemberPlayers.Where(p => !p.IsInLineup && p.Player?.Availability == Availability.Confirmed);
            if (avail.Any())
            {
                if (!inLineup.Any())
                {
                    sb.Append("<h4>Available players</hr>");
                }
                else
                {
                    sb.Append("<h4>Also available to play/sub</h4>");
                }
                foreach (var p in avail)
                {
                    sb.Append($"<li>{p.FullName}</li>");
                }
                sb.Append("</ul>");
            }
            else
            {
                sb.Append("<h4 style=\"color:red;\">WARNING - There are no available subs for this match!</h4>");
            }
            var notResponded = leagueMemberPlayers.Where(lmp => lmp.Player == null || lmp.Player.Availability == Availability.Unknown)
                .Where(lmp => !lmp.IsInLineup);
            if (notResponded.Any())
            {
                sb.Append("<h4>Not yet responded</h4>");
                sb.Append("<ul>");
                foreach (var p in notResponded)
                {
                    sb.Append($"<li>{p.FullName}</li>");
                }
                sb.Append("</ul>");
            }
            var unavailable = leagueMemberPlayers.Where(p => p.Player?.Availability == Availability.Unavailable)
                .Where(lmp => !lmp.IsInLineup);
            if (unavailable.Any())
            {
                sb.Append("<h4>Unavailable players</h4>");
                sb.Append("<ul>");
                foreach (var p in unavailable)
                {
                    sb.Append($"<li>{p.FullName}</li>");
                }
                sb.Append("</ul>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// reminder e-mail for matches that have been added, but players have not responded to with their availability
        /// </summary>
        /// <param name="notificationEvent"></param>
        /// <returns></returns>
        private async Task HandleMatchAddedReminder(NotificationEvent notificationEvent)
        {
            // change to send this only to league captains
            var captainIds = _context.LeagueMembers
                .Where(lm => lm.LeagueId == notificationEvent.LeagueId)
                .Where(lm => lm.IsCaptain)
                .Select(lm => lm.MemberId)
                .ToList();
            notificationEvent.LeagueMemberIds.Clear();
            notificationEvent.MemberIds.AddRange(captainIds);
            // end change

            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifyMatchAdded));
            var emails = await GetMailRecipientsFromUserIds(userIds);
            if (!emails.Any()) return;

            var matchSummaryHtml = BuildMatchSummaryHtml(notificationEvent.LeagueId, notificationEvent.MatchId);

            var matchUrl = GetMatchUrl(notificationEvent);
            var subject = $"MTP{_envTag}: Upcoming match - please set your availability";
            var message = $"{emailHeader}{notificationEvent.EventDescription}<p>A match has been scheduled, and you have not yet replied with your availability. The time/date and venue are shown above.</p><p>Click link <a href=\"{siteUrl + "/dashboard"}\">HERE</a> to view all the latest details for this match on your dashboard, or see more details below.  Click one of the links below to let us know if you are available for this match.</p>";
            message += $"<p>We are sending this reminder because the captain requested it, or because early tomorrow morning, courts can be assigned and reserved, if there are enough available players.  If your {ApplicationConstants.league} has the auto-add feature enabled, then any available players will be added automatically to the line-up, in groups, to fill courts.  If your league has auto court reservations enabled, then those courts will be reserved as well.</p>";
            var acceptPath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId=0&availability={(int)Availability.Confirmed}";
            var declinePath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId=0&availability={(int)Availability.Unavailable}";
            var acceptUrl = siteUrl + acceptPath;
            var declineUrl = siteUrl + declinePath;

            message += $"<div>If you are available for this match, click here: <br /><br /></div><div><span style=\"background-color:#408800;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{acceptUrl}\">I'm Available</a></span></div>";
            message += "<br />";
            message += $"<div>If you are not available for this match, click here: <br /><br /></div><div><span style=\"background-color:red;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{declineUrl}\">I'm NOT available</a></span></div>";
            message += matchSummaryHtml;
            await SendEmail(emails, subject, message);
        }

#pragma warning disable IDE0051 // Remove unused private members
        /// <summary>
        /// handle match general reminder 
        /// </summary>
        /// <param name="notificationEvent"></param>
        /// <returns></returns>
        private async Task HandlePlayerResponded(NotificationEvent notificationEvent)
#pragma warning restore IDE0051 // Remove unused private members
        {
            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifyMatchAdded));
            var emails = await GetMailRecipientsFromUserIds(userIds);
            if (!emails.Any()) return;

            var matchSummaryHtml = BuildMatchSummaryHtml(notificationEvent.LeagueId, notificationEvent.MatchId);
            
            var referringMember = await _context.Members
                .FindAsync(notificationEvent.ReferringMemberId);
            var respondentFullName = $"{referringMember.FirstName} {referringMember.LastName}";

            var referringPlayer = await _context.Players
                .Where(p => p.MemberId == referringMember.MemberId) 
                .Where(p => p.MatchId == notificationEvent.MatchId)
                .FirstOrDefaultAsync();

            var genderPronoun = referringMember?.Gender == Gender.Male ? "he" : referringMember?.Gender == Gender.Female ? "she" : "they";
            var response = referringPlayer?.Availability == Availability.Confirmed   ? "is available."
                         : referringPlayer?.Availability == Availability.Unavailable ? "will not be able to make it."
                         :                                                             "is still up in the air.";
                         
            var matchUrl = GetMatchUrl(notificationEvent);
            var subject = $"MTP{_envTag}: Upcoming match - {respondentFullName} {response}";
            var message = $"{emailHeader}{notificationEvent.EventDescription}<p>{respondentFullName} has responded to the match and {genderPronoun} {response}</p>";
            message += $"<p>A summary of who has responded and their status is shown below.  These updates will not be sent to those who have declined the match.  If you have not yet responded, you can do so by clicking on the buttons at the bottom of this e-mail.</p>";
            var acceptPath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId=0&availability={(int)Availability.Confirmed}";
            var declinePath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId=0&availability={(int)Availability.Unavailable}";
            var acceptUrl = siteUrl + acceptPath;
            var declineUrl = siteUrl + declinePath;
            message += matchSummaryHtml;
            message += $"<div>If you haven't responded and are available for this match, click here: <br /><br /></div><div><span style=\"background-color:#408800;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{acceptUrl}\">I'm Available</a></span></div>";
            message += "<br />";
            message += $"<div>If you haven't reseponded and are not available for this match, click here: <br /><br /></div><div><span style=\"background-color:red;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{declineUrl}\">I'm NOT available</a></span></div>";
            await SendEmail(emails, subject, message);
        }

        /// <summary>
        /// handle event when a court is auto-added due to someone becoming available and rounding out a court
        /// sends only to those that are not yet in line-up, and un-decided
        /// </summary>
        /// <param name="notificationEvent"></param>
        /// <returns></returns>
        private async Task HandleCourtAutoAdded(NotificationEvent notificationEvent)
        {
            var userIds = await GetUsers(notificationEvent, m => m.Where(mm => mm.NotifyMatchAdded));
            var emails = await GetMailRecipientsFromUserIds(userIds);
            if (!emails.Any()) return;

            var matchSummaryHtml = BuildMatchSummaryHtml(notificationEvent.LeagueId, notificationEvent.MatchId);

            var matchUrl = GetMatchUrl(notificationEvent);
            var subject = $"MTP{_envTag}: Court Filled!";
            var message = $"{emailHeader}{notificationEvent.EventDescription}<p>We just got enough available players to fill out a court!  Status of all players and courts, line-up etc. is shown below.</p>";
            message += $"<p>These updates will not be sent to those who have declined the match.  If you have not yet responded, please do so by clicking on the buttons at the bottom of this e-mail.</p>";
            var acceptPath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId=0&availability={(int)Availability.Confirmed}";
            var declinePath = $"/matches/Respond?matchId={notificationEvent.MatchId}&leagueId={notificationEvent.LeagueId}&referringMemberId=0&availability={(int)Availability.Unavailable}";
            var acceptUrl = siteUrl + acceptPath;
            var declineUrl = siteUrl + declinePath;
            message += matchSummaryHtml;
            message += $"<div>If you are available for this match, click here: <br /><br /></div><div><span style=\"background-color:#408800;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{acceptUrl}\">I'm Available</a></span></div>";
            message += "<br />";
            message += $"<div>If you are not available for this match, click here: <br /><br /></div><div><span style=\"background-color:red;padding:8px;\"><a style=\"color:white;font-family:sans-serif;font-weight:bold;text-decoration:none;\" href=\"{declineUrl}\">I'm NOT available</a></span></div>";
            await SendEmail(emails, subject, message);
        }

        /// <summary>
        /// handle added to league notification
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private async Task HandleAddedToLeague(NotificationEvent notificationEvent) {
            var userIds = await GetUsers(notificationEvent, m => m);
            var emails = await GetMailRecipientsFromUserIds(userIds);
            if (!emails.Any()) return;

            var leagueName = _context.Leagues.First(l => l.LeagueId == notificationEvent.LeagueId).Name;
            var leaguePath = $"/leagues/schedule/{notificationEvent.LeagueId}";
            var leagueUrl = siteUrl + leaguePath;
            var subject = $"MTP{_envTag}: You've been added to a {ApplicationConstants.league}: {leagueName}";
            var message = $"{emailHeader}{notificationEvent.EventDescription}<p>You have been added to a {ApplicationConstants.league}: {leagueName}.  Welcome to the {ApplicationConstants.league}!</p><p>Click link <a href=\"{leagueUrl}\">HERE</a> to view details for this {ApplicationConstants.league}.</p>";

            await SendEmail(emails, subject, message);
        }

        /// <summary>
        /// get url to match
        /// </summary>
        /// <param name="notificationEvent"></param>
        /// <returns></returns>
        private string GetMatchUrl(NotificationEvent notificationEvent)
        {
            var matchPath = $"/matches/details/{notificationEvent.MatchId}/{notificationEvent.LeagueId}";
            var matchUrl = $"{siteUrl}{matchPath}";
            return matchUrl;
        }

        /// <summary>
        /// send the e-mail
        /// </summary>
        /// <param name="emails"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendEmail(List<MailRecipient> emails, string subject, string message)
        {
            // add a footer at the bottom of every e-mail
            var footer = $"<div><small><br /><p>If you want to un-subscribe to these e-mail alerts, go to <a href=\"{siteUrl}\">My Profile</a> and click on Notification Preferences to customize your messaging.</p></small></div>";
            message += footer;

            // send message
            await _emailSender.SendEmailAsync(emails, subject, message);
        }
    }
}
