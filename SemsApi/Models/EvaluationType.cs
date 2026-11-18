namespace SemsApi.Models
{
    public class EvaluationType
    {
        public int EvaluationTypeId { get; set; }
        public string EvaluationTypeName { get; set; } = string.Empty;

        /// <summary>
        /// True when forms of this type are evaluated as a facility/service
        /// (e.g. Clinic, Canteen) instead of a subject teacher.
        /// </summary>
        public bool IsFacility { get; set; }
    }
}