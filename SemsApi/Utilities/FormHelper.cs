using SemsApi.DTO.Form;
using SemsApi.Models;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    public static class FormHelper
    {
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 1000;

        public const int Inactive = 0;
        public const int Active = 1;

        /// <summary>
        /// True when the evaluation type is a facility/service evaluation.
        /// Detection is by name ("Facility"), not a DB flag, so the name must
        /// be kept consistent when creating evaluation types.
        /// </summary>
        public static bool IsFacilityEvaluation(EvaluationType? evaluationType) =>
            string.Equals(evaluationType?.EvaluationTypeName, "Facility", StringComparison.OrdinalIgnoreCase);

        public static FormDto MapToDto(EvaluationForm f, string evaluationTypeName) =>
            new()
            {
                FormId = f.FormId,
                EvaluationTypeId = f.EvaluationTypeId,
                EvaluationTypeName = evaluationTypeName,
                Title = f.Title,
                Description = f.Description,
                CreatedBy = f.CreatedBy,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt
            };

        public static string? Validate(FormRequestDto request)
        {
            if (request is null)
                return "Request body is required.";

            if (!request.EvaluationTypeId.HasValue)
                return "EvaluationTypeId is required.";
            if (request.EvaluationTypeId.Value <= 0)
                return "EvaluationTypeId must be a positive number.";

            if (string.IsNullOrWhiteSpace(request.Title))
                return "Title is required.";
            if (request.Title.Trim().Length > MaxTitleLength)
                return $"Title must not exceed {MaxTitleLength} characters.";

            if (request.Description is not null && request.Description.Trim().Length > MaxDescriptionLength)
                return $"Description must not exceed {MaxDescriptionLength} characters.";

            if (request.IsActive.HasValue
                && request.IsActive.Value != Inactive
                && request.IsActive.Value != Active)
                return "IsActive must be 0 or 1.";

            return null;
        }
    }
}
