using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IEventViewModel
    {
        Task<EventCalendarHoursResponse> GetEventCalendarHours(int businessPropertyId);
        Task<EventCalendarHoursResponse> AddUpdateEventCalendarHours(List<EventCalendarView> eventCalendarViews);
    }
}
