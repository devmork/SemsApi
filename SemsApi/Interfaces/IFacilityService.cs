using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Facility;

namespace SemsApi.Interfaces
{
    public interface IFacilityService
    {
        Task<ActionResult<List<FacilityDto>>> GetAllAsync(
            string? search = null, bool? isActive = null, CancellationToken ct = default);
        Task<ActionResult<FacilityDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ActionResult<FacilityDto>> CreateAsync(CreateFacilityRequest request, CancellationToken ct = default);
        Task<ActionResult<FacilityDto>> UpdateAsync(int id, UpdateFacilityRequest request, CancellationToken ct = default);
        Task<ActionResult> DeleteAsync(int id, CancellationToken ct = default);
    }
}