using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class ReportTypeRepository : GenericRepository<ReportType>, IReportTypeRepository
    {
        public ReportTypeRepository(SHARPContext context) : base(context)
        {
        }

    }
}
