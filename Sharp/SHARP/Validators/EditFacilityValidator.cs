using FluentValidation;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.ViewModels.Facility;
using System.Linq;

namespace SHARP.Validators
{
    public class EditFacilityValidator : AbstractValidator<EditFacilityModel>
    {
        public EditFacilityValidator(IFacilityService facilityService)
        {
            RuleFor(i => i.Name)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .MustAsync(async (model, time, ct) => !await facilityService.IsFacilityNameAlreadyExist(model.Name, model.OrganizationId, model.Id))
               .WithMessage(i => string.Format(ErrorConstants.ALREADY_EXISTS_TEMPLATE, i.Name));

            RuleFor(i => i.OrganizationId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Organization"));

            RuleFor(i => i.TimeZoneId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Time Zone"));

            /*RuleFor(i => i.Recipients)
                .Cascade(CascadeMode.Stop)
                .ForEach(i => i.SetValidator(emails => new RecipientEmailValidator()))
                .When(i => i.Recipients?.Count > 0);*/
        }
    }
}
