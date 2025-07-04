using SHARP.Common.Enums;

namespace SHARP.DAL.Models.QueryModels
{
    public class AuditKPIQueryModel
    {
        public int OrganizationId { get; set; }

        public string OrganizationName { get; set; }

        public AuditStatus AuditStatus { get; set; }

        public int AuditCount { get; set; }
    }
}
