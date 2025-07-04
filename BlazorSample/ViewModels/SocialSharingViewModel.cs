using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class SocialSharingViewModel : ISocialSharingViewModel
    {
        private ISocialSharingService _socialSharingService;

        public SocialSharingViewModel(ISocialSharingService socialSharingService)
        {
            _socialSharingService = socialSharingService;
        }
        public async Task<BusinessSettingsListResponse> GetSocialSharingListAsync(int businessId, string metaNamespace)
        {
            return await _socialSharingService.GetSocialSharingListAsync(businessId, metaNamespace);
        }

        public async Task<BusinessSettingsListResponse> AddUpdateSocialSharingAsync(List<BusinessSettingsRequestModel> models)
        {
            return await _socialSharingService.AddUpdateSocialSharingAsync(models);
        }
    }
}
