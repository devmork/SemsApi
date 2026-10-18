using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Interfaces;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    /// <summary>
    /// Category CRUD. A category belongs to one EvaluationType, and its CatNo is unique across all evaluation types.
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
                .OrderBy(c => c.CatNo)
                .Select(c => new CategoryResponseDto
                {
                    CatNo = c.CatNo,
                    EvaluationTypeId = c.EvaluationTypeId,
                    CatRn = c.CatRn,
                    CatName = c.CatName,
                    CatRate = c.CatRate
                })
                .ToListAsync();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<CategoryResponseDto>> GetByIdAsync(int catNo)
        {
            var category = await _context.TeacherEvaluationCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CatNo == catNo);

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

            var entity = new TeacherEvaluationCategory
            {
                EvaluationTypeId = request.EvaluationTypeId!.Value,
                CatRn = request.CatRn!.Trim(),
                CatName = request.CatName!.Trim(),
                CatRate = Math.Round(request.CatRate!.Value, 2)
            };

            _context.TeacherEvaluationCategories.Add(entity);
            await _context.SaveChangesAsync();

            return new CreatedResult(
                $"/api/evaluation/categories/{entity.CatNo}",
                new
                {
                    message = "Category created successfully.",
                    data = CategoryHelper.MapToResponse(entity)
                });
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> UpdateAsync(int catNo, CategoryRequestDto request)
        {
            var category = await _context.TeacherEvaluationCategories
                .FirstOrDefaultAsync(c => c.CatNo == catNo);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            var validationError = CategoryHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var typeExists = await _context.EvaluationTypes
                .AnyAsync(t => t.EvaluationTypeId == request.EvaluationTypeId);

            if (!typeExists)
                return new BadRequestObjectResult(new { message = "Evaluation type not found." });

            category.EvaluationTypeId = request.EvaluationTypeId!.Value;
            category.CatRn = request.CatRn!.Trim();
            category.CatName = request.CatName!.Trim();
            category.CatRate = Math.Round(request.CatRate!.Value, 2);

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

        public async Task<ActionResult> DeleteAsync(int catNo)
        {
            var category = await _context.TeacherEvaluationCategories
                .FirstOrDefaultAsync(c => c.CatNo == catNo);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            var hasDependentQuestions = await _context.TeacherEvaluationResults
                .AnyAsync(q => q.CatNo == catNo);

            if (hasDependentQuestions)
            {
                return new ConflictObjectResult(new
                {
                    message = "Cannot delete this category because evaluation questions are associated with it."
                });
            }

            _context.TeacherEvaluationCategories.Remove(category);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}