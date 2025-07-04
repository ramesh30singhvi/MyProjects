using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface ILoginsTrackingRepository : IRepository<LoginsTracking>
    {
        Task<LoginsTracking> GetLatestLoginsTrackingByUserId(int userId);
        Task<Tuple<LoginsTracking[], int>> GetPortalLoginsTrackingAsync(PortalLoginsTrackingViewFilter filter, Expression<Func<LoginsTracking, object>> orderBySelector);
    }
}
