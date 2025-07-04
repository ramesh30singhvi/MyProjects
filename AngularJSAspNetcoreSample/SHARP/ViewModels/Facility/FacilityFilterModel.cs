using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.Facility
{
    public class FacilityFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<FilterOptionModel> Name { get; set; }

        public IReadOnlyCollection<FilterOptionModel> TimeZoneName { get; set; }

        public ICollection<FilterOptionModel> Active { get; set; }

        public int OrganizationId { get; set; }

    }
}
