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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get the logged-in participant's race history
        /// </summary>
        [HttpGet("my-results")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(typeof(List<ResultDto>), 200)]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = GetCurrentUserId();

            var results = await _context.Results
                .Include(r => r.Registration)
                    .ThenInclude(reg => reg!.User)
                .Include(r => r.Registration)
                    .ThenInclude(reg => reg!.Race)
                        .ThenInclude(ra => ra!.Event)
                .Where(r => r.Registration != null && r.Registration.UserId == userId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    RegistrationId = r.RegistrationId,
                    ParticipantName = r.Registration != null && r.Registration.User != null
                        ? r.Registration.User.FullName : "",
                    RaceName = r.Registration != null && r.Registration.Race != null
                        ? r.Registration.Race.RaceName : "",
                    EventName = r.Registration != null && r.Registration.Race != null
                        && r.Registration.Race.Event != null
                        ? r.Registration.Race.Event.EventName : "",
                    FinishTime = r.FinishTime,
                    OverallRank = r.OverallRank,
                    AgeGroupRank = r.AgeGroupRank,
                    PacePerKm = r.PacePerKm,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Get all results for a specific event (Organiser only)
        /// </summary>
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(List<ResultDto>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var userId = GetCurrentUserId();

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null)
                return NotFound(new { message = "Event not found" });

            if (eventEntity.OrganizerId != userId)
                return Forbid();

            var results = await _context.Results
                .Include(r => r.Registration)
                    .ThenInclude(reg => reg!.User)
                .Include(r => r.Registration)
                    .ThenInclude(reg => reg!.Race)
                        .ThenInclude(ra => ra!.Event)
                .Where(r => r.Registration != null && r.Registration.Race != null
                    && r.Registration.Race.EventId == eventId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    RegistrationId = r.RegistrationId,
                    ParticipantName = r.Registration != null && r.Registration.User != null
                        ? r.Registration.User.FullName : "",
                    RaceName = r.Registration != null && r.Registration.Race != null
                        ? r.Registration.Race.RaceName : "",
                    EventName = r.Registration != null && r.Registration.Race != null
                        && r.Registration.Race.Event != null
                        ? r.Registration.Race.Event.EventName : "",
                    FinishTime = r.FinishTime,
                    OverallRank = r.OverallRank,
                    AgeGroupRank = r.AgeGroupRank,
                    PacePerKm = r.PacePerKm,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Submit a result for a registration (Organiser only)
        /// </summary>
        [HttpPost("registration/{registrationId}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(ResultDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SubmitResult(int registrationId, [FromBody] CreateResultDto dto)
        {
            var userId = GetCurrentUserId();

            var registration = await _context.Registrations
                .Include(r => r.Race)
                    .ThenInclude(ra => ra!.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

            if (registration == null)
                return NotFound(new { message = "Registration not found" });

            if (registration.Race?.Event?.OrganizerId != userId)
                return Forbid();

            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

            if (existingResult != null)
                return BadRequest(new { message = "Result already exists for this registration" });

            var result = new Result
            {
                RegistrationId = registrationId,
                FinishTime = dto.FinishTime,
                OverallRank = dto.OverallRank,
                AgeGroupRank = dto.AgeGroupRank,
                PacePerKm = dto.PacePerKm,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new ResultDto
            {
                ResultId = result.ResultId,
                RegistrationId = result.RegistrationId,
                ParticipantName = registration.User?.FullName ?? "",
                RaceName = registration.Race?.RaceName ?? "",
                EventName = registration.Race?.Event?.EventName ?? "",
                FinishTime = result.FinishTime,
                OverallRank = result.OverallRank,
                AgeGroupRank = result.AgeGroupRank,
                PacePerKm = result.PacePerKm,
                Status = result.Status
            });
        }

        /// <summary>
        /// Update a result (Organiser only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] CreateResultDto dto)
        {
            var userId = GetCurrentUserId();

            var result = await _context.Results
                .Include(r => r.Registration)
                    .ThenInclude(reg => reg!.Race)
                        .ThenInclude(ra => ra!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = "Result not found" });

            if (result.Registration?.Race?.Event?.OrganizerId != userId)
                return Forbid();

            if (dto.FinishTime.HasValue) result.FinishTime = dto.FinishTime;
            if (dto.OverallRank.HasValue) result.OverallRank = dto.OverallRank;
            if (dto.AgeGroupRank.HasValue) result.AgeGroupRank = dto.AgeGroupRank;
            if (dto.PacePerKm.HasValue) result.PacePerKm = dto.PacePerKm;
            if (dto.Status != null) result.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Result updated successfully" });
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