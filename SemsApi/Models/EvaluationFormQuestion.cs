namespace SemsApi.Models.Eval
{
    /// <summary>
    /// Form-scoped question. Replaces EvaluationQuestion so each EvaluationForm authors its own
    /// questions under shared categories instead of inheriting them from an EvaluationType.
    /// </summary>
    public class EvaluationFormQuestion
    {
        public int FormQuestionId { get; set; }

        /// <summary>FK to EvaluationFormCategory.FormCategoryId</summary>
        public int FormCategoryId { get; set; }
        public EvaluationFormCategory FormCategory { get; set; } = null!;

        public string? QuestionName { get; set; }
        public int DisplayOrder { get; set; }
        public decimal? Points { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
