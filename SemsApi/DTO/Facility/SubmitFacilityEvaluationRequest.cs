namespace SemsApi.DTO.Facility
{
    public class SubmitFacilityEvaluationRequest
    {
        public int FacilityId { get; set; }
        public int EvaluationSessionId { get; set; }
        public int EvaluationTypeId { get; set; }
        public int? FormId { get; set; }
        public List<FacilityEvaluationAnswerDto> Answers { get; set; } = new();
    }
}