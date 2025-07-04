namespace SHARP.DAL.Models
{
    public class FormFieldItem
    {
        public int Id { get; set; }

        public int FormFieldId { get; set; }

        public string Value { get; set; }

        public string? Code { get; set; }

        public int Sequence { get; set; }

        public FormField FormField { get; set; }

        public FormFieldItem() { }

        public FormFieldItem(string value, int sequence, FormField formField)
        {
            Value = value;
            Sequence = sequence;
            FormField = formField;
        }

        public FormFieldItem Clone(FormField formField)
        {
            FormFieldItem formFieldItem = new FormFieldItem(Value, Sequence, formField);
            formFieldItem.Code = Code;
            return formFieldItem;
        }
    }
}
