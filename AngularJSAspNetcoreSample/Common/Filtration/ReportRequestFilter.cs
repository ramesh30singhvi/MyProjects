using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class ReportRequestFilter : FilterModel
    {
        public IReadOnlyCollection<FilterOption> Organization { get; set; }

        public IReadOnlyCollection<FilterOption> Facility { get; set; }

        public IReadOnlyCollection<FilterOption> Form { get; set; }

        public IReadOnlyCollection<FilterOption> AuditType { get; set; }

        public IReadOnlyCollection<FilterOption> User { get; set; }

        public IReadOnlyCollection<FilterOption> Status { get; set; }

        public DateFilterModel FromDate { get; set; }

        public DateFilterModel ToDate { get; set; }

        public DateFilterModel RequestedTime { get; set; }

        public DateFilterModel GeneratedTime { get; set; }

        public int? UserId { get; set; }
    }
}
