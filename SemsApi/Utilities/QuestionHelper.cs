using SemsApi.DTO.Evaluation;
using SemsApi.Models;

namespace SemsApi.Utilities
{
    public static class QuestionHelper
    {
        public static EvaluationQuestionDto MapToDto(EvaluationQuestion question) =>
            new()
            {
                QuestionId = question.QuestionId,
                QuestionName = question.QuestionName
            };

        public static string? Validate(EvaluationQuestionDto? request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.QuestionName))
                return "QuestionName is required.";

            return null;
        }
    }
}
