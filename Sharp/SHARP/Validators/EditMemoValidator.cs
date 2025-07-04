using FluentValidation;
using SHARP.ViewModels.Memo;

namespace SHARP.Validators
{
    public class EditMemoValidator : AbstractValidator<EditMemoModel>
    {
        public EditMemoValidator()
        {
            RuleFor(i => i.Id)
              .Cascade(CascadeMode.Stop)
              .NotEmpty();

            Include(new AddMemoValidator());
        }
    }
}
