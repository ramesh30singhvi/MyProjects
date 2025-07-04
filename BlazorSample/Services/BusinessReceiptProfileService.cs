using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.DAL.Repositories.Setting;
using CellarPassAppAdmin.Shared.Models.RequestModel;
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
    public class BusinessReceiptProfileService : IBusinessReceiptProfileService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        public BusinessReceiptProfileService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<AddUpdateReceiptProfileResponse> AddUpdateBusinessReceiptProfile(BusinessReceiptProfileRequestModel model)
        {
            try
            {
                var result = await _apiManager.PostAsync<BusinessReceiptProfileRequestModel, AddUpdateReceiptProfileResponse>(_configuration["App:SettingsApiUrl"] + "ReceiptProfile", model);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateReceiptProfileResponse();
            }
        }

        public async Task<GetReceiptProfileDetailsResponse> GetBusinessReceiptProfileDetails(Guid IdGUID)
        {
            try
            {
                var result = await _apiManager.GetAsync<GetReceiptProfileDetailsResponse>(_configuration["App:SettingsApiUrl"] + "ReceiptProfile/get-business-receipt-profile-details/" + IdGUID);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetReceiptProfileDetailsResponse();
            }
        }

        public async Task<GetReceiptProfilesResponse> GetBusinessReceiptProfiles(int businessId)
        {
            try
            {
                var result = await _apiManager.GetAsync<GetReceiptProfilesResponse>(_configuration["App:SettingsApiUrl"] + "ReceiptProfile/" + businessId);
                return result;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetReceiptProfilesResponse();
            }
        }
    }
}
