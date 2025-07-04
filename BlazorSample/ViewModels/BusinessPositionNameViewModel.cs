using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessPositionNameViewModel : IBusinessPositionNameViewModel
    {
        private IBusinessPositionNameService _businessPositionNameService;

        public BusinessPositionNameViewModel(IBusinessPositionNameService businessPositionNameService)
        {
            _businessPositionNameService = businessPositionNameService;
        }
        public async Task<BusinessPositionNameListResponse> GetBusinessPositionNameListAsync(int businessId)
        {
            return await _businessPositionNameService.GetBusinessPositionNameListAsync(businessId);
        }
        public async Task<BusinessPositionNameResponse> AddUpdateBusinessPositionNameListAsync(BusinessPositionNameRequestModel model)
        {
            return await _businessPositionNameService.AddUpdateBusinessPositionNameListAsync(model);
        }
    }
}
