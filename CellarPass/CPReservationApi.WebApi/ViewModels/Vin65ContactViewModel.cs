using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class Vin65ContactViewModel
    {
        public string vin65_id { get; set; }
        public string rms_id { get; set; }
        public string home_phone { get; set; }
        public DateTime date_modified { get; set; }
        public bool club_member_status { get; set; }
        public string email { get; set; }
        public string billing_street { get; set; }
        public string billing_city { get; set; }
        public string billing_state { get; set; }
        public string billing_zip { get; set; }
        public string country { get; set; }

    }

    public class ThirdPartyNoteResponse : BaseResponse
    {
        public ThirdPartyNoteResponse()
        {
            data = new List<AccountNote>();
        }
        public List<AccountNote>  data { get; set; }
    }
}
