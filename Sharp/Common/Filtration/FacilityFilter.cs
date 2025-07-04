using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class FacilityFilter : FilterModel
    {
        public IReadOnlyCollection<FilterOption> Name { get; set; }

        public IReadOnlyCollection<FilterOption> TimeZone { get; set; }

        public IReadOnlyCollection<FilterOption> Active { get; set; }

        public int OrganizationId { get; set; }

    }
}
