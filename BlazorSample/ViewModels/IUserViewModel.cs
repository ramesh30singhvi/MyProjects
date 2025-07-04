using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IUserViewModel
    {
        Task<UserDetailResponse> GetUserByEmail(string email);
        Task<UserListResponse> GetUsers();
        Task<UserAddressListResponse> GetUserAddressList(Guid userGuid);
        Task<UserAddressDetailResponse> GetUserAddressDetail(Guid addressGuid);
        Task<BaseResponse> AddUpdateUserAddress(UserAddressRequestModel request);
        Task<BaseResponse> UpdateUserDefaultBillingAddress(Guid addressGuid);
        Task<BaseResponse> UpdateUserDefaultShippingAddress(Guid addressGuid);
        Task<BaseResponse> RemoveUserAddress(Guid addressGuid);
        Task<BaseResponse> UpdateUserDetail(UpdateUserRequestModel request);
        Task<UserMetafieldListResponse> GetUserMetafieldListAsync(int userId, int businessId, string metaNamespace);
        Task<UserMetafieldListResponse> AddUpdateUserMetafieldListAsync(List<UserMetafieldRequestModel> models);
        Task<UserBookmarksResponse> GetUserBookmarks(int businessId);
        Task<BaseResponse> AddUpdateUserBookmarks(List<UserBookmarkRequestModel> request);
    }
}
