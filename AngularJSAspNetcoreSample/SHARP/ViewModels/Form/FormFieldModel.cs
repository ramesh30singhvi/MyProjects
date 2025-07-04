using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class FormFieldModel
    {
        public int Id { get; set; }

        public int Sequence { get; set; }

        public string FieldName { get; set; }

        public string LabelName { get; set; }

        public bool IsRequired { get; set; }

        public OptionModel FieldType { get; set; }

        public IReadOnlyCollection<FormFieldItemModel> Items { get; set; }

        public ControlOptionModel Value { get; set; }
    }
}
