using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class FloorPlanModel
    {
        public int floor_plan_id { get; set; }
        public string plan_name { get; set; }
        public int location_id { get; set; }
        public string location_name { get; set; }
        public int plan_width { get; set; }
        public int plan_height { get; set; }
        public string seating_reset_time { get; set; }
        public int member_id { get; set; }
        public bool is_active { get; set; }
        public int sort_order { get; set; }
        public string technical_name { get; set; }
    }

    public class FloorPlanLocation
    {
        public int location_id { get; set; }
        public int floor_plan_id { get; set; }
    }
}
