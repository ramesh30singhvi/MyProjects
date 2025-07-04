using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class SafetyPledgeViewModel: ISafetyPledgeViewModel
    {
        private ISafetyPledgeService _safetyPledgeService;

        public SafetyPledgeViewModel(ISafetyPledgeService safetyPledgeService)
        {
            _safetyPledgeService = safetyPledgeService;
        }
        public async Task<BusinessSettingsListResponse> GetSafetyPledgeSettingListAsync(int businessId, string metaNamespace)
        {
            return await _safetyPledgeService.GetSafetyPledgeSettingListAsync(businessId, metaNamespace);
        }
        public async Task<BusinessSettingsListResponse> AddUpdateSafetyPledgeSettingAsync(List<BusinessSettingsRequestModel> models)
        {
            return await _safetyPledgeService.AddUpdateSafetyPledgeSettingAsync(models);
        }       
    }
}
