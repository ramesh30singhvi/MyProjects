using SHARP.Common.Enums;

namespace SHARP.ViewModels.Audit
{
    public class AuditStatusRequestModel
    {
        public AuditStatus Status { get; set; }

        public string Comment { get; set; }

        public int? ReportType { get; set; }
    }
}
