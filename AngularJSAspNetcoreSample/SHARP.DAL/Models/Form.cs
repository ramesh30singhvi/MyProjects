using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class Form
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int AuditTypeId { get; set; }

        public bool IsActive { get; set; }

        public int? LegacyFormId { get; set; }

        public int? OrganizationId { get; set; }

        public int DisableCompliance { get; set; }

        public int AllowEmptyComment { get; set; }

        public AuditType AuditType { get; set; }

        public Organization Organization { get; set; }

        public ICollection<FormVersion> Versions { get; set; }

        public ICollection<FormOrganization> FormOrganizations { get; set; }

        public bool UseHighAlert { get;set; }

        public int? AHTime { get; set; }
    }
}
