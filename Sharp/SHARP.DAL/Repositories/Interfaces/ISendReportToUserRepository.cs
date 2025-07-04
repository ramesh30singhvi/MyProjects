using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface ISendReportToUserRepository : IRepository<SendReportToUser>
    {
   
        Task<IReadOnlyCollection<int>> GetReportForUserAsync(int[] facilities);
        Task<Tuple<bool, string,int>> HasAccessFromEmail(int facilityId, string password);
    }
}
