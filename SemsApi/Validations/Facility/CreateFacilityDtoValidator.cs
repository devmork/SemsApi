using FluentValidation;
using SemsApi.DTO.Facility;

namespace SemsApi.Validations.Facility
{
    public class CreateFacilityDtoValidator : AbstractValidator<CreateFacilityDto>
    {
        public CreateFacilityDtoValidator()
        {
            RuleFor(x => x.FacilityName)
                .NotEmpty();
        }
    }
}
