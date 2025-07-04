using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public class ReportRangeRepository : GenericRepository<ReportRange>, IReportRangeRepository
    {
        public ReportRangeRepository(SHARPContext context) : base(context)
        {
        }
    }
}
