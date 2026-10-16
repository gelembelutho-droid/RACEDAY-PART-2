using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganizerId { get; set; }

        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        public string? Description { get; set; }

        [StringLength(20)]
        public string? EventType { get; set; } // Run, Walk, Cycle

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrganizerId")]
        public User? Organizer { get; set; }
        public ICollection<Race> Races { get; set; } = new List<Race>();
        public ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
    }
}
