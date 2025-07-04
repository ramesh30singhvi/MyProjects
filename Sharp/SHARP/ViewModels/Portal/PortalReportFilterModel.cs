using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.Portal
{
    public class PortalReportFilterModel : BaseFilterModel
    {
        public FilterOptionModel Organization { get; set; }

        public FilterOptionModel Report { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Facilities { get; set; }

        public FilterOptionModel ReportType { get; set; }


        public FilterOptionModel ReportCategory { get; set; }

        public string Date { get; set; }


    }
}
