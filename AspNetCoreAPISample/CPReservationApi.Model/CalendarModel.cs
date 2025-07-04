using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class CalendarModel
    {
        public int id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public string title { get; set; }
        public string text { get; set; }
    }
}
