using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IShiftBreakViewModel
    {
        Task<ShiftBreakListResponse> GetShiftBreakListAsync(int businessId);
        Task<ShiftBreakListResponse> AddUpdateShiftBreakListAsync(List<ShiftBreakRequestModel> models);
    }
}
