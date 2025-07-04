using System;

namespace SHARP.ViewModels.Report
{
    public class ReportAIContentModel
    {
        public int Id { get; set; }

        public string OrganizationName { get; set; }

        public string FacilityName { get; set; }

        public string SummaryAI { get; set; }

        public string Keywords { get; set; }

        public string PdfFileName { get; set; }

        public string ContainerName { get; set; }

        public string AuditorName { get; set; }

        public string AuditTime { get; set; }

        public DateTime AuditDate { get; set; }

        public string FilteredDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public int Status { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public DateTime? SentForApprovalDate { get; set; }

        public int State { get; set; }
    }
}
