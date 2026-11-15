using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Facility;
using SemsApi.Interfaces;
using SemsApi.Utilities;

namespace SemsApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/evaluation/facility-assignments")]
    public class FacilityEvaluationAssignmentsController : ControllerBase
    {
        private readonly IFacilityEvaluationService _service;
        public FacilityEvaluationAssignmentsController(IFacilityEvaluationService service)
            => _service = service;

        [HttpGet("me")]
        public async Task<ActionResult<List<FacilityEvaluationAssignmentDto>>> GetMine(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetAssignmentsAsync(userId.Value, schoolYear, term, filter: null, ct);
        }

        [HttpGet("me/pending")]
        public async Task<ActionResult<List<FacilityEvaluationAssignmentDto>>> GetPending(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetAssignmentsAsync(userId.Value, schoolYear, term, filter: false, ct);
        }

        [HttpGet("me/completed")]
        public async Task<ActionResult<List<FacilityEvaluationAssignmentDto>>> GetCompleted(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetAssignmentsAsync(userId.Value, schoolYear, term, filter: true, ct);
        }

        [HttpPost("me/submit")]
        public async Task<ActionResult> Submit(
            [FromBody] SubmitFacilityEvaluationRequest request,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.SubmitAsync(userId.Value, request, ct);
        }
    }
}
