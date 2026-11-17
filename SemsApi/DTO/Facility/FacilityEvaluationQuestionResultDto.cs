namespace SemsApi.DTO.Facility
{
    public class FacilityEvaluationQuestionResultDto
    {
        public int QuestionId { get; set; }
        public int CategoryId { get; set; }
        public string? QuestionName { get; set; }
        public int ResponseCount { get; set; }
        public decimal? AverageRating { get; set; }
    }
}
