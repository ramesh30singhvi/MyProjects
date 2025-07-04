using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.DTO;
using SHARP.Common.Enums;
using System.Collections.Generic;
using System;

namespace SHARP.ViewModels.Report
{
    public class AIAuditReportV2ViewModel
    {

        public int Id { get; set; }
        public string OrganizationName { get; set; }

        public string FacilityName { get; set; }
        public string AuditorName { get; set; }

        public string AuditTime { get; set; }

        public string AuditDate { get; set; }

        public string FilteredDate { get; set; }


        public DateTime? SubmittedDate { get; set; }

        public ReportAIStatus Status { get; set; }

        public DateTime? SentForApprovalDate { get; set; }

        public AIAuditState State { get; set; }

        public OptionModel Organization { get; set; }

        public OptionModel Facility { get; set; }

        public IReadOnlyCollection<AuditAIPatientPdfNotesViewModel> Values { get; set; }
    }
}
