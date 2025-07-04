using FluentValidation;
using SHARP.Common.Enums;
using SHARP.ViewModels.Form;
using System.Linq;

namespace SHARP.Validators
{
    public class AddTrackerQuestionValidator : AbstractValidator<AddTrackerQuestionModel>
    {
        public AddTrackerQuestionValidator()
        {
            RuleFor(i => i.FormVersionId)
               .Cascade(CascadeMode.Stop)
               .NotEmpty();

            RuleFor(i => i.FieldTypeId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty();

            RuleFor(i => i.Question)
                .Cascade(CascadeMode.Stop)
                .NotEmpty();

            RuleFor(i => i.Items)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .When(model => (
               model.FieldTypeId == (int)FieldTypes.DropdownSingleSelect ||
               model.FieldTypeId == (int)FieldTypes.ToggleSingleSelect || 
               model.FieldTypeId == (int)FieldTypes.DropdownMultiselect || 
               model.FieldTypeId == (int)FieldTypes.ToggleMultiselect) && (model.Items == null || !model.Items.Any()))
               .WithMessage("The list of values must contain at least one item");
        }
    }
}
