using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class FormVersionFilter : FilterModel
    {
        public IReadOnlyCollection<FilterOption> Name { get; set; }

        public IReadOnlyCollection<FilterOption> Organizations { get; set; }

        public IReadOnlyCollection<FilterOption> AuditType { get; set; }

        public IReadOnlyCollection<FilterOption> Status { get; set; }

        public DateFilterModel CreatedDate { get; set; }
    }
}
