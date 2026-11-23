namespace SemsApi.DTO.EvaluationSessions
{
    public class EvaluationSessionCreateRequest
    {
        public int FormId { get; set; }

        /// <summary>
        /// Required when the form's evaluation type is a facility evaluation. Null for
        /// teacher/subject evaluation sessions.
        /// </summary>
        public int? FacilityId { get; set; }

        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public int? GradeLevelFrom { get; set; }
        public int? GradeLevelTo { get; set; }
        public DateTime? OpensAtUtc { get; set; }
        public DateTime? ClosesAtUtc { get; set; }
        public string? Notes { get; set; }
    }
}
