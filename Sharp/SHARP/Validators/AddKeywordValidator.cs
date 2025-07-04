using FluentValidation;
using SHARP.ViewModels.Audit;

namespace SHARP.Validators
{
    public class AddKeywordValidator : AbstractValidator<AddKeywordModel>
    {
        public AddKeywordValidator()
        {
            RuleFor(i => i.KeywordId)
               .Cascade(CascadeMode.Stop)
               .NotEmpty();

            RuleFor(i => i.AuditId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty();

            RuleFor(i => i.Resident)
                .Cascade(CascadeMode.Stop)
                .NotEmpty();

            RuleFor(i => i.ProgressNoteDate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty();

            RuleFor(i => i.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty();
        }
    }
}
