using SHARP.BusinessLogic.DTO.Memo;
using SHARP.Common.Filtration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IMemoService
    {
        Task<IReadOnlyCollection<MemoDto>> GetMemosAsync(MemoFilter filter);

        Task<MemoDto> AddMemoAsync(AddMemoDto addMemoDto);

        Task<MemoDto> EditMemoAsync(EditMemoDto editMemoDto);

        Task<bool> DeleteMemoAsync(int id);
    }
}
