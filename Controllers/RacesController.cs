using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RacesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public RacesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all races for a specific event (public)
        /// </summary>
        [HttpGet("event/{eventId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RaceDto>), 200)]
        public async Task<IActionResult> GetRacesByEvent(int eventId)
        {
            var races = await _context.Races
                .Where(r => r.EventId == eventId)
                .Select(r => new RaceDto
                {
                    RaceId = r.RaceId,
                    EventId = r.EventId,
                    RaceName = r.RaceName,
                    DistanceKm = r.DistanceKm,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    AgeGroup = r.AgeGroup,
                    MaxParticipants = r.MaxParticipants
                })
                .ToListAsync();

            return Ok(races);
        }

        /// <summary>
        /// Get a specific race by ID (public)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RaceDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRace(int id)
        {
            var race = await _context.Races.FindAsync(id);
            if (race == null)
                return NotFound(new { message = "Race not found" });

            return Ok(new RaceDto
            {
                RaceId = race.RaceId,
                EventId = race.EventId,
                RaceName = race.RaceName,
                DistanceKm = race.DistanceKm,
                StartTime = race.StartTime,
                EndTime = race.EndTime,
                AgeGroup = race.AgeGroup,
                MaxParticipants = race.MaxParticipants
            });
        }

        /// <summary>
        /// Create a new race/category (Organiser only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(RaceDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateRace([FromBody] CreateRaceDto dto)
        {
            var userId = GetCurrentUserId();

            // Verify the organiser owns the event
            var eventEntity = await _context.Events.FindAsync(dto.EventId);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found" });

            if (eventEntity.OrganizerId != userId)
                return Forbid("You can only add races to your own events");

            var race = new Race
            {
                EventId = dto.EventId,
                RaceName = dto.RaceName,
                DistanceKm = dto.DistanceKm,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                AgeGroup = dto.AgeGroup,
                MaxParticipants = dto.MaxParticipants,
                CreatedAt = DateTime.UtcNow
            };

            _context.Races.Add(race);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRace), new { id = race.RaceId }, new RaceDto
            {
                RaceId = race.RaceId,
                EventId = race.EventId,
                RaceName = race.RaceName,
                DistanceKm = race.DistanceKm,
                StartTime = race.StartTime,
                EndTime = race.EndTime,
                AgeGroup = race.AgeGroup,
                MaxParticipants = race.MaxParticipants
            });
        }

        /// <summary>
        /// Update a race (Organiser only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateRace(int id, [FromBody] UpdateRaceDto dto)
        {
            var userId = GetCurrentUserId();

            var race = await _context.Races
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RaceId == id);

            if (race == null)
                return NotFound(new { message = "Race not found" });

            if (race.Event?.OrganizerId != userId)
                return Forbid("You can only update races for your own events");

            if (dto.RaceName != null) race.RaceName = dto.RaceName;
            if (dto.DistanceKm.HasValue) race.DistanceKm = dto.DistanceKm.Value;
            if (dto.StartTime.HasValue) race.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue) race.EndTime = dto.EndTime;
            if (dto.AgeGroup != null) race.AgeGroup = dto.AgeGroup;
            if (dto.MaxParticipants.HasValue) race.MaxParticipants = dto.MaxParticipants;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Race updated successfully" });
        }

        /// <summary>
        /// Delete a race (Organiser only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(204)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRace(int id)
        {
            var userId = GetCurrentUserId();

            var race = await _context.Races
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RaceId == id);

            if (race == null)
                return NotFound(new { message = "Race not found" });

            if (race.Event?.OrganizerId != userId)
                return Forbid("You can only delete races from your own events");

            _context.Races.Remove(race);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? "0";

            return int.Parse(userIdClaim);
        }
    }
}