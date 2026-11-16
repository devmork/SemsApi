namespace SemsApi.Models
{
    /// <summary>
    /// A question. Its evaluation type comes through its category.
    /// </summary>
    public class EvaluationQuestion
    {
        /// <summary>Primary key, database-generated. Unique across ALL categories (the "QuestionNumber").</summary>
        public int QnNo { get; set; }

        /// <summary>FK to EvaluationCategory.CatNo.</summary>
        public int CatNo { get; set; }
        public EvaluationCategory Category { get; set; } = null!;

        public string? QnName { get; set; }
    }
}