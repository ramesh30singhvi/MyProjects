using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.Common.Filtration
{
    public class PortalReportFilter : FilterModel
    {
        public FilterOption Organization { get; set; }

        public FilterOption Report { get; set; }

        public IReadOnlyCollection<FilterOption> Facilities { get; set; }

        public IReadOnlyCollection<FilterOption> AuditType { get; set; }

        public FilterOption ReportType { get; set; }


        public FilterOption ReportCategory { get; set; }

        public DateFilterModel Date { get; set; }

        public ICollection<int> UserOrganizationIds { get; set; }

        public IReadOnlyCollection<int> ReportIds { get; set; }

        public ICollection<int> UserFacilityIds { get; set; }
    }
}
