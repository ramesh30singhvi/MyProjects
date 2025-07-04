using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class PaymentprofileViewModel : IPaymentProfileViewModel
    {
        private IPaymentProfileService _paymentProfileService;
        public PaymentprofileViewModel(IPaymentProfileService paymentProfileService)
        {
            _paymentProfileService = paymentProfileService;
        }
        public async Task<PaymentProfileResponse> CreatePaymentProfileAsync(PaymentProfileDetailsViewModel paymentProfileDetails)
        {
            return await _paymentProfileService.CreatePaymentProfileAsync(paymentProfileDetails);
        }

        public async Task<BusinessPaymentProviderProfileListResponse> CreatePaymentProfileSettingsListAsync(List<BusinessPaymentProviderProfileSettingsViewModel> models)
        {
            return await _paymentProfileService.CreatePaymentProfileSettingsListAsync(models);
        }

        public async Task<PaymentProfilesResponse> DeletePaymentProfile(Guid IdGUID, int businessId)
        {
            return await _paymentProfileService.DeletePaymentProfile(IdGUID, businessId);
        }

        public async Task<PaymentProfileResponse> GetPaymentProfileDetails(int Id, string IdGUID)
        {
            return await _paymentProfileService.GetPaymentProfileDetails(Id, IdGUID);
        }

        public async Task<PaymentProfilesResponse> GetPaymentProfilesAsync(int? businessId = null)
        {
            return await _paymentProfileService.GetPaymentProfilesAsync(businessId);
        }
    }
}
