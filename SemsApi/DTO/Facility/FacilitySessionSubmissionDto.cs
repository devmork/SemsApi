namespace SemsApi.DTO.Facility
{
    public class FacilitySessionSubmissionDto
    {
        public int SubmissionId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public DateTime SubmittedAtUtc { get; set; }
    }
}
