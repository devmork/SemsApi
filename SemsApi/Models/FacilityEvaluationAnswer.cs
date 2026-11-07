namespace SemsApi.Models
{
    public class FacilityEvaluationAnswer
    {
        public int AnswerId { get; set; }
        public int SubmissionId { get; set; }
        public FacilityEvaluationSubmission Submission { get; set; } = null!;

        public int CatNo { get; set; }
        public int QnNo { get; set; }
        public int RateVal { get; set; }
    }
}