using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Subject;
using SemsApi.DTO.Subjects;

namespace SemsApi.Interfaces
{
    public interface ISubjectService
    {
        Task<ActionResult<List<SubjectDto>>> GetAllAsync(string? search = null, CancellationToken ct = default);
        Task<ActionResult<SubjectDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ActionResult<SubjectDto>> CreateAsync(CreateSubjectRequest request, CancellationToken ct = default);
        Task<ActionResult<SubjectDto>> UpdateAsync(int id, UpdateSubjectRequest request, CancellationToken ct = default);
        Task<ActionResult> DeleteAsync(int id, CancellationToken ct = default);
    }
}