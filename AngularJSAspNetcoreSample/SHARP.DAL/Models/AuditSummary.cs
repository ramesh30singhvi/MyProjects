
namespace SHARP.DAL.Models
{
    public class AuditSummary
    {
        public string TypeOfAudit { get; set; }

        public string NumberOfAudits { get; set; }

        public string CompliancePercentage { get; set; }

        public string NonCompliantQuestion { get; set; }

        public string NonCompliantResident { get; set; }
    }
}
