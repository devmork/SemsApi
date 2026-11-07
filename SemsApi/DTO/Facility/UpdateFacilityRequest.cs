namespace SemsApi.DTO.Facility
{
    public class UpdateFacilityRequest
    {
        public string FacilityName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool? IsActive { get; set; }
    }
}
