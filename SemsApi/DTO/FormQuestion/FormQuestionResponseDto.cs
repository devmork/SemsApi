namespace SemsApi.DTO.FormQuestion
{
    public class FormQuestionResponseDto
    {
        public int FormQuestionId { get; set; }
        public int FormCategoryId { get; set; }
        public string? QuestionName { get; set; }
        public int DisplayOrder { get; set; }
        public decimal? Points { get; set; }
    }
}
