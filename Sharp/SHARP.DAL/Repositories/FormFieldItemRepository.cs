using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class FormFieldItemRepository : GenericRepository<FormFieldItem>, IFormFieldItemRepository
    {
        public FormFieldItemRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<FormFieldItem[]> GetFormFieldItemsAsync(params int[] formFieldIds)
        {
            return await _context.FormFieldItems
                .Where(formField => formFieldIds.Contains(formField.FormFieldId))
                .ToArrayAsync();
        }
    }
}