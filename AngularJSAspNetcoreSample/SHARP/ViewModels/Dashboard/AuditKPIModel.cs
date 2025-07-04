using SHARP.Common.Enums;

namespace SHARP.ViewModels.Dashboard
{
    public class AuditKPIModel
    {
        public OptionModel Organization { get; set; }

        public AuditStatus AuditStatus { get; set; }

        public int AuditCount { get; set; }
    }
}
