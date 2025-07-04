using System;

namespace SHARP.BusinessLogic.DTO.ReportRequest
{
    public class AddReportRequestDto
    {
        public string AuditType { get; set; }

        public int OrganizationId { get; set; }

        public int? FacilityId { get; set; }

        public int FormId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
