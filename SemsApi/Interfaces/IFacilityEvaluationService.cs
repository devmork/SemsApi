using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Facility;

namespace SemsApi.Interfaces
{
    public interface IFacilityEvaluationService
    {
        Task<ActionResult<List<FacilityEvaluationAssignmentDto>>> GetAssignmentsAsync(
            int userId, string? schoolYear = null, int? term = null, bool? filter = null,
            CancellationToken ct = default);

        Task<ActionResult> SubmitAsync(
            int userId, SubmitFacilityEvaluationRequest request, CancellationToken ct = default);

        Task<ActionResult<List<FacilityEvaluationSummaryDto>>> GetSummaryAsync(
            int? sessionId = null, string? schoolYear = null, int? term = null,
            CancellationToken ct = default);

        Task<ActionResult<FacilityEvaluationDetailResultDto>> GetFacilityResultsAsync(
            int facilityId, string? schoolYear = null, int? term = null, int? sessionId = null,
            CancellationToken ct = default);

        Task<ActionResult<List<FacilitySessionSubmissionDto>>> GetSessionSubmissionsAsync(
            int sessionId, CancellationToken ct = default);
    }
}