using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.Report
{
    public class AuditAIReportFilterModel : BaseFilterModel
    {
        public IReadOnlyCollection<FilterOptionModel> OrganizationName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FacilityName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> SummaryAI { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Keywords { get; set; }

        public IReadOnlyCollection<FilterOptionModel> PdfFileName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> ContainerName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> AuditorName { get; set; }

        public IReadOnlyCollection<FilterOptionModel> AuditTime { get; set; }

        public string AuditDate { get; set; }

        public IReadOnlyCollection<FilterOptionModel> FilteredDate { get; set; }

        public string CreatedAt { get; set; }

        public IReadOnlyCollection<FilterOptionModel> Status { get; set; }

        public string SubmittedDate { get; set; }

        public int State { get; set; }

    }
}
