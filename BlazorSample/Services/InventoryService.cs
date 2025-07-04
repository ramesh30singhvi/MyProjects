using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
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
    public class InventoryService : IInventoryService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public InventoryService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<BaseResponse> AddUpdateBusinessInventorySettings(BusinessInventoryAddUpdateRequestModel model)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<BaseResponse>(_configuration["App:ProductApiUrl"] + $"Inventory/add-update-business-inventory-settings", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<GetBusinessReturnReasonsResponse> GetBusinessReturnReasons(int businessId)
        {
            try
            {
                GetBusinessReturnReasonsResponse response = await _apiManager.GetAsync<GetBusinessReturnReasonsResponse>(_configuration["App:ProductApiUrl"] + $"Inventory/get-business-return-reasons?businessId={businessId}");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetBusinessReturnReasonsResponse();
            }
        }

        public async Task<InventoryDetailsResponse> GetInventoryDetails(int businessId)
        {
            try
            {
                InventoryDetailsResponse response = await _apiManager.GetAsync<InventoryDetailsResponse>(_configuration["App:ProductApiUrl"] + $"Inventory/inventory-details?businessId={businessId}");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new InventoryDetailsResponse();
            }
        }

        public async Task<InventoryLocationsResponse> GetInventoryLocations(int businessId)
        {
            try
            {
                InventoryLocationsResponse response = await _apiManager.GetAsync<InventoryLocationsResponse>(_configuration["App:ProductApiUrl"] + $"Inventory/inventory-locations?businessId={businessId}");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new InventoryLocationsResponse();
            }
        }

        public async Task<BaseResponse> UpdateInventory(List<InventoryUpdateRequestModel> model)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<BaseResponse>(_configuration["App:ProductApiUrl"] + $"Inventory/update-inventory", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<GetSalesChannelsForInventoryLocationResponse> GetSalesChannelsForInventoryLocation(Guid locationGUID)
        {
            try
            {
                GetSalesChannelsForInventoryLocationResponse response = await _apiManager.GetAsync<GetSalesChannelsForInventoryLocationResponse>(_configuration["App:ProductApiUrl"] + $"Inventory/get-saleschannels-by-inventory-location?locationGUID={locationGUID}");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetSalesChannelsForInventoryLocationResponse();
            }
        }
    }
}
