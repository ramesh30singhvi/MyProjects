using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessCustomerTypesService : IBusinessCustomerTypesService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public BusinessCustomerTypesService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<AddUpdateBusinessCustomerTypeResponse> AddUpdateBusinessCustomerType(AddUpdateBusinessCustomerTypeRequestModel model)
        {
            try
            {
                AddUpdateBusinessCustomerTypeResponse response = await _apiManager.PostAsync<AddUpdateBusinessCustomerTypeRequestModel, AddUpdateBusinessCustomerTypeResponse>(_configuration["App:SettingsApiUrl"] + $"BusinessCustomerTypes/add-update-business-customer-type", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateBusinessCustomerTypeResponse();
            }
        }

        public async Task<BaseResponse> DeleteBusinessCustomerType(int id)
        {
            try
            {
                BaseResponse response = await _apiManager.DeleteAsync<BaseResponse>(_configuration["App:SettingsApiUrl"] + $"BusinessCustomerTypes/delete-business-customer-type?id={id}");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BusinessCustomerTypesResponse> GetBusinessCustomerTypes(int businessId)
        {
            try
            {
                BusinessCustomerTypesResponse response = await _apiManager.GetAsync<BusinessCustomerTypesResponse>(_configuration["App:SettingsApiUrl"] + $"BusinessCustomerTypes/list?businessId={businessId}");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessCustomerTypesResponse();
            }
        }
    }
}
