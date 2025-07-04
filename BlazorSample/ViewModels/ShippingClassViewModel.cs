using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ShippingClassViewModel : IShippingClassViewModel
    {
        private IShippingClassService _shippingClassService;
        public ShippingClassViewModel(IShippingClassService shippingClassService)
        {
            _shippingClassService = shippingClassService;
        }
        public async Task<ShippingClassResponse> AddUpdateShippingClassAsync(ShippingClassRequestModel model)
        {
            return await _shippingClassService.AddUpdateShippingClassAsync(model);
        }
        public async Task<BaseResponse> DeleteShippingClass(int Id)
        {
            return await _shippingClassService.DeleteShippingClass(Id);
        }
        public async Task<GridPaginatedResponseModel<ShippingClassModel>> GetShippingClasses(GridPaginatedRequestViewModel gridPaginatedRequestViewModel)
        {
            return await _shippingClassService.GetShippingClasses(gridPaginatedRequestViewModel);
        }
        public async Task<ShippingClassListResponse> GetShippingClasses(int businessId)
        {
            return await _shippingClassService.GetShippingClasses(businessId);
        }
    }
}
