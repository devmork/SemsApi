namespace SemsApi.DTO.SubjectOfferings
{
    public class UpdateSubjectOfferingRequest
    {
        public string ProgramCode { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public string Section { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public int? TeacherId { get; set; }
        public int? AdviserId { get; set; }
        public bool? IsActive { get; set; }
    }
}