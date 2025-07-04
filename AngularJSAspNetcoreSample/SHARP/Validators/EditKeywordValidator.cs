using FluentValidation;
using SHARP.ViewModels.Audit;

namespace SHARP.Validators
{
    public class EditKeywordValidator : AbstractValidator<EditKeywordModel>
    {
        public EditKeywordValidator()
        {
            RuleFor(i => i.Id)
               .Cascade(CascadeMode.Stop)
               .NotEmpty();

            Include(new AddKeywordValidator());
        }
    }
}
