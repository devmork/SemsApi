namespace SemsApi.DTO
{
    /// <summary>
    /// The API shape of a form (list, create and update results).
    /// Not to be confused with FormResponses, the table of student submissions.
    /// </summary>
    public class FormDto
    {
        public int FormId { get; set; }
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