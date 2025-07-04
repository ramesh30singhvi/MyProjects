using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class TempQueueDataModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public string EventOrganizerName { get; set; }
        public string EventOrganizerPhone { get; set; }
        public string EventOrganizerEmail { get; set; }
        public DateTime StartDateTime { get; set; }
        public Common.Times.TimeZone TimeZoneId { get; set; }
        public string GuestName { get; set; }
        public string GuestEmailAddress { get; set; }
        public string ContactReason { get; set; }
        public string ContactMessage { get; set; }
        public int member_id { get; set; }
    }

    public class ContactEventOrganizerEmailRequest
    {
        public int event_id { get; set; }
        public string guest_name { get; set; }
        public string guest_email_address { get; set; }
        public string contact_reason { get; set; }
        public string contact_message { get; set; }
    }

    public class UnsubscribeEventRequest
    {
        public string q { get; set; }
    }
}
