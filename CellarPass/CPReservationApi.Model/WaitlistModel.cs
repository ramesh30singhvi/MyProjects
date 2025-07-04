using System;
using System.Collections.Generic;

namespace CPReservationApi.Model
{
    public class CreateWaitlist
    {
        public int member_id { get; set; }
        public int event_id { get; set; }
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public int guest_count { get; set; }
        public int user_id { get; set; }
        public string event_name { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string mobile_phone { get; set; }
        public Common.Common.Notification_Preference notification_preference { get; set; }
        public string note { get; set; }
        public string note_internal { get; set; }
        public string event_date { get; set; }
        public string start_time { get; set; }

        public int affiliate_id { get; set; }

        public string concierge_note { get; set; }

    }

    public class UpdateWaitlist
    {
        public string id { get; set; }
        public int reservation_id { get; set; }
        public Common.Common.Waitlist_Status waitlist_status { get; set; }
        public int valid_minutes { get; set; }
        public bool send_notice { get; set; } = false;
    }

    public class Waitlist
    {
        public Waitlist()
        {
            address = new UserAddress();
        }
        public string id { get; set; }
        public Common.Common.Waitlist_Status waitlist_status { get; set; }
        public string waitlist_status_desc
        {
            get
            {
                string message = "";
                if (waitlist_status == Common.Common.Waitlist_Status.pending)
                    message = "Pending";
                else if (waitlist_status == Common.Common.Waitlist_Status.approved)
                    message = "Approved";
                else if (waitlist_status == Common.Common.Waitlist_Status.converted)
                    message = "Converted";
                else if (waitlist_status == Common.Common.Waitlist_Status.canceled)
                    message = "Cancelled";
                else if (waitlist_status == Common.Common.Waitlist_Status.noresponse)
                    message = "No Response";
                else if (waitlist_status == Common.Common.Waitlist_Status.expired)
                    message = "Expired";
                return message;
            }
        }

        public int member_id { get; set; }
        public int reservation_id { get; set; }
        public int event_id { get; set; }
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public int guest_count { get; set; }
        public string event_name { get; set; }
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string mobile_phone { get; set; }
        public int mobile_phone_status { get; set; }
        public string note { get; set; }
        public string note_internal { get; set; }
        public DateTime event_date_time { get; set; }
        public Common.Common.Notification_Preference notification_preference { get; set; }
        public DateTime created_date_time { get; set; }
        public DateTime status_date_time { get; set; }
        public DateTime? invited_date_time { get; set; }
        public int valid_minutes { get; set; }
        public string member_name { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string destination_name { get; set; }
        public string currency_symbol { get; set; }
        public string currency_code { get; set; }
        public decimal fee_Per_person { get; set; }
        public string work_phone { get; set; }
        public string member_url { get; set; }
        public int location_timezone { get; set; }
        public UserAddress address { get; set; }
        public int affiliate_id { get; set; }
        public string concierge_note { get; set; }
        public string event_location { get; set; }
        public string location_notification_email { get; set; }
    }

    public class WaitlistTPResponse : WaitlistTP
    {

        public UserNote account_note { get; set; }
        public int assigned_server_id { get; set; } = 0;
        public int assigned_floor_plan_id { get; set; } = 0;
        public string assigned_floor_plan_name { get; set; } = "";
        public string assigned_floor_plan_technical_name { get; set; } = "";
        public List<int> assign_table_ids { get; set; }
        public string floor_plan_name { get; set; } = "";
        public string floor_plan_technical_name { get; set; } = "";
        public string pre_assigned_server_first_name { get; set; } = "";
        public string pre_assigned_server_last_name { get; set; } = "";
        public string pre_assigned_server_color { get; set; } = "";
        public string assigned_server_first_name { get; set; } = "";
        public string assigned_server_last_name { get; set; } = "";
        public string assigned_server_color { get; set; } = "";
        public Common.Common.CustomerType customer_type { get; set; } = Common.Common.CustomerType.general;
        public string event_name { get; set; } = "";
        public string referred_by_first_name { get; set; } = "";
        public string referred_by_last_name { get; set; } = "";
    }

    public class WaitlistTP
    {
        public int id { get; set; }
        public Common.Common.Waitlist_Status status { get; set; }
        public int member_id { get; set; }
        public int location_id { get; set; }
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string mobile_phone { get; set; }
        public int mobile_phone_status { get; set; }
        public DateTime wait_start_time { get; set; }
        public int party_size { get; set; }
        public int wait_time_minutes { get; set; }
        public string guest_notes { get; set; }
        public string waitlist_notes { get; set; }
        public string tags { get; set; }
        public int visits_count { get; set; } = 0;
        public int cancellations_count { get; set; } = 0;
        public int no_shows_count { get; set; } = 0;
        public DateTime? seating_start_time { get; set; }
        public DateTime? seating_end_time { get; set; }
        public int? pre_assign_server_id { get; set; } = 0;
        public List<int> pre_assign_table_ids { get; set; }
        public int waitlist_index { get; set; }
        public int duration_in_minutes { get; set; }
        public int floor_plan_id { get; set; } = 0;
        public bool is_walk_in { get; set; } = false;
        public int event_id { get; set; } = 0;
        public int referred_by_id { get; set; } = 0;
    }

    public class UserNote
    {
        public string note { get; set; }
        public DateTime? note_date { get; set; }
        public string modified_by { get; set; }
    }
}
