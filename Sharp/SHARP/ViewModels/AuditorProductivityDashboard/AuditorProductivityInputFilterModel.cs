using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.AuditorProductivityDashboard
{
    public class AuditorProductivityInputFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<FilterOptionModel> Id { get; set; }

        public string StartTime { get; set; }

        public string CompletionTime { get; set; }

        public FilterOptionModel Team { get; set; }

        public IReadOnlyCollection<FilterOptionModel> UserFullName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FacilityName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> TypeOfAudit { get; set; }

        public IReadOnlyCollection<FilterOptionModel> NoOfFilteredAuditsAllType { get; set; }

        public IReadOnlyCollection<FilterOptionModel> HandlingTime { get; set; }

        public IReadOnlyCollection<FilterOptionModel> AHTPerAudit { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Hour { get; set; }

        public IReadOnlyCollection<FilterOptionModel> NoOfFilteredAudits { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FinalAHT { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Month { get; set; }

        public IReadOnlyCollection<FilterOptionModel> NoOfResidents { get; set; }
    }
}
