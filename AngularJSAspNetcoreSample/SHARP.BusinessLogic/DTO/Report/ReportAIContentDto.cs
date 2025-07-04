using SHARP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class ReportAIContentDto
    {
        public int Id { get; set; }

        public int OrganizationId { get; set; }

        public int? FacilityId { get; set; }

        public string SummaryAI { get; set; }

        public string Keywords { get; set; }

        public string PdfFileName { get; set; }

        public string ContainerName { get; set; }

        public string AuditorName { get; set; }

        public string AuditTime { get; set; }

        public DateTime AuditDate { get; set; }

        public string FilteredDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public ReportAIStatus Status { get; set; }
        
        public DateTime? SentForApprovalDate { get; set; }

        public AIAuditState State { get; set; }

        public OptionDto Organization { get; set; }

        public OptionDto Facility { get; set; }
    }
}
