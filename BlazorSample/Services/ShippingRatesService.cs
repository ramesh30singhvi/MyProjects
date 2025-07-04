using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static CellarPassAppAdmin.Shared.Models.ViewModel.ShippingRateViewModel;

namespace CellarPassAppAdmin.Client.Services
{
    public class ShippingRatesService : IShippingRatesService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public ShippingRatesService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<AddUpdateShippingChargeResponse> AddUpdateShippingCharge(ShippingChargeRequestModel model)
        {
            try
            {
                var result = await _apiManager.PostAsync<ShippingChargeRequestModel, AddUpdateShippingChargeResponse>(_configuration["App:SettingsApiUrl"] + "ShippingRates/add-update-shipping-charge", model);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateShippingChargeResponse();
            }
        }

        public async Task<ShippingRateResponse> AddUpdateShippingRates(ShippingRateRequestModel model)
        {
            try
            {
                var result = await _apiManager.PostAsync<ShippingRateRequestModel, ShippingRateResponse>(_configuration["App:SettingsApiUrl"] + "ShippingRates/add-update-shipping-rate", model);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ShippingRateResponse();
            }
        }

        public async Task<BaseResponse> AdjustRatesForMultipleShippingRates(AdjustRatesRequestModel model)
        {
            try
            {
                var result = await _apiManager.PostAsync<AdjustRatesRequestModel, BaseResponse>(_configuration["App:SettingsApiUrl"] + "ShippingRates/adjust-shipping-rates", model);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> DeleteShippingCharge(int Id)
        {
            try
            {
                var result = await _apiManager.DeleteAsync<BaseResponse>(_configuration["App:SettingsApiUrl"] + "ShippingRates/delete-shipping-charge/" + Id);
                return result;

            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> DeleteShippingRate(int Id)
        {
            try
            {
                var result = await _apiManager.DeleteAsync<BaseResponse>(_configuration["App:SettingsApiUrl"] + "ShippingRates/delete-shipping-rate/" + Id);
                return result;

            }catch(HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<ShippingRateResponse> GetShippingRate(int Id, Guid IdGUID)
        {
            try
            {
                var result = await _apiManager.GetAsync<ShippingRateResponse>($"{_configuration["App:SettingsApiUrl"]}ShippingRates/get-shipping-rate/{Id}/{IdGUID}");
                return result;
            }
            catch(HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ShippingRateResponse();
            }
        }

        public async Task<ShippingRatesResponse> GetShippingRates(int businessId)
        {
            try
            {
                var result = await _apiManager.GetAsync<ShippingRatesResponse>(_configuration["App:SettingsApiUrl"] + "ShippingRates/"+businessId);
                return result;
            }
            catch(HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ShippingRatesResponse();
            }
        }
    }
}
