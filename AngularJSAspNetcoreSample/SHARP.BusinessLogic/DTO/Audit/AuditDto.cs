using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.User;
using SHARP.Common.Enums;
using System;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class AuditDto
    {
        public int Id { get; set; }

        public DateTime SubmittedDate { get; set; }

        public AuditStatus Status { get; set; }

        public AuditState State { get; set; }

        public int? TotalYES { get; set; }

        public int? TotalNO { get; set; }

        public int? TotalNA { get; set; }

        public double? TotalCompliance { get; set; }

        public UserOptionDto SubmittedByUser { get; set; }

        public FacilityOptionDto Facility { get; set; }

        public OptionDto Organization { get; set; }

        public FormOptionDto Form { get; set; }

        public string Unit { get; set; }

        public string Room { get; set; }

        public string Identifier { get; set; }

        public DateTime? IncidentDateFrom { get; set; }

        public DateTime? IncidentDateTo{ get; set; }

        public string Reason { get; set; }

        public string Resident { get; set; }

        public bool IsReadyForNextStatus { get; set; }

        public DateTime? LastDeletedDate { get; set; }

        public OptionDto HighAlertCategory { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }

        public UserOptionDto DeletedByUser { get; set; }
        public DateTime? SentForApprovalDate { get; set; }
        public DateTime? AuditCompletedDate { get; set; }
        public int? ReportTypeId { get; set; }

    }
}
