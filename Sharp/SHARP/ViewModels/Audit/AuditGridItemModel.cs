using System;

namespace SHARP.ViewModels.Audit
{
    public class AuditGridItemModel
    {
        public int Id { get; set; }

        public string OrganizationName { get; set; }

        public string FacilityName { get; set; }

        public string FormName { get; set; }

        public string AuditType { get; set; }

        public DateTime AuditDate { get; set; }

        public string AuditorUserId { get; set; }

        public string AuditorName { get; set; }

        public string Unit { get; set; }

        public string Room { get; set; }

        public string Resident { get; set; }

        public string Identifier { get; set; }

        public DateTime? IncidentDateFrom { get; set; }

        public DateTime? IncidentDateTo { get; set; }

        public int Compliance { get; set; }

        public int Status { get; set; }
        public int State { get; set; }

        public string Reason { get; set; }

        public bool IsReadyForNextStatus { get; set; }

        public DateTime? LastDeletedDate { get; set; }

        public string DeletedByUser { get; set; }

        public DateTime? SentForApprovalDate { get; set; }

        public DateTime? AuditCompletedDate { get; set; }
        public int? ReportTypeId { get; set; }
    }
}
