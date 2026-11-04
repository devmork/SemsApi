using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.EvaluationAssignments;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    public class EvaluationAssignmentService : IEvaluationAssignmentService
    {
        private readonly ApplicationDbContext _context;
        public EvaluationAssignmentService(ApplicationDbContext context) { _context = context; }

        public Task<ActionResult<List<EvaluationAssignmentDto>>> GetAllAsync(
            int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default)
            => BuildAsync(userId, schoolYear, term, filter: null, ct);

        public Task<ActionResult<List<EvaluationAssignmentDto>>> GetPendingAsync(
            int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default)
            => BuildAsync(userId, schoolYear, term, filter: false, ct);

        public Task<ActionResult<List<EvaluationAssignmentDto>>> GetCompletedAsync(
            int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default)
            => BuildAsync(userId, schoolYear, term, filter: true, ct);

        private async Task<ActionResult<List<EvaluationAssignmentDto>>> BuildAsync(
            int userId, string? sy, int? term, bool? filter, CancellationToken ct)
        {
            var student = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (student is null) return new NotFoundObjectResult(new { message = "No student profile linked to this user." });

            var query = _context.StudentSubjectEnrollments.AsNoTracking()
                .Where(e => e.StudentId == student.StudentId && e.Status == "Enrolled")
                .Where(e => e.SubjectOffering.TeacherId != null)
                .Where(e => e.SubjectOffering.IsActive);

            if (!string.IsNullOrWhiteSpace(sy)) query = query.Where(e => e.SubjectOffering.SchoolYear == sy);
            if (term.HasValue) query = query.Where(e => e.SubjectOffering.Term == term.Value);

            var rows = await query
                .Include(e => e.SubjectOffering).ThenInclude(o => o.Subject)
                .Include(e => e.SubjectOffering).ThenInclude(o => o.Teacher).ThenInclude(t => t!.User)
                .ToListAsync(ct);

            var submissions = await _context.TeacherEvaluationSubmissions.AsNoTracking()
                .Where(s => s.StudentId == student.StudentId)
                .ToListAsync(ct);

            var submissionMap = submissions
                .GroupBy(s => s.SubjectOfferingId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SubmittedAtUtc).First());

            var dto = new List<EvaluationAssignmentDto>();
            foreach (var e in rows)
            {
                var o = e.SubjectOffering;
                var t = o.Teacher!;
                submissionMap.TryGetValue(o.SubjectOfferingId, out var sub);

                // Prefer an open EvaluationSession for the offering's SY + Term.
                // Falls back to the legacy grade-level Form resolution when no session exists.
                EvaluationSession? openSession = null;
                if (sub is null)
                {
                    openSession = await ResolveOpenSessionAsync(o.GradeLevel, o.SchoolYear, o.Term, ct);
                }

                var evaluationTypeId = sub?.EvaluationTypeId
                    ?? openSession?.Form.EvaluationTypeId
                    ?? await ResolveEvaluationTypeIdAsync(o.GradeLevel, ct);

                var formId = sub?.FormId
                    ?? openSession?.FormId
                    ?? await ResolveFormIdAsync(o.GradeLevel, ct);

                var item = new EvaluationAssignmentDto
                {
                    SubjectOfferingId = o.SubjectOfferingId,
                    SubjectId = o.SubjectId,
                    SubjectCode = o.Subject.Code,
                    SubjectDescription = o.Subject.Description,
                    TeacherId = t.TeacherId,
                    TeacherName = $"{t.User.FirstName} {t.User.LastName}".Trim(),
                    TeacherEmployeeNumber = t.EmployeeNumber,
                    GradeLevel = o.GradeLevel,
                    Section = o.Section,
                    SchoolYear = o.SchoolYear,
                    Term = o.Term,
                    IsCompleted = sub is not null,
                    SubmittedAtUtc = sub?.SubmittedAtUtc,
                    EvaluationTypeId = evaluationTypeId,
                    FormId = formId
                };

                if (filter is null || item.IsCompleted == filter.Value)
                    dto.Add(item);
            }

            return new OkObjectResult(dto.OrderBy(x => x.SubjectCode).ToList());
        }

        public async Task<ActionResult> SubmitAsync(int userId, SubmitSubjectEvaluationRequest request, CancellationToken ct = default)
        {
            var student = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (student is null) return new NotFoundObjectResult(new { message = "No student profile linked to this user." });

            if (request.Answers is null || request.Answers.Count == 0)
                return new BadRequestObjectResult(new { message = "At least one answer is required." });

            var offering = await _context.SubjectOfferings
                .Include(o => o.Subject)
                .Include(o => o.Teacher).ThenInclude(t => t!.User)
                .FirstOrDefaultAsync(o => o.SubjectOfferingId == request.SubjectOfferingId, ct);

            if (offering is null) return new BadRequestObjectResult(new { message = "Subject offering not found." });
            if (offering.TeacherId is null) return new BadRequestObjectResult(new { message = "This subject offering has no assigned teacher and cannot be evaluated." });
            if (!offering.IsActive) return new BadRequestObjectResult(new { message = "This subject offering is inactive." });

            // Verify enrollment
            var isEnrolled = await _context.StudentSubjectEnrollments.AnyAsync(e =>
                e.StudentId == student.StudentId &&
                e.SubjectOfferingId == offering.SubjectOfferingId &&
                e.Status == "Enrolled", ct);
            if (!isEnrolled)
                return new ForbidResult("The student is not enrolled in this subject offering.");

            // Duplicate submission check
            if (await _context.TeacherEvaluationSubmissions.AnyAsync(s =>
                    s.StudentId == student.StudentId &&
                    s.SubjectOfferingId == offering.SubjectOfferingId, ct))
                return new ConflictObjectResult(new { message = "This subject has already been evaluated." });

            // ---------- Resolve the evaluation session for this submission ----------
            EvaluationSession? session = null;

            if (request.EvaluationSessionId.HasValue)
            {
                session = await _context.EvaluationSessions
                    .AsNoTracking()
                    .Include(s => s.Form)
                    .FirstOrDefaultAsync(
                        s => s.EvaluationSessionId == request.EvaluationSessionId.Value,
                        ct);

                if (session is null)
                    return new BadRequestObjectResult(new { message = "Evaluation session not found." });

                if (!EvaluationSessionHelper.IsWithinWindow(session, DateTime.UtcNow))
                    return new BadRequestObjectResult(new { message = "This evaluation session is not open." });

                if (session.SchoolYear != offering.SchoolYear || session.Term != offering.Term)
                    return new BadRequestObjectResult(new
                    {
                        message = "The evaluation session does not match the subject offering's school year and term."
                    });

                if (request.FormId.HasValue && request.FormId.Value != session.FormId)
                    return new BadRequestObjectResult(new
                    {
                        message = "The submitted FormId does not match the evaluation session."
                    });
            }
            else if (request.FormId.HasValue)
            {
                // Caller supplied a FormId but no session id: try to attach the matching open session
                // for auditability. If none exists, submission still proceeds (legacy behaviour).
                session = await FindOpenSessionForFormAsync(
                    request.FormId.Value,
                    offering.SchoolYear,
                    offering.Term,
                    ct);
            }

            if (session is not null &&
                request.EvaluationTypeId != session.Form.EvaluationTypeId)
            {
                return new BadRequestObjectResult(new
                {
                    message = "EvaluationTypeId does not match the evaluation session's form."
                });
            }

            if (!await _context.EvaluationTypes.AnyAsync(t => t.EvaluationTypeId == request.EvaluationTypeId, ct))
                return new BadRequestObjectResult(new { message = "Evaluation type not found." });

            // Validate answer references
            var qnNos = request.Answers.Select(a => a.QnNo).Distinct().ToList();
            var validQnNos = await _context.TeacherEvaluationResults.AsNoTracking()
                .Where(q => qnNos.Contains(q.QnNo))
                .Select(q => q.QnNo)
                .ToListAsync(ct);
            if (validQnNos.Count != qnNos.Count)
                return new BadRequestObjectResult(new { message = "One or more question ids are invalid." });

            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var submission = new TeacherEvaluationSubmission
                {
                    StudentId = student.StudentId,
                    SubjectOfferingId = offering.SubjectOfferingId,
                    TeacherId = offering.TeacherId.Value,
                    EvaluationTypeId = request.EvaluationTypeId,
                    FormId = request.FormId ?? session?.FormId,
                    EvaluationSessionId = session?.EvaluationSessionId,
                    SchoolYear = offering.SchoolYear,
                    Term = offering.Term,
                    SubmittedAtUtc = DateTime.UtcNow
                };

                foreach (var a in request.Answers)
                {
                    submission.Answers.Add(new TeacherEvaluationAnswer
                    {
                        CatNo = a.CatNo,
                        QnNo = a.QnNo,
                        RateVal = a.RateVal
                    });
                }

                _context.TeacherEvaluationSubmissions.Add(submission);
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return new OkObjectResult(new
                {
                    message = "Evaluation submitted successfully.",
                    submissionId = submission.SubmissionId
                });
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        // ---------- helpers ----------

        /// <summary>
        /// Finds an Open EvaluationSession for the given grade level / school year / term,
        /// preferring exact ProgramId matches. Used when resolving pending assignments.
        /// </summary>
        private async Task<EvaluationSession?> ResolveOpenSessionAsync(
            int gradeLevel,
            string schoolYear,
            int term,
            CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var programId = GradeLevelToProgramId(gradeLevel);

            return await _context.EvaluationSessions
                .AsNoTracking()
                .Include(s => s.Form)
                    .ThenInclude(f => f.EvaluationType)
                .Where(s =>
                    s.Status == EvaluationSessionStatus.Open &&
                    s.SchoolYear == schoolYear &&
                    s.Term == term &&
                    (!s.OpensAtUtc.HasValue || s.OpensAtUtc <= now) &&
                    (!s.ClosesAtUtc.HasValue || s.ClosesAtUtc > now) &&
                    s.Form.IsActive == 1 &&
                    s.Form.TargetType == "SubjectTeacher" &&
                    (s.Form.ProgramId == programId || s.Form.ProgramId == 0))
                .OrderBy(s => s.Form.ProgramId == programId ? 0 : 1)
                .ThenByDescending(s => s.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Finds an Open EvaluationSession for a specific Form + SY + Term.
        /// Used at submit time when the caller only supplied a FormId.
        /// </summary>
        private async Task<EvaluationSession?> FindOpenSessionForFormAsync(
            int formId,
            string schoolYear,
            int term,
            CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            return await _context.EvaluationSessions
                .AsNoTracking()
                .Include(s => s.Form)
                .Where(s =>
                    s.FormId == formId &&
                    s.SchoolYear == schoolYear &&
                    s.Term == term &&
                    s.Status == EvaluationSessionStatus.Open &&
                    (!s.OpensAtUtc.HasValue || s.OpensAtUtc <= now) &&
                    (!s.ClosesAtUtc.HasValue || s.ClosesAtUtc > now))
                .FirstOrDefaultAsync(ct);
        }

        private static int GradeLevelToProgramId(int gradeLevel) => gradeLevel switch
        {
            >= 0 and <= 5 => 1,
            >= 6 and <= 8 => 2,
            _ => 3
        };

        private async Task<int?> ResolveEvaluationTypeIdAsync(int gradeLevel, CancellationToken ct)
        {
            var formId = await ResolveFormIdAsync(gradeLevel, ct);
            if (formId is null) return null;
            return await _context.Forms.AsNoTracking()
                .Where(f => f.FormId == formId.Value)
                .Select(f => (int?)f.EvaluationTypeId)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<int?> ResolveFormIdAsync(int gradeLevel, CancellationToken ct)
        {
            // ProgramId: 0=all, 1=PreK-G3, 2=G4-G6, 3=JHS-SHS
            var programId = GradeLevelToProgramId(gradeLevel);

            var form = await _context.Forms.AsNoTracking()
                .Where(f => f.IsActive == 1
                         && f.TargetType == "SubjectTeacher"
                         && (f.ProgramId == programId || f.ProgramId == 0))
                .OrderBy(f => f.ProgramId == programId ? 0 : 1) // prefer exact match
                .ThenByDescending(f => f.CreatedAt)
                .Select(f => (int?)f.FormId)
                .FirstOrDefaultAsync(ct);

            return form;
        }
    }
}