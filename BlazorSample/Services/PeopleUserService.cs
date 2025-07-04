using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class PeopleUserService : IPeopleUserService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public PeopleUserService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        #region User
        public async Task<BaseResponse> UpdateUserDetail(UpdateUserRequestModel request)
        {
            try
            {
                BaseResponse response = await _apiManager.PutAsync<UpdateUserRequestModel, BaseResponse>(_configuration["App:PeopleApiUrl"] + "User/update-user-detail/", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        #endregion User

        #region User Adddress
        public async Task<UserAddressListResponse> GetUserAddressList(Guid userGuid)
        {
            try
            {
                UserAddressListResponse response = await _apiManager.GetAsync<UserAddressListResponse>(_configuration["App:PeopleApiUrl"] + "User/address-list/" + userGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserAddressListResponse();
            }
        }

        public async Task<UserAddressDetailResponse> GetUserAddressDetail(Guid addressGuid)
        {
            try
            {
                UserAddressDetailResponse response = await _apiManager.GetAsync<UserAddressDetailResponse>(_configuration["App:PeopleApiUrl"] + "User/address-detail/" + addressGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserAddressDetailResponse();
            }
        }

        public async Task<BaseResponse> AddUpdateUserAddress(UserAddressRequestModel request)
        {
            try
            {
                BaseResponse response = await _apiManager.PostAsync<UserAddressRequestModel, BaseResponse>(_configuration["App:PeopleApiUrl"] + "User/add-update-address", request);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> UpdateUserDefaultBillingAddress(Guid addressGuid)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "User/update-default-billing-address/" + addressGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> UpdateUserDefaultShippingAddress(Guid addressGuid)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "User/update-default-shipping-address/" + addressGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        public async Task<BaseResponse> RemoveUserAddress(Guid addressGuid)
        {
            try
            {
                BaseResponse response = await _apiManager.GetAsync<BaseResponse>(_configuration["App:PeopleApiUrl"] + "User/remove-user-address/" + addressGuid);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        #endregion User Adddress

        #region User Metafield
        public async Task<UserMetafieldListResponse> GetUserMetafieldListAsync(int userId, int businessId, string metaNamespace)
        {
            try
            {
                return await _apiManager.GetAsync<UserMetafieldListResponse>(_configuration["App:PeopleApiUrl"] + "User/get-user-metafields/" + userId + "/" + businessId + "/" + metaNamespace);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserMetafieldListResponse();
            }
        }
        public async Task<UserMetafieldListResponse> AddUpdateUserMetafieldListAsync(List<UserMetafieldRequestModel> models)
        {
            try
            {
                return await _apiManager.PostAsync<UserMetafieldListResponse>(_configuration["App:PeopleApiUrl"] + "User/add-update-user-metafields", models);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserMetafieldListResponse();
            }
        }

        public Task<VerifiedUserDataResponse> GetVerifiedUserData(int BusinessId, int Pin)
        {
            throw new NotImplementedException();
        }
        #endregion User Metafield

        #region User Bookmarks

        public async Task<UserBookmarksResponse> GetUserBookmarks(int businessId, int userId)
        {
            try
            {
                return await _apiManager.GetAsync<UserBookmarksResponse>(_configuration["App:PeopleApiUrl"] + "User/bookmarks/" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserBookmarksResponse();
            }
        }

        public async Task<BaseResponse> AddUpdateUserBookmarks(List<UserBookmarkRequestModel> request, int userId)
        {
            try
            {
                return await _apiManager.PostAsync<List<UserBookmarkRequestModel>, BaseResponse>(_configuration["App:PeopleApiUrl"] + "User/add-update-bookmarks", request);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserMetafieldListResponse();
            }
        }

        #endregion User Bookmarks
    }
}
