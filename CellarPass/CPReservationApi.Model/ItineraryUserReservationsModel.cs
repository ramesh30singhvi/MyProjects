using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class ItineraryUserReservationsModel
    {
        public int reservation_id { get; set; }
        public string member_name { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime event_end_date { get; set; }
        public string booking_code { get; set; }
        public string member_business_phone { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string address1 { get; set; }
    }
}
