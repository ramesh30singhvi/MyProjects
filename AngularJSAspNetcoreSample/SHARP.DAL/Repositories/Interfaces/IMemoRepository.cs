using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IMemoRepository : IRepository<Memo>
    {
        Task<IReadOnlyCollection<Memo>> GetMemosAsync(MemoFilter filter);
        Task<Memo> GetMemoDetailsAsync(int id);
    }
}
