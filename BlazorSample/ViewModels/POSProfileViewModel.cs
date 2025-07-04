using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class POSProfileViewModel : IPOSProfileViewModel
    {
        private IPOSProfileService _pOSProfileService;

        public POSProfileViewModel(IPOSProfileService pOSProfileService)
        {
            _pOSProfileService = pOSProfileService;
        }
        public async Task<AddEditPOSProfileResponse> AddUpdatePOSProfile(POSProfileRequestModel model)
        {
            return await _pOSProfileService.AddUpdatePOSProfile(model);
        }

        public async Task<GetPOSProfileDetailsResponse> GetPOSProfileDetails(int posProfileId, string posProfileGuid)
        {
            return await _pOSProfileService.GetPOSProfileDetails(posProfileId, posProfileGuid);
        }

        public async Task<GetPOSProfileListResponse> GetPOSProfileList(int businessId, bool activeOnly)
        {
            return await _pOSProfileService.GetPOSProfileList(businessId, activeOnly);
        }
        public async Task<BaseResponse> UpdatePOSProfileStatus(POSProfileStatusRequestModel model)
        {
            return await _pOSProfileService.UpdatePOSProfileStatus(model);
        }

        public async Task<GetPOSPaymentProfileListResponse> GetPOSPaymentProfileList(int businessId)
        {
            return await _pOSProfileService.GetPOSPaymentProfileList(businessId);
        }
    }
}
