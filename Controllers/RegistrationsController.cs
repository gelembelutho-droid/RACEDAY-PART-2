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
    [Authorize]
    public class RegistrationsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public RegistrationsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all registrations for the logged-in participant
        /// </summary>
        [HttpGet("my-registrations")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(typeof(List<RegistrationDto>), 200)]
        public async Task<IActionResult> GetMyRegistrations()
        {
            var userId = GetCurrentUserId();

            var registrations = await _context.Registrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Race)
                    .ThenInclude(ra => ra!.Event)
                .Include(r => r.User)
                .Select(r => new RegistrationDto
                {
                    RegistrationId = r.RegistrationId,
                    UserId = r.UserId,
                    ParticipantName = r.User != null ? r.User.FullName : "",
                    RaceId = r.RaceId,
                    RaceName = r.Race != null ? r.Race.RaceName : "",
                    EventName = r.Race != null && r.Race.Event != null ? r.Race.Event.EventName : "",
                    RegistrationDate = r.RegistrationDate,
                    Status = r.Status,
                    BibNumber = r.BibNumber
                })
                .ToListAsync();

            return Ok(registrations);
        }

        /// <summary>
        /// Get all registrations for a specific event (Organiser only)
        /// </summary>
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(List<RegistrationDto>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetEventRegistrations(int eventId)
        {
            var userId = GetCurrentUserId();

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found" });

            if (eventEntity.OrganizerId != userId)
                return Forbid();

            var registrations = await _context.Registrations
                .Where(r => r.Race != null && r.Race.EventId == eventId)
                .Include(r => r.Race)
                    .ThenInclude(ra => ra!.Event)
                .Include(r => r.User)
                .Select(r => new RegistrationDto
                {
                    RegistrationId = r.RegistrationId,
                    UserId = r.UserId,
                    ParticipantName = r.User != null ? r.User.FullName : "",
                    RaceId = r.RaceId,
                    RaceName = r.Race != null ? r.Race.RaceName : "",
                    EventName = r.Race != null && r.Race.Event != null ? r.Race.Event.EventName : "",
                    RegistrationDate = r.RegistrationDate,
                    Status = r.Status,
                    BibNumber = r.BibNumber
                })
                .ToListAsync();

            return Ok(registrations);
        }

        /// <summary>
        /// Enrol in a race (Participant only)
        /// </summary>
        [HttpPost("enrol")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(typeof(RegistrationDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Enrol([FromBody] CreateRegistrationDto dto)
        {
            var userId = GetCurrentUserId();

            var race = await _context.Races
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RaceId == dto.RaceId);

            if (race == null)
                return NotFound(new { message = "Race not found" });

            var existing = await _context.Registrations
                .AnyAsync(r => r.UserId == userId && r.RaceId == dto.RaceId);

            if (existing)
                return BadRequest(new { message = "You are already enrolled in this race" });

            if (race.MaxParticipants.HasValue)
            {
                var currentCount = await _context.Registrations
                    .CountAsync(r => r.RaceId == dto.RaceId && r.Status != "cancelled");

                if (currentCount >= race.MaxParticipants.Value)
                    return BadRequest(new { message = "Race is full" });
            }

            var bibNumber = $"BIB{DateTime.UtcNow.Ticks.ToString().Substring(10)}";

            var registration = new Registration
            {
                UserId = userId,
                RaceId = dto.RaceId,
                RegistrationDate = DateTime.UtcNow,
                Status = "confirmed",
                BibNumber = bibNumber,
                CreatedAt = DateTime.UtcNow
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return StatusCode(201, new RegistrationDto
            {
                RegistrationId = registration.RegistrationId,
                UserId = registration.UserId,
                RaceId = registration.RaceId,
                RaceName = race.RaceName,
                EventName = race.Event?.EventName ?? "",
                RegistrationDate = registration.RegistrationDate,
                Status = registration.Status,
                BibNumber = registration.BibNumber
            });
        }

        /// <summary>
        /// Cancel a registration (Participant only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(204)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            var userId = GetCurrentUserId();

            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
                return NotFound(new { message = "Registration not found" });

            if (registration.UserId != userId)
                return Forbid();

            registration.Status = "cancelled";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Update payment status for a registration (Participant only)
        /// </summary>
        [HttpPut("{id}/payment")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentDto dto)
        {
            var userId = GetCurrentUserId();

            var registration = await _context.Registrations
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);

            if (registration == null)
                return NotFound(new { message = "Registration not found" });

            if (registration.UserId != userId)
                return Forbid();

            if (registration.Payment == null)
            {
                registration.Payment = new Payment
                {
                    RegistrationId = registration.RegistrationId,
                    Amount = 0,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = "credit_card",
                    TransactionId = $"TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(registration.Payment);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment updated successfully" });
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