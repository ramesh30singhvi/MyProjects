using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class BusinessMetaDataService : IBusinessMetaDataService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessMetaDataService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BusinessMetaDataListResponse> GetBusinessMetaDataListAsync(int businessId, int category)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMetaDataListResponse>(_configuration["App:SettingsApiUrl"] + "BusinessMetaData/get-business-metadata?BusinessId=" + businessId+ "&category=" + category);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMetaDataListResponse();
            }
        }
        public async Task<BusinessMetaDataResponse> GetBusinessMetaDataAsync(int id)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMetaDataResponse>(_configuration["App:SettingsApiUrl"] + "BusinessMetaData/get-business-metadata-detail?Id=" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMetaDataResponse();
            }
        }
        public async Task<BusinessMetaDataResponse> AddUpdateBusinessMetaDataAsync(BusinessMetaDataRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessMetaDataResponse>(_configuration["App:SettingsApiUrl"] + "BusinessMetaData/add-update-business-metadata", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMetaDataResponse();
            }
        }

        public async Task<BusinessMetaDataXProductTypeListResponse> GetBusinessMetaDataXProductTypeListAsync()
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMetaDataXProductTypeListResponse>(_configuration["App:SettingsApiUrl"] + "BusinessMetaData/get-business-metadata-producttype");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMetaDataXProductTypeListResponse();
            }
        }
        public async Task<BusinessMetaDataXProductTypeItemListResponse> GetBusinessMetaDataXProductTypeItemListAsync(int? id, int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessMetaDataXProductTypeItemListResponse>(_configuration["App:SettingsApiUrl"] + "BusinessMetaData/get-business-metadata-producttype-item?Id=" + id+ "&BusinessId=" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMetaDataXProductTypeItemListResponse();
            }
        }

        public async Task<BusinessMetaDataXProductTypeListResponse> AddUpdateBusinessMetaDataXProductTypeAsync(BusinessMetaDataXProductTypeRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessMetaDataXProductTypeListResponse>(_configuration["App:SettingsApiUrl"] + "BusinessMetaData/add-update-business-metadata-producttype", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessMetaDataXProductTypeListResponse();
            }
        }
        public async Task<BaseResponse> SaveBusinessMetaDataSortOrder(BusinessMetaDataSortRequestModel model)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<BusinessMetaDataSortRequestModel, BaseResponse>(_configuration["App:SettingsApiUrl"] + "BusinessMetaData/business-metadata-sortorder", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
    }
}
