namespace SemsApi.Models
{
    using SemsApi.Models.Eval;

    public class TeacherEvaluationAnswer
    {
        public int AnswerId { get; set; }
        public int SubmissionId { get; set; }
        public TeacherEvaluationSubmission Submission { get; set; } = null!;

        public int CategoryId { get; set; }

        /// <summary>FK to EvaluationFormQuestion.FormQuestionId</summary>
        public int QuestionId { get; set; }
        public EvaluationFormQuestion Question { get; set; } = null!;

        public int RateVal { get; set; }
    }
}