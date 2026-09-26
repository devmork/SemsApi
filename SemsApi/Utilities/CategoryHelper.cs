using SemsApi.DTO;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    /// <summary>
    /// Shared helper methods for TeacherEvaluationCategory CRUD operations.
    /// </summary>
    public static class CategoryHelper
    {
        /// <summary>
        /// Individual category rate must stay within 0–100 (percent).
        /// </summary>
        public const decimal MaxCatRate = 100.00m;

        /// <summary>
        /// Maps a TeacherEvaluationCategory entity to the response DTO.
        /// </summary>
        public static CategoryResponseDto MapToResponse(TeacherEvaluationCategory c) =>
            new()
            {
                Recno = c.Recno,
                EvalType = c.EvalType,
                CatNo = c.CatNo,
                CatRn = c.CatRn,
                CatName = c.CatName,
                CatRate = c.CatRate
            };

        /// <summary>
        /// Server-side validation matching EF configuration and business rules.
        /// Returns an error message string, or null when the request is valid.
        /// </summary>
        public static string? Validate(CategoryRequestDto request)
        {
            if (request is null)
                return "Request body is required.";

            if (string.IsNullOrWhiteSpace(request.EvalType))
                return "EvalType is required.";
            if (request.EvalType.Trim().Length > 50)
                return "EvalType must not exceed 50 characters.";

            if (!request.CatNo.HasValue)
                return "CatNo is required.";
            if (request.CatNo.Value <= 0)
                return "CatNo must be a positive number.";

            if (string.IsNullOrWhiteSpace(request.CatRn))
                return "CatRn is required.";
            if (request.CatRn.Trim().Length > 10)
                return "CatRn must not exceed 10 characters.";

            if (string.IsNullOrWhiteSpace(request.CatName))
                return "CatName is required.";
            if (request.CatName.Trim().Length > 150)
                return "CatName must not exceed 150 characters.";

            if (!request.CatRate.HasValue)
                return "CatRate is required.";
            if (request.CatRate.Value < 0)
                return "CatRate must not be negative.";
            if (request.CatRate.Value > MaxCatRate)
                return $"CatRate must not exceed {MaxCatRate}.";

            return null;
        }
    }
}