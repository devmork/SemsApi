namespace SemsApi.DTO.TeacherSubjects
{
    public class TeacherStudentDto
    {
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int SubjectOfferingId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectDescription { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
    }
}