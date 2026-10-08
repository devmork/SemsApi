using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Models.Eval;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/evaluation/forms")]
    public class EvaluationFormController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EvaluationFormController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        /// <summary>
        /// Get all evaluation forms
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EvaluationFormDto>>> GetAll(
            [FromQuery] bool onlyActive = false)
        {
            var query = _context.EvaluationForms.AsQueryable();

            if (onlyActive)
                query = query.Where(f => f.IsActive);

            var forms = await query
                .OrderBy(f => f.FormId)
                .Select(f => new EvaluationFormDto
                {
                    FormId = f.FormId,
                    Name = f.Name,
                    Code = f.Code,
                    GradeBand = f.GradeBand,
                    TargetType = f.TargetType,
                    IsActive = f.IsActive
                })
                .ToListAsync();

            return Ok(forms);
        }

        /// <summary>
        /// Get a single form by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EvaluationFormDto>> GetById(int id)
        {
            var form = await _context.EvaluationForms
                .Where(f => f.FormId == id)
                .Select(f => new EvaluationFormDto
                {
                    FormId = f.FormId,
                    Name = f.Name,
                    Code = f.Code,
                    GradeBand = f.GradeBand,
                    TargetType = f.TargetType,
                    IsActive = f.IsActive
                })
                .FirstOrDefaultAsync();

            if (form is null)
                return NotFound(new { message = "Evaluation form not found." });

            return Ok(form);
        }

        /// <summary>
        /// Get a single form by its Code (e.g. PRE-G3, CAFETERIA, LIBRARY)
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<EvaluationFormDto>> GetByCode(string code)
        {
            var form = await _context.EvaluationForms
                .Where(f => f.Code == code.ToUpper())
                .Select(f => new EvaluationFormDto
                {
                    FormId = f.FormId,
                    Name = f.Name,
                    Code = f.Code,
                    GradeBand = f.GradeBand,
                    TargetType = f.TargetType,
                    IsActive = f.IsActive
                })
                .FirstOrDefaultAsync();

            if (form is null)
                return NotFound(new { message = "Evaluation form not found." });

            return Ok(form);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Create a new Evaluation Form
        /// TargetType must be either "SubjectTeacher" or "Facility"
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] EvaluationFormRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Code) ||
                string.IsNullOrWhiteSpace(request.GradeBand) ||
                string.IsNullOrWhiteSpace(request.TargetType))
            {
                return BadRequest(new { message = "Name, Code, GradeBand, and TargetType are required." });
            }

            var allowedTargetTypes = new[] { "SubjectTeacher", "Facility" };
            if (!allowedTargetTypes.Contains(request.TargetType.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "TargetType must be either 'SubjectTeacher' or 'Facility'." });
            }

            var normalizedCode = request.Code.Trim().ToUpper();

            var codeExists = await _context.EvaluationForms
                .AnyAsync(f => f.Code == normalizedCode);

            if (codeExists)
                return Conflict(new { message = "A form with this Code already exists." });

            var form = new EvaluationForm
            {
                Name = request.Name.Trim(),
                Code = normalizedCode,
                GradeBand = request.GradeBand.Trim(),
                TargetType = request.TargetType.Trim(),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.EvaluationForms.Add(form);
            await _context.SaveChangesAsync();

            var response = new EvaluationFormDto
            {
                FormId = form.FormId,
                Name = form.Name,
                Code = form.Code,
                GradeBand = form.GradeBand,
                TargetType = form.TargetType,
                IsActive = form.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = form.FormId },
                new { message = "Evaluation form created successfully.", data = response });
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Update an existing Evaluation Form
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] EvaluationFormRequestDto request)
        {
            var form = await _context.EvaluationForms.FindAsync(id);
            if (form is null)
                return NotFound(new { message = "Evaluation form not found." });

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Code) ||
                string.IsNullOrWhiteSpace(request.GradeBand) ||
                string.IsNullOrWhiteSpace(request.TargetType))
            {
                return BadRequest(new { message = "Name, Code, GradeBand, and TargetType are required." });
            }

            var allowedTargetTypes = new[] { "SubjectTeacher", "Facility" };
            if (!allowedTargetTypes.Contains(request.TargetType.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "TargetType must be either 'SubjectTeacher' or 'Facility'." });
            }

            var normalizedCode = request.Code.Trim().ToUpper();

            var codeExists = await _context.EvaluationForms
                .AnyAsync(f => f.Code == normalizedCode && f.FormId != id);

            if (codeExists)
                return Conflict(new { message = "A form with this Code already exists." });

            form.Name = request.Name.Trim();
            form.Code = normalizedCode;
            form.GradeBand = request.GradeBand.Trim();
            form.TargetType = request.TargetType.Trim();
            form.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Evaluation form updated successfully.",
                data = new EvaluationFormDto
                {
                    FormId = form.FormId,
                    Name = form.Name,
                    Code = form.Code,
                    GradeBand = form.GradeBand,
                    TargetType = form.TargetType,
                    IsActive = form.IsActive
                }
            });
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Soft delete (recommended) – sets IsActive = false
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> SoftDelete(int id)
        {
            var form = await _context.EvaluationForms.FindAsync(id);
            if (form is null)
                return NotFound(new { message = "Evaluation form not found." });

            form.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Evaluation form deactivated successfully." });
        }

        /// <summary>
        /// Hard delete – only allowed if the form has no categories
        /// </summary>
        [HttpDelete("{id:int}/hard")]
        public async Task<ActionResult> HardDelete(int id)
        {
            var form = await _context.EvaluationForms.FindAsync(id);
            if (form is null)
                return NotFound(new { message = "Evaluation form not found." });

            var hasCategories = await _context.TeacherEvaluationCategories
                .AnyAsync(c => c.FormId == id);

            if (hasCategories)
            {
                return Conflict(new
                {
                    message = "Cannot delete this form because it still has categories. Remove the categories first or use soft delete."
                });
            }

            _context.EvaluationForms.Remove(form);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}