using FluentValidation;
using SHARP.ViewModels.Form;

namespace SHARP.Validators
{
    public class EditFormKeywordValidator : AbstractValidator<EditFormKeywordModel>
    {
        public EditFormKeywordValidator()
        {
            RuleFor(i => i.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty();
        }
    }
}
