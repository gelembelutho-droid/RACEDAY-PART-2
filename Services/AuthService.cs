using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly RaceDayDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(RaceDayDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Check if email exists
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("Email already exists");

            // Validate role
            if (dto.Role != "Organiser" && dto.Role != "Participant")
                throw new Exception("Role must be 'Organiser' or 'Participant'");

            // Hash password - FIXED: Using fully qualified BCrypt.Net.BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Create user
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = passwordHash,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Phone = dto.Phone,
                EmergencyContact = dto.EmergencyContact,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // FIXED: Properly await and handle SaveChangesAsync
            await _context.SaveChangesAsync();

            // Generate token
            var token = _jwtService.GenerateToken(user, dto.Role);

            return new AuthResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = dto.Role,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    EmergencyContact = user.EmergencyContact,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new Exception("Invalid credentials");

            // FIXED: Using fully qualified BCrypt.Net.BCrypt
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            // Determine role - first check if user has organized events
            var isOrganizer = await _context.Events.AnyAsync(e => e.OrganizerId == user.UserId);
            var role = isOrganizer ? "Organiser" : "Participant";

            // If user has no events, check if they have registrations
            if (!isOrganizer)
            {
                var hasRegistrations = await _context.Registrations.AnyAsync(r => r.UserId == user.UserId);
                if (!hasRegistrations)
                    role = "Participant"; // Default
            }

            var token = _jwtService.GenerateToken(user, role);

            return new AuthResponseDto
            {
                Token = token,
                User = new UserDto
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
                }
            };
        }
    }
}