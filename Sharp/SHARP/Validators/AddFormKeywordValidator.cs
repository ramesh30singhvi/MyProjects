using FluentValidation;
using SHARP.ViewModels.Form;

namespace SHARP.Validators
{
    public class AddFormKeywordValidator : AbstractValidator<AddFormKeywordModel>
    {
        public AddFormKeywordValidator()
        {
            RuleFor(i => i.FormVersionId)
               .Cascade(CascadeMode.Stop)
               .NotEmpty();

            RuleFor(i => i.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty();
        }
    }
}
