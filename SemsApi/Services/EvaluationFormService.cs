using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Evaluation;
using SemsApi.Interfaces;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    public class EvaluationFormService : IEvaluationFormService
    {
        private readonly ApplicationDbContext _context;

        public EvaluationFormService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        public async Task<ActionResult<List<EvaluationFormDto>>> GetAllAsync(bool onlyActive = false)
        {
            var query = _context.EvaluationForms.AsQueryable();

            if (onlyActive)
                query = query.Where(f => f.IsActive);

            var forms = await query
                .OrderBy(f => f.FormId)
                .Select(f => EvaluationFormHelper.MapToDto(f))
                .ToListAsync();

            return new OkObjectResult(forms);
        }

        public async Task<ActionResult<EvaluationFormDto>> GetByIdAsync(int id)
        {
            var form = await _context.EvaluationForms
                .AsNoTracking()
                .Where(f => f.FormId == id)
                .Select(f => EvaluationFormHelper.MapToDto(f))
                .FirstOrDefaultAsync();

            if (form is null)
                return new NotFoundObjectResult(new { message = "Evaluation form not found." });

            return new OkObjectResult(form);
        }

        public async Task<ActionResult<EvaluationFormDto>> GetByCodeAsync(string code)
        {
            var normalizedCode = code.Trim().ToUpper();

            var form = await _context.EvaluationForms
                .AsNoTracking()
                .Where(f => f.Code == normalizedCode)
                .Select(f => EvaluationFormHelper.MapToDto(f))
                .FirstOrDefaultAsync();

            if (form is null)
                return new NotFoundObjectResult(new { message = "Evaluation form not found." });

            return new OkObjectResult(form);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> CreateAsync(EvaluationFormRequestDto request)
        {
            var validationError = EvaluationFormHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var normalizedCode = request.Code.Trim().ToUpper();

            var codeExists = await _context.EvaluationForms
                .AnyAsync(f => f.Code == normalizedCode);

            if (codeExists)
                return new ConflictObjectResult(new { message = "A form with this Code already exists." });

            var form = new EvaluationForm
            {
                Name = request.Name.Trim(),
                Code = normalizedCode,
                GradeBand = request.GradeBand.Trim(),
                TargetType = EvaluationFormHelper.NormalizeTargetType(request.TargetType),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.EvaluationForms.Add(form);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(
                actionName: nameof(GetByIdAsync),
                controllerName: null,
                routeValues: new { id = form.FormId },
                value: new
                {
                    message = "Evaluation form created successfully.",
                    data = EvaluationFormHelper.MapToDto(form)
                });
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> UpdateAsync(int id, EvaluationFormRequestDto request)
        {
            var form = await _context.EvaluationForms.FindAsync(id);
            if (form is null)
                return new NotFoundObjectResult(new { message = "Evaluation form not found." });

            var validationError = EvaluationFormHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var normalizedCode = request.Code.Trim().ToUpper();

            var codeExists = await _context.EvaluationForms
                .AnyAsync(f => f.Code == normalizedCode && f.FormId != id);

            if (codeExists)
                return new ConflictObjectResult(new { message = "A form with this Code already exists." });

            form.Name = request.Name.Trim();
            form.Code = normalizedCode;
            form.GradeBand = request.GradeBand.Trim();
            form.TargetType = EvaluationFormHelper.NormalizeTargetType(request.TargetType);
            form.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Evaluation form updated successfully.",
                data = EvaluationFormHelper.MapToDto(form)
            });
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> SoftDeleteAsync(int id)
        {
            var form = await _context.EvaluationForms.FindAsync(id);
            if (form is null)
                return new NotFoundObjectResult(new { message = "Evaluation form not found." });

            form.IsActive = false;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Evaluation form deactivated successfully." });
        }

        public async Task<ActionResult> HardDeleteAsync(int id)
        {
            var form = await _context.EvaluationForms.FindAsync(id);
            if (form is null)
                return new NotFoundObjectResult(new { message = "Evaluation form not found." });

            var hasCategories = await _context.TeacherEvaluationCategories
                .AnyAsync(c => c.FormId == id);

            if (hasCategories)
            {
                return new ConflictObjectResult(new
                {
                    message = "Cannot delete this form because it still has categories. Remove the categories first or use soft delete."
                });
            }

            _context.EvaluationForms.Remove(form);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}