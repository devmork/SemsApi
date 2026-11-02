using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.EvaluationAssignments;

namespace SemsApi.Interfaces
{
    public interface IEvaluationAssignmentService
    {
        Task<ActionResult<List<EvaluationAssignmentDto>>> GetAllAsync(int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default);
        Task<ActionResult<List<EvaluationAssignmentDto>>> GetPendingAsync(int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default);
        Task<ActionResult<List<EvaluationAssignmentDto>>> GetCompletedAsync(int userId, string? schoolYear = null, int? term = null, CancellationToken ct = default);
        Task<ActionResult> SubmitAsync(int userId, SubmitSubjectEvaluationRequest request, CancellationToken ct = default);
    }
}