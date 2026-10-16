namespace RaceDay.API.DTOs
{
    public class RegistrationDto
    {
        public int RegistrationId { get; set; }
        public int UserId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int RaceId { get; set; }
        public string RaceName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? BibNumber { get; set; }
    }

    public class CreateRegistrationDto
    {
        public int RaceId { get; set; }
    }

    public class UpdatePaymentDto
    {
        public string PaymentStatus { get; set; } = "Paid";
    }
}