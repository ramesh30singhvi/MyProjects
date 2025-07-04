using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class FormFieldDto
    {
        public int Id { get; set; }

        public int Sequence { get; set; }

        public string FieldName { get; set; }

        public string LabelName { get; set; }

        public bool IsRequired { get; set; }

        public OptionDto FieldType { get; set; }

        public IReadOnlyCollection<FormFieldItemDto> Items { get; set; }

        public FormFieldValueDto Value { get; set; }
    }
}
