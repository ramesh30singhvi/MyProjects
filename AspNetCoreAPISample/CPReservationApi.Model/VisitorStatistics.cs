using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class VisitorStatistics
    {
        public int member_id { get; set; }
        public int profile_page_views { get; set; }

        public int ticket_event_views { get; set; }

        public int directory_profile_views { get; set; }
        public int profile_widget_views { get; set; }
        public int iframe_views { get; set; }
        public int ticket_order_views { get; set; }
        public int rsvp_detail_views { get; set; }

    }
}
