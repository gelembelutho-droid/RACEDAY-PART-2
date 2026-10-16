using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class RaceDto
    {
        public int RaceId { get; set; }
        public int EventId { get; set; }
        public string RaceName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? AgeGroup { get; set; }
        public int? MaxParticipants { get; set; }
    }

    public class CreateRaceDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string RaceName { get; set; } = string.Empty;

        [Required]
        public decimal DistanceKm { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
        public string? AgeGroup { get; set; }
        public int? MaxParticipants { get; set; }
    }

    public class UpdateRaceDto
    {
        public string? RaceName { get; set; }
        public decimal? DistanceKm { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? AgeGroup { get; set; }
        public int? MaxParticipants { get; set; }
    }
}