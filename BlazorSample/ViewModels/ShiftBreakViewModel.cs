using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ShiftBreakViewModel : IShiftBreakViewModel
    {
        private IShiftBreakService _shiftBreakService;

        public ShiftBreakViewModel(IShiftBreakService shiftBreakService)
        {
            _shiftBreakService = shiftBreakService;
        }
        public async Task<ShiftBreakListResponse> GetShiftBreakListAsync(int businessId)
        {
            return await _shiftBreakService.GetShiftBreakListAsync(businessId);
        }
        public async Task<ShiftBreakListResponse> AddUpdateShiftBreakListAsync(List<ShiftBreakRequestModel> models)
        {
            return await _shiftBreakService.AddUpdateShiftBreakListAsync(models);
        }
    }
}
