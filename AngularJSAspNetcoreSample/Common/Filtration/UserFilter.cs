using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class UserFilter : FilterModel
    {
        public IReadOnlyCollection<FilterOption> Name { get; set; }

        public IReadOnlyCollection<FilterOption> Email { get; set; }

        public IReadOnlyCollection<FilterOption> Role { get; set; }

        public IReadOnlyCollection<FilterOption> Access { get; set; }

        public IReadOnlyCollection<FilterOption> FacilityAccess { get; set; }

        public IReadOnlyCollection<FilterOption> Status { get; set; }

        public int SiteId { get; set; }
    }
}
