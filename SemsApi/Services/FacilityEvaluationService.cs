using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Facility;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    public class FacilityEvaluationService : IFacilityEvaluationService
    {
        private readonly ApplicationDbContext _context;
        public FacilityEvaluationService(ApplicationDbContext context) { _context = context; }

        // -----------------------------------------------------------------
        // STUDENT-FACING: assignment list
        // -----------------------------------------------------------------
        public async Task<ActionResult<List<FacilityEvaluationAssignmentDto>>> GetAssignmentsAsync(
            int userId, string? schoolYear = null, int? term = null, bool? filter = null,
            CancellationToken ct = default)
        {
            var student = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (student is null)
                return new NotFoundObjectResult(new { message = "No student profile linked to this user." });

            if (!string.Equals(student.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                return new ObjectResult(new { message = "Student profile is not active." })
                {
                    StatusCode = 403
                };
            }

            var now = DateTime.UtcNow;

            // Default to the student's own school year unless the caller overrides it.
            var effectiveSchoolYear = !string.IsNullOrWhiteSpace(schoolYear)
                ? schoolYear.Trim()
                : (string.IsNullOrWhiteSpace(student.SchoolYear) ? null : student.SchoolYear);

            // Active facilities are the only valid targets for a facility session.
            var facilities = await _context.Facilities.AsNoTracking()
                .Where(f => f.IsActive)
                .OrderBy(f => f.FacilityName)
                .ToListAsync(ct);

            if (facilities.Count == 0)
                return new OkObjectResult(new List<FacilityEvaluationAssignmentDto>());

            var facilityIds = facilities.Select(f => f.FacilityId).ToList();

            // Facility sessions currently open for this student's SY/Term and
            // explicitly scoped to one of those facilities. Legacy sessions with
            // no FacilityId are excluded here: broadcasting them to every facility
            // is what made all facilities show the same form.
            var sessionsQuery = _context.EvaluationSessions.AsNoTracking()
                .Include(s => s.EvaluationForm).ThenInclude(f => f.EvaluationType)
                .Where(s =>
                    s.Status == EvaluationSessionStatus.Open &&
                    (!s.OpensAtUtc.HasValue || s.OpensAtUtc <= now) &&
                    (!s.ClosesAtUtc.HasValue || s.ClosesAtUtc > now) &&
                    s.EvaluationForm.IsActive == 1 &&
                    FormHelper.IsFacilityEvaluation(s.EvaluationForm.EvaluationType) &&
                    s.FacilityId.HasValue &&
                    facilityIds.Contains(s.FacilityId.Value));

            if (!string.IsNullOrWhiteSpace(effectiveSchoolYear))
                sessionsQuery = sessionsQuery.Where(s => s.SchoolYear == effectiveSchoolYear);
            if (term.HasValue)
                sessionsQuery = sessionsQuery.Where(s => s.Term == term.Value);

            var sessions = await sessionsQuery.ToListAsync(ct);

            // Honour the session grade scope when the student's grade level is known.
            var studentGrade = await ResolveStudentGradeAsync(student, ct);
            if (studentGrade.HasValue)
            {
                sessions = sessions
                    .Where(s =>
                        (!s.GradeLevelFrom.HasValue || s.GradeLevelFrom.Value <= studentGrade.Value) &&
                        (!s.GradeLevelTo.HasValue || s.GradeLevelTo.Value >= studentGrade.Value))
                    .ToList();
            }

            if (sessions.Count == 0)
                return new OkObjectResult(new List<FacilityEvaluationAssignmentDto>());

            var sessionIds = sessions.Select(s => s.EvaluationSessionId).ToList();

            var submissions = await _context.FacilityEvaluationSubmissions.AsNoTracking()
                .Where(s => s.StudentId == student.StudentId &&
                            s.EvaluationSessionId.HasValue &&
                            sessionIds.Contains(s.EvaluationSessionId.Value))
                .ToListAsync(ct);

            var submissionLookup = submissions
                .GroupBy(s => (s.EvaluationSessionId!.Value, s.FacilityId))
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SubmittedAtUtc).First());

            var facilitiesById = facilities.ToDictionary(f => f.FacilityId);
            var result = new List<FacilityEvaluationAssignmentDto>();

            foreach (var session in sessions)
            {
                if (!session.FacilityId.HasValue)
                    continue;
                if (!facilitiesById.TryGetValue(session.FacilityId.Value, out var facility))
                    continue;

                submissionLookup.TryGetValue(
                    (session.EvaluationSessionId, facility.FacilityId), out var sub);

                var item = new FacilityEvaluationAssignmentDto
                {
                    FacilityId = facility.FacilityId,
                    FacilityName = facility.FacilityName,
                    EvaluationTypeId = session.EvaluationForm.EvaluationTypeId,
                    FormId = session.FormId,
                    EvaluationSessionId = session.EvaluationSessionId,
                    SchoolYear = session.SchoolYear,
                    Term = session.Term,
                    IsCompleted = sub is not null,
                    SubmittedAtUtc = sub?.SubmittedAtUtc
                };

                if (filter is null || item.IsCompleted == filter.Value)
                    result.Add(item);
            }

            return new OkObjectResult(result.OrderBy(x => x.FacilityName).ToList());
        }

        // -----------------------------------------------------------------
        // STUDENT-FACING: submit
        // -----------------------------------------------------------------
        public async Task<ActionResult> SubmitAsync(
            int userId, SubmitFacilityEvaluationRequest request, CancellationToken ct = default)
        {
            var student = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (student is null)
                return new NotFoundObjectResult(new { message = "No student profile linked to this user." });

            if (!string.Equals(student.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                return new ObjectResult(new { message = "Student profile is not active." })
                {
                    StatusCode = 403
                };
            }

            if (request.Answers is null || request.Answers.Count == 0)
                return new BadRequestObjectResult(new { message = "At least one answer is required." });

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.FacilityId == request.FacilityId, ct);
            if (facility is null)
                return new BadRequestObjectResult(new { message = "Facility not found." });
            if (!facility.IsActive)
                return new BadRequestObjectResult(new { message = "This facility is inactive and cannot be evaluated." });

            if (request.EvaluationSessionId <= 0)
                return new BadRequestObjectResult(new { message = "EvaluationSessionId is required for facility evaluations." });

            var session = await _context.EvaluationSessions.AsNoTracking()
                .Include(s => s.EvaluationForm)
                    .ThenInclude(f => f.EvaluationType)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == request.EvaluationSessionId, ct);

            if (session is null)
                return new BadRequestObjectResult(new { message = "Evaluation session not found." });

            var studentGrade = await ResolveStudentGradeAsync(student, ct);
            if (studentGrade.HasValue &&
                ((session.GradeLevelFrom.HasValue && studentGrade.Value < session.GradeLevelFrom.Value) ||
                 (session.GradeLevelTo.HasValue && studentGrade.Value > session.GradeLevelTo.Value)))
            {
                return new BadRequestObjectResult(new
                {
                    message = "This evaluation session is not open for your grade level."
                });
            }

            if (!FormHelper.IsFacilityEvaluation(session.EvaluationForm.EvaluationType))
                return new BadRequestObjectResult(new
                {
                    message = "The selected evaluation session is not a facility evaluation session."
                });

            // The assignment list only exposes a session for its own facility.
            // Enforce the same rule here so a student cannot submit facility B's
            // answers against facility A's session by manipulating request ids.
            // Legacy null-FacilityId sessions are intentionally not accepted.
            if (session.FacilityId != facility.FacilityId)
                return new BadRequestObjectResult(new
                {
                    message = "The submitted facility does not match the evaluation session."
                });

            if (!EvaluationSessionHelper.IsWithinWindow(session, DateTime.UtcNow))
                return new BadRequestObjectResult(new { message = "This evaluation session is not open." });

            if (request.FormId.HasValue && request.FormId.Value != session.FormId)
                return new BadRequestObjectResult(new
                {
                    message = "The submitted FormId does not match the evaluation session."
                });

            if (request.EvaluationTypeId != session.EvaluationForm.EvaluationTypeId)
                return new BadRequestObjectResult(new
                {
                    message = "EvaluationTypeId does not match the evaluation session's EvaluationForm."
                });

            if (!await _context.EvaluationTypes
                    .AnyAsync(t => t.EvaluationTypeId == request.EvaluationTypeId, ct))
                return new BadRequestObjectResult(new { message = "Evaluation type not found." });

            // Validate the questions belong to this form's selected categories
            if (request.Answers.Select(a => a.QuestionId).Distinct().Count() != request.Answers.Count)
                return new BadRequestObjectResult(new { message = "Duplicate question IDs are not allowed." });

            var questionIds = request.Answers.Select(a => a.QuestionId).Distinct().ToList();
            var validQuestions = await _context.EvaluationFormQuestions.AsNoTracking()
                .Join(
                    _context.EvaluationFormCategories.AsNoTracking(),
                    q => q.FormCategoryId,
                    fc => fc.FormCategoryId,
                    (q, fc) => new { q.FormQuestionId, fc.CategoryId, fc.FormId })
                .Where(x => questionIds.Contains(x.FormQuestionId) && x.FormId == session.FormId)
                .ToListAsync(ct);

            if (validQuestions.Count != questionIds.Count)
                return new BadRequestObjectResult(new { message = "One or more question ids are invalid." });

            // Ensure each answer's CategoryId matches the question's form category
            var questionMap = validQuestions.ToDictionary(q => q.FormQuestionId);
            if (request.Answers.Any(a => !questionMap.TryGetValue(a.QuestionId, out var q) || q.CategoryId != a.CategoryId))
                return new BadRequestObjectResult(new { message = "One or more answers have a CategoryId that does not match the question." });

            // Validate rating values (0..5 is the current frontend scale; extend if needed)
            if (request.Answers.Any(a => a.RateVal < 1 || a.RateVal > 5))
                return new BadRequestObjectResult(new
                {
                    message = "RateVal must be between 1 and 5."
                });

            // Duplicate submission check (aligned with the DB unique index:
            // StudentId, FacilityId, EvaluationSessionId)
            var duplicate = await _context.FacilityEvaluationSubmissions.AnyAsync(s =>
                s.StudentId == student.StudentId &&
                s.FacilityId == facility.FacilityId &&
                s.EvaluationSessionId == session.EvaluationSessionId, ct);
            if (duplicate)
                return new ConflictObjectResult(new
                {
                    message = "This facility has already been evaluated for this evaluation session."
                });

            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var submission = new FacilityEvaluationSubmission
                {
                    StudentId = student.StudentId,
                    FacilityId = facility.FacilityId,
                    EvaluationTypeId = request.EvaluationTypeId,
                    FormId = request.FormId ?? session.FormId,
                    EvaluationSessionId = session.EvaluationSessionId,
                    SchoolYear = session.SchoolYear,
                    Term = session.Term,
                    SubmittedAtUtc = DateTime.UtcNow
                };

                foreach (var a in request.Answers)
                {
                    submission.Answers.Add(new FacilityEvaluationAnswer
                    {
                        CategoryId = a.CategoryId,
                        QuestionId = a.QuestionId,
                        RateVal = a.RateVal
                    });
                }

                _context.FacilityEvaluationSubmissions.Add(submission);
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return new OkObjectResult(new
                {
                    message = "Facility evaluation submitted successfully.",
                    submissionId = submission.SubmissionId
                });
            }
            catch (DbUpdateException ex) when (DbUpdateExceptionHelper.IsUniqueConstraintViolation(ex))
            {
                await tx.RollbackAsync(ct);
                return new ConflictObjectResult(new { message = "This facility has already been evaluated for this session." });
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        // -----------------------------------------------------------------
        // ADMIN/QA: summary by facility
        // -----------------------------------------------------------------
        public async Task<ActionResult<List<FacilityEvaluationSummaryDto>>> GetSummaryAsync(
            int? sessionId = null, string? schoolYear = null, int? term = null,
            CancellationToken ct = default)
        {
            var query = _context.FacilityEvaluationSubmissions.AsNoTracking()
                .Include(s => s.Answers)
                .Include(s => s.Facility)
                .AsQueryable();

            if (sessionId.HasValue) query = query.Where(s => s.EvaluationSessionId == sessionId.Value);
            if (!string.IsNullOrWhiteSpace(schoolYear)) query = query.Where(s => s.SchoolYear == schoolYear);
            if (term.HasValue) query = query.Where(s => s.Term == term.Value);

            var submissions = await query.ToListAsync(ct);

            var data = submissions
                .GroupBy(s => new { s.FacilityId, s.Facility.FacilityName })
                .Select(g =>
                {
                    var allAnswers = g.SelectMany(s => s.Answers).ToList();
                    return new FacilityEvaluationSummaryDto
                    {
                        FacilityId = g.Key.FacilityId,
                        FacilityName = g.Key.FacilityName,
                        ResponseCount = g.Select(s => s.StudentId).Distinct().Count(),
                        AverageRating = allAnswers.Count > 0
                            ? (decimal?)Math.Round(allAnswers.Average(a => a.RateVal), 2)
                            : null
                    };
                })
                .OrderBy(x => x.FacilityName)
                .ToList();

            return new OkObjectResult(data);
        }

        // -----------------------------------------------------------------
        // ADMIN/QA: per-facility detail
        // -----------------------------------------------------------------
        public async Task<ActionResult<FacilityEvaluationDetailResultDto>> GetFacilityResultsAsync(
            int facilityId, string? schoolYear = null, int? term = null, int? sessionId = null,
            CancellationToken ct = default)
        {
            var facility = await _context.Facilities.AsNoTracking()
                .FirstOrDefaultAsync(f => f.FacilityId == facilityId, ct);
            if (facility is null)
                return new NotFoundObjectResult(new { message = "Facility not found." });

            var query = _context.FacilityEvaluationSubmissions.AsNoTracking()
                .Where(s => s.FacilityId == facilityId);

            if (sessionId.HasValue) query = query.Where(s => s.EvaluationSessionId == sessionId.Value);
            if (!string.IsNullOrWhiteSpace(schoolYear)) query = query.Where(s => s.SchoolYear == schoolYear);
            if (term.HasValue) query = query.Where(s => s.Term == term.Value);

            var submissions = await query.Include(s => s.Answers).ToListAsync(ct);
            var allAnswers = submissions.SelectMany(s => s.Answers).ToList();
            var responseCount = submissions.Select(s => s.StudentId).Distinct().Count();

            var categoryResults = allAnswers
                .GroupBy(a => a.CategoryId)
                .Select(g => new FacilityEvaluationCategoryResultDto
                {
                    CategoryId = g.Key,
                    ResponseCount = g.Count(),
                    AverageRating = g.Any()
                        ? (decimal?)Math.Round(g.Average(a => a.RateVal), 2)
                        : null
                })
                .ToList();

            var questionResults = allAnswers
                .GroupBy(a => new { a.QuestionId, a.CategoryId })
                .Select(g => new FacilityEvaluationQuestionResultDto
                {
                    QuestionId = g.Key.QuestionId,
                    CategoryId = g.Key.CategoryId,
                    ResponseCount = g.Count(),
                    AverageRating = g.Any()
                        ? (decimal?)Math.Round(g.Average(a => a.RateVal), 2)
                        : null
                })
                .ToList();

            // Enrich with names
            var categoryIds = categoryResults.Select(c => c.CategoryId).ToList();
            var categories = await _context.TeacherEvaluationCategories.AsNoTracking()
                .Where(c => categoryIds.Contains(c.CategoryId)).ToListAsync(ct);
            var catMap = categories.ToDictionary(c => c.CategoryId);
            foreach (var c in categoryResults)
            {
                if (catMap.TryGetValue(c.CategoryId, out var cat))
                {
                    c.CategoryName = cat.CategoryName;
                    c.CategoryRate = cat.CategoryRate;
                }
            }

            var questionIds = questionResults.Select(q => q.QuestionId).ToList();
            var questions = await _context.EvaluationFormQuestions.AsNoTracking()
                .Where(q => questionIds.Contains(q.FormQuestionId)).ToListAsync(ct);
            var qMap = questions.ToDictionary(q => q.FormQuestionId);
            foreach (var q in questionResults)
                if (qMap.TryGetValue(q.QuestionId, out var qn)) q.QuestionName = qn.QuestionName;

            var detail = new FacilityEvaluationDetailResultDto
            {
                FacilityId = facility.FacilityId,
                FacilityName = facility.FacilityName,
                ResponseCount = responseCount,
                AverageRating = allAnswers.Count > 0
                    ? (decimal?)Math.Round(allAnswers.Average(a => a.RateVal), 2)
                    : null,
                Categories = categoryResults.OrderBy(c => c.CategoryId).ToList(),
                Questions = questionResults.OrderBy(q => q.CategoryId).ThenBy(q => q.QuestionId).ToList()
            };

            return new OkObjectResult(detail);
        }

        // -----------------------------------------------------------------
        // ADMIN/QA: raw submission list for one facility session
        // -----------------------------------------------------------------
        public async Task<ActionResult<List<FacilitySessionSubmissionDto>>> GetSessionSubmissionsAsync(
            int sessionId, CancellationToken ct = default)
        {
            var session = await _context.EvaluationSessions.AsNoTracking()
                .Include(s => s.EvaluationForm)
                    .ThenInclude(f => f.EvaluationType)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == sessionId, ct);
            if (session is null)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            if (!FormHelper.IsFacilityEvaluation(session.EvaluationForm.EvaluationType))
                return new BadRequestObjectResult(new
                {
                    message = "This session is not a facility evaluation session."
                });

            var data = await _context.FacilityEvaluationSubmissions.AsNoTracking()
                .Where(s => s.EvaluationSessionId == sessionId)
                .OrderByDescending(s => s.SubmittedAtUtc)
                .Select(s => new FacilitySessionSubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    StudentId = s.StudentId,
                    StudentName = (s.Student.User.FirstName + " " + s.Student.User.LastName).Trim(),
                    StudentNumber = s.Student.StudentNumber,
                    FacilityId = s.FacilityId,
                    FacilityName = s.Facility.FacilityName,
                    SubmittedAtUtc = s.SubmittedAtUtc
                })
                .ToListAsync(ct);

            return new OkObjectResult(data);
        }

        // -----------------------------------------------------------------
        // HELPERS
        // -----------------------------------------------------------------
        /// <summary>
        /// Resolves the student's numeric grade level. Prefers the most recent term
        /// enrollment (canonical numeric value) and falls back to the legacy free-text
        /// Student.GradeLevel. Returns null when neither yields a usable grade.
        /// </summary>
        private async Task<int?> ResolveStudentGradeAsync(Student student, CancellationToken ct)
        {
            var enrollmentGrade = await _context.StudentTermEnrollments.AsNoTracking()
                .Where(e => e.StudentId == student.StudentId)
                .OrderByDescending(e => e.SchoolYear)
                .ThenByDescending(e => e.Term)
                .Select(e => (int?)e.GradeLevel)
                .FirstOrDefaultAsync(ct);

            return enrollmentGrade ?? GradeLevelHelper.ParseDisplay(student.GradeLevel);
        }
    }
}