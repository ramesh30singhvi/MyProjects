using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class PdfReportUploadAzureDto
    {
        public IFormFile PdfFile { get; set; }
        public IFormFile KeywordFileJson { get; set; }
        public string Keyword { get; set; }
        public string BuidlWordIndex { get;set; }

        public string OrganizationId { get; set; }
        public string FacilityId { get; set; }

    }
}
