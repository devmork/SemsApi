using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.TeacherSubjects;

namespace SemsApi.Interfaces
{
    public interface ITeacherSubjectService
    {
        Task<ActionResult<List<TeacherSubjectDto>>> GetSubjectsForTeacherAsync(
            int teacherId, string? schoolYear = null, int? term = null, CancellationToken ct = default);
        Task<ActionResult<List<TeacherStudentDto>>> GetStudentsForTeacherAsync(
            int teacherId, string? schoolYear = null, int? term = null, CancellationToken ct = default);
        Task<ActionResult<List<TeacherSubjectDto>>> GetSubjectsForCurrentUserAsync(
            int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default);
        Task<ActionResult<List<TeacherStudentDto>>> GetStudentsForCurrentUserAsync(
            int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default);
    }
}