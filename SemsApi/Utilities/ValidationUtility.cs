using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace SemsApi.Utilities
{
    public static class ValidationUtility
    {
        public static ValidationProblemDetails ToValidationProblemDetails(this ValidationResult validationResult)
        {
            var problemDetails = new ValidationProblemDetails();

            foreach (var failure in validationResult.Errors)
            {
                var propertyName = failure.PropertyName;
                var errorMessage = failure.ErrorMessage;

                if (problemDetails.Errors.ContainsKey(propertyName))
                {
                    problemDetails.Errors[propertyName] = problemDetails.Errors[propertyName]
                        .Concat(new[] { errorMessage })
                        .ToArray();
                }
                else
                {
                    problemDetails.Errors[propertyName] = new[] { errorMessage };
                }
            }

            return problemDetails;
        }
    }
}
