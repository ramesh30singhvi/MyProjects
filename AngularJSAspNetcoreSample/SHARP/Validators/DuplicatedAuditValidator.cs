using FluentValidation;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.ViewModels.Audit;

namespace SHARP.Validators
{
    public class DuplicatedAuditValidator : AbstractValidator<DuplicatedAuditAddEditModel>
    {
        public DuplicatedAuditValidator(IFormService formService)
        {
            RuleFor(i => i.FacilityId)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Facility"));

            RuleFor(i => i.FormVersionId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Form"))
                .MustAsync(async (model, time, ct) => await formService.IsFormActive(model.FormVersionId.Value))
                .When(i => !i.Id.HasValue)
                .WithMessage(i => ErrorConstants.FORM_INACTIVE_TEMPLATE);

            RuleFor(i => i.IncidentDateFrom)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Date"));
        }
    }
}
