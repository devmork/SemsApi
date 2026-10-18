namespace SemsApi.DTO
{
    /// <summary>
    /// Request payload for creating or updating a TeacherEvaluationCategory.
    /// CatNo is intentionally omitted - it is database-generated and unique across all evaluation types.
    /// </summary>
    public class CategoryRequestDto
    {
        public int? EvaluationTypeId { get; set; }
        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
    }
}