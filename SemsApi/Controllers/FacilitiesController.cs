using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Facility;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/facilities")]
    public class FacilitiesController : ControllerBase
    {
        private readonly IFacilityService _service;
        public FacilitiesController(IFacilityService service) { _service = service; }

        [HttpGet]
        public async Task<ActionResult<List<FacilityDto>>> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
            CancellationToken ct = default)
            => await _service.GetAllAsync(search, isActive, ct);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FacilityDto>> GetById(int id,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
            CancellationToken ct = default)
            => await _service.GetByIdAsync(id, ct);

        //[Authorize(Roles = "QA,Admin")]
        [HttpPost]
        public async Task<ActionResult<FacilityDto>> Create(
            [FromBody] CreateFacilityRequest request,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
            CancellationToken ct = default)
            => await _service.CreateAsync(request, ct);

        //[Authorize(Roles = "QA,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<FacilityDto>> Update(
            int id, [FromBody] UpdateFacilityRequest request,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
            CancellationToken ct = default)
            => await _service.UpdateAsync(id, request, ct);

        //[Authorize(Roles = "QA,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
            CancellationToken ct = default)
            => await _service.DeleteAsync(id, ct);
    }
}