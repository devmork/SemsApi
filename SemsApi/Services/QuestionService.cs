using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.FormQuestion;
using SemsApi.Interfaces;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    /// <summary>
    /// Form-scoped question CRUD. A question belongs to an EvaluationFormCategory,
    /// which links a shared EvaluationCategory to a specific EvaluationForm.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly ApplicationDbContext _context;

        public QuestionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<List<FormQuestionResponseDto>>> GetAllAsync(int formId, int categoryId)
        {
            var formCategory = await FindFormCategoryAsync(formId, categoryId, track: false);
            if (formCategory is null)
                return new NotFoundObjectResult(new { message = "This category is not part of the form." });

            var data = await _context.EvaluationFormQuestions
                .AsNoTracking()
                .Where(q => q.FormCategoryId == formCategory.FormCategoryId)
                .OrderBy(q => q.DisplayOrder)
                .ThenBy(q => q.FormQuestionId)
                .Select(q => new FormQuestionResponseDto
                {
                    FormQuestionId = q.FormQuestionId,
                    FormCategoryId = q.FormCategoryId,
                    QuestionName = q.QuestionName,
                    DisplayOrder = q.DisplayOrder,
                    Points = q.Points
                })
                .ToListAsync();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<FormQuestionResponseDto>> GetByNoAsync(int formId, int categoryId, int questionId)
        {
            var formCategory = await FindFormCategoryAsync(formId, categoryId, track: false);
            if (formCategory is null)
                return new NotFoundObjectResult(new { message = "This category is not part of the form." });

            var question = await FindQuestionAsync(formCategory.FormCategoryId, questionId, track: false);
            if (question is null)
                return new NotFoundObjectResult(new { message = "Question not found." });

            return new OkObjectResult(FormQuestionHelper.MapToResponse(question));
        }

        public async Task<ActionResult> CreateAsync(int formId, int categoryId, FormQuestionRequestDto request, int createdBy)
        {
            var formCategory = await FindFormCategoryAsync(formId, categoryId, track: false);
            if (formCategory is null)
                return new NotFoundObjectResult(new { message = "This category is not part of the form." });

            // FormCategoryId comes from the route, not the client body.
            request.FormCategoryId = formCategory.FormCategoryId;

            var validationError = FormQuestionHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var displayOrder = request.DisplayOrder
                ?? await NextDisplayOrderAsync(formCategory.FormCategoryId);

            var entity = new EvaluationFormQuestion
            {
                FormCategoryId = formCategory.FormCategoryId,
                QuestionName = request.QuestionName!.Trim(),
                DisplayOrder = displayOrder,
                Points = request.Points,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.EvaluationFormQuestions.Add(entity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (DbUpdateExceptionHelper.IsUniqueConstraintViolation(ex))
            {
                return new ConflictObjectResult(new
                {
                    message = "Another question already uses this DisplayOrder in the category."
                });
            }

            return new CreatedAtActionResult(
                nameof(Controllers.QuestionController.GetByNo),
                "Question",
                new { formId, categoryId, questionId = entity.FormQuestionId },
                new { message = "Question created successfully.", data = FormQuestionHelper.MapToResponse(entity) });
        }

        public async Task<ActionResult> UpdateAsync(int formId, int categoryId, int questionId, FormQuestionRequestDto request)
        {
            var formCategory = await FindFormCategoryAsync(formId, categoryId, track: false);
            if (formCategory is null)
                return new NotFoundObjectResult(new { message = "This category is not part of the form." });

            var question = await FindQuestionAsync(formCategory.FormCategoryId, questionId, track: true);
            if (question is null)
                return new NotFoundObjectResult(new { message = "Question not found." });

            request.FormCategoryId = formCategory.FormCategoryId;

            var validationError = FormQuestionHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            question.QuestionName = request.QuestionName!.Trim();
            question.Points = request.Points;
            if (request.DisplayOrder.HasValue)
                question.DisplayOrder = request.DisplayOrder.Value;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (DbUpdateExceptionHelper.IsUniqueConstraintViolation(ex))
            {
                return new ConflictObjectResult(new
                {
                    message = "Another question already uses this DisplayOrder in the category."
                });
            }

            return new OkObjectResult(new
            {
                message = "Question updated successfully.",
                data = FormQuestionHelper.MapToResponse(question)
            });
        }

        public async Task<ActionResult> DeleteAsync(int formId, int categoryId, int questionId)
        {
            var formCategory = await FindFormCategoryAsync(formId, categoryId, track: false);
            if (formCategory is null)
                return new NotFoundObjectResult(new { message = "This category is not part of the form." });

            var question = await FindQuestionAsync(formCategory.FormCategoryId, questionId, track: true);
            if (question is null)
                return new NotFoundObjectResult(new { message = "Question not found." });

            _context.EvaluationFormQuestions.Remove(question);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        private Task<EvaluationFormCategory?> FindFormCategoryAsync(int formId, int categoryId, bool track)
        {
            var query = track
                ? _context.EvaluationFormCategories.AsQueryable()
                : _context.EvaluationFormCategories.AsNoTracking();

            return query.FirstOrDefaultAsync(fc => fc.FormId == formId && fc.CategoryId == categoryId);
        }

        private Task<EvaluationFormQuestion?> FindQuestionAsync(int formCategoryId, int questionId, bool track)
        {
            var query = track
                ? _context.EvaluationFormQuestions.AsQueryable()
                : _context.EvaluationFormQuestions.AsNoTracking();

            return query.FirstOrDefaultAsync(q =>
                q.FormCategoryId == formCategoryId && q.FormQuestionId == questionId);
        }

        private async Task<int> NextDisplayOrderAsync(int formCategoryId)
        {
            var max = await _context.EvaluationFormQuestions
                .AsNoTracking()
                .Where(q => q.FormCategoryId == formCategoryId)
                .Select(q => (int?)q.DisplayOrder)
                .MaxAsync();

            return (max ?? 0) + 1;
        }
    }
}
