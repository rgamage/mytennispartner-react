using MyTennisPartner.Data.Context;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.Enums;
using System.Globalization;

namespace MyTennisPartner.Data.Test.Seeding
{
    public static class DbInitializer
    {
        public static void DeleteAllData(TennisContext context)
        {
            if (context is null) throw new Exception("ERROR deleting data in database: DB Context is null");
            context.Addresses.RemoveRange(context.Addresses);
            context.Contacts.RemoveRange(context.Contacts);
            context.Venues.RemoveRange(context.Venues);
            context.Players.RemoveRange(context.Players);
            context.Leagues.RemoveRange(context.Leagues);
            context.Members.RemoveRange(context.Members);
            context.Lines.RemoveRange(context.Lines);
            context.Matches.RemoveRange(context.Matches);
            context.SaveChanges();
        }

        public static void Initialize(TennisContext context)
        {
            if (context is null) throw new Exception("ERROR initializing database: DB Context is null");

            try
            {
                var random = new Random();
                // Creates a TextInfo based on the "en-US" culture.
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

                // seed user database with users based on member data
                // to re-generate the random-users.json file, enter this into a browser or fiddler:
                //  https://randomuser.me/api?results=50&format=pretty&nat=us

                // only seed if there are no existing users or members in the db
                if (context.Members.Any()) return;
                DbInitializer.DeleteAllData(context);

                var venues = JsonConvert.DeserializeObject<List<Venue>>(File.ReadAllText($"Seeding/venues.json"));
                context.Venues.AddRange(venues);

                // Get random user data
                RandomUserResults randomUserResult = JsonConvert.DeserializeObject<RandomUserResults>(File.ReadAllText($"Seeding/random-users.json"));
                var randomUsers = randomUserResult.Results.ToList();

                foreach (var randomUser in randomUsers)
                {
                    var skill = random.Next(0, 3);
                    var venueIndex = random.Next(0, venues.Count);
                    var member = new Member
                    {
                        UserId = Guid.NewGuid().ToString(),
                        FirstName = myTI.ToTitleCase(randomUser.name.first),
                        LastName = myTI.ToTitleCase(randomUser.name.last),
                        Gender = randomUser.gender == "male" ? Gender.Male : Gender.Female,
                        HomeVenue = venues[venueIndex],
                        ZipCode = randomUser.location.postcode.ToString(),
                        BirthYear = randomUser.dob.Year,
                        SkillRanking = skill == 0 ? "3.0" : skill == 1 ? "3.5" : "4.0",
                        PhoneNumber = randomUser.phone
                    };
                    if (member.FirstName == "Randy" && member.LastName == "Gamage")
                    {
                        member.SpareTimeUsername = "rgamage";
                        member.SpareTimePassword = "august07";
                    }
                    context.Members.Add(member);
                }

                List<League> leagues = JsonConvert.DeserializeObject<List<League>>(File.ReadAllText($"Seeding/leagues.json"));
                foreach (var league in leagues)
                {
                    var memberIndex = random.Next(0, context.Members.Count());
                    league.Owner = context.Members.OrderBy(m => m.MemberId).Skip(memberIndex).Take(1).FirstOrDefault();
                    var venueIndex = random.Next(0, venues.Count);
                    league.HomeVenue = context.Venues.OrderBy(v => v.VenueId).Skip(venueIndex).Take(1).FirstOrDefault();
                }

                context.Leagues.AddRange(leagues);

                context.SaveChanges();

                // add some members to the leagues
                //const int minLeagueMembers = 4;
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
                    var numToTake = Math.Min(numRegular + numSubs, numMembers);
                    var startIndex = random.Next(0, numMembers - numToTake);
                    var members = context.Members.OrderBy(m => m.MemberId).Skip(startIndex).Take(numToTake).ToList();
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
                foreach(var league in leagues2)
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
                        var leagueMemberPool = league.LeagueMembers.ToList();
                        var matchPlayers = new List<Player>();
                        for (var j = 0; j < league.DefaultNumberOfLines; j++) {
                            var players = new List<Player>();
                            for (var k =0; k<4; k++)
                            {
                                var leagueMember = leagueMemberPool.OrderBy(m => m.LeagueMemberId).Skip((j + k) % leagueMemberPool.Count).First();
                                var p = new Player
                                {
                                    LeagueId = league.LeagueId,
                                    LeagueMemberId = leagueMember.LeagueMemberId,
                                    MemberId = leagueMember.MemberId,
                                    IsSubstitute = leagueMember.IsSubstitute
                                };
                                players.Add(p);
                                leagueMemberPool.Remove(leagueMemberPool.FirstOrDefault(lm => lm.LeagueMemberId == leagueMember.LeagueMemberId));
                            }
                            var line = new Line
                            {
                                CourtNumber = (j + 1).ToString(),
                                Format = league.DefaultFormat,
                            };
                            lines.Add(line);
                            foreach(var p in players)
                            {
                                p.LineId = line.LineId;
                            }
                            matchPlayers.AddRange(players);
                        }
                        var match = new Match
                        {
                            LeagueId = leagueId,
                            StartTime = startTime,
                            EndTime = startTime.AddHours(1.5),
                            MatchVenue = venue,
                            MatchVenueVenueId = venue.VenueId,
                            Format = (PlayFormat)format,
                            WarmupTime = startTime.AddMinutes(-league.WarmupTimeMinutes),
                            HomeMatch = isHome
                        };
                        lines.ForEach(l => match.Lines.Add(l));
                        matchPlayers.ForEach(p => match.Players.Add(p));
                        league.Matches.Add(match);
                        weekCounter++;
                    }
                }
                context.SaveChanges();
            }
            catch(Exception ex)
            {
                var message = $"{ex.Message} - {ex.InnerException?.Message}";
                Console.WriteLine($"Error during db seeding: {message}");
                throw new Exception(message);
            }
        }
        
    }
}
