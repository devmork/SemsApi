using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.SubjectOfferings;

namespace SemsApi.Interfaces
{
    public interface ISubjectOfferingService
    {
        Task<ActionResult<List<SubjectOfferingDto>>> GetAllAsync(
            int? gradeLevel = null, string? section = null, string? schoolYear = null,
            int? term = null, int? teacherId = null, CancellationToken ct = default);

        Task<ActionResult<SubjectOfferingDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ActionResult<SubjectOfferingDto>> CreateAsync(CreateSubjectOfferingRequest request, CancellationToken ct = default);
        Task<ActionResult<SubjectOfferingDto>> UpdateAsync(int id, UpdateSubjectOfferingRequest request, CancellationToken ct = default);
        Task<ActionResult> DeleteAsync(int id, CancellationToken ct = default);
    }
}