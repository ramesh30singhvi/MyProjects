using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using CellarPassAppAdmin.Shared.Services.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public class TicketingPlanViewModel : ITicketingPlanViewModel
    {
        private readonly ITicketingPlanService _ticketingPlanService;

        public TicketingPlanViewModel(ITicketingPlanService ticketingPlanService)
        {
            _ticketingPlanService = ticketingPlanService;
        }

        public async Task<TicketingPlanResponse> AddUpdateTicketingPlan(TicketingPlanRequestModel requestModel)
        {
            return await _ticketingPlanService.AddUpdateTicketingPlan(requestModel);
        }
        public async Task<TicketingPlanListResponse> GetTicketingPlanList()
        {
            return await _ticketingPlanService.GetTicketingPlanList();
        }
        public async Task<TicketingPlanResponse> GetTicketingPlanById(Guid idGuid)
        {
            return await _ticketingPlanService.GetTicketingPlanById(idGuid);
        }
        public async Task<BaseResponse> DeleteTicketingPlan(Guid idGuid)
        {
            return await _ticketingPlanService.DeleteTicketingPlan(idGuid);
        }
    }
}
