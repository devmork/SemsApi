using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Interfaces;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
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

        public async Task<ActionResult<List<CategoryResponseDto>>> GetAllAsync(string? evalType = null)
        {
            var query = _context.TeacherEvaluationCategories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(evalType))
                query = query.Where(c => c.EvalType == evalType);

            var data = await query
                .OrderBy(c => c.CatNo)
                .Select(c => new CategoryResponseDto
                {
                    Recno = c.Recno,
                    EvalType = c.EvalType,
                    CatNo = c.CatNo,
                    CatRn = c.CatRn,
                    CatName = c.CatName,
                    CatRate = c.CatRate
                })
                .ToListAsync();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<CategoryResponseDto>> GetByIdAsync(int recno)
        {
            var category = await _context.TeacherEvaluationCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Recno == recno);

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

            var evalType = request.EvalType!.Trim();

            var duplicateExists = await _context.TeacherEvaluationCategories
                .AnyAsync(c => c.EvalType == evalType && c.CatNo == request.CatNo);

            if (duplicateExists)
            {
                return new ConflictObjectResult(new
                {
                    message = "A category with the same category number already exists for this evaluation type."
                });
            }

            var entity = new TeacherEvaluationCategory
            {
                EvalType = evalType,
                CatNo = request.CatNo,
                CatRn = request.CatRn!.Trim(),
                CatName = request.CatName!.Trim(),
                CatRate = Math.Round(request.CatRate!.Value, 2)
            };

            _context.TeacherEvaluationCategories.Add(entity);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(
                actionName: nameof(GetByIdAsync),
                controllerName: null,
                routeValues: new { recno = entity.Recno },
                value: new
                {
                    message = "Category created successfully.",
                    data = CategoryHelper.MapToResponse(entity)
                });
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> UpdateAsync(int recno, CategoryRequestDto request)
        {
            var category = await _context.TeacherEvaluationCategories
                .FirstOrDefaultAsync(c => c.Recno == recno);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            var validationError = CategoryHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var evalType = request.EvalType!.Trim();

            var duplicateExists = await _context.TeacherEvaluationCategories
                .AnyAsync(c => c.Recno != recno
                            && c.EvalType == evalType
                            && c.CatNo == request.CatNo);

            if (duplicateExists)
            {
                return new ConflictObjectResult(new
                {
                    message = "A category with the same category number already exists for this evaluation type."
                });
            }

            category.EvalType = evalType;
            category.CatNo = request.CatNo;
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

        public async Task<ActionResult> DeleteAsync(int recno)
        {
            var category = await _context.TeacherEvaluationCategories
                .FirstOrDefaultAsync(c => c.Recno == recno);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            var hasDependentQuestions = await _context.TeacherEvaluationResults
                .AnyAsync(q => q.EvalType == category.EvalType
                            && q.CatNo == category.CatNo);

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