namespace SemsApi.DTO
{
    /// <summary>
    /// Request payload for creating or updating a EvaluationCategory.
    /// CategoryId is intentionally omitted - it is database-generated and unique across all evaluation types.
    /// </summary>
    public class CategoryRequestDto
    {
        public int? EvaluationTypeId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? CategoryRate { get; set; }
    }
}
