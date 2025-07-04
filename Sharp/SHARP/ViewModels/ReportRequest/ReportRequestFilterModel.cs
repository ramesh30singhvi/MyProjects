using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.ReportRequest
{
    public class ReportRequestFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<FilterOptionModel> OrganizationName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FacilityName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FormName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> AuditType { get; set; }

        public IReadOnlyCollection<FilterOptionModel> UserFullName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Status { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public string RequestedTime { get; set; }

        public string GeneratedTime { get; set; }
    }
}
