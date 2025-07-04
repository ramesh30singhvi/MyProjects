using System;

namespace SHARP.ViewModels.Audit
{
    public class AuditProgressNoteFilterModel
    {
        public int KeywordId { get; set; }

        public string KeywordName { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public double? TimeZoneOffset { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }
    }
}
