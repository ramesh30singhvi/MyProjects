using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class FloorPlanViewModel:IFloorPlanViewModel
    {
        private IFloorPlanService _floorPlanService;
        public FloorPlanViewModel(IFloorPlanService floorPlanService)
        {
            _floorPlanService = floorPlanService;
        }
        public async Task<List<FloorPlanModel>> GetFloorPlan(int memberId,bool active_only)
        {
            List<FloorPlanModel> user = await _floorPlanService.GetFloorPlanAsync(memberId, active_only);
            return user;
        }
        public async Task<List<SeatingReportTagModel>> GetTags(int memberId)
        {
            List<SeatingReportTagModel> user = await _floorPlanService.GetTagsAsync(memberId);
            return user;
        }
    }
}
