using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Facility;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "QA,Admin")]
    [Route("api/evaluation/facility-results")]
    public class FacilityEvaluationResultsController : ControllerBase
    {
        private readonly IFacilityEvaluationService _service;
        public FacilityEvaluationResultsController(IFacilityEvaluationService service)
            => _service = service;

        [HttpGet("summary")]
        public async Task<ActionResult<List<FacilityEvaluationSummaryDto>>> GetSummary(
            [FromQuery] int? sessionId = null,
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            CancellationToken ct = default)
            => await _service.GetSummaryAsync(sessionId, schoolYear, term, ct);

        [HttpGet("facility/{facilityId:int}")]
        public async Task<ActionResult<FacilityEvaluationDetailResultDto>> GetFacilityResults(
            int facilityId,
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            [FromQuery] int? sessionId = null,
            CancellationToken ct = default)
            => await _service.GetFacilityResultsAsync(facilityId, schoolYear, term, sessionId, ct);

        [HttpGet("session/{sessionId:int}/submissions")]
        public async Task<ActionResult<List<FacilitySessionSubmissionDto>>> GetSessionSubmissions(
            int sessionId,
            CancellationToken ct = default)
            => await _service.GetSessionSubmissionsAsync(sessionId, ct);
    }
}
