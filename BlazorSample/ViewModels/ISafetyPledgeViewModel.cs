using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ISafetyPledgeViewModel
    {
        Task<BusinessSettingsListResponse> GetSafetyPledgeSettingListAsync(int businessId, string metaNamespace);
        Task<BusinessSettingsListResponse> AddUpdateSafetyPledgeSettingAsync(List<BusinessSettingsRequestModel> requestModels);
    }
}
