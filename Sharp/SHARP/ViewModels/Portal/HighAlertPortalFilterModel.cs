using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.Portal
{
    public class HighAlertPortalFilterModel : BaseFilterModel
    {

        public FilterOptionModel Organization { get; set; }

        public FilterOptionModel Facility { get; set; }

        public FilterOptionModel ReportType { get; set; }

        public FilterOptionModel HighAlertStatus { get; set; }

        public FilterOptionModel HighAlertCategory { get; set; }

        public IReadOnlyCollection<FilterOptionModel> PotentialAreas { get; set; }

        public string Date { get; set; }
    }
}
