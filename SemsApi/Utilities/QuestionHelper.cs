using SemsApi.DTO.Evaluation;
using SemsApi.Models;

namespace SemsApi.Utilities
{
    public static class QuestionHelper
    {
        public static EvaluationQuestionDto MapToDto(TeacherEvaluationResult question) =>
            new()
            {
                QnNo = question.QnNo,
                QnName = question.QnName
            };

        public static string? Validate(EvaluationQuestionDto? request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.QnName))
                return "QnName is required.";

            return null;
        }
    }
}
