namespace SemsApi.Models
{
    /// <summary>
    /// A school facility that students can evaluate (Library, Canteen, Clinic, etc.).
    /// </summary>
    public class Facility
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FacilityEvaluationSubmission> EvaluationSubmissions { get; set; }
            = new List<FacilityEvaluationSubmission>();
    }
}