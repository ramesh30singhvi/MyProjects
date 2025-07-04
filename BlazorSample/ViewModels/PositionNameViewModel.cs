using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class PositionNameViewModel : IPositionNameViewModel
    {
        private IPositionNameService _positionNameService;

        public PositionNameViewModel(IPositionNameService positionNameService)
        {
            _positionNameService = positionNameService;
        }
        public async Task<CPPositionNameListResponse> GetPositionNameAsync(int? id)
        {
            return await _positionNameService.GetPositionNameAsync(id);
        }
    }
}
