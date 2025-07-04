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
    public class POSLicenceService : IPOSLicenceService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _settingsBaseUrl;

        public POSLicenceService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _settingsBaseUrl = _configuration["App:SettingsApiUrl"];
        }
        public async Task<POSLicenceAddResponse> ActivatePosLicenceKey(int id, POSLicenceActivateRequest pOSLicenceActivateRequest)
        {
            try
            {
                var response = await _apiManager.PutAsync<POSLicenceActivateRequest, POSLicenceAddResponse>(string.Format("{0}POSLicence/activate-pos-license-key/{1}", _settingsBaseUrl, id), pOSLicenceActivateRequest);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new POSLicenceAddResponse();
            }
        }

        public async Task<POSLicenceAddResponse> AddPosLicenceKey(POSLicenceAddRequest pOSLicenceViewModel)
        {
            try
            {
                var response = await _apiManager.PostAsync<POSLicenceAddRequest, POSLicenceAddResponse>(string.Format("{0}POSLicence/create-pos-license-key",_settingsBaseUrl), pOSLicenceViewModel);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new POSLicenceAddResponse();
            }
        }

        public async Task<POSLicenceAddResponse> UpdatePosLicenceKey(int id, POSLicenceAddRequest pOSLicenceViewModel)
        {
            try
            {
                var response = await _apiManager.PutAsync<POSLicenceAddRequest, POSLicenceAddResponse>(string.Format("{0}POSLicence/update-pos-license-key/{1}",_settingsBaseUrl, id), pOSLicenceViewModel);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new POSLicenceAddResponse();
            }
        }

        public Task<POSLicenceViewModel> VerifyPosLicenceKey
            (POSLicenceVerifyRequest pOSLicenceViewModel) => throw new NotImplementedException();

        public Task<GridPaginatedResponseModel<POSLicenceViewModel>> GetPosLicenseKeys
        (GridPaginatedRequestViewModel gridPaginatedRequestViewModel)  => throw new NotImplementedException();

        public async Task<POSLicenceResponse> GetPosLicenseKeys(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<POSLicenceResponse>(_configuration["App:SettingsApiUrl"] + "POSLicence/get-pos-license-keys/" + businessId );
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new POSLicenceResponse();
            }
        }

        public async Task<POSLicenceAddResponse> ResetPosLicenceKey(POSLicenceResetRequest model)
        {
            try
            {
                var response = await _apiManager.PutAsync<POSLicenceResetRequest, POSLicenceAddResponse>(string.Format("{0}POSLicence/reset-pos-license-key", _settingsBaseUrl), model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new POSLicenceAddResponse();
            }
        }
    }
}
