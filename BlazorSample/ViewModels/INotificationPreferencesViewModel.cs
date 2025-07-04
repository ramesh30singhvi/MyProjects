using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface INotificationPreferencesViewModel
    {
        Task<BusinessNotificationMembersResponse> GetBusinessNotificationMembers(int businessId, int? NotificationType = null);
        Task<BusinessNotificationMembersResponse> AddUpdateBusinessNotificationPreferences(AddUpdateBusinessNotificationPreferences model);
    }
}
