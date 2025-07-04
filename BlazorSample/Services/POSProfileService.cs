using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
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
    public class POSProfileService : IPOSProfileService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public POSProfileService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<AddEditPOSProfileResponse> AddUpdatePOSProfile(POSProfileRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<POSProfileRequestModel, AddEditPOSProfileResponse>(_configuration["App:SettingsApiUrl"] + "POSProfile/add-update", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddEditPOSProfileResponse();
            }
        }

        public async Task<GetPOSProfileDetailsResponse> GetPOSProfileDetails(int posProfileId, string posProfileGuid)
        {
            try
            {
                return await _apiManager.GetAsync<GetPOSProfileDetailsResponse>(_configuration["App:SettingsApiUrl"] + "POSProfile/details?posProfileId=" + posProfileId + "&posProfileGuid=" + posProfileGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetPOSProfileDetailsResponse();
            }
        }

        public async Task<GetPOSProfileListResponse> GetPOSProfileList(int businessId, bool activeOnly)
        {
            try
            {
                return await _apiManager.GetAsync<GetPOSProfileListResponse>(_configuration["App:SettingsApiUrl"] + "POSProfile/list/?businessId=" + businessId + "&activeOnly=" + activeOnly);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetPOSProfileListResponse();
            }
        }
        public async Task<BaseResponse> UpdatePOSProfileStatus(POSProfileStatusRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<POSProfileStatusRequestModel, BaseResponse>(_configuration["App:SettingsApiUrl"] + "POSProfile/update-profile-status", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<GetPOSProfileDataResponse> GetPOSProfileData(int posProfileId, string posProfileGuid)
        {
            try
            {
                GetPOSProfileDataResponse response = await _apiManager.GetAsync<GetPOSProfileDataResponse>(_configuration["App:SettingsApiUrl"] + "POSProfile/pos-profile-detail?posProfileId=" + posProfileId + "&posProfileGuid=" + posProfileGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetPOSProfileDataResponse();
            }
        }
        public async Task<GetPOSPaymentProfileListResponse> GetPOSPaymentProfileList(int businessId)
        {
            try
            {
                GetPOSPaymentProfileListResponse response = await _apiManager.GetAsync<GetPOSPaymentProfileListResponse>(_configuration["App:SettingsApiUrl"] + "POSProfile/get-pos-payment-profile-list/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetPOSPaymentProfileListResponse();
            }
        }
    }
}
