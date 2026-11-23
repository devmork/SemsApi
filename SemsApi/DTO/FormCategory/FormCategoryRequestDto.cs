namespace SemsApi.DTO.FormCategory
{
    /// <summary>Request payload for linking a shared category to a form.</summary>
    public class FormCategoryRequestDto
    {
        public int FormId { get; set; }
        public int CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
