using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class EventSponsor
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int SponsorId { get; set; }

        [StringLength(50)]
        public string? SponsorshipLevel { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [ForeignKey("SponsorId")]
        public Sponsor? Sponsor { get; set; }
    }
}