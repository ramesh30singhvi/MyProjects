using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class AuditorProductivityInputFilter : FilterModel
    {
        public IReadOnlyCollection<FilterOption> Id { get; set; }
        public DateFilterModel StartTime { get; set; }
        public DateFilterModel CompletionTime { get; set; }
        public IReadOnlyCollection<FilterOption> UserFullName { get; set; }
        public IReadOnlyCollection<FilterOption> FacilityName { get; set; }
        public IReadOnlyCollection<FilterOption> TypeOfAudit { get; set; }
        public IReadOnlyCollection<FilterOption> NoOfFilteredAuditsAllType { get; set; }
        public IReadOnlyCollection<FilterOption> HandlingTime { get; set; }
        public IReadOnlyCollection<FilterOption> AHTPerAudit { get; set; }
        public IReadOnlyCollection<FilterOption> Hour { get; set; }
        public IReadOnlyCollection<FilterOption> NoOfFilteredAudits { get; set; }
        public IReadOnlyCollection<FilterOption> FinalAHT { get; set; }
        public IReadOnlyCollection<FilterOption> Month { get; set; }
        public IReadOnlyCollection<FilterOption> NoOfResidents { get; set; }

        public FilterOption Team { get;set; }
    }
}
