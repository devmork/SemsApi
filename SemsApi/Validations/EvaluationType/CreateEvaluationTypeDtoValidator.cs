using FluentValidation;
using SemsApi.DTO.EvaluationType;

namespace SemsApi.Validations.EvaluationType
{
    public class CreateEvaluationTypeDtoValidator : AbstractValidator<CreateEvaluationTypeDto>
    {
        public CreateEvaluationTypeDtoValidator()
        {
            RuleFor(x => x.EvaluationTypeName)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
