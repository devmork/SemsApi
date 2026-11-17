using SemsApi.DTO.Facility;
using SemsApi.Models;

namespace SemsApi.Utilities
{
    public static class FacilityHelper
    {
        public const int MaxNameLength = 150;

        public static FacilityDto MapToDto(Facility f, int evaluationCount = 0) =>
            new()
            {
                FacilityId = f.FacilityId,
                FacilityName = f.FacilityName,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                EvaluationCount = evaluationCount
            };

        public static string? ValidateCreate(CreateFacilityRequest request)
        {
            if (request is null) return "Request body is required.";
            if (string.IsNullOrWhiteSpace(request.FacilityName))
                return "FacilityName is required.";
            if (request.FacilityName.Trim().Length > MaxNameLength)
                return $"FacilityName must not exceed {MaxNameLength} characters.";
            return null;
        }

        public static string? ValidateUpdate(UpdateFacilityRequest request)
        {
            if (request is null) return "Request body is required.";
            return ValidateCreate(new CreateFacilityRequest
            {
                FacilityName = request.FacilityName
            });
        }
    }
}