namespace SemsApi.DTO.EvaluationSessions
{
    public class EvaluationSessionCreateRequest
    {
        public int FormId { get; set; }
        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public int? GradeLevelFrom { get; set; }
        public int? GradeLevelTo { get; set; }
        public DateTime? OpensAtUtc { get; set; }
        public DateTime? ClosesAtUtc { get; set; }
        public string? Notes { get; set; }
    }
}
