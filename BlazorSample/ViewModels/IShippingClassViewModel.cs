using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IShippingClassViewModel
    {
        Task<ShippingClassResponse> AddUpdateShippingClassAsync(ShippingClassRequestModel model);
        Task<ShippingClassListResponse> GetShippingClasses(int businessId);
        Task<GridPaginatedResponseModel<ShippingClassModel>> GetShippingClasses(GridPaginatedRequestViewModel gridPaginatedRequestViewModel);
        Task<BaseResponse> DeleteShippingClass(int Id);
    }
}
