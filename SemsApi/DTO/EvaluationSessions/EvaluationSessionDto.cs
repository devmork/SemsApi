namespace SemsApi.DTO.EvaluationSessions
{
    public class EvaluationSessionDto
    {
        public int EvaluationSessionId { get; set; }

        public int FormId { get; set; }
        public string FormCode { get; set; } = string.Empty;
        public string FormTitle { get; set; } = string.Empty;

        public int EvaluationTypeId { get; set; }
        public string EvaluationTypeName { get; set; } = string.Empty;

        public string TargetType { get; set; } = string.Empty;
        public int ProgramId { get; set; }

        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }

        public DateTime? OpensAtUtc { get; set; }
        public DateTime? ClosesAtUtc { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public int SubmissionCount { get; set; }
        public bool IsAvailableNow { get; set; }
    }
}
