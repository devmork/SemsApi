namespace SemsApi.DTO
{
    /// <summary>
    /// Response shape for category CRUD operations.
    /// CatNo is the category's unique id, used for update/delete and by its questions.
    /// </summary>
    public class CategoryResponseDto
    {
        public int CatNo { get; set; }
        public int EvaluationTypeId { get; set; }
        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
    }
}