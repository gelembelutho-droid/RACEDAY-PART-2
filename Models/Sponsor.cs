using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Sponsor
    {
        [Key]
        public int SponsorId { get; set; }

        [Required]
        [StringLength(100)]
        public string SponsorName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(100)]
        public string? ContactEmail { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
    }
}