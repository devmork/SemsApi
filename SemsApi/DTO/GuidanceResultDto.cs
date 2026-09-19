namespace SemsApi.DTO
{
    public class GuidanceResultDto //Purpose: Returning saved guidance evaluation results
    {
        public int Recno { get; set; }
        public string? StudentId { get; set; }
        public int? CatNo { get; set; }
        public int? QnNo { get; set; }
        public string? Sy { get; set; }
        public int? Term { get; set; }
        public int? RateVal { get; set; }
    }
}
