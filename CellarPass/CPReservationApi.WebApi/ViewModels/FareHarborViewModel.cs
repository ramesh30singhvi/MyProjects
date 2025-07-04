using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPReservationApi.Model;

namespace CPReservationApi.WebApi.ViewModels
{
    public class FareharborLookupResponse : BaseResponse
    {
        public FareharborLookupResponse()
        {
            data = new FareharborModel();
        }
        public FareharborModel data { get; set; }
    }

    public class FareharborCompanyItemLookupResponse : BaseResponse
    {
        public FareharborCompanyItemLookupResponse()
        {
            data = new FareharborCompanyItem();
        }
        public FareharborCompanyItem data { get; set; }
    }

    public class FareharborAvailabilityResponse : BaseResponse
    {
        public FareharborAvailabilityResponse()
        {
            data = new AvailabilityModel();
        }
        public AvailabilityModel data { get; set; }
    }
}
