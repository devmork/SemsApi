namespace SemsApi.DTO.Facility
{
    public class FacilityDto
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int EvaluationCount { get; set; } // Number of evaluations submitted for this facility
    }
}
