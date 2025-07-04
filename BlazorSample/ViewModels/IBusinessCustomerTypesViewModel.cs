using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessCustomerTypesViewModel
    {
        Task<AddUpdateBusinessCustomerTypeResponse> AddUpdateBusinessCustomerType(AddUpdateBusinessCustomerTypeRequestModel model);
        Task<BusinessCustomerTypesResponse> GetBusinessCustomerTypes(int businessId);
        Task<BaseResponse> DeleteBusinessCustomerType(int id);
    }
}
