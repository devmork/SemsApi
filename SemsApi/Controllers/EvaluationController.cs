using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.DTO.Evaluation;
using SemsApi.DTO.Guidance;
using SemsApi.DTO.Teacher;
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
        /// Get full evaluation form (Categories + Questions) for an evaluation type
        /// </summary>
        [HttpGet("form")]
        public async Task<ActionResult<List<EvaluationCategoryDto>>> GetForm(
            [FromQuery] int evaluationTypeId)
        {
            if (!await _context.EvaluationTypes.AnyAsync(t => t.EvaluationTypeId == evaluationTypeId))
                return NotFound(new { message = "Evaluation type not found." });

            var categories = await _context.TeacherEvaluationCategories
                .AsNoTracking()
                .Where(c => c.EvaluationTypeId == evaluationTypeId)
                .OrderBy(c => c.CatNo)
                .ToListAsync();

            var catNos = categories.Select(c => c.CatNo).ToList();

            var questions = await _context.TeacherEvaluationResults
                .AsNoTracking()
                .Where(q => catNos.Contains(q.CatNo))
                .OrderBy(q => q.CatNo)
                .ThenBy(q => q.QnNo)
                .ToListAsync();

            var result = categories.Select(c => new EvaluationCategoryDto
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
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get questions (optionally filtered by evaluation type and/or category)
        /// </summary>
        [HttpGet("questions")]
        public async Task<ActionResult> GetQuestions(
            [FromQuery] int? evaluationTypeId = null,
            [FromQuery] int? catNo = null)
        {
            var query = _context.TeacherEvaluationResults.AsNoTracking().AsQueryable();

            if (evaluationTypeId.HasValue)
                query = query.Where(q => q.Category.EvaluationTypeId == evaluationTypeId.Value);

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

            if (!await _context.EvaluationTypes.AnyAsync(t => t.EvaluationTypeId == request.EvaluationTypeId))
            {
                return BadRequest(new { message = "Evaluation type not found." });
            }

            var strength = new TeacherEvaluationStrength
            {
                EvaluationTypeId = request.EvaluationTypeId,
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
                .Select(s => new TeacherStrengthDto
                {
                    Recno = s.Recno,
                    EvaluationTypeId = s.EvaluationTypeId,
                    TeacherId = s.TeacherId,
                    StudentId = s.StudentId,
                    Sy = s.Sy,
                    Term = s.Term,
                    CommentName = s.CommentName
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}