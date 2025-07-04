using FluentValidation;
using SHARP.ViewModels.Common;

namespace SHARP.Validators
{
    public class RearrangeItemsValidator : AbstractValidator<RearrangeItemsModel>
    {
        public RearrangeItemsValidator()
        {
            RuleFor(i => i.Items)
               .Cascade(CascadeMode.Stop)
               .NotEmpty();
        }
    }
}
