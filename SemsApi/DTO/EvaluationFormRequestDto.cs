namespace SemsApi.DTO
{
    public class EvaluationFormRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string GradeBand { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty; // "SubjectTeacher" or "Facility"
        public bool IsActive { get; set; } = true;
    }
}