namespace SemsApi.Models.Eval
{
    /// <summary>
    /// Represents a type of evaluation (e.g. "Teacher Evaluation", "Facility Evaluation").
    /// Kept minimal per requirement: only an Id and a name.
    /// </summary>
    public class EvaluationType
    {
        public int EvaluationTypeId { get; set; }

        /// <summary>
        /// Display name of the evaluation type e.g. "Teacher Evaluation", "Facility Evaluation"
        /// </summary>
        public string EvaluationTypeName { get; set; } = string.Empty;
    }
}