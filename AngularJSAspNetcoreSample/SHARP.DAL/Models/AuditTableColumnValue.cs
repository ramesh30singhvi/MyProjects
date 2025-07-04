using SHARP.Common.Constants;
using System;

namespace SHARP.DAL.Models
{
    public class AuditTableColumnValue : IIdModel<int>
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public int TableColumnId { get; set; }

        public string Value { get; set; }

        public string AuditorComment { get; set; }

        public string DONComment { get; set; }

        public string Resident { get; set; }

        public DateTime? ProgressNoteDate { get; set; }

        public TimeSpan? ProgressNoteTime { get; set; }

        public string Description { get; set; }

        public string GroupId { get; set; }

        public Audit Audit { get; set; }

        public TableColumn Column { get; set; }

        public int HighAlertID { get; set; }

        public HighAlertAuditValue HighAlertAuditValue { get; set; }

        public AuditTableColumnValue() { }

        public AuditTableColumnValue(
            Audit audit, 
            int tableColumnId,
            string value,
            string auditorComment,
            string donComment, 
            string resident,
            DateTime? progressNoteDate,
            TimeSpan? progressNoteTime,
            string description,
            string groupId,
            int highAlertAuditValueId)
        {
            Audit = audit;
            TableColumnId = tableColumnId;

            Value = value ?? string.Empty;
            AuditorComment = auditorComment;
            DONComment = donComment;

            Resident = resident;
            ProgressNoteDate = progressNoteDate;
            ProgressNoteTime = progressNoteTime;
            Description = description;
            GroupId = groupId;
            HighAlertID = highAlertAuditValueId;
        }

        public AuditTableColumnValue Clone(Audit audit, string groupId)
        {
            return new AuditTableColumnValue(
                audit, 
                TableColumnId,
                Value ?? string.Empty,
                AuditorComment, 
                DONComment, 
                Resident,
                ProgressNoteDate,
                ProgressNoteTime,
                Description,
                groupId,HighAlertID);
        }
    }
}
