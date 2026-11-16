using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using SemsApi.Data;
using SemsApi.DTO.Evaluation;
using SemsApi.DTO.Form;
using SemsApi.Interfaces;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    /// <summary>
    /// EvaluationForm CRUD. A EvaluationForm's categories and questions come from its EvaluationType.
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
            int? programId = null, int? evaluationTypeId = null, int? isActive = null, bool studentView = false)
        {
            var query = _context.Forms.AsNoTracking().AsQueryable();

            if (programId.HasValue)
                query = query.Where(f => f.ProgramId == programId.Value);

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
                    Code = f.Code,
                    GradeBand = f.GradeBand,
                    TargetType = f.TargetType,
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

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<FormDetailDto>> GetByIdAsync(int formId, bool studentView = false)
        {
            var EvaluationForm = await _context.Forms
                .AsNoTracking()
                .Include(f => f.EvaluationType)
                .FirstOrDefaultAsync(f => f.FormId == formId);

            if (EvaluationForm is null || (studentView && EvaluationForm.IsActive != FormHelper.Active))
                return new NotFoundObjectResult(new { message = "EvaluationForm not found." });

            var categories = await _context.TeacherEvaluationCategories
                .AsNoTracking()
                .Where(c => c.EvaluationTypeId == EvaluationForm.EvaluationTypeId)
                .OrderBy(c => c.CatNo)
                .ToListAsync();

            var catNos = categories.Select(c => c.CatNo).ToList();

            var questions = await _context.EvaluationQuestions
                .AsNoTracking()
                .Where(q => catNos.Contains(q.CatNo))
                .OrderBy(q => q.CatNo)
                .ThenBy(q => q.QnNo)
                .ToListAsync();

            var detail = new FormDetailDto
            {
                FormId = EvaluationForm.FormId,
                Code = EvaluationForm.Code,
                GradeBand = EvaluationForm.GradeBand,
                TargetType = EvaluationForm.TargetType,
                ProgramId = EvaluationForm.ProgramId,
                EvaluationTypeId = EvaluationForm.EvaluationTypeId,
                EvaluationTypeName = EvaluationForm.EvaluationType.EvaluationTypeName,
                Title = EvaluationForm.Title,
                Description = EvaluationForm.Description,
                CreatedBy = EvaluationForm.CreatedBy,
                IsActive = EvaluationForm.IsActive,
                CreatedAt = EvaluationForm.CreatedAt,
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

            return new OkObjectResult(detail);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// The evaluation type must have at least one category, and every category must have at least one question.
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

            var incompleteError = await GetIncompleteFormErrorAsync(evaluationTypeId);
            if (incompleteError is not null)
                return new BadRequestObjectResult(new { message = incompleteError });

            var normalizedCode = FormHelper.NormalizeCode(request.Code!);

            var codeExists = await _context.Forms.AnyAsync(f => f.Code == normalizedCode);
            if (codeExists)
                return new ConflictObjectResult(new { message = "A EvaluationForm with this Code already exists." });

            var entity = new EvaluationForm
            {
                Code = normalizedCode,
                GradeBand = string.IsNullOrWhiteSpace(request.GradeBand) ? null : request.GradeBand.Trim(),
                TargetType = FormHelper.NormalizeTargetType(request.TargetType),
                ProgramId = request.ProgramId!.Value,
                EvaluationTypeId = evaluationTypeId,
                Title = request.Title!.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                CreatedBy = createdBy,
                IsActive = request.IsActive ?? FormHelper.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Forms.Add(entity);
            await _context.SaveChangesAsync();

            return new CreatedResult(
                $"/api/evaluation/forms/{entity.FormId}",
                new
                {
                    message = "EvaluationForm created successfully.",
                    data = FormHelper.MapToDto(entity, evaluationType.EvaluationTypeName)
                });
        }

        public async Task<ActionResult<FormDto>> GetByCodeAsync(string code, bool studentView = false)
        {
            var normalizedCode = FormHelper.NormalizeCode(code);

            var EvaluationForm = await _context.Forms
                .AsNoTracking()
                .Include(f => f.EvaluationType)
                .FirstOrDefaultAsync(f => f.Code == normalizedCode);

            if (EvaluationForm is null || (studentView && EvaluationForm.IsActive != FormHelper.Active))
                return new NotFoundObjectResult(new { message = "EvaluationForm not found." });

            return new OkObjectResult(FormHelper.MapToDto(EvaluationForm, EvaluationForm.EvaluationType.EvaluationTypeName));
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// FormId, CreatedBy and CreatedAt are never modified.
        /// </summary>
        public async Task<ActionResult> UpdateAsync(int formId, FormRequestDto request)
        {
            var EvaluationForm = await _context.Forms.FirstOrDefaultAsync(f => f.FormId == formId);

            if (EvaluationForm is null)
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

            var willBeActive = (request.IsActive ?? EvaluationForm.IsActive) == FormHelper.Active;

            // Re-check completeness when the EvaluationForm switches evaluation type or is/will be active.
            if (evaluationTypeId != EvaluationForm.EvaluationTypeId || willBeActive)
            {
                var incompleteError = await GetIncompleteFormErrorAsync(evaluationTypeId);
                if (incompleteError is not null)
                    return new BadRequestObjectResult(new { message = incompleteError });
            }
            var normalizedCode = FormHelper.NormalizeCode(request.Code!);

            var codeExists = await _context.Forms
                .AnyAsync(f => f.Code == normalizedCode && f.FormId != formId);
            if (codeExists)
                return new ConflictObjectResult(new { message = "A EvaluationForm with this Code already exists." });

            EvaluationForm.Code = normalizedCode;
            EvaluationForm.GradeBand = string.IsNullOrWhiteSpace(request.GradeBand) ? null : request.GradeBand.Trim();
            EvaluationForm.TargetType = FormHelper.NormalizeTargetType(request.TargetType);
            EvaluationForm.ProgramId = request.ProgramId!.Value;
            EvaluationForm.EvaluationTypeId = evaluationTypeId;
            EvaluationForm.Title = request.Title!.Trim();
            EvaluationForm.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

            if (request.IsActive.HasValue)
                EvaluationForm.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "EvaluationForm updated successfully.",
                data = FormHelper.MapToDto(EvaluationForm, evaluationType.EvaluationTypeName)
            });
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> DeleteAsync(int formId)
        {
            var EvaluationForm = await _context.Forms.FirstOrDefaultAsync(f => f.FormId == formId);

            if (EvaluationForm is null)
                return new NotFoundObjectResult(new { message = "EvaluationForm not found." });

            _context.Forms.Remove(EvaluationForm);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        // ---------------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------------

        /// <summary>
        /// A EvaluationForm needs at least one category for its evaluation type, and every category needs at least one question.
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

            var categoriesWithQuestions = await _context.EvaluationQuestions
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