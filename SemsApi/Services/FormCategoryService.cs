using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.FormCategory;
using SemsApi.Interfaces;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    /// <summary>
    /// Links shared EvaluationCategories to a specific EvaluationForm. The category
    /// definition (name, weight) stays in one place; only the association is per form.
    /// </summary>
    public class FormCategoryService : IFormCategoryService
    {
        private readonly ApplicationDbContext _context;

        public FormCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<List<FormCategoryResponseDto>>> GetAllAsync(int formId, bool? isActive = null)
        {
            if (!await _context.Forms.AsNoTracking().AnyAsync(f => f.FormId == formId))
                return new NotFoundObjectResult(new { message = "Evaluation form not found." });

            var query = _context.EvaluationFormCategories
                .AsNoTracking()
                .Include(fc => fc.EvaluationCategory)
                .Where(fc => fc.FormId == formId);

            if (isActive.HasValue)
                query = query.Where(fc => fc.IsActive == isActive.Value);

            var rows = await query
                .OrderBy(fc => fc.DisplayOrder)
                .ThenBy(fc => fc.CategoryId)
                .ToListAsync();

            var data = rows.Select(FormCategoryHelper.MapToResponse).ToList();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult> CreateAsync(int formId, FormCategoryRequestDto request, int createdBy)
        {
            request.FormId = formId;
            var validationError = FormCategoryHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            if (!await _context.Forms.AsNoTracking().AnyAsync(f => f.FormId == formId))
                return new NotFoundObjectResult(new { message = "Evaluation form not found." });

            if (!await _context.TeacherEvaluationCategories.AsNoTracking()
                    .AnyAsync(c => c.CategoryId == request.CategoryId))
                return new BadRequestObjectResult(new { message = "Category not found." });

            var alreadyLinked = await _context.EvaluationFormCategories
                .AnyAsync(fc => fc.FormId == formId && fc.CategoryId == request.CategoryId);
            if (alreadyLinked)
                return new ConflictObjectResult(new { message = "This category is already part of the form." });

            var displayOrder = request.DisplayOrder ?? await NextDisplayOrderAsync(formId);

            var entity = new EvaluationFormCategory
            {
                FormId = formId,
                CategoryId = request.CategoryId,
                IsActive = request.IsActive ?? true,
                DisplayOrder = displayOrder,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.EvaluationFormCategories.Add(entity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (DbUpdateExceptionHelper.IsUniqueConstraintViolation(ex))
            {
                return new ConflictObjectResult(new { message = "This category is already part of the form." });
            }

            await _context.Entry(entity).Reference(e => e.EvaluationCategory).LoadAsync();

            return new CreatedResult(
                $"/api/evaluation/forms/{formId}/categories/{entity.FormCategoryId}",
                new
                {
                    message = "Category added to form successfully.",
                    data = FormCategoryHelper.MapToResponse(entity)
                });
        }

        public async Task<ActionResult> UpdateAsync(int formId, int formCategoryId, FormCategoryRequestDto request)
        {
            request.FormId = formId;
            var validationError = FormCategoryHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var entity = await _context.EvaluationFormCategories
                .Include(fc => fc.EvaluationCategory)
                .FirstOrDefaultAsync(fc => fc.FormId == formId && fc.FormCategoryId == formCategoryId);

            if (entity is null)
                return new NotFoundObjectResult(new { message = "Form category not found." });

            if (entity.CategoryId != request.CategoryId)
            {
                var alreadyLinked = await _context.EvaluationFormCategories
                    .AnyAsync(fc => fc.FormId == formId
                                 && fc.CategoryId == request.CategoryId
                                 && fc.FormCategoryId != formCategoryId);
                if (alreadyLinked)
                    return new ConflictObjectResult(new { message = "This category is already part of the form." });

                entity.CategoryId = request.CategoryId;
            }

            if (request.IsActive.HasValue)
                entity.IsActive = request.IsActive.Value;
            if (request.DisplayOrder.HasValue)
                entity.DisplayOrder = request.DisplayOrder.Value;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (DbUpdateExceptionHelper.IsUniqueConstraintViolation(ex))
            {
                return new ConflictObjectResult(new { message = "This category is already part of the form." });
            }

            return new OkObjectResult(new
            {
                message = "Form category updated successfully.",
                data = FormCategoryHelper.MapToResponse(entity)
            });
        }

        public async Task<ActionResult> DeleteAsync(int formId, int formCategoryId)
        {
            var entity = await _context.EvaluationFormCategories
                .FirstOrDefaultAsync(fc => fc.FormId == formId && fc.FormCategoryId == formCategoryId);

            if (entity is null)
                return new NotFoundObjectResult(new { message = "Form category not found." });

            var hasQuestions = await _context.EvaluationFormQuestions
                .AnyAsync(q => q.FormCategoryId == formCategoryId);
            if (hasQuestions)
            {
                return new ConflictObjectResult(new
                {
                    message = "Cannot remove this category while it still has questions. Delete its questions first."
                });
            }

            _context.EvaluationFormCategories.Remove(entity);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        private async Task<int> NextDisplayOrderAsync(int formId)
        {
            var max = await _context.EvaluationFormCategories
                .AsNoTracking()
                .Where(fc => fc.FormId == formId)
                .Select(fc => (int?)fc.DisplayOrder)
                .MaxAsync();

            return (max ?? 0) + 1;
        }
    }
}
