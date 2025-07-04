using System.Collections.Generic;

namespace SHARP.ViewModels.Audit
{
    public class AuditTrackerAnswerGroupModel
    {
        public string GroupId { get; set; }

        public IReadOnlyCollection<AuditTrackerAnswerModel> Answers { get; set; }
    }
}
