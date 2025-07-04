using FluentValidation;
using SHARP.ViewModels.Form;

namespace SHARP.Validators
{
    public class EditTrackerQuestionValidator : AbstractValidator<EditTrackerQuestionModel>
    {
        public EditTrackerQuestionValidator()
        {
            RuleFor(i => i.Id)
               .Cascade(CascadeMode.Stop)
               .NotEmpty();

            Include(new AddTrackerQuestionValidator());
        }
    }
}
