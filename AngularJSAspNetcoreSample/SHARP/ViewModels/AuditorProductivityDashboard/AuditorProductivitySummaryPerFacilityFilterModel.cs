using SHARP.Common.Filtration;
using SHARP.ViewModels.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SHARP.ViewModels.AuditorProductivityDashboard
{
    public class AuditorProductivitySummaryPerFacilityFilterModel : FilterModel
    {
        [Required]
        public string DateProcessed { get; set; }
        [Required]
        public FilterOptionModel Organization { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Facilities { get; set; }
    }
}
