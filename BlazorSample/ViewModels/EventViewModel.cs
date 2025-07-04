using CellarPassAppAdmin.Shared.Entities.v4;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class EventViewModel : IEventViewModel
    {
        private IEventService _eventCalendarViewService;

        public EventViewModel(IEventService eventCalendarViewService)
        {
            _eventCalendarViewService = eventCalendarViewService;
        }
        public async Task<EventCalendarHoursResponse> GetEventCalendarHours(int businessPropertyId)
        {
            return await _eventCalendarViewService.GetEventCalendarHoursAsync(businessPropertyId);
        }
        public async Task<EventCalendarHoursResponse> AddUpdateEventCalendarHours(List<EventCalendarView> eventCalendarViews)
        {
            return await _eventCalendarViewService.AddUpdateEventCalendarHoursAsync(eventCalendarViews);
        }
    }
}
