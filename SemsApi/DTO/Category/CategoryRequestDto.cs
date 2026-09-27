namespace SemsApi.DTO
{
    /// <summary>
    /// Request payload for creating or updating a TeacherEvaluationCategory.
    /// Recno is intentionally omitted — it is database-generated.
    /// </summary>
    public class CategoryRequestDto
    {
        public string? EvalType { get; set; }
        public int? CatNo { get; set; }
        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
    }
}