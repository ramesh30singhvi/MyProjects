using EFCore.BulkExtensions;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public  class AuditAIKeywordSummaryRepository : GenericRepository<AuditAIKeywordSummary>, IAuditAIKeywordSummaryRepository
    {
        public AuditAIKeywordSummaryRepository(SHARPContext context) : base(context)
        {
        }
    }
}
