namespace SemsApi.DTO
{
    public class CategoryResponseDto
    {
        public int Recno { get; set; }
        public int FormId { get; set; }
        public string FormCode { get; set; } = string.Empty;
        public int EvaluationTypeId { get; set; }
        public string EvaluationTypeName { get; set; } = string.Empty;
        public int? CatNo { get; set; }
        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
    }
}