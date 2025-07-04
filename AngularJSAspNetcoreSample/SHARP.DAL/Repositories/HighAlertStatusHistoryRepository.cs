using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public class HighAlertStatusHistoryRepository : GenericRepository<HighAlertStatusHistory>, IHighAlertStatusHistoryRepository
    {
        public HighAlertStatusHistoryRepository(SHARPContext context) : base(context)
        {
        }
    }
}
