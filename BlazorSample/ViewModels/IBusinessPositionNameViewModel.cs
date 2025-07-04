using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessPositionNameViewModel
    {
        Task<BusinessPositionNameListResponse> GetBusinessPositionNameListAsync(int businessId);
        Task<BusinessPositionNameResponse> AddUpdateBusinessPositionNameListAsync(BusinessPositionNameRequestModel model);
    }
}
