using FluentValidation;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using SHARP.ViewModels.Audit;

namespace SHARP.Validators
{
    public class DisapproveAuditValidator : AbstractValidator<AuditStatusRequestModel>
    {
        public DisapproveAuditValidator()
        {
            RuleFor(i => i.Comment)
               .Cascade(CascadeMode.Stop)
               .Must((model, time, ct) => !(model.Status == AuditStatus.Disapproved && string.IsNullOrEmpty(model.Comment)))
               .WithMessage(i => string.Format(ErrorConstants.MUST_NOT_BE_EMPTY, "Reason"));

        }
    }
}
