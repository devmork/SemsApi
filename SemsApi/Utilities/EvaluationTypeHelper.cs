using SemsApi.DTO.EvaluationType;
using SemsApi.Models;

namespace SemsApi.Utilities
{
    /// <summary>
    /// Shared helper methods for EvaluationType CRUD operations.
    /// </summary>
    public static class EvaluationTypeHelper
    {
        public const int MaxNameLength = 100;

        public static EvaluationTypeResponseDto MapToResponse(EvaluationType e) =>
            new()
            {
                EvaluationTypeId = e.EvaluationTypeId,
                EvaluationTypeName = e.EvaluationTypeName
            };

        public static string? Validate(EvaluationTypeRequestDto request)
        {
            if (request is null)
                return "Request body is required.";

            if (string.IsNullOrWhiteSpace(request.EvaluationTypeName))
                return "EvaluationTypeName is required.";

            if (request.EvaluationTypeName.Trim().Length > MaxNameLength)
                return $"EvaluationTypeName must not exceed {MaxNameLength} characters.";

            return null;
        }
    }
}