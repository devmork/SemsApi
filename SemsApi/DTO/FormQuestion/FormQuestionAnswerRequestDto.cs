namespace SemsApi.DTO.FormQuestion
{
    /// <summary>Student's answer to a single form-scoped question.</summary>
    public class FormQuestionAnswerRequestDto
    {
        public int FormQuestionId { get; set; }
        public decimal RateVal { get; set; }
    }
}
