using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.EvaluationAssignments;
using SemsApi.Interfaces;
using SemsApi.Utilities;

namespace SemsApi.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/evaluation/assignments")]
    public class EvaluationAssignmentsController : ControllerBase
    {
        private readonly IEvaluationAssignmentService _service;
        public EvaluationAssignmentsController(IEvaluationAssignmentService service) { _service = service; }

        [HttpGet("me")]
        public async Task<ActionResult<List<EvaluationAssignmentDto>>> GetMine(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetAllAsync(userId.Value, schoolYear, term, ct);
        }

        [HttpGet("me/pending")]
        public async Task<ActionResult<List<EvaluationAssignmentDto>>> GetPending(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetPendingAsync(userId.Value, schoolYear, term, ct);
        }

        [HttpGet("me/completed")]
        public async Task<ActionResult<List<EvaluationAssignmentDto>>> GetCompleted(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetCompletedAsync(userId.Value, schoolYear, term, ct);
        }

        [HttpPost("me/submit")]
        public async Task<ActionResult> Submit(
            [FromBody] SubmitSubjectEvaluationRequest request,
            [FromQuery] int? debug_userId = null,
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.SubmitAsync(userId.Value, request, ct);
        }
    }
}