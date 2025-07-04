using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class LocationResponse : BaseResponse
    {
        public LocationResponse()
        {
            data = new List<LocationModel>();
        }
        public List<LocationModel> data { get; set; }
    }
}
