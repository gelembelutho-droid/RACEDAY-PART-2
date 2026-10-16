namespace RaceDay.API.DTOs
{
    public class ResultDto
    {
        public int ResultId { get; set; }
        public int RegistrationId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string RaceName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public TimeSpan? FinishTime { get; set; }
        public int? OverallRank { get; set; }
        public int? AgeGroupRank { get; set; }
        public decimal? PacePerKm { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateResultDto
    {
        public TimeSpan? FinishTime { get; set; }
        public int? OverallRank { get; set; }
        public int? AgeGroupRank { get; set; }
        public decimal? PacePerKm { get; set; }
        public string Status { get; set; } = "finished";
    }
}