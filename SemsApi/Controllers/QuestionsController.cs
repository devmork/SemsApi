using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Models.Eval;

namespace SemsApi.Controllers
{
    /// <summary>
    /// CRUD for the questions (TeacherEvaluationResult) inside a category.
    /// The category is identified by EvalType + CatNo (CatNo is only unique within an EvalType,
    /// so both are needed), so the existing EvaluationQuestionDto (QnNo, QnName) is all the payload needs.
    /// Any authenticated user can read; only Quality Assurance (QA) can create, update or delete.
    /// </summary>
    [ApiController]
    //[Authorize]
    [Route("api/evaluation/categories/{evalType}/{catNo:int}/questions")]
    public class QuestionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuestionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        /// <summary>
        /// List the questions of a category, ordered by QnNo.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EvaluationQuestionDto>>> GetAll(string evalType, int catNo)
        {
            if (!await CategoryExistsAsync(evalType, catNo))
                return NotFound(new { message = "Category not found." });

            var data = await _context.TeacherEvaluationResults
                .AsNoTracking()
                .Where(q => q.EvalType == evalType && q.CatNo == catNo)
                .OrderBy(q => q.QnNo)
                .Select(q => new EvaluationQuestionDto
                {
                    QnNo = q.QnNo ?? 0,
                    QnName = q.QnName
                })
                .ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// Get a single question of a category by QnNo.
        /// </summary>
        [HttpGet("{qnNo:int}")]
        public async Task<ActionResult<EvaluationQuestionDto>> GetByNo(string evalType, int catNo, int qnNo)
        {
            if (!await CategoryExistsAsync(evalType, catNo))
                return NotFound(new { message = "Category not found." });

            var question = await FindQuestionAsync(evalType, catNo, qnNo, track: false);
            if (question is null)
                return NotFound(new { message = "Question not found." });

            return Ok(MapToDto(question));
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Add a question to a category. QnNo is assigned automatically (highest existing QnNo in
        /// the category, plus one; 1 if the category has none yet) - the caller only supplies QnName.
        /// Any QnNo sent in the request body is ignored.
        /// </summary>
        //[Authorize(Roles = "QA")]
        [HttpPost]
        public async Task<ActionResult> Create(string evalType, int catNo, [FromBody] EvaluationQuestionDto request)
        {
            if (!await CategoryExistsAsync(evalType, catNo))
                return NotFound(new { message = "Category not found." });

            if (request is null || string.IsNullOrWhiteSpace(request.QnName))
                return BadRequest(new { message = "QnName is required." });

            var maxQnNo = await _context.TeacherEvaluationResults
                .Where(q => q.EvalType == evalType && q.CatNo == catNo)
                .MaxAsync(q => (int?)q.QnNo) ?? 0;

            var entity = new TeacherEvaluationResult
            {
                EvalType = evalType,
                CatNo = catNo,
                QnNo = maxQnNo + 1,
                QnName = request.QnName.Trim()
            };

            _context.TeacherEvaluationResults.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetByNo),
                new { evalType, catNo, qnNo = entity.QnNo },
                new { message = "Question created successfully.", data = MapToDto(entity) });
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Update a question's text. The route qnNo identifies it and does not change;
        /// only QnName is editable. Any QnNo sent in the request body is ignored.
        /// </summary>
        //[Authorize(Roles = "QA")]
        [HttpPut("{qnNo:int}")]
        public async Task<ActionResult> Update(string evalType, int catNo, int qnNo, [FromBody] EvaluationQuestionDto request)
        {
            if (!await CategoryExistsAsync(evalType, catNo))
                return NotFound(new { message = "Category not found." });

            var question = await FindQuestionAsync(evalType, catNo, qnNo, track: true);
            if (question is null)
                return NotFound(new { message = "Question not found." });

            if (request is null || string.IsNullOrWhiteSpace(request.QnName))
                return BadRequest(new { message = "QnName is required." });

            question.QnName = request.QnName.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Question updated successfully.",
                data = MapToDto(question)
            });
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Delete a question from a category.
        /// </summary>
        //[Authorize(Roles = "QA")]
        [HttpDelete("{qnNo:int}")]
        public async Task<ActionResult> Delete(string evalType, int catNo, int qnNo)
        {
            if (!await CategoryExistsAsync(evalType, catNo))
                return NotFound(new { message = "Category not found." });

            var question = await FindQuestionAsync(evalType, catNo, qnNo, track: true);
            if (question is null)
                return NotFound(new { message = "Question not found." });

            _context.TeacherEvaluationResults.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------------

        private Task<bool> CategoryExistsAsync(string evalType, int catNo) =>
            _context.TeacherEvaluationCategories
                .AsNoTracking()
                .AnyAsync(c => c.EvalType == evalType && c.CatNo == catNo);

        private Task<TeacherEvaluationResult?> FindQuestionAsync(
            string evalType, int catNo, int qnNo, bool track)
        {
            var query = track
                ? _context.TeacherEvaluationResults.AsQueryable()
                : _context.TeacherEvaluationResults.AsNoTracking();

            return query.FirstOrDefaultAsync(q =>
                q.EvalType == evalType && q.CatNo == catNo && q.QnNo == qnNo);
        }

        private static EvaluationQuestionDto MapToDto(TeacherEvaluationResult q) =>
            new()
            {
                QnNo = q.QnNo ?? 0,
                QnName = q.QnName
            };
    }
}