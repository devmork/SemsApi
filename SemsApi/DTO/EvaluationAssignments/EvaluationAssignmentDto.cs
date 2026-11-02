namespace SemsApi.DTO.EvaluationAssignments
{
    public class EvaluationAssignmentDto
    {
        public int SubjectOfferingId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectDescription { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherEmployeeNumber { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public string Section { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public int? EvaluationTypeId { get; set; }
        public int? FormId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? SubmittedAtUtc { get; set; }
    }
}