using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessCustomerTypesViewModel : IBusinessCustomerTypesViewModel
    {
        private readonly IBusinessCustomerTypesService _businessCustomerTypesService;
        public BusinessCustomerTypesViewModel(IBusinessCustomerTypesService businessCustomerTypesService)
        {
            _businessCustomerTypesService = businessCustomerTypesService;
        }

        public async Task<AddUpdateBusinessCustomerTypeResponse> AddUpdateBusinessCustomerType(AddUpdateBusinessCustomerTypeRequestModel model)
        {
            return await _businessCustomerTypesService.AddUpdateBusinessCustomerType(model);
        }

        public async Task<BaseResponse> DeleteBusinessCustomerType(int id)
        {
            return await _businessCustomerTypesService.DeleteBusinessCustomerType(id);
        }

        public async Task<BusinessCustomerTypesResponse> GetBusinessCustomerTypes(int businessId)
        {
            return await _businessCustomerTypesService.GetBusinessCustomerTypes(businessId);
        }
    }
}
