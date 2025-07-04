using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Entities.v4;
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
    public class EventService : IEventService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public EventService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<EventCalendarHoursResponse> GetEventCalendarHoursAsync(int businessPropertyId)
        {
            try
            {
                EventCalendarHoursResponse response = await _apiManager.GetAsync<EventCalendarHoursResponse>(_configuration["App:SettingsApiUrl"] + "Event/get-event-calendar-hours/" + businessPropertyId);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new EventCalendarHoursResponse();
            }
        }

        public async Task<EventCalendarHoursResponse> AddUpdateEventCalendarHoursAsync(List<EventCalendarView> eventCalendarViews)
        {
            try
            {
                EventCalendarHoursResponse response = await _apiManager.PostAsync<List<EventCalendarView>, EventCalendarHoursResponse>(_configuration["App:SettingsApiUrl"] + "Event/add-update-event-calendar-hours", eventCalendarViews);
                return response;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new EventCalendarHoursResponse();
            }
        }
    }
}
