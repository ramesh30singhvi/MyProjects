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
    public class ShippingCarrierService : IShippingCarrierService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public ShippingCarrierService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<GetShippingCarriersResponse> GetAllShippingCarriers()
        {
            try
            {
                var result = await _apiManager.GetAsync<GetShippingCarriersResponse>(_configuration["App:SettingsApiUrl"] + "ShippingCarrier" );
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetShippingCarriersResponse();
            }
        }

        public async Task<GetShippingCarrierServiceCodesResponse> GetAllShippingCarrierServiceCodes()
        {
            try
            {
                var result = await _apiManager.GetAsync<GetShippingCarrierServiceCodesResponse>(_configuration["App:SettingsApiUrl"] + "ShippingCarrier/get-all-shipping-carrier-service-codes");
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetShippingCarrierServiceCodesResponse();
            }
        }

        public async Task<GetShippingCarrierServiceCodesResponse> GetShippingCarrierServiceCodesByCarrierId(int carrierId)
        {
            try
            {
                var result = await _apiManager.GetAsync<GetShippingCarrierServiceCodesResponse>(_configuration["App:SettingsApiUrl"] + "ShippingCarrier/get-shipping-carrier-service-codes/"+carrierId);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetShippingCarrierServiceCodesResponse();
            }
        }
    }
}
