namespace SemsApi.DTO.Facility
{
    public class UpdateFacilityRequest
    {
        public string FacilityName { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }
}
