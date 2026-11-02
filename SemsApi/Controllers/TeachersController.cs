using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.TeacherSubjects;
using SemsApi.Interfaces;
using SemsApi.Utilities;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/teachers")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherSubjectService _service;
        public TeachersController(ITeacherSubjectService service) { _service = service; }

        //[Authorize]
        [HttpGet("me/subjects")]
        public async Task<ActionResult<List<TeacherSubjectDto>>> GetMySubjects(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetSubjectsForCurrentUserAsync(userId.Value, schoolYear, term, ct);
        }

        //[Authorize]
        [HttpGet("me/students")]
        public async Task<ActionResult<List<TeacherStudentDto>>> GetMyStudents(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            [FromQuery] int? debug_userId = null,
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetStudentsForCurrentUserAsync(userId.Value, schoolYear, term, ct);
        }

        [HttpGet("{teacherId:int}/subjects")]
        public async Task<ActionResult<List<TeacherSubjectDto>>> GetSubjects(
            int teacherId,
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            CancellationToken ct = default)
            => await _service.GetSubjectsForTeacherAsync(teacherId, schoolYear, term, ct);

        [HttpGet("{teacherId:int}/students")]
        public async Task<ActionResult<List<TeacherStudentDto>>> GetStudents(
            int teacherId,
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            CancellationToken ct = default)
            => await _service.GetStudentsForTeacherAsync(teacherId, schoolYear, term, ct);
    }
}