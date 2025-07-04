using SHARP.BusinessLogic.DTO.Form;
using SHARP.Common.Filtration;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class MdsAuditDetailsDto: AuditDto
    {
        public MdsFormDetailsDto FormVersion { get; set; }

        public IReadOnlyCollection<FormFieldValueDto> SubHeaderValues { get; set; }

    }
}
