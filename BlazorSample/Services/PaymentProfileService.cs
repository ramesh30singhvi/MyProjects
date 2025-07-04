using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class PaymentProfileService : IPaymentProfileService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public PaymentProfileService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }


        public async Task<PaymentProfilesResponse> GetPaymentProfilesAsync(int? businessId = null)
        {
            try
            {
                var response = await _apiManager.GetAsync<PaymentProfilesResponse>(_configuration["App:SettingsApiUrl"] + "PaymentProfile/get-payment-profiles?businessId="+ businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new PaymentProfilesResponse();
            }
        }


        public async Task<PaymentProfilesResponse> DeletePaymentProfile(Guid IdGUID, int businessId)
        {
            try
            {
                var response = await _apiManager.DeleteAsync<PaymentProfilesResponse>(_configuration["App:SettingsApiUrl"] + "PaymentProfile/delete-payment-profile/"+ IdGUID + "/"+ businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new PaymentProfilesResponse();
            }
        }
        public async Task<PaymentProfileResponse> CreatePaymentProfileAsync(PaymentProfileDetailsViewModel paymentProfileDetails)
        {
            try
            {
                var response = await _apiManager.PostAsync<PaymentProfileDetailsViewModel, PaymentProfileResponse>(_configuration["App:SettingsApiUrl"] + "PaymentProfile/add-update-payment-profile", paymentProfileDetails);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new PaymentProfileResponse();
            }
        }

        public Task<BusinessPaymentProviderProfileListResponse> CreatePaymentProfileSettingsListAsync(List<BusinessPaymentProviderProfileSettingsViewModel> models)
        {
            throw new NotImplementedException();
        }

        public async Task<PaymentProfileResponse> GetPaymentProfileDetails(int Id, string IdGUID)
        {
            try
            {
                var response = await _apiManager.GetAsync<PaymentProfileResponse>(_configuration["App:SettingsApiUrl"] + "PaymentProfile/get-payment-profile"+$"?IdGUID={IdGUID}");
                return response;
            }
            catch(HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new PaymentProfileResponse();
            }

        }

    }
}
