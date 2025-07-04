using SHARP.BusinessLogic.DTO.Facility;
using SHARP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class AzureReportProcessResultDto
    {
        public string Error { get; set; }
        public string JsonResult { get; set; }
        public OptionDto Organization { get; set; }
        public OptionDto Facility { get; set; }
        public string User { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string Keywords { get; set; }
        public string ReportFileName {get;set;}
        public string ContainerName { get; set;}

        public ReportAIStatus Status { get; set; }
        public string SearchWord { get; set; }

    }
}
