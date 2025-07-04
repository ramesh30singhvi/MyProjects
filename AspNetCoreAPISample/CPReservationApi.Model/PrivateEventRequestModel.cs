using System;
using System.Collections.Generic;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Model
{
    public class PrivateEventFormSubmittedModel
    {
        public int id { get; set; }
        public string private_event_guid { get; set; }
    }

    public class PrivateEventRequest
    {
        public int member_id { get; set; }
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string country { get; set; }
        public string phone_number { get; set; }
        public string preferred_date { get; set; }
        public string preferred_start_time { get; set; }
        public int preferred_visit_duration { get; set; }
        public int guest { get; set; }
        public int reason_for_visit { get; set; }
        public string details { get; set; }
        public string captcha_response { get; set; }
    }

    public class PrivateEventRequestDetails
    {
        public int id { get; set; }
        public string private_event_guid { get; set; }
        public int member_id { get; set; }
        //public string member_name { get; set; }
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string country { get; set; }
        public string phone_number { get; set; }
        public DateTime preferred_date { get; set; }
        public TimeSpan preferred_start_time { get; set; }
        public int preferred_visit_duration { get; set; }
        public string preferred_visit_duration_desc { get; set; }
        public int guest { get; set; }
        public int reason_for_visit { get; set; }
        public string reason_for_visit_desc { get; set; }
        public string details { get; set; }
        public string member_name { get; set; }
        public bool show_private_request_content { get; set; }
        public string private_booking_request_email { get; set; }
    }

    public class EventsUpdateRequest
    {
        public int EventId { get; set; }
        public int OldLocationId { get; set; }
        public int OldStatus { get; set; }
        public string OldStartDate { get; set; }
        public string OldEndDate { get; set; }
        public bool OldIgnoreHolidays { get; set; }
        public int OldDurationMinutes { get; set; }
        public bool OldLimitByMaxOrders { get; set; }
        public bool OldLimitByCapacity { get; set; }
        public int OldDepartmentId { get; set; }
        public int OldItemType { get; set; }
        public int OldTaxClass { get; set; }
    }
}
