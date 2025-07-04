namespace SHARP.BusinessLogic.DTO.Form
{
    public class FormFieldValueDto
    {
        public int Id { get; set; }

        public int FormFieldId { get; set; }

        public string Value { get; set; }

        public string FormattedValue { get; set; }
    }
}
