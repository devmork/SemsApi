namespace SemsApi.Models
{
    public class EvaluationQuestion
    {
        public int QuestionId { get; set; }
        public int CategoryId { get; set; }
        public string? QuestionName { get; set; }

        // REFERENCE TYPE
        public EvaluationCategory Category { get; set; } = null!;

    }
}