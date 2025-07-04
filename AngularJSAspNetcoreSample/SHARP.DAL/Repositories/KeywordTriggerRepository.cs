using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    internal class KeywordTriggerRepository : GenericRepository<KeywordTrigger>, IKeywordTriggerRepository
    {
        public KeywordTriggerRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<KeywordTrigger[]> GetAsync(int keywordId, int formId)
        {
            return await _context.KeywordTrigger.Where(x => x.KeywordFormId == formId && x.KeywordId == keywordId).ToArrayAsync();
        }
    }
}
