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
    public class POSDeviceSettingService : IPOSDeviceSettingService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _settingsBaseUrl;

        public POSDeviceSettingService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _settingsBaseUrl = _configuration["App:SettingsApiUrl"];
        }
        public async Task<POSDeviceSettingViewModel> GetDeviceSettingDetails(int id)
        {
            try
            {
                var response = await _apiManager.GetAsync<POSDeviceDetailsResponse>(string.Format("{0}POSDeviceSetting/{1}", _settingsBaseUrl, id));
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return null;
            }
        }

        public async Task<RegisterDeviceResponse> RegisterDevice(RegisterDeviceRequestModel device)
        {
            try
            {
                var response = await _apiManager.PostAsync<RegisterDeviceRequestModel, RegisterDeviceResponse>(string.Format("{0}register-device", _settingsBaseUrl), device);
                return response;
            }
            catch(HttpRequestExceptionEx e)
            {
                Debug.Write(e.HttpCode);
                return null;
            }
        }

        public async Task<BaseResponse> UnregisterDevice(RegisterDeviceRequestModel device)
        {
            try
            {
                var response = await _apiManager.PostAsync<RegisterDeviceRequestModel, RegisterDeviceResponse>(string.Format("{0}unregister-device", _settingsBaseUrl), device);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.Write(e.HttpCode);
                return null;
            }
        }

        public async Task<POSDeviceDetailsResponse> SaveDeviceSettingDetails(POSDeviceSettingViewModel device)
        {
            try
            {
                var response = await _apiManager.PostAsync<POSDeviceSettingViewModel, POSDeviceDetailsResponse>(string.Format("{0}POSDeviceSetting", _settingsBaseUrl), device);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return null;
            }
        }
        public async Task<POSDeviceCodeResponse> GenerateDeviceCode(int codeLength)
        {
            var response = await _apiManager.GetAsync<POSDeviceCodeResponse>(string.Format("{0}generate-device-code", _settingsBaseUrl, codeLength));
            return response;
        }
    }
}
