namespace SemsApi.DTO.Facility
{
    public class FacilityEvaluationCategoryResultDto
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? CategoryRate { get; set; }
        public int ResponseCount { get; set; }
        public decimal? AverageRating { get; set; }
    }
}
