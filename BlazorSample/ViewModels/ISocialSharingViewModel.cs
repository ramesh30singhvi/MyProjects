using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ISocialSharingViewModel
    {
        Task<BusinessSettingsListResponse> GetSocialSharingListAsync(int businessId, string metaNamespace);
        Task<BusinessSettingsListResponse> AddUpdateSocialSharingAsync(List<BusinessSettingsRequestModel> requestModels);
    }
}
