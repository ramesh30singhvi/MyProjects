using FluentValidation;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.ViewModels.Audit;

namespace SHARP.Validators
{
    public class AuditValidator : AbstractValidator<AuditAddEditModel>
    {
        public AuditValidator(IFormService formService)
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

            Transform(i => i.Room, v => v?.Trim())
                .Cascade(CascadeMode.Stop)
                .MaximumLength(15)
                .WithMessage(string.Format(ErrorConstants.MUST_CONTAINS_MAX_CHARACTERS, "Room", "15"));

            Transform(i => i.Resident, v => v?.Trim())
                .Cascade(CascadeMode.Stop)
                .MaximumLength(75)
                .WithMessage(string.Format(ErrorConstants.MUST_CONTAINS_MAX_CHARACTERS, "Resident Name", "75"));
        }
    }
}
