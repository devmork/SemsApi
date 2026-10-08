namespace SemsApi.DTO
{
    public class EvaluationFormDto
    {
        public int FormId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string GradeBand { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public bool IsActive { get; set; }

    }
}
