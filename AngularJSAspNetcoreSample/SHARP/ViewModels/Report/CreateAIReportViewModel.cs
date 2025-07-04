using Microsoft.AspNetCore.Http;
using SHARP.ViewModels.Facilitty;

namespace SHARP.ViewModels.Report
{
    public class CreateAIReportViewModel
    {
        public IFormFile PdfFile { get; set; }
        
        public string User { get; set; }

        public string OrganizationId { get; set; }
        public string FacilityId { get; set; }
    }
}
