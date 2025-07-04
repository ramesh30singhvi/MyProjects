using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class AuditFilterColumnSource<T> : FilterColumnSource<T>
    {
        public ICollection<int> UserOrganizationIds { get; set; }

        public AuditFilter AuditFilter { get; set; }
    }
}
