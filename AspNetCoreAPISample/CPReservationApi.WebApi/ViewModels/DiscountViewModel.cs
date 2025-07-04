using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class DiscountResponse : BaseResponse
    {
        public DiscountResponse()
        {
            data = new List<DiscountModel>();
        }
        public List<DiscountModel> data { get; set; }
    }
}
