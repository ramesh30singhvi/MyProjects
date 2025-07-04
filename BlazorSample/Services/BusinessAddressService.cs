using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessAddressService : IBusinessAddressService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessAddressService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<List<BusinessAddressViewModel>> GetBusinessAddresses(int businessId)
        {
            try
            {
                var response = await _apiManager.GetAsync<BusinessAddressResponse>(_configuration["App:SettingsApiUrl"] + "BusinessAddress/get-business-addressess/"+ businessId);
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<BusinessAddressViewModel>();
            }
        }
        public async Task<GetBusinessAddressResponse> GetBusinessAddressByIdGuid(Guid AddressGuid)
        {
            try
            {
                var response = await _apiManager.GetAsync<GetBusinessAddressResponse>(_configuration["App:SettingsApiUrl"] + "BusinessAddress/details/" + AddressGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetBusinessAddressResponse();
            }
        }
    }
}
