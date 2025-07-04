using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ILoginViewModel
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        public Task<LoginResultResponse> LoginUser();
        Task<UserDetailResponse> GetUserByEmail(string email);
        Task<UploadImageResponse> UploadUserProfileImage(ImageUploadRequestModel model);
        Task<BaseResponse> ReCAPTCHAValidation(ReCAPTCHARequestModel model);
        Task<UserDetailResponse> GetUserByPasswordChangeKey(string passwordChangeKey);
        Task<BaseResponse> EmailExist(string email);
        Task<LoginResultResponse> UpdateUserPassword(UpdatePasswordRequestModel model);
        Task<EmailNotificationResponse> SendForgotPasswordEmailNotification(ForgotPasswordEmailRequestModel requestModel);
    }
}
