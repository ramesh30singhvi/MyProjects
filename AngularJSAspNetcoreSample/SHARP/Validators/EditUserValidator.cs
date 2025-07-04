using FluentValidation;
using SHARP.Common.Constants;
using SHARP.Extensions;
using SHARP.ViewModels.User;

namespace SHARP.Validators
{
    public class EditUserValidator : AbstractValidator<EditUserModel>
    {
        public EditUserValidator()
        {
            //RuleFor(i => i.FirstName)
            //   .Cascade(CascadeMode.Stop)
            //   .NotEmpty()
            //   .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "First Name"));

            //RuleFor(i => i.LastName)
            //    .Cascade(CascadeMode.Stop)
            //    .NotEmpty()
            //    .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Last Name"));

            RuleFor(i => i.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Email Address"))
                .IsEmailAddress()
                .WithMessage(string.Format(ErrorConstants.INVALID_FORMAT_TEMPLATE, "Email"));

            //RuleFor(i => i.TimeZone)
            //  .Cascade(CascadeMode.Stop)
            //  .NotEmpty()
            //  .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Time Zone"));

            RuleFor(i => i.Roles)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Role"));

            RuleFor(i => i.Organizations)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Organization Access"));
        }
    }
}
