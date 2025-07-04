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
    public class SafetyPledgeService : ISafetyPledgeService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public SafetyPledgeService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BusinessSettingsListResponse> GetSafetyPledgeSettingListAsync(int businessId, string metaNamespace)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessSettingsListResponse>(_configuration["App:ContentApiUrl"] + "SafetyPledge/get-safety-pledge-settings/" + businessId + "/" + metaNamespace);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessSettingsListResponse();
            }
        }
        public async Task<BusinessSettingsListResponse> AddUpdateSafetyPledgeSettingAsync(List<BusinessSettingsRequestModel> requestModels)
        {
            try
            {
                return await _apiManager.PostAsync<List<BusinessSettingsRequestModel>, BusinessSettingsListResponse>(_configuration["App:ContentApiUrl"] + "SafetyPledge/add-update-safety-pledge-settings", requestModels);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessSettingsListResponse();
            }
        }
    }
}
