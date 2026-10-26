namespace SemsApi.DTO.Form
{
    public class FormDto
    {
        public int FormId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? GradeBand { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public int ProgramId { get; set; }
        public int EvaluationTypeId { get; set; }
        public string EvaluationTypeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public int IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}