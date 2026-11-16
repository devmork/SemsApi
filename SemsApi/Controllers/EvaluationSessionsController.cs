using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.EvaluationSessions;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Controllers
{
    /// <summary>
    /// QA-managed evaluation sessions. A session binds a EvaluationForm to a SchoolYear + Term
    /// and controls when the EvaluationForm becomes an actionable evaluation opportunity.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "QA,Admin")]
    [Route("api/evaluation/sessions")]
    public class EvaluationSessionsController : ControllerBase
    {
        private readonly IEvaluationSessionService _service;

        public EvaluationSessionsController(IEvaluationSessionService service)
        {
            _service = service;
        }

        /// <summary>
        /// List evaluation sessions. Students only receive sessions that are currently open
        /// and inside their optional UTC window.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EvaluationSessionDto>>> GetAll(
            [FromQuery] int? formId = null,
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            [FromQuery] EvaluationSessionStatus? status = null,
            [FromQuery] bool onlyOpen = false,
            CancellationToken ct = default)
        {
            return await _service.GetAllAsync(
                formId,
                schoolYear,
                term,
                status,
                onlyOpen,
                studentView: User.IsInRole("Student"),
                ct);
        }

        /// <summary>
        /// Get one session with its underlying EvaluationForm, categories and questions.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EvaluationSessionDetailDto>> GetById(
            int id,
            CancellationToken ct = default)
        {
            return await _service.GetByIdAsync(
                id,
                studentView: User.IsInRole("Student"),
                ct);
        }

        /// <summary>
        /// Create a Draft session from an existing EvaluationForm. CreatedBy is captured from the
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Create(
            [FromBody] EvaluationSessionCreateRequest request,
            CancellationToken ct = default)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null)
                return Unauthorized(new { message = "Sign in to create an evaluation session." });

            return await _service.CreateAsync(request, userId.Value, ct);
        }

        /// <summary>
        /// Update status, window or notes. Closed -> Open is only allowed for Admins.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(
            int id,
            [FromBody] EvaluationSessionUpdateRequest request,
            CancellationToken ct = default)
        {
            return await _service.UpdateAsync(
                id,
                request,
                allowReopen: User.IsInRole("Admin"),
                ct);
        }

        /// <summary>
        /// Delete a session. Open sessions must be closed first; sessions with submissions
        /// must be archived instead.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            return await _service.DeleteAsync(id, ct);
        }

        /// <summary>
        /// Convenience endpoint: move a Draft session to Open after validating the EvaluationForm,
        /// completeness rules and window.
        /// </summary>
        [HttpPost("{id:int}/open")]
        public async Task<ActionResult> Open(int id, CancellationToken ct = default)
        {
            return await _service.OpenAsync(id, ct);
        }

        /// <summary>
        /// Get the list of students who have already submitted an evaluation for this session.
        /// </summary>
        [HttpGet("{id:int}/submissions")]
        public async Task<ActionResult<List<SessionSubmissionDto>>> GetSubmissions(
            int id,
            CancellationToken ct = default)
        {
            return await _service.GetSubmissionsAsync(id, ct);
        }

        /// <summary>
        /// Convenience endpoint: move a Draft/Open session to Closed.
        /// </summary>
        [HttpPost("{id:int}/close")]
        public async Task<ActionResult> Close(int id, CancellationToken ct = default)
        {
            return await _service.CloseAsync(id, ct);
        }
    }
}
