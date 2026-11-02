using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.EvaluationAssignments;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Models.Eval;

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
                    EvaluationTypeId = sub?.EvaluationTypeId ?? await ResolveEvaluationTypeIdAsync(o.GradeLevel, ct),
                    FormId = sub?.FormId ?? await ResolveFormIdAsync(o.GradeLevel, ct)
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
                    FormId = request.FormId,
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
            var programId = gradeLevel switch
            {
                >= 0 and <= 5 => 1,
                >= 6 and <= 8 => 2,
                _ => 3
            };

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