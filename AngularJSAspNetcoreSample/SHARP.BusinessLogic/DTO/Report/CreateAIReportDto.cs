using Microsoft.AspNetCore.Http;
using SHARP.BusinessLogic.DTO.Facility;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class CreateAIReportDto
    {
        public IFormFile PdfFile { get; set; }

        public string User { get; set; }

        public string OrganizationId { get; set; }
        public string FacilityId { get; set; }
    }
}
