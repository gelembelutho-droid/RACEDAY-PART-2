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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all events (public)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<EventDto>), 200)]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Races)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    City = e.City,
                    Country = e.Country,
                    Description = e.Description,
                    EventType = e.EventType,
                    OrganizerId = e.OrganizerId,
                    OrganizerName = e.Organizer != null ? e.Organizer.FullName : null,
                    CreatedAt = e.CreatedAt,
                    Races = e.Races.Select(r => new RaceDto
                    {
                        RaceId = r.RaceId,
                        EventId = r.EventId,
                        RaceName = r.RaceName,
                        DistanceKm = r.DistanceKm,
                        StartTime = r.StartTime,
                        EndTime = r.EndTime,
                        AgeGroup = r.AgeGroup,
                        MaxParticipants = r.MaxParticipants
                    }).ToList()
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Get a specific event by ID (public)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EventDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Races)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
                return NotFound(new { message = "Event not found" });

            return Ok(new EventDto
            {
                EventId = eventEntity.EventId,
                EventName = eventEntity.EventName,
                EventDate = eventEntity.EventDate,
                Location = eventEntity.Location,
                City = eventEntity.City,
                Country = eventEntity.Country,
                Description = eventEntity.Description,
                EventType = eventEntity.EventType,
                OrganizerId = eventEntity.OrganizerId,
                OrganizerName = eventEntity.Organizer?.FullName,
                CreatedAt = eventEntity.CreatedAt,
                Races = eventEntity.Races.Select(r => new RaceDto
                {
                    RaceId = r.RaceId,
                    EventId = r.EventId,
                    RaceName = r.RaceName,
                    DistanceKm = r.DistanceKm,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    AgeGroup = r.AgeGroup,
                    MaxParticipants = r.MaxParticipants
                }).ToList()
            });
        }

        /// <summary>
        /// Create a new event (Organiser only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(EventDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = GetCurrentUserId();

            var eventEntity = new Event
            {
                OrganizerId = userId,
                EventName = dto.EventName,
                EventDate = dto.EventDate,
                Location = dto.Location,
                City = dto.City,
                Country = dto.Country,
                Description = dto.Description,
                EventType = dto.EventType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var result = new EventDto
            {
                EventId = eventEntity.EventId,
                EventName = eventEntity.EventName,
                EventDate = eventEntity.EventDate,
                Location = eventEntity.Location,
                City = eventEntity.City,
                Country = eventEntity.Country,
                Description = eventEntity.Description,
                EventType = eventEntity.EventType,
                OrganizerId = eventEntity.OrganizerId,
                CreatedAt = eventEntity.CreatedAt
            };

            return CreatedAtAction(nameof(GetEvent), new { id = eventEntity.EventId }, result);
        }

        /// <summary>
        /// Update an event (Organiser only, must own the event)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var userId = GetCurrentUserId();

            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found" });

            if (eventEntity.OrganizerId != userId)
                return Forbid();

            if (dto.EventName != null) eventEntity.EventName = dto.EventName;
            if (dto.EventDate.HasValue) eventEntity.EventDate = dto.EventDate.Value;
            if (dto.Location != null) eventEntity.Location = dto.Location;
            if (dto.City != null) eventEntity.City = dto.City;
            if (dto.Country != null) eventEntity.Country = dto.Country;
            if (dto.Description != null) eventEntity.Description = dto.Description;
            if (dto.EventType != null) eventEntity.EventType = dto.EventType;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated successfully" });
        }

        /// <summary>
        /// Delete an event (Organiser only, must own the event)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(204)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetCurrentUserId();

            var eventEntity = await _context.Events
                .Include(e => e.Races)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
                return NotFound(new { message = "Event not found" });

            if (eventEntity.OrganizerId != userId)
                return Forbid();

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get all events created by the logged-in Organiser
        /// </summary>
        [HttpGet("my-events")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(List<EventDto>), 200)]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = GetCurrentUserId();

            var events = await _context.Events
                .Where(e => e.OrganizerId == userId)
                .Include(e => e.Races)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    City = e.City,
                    Country = e.Country,
                    Description = e.Description,
                    EventType = e.EventType,
                    OrganizerId = e.OrganizerId,
                    CreatedAt = e.CreatedAt,
                    Races = e.Races.Select(r => new RaceDto
                    {
                        RaceId = r.RaceId,
                        EventId = r.EventId,
                        RaceName = r.RaceName,
                        DistanceKm = r.DistanceKm,
                        StartTime = r.StartTime,
                        EndTime = r.EndTime,
                        AgeGroup = r.AgeGroup,
                        MaxParticipants = r.MaxParticipants
                    }).ToList()
                })
                .ToListAsync();

            return Ok(events);
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