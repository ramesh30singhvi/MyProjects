namespace SHARP.BusinessLogic.DTO.Form
{
    public class FormOptionDto : OptionDto
    {
        public OptionDto AuditType { get; set; }
        public bool IsActive { get; set; }

        public int OrganizationId { get; set; }

        public int? FormId { get; set; }
        public int DisableCompliance { get; set; }

        public int AllowEmptyComment { get; set; }

        public bool UseHighAlert { get; set; }

        public int? AHTime { get; set; }
    }
}
