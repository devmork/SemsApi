namespace SemsApi.DTO.EvaluationAssignments
{
    public class SubmitSubjectEvaluationRequest
    {
        public int SubjectOfferingId { get; set; }
        public int EvaluationTypeId { get; set; }
        public int? FormId { get; set; }
        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public List<EvaluationAnswerDto> Answers { get; set; } = new();
    }
}