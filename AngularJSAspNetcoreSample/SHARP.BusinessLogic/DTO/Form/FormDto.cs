namespace SHARP.BusinessLogic.DTO.Form
{
    public class FormDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public OptionDto AuditType { get; set; }

        public bool IsActive { get; set; }
    }
}
