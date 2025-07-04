using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Entities.v4;
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
    public class BusinessPageService : IBusinessPageService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _contentBaseUrl;

        public BusinessPageService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _contentBaseUrl = _configuration["App:ContentApiUrl"];
        }
        public async Task<List<CP_Attribute>> GetAttributeMastersAsync()
        {
            try
            {
                var response = await _apiManager.GetAsync<AttributeResponse>(string.Format("{0}BusinessPage/Attributes", _contentBaseUrl));
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<CP_Attribute>();
            }
        }

        public async Task<BusinessPageViewModel> GetBusinessContent(int businessId, int businessProperty)
        {
            try
            {
                var response = await _apiManager.GetAsync<BusinessPageResponse>(string.Format("{0}BusinessPage/{1}/{2}", _contentBaseUrl, businessId, businessProperty));
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return null;
            }
        }

        public async Task<BusinessPageViewModel> SaveBusinessContent(BusinessPageViewModel businessPageViewModel)
        {
            try
            {
                var response = await _apiManager.PostAsync<BusinessPageViewModel, BusinessPageResponse>(string.Format("{0}BusinessPage", _contentBaseUrl), businessPageViewModel);
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return null;
            }
        }

        public async Task<BusinessPageViewModel> UpdateBusinessContent(BusinessPageViewModel businessPageViewModel)
        {
            try
            {
                var response = await _apiManager.PutAsync<BusinessPageViewModel, BusinessPageResponse>(string.Format("{0}BusinessPage", _contentBaseUrl), businessPageViewModel);
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return null;
            }
        }

        public async Task<UploadImageResponse> UpdateBusinessBannerImage(ImageUploadRequestModel model)
        {
            try
            {
                var response = await _apiManager.PostAsync<ImageUploadRequestModel, UploadImageResponse>(string.Format("{0}BusinessPage/upload-brand-image", _contentBaseUrl), model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return null;
            }
        }

        public async Task<UploadImageResponse> UpdateBusinessImage(ImageUploadRequestModel model)
        {
            try
            {
                var response = await _apiManager.PostAsync<ImageUploadRequestModel, UploadImageResponse>(string.Format("{0}BusinessPage/upload-business-image", _contentBaseUrl), model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return null;
            }
        }

        public async Task<GetReceiptProfilesResponse> GetBusinessReceiptProfiles(int businessId)
        {
            try
            {
                var result = await _apiManager.GetAsync<GetReceiptProfilesResponse>(_configuration["App:SettingsApiUrl"] + "ReceiptProfile/" + businessId);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetReceiptProfilesResponse();
            }
        }
    }
}
