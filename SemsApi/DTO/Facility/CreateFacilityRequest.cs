
namespace SemsApi.DTO.Facility
{
    public class CreateFacilityRequest
    {
        public string FacilityName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
    }
}
