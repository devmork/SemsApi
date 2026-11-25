using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Evaluation;
using SemsApi.DTO.EvaluationSessions;
using SemsApi.DTO.Form;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Utilities;
using SemsApi.Models.Eval;

namespace SemsApi.Services
{
    public class EvaluationSessionService : IEvaluationSessionService
    {
        private readonly ApplicationDbContext _context;

        public EvaluationSessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<List<EvaluationSessionDto>>> GetAllAsync(
            int? formId = null,
            string? schoolYear = null,
            int? term = null,
            EvaluationSessionStatus? status = null,
            bool onlyOpen = false,
            bool studentView = false,
            int? studentUserId = null,
            CancellationToken ct = default)
        {
            var query = _context.EvaluationSessions
                .AsNoTracking()
                .AsQueryable();

            if (formId.HasValue)
                query = query.Where(s => s.FormId == formId.Value);

            if (!string.IsNullOrWhiteSpace(schoolYear))
                query = query.Where(s => s.SchoolYear == schoolYear.Trim());

            if (term.HasValue)
                query = query.Where(s => s.Term == term.Value);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (onlyOpen || studentView)
            {
                var now = DateTime.UtcNow;
                query = query.Where(s =>
                    s.Status == EvaluationSessionStatus.Open &&
                    (!s.OpensAtUtc.HasValue || s.OpensAtUtc <= now) &&
                    (!s.ClosesAtUtc.HasValue || s.ClosesAtUtc > now));
            }

            var sessions = await query
                .Include(s => s.EvaluationForm)
                    .ThenInclude(f => f.EvaluationType)
                .Include(s => s.Facility)
                .OrderByDescending(s => s.CreatedAtUtc)
                .ThenByDescending(s => s.EvaluationSessionId)
                .ToListAsync(ct);

            if (studentView)
            {
                var studentGrade = await ResolveStudentGradeAsync(studentUserId, ct);
                sessions = sessions
                    .Where(s => IsSessionInStudentGradeScope(s, studentGrade))
                    .ToList();
            }

            var sessionIds = sessions
                .Select(s => s.EvaluationSessionId)
                .ToList();

            var counts = await GetSubmissionCountsAsync(sessionIds, ct);

            var data = sessions
                .Select(s => EvaluationSessionHelper.MapToDto(
                    s,
                    counts.GetValueOrDefault(s.EvaluationSessionId)))
                .ToList();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<EvaluationSessionDetailDto>> GetByIdAsync(
            int id,
            bool studentView = false,
            int? studentUserId = null,
            CancellationToken ct = default)
        {
            var session = await _context.EvaluationSessions
                .AsNoTracking()
                .Include(s => s.EvaluationForm)
                    .ThenInclude(f => f.EvaluationType)
                .Include(s => s.Facility)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == id, ct);

            if (session is null)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            if (studentView)
            {
                if (!EvaluationSessionHelper.IsWithinWindow(session, DateTime.UtcNow))
                    return new NotFoundObjectResult(new { message = "Evaluation session not found." });

                var studentGrade = await ResolveStudentGradeAsync(studentUserId, ct);
                if (!IsSessionInStudentGradeScope(session, studentGrade))
                    return new NotFoundObjectResult(new { message = "Evaluation session not found." });
            }

            var submissionCount = await GetSubmissionCountAsync(id, ct);

            var detail = new EvaluationSessionDetailDto
            {
                EvaluationSessionId = session.EvaluationSessionId,
                FormId = session.FormId,
                FormTitle = session.EvaluationForm.Title,
                EvaluationTypeId = session.EvaluationForm.EvaluationTypeId,
                EvaluationTypeName = session.EvaluationForm.EvaluationType.EvaluationTypeName,
                FacilityId = session.FacilityId,
                FacilityName = session.Facility?.FacilityName,
                SchoolYear = session.SchoolYear,
                Term = session.Term,
                GradeLevelFrom = session.GradeLevelFrom,
                GradeLevelTo = session.GradeLevelTo,
                OpensAtUtc = session.OpensAtUtc,
                ClosesAtUtc = session.ClosesAtUtc,
                Status = session.Status.ToString(),
                Notes = session.Notes,
                CreatedBy = session.CreatedBy,
                CreatedAtUtc = session.CreatedAtUtc,
                UpdatedAtUtc = session.UpdatedAtUtc,
                SubmissionCount = submissionCount,
                IsAvailableNow = EvaluationSessionHelper.IsWithinWindow(session, DateTime.UtcNow),
                EvaluationForm = await BuildFormDetailAsync(session.EvaluationForm, ct)
            };

            return new OkObjectResult(detail);
        }

        public async Task<ActionResult> CreateAsync(
            EvaluationSessionCreateRequest request,
            int createdBy,
            CancellationToken ct = default)
        {
            var validationError = EvaluationSessionHelper.ValidateCreate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var EvaluationForm = await _context.Forms
                .AsNoTracking()
                .Include(f => f.EvaluationType)
                .FirstOrDefaultAsync(f => f.FormId == request.FormId, ct);

            if (EvaluationForm is null)
                return new BadRequestObjectResult(new { message = "EvaluationForm not found." });

            var isFacilityForm = FormHelper.IsFacilityEvaluation(EvaluationForm.EvaluationType);

            if (isFacilityForm && !request.FacilityId.HasValue)
                return new BadRequestObjectResult(new
                {
                    message = "FacilityId is required for a facility evaluation session."
                });

            if (!isFacilityForm && request.FacilityId.HasValue)
                return new BadRequestObjectResult(new
                {
                    message = "FacilityId is only valid for facility evaluation sessions."
                });

            if (request.FacilityId.HasValue)
            {
                var facilityExists = await _context.Facilities
                    .AnyAsync(f => f.FacilityId == request.FacilityId.Value && f.IsActive, ct);

                if (!facilityExists)
                    return new BadRequestObjectResult(new { message = "Facility not found or inactive." });
            }

            var schoolYear = request.SchoolYear.Trim();

            var duplicateExists = await _context.EvaluationSessions
                .AnyAsync(s =>
                    s.FormId == request.FormId &&
                    s.FacilityId == request.FacilityId &&
                    s.SchoolYear == schoolYear &&
                    s.Term == request.Term &&
                    s.GradeLevelFrom == request.GradeLevelFrom &&
                    s.GradeLevelTo == request.GradeLevelTo &&
                    s.Status != EvaluationSessionStatus.Archived,
                    ct);

            if (duplicateExists)
            {
                return new ConflictObjectResult(new
                {
                    message = "An evaluation session already exists for this EvaluationForm, SchoolYear, Term and grade range. Archive the existing session before creating another."
                });
            }

            var entity = new EvaluationSession
            {
                FormId = request.FormId,
                FacilityId = request.FacilityId,
                SchoolYear = schoolYear,
                Term = request.Term,
                GradeLevelFrom = request.GradeLevelFrom,
                GradeLevelTo = request.GradeLevelTo,
                OpensAtUtc = request.OpensAtUtc,
                ClosesAtUtc = request.ClosesAtUtc,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                Status = EvaluationSessionStatus.Draft,
                CreatedBy = createdBy,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.EvaluationSessions.Add(entity);
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (Utilities.DbUpdateExceptionHelper.IsUniqueConstraintViolation(ex))
            {
                return new ConflictObjectResult(new
                {
                    message = "An evaluation session already exists for this Form, SchoolYear and Term. Archive the existing session before creating another."
                });
            }

            var created = await _context.EvaluationSessions
                .Include(s => s.EvaluationForm)
                    .ThenInclude(f => f.EvaluationType)
                .Include(s => s.Facility)
                .FirstAsync(s => s.EvaluationSessionId == entity.EvaluationSessionId, ct);

            // A session created through the QA dialog is immediately visible to
            // students when the form and window allow it; otherwise it stays a
            // Draft until someone opens it.
            var openBlockingError = await GetOpenBlockingErrorAsync(created, ct);
            if (openBlockingError is null)
            {
                created.Status = EvaluationSessionStatus.Open;
                await _context.SaveChangesAsync(ct);
            }

            return new CreatedResult(
                $"/api/evaluation/sessions/{entity.EvaluationSessionId}",
                new
                {
                    message = "Evaluation session created successfully.",
                    data = EvaluationSessionHelper.MapToDto(created)
                });
        }

        public async Task<ActionResult> UpdateAsync(
            int id,
            EvaluationSessionUpdateRequest request,
            bool allowReopen = false,
            CancellationToken ct = default)
        {
            var session = await _context.EvaluationSessions
                .Include(s => s.EvaluationForm)
                    .ThenInclude(f => f.EvaluationType)
                .Include(s => s.Facility)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == id, ct);

            if (session is null)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            if (session.Status == EvaluationSessionStatus.Archived)
            {
                return new ConflictObjectResult(new
                {
                    message = "Archived evaluation sessions cannot be modified."
                });
            }

            var validationError = EvaluationSessionHelper.ValidateUpdate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var targetStatus = request.Status ?? session.Status;

            if (targetStatus != session.Status)
            {
                if (session.Status == EvaluationSessionStatus.Closed &&
                    targetStatus == EvaluationSessionStatus.Open &&
                    !allowReopen)
                {
                    return new ObjectResult(new
                    {
                        message = "Closed evaluation sessions can only be reopened by an Admin."
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                if (!EvaluationSessionHelper.CanTransition(
                        session.Status,
                        targetStatus,
                        allowReopen))
                {
                    return new ConflictObjectResult(new
                    {
                        message = $"Cannot change status from {session.Status} to {targetStatus}."
                    });
                }
            }

            if (session.Status == EvaluationSessionStatus.Closed &&
                (request.OpensAtUtc.HasValue || request.ClosesAtUtc.HasValue))
            {
                return new ConflictObjectResult(new
                {
                    message = "Closed evaluation sessions cannot change their date window."
                });
            }

            if (request.OpensAtUtc.HasValue)
                session.OpensAtUtc = request.OpensAtUtc;

            if (request.ClosesAtUtc.HasValue)
                session.ClosesAtUtc = request.ClosesAtUtc;

            if (request.GradeLevelFrom.HasValue)
                session.GradeLevelFrom = request.GradeLevelFrom;

            if (request.GradeLevelTo.HasValue)
                session.GradeLevelTo = request.GradeLevelTo;

            var gradeError = EvaluationSessionHelper.ValidateGradeRange(
                session.GradeLevelFrom,
                session.GradeLevelTo);
            if (gradeError is not null)
                return new BadRequestObjectResult(new { message = gradeError });

            if (request.Notes is not null)
            {
                session.Notes = string.IsNullOrWhiteSpace(request.Notes)
                    ? null
                    : request.Notes.Trim();
            }

            if (session.OpensAtUtc.HasValue &&
                session.ClosesAtUtc.HasValue &&
                session.OpensAtUtc.Value >= session.ClosesAtUtc.Value)
            {
                return new BadRequestObjectResult(new
                {
                    message = "OpensAtUtc must be earlier than ClosesAtUtc."
                });
            }

            if (targetStatus == EvaluationSessionStatus.Open &&
                session.Status != EvaluationSessionStatus.Open)
            {
                var blockingError = await GetOpenBlockingErrorAsync(session, ct);
                if (blockingError is not null)
                    return new BadRequestObjectResult(new { message = blockingError });
            }

            session.Status = targetStatus;
            session.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var submissionCount = await GetSubmissionCountAsync(session.EvaluationSessionId, ct);

            return new OkObjectResult(new
            {
                message = "Evaluation session updated successfully.",
                data = EvaluationSessionHelper.MapToDto(session, submissionCount)
            });
        }

        public async Task<ActionResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var session = await _context.EvaluationSessions
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == id, ct);

            if (session is null)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            if (session.Status == EvaluationSessionStatus.Open)
            {
                return new ConflictObjectResult(new
                {
                    message = "Open evaluation sessions must be closed before deletion."
                });
            }

            var hasTeacherSubmissions = await _context.TeacherEvaluationSubmissions
                .AnyAsync(s => s.EvaluationSessionId == id, ct);

            if (hasTeacherSubmissions)
            {
                return new ConflictObjectResult(new
                {
                    message = "Cannot delete this evaluation session because submissions already reference it. Archive it instead."
                });
            }

            var hasFacilitySubmissions = await _context.FacilityEvaluationSubmissions
                .AnyAsync(s => s.EvaluationSessionId == id, ct);

            if (hasFacilitySubmissions)
            {
                return new ConflictObjectResult(new
                {
                    message = "Cannot delete this evaluation session because submissions already reference it. Archive it instead."
                });
            }

            _context.EvaluationSessions.Remove(session);
            await _context.SaveChangesAsync(ct);

            return new NoContentResult();
        }

        public async Task<ActionResult> OpenAsync(int id, CancellationToken ct = default)
        {
            var session = await _context.EvaluationSessions
                .Include(s => s.EvaluationForm)
                    .ThenInclude(f => f.EvaluationType)
                .Include(s => s.Facility)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == id, ct);

            if (session is null)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            if (session.Status == EvaluationSessionStatus.Archived)
            {
                return new ConflictObjectResult(new
                {
                    message = "Archived evaluation sessions cannot be opened."
                });
            }

            if (session.Status == EvaluationSessionStatus.Closed)
            {
                return new ConflictObjectResult(new
                {
                    message = "Closed evaluation sessions can only be reopened by an Admin using the update endpoint."
                });
            }

            if (session.Status == EvaluationSessionStatus.Open)
            {
                var existingCount = await GetSubmissionCountAsync(id, ct);

                return new OkObjectResult(new
                {
                    message = "Evaluation session is already open.",
                    data = EvaluationSessionHelper.MapToDto(session, existingCount)
                });
            }

            var blockingError = await GetOpenBlockingErrorAsync(session, ct);
            if (blockingError is not null)
                return new BadRequestObjectResult(new { message = blockingError });

            session.Status = EvaluationSessionStatus.Open;
            session.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var submissionCount = await GetSubmissionCountAsync(id, ct);

            return new OkObjectResult(new
            {
                message = "Evaluation session opened successfully.",
                data = EvaluationSessionHelper.MapToDto(session, submissionCount)
            });
        }

        public async Task<ActionResult> CloseAsync(int id, CancellationToken ct = default)
        {
            var session = await _context.EvaluationSessions
                .Include(s => s.EvaluationForm)
                    .ThenInclude(f => f.EvaluationType)
                .Include(s => s.Facility)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == id, ct);

            if (session is null)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            if (session.Status == EvaluationSessionStatus.Archived)
            {
                return new ConflictObjectResult(new
                {
                    message = "Archived evaluation sessions cannot be closed."
                });
            }

            if (session.Status == EvaluationSessionStatus.Closed)
            {
                var existingCount = await GetSubmissionCountAsync(id, ct);

                return new OkObjectResult(new
                {
                    message = "Evaluation session is already closed.",
                    data = EvaluationSessionHelper.MapToDto(session, existingCount)
                });
            }

            session.Status = EvaluationSessionStatus.Closed;
            session.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var submissionCount = await GetSubmissionCountAsync(id, ct);

            return new OkObjectResult(new
            {
                message = "Evaluation session closed successfully.",
                data = EvaluationSessionHelper.MapToDto(session, submissionCount)
            });
        }

        public async Task<ActionResult<List<SessionSubmissionDto>>> GetSubmissionsAsync(
                int id,
                CancellationToken ct = default)
        {
            var session = await _context.EvaluationSessions
                .AsNoTracking()
                .Include(s => s.EvaluationForm)
                    .ThenInclude(f => f.EvaluationType)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == id, ct);

            if (session is null)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            if (FormHelper.IsFacilityEvaluation(session.EvaluationForm.EvaluationType))
            {
                var facilitySubmissions = await _context.FacilityEvaluationSubmissions
                    .AsNoTracking()
                    .Where(s => s.EvaluationSessionId == id)
                    .OrderByDescending(s => s.SubmittedAtUtc)
                    .Select(s => new SessionSubmissionDto
                    {
                        SubmissionId = s.SubmissionId,
                        StudentId = s.StudentId,
                        StudentName = $"{s.Student.User.FirstName} {s.Student.User.LastName}".Trim(),
                        StudentNumber = s.Student.StudentNumber,
                        SubjectCode = s.Facility.FacilityName,
                        TeacherName = "Facility",
                        SubmittedAtUtc = s.SubmittedAtUtc
                    })
                    .ToListAsync(ct);

                return new OkObjectResult(facilitySubmissions);
            }

            var submissions = await _context.TeacherEvaluationSubmissions
                .AsNoTracking()
                .Where(s => s.EvaluationSessionId == id)
                .OrderByDescending(s => s.SubmittedAtUtc)
                .Select(s => new SessionSubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    StudentId = s.StudentId,
                    StudentName = $"{s.Student.User.FirstName} {s.Student.User.LastName}".Trim(),
                    StudentNumber = s.Student.StudentNumber,
                    SubjectCode = s.SubjectOffering.Subject.Code,
                    TeacherName = $"{s.Teacher.User.FirstName} {s.Teacher.User.LastName}".Trim(),
                    SubmittedAtUtc = s.SubmittedAtUtc
                })
                .ToListAsync(ct);

            return new OkObjectResult(submissions);
        }

        private async Task<string?> GetOpenBlockingErrorAsync(
            EvaluationSession session,
            CancellationToken ct)
        {
            var EvaluationForm = session.EvaluationForm ?? await _context.Forms
                .AsNoTracking()
                .Include(f => f.EvaluationType)
                .FirstOrDefaultAsync(f => f.FormId == session.FormId, ct);

            if (EvaluationForm is null)
                return "EvaluationForm not found.";

            if (EvaluationForm.IsActive != FormHelper.Active)
                return "The EvaluationForm is inactive and cannot be opened for evaluation.";

            var incompleteError = await FormCompletenessHelper
                .GetIncompleteFormErrorAsync(_context, EvaluationForm.FormId, ct);

            if (incompleteError is not null)
                return incompleteError;

            if (session.ClosesAtUtc.HasValue &&
                session.ClosesAtUtc.Value <= DateTime.UtcNow)
            {
                return "ClosesAtUtc must be in the future to open an evaluation session.";
            }

            return null;
        }

        private async Task<FormDetailDto> BuildFormDetailAsync(
            EvaluationForm EvaluationForm,
            CancellationToken ct)
        {
            var formCategories = await _context.EvaluationFormCategories
                .AsNoTracking()
                .Include(fc => fc.EvaluationCategory)
                .Where(fc => fc.FormId == EvaluationForm.FormId && fc.IsActive)
                .OrderBy(fc => fc.DisplayOrder)
                .ThenBy(fc => fc.CategoryId)
                .ToListAsync(ct);

            var formCategoryIds = formCategories.Select(fc => fc.FormCategoryId).ToList();

            var formQuestions = await _context.EvaluationFormQuestions
                .AsNoTracking()
                .Include(q => q.FormCategory)
                .Where(q => formCategoryIds.Contains(q.FormCategoryId))
                .OrderBy(q => q.FormCategory.DisplayOrder)
                .ThenBy(q => q.DisplayOrder)
                .ToListAsync(ct);

            return new FormDetailDto
            {
                FormId = EvaluationForm.FormId,
                EvaluationTypeId = EvaluationForm.EvaluationTypeId,
                EvaluationTypeName = EvaluationForm.EvaluationType.EvaluationTypeName,
                Title = EvaluationForm.Title,
                Description = EvaluationForm.Description,
                CreatedBy = EvaluationForm.CreatedBy,
                IsActive = EvaluationForm.IsActive,
                CreatedAt = EvaluationForm.CreatedAt,
                Categories = formCategories.Select(fc => new EvaluationCategoryDto
                {
                    CategoryId = fc.CategoryId,
                    CategoryName = fc.EvaluationCategory?.CategoryName,
                    CategoryRate = fc.EvaluationCategory?.CategoryRate,
                    Questions = formQuestions
                        .Where(q => q.FormCategoryId == fc.FormCategoryId)
                        .Select(q => new EvaluationQuestionDto
                        {
                            QuestionId = q.FormQuestionId,
                            QuestionName = q.QuestionName
                        })
                        .ToList()
                }).ToList()
            };
        }

        private async Task<Dictionary<int, int>> GetSubmissionCountsAsync(
            List<int> sessionIds,
            CancellationToken ct)
        {
            if (sessionIds.Count == 0)
                return new Dictionary<int, int>();

            var teacherCounts = await _context.TeacherEvaluationSubmissions
                .AsNoTracking()
                .Where(x => x.EvaluationSessionId.HasValue &&
                            sessionIds.Contains(x.EvaluationSessionId.Value))
                .GroupBy(x => x.EvaluationSessionId!.Value)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count, ct);

            var facilityCounts = await _context.FacilityEvaluationSubmissions
                .AsNoTracking()
                .Where(x => x.EvaluationSessionId.HasValue &&
                            sessionIds.Contains(x.EvaluationSessionId.Value))
                .GroupBy(x => x.EvaluationSessionId!.Value)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count, ct);

            var totals = new Dictionary<int, int>(sessionIds.Count);
            foreach (var id in sessionIds)
            {
                totals[id] =
                    teacherCounts.GetValueOrDefault(id) +
                    facilityCounts.GetValueOrDefault(id);
            }

            return totals;
        }

        private async Task<int> GetSubmissionCountAsync(int sessionId, CancellationToken ct)
        {
            var teacherCount = await _context.TeacherEvaluationSubmissions
                .AsNoTracking()
                .CountAsync(s => s.EvaluationSessionId == sessionId, ct);

            var facilityCount = await _context.FacilityEvaluationSubmissions
                .AsNoTracking()
                .CountAsync(s => s.EvaluationSessionId == sessionId, ct);

            return teacherCount + facilityCount;
        }

        private async Task<int?> ResolveStudentGradeAsync(int? userId, CancellationToken ct)
        {
            if (!userId.HasValue)
                return null;

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId.Value, ct);

            if (student is null)
                return null;

            var enrollmentGrade = await _context.StudentTermEnrollments
                .AsNoTracking()
                .Where(e => e.StudentId == student.StudentId)
                .OrderByDescending(e => e.SchoolYear)
                .ThenByDescending(e => e.Term)
                .Select(e => (int?)e.GradeLevel)
                .FirstOrDefaultAsync(ct);

            return enrollmentGrade ?? GradeLevelHelper.ParseDisplay(student.GradeLevel);
        }

        private static bool IsSessionInStudentGradeScope(EvaluationSession session, int? studentGrade)
        {
            if (!session.GradeLevelFrom.HasValue && !session.GradeLevelTo.HasValue)
                return true;

            // A student whose grade cannot be resolved (for example a legacy
            // "N/A" profile with no term enrollment) is not excluded by a grade
            // scope. This matches the assignment lists and submission checks,
            // so a session that appears as pending can always be opened.
            if (!studentGrade.HasValue)
                return true;

            if (session.GradeLevelFrom.HasValue && studentGrade.Value < session.GradeLevelFrom.Value)
                return false;

            if (session.GradeLevelTo.HasValue && studentGrade.Value > session.GradeLevelTo.Value)
                return false;

            return true;
        }
    }
}
