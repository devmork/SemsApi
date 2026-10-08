 namespace SemsApi.Models.Eval
{
    public class EvaluationForm
    {
        public int FormId { get; set; }

        /// <summary>
        /// Display name e.g. "Preschool - Grade 3 Teacher Evaluation Form"
        /// </summary> 
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Short code e.g. "Pre-G3", "G4-G6", "JHS-SHS"
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Grade band this form applies to
        /// </summary>
        public string GradeBand { get; set; } = string.Empty; // "Preschool-G3", "G4-G6", "JHS-SHS"

        /// <summary>
        /// What this form evaluates (for future expansion)
        /// </summary>
        public string TargetType { get; set; } = "SubjectTeacher"; // or "Facility"

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<TeacherEvaluationCategory> Categories { get; set; } = new List<TeacherEvaluationCategory>();
    }
}
