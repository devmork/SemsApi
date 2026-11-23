namespace SemsApi.DTO.FormQuestion
{
    /// <summary>Request payload for creating or updating a form-scoped question.</summary>
    public class FormQuestionRequestDto
    {
        public int FormCategoryId { get; set; }
        public string? QuestionName { get; set; }
        public int? DisplayOrder { get; set; }
        public decimal? Points { get; set; }
    }
}
