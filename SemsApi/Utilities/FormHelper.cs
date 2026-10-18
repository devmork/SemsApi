using SemsApi.DTO.Form;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    /// <summary>
    /// Shared helper methods for Form CRUD operations.
    /// </summary>
    public static class FormHelper
    {
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 1000;

        /// <summary>
        /// Program ids: 0 = all students (canteen, library, guidance, etc.),
        /// 1 = Preschool to Grade 3, 2 = Grade 4 to 6, 3 = JHS to SHS.
        /// </summary>
        public const int MinProgramId = 0;
        public const int MaxProgramId = 3;

        public const int Inactive = 0;
        public const int Active = 1;

        /// <summary>
        /// Maps a Form entity to its DTO.
        /// </summary>
        public static FormDto MapToDto(Form f, string evaluationTypeName) =>
            new()
            {
                FormId = f.FormId,
                ProgramId = f.ProgramId,
                EvaluationTypeId = f.EvaluationTypeId,
                EvaluationTypeName = evaluationTypeName,
                Title = f.Title,
                Description = f.Description,
                CreatedBy = f.CreatedBy,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt
            };

        /// <summary>
        /// Server-side validation matching EF configuration and business rules.
        /// Returns an error message string, or null when the request is valid.
        /// </summary>
        public static string? Validate(FormRequestDto request)
        {
            if (request is null)
                return "Request body is required.";

            if (!request.ProgramId.HasValue)
                return "ProgramId is required.";
            if (request.ProgramId.Value < MinProgramId || request.ProgramId.Value > MaxProgramId)
                return "ProgramId must be 0 (all students), 1 (Preschool to Grade 3), 2 (Grade 4 to 6) or 3 (JHS to SHS).";

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