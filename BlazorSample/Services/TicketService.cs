using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class TicketService : ITicketService
    {
        private readonly IApiManager _apiManager;

        public TicketService(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }

        public async Task<List<TicketsOrderViewModel>> GetTicketOrdersAsync(int memberId, Guid userId)
        {
            try
            {
                List<TicketsOrderViewModel> orders = await _apiManager.GetAsync<List<TicketsOrderViewModel>>("ticket/orders/" + memberId + "/" + userId);
                return orders;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<TicketsOrderViewModel>();
            }
        }
    }
}
