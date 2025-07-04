using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class SettingsViewModel : ISettingsViewModel
    {
        private ISettingsService _settingsService;

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<BusinessHolidayResponse> AddUpdateBusinessHolidayAsync(BusinessHolidayRequestModel model)
        {
            return await _settingsService.AddUpdateBusinessHolidayAsync(model);
        }
        public async Task<BusinessHolidayListResponse> GetBusinessHolidayAsync(BusinessHolidayRequestDto dto)
        {
            return await _settingsService.GetBusinessHolidayAsync(dto);
        }

        public async Task<bool> DeleteBusinessHolidayById(int id)
        {
            return await _settingsService.DeleteBusinessHolidayById(id);
        }
        public async Task<BusinessStatesListResponse> GetBusinessStatesAsync(int businessId)
        {
            return await _settingsService.GetBusinessStatesAsync(businessId);
        }
        public async Task<BusinessStatesListResponse> AddUpdateBusinessStateSettingsAsync(SettingCountriesRequestModel model)
        {
            return await _settingsService.AddUpdateBusinessStateSettingsAsync(model);
        }
        public async Task<BusinessSettingsResponse> GetBusinessSettingAsync(int businessId, string metaNamespace, string metaKey)
        {
            return await _settingsService.GetBusinessSettingAsync(businessId, metaNamespace, metaKey);
        }
        public async Task<BusinessSettingsListResponse> GetBusinessSettingsListAsync(int businessId, string metaNamespace)
        {
            return await _settingsService.GetBusinessSettingsListAsync(businessId, metaNamespace);
        }
        public async Task<BusinessSettingsResponse> CreateBusinessSettingAsync(BusinessSettingsRequestModel model)
        {
            return await _settingsService.CreateBusinessSettingAsync(model);
        }
        public async Task<BusinessSettingsListResponse> CreateBusinessSettingListAsync(List<BusinessSettingsRequestModel> models)
        {
            return await _settingsService.CreateBusinessSettingListAsync(models);
        }
        public async Task<BusinessIntegrationSettingResponse> GetBusinessIntegrationSettingAsync(int businessId, int integrationId, string metaNamespace, string metaKey)
        {
            return await _settingsService.GetBusinessIntegrationSettingAsync(businessId, integrationId, metaNamespace, metaKey);
        }
        public async Task<BusinessIntegrationSettingListResponse> GetBusinessIntegrationSettingListAsync(string metaNamespace, string metaKey, int? businessId, int? integrationId)
        {
            return await _settingsService.GetBusinessIntegrationSettingListAsync(metaNamespace, metaKey, businessId, integrationId);
        }
        public async Task<BusinessIntegrationSettingResponse> AddUpdateBusinessIntegrationSettingAsync(BusinessIntegrationSettingRequestModel model)
        {
            return await _settingsService.AddUpdateBusinessIntegrationSettingAsync(model);
        }
        public async Task<BusinessIntegrationSettingListResponse> AddUpdateBusinessIntegrationSettingListAsync(List<BusinessIntegrationSettingRequestModel> models)
        {
            return await _settingsService.AddUpdateBusinessIntegrationSettingListAsync(models);
        }
        public async Task<BaseResponse> Commerce7TestLogin(string commerce7Username, string commerce7Password, string commerce7Tenant)
        {
            return await _settingsService.Commerce7TestLogin(commerce7Username, commerce7Password, commerce7Tenant);
        }
        public async Task<BaseResponse> ShopifyTestLogin(int businessId)
        {
            return await _settingsService.ShopifyTestLogin(businessId);
        }
        public async Task<BaseResponse> BigCommerceTestLogin(string storeId, string accessToken)
        {
            return await _settingsService.BigCommerceTestLogin(storeId, accessToken);
        }
        public async Task<BaseResponse> WineDirectTestLogin(string username, string password)
        {
            return await _settingsService.WineDirectTestLogin(username, password);
        }
        public async Task<BaseResponse> VinSuiteTestLogin(string username, string password)
        {
            return await _settingsService.VinSuiteTestLogin(username, password);
        }
        public async Task<MailChimpListResponse> GetMailChimpLists(string mailChimpAPIKey)
        {
            return await _settingsService.GetMailChimpLists(mailChimpAPIKey);
        }
        public async Task<MailChimpStoreResponse> CreateMailChimpStore(string mailChimpAPIKey, int businessId, string listId)
        {
            return await _settingsService.CreateMailChimpStore(mailChimpAPIKey, businessId, listId);
        }
        public async Task<BaseResponse> AvalaraTestLogin(int accountId, string licenseKey, bool isSandboxMode)
        {
            return await _settingsService.AvalaraTestLogin(accountId, licenseKey, isSandboxMode);
        }
        public async Task<BaseResponse> ShipCompliantTestLogin(string userName, string password, string brandKey)
        {
            return await _settingsService.ShipCompliantTestLogin(userName, password, brandKey);
        }
        public async Task<GeoLocation> GetGeoLatLongByAddress(string address)
        {
            return await _settingsService.GetGeoLatLongByAddress(address);
        }
        public async Task<BusinessReturnReasonListResponse> GetBusinessReturnReasonList()
        {
            return await _settingsService.GetBusinessReturnReasonList();
        }
        public async Task<BaseResponse> AddBusinessReturnReason(BusinessReturnReasonRequestModel model)
        {
            return await _settingsService.AddBusinessReturnReason(model);
        }
        public async Task<BaseResponse> RemoveBusinessReturnReason(int id)
        {
            return await _settingsService.RemoveBusinessReturnReason(id);
        }
        public async Task<BusinessTaxClassListResponse> GetBusinessTaxClassList(int businessId)
        {
            return await _settingsService.GetBusinessTaxClassList(businessId);
        }
        public async Task<BaseResponse> AddUpdateBusinessTaxClass(TaxClassModel model)
        {
            return await _settingsService.AddUpdateBusinessTaxClass(model);
        }
        public async Task<BusinessCustomTaxRateListResponse> GetBusinessCustomTaxRates(int businessId)
        {
            return await _settingsService.GetBusinessCustomTaxRates(businessId);
        }
        public async Task<BaseResponse> AddUpdateBusinessCustomTaxRate(BusinessCustomTaxRateRequestModel model)
        {
            return await _settingsService.AddUpdateBusinessCustomTaxRate(model);
        }
    }
}
