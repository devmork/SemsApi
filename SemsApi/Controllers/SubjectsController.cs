using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Subject;
using SemsApi.DTO.Subjects;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/subjects")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _service;
        public SubjectsController(ISubjectService service) { _service = service; }

        [HttpGet]
        public async Task<ActionResult<List<SubjectDto>>> GetAll([FromQuery] string? search = null, CancellationToken ct = default)
            => await _service.GetAllAsync(search, ct);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SubjectDto>> GetById(int id, CancellationToken ct = default)
            => await _service.GetByIdAsync(id, ct);

        // [Authorize(Roles = "QA,Admin")]
        [HttpPost]
        public async Task<ActionResult<SubjectDto>> Create([FromBody] CreateSubjectRequest request, CancellationToken ct = default)
            => await _service.CreateAsync(request, ct);

        // [Authorize(Roles = "QA,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<SubjectDto>> Update(int id, [FromBody] UpdateSubjectRequest request, CancellationToken ct = default)
            => await _service.UpdateAsync(id, request, ct);

        // [Authorize(Roles = "QA,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
            => await _service.DeleteAsync(id, ct);
    }
}