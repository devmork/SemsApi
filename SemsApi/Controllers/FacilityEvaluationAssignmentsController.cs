using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
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
        private readonly ApplicationDbContext _context;
        public FacilityEvaluationAssignmentsController(IFacilityEvaluationService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<List<FacilityEvaluationAssignmentDto>>> GetMine(
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
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
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.GetAssignmentsAsync(userId.Value, schoolYear, term, filter: true, ct);
        }

        [HttpPost("me/submit")]
        public async Task<ActionResult> Submit(
            [FromBody] SubmitFacilityEvaluationRequest request,
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null) return Unauthorized();
            return await _service.SubmitAsync(userId.Value, request, ct);
        }

        [HttpDelete("{submissionId:int}")]
        public async Task<ActionResult> DeleteSubmission(
            int submissionId,
            CancellationToken ct = default)
        {
            var submission = await _context.FacilityEvaluationSubmissions
                .Include(s => s.Answers)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId, ct);
            if (submission is null)
                return NotFound(new { message = "Submission not found." });

            _context.FacilityEvaluationAnswers.RemoveRange(submission.Answers);
            _context.FacilityEvaluationSubmissions.Remove(submission);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
