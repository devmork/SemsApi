namespace SemsApi.DTO.Facility
{
    public class FacilityEvaluationCategoryResultDto
    {
        public int CatNo { get; set; }
        public string? CatName { get; set; }
        public string? CatRn { get; set; }
        public decimal? CatRate { get; set; }
        public int ResponseCount { get; set; }
        public decimal? AverageRating { get; set; }
    }
}
