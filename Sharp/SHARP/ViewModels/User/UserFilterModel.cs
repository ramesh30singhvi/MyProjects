using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.User
{
    public class UserFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<FilterOptionModel> Name { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Email { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Role { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Access { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FacilityAccess { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Status { get; set; }

        public int SiteId { get; set; }
    }
}
