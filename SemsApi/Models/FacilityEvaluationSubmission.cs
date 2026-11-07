using SemsApi.Models.Eval;

namespace SemsApi.Models
{
    /// <summary>
    /// One submission = one student's evaluation of one facility in one session.
    /// Uniqueness: (StudentId, FacilityId, SchoolYear, Term).
    /// </summary>
    public class FacilityEvaluationSubmission
    {
        public int SubmissionId { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int FacilityId { get; set; }
        public Facility Facility { get; set; } = null!;

        public int EvaluationTypeId { get; set; }
        public EvaluationType EvaluationType { get; set; } = null!;

        public int? FormId { get; set; }
        public Form? Form { get; set; }

        public int? EvaluationSessionId { get; set; }
        public EvaluationSession? EvaluationSession { get; set; }

        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<FacilityEvaluationAnswer> Answers { get; set; }
            = new List<FacilityEvaluationAnswer>();
    }
}