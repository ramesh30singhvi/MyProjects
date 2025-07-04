using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SHARP.Common.Enums;
using SHARP.Common.Extensions;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Extensions;
using SHARP.DAL.Helpers;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class AuditStatusHistoryRepository : GenericRepository<AuditStatusHistory>, IAuditStatusHistoryRepository
    {
        public AuditStatusHistoryRepository(SHARPContext context) : base(context)
        {
        }
    }
}