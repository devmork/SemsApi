using SemsApi.Models.Eval;

namespace SemsApi.Models
{
    /// <summary>
    /// A QA-managed, time-bounded instance of a Form for a specific SchoolYear + Term.
    /// The Form remains the reusable template; the session controls when and for whom
    /// that template becomes an actionable evaluation opportunity.
    /// </summary>
    public class EvaluationSession
    {
        public int EvaluationSessionId { get; set; }

        public int FormId { get; set; }
        public Form Form { get; set; } = null!;

        /// <summary>School year, e.g. "2025-2026".</summary>
        public string SchoolYear { get; set; } = string.Empty;

        /// <summary>Term number, e.g. 1, 2, 3.</summary>
        public int Term { get; set; }

        /// <summary>Optional UTC opening time. Null means no lower bound.</summary>
        public DateTime? OpensAtUtc { get; set; }

        /// <summary>Optional UTC closing time. Null means no upper bound.</summary>
        public DateTime? ClosesAtUtc { get; set; }

        public EvaluationSessionStatus Status { get; set; } = EvaluationSessionStatus.Draft;

        /// <summary>Optional QA notes / instructions.</summary>
        public string? Notes { get; set; }

        public int CreatedBy { get; set; }
        public ApplicationUser Creator { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<TeacherEvaluationSubmission> Submissions { get; set; }
            = new List<TeacherEvaluationSubmission>();
    }
}
