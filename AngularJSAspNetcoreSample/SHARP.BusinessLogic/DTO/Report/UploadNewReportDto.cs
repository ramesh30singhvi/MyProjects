using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public  class UploadNewReportDto
    {
        public int OrganizationId { get; set; }
        public int FacilityId { get; set; }
 

        public int ReportCategoryId { get; set; }
        public IFormFile FileUpload { get; set; }

 

    }
}
