using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class AffiliateResponse : BaseResponse
    {
        public AffiliateResponse()
        {
            data = new List<AffiliateModel>();
        }
        public List<AffiliateModel> data { get; set; }
    }
}
