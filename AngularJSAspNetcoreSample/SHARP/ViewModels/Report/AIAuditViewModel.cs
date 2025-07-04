using SHARP.BusinessLogic.DTO.Report;
using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Report
{
    public class AIAuditViewModel
    {
        public int Id { get; set; }

        public string OrganizationName { get; set; }

        public string FacilityName { get; set; }


        public string AuditorName { get; set; }

        public string AuditTime { get; set; }

        public DateTime AuditDate { get; set; }

        public string FilteredDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public int Status { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public DateTime? SentForApprovalDate { get; set; }

        public int State { get; set; }

        public IReadOnlyCollection<AuditAIPatientPdfNotesViewModel> Values { get; set; }
    }
}
