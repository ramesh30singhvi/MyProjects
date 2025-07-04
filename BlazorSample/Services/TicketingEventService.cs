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
    public class TicketingEventService : ITicketingEventService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public TicketingEventService(IApiManager apiManager,
            IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
        }

        public async Task<EventCategoriesResponse> GetEventCategories()
        {
            try
            {
                return await _apiManager.GetAsync<EventCategoriesResponse>("TicketingEvent/get-event-categories");
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new EventCategoriesResponse();
            }
        }
    }
}
