namespace SemsApi.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public string StudentNumber { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        public ICollection<StudentSubjectEnrollment> SubjectEnrollments { get; set; } = new List<StudentSubjectEnrollment>();
        public ICollection<StudentTermEnrollment> TermEnrollments { get; set; } = new List<StudentTermEnrollment>();
        public ICollection<TeacherEvaluationSubmission> EvaluationSubmissions { get; set; } = new List<TeacherEvaluationSubmission>();
    }
}
