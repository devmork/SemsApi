using SemsApi.DTO.FormCategory;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    public static class FormCategoryHelper
    {
        public static FormCategoryResponseDto MapToResponse(EvaluationFormCategory fc) =>
            new()
            {
                FormCategoryId = fc.FormCategoryId,
                FormId = fc.FormId,
                CategoryId = fc.CategoryId,
                CategoryName = fc.EvaluationCategory?.CategoryName,
                CategoryRate = fc.EvaluationCategory?.CategoryRate,
                IsActive = fc.IsActive,
                DisplayOrder = fc.DisplayOrder
            };

        public static string? Validate(FormCategoryRequestDto request)
        {
            if (request is null)
                return "Request body is required.";

            if (request.FormId <= 0)
                return "FormId must be a positive number.";
            if (request.CategoryId <= 0)
                return "CategoryId must be a positive number.";
            if (request.DisplayOrder.HasValue && request.DisplayOrder.Value < 0)
                return "DisplayOrder must not be negative.";

            return null;
        }
    }
}
