namespace SemsApi.Models.Eval
{
    /// <summary>
    /// Student's answer to a form-scoped question. Replaces TeacherEvaluationAnswer.FacilityEvaluationAnswer
    /// reference to the removed EvaluationQuestion table.
    /// </summary>
    public class EvaluationFormQuestionAnswer
    {
        public int AnswerId { get; set; }

        /// <summary>FK to TeacherEvaluationSubmission.SubmissionId or FacilityEvaluationSubmission.SubmissionId</summary>
        public int SubmissionId { get; set; }

        /// <summary>FK to EvaluationFormQuestion.FormQuestionId</summary>
        public int FormQuestionId { get; set; }
        public EvaluationFormQuestion FormQuestion { get; set; } = null!;

        public decimal RateVal { get; set; }
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
}
