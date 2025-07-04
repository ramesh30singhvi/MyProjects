using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class RsvpConfirmationEmailTemplateResponse : BaseResponse
    {
        public RsvpConfirmationEmailTemplateResponse()
        {
            data = new List<RsvpConfirmationEmailTemplateModel>();
        }
        public List<RsvpConfirmationEmailTemplateModel> data { get; set; }
    }
}
