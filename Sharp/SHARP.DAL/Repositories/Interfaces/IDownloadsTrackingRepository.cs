using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IDownloadsTrackingRepository : IRepository<DownloadsTracking>
    {
        Task<DownloadsTracking> GetPortalDownloadsTrackingByUserIdPortalReportIdAsync(int userId, int portalReportId);
        Task<Tuple<DownloadsTracking[], int>> GetPortalDownloadsTrackingAsync(PortalDownloadsTrackingViewFilter filter, Expression<Func<DownloadsTracking, object>> orderBySelector);
    }
}
