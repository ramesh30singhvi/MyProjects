using CellarPassAdmin.BLL.Interfaces.User;
using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Entities;
using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class UserService : IUserService
    {
        private readonly IApiManager _apiManager;
        private readonly AuthenticationStateProvider _authProvider;

        public event EventHandler<UserAuthenticatedArgs> UserAuthenticatedEvent;
        public UserService(IApiManager apiManager,
            AuthenticationStateProvider authProvider)
        {
            _apiManager = apiManager;
            _authProvider = authProvider;
            _authProvider.AuthenticationStateChanged += _authProvider_AuthenticationStateChanged;
        }

        private void _authProvider_AuthenticationStateChanged(Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState> task)
        {
            if (task.Result.User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)task.Result.User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                UserAuthenticatedEvent?.Invoke(this, new UserAuthenticatedArgs(userId));
            }
            else
            {
                UserAuthenticatedEvent?.Invoke(this, new UserAuthenticatedArgs(""));
            }
        }

        public async Task LogoutUser()
        {
            await ((ICellarPassAuthenticationStateProvider)_authProvider).MarkUserAsLoggedOut();
        }

        public async Task<UserDetailResponse> GetUserByEmail(string email)
        {
            try
            {
                UserDetailResponse response = await _apiManager.GetAsync<UserDetailResponse>("account/user-by-email/" + email);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserDetailResponse();
            }
        }

        public async Task<UploadImageResponse> UploadUserProfileImageAsync(ImageUploadRequestModel model)
        {
            try
            {
                UploadImageResponse response = await _apiManager.PostAsync<ImageUploadRequestModel, UploadImageResponse>("account/upload-profile-image", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UploadImageResponse();
            }
        }

        public async Task<UserResponse> CreateUserAsync(V4User model)
        {
            try
            {
                UserResponse response = await _apiManager.PostAsync<V4User, UserResponse>("account/create-user", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserResponse();
            }
        }

        public async Task<UserAddressResponse> GetUserAddressByUserIdAsync(int userId)
        {
            try
            {
                UserAddressResponse response = await _apiManager.GetAsync<UserAddressResponse>("account/get-address-by-userid");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserAddressResponse();
            }
        }

        public async Task<AddUpdateUserAddressResponse> CreateUserAddressAsync(V4UserAddress model)
        {
            try
            {
                AddUpdateUserAddressResponse response = await _apiManager.PostAsync<V4UserAddress, AddUpdateUserAddressResponse>("account/create-user-address", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateUserAddressResponse();
            }
        }

        public async Task<UserMetafieldResponse> GetUserMetafieldByUserIdAsync(int userId)
        {
            try
            {
                UserMetafieldResponse response = await _apiManager.GetAsync<UserMetafieldResponse>("account/get-metafield-by-user-id");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserMetafieldResponse();
            }
        }

        public async Task<AddUpdateUserMetafieldResponse> CreateUserMetafieldAsync(UserMetafield model)
        {
            try
            {
                AddUpdateUserMetafieldResponse response = await _apiManager.PostAsync<UserMetafield, AddUpdateUserMetafieldResponse>("account/create-user-metafield", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddUpdateUserMetafieldResponse();
            }
        }

        public async Task<LoginResultResponse> LoginUserAsync(CredentialsModel CredentialsModel)
        {
            try
            {
                LoginResultResponse user = await _apiManager.PostAsync<CredentialsModel, LoginResultResponse>("account/login", CredentialsModel);
                if (user.success && !string.IsNullOrEmpty(user.data.Token))
                {
                    await ((ICellarPassAuthenticationStateProvider)_authProvider).MarkUserAsAuthenticated(user.data);
                }
                return user;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                await LogoutUser();
                return new LoginResultResponse();
            }
        }

        public async Task<AppUserModel> ValidateAppLogin(AppCredentialsModel model)
        {
            try
            {
                AppUserModel response = await _apiManager.PostAsync<AppCredentialsModel, AppUserModel>("account/app-login", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AppUserModel();
            }
        }
        public async Task<UserListResponse> GetUsers()
        {
            try
            {
                UserListResponse response = await _apiManager.GetAsync<UserListResponse>("account/users");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserListResponse();
            }
        }
        public async Task<POSProfileUserModel> ValidatePOSProfileLogin(POSProfileCredentialsModel model)
        {
            return null;
        }

        public async Task<BaseResponse> ReCAPTCHAValidation(ReCAPTCHARequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<ReCAPTCHARequestModel, BaseResponse>("account/validate-recaptcha", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<UserDetailResponse> GetUserByPasswordChangeKey(string passwordChangeKey)
        {
            try
            {
                var response = await _apiManager.GetAsync<UserDetailResponse>("account/get-user-by-key/" + passwordChangeKey);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UserDetailResponse();
            }
        }
        public async Task<BaseResponse> EmailExist(string email)
        {
            try
            {
                var response = await _apiManager.GetAsync<BaseResponse>("account/email-exist/" + email);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
        public async Task<LoginResultResponse> UpdateUserPassword(UpdatePasswordRequestModel model)
        {
            try
            {
                var user = await _apiManager.PostAsync<LoginResultResponse>("account/update-user-password", model);
                if (user.success && !string.IsNullOrEmpty(user.data.Token))
                {
                    await ((ICellarPassAuthenticationStateProvider)_authProvider).MarkUserAsAuthenticated(user.data);
                }
                return user;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new LoginResultResponse();
            }
        }

        public async Task<EmailNotificationResponse> SendForgotPasswordEmailNotification(ForgotPasswordEmailRequestModel requestModel)
        {
            try
            {
                return await _apiManager.PostAsync<EmailNotificationResponse>("account/send-forgot-password-email", requestModel);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new EmailNotificationResponse();
            }
        }
    }
}
