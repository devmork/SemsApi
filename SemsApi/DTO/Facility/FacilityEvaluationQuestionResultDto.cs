namespace SemsApi.DTO.Facility
{
    public class FacilityEvaluationQuestionResultDto
    {
        public int QnNo { get; set; }
        public int CatNo { get; set; }
        public string? QnName { get; set; }
        public int ResponseCount { get; set; }
        public decimal? AverageRating { get; set; }
    }
}
