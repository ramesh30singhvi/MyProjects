using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class UserViewModel : IUserViewModel
    {
        private IUserService _userService;
        private IPeopleUserService _peopleUserService;

        public UserViewModel(IUserService userService, IPeopleUserService peopleUserService)
        {
            _userService = userService;
            _peopleUserService = peopleUserService;
        }

        #region User
        public async Task<BaseResponse> UpdateUserDetail(UpdateUserRequestModel request)
        {
            return await _peopleUserService.UpdateUserDetail(request);
        }
        #endregion User

        #region User Adddress
        public async Task<UserDetailResponse> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmail(email);
            return user;
        }

        public async Task<UserListResponse> GetUsers()
        {
            var users = await _userService.GetUsers();
            return users;
        }

        public async Task<UserAddressListResponse> GetUserAddressList(Guid userGuid)
        {
            return await _peopleUserService.GetUserAddressList(userGuid);
        }

        public async Task<UserAddressDetailResponse> GetUserAddressDetail(Guid addressGuid)
        {
            return await _peopleUserService.GetUserAddressDetail(addressGuid);
        }

        public async Task<BaseResponse> AddUpdateUserAddress(UserAddressRequestModel request)
        {
            return await _peopleUserService.AddUpdateUserAddress(request);
        }

        public async Task<BaseResponse> UpdateUserDefaultBillingAddress(Guid addressGuid)
        {
            return await _peopleUserService.UpdateUserDefaultBillingAddress(addressGuid);
        }

        public async Task<BaseResponse> UpdateUserDefaultShippingAddress(Guid addressGuid)
        {
            return await _peopleUserService.UpdateUserDefaultShippingAddress(addressGuid);
        }

        public async Task<BaseResponse> RemoveUserAddress(Guid addressGuid)
        {
            return await _peopleUserService.RemoveUserAddress(addressGuid);
        }
        #endregion User Adddress

        #region User Metafield
        public async Task<UserMetafieldListResponse> GetUserMetafieldListAsync(int userId, int businessId, string metaNamespace)
        {
            return await _peopleUserService.GetUserMetafieldListAsync(userId, businessId, metaNamespace);
        }
        public async Task<UserMetafieldListResponse> AddUpdateUserMetafieldListAsync(List<UserMetafieldRequestModel> models)
        {
            return await _peopleUserService.AddUpdateUserMetafieldListAsync(models);
        }
        #endregion User Metafield

        #region User Bookmarks

        public async Task<UserBookmarksResponse> GetUserBookmarks(int businessId)
        {
            return await _peopleUserService.GetUserBookmarks(businessId, 0);
        }

        public async Task<BaseResponse> AddUpdateUserBookmarks(List<UserBookmarkRequestModel> request)
        {
            return await _peopleUserService.AddUpdateUserBookmarks(request, 0);
        }

        #endregion User Bookmarks
    }
}
