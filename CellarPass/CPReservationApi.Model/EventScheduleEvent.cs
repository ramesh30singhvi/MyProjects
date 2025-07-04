using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CPReservationApi.Common.Common;
using uc = CPReservationApi.Common;

namespace CPReservationApi.Model
{
    public class ScheduleEventId
    {
        public List<EventScheduleModel> EventScheduleEventId { get; set; }
        public bool isAnyEventAvailable { get; set; } = false;
    }
    public class EventScheduleModel
    {
        public int event_id { get; set; }
        public string event_name { get; set; }
    }
    public class EventSchedule
    {
        public List<EventScheduleEvent> EventScheduleEvent { get; set; }
        public bool isAnyEventAvailable { get; set; } = false;
    }
    public class EventScheduleEvent
    {
        public int event_id { get; set; }
        public string event_name { get; set; }
        public string event_location { get; set; }
        public int location_id { get; set; }
        public string event_desc { get; set; }
        public string duration_desc { get; set; }
        public decimal fee_Per_person { get; set; }
        public decimal all_inclusive_price { get; set; }
        public decimal fee_type { get; set; }
        public string fee_per_person_desc { get; set; }
        public string location_zip { get; set; }
        public string location_street2 { get; set; }
        public string location_street1 { get; set; }
        public string location_state { get; set; }
        public string location_city { get; set; }
        public string club_member_benefit { get; set; }
        public bool show_club_member_benefits { get; set; }
        public string lead_time_desc { get; set; }
        public string max_lead_time_desc { get; set; }
        public string cancel_time_desc { get; set; }
        public int cancel_lead_time_in_minutes { get; set; }
        public int min_persons { get; set; }
        public int max_persons { get; set; }
        public string deposit_policy { get; set; }
        public string cancel_policy { get; set; }
        public int member_id { get; set; }
        public bool show_additional_guests { get; set; }
        public bool show_guest_tags { get; set; }
        public bool require_additional_guests { get; set; }
        public List<EventScheduleTime> event_times { get; set; }
        public bool require_credit_card { get; set; }
        public bool reject_credit_card_on_decline { get; set; }
        public int table_status_group_id { get; set; }
        public bool account_type_required { get; set; }
        public bool member_benefit_required { get; set; }
        public bool show_promo_code { get; set; }
        public bool show_passport_code { get; set; }
        public bool show_transportation { get; set; }
        public List<AddOn_Group> event_addOn { get; set; }
        public string member_url { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime? event_end_date { get; set; }
        public int email_content_id { get; set; }
        public decimal gratuity_percentage { get; set; }
        public bool tax_gratuity { get; set; }
        public List<EventQuestion> event_questions { get; set; }
        public List<Event_FloorPlans> event_floor_plans { get; set; }
        
    }

    public class ReservationCheckoutModel
    {
        public int event_id { get; set; }
        public string event_name { get; set; }
        public string event_location { get; set; }
        public int location_id { get; set; }
        public string event_desc { get; set; }
        public string duration_desc { get; set; }
        public int fee_type { get; set; }
        public decimal fee_Per_person { get; set; }
        public string fee_per_person_desc { get; set; }
        public string location_zip { get; set; }
        public string location_street2 { get; set; }
        public string location_street1 { get; set; }
        public string location_state { get; set; }
        public string location_city { get; set; }
        public string club_member_benefit { get; set; }
        public bool show_club_member_benefits { get; set; }
        public string lead_time_desc { get; set; }
        public string max_lead_time_desc { get; set; }
        public string cancel_time_desc { get; set; }
        public int cancel_lead_time_in_minutes { get; set; }
        public int min_persons { get; set; }
        public int max_persons { get; set; }
        public string deposit_policy { get; set; }
        public int charge_fee { get; set; }
        public string cancel_policy { get; set; }
        public int member_id { get; set; }
        public bool show_additional_guests { get; set; }
        public bool show_guest_tags { get; set; }
        public bool require_additional_guests { get; set; }
        public bool require_credit_card { get; set; }
        public bool reject_credit_card_on_decline { get; set; }
        public int table_status_group_id { get; set; }
        public bool account_type_required { get; set; }
        public bool member_benefit_required { get; set; }
        public bool show_promo_code { get; set; }
        public bool show_passport_code { get; set; }
        public bool show_transportation { get; set; }
        public List<AddOn_Group> event_addOn { get; set; }
        public string member_url { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime event_end_date { get; set; }
        public int email_content_id { get; set; }
        public string currency_symbol { get; set; }
        public string currency_code { get; set; }
        public bool payment_enabled { get; set; }
        public List<int> accepted_card_types { get; set; }
        public string member_name { get; set; }
        public decimal transaction_fees { get; set; }
        public bool show_fees { get; set; }
        public decimal gratuity_percentage { get; set; }
        public bool tax_gratuity { get; set; }
        public bool ignore_sales_tax { get; set; }
        public bool ignore_deposit_cc_policy_on_zero_total { get; set; }
        public List<EventQuestion> event_questions { get; set; }
        public string member_benefits_url { get; set; }
        public string member_benefits_desc { get; set; }
        public Guid? cart_guid { get; set; }
        public int favorite_region_id { get; set; }
        public bool show_winery_visit_question { get; set; } = true;
        public string event_image { get; set; } = string.Empty;
        public int required_hdyh { get; set; }
        public bool commerce7_enabled { get; set; }
        public string price_range { get; set; }
        public bool detailed_address_info_required { get; set; }
        public string event_type { get; set; }
        public string event_sku { get; set; }
        public string member_website { get; set; }
        public int mobile_number_status { get; set; }
        public bool show_marketing_optin { get; set; }
        public string marketing_optin_text { get; set; }
        public string member_phone { get; set; }
        public bool twilio_disable_verification_service { get; set; }
        public int sms_provider { get; set; }
        public DateTime? cancel_by_date { get; set; }
        public string cancel_message { get; set; }
        public int location_timezone { get; set; }
        public double location_timezone_offset { get; set; }
    }

    public class EventQuestion
    {
        public int id { get; set; }
        public string question_text { get; set; }
        public string question_type { get; set; }

        public bool is_required { get; set; }
        public List<string> choices { get; set; }
    }


    public class EventScheduleTime
    {
        public int slot_id { get; set; }
        public int seats_left { get; set; }
        public int event_total_seats { get; set; }
        public int slot_type { get; set; }
        public string time { get; set; }
        public string end_time { get; set; }
        public bool is_available { get; set; }
        public int slot_color { get; set; }
        public bool is_recurring { get; set; }
        public int event_duration_minutes { get; set; }
    }



    public class MaxSeatsLeft
    {
        public int max_seats { get; set; } = 0;
        public int min_seats { get; set; } = 0;
    }

    public class Event
    {
        public int event_id { get; set; }
        public string event_name { get; set; }
        public string event_desc { get; set; }
        public int max_persons { get; set; }
        public int min_persons { get; set; }
        public decimal fee_Per_person { get; set; }
        public decimal all_inclusive_price { get; set; }
        public int fee_type { get; set; }
        public bool account_type_required { get; set; }
        public bool member_benefit_required { get; set; }
        public string duration { get; set; }
        public string week_days { get; set; }
        public List<Event_FloorPlans> event_floor_plans { get; set; }
        public string price_range { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime? event_end_date { get; set; }
        public DateTime next_available_date { get; set; }
    }

    public class EventData : Event
    {
        public DateTime? first_available_date { get; set; }
        public List<EventTimeV2> next_available_times { get; set; }
    }

    public class EventAccess
    {
        public bool IsValid { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class ScheduleV2
    {
        public int member_id { get; set; }
        public string member_name { get; set; }
        public string member_city { get; set; }
        public string member_state { get; set; }
        public int review_stars { get; set; }
        public decimal avg_review_value { get; set; }
        public int total_reviews { get; set; }
        public string member_url { get; set; }
        public string tag_1 { get; set; }
        public string tag_2 { get; set; }
        public string recommended_tag { get; set; }
        public int booked_count { get; set; }
        public string listing_image_url { get; set; }
        public List<string> times { get; set; }
    }

    public class MostBookedEvent
    {
        public int event_id { get; set; }
        public string event_name { get; set; }
        public string event_desc { get; set; }
    }

    public class MostBookedEventType
    {
        public int event_type_id { get; set; }
        public string event_type_name { get; set; }
        public string event_type_desc { get; set; }
    }

    public class EventV3
    {
        public int booked_count { get; set; }
        public List<EventV2> events { get; set; }
        public Visitation_Rule visitation_rule { get; set; }
        public string visitation_rule_desc { get; set; }
        public string passport_benefit_desc { get; set; }
        public string passport_event_name { get; set; }
        public bool show_complementary_msg { get; set; } = false;
        public string visitation_external_url { get; set; }
        public bool show_book_button { get; set; } = false;
        public bool show_private_request_content { get; set; } = false;
        public string private_booking_request_email { get; set; }   
        public List<TicketEventDetail2Model> ticket_events { get; set; }
    }

    public class RSVPEventLandingPage
    {
        public int booked_count { get; set; }
        public int total_reviews { get; set; }
        public int review_stars { get; set; }
        public decimal avg_review_value { get; set; }
        public EventV2 event_details { get; set; }
        public bool has_other_events_at_member { get; set; }
        public bool has_other_events_nearby { get; set; }
        public bool has_ticket_events { get; set; }
        public bool has_reviews { get; set; }
        public bool has_blog_articles { get; set; }
        //public Visitation_Rule visitation_rule { get; set; }
        //public string visitation_rule_desc { get; set; }
        //public string passport_benefit_desc { get; set; }
        //public string passport_event_name { get; set; }
        //public bool show_complementary_msg { get; set; } = false;
        //public string visitation_external_url { get; set; }
        //public bool show_book_button { get; set; } = false;
        public bool show_private_request_content { get; set; } = false;
        public string private_booking_request_email { get; set; }
        public string deposit_policy { get; set; }
        public string cancel_policy { get; set; }
        //public string event_review { get; set; }
        //public List<TicketEventDetail2Model> other_events_at_member { get; set; }
        //public List<TicketEventDetail2Model> other_events_nearby { get; set; }
        //public List<TicketEventDetail2Model> ticket_events { get; set; }
        //public List<UpcomingEventModel> blog_articles { get; set; }
    }

    public class EventDateV3
    {
        public int event_id { get; set; }
        public string event_name { get; set; }
        public string event_location { get; set; }
        public string event_desc { get; set; }
        public string duration { get; set; }
        public decimal fee_Per_person { get; set; }
        public decimal all_inclusive_price { get; set; }
        public int fee_type { get; set; }
        public int charge_fee { get; set; }
        public decimal qualified_purchase { get; set; }
        public int sort_order { get; set; }
        public bool member_benefit_required { get; set; }
        public bool account_type_required { get; set; }
        public bool required_login { get; set; }
        public string currency_symbol { get; set; }
        public string currency_code { get; set; }
        public string member_url { get; set; }
        public string week_days { get; set; }
        public string require_login_message { get; set; }
        public string event_image { get; set; } = string.Empty;
        public bool discount_only { get; set; }
        public bool charge_sales_tax { get; set; }
        public List<EventDateTimeV2> event_dates { get; set; }
        public List<DateTime> sold_out_dates { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime? event_end_date { get; set; }
        public bool is_prepaid { get; set; }
        public int min_persons { get; set; }
        public int max_persons { get; set; }
    }

    public class MemberDetails
    {
        public int member_id { get; set; }
        public string member_name { get; set; }
        public string member_url { get; set; }
    }


    public class EventDetails
    {
        public int event_id { get; set; }
        public string event_name { get; set; }
        public string event_location { get; set; }
        public string event_desc { get; set; }
        public string duration { get; set; }
        public decimal fee_Per_person { get; set; }
        public decimal all_inclusive_price { get; set; }
        public int fee_type { get; set; }
        public int charge_fee { get; set; }
        public decimal qualified_purchase { get; set; }
        public int sort_order { get; set; }
        public bool member_benefit_required { get; set; }
        public bool member_benfeit { get; set; }
        public string member_benefit_desc { get; set; }
        public bool account_type_required { get; set; }
        public bool passport_promoted_event { get; set; }
        public int min_persons { get; set; }
        public int max_persons { get; set; }
        public bool required_login { get; set; }
        public string currency_symbol { get; set; }
        public string currency_code { get; set; }
        public string member_url { get; set; }
        public string week_days { get; set; }
        public string require_login_message { get; set; }
        public string event_image { get; set; } = string.Empty;
        public bool discount_only { get; set; }
        public bool charge_sales_tax { get; set; }
        public List<Event_FloorPlans> event_floor_plans { get; set; }
        public string price_range { get; set; }
        public string min_lead_time { get; set; }
        public string max_lead_time { get; set; }
        public string venue_country { get; set; }
        public string venue_city { get; set; }
        public string venue_state { get; set; }
        public string address1 { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime? event_end_date { get; set; }
        public int region_id { get; set; }
        public int total_reviews { get; set; }
        public int review_stars { get; set; }
        public decimal avg_review_value { get; set; }
        public bool sold_out { get; set; }
    }
    public class EventV2 : EventDetails
    {
        public List<EventTimeV2> times { get; set; }
        public List<EventTimeV2> next_available_times { get; set; }
        public List<DateTime> sold_out_dates { get; set; }
        public bool is_prepaid { get; set; }
    }

    public class ProfileEvent : EventDetails
    {
        public bool is_prepaid { get; set; }

        public int booked_count { get; set; }

        public DateTime next_available_date { get; set; }
        public DateTime event_start_date { get; set; }

        public DateTime? event_end_date { get; set; }
        public string event_organizer_url { get; set; }
        public bool is_past_date { get; set; }
        public bool is_multiple_dates { get; set; }
        public int total_reviews { get; set; }
        public int review_stars { get; set; }
        public decimal avg_review_value { get; set; }
        public bool sold_out { get; set; }
    }
    public class EventDateTimes
    {
        public DateTime event_date { get; set; }

        public List<EventTimeV2> times { get; set; }
    }

    public class AvailableDaysEvent : EventDetails
    {
        public int booked_count { get; set; }
        public List<EventDateTimes> event_date_times { get; set; }
    }

    public class AvailableEventsForFutureDate
    {
        public DateTime event_date { get; set; }
        public DateTime start_time { get; set; }
    }

    public class EventTimeV2
    {
        public string start_time { get; set; }
        public string end_time { get; set; }
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public bool wait_list { get; set; }
        public bool sold_out { get; set; } = false;
        public int min_persons { get; set; }
        public int max_persons { get; set; }
        public int seats_left { get; set; }
        public int wait_list_left { get; set; }
    }

    public class EventTimeV3
    {
        public string start_time { get; set; }
        public string end_time { get; set; }
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public int wait_list_left { get; set; }
        public bool sold_out { get; set; } = false;
        public int min_persons { get; set; }
        public int max_persons { get; set; }
        public int seats_left { get; set; }
    }

    public class EventDateTimeV2
    {
        public DateTime event_date { get; set; }
        public List<EventTimeV3> times { get; set; }
    }

    public class TransportationModel
    {
        public string company_name { get; set; }
        public int id { get; set; }
    }

    public class TaskMemberDetails
    {
        public int Id { get; set; }
        public int TimeZoneId { get; set; }
        public int EnableSyncDate { get; set; }
        public int BillingPlan { get; set; }
        public string OrderPortApiKey { get; set; }
        public string OrderPortApiToken { get; set; }
        public string OrderPortClientId { get; set; }
        public bool EnablePayments { get; set; }
        public bool EnableVin65 { get; set; }
        public string Vin65Username { get; set; }
        public string Vin65Password { get; set; }
        public bool eWineryEnabled { get; set; }
        public string eWineryUsername { get; set; }
        public string eWineryPassword { get; set; }
        public bool EnableYelp { get; set; }
        public bool EnableOrderPort { get; set; }
        public bool EnableCommerce7 { get; set; }
        public string Commerce7Username { get; set; }
        public string Commerce7Password { get; set; }
        public string Commerce7Tenant { get; set; }
        public string Commerce7POSProfileId { get; set; }
        public bool UpsertFulfillmentDate { get; set; }
    }

    public class TransactionsForExport
    {
        public int ExportId { get; set; }
        public int PayStatus { get; set; }
        public int Id { get; set; }
    }

    public class GuestTags
    {
        public string tag { get; set; }
        public bool is_public { get; set; }
        public Common.TagType tag_type { get; set; }
        public string tag_type_desc { get; set; } = "NA";
    }

    public class Event_FloorPlans
    {
        public int floor_plan_id { get; set; }
        public string floor_plan_name { get; set; }
        public string technical_name { get; set; }
        public int sort_order { get; set; }
    }

    public class CheckAvailableQtyPrivatersvpModel
    {
        public bool party_can_seated { get; set; }
        public int max_qty_available { get; set; }
    }

    public class PassportEventAvailabilityV2Model
    {
        public string event_name { get; set; }
        public List<PassportEventDateV2> event_dates { get; set; }
    }

    public class PassportEventDateV2
    {
        public string event_date { get; set; }
        public List<PassportEventTimeV2> times { get; set; }
    }

    public class PassportEventTimeV2
    {
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string availability_string { get; set; }
        public DateTime start_date_time { get; set; }
        public DateTime end_date_time { get; set; }
    }

    public class PassportEventAvailabilityModel
    {
        public int slot_id { get; set; }

        public int slot_type { get; set; }
        public string start_time { get; set; }

        public string end_time { get; set; }

        public DateTime start_date_time { get; set; }

        public DateTime end_date_time { get; set; }

        public string event_name { get; set; }

        public string availability_string {
            get
            {
                return string.Format("{2} | {0} - {1}", start_time, end_time, event_name);
            }
        }
    }

    public class ScoutPromotion
    {
        public int promo_id { get; set; }
        public string promo_name { get; set; }

        public int member_id { get; set; }
        public string member_name { get; set; }
        public string member_city { get; set; }
        public int promo_used_count { get; set; }
        public string member_banner_image { get; set; }
        public string purchase_url { get; set; }
        public int total_reviews { get; set; }
        public int review_stars { get; set; }
        public int favorite_count { get; set; }
        public string promotion_type { get; set; }
        public string offer_name { get; set; }
        public string member_state { get; set; }
        public string currency { get; set; }
        public string offer_price { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }

    }

    //public class PromotionDetail
    //{
    //    public int id { get; set; }
    //    public string member_name { get; set; }
    //    public int promo_zone { get; set; }
    //    public string promo_schema { get; set; }
    //    public DateTime end_date { get; set; }
    //    public string promo_fine_print { get; set; }
    //    public string referral_code { get; set; }
    //    public string redemption_instructions { get; set; }
    //    public string purchase_url { get; set; }
    //}

    public class ClaimModel
    {
        public int id { get; set; }
    }

    public class PromotionDetail
    {
   
      public int promotions_Id { get; set; }
      public string promo_Name { get; set; }
      public string promo_fine_print { get; set; }
      public int promo_zone { get; set; }
      public string promo_value { get; set; }
      public int member_id { get; set; }
      public string member_name { get; set; }

      public string referral_code { get; set; }
      public string redemption_instructions { get; set; }
      public string promo_value_detail { get; set; }
      public int promotions_promo_Value { get; set; }    

      public string purchase_url { get; set; }
      public string member_business_phone { get; set; }
      //public string member_mobile_phone { get; set; }
      //public string member_fax_phone { get; set; }
      public UserAddress member_address { get; set; }

        public DateTime promo_end_date { get; set; }
        public decimal promo_offer_value { get; set; }
    }

    public class PromotionDetailUser : PromotionDetail
    {
        public int id { get; set; }
        public int user_Id { get; set; }

        public DateTime claim_date { get; set; }
        public DateTime expiration_date { get; set; }

        public DateTime? redemption_date { get; set; }

        public string promotion_code { get; set; }
    }
    public class CellarScoutOfferTypesModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class PromotionDetailMember : PromotionDetail
    {
        public int id { get; set; }
        public DateTime expiration_date { get; set; }

    }
    public class CellarScoutLocationsModel
    {
        public string state_code { get; set; }
        public string state { get; set; }
        public List<string> city { get; set; }
    }

    public class PrivateConfirmationMessageModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string confirmation_message { get; set; }
    }

    public class WineryDetailForEvent
    {
        public int MemberId { get; set; }
        public int RegionId { get; set; }
    }
}
