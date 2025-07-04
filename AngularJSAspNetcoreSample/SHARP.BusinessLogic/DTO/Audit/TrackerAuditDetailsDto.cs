using SHARP.BusinessLogic.DTO.Form;
using SHARP.Common.Filtration;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class TrackerAuditDetailsDto: AuditDto
    {
        public TrackerFormDetailsDto FormVersion { get; set; }

        public IReadOnlyCollection<dynamic> PivotAnswerGroups { get; set; }

        public SortModel SortModel { get; set; }
    }
}
