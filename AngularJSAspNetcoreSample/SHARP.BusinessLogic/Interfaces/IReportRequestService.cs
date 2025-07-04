using SHARP.BusinessLogic.DTO.ReportRequest;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IReportRequestService
    {
        Task<IReadOnlyCollection<ReportRequestDto>> GetReportRequestsAsync(ReportRequestFilter reportRequestFilter);

        Task<MessageResponse> AddReportRequestAsync(AddReportRequestDto addReportRequestDto);

        Task<MessageResponse>EditReportRequestAsync(EditReportRequestDto editReportRequestDto);

        Task<string> SaveToBlobAsync(byte[] file);

        Task<byte[]> GetReportAsync(string report);

        Task ResendReportRequestAsync(int id);

        Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(ReportRequestFilterColumnSource<ReportRequestFilterColumn> columnData);
    }
}
