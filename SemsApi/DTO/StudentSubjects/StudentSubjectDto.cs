namespace SemsApi.DTO.StudentSubjects
{
    public class StudentSubjectDto
    {
        public int EnrollmentId { get; set; }
        public int SubjectOfferingId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectDescription { get; set; } = string.Empty;
        public string EdpCode { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public string Section { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public string? TeacherEmployeeNumber { get; set; }
    }
}