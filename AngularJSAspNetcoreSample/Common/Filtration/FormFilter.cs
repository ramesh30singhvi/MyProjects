using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class FormFilter : FilterModel
    {
        public IReadOnlyCollection<FilterOption> Name { get; set; }

        public IReadOnlyCollection<FilterOption> AuditType { get; set; }

        public IReadOnlyCollection<FilterOption> IsFormActive { get; set; }

        public IReadOnlyCollection<FilterOption> SettingType { get; set; }

        public IReadOnlyCollection<FilterOption> ScheduleSetting { get; set; }

        public int? OrganizationId { get; set; }
    }
}
