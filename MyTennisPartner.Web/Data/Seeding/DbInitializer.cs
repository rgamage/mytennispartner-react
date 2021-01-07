using MyTennisPartner.Data.Context;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyTennisPartner.Data.Models;
using Microsoft.AspNetCore.Identity;
using MyTennisPartner.Models.Enums;
using System.Globalization;
using MyTennisPartner.Web.Utilities;
using Microsoft.AspNetCore.Hosting;
using MyTennisPartner.Models.Users;
using MyTennisPartner.Utilities;

namespace MyTennisPartner.Web.Data.Seeding
{
    public class DbInitializer
    {

        private readonly TennisContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SeedHttpClient httpClient;
        private readonly List<RandomUser> seedUsers;
        private readonly List<ApplicationUser> appUsers;
        private readonly IHostingEnvironment environment;
        private readonly Random random;

        public DbInitializer(
            TennisContext context,
            UserManager<ApplicationUser> userManager,
            SeedHttpClient httpClient,
            List<RandomUser> seedUsers,
            IHostingEnvironment environment
        )
        {
            this.context = context;
            this.userManager = userManager;
            this.httpClient = httpClient;
            this.seedUsers = seedUsers;
            this.environment = environment;
            random = new Random();
            if (this.userManager != null)
            {
                appUsers = this.userManager.Users.ToList();
            }
            else
            {
                appUsers = new List<ApplicationUser>();
            }
        }

        public void Initialize()
        {
            if (context is null || httpClient is null || seedUsers is null || userManager is null) return;
            try
            {                
                // Creates a TextInfo based on the "en-US" culture.
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

                // seed user database with users based on member data
                // to re-generate the random-users.json file, enter this into a browser or fiddler:
                //  https://randomuser.me/api?results=50&format=pretty&nat=us

                // always seed venues if there are none, because no member can save their profile without one
                var venuesBase = JsonConvert.DeserializeObject<List<Venue>>(File.ReadAllText($"Data/Seeding/venues.json"));
                if (!context.Venues.Any())
                {
                    context.Venues.AddRange(venuesBase);
                    context.SaveChanges();
                }
                else
                {
                    // add any new venues
                    var existingVenueNames = context.Venues.Select(v => v.Name).ToList();
                    foreach(var venue in venuesBase)
                    {
                        if (!existingVenueNames.Contains(venue.Name))
                        {
                            context.Venues.Add(venue);
                        }
                    }
                    context.SaveChanges();
                }

                var existingVenues = context.Venues.ToList();

                var adminUser = userManager.GetUsersInRoleAsync("Admin").Result.FirstOrDefault();
                var seedAdminUser = seedUsers.FirstOrDefault(u => u.email == adminUser?.Email);

                // create admin Member profile - for the case of a new db, in PROD
                if (seedAdminUser != null)
                {
                    CreateMemberFromUser(seedAdminUser, existingVenues);
                    context.SaveChanges();
                }

                if (environment.IsProduction()) return;

                // if we haven't added any seed users beyond the one admin, then we don't want to seed leagues, matches, etc.
                if (seedUsers.Count <= 1) return;

                context.Addresses.RemoveRange(context.Addresses);
                context.Contacts.RemoveRange(context.Contacts);
                context.ReservationSystems.RemoveRange(context.ReservationSystems);
                context.Venues.RemoveRange(context.Venues);
                context.Players.RemoveRange(context.Players);
                context.Leagues.RemoveRange(context.Leagues);
                context.Members.RemoveRange(context.Members);
                context.Lines.RemoveRange(context.Lines);
                context.Matches.RemoveRange(context.Matches);
                context.SaveChanges();

                var venues = JsonConvert.DeserializeObject<List<Venue>>(File.ReadAllText($"Data/Seeding/venues.json"));
                context.Venues.AddRange(venues);

                foreach (var seedUser in seedUsers)
                {
                    CreateMemberFromUser(seedUser, venues);
                }
                context.SaveChanges();

                List<League> leagues = JsonConvert.DeserializeObject<List<League>>(File.ReadAllText($"Data/Seeding/leagues.json"));
                foreach (var league in leagues)
                {
                    var memberIndex = random.Next(0, context.Members.Count());
                    league.Owner = context.Members.OrderBy(m => m.MemberId).Skip(memberIndex).Take(1).FirstOrDefault();
                    var venueIndex = random.Next(0, venues.Count);
                    league.HomeVenue = context.Venues.OrderBy(v => v.VenueId).Skip(venueIndex).Take(1).FirstOrDefault();
                }

                context.Leagues.AddRange(leagues);
                context.SaveChanges();

                Member adminMember = adminUser == null ? null :
                    context.Members.FirstOrDefault(m => m.UserId == adminUser.Id);

                // add some members to the leagues
                var numMembers = context.Members.Count();

                // add regular and sub members to each league
                // don't exceed max num of regular members specified for each league
                // add a handful of subs to each league
                // assign one person as captain of each league
                foreach (var league in leagues)
                {
                    var minLeagueMembers = league.DefaultNumberOfLines * 4;
                    var numSubs = random.Next(2, 13);
                    var numRegular = random.Next(minLeagueMembers, league.MaxNumberRegularMembers > minLeagueMembers ? league.MaxNumberRegularMembers : minLeagueMembers);
                    var numToTake = numRegular + numSubs;
                    var startIndex = random.Next(0, Math.Max(numMembers - numToTake,0));
                    var members = context.Members.OrderBy(m => m.MemberId).Skip(startIndex).Take(numToTake).ToList();
                    if (adminMember != null && !members.Any(m => m.MemberId == adminMember.MemberId))
                    {
                        // make sure we have the admin user on every league
                        members.Insert(0,adminMember);
                    }
                    var captainIndex = random.Next(0, numRegular);
                    var i = 0;
                    foreach (var member in members)
                    {
                        var leagueMember = new LeagueMember
                        {
                            MemberId = member.MemberId,
                            LeagueId = league.LeagueId
                        };
                        if (i == captainIndex)
                        {
                            leagueMember.IsCaptain = true;
                            league.Owner = member;
                        }
                        if (i >= numRegular)
                        {
                            leagueMember.IsSubstitute = true;
                        }
                        var homeVenue = context.Venues.OrderBy(v => v.VenueId).Skip(random.Next(0, venues.Count)).Take(1).FirstOrDefault();
                        league.HomeVenue = homeVenue;
                        i++;
                        context.LeagueMembers.Add(leagueMember);
                    }
                }

                context.SaveChanges();

                // now add some matches
                var leagues2 = context.Leagues.ToList();
                var weekCounter = 0;
                List<Player> allPlayers = new List<Player>();
                List<Line> allLines = new List<Line>();
                foreach (var league in leagues2)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        const int timeZoneShift = 7;
                        var monthDiff = random.Next(3) - 1;
                        var date = DateTime.Today.AddMonths(monthDiff);  // some matches will be in the past, some in the future
                        var month = date.Month;
                        var year = date.Year;
                        var hour = (9 + random.Next(0, 11) + timeZoneShift) % 24;  // match start times from 9am to 7pm
                        var leagueId = league.LeagueId;
                        var startTime = new DateTime(year, month, (weekCounter * 7) % 28 + 1, hour, 0, 0);
                        var endTime = startTime.AddHours(1.5);
                        var format = 3 + random.Next(3);
                        var venue = venues[random.Next(venues.Count())];
                        var isHome = random.Next(2) == 1;
                        var lines = new List<Line>();
                        var matchPlayers = new List<Player>();
                        var leagueMemberPool = league.LeagueMembers.ToList();
                        for (var j = 0; j < league.DefaultNumberOfLines; j++) {
                            var players = new List<Player>();
                            for (var k = 0; k<4; k++)
                            {
                                var leagueMember = leagueMemberPool.OrderBy(l => l.LeagueMemberId).Skip((j + k) % leagueMemberPool.Count).First();
                                var p = new Player
                                {
                                    LeagueId = league.LeagueId,
                                    LeagueMemberId = leagueMember.LeagueMemberId,
                                    MemberId = leagueMember.MemberId,
                                    IsHomePlayer = (k % 2 == 0),
                                    IsSubstitute = leagueMember.IsSubstitute
                                };
                                players.Add(p);
                                leagueMemberPool.Remove(leagueMemberPool.FirstOrDefault(lm => lm.LeagueMemberId == leagueMember.LeagueMemberId));
                            }
                            var line = new Line
                            {
                                CourtNumber = (j + 1).ToString(),
                                Format = league.DefaultFormat,
                                Guid = Guid.NewGuid().ToString()
                            };
                            lines.Add(line);
                            foreach (var p in players)
                            {
                                // associate this player with its line, using the Guid field
                                p.Guid = line.Guid;
                            }
                            matchPlayers.AddRange(players);
                        }
                        var match = new Match
                        {
                            LeagueId = leagueId,
                            StartTime = startTime,
                            EndTime = startTime.AddHours(1.5),
                            MatchVenue = venue,
                            MatchVenueVenueId = league.HomeVenueVenueId,
                            Format = league.DefaultFormat,
                            WarmupTime = startTime.AddMinutes(-league.WarmupTimeMinutes),
                            HomeMatch = isHome
                        };
                        lines.ForEach(l => match.Lines.Add(l));
                        matchPlayers.ForEach(p => match.Players.Add(p));
                        league.Matches.Add(match);
                        weekCounter++;
                        allLines.AddRange(lines);
                        allPlayers.AddRange(matchPlayers);
                    }
                }
                context.SaveChanges();

                // link up players with lines, using guid and lineId fields
                foreach(var line in allLines)
                {
                    foreach(var player in allPlayers.Where(p => p.Guid == line.Guid))
                    {
                        player.LineId = line.LineId;
                    }
                }

                context.Players.UpdateRange(allPlayers);
                context.SaveChanges();
            }
            catch(Exception ex)
            {
                var message = $"{ex.Message} - {ex.InnerException?.Message}";
                Console.WriteLine($"Error during db seeding: {message}");
                throw new Exception(message);
            }
        }

        private void CreateMemberFromUser(RandomUser seedUser, List<Venue> venues)
        {
            // match up application user to seed user, skip to next if not found
            var user = appUsers.FirstOrDefault(u => u.Email == seedUser.email);
            if (user == null) return;

            var existingMember = context.Members.FirstOrDefault(m => m.UserId == user.Id);
            if (existingMember != null) return;  // already exists, so exit

            var skill = random.Next(0, 3);
            var venueIndex = random.Next(0, venues.Count);
            var member = new Member
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = seedUser.gender == "male" ? Gender.Male : Gender.Female,
                HomeVenue = venues[venueIndex],
                ZipCode = seedUser.location.postcode.ToString(),
                BirthYear = seedUser.dob.Year,
                SkillRanking = skill == 0 ? "3.0" : skill == 1 ? "3.5" : "4.0",
                PhoneNumber = seedUser.phone,
                MemberRoleFlags = MemberRoleFlags.Player
            };

            var imageBytes = GetProfileImage(httpClient, seedUser.picture.large);
            member.Image = new MemberImage
            {
                ImageBytes = imageBytes
            };

            // handle special case user for court reservation creds
            if (member.FirstName == "Randy" && member.LastName == "Gamage")
            {
                member.SpareTimeUsername = "rgamage";
                member.SpareTimePassword = "august07";
                member.SpareTimeMemberNumber = 564705;
            }

            context.Members.Add(member);
        }

        private static byte[] GetProfileImage(SeedHttpClient client, string url)
        {
            try
            {
                using (var stream = client.GetStreamAsync(new Uri(url)).Result)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                var message = $"{ex.Message} - {ex.InnerException?.Message}";
                Console.WriteLine($"Error downloading image: {message}");
                return Array.Empty<byte>();
            }
        }
    }
}
