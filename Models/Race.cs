using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Race
    {
        [Key]
        public int RaceId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string RaceName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DistanceKm { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [StringLength(50)]
        public string? AgeGroup { get; set; }

        public int? MaxParticipants { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("EventId")]
        public Event? Event { get; set; }
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}