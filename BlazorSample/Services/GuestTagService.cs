using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class GuestTagService : IGuestTagService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public GuestTagService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<CpTagsResponse> GetCpTagsAsync(int tagCategory)
        {
            try
            {
                return await _apiManager.GetAsync<CpTagsResponse>(_configuration["App:SettingsApiUrl"] + "GuestTag/get-all-cp-tags/" + tagCategory);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CpTagsResponse();
            }
        }

        public async Task<CpTagTypesResponse> GetCpTagTypesAsync()
        {
            try
            {
                return await _apiManager.GetAsync<CpTagTypesResponse>(_configuration["App:SettingsApiUrl"] + "GuestTag/get-all-cp-tag-types");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CpTagTypesResponse();
            }
        }

        public async Task<BusinessTagListResponse> GetBusinessTagAsync(int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessTagListResponse>(_configuration["App:SettingsApiUrl"] + "GuestTag/get-business-tag/" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTagListResponse();
            }
        }
        public async Task<BusinessTagResponse> GetBusinessTagByIdAsync(int id)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessTagResponse>(_configuration["App:SettingsApiUrl"] + "GuestTag/get-business-tag-by-id/" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTagResponse();
            }
        }
        public async Task<BusinessTagListResponse> AddUpdateBusinessTagAsync(BusinessTagRequestModel businessTag)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessTagRequestModel, BusinessTagListResponse>(_configuration["App:SettingsApiUrl"] + "GuestTag/add-update-business-tag", businessTag);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTagListResponse();
            }
        }

        public async Task<BusinessTagListResponse> DeleteBusinessTagByIdAsync(int id)
        {
            try
            {
                return await _apiManager.DeleteAsync<BusinessTagListResponse>(_configuration["App:SettingsApiUrl"] + "GuestTag/delete-business-tag-by-id/" + id);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessTagListResponse();
            }
        }

        public async Task<BaseResponse> SetBusinessTagPublicAsync(int id, bool isPublic)
        {
            try
            {
                return await _apiManager.GetAsync<BaseResponse>(_configuration["App:SettingsApiUrl"] + "GuestTag/set-business-tag-public/" + id + "/" + isPublic);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
    }
}
