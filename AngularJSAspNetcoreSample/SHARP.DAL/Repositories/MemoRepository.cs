using Microsoft.EntityFrameworkCore;
using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class MemoRepository : GenericRepository<Memo>, IMemoRepository
    {
        public MemoRepository(SHARPContext context) : base(context)
        {
            
        }

        public async Task<IReadOnlyCollection<Memo>> GetMemosAsync(MemoFilter filter)
        {
            DateTime minDate = DateTime.UtcNow.AddHours(-12).Date;

            DateTime filterDate = filter.CurrentDate < minDate ? minDate : filter.CurrentDate;

            var memos = _context.Memos
                .Include(memo => memo.User)
                .Include(memo => memo.OrganizationMemos)
                    .ThenInclude(organizationMemo => organizationMemo.Organization)
                .Where(memo => !memo.ValidityDate.HasValue || memo.ValidityDate >= filterDate)
                .AsQueryable();

            if (filter.OrganizationIds.Any())
            {
                memos = memos.Where(memo => 
                memo.OrganizationMemos.Any(memoOrganization => 
                filter.OrganizationIds.Contains(memoOrganization.OrganizationId)) || !memo.OrganizationMemos.Any());
            }

            return await memos
                .OrderByDescending(memo => memo.CreatedDate)
                .ToArrayAsync();
        }

        public async Task<Memo> GetMemoDetailsAsync(int id)
        {
            return await _context.Memos
                .Include(memo => memo.User)
                .Include(memo => memo.OrganizationMemos)
                    .ThenInclude(organizationMemo => organizationMemo.Organization)
                .FirstOrDefaultAsync(memo => memo.Id == id);
        }
    }
}