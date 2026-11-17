using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Evaluation;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    /// <summary>
    /// Question CRUD for TeacherEvaluationResult records within a category.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly ApplicationDbContext _context;

        public QuestionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<List<EvaluationQuestionDto>>> GetAllAsync(int categoryId)
        {
            if (!await CategoryExistsAsync(categoryId))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var data = await _context.EvaluationQuestions
                .AsNoTracking()
                .Where(q => q.CategoryId == categoryId)
                .OrderBy(q => q.QuestionId)
                .Select(q => new EvaluationQuestionDto
                {
                    QuestionId = q.QuestionId,
                    QuestionName = q.QuestionName
                })
                .ToListAsync();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<EvaluationQuestionDto>> GetByNoAsync(int categoryId, int questionId)
        {
            if (!await CategoryExistsAsync(categoryId))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var question = await FindQuestionAsync(categoryId, questionId, track: false);
            if (question is null)
                return new NotFoundObjectResult(new { message = "Question not found." });

            return new OkObjectResult(QuestionHelper.MapToDto(question));
        }

        public async Task<ActionResult> CreateAsync(int categoryId, EvaluationQuestionDto request)
        {
            if (!await CategoryExistsAsync(categoryId))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var validationError = QuestionHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var qnName = request.QuestionName!.Trim();
            var entity = new EvaluationQuestion
            {
                CategoryId = categoryId,
                QuestionName = qnName
            };

            _context.EvaluationQuestions.Add(entity);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(
                nameof(Controllers.QuestionController.GetByNo),
                "Question",
                new { categoryId, questionId = entity.QuestionId },
                new { message = "Question created successfully.", data = QuestionHelper.MapToDto(entity) });
        }

        public async Task<ActionResult> UpdateAsync(int categoryId, int questionId, EvaluationQuestionDto request)
        {
            if (!await CategoryExistsAsync(categoryId))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var question = await FindQuestionAsync(categoryId, questionId, track: true);
            if (question is null)
                return new NotFoundObjectResult(new { message = "Question not found." });

            var validationError = QuestionHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            question.QuestionName = request.QuestionName!.Trim();
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Question updated successfully.",
                data = QuestionHelper.MapToDto(question)
            });
        }

        public async Task<ActionResult> DeleteAsync(int categoryId, int questionId)
        {
            if (!await CategoryExistsAsync(categoryId))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var question = await FindQuestionAsync(categoryId, questionId, track: true);
            if (question is null)
                return new NotFoundObjectResult(new { message = "Question not found." });

            _context.EvaluationQuestions.Remove(question);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        private Task<bool> CategoryExistsAsync(int categoryId) =>
            _context.TeacherEvaluationCategories
                .AsNoTracking()
                .AnyAsync(c => c.CategoryId == categoryId);

        private Task<EvaluationQuestion?> FindQuestionAsync(int categoryId, int questionId, bool track)
        {
            var query = track
                ? _context.EvaluationQuestions.AsQueryable()
                : _context.EvaluationQuestions.AsNoTracking();

            return query.FirstOrDefaultAsync(q => q.CategoryId == categoryId && q.QuestionId == questionId);
        }
    }
}
