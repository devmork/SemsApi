namespace SemsApi.DTO.TeacherSubjects
{
    public class TeacherSubjectDto
    {
        public int SubjectOfferingId { get; set; }
        public string EdpCode { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectDescription { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public string Section { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public int EnrolledStudentCount { get; set; }
    }
}