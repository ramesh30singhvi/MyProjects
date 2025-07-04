using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class HourKeywordAuditDetailsDto : AuditDto
    {
        public IReadOnlyCollection<KeywordOptionDto> Keywords { get; set; }

        public IReadOnlyCollection<KeywordDto> MatchedKeywords { get; set; }
    }
}
