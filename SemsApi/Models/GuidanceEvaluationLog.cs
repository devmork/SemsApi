namespace SemsApi.Models
{
    public class GuidanceEvaluationLog
    {
        public int Recno { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public string? Sy { get; set; }
        public int? Term { get; set; }
        public DateTime? DateEvaluated { get; set; }
    }
}
