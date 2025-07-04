using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ISettingsViewModel
    {
        Task<BusinessHolidayResponse> AddUpdateBusinessHolidayAsync(BusinessHolidayRequestModel model);
        Task<BusinessHolidayListResponse> GetBusinessHolidayAsync(BusinessHolidayRequestDto dto);
        Task<bool> DeleteBusinessHolidayById(int id);
        Task<BusinessStatesListResponse> GetBusinessStatesAsync(int businessId);
        Task<BusinessStatesListResponse> AddUpdateBusinessStateSettingsAsync(SettingCountriesRequestModel model);
        Task<BusinessSettingsResponse> GetBusinessSettingAsync(int businessId, string metaNamespace, string metaKey);
        Task<BusinessSettingsListResponse> GetBusinessSettingsListAsync(int businessId, string metaNamespace);
        Task<BusinessSettingsResponse> CreateBusinessSettingAsync(BusinessSettingsRequestModel model);
        Task<BusinessSettingsListResponse> CreateBusinessSettingListAsync(List<BusinessSettingsRequestModel> models);
        Task<BusinessIntegrationSettingResponse> GetBusinessIntegrationSettingAsync(int businessId, int integrationId, string metaNamespace, string metaKey);
        Task<BusinessIntegrationSettingListResponse> GetBusinessIntegrationSettingListAsync(string metaNamespace, string metaKey, int? businessId, int? integrationId);
        Task<BusinessIntegrationSettingResponse> AddUpdateBusinessIntegrationSettingAsync(BusinessIntegrationSettingRequestModel model);
        Task<BusinessIntegrationSettingListResponse> AddUpdateBusinessIntegrationSettingListAsync(List<BusinessIntegrationSettingRequestModel> models);
        Task<BaseResponse> Commerce7TestLogin(string commerce7Username, string commerce7Password, string commerce7Tenant);
        Task<BaseResponse> ShopifyTestLogin(int businessId);
        Task<BaseResponse> BigCommerceTestLogin(string storeId, string accessToken);
        Task<BaseResponse> WineDirectTestLogin(string username, string password);
        Task<BaseResponse> VinSuiteTestLogin(string username, string password);
        Task<MailChimpListResponse> GetMailChimpLists(string mailChimpAPIKey);
        Task<MailChimpStoreResponse> CreateMailChimpStore(string mailChimpAPIKey, int businessId, string listId);
        Task<BaseResponse> AvalaraTestLogin(int accountId, string licenseKey, bool isSandboxMode);
        Task<BaseResponse> ShipCompliantTestLogin(string userName, string password, string brandKey);
        Task<GeoLocation> GetGeoLatLongByAddress(string address);
        Task<BusinessReturnReasonListResponse> GetBusinessReturnReasonList();
        Task<BaseResponse> AddBusinessReturnReason(BusinessReturnReasonRequestModel model);
        Task<BaseResponse> RemoveBusinessReturnReason(int id);
        Task<BusinessTaxClassListResponse> GetBusinessTaxClassList(int businessId);
        Task<BaseResponse> AddUpdateBusinessTaxClass(TaxClassModel model);
        Task<BusinessCustomTaxRateListResponse> GetBusinessCustomTaxRates(int businessId);
        Task<BaseResponse> AddUpdateBusinessCustomTaxRate(BusinessCustomTaxRateRequestModel model);
    }
}
