using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Facility
{
    public class AuditSummaryDto
    {
        public string TypeOfAudit { get; set; }

        public string NumberOfAudits { get; set; }

        public string CompliancePercentage { get; set; }

        public List<SummaryOfFindings> SummaryOfFindings { get; set; }
    }

}
