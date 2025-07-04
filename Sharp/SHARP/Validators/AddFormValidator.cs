using FluentValidation;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.ViewModels.Form;

namespace SHARP.Validators
{
    public class AddFormValidator : AbstractValidator<AddFormModel>
    {
        public AddFormValidator(IFormService formService)
        {
            RuleFor(i => i.OrganizationId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Organization"));

            RuleFor(i => i.Name)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .MustAsync(async (model, time, ct) => !await formService.IsFormNameAlreadyExist(model.Name, model.OrganizationId))
               .WithMessage(i => string.Format(ErrorConstants.ALREADY_EXISTS_TEMPLATE, i.Name));

            RuleFor(i => i.AuditTypeId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY,"Audit Type"));
        }
    }
}
