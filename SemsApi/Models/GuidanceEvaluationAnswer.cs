namespace SemsApi.Models
{
    public class GuidanceEvaluationAnswer
    {
        public int Recno { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public int? CatNo { get; set; }
        public int? QnNo { get; set; }
        public string? Sy { get; set; }
        public int? Term { get; set; }
        public int? RateVal { get; set; }
    }
}
