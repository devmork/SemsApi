using SemsApi.DTO.Evaluation;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    /// <summary>
    /// Shared helper methods for EvaluationForm CRUD operations.
    /// </summary>
    public static class EvaluationFormHelper
    {
        /// <summary>
        /// Allowed values for EvaluationForm.TargetType.
        /// </summary>
        public static readonly string[] AllowedTargetTypes = { "SubjectTeacher", "Facility" };

        /// <summary>
        /// Maps an EvaluationForm entity to its DTO.
        /// </summary>
        public static EvaluationFormDto MapToDto(EvaluationForm f) =>
            new()
            {
                FormId = f.FormId,
                Name = f.Name,
                Code = f.Code,
                GradeBand = f.GradeBand,
                TargetType = f.TargetType,
                IsActive = f.IsActive
            };

        /// <summary>
        /// Validates the request body for Create and Update.
        /// Returns an error message string, or null when the request is valid.
        /// </summary>
        public static string? Validate(EvaluationFormRequestDto request)
        {
            if (request is null)
                return "Request body is required.";

            if (string.IsNullOrWhiteSpace(request.Name))
                return "Name is required.";
            if (string.IsNullOrWhiteSpace(request.Code))
                return "Code is required.";
            if (string.IsNullOrWhiteSpace(request.GradeBand))
                return "GradeBand is required.";
            if (string.IsNullOrWhiteSpace(request.TargetType))
                return "TargetType is required.";

            if (!AllowedTargetTypes.Contains(request.TargetType.Trim(), StringComparer.OrdinalIgnoreCase))
                return "TargetType must be either 'SubjectTeacher' or 'Facility'.";

            return null;
        }

        /// <summary>
        /// Canonicalizes TargetType casing to match the seed/display convention
        /// ("SubjectTeacher" or "Facility"), regardless of the casing the client sent.
        /// Assumes the value has already passed Validate().
        /// </summary>
        public static string NormalizeTargetType(string targetType)
        {
            var trimmed = targetType.Trim();
            var match = AllowedTargetTypes
                .FirstOrDefault(t => string.Equals(t, trimmed, StringComparison.OrdinalIgnoreCase));

            return match ?? trimmed;
        }
    }
}