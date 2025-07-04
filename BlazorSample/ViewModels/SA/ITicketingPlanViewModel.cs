using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel.SA;
using CellarPassAppAdmin.Shared.Models.ViewModel.SA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels.SA
{
    public interface ITicketingPlanViewModel
    {
        Task<TicketingPlanResponse> AddUpdateTicketingPlan(TicketingPlanRequestModel requestModel);
        Task<TicketingPlanListResponse> GetTicketingPlanList();
        Task<TicketingPlanResponse> GetTicketingPlanById(Guid idGuid);
        Task<BaseResponse> DeleteTicketingPlan(Guid idGuid);
    }
}
