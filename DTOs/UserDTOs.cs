namespace RaceDay.API.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? EmergencyContact { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}