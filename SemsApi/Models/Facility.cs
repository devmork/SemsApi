namespace SemsApi.Models
{
    public class Facility
    {
        public int FacilityId { get; set; }
        public string? FacilityName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FacilityEvaluationSubmission> EvaluationSubmissions { get; set; } = [];
    }
}