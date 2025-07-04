using FluentValidation;
using SHARP.Common.Constants;
using SHARP.ViewModels.Memo;

namespace SHARP.Validators
{
    public class AddMemoValidator : AbstractValidator<AddMemoModel>
    {
        public AddMemoValidator()
        {
            RuleFor(i => i.Text)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must((model, time, ct) => !HasInvalidString(model.Text))
                .WithMessage(i => string.Format(ErrorConstants.CONTAINS_INVALID_CHARACTERS, nameof(i.Text)));
        }

        private bool HasInvalidString(string str)
        {
            string[] invalidStrs = new string[] { "<script>", "delete*" };

            foreach (string invaliStr in invalidStrs)
            {
                if(str.ToLower().Replace(" ","").Contains(invaliStr))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
