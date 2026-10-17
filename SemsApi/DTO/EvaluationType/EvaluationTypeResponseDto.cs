namespace SemsApi.DTO
{
    /// <summary>
    /// Response shape for evaluation type CRUD operations.
    /// </summary>
    public class EvaluationTypeResponseDto
    {
        public int EvaluationTypeId { get; set; }
        public string EvaluationTypeName { get; set; } = string.Empty;
    }
}