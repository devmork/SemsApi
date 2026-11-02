namespace SemsApi.Models
{
    public class TeacherEvaluationAnswer
    {
        public int AnswerId { get; set; }
        public int SubmissionId { get; set; }
        public TeacherEvaluationSubmission Submission { get; set; } = null!;

        public int CatNo { get; set; }
        public int QnNo { get; set; }
        public int RateVal { get; set; }
    }
}