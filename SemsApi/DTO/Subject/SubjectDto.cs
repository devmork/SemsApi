namespace SemsApi.DTO.Subject
{
    public class SubjectDto
    {
        public int SubjectId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
