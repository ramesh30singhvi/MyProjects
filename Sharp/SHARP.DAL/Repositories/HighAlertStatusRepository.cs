using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public class HighAlertStatusRepository : GenericRepository<HighAlertStatus>, IHighAlertStatusRepository
    {
        public HighAlertStatusRepository(SHARPContext context) : base(context)
        {
        }
    }
}
