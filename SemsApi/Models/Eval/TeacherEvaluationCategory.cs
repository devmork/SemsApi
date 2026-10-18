namespace SemsApi.Models.Eval
{
    public class TeacherEvaluationCategory
    {
        /// <summary>Primary key, database-generated. Unique across ALL evaluation types (the "CategoryNumber").</summary>
        public int CatNo { get; set; }

        public int EvaluationTypeId { get; set; }
        public EvaluationType EvaluationType { get; set; } = null!;

        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
    }
}