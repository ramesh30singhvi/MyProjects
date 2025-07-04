using FluentValidation;
using SHARP.ViewModels.Form;

namespace SHARP.Validators
{
    public class EditQuestionValidator : AbstractValidator<EditCriteriaQuestionModel>
    {
        public EditQuestionValidator()
        {
            RuleFor(i => i.Id)
               .Cascade(CascadeMode.Stop)
               .NotEmpty();

            Include(new AddQuestionValidator());
        }
    }
}
