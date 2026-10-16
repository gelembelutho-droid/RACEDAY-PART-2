using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get the logged-in user's profile
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Participant";

            return Ok(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = role,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Phone = user.Phone,
                EmergencyContact = user.EmergencyContact,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Update the logged-in user's profile
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.EmergencyContact != null) user.EmergencyContact = dto.EmergencyContact;
            if (dto.Gender != null) user.Gender = dto.Gender;
            if (dto.DateOfBirth.HasValue) user.DateOfBirth = dto.DateOfBirth;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
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