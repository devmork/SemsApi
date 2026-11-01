using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.StudentSubjects;

namespace SemsApi.Interfaces
{
    public interface IStudentSubjectService
    {
        Task<ActionResult<List<StudentSubjectDto>>> GetForStudentAsync(
            int studentId, string? schoolYear = null, int? term = null, CancellationToken ct = default);
        Task<ActionResult<List<StudentSubjectDto>>> GetForCurrentUserAsync(
            int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default);
        Task<ActionResult> AssignAsync(AssignStudentToOfferingRequest request, CancellationToken ct = default);
        Task<ActionResult> DeactivateAsync(int enrollmentId, CancellationToken ct = default);
    }
}