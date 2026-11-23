namespace SemsApi.DTO.FormCategory
{
    public class FormCategoryResponseDto
    {
        public int FormCategoryId { get; set; }
        public int FormId { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? CategoryRate { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
