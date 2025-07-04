using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class TrackerOptionDto
    {
        public bool IsRequired { get; set; }

        public bool Compliance { get; set; }

        public bool Quality { get; set; }

        public bool Priority { get; set; }

        public OptionDto FieldType { get; set; }

        public IReadOnlyCollection<FormFieldItemDto> Items { get; set; }
    }
}
