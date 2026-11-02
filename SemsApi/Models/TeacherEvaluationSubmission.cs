using SemsApi.Models.Eval;

namespace SemsApi.Models
{
    /// <summary>
    /// One submission = the student's evaluation of a specific teacher for a specific subject offering.
    /// Uniqueness is (StudentId, SubjectOfferingId) — a student who has the same teacher in 2 subjects
    /// will have 2 separate submissions.
    /// </summary>
    public class TeacherEvaluationSubmission
    {
        public int SubmissionId { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int SubjectOfferingId { get; set; }
        public SubjectOffering SubjectOffering { get; set; } = null!;

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;

        public int EvaluationTypeId { get; set; }
        public EvaluationType EvaluationType { get; set; } = null!;

        public int? FormId { get; set; }
        public Form? Form { get; set; }

        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<TeacherEvaluationAnswer> Answers { get; set; } = new List<TeacherEvaluationAnswer>();
    }
}