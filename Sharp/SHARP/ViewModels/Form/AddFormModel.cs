namespace SHARP.ViewModels.Form
{
    public class AddFormModel
    {
        public string Name { get; set; }

        public int OrganizationId { get; set; }

        public int AuditTypeId { get; set; }


        public int DisableCompliance { get; set; }

        public int AllowEmptyComment { get; set; }

        public bool UseHighAlert { get; set; }
    }
}
