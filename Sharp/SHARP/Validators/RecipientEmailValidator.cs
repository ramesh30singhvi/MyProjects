using FluentValidation;
using SHARP.Common.Constants;
using SHARP.Extensions;
using SHARP.ViewModels.Facility;

namespace SHARP.Validators
{
    public class RecipientEmailValidator : AbstractValidator<FacilityRecipientModel>
    {
        public RecipientEmailValidator()
        {
            RuleFor(i => i.Email)
                .Cascade(CascadeMode.Stop)
                .IsEmailAddress()
                .WithMessage(string.Format(ErrorConstants.INVALID_FORMAT_TEMPLATE, "Email"));
        }
    }
}
