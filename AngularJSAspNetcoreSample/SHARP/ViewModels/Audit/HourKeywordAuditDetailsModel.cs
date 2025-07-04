using System.Collections.Generic;

namespace SHARP.ViewModels.Audit
{
    public class HourKeywordAuditDetailsModel: AuditDetailsModel
    {
        public IReadOnlyCollection<KeywordOptionModel> Keywords { get; set; }

        public IReadOnlyCollection<KeywordModel> MatchedKeywords { get; set; }
    }
}
