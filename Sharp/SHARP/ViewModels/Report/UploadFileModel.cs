using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SHARP.ViewModels.Report
{
    public class UploadFileModel
    {
        public IFormFile PdfFile { get; set; }
        public IFormFile KeywordFileJson { get; set; }

        public string Keyword { get; set; }
        public string BuidlWordIndex { get; set; }

        public string OrganizationId { get; set; }
        public string FacilityId { get; set; }

    }
}
