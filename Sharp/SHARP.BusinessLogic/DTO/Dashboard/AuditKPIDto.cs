using SHARP.Common.Enums;

namespace SHARP.BusinessLogic.DTO.Dashboard
{
    public class AuditKPIDto
    {
        public OptionDto Organization { get; set; }

        public AuditStatus AuditStatus { get; set; }

        public int AuditCount { get; set; }
    }
}
