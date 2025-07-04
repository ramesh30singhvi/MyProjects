using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class LocationModel
    {
        public int location_id { get; set; }
        public string destination_name { get; set; }
        public string location_name { get; set; }
        public string technical_name { get; set; }
        public string description { get; set; }
        public Address address { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
        public int location_timezone { get; set; }
        public double location_timezone_offset { get; set; }
        public string seating_reset_time { get; set; }
        public int sort_order { get; set; } 
        public int server_mode { get; set; }
        public int room_width { get; set; }
        public int room_height { get; set; }
        public int member_id { get; set; }

        public bool is_primary { get; set; }
    }

    public class Address
    {
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
        public string country { get; set; }
    }

    public class LocationMapModel
    {
        public int location_id { get; set; }
        public string destination_name { get; set; }
        public string location_name { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
        public string map_and_directions_url { get; set; }
    }

    public class VenueLocation : Address
    {
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
    }

}
