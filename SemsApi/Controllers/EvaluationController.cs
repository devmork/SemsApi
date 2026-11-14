using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Evaluation;
using SemsApi.DTO.Guidance;
using SemsApi.DTO.Teacher;
using SemsApi.Models;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

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
        /// Get the categories and questions of an evaluation type.
        /// Example: /api/evaluation/form?evaluationTypeId=2
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
        [Authorize(Roles = "Student")]
        [HttpPost("guidance/submit")]
        public async Task<ActionResult> SubmitGuidance([FromBody] SubmitGuidanceRequest request, CancellationToken ct)
        {
            if (request.Answers == null || request.Answers.Count == 0)
                return BadRequest(new { message = "At least one answer is required." });

            if (string.IsNullOrWhiteSpace(request.StudentId))
                return BadRequest(new { message = "StudentId is required." });

            if (string.IsNullOrWhiteSpace(request.Sy))
                return BadRequest(new { message = "Sy is required." });

            if (request.Term is < 1 or > 4)
                return BadRequest(new { message = "Term must be between 1 and 4." });

            var currentUserId = Utilities.UserContextResolver.GetUserId(HttpContext);
            if (currentUserId is null)
                return Unauthorized();

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == currentUserId.Value, ct);
            if (student is null)
                return Unauthorized(new { message = "Student profile not found for the current user." });

            var requestedStudentId = request.StudentId.Trim();
            var matchesCurrentStudent =
                requestedStudentId == student.StudentId.ToString() ||
                string.Equals(requestedStudentId, student.StudentNumber, StringComparison.OrdinalIgnoreCase);
            if (!matchesCurrentStudent)
                return Forbid();

            var resolvedStudentId = !string.IsNullOrWhiteSpace(student.StudentNumber)
                ? student.StudentNumber
                : student.StudentId.ToString();

            var questionPairs = request.Answers
                .Select(a => new { a.CatNo, a.QnNo })
                .Distinct()
                .ToList();

            if (request.Answers.Any(a => a.CatNo <= 0 || a.QnNo <= 0))
                return BadRequest(new { message = "CatNo and QnNo must be positive integers." });

            if (request.Answers.Any(a => a.RateVal < 0 || a.RateVal > 5))
                return BadRequest(new { message = "RateVal must be between 0 and 5." });

            var catNos = questionPairs.Select(q => q.CatNo).Distinct().ToList();
            var validPairs = await _context.TeacherEvaluationResults
                .AsNoTracking()
                .Where(q => catNos.Contains(q.CatNo))
                .Select(q => new { q.CatNo, q.QnNo })
                .ToListAsync(ct);

            var validSet = validPairs.Select(q => $"{q.CatNo}:{q.QnNo}").ToHashSet();
            if (questionPairs.Any(q => !validSet.Contains($"{q.CatNo}:{q.QnNo}")))
                return BadRequest(new { message = "One or more question IDs are invalid." });

            // Prevent duplicate submission
            var alreadySubmitted = await _context.GuidanceEvaluationLogs
                .AnyAsync(l => l.StudentId == resolvedStudentId
                            && l.Sy == request.Sy
                            && l.Term == request.Term, ct);

            if (alreadySubmitted)
                return Conflict(new { message = "This student has already submitted the evaluation for this term." });

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var log = new GuidanceEvaluationLog
                {
                    StudentId = resolvedStudentId,
                    Sy = request.Sy,
                    Term = request.Term,
                    DateEvaluated = DateTime.UtcNow
                };
                _context.GuidanceEvaluationLogs.Add(log);

                foreach (var answer in request.Answers)
                {
                    _context.GuidanceEvaluationResults.Add(new GuidanceEvaluationResult
                    {
                        StudentId = resolvedStudentId,
                        CatNo = answer.CatNo,
                        QnNo = answer.QnNo,
                        Sy = request.Sy,
                        Term = request.Term,
                        RateVal = answer.RateVal
                    });
                }

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Ok(new { message = "Guidance evaluation submitted successfully." });
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// Submit Teacher Strength / Comment
        /// </summary>
        [Authorize(Roles = "Student")]
        [HttpPost("teacher/strength")]
        public async Task<ActionResult> SubmitTeacherStrength([FromBody] SubmitTeacherStrengthRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.TeacherId) ||
                string.IsNullOrWhiteSpace(request.StudentId) ||
                string.IsNullOrWhiteSpace(request.Comment))
            {
                return BadRequest(new { message = "TeacherId, StudentId, and Comment are required." });
            }

            if (string.IsNullOrWhiteSpace(request.Sy))
                return BadRequest(new { message = "Sy is required." });

            if (request.Term is < 1 or > 4)
                return BadRequest(new { message = "Term must be between 1 and 4." });

            var currentUserId = Utilities.UserContextResolver.GetUserId(HttpContext);
            if (currentUserId is null)
                return Unauthorized();

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == currentUserId.Value, ct);
            if (student is null)
                return Unauthorized(new { message = "Student profile not found for the current user." });

            var requestedStudentId = request.StudentId.Trim();
            var matchesCurrentStudent =
                requestedStudentId == student.StudentId.ToString() ||
                string.Equals(requestedStudentId, student.StudentNumber, StringComparison.OrdinalIgnoreCase);
            if (!matchesCurrentStudent)
                return Forbid();

            var resolvedStudentId = !string.IsNullOrWhiteSpace(student.StudentNumber)
                ? student.StudentNumber
                : student.StudentId.ToString();

            if (!await _context.EvaluationTypes.AnyAsync(t => t.EvaluationTypeId == request.EvaluationTypeId, ct))
                return BadRequest(new { message = "Evaluation type not found." });

            var requestedTeacherId = request.TeacherId.Trim();
            int.TryParse(requestedTeacherId, out var teacherIdInt);
            var teacherExists = await _context.Teachers
                .AsNoTracking()
                .AnyAsync(t =>
                    (teacherIdInt > 0 && t.TeacherId == teacherIdInt) ||
                    t.EmployeeNumber == requestedTeacherId, ct);
            if (!teacherExists)
                return BadRequest(new { message = "TeacherId does not exist." });

            var strength = new TeacherEvaluationStrength
            {
                EvaluationTypeId = request.EvaluationTypeId,
                TeacherId = requestedTeacherId,
                StudentId = resolvedStudentId,
                Sy = request.Sy,
                Term = request.Term,
                CommentName = request.Comment.Trim()
            };

            _context.TeacherEvaluationStrengths.Add(strength);
            await _context.SaveChangesAsync(ct);

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