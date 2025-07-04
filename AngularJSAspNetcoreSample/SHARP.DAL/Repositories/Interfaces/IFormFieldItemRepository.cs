using SHARP.DAL.Models;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IFormFieldItemRepository : IRepository<FormFieldItem>
    {
        Task<FormFieldItem[]> GetFormFieldItemsAsync(params int[] formFieldIds);
    }
}
