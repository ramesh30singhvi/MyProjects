using SHARP.BusinessLogic.DTO;
using SHARP.Common.Enums;
using SHARP.ViewModels.Facilitty;
using SHARP.ViewModels.Form;
using System;

namespace SHARP.ViewModels.Audit
{
    public class AuditModel
    {
        public int Id { get; set; }

        public DateTime SubmittedDate { get; set; }

        public AuditStatus Status { get; set; }
        public AuditState State { get; set; }

        public int? TotalYES { get; set; }

        public int? TotalNO { get; set; }

        public int? TotalNA { get; set; }

        public double? TotalCompliance { get; set; }

        public UserOptionModel SubmittedByUser { get; set; }

        public FacilityOptionModel Facility { get; set; }

        public OptionModel Organization { get; set; }

        public OptionModel AuditType { get; set; }

        public FormOptionModel Form { get; set; }

        public string Unit { get; set; }

        public string Room { get; set; }

        public string Identifier { get; set; }

        public DateTime? IncidentDateFrom { get; set; }

        public DateTime? IncidentDateTo { get; set; }

        public string Reason { get; set; }

        public string Resident { get; set; }

        public bool IsReadyForNextStatus { get; set; }

        public DateTime? LastDeletedDate { get; set; }

        public OptionModel HighAlertCategory { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }

        public int? ReportTypeId { get; set; }
    }
}
