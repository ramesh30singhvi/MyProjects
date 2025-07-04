using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        #region Business Customer

        public CustomerService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<SearchCustomerResponse> SearchBusinessCustomers(int businessId, string searchText)
        {
            try
            {
                SearchCustomerResponse response = await _apiManager.GetAsync<SearchCustomerResponse>(_configuration["App:PeopleApiUrl"] + "Customer/search-business-customers/" + businessId + "/" + searchText);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SearchCustomerResponse();
            }
        }

        public async Task<CustomerListResponse> GetBusinessCustomerList(BusinessCustomerRequestModel request)
        {
            try
            {
                CustomerListResponse response = await _apiManager.PostAsync<BusinessCustomerRequestModel, CustomerListResponse>(_configuration["App:PeopleApiUrl"] + "Customer/business-customer-list", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CustomerListResponse();
            }
        }

        public async Task<CustomerDetailResponse> GetBusinessCustomerDetail(Guid userGUID)
        {
            try
            {
                CustomerDetailResponse response = await _apiManager.GetAsync<CustomerDetailResponse>(_configuration["App:PeopleApiUrl"] + "Customer/detail/" + userGUID);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CustomerDetailResponse();
            }
        }

        public async Task<SearchCustomerResponse> GetLatestBusinessCustomers(int businessId, int count)
        {
            try
            {
                SearchCustomerResponse response = await _apiManager.GetAsync<SearchCustomerResponse>(_configuration["App:PeopleApiUrl"] + "Customer/latest-business-customers/" + businessId + "/" + count);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SearchCustomerResponse();
            }
        }

        public async Task<ExportCustomerListResponse> ExportBusinessCustomers(int businessId, string type)
        {
            try
            {
                ExportCustomerListResponse response = await _apiManager.GetAsync<ExportCustomerListResponse>(_configuration["App:PeopleApiUrl"] + "Customer/export-business-customers/" + businessId + "/" + type);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new ExportCustomerListResponse();
            }
        }

        public async Task<SearchCustomerResponse> GetRecentBusinessCustomers(int businessId)
        {
            try
            {
                SearchCustomerResponse response = await _apiManager.GetAsync<SearchCustomerResponse>(_configuration["App:PeopleApiUrl"] + "Customer/recent-business-customers/" + businessId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SearchCustomerResponse();
            }
        }

        public async Task<AddCustomerResponse> AddBusinessCustomer(AddCustomerRequestModel request)
        {
            try
            {
                AddCustomerResponse response = await _apiManager.PostAsync<AddCustomerRequestModel, AddCustomerResponse>(_configuration["App:PeopleApiUrl"] + "Customer/add-business-customer", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddCustomerResponse();
            }
        }

        public async Task<BusinessCustomerDetailResponse> GetBusinessCustomerByEmail(string email)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessCustomerDetailResponse>(_configuration["App:PeopleApiUrl"] + "Customer/get-customer-by-email/" + email);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessCustomerDetailResponse();
            }
        }

        public async Task<BaseResponse> UpdateCustomerDetail(UpdateUserRequestModel request)
        {
            try
            {
                BaseResponse response = await _apiManager.PutAsync<UpdateUserRequestModel, BaseResponse>(_configuration["App:PeopleApiUrl"] + "Customer/update-customer-detail/", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<UploadImageResponse> UploadCustomerProfileImageAsync(CustomerImageUploadRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<UploadImageResponse>(_configuration["App:PeopleApiUrl"] + "Customer/update-customer-profile-image", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UploadImageResponse();
            }
        }

        public async Task<GetBusinessCustomerTrendResponse> GetBusinessCustomerTrends(int businessId, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                return await _apiManager.GetAsync<GetBusinessCustomerTrendResponse>(string.Format("{0}Customer/get-customer-trends?BusinessId={1}&StartDate={2}&EndDate={3}", _configuration["App:PeopleApiUrl"], businessId, StartDate.ToString("MM-dd-yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture), EndDate.ToString("MM-dd-yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture)));
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new GetBusinessCustomerTrendResponse();
            }
        }
        public async Task<BaseResponse> UpdateBusinessCustomerTags(UpdateBusinessCustomerTagsRequestModel model)
        {
            try
            {
                return await _apiManager.PutAsync<UpdateBusinessCustomerTagsRequestModel, BaseResponse>(_configuration["App:PeopleApiUrl"] + "Customer/update-business-customer-tags/", model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new BaseResponse();
            }
        }

        #endregion Business Customer

        #region Business Customer Address

        public async Task<UserAddressListResponse> GetCustomerAddressList(Guid customerGuid)
        {
            try
            {
                UserAddressListResponse response = await _apiManager.GetAsync<UserAddressListResponse>(_configuration["App:PeopleApiUrl"] + "Customer/address-list/" + customerGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserAddressListResponse();
            }
        }

        public async Task<UserAddressDetailResponse> GetCustomerAddressDetail(Guid addressGuid)
        {
            try
            {
                UserAddressDetailResponse response = await _apiManager.GetAsync<UserAddressDetailResponse>(_configuration["App:PeopleApiUrl"] + "Customer/address-detail/" + addressGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserAddressDetailResponse();
            }
        }

        public async Task<BaseResponse> AddUpdateCustomerAddress(UserAddressRequestModel request)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<UserAddressRequestModel, BaseResponse>(_configuration["App:PeopleApiUrl"] + "Customer/add-update-address", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> UpdateCustomerDefaultBillingAddress(Guid addressGuid)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Customer/update-default-billing-address/" + addressGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> UpdateCustomerDefaultShippingAddress(Guid addressGuid)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Customer/update-default-shipping-address/" + addressGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> RemoveCustomerAddress(Guid addressGuid)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "Customer/remove-address/" + addressGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        #endregion Business Customer Address

        #region Business Customer Note

        public async Task<BusinessCustomerNotesResponse> AddUpdateBusinessCustomerNote(BusinessCustomerNoteRequestModel model)
        {
            try
            {
                BusinessCustomerNotesResponse response = await _apiManager.PostAsync<BusinessCustomerNoteRequestModel, BusinessCustomerNotesResponse>(_configuration["App:PeopleApiUrl"] + "Customer/add-update-note", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessCustomerNotesResponse();
            }
        }

        public async Task<BusinessCustomerNotesResponse> DeleteBusinessCustomerNote(int id, int businessCustomerId)
        {
            try
            {
                BusinessCustomerNotesResponse response = await _apiManager.GetAsync<BusinessCustomerNotesResponse>(_configuration["App:PeopleApiUrl"] + "Customer/delete-note/" + id + "/" + businessCustomerId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessCustomerNotesResponse();
            }
        }

        #endregion Business Customer Note

        public async Task<BusinessCustomerMetafieldListResponse> AddUpdateBusinessCustomerMetafieldListAsync(List<BusinessCustomerMetafieldRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessCustomerMetafieldListResponse>(_configuration["App:PeopleApiUrl"] + "Customer/add-update-business-customer-metafields", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessCustomerMetafieldListResponse();
            }
        }
    }
}
