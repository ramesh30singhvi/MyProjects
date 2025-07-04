using Microsoft.AspNetCore.Http;
using SHARP.ViewModels.Facilitty;

namespace SHARP.ViewModels.Portal
{
    public class UploadNewReportModal
    {
        public string OrganizationId { get; set; }
        public string FacilityId { get; set; }
        public int ReportCategoryId { get; set; }

        public IFormFile FileUpload { get; set; }


    }
}
