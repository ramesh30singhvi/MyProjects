using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class TrackerOptionModel
    {
        public bool IsRequired { get; set; }

        public bool Compliance { get; set; }

        public bool Quality { get; set; }

        public bool Priority { get; set; }

        public OptionModel FieldType { get; set; }

        public IReadOnlyCollection<FormFieldItemModel> Items { get; set; }
    }
}
