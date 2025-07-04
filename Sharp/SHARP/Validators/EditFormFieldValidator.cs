using FluentValidation;
using SHARP.ViewModels.Form;

namespace SHARP.Validators
{
    public class EditFormFieldValidator : AbstractValidator<EditFormFieldModel>
    {
        public EditFormFieldValidator()
        {
            RuleFor(i => i.Id)
               .Cascade(CascadeMode.Stop)
               .NotEmpty();

            Include(new AddFormFieldValidator());
        }
    }
}
