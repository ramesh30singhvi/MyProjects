using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class TicketingEventViewModel : ITicketingEventViewModel
    {
        private readonly ITicketingEventService _ticketingEventService;

        public TicketingEventViewModel(ITicketingEventService ticketingEventService)
        {
            _ticketingEventService = ticketingEventService;
        }

        public async Task<EventCategoriesResponse> GetEventCategories()
        {
            return await _ticketingEventService.GetEventCategories();
        }
    }
}
