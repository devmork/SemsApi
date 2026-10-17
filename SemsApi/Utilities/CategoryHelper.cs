using SemsApi.DTO;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    public static class CategoryHelper
    {
        public const decimal MaxCatRate = 100.00m;

        public static CategoryResponseDto MapToResponse(TeacherEvaluationCategory c) =>
            new()
            {
                Recno = c.Recno,
                FormId = c.FormId,
                FormCode = c.Form?.Code ?? string.Empty,
                EvaluationTypeId = c.EvaluationTypeId,
                EvaluationTypeName = c.EvaluationType?.EvaluationTypeName ?? string.Empty,
                CatNo = c.CatNo,
                CatRn = c.CatRn,
                CatName = c.CatName,
                CatRate = c.CatRate
            };

        public static string? Validate(CategoryRequestDto request)
        {
            if (request is null)
                return "Request body is required.";

            if (!request.FormId.HasValue || request.FormId.Value <= 0)
                return "FormId is required and must be a positive number.";

            if (!request.EvaluationTypeId.HasValue && string.IsNullOrWhiteSpace(request.EvaluationTypeName))
                return "Either EvaluationTypeId or EvaluationTypeName is required.";

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