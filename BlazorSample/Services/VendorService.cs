using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class VendorService : IVendorService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public VendorService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<BusinessVendorListResponse> GetBusinessVendorsAsync(int businessId)
        {
            try
            {
                BusinessVendorListResponse response = await _apiManager.GetAsync<BusinessVendorListResponse>(_configuration["App:PeopleApiUrl"] + "Vendor/business-vendors/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessVendorListResponse();
            }
        }

        public async Task<BusinessVendorResponse> GetBusinessVendorByIdAsync(Guid id)
        {
            try
            {
                BusinessVendorResponse response = await _apiManager.GetAsync<BusinessVendorResponse>(_configuration["App:PeopleApiUrl"] + "Vendor/business-vendor-by-id/" + id);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessVendorResponse();
            }
        }

        public async Task<BusinessVendorResponse> AddUpdateBusinessVendorAsync(BusinessVendorRequestModel request)
        {
            try
            {
                BusinessVendorResponse response = await _apiManager.PostAsync<BusinessVendorRequestModel, BusinessVendorResponse>(_configuration["App:PeopleApiUrl"] + "Vendor/add-update-business-vendor", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessVendorResponse();
            }
        }
    }
}
