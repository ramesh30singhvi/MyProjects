using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.Common.Filtration
{
    public class PortalReportFacilityViewFilter : FilterModel
    {
        public FilterOption Organization { get; set; }
        public FilterOption Facility { get; set; }

        public FilterOption ReportType { get; set; }


        public FilterOption ReportCategory { get; set; }

        public DateFilterModel Date { get; set; }
    }
}
