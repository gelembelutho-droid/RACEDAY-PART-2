using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Registration
    {
        [Key]
        public int RegistrationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RaceId { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        [StringLength(20)]
        public string? BibNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("RaceId")]
        public Race? Race { get; set; }

        public Result? Result { get; set; }
        public Payment? Payment { get; set; }
    }
}