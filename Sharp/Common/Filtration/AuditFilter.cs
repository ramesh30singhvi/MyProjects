using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.Common.Filtration
{
    public class AuditFilter : FilterModel
    {
        public IReadOnlyCollection<FilterOption> Organization { get; set; }

        public IReadOnlyCollection<FilterOption> Facility { get; set; }

        public IReadOnlyCollection<FilterOption> Form { get; set; }

        public IReadOnlyCollection<FilterOption> AuditType { get; set; }

        public IReadOnlyCollection<FilterOption> Auditor { get; set; }

        public IReadOnlyCollection<FilterOption> Unit { get; set; }

        public IReadOnlyCollection<FilterOption> Room { get; set; }

        public IReadOnlyCollection<FilterOption> Resident { get; set; }

        public IReadOnlyCollection<FilterOption> Status { get; set; }

        public DateFilterModel AuditDate { get; set; }

        public AuditState State { get; set; }

        public DateFilterModel IncidentDateFrom { get; set; }

        public DateFilterModel IncidentDateTo { get; set; }

        public NumberFilterModel Compliance { get; set; }

        public ICollection<AuditStatus> StatusFilter { get; set; }

        public ICollection<int> UserOrganizationIds { get; set; }

        public DateFilterModel LastDeletedDate { get; set; }

        public IReadOnlyCollection<FilterOption> DeletedByUser { get; set; }
    }
}
