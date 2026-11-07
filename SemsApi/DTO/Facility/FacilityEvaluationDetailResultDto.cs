namespace SemsApi.DTO.Facility
{
    public class FacilityEvaluationDetailResultDto
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public int ResponseCount { get; set; }
        public decimal? AverageRating { get; set; }
        public List<FacilityEvaluationCategoryResultDto> Categories { get; set; } = new();
        public List<FacilityEvaluationQuestionResultDto> Questions { get; set; } = new();
    }
}
