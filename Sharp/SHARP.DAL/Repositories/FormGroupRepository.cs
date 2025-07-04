using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
	public class FormGroupRepository : GenericRepository<FormGroup>, IFormGroupRepository
	{
		public FormGroupRepository(SHARPContext context): base(context)
		{
		}

        public async Task<FormGroup[]> GetGroupsAsync(params int[] formSectionIds)
        {
            return await _context.FormGroup.Where(group => formSectionIds.Contains(group.FormSectionId)).ToArrayAsync();
        }

        public async Task<FormGroup> GetFormGroupAsync(int id)
        {
	        return await _context.FormGroup.Include(group => group.FormSection)
		        .FirstOrDefaultAsync(formGroup => formGroup.Id == id);
        }
    }
}

