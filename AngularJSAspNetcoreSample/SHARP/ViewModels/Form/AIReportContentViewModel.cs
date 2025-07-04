using SHARP.BusinessLogic.DTO;
using SHARP.Common.Enums;
using SHARP.ViewModels.Facilitty;

namespace SHARP.ViewModels.Form
{
    public class AIReportContentViewModel
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

        public string AuditDate { get; set; }

        public string FilteredDate { get; set; }

        public string CreatedAt { get; set; }

        public string SubmittedDate { get; set; }

        public ReportAIStatus Status { get; set; }

        public string SentForApprovalDate { get; set; }

        public AIAuditState State { get; set; }

        public OptionModel Organization { get; set; }

        public OptionModel Facility { get; set; }
    }
}
