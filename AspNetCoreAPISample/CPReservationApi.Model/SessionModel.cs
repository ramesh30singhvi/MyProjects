using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class SessionModel
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public DateTime session_datetime { get; set; }
        public string color { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }
}
