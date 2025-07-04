using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class TicketViewModel : ITicketViewModel
    {
        private ITicketService _ticketService;
        public TicketViewModel(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public async Task<List<TicketsOrderViewModel>> GetTicketOrders(int memberId, Guid userId)
        {
            List<TicketsOrderViewModel> orders = await _ticketService.GetTicketOrdersAsync(memberId, userId);
            return orders;
        }
    }
}
