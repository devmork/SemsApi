using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.StudentSubjects;
using SemsApi.Interfaces;
using SemsApi.Utilities;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentSubjectsController : ControllerBase
    {
        private readonly IStudentSubjectService _service;
        public StudentSubjectsController(IStudentSubjectService service) { _service = service; }

        //[Authorize]
        [HttpGet("me/subjects")]
        public async Task<ActionResult<List<StudentSubjectDto>>> GetMine(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetForCurrentUserAsync(userId.Value, schoolYear, term, ct);
        }

        [HttpGet("{studentId:int}/subjects")]
        public async Task<ActionResult<List<StudentSubjectDto>>> GetForStudent(
            int studentId,
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            CancellationToken ct = default)
            => await _service.GetForStudentAsync(studentId, schoolYear, term, ct);

        // [Authorize(Roles = "QA,Admin")]
        [HttpPost("assign-subject")]
        public async Task<ActionResult> Assign([FromBody] AssignStudentToOfferingRequest request, CancellationToken ct = default)
            => await _service.AssignAsync(request, ct);

        // [Authorize(Roles = "QA,Admin")]
        [HttpDelete("enrollments/{enrollmentId:int}")]
        public async Task<ActionResult> Deactivate(int enrollmentId, CancellationToken ct = default)
            => await _service.DeactivateAsync(enrollmentId, ct);
    }
}