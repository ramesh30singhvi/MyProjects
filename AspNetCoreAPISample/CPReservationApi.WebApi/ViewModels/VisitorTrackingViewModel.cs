using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class VisitorTrackingResponse : BaseResponse
    {
        public VisitorTrackingResponse()
        {
            data = new VisitorStatistics();
        }
        public VisitorStatistics data { get; set; }
    }


}
