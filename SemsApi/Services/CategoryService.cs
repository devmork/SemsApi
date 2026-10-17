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

        public async Task<ActionResult<List<CategoryResponseDto>>> GetAllAsync(string? evalType = null, int? formId = null)
        {
            var query = _context.TeacherEvaluationCategories
                .Include(c => c.EvaluationType)
                .Include(c => c.Form)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(evalType))
                query = query.Where(c => c.EvaluationType.EvaluationTypeName == evalType);

            if (formId.HasValue)
                query = query.Where(c => c.FormId == formId.Value);

            var data = await query
                .OrderBy(c => c.CatNo)
                .Select(c => CategoryHelper.MapToResponse(c))
                .ToListAsync();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<CategoryResponseDto>> GetByIdAsync(int recno)
        {
            var category = await _context.TeacherEvaluationCategories
                .Include(c => c.EvaluationType)
                .Include(c => c.Form)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Recno == recno);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            return new OkObjectResult(CategoryHelper.MapToResponse(category));
        }

        public async Task<ActionResult> CreateAsync(CategoryRequestDto request)
        {
            var validationError = CategoryHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var formExists = await _context.EvaluationForms.AnyAsync(f => f.FormId == request.FormId!.Value);
            if (!formExists)
                return new NotFoundObjectResult(new { message = $"Evaluation form {request.FormId} not found." });

            var (evalTypeId, resolveError) = await ResolveEvaluationTypeAsync(request);
            if (resolveError is not null)
                return resolveError;

            var duplicateExists = await _context.TeacherEvaluationCategories.AnyAsync(c =>
                c.FormId == request.FormId!.Value &&
                c.EvaluationTypeId == evalTypeId &&
                c.CatNo == request.CatNo);

            if (duplicateExists)
                return new ConflictObjectResult(new { message = "A category with the same CatNo already exists for this form and evaluation type." });

            var entity = new TeacherEvaluationCategory
            {
                FormId = request.FormId!.Value,
                EvaluationTypeId = evalTypeId,
                CatNo = request.CatNo,
                CatRn = request.CatRn!.Trim(),
                CatName = request.CatName!.Trim(),
                CatRate = Math.Round(request.CatRate!.Value, 2)
            };

            _context.TeacherEvaluationCategories.Add(entity);
            await _context.SaveChangesAsync();

            await _context.Entry(entity).Reference(e => e.EvaluationType).LoadAsync();
            await _context.Entry(entity).Reference(e => e.Form).LoadAsync();

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

        public async Task<ActionResult> UpdateAsync(int recno, CategoryRequestDto request)
        {
            var category = await _context.TeacherEvaluationCategories
                .Include(c => c.EvaluationType)
                .Include(c => c.Form)
                .FirstOrDefaultAsync(c => c.Recno == recno);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            var validationError = CategoryHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var formExists = await _context.EvaluationForms.AnyAsync(f => f.FormId == request.FormId!.Value);
            if (!formExists)
                return new NotFoundObjectResult(new { message = $"Evaluation form {request.FormId} not found." });

            var (evalTypeId, resolveError) = await ResolveEvaluationTypeAsync(request);
            if (resolveError is not null)
                return resolveError;

            var duplicateExists = await _context.TeacherEvaluationCategories.AnyAsync(c =>
                c.Recno != recno &&
                c.FormId == request.FormId!.Value &&
                c.EvaluationTypeId == evalTypeId &&
                c.CatNo == request.CatNo);

            if (duplicateExists)
                return new ConflictObjectResult(new { message = "A category with the same CatNo already exists for this form and evaluation type." });

            category.FormId = request.FormId!.Value;
            category.EvaluationTypeId = evalTypeId;
            category.CatNo = request.CatNo;
            category.CatRn = request.CatRn!.Trim();
            category.CatName = request.CatName!.Trim();
            category.CatRate = Math.Round(request.CatRate!.Value, 2);

            await _context.SaveChangesAsync();

            await _context.Entry(category).Reference(c => c.EvaluationType).LoadAsync();
            await _context.Entry(category).Reference(c => c.Form).LoadAsync();

            return new OkObjectResult(new
            {
                message = "Category updated successfully.",
                data = CategoryHelper.MapToResponse(category)
            });
        }

        public async Task<ActionResult> DeleteAsync(int recno)
        {
            var category = await _context.TeacherEvaluationCategories
                .FirstOrDefaultAsync(c => c.Recno == recno);

            if (category is null)
                return new NotFoundObjectResult(new { message = "Category not found." });

            var hasDependentQuestions = await _context.TeacherEvaluationResults
                .AnyAsync(q => q.FormId == category.FormId && q.CatNo == category.CatNo);

            if (hasDependentQuestions)
                return new ConflictObjectResult(new { message = "Cannot delete this category because evaluation questions are associated with it." });

            _context.TeacherEvaluationCategories.Remove(category);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        private async Task<(int id, ActionResult? error)> ResolveEvaluationTypeAsync(CategoryRequestDto request)
        {
            if (request.EvaluationTypeId.HasValue)
            {
                var exists = await _context.EvaluationTypes.AnyAsync(t => t.EvaluationTypeId == request.EvaluationTypeId.Value);
                return exists ? (request.EvaluationTypeId.Value, null) : (0, new NotFoundObjectResult(new { message = $"Evaluation type {request.EvaluationTypeId} not found." }));
            }

            if (!string.IsNullOrWhiteSpace(request.EvaluationTypeName))
            {
                var name = request.EvaluationTypeName.Trim();
                var type = await _context.EvaluationTypes.FirstOrDefaultAsync(t => t.EvaluationTypeName.ToLower() == name.ToLower());
                return type is not null ? (type.EvaluationTypeId, null) : (0, new NotFoundObjectResult(new { message = $"Evaluation type '{name}' not found." }));
            }

            return (0, new BadRequestObjectResult(new { message = "Either EvaluationTypeId or EvaluationTypeName is required." }));
        }
    }
}