namespace SemsApi.DTO.Form
{
    public class FormRequestDto
    {
        public int? EvaluationTypeId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        /// <summary>1 = active, 0 = inactive. Null on create = 1. Null on update = leave unchanged.</summary>
        public int? IsActive { get; set; }
    }
}
