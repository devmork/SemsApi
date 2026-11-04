namespace SemsApi.DTO.EvaluationSessions
{
    public class SessionSubmissionDto
    {
        public int SubmissionId { get; set; }

        // Student Info
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;

        // Subject & Teacher Info
        public string SubjectCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;

        // Metadata
        public DateTime SubmittedAtUtc { get; set; }
    }
}