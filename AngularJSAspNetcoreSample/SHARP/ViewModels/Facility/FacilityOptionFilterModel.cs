using SHARP.ViewModels.Base;
using System.Collections.Generic;

namespace SHARP.ViewModels.Facility
{
    public class FacilityOptionFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<int> OrganizationIds { get; set; }
    }
}
