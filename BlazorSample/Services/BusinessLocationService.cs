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
    public class BusinessLocationService : IBusinessLocationService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public BusinessLocationService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<BusinessLocationResponse> GetBusinessLocations(int businessId)
        {
            try
            {
                BusinessLocationResponse response = await _apiManager.GetAsync<BusinessLocationResponse>(_configuration["App:SettingsApiUrl"] + "BusinessLocation/list/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessLocationResponse();
            }
        }

        public async Task<BusinessLocationDetailResponse> GetBusinessLocationDetail(Guid locationGUID)
        {
            try
            {
                BusinessLocationDetailResponse response = await _apiManager.GetAsync<BusinessLocationDetailResponse>(_configuration["App:SettingsApiUrl"] + "BusinessLocation/detail/" + locationGUID);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessLocationDetailResponse();
            }
        }

        public async Task<AddUpdateBusinessLocationResponse> AddUpdateBusinessLocation(BusinessLocationRequestModel request)
        {
            try
            {
                AddUpdateBusinessLocationResponse response = await _apiManager.PostAsync<BusinessLocationRequestModel, AddUpdateBusinessLocationResponse>(_configuration["App:SettingsApiUrl"] + "BusinessLocation/add-update-location", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateBusinessLocationResponse();
            }
        }
    }
}
