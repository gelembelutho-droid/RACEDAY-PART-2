using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }
        public string? EventType { get; set; }
        public int OrganizerId { get; set; }
        public string? OrganizerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RaceDto> Races { get; set; } = new();
    }

    public class CreateEventDto
    {
        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        public string? Location { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }

        [Required]
        public string EventType { get; set; } = "Run"; // Run, Walk, Cycle
    }

    public class UpdateEventDto
    {
        public string? EventName { get; set; }
        public DateTime? EventDate { get; set; }
        public string? Location { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }
        public string? EventType { get; set; }
    }
}