namespace SemsApi.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public string EmployeeNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        public ICollection<SubjectOffering> SubjectOfferings { get; set; } = new List<SubjectOffering>();
        public ICollection<TeacherEvaluationSubmission> EvaluationSubmissions { get; set; } = new List<TeacherEvaluationSubmission>();
    }
}
