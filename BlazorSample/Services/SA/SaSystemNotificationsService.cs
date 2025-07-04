using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services.SA
{
    public class SaSystemNotificationsService : ISaSystemNotificationsService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public SaSystemNotificationsService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<CPNotificationListResponse> GetCPNotificationListAsync(int? id, Guid? idGuid, int? notificationType)
        {
            try
            {
                return await _apiManager.GetAsync<CPNotificationListResponse>("SaSystemNotifications/get-cp-notification-list?id=" + id + "&idGuid=" + idGuid + "&notificationType=" + notificationType);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPNotificationListResponse();
            }
        }
        public async Task<CPNotificationResponse> AddUpdateCPNotificationAsync(CPNotificationRequestModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CPNotificationResponse>("SaSystemNotifications/add-update-cp-notification", model);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CPNotificationResponse();
            }
        }
        public async Task<BaseResponse> SendPreviewEmailToBusQueue(PreviewEmailRequestModel requestModel)
        {
            try
            {
                return await _apiManager.PostAsync<BaseResponse>("SaSystemNotifications/send-preview-email", requestModel);
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BaseResponse();
            }
        }
    }
}
