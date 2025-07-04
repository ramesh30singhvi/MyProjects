using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IBusinessReceiptProfileViewModel
    {
        Task<AddUpdateReceiptProfileResponse> AddUpdateBusinessReceiptProfile(BusinessReceiptProfileRequestModel model);
        Task<GetReceiptProfilesResponse> GetBusinessReceiptProfiles(int businessId);
        Task<GetReceiptProfileDetailsResponse> GetBusinessReceiptProfileDetails(Guid IdGUID);
    }
}
