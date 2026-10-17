namespace SemsApi.DTO
{
    public class CategoryRequestDto
    {
        public int? FormId { get; set; }
        public int? EvaluationTypeId { get; set; }
        public string? EvaluationTypeName { get; set; }
        public int? CatNo { get; set; }
        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
    }
}