using SHARP.BusinessLogic.DTO.Form;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class CriteriaAuditDetailsDto: AuditDto
    {
        public CriteriaFormDetailsDto FormVersion { get; set; }

        public IReadOnlyCollection<CriteriaAnswerDto> Answers { get; set; }

        public IReadOnlyCollection<FormFieldValueDto> SubHeaderValues { get; set; }
    }
}
