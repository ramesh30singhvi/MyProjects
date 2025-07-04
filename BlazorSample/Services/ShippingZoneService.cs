using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
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
    public class ShippingZoneService : IShippingZoneService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public ShippingZoneService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<AddUpdateShippingZoneResponse> AddUpdateShippingZone(ShippingZoneRequestModel model)
        {
            try
            {
                var result = await _apiManager.PostAsync<ShippingZoneRequestModel, AddUpdateShippingZoneResponse>($"{_configuration["App:SettingsApiUrl"]}ShippingZone", model);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateShippingZoneResponse();
            }
        }

        public async Task<GetShippingZoneListResponse> DeleteShippingZone(int Id, int businessId)
        {
            try
            {
                var result = await _apiManager.DeleteAsync<GetShippingZoneListResponse>($"{_configuration["App:SettingsApiUrl"]}ShippingZone/delete-shipping-zone/{Id}/{businessId}");
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetShippingZoneListResponse();
            }
        }

        public async Task<GetShippingZoneResponse> GetShippingZoneByIdGUID(Guid guid)
        {
            try
            {
                var result = await _apiManager.GetAsync<GetShippingZoneResponse>(_configuration["App:SettingsApiUrl"] + "ShippingZone/get-shipping-zone/" + guid);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetShippingZoneResponse();
            }
        }

        public async Task<GetShippingZoneListResponse> GetShippingZones(int businessId)
        {
            try
            {
                var result = await _apiManager.GetAsync<GetShippingZoneListResponse>(_configuration["App:SettingsApiUrl"] + "ShippingZone/get-all-shipping-zones/" + businessId);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetShippingZoneListResponse();
            }
        }
    }
}
