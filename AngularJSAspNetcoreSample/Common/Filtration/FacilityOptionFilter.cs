using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class FacilityOptionFilter : FilterModel
    {
        public ICollection<int> OrganizationIds { get; set; }
        public ICollection<int> FacilityIds { get; set; }
    }
}
