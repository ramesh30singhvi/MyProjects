using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class ReservationV2WaitlistResponse : BaseResponse
    {
        public ReservationV2WaitlistResponse()
        {
            data = new SaveReservationV2Waitlist();
        }
        public SaveReservationV2Waitlist data { get; set; }
    }

    public class SaveReservationV2Waitlist
    {
        public string id { get; set; }
    }

    public class GetReservationV2WaitlistResponse : BaseResponse
    {
        public GetReservationV2WaitlistResponse()
        {
            data = new Waitlist();
        }
        public Waitlist data { get; set; }
    }

    public class GetListReservationV2WaitlistResponse : BaseResponse
    {
        public GetListReservationV2WaitlistResponse()
        {
            data = new List<Waitlist>();
        }
        public List<Waitlist> data { get; set; }
    }

    public class CreateTPWaitListResponse : BaseResponse
    {
        public CreateTPWaitListResponse()
        {
            data = new WaitlistTPResponse();
        }
        public WaitlistTPResponse data { get; set; }
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
    }


}
