using SHARP.Common.Filtration;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.AuditorProductivityDashboard
{
    public class AuditorProductivityAHTPerAuditTypeFilterModel : FilterModel
    {
        public string DateProcessed { get; set; }

        public IReadOnlyCollection<FilterOptionModel> UserFullName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FacilityName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> TypeOfAudit { get; set; }

        public FilterOptionModel Team { get; set; }
    }
}
