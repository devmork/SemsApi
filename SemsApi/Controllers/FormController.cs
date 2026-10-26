using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using SemsApi.DTO.Form;
using SemsApi.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace SemsApi.Controllers
{
    /// <summary>
    /// CRUD operations for forms.
    /// All logic lives in IFormService; this controller only handles HTTP concerns.
    /// Intended access: any signed-in user can read (Students only see active forms);
    /// only Quality Assurance (QA) can create, update or delete.
    /// </summary>
    [ApiController]
    //[Authorize]
    [Route("api/evaluation/forms")]
    public class FormController : ControllerBase
    {
        private readonly IFormService _service;

        public FormController(IFormService service)
        {
            _service = service;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        /// <summary>
        /// List forms. Optionally filter by programId, evaluationTypeId and/or isActive (1 or 0).
        /// Newest first. Students only ever receive active forms.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FormDto>>> GetAll(
            [FromQuery] int? programId = null,
            [FromQuery] int? evaluationTypeId = null,
            [FromQuery] int? isActive = null)
        {
            return await _service.GetAllAsync(
                programId, evaluationTypeId, isActive, studentView: User.IsInRole("Student"));
        }

        /// <summary>
        /// Get a single form by FormId, including its categories and the questions in each category.
        /// </summary>
        [HttpGet("{formId:int}")]
        public async Task<ActionResult<FormDetailDto>> GetById(int formId)
        {
            return await _service.GetByIdAsync(formId, studentView: User.IsInRole("Student"));
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Create a new form. CreatedBy is taken from the signed-in user's token, so a valid token is required.
        /// The evaluation type must have at least one category, and every category must have at least one question.
        /// </summary>
        //[Authorize(Roles = "QA")]
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] FormRequestDto request)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Sign in to create a form." });

            return await _service.CreateAsync(request, userId);
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "QA")]
        [HttpPut("{formId:int}")]
        public async Task<ActionResult> Update(int formId, [FromBody] FormRequestDto request)
        {
            return await _service.UpdateAsync(formId, request);
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "QA")]
        [HttpDelete("{formId:int}")]
        public async Task<ActionResult> Delete(int formId)
        {
            return await _service.DeleteAsync(formId);
        }

        /// <summary>
        /// Get a single form by its Code (e.g. PRE-G3, CAFETERIA, LIBRARY).
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<FormDto>> GetByCode(string code)
        {
            return await _service.GetByCodeAsync(code, studentView: User.IsInRole("Student"));
        }
    }
}