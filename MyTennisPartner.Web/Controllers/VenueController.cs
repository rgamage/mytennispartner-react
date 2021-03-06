﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Models.ViewModels;

namespace MyTennisPartner.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/Venue")]
    [Authorize]
    [ApiController]
    public class VenueController : Controller
    {
        private readonly TennisContext _context;

        public VenueController(TennisContext context)
        {
            _context = context;
        }

        // GET: api/Venue
        [HttpGet]
        public IEnumerable<Venue> GetVenues()
        {
            return _context.Venues;
        }

        // GET: api/Venue/search
        [HttpGet("search")]
        public async Task<IActionResult> GetVenue([FromQuery] string name="")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var venues = await _context.Venues.Where(m => string.IsNullOrEmpty(name) || m.Name.Contains(name))
                .Include(v => v.VenueAddress)
                .Include(v => v.VenueContact)
                .OrderBy(v => v.Name)
                .ToListAsync();

            if (!venues.Any())
            {
                return NotFound();
            }

            var venueViewModels = ModelMapper.Map<List<VenueViewModel>>(venues);
            return Ok(venueViewModels);
        }

        // GET: api/Venue/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenue([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var venue = await _context.Venues.SingleOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            return Ok(venue);
        }

        // PUT: api/Venue/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenue([FromRoute] int id, [FromBody] Venue venue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (venue is null) return BadRequest();

            if (id != venue.VenueId)
            {
                return BadRequest();
            }

            _context.Entry(venue).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Venue
        [HttpPost]
        public async Task<IActionResult> PostVenue([FromBody] Venue venue)
        {
            if (!ModelState.IsValid || venue is null)
            {
                return BadRequest(ModelState);
            }

            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVenue", new { id = venue.VenueId }, venue);
        }

        // DELETE: api/Venue/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenue([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var venue = await _context.Venues.SingleOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            return Ok(venue);
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}