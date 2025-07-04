using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public EmailNotificationService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }
        public async Task<GetEmailNotificationResponse> GetEmailNotificationByCategoryId(int categoryId, int businessId)
        {
            try
            {
                return await _apiManager.GetAsync<GetEmailNotificationResponse>(_configuration["App:SettingsApiUrl"] + "EmailNotification/get-email-notifications/" + categoryId + "/" + businessId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetEmailNotificationResponse();
            }
        }
        public async Task<GetEmailNotificationDetailResponse> GetEmailNotificationDetail(Guid? idGUID = null, int? notificationType = null, int? categoryId = null)
        {
            try
            {
                return await _apiManager.GetAsync<GetEmailNotificationDetailResponse>(_configuration["App:SettingsApiUrl"] + "EmailNotification/get-email-notification-detail?idGUID=" + idGUID + "&notificationType=" + notificationType + "&categoryId=" + categoryId);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new GetEmailNotificationDetailResponse();
            }
        }
        public async Task<BusinessNotificationEmailListResponse> AddUpdateBusinessNotificationEmail(List<AddUpdateBusinessEmailRequestModel> requestModels)
        {
            try
            {
                return await _apiManager.PostAsync<BusinessNotificationEmailListResponse>(_configuration["App:SettingsApiUrl"] + "EmailNotification/add-update-business-notification-email", requestModels);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessNotificationEmailListResponse();
            }
        }
        public async Task<BusinessNotificationEmailResponse> GetBusinessNotificationEmailById(Guid idGuid)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessNotificationEmailResponse>(_configuration["App:SettingsApiUrl"] + "EmailNotification/get-business-notification-email-by-id/" + idGuid);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessNotificationEmailResponse();
            }
        }
        public async Task<BusinessNotificationEmailListResponse> GetBusinessNotificationEmails(int businessId, int? notificationEmailId = null, int? category = null, int? notificationType = null)
        {
            try
            {
                return await _apiManager.GetAsync<BusinessNotificationEmailListResponse>(_configuration["App:SettingsApiUrl"] + "EmailNotification/get-business-notification-emails/" + businessId + "?notificationEmailId=" + notificationEmailId + "&category=" + category + "&notificationType=" + notificationType);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessNotificationEmailListResponse();
            }
        }
        public async Task<BaseResponse> UpdateBusinessNotificationEmailStatus(Guid idGuid, bool active)
        {
            try
            {
                return await _apiManager.GetAsync<BaseResponse>(_configuration["App:SettingsApiUrl"] + "EmailNotification/update-business-notification-email-status/" + idGuid + "/" + active);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }

        #region CP Notification Email
        public async Task<AddNotificationEmailResponse> AddCPNotificationEmailTemplate(AddCPNotificationEmailRequestModel requestModel)
        {
            try
            {
                return await _apiManager.PostAsync<AddNotificationEmailResponse>(_configuration["App:SettingsApiUrl"] + "EmailNotification/add-notification-email-template", requestModel);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddNotificationEmailResponse();
            }
        }
        public async Task<AddNotificationEmailResponse> UpdateCPNotificationEmailTemplate(UpdateCPNotificationEmailRequestModel requestModel)
        {
            try
            {
                return await _apiManager.PostAsync<AddNotificationEmailResponse>(_configuration["App:SettingsApiUrl"] + "EmailNotification/update-notification-email-template", requestModel);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new AddNotificationEmailResponse();
            }
        }
        #endregion CP Notification Email
    }
}
