using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class Recurrence
    {
        public string type { get; set; }
        public string repeat_interval { get; set; }
        public string weekly_days { get; set; }
        public string monthly_day { get; set; }
        public string monthly_week { get; set; }
        public string monthly_week_day { get; set; }
        public string end_times { get; set; }
        public string end_date_time { get; set; }
    }

    public class SettingsRequest
    {
        //public string host_video { get; set; }
        //public string participant_video { get; set; }
        //public string cn_meeting { get; set; }
        //public string in_meeting { get; set; }
        //public string join_before_host { get; set; }
        //public string mute_upon_entry { get; set; }
        //public string watermark { get; set; }
        public bool use_pmi { get; set; } = false;
        public int approval_type { get; set; } = 0;
        //public string registration_type { get; set; }
        //public string audio { get; set; }
        //public string auto_recording { get; set; }
        //public string enforce_login { get; set; }
        //public string enforce_login_domains { get; set; }
        //public string alternative_hosts { get; set; }
        //public IList<string> global_dial_in_countries { get; set; }
        public bool registrants_email_notification { get; set; } = true;
    }

    public class ZoomCreateMeetingRequest
    {
        public string topic { get; set; }
        public int type { get; set; } = 2;
        public string start_time { get; set; }
        public int duration { get; set; }
        // public string timezone { get; set; }
        public string password { get; set; } = "";
        public string agenda { get; set; }
        //public Recurrence recurrence { get; set; }
        public SettingsRequest settings { get; set; }
    }

    public class GlobalDialInNumber
    {
        public string city { get; set; }
        public string country { get; set; }
        public string country_name { get; set; }
        public string number { get; set; }
        public string type { get; set; }
    }

    public class SettingsResponse
    {
        public string alternative_hosts { get; set; }
        public int approval_type { get; set; }
        public string audio { get; set; }
        public string auto_recording { get; set; }
        public bool close_registration { get; set; }
        public bool cn_meeting { get; set; }
        public bool enforce_login { get; set; }
        public string enforce_login_domains { get; set; }
        public IList<string> global_dial_in_countries { get; set; }
        public IList<GlobalDialInNumber> global_dial_in_numbers { get; set; }
        public bool host_video { get; set; }
        public bool in_meeting { get; set; }
        public bool join_before_host { get; set; }
        public bool mute_upon_entry { get; set; }
        public bool participant_video { get; set; }
        public bool registrants_confirmation_email { get; set; }
        public bool use_pmi { get; set; }
        public bool waiting_room { get; set; }
        public bool watermark { get; set; }
        public bool registrants_email_notification { get; set; }
    }

    public class ZoomCreateMeetingResponse
    {
        public DateTime created_at { get; set; }
        public int duration { get; set; }
        public string host_id { get; set; }
        public long id { get; set; }
        public string join_url { get; set; }
        public SettingsResponse settings { get; set; }
        public DateTime start_time { get; set; }
        public string start_url { get; set; }
        public string status { get; set; }
        public string timezone { get; set; }
        public string topic { get; set; }
        public int type { get; set; }
        public string uuid { get; set; }
    }
}
