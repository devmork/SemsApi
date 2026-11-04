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
                .Include(s => s.Form)
                    .ThenInclude(f => f.EvaluationType)
                .OrderByDescending(s => s.CreatedAtUtc)
                .ThenByDescending(s => s.EvaluationSessionId)
                .ToListAsync(ct);

            var sessionIds = sessions
                .Select(s => s.EvaluationSessionId)
                .ToList();

            var counts = await _context.TeacherEvaluationSubmissions
                .AsNoTracking()
                .Where(x => x.EvaluationSessionId.HasValue &&
                            sessionIds.Contains(x.EvaluationSessionId.Value))
                .GroupBy(x => x.EvaluationSessionId!.Value)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count, ct);

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
            CancellationToken ct = default)
        {
            var session = await _context.EvaluationSessions
                .AsNoTracking()
                .Include(s => s.Form)
                    .ThenInclude(f => f.EvaluationType)
                .FirstOrDefaultAsync(s => s.EvaluationSessionId == id, ct);

            if (session is null)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            if (studentView &&
                !EvaluationSessionHelper.IsWithinWindow(session, DateTime.UtcNow))
            {
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });
            }

            var submissionCount = await _context.TeacherEvaluationSubmissions
                .AsNoTracking()
                .CountAsync(s => s.EvaluationSessionId == id, ct);

            var detail = new EvaluationSessionDetailDto
            {
                EvaluationSessionId = session.EvaluationSessionId,
                FormId = session.FormId,
                FormCode = session.Form.Code,
                FormTitle = session.Form.Title,
                EvaluationTypeId = session.Form.EvaluationTypeId,
                EvaluationTypeName = session.Form.EvaluationType.EvaluationTypeName,
                TargetType = session.Form.TargetType,
                ProgramId = session.Form.ProgramId,
                SchoolYear = session.SchoolYear,
                Term = session.Term,
                OpensAtUtc = session.OpensAtUtc,
                ClosesAtUtc = session.ClosesAtUtc,
                Status = session.Status.ToString(),
                Notes = session.Notes,
                CreatedBy = session.CreatedBy,
                CreatedAtUtc = session.CreatedAtUtc,
                UpdatedAtUtc = session.UpdatedAtUtc,
                SubmissionCount = submissionCount,
                IsAvailableNow = EvaluationSessionHelper.IsWithinWindow(session, DateTime.UtcNow),
                Form = await BuildFormDetailAsync(session.Form, ct)
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

            var form = await _context.Forms
                .AsNoTracking()
                .Include(f => f.EvaluationType)
                .FirstOrDefaultAsync(f => f.FormId == request.FormId, ct);

            if (form is null)
                return new BadRequestObjectResult(new { message = "Form not found." });

            var schoolYear = request.SchoolYear.Trim();

            var duplicateExists = await _context.EvaluationSessions
                .AnyAsync(s =>
                    s.FormId == request.FormId &&
                    s.SchoolYear == schoolYear &&
                    s.Term == request.Term &&
                    s.Status != EvaluationSessionStatus.Archived,
                    ct);

            if (duplicateExists)
            {
                return new ConflictObjectResult(new
                {
                    message = "An evaluation session already exists for this Form, SchoolYear and Term. Archive the existing session before creating another."
                });
            }

            var entity = new EvaluationSession
            {
                FormId = request.FormId,
                SchoolYear = schoolYear,
                Term = request.Term,
                OpensAtUtc = request.OpensAtUtc,
                ClosesAtUtc = request.ClosesAtUtc,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                Status = EvaluationSessionStatus.Draft,
                CreatedBy = createdBy,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.EvaluationSessions.Add(entity);
            await _context.SaveChangesAsync(ct);

            var created = await _context.EvaluationSessions
                .AsNoTracking()
                .Include(s => s.Form)
                    .ThenInclude(f => f.EvaluationType)
                .FirstAsync(s => s.EvaluationSessionId == entity.EvaluationSessionId, ct);

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
                .Include(s => s.Form)
                    .ThenInclude(f => f.EvaluationType)
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

            var submissionCount = await _context.TeacherEvaluationSubmissions
                .AsNoTracking()
                .CountAsync(s => s.EvaluationSessionId == session.EvaluationSessionId, ct);

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

            var hasSubmissions = await _context.TeacherEvaluationSubmissions
                .AnyAsync(s => s.EvaluationSessionId == id, ct);

            if (hasSubmissions)
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
                .Include(s => s.Form)
                    .ThenInclude(f => f.EvaluationType)
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
                var existingCount = await _context.TeacherEvaluationSubmissions
                    .AsNoTracking()
                    .CountAsync(s => s.EvaluationSessionId == id, ct);

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

            var submissionCount = await _context.TeacherEvaluationSubmissions
                .AsNoTracking()
                .CountAsync(s => s.EvaluationSessionId == id, ct);

            return new OkObjectResult(new
            {
                message = "Evaluation session opened successfully.",
                data = EvaluationSessionHelper.MapToDto(session, submissionCount)
            });
        }

        public async Task<ActionResult> CloseAsync(int id, CancellationToken ct = default)
        {
            var session = await _context.EvaluationSessions
                .Include(s => s.Form)
                    .ThenInclude(f => f.EvaluationType)
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
                var existingCount = await _context.TeacherEvaluationSubmissions
                    .AsNoTracking()
                    .CountAsync(s => s.EvaluationSessionId == id, ct);

                return new OkObjectResult(new
                {
                    message = "Evaluation session is already closed.",
                    data = EvaluationSessionHelper.MapToDto(session, existingCount)
                });
            }

            session.Status = EvaluationSessionStatus.Closed;
            session.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var submissionCount = await _context.TeacherEvaluationSubmissions
                .AsNoTracking()
                .CountAsync(s => s.EvaluationSessionId == id, ct);

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
            // 1. Verify the session exists
            var sessionExists = await _context.EvaluationSessions
                .AsNoTracking()
                .AnyAsync(s => s.EvaluationSessionId == id, ct);

            if (!sessionExists)
                return new NotFoundObjectResult(new { message = "Evaluation session not found." });

            // 2. Fetch the submissions with Student, Subject, and Teacher details
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
            var form = session.Form ?? await _context.Forms
                .AsNoTracking()
                .Include(f => f.EvaluationType)
                .FirstOrDefaultAsync(f => f.FormId == session.FormId, ct);

            if (form is null)
                return "Form not found.";

            if (form.IsActive != FormHelper.Active)
                return "The form is inactive and cannot be opened for evaluation.";

            var incompleteError = await FormCompletenessHelper
                .GetIncompleteFormErrorAsync(_context, form.EvaluationTypeId, ct);

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
            Form form,
            CancellationToken ct)
        {
            var categories = await _context.TeacherEvaluationCategories
                .AsNoTracking()
                .Where(c => c.EvaluationTypeId == form.EvaluationTypeId)
                .OrderBy(c => c.CatNo)
                .ToListAsync(ct);

            var categoryNos = categories
                .Select(c => c.CatNo)
                .ToList();

            var questions = await _context.TeacherEvaluationResults
                .AsNoTracking()
                .Where(q => categoryNos.Contains(q.CatNo))
                .OrderBy(q => q.CatNo)
                .ThenBy(q => q.QnNo)
                .ToListAsync(ct);

            return new FormDetailDto
            {
                FormId = form.FormId,
                Code = form.Code,
                GradeBand = form.GradeBand,
                TargetType = form.TargetType,
                ProgramId = form.ProgramId,
                EvaluationTypeId = form.EvaluationTypeId,
                EvaluationTypeName = form.EvaluationType.EvaluationTypeName,
                Title = form.Title,
                Description = form.Description,
                CreatedBy = form.CreatedBy,
                IsActive = form.IsActive,
                CreatedAt = form.CreatedAt,
                Categories = categories.Select(c => new EvaluationCategoryDto
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
                }).ToList()
            };
        }
    }
}