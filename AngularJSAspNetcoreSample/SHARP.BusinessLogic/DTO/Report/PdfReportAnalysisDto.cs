using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public  class PdfReportAnalysisDto
    {
        public int NumberKeywordsFound { get; set; }

        public string Error { get; set; }
        public string Date { get; set; }

        public string Time { get; set; }

        public string BuildIndexJson { get; set; }

        public string Keywords { get; set; }

    }
}
