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

            var now = DateTime.UtcNow;

            // Facility sessions currently open for this student's SY/Term
            var sessionsQuery = _context.EvaluationSessions.AsNoTracking()
                .Include(s => s.Form).ThenInclude(f => f.EvaluationType)
                .Where(s =>
                    s.Status == EvaluationSessionStatus.Open &&
                    (!s.OpensAtUtc.HasValue || s.OpensAtUtc <= now) &&
                    (!s.ClosesAtUtc.HasValue || s.ClosesAtUtc > now) &&
                    s.Form.TargetType == "Facility" &&
                    s.Form.IsActive == 1);

            if (!string.IsNullOrWhiteSpace(schoolYear))
                sessionsQuery = sessionsQuery.Where(s => s.SchoolYear == schoolYear);
            if (term.HasValue)
                sessionsQuery = sessionsQuery.Where(s => s.Term == term.Value);

            var sessions = await sessionsQuery.ToListAsync(ct);
            if (sessions.Count == 0)
                return new OkObjectResult(new List<FacilityEvaluationAssignmentDto>());

            var sessionIds = sessions.Select(s => s.EvaluationSessionId).ToList();

            var facilities = await _context.Facilities.AsNoTracking()
                .Where(f => f.IsActive)
                .OrderBy(f => f.FacilityName)
                .ToListAsync(ct);

            if (facilities.Count == 0)
                return new OkObjectResult(new List<FacilityEvaluationAssignmentDto>());

            var submissions = await _context.FacilityEvaluationSubmissions.AsNoTracking()
                .Where(s => s.StudentId == student.StudentId &&
                            s.EvaluationSessionId.HasValue &&
                            sessionIds.Contains(s.EvaluationSessionId.Value))
                .ToListAsync(ct);

            var submissionLookup = submissions
                .GroupBy(s => (s.EvaluationSessionId!.Value, s.FacilityId))
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SubmittedAtUtc).First());

            var result = new List<FacilityEvaluationAssignmentDto>();

            foreach (var session in sessions)
            {
                foreach (var facility in facilities)
                {
                    submissionLookup.TryGetValue(
                        (session.EvaluationSessionId, facility.FacilityId), out var sub);

                    var item = new FacilityEvaluationAssignmentDto
                    {
                        FacilityId = facility.FacilityId,
                        FacilityName = facility.FacilityName,
                        Description = facility.Description,
                        Location = facility.Location,
                        EvaluationTypeId = session.Form.EvaluationTypeId,
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
                .Include(s => s.Form)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == request.EvaluationSessionId, ct);

            if (session is null)
                return new BadRequestObjectResult(new { message = "Evaluation session not found." });

            if (session.Form.TargetType != "Facility")
                return new BadRequestObjectResult(new
                {
                    message = "The selected evaluation session is not a facility evaluation session."
                });

            if (!EvaluationSessionHelper.IsWithinWindow(session, DateTime.UtcNow))
                return new BadRequestObjectResult(new { message = "This evaluation session is not open." });

            if (request.FormId.HasValue && request.FormId.Value != session.FormId)
                return new BadRequestObjectResult(new
                {
                    message = "The submitted FormId does not match the evaluation session."
                });

            if (request.EvaluationTypeId != session.Form.EvaluationTypeId)
                return new BadRequestObjectResult(new
                {
                    message = "EvaluationTypeId does not match the evaluation session's form."
                });

            if (!await _context.EvaluationTypes
                    .AnyAsync(t => t.EvaluationTypeId == request.EvaluationTypeId, ct))
                return new BadRequestObjectResult(new { message = "Evaluation type not found." });

            // Validate the questions belong to this evaluation type and exist
            var qnNos = request.Answers.Select(a => a.QnNo).Distinct().ToList();
            var validQuestions = await _context.TeacherEvaluationResults.AsNoTracking()
                .Where(q => qnNos.Contains(q.QnNo))
                .Select(q => new { q.QnNo, q.CatNo, q.Category.EvaluationTypeId })
                .ToListAsync(ct);

            if (validQuestions.Count != qnNos.Count)
                return new BadRequestObjectResult(new { message = "One or more question ids are invalid." });

            if (validQuestions.Any(q => q.EvaluationTypeId != request.EvaluationTypeId))
                return new BadRequestObjectResult(new
                {
                    message = "One or more questions do not belong to the selected evaluation type."
                });

            // Validate rating values (0..5 is the current frontend scale; extend if needed)
            if (request.Answers.Any(a => a.RateVal < 0 || a.RateVal > 5))
                return new BadRequestObjectResult(new
                {
                    message = "RateVal must be between 0 and 5."
                });

            // Duplicate submission check
            var duplicate = await _context.FacilityEvaluationSubmissions.AnyAsync(s =>
                s.StudentId == student.StudentId &&
                s.FacilityId == facility.FacilityId &&
                s.EvaluationSessionId == session.EvaluationSessionId, ct);
            if (duplicate)
                return new ConflictObjectResult(new
                {
                    message = "This facility has already been evaluated for this session."
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
                        CatNo = a.CatNo,
                        QnNo = a.QnNo,
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
                .GroupBy(a => a.CatNo)
                .Select(g => new FacilityEvaluationCategoryResultDto
                {
                    CatNo = g.Key,
                    ResponseCount = g.Count(),
                    AverageRating = g.Any()
                        ? (decimal?)Math.Round(g.Average(a => a.RateVal), 2)
                        : null
                })
                .ToList();

            var questionResults = allAnswers
                .GroupBy(a => new { a.QnNo, a.CatNo })
                .Select(g => new FacilityEvaluationQuestionResultDto
                {
                    QnNo = g.Key.QnNo,
                    CatNo = g.Key.CatNo,
                    ResponseCount = g.Count(),
                    AverageRating = g.Any()
                        ? (decimal?)Math.Round(g.Average(a => a.RateVal), 2)
                        : null
                })
                .ToList();

            // Enrich with names
            var catNos = categoryResults.Select(c => c.CatNo).ToList();
            var categories = await _context.TeacherEvaluationCategories.AsNoTracking()
                .Where(c => catNos.Contains(c.CatNo)).ToListAsync(ct);
            var catMap = categories.ToDictionary(c => c.CatNo);
            foreach (var c in categoryResults)
            {
                if (catMap.TryGetValue(c.CatNo, out var cat))
                {
                    c.CatName = cat.CatName;
                    c.CatRn = cat.CatRn;
                    c.CatRate = cat.CatRate;
                }
            }

            var qnNos = questionResults.Select(q => q.QnNo).ToList();
            var questions = await _context.TeacherEvaluationResults.AsNoTracking()
                .Where(q => qnNos.Contains(q.QnNo)).ToListAsync(ct);
            var qMap = questions.ToDictionary(q => q.QnNo);
            foreach (var q in questionResults)
                if (qMap.TryGetValue(q.QnNo, out var qn)) q.QnName = qn.QnName;

            var detail = new FacilityEvaluationDetailResultDto
            {
                FacilityId = facility.FacilityId,
                FacilityName = facility.FacilityName,
                ResponseCount = responseCount,
                AverageRating = allAnswers.Count > 0
                    ? (decimal?)Math.Round(allAnswers.Average(a => a.RateVal), 2)
                    : null,
                Categories = categoryResults.OrderBy(c => c.CatNo).ToList(),
                Questions = questionResults.OrderBy(q => q.CatNo).ThenBy(q => q.QnNo).ToList()
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
                .Include(s => s.Form)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == sessionId, ct);
            if (session is null)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            if (session.Form.TargetType != "Facility")
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
    }
}