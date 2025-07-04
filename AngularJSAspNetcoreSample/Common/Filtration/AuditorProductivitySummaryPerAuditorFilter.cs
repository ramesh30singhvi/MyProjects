using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class AuditorProductivitySummaryPerAuditorFilter : FilterModel
    {
        public DateFilterModel DateProcessed { get; set; }
        public IReadOnlyCollection<FilterOption> UserFullName { get; set; }
        public IReadOnlyCollection<FilterOption> FacilityName { get; set; }
        public IReadOnlyCollection<FilterOption> TypeOfAudit { get; set; }

        public FilterOption Team { get; set; }
    }
}
