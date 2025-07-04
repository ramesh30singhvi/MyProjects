using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Portal;
using SHARP.Common.Filtration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IHighAlertService
    {
        Task<byte[]> DownloadReportForHighAlert(int id);
        Task<OptionDto[]> GetHighAlertCategories();
        Task<OptionDto[]> GetHighAlertPotentialAreas();

        Task<HighAlertCategoryPotentialAreaDto[]> GetHighAlertCategoriesWithPotentialArea();
   //     Task<OptionDto[]> GetHighAlertPotentialAreas(int categoryId);
        Task<Tuple<IReadOnlyCollection<HighAlertValueDto>, int>> GetHighAlertsAsync(HighAlertPortalFilter filter);
        Task<HighAlertStatisticDto> GetHighAlertStatistics(OptionDto facilityDto);
        Task<OptionDto[]> GetHighAlertStatuses();
        Task<HighAlertValueDto> SetHighAlertStatus(int highAlertId, OptionDto statusDto, string userNotes,string changeBy);
        Task<byte[]> DownloadReportForHighAlertAsExcel(int id);
    }
}
