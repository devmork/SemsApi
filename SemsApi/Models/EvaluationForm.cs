namespace SemsApi.Models.Eval
{
    /// <summary>
    /// A reusable evaluation template (e.g. the Teacher Evaluation template for Grade 4 to 6,
    /// or the Canteen Evaluation template). Its categories and questions are authored per form;
    /// targeting (which students, which term) lives on <c>EvaluationSession</c>, not here.
    /// </summary>
    public class EvaluationForm
    {
        public int FormId { get; set; }

        /// <summary>FK to EvaluationType - decides the kind of evaluation (subject teacher vs facility).</summary>
        public int EvaluationTypeId { get; set; }
        public EvaluationType EvaluationType { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>FK to Users (AspNetUsers.Id) - the user who created the EvaluationForm.</summary>
        public int CreatedBy { get; set; }
        public ApplicationUser Creator { get; set; } = null!;

        /// <summary>1 = active, 0 = inactive/draft.</summary>
        public int IsActive { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
