using SemsApi.DTO;
using SemsApi.Models;

namespace SemsApi.Utilities
{
    /// <summary>
    /// Shared helper methods for EvaluationCategory CRUD operations.
    /// </summary>
    public static class CategoryHelper
    {
        /// <summary>
        /// Individual category rate must stay within 0–100 (percent).
        /// </summary>
        public const decimal MaxCatRate = 100.00m;

        /// <summary>
        /// Maps a EvaluationCategory entity to the response DTO.
        /// </summary>
        public static CategoryResponseDto MapToResponse(EvaluationCategory c) =>
            new()
            {
                CategoryId = c.CategoryId,
                EvaluationTypeId = c.EvaluationTypeId,
                CategoryName = c.CategoryName,
                CategoryRate = c.CategoryRate
            };

        /// <summary>
        /// Server-side validation matching EF configuration and business rules.
        /// Returns an error message string, or null when the request is valid.
        /// </summary>
        public static string? Validate(CategoryRequestDto request)
        {
            if (request is null)
                return "Request body is required.";

            if (!request.EvaluationTypeId.HasValue)
                return "EvaluationTypeId is required.";
            if (request.EvaluationTypeId.Value <= 0)
                return "EvaluationTypeId must be a positive number.";


            if (string.IsNullOrWhiteSpace(request.CategoryName))
                return "CategoryName is required.";
            if (request.CategoryName.Trim().Length > 150)
                return "CategoryName must not exceed 150 characters.";

            if (!request.CategoryRate.HasValue)
                return "CategoryRate is required.";
            if (request.CategoryRate.Value < 0)
                return "CategoryRate must not be negative.";
            if (request.CategoryRate.Value > MaxCatRate)
                return $"CategoryRate must not exceed {MaxCatRate}.";

            return null;
        }
    }
}
