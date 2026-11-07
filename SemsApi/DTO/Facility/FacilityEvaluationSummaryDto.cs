namespace SemsApi.DTO.Facility
{
    public class FacilityEvaluationSummaryDto
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public int ResponseCount { get; set; }
        public decimal? AverageRating { get; set; }
    }
}
