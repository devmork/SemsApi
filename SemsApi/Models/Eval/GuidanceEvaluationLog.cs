namespace SemsApi.Models.Eval
{
    public class GuidanceEvaluationLog
    {
        public int Recno { get; set; }
        public string? StudentId { get; set; }
        public string? Sy { get; set; }
        public int? Term { get; set; }
        public DateTime? DateEvaluated { get; set; }
    }
}
