using SHARP.Common.Filtration;
using SHARP.ViewModels.Form;
using System.Collections.Generic;

namespace SHARP.ViewModels.Audit
{
    public class TrackerAuditDetailsModel : AuditDetailsModel
    {
        public TrackerFormDetailsModel FormVersion { get; set; }

        public IReadOnlyCollection<AuditTrackerAnswerGroupModel> AnswerGroups { get; set; }

        public IReadOnlyCollection<dynamic> PivotAnswerGroups { get; set; }

        public SortModel SortModel { get; set; }
    }
}
