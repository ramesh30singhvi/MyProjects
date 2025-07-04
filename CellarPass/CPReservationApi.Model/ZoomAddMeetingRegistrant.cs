using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class CustomQuestion
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class ZoomAddMeetingRegistrantRequest
    {
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        //public string address { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        //public string state { get; set; }
        public string phone { get; set; }
        //public string industry { get; set; }
        //public string org { get; set; }
        //public string job_title { get; set; }
        //public string purchasing_time_frame { get; set; }
        //public string role_in_purchase_process { get; set; }
        //public string no_of_employees { get; set; }
        //public string comments { get; set; }
        //public IList<CustomQuestion> custom_questions { get; set; }
    }

    public class ZoomAddMeetingRegistrantResponse
    {
        public long id { get; set; }
        public string join_url { get; set; }
        public string registrant_id { get; set; }
        public string start_time { get; set; }
        public string topic { get; set; }
    }

    public class Registrant
    {
        public string id { get; set; }
        public string email { get; set; }
    }

    public class ZoomMeetingRegistrantStatusRequest
    {
        public string action { get; set; }
        public IList<Registrant> registrants { get; set; }
    }

}
