using SemsApi.DTO.FormQuestion;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    public static class FormQuestionHelper
    {
        public static FormQuestionResponseDto MapToResponse(EvaluationFormQuestion q) =>
            new()
            {
                FormQuestionId = q.FormQuestionId,
                FormCategoryId = q.FormCategoryId,
                QuestionName = q.QuestionName,
                DisplayOrder = q.DisplayOrder,
                Points = q.Points
            };

        public static string? Validate(FormQuestionRequestDto request)
        {
            if (request is null)
                return "Request body is required.";

            if (request.FormCategoryId <= 0)
                return "FormCategoryId must be a positive number.";
            if (string.IsNullOrWhiteSpace(request.QuestionName))
                return "QuestionName is required.";
            if (request.QuestionName!.Trim().Length > 1000)
                return "QuestionName must not exceed 1000 characters.";
            if (request.DisplayOrder.HasValue && request.DisplayOrder.Value < 0)
                return "DisplayOrder must not be negative.";

            return null;
        }
    }
}
