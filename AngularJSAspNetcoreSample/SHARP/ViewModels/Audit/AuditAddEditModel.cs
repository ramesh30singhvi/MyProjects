using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Audit
{
    public class AuditAddEditModel
    {
        public int? Id { get; set; }

        public int? FacilityId { get; set; }

        public int? FormVersionId { get; set; }

        public DateTime IncidentDateFrom { get; set; }

        public DateTime? IncidentDateTo { get; set; }

        public string Room { get; set; }

        public string Resident { get; set; }

        public int TotalYes { get; set; }

        public int TotalNo { get; set; }

        public int TotalNA { get; set; }

        public double TotalCompliance { get; set; }

        public IReadOnlyCollection<AuditAddEditValueModel> Values { get; set; }

        public IReadOnlyCollection<AuditAddEditSubHeaderValueModel> SubHeaderValues { get; set; }

        public OptionModel HighAlertCategory { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }
    }

    public class AuditAddEditValueModel
    {
        public int? Id { get; set; }

        public int TableColumnId { get; set; }

        public string Value { get; set; }

        public string AuditorComment { get; set; }
    }

    public class AuditAddEditSubHeaderValueModel
    {
        public int? Id { get; set; }

        public int FormFieldId { get; set; }

        public string Value { get; set; }
    }
}
