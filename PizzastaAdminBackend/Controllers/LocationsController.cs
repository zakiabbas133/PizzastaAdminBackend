using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzastaAdminBackend.Data;
using PizzastaAdminBackend.DTOs.Locations;
using PizzastaAdminBackend.Models;

namespace PizzastaAdminBackend.Controllers
{
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ListLocations()
        {
            var locations = await _context.Locations
                .AsNoTracking()
                .Select(l => new LocationDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Address = l.Address,
                    Phone = l.Phone,
                    Whatsapp = l.Whatsapp,
                    OpeningHours = l.OpeningHours,
                    Coordinates = new CoordinatesDto
                    {
                        Lat = l.Latitude,
                        Lng = l.Longitude
                    }
                })
                .ToListAsync();

            return Ok(new { success = true, data = locations });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddLocation([FromBody] LocationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid location data.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            var location = new Location
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Address = dto.Address.Trim(),
                Phone = dto.Phone.Trim(),
                Whatsapp = dto.Whatsapp.Trim(),
                Latitude = dto.Coordinates?.Lat ?? 0,
                Longitude = dto.Coordinates?.Lng ?? 0,
                OpeningHours = dto.OpeningHours
            };


            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, new { success = true, id = location.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetLocation(Guid id)
        {
            var l = await _context.Locations
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new LocationDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    Phone = x.Phone,
                    Whatsapp = x.Whatsapp,
                    OpeningHours = x.OpeningHours,
                    Coordinates = new CoordinatesDto { Lat = x.Latitude, Lng = x.Longitude }
                })
                .FirstOrDefaultAsync();

            if (l == null)
            {
                return NotFound(new { success = false, message = "Location not found." });
            }

            return Ok(new { success = true, data = l });
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] LocationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid location data.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (location == null)
            {
                return NotFound(new { success = false, message = "Location not found." });
            }

            location.Name = dto.Name.Trim();
            location.Address = dto.Address.Trim();
            location.Phone = dto.Phone.Trim();
            location.Whatsapp = dto.Whatsapp.Trim();
            location.Latitude = dto.Coordinates?.Lat ?? 0;
            location.Longitude = dto.Coordinates?.Lng ?? 0;
            location.OpeningHours = dto.OpeningHours;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Location updated successfully." });
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteLocation(Guid id)
        {
            var loc = await _context.Locations.FirstOrDefaultAsync(x => x.Id == id);

            if (loc == null)
            {
                return NotFound(new { success = false, message = "Location not found." });
            }

            _context.Locations.Remove(loc);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Location deleted successfully." });
        }
    }
}