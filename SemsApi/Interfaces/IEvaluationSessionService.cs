using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.EvaluationSessions;
using SemsApi.Models;

namespace SemsApi.Interfaces
{
    public interface IEvaluationSessionService
    {
        Task<ActionResult<List<EvaluationSessionDto>>> GetAllAsync(
            int? formId = null,
            string? schoolYear = null,
            int? term = null,
            EvaluationSessionStatus? status = null,
            bool onlyOpen = false,
            bool studentView = false,
            CancellationToken ct = default);

        Task<ActionResult<EvaluationSessionDetailDto>> GetByIdAsync(
            int id,
            bool studentView = false,
            CancellationToken ct = default);

        Task<ActionResult> CreateAsync(
            EvaluationSessionCreateRequest request,
            int createdBy,
            CancellationToken ct = default);

        Task<ActionResult> UpdateAsync(
            int id,
            EvaluationSessionUpdateRequest request,
            bool allowReopen = false,
            CancellationToken ct = default);
        Task<ActionResult<List<SessionSubmissionDto>>> GetSubmissionsAsync(
            int id,
            CancellationToken ct = default);

        Task<ActionResult> DeleteAsync(int id, CancellationToken ct = default);

        Task<ActionResult> OpenAsync(int id, CancellationToken ct = default);

        Task<ActionResult> CloseAsync(int id, CancellationToken ct = default);
    }
}
