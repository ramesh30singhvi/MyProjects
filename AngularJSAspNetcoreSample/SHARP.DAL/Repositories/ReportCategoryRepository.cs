using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public class ReportCategoryRepository : GenericRepository<ReportCategory>, IReportCategoryRepository
    {
        public ReportCategoryRepository(SHARPContext context) : base(context)
        {
        }
    }
}
