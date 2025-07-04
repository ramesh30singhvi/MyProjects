using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.Audit
{
    public class AuditFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<FilterOptionModel> OrganizationName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FacilityName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FormName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> AuditType { get; set; }

        public IReadOnlyCollection<FilterOptionModel> AuditorName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Unit { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Room { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Resident { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Status { get; set; }

        public string AuditDate { get; set; }

        public string IncidentDateFrom { get; set; }

        public string IncidentDateTo { get; set; }

        public string Compliance { get; set; }

        public int State { get; set; }

        public string LastDeletedDate { get; set; }

        public IReadOnlyCollection<FilterOptionModel> DeletedByUser { get; set; }
    }
}
