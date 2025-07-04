using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class RootObject
    {
        public string rootEntity { get; set; }
        public int count { get; set; }
        public List<string> header { get; set; }
        public List<List<string>> results { get; set; }
    }

    public class ReserveCloudReservation
    {
        public string function_name { get; set; }
        public string function_start_date { get; set; }
        public string function_type_name { get; set; }
        public string locations_name { get; set; }
        public string function_start_time { get; set; }
        public string function_end_time { get; set; }
        public string setup_minutes { get; set; }
        public string teardown_minutes { get; set; }
        public string setup_style { get; set; }
        public string function_attendance { get; set; }
        public string function_number { get; set; }
        public string site_name { get; set; }
        public string primary_contact_first_name { get; set; }
        public string primary_contact_last_name { get; set; }
        public string referral_type { get; set; }
        public string event_number { get; set; }
        public string event_salesperson_first_name { get; set; }
        public string event_salesperson_last_name { get; set; }
        public string event_status { get; set; }
        public string event_type { get; set; }
        public string event_start_date { get; set; }
        public string estimated_attendance { get; set; }
        public string event_name { get; set; }
        public string owner_first_name { get; set; }
        public string owner_last_name { get; set; }
        public string primary_contact_phone { get; set; }
        public string primary_contact_mobile { get; set; }
    }
}
