using SHARP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.Common.Filtration
{
    public class AuditAIReportFilter :  FilterModel
    {
        public IReadOnlyCollection<FilterOption> OrganizationName { get; set; }

        public IReadOnlyCollection<FilterOption> FacilityName { get; set; }

        public IReadOnlyCollection<FilterOption> SummaryAI { get; set; }

        public IReadOnlyCollection<FilterOption> Keywords { get; set; }

        public IReadOnlyCollection<FilterOption> PdfFileName { get; set; }

        public IReadOnlyCollection<FilterOption> ContainerName { get; set; }

        public IReadOnlyCollection<FilterOption> AuditorName { get; set; }

        public IReadOnlyCollection<FilterOption> AuditTime { get; set; }

        public DateFilterModel AuditDate { get; set; }

        public IReadOnlyCollection<FilterOption> FilteredDate { get; set; }

        public DateFilterModel CreatedAt { get; set; }

        public IReadOnlyCollection<FilterOption> Status { get; set; }

        public DateFilterModel SubmittedDate { get; set; }

        public AIAuditState State { get; set; }
    }
}
