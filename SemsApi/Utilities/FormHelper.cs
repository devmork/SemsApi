using SemsApi.DTO.Form;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    public static class FormHelper
    {
        public const int MaxCodeLength = 20;
        public const int MaxGradeBandLength = 30;
        public const int MaxTargetTypeLength = 30;
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 1000;

        public const int MinProgramId = 0;
        public const int MaxProgramId = 3;

        public const int Inactive = 0;
        public const int Active = 1;

        public const string DefaultTargetType = "SubjectTeacher";
        public static readonly string[] AllowedTargetTypes = { "SubjectTeacher", "Facility" };

        public static FormDto MapToDto(Form f, string evaluationTypeName) =>
            new()
            {
                FormId = f.FormId,
                Code = f.Code,
                GradeBand = f.GradeBand,
                TargetType = f.TargetType,
                ProgramId = f.ProgramId,
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

            if (string.IsNullOrWhiteSpace(request.Code))
                return "Code is required.";
            if (request.Code.Trim().Length > MaxCodeLength)
                return $"Code must not exceed {MaxCodeLength} characters.";

            if (request.GradeBand is not null && request.GradeBand.Trim().Length > MaxGradeBandLength)
                return $"GradeBand must not exceed {MaxGradeBandLength} characters.";

            if (!string.IsNullOrWhiteSpace(request.TargetType) &&
                !AllowedTargetTypes.Contains(request.TargetType.Trim(), StringComparer.OrdinalIgnoreCase))
                return "TargetType must be either 'SubjectTeacher' or 'Facility'.";

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

        public static string NormalizeCode(string code) => code.Trim().ToUpper();

        public static string NormalizeTargetType(string? targetType)
        {
            if (string.IsNullOrWhiteSpace(targetType))
                return DefaultTargetType;

            var trimmed = targetType.Trim();
            var match = AllowedTargetTypes
                .FirstOrDefault(t => string.Equals(t, trimmed, StringComparison.OrdinalIgnoreCase));

            return match ?? DefaultTargetType;
        }
    }
}