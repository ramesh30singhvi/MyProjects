using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class NotificationPreferencesViewModel : INotificationPreferencesViewModel
    {
        private readonly INotificationPreferencesService _notificationPreferencesService;
        public NotificationPreferencesViewModel(INotificationPreferencesService notificationPreferencesService)
        {
            _notificationPreferencesService = notificationPreferencesService;
        }

        public async Task<BusinessNotificationMembersResponse> AddUpdateBusinessNotificationPreferences(AddUpdateBusinessNotificationPreferences model)
        {
            return await _notificationPreferencesService.AddUpdateBusinessNotificationPreferences(model);
        }

        public async Task<BusinessNotificationMembersResponse> GetBusinessNotificationMembers(int businessId, int? NotificationType = null)
        {
            return await _notificationPreferencesService.GetBusinessNotificationMembers(businessId, NotificationType);
        }
    }
}
