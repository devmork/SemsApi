using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.TeacherSubjects;
using SemsApi.Interfaces;

namespace SemsApi.Services
{
    public class TeacherSubjectService : ITeacherSubjectService
    {
        private readonly ApplicationDbContext _context;
        public TeacherSubjectService(ApplicationDbContext context) { _context = context; }

        public async Task<ActionResult<List<TeacherSubjectDto>>> GetSubjectsForTeacherAsync(
            int teacherId, string? schoolYear = null, int? term = null, CancellationToken ct = default)
        {
            if (!await _context.Teachers.AnyAsync(t => t.TeacherId == teacherId, ct))
                return new NotFoundObjectResult(new { message = "Teacher not found." });

            var q = _context.SubjectOfferings.AsNoTracking().Where(o => o.TeacherId == teacherId);
            if (!string.IsNullOrWhiteSpace(schoolYear)) q = q.Where(o => o.SchoolYear == schoolYear);
            if (term.HasValue) q = q.Where(o => o.Term == term.Value);

            var data = await q.OrderBy(o => o.SchoolYear).ThenBy(o => o.Term)
                .Select(o => new TeacherSubjectDto
                {
                    SubjectOfferingId = o.SubjectOfferingId,
                    EdpCode = o.EdpCode,
                    SubjectId = o.SubjectId,
                    SubjectCode = o.Subject.Code,
                    SubjectDescription = o.Subject.Description,
                    GradeLevel = o.GradeLevel,
                    Section = o.Section,
                    SchoolYear = o.SchoolYear,
                    Term = o.Term,
                    EnrolledStudentCount = o.Enrollments.Count(e => e.Status == "Enrolled")
                })
                .ToListAsync(ct);

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<List<TeacherStudentDto>>> GetStudentsForTeacherAsync(
            int teacherId, string? schoolYear = null, int? term = null, CancellationToken ct = default)
        {
            if (!await _context.Teachers.AnyAsync(t => t.TeacherId == teacherId, ct))
                return new NotFoundObjectResult(new { message = "Teacher not found." });

            var q = _context.StudentSubjectEnrollments.AsNoTracking()
                .Where(e => e.SubjectOffering.TeacherId == teacherId && e.Status == "Enrolled");

            if (!string.IsNullOrWhiteSpace(schoolYear)) q = q.Where(e => e.SubjectOffering.SchoolYear == schoolYear);
            if (term.HasValue) q = q.Where(e => e.SubjectOffering.Term == term.Value);

            var data = await q
                .OrderBy(e => e.SubjectOffering.SchoolYear).ThenBy(e => e.SubjectOffering.Subject.Code)
                .Select(e => new TeacherStudentDto
                {
                    StudentId = e.StudentId,
                    StudentNumber = e.Student.StudentNumber,
                    FullName = (e.Student.User.FirstName + " " + e.Student.User.LastName).Trim(),
                    GradeLevel = e.Student.GradeLevel,
                    Section = e.Student.Section,
                    SubjectOfferingId = e.SubjectOfferingId,
                    SubjectCode = e.SubjectOffering.Subject.Code,
                    SubjectDescription = e.SubjectOffering.Subject.Description,
                    SchoolYear = e.SubjectOffering.SchoolYear,
                    Term = e.SubjectOffering.Term
                })
                .ToListAsync(ct);

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<List<TeacherSubjectDto>>> GetSubjectsForCurrentUserAsync(
            int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default)
        {
            var teacherId = await _context.Teachers.AsNoTracking()
                .Where(t => t.UserId == userId).Select(t => t.TeacherId).FirstOrDefaultAsync(ct);
            if (teacherId == 0) return new NotFoundObjectResult(new { message = "No teacher profile linked to this user." });
            return await GetSubjectsForTeacherAsync(teacherId, schoolYear, term, ct);
        }

        public async Task<ActionResult<List<TeacherStudentDto>>> GetStudentsForCurrentUserAsync(
            int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default)
        {
            var teacherId = await _context.Teachers.AsNoTracking()
                .Where(t => t.UserId == userId).Select(t => t.TeacherId).FirstOrDefaultAsync(ct);
            if (teacherId == 0) return new NotFoundObjectResult(new { message = "No teacher profile linked to this user." });
            return await GetStudentsForTeacherAsync(teacherId, schoolYear, term, ct);
        }
    }
}