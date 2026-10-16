using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [StringLength(1)]
        public string? Gender { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? EmergencyContact { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}