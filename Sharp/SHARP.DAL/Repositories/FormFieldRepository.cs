using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories
{
    public class FormFieldRepository : GenericRepository<FormField>, IFormFieldRepository
    {
        public FormFieldRepository(SHARPContext context) : base(context)
        {
        }

        public async Task<FormField[]> GetFormFieldsAsync(params int[] formVersionIds)
        {
            return await _context.FormField
                .Include(formField => formField.FieldType)
                .Include(formField => formField.Items)
                .Where(formField => formVersionIds.Contains(formField.FormVersionId))
                .ToArrayAsync();
        }

        public async Task<FormField> GetFormFieldAsync(int id)
        {
            return await _context.FormField
                .Include(form => form.FormVersion)
                .Include(form => form.Items)
                .Where(formField => formField.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}