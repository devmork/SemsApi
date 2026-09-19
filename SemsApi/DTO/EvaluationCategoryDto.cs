namespace SemsApi.DTO
{
    public class EvaluationCategoryDto //Purpose: Category + nested questions used by /form
    {
        public int CatNo { get; set; }
        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
        public List<EvaluationQuestionDto> Questions { get; set; } = new();
    }
}
