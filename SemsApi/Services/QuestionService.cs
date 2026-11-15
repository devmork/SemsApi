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

        public async Task<ActionResult<List<EvaluationQuestionDto>>> GetAllAsync(int catNo)
        {
            if (!await CategoryExistsAsync(catNo))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var data = await _context.TeacherEvaluationResults
                .AsNoTracking()
                .Where(q => q.CatNo == catNo)
                .OrderBy(q => q.QnNo)
                .Select(q => new EvaluationQuestionDto
                {
                    QnNo = q.QnNo,
                    QnName = q.QnName
                })
                .ToListAsync();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<EvaluationQuestionDto>> GetByNoAsync(int catNo, int qnNo)
        {
            if (!await CategoryExistsAsync(catNo))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var question = await FindQuestionAsync(catNo, qnNo, track: false);
            if (question is null)
                return new NotFoundObjectResult(new { message = "Question not found." });

            return new OkObjectResult(QuestionHelper.MapToDto(question));
        }

        public async Task<ActionResult> CreateAsync(int catNo, EvaluationQuestionDto request)
        {
            if (!await CategoryExistsAsync(catNo))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var validationError = QuestionHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var qnName = request.QnName!.Trim();
            var entity = new TeacherEvaluationResult
            {
                CatNo = catNo,
                QnName = qnName
            };

            _context.TeacherEvaluationResults.Add(entity);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(
                nameof(Controllers.QuestionController.GetByNo),
                "Question",
                new { catNo, qnNo = entity.QnNo },
                new { message = "Question created successfully.", data = QuestionHelper.MapToDto(entity) });
        }

        public async Task<ActionResult> UpdateAsync(int catNo, int qnNo, EvaluationQuestionDto request)
        {
            if (!await CategoryExistsAsync(catNo))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var question = await FindQuestionAsync(catNo, qnNo, track: true);
            if (question is null)
                return new NotFoundObjectResult(new { message = "Question not found." });

            var validationError = QuestionHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            question.QnName = request.QnName!.Trim();
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Question updated successfully.",
                data = QuestionHelper.MapToDto(question)
            });
        }

        public async Task<ActionResult> DeleteAsync(int catNo, int qnNo)
        {
            if (!await CategoryExistsAsync(catNo))
                return new NotFoundObjectResult(new { message = "Category not found." });

            var question = await FindQuestionAsync(catNo, qnNo, track: true);
            if (question is null)
                return new NotFoundObjectResult(new { message = "Question not found." });

            _context.TeacherEvaluationResults.Remove(question);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        private Task<bool> CategoryExistsAsync(int catNo) =>
            _context.TeacherEvaluationCategories
                .AsNoTracking()
                .AnyAsync(c => c.CatNo == catNo);

        private Task<TeacherEvaluationResult?> FindQuestionAsync(int catNo, int qnNo, bool track)
        {
            var query = track
                ? _context.TeacherEvaluationResults.AsQueryable()
                : _context.TeacherEvaluationResults.AsNoTracking();

            return query.FirstOrDefaultAsync(q => q.CatNo == catNo && q.QnNo == qnNo);
        }
    }
}
