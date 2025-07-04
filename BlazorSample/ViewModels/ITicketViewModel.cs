using CellarPassAppAdmin.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ITicketViewModel
    {
        Task<List<TicketsOrderViewModel>> GetTicketOrders(int memberId, Guid userId);
    }
}
