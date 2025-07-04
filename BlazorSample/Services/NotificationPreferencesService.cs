using CellarPassAppAdmin.Client.Exceptions;
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
    public class NotificationPreferencesService : INotificationPreferencesService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public NotificationPreferencesService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<BusinessNotificationMembersResponse> AddUpdateBusinessNotificationPreferences(AddUpdateBusinessNotificationPreferences model)
        {
            try
            {
                BusinessNotificationMembersResponse response = await _apiManager.PostAsync<AddUpdateBusinessNotificationPreferences, BusinessNotificationMembersResponse>(_configuration["App:SettingsApiUrl"] + $"NotificationPreferences/add-update-business-notification-preferences", model);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessNotificationMembersResponse();
            }
        }

        public async Task<BusinessNotificationMembersResponse> GetBusinessNotificationMembers(int businessId, int? NotificationType = null)
        {
            try
            {
                BusinessNotificationMembersResponse response = await _apiManager.GetAsync<BusinessNotificationMembersResponse>(_configuration["App:SettingsApiUrl"] + $"NotificationPreferences/list?businessId={businessId}&NotificationType={NotificationType}");
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new BusinessNotificationMembersResponse();
            }
        }
    }
}
