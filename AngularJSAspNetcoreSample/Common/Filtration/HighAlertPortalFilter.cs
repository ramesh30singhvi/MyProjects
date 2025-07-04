using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.Common.Filtration
{
    public class HighAlertPortalFilter : FilterModel
    {
        public FilterOption Organization { get; set; }

        public FilterOption Facility { get; set; }


        public FilterOption HighAlertCategory { get; set; }

        public DateFilterModel Date { get; set; }

        public FilterOption HighAlertStatus { get; set; }

        public IReadOnlyCollection<FilterOption> PotentialAreas { get; set; }
    }
}
