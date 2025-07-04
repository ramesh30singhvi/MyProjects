using FluentValidation;
using SHARP.ViewModels.Form;

namespace SHARP.Validators
{
    public class AddQuestionValidator : AbstractValidator<AddCriteriaQuestionModel>
    {
        public AddQuestionValidator()
        {
            RuleFor(i => i.FormVersionId)
               .Cascade(CascadeMode.Stop)
               .NotEmpty();

            RuleFor(i => i.Question)
                .Cascade(CascadeMode.Stop)
                .NotEmpty();
        }
    }
}
