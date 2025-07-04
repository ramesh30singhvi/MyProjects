using System;

namespace SHARP.Common.Filtration
{
    public class AuditProgressNoteFilter
    {
        public int AuditId { get; set; }

        public int FacilityId { get; set; }

        public OptionFilter Keyword { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public double? TimeZoneOffset { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }
    }
}
