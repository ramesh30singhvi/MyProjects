using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class CustomerViewModel : ICustomerViewModel
    {
        private ICustomerService _customerService;

        public CustomerViewModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        #region Business Customer

        public async Task<SearchCustomerResponse> SearchBusinessCustomers(int businessId, string searchText)
        {
            var response = await _customerService.SearchBusinessCustomers(businessId, searchText);
            return response;
        }

        public async Task<CustomerListResponse> GetBusinessCustomerList(BusinessCustomerRequestModel request)
        {
            var response = await _customerService.GetBusinessCustomerList(request);
            return response;
        }

        public async Task<CustomerDetailResponse> GetBusinessCustomerDetail(Guid userGUID)
        {
            var response = await _customerService.GetBusinessCustomerDetail(userGUID);
            return response;
        }

        public async Task<SearchCustomerResponse> GetLatestBusinessCustomers(int businessId, int count)
        {
            return await _customerService.GetLatestBusinessCustomers(businessId, count);
        }

        public async Task<ExportCustomerListResponse> ExportBusinessCustomers(int businessId, string type)
        {
            return await _customerService.ExportBusinessCustomers(businessId, type);
        }

        public async Task<SearchCustomerResponse> GetRecentBusinessCustomers(int businessId)
        {
            return await _customerService.GetRecentBusinessCustomers(businessId);
        }

        public async Task<AddCustomerResponse> AddBusinessCustomer(AddCustomerRequestModel request)
        {
            var response = await _customerService.AddBusinessCustomer(request);
            return response;
        }

        public async Task<BusinessCustomerDetailResponse> GetBusinessCustomerByEmail(string email)
        {
            return await _customerService.GetBusinessCustomerByEmail(email);
        }

        public async Task<BaseResponse> UpdateCustomerDetail(UpdateUserRequestModel request)
        {
            return await _customerService.UpdateCustomerDetail(request);
        }
        public async Task<UploadImageResponse> UploadCustomerProfileImageAsync(CustomerImageUploadRequestModel model)
        {
            return await _customerService.UploadCustomerProfileImageAsync(model);
        }

        public async Task<GetBusinessCustomerTrendResponse> GetBusinessCustomerTrends(int businessId, DateTime StartDate, DateTime EndDate)
        {
            return await _customerService.GetBusinessCustomerTrends(businessId, StartDate, EndDate);
        }
        public async Task<BaseResponse> UpdateBusinessCustomerTags(UpdateBusinessCustomerTagsRequestModel model)
        {
            return await _customerService.UpdateBusinessCustomerTags(model);
        }

        #endregion Business Customer

        #region Business Customer Address

        public async Task<UserAddressListResponse> GetCustomerAddressList(Guid customerGuid)
        {
            return await _customerService.GetCustomerAddressList(customerGuid);
        }

        public async Task<UserAddressDetailResponse> GetCustomerAddressDetail(Guid addressGuid)
        {
            return await _customerService.GetCustomerAddressDetail(addressGuid);
        }

        public async Task<BaseResponse> AddUpdateCustomerAddress(UserAddressRequestModel request)
        {
            return await _customerService.AddUpdateCustomerAddress(request);
        }

        public async Task<BaseResponse> UpdateCustomerDefaultBillingAddress(Guid addressGuid)
        {
            return await _customerService.UpdateCustomerDefaultBillingAddress(addressGuid);
        }

        public async Task<BaseResponse> UpdateCustomerDefaultShippingAddress(Guid addressGuid)
        {
            return await _customerService.UpdateCustomerDefaultShippingAddress(addressGuid);
        }

        public async Task<BaseResponse> RemoveCustomerAddress(Guid addressGuid)
        {
            return await _customerService.RemoveCustomerAddress(addressGuid);
        }

        #endregion Business Customer Address

        #region Business Customer Note

        public async Task<BusinessCustomerNotesResponse> AddUpdateBusinessCustomerNote(BusinessCustomerNoteRequestModel model)
        {
            return await _customerService.AddUpdateBusinessCustomerNote(model);
        }

        public async Task<BusinessCustomerNotesResponse> DeleteBusinessCustomerNote(int id, int businessCustomerId)
        {
            return await _customerService.DeleteBusinessCustomerNote(id, businessCustomerId);
        }

        #endregion Business Customer Note

        public async Task<BusinessCustomerMetafieldListResponse> AddUpdateBusinessCustomerMetafieldListAsync(List<BusinessCustomerMetafieldRequestModel> models)
        {
            return await _customerService.AddUpdateBusinessCustomerMetafieldListAsync(models);
        }
    }
}
