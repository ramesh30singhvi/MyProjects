using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPReservationApi.Model;

namespace CPReservationApi.WebApi.ViewModels
{
    public class FloorPlanResponse : BaseResponse
    {
        public FloorPlanResponse()
        {
            data = new List<FloorPlanModel>();
        }
        public List<FloorPlanModel> data { get; set; }
    }

}
