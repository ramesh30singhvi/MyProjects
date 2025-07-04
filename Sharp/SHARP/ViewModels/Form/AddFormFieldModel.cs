using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class AddFormFieldModel
    {
        public int FormVersionId { get; set; }

        public int Sequence { get; set; }

        public string FieldName { get; set; }

        public string LabelName { get; set; }

        public bool IsRequired { get; set; }

        public int FieldTypeId { get; set; }

        public IReadOnlyCollection<FormFieldItemModel> Items { get; set; }
    }
}
