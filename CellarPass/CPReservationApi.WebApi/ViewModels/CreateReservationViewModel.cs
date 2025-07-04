using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateReservationRequest
    {
        public int reservation_id { get; set; }
        public int member_id { get; set; }
        public int? event_id { get; set; }
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public string event_name { get; set; }
        public int location_id { get; set; }
        public string location_name { get; set; }
        public string event_start_date { get; set; }
        public string event_end_date { get; set; }
        public decimal fee_per_person { get; set; }
        public int fee_type { get; set; }
        public int charge_fee { get; set; }
        public int total_guests { get; set; }
        public string guest_note { get; set; }
        public int referral_type { get; set; }
        public int? booked_by_id { get; set; }
        public string booked_by_name { get; set; }
        public string concierge_note { get; set; }
        public int affiliate_id { get; set; }
        public int referral_id { get; set; }
        public int email_content_id { get; set; }
        public string hdyh { get; set; }
        public string internal_note { get; set; }
        public string discount_code { get; set; }
        public decimal discount_amount { get; set; }
        public bool is_club_discount { get; set; }
        public string personal_message { get; set; }
        public List<Additional_guests> additional_guests { get; set; }
        public UserDetailViewModel user_detail { get; set; }
        public PayCard pay_card { get; set; }
        public List<Reservation_Addon> reservation_addon { get; set; }
        public bool force_rsvp { get; set; } = false;
        public bool ignore_sales_tax { get; set; }
        public string account_notes { get; set; }
        public bool account_notes_have_changes { get; set; }
        public string tags { get; set; } = "";
        public int? pre_assign_server_id { get; set; }
        public List<int> pre_assign_table_ids { get; set; }
        public int? cancel_lead_time { get; set; }
        public int? transportation_id { get; set; }
        public string transportation_name { get; set; } = string.Empty;
        public decimal gratuity_percentage { get; set; }
        public bool tax_gratuity { get; set; }
        public decimal gratuity_total { get; set; }
        public List<AttendeeQuestion> attendee_questions { get; set; }
        public List<string> activation_codes { get; set; }
        public string waitlist_guid { get; set; }
        public string cust_id { get; set; }
        public Guid cart_guid { get; set; } = default(Guid);
        public bool send_guest_email { get; set; } = true;
        public bool send_affiliate_email { get; set; } = true;
        public string modified_by_name { get; set; }
        public string access_code { get; set; }
        public int floor_plan_id { get; set; }
        public bool send_guest_sms { get; set; }
        public bool subscribe_marketing_optin { get; set; }
        public bool over_book { get; set; } = false;
        public bool ignore_discount { get; set; } = false;
        public int status { get; set; } = 0;
        public bool cellarPass_marketing_optin { get; set; } = false;
        public bool ignore_club_member { get; set; } = false;
    }

    public class Additional_guests
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; } = "";
    }

    public class UserDetailViewModel
    {
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int customer_type { get; set; }
        public string phone_number { get; set; }
        public UserAddress address { get; set; }
        public int region_most_visited { get; set; }
    }

    public class UserAddress
    {
        public string city { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
        public string country { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
    }
    //public class PayCard
    //{
    //    public string number { get; set; }
    //    public string cust_name { get; set; }
    //    public string exp_month { get; set; }
    //    public string exp_year { get; set; }
    //    public string cvv2 { get; set; }
    //    public string card_token { get; set; }
    //}

    public class Reservation_Addon
    {
        public int qty { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public decimal cost { get; set; }
        public decimal price { get; set; }
        public bool Taxable { get; set; }
        public int category { get; set; }
        public int item_type { get; set; }
        public string image { get; set; }
        public int item_id { get; set; }
        public int group_item_id { get; set; }
        public int group_id { get; set; }
        public bool calculate_gratuity { get; set; }
        public decimal? addon_price_manual { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CreateReservationResponse : BaseResponse
    {
        public CreateReservationResponse()
        {
            data = new CreateReservationResponseModel();
        }
        public CreateReservationResponseModel data { get; set; }
    }


    public class CreateReservationResponseModel
    {
        public int reservation_id { get; set; }
        public string booking_code { get; set; }

        public string booking_guid { get; set; }
        public Common.SaveType save_type { get; set; }
        public string message { get; set; }
        public string payment_message { get; set; }
    }
}
