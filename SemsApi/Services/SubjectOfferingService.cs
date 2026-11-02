using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.SubjectOfferings;
using SemsApi.Interfaces;
using SemsApi.Models;

namespace SemsApi.Services
{
    public class SubjectOfferingService : ISubjectOfferingService
    {
        private readonly ApplicationDbContext _context;
        public SubjectOfferingService(ApplicationDbContext context) { _context = context; }

        public async Task<ActionResult<List<SubjectOfferingDto>>> GetAllAsync(
            int? gradeLevel = null, string? section = null, string? schoolYear = null,
            int? term = null, int? teacherId = null, CancellationToken ct = default)
        {
            var query = _context.SubjectOfferings.AsNoTracking().AsQueryable();

            if (gradeLevel.HasValue) query = query.Where(o => o.GradeLevel == gradeLevel.Value);
            if (!string.IsNullOrWhiteSpace(section)) query = query.Where(o => o.Section == section);
            if (!string.IsNullOrWhiteSpace(schoolYear)) query = query.Where(o => o.SchoolYear == schoolYear);
            if (term.HasValue) query = query.Where(o => o.Term == term.Value);
            if (teacherId.HasValue) query = query.Where(o => o.TeacherId == teacherId.Value);

            var data = await query
                .OrderBy(o => o.GradeLevel).ThenBy(o => o.Section).ThenBy(o => o.Subject.Code)
                .Select(o => new SubjectOfferingDto
                {
                    SubjectOfferingId = o.SubjectOfferingId,
                    SubjectId = o.SubjectId,
                    SubjectCode = o.Subject.Code,
                    SubjectDescription = o.Subject.Description,
                    EdpCode = o.EdpCode,
                    ProgramCode = o.ProgramCode,
                    GradeLevel = o.GradeLevel,
                    Section = o.Section,
                    SchoolYear = o.SchoolYear,
                    Term = o.Term,
                    TeacherId = o.TeacherId,
                    TeacherName = o.Teacher == null ? null : (o.Teacher.User.FirstName + " " + o.Teacher.User.LastName).Trim(),
                    TeacherEmployeeNumber = o.Teacher == null ? null : o.Teacher.EmployeeNumber,
                    AdviserId = o.AdviserId,
                    IsActive = o.IsActive,
                    EnrolledStudentCount = o.Enrollments.Count(e => e.Status == "Enrolled")
                })
                .ToListAsync(ct);

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<SubjectOfferingDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var o = await _context.SubjectOfferings
                .AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Teacher).ThenInclude(t => t!.User)
                .Include(x => x.Enrollments)
                .FirstOrDefaultAsync(x => x.SubjectOfferingId == id, ct);

            if (o is null) return new NotFoundObjectResult(new { message = "Subject offering not found." });
            return new OkObjectResult(Map(o));
        }

        public async Task<ActionResult<SubjectOfferingDto>> CreateAsync(CreateSubjectOfferingRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.EdpCode) || string.IsNullOrWhiteSpace(request.ProgramCode))
                return new BadRequestObjectResult(new { message = "EdpCode and ProgramCode are required." });
            if (string.IsNullOrWhiteSpace(request.Section) || string.IsNullOrWhiteSpace(request.SchoolYear))
                return new BadRequestObjectResult(new { message = "Section and SchoolYear are required." });
            if (request.Term <= 0) return new BadRequestObjectResult(new { message = "Term must be positive." });

            if (!await _context.Subjects.AnyAsync(s => s.SubjectId == request.SubjectId, ct))
                return new BadRequestObjectResult(new { message = "Subject not found." });

            if (request.TeacherId.HasValue && !await _context.Teachers.AnyAsync(t => t.TeacherId == request.TeacherId, ct))
                return new BadRequestObjectResult(new { message = "Teacher not found." });
            if (request.AdviserId.HasValue && !await _context.Teachers.AnyAsync(t => t.TeacherId == request.AdviserId, ct))
                return new BadRequestObjectResult(new { message = "Adviser not found." });

            var edp = request.EdpCode.Trim();
            var sy = request.SchoolYear.Trim();
            if (await _context.SubjectOfferings.AnyAsync(o => o.EdpCode == edp && o.SchoolYear == sy && o.Term == request.Term, ct))
                return new ConflictObjectResult(new { message = "A subject offering with this EdpCode/SY/Term already exists." });

            var entity = new SubjectOffering
            {
                SubjectId = request.SubjectId,
                ProgramCode = request.ProgramCode.Trim(),
                EdpCode = edp,
                GradeLevel = request.GradeLevel,
                Section = request.Section.Trim(),
                SchoolYear = sy,
                Term = request.Term,
                TeacherId = request.TeacherId,
                AdviserId = request.AdviserId,
                IsActive = true
            };

            _context.SubjectOfferings.Add(entity);
            await _context.SaveChangesAsync(ct);

            await _context.Entry(entity).Reference(x => x.Subject).LoadAsync(ct);
            if (entity.TeacherId.HasValue)
                await _context.Entry(entity).Reference(x => x.Teacher!).Query().Include(t => t.User).LoadAsync(ct);

            return new CreatedResult($"/api/subject-offerings/{entity.SubjectOfferingId}", Map(entity));
        }

        public async Task<ActionResult<SubjectOfferingDto>> UpdateAsync(int id, UpdateSubjectOfferingRequest request, CancellationToken ct = default)
        {
            var entity = await _context.SubjectOfferings
                .Include(o => o.Subject)
                .Include(o => o.Teacher).ThenInclude(t => t!.User)
                .Include(o => o.Enrollments)
                .FirstOrDefaultAsync(o => o.SubjectOfferingId == id, ct);

            if (entity is null) return new NotFoundObjectResult(new { message = "Subject offering not found." });

            if (request.TeacherId.HasValue && !await _context.Teachers.AnyAsync(t => t.TeacherId == request.TeacherId, ct))
                return new BadRequestObjectResult(new { message = "Teacher not found." });
            if (request.AdviserId.HasValue && !await _context.Teachers.AnyAsync(t => t.TeacherId == request.AdviserId, ct))
                return new BadRequestObjectResult(new { message = "Adviser not found." });

            entity.ProgramCode = request.ProgramCode.Trim();
            entity.GradeLevel = request.GradeLevel;
            entity.Section = request.Section.Trim();
            entity.SchoolYear = request.SchoolYear.Trim();
            entity.Term = request.Term;
            entity.TeacherId = request.TeacherId;
            entity.AdviserId = request.AdviserId;
            if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync(ct);

            if (entity.TeacherId.HasValue)
                await _context.Entry(entity).Reference(x => x.Teacher!).Query().Include(t => t.User).LoadAsync(ct);

            return new OkObjectResult(Map(entity));
        }

        public async Task<ActionResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.SubjectOfferings.FirstOrDefaultAsync(o => o.SubjectOfferingId == id, ct);
            if (entity is null) return new NotFoundObjectResult(new { message = "Subject offering not found." });

            if (await _context.TeacherEvaluationSubmissions.AnyAsync(s => s.SubjectOfferingId == id, ct))
                return new ConflictObjectResult(new { message = "Cannot delete: evaluation submissions exist for this offering. Deactivate it instead." });

            if (await _context.StudentSubjectEnrollments.AnyAsync(e => e.SubjectOfferingId == id, ct))
                return new ConflictObjectResult(new { message = "Cannot delete: students are still enrolled. Remove enrollments first." });

            _context.SubjectOfferings.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return new NoContentResult();
        }

        private static SubjectOfferingDto Map(SubjectOffering o) => new()
        {
            SubjectOfferingId = o.SubjectOfferingId,
            SubjectId = o.SubjectId,
            SubjectCode = o.Subject.Code,
            SubjectDescription = o.Subject.Description,
            EdpCode = o.EdpCode,
            ProgramCode = o.ProgramCode,
            GradeLevel = o.GradeLevel,
            Section = o.Section,
            SchoolYear = o.SchoolYear,
            Term = o.Term,
            TeacherId = o.TeacherId,
            TeacherName = o.Teacher == null ? null : (o.Teacher.User.FirstName + " " + o.Teacher.User.LastName).Trim(),
            TeacherEmployeeNumber = o.Teacher == null ? null : o.Teacher.EmployeeNumber,
            AdviserId = o.AdviserId,
            IsActive = o.IsActive,
            EnrolledStudentCount = o.Enrollments?.Count(e => e.Status == "Enrolled") ?? 0
        };
    }
}