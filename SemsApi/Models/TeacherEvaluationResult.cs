namespace SemsApi.Models
{
    /// <summary>
    /// A question. Its evaluation type comes through its category.
    /// </summary>
    public class TeacherEvaluationResult
    {
        /// <summary>Primary key, database-generated. Unique across ALL categories (the "QuestionNumber").</summary>
        public int QnNo { get; set; }

        /// <summary>FK to TeacherEvaluationCategory.CatNo.</summary>
        public int CatNo { get; set; }
        public TeacherEvaluationCategory Category { get; set; } = null!;

        public string? QnName { get; set; }
    }
}