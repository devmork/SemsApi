namespace SemsApi.DTO
{
    /// <summary>
    /// Response shape for category CRUD operations.
    /// CategoryId is the category's unique id, used for update/delete and by its questions.
    /// </summary>
    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public int EvaluationTypeId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? CategoryRate { get; set; }
    }
}
