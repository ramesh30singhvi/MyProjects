using SHARP.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace SHARP.ViewModels.Report
{
    public class ReportAIStatusRequestModel
    {
        [Required]
        public int ReportAIContentId { get; set; }
        public ReportAIStatus Status { get; set; }
    }
}
