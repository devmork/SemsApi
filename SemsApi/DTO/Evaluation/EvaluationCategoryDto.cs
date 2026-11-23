namespace SemsApi.DTO.Evaluation
{
    public class EvaluationCategoryDto //Purpose: Category + nested questions used by /EvaluationForm
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? CategoryRate { get; set; }
        public List<EvaluationQuestionDto> Questions { get; set; } = new();
    }
}
