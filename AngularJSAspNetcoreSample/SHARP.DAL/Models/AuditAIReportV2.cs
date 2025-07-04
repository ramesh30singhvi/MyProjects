using SHARP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class AuditAIReportV2
    {

        public int Id { get; set; }

        public int OrganizationId { get; set; }

        public int? FacilityId { get; set; }
    
        public string PdfFileName { get; set; }

        public string AuditorName { get; set; }

        public string AuditTime { get; set; }

        public string AuditDate { get; set; }

        public string FilteredDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public ReportAIStatus Status { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public DateTime? SentForApprovalDate { get; set; }

        public AIAuditState State { get; set; }

        public Organization Organization { get; set; }

        public Facility Facility { get; set; }

        public ICollection<AuditAIPatientPdfNotes> Values { get; set; }

        public AuditAIReportV2()
        {
            Values = new List<AuditAIPatientPdfNotes>();
        }
    }
}
