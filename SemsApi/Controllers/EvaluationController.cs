using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Evaluation;
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
        /// Get the categories and questions of an evaluation form.
        /// Example: /api/evaluation/form?formId=3
        /// </summary>
        [Authorize(Roles = "QA,Admin")]
        [HttpGet("form")]
        public async Task<ActionResult<List<EvaluationCategoryDto>>> GetForm(
            [FromQuery] int formId)
        {
            var form = await _context.Forms
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FormId == formId);

            if (form is null)
                return NotFound(new { message = "Evaluation form not found." });

            var formCategories = await _context.EvaluationFormCategories
                .AsNoTracking()
                .Include(fc => fc.EvaluationCategory)
                .Where(fc => fc.FormId == formId && fc.IsActive)
                .OrderBy(fc => fc.DisplayOrder)
                .ThenBy(fc => fc.CategoryId)
                .ToListAsync();

            var categoryIds = formCategories.Select(fc => fc.CategoryId).ToList();

            var questions = await _context.EvaluationFormQuestions
                .AsNoTracking()
                .Include(q => q.FormCategory)
                .Where(q => categoryIds.Contains(q.FormCategory.CategoryId))
                .OrderBy(q => q.FormCategory.DisplayOrder)
                .ThenBy(q => q.DisplayOrder)
                .ToListAsync();

            var result = formCategories.Select(fc => new EvaluationCategoryDto
            {
                CategoryId = fc.CategoryId,
                CategoryName = fc.EvaluationCategory?.CategoryName,
                CategoryRate = fc.EvaluationCategory?.CategoryRate,
                Questions = questions
                    .Where(q => q.FormCategory.CategoryId == fc.CategoryId)
                    .Select(q => new EvaluationQuestionDto
                    {
                        QuestionId = q.FormQuestionId,
                        QuestionName = q.QuestionName
                    })
                    .ToList()
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get form-scoped questions (optionally filtered by form and/or category).
        /// </summary>
        [Authorize(Roles = "QA,Admin")]
        [HttpGet("questions")]
        public async Task<ActionResult> GetQuestions(
            [FromQuery] int? formId = null,
            [FromQuery] int? categoryId = null)
        {
            var query = _context.EvaluationFormQuestions
                .AsNoTracking()
                .AsQueryable();

            if (formId.HasValue)
                query = query.Where(q => q.FormCategory.FormId == formId.Value);

            if (categoryId.HasValue)
                query = query.Where(q => q.FormCategory.CategoryId == categoryId.Value);

            var data = await query
                .OrderBy(q => q.FormCategoryId)
                .ThenBy(q => q.DisplayOrder)
                .Select(q => new
                {
                    q.FormCategoryId,
                    FormId = q.FormCategory.FormId,
                    CategoryId = q.FormCategory.CategoryId,
                    QuestionId = q.FormQuestionId,
                    q.QuestionName
                })
                .ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// Submit Teacher Strength / Comment
        /// </summary>
        [Authorize(Roles = "Student")]
        [HttpPost("teacher/strength")]
        public async Task<ActionResult> SubmitTeacherStrength([FromBody] SubmitTeacherStrengthRequest request, CancellationToken ct)
        {
            if (request.TeacherId <= 0 ||
                request.StudentId <= 0 ||
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

            var requestedStudentId = request.StudentId.ToString();
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

            var requestedTeacherId = request.TeacherId.ToString();
            int.TryParse(requestedTeacherId, out var teacherIdInt);
            var teacherExists = await _context.Teachers
                .AsNoTracking()
                .AnyAsync(t =>
                    (teacherIdInt > 0 && t.TeacherId == teacherIdInt) ||
                    t.EmployeeNumber == requestedTeacherId, ct);
            if (!teacherExists)
                return BadRequest(new { message = "TeacherId does not exist." });

            var strength = new TeacherEvaluationComment
            {
                EvaluationTypeId = request.EvaluationTypeId,
                TeacherId = request.TeacherId,
                StudentId = student.StudentId,
                Sy = request.Sy,
                Term = request.Term,
                CommentName = request.Comment.Trim()
            };

            _context.TeacherEvaluationComments.Add(strength);
            await _context.SaveChangesAsync(ct);

            return Ok(new { message = "Teacher strength comment saved successfully." });
        }

        /// <summary>
        /// Get Teacher Strength comments
        /// </summary>
        [Authorize(Roles = "QA,Admin")]
        [HttpGet("teacher/strengths")]
        public async Task<ActionResult> GetTeacherStrengths(
            [FromQuery] int? teacherId = null,
            [FromQuery] string sy = "2025-2026",
            [FromQuery] int term = 1)
        {
            var query = _context.TeacherEvaluationComments
                .Where(s => s.Sy == sy && s.Term == term);

            if (teacherId.HasValue && teacherId.Value > 0)
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
