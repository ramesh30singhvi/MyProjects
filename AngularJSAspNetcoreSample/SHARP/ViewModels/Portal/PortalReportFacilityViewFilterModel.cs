using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.Portal
{
    public class PortalReportFacilityViewFilterModel : BaseFilterModel
    {

        public FilterOptionModel Facility { get; set; }
        public FilterOptionModel ReportType { get; set; }

        public FilterOptionModel ReportCategory { get; set; }

        public FilterOptionModel Organization { get; set; }

        public string Date { get; set; }
    }
}
