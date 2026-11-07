namespace SemsApi.DTO.Facility
{
    public class FacilityEvaluationAssignmentDto
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }

        public int EvaluationTypeId { get; set; }
        public int FormId { get; set; }
        public int EvaluationSessionId { get; set; }
        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? SubmittedAtUtc { get; set; }
    }
}