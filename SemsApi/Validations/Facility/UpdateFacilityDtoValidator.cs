using FluentValidation;
using SemsApi.DTO.Facility;

namespace SemsApi.Validations.Facility
{
    public class UpdateFacilityDtoValidator : AbstractValidator<UpdateFacilityDto>
    {
        public UpdateFacilityDtoValidator()
        {
            RuleFor(x => x.FacilityName)
                .NotEmpty();
        }
    }
}
