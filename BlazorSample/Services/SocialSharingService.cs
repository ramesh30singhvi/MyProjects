using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CellarPassAppAdmin.Client.Services
{
    public class SocialSharingService : ISocialSharingService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public SocialSharingService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<BusinessSettingsListResponse> GetSocialSharingListAsync(int businessId, string metaNamespace)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessSettingsListResponse>(_configuration["App:ContentApiUrl"] + "SocialSharing/get-social-sharing-settings/" + businessId + "/" + metaNamespace);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessSettingsListResponse();
            }
        }
        public async Task<BusinessSettingsListResponse> AddUpdateSocialSharingAsync(List<BusinessSettingsRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<List<BusinessSettingsRequestModel>, BusinessSettingsListResponse>(_configuration["App:ContentApiUrl"] + "SocialSharing/add-update-social-sharing-settings", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessSettingsListResponse();
            }
        }

    }
}
