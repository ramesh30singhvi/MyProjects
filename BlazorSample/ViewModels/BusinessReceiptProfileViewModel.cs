using CellarPassAppAdmin.DAL.Repositories.Setting;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class BusinessReceiptProfileViewModel : IBusinessReceiptProfileViewModel
    {
        private readonly IBusinessReceiptProfileService _businessReceiptProfileService;
        public BusinessReceiptProfileViewModel(IBusinessReceiptProfileService businessReceiptProfileService)
        {
            _businessReceiptProfileService = businessReceiptProfileService;
        }
        public async Task<AddUpdateReceiptProfileResponse> AddUpdateBusinessReceiptProfile(BusinessReceiptProfileRequestModel model)
        {
            return await _businessReceiptProfileService.AddUpdateBusinessReceiptProfile(model);
        }

        public async Task<GetReceiptProfileDetailsResponse> GetBusinessReceiptProfileDetails(Guid IdGUID)
        {
            return await _businessReceiptProfileService.GetBusinessReceiptProfileDetails(IdGUID);
        }

        public async Task<GetReceiptProfilesResponse> GetBusinessReceiptProfiles(int businessId)
        {
            return await _businessReceiptProfileService.GetBusinessReceiptProfiles(businessId);
        }
    }
}
