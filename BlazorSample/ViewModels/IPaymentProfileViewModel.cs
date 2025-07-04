using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IPaymentProfileViewModel
    {
        Task<PaymentProfilesResponse> GetPaymentProfilesAsync(int? businessId = null);
        Task<PaymentProfilesResponse> DeletePaymentProfile(Guid IdGUID, int businessId);
        Task<PaymentProfileResponse> GetPaymentProfileDetails(int Id, string IdGUID);
        Task<PaymentProfileResponse> CreatePaymentProfileAsync(PaymentProfileDetailsViewModel paymentProfileDetails);
        Task<BusinessPaymentProviderProfileListResponse> CreatePaymentProfileSettingsListAsync(List<BusinessPaymentProviderProfileSettingsViewModel> models);
    }
}
