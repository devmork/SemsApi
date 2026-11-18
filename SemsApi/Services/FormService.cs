using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Evaluation;
using SemsApi.DTO.Form;
using SemsApi.Interfaces;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    /// <summary>
    /// EvaluationForm CRUD. Each form selects shared EvaluationCategories via
    /// EvaluationFormCategory and owns its own questions via EvaluationFormQuestion.
    /// </summary>
    public class FormService : IFormService
    {
        private readonly ApplicationDbContext _context;

        public FormService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        public async Task<ActionResult<List<FormDto>>> GetAllAsync(
            int? evaluationTypeId = null, int? isActive = null, bool studentView = false)
        {
            var query = _context.Forms.AsNoTracking().AsQueryable();

            if (evaluationTypeId.HasValue)
                query = query.Where(f => f.EvaluationTypeId == evaluationTypeId.Value);

            if (studentView)
                query = query.Where(f => f.IsActive == FormHelper.Active);
            else if (isActive.HasValue)
                query = query.Where(f => f.IsActive == isActive.Value);

            var data = await query
                .OrderByDescending(f => f.CreatedAt)
                .ThenByDescending(f => f.FormId)
                .Select(f => new FormDto
                {
                    FormId = f.FormId,
                    EvaluationTypeId = f.EvaluationTypeId,
                    EvaluationTypeName = f.EvaluationType.EvaluationTypeName,
                    Title = f.Title,
                    Description = f.Description,
                    CreatedBy = f.CreatedBy,
                    IsActive = f.IsActive,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<FormDetailDto>> GetByIdAsync(int formId, bool studentView = false)
        {
            var evaluationForm = await _context.Forms
                .AsNoTracking()
                .Include(f => f.EvaluationType)
                .FirstOrDefaultAsync(f => f.FormId == formId);

            if (evaluationForm is null || (studentView && evaluationForm.IsActive != FormHelper.Active))
                return new NotFoundObjectResult(new { message = "EvaluationForm not found." });

            var formCategories = await _context.EvaluationFormCategories
                .AsNoTracking()
                .Include(fc => fc.EvaluationCategory)
                .Where(fc => fc.FormId == formId && fc.IsActive)
                .OrderBy(fc => fc.DisplayOrder)
                .ThenBy(fc => fc.CategoryId)
                .ToListAsync();

            var categoryIds = formCategories.Select(fc => fc.CategoryId).ToList();

            var formQuestions = await _context.EvaluationFormQuestions
                .AsNoTracking()
                .Include(q => q.FormCategory)
                .Where(q => categoryIds.Contains(q.FormCategory.CategoryId))
                .OrderBy(q => q.FormCategory.DisplayOrder)
                .ThenBy(q => q.DisplayOrder)
                .ToListAsync();

            var detail = new FormDetailDto
            {
                FormId = evaluationForm.FormId,
                EvaluationTypeId = evaluationForm.EvaluationTypeId,
                EvaluationTypeName = evaluationForm.EvaluationType.EvaluationTypeName,
                Title = evaluationForm.Title,
                Description = evaluationForm.Description,
                CreatedBy = evaluationForm.CreatedBy,
                IsActive = evaluationForm.IsActive,
                CreatedAt = evaluationForm.CreatedAt,
                Categories = formCategories.Select(fc => new EvaluationCategoryDto
                {
                    CategoryId = fc.CategoryId,
                    CategoryName = fc.EvaluationCategory?.CategoryName,
                    CategoryRate = fc.EvaluationCategory?.CategoryRate,
                    Questions = formQuestions
                        .Where(q => q.FormCategory.CategoryId == fc.CategoryId)
                        .Select(q => new EvaluationQuestionDto
                        {
                            QuestionId = q.FormQuestionId,
                            QuestionName = q.QuestionName
                        })
                        .ToList()
                }).ToList()
            };

            return new OkObjectResult(detail);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// A draft form (IsActive = 0) may be created without categories/questions so they can be
        /// authored afterwards. Completeness is enforced only when the form is active.
        /// </summary>
        public async Task<ActionResult> CreateAsync(FormRequestDto request, int createdBy)
        {
            var validationError = FormHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var evaluationTypeId = request.EvaluationTypeId!.Value;

            var evaluationType = await _context.EvaluationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.EvaluationTypeId == evaluationTypeId);

            if (evaluationType is null)
                return new BadRequestObjectResult(new { message = "Evaluation type not found." });

            var entity = new EvaluationForm
            {
                EvaluationTypeId = evaluationTypeId,
                Title = request.Title!.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                CreatedBy = createdBy,
                // Null on create keeps the historical behaviour of defaulting to active.
                IsActive = request.IsActive ?? FormHelper.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Forms.Add(entity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return new ConflictObjectResult(new { message = "Unable to create the evaluation form." });
            }

            // Only active forms must be complete. Drafts can be filled in later.
            if (entity.IsActive == FormHelper.Active)
            {
                var incompleteError = await GetIncompleteFormErrorAsync(entity.FormId);
                if (incompleteError is not null)
                {
                    _context.Forms.Remove(entity);
                    await _context.SaveChangesAsync();
                    return new BadRequestObjectResult(new { message = incompleteError });
                }
            }

            return new CreatedResult(
                $"/api/evaluation/forms/{entity.FormId}",
                new
                {
                    message = entity.IsActive == FormHelper.Active
                        ? "EvaluationForm created successfully."
                        : "EvaluationForm created successfully (draft).",
                    data = FormHelper.MapToDto(entity, evaluationType.EvaluationTypeName)
                });
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// FormId, CreatedBy and CreatedAt are never modified.
        /// </summary>
        public async Task<ActionResult> UpdateAsync(int formId, FormRequestDto request)
        {
            var evaluationForm = await _context.Forms.FirstOrDefaultAsync(f => f.FormId == formId);

            if (evaluationForm is null)
                return new NotFoundObjectResult(new { message = "EvaluationForm not found." });

            var validationError = FormHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var evaluationTypeId = request.EvaluationTypeId!.Value;

            var evaluationType = await _context.EvaluationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.EvaluationTypeId == evaluationTypeId);

            if (evaluationType is null)
                return new BadRequestObjectResult(new { message = "Evaluation type not found." });

            var willBeActive = (request.IsActive ?? evaluationForm.IsActive) == FormHelper.Active;

            // Only active forms must be complete; drafts may still be missing questions.
            if (willBeActive)
            {
                var incompleteError = await GetIncompleteFormErrorAsync(formId);
                if (incompleteError is not null)
                    return new BadRequestObjectResult(new { message = incompleteError });
            }

            evaluationForm.EvaluationTypeId = evaluationTypeId;
            evaluationForm.Title = request.Title!.Trim();
            evaluationForm.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

            if (request.IsActive.HasValue)
                evaluationForm.IsActive = request.IsActive.Value;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return new ConflictObjectResult(new { message = "Unable to update the evaluation form." });
            }

            return new OkObjectResult(new
            {
                message = "EvaluationForm updated successfully.",
                data = FormHelper.MapToDto(evaluationForm, evaluationType.EvaluationTypeName)
            });
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> DeleteAsync(int formId)
        {
            var evaluationForm = await _context.Forms.FirstOrDefaultAsync(f => f.FormId == formId);

            if (evaluationForm is null)
                return new NotFoundObjectResult(new { message = "EvaluationForm not found." });

            if (await _context.EvaluationSessions.AnyAsync(s => s.FormId == formId))
                return new ConflictObjectResult(new { message = "Cannot delete: form is referenced by evaluation sessions." });

            if (await _context.TeacherEvaluationSubmissions.AnyAsync(s => s.FormId == formId))
                return new ConflictObjectResult(new { message = "Cannot delete: form has teacher evaluation submissions." });

            if (await _context.FacilityEvaluationSubmissions.AnyAsync(s => s.FormId == formId))
                return new ConflictObjectResult(new { message = "Cannot delete: form has facility evaluation submissions." });

            // Remove the form's own categories and questions first (FKs are Restrict).
            var formCategoryIds = await _context.EvaluationFormCategories
                .Where(fc => fc.FormId == formId)
                .Select(fc => fc.FormCategoryId)
                .ToListAsync();

            if (formCategoryIds.Count > 0)
            {
                var questionsToDelete = await _context.EvaluationFormQuestions
                    .Where(q => formCategoryIds.Contains(q.FormCategoryId))
                    .ToListAsync();

                _context.EvaluationFormQuestions.RemoveRange(questionsToDelete);
                _context.EvaluationFormCategories.RemoveRange(
                    await _context.EvaluationFormCategories
                        .Where(fc => formCategoryIds.Contains(fc.FormCategoryId))
                        .ToListAsync());
            }

            _context.Forms.Remove(evaluationForm);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        // ---------------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------------

        /// <summary>
        /// A EvaluationForm is complete when every active selected category on that form
        /// has at least one form-scoped question.
        /// Returns an error message, or null when complete.
        /// </summary>
        private async Task<string?> GetIncompleteFormErrorAsync(int formId)
        {
            var activeCategoryIds = await _context.EvaluationFormCategories
                .AsNoTracking()
                .Where(fc => fc.FormId == formId && fc.IsActive)
                .Select(fc => fc.CategoryId)
                .ToListAsync();

            if (activeCategoryIds.Count == 0)
                return "This form has no categories selected. Add categories and questions first.";

            var categoriesWithQuestions = await _context.EvaluationFormQuestions
                .AsNoTracking()
                .Join(
                    _context.EvaluationFormCategories.AsNoTracking(),
                    q => q.FormCategoryId,
                    fc => fc.FormCategoryId,
                    (q, fc) => new { q.FormCategoryId, fc.CategoryId, fc.FormId, fc.IsActive })
                .Where(x => x.FormId == formId && x.IsActive)
                .Select(x => x.CategoryId)
                .Distinct()
                .ToListAsync();

            var missing = activeCategoryIds
                .Except(categoriesWithQuestions)
                .ToList();

            if (missing.Count > 0)
                return $"Every selected category needs at least one question. Categories without questions (CategoryId): {string.Join(", ", missing)}.";

            return null;
        }
    }
}
