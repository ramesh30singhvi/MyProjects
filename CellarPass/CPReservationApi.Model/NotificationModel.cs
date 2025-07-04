using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class CreateDeltaRequest
    {
        public int item_type { get; set; }
        public int member_id { get; set; }
        public int item_id { get; set; }
        public int location_id { get; set; } = 0;
        public int status { get; set; } = 0;
        public DateTime action_date { get; set; }
        public int floor_plan_id { get; set; } = 0;
    }

    public class OpenDeviceSessionRequest
    {
        public string user_id { get; set; }
        public string device_id { get; set; }
        public int[] locations { get; set; }
        public string action_date { get; set; }
        public bool use_live_cert { get; set; }
    }

    public class OpenDeviceSessionRequestv2
    {
        public string user_id { get; set; }
        public string device_id { get; set; }
        public int[] floor_plans { get; set; }
        public string action_date { get; set; }
        public bool use_live_cert { get; set; }
    }

    public class CloseDeviceSessionRequest
    {
        public string device_id { get; set; }
    }

    public class DeviceSessionModel
    {
        public int id { get; set; }
        public string user_id { get; set; }
        public string device_id { get; set; }
        public DateTime date_updated { get; set; }
        public List<int> location_id { get; set; }
        public DateTime action_date { get; set; }
        public bool use_live_cert { get; set; }
    }

    public class DeviceSessionModelV2
    {
        public int id { get; set; }
        public string user_id { get; set; }
        public string device_id { get; set; }
        public DateTime date_updated { get; set; }
        public List<int> floor_plan_id { get; set; }
        public DateTime action_date { get; set; }
        public bool use_live_cert { get; set; }
    }

    public class DeltaModel
    {
        public int item_type { get; set; }
        public int member_id { get; set; }
        public int location_id { get; set; }
    }

    public class UpdateDeltaRequest
    {
        public string device_id { get; set; }
        public string log_msg { get; set; }
        public int max_delta_id { get; set; }
    }

    public class AvailableNotificationsModel
    {
        public string device_id { get; set; }
        public string item_type { get; set; }
        public bool use_live_cert { get; set; }
        public string rsvps { get; set; }
        public string waitlist { get; set; }
        public int max_delta_id { get; set; }
        public int app_type { get; set; }
        public string chat_rsvps { get; set; }
        public string chat_waitlists { get; set; }
        public string floor_plan_ids { get; set; } = string.Empty;
        public string location_ids { get; set; } = string.Empty;
    }

}
