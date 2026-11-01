namespace SemsApi.DTO.Subjects
{
    public class UpdateSubjectRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }
}