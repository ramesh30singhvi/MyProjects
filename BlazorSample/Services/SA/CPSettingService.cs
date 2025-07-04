using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services.SA
{
    public class CPSettingService : ICPSettingService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public CPSettingService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<CPSettingResponse> AddUpdateCPSettingAsync(CPSettingRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CPSettingResponse>("CPSetting/add-update-cp-setting", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPSettingResponse();
            }
        }
        public async Task<CPSettingResponse> GetCPSettingAsync(int? id, string metaNamespace, string metaKey)
        {
            try
            {
                return await _apiManager.GetAsync<CPSettingResponse>("CPSetting/get-cp-setting?id=" + id + "&metaNamespace=" + metaNamespace + "&metaKey=" + metaKey);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPSettingResponse();
            }
        }
        public async Task<CPSettingListResponse> GetCPSettingListAsync(string metaNamespace)
        {
            try
            {
                return await _apiManager.GetAsync<CPSettingListResponse>("CPSetting/get-cp-setting-list?metaNamespace=" + metaNamespace);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPSettingListResponse();
            }
        }
    }
}
