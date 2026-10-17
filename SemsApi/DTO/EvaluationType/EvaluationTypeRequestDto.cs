namespace SemsApi.DTO
{
    /// <summary>
    /// Request payload for creating or updating an EvaluationType.
    /// EvaluationTypeId is intentionally omitted - it is database-generated.
    /// </summary>
    public class EvaluationTypeRequestDto
    {
        public string? EvaluationTypeName { get; set; }
    }
}