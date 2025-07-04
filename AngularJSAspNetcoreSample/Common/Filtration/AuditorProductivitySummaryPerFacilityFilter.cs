using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.Common.Filtration
{
    public  class AuditorProductivitySummaryPerFacilityFilter : FilterModel
    {
        public DateFilterModel DateProcessed { get; set; }
        public FilterOption Organization { get; set; }
        public IReadOnlyCollection<FilterOption> Facilities { get; set; }
    }
}
