using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.StudentSubjects;
using SemsApi.Interfaces;

namespace SemsApi.Services
{
    public class StudentSubjectService : IStudentSubjectService
    {
        private readonly ApplicationDbContext _context;
        public StudentSubjectService(ApplicationDbContext context) { _context = context; }

        public async Task<ActionResult<List<StudentSubjectDto>>> GetForStudentAsync(
            int studentId, string? schoolYear = null, int? term = null, CancellationToken ct = default)
        {
            if (!await _context.Students.AnyAsync(s => s.StudentId == studentId, ct))
                return new NotFoundObjectResult(new { message = "Student not found." });

            return new OkObjectResult(await BuildQuery(studentId, schoolYear, term).ToListAsync(ct));
        }

        public async Task<ActionResult<List<StudentSubjectDto>>> GetForCurrentUserAsync(
            int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default)
        {
            var studentId = await _context.Students.AsNoTracking()
                .Where(s => s.UserId == userId)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync(ct);

            if (studentId == 0) return new NotFoundObjectResult(new { message = "No student profile linked to this user." });
            return new OkObjectResult(await BuildQuery(studentId, schoolYear, term).ToListAsync(ct));
        }

        public async Task<ActionResult> AssignAsync(AssignStudentToOfferingRequest request, CancellationToken ct = default)
        {
            if (!await _context.Students.AnyAsync(s => s.StudentId == request.StudentId, ct))
                return new BadRequestObjectResult(new { message = "Student not found." });

            var offering = await _context.SubjectOfferings.FirstOrDefaultAsync(o => o.SubjectOfferingId == request.SubjectOfferingId, ct);
            if (offering is null) return new BadRequestObjectResult(new { message = "Subject offering not found." });
            if (!offering.IsActive) return new BadRequestObjectResult(new { message = "Subject offering is inactive." });

            var existing = await _context.StudentSubjectEnrollments
                .FirstOrDefaultAsync(e => e.StudentId == request.StudentId && e.SubjectOfferingId == request.SubjectOfferingId, ct);

            if (existing is not null)
            {
                if (existing.Status == "Enrolled")
                    return new ConflictObjectResult(new { message = "Student is already enrolled in this subject offering." });

                existing.Status = "Enrolled";
                existing.DroppedAtUtc = null;
                existing.EnrolledAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                return new OkObjectResult(new { message = "Enrollment reactivated.", data = Map(existing, offering) });
            }

            var entity = new Models.StudentSubjectEnrollment
            {
                StudentId = request.StudentId,
                SubjectOfferingId = request.SubjectOfferingId,
                Status = "Enrolled"
            };
            _context.StudentSubjectEnrollments.Add(entity);
            await _context.SaveChangesAsync(ct);
            return new OkObjectResult(new { message = "Student enrolled successfully.", data = Map(entity, offering) });
        }

        public async Task<ActionResult> DeactivateAsync(int enrollmentId, CancellationToken ct = default)
        {
            var entity = await _context.StudentSubjectEnrollments.FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId, ct);
            if (entity is null) return new NotFoundObjectResult(new { message = "Enrollment not found." });

            if (await _context.TeacherEvaluationSubmissions
                    .AnyAsync(s => s.StudentId == entity.StudentId && s.SubjectOfferingId == entity.SubjectOfferingId, ct))
            {
                entity.Status = "Dropped";
                entity.DroppedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                return new NoContentResult();
            }

            _context.StudentSubjectEnrollments.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return new NoContentResult();
        }

        private IQueryable<StudentSubjectDto> BuildQuery(int studentId, string? sy, int? term)
        {
            var q = _context.StudentSubjectEnrollments
                .AsNoTracking()
                .Where(e => e.StudentId == studentId);

            if (!string.IsNullOrWhiteSpace(sy))
                q = q.Where(e => e.SubjectOffering.SchoolYear == sy);
            if (term.HasValue)
                q = q.Where(e => e.SubjectOffering.Term == term.Value);

            return q.OrderBy(e => e.SubjectOffering.SchoolYear)
                    .ThenBy(e => e.SubjectOffering.Term)
                    .ThenBy(e => e.SubjectOffering.Subject.Code)
                    .Select(e => new StudentSubjectDto
                    {
                        EnrollmentId = e.EnrollmentId,
                        SubjectOfferingId = e.SubjectOfferingId,
                        SubjectId = e.SubjectOffering.SubjectId,
                        SubjectCode = e.SubjectOffering.Subject.Code,
                        SubjectDescription = e.SubjectOffering.Subject.Description,
                        EdpCode = e.SubjectOffering.EdpCode,
                        GradeLevel = e.SubjectOffering.GradeLevel,
                        Section = e.SubjectOffering.Section,
                        SchoolYear = e.SubjectOffering.SchoolYear,
                        Term = e.SubjectOffering.Term,
                        Status = e.Status,
                        TeacherId = e.SubjectOffering.TeacherId,
                        TeacherName = e.SubjectOffering.Teacher == null
                            ? null
                            : (e.SubjectOffering.Teacher.User.FirstName + " " + e.SubjectOffering.Teacher.User.LastName).Trim(),
                        TeacherEmployeeNumber = e.SubjectOffering.Teacher == null
                            ? null
                            : e.SubjectOffering.Teacher.EmployeeNumber
                    });
        }

        private static StudentSubjectDto Map(Models.StudentSubjectEnrollment e, Models.SubjectOffering o) => new()
        {
            EnrollmentId = e.EnrollmentId,
            SubjectOfferingId = o.SubjectOfferingId,
            SubjectId = o.SubjectId,
            EdpCode = o.EdpCode,
            GradeLevel = o.GradeLevel,
            Section = o.Section,
            SchoolYear = o.SchoolYear,
            Term = o.Term,
            Status = e.Status,
            TeacherId = o.TeacherId
        };
    }
}