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
using System.Threading.Tasks;
using System.Web;

namespace CellarPassAppAdmin.Client.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public SettingsService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        #region Business Holiday
        public async Task<BusinessHolidayResponse> AddUpdateBusinessHolidayAsync(BusinessHolidayRequestModel model)
        {
            try
            {
                BusinessHolidayResponse response = await _apiManager.PostAsync<BusinessHolidayRequestModel, BusinessHolidayResponse>(_configuration["App:SettingsApiUrl"] + "Settings/add-update-business-holiday", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessHolidayResponse();
            }
        }

        public async Task<BusinessHolidayListResponse> GetBusinessHolidayAsync(BusinessHolidayRequestDto dto)
        {
            try
            {
                BusinessHolidayListResponse response = await _apiManager.PostAsync<BusinessHolidayRequestDto, BusinessHolidayListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/get-business-holidays", dto);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessHolidayListResponse();
            }
        }

        public async Task<bool> DeleteBusinessHolidayById(int id)
        {
            try
            {
                bool response = await _apiManager.DeleteAsync<bool>(_configuration["App:SettingsApiUrl"] + "Settings/delete-business-holiday-by-id/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return false;
            }
        }

        #endregion Business Holiday

        #region Business Country States
        public async Task<BusinessStatesListResponse> GetBusinessStatesAsync(int businessId)
        {
            try
            {
                BusinessStatesListResponse response = await _apiManager.GetAsync<BusinessStatesListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/get-business-states/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessStatesListResponse();
            }
        }
        public async Task<BusinessStatesListResponse> AddUpdateBusinessStateSettingsAsync(SettingCountriesRequestModel model)
        {
            try
            {
                BusinessStatesListResponse response = await _apiManager.PostAsync<SettingCountriesRequestModel, BusinessStatesListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/add-update-business-state-settings", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessStatesListResponse();
            }
        }
        #endregion Business Country States

        #region Business Setting
        public async Task<BusinessSettingsResponse> GetBusinessSettingAsync(int businessId, string metaNamespace, string metaKey)
        {
            try
            {
                BusinessSettingsResponse response = await _apiManager.GetAsync<BusinessSettingsResponse>(_configuration["App:SettingsApiUrl"] + "Settings/get-business-setting/" + businessId + "/" + metaNamespace + "/" + metaKey);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessSettingsResponse();
            }
        }
        public async Task<BusinessSettingsListResponse> GetBusinessSettingsListAsync(int businessId, string metaNamespace)
        {
            try
            {
                BusinessSettingsListResponse response = await _apiManager.GetAsync<BusinessSettingsListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/get-business-setting-list/" + businessId + "/" + metaNamespace);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessSettingsListResponse();
            }
        }
        public async Task<BusinessSettingsResponse> CreateBusinessSettingAsync(BusinessSettingsRequestModel model)
        {
            try
            {
                BusinessSettingsResponse response = await _apiManager.PostAsync<BusinessSettingsRequestModel, BusinessSettingsResponse>(_configuration["App:SettingsApiUrl"] + "Settings/create-business-setting", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessSettingsResponse();
            }
        }
        public async Task<BusinessSettingsListResponse> CreateBusinessSettingListAsync(List<BusinessSettingsRequestModel> models)
        {
            try
            {
                BusinessSettingsListResponse response = await _apiManager.PostAsync<List<BusinessSettingsRequestModel>, BusinessSettingsListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/create-business-setting-list", models);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessSettingsListResponse();
            }
        }
        public async Task<BaseResponse> Commerce7TestLogin(string commerce7Username, string commerce7Password, string commerce7Tenant)
        {
            try
            {
                var url = _configuration["App:SettingsApiUrl"] + "Settings/commerce7-test-login/" + HttpUtility.UrlEncode(commerce7Username) + "/" + HttpUtility.UrlEncode(commerce7Password) + "/" + HttpUtility.UrlEncode(commerce7Tenant);
                return await _apiManager.GetAsync<BaseResponse>(url);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> ShopifyTestLogin(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<BaseResponse>(_configuration["App:SettingsApiUrl"] + "Settings/shopify-test-login/" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> BigCommerceTestLogin(string storeId, string accessToken)
        {
            try
            {
                var url = _configuration["App:SettingsApiUrl"] + "Settings/bigcommerce-test-login/" + HttpUtility.UrlEncode(storeId) + "/" + HttpUtility.UrlEncode(accessToken);
                return await _apiManager.GetAsync<BaseResponse>(url);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> WineDirectTestLogin(string username, string password)
        {
            try
            {
                var url = _configuration["App:SettingsApiUrl"] + "Settings/winedirect-test-login/" + HttpUtility.UrlEncode(username) + "/" + HttpUtility.UrlEncode(password);
                return await _apiManager.GetAsync<BaseResponse>(url);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> VinSuiteTestLogin(string username, string password)
        {
            try
            {
                var url = _configuration["App:SettingsApiUrl"] + "Settings/vinsuite-test-login/" + HttpUtility.UrlEncode(username) + "/" + HttpUtility.UrlEncode(password);
                return await _apiManager.GetAsync<BaseResponse>(url);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<MailChimpListResponse> GetMailChimpLists(string mailChimpAPIKey)
        {
            try
            {
                var url = _configuration["App:SettingsApiUrl"] + "Settings/get-mailchimp-lists/" + HttpUtility.UrlEncode(mailChimpAPIKey);
                return await _apiManager.GetAsync<MailChimpListResponse>(url);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new MailChimpListResponse();
            }
        }
        public async Task<MailChimpStoreResponse> CreateMailChimpStore(string mailChimpAPIKey, int businessId, string listId)
        {
            try
            {
                var url = _configuration["App:SettingsApiUrl"] + "Settings/create-get-mailchimp-store/" + HttpUtility.UrlEncode(mailChimpAPIKey) + "/" + businessId + "/" + listId;
                return await _apiManager.GetAsync<MailChimpStoreResponse>(url);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new MailChimpStoreResponse();
            }
        }

        public async Task<BaseResponse> AvalaraTestLogin(int accountId, string licenseKey, bool isSandboxMode)
        {
            try
            {
                var url = _configuration["App:SettingsApiUrl"] + "Settings/avalara-test-login/" + accountId + "/" + HttpUtility.UrlEncode(licenseKey) + "/" + isSandboxMode;
                return await _apiManager.GetAsync<BaseResponse>(url);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<BaseResponse> ShipCompliantTestLogin(string userName, string password, string brandKey)
        {
            try
            {
                var url = _configuration["App:SettingsApiUrl"] + "Settings/shipcompliant-test-login/" + HttpUtility.UrlEncode(userName) + "/" + HttpUtility.UrlEncode(password) + "/" + HttpUtility.UrlEncode(brandKey);
                return await _apiManager.GetAsync<BaseResponse>(url);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        #endregion

        #region GeoLocation
        public async Task<GeoLocation> GetGeoLatLongByAddress(string address)
        {
            try
            {
                GeoLocation response = await _apiManager.GetAsync<GeoLocation>(_configuration["App:SettingsApiUrl"] + "Settings/get-geo-lat-lng-by-address/" + address);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GeoLocation();
            }
        }
        #endregion GeoLocation

        #region Business Integration Setting
        public async Task<BusinessIntegrationSettingListResponse> GetBusinessIntegrationSettingListAsync(string metaNamespace, string metaKey, int? businessId, int? integrationId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessIntegrationSettingListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/business-integration-setting-list?metaNamespace=" + metaNamespace + "&metaKey=" + metaKey + "&businessId=" + businessId + "&integrationId=" + null);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessIntegrationSettingListResponse();
            }
        }
        public async Task<BusinessIntegrationSettingResponse> GetBusinessIntegrationSettingAsync(int businessId, int integrationId, string metaNamespace, string metaKey)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessIntegrationSettingResponse>(_configuration["App:SettingsApiUrl"] + "Settings/business-integration-setting?" + businessId + "/" + integrationId + "/" + metaNamespace + "/" + metaKey);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessIntegrationSettingResponse();
            }
        }
        public async Task<BusinessIntegrationSettingResponse> AddUpdateBusinessIntegrationSettingAsync(BusinessIntegrationSettingRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessIntegrationSettingRequestModel, BusinessIntegrationSettingResponse>(_configuration["App:SettingsApiUrl"] + "Settings/add-update-business-integration-setting", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessIntegrationSettingResponse();
            }
        }
        public async Task<BusinessIntegrationSettingListResponse> AddUpdateBusinessIntegrationSettingListAsync(List<BusinessIntegrationSettingRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<List<BusinessIntegrationSettingRequestModel>, BusinessIntegrationSettingListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/add-update-business-integration-setting-list", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessIntegrationSettingListResponse();
            }
        }
        #endregion Business Integration Setting

        #region Business Return Reason
        public async Task<BusinessReturnReasonListResponse> GetBusinessReturnReasonList()
        {
            try
            {
                BusinessReturnReasonListResponse response = await _apiManager.GetAsync<BusinessReturnReasonListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/return-reason-list/");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessReturnReasonListResponse();
            }
        }

        public async Task<BaseResponse> AddBusinessReturnReason(BusinessReturnReasonRequestModel model)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<BusinessReturnReasonRequestModel, BaseResponse>(_configuration["App:SettingsApiUrl"] + "Settings/add-return-reason", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> RemoveBusinessReturnReason(int id)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:SettingsApiUrl"] + "Settings/remove-return-reason/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        #endregion Business Return Reason

        #region Business Tax Class
        public async Task<BusinessTaxClassListResponse> GetBusinessTaxClassList(int businessId)
        {
            try
            {
                BusinessTaxClassListResponse response = await _apiManager.GetAsync<BusinessTaxClassListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/tax-class-list/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTaxClassListResponse();
            }
        }

        public async Task<BaseResponse> AddUpdateBusinessTaxClass(TaxClassModel model)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<TaxClassModel, BaseResponse>(_configuration["App:SettingsApiUrl"] + "Settings/add-update-tax-class", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        #endregion Business Tax Class

        #region Business Custom Tax Rate
        public async Task<BusinessCustomTaxRateListResponse> GetBusinessCustomTaxRates(int businessId)
        {
            try
            {
                BusinessCustomTaxRateListResponse response = await _apiManager.GetAsync<BusinessCustomTaxRateListResponse>(_configuration["App:SettingsApiUrl"] + "Settings/custom-tax-rate/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessCustomTaxRateListResponse();
            }
        }

        public async Task<BaseResponse> AddUpdateBusinessCustomTaxRate(BusinessCustomTaxRateRequestModel model)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<BusinessCustomTaxRateRequestModel, BaseResponse>(_configuration["App:SettingsApiUrl"] + "Settings/add-update-custom-tax-rate", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        #endregion Business Custom Tax Rate

        #region Avalara Tax Rate

        public async Task<BaseResponse> SaveTaxRates(int accountId, string licenseKey)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:SettingsApiUrl"] + "Settings/save-avalara-tax-rates" + accountId + "/" + licenseKey);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        #endregion Avalara Tax Rate
    }
}
