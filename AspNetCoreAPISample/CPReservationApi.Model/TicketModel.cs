using CPReservationApi.Common;
using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using static CPReservationApi.Common.Common;
using System.Text.RegularExpressions;

namespace CPReservationApi.Model
{
    public class TicketEventModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        //public int location_id { get; set; }
        public Times.TimeZone timezone { get; set; }
        public double timezone_offset { get; set; }
        public Common.Common.TicketsEventStatus status { get; set; }
        public string status_description { get; set; }
        public Common.Common.AttendeeAppCheckInMode checkin_type { get; set; }
        public string checkin_type_description { get; set; }
        public bool checkin_allowed { get; set; }
        public int event_type { get; set; }
        public int total_tickets_sold { get; set; }
        public int event_capacity { get; set; }
        public int event_remaining_qty
        {
            get
            {
                return event_capacity - total_tickets_sold;
            }
        }
        public bool event_ended { get; set; } = false;
        public bool show_upcoming_events { get; set; } = false;
        public string city_url { get; set; }
        //public List<LocationModel> locations { get; set; }
        public List<ParticipatingLocations> participating_members { get; set; }
        public bool guest_list_enabled { get; set; } = false;
        public Common.Common.AttendanceModeStatus attendance_mode { get; set; }
        public DateTime membership_start_date { get; set; }
        public DateTime membership_end_date { get; set; }
        public string event_organizer_name { get; set; }
        public string event_image { get; set; }
        public string event_image_full_path { get; set; }
        public string venue_address_1 { get; set; }
        public string venue_address_2 { get; set; }
        public string venue_country { get; set; }
        public string venue_city { get; set; }
        public string venue_state { get; set; }
        public string venue_zip { get; set; }
        public string venue_latitude { get; set; }
        public string venue_longitude { get; set; }
        public List<int> regions_ids { get; set; }
        public List<string> tags { get; set; }
        public int primary_category { get; set; }
        public int secondary_category { get; set; }
        public TicketRefundPolicy refund_policy { get; set; }
        public string refund_policy_text { get; set; }
        public RefundServiceFeesOption refund_service_fees_option { get; set; }
        public string refund_service_fees_option_text { get; set; }
        public bool collect_tax { get; set; } = false;
        public List<int> tax_ticketlevels { get; set; }
        public string order_special_instructions { get; set; }
        public string confirmation_page { get; set; }
        public string business_message { get; set; }
        public List<int> internal_notification_recipient { get; set; }
        public bool sold_out { get; set; }
        public bool waitlist_available { get; set; }
        public bool require_reservations { get; set; }
        public bool disable_travel_time_restrictions { get; set; }
        public DateTime? last_ticket_sales_end_in { get; set; }
        public decimal min_price { get; set; }
        public decimal max_price { get; set; }
        public string price_range { get; set; }
        public string event_organizer_url { get; set; }
        public bool disable_book_itinerary_msg { get; set; }
        public int cta_button { get; set; }
    }

    public class ProfileTicketEventModel : TicketEventModel
    {
        public string short_description { get; set; }
        public string long_description { get; set; }

        public string event_url { get; set; }

    }

    public class TicketEventDetail2Model
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public string event_organizer_name { get; set; }
        public string event_image { get; set; }
        public string event_image_full_path { get; set; }
        public string venue_country { get; set; }
        public string venue_city { get; set; }
        public string venue_state { get; set; }
        public string venue_zip { get; set; }
        public string short_description { get; set; }
        public string event_url { get; set; }
    }

    public class ParticipatingLocations
    {
        public int member_id { get; set; }
        public string member_name { get; set; }
    }

    public class TicketEventDetailModel : TicketEventModel
    {
        public Common.Common.TicketsServiceFeesOption service_fee_option { get; set; }

        public string short_description { get; set; }

        public string long_description { get; set; }

        public string member_name { get; set; }
        public int member_id { get; set; }
        public string event_image_big { get; set; }
        public string event_image_big_full_path { get; set; }
        public bool requires_invite { get; set; }
        public bool requires_password { get; set; }
        public bool is_private { get; set; }

        public int email_template_id { get; set; }
        public int purchase_policy_id { get; set; }

        public int purchase_timeout { get; set; }

        public int waitlist_expiration { get; set; }
        //public bool self_print_enabled { get; set; }

        //public bool will_call_enabled { get; set; }

        public List<LocationModel> will_call_locations { get; set; }
        public List<TicketsEventWillCallLocation> will_call_location_details { get; set; }
        public bool show_guest_list { get; set; }

        public string guest_lists { get; set; }

        public bool show_discount_code { get; set; }
        public bool is_automated_discounts { get; set; }

        public bool show_access_code { get; set; }

        public bool show_will_call_adress { get; set; }

        public bool require_hdyh { get; set; }

        public TicketHolderConfig ticket_holder_config { get; set; }

        public TicketPostCaptureConfig post_capture_config { get; set; }

        public string member_url { get; set; }

        public string timezone_name { get; set; }
        public bool sold_out { get; set; }
        public bool waitlist_available { get; set; }
        public bool available_tickets { get; set; }
        public string ticket_cancel_policy { get; set; }
        public string event_attendee_policy { get; set; }
        public string event_url { get; set; }
        public bool is_cp_payment_gateway { get; set; }
        public string event_organizer_name { get; set; }

        public string event_organizer_email { get; set; }
        public string event_organizer_phone { get; set; }
        public string event_organizer_url { get; set; }
        public string region_name { get; set; }
        public string state { get; set; }

        public List<int> accepted_card_types { get; set; }

        public bool show_organizer_phone { get; set; }

        public bool show_map { get; set; }

        public bool show_book_rsvp_btn { get; set; }

        public string rsvp_booking_short_desc { get; set; }

        public string rsvp_booking_long_desc { get; set; }
        public bool block_repeat_bookings { get; set; }
        public int limit_bookings_per_day { get; set; }

        public bool show_availability_btn { get; set; }
        public bool disable_activate_button { get; set; }
        public List<TicketQuestion> attendee_questions { get; set; }
        public List<AdditionalContent> additional_content { get; set; }

        public int review_stars { get; set; }
        public decimal avg_review_value { get; set; }
        public int total_reviews { get; set; }

        public bool show_marketing_optin { get; set; }
        public string marketing_optin_text { get; set; }
    }


    public class PassportEventDetailModel : TicketEventDetailModel
    {
        public List<PassportParticipatingMemberModel> passport_members { get; set; }
    }

    public class TicketHolderConfig : TicketConfig
    {
        public bool require_mem_num { get; set; }

        public bool require_address { get; set; }
    }

    public class TicketPostCaptureConfig : TicketConfig
    {
        public bool show_zipcode { get; set; }
        public bool require_zipcode { get; set; }

        public bool show_age_group { get; set; }
        public bool require_age_group { get; set; }

        public bool require_address { get; set; }

    }


    public class PostCaptureParser
    {
        public bool show_flag { get; set; }

        public bool require_flag { get; set; }
    }


    public class TicketConfig
    {

        public bool show_dob { get; set; }
        public bool require_dob { get; set; }

        public bool require_age { get; set; }

        public bool require_website { get; set; }

        public bool show_firstname { get; set; }

        public bool require_firstname { get; set; }

        public bool show_lastname { get; set; }

        public bool require_lastname { get; set; }

        public bool show_company { get; set; }

        public bool require_company { get; set; }

        public bool show_title { get; set; }

        public bool require_title { get; set; }

        public bool show_email { get; set; }

        public bool require_email { get; set; }
        public bool unique_email { get; set; }

        public bool show_gender { get; set; }

        public bool require_gender { get; set; }

        public bool show_workphone { get; set; }

        public bool require_workphone { get; set; }

        public bool show_mobilephone { get; set; }

        public bool require_mobilephone { get; set; }
        public bool show_zipcode { get; set; }

        public bool require_zipcode { get; set; }

        public List<int> ticket_levels { get; set; }

        public bool requires_21 { get; set; } = false;

        public bool show_age_group { get; set; }
        public bool require_age_group { get; set; }
    }

    public class TicketEventMetrics
    {
        public int total_qty_available { get; set; }
        public int total_qty_sold { get; set; }
        public int total_qty_checkedin { get; set; }
        public int total_capacity { get; set; }
        public decimal total_ticket_sales { get; set; }
        public decimal revenue { get; set; }
        public List<TicketMetrics> ticket_metrics { get; set; }
    }

    public class TicketMetrics
    {
        public int ticket_id { get; set; }
        public string ticket_name { get; set; }
        public int qty_available { get; set; }
        public int qty_sold { get; set; }
        public int qty_checkedin { get; set; }      
    }

    public class TicketPassportEventMetrics
    {
        public int total_qty_available { get; set; }
        public int total_qty_sold { get; set; }
        public int total_qty_checkedin { get; set; }
        public List<TicketPassportMetrics> ticket_metrics { get; set; }
    }

    public class TicketPassportMetrics
    {
        public int member_id { get; set; }
        public string member_name { get; set; }
        public int qty_checkedin { get; set; }
        public DateTime last_checkin { get; set; }
    }

    public class TicketModel
    {
        public int ticket_id { get; set; }
        public int ticket_order_id { get; set; }
        public string ticket_number { get; set; }
        public string ticket_barcode { get; set; }
        public int ticket_level_id { get; set; }
        public string ticket_name { get; set; }
        public decimal ticket_price { get; set; }
        public Common.Common.TicketStatus ticket_status { get; set; }
        public string ticket_status_description { get; set; }
        public TicketHolder ticket_holder { get; set; }
        public DateTime valid_start_date { get; set; }
        public DateTime valid_end_date { get; set; }
        public int event_id { get; set; }
        public string event_title { get; set; }
        public int user_id { get; set; }
        public Common.Common.CheckInStatus checkin_status { get; set; }
        public string checkin_status_desc
        {
            get
            {
                return Enum.GetName(typeof(Common.Common.CheckInStatus), checkin_status);
            }
        }

        public string checkin_status_message
        {
            get
            {
                string message = "NA";
                if (checkin_status == Common.Common.CheckInStatus.NA)
                    message = "NA";
                else if (checkin_status == Common.Common.CheckInStatus.SUCCESS)
                    message = "Check-in Successful";
                else if (checkin_status == Common.Common.CheckInStatus.NOT_ALLOWED)
                    message = "Ticket already checked-in or not allowed";
                else if (checkin_status == Common.Common.CheckInStatus.FAILED)
                    message = "Error with Check-in";
                else if (checkin_status == Common.Common.CheckInStatus.NOT_ALLOWED_BAD_EVENT)
                    message = "Ticket not valid for this event";
                else if (checkin_status == Common.Common.CheckInStatus.NOT_ALLOWED_BAD_DATE)
                    message = "Ticket not valid on this date";
                return message;
            }
        }

        public TicketCheckIn previous_checkin { get; set; }
        public Common.Common.TicketPostCaptureStatus ticket_post_capture_status { get; set; }
        public Common.Common.AttendanceModeStatus attendance_mode { get; set; }
    }

    public class TicketPostCaptureRequest
    {
        public int ticket_order_id { get; set; }
        public int ticket_order_ticket_id { get; set; }
        public int member_id { get; set; }
        public TicketHolder ticket_holder { get; set; }

        public List<TicketQandA> attendee_questions { get; set; }
    }

    public class TicketQandA
    {
        public string question_text { get; set; }
        public string choice { get; set; }
    }

    public class TicketPostCaptureInvite
    {
        public int ticket_order_id { get; set; }
        public int ticket_order_ticket_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
    }
    public class TicketLevelModel
    {
        public int ticket_id { get; set; }
        public string ticket_name { get; set; }
        public string ticket_name_on_badge { get; set; }
        public decimal price { get; set; }
        public decimal original_price { get; set; }
        public string ticket_desc { get; set; }
        public int tickets_event_id { get; set; }
        public Common.Common.TicketsSaleStatus sale_status { get; set; }
        public DateTime valid_start_date { get; set; }
        public DateTime valid_end_date { get; set; }
        public DateTime date_time_available { get; set; }
        public DateTime date_time_unavailable { get; set; }
        public int max_available { get; set; }
        public int min_per_order { get; set; }
        public int max_per_order { get; set; }
        public int show_remaining { get; set; }
        public bool waitlist_enabled { get; set; }
        public bool waitlist_limit_enabled { get; set; }
        public int waitlist_limit { get; set; }
        public int sort_order { get; set; }
        public Common.Common.TicketType ticket_type { get; set; }
        //public bool charge_tax { get; set; }
        public bool self_print_enabled { get; set; }
        public bool will_call_enabled { get; set; }
        public bool shipped_enabled { get; set; }
        public decimal gratuity_percentage { get; set; }
        public bool tax_gratuity { get; set; }
        public Common.Common.TimeZone time_zone_id { get; set; }
        public int qty_sold { get; set; }
        public int waitlist_qty_sold { get; set; }
        public bool show_waitlist { get; set; }
        public bool is_valid { get; set; }
        public DateTime ticket_event_date { get; set; }
        public decimal ticket_fee { get; set; }
        public string currency_symbol { get; set; }
        public string currency_code { get; set; }
        public int remaining_qty { get; set; }
        public string remaining_qty_text { get; set; }
        public int member_id { get; set; }
        public bool? promo_code_valid { get; set; }
        public string promo_code_msg { get; set; }
        public string rsvp_access_code { get; set; }
        public int max_tickets_per_order { get; set; }
        public bool? is_private_event { get; set; }
        public int fulfillment_lead_time { get; set; }
        public string fulfillment_lead_time_desc
        {
            get
            {
                if (fulfillment_lead_time > 0)
                    return Common.Common.GetEnumDescription((Common.Common.FulfillmentLeadTime)fulfillment_lead_time);
                else
                    return "";
            }
        }
        public List<TicketsEventWillCallLocation> will_call_location_details { get; set; }
    }

    public class WillCallLocationCheckModel
    {
        public int ticket_id { get; set; }
        public string ticket_name { get; set; }
        public List<TicketsEventWillCallLocation> will_call_location_details { get; set; }
    }

    public class TicketsEventWillCallLocation
    {
        public int ticket_id { get; set; }
        public int location_id { get; set; }
        public string location_name { get; set; }
        public int will_call_limit { get; set; }
        public int order_qty { get; set; }
        public int available_qty { get; set; }
    }

    public class WillCallLocationDetail
    {
        public int location_id { get; set; }
        public string location_name { get; set; }
    }

    public class TicketLevelWillCallLocationDetail
    {
        public int ticket_id { get; set; }
        public List<WillCallLocationDetail> will_call_locations { get; set; }
    }

    public class EventWillCallLocation
    {
        public int location_id { get; set; }
        public string location_name { get; set; }
        public int will_call_limit { get; set; }
        public int order_qty { get; set; }
        public int available_qty { get; set; }
    }

    public class TicketHolder
    {
        public string first_name { get; set; } = "";
        public string last_name { get; set; } = "";
        public string email { get; set; } = "";
        public string company { get; set; } = "";
        public string title { get; set; } = "";
        public string gender { get; set; } = "";
        public int? age { get; set; }
        public string age_group { get; set; } = "";
        public string website { get; set; } = "";
        public string work_phone { get; set; } = "";
        public string mobile_phone { get; set; } = "";
        public string dob { get; set; } = "1/1/1900";
        public string country { get; set; } = "";
        public string address1 { get; set; } = "";
        public string address2 { get; set; } = "";
        public string city { get; set; } = "";
        public string state { get; set; } = "";
        public string postal_code { get; set; } = "";
        public string will_call_location { get; set; } = "";
        public string will_call_location_id { get; set; } = "";
        public int delivery_type { get; set; } = 0;
        public string membership { get; set; } = "";
        public List<TicketQandA> attendee_questions { get; set; }
    }

    public class TicketHolder2
    {
        public int will_call_location_id { get; set; }
    }

    public class TicketCheckIn
    {
        public int id { get; set; }
        //public int location_id { get; set; }
        //public string location_name { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
        public string checkin_date { get; set; }
    }

    public class PassportCheckIn
    {
        public int id { get; set; }
        public string member_name { get; set; }
        public int member_id { get; set; }
        public DateTime checkin_date { get; set; }
    }

    public class TicketsOrderPaymentDetail
    {
        public int ticket_order_id { get; set; }
        public decimal amount { get; set; }
        public string transaction_id { get; set; }
        public Common.Payments.Configuration.Gateway payment_gateway { get; set; }
        public string pay_card_number { get; set; }
        public string pay_card_custName { get; set; }
        public string pay_card_exp_month { get; set; }
        public string pay_card_exp_year { get; set; }
        public string pay_card_last_four_digits { get; set; }
        //public string pay_card_first_four_digits { get; set; }
        public string pay_card_type { get; set; }
        public string card_token { get; set; }
    }

    public class DiscountTicketLevel
    {
        public int ticket_id { get; set; }
    }

    public class CalculateDiscountModel
    {
        public bool useExceeded { get; set; } = false;
        public bool qtyInvalid { get; set; } = false;
        public bool promoApplied { get; set; } = false;
        public bool promo_code_valid { get; set; } = false;
        public bool automated_discount_valid { get; set; } = false;
        public bool automated_discountApplied { get; set; } = false;
        public decimal discount_price { get; set; } = 0;
    }

    public class TicketDiscount
    {
        public int id { get; set; }
        public int ticket_event_id { get; set; }

        public string discount_code { get; set; }
        public decimal discount_amount { get; set; }
        public decimal discount_percent { get; set; }
        public List<DiscountTicketLevel> discount_ticket_levels { get; set; }
        public int number_of_uses { get; set; }
        public int use_count { get; set; }
        public DateTime start_datetime { get; set; }
        public DateTime end_datetime { get; set; }
        public int min_qty_reqd { get; set; }
        public int max_per_order { get; set; }
        public int discount_type { get; set; }
        public int guest_type { get; set; }
        public List<int> assigned_lists { get; set; }
        public int wineryId { get; set; }
    }

    public class TicketPlan
    {
        public int plan_id { get; set; }
        public decimal monthly_fee { get; set; }
        public decimal per_ticket_fee { get; set; }
        public decimal service_fee { get; set; }
        public decimal visa_processing_fee { get; set; }
        public decimal mastercard_processing_fee { get; set; }
        public decimal amex_processing_fee { get; set; }
        public decimal discover_processing_fee { get; set; }
        public decimal max_ticket_fee { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool active { get; set; }
        public string winery_name { get; set; }
        public int svc_agreement_id { get; set; }
    }

    public class TicketLevelForTax
    {
        public int ticket_id { get; set; }
        public int quantity { get; set; }
        public decimal original_price { get; set; }
        public decimal price { get; set; }
        public bool charge_tax { get; set; }
        public decimal gratuity_percentage { get; set; }
        public bool tax_gratuity { get; set; }
        public Common.Common.TicketType ticket_type { get; set; }
    }

    public class TicketLevelForTax2
    {
        public int ticket_id { get; set; }
        public int will_call_location_id { get; set; }
    }

    public class SaveTicketRequest
    {
        public int order_id { get; set; }
        public int member_id { get; set; }
        public string order_date { get; set; }
        public int user_id { get; set; }
        public int event_id { get; set; }
        public string discount_code { get; set; }
        public string access_code { get; set; }
        public List<TicketLevelForTax> ticket_levels { get; set; }
        public List<TicketHolder> ticket_holders { get; set; }
        public string email_address { get; set; }
        public string first_name { get; set; } = "";
        public string last_name { get; set; } = "";
        public string address_1 { get; set; } = "";
        public string address_2 { get; set; } = "";
        public string home_phone { get; set; } = "";
        public string city { get; set; } = "";
        public string state { get; set; } = "";
        public string zip { get; set; } = "";
        public string country { get; set; } = "";
        public string order_note { get; set; }
        public string pay_type { get; set; }
        public string pay_check_number { get; set; }
        public string pay_amount_received { get; set; }
        public string pay_card_type { get; set; }
        public string pay_card_number { get; set; }
        public string pay_card_custName { get; set; }
        public string pay_card_exp_month { get; set; }
        public string pay_card_exp_year { get; set; }
        public string pay_card_cvv { get; set; }
        public string pay_card_last_four_digits { get; set; }
        //public string pay_card_first_four_digits { get; set; }
        public string tokenized_card { get; set; }
        public bool save_card { get; set; }
        public string cust_id { get; set; }
        public string hdyh { get; set; }
        public string wait_list_guid { get; set; }
        public Common.ReferralType referral_type { get; set; } = Common.ReferralType.CellarPass;
        public Guid cart_guid { get; set; } = default(Guid);

        public int itinerary_id { get; set; } = 0;
        public int discount_type { get; set; } = 0;
        public int discount_id { get; set; } = 0;
        public int booked_by_id { get; set; }
        public string booked_by_login_name { get; set; }
        public bool cellarPass_marketing_optin { get; set; } = false;
        public bool subscribe_marketing_optin { get; set; }
        public int attempts { get; set; }
    }

    public class WillCallLocationCheckRequest
    {
        public int event_id { get; set; }
        public List<TicketLevelForTax2> ticket_levels { get; set; }
        //public List<TicketHolder2> ticket_holders { get; set; }
    }

    public class AvailablewclocationsfororderRequest
    {
        public int event_id { get; set; }
        public int ticket_qty { get; set; }
    }

    public class TixOrderCalculationModel
    {
        public decimal subtotal { get; set; }
        public decimal discount { get; set; }
        public decimal service_fees { get; set; }
        public decimal processing_fees { get; set; }
        public decimal gratuity { get; set; }
        public decimal sales_tax { get; set; }
        public decimal sales_tax_percent { get; set; }
        //public decimal processing { get; set; }
        public decimal grand_total { get; set; }
        public string discount_code { get; set; }
        public Guid? cart_guid { get; set; }
        public List<TicketLevelDiscount> ticket_level_discounts { get; set; }
        public List<TicketLevelWillCall> ticket_level_willcall_locations { get; set; }
        public string card_type { get; set; } = "";
        
    }

    public class TicketLevelWillCall
    {
        public int ticket_id { get; set; }
        public List<LocationModel> will_call_locations { get; set; }
    }

    public class TicketLevelDiscount
    {
        public int ticket_id { get; set; }
        public decimal discount_amount { get; set; }
        public int applied_discount_qty { get; set; }
        public decimal discounted_service_fees { get; set; }
        public int discount_id { get; set; }
    }
    public class TicketOrder
    {
        public int Id { get; set; }
        public int Winery_Id { get; set; }
        public int User_Id { get; set; }
        public int Tickets_Event_Id { get; set; }
        public DateTime EventStartDateTime { get; set; }
        public DateTime EventEndDateTime { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal PaidAmt { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal FeeTotal { get; set; }
        public DateTime OrderDate { get; set; }
        public string BillEmailAddress { get; set; }
        public string BillFirstName { get; set; }
        public string BillLastName { get; set; }
        public string BillHomePhone { get; set; }
        public string BillCountry { get; set; }
        public string BillZip { get; set; }
        public string EventTitle { get; set; }
        //public string EventVenueName { get; set; }
        public string EventVenueAddress { get; set; }
        public string OrderGUID { get; set; }
        public string OrderNote { get; set; }
        public string EventOrganizerEmail { get; set; }
        public int EmailReceiptTemplate { get; set; }
        public string EventOrganizerName { get; set; }
        public string EventOrganizerPhone { get; set; }
        public PaymentType PaymentType { get; set; }
        public string wineryName { get; set; }
        public string DynamicPaymentDesc { get; set; }
        public int EmailBuyerReminderTemplate { get; set; }
        public TicketCategory PrimaryCategory { get; set; }
        public string ItineraryGUID { get; set; }
        public string EventBannerImage { get; set; } = "";
        public TicketRefundPolicy refund_policy { get; set; }
        public string refund_policy_text { get; set; }
        public string business_message { get; set; }
        public List<int> internal_notification_recipient { get; set; } = new List<int>();
        public RefundServiceFeesOption refund_service_fees_option { get; set; }
        public string refund_service_fees_option_text { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
        public string ticket_event_policy { get; set; }
        public string venue_full_address { get; set; }
        public int region_id { get; set; }
        public string event_url { get; set; }
        //public int OrderType { get; set; }
        //public TicketOrderStatus TicketOrderStatus { get; set; }
        //public string PaymentTranId { get; set; }
        //public string PaymentDetail { get; set; }
        //public int User_Id { get; set; }
        //public string EventVenueLatitude { get; set; }
        //public string EventVenueCounty { get; set; }
        //public string EventVenueLongitude { get; set; }
        //public decimal PerTicketFee { get; set; }
        //public decimal ServiceFee { get; set; }
        //public decimal ProcessingFee { get; set; }
        //public decimal MaxTicketFee { get; set; }
        //public decimal ProcessingFeeTotal { get; set; }
        //public int DeliveryOption { get; set; }
        //public string BillAddress1 { get; set; }
        //public string BillAddress2 { get; set; }
        //public string BillCity { get; set; }
        //public string BillState { get; set; }
        //public string BillZip { get; set; }
        //public string BillCountry { get; set; }
        //public int ServiceFeesOption { get; set; }
        //public int ReferralID { get; set; }
        //public bool ReminderSent { get; set; }
        //public string MembershipNum { get; set; }
        //public string HDYH { get; set; }
        //public string ZohoOrderRefId { get; set; }
        //public string InternalNote { get; set; }
        //public bool IsChargeback { get; set; }
        //public int MobilePhoneStatus { get; set; }
        //public int PaymentProcessor { get; set; }
    }

    public class TicketBaseModel
    {
        public int id { get; set; }
        public int member_id { get; set; }
        public int tickets_event_id { get; set; }
        public DateTime event_start_date_time { get; set; }
        public DateTime event_end_date_time { get; set; }
        public decimal order_total { get; set; }
        public decimal paid_total { get; set; }
        public decimal sub_total { get; set; }
        public decimal gratuity { get; set; }
        public decimal sales_tax { get; set; }
        public decimal service_fees { get; set; }
        public decimal processing_fee { get; set; }
        public DateTime order_date { get; set; }
        public string bill_email_address { get; set; }
        public string bill_first_name { get; set; }
        public string bill_last_name { get; set; }
        public string bill_home_phone { get; set; }
        public string bill_country { get; set; }
        public string event_title { get; set; }
        //public string event_venue_name { get; set; }
        public string event_venue_address { get; set; }
        public string order_guid { get; set; }
        public string order_note { get; set; }
        public string event_organizer_email { get; set; }
        public string event_organizer_url { get; set; }
        public string event_organizer_name { get; set; }
        public string event_organizer_phone { get; set; }
        public PaymentType payment_type { get; set; }
        public string member_name { get; set; }
        public string dynamic_payment_desc { get; set; }
        public string currency_symbol { get; set; }
        public string currency_code { get; set; }
        public string member_url { get; set; }
        public Times.TimeZone timezone { get; set; }
        public string timezone_name { get; set; }
        public double timezone_offset { get; set; }
        public string event_image { get; set; }
        public string event_image_big { get; set; }

        public string event_image_full_path { get; set; }
        public string event_image_big_full_path { get; set; }

        public TicketCategory primary_category { get; set; }

        public string primary_category_desc
        {
            get
            {
                string primarycategoryDesc = "";
                switch (primary_category)
                {
                    case TicketCategory.ArtExhibit:
                        primarycategoryDesc = "Art Exhibit";
                        break;
                    case TicketCategory.BeerFestival:
                        primarycategoryDesc = "Beer Festival";
                        break;
                    case TicketCategory.BeerTasting:
                        primarycategoryDesc = "Beer Tasting";
                        break;
                    case TicketCategory.Concert:
                        primarycategoryDesc = "Concert";
                        break;
                    case TicketCategory.Education:
                        primarycategoryDesc = "Education";
                        break;
                    case TicketCategory.Festival:
                        primarycategoryDesc = "Festival";
                        break;
                    case TicketCategory.FilmFestival:
                        primarycategoryDesc = "Film Festival";
                        break;
                    case TicketCategory.FoodWine:
                        primarycategoryDesc = "Food Wine";
                        break;
                    case TicketCategory.Fundraiser:
                        primarycategoryDesc = "Fundraiser";
                        break;
                    case TicketCategory.GuestLecture:
                        primarycategoryDesc = "Guest Lecture";
                        break;
                    case TicketCategory.GuidedTour:
                        primarycategoryDesc = "Guided Tour";
                        break;
                    case TicketCategory.LiveMusic:
                        primarycategoryDesc = "Live Music";
                        break;
                    case TicketCategory.LivePerformance:
                        primarycategoryDesc = "Live Performance";
                        break;
                    case TicketCategory.Other:
                        primarycategoryDesc = "Other";
                        break;
                    case TicketCategory.Passport:
                        primarycategoryDesc = "Passport";
                        break;
                    case TicketCategory.SpecialEvent:
                        primarycategoryDesc = "Special Event";
                        break;
                    case TicketCategory.LiveBroadcast:
                        primarycategoryDesc = "Live Broadcast";
                        break;
                    case TicketCategory.Theater:
                        primarycategoryDesc = "Theater";
                        break;
                    case TicketCategory.TourTasting:
                        primarycategoryDesc = "Tour Tasting";
                        break;
                    case TicketCategory.WineCompetition:
                        primarycategoryDesc = "Wine Competition";
                        break;
                    case TicketCategory.WineTasting:
                        primarycategoryDesc = "Wine Tasting";
                        break;
                    case TicketCategory.Workshop:
                        primarycategoryDesc = "Workshop";
                        break;
                    case TicketCategory.Webinar:
                        primarycategoryDesc = "Webinar";
                        break;
                }
                return primarycategoryDesc;
            }
        }

        public int email_receipt_template { get; set; }
        public string ad_image { get; set; }
        public string itinerary_guid { get; set; }
        public string order_special_instructions { get; set; }
        public string confirmation_page { get; set; }
        public string business_message { get; set; }
        public List<int> internal_notification_recipient { get; set; }
        public string venue_city { get; set; }
        public string venue_state { get; set; }
        public string venue_zip { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
        public string venue_full_address { get; set; }
        public int region_id { get; set; }
        public string event_url { get; set; }
        public string ticket_event_policy { get; set; }
    }
    public class TicketOrderModel : TicketBaseModel
    {
        public List<TixOrderTicket> ticket_order_ticket { get; set; }
        public List<TixOrderHolder> ticket_holder { get; set; }
        public List<TixOrderTicket> ticket_order_ticket_summary { get; set; }
        public TicketRefundPolicy refund_policy { get; set; }
        public string refund_policy_text { get; set; }
    }

    public class TicketOrderV2Model : TicketBaseModel
    {
        public List<TixOrderTicketV2> ticket_order_ticket { get; set; }
        public List<TixOrderTicket> ticket_order_ticket_summary { get; set; }
    }

    public class TicketOrderClaimModel : TicketBaseModel
    {
        public TixOrderTicket ticket_details { get; set; }

        public TixOrderHolder attendee_details { get; set; }
        public List<TicketQuestion> attendee_questions { get; set; }
        public List<WillCallLocation> will_call_locations { get; set; }

        public List<TicketQandA> question_answers { get; set; }
    }

    public class WillCallLocation
    {
        public int id { get; set; }

        public string will_call_location { get; set; }
    }

    public class TixOrderHolder
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }

        public string web_site { get; set; }

        public string title { get; set; }

        public int age { get; set; }

        public string work_phone { get; set; }

        public string mobile_phone { get; set; }

        public string company { get; set; }

        public DateTime? birth_date { get; set; }


        public string address1 { get; set; }

        public string address2 { get; set; }

        public string city { get; set; }

        public string state { get; set; }

        public string zip_code { get; set; }

        public string country { get; set; }

        public string gender { get; set; }

        public string age_group { get; set; }
    }

    public class TixOrderTicket
    {
        public int id { get; set; }
        public DateTime valid_start_date { get; set; }
        public DateTime validend_date { get; set; }
        public TicketDelivery delivery_type { get; set; }
        public decimal sales_tax { get; set; }
        public string ticket_name { get; set; }
        public int ticket_qty { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public decimal ticket_price { get; set; }
        public decimal ticket_fee { get; set; }
        public decimal gratuity { get; set; }
        public int will_call_location_id { get; set; }
        public string will_call_location_name { get; set; }
        public string will_call_location_address { get; set; }

        public TicketPostCaptureStatus post_capture_status { get; set; }

        public string post_capture_key { get; set; }

        public string post_capture_status_text
        {
            get
            {
                return this.post_capture_status.ToString();
            }
        }

        public bool can_post_capture
        {
            get
            {
                return (this.post_capture_status == TicketPostCaptureStatus.Available);
            }
        }

        public bool can_ticket_claimed
        {
            get
            {
                return (this.post_capture_status != TicketPostCaptureStatus.Claimed && this.post_capture_status != TicketPostCaptureStatus.NA);
            }
        }

        public string delivery_type_desc
        {
            get
            {
                string deliveryDesc = "";
                if (this.delivery_type == TicketDelivery.SelfPrint)
                {
                    deliveryDesc = "SELF-PRINT - Print your tickets before the event";
                }
                else if (this.delivery_type == TicketDelivery.Shipped)
                {
                    deliveryDesc = "Shipped - Tickets will be shipped in 1-2 business days to your billing address";

                    if (fulfillment_lead_time > 0)
                        deliveryDesc = Common.Common.GetEnumDescription((Common.Common.FulfillmentLeadTime)fulfillment_lead_time);
                }
                else
                {
                    if(!string.IsNullOrEmpty(this.will_call_location_name))
                    deliveryDesc = string.Format("Will Call- {0}", this.will_call_location_name.Replace("Will Call: ", ""));
                }

                return deliveryDesc;
            }
        }

        public string delivery_type_description
        {
            get
            {
                string deliveryDesc = "";
                if (this.delivery_type == TicketDelivery.SelfPrint)
                {
                    deliveryDesc = "Self Print";
                }
                else if (this.delivery_type == TicketDelivery.Shipped)
                {
                    deliveryDesc = "Shipped";
                }
                else
                {
                    deliveryDesc = "Will Call";
                }

                return deliveryDesc;
            }
        }

        public string ticket_desc { get; set; }
        public Common.Common.TicketType ticket_type { get; set; }
        public int ticket_id { get; set; }
        public string ticket_barcode { get; set; }

        public int fulfillment_lead_time { get; set; }
        public string fulfillment_lead_time_desc
        {
            get
            {
                if (fulfillment_lead_time > 0)
                    return Common.Common.GetEnumDescription((Common.Common.FulfillmentLeadTime)fulfillment_lead_time);
                else
                    return "";
            }
        }
        public bool include_confirmation_message { get; set; }
        public string confirmation_message { get; set; }
        public bool display_venue_on_ticket { get; set; }
    }

    public class TixOrderTicketV2
    {
        public int id { get; set; }
        public int tickets_order_id { get; set; }
        public DateTime valid_start_date { get; set; }
        public DateTime validend_date { get; set; }
        public TicketDelivery delivery_type { get; set; }
        public decimal sales_tax { get; set; }
        public string ticket_name { get; set; }
        public int ticket_qty { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public decimal ticket_price { get; set; }
        public decimal ticket_fee { get; set; }
        public decimal gratuity { get; set; }
        public int will_call_location_id { get; set; }
        public string will_call_location_name { get; set; }
        public string will_call_location_address { get; set; }
        public int fulfillment_lead_time { get; set; }
        public string fulfillment_lead_time_desc
        {
            get
            {
                if (fulfillment_lead_time > 0)
                    return Common.Common.GetEnumDescription((Common.Common.FulfillmentLeadTime)fulfillment_lead_time);
                else
                    return "";
            }
        }
        public TicketPostCaptureStatus post_capture_status { get; set; }

        public string post_capture_key { get; set; }

        public string post_capture_status_text
        {
            get
            {
                return this.post_capture_status.ToString();
            }
        }

        public bool can_post_capture
        {
            get
            {
                return (this.post_capture_status == TicketPostCaptureStatus.Available);
            }
        }

        public bool can_ticket_claimed
        {
            get
            {
                return (this.post_capture_status != TicketPostCaptureStatus.Claimed && this.post_capture_status != TicketPostCaptureStatus.NA);
            }
        }

        public string delivery_type_desc
        {
            get
            {
                string deliveryDesc = "";
                if (this.delivery_type == TicketDelivery.SelfPrint)
                {
                    deliveryDesc = "SELF-PRINT - Print your tickets before the event";
                }
                else if (this.delivery_type == TicketDelivery.Shipped)
                {
                    deliveryDesc = "Shipped - Tickets will be shipped in 1-2 business days to your billing address";

                    if (fulfillment_lead_time > 0)
                        deliveryDesc = Common.Common.GetEnumDescription((Common.Common.FulfillmentLeadTime)fulfillment_lead_time);
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.will_call_location_name))
                        deliveryDesc = string.Format("Will Call- {0}", this.will_call_location_name.Replace("Will Call: ", ""));
                }

                return deliveryDesc;
            }
        }

        public string delivery_type_description
        {
            get
            {
                string deliveryDesc = "";
                if (this.delivery_type == TicketDelivery.SelfPrint)
                {
                    deliveryDesc = "Self Print";
                }
                else if (this.delivery_type == TicketDelivery.Shipped)
                {
                    deliveryDesc = "Shipped";
                }
                else
                {
                    deliveryDesc = "Will Call";
                }

                return deliveryDesc;
            }
        }

        public string ticket_desc { get; set; }
        public Common.Common.TicketType ticket_type { get; set; }
        public int ticket_id { get; set; }
        public string ticket_barcode { get; set; }
        public string web_site { get; set; }
        public string title { get; set; }
        public int age { get; set; }
        public string work_phone { get; set; }
        public string mobile_phone { get; set; }
        public string company { get; set; }
        public DateTime? birth_date { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
        public string country { get; set; }
        public string gender { get; set; }
        public string age_group { get; set; }
        public bool include_confirmation_message { get; set; }
        public string confirmation_message { get; set; }
        public string event_title { get; set; }
        public string winery_name { get; set; }
        public string event_start_time { get; set; }
        public string member_url { get; set; }
        public List<int> regions_ids { get; set; }
    }

    public class TicketOrderTicket
    {
        public int Id { get; set; }

        public int Winery_Id { get; set; }
        public DateTime ValidStartDate { get; set; }
        public DateTime ValidEndDate { get; set; }
        public TicketDelivery DeliveryType { get; set; }
        public decimal SalesTax { get; set; }
        public string TicketName { get; set; }
        public int TicketQty { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal TicketFee { get; set; }
        public decimal Gratuity { get; set; }
        public TicketPostCaptureStatus PostCaptureStatus { get; set; }
        public string PostCaptureKey { get; set; }

        public string Email { get; set; }

        public int WillCallLcationId { get; set; }
        public string WillCallLocationName { get; set; }
        public string WillCallLocationAddress { get; set; }
        public bool include_confirmation_message { get; set; }
        public string confirmation_message { get; set; }
        public int fulfillment_lead_time { get; set; }
        public List<int> internal_notification_recipient { get; set; }
        public string fulfillment_lead_time_desc
        {
            get
            {
                if (fulfillment_lead_time > 0)
                    return Common.Common.GetEnumDescription((Common.Common.FulfillmentLeadTime)fulfillment_lead_time);
                else
                    return "";
            }
        }
    }

    public class TicketsWaitlistRequest
    {
        public int ticket_event_id { get; set; }
        public int ticket_ticket_id { get; set; }
        public int qty { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string home_phone { get; set; }
        public int member_id { get; set; }
        public string note { get; set; } = "";
    }

    public class TicketsWaitlistUpdateRequest
    {
        public int id { get; set; } = 0;
        public string waitlist_guid { get; set; } = "";
        public TicketWaitlistStatus status { get; set; }
    }

    public class TicketsWaitlistUpdateNoteRequest
    {
        public int id { get; set; } = 0;
        public string waitlist_guid { get; set; } = "";
        public string note { get; set; } = "";
    }

    public class TicketWaitlistDetail
    {
        public DateTime start_date_time { get; set; }
        public DateTime end_date_time { get; set; }
        public string organizer_name { get; set; }
        public string event_title { get; set; }
        //public string venue_name { get; set; }
        public string home_phone { get; set; }
        public int event_id { get; set; }
        public string waitlist_guid { get; set; }

        public int ticket_id { get; set; }
        public string ticket_name { get; set; }
        public decimal price { get; set; }
        public int qty_desired { get; set; }
        public int qty_offered { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public int waitlist_expiration { get; set; }
        public int member_id { get; set; }
        public string currency_symbol { get; set; }
        public string currency_code { get; set; }
        public string organizer_phone { get; set; }
        public string venue_address { get; set; }
        public string event_url { get; set; }
        public DateTime valid_start_date { get; set; }
        public DateTime valid_end_date { get; set; }
        public string member_phone { get; set; }
        public string member_country { get; set; }

        public TicketWaitlistStatus status { get; set; }

        public string status_desc
        {
            get
            {
                return this.status.ToString();
            }
        }
        public DateTime? date_offered { get; set; }

        public int hours_left { get; set; }
        public string note { get; set; } = "";
    }

    public class PassportParticipatingMemberModel
    {
        public int member_id { get; set; }
        public int review_stars { get; set; }
        public int review_count { get; set; }
        public string member_url { get; set; }
        public string benefit_desc { get; set; }
        public string member_name { get; set; }
        public string member_city { get; set; }
        public string member_appellation_name { get; set; }
        public string thumbnail_image { get; set; }
        public string visitation_external_url { get; set; }
        public Visitation_Rule visitation_rule { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }

        public bool is_active { get; set; }

        public bool is_hidden { get; set; }
        public bool is_hidden_gem { get; set; }
        public string visitation_rule_desc
        {
            get
            {
                return GetPassportVisitationRulesConsumer().Where(f => f.ID == (int)visitation_rule).Select(f => f.Name).FirstOrDefault();
            }
        }
        public string member_state { get; set; }
        public string passport_event_name { get; set; }
    }

    public class TicketQuestion
    {
        public int question_id { get; set; }
        public string question_text { get; set; }
        public string question_type { get; set; }
        public bool is_required { get; set; }
        public int question_show_to { get; set; }
        public bool is_default_state { get; set; }
        [JsonIgnore]
        public string choices_string { get; set; }
        private List<string> _lstchoices = null;
        public List<string> choices {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.choices_string))
                {
                    return this.choices_string.Split("@!@").ToList();
                }
                else
                    return _lstchoices;

            }
            set
            {
                _lstchoices = value;
            }
        }
    }

    public class TicketFAQ
    {
        public int faq_id { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public int sort_order{ get; set; }
    }

    public class AdditionalContent
    {
        public int id { get; set; }
        public int content_type { get; set; }
        public string content { get; set; }
        public string image_name { get; set; }
        //public string url { get; set; }
        public int sort_order { get; set; }
        //public string button_text { get; set; }
    }

    public class EventTicketHolder
    {
        public int Id { get; set; }
        public string TicketHolderEmail { get; set; }
        public string TicketHolderFirstName { get; set; }
        public string TicketHolderLastName { get; set; }
        public string EventTitle { get; set; }
        public string EventVenueName { get; set; }
        public DateTime ValidStartDate { get; set; }
        public DateTime ClaimDate { get; set; }
        public string TicketsOrderTicketGUID { get; set; }
    }

    public class ActivatePassportModel
    {
        public string post_capture_key { get; set; }
        public int event_id { get; set; }
    }

    public class TicketWillCallLocation
    {
        public int Id { get; set; }
        public int Tickets_Event_WillCallLocation_Id { get; set; }
        public int Tickets_Ticket_Id { get; set; }
        public int WillCallLimit { get; set; }
        public int Location_Id { get; set; }
    }
    public class WillCallLocations
    {
        public int Id { get; set; }
        public string LocationName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    public class WillCallAllocation
    {
        public int LocationId { get; set; }
        public int Allocated { get; set; }
        public int TicketId { get; set; }
    }

    public class OrderPDFStrModel
    {
        public string OrderPDFStr { get; set; }
    }

    public class PassportItineraryInstructionModel
    {
        public string passport_itinerary_instructions { get; set; }
    }

    public class UpcomingEventModel
    {
        public string friendly_url { get; set; }
        public string event_date { get; set; }
        public bool is_single_date { get; set; }
        public string start_date_month { get; set; }
        public string start_date_date { get; set; }
        public string end_date_month { get; set; }
        public string end_date_date { get; set; }
        public int event_id { get; set; }
        public string event_title { get; set; }
        public string organizer_name { get; set; }
        public DateTime start_date_time { get; set; }
        public DateTime end_date_time { get; set; }
        public string primary_category { get; set; }
        public string secondary_category { get; set; }
        public string event_listing_URL { get; set; }
        public string event_banner_URL { get; set; }

        public string event_listing_url_full_path { get; set; }
        public string event_banner_url_full_path { get; set; }

        public string venue_country { get; set; }
        public string venue_city { get; set; }
        public string venue_state { get; set; }
        public string state_name { get; set; }
        public int tickets_sold { get; set; }
        public int max_capacity { get; set; }
        public int member_id { get; set; }
        public string member_name { get; set; }
        public bool is_favorites { get; set; }
        public Times.TimeZone time_zone { get; set; }
        public string time_zone_name { get; set; }
        public bool sold_out { get; set; }
        public string address1 { get; set; }
        public string short_description { get; set; }
        public bool enable_map { get; set; }
        public DateTime? last_ticket_sales_end_in { get; set; }
        public string region_name { get; set; }
        public string member_url { get; set; }
        public decimal min_price { get; set; }
        public decimal max_price { get; set; }
        public int regions_id { get; set; }
        public string event_organizer_url { get; set; }
        public int total_events { get; set; }
        public bool requires_invite { get; set; }
    }

    public class TicketEventsComponentRequest
    {
        public int? topRecords { get; set; } = null;
        public int? memberid { get; set; } = null;
        public int? regionId { get; set; } = null;
        public string orderby { get; set; } = "TicketsSold DESC, StartDateTime ASC";
        public bool? onePerMember { get; set; } = null;
        public bool? onePerRegion { get; set; } = null;
        public int? daysAfter { get; set; } = null;
        public int? subregionId { get; set; } = null;
        public bool? isAdvancedFilter { get; set; } = null;
        public string subRegionIds { get; set; } = null;
        public string eventTypes { get; set; } = null;
        public string reviews { get; set; } = null;
        public string populartags { get; set; } = null;
        public string varietals { get; set; } = null;
        public string notableFeatures { get; set; } = null;
        public string eventOrganizerName { get; set; } = null;
        public int? excludeEventId { get; set; } = null;
        public int? excludeWineryId { get; set; } = null;
        public string excludeOrganizerName { get; set; } = null;
        public int? categoryId { get; set; } = null;
        public int? status { get; set; } = null;
        public bool? isPrivate { get; set; } = null;
        public string searchTerm { get; set; } = null;
        public bool? isUniqueRecords { get; set; } = null;
        public int? userID { get; set; } = null;
        public bool? isPastEvents { get; set; } = null;
        public DateTime? arrivalDate { get; set; } = null;
        public DateTime? departureDate { get; set; } = null;
        public bool? valid_sale { get; set; } = null;
        //public string regionIds { get; set; } = null;
    }

    public class TicketEventReminderMOdel
    {
        public int queue_id { get; set; }
        public int member_id { get; set; }
        public int event_id { get; set; }
        public string event_title { get; set; }

    }

    public class TicketEventUserNotificationMOdel
    {
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
    }


    public class TicketEventReviewRequest
    {
        public string ticket_guid { get; set; }
        public int rating { get; set; }
        public int first_impression_rating { get; set; }
        public int ambiance_rating { get; set; }
        public int hospitality_rating { get; set; }       
        public int recommend_to { get; set; }
        public Boolean any_purchases { get; set; }
        public int purchase_amount_range { get; set; }
        public int membership_signup { get; set; }
        public string review { get; set; }
        public string private_comment { get; set; }
        public string email { get; set; }
        public Boolean is_favorite { get; set; }
    }

    public class EventReviewModel
    {
        public int id { get; set; }
        public bool active { get; set; }
        public int winery_id { get; set; }
        public int metric1 { get; set; }
        public int metric2 { get; set; }
        public int metric3 { get; set; }
        public int metric4 { get; set; }
        public int metric5 { get; set; }
        public int metric6 { get; set; }
        public string description { get; set; }
        public string private_comment { get; set; }
        public DateTime date_of_review { get; set; }
        public string event_title { get; set; }
        public DateTime user_join_date { get; set; }
        public string user_first_name { get; set; }
        public string user_last_name { get; set; }
        public string purchase_url { get; set; }
    }

    public class AdditionalTicketInfoModel
    {
        public int id { get; set; }
        public string TicketName { get; set; }
        public string ConfirmationMessage { get; set; }
    }

    public class TicketsFaqModel
    {
        public int id { get; set; }
        public int winery_id { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
    }

    public class TicketsByEventExport
    {
        public int TicketId { get; set; }
        public string TicketLevel { get; set; }
        public string DeliveryMethod { get; set; }
        public string TicketStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public int Order { get; set; }
        public string TicketholderFirstName { get; set; }
        public string TicketholderLastName { get; set; }
        public string TicketholderEmail { get; set; }
        public string TicketholderTitle { get; set; }
        public string TicketholderCompany { get; set; }
        public string TicketholderWPhone { get; set; }
        public string TicketholderMPhone { get; set; }
        public string TicketholderAddress1 { get; set; }
        public string TicketholderAddress2 { get; set; }
        public string TicketHolderCity { get; set; }
        public string TicketHolderState { get; set; }
        public string TicketHolderZip { get; set; }
        public string TicketHolderCountry { get; set; }
        public string TicketHolderGender { get; set; }
        public string TicketHolderAgeGroup { get; set; }
        public int TicketHolderAge { get; set; }
        public string TicketHolderDOB { get; set; }
        public string TicketHolderWebsite { get; set; }
        public decimal Ticket { get; set; }
        public decimal Gratuity { get; set; }
        public decimal BuyerSvcFee { get; set; }
        public decimal HostSvcFee { get; set; }
        public decimal TotalSvcFee { get; set; }
        public decimal SalesTax { get; set; }
        public decimal TicketTotal { get; set; }
        public decimal CCProcFee { get; set; }
        public string PromoCode { get; set; }
        public string AccessCode { get; set; }
        public string OrderNotes { get; set; }
    }

    public class ExportCheckInAttendees
    {
        public string Email { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string HomePhone { get; set; }
        public string AccountType { get; set; }
        public string DateCheckedin { get; set; }
        public string Location { get; set; }
        public int TicketId { get; set; }

    }

    public class TicketOrderV2
    {
        public int id { get; set; }
        public string order_guid { get; set; }
        public int order_status { get; set; }
        public bool is_charge_back { get; set; }
        public int user_id { get; set; }
        public string order_notes { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int event_id { get; set; }
        public string event_title { get; set; }
        public DateTime order_date { get; set; }
        public decimal order_total { get; set; }
        public int time_zone { get; set; }
        public decimal ticket_fee { get; set; }
        public decimal svc_fees { get; set; }
        public decimal cc_fees { get; set; }
        public int pay_type { get; set; }
        public int ticket_count { get; set; }
        public string promo_code { get; set; }
        public decimal sales_tax { get; set; }
        public int visit_count { get; set; }
    }

    public class TicketEventLandingPageModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string meta_keywords { get; set; }
        public string meta_description { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public Times.TimeZone timezone { get; set; }
        public double timezone_offset { get; set; }
        public Common.Common.TicketsEventStatus status { get; set; }
        public string status_description { get; set; }
        public int event_type { get; set; }
        public int total_tickets_sold { get; set; }
        public int event_capacity { get; set; }
        public int event_remaining_qty
        {
            get
            {
                return event_capacity - total_tickets_sold;
            }
        }
        public bool event_ended { get; set; } = false;
        public bool show_upcoming_events { get; set; } = false;
        public bool has_upcoming_events { get; set; } = false;
        public string city_url { get; set; }
        public VenueLocation location { get; set; }
        public List<ParticipatingLocations> participating_locations { get; set; }
        public bool guest_list_enabled { get; set; } = false;
        public Common.Common.AttendanceModeStatus attendance_mode { get; set; }
        public DateTime membership_start_date { get; set; }
        public DateTime membership_end_date { get; set; }
        public string event_organizer_name { get; set; }
        public string event_organizer_url { get; set; }
        public string event_organizer_email { get; set; }
        public string event_organizer_phone { get; set; }
        public string event_image { get; set; }
        public string event_image_big { get; set; }
        public string event_image_full_path { get; set; }
        public string event_image_big_full_path { get; set; }
        public List<int> regions_ids { get; set; }
        public List<string> tags { get; set; }
        public int primary_category_id { get; set; }
        public string primary_category { get; set; }
        public int secondary_category_id { get; set; }
        public string secondary_category { get; set; }
        public TicketRefundPolicy refund_policy { get; set; }
        public string refund_policy_text { get; set; }
        public RefundServiceFeesOption refund_service_fees_option { get; set; }
        public string refund_service_fees_option_text { get; set; }

        public bool sold_out { get; set; }
        public bool waitlist_available { get; set; }
        public bool require_reservations { get; set; }
        //public bool disable_travel_time_restrictions { get; set; }
        public DateTime? last_ticket_sales_end_in { get; set; }
        public decimal min_price { get; set; }
        public decimal max_price { get; set; }
        

        public string short_description { get; set; }

        public string long_description { get; set; }

        public string business_name { get; set; }
        public int business_id { get; set; }
        public string instagram_url { get; set; }
        public string twitter_url { get; set; }
        public string facebook_url { get; set; }
        public string pinterest_url { get; set; }

        public bool is_user_favorite { get; set; } = false;
        public bool show_broadcast { get; set; } = false;
        public string broadcast_url { get; set; } = "";

        public bool requires_invite { get; set; }
        public bool requires_password { get; set; }
        public bool is_private { get; set; }

        public bool show_guest_list { get; set; }

        public string guest_lists { get; set; }

        public bool show_discount_code { get; set; }
        public bool is_automated_discounts { get; set; }

        public bool show_access_code { get; set; }



        public string business_profile_url { get; set; }

        public string timezone_name { get; set; }
        public bool available_tickets { get; set; }
        public string ticket_cancel_policy { get; set; }
        public string event_attendee_policy { get; set; }

        public string event_url { get; set; }
        public bool is_cp_payment_gateway { get; set; }


        public string business_region_name { get; set; }

        public bool show_organizer_phone { get; set; }

        public bool show_map { get; set; }

        public bool show_book_rsvp_btn { get; set; }

        public string rsvp_booking_short_desc { get; set; }

        public string rsvp_booking_long_desc { get; set; }

        public bool show_availability_btn { get; set; }
        public bool disable_activate_button { get; set; }
        public bool has_reviews{ get; set; }
        public bool has_post_config_ticket_levels { get; set; }
        public bool disable_book_itinerary_msg { get; set; }
        public int cta_button { get; set; }
        public string country_name { get; set; }
        public string state_name { get; set; }
        public string state_url
        {
            get
            {
                if (string.IsNullOrEmpty(state_name))
                    return "";

                return string.Format("{0}", Regex.Replace(state_name.ToLower(), "  ", " ").Replace(" ", "-").TrimEnd('-'));
            }
        }
        public string region_url { get; set; }

        public int review_stars { get; set; }
        public decimal avg_review_value { get; set; }
        public int total_reviews { get; set; }

        public TicketHolderConfig ticket_holder_config { get; set; }
        public TicketPostCaptureConfig post_capture_config { get; set; }
        public List<TicketQuestion> attendee_questions { get; set; }
        public List<TicketFAQ> faqs { get; set; }
        public List<AdditionalContent> additional_content
        {
            get; set;
        }
    }

    public class Availablewclocationsfororderv2Request
    {
        public int event_id { get; set; }
        public List<Availablewclocationsfororderv2Model> ticket_level_details { get; set; }
    }

    public class Availablewclocationsfororderv2Model
    {
        public int ticket_id { get; set; }
        public int? ticket_qty { get; set; }
    }

    public class UserOrdersModel
    {
        public string event_title { get; set; }
        public string member_name { get; set; }
        public DateTimeOffset start_date_time { get; set; }
        public DateTimeOffset end_date_time { get; set; }
        public int order_id { get; set; }
        public DateTimeOffset order_date { get; set; }
        public Decimal order_total { get; set; }
        public int no_of_tickets { get; set; }
        public string order_guid { get; set; }
        public int event_id { get; set; }
        public string purchase_url { get; set; }
        public bool is_self_print { get; set; }
        public int primary_category { get; set; }
        public string start_date_time_formated { get; set; }
        public string end_date_time_formated { get; set; }
        public string order_date_formated { get; set; }
        public bool is_single_date { get; set; }
        public string order_total_text { get; set; }
        public bool is_past_event { get; set; }
        public string event_url
        {
            get
            {
                return string.Format("{0}-{1}", Regex.Replace(event_title.ToLower(), "[^A-Za-z0-9 -]+", "").Replace("  ", " ").Replace(" ", "-").TrimEnd('-'), event_id);
            }
        }
    }

    public class UserTicketModel
    {
        public string event_title { get; set; }
        public string member_name { get; set; }
        public string ticket_name { get; set; }
        public int id { get; set; }
        public DateTime valid_start_date { get; set; }
        public string delivery_type { get; set; }
        public bool is_past_event { get; set; }
        public int star { get; set; }
        public int event_id { get; set; }
        public int member_id { get; set; }
        public bool is_self_print { get; set; }
        public DateTime event_start_date_time { get; set; }
        public DateTime event_end_date_time { get; set; }
        public String purchase_url { get; set; }
        public string order_guid { get; set; }
        public string tickets_order_ticket_guid { get; set; }
        public int primary_category { get; set; }
        public string valid_start_date_formated { get; set; }
        public bool is_reviewed { get; set; }
        public bool is_single_date { get; set; }
        public string event_start_date_time_formated { get; set; }
        public string event_end_date_time_formated { get; set; }
        public string event_url
        {
            get
            {
                return string.Format("{0}-{1}", Regex.Replace(event_title.ToLower(), "[^A-Za-z0-9 -]+", "").Replace("  ", " ").Replace(" ", "-").TrimEnd('-'), event_id);
            }
        }
    }

    public class TicketEventByEventTypeModel
    {
        public int event_id { get; set; }
        public string event_title { get; set; }
        public string organizer_name { get; set; }
        public DateTime start_date_time { get; set; }
        public DateTime end_date_time { get; set; }
        public string primary_category { get; set; }
        public string secondary_category { get; set; }
        public string event_image { get; set; }
        public string event_image_big { get; set; }

        public string event_image_full_path { get; set; }
        public string event_image_big_full_path { get; set; }

        public string venue_county { get; set; }
        public string venue_city { get; set; }
        public string venue_state { get; set; }
        public int location_id { get; set; }
        public string state_name { get; set; }
        public int? tickets_sold { get; set; }
        public int max_capacity { get; set; }
        public int? offer_business_id { get; set; }
        public string offer_business_name { get; set; }
        public bool is_favorites { get; set; }
        public string time_zone { get; set; }
        public bool sold_out { get; set; }
        public string friendly_url
        {
            get
            {
                return string.Format("{0}-{1}", Regex.Replace(event_title.ToLower(), "[^A-Za-z0-9 -]+", "").Replace("  ", " ").Replace(" ", "-").TrimEnd('-'), event_id);
            }
        }
        public string event_date { get; set; }
        public bool is_single_date { get; set; }
        public string start_date_month { get; set; }
        public string start_date_date { get; set; }
        public string end_date_month { get; set; }
        public string end_date_date { get; set; }
        public int event_type { get; set; }
    }

    public class TicketReviewModel
    {
        public int metric_1 { get; set; }
        public string description { get; set; }
    }
    public class TicketReviewPostModel : TicketReviewModel
    {
        public int business_id { get; set; }
        public int event_id { get; set; }
        public int ticket_order_ticket_id { get; set; }
    }
}
