using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
	public class FormSectionRepository : GenericRepository<FormSection>, IFormSectionRepository
	{
		public FormSectionRepository(SHARPContext context): base(context)
		{

		}

        public async Task<FormSection[]> GetSectionsAsync(params int[] formVersionIds)
        {
            return await _context.FormSection
                .Where(formSection => formVersionIds.Contains(formSection.FormVersionId))
                .Include(formSection => formSection.Groups)
                    .ThenInclude(formGroup => formGroup.FormFields)
                        .ThenInclude(formField => formField.FieldType)
                .Include(formSection => formSection.Groups)
                    .ThenInclude(formGroup => formGroup.FormFields)
                    .ThenInclude(formField => formField.Items)
                .ToArrayAsync();
        }
    }
}

