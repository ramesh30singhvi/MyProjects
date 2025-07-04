namespace SHARP.ViewModels.Form
{
    public class FormOptionModel : OptionModel
    {
        public OptionModel AuditType { get; set; }

        public OptionModel ReportRange { get; set; }

        public bool IsActive { get; set; }

        public int DisableCompliance { get; set; }

        public int AllowEmptyComment { get; set; }

        public bool UseHighAlert { get; set; }

        public int AHTime { get; set; }

        public int OrganizationId { get; set; }

        public int? FormId { get; set; }
    }
}
