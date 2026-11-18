namespace SemsApi.Models.Eval
{
    /// <summary>
    /// Junction table: declares which shared EvaluationCategory a given EvaluationForm includes.
    /// Keeps EvaluationCategory un-duplicated across grade-band forms.
    /// </summary>
    public class EvaluationFormCategory
    {
        public int FormCategoryId { get; set; }

        /// <summary>FK to EvaluationForms.FormId</summary>
        public int FormId { get; set; }
        public EvaluationForm EvaluationForm { get; set; } = null!;

        /// <summary>FK to EvaluationCategory.CategoryId</summary>
        public int CategoryId { get; set; }
        public EvaluationCategory EvaluationCategory { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
