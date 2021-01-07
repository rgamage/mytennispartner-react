using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTennisPartner.Data.Managers
{
    public class VenueManager: ManagerBase
    {
        public VenueManager(TennisContext context, ILogger<VenueManager> logger) : base(context, logger)
        {            
        }

        /// <summary>
        /// get venues
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<VenueViewModel>> GetVenues() {
            var venues = await Context.Venues
                .Include(v => v.VenueAddress)
                .Include(v => v.VenueContact)
                .Include(v => v.ReservationSystem)
                .OrderBy(v => v.Name)
                .ToListAsync();
            var venueViewModels = ModelMapper.Map<List<VenueViewModel>>(venues);
            return venueViewModels;
        }

        /// <summary>
        /// get venue by id
        /// </summary>
        /// <param name="venueId"></param>
        /// <returns></returns>
        public async Task<VenueViewModel> GetVenue(int venueId)
        {
            var venue = await Context.Venues
                .Include(v => v.VenueAddress)
                .Include(v => v.VenueContact)
                .Include(v => v.ReservationSystem)
                .Where(v => v.VenueId == venueId)
                .FirstOrDefaultAsync();
            var venueViewModel = ModelMapper.Map<VenueViewModel>(venue);
            return venueViewModel;
        }

        public async Task<VenueViewModel> UpdateVenue(VenueViewModel venueVm)
        {
            Venue venue;
            if (venueVm.VenueId > 0)
            {
                // updating an existing venue
                venue = Context.Venues.Where(v => v.VenueId == venueVm.VenueId).FirstOrDefault();
                ModelMapper.Map(venueVm, venue);
                Context.Venues.Update(venue);
            }
            else
            {
                // creating a new venue
                venue = new Venue()
                {
                    VenueAddress = new Address(),
                    VenueContact = new Contact()
                };
                ModelMapper.Map(venueVm, venue);
                Context.Venues.Add(venue);
            }
            await Context.SaveChangesAsync();
            return ModelMapper.Map<VenueViewModel>(venue);
        }

        /// <summary>
        /// search venues
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Venue>> SearchVenues(string search)
        {
            var venues = await Context.Venues
                .Where(v => v.Name.Contains(search))
                .Include(v => v.ReservationSystem)
                .Include(v => v.VenueAddress)
                .Include(v => v.VenueContact)
                .OrderBy(v => v.Name)
                .ToListAsync();

            return venues;
        }


        /// <summary>
        /// delete a venue
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteVenue(int id)
        {
            var venue = await Context.Venues.Where(v => v.VenueId == id).FirstOrDefaultAsync();
            if (venue != null)
            {
                 Context.Venues.Remove(venue);
                 await Context.SaveChangesAsync();
            }
        }
    }
}
