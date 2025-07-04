using CellarPassAppAdmin.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IFloorPlanViewModel
    {
        public Task<List<FloorPlanModel>> GetFloorPlan(int memberId ,bool active_only);
        public Task<List<SeatingReportTagModel>> GetTags(int memberId);
    }
}
