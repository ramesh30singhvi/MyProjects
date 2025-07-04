using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class PrivateEventDetailsResponse : BaseResponse
    {
        public PrivateEventDetailsResponse()
        {
            data = new PrivateEventRequestDetails();
        }
        public PrivateEventRequestDetails data { get; set; }
    }

    public class PrivateEventFormSubmittedResponse : BaseResponse
    {
        public PrivateEventFormSubmittedResponse()
        {
            data = new PrivateEventFormSubmittedModel();
        }
        public PrivateEventFormSubmittedModel data { get; set; }
    }
}
