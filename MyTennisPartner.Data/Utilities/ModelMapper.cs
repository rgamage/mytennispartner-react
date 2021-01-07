using AutoMapper;
using MyTennisPartner.Models.ViewModels;
using System;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Data.DataTransferObjects;
using MyTennisPartner.Models.Enums;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace MyTennisPartner.Data.Utilities
{
    /// <summary>
    /// static class to manage mapping methods for populating one model from another similar model
    /// for example, datamodel to business model, or to viewmodel, etc.
    /// </summary>
    public static class ModelMapper
    {
        /// <summary>
        /// mapper
        /// </summary>
        public static IRuntimeMapper Mapper { get; }

        /// <summary>
        /// constructor
        /// </summary>
        static ModelMapper()
        {
            // create mapping configurations for our models
            var mapperConfig = new MapperConfiguration(cfg =>
            {

            cfg.CreateMap<League, League>();
            cfg.CreateMap<Match, Match>();
            cfg.CreateMap<Line, Line>();

            cfg.CreateMap<MemberViewModel, Member>()
                .ForMember(dest => dest.HomeVenueVenueId,
                    opt => opt.MapFrom(src => src.HomeVenue.VenueId))
                .ForMember(dest => dest.HomeVenue,
                    opt => opt.Ignore())  // ignore home venue object, because we don't want to insert/update when updating/inserting member
                .ForMember(dest => dest.PlayerPreferences,
                    opt => opt.MapFrom(src => src.PlayerPreferences))
                ;

            cfg.CreateMap<Member, MemberViewModel>()
                    .ForMember(dest => dest.HomeVenueName,
                        opt => opt.MapFrom(src => src.HomeVenue.Name))
                    .ForMember(dest => dest.HomeVenue,
                        opt => opt.MapFrom(src => src.HomeVenue))
                    .ForPath(dest => dest.HomeVenue.VenueAddress,
                        opt => opt.MapFrom(src => src.HomeVenue.VenueAddress))
                    .ForPath(dest => dest.HomeVenue.VenueContact,
                        opt => opt.MapFrom(src => src.HomeVenue.VenueContact))
                    .ForMember(dest => dest.PlayerPreferences,
                        opt => opt.MapFrom(src => src.PlayerPreferences))
                    .ForMember(dest => dest.ReservationCredentials,
                        opt => opt.Ignore())
                    ;
            //.ForMember(dest => dest.SpareTimeUsername,
            //    opt => opt.MapFrom(src => src.ReservationCredentials == null ? null 
            //        : src.ReservationCredentials.FirstOrDefault(c => c.CourtReservationProvider == Models.CourtReservationProvider.TennisBookings) == null ? null 
            //        : src.ReservationCredentials.FirstOrDefault(c => c.CourtReservationProvider == Models.CourtReservationProvider.TennisBookings).Username))
            //.ForMember(dest => dest.SpareTimePassword,
            //    opt => opt.MapFrom(src => src.ReservationCredentials == null ? null
            //        : src.ReservationCredentials.FirstOrDefault(c => c.CourtReservationProvider == Models.CourtReservationProvider.TennisBookings) == null ? null
            //        : src.ReservationCredentials.FirstOrDefault(c => c.CourtReservationProvider == Models.CourtReservationProvider.TennisBookings).Password));

            cfg.CreateMap<League, LeagueSearchViewModel>()
                .ForMember(dest => dest.VenueName,
                    opt => opt.MapFrom(src => src.HomeVenue.Name))
                .ForMember(dest => dest.StartTimeLocal,
                    opt => opt.MapFrom(src => DateTime.Today + src.MatchStartTime))
                .ForMember(dest => dest.OwnerMemberId,
                    opt => opt.MapFrom(src => src.Owner.MemberId))
                .ForMember(dest => dest.VenueIconColor,
                    opt => opt.MapFrom(src => ColorHelper.GetSeededRandomColor(src.LeagueId)));

                cfg.CreateMap<League, LeagueSummary>();
                   //.ForPath(dest => dest.HomeVenue.VenueAddress,
                   //     opt => opt.MapFrom(src => src.HomeVenue.VenueAddress));


                cfg.CreateMap<LeagueSummary, League>()
                    .ForMember(dest => dest.HomeVenueVenueId,
                        opt => opt.MapFrom(src => src.HomeVenue.VenueId))
                    .ForMember(dest => dest.OwnerMemberId,
                        opt => opt.MapFrom(src => src.Owner.MemberId));

                cfg.CreateMap<League, LeagueSummaryViewModel>()
                    .ForMember(dest => dest.MatchStartTime,
                        opt => opt.MapFrom(src => TimeOfDayToString(src.MatchStartTime)));

                cfg.CreateMap<Address, AddressViewModel>();
                cfg.CreateMap<Contact, ContactViewModel>();
                cfg.CreateMap<AddressViewModel, Address>();
                cfg.CreateMap<ContactViewModel, Contact>();
                cfg.CreateMap<ReservationSystem, ReservationSystemViewModel>();
                cfg.CreateMap<ReservationSystemViewModel, ReservationSystem>();

                cfg.CreateMap<LeagueSummary, LeagueSummaryViewModel>()
                    .ForMember(dest => dest.MatchStartTimeLocal,
                        opt => opt.MapFrom(src => DateTimeHelper.UtcToLocalTimeString(TimeOfDayToString(src.MatchStartTime))))
                    .ForMember(dest => dest.MatchStartTime,
                        opt => opt.MapFrom(src => TimeOfDayToString(src.MatchStartTime)));
                    //.ForPath(dest => dest.HomeVenue.VenueAddress,
                    //    opt => opt.MapFrom(src => src.HomeVenue.VenueAddress));

                cfg.CreateMap<LeagueSummaryViewModel, League>()
                    .ForMember(dest => dest.MatchStartTime,
                        opt => opt.MapFrom(src => ParseTimeOfDay(src.MatchStartTime)))
                    .ForMember(dest => dest.OwnerMemberId,
                        opt => opt.MapFrom(src => src.Owner.MemberId))
                    .ForMember(dest => dest.HomeVenueVenueId,
                        opt => opt.MapFrom(src => src.HomeVenue == null ? (int?)null : src.HomeVenue.VenueId))
                    .ForMember(dest => dest.HomeVenue,
                        opt => opt.Ignore())  // we don't want to map entire object, only the id.  Otherwise we will get an EF insert error, when we are just trying to update the id
                    .ForMember(dest => dest.Owner,
                        opt => opt.Ignore()); // we don't want to map entire object, only the id.  Otherwise we will get an EF insert error, when we are just trying to update the id

                cfg.CreateMap<Venue, VenueViewModel>()
                    .ForMember(dest => dest.Value,
                        opt => opt.MapFrom(src => src.VenueId))
                    .ForMember(dest => dest.Label,
                        opt => opt.MapFrom(src => src.Name));

                cfg.CreateMap<VenueViewModel, Venue>()
                    .ForMember(dest => dest.VenueAddress,
                        opt => opt.MapFrom(src => src.VenueAddress))
                    .ForMember(dest => dest.VenueContact,
                        opt => opt.MapFrom(src => src.VenueContact))
                    ;

                cfg.CreateMap<Member, MemberNameViewModel>()
                    .ForMember(dest => dest.HomeVenueName,
                        opt => opt.MapFrom(src => src.HomeVenue.Name))
                    .ForMember(dest => dest.Value,
                        opt => opt.MapFrom(src => src.MemberId))
                    .ForMember(dest => dest.Label,
                        opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

                cfg.CreateMap<MemberNameViewModel, Member>();

                cfg.CreateMap<MemberViewModel, MemberNameViewModel>()
                    .ForMember(dest => dest.Value,
                        opt => opt.MapFrom(src => src.MemberId))
                    .ForMember(dest => dest.Label,
                        opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}{(src.IsSubstitute ? " (Sub)" : "")}"));

                cfg.CreateMap<PlayerPreference, PlayerPreferenceViewModel>()
                    .ForMember(dest => dest.Value,
                        opt => opt.MapFrom(src => (int)src.Format))
                    .ForMember(dest => dest.Label,
                        opt => opt.MapFrom(src => src.Format.DescriptionAttr()));

                cfg.CreateMap<PlayerPreferenceViewModel, PlayerPreference>()
                    .ForMember(dest => dest.Format,
                        opt => opt.MapFrom(src => (PlayFormat)src.Value));

                cfg.CreateMap<Match, MatchViewModel>()
                    .ForMember(dest => dest.VenueIconColor,
                        opt => opt.MapFrom(src => ColorHelper.GetSeededRandomColor(src.League.LeagueId)))
                    .ForMember(dest => dest.LeagueName,
                        opt => opt.MapFrom(src => src.League.Name))
                    .ForMember(dest => dest.VenueHasReservationSystem,
                        opt => opt.MapFrom(src => src.MatchVenue.ReservationSystem != null))
                    .ForMember(dest => dest.MatchVenue,
                        opt => opt.MapFrom(src => src.MatchVenue))
                    .ForMember(dest => dest.Players,
                        opt => opt.MapFrom(src => src.Players))
                    .ForPath(dest => dest.MatchVenue.VenueAddress,
                        opt => opt.MapFrom(src => src.MatchVenue.VenueAddress))
                    .ForPath(dest => dest.MatchVenue.VenueContact,
                        opt => opt.MapFrom(src => src.MatchVenue.VenueContact))
                    .ForMember(dest => dest.MarkNewCourtsReserved,
                        opt => opt.MapFrom(src => src.League.MarkNewCourtsReserved))
                    .ForMember(dest => dest.MarkNewPlayersConfirmed,
                        opt => opt.MapFrom(src => src.League.MarkNewPlayersConfirmed))
                    // note, since Automapper 9.0, this line is not necessary.  Nested mapping is not automatic, so lines will not be mapped if not explicitly configured
                    .ForMember(dest => dest.Lines,
                        opt => opt.Ignore())  // when fetching a match from the client, do not include lines because lines are fetched with linemanager/controller which add more information
                    ;

                cfg.CreateMap<LineViewModel, Line>();

                cfg.CreateMap<Line, LineViewModel>()
                    .ForMember(dest => dest.LeagueId,
                        opt => opt.MapFrom(src => src.Match.LeagueId))
                    .ForMember(dest => dest.LeagueName,
                        opt => opt.MapFrom(src => src.Match.League.Name));

                cfg.CreateMap<MatchViewModel, Match>()
                    .ForMember(dest => dest.MatchVenueVenueId,
                        opt => opt.MapFrom(src => src.MatchVenue.VenueId))
                    .ForMember(dest => dest.Lines,
                        opt => opt.MapFrom(src => src.Lines))
                    .ForMember(dest => dest.Players,
                        opt => opt.MapFrom(src => src.Players))
                    .ForMember(dest => dest.MatchVenue,
                        opt => opt.MapFrom(src => src.MatchVenue))
                    .ForPath(dest => dest.MatchVenue.VenueAddress,
                        opt => opt.MapFrom(src => src.MatchVenue.VenueAddress))
                    .ForPath(dest => dest.MatchVenue.VenueContact,
                        opt => opt.MapFrom(src => src.MatchVenue.VenueContact))
                    .ForMember(dest => dest.WarmupTime,
                        opt => opt.MapFrom(src => src.StartTime - TimeSpan.FromMinutes(src.WarmUpDurationMinutes)))
                    .ForMember(dest => dest.EndTime,
                        opt => opt.MapFrom(src => src.StartTime + TimeSpan.FromMinutes(src.MatchDurationMinutes)))
                    ;

                cfg.CreateMap<Match, MatchSummaryViewModel>()
                    .ForMember(dest => dest.StartTimeLocal,
                        opt => opt.MapFrom(src => TimeZoneInfo.ConvertTimeFromUtc(src.StartTime, DataConstants.AppTimeZoneInfo)))
                    //.ForMember(dest => dest.WarmupTimeLocal,
                    //    opt => opt.MapFrom(src => TimeZoneInfo.ConvertTimeFromUtc(src.WarmupTime, DataConstants.AppTimeZoneInfo)))
                    //.ForMember(dest => dest.EndTimeLocal,
                    //    opt => opt.MapFrom(src => TimeZoneInfo.ConvertTimeFromUtc(src.EndTime, DataConstants.AppTimeZoneInfo)))
                    .ForMember(dest => dest.VenueName,
                        opt => opt.MapFrom(src => src.MatchVenue.Name))
                    .ForMember(dest => dest.LeagueName,
                        opt => opt.MapFrom(src => src.League.Name));

                cfg.CreateMap<Player, PlayerViewModel>()
                    .ForMember(dest => dest.FirstName,
                        opt => opt.MapFrom(src => src.Member.FirstName))
                    .ForMember(dest => dest.LastName,
                        opt => opt.MapFrom(src => src.Member.LastName))
                    .ForMember(dest => dest.HomeVenueName,
                        opt => opt.MapFrom(src => src.Member.HomeVenue.Name))
                    .ForMember(dest => dest.Gender,
                        opt => opt.MapFrom(src => src.Member.Gender))
                    .ForMember(dest => dest.SpareTimeMemberNumber,
                        opt => opt.MapFrom(src => src.Member.SpareTimeMemberNumber))
                    .ForMember(dest => dest.IsCaptain,
                        opt => opt.MapFrom(src => src.LeagueMember.IsCaptain))
                    .ForMember(dest => dest.AutoReserveVenues,
                        opt => opt.MapFrom(src => src.Member.AutoReserveVenues))
                    .ForMember(dest => dest.CanReserveCourt,
                        opt => opt.MapFrom(src => src.Member.AutoReserveVenues == null ? false : src.Member.AutoReserveVenues.Contains($"[{src.Match.MatchVenueVenueId}]")));

                cfg.CreateMap<PlayerViewModel, Player>();

                cfg.CreateMap<LeagueMember, MemberNameViewModel>()
                    .ForMember(dest => dest.UserId,
                        opt => opt.MapFrom(src => src.Member.UserId))
                    .ForMember(dest => dest.FirstName,
                        opt => opt.MapFrom(src => src.Member.FirstName))
                    .ForMember(dest => dest.LastName,
                        opt => opt.MapFrom(src => src.Member.LastName))
                    .ForMember(dest => dest.HomeVenueName,
                        opt => opt.MapFrom(src => src.Member.HomeVenue.Name))
                    .ForMember(dest => dest.Gender,
                        opt => opt.MapFrom(src => src.Member.Gender));

            });

            // un-comment to test validity of mapping - will error if any properties are un-mapped
            //mapperConfig.AssertConfigurationIsValid();
            
            Mapper = new Mapper(mapperConfig);
        }

        /// <summary>
        /// parse time string in format "hh:mm" to a timespan
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns></returns>
        private static TimeSpan ParseTimeOfDay(string timeString)
        {
            var defaultTime = new TimeSpan(18,0,0);
            try
            {
                var hrs = int.Parse(timeString.Split(':')[0]);
                var minutes = int.Parse(timeString.Split(':')[1]);
                var result = new TimeSpan(hrs, minutes, 0);
                return result;
            }
            catch
            {
                return defaultTime;
            }
        }

        private static string TimeOfDayToString(TimeSpan timeOfDay)
        {
            var hours = timeOfDay.Hours.ToString("00");
            var minutes = timeOfDay.Minutes.ToString("00");
            return $"{hours}:{minutes}";
        }

        /// <summary>
        /// Wrapper to simplify caller syntax
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Map<T>(object myObject)
        {
            return Mapper.Map<T>(myObject);
        }

        public static void Map<T1,T2>(T1 obj1, T2 obj2)
        {
            Mapper.Map(obj1, obj2);
        }

    }
}
