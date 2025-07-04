using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Entities.v4;
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
    public class BusinessPropertyService : IBusinessPropertyService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public BusinessPropertyService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        #region Business Property Setting
        public async Task<BusinessPropertySettingsResponse> GetBusinessPropertySettingsAsync(int businessPropertyId, string metaNamespace, string metaKey)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessPropertySettingsResponse>(_configuration["App:SettingsApiUrl"] + "BusinessProperty/get-business-property-setting/" + businessPropertyId + "/" + metaNamespace + "/" + metaKey);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessPropertySettingsResponse();
            }
        }
        public async Task<BusinessPropertySettingsListResponse> GetBusinessPropertySettingsListAsync(int businessPropertyId, string metaNamespace)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessPropertySettingsListResponse>(_configuration["App:SettingsApiUrl"] + "BusinessProperty/add-update-business-property-setting/" + businessPropertyId + "/" + metaNamespace);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessPropertySettingsListResponse();
            }
        }
        public async Task<BusinessPropertySettingsResponse> CreateBusinessPropertySettingsAsync(BusinessPropertySettingsRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessPropertySettingsRequestModel, BusinessPropertySettingsResponse>(_configuration["App:SettingsApiUrl"] + "BusinessProperty/add-update-business-property-setting", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessPropertySettingsResponse();
            }
        }
        public async Task<BusinessPropertySettingsListResponse> CreateBusinessPropertySettingsListAsync(List<BusinessPropertySettingsRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<List<BusinessPropertySettingsRequestModel>, BusinessPropertySettingsListResponse>(_configuration["App:SettingsApiUrl"] + "BusinessProperty/add-update-business-property-setting-list", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessPropertySettingsListResponse();
            }
        }
        #endregion


        #region Business Profiles
        public async Task<List<BusinessPropertyModel>> GetBusinessProperties(int businessId)
        {
            try
            {
                var response = await _apiManager.GetAsync<BusinessPropertiesResponse>(_configuration["App:SettingsApiUrl"] + "BusinessProperty/get-business-profiles/" + businessId);
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<BusinessPropertyModel>();
            }
        }
        public async Task<BusinessProfileViewModel> CreateBusinessPropertyAsync(BusinessProfileViewModel businessProfileDetails)
        {
            try
            {
                var response = await _apiManager.PostAsync<BusinessProfileViewModel,BusinessProfileResponse>(_configuration["App:SettingsApiUrl"] + "BusinessProperty/add-update-business-property", businessProfileDetails);
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessProfileViewModel();
            }
        }
        public async Task<BusinessProfileViewModel> GetBusinessProfilesDetails(Guid propertyGuid, string metanamespace)
        {
            try
            {
                var response = await _apiManager.GetAsync<BusinessProfileResponse>(_configuration["App:SettingsApiUrl"] + $"BusinessProperty/get-business-profile-details/{propertyGuid}");
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessProfileViewModel();
            }
        }
        public async Task<List<BusinessPropertyModel>> DeleteBusinessProperty(int businessId, int id)
        {
            try
            {
                var response = await _apiManager.DeleteAsync<BusinessPropertiesResponse>(_configuration["App:SettingsApiUrl"] + "BusinessProperty/delete-business-property/" + businessId+"/"+id);
                return response.data;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<BusinessPropertyModel>();
            }
        }
       
        #endregion
    }
}
