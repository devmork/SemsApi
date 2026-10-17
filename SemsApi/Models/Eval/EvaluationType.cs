namespace SemsApi.Models.Eval
{
    /// <summary>
    /// A kind of evaluation (e.g. "Canteen Evaluation", "Teacher Evaluation - Grade 4 to 6").
    /// Categories, forms and teacher comments reference it by EvaluationTypeId.
    /// </summary>
    public class EvaluationType
    {
        public int EvaluationTypeId { get; set; }
        public string EvaluationTypeName { get; set; } = string.Empty;
    }
}