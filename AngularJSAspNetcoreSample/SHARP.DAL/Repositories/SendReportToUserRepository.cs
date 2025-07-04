using Microsoft.EntityFrameworkCore;
using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class SendReportToUserRepository : GenericRepository<SendReportToUser>, ISendReportToUserRepository
    {
        public SendReportToUserRepository(SHARPContext context) : base(context)
        {
        }


        public async Task<IReadOnlyCollection<int>> GetReportForUserAsync(int [] facilities)
        {
            var sendReports = _context.SendReportToUser.Where(x => facilities.Contains(x.FacilityId)).AsNoTracking();
            return await sendReports.Select(x =>x.PortalReportId).ToListAsync();
        }

        public async Task<Tuple<bool, string,int>> HasAccessFromEmail(int facilityId, string password)
        {
            var d = _context.SendReportToUser.Where(x => x.FacilityId == facilityId).ToList();
            var list = _context.SendReportToUser.Where(x => x.FacilityId == facilityId && password.CompareTo(x.Token) == 0);
            bool hasAcess =  list != null && list.Any();
            if(hasAcess)
            {
                var reportSend = list.FirstOrDefault();
                if(reportSend != null)
                {
                    var endDate = reportSend.CreatedAt.AddDays(1);
                    var nowDate = DateTime.UtcNow;

                    if ((endDate - nowDate).TotalDays <= 1)
                    {
                        return new Tuple<bool, string,int>(hasAcess, string.Empty,reportSend.FacilityId);
                    }
                }
            }
            return new Tuple<bool,string,int>(false, "The access is expired.",0);
        }
    }
}
