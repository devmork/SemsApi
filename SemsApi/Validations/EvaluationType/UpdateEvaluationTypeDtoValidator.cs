using FluentValidation;
using SemsApi.DTO.EvaluationType;

namespace SemsApi.Validations.EvaluationType
{
    public class UpdateEvaluationTypeDtoValidator : AbstractValidator<UpdateEvaluationTypeDto>
    {
        public UpdateEvaluationTypeDtoValidator()
        {
            RuleFor(x => x.EvaluationTypeName)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
