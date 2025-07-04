using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IPortalReportRepository : IRepository<PortalReport>
    {
        Task<PortalReport> GetPortalReportAsync(int id);
        Task<Tuple<PortalReport[],int>> GetPortalReportsAsync(PortalReportFilter filter, Expression<Func<PortalReport, object>> orderBySelector);
        Task<Tuple<IReadOnlyCollection<PortalReport>, int>> GetPortalReportsAsync(IReadOnlyCollection<int> selectedReports);
        Task<Tuple<PortalReport[], int>> GetPortalReportsAsyncByPage(PortalReportFilter filter, Expression<Func<PortalReport, object>> orderBySelector);
    }
}
