using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Facility;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/facilities")]
    public class FacilitiesController : ControllerBase
    {
        private readonly IFacilityService _service;
        public FacilitiesController(IFacilityService service) { _service = service; }

        [HttpGet]
        public async Task<ActionResult<List<FacilityDto>>> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            CancellationToken ct = default)
            => await _service.GetAllAsync(search, isActive, ct);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FacilityDto>> GetById(int id,
            CancellationToken ct = default)
            => await _service.GetByIdAsync(id, ct);

        [HttpPost]
        public async Task<ActionResult<FacilityDto>> Create(
            [FromBody] CreateFacilityRequest request,
            CancellationToken ct = default)
            => await _service.CreateAsync(request, ct);

        [HttpPut("{id:int}")]
        public async Task<ActionResult<FacilityDto>> Update(
            int id, [FromBody] UpdateFacilityRequest request,
            CancellationToken ct = default)
            => await _service.UpdateAsync(id, request, ct);

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id,
            CancellationToken ct = default)
            => await _service.DeleteAsync(id, ct);
    }
}
