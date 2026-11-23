using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    /// <summary>
    /// Category CRUD. A category belongs to one EvaluationType, and its CategoryId is unique across all evaluation types.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        public async Task<ActionResult<List<CategoryResponseDto>>> GetAllAsync(int? evaluationTypeId = null)
        {
            var query = _context.TeacherEvaluationCategories.AsNoTracking().AsQueryable();

            if (evaluationTypeId.HasValue)
                query = query.Where(c => c.EvaluationTypeId == evaluationTypeId.Value);

            var data = await query
                .OrderBy(c => c.CategoryId)
                .Select(c => new CategoryResponseDto
                {
                    CategoryId = c.CategoryId,
                    EvaluationTypeId = c.EvaluationTypeId,
                    CategoryName = c.CategoryName,
                    CategoryRate = c.CategoryRate
                })
                .ToListAsync();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<CategoryResponseDto>> GetByIdAsync(int categoryId)
        {
            var category = await _context.TeacherEvaluationCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            return new OkObjectResult(CategoryHelper.MapToResponse(category));
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> CreateAsync(CategoryRequestDto request)
        {
            var validationError = CategoryHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var typeExists = await _context.EvaluationTypes
                .AnyAsync(t => t.EvaluationTypeId == request.EvaluationTypeId);

            if (!typeExists)
                return new BadRequestObjectResult(new { message = "Evaluation type not found." });

            var entity = new EvaluationCategory
            {
                EvaluationTypeId = request.EvaluationTypeId!.Value,
                CategoryName = request.CategoryName!.Trim(),
                CategoryRate = Math.Round(request.CategoryRate!.Value, 2)
            };

            _context.TeacherEvaluationCategories.Add(entity);
            await _context.SaveChangesAsync();

            return new CreatedResult(
                $"/api/evaluation/categories/{entity.CategoryId}",
                new
                {
                    message = "Category created successfully.",
                    data = CategoryHelper.MapToResponse(entity)
                });
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> UpdateAsync(int categoryId, CategoryRequestDto request)
        {
            var category = await _context.TeacherEvaluationCategories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            var validationError = CategoryHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var typeExists = await _context.EvaluationTypes
                .AnyAsync(t => t.EvaluationTypeId == request.EvaluationTypeId);

            if (!typeExists)
                return new BadRequestObjectResult(new { message = "Evaluation type not found." });

            var newEvaluationTypeId = request.EvaluationTypeId!.Value;
            if (category.EvaluationTypeId != newEvaluationTypeId)
            {
                var hasFormsForOldType = await _context.Forms
                    .AnyAsync(f => f.EvaluationTypeId == category.EvaluationTypeId);

                var hasSessionsForOldType = await _context.EvaluationSessions
                    .AnyAsync(s => s.EvaluationForm.EvaluationTypeId == category.EvaluationTypeId);

                if (hasFormsForOldType || hasSessionsForOldType)
                {
                    return new ConflictObjectResult(new
                    {
                        message = "Cannot reassign this category because forms or evaluation sessions already reference its current evaluation type."
                    });
                }
            }

            category.EvaluationTypeId = newEvaluationTypeId;
            category.CategoryName = request.CategoryName!.Trim();
            category.CategoryRate = Math.Round(request.CategoryRate!.Value, 2);

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Category updated successfully.",
                data = CategoryHelper.MapToResponse(category)
            });
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> DeleteAsync(int categoryId)
        {
            var category = await _context.TeacherEvaluationCategories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            var hasDependentFormCategories = await _context.EvaluationFormCategories
                .AnyAsync(fc => fc.CategoryId == categoryId);

            var hasDependentQuestions = await _context.EvaluationFormQuestions
                .Join(
                    _context.EvaluationFormCategories,
                    q => q.FormCategoryId,
                    fc => fc.FormCategoryId,
                    (q, fc) => new { fc.CategoryId })
                .AnyAsync(x => x.CategoryId == categoryId);

            if (hasDependentFormCategories || hasDependentQuestions)
            {
                return new ConflictObjectResult(new
                {
                    message = "Cannot delete this category because it is referenced by forms or evaluation questions."
                });
            }

            _context.TeacherEvaluationCategories.Remove(category);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}
