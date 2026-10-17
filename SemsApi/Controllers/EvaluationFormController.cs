using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.DTO.Evaluation;
using SemsApi.Models;
using SemsApi.Utilities;
using System.IdentityModel.Tokens.Jwt;

namespace SemsApi.Controllers
{
    /// <summary>
    /// CRUD operations for Forms.
    /// Any authenticated user can read (Students only see active forms);
    /// only Quality Assurance (QA) can create, update or delete.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/forms")]
    public class FormController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FormController(ApplicationDbContext context)
        {
            _context = context;
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
            var query = _context.Forms.AsNoTracking().AsQueryable();

            if (programId.HasValue)
                query = query.Where(f => f.ProgramId == programId.Value);

            if (evaluationTypeId.HasValue)
                query = query.Where(f => f.EvaluationTypeId == evaluationTypeId.Value);

            if (User.IsInRole("Student"))
                query = query.Where(f => f.IsActive == FormHelper.Active);
            else if (isActive.HasValue)
                query = query.Where(f => f.IsActive == isActive.Value);

            var data = await query
                .OrderByDescending(f => f.CreatedAt)
                .ThenByDescending(f => f.FormId)
                .Select(f => new FormDto
                {
                    FormId = f.FormId,
                    ProgramId = f.ProgramId,
                    EvaluationTypeId = f.EvaluationTypeId,
                    EvaluationTypeName = f.EvaluationType.EvaluationTypeName,
                    Title = f.Title,
                    Description = f.Description,
                    CreatedBy = f.CreatedBy,
                    IsActive = f.IsActive,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// Get a single form by FormId, including its categories and the questions in each category.
        /// </summary>
        [HttpGet("{formId:int}")]
        public async Task<ActionResult<FormDetailDto>> GetById(int formId)
        {
            var form = await _context.Forms
                .AsNoTracking()
                .Include(f => f.EvaluationType)
                .FirstOrDefaultAsync(f => f.FormId == formId);

            if (form is null || (User.IsInRole("Student") && form.IsActive != FormHelper.Active))
                return NotFound(new { message = "Form not found." });

            var categories = await _context.TeacherEvaluationCategories
                .AsNoTracking()
                .Where(c => c.EvaluationTypeId == form.EvaluationTypeId)
                .OrderBy(c => c.CatNo)
                .ToListAsync();

            var catNos = categories.Select(c => c.CatNo).ToList();

            var questions = await _context.TeacherEvaluationResults
                .AsNoTracking()
                .Where(q => catNos.Contains(q.CatNo))
                .OrderBy(q => q.CatNo)
                .ThenBy(q => q.QnNo)
                .ToListAsync();

            var detail = new FormDetailDto
            {
                FormId = form.FormId,
                ProgramId = form.ProgramId,
                EvaluationTypeId = form.EvaluationTypeId,
                EvaluationTypeName = form.EvaluationType.EvaluationTypeName,
                Title = form.Title,
                Description = form.Description,
                CreatedBy = form.CreatedBy,
                IsActive = form.IsActive,
                CreatedAt = form.CreatedAt,
                Categories = categories.Select(c => new EvaluationCategoryDto
                {
                    CatNo = c.CatNo,
                    CatRn = c.CatRn,
                    CatName = c.CatName,
                    CatRate = c.CatRate,
                    Questions = questions
                        .Where(q => q.CatNo == c.CatNo)
                        .Select(q => new EvaluationQuestionDto
                        {
                            QnNo = q.QnNo,
                            QnName = q.QnName
                        })
                        .ToList()
                }).ToList()
            };

            return Ok(detail);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Create a new form. CreatedBy is taken from the authenticated user's token.
        /// The evaluation type must have at least one category, and every category must have at least one question.
        /// </summary>
        [Authorize(Roles = "QA")]
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] FormRequestDto request)
        {
            var validationError = FormHelper.Validate(request);
            if (validationError is not null)
                return BadRequest(new { message = validationError });

            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var evaluationTypeId = request.EvaluationTypeId!.Value;

            var evaluationType = await _context.EvaluationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.EvaluationTypeId == evaluationTypeId);

            if (evaluationType is null)
                return BadRequest(new { message = "Evaluation type not found." });

            var incompleteError = await GetIncompleteFormErrorAsync(evaluationTypeId);
            if (incompleteError is not null)
                return BadRequest(new { message = incompleteError });

            var entity = new Form
            {
                ProgramId = request.ProgramId!.Value,
                EvaluationTypeId = evaluationTypeId,
                Title = request.Title!.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),
                CreatedBy = userId,
                IsActive = request.IsActive ?? FormHelper.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Forms.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { formId = entity.FormId },
                new { message = "Form created successfully.", data = FormHelper.MapToDto(entity, evaluationType.EvaluationTypeName) });
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Update an existing form. FormId, CreatedBy and CreatedAt are never modified.
        /// </summary>
        [Authorize(Roles = "QA")]
        [HttpPut("{formId:int}")]
        public async Task<ActionResult> Update(int formId, [FromBody] FormRequestDto request)
        {
            var form = await _context.Forms.FirstOrDefaultAsync(f => f.FormId == formId);

            if (form is null)
                return NotFound(new { message = "Form not found." });

            var validationError = FormHelper.Validate(request);
            if (validationError is not null)
                return BadRequest(new { message = validationError });

            var evaluationTypeId = request.EvaluationTypeId!.Value;

            var evaluationType = await _context.EvaluationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.EvaluationTypeId == evaluationTypeId);

            if (evaluationType is null)
                return BadRequest(new { message = "Evaluation type not found." });

            var willBeActive = (request.IsActive ?? form.IsActive) == FormHelper.Active;

            // Re-check completeness when the form switches evaluation type or is/will be active.
            if (evaluationTypeId != form.EvaluationTypeId || willBeActive)
            {
                var incompleteError = await GetIncompleteFormErrorAsync(evaluationTypeId);
                if (incompleteError is not null)
                    return BadRequest(new { message = incompleteError });
            }

            form.ProgramId = request.ProgramId!.Value;
            form.EvaluationTypeId = evaluationTypeId;
            form.Title = request.Title!.Trim();
            form.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

            if (request.IsActive.HasValue)
                form.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Form updated successfully.",
                data = FormHelper.MapToDto(form, evaluationType.EvaluationTypeName)
            });
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Delete a form.
        /// </summary>
        [Authorize(Roles = "QA")]
        [HttpDelete("{formId:int}")]
        public async Task<ActionResult> Delete(int formId)
        {
            var form = await _context.Forms.FirstOrDefaultAsync(f => f.FormId == formId);

            if (form is null)
                return NotFound(new { message = "Form not found." });

            _context.Forms.Remove(form);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------------

        /// <summary>
        /// A form needs at least one category for its evaluation type, and every category needs at least one question.
        /// Returns an error message, or null when the evaluation type is complete.
        /// </summary>
        private async Task<string?> GetIncompleteFormErrorAsync(int evaluationTypeId)
        {
            var categoryNos = await _context.TeacherEvaluationCategories
                .AsNoTracking()
                .Where(c => c.EvaluationTypeId == evaluationTypeId)
                .Select(c => c.CatNo)
                .ToListAsync();

            if (categoryNos.Count == 0)
                return "This evaluation type has no categories yet. Create its categories and questions first.";

            var categoriesWithQuestions = await _context.TeacherEvaluationResults
                .AsNoTracking()
                .Where(q => categoryNos.Contains(q.CatNo))
                .Select(q => q.CatNo)
                .Distinct()
                .ToListAsync();

            var missing = categoryNos
                .Except(categoriesWithQuestions)
                .ToList();

            if (missing.Count > 0)
                return $"Every category needs at least one question. Categories without questions (CatNo): {string.Join(", ", missing)}.";

            return null;
        }
    }
}