using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class OpenTableLookupResponse : BaseResponse
    {
        public OpenTableLookupResponse()
        {
            data = new OpenTableMemberModel();
        }
        public OpenTableMemberModel data { get; set; }
    }


}
