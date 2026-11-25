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
        public FacilitiesController(IFacilityService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<FacilityDto>>> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            CancellationToken ct = default)
        {
            return await _service.GetAllAsync(search, isActive, ct);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FacilityDto>> GetById(
            int id,
            CancellationToken ct = default)
        {
            return await _service.GetByIdAsync(id, ct);
        }

        [HttpPost]
        public async Task<ActionResult<CreateFacilityDto>> Create(
            [FromBody] CreateFacilityDto dto,
            CancellationToken ct = default)
        {
            return await _service.CreateAsync(dto, ct);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UpdateFacilityDto>> Update(
            int id, 
            [FromBody] UpdateFacilityDto dto,
            CancellationToken ct = default)
        {
            return await _service.UpdateAsync(id, dto, ct);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(
            int id,
            CancellationToken ct = default)
        {
            return await _service.DeleteAsync(id, ct);
        }
    }
}
