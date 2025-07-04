using SHARP.DAL.Models;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IFormFieldRepository : IRepository<FormField>
    {
        Task<FormField[]> GetFormFieldsAsync(params int[] formVersionIds);
        Task<FormField> GetFormFieldAsync(int id);
    }
}
