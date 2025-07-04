using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{

    public class GuestDashboardResponse : BaseResponse
    {
        public GuestDashboardResponse()
        {
            data = new GuestDashboardModel();
        }
        public GuestDashboardModel data { get; set; }
    }


}
