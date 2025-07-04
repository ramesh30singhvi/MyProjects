using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.DTO;
using SHARP.Common.Filtration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SHARP.BusinessLogic.DTO.Portal;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IPortalReportService
    {
        Task<Tuple<IReadOnlyCollection<PortalReportDto>, int>> GetPortalReportsAsync(PortalReportFilter filter);
        Task<OptionDto[]> GetReportTypes();
        Task<Tuple<PortalReportDto, string>> UploadNewReportAsync(UploadNewReportDto reportUploadDto);
        Task<OptionDto[]> GetReportRanges();
        Task<OptionDto[]> GetReportCategories();
        Task<SendReportDto> AddToSending(string useremail, IList<int> portalReports, string tokenCode);
        Task<Tuple<IReadOnlyCollection<PortalReportDto>, int>> GetPortalReportsForFacilityAsync(PortalReportFacilityViewFilter filter);
        Task<byte[]> DownloadPortalReport(SelectedDto portalReportSelectedDto);

        Task<Tuple<byte[], string>> DownloadPortalReportForAttachment(int reportId);
        Task<Tuple<bool, string, int>> HasAccess(FacilityAccessDto facilityAccess);
        Task<bool> DeletePortalReport(int id);
        Task<Tuple<IReadOnlyCollection<PortalReportDto>, int>> GetPortalReportsByPageAsync(PortalReportFilter filter);
        Task<OptionDto[]> GetPortalFeatures();

        Task<Tuple<IReadOnlyCollection<DownloadsTrackingDto>, int>> GetPortalDownloadsTrackingAsync(PortalDownloadsTrackingViewFilter filter);

    }
}
