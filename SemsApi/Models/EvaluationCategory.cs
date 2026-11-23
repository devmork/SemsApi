namespace SemsApi.Models
{
    public class EvaluationCategory
    {
        public int CategoryId { get; set; }
        public int EvaluationTypeId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? CategoryRate { get; set; }

        // REFERENCE TYPE
        public EvaluationType EvaluationType { get; set; } = null!;
    }
}
