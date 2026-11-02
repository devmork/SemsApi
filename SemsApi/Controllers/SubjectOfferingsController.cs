using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.SubjectOfferings;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/subject-offerings")]
    public class SubjectOfferingsController : ControllerBase
    {
        private readonly ISubjectOfferingService _service;
        public SubjectOfferingsController(ISubjectOfferingService service) { _service = service; }

        [HttpGet]
        public async Task<ActionResult<List<SubjectOfferingDto>>> GetAll(
            [FromQuery] int? gradeLevel = null,
            [FromQuery] string? section = null,
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            [FromQuery] int? teacherId = null,
            CancellationToken ct = default)
            => await _service.GetAllAsync(gradeLevel, section, schoolYear, term, teacherId, ct);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SubjectOfferingDto>> GetById(int id, CancellationToken ct = default)
            => await _service.GetByIdAsync(id, ct);

        // [Authorize(Roles = "QA,Admin")]
        [HttpPost]
        public async Task<ActionResult<SubjectOfferingDto>> Create([FromBody] CreateSubjectOfferingRequest request, CancellationToken ct = default)
            => await _service.CreateAsync(request, ct);

        // [Authorize(Roles = "QA,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<SubjectOfferingDto>> Update(int id, [FromBody] UpdateSubjectOfferingRequest request, CancellationToken ct = default)
            => await _service.UpdateAsync(id, request, ct);

        // [Authorize(Roles = "QA,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
            => await _service.DeleteAsync(id, ct);
    }
}