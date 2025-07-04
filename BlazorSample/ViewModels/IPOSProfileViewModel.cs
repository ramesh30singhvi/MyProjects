using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IPOSProfileViewModel
    {
        Task<AddEditPOSProfileResponse> AddUpdatePOSProfile(POSProfileRequestModel model);
        Task<GetPOSProfileDetailsResponse> GetPOSProfileDetails(int pOSProfileId, string pOSProfileGuid);
        Task<GetPOSProfileListResponse> GetPOSProfileList(int businessId, bool activeOnly);
        Task<BaseResponse> UpdatePOSProfileStatus(POSProfileStatusRequestModel model);
        Task<GetPOSPaymentProfileListResponse> GetPOSPaymentProfileList(int businessId);
    }
}
