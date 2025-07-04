using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ICustomerViewModel
    {
        #region Business Customer

        Task<SearchCustomerResponse> SearchBusinessCustomers(int businessId, string searchText);
        Task<CustomerListResponse> GetBusinessCustomerList(BusinessCustomerRequestModel request);
        Task<CustomerDetailResponse> GetBusinessCustomerDetail(Guid userGUID);
        Task<SearchCustomerResponse> GetLatestBusinessCustomers(int businessId, int count);
        Task<ExportCustomerListResponse> ExportBusinessCustomers(int businessId, string type);
        Task<SearchCustomerResponse> GetRecentBusinessCustomers(int businessId);
        Task<AddCustomerResponse> AddBusinessCustomer(AddCustomerRequestModel request);
        Task<BusinessCustomerDetailResponse> GetBusinessCustomerByEmail(string email);
        Task<BaseResponse> UpdateCustomerDetail(UpdateUserRequestModel request);
        Task<UploadImageResponse> UploadCustomerProfileImageAsync(CustomerImageUploadRequestModel model);
        Task<GetBusinessCustomerTrendResponse> GetBusinessCustomerTrends(int businessId, DateTime StartDate, DateTime EndDate);
        Task<BaseResponse> UpdateBusinessCustomerTags(UpdateBusinessCustomerTagsRequestModel model);

        #endregion Business Customer

        #region Business Customer Address

        Task<UserAddressListResponse> GetCustomerAddressList(Guid customerGuid);
        Task<UserAddressDetailResponse> GetCustomerAddressDetail(Guid addressGuid);
        Task<BaseResponse> AddUpdateCustomerAddress(UserAddressRequestModel request);
        Task<BaseResponse> UpdateCustomerDefaultBillingAddress(Guid addressGuid);
        Task<BaseResponse> UpdateCustomerDefaultShippingAddress(Guid addressGuid);
        Task<BaseResponse> RemoveCustomerAddress(Guid addressGuid);

        #endregion Business Customer Address

        #region Business Customer Note

        Task<BusinessCustomerNotesResponse> AddUpdateBusinessCustomerNote(BusinessCustomerNoteRequestModel model);

        Task<BusinessCustomerNotesResponse> DeleteBusinessCustomerNote(int id, int businessCustomerId);

        #endregion Business Customer Note

        Task<BusinessCustomerMetafieldListResponse> AddUpdateBusinessCustomerMetafieldListAsync(List<BusinessCustomerMetafieldRequestModel> models);
    }
}
