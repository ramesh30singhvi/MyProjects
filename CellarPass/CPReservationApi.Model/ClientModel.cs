using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class ClientModel
    {
        public bool client_authorized { get; set; }
        public int client_id { get; set; }
        public string client_name { get; set; }
        public string sms_number { get; set; }
        public RsvpPlan rsvp_plan { get; set; }
        public TicketingPlan ticketing_plan { get; set; }
        public string ecom_provider { get; set; }
        public string club_partner { get; set; }
        public int inventory_mode { get; set; }
        public bool detailed_address_info_required { get; set; }
        public bool pos_enabled { get; set; }
        public bool cellar_scout_member { get; set; }
        public string payment_gateway { get; set; }
        public int payment_gateway_id { get; set; }
        public int private_rsvp_email_template_id { get; set; }
    }

    public class RsvpPlan
    {
        public int id { get; set; }
        public string plan_name { get; set; }
        public int? plan_type { get; set; }
    }

    public class TicketingPlan
    {
        public int id { get; set; }
        public string plan_name { get; set; }
    }

    public class UserSessionModel
    {
        public string token { get; set; }
        public double token_expiry { get; set; }
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int default_role_id { get; set; }
        public LoginStatus login_status { get; set; }
        public List<UserSessionWineryModel> user_wineries { get; set; }
    }

    public class UserSessionWineryModel
    {
        public int role_id { get; set; }
        public int winery_id { get; set; }
        public string customer_reference_number { get; set; }

    }
}
