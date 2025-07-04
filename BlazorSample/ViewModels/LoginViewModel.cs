using CellarPassAppAdmin.Shared.Services;
using CellarPassAppAdmin.Shared.Models;
using System.Threading.Tasks;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class LoginViewModel : ILoginViewModel
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        private IUserService _userService;
        public LoginViewModel()
        {

        }
        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<LoginResultResponse> LoginUser()
        {
            LoginResultResponse user = await _userService.LoginUserAsync(this);
            return user;
        }
        public async Task<UserDetailResponse> GetUserByEmail(string email)
        {
            return await _userService.GetUserByEmail(email);
        }

        public static implicit operator LoginViewModel(CredentialsModel CredentialsModel)
        {
            return new LoginViewModel
            {
                EmailAddress = CredentialsModel.UserName,
                Password = CredentialsModel.Password
            };
        }

        public static implicit operator CredentialsModel(LoginViewModel loginViewModel)
        {
            return new CredentialsModel
            {
                UserName = loginViewModel.EmailAddress,
                Password = loginViewModel.Password
            };
        }

        public async Task<UploadImageResponse> UploadUserProfileImage(ImageUploadRequestModel model)
        {
            UploadImageResponse response = await _userService.UploadUserProfileImageAsync(model);
            return response;
        }
        public async Task<BaseResponse> ReCAPTCHAValidation(ReCAPTCHARequestModel model)
        {
            return await _userService.ReCAPTCHAValidation(model);
        }
        public async Task<UserDetailResponse> GetUserByPasswordChangeKey(string passwordChangeKey)
        {
            return await _userService.GetUserByPasswordChangeKey(passwordChangeKey);
        }
        public async Task<BaseResponse> EmailExist(string email)
        {
            return await _userService.EmailExist(email);
        }
        public async Task<LoginResultResponse> UpdateUserPassword(UpdatePasswordRequestModel model)
        {
            return await _userService.UpdateUserPassword(model);
        }
        public async Task<EmailNotificationResponse> SendForgotPasswordEmailNotification(ForgotPasswordEmailRequestModel requestModel)
        {
            return await _userService.SendForgotPasswordEmailNotification(requestModel);
        }
    }
}
