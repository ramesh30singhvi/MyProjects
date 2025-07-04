using FluentValidation;
using SHARP.Common.Constants;
using SHARP.ViewModels.Audit;

namespace SHARP.Validators
{
    public class AddReportRequestValidator : AbstractValidator<PdfFilterModel>
    {
        public AddReportRequestValidator()
        {
            RuleFor(i => i.OrganizationId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Organization"));

            RuleFor(i => i.FacilityId)
               .Cascade(CascadeMode.Stop)
               .Must((model, time, ct) => !(model.AuditType == CommonConstants.CRITERIA && !model.FacilityId.HasValue))
               .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Facility"));

            RuleFor(i => i.FormId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Form"));

            RuleFor(i => i.FromDate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Date"));
        }
    }
}
