namespace SemsApi.DTO.EvaluationType
{
    /// <summary>
    /// Response shape for EvaluationType CRUD operations.
    /// </summary>
    public class EvaluationTypeResponseDto
    {
        public int EvaluationTypeId { get; set; }
        public string EvaluationTypeName { get; set; } = string.Empty;
    }
}