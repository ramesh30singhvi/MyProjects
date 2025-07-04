using SHARP.Common.Constants;
using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class AuditAddEditDto
    {
        public int? Id { get; set; }

        public int FacilityId { get; set; }

        public int FormVersionId { get; set; }

        public DateTime IncidentDateFrom { get; set; }

        public DateTime? IncidentDateTo { get; set; }

        public string Room { get; set; }

        public string Resident { get; set; }

        public int TotalYes { get; set; }

        public int TotalNo { get; set; }

        public int TotalNA { get; set; }

        public double TotalCompliance { get; set; }

        public IReadOnlyCollection<AuditAddEditValueDto> Values { get; set; }

        public IReadOnlyCollection<AuditAddEditSubHeaderValueDto> SubHeaderValues { get; set; }

        public DateTime CurrentClientDate { get; set; }

        public DateTime LastDeletedDate { get; set; }

        public OptionDto HighAlertCategory { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }
    }

    public class AuditAddEditValueDto : IIdModel<int>
    {
        public int Id { get; set; }

        public int TableColumnId { get; set; }

        public string Value { get; set; }

        public string AuditorComment { get; set; }
    }

    public class AuditAddEditSubHeaderValueDto : IIdModel<int>
    {
        public int Id { get; set; }

        public int FormFieldId { get; set; }

        public string Value { get; set; }
    }
}
