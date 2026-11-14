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
    /// QA-managed evaluation sessions. A session binds a Form to a SchoolYear + Term
    /// and controls when the Form becomes an actionable evaluation opportunity.
    /// </summary>
    [ApiController]
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
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<EvaluationSessionDto>>> GetAll(
            [FromQuery] int? formId = null,
            [FromQuery] string? schoolYear = null,
            [FromQuery] int? term = null,
            [FromQuery] EvaluationSessionStatus? status = null,
            [FromQuery] bool onlyOpen = false,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
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
        /// Get one session with its underlying Form, categories and questions.
        /// </summary>
        [Authorize]
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
        /// Create a Draft session from an existing Form. CreatedBy is captured from the
        /// authenticated user (or the development debug_userId bypass).
        /// </summary>
        [Authorize(Roles = "QA,Admin")]
        [HttpPost]
        public async Task<ActionResult> Create(
            [FromBody] EvaluationSessionCreateRequest request,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
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
        [Authorize(Roles = "QA,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(
            int id,
            [FromBody] EvaluationSessionUpdateRequest request,
            [FromQuery] int? debug_userId = null, //delete this this is only for development bypass
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
        [Authorize(Roles = "QA,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            return await _service.DeleteAsync(id, ct);
        }

        /// <summary>
        /// Convenience endpoint: move a Draft session to Open after validating the Form,
        /// completeness rules and window.
        /// </summary>
        [Authorize(Roles = "QA,Admin")]
        [HttpPost("{id:int}/open")]
        public async Task<ActionResult> Open(int id, CancellationToken ct = default)
        {
            return await _service.OpenAsync(id, ct);
        }

        /// <summary>
        /// Get the list of students who have already submitted an evaluation for this session.
        /// </summary>
        [Authorize(Roles = "QA,Admin")]
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
        [Authorize(Roles = "QA,Admin")]
        [HttpPost("{id:int}/close")]
        public async Task<ActionResult> Close(int id, CancellationToken ct = default)
        {
            return await _service.CloseAsync(id, ct);
        }
    }
}
