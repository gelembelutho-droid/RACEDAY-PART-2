using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int RegistrationId { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? OverallRank { get; set; }

        public int? AgeGroupRank { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? PacePerKm { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "DNS";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("RegistrationId")]
        public Registration? Registration { get; set; }
    }
}