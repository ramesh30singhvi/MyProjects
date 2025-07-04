using SHARP.Common.Enums;
using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class Audit
    {
        public int Id { get; set; }

        public DateTime SubmittedDate { get; set; }

        public int? SubmittedByUserId { get; set; }

        public AuditStatus Status { get; set; }

        public int? TotalYES { get; set; }

        public int? TotalNO { get; set; }

        public int? TotalNA { get; set; }

        public double? TotalCompliance { get; set; }

        public string Unit { get; set; }

        public string Room { get; set; }

        public string Identifier { get; set; }

        public DateTime? IncidentDateFrom { get; set; }

        public DateTime? IncidentDateTo { get; set; }

        public string Reason { get; set; }

        public string ResidentName { get; set; }

        public int FacilityId { get; set; }

        public int FormVersionId { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? LastUnarchivedDate { get; set; }
        public AuditState State { get; set; }

        public DateTime? LastDeletedDate { get; set; }

        public bool? IsFilled { get; set; }


        public User SubmittedByUser { get; set; }

        public Facility Facility { get; set; }

        public FormVersion FormVersion { get; set; }

        public ICollection<AuditTableColumnValue> Values { get; set; }

        public ICollection<AuditFieldValue> AuditFieldValues { get; set; }
        public ICollection<AuditStatusHistory> AuditStatusHistory { get; set; }

        public ICollection<AuditSetting> Settings { get; set; }

        public ICollection<AuditTriggeredByKeyword> AuditsTriggeredByKeyword { get; set; }

        public ICollection<HighAlertAuditValue> HighAlertAuditValues { get; set; }

        public int? DeletedByUserId { get; set; }
        public User DeletedByUser { get; set; }

        public DateTime? SentForApprovalDate { get; set; }

        public PortalReport PortalReport { get; set; }

        public Audit() { }

        public Audit(
            int? userId, 
            DateTime submittedDate,
            AuditStatus status, 
            int? totalYES, 
            int? totalNO, 
            int? totalNA,
            double? totalCompliance,

            string unit,
            string room,
            string identifier,

            string reason,
            string residentName,

            int facilityId,
            Facility facility,
            int formVersionId,
            FormVersion formVersion,
            bool? isFilled,
            AuditState state)
        {
            SubmittedByUserId = userId;
            SubmittedDate = submittedDate;

            Status = status;
            State = state;
            TotalYES = totalYES;
            TotalNO = totalNO;
            TotalNA = totalNA;
            TotalCompliance = totalCompliance;

            Unit = unit;
            Room = room;
            Identifier = identifier;

            Reason = reason;
            ResidentName = residentName;

            FacilityId = facilityId;
            Facility = facility;
            FormVersionId = formVersionId;
            FormVersion = formVersion;

            IsFilled = isFilled;
            LastUnarchivedDate = null;
            LastDeletedDate = null;
            SentForApprovalDate = null;

            Values = new List<AuditTableColumnValue>();
            AuditFieldValues = new List<AuditFieldValue>();
            Settings = new List<AuditSetting>();
            AuditsTriggeredByKeyword = new List<AuditTriggeredByKeyword>();
            HighAlertAuditValues = new List<HighAlertAuditValue>();
            AuditStatusHistory = new List<AuditStatusHistory>();
        }

        public Audit Clone(int? userId)
        {
            return new Audit(
                userId, 
                DateTime.UtcNow,
                AuditStatus.Duplicated, 
                TotalYES, 
                TotalNO, 
                TotalNA, 
                TotalCompliance,
                Unit,
                Room,
                Identifier,
                Reason,
                ResidentName,
                FacilityId,
                Facility,
                FormVersionId,
                FormVersion,
                IsFilled,
                State);
        }
    }
}
