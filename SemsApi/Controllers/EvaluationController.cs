using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Models.Eval;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/evaluation")]
    public class EvaluationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EvaluationController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get full evaluation form (Categories + Questions)
        /// You can pass formCode (recommended) or the old evalType
        /// Examples:
        ///   /api/evaluation/form?formCode=PRE-G3
        ///   /api/evaluation/form?formCode=G4-G6
        ///   /api/evaluation/form?formCode=JHS-SHS
        /// </summary>
        [HttpGet("form")]
        public async Task<ActionResult<List<EvaluationCategoryDto>>> GetForm(
            [FromQuery] string? formCode = null,
            [FromQuery] string evalType = "Teacher Evaluation")
        {
            int? formId = null;

            if (!string.IsNullOrWhiteSpace(formCode))
            {
                formId = await _context.EvaluationForms
                    .Where(f => f.Code == formCode && f.IsActive)
                    .Select(f => (int?)f.FormId)
                    .FirstOrDefaultAsync();

                if (formId is null)
                    return NotFound(new { message = $"Form with code '{formCode}' not found." });
            }

            var categoriesQuery = _context.TeacherEvaluationCategories.AsQueryable();
            var questionsQuery = _context.TeacherEvaluationResults.AsQueryable();

            if (formId.HasValue)
            {
                categoriesQuery = categoriesQuery.Where(c => c.FormId == formId.Value);
                questionsQuery = questionsQuery.Where(q => q.FormId == formId.Value);
            }
            else
            {
                // Fallback to old behavior
                categoriesQuery = categoriesQuery.Where(c => c.EvalType == evalType);
                questionsQuery = questionsQuery.Where(q => q.EvalType == evalType);
            }

            var categories = await categoriesQuery
                .OrderBy(c => c.CatNo)
                .ToListAsync();

            var questions = await questionsQuery
                .OrderBy(q => q.CatNo)
                .ThenBy(q => q.QnNo)
                .ToListAsync();

            var result = categories.Select(c => new EvaluationCategoryDto
            {
                CatNo = c.CatNo ?? 0,
                CatRn = c.CatRn,
                CatName = c.CatName,
                CatRate = c.CatRate,
                Questions = questions
                    .Where(q => q.CatNo == c.CatNo)
                    .Select(q => new EvaluationQuestionDto
                    {
                        QnNo = q.QnNo ?? 0,
                        QnName = q.QnName
                    })
                    .ToList()
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get questions (optionally filtered by category)
        /// </summary>
        [HttpGet("questions")]
        public async Task<ActionResult> GetQuestions(
            [FromQuery] string evalType = "Teacher Evaluation",
            [FromQuery] int? catNo = null)
        {
            var query = _context.TeacherEvaluationResults
                .Where(q => q.EvalType == evalType);

            if (catNo.HasValue)
                query = query.Where(q => q.CatNo == catNo.Value);

            var data = await query
                .OrderBy(q => q.CatNo)
                .ThenBy(q => q.QnNo)
                .Select(q => new
                {
                    q.CatNo,
                    q.QnNo,
                    q.QnName
                })
                .ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// Submit Guidance Evaluation (answers + log)
        /// </summary>
        [HttpPost("guidance/submit")]
        public async Task<ActionResult> SubmitGuidance([FromBody] SubmitGuidanceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.StudentId) || request.Answers == null || !request.Answers.Any())
            {
                return BadRequest(new { message = "StudentId and at least one answer are required." });
            }

            // Prevent duplicate submission
            var alreadySubmitted = await _context.GuidanceEvaluationLogs
                .AnyAsync(l => l.StudentId == request.StudentId
                            && l.Sy == request.Sy
                            && l.Term == request.Term);

            if (alreadySubmitted)
            {
                return Conflict(new { message = "This student has already submitted the evaluation for this term." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Insert Log
                var log = new GuidanceEvaluationLog
                {
                    StudentId = request.StudentId,
                    Sy = request.Sy,
                    Term = request.Term,
                    DateEvaluated = DateTime.UtcNow
                };
                _context.GuidanceEvaluationLogs.Add(log);

                // Insert Answers
                foreach (var answer in request.Answers)
                {
                    _context.GuidanceEvaluationResults.Add(new GuidanceEvaluationResult
                    {
                        StudentId = request.StudentId,
                        CatNo = answer.CatNo,
                        QnNo = answer.QnNo,
                        Sy = request.Sy,
                        Term = request.Term,
                        RateVal = answer.RateVal
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Guidance evaluation submitted successfully." });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Submit Teacher Strength / Comment
        /// </summary>
        [HttpPost("teacher/strength")]
        public async Task<ActionResult> SubmitTeacherStrength([FromBody] SubmitTeacherStrengthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TeacherId) ||
                string.IsNullOrWhiteSpace(request.StudentId) ||
                string.IsNullOrWhiteSpace(request.Comment))
            {
                return BadRequest(new { message = "TeacherId, StudentId, and Comment are required." });
            }

            var strength = new TeacherEvaluationStrength
            {
                EvalType = request.EvalType,
                TeacherId = request.TeacherId,
                StudentId = request.StudentId,
                Sy = request.Sy,
                Term = request.Term,
                CommentName = request.Comment
            };

            _context.TeacherEvaluationStrengths.Add(strength);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Teacher strength comment saved successfully." });
        }

        /// <summary>
        /// Get Guidance results of a student
        /// </summary>
        [HttpGet("guidance/results/{studentId}")]
        public async Task<ActionResult> GetGuidanceResults(
            string studentId,
            [FromQuery] string sy = "2025-2026",
            [FromQuery] int term = 1)
        {
            var results = await _context.GuidanceEvaluationResults
                .Where(r => r.StudentId == studentId && r.Sy == sy && r.Term == term)
                .OrderBy(r => r.CatNo)
                .ThenBy(r => r.QnNo)
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Get Teacher Strength comments
        /// </summary>
        [HttpGet("teacher/strengths")]
        public async Task<ActionResult> GetTeacherStrengths(
            [FromQuery] string? teacherId = null,
            [FromQuery] string sy = "2025-2026",
            [FromQuery] int term = 1)
        {
            var query = _context.TeacherEvaluationStrengths
                .Where(s => s.Sy == sy && s.Term == term);

            if (!string.IsNullOrWhiteSpace(teacherId))
                query = query.Where(s => s.TeacherId == teacherId);

            var data = await query
                .OrderByDescending(s => s.Recno)
                .ToListAsync();

            return Ok(data);
        }
    }
}