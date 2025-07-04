using SHARP.Common.Enums;
using SHARP.ViewModels.Facilitty;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class AIResultViewModel
    {
        public string Error { get; set; }
        public string JsonResult { get; set; }

        public OptionModel Organization { get; set; }
        public FacilityOptionModel Facility { get; set; }
        public string User { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Keywords { get; set; }
        public string ReportFileName { get; set; }
        public string ContainerName { get; set; }

        public ReportAIStatus Status { get; set; }
        public int? ReportAIContentId { get; set; }

    }
}
