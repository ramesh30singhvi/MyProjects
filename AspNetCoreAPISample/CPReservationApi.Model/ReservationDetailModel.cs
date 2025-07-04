using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Model
{
    public class ReservationStatusModel
    {
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public int location_id { get; set; }
        public int cancel_lead_time { get; set; }
        public DateTime event_start_date { get; set; }
        public int user_id { get; set; }
        public int? pre_assign_server_id { get; set; }
        public List<int> pre_assign_table_ids { get; set; }
        public int seated_status { get; set; }
    }

    public class ReservationDetail2Model
    {
        public int reservation_id { get; set; }
        public int member_id { get; set; }
        public string booking_code { get; set; }
        public int referral_type { get; set; }
        public UserDetail2 user_detail { get; set; }
        public PayCard pay_card { get; set; }

    }

    public class UserDetail2
    {
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        //public int customer_type { get; set; }
        public string phone_number { get; set; }
        public ReservationUserAddress address { get; set; }
        //public List<string> contact_types { get; set; } = new List<string>();
        //public AccountNote account_note { get; set; }
        //public string mobile_phone { get; set; }
        //public MobileNumberStatus mobile_phone_status { get; set; }
        //public RSVPPostCaptureStatus survey_status { get; set; } = RSVPPostCaptureStatus.NA;
        //public DateTime? survey_expire_date { get; set; } = null;
        //public DateTime? modified_date { get; set; } = null;
        //public RSVPPostCaptureStatus waiver_status { get; set; } = RSVPPostCaptureStatus.NA;
        //public int region_most_visited { get; set; }
        //public string mobile_number { get; set; }
        //public int mobile_number_status { get; set; }
        //public DateTime? marketing_opt_in_date { get; set; } = null;
        //public string email_marketing_status { get; set; } = null;
    }

    public class ReservationDetailModel
    {
        public int reservation_id { get; set; }
        public int member_id { get; set; }
        public int? event_id { get; set; }
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public string event_name { get; set; }
        public int location_id { get; set; }
        public string location_name { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime event_end_date { get; set; }
        public decimal fee_per_person { get; set; }
        public int fee_type { get; set; }
        public int charge_fee { get; set; }
        public int total_guests { get; set; }
        public string guest_note { get; set; }
        public int referral_type { get; set; }
        public string referral_type_text { get; set; }
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
        public int lead_time { get; set; }
        public int cancel_lead_time { get; set; }
        public DateTime booking_date { get; set; }
        public string booking_code { get; set; }
        public byte status { get; set; }
        public decimal fee_due { get; set; }
        public decimal balance_due
        {
            get
            {
                return fee_due - amount_paid;
            }
        }
        public decimal amount_paid { get; set; }
        public bool full_paid { get; set; }
        public string concierge_name { get; set; }
        public string referral_name { get; set; }
        public string cancel_policy { get; set; }
        public List<Additional_guests> additional_guests { get; set; }
        public UserDetail user_detail { get; set; }
        public PayCard pay_card { get; set; }
        public List<Reservation_Addon> reservation_addon { get; set; }
        public ReservationHolderMetrics reservation_holder_metrics { get; set; }
        public List<ReservationChangeLog> reservation_status_log { get; set; }
        public List<ReservationChangeLog> reservation_emails_log { get; set; }
        public List<PaymentModel> reservation_payments_log { get; set; }
        public List<ReservationChangeLog> reservation_discounts_log { get; set; }
        public int seated_status { get; set; }
        public int payment_status { get; set; }
        public int delay_in_minutes { get; set; }
        public string charge_fee_description { get; set; }
        public string tags { get; set; }
        public string user_tags { get; set; }
        public int reservation_type { get; set; }
        public string payment_details { get; set; }
        public int table_status_group_id { get; set; }
        public int? pre_assign_server_id { get; set; }
        public string pre_assigned_server_first_name { get; set; }
        public string pre_assigned_server_last_name { get; set; }
        public string pre_assigned_server_color { get; set; }
        public string assigned_server_first_name { get; set; }
        public string assigned_server_last_name { get; set; }
        public string assigned_server_color { get; set; }
        public List<int> pre_assign_table_ids { get; set; }
        public DateTime? seating_start_time { get; set; }
        public DateTime? seating_end_time { get; set; }
        public DateTime? status_changed_date { get; set; }
        public string winery_name { get; set; }
        public string location_address1 { get; set; }
        public string location_address2 { get; set; }
        public string location_city { get; set; }
        public string location_state { get; set; }
        public string location_zip { get; set; }
        public string location_country { get; set; }
        public string location_phone { get; set; }
        public decimal convenience_fee { get; set; }
        public decimal gratuity_amount { get; set; }
        public int transportation_id { get; set; }
        public string transportation_name { get; set; } = string.Empty;
        public string currency_symbol { get; set; }
        public string currency_code { get; set; }
        public string deposit_policy { get; set; }
        public string booking_guid { get; set; }
        public bool allow_cancel { get; set; }
        public List<AttendeeQuestion> attendee_questions { get; set; }
        public string member_url { get; set; }
        public string member_website { get; set; }
        public string destination_name { get; set; }
        public decimal addon_total { get; set; }
        public decimal sales_tax_percent { get; set; }
        public decimal sales_tax { get; set; }
        public decimal taxes_and_fees { get; set; }
        public bool show_convenience_fee { get; set; } = false;

        public string rms_sku { get; set; } = "";

        public string event_description { get; set; } = "";
        //public int required_hdyh { get; set; }
        public bool limit_my_account { get; set; }
        public string google_calendar_event_url { get; set; } = "";
        //public int region_id { get; set; }

        public int assigned_server_id { get; set; } = 0;

        public bool soft_assigned_tables { get; set; }
        public int floor_plan_id { get; set; }
        public string floor_plan_name { get; set; }
        public string floor_plan_technical_name { get; set; }
        public int assigned_floor_plan_id { get; set; }
        public List<int> assign_table_ids { get; set; }
        public string contact_types { get; set; }
        public TicketOrderDetail ticket_order { get; set; }
        public decimal calculated_gratuity_amount { get; set; }
        public bool sms_opt_out { get; set; } = false;
        public bool ignore_discount { get; set; } = false;
        public string cancellation_reason { get; set; }
        public CancellationReasonSetting cancellation_reason_setting { get; set; }
        public List<CancellationReason> cancellation_reasons { get; set; }
        public string personal_message { get; set; }
        public DateTime? cancel_by_date { get; set; }
        public string cancel_message { get; set; }
        public int region_id { get; set; }
        public string region_name { get; set; }
        public string region_friendly_url { get; set; }
        public string timezone_name { get; set; }
    }

    public class ReservationDetailV3Model
    {
        public int reservation_id { get; set; }
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime event_end_date { get; set; }
        public int total_guests { get; set; }
        public string guest_note { get; set; }
        public string internal_note { get; set; }
        public int user_id { get; set; }
        public string email { get; set; }
        public PayCard pay_card { get; set; }
        public string tags { get; set; }
        public string personal_message { get; set; }

        public int MemberBenefit { get; set; }
        public int EventId { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal FeePerPerson { get; set; }
        public int FeeType { get; set; }
        public string EventName { get; set; }
        public decimal GratuityAmount { get; set; }
        public decimal SalesTaxPercentage { get; set; }
        public decimal SalesTax { get; set; }
        public decimal TransactionFee { get; set; }
        public int TransactionType { get; set; }
        public int ChargeFee { get; set; }

        public int FloorPlanId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int CustomerType { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int TransportationId { get; set; }
        public string TransportationName { get; set; }
        public decimal GratuityPercentage { get; set; }
        public bool TaxGratuity { get; set; }
        public int LocationId { get; set; }
        public int EmailContentId { get; set; }
        public int CancelLeadTime { get; set; }
        public string EventLocation { get; set; }
        public string ConciergeNote { get; set; }
        public int AffiliateID { get; set; }
        public int ReferralId { get; set; }

        public List<Reservation_Addon> reservation_addon { get; set; }
    }

    public class TicketOrderDetail
    {
        public int ticket_order_id { get; set; }
        public List<TicketLevels> ticketlevels { get; set; }
    }

    public class TicketLevels
    {
        public int qty { get; set; }
        public string ticket_level_name { get; set; }
    }

    public class AttendeeQuestion
    {
        public string question { get; set; }
        public string answer { get; set; }
    }

    public class CancellationReason
    {
        public int id { get; set; }
        public string reason { get; set; }
    }

    public class ReservationHolderMetrics
    {
        public int visits_count { get; set; }
        public int cancellations_count { get; set; }
        public int no_shows_count { get; set; }

    }

    public class Additional_guests
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public int user_id { get; set; }
        public RSVPPostCaptureStatus survey_status { get; set; } = RSVPPostCaptureStatus.NA;
        public DateTime? survey_expire_date { get; set; } = null;
        public DateTime? modified_date { get; set; } = null;
        public RSVPPostCaptureStatus waiver_status { get; set; } = RSVPPostCaptureStatus.NA;
    }

    public class UserDetail
    {
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int customer_type { get; set; }
        public string phone_number { get; set; }
        public ReservationUserAddress address { get; set; }
        public List<string> contact_types { get; set; } = new List<string>();
        public AccountNote account_note { get; set; }
        public string mobile_phone { get; set; }
        public MobileNumberStatus mobile_phone_status { get; set; }
        public RSVPPostCaptureStatus survey_status { get; set; } = RSVPPostCaptureStatus.NA;
        public DateTime? survey_expire_date { get; set; } = null;
        public DateTime? modified_date { get; set; } = null;
        public RSVPPostCaptureStatus waiver_status { get; set; } = RSVPPostCaptureStatus.NA;
        public int region_most_visited { get; set; }
        public string mobile_number { get; set; }
        public int mobile_number_status { get; set; }
        public DateTime? marketing_opt_in_date { get; set; } = null;
        public string email_marketing_status { get; set; } = null;
    }

    public class ReservationUserAddress
    {
        public string city { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
        public string country { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
    }
    public class PayCard
    {
        public string number { get; set; }
        public string cust_name { get; set; }
        public string exp_month { get; set; }
        public string exp_year { get; set; }
        public CardEntry card_entry { get; set; }
        public ApplicationType application_type { get; set; }
        public string application_version { get; set; }
        public string terminal_id { get; set; }
        public string card_reader { get; set; }
        public string cvv2 { get; set; }
        public string card_token { get; set; }
        public string card_type { get; set; }
        public string card_last_four_digits { get; set; } = "";
        public string card_first_four_digits { get; set; } = "";
    }

    public class Reservation_Addon
    {
        public int qty { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public decimal cost { get; set; }
        public decimal price { get; set; }
        public int category { get; set; }
        public int item_type { get; set; }
        public string image { get; set; }
        public int item_id { get; set; }
        public int group_item_id { get; set; }
        public int group_id { get; set; }
        public string group_name { get; set; }
        public int group_type { get; set; }
        public int sort_order { get; set; }
        public bool taxable { get; set; }
        public bool calculate_gratuity { get; set; }

        public string item_category_name { get; set; }
    }
    public class EmailReservationDetailsModel
    {
        public EmailReservationDetailsModel()
        {
            ReservationId = 0;
            WineryID = 0;
            // Dim BookingCode As String
            BookingDate = DateTime.UtcNow;
            GuestName = "";
            GuestEmail = "";
            GuestPhone = "";
            GuestWPhone = "";
            GuestAddress1 = "";
            GuestAddress2 = "";
            GuestCity = "";
            GuestState = "";
            GuestZipCode = "";
            Fee = 0;
            ChargeFee = 0;
            FeePaid = 0;
            GuestCount = 0;
            MemberName = "";
            MemberPhone = "";
            MemberEmail = "";
            MemberAddress1 = "";
            MemberAddress2 = "";
            MemberCity = "";
            MemberState = "";
            MemberZipCode = "";
            EventDate = DateTime.UtcNow;
            StartTime = default(System.TimeSpan);
            EndTime = default(System.TimeSpan);
            EventName = "";
            EventLocation = "";
            Notes = "";
            Status = 0;
            InternalNote = "";
            ConciergeNote = "";
            BookedById = 0;
            CancelLeadTime = 0;
            EmailContentID = 0;
            EmailTemplateID = 0;
            Host = "";
            AffiliateID = 0;
            Content = "";
            DestinationName = "";
            locAddress1 = "";
            locAddress2 = "";
            locCity = "";
            locState = "";
            locZip = "";
            BookingGUID = "";
        }
        public int ReservationId { get; set; }
        public int TimeZoneId { get; set; }
        public int WineryID { get; set; }
        public DateTime BookingDate { get; set; }
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public string GuestPhone { get; set; }
        public string GuestWPhone { get; set; }
        public string GuestAddress1 { get; set; }
        public string GuestAddress2 { get; set; }
        public string GuestCity { get; set; }
        public string GuestState { get; set; }
        public string GuestZipCode { get; set; }
        public decimal Fee { get; set; }
        public int ChargeFee { get; set; }
        public decimal FeePaid { get; set; }
        public short GuestCount { get; set; }
        public string MemberName { get; set; }
        public string MemberPhone { get; set; }
        public string MemberEmail { get; set; }
        public string MemberAddress1 { get; set; }
        public string MemberAddress2 { get; set; }
        public string MemberCity { get; set; }
        public string MemberState { get; set; }
        public string MemberZipCode { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string EventName { get; set; }
        public string EventLocation { get; set; }
        public string Notes { get; set; }
        public int Status { get; set; }
        public string InternalNote { get; set; }
        public string ConciergeNote { get; set; }
        public int BookedById { get; set; }
        public int CancelLeadTime { get; set; }
        public int EmailContentID { get; set; }
        public int EmailTemplateID { get; set; }
        public string Host { get; set; }
        public int AffiliateID { get; set; }
        public string Content { get; set; }
        public string DestinationName { get; set; }
        public string locAddress1 { get; set; }
        public string locAddress2 { get; set; }
        public string locCity { get; set; }
        public string locState { get; set; }
        public string locZip { get; set; }
        public string BookingCode { get; set; }
        public string ProfileUrl { get; set; } = "";
        public decimal FeePerPerson { get; set; }
        public decimal SalesTax { get; set; }
        public decimal GratuityAmount { get; set; }
        public string BookingGUID { get; set; }
        public int slotid { get; set; }
        public int slottype { get; set; }
        public string MapAndDirectionsURL { get; set; }
        public ReferralType referralType { get; set; }
        public int EventId { get; set; }
        public string CancellationUsers { get; set; }
        public string ConfirmationUsers { get; set; }
        public string ItineraryGUID { get; set; }
        public string EventConfirmationMessage { get; set; }
        public string EventCancellationMessage { get; set; }
        public int LocationId { get; set; }
        public string CancellationReason { get; set; }
        public DateTime ReservationInviteExpirationDateTime { get; set; }
        public bool HasInvite { get; set; }
        public List<AttendeeQuestion> attendee_questions { get; set; }
        public int region_id { get; set; }
        public int EventTypeId { get; set; }
        public DateTime? cancel_by_date { get; set; }
        public string cancel_message { get; set; }
        public string timezone_name { get; set; }
    }

    public class AddOnModel
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public bool Taxable { get; set; }
        public string Image { get; set; }
        public int Category { get; set; }
        public int ItemType { get; set; }
        public int Qty { get; set; }
        public int PurchaseId { get; set; }
        public decimal SalesTax { get; set; }
        public int PurchasedId { get; set; }
        public bool Active { get; set; }

        public bool CalculateGratuity { get; set; }

    }

    public class UserModel
    {
        public string AffiliateEmail { get; set; }
        public string AffiliateName { get; set; }
        public string AffiliateCompany { get; set; }
    }

    public class ReservationChangeLog
    {
        public int change_log_id { get; set; }
        public DateTime reservation_status_date { get; set; }
        public string reservation_status_user { get; set; }
        public string log_note { get; set; }
        public string cancellation_reason { get; set; }
    }

    public class CreateReservation
    {
        public int Id { get; set; }
        public string BookingCode { get; set; }
        public Common.ResponseStatus Status { get; set; }
        public Common.SaveType SaveType { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public int error_type { get; set; }
        public string BookingGUID { get; set; }
        //public string City { get; set; }
        //public string State { get; set; }
    }

    public class ReservationStatusNotes
    {
        public int Id { get; set; }
        public int RefId { get; set; }
        public int ReservationStatus { get; set; }
        public string Note { get; set; }
        public DateTime NoteDate { get; set; }
        public string CurrentUser { get; set; }
        public int WineryID { get; set; }
        public int StatusType { get; set; }
    }

    public class ReservationConflicts
    {
        public ReservationConflicts()
        {
            error_data = new ErrorData();
        }
        public bool success { get; set; } = false;
        public int error_type { get; set; }
        public string extra_info { get; set; } = "";
        public string description { get; set; } = "";
        public ErrorData error_data { get; set; }
    }

    public class ErrorData
    {
        public string event_name { get; set; }
        public string location_name { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
    }

    public class SurveyWaiverStatus
    {
        public Common.Common.RSVPPostCaptureStatus survey_status { get; set; } = RSVPPostCaptureStatus.NA;
        public DateTime? survey_expire_date { get; set; } = null;
        public Common.Common.RSVPPostCaptureStatus waiver_status { get; set; } = RSVPPostCaptureStatus.NA;
        public DateTime? modified_date { get; set; } = null;
    }

    public class ReservationServer
    {
        public int reservation_id { get; set; }
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }
    public class ReservationDetailV2Model
    {
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public DateTime event_start_date { get; set; }
        public int total_guests { get; set; }
        public DateTime event_end_date { get; set; }
        public int reservation_id { get; set; }
        public int event_id { get; set; }
        public int member_id { get; set; }
        public int location_id { get; set; }
        public int seated_status { get; set; }
        public bool exported { get; set; }
        public string user_image { get; set; }
        public decimal fee_due { get; set; }
        public decimal amount_paid { get; set; }
        public bool full_paid { get; set; }
        public decimal refund_paid { get; set; }
        public int charge_fee { get; set; }
        public decimal fee_per_person { get; set; }
        public int fee_type { get; set; }
        public bool has_payment_decline { get; set; }
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public int customer_type { get; set; }
        public string phone_number { get; set; }
        public int delay_in_minutes { get; set; }
        public bool has_review { get; set; }
        public string event_name { get; set; }
        public string location_name { get; set; }
        public DateTime booking_date { get; set; }
        public string booking_code { get; set; }
        public ReservationServer reservation_server { get; set; }
        public int referral_type { get; set; }
        public string booked_by_name { get; set; }
        public int affiliate_id { get; set; }
        public string concierge_name { get; set; }
        public string referral_name { get; set; }
        public int time_zone_id { get; set; }
        public int? pre_assign_server_id { get; set; }
        public int transportation_id { get; set; }
        public string transportation_name { get; set; } = string.Empty;
        public int cancel_lead_time { get; set; }
        public string cancel_policy { get; set; }
        public byte status { get; set; }
        public bool show_sales_tax { get; set; }
        public decimal sales_tax { get; set; }
        public decimal discount_amount { get; set; }
        public decimal gratuity_amount { get; set; }
        public List<Reservation_Addon> reservation_addon { get; set; }
        public bool soft_assigned_tables { get; set; }
        public List<int> pre_assign_table_ids { get; set; }
        public int floor_plan_id { get; set; }
        public string floor_plan_name { get; set; }
        public string table_names { get; set; }
        public string contact_types { get; set; }
        public int region_id { get; set; }
        public DateTime? cancel_by_date { get; set; }
        public string cancel_message { get; set; }
        public string timezone_name { get; set; }
    }

    public class ReservationDetailV4Model
    {
        public DateTime event_start_date { get; set; }
        public int total_guests { get; set; }
        public DateTime event_end_date { get; set; }
        public int reservation_id { get; set; }
        public decimal fee_per_person { get; set; }
        public int fee_type { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone_number { get; set; }
        public string event_name { get; set; }
        public string booking_code { get; set; }
        public string booking_guid { get; set; }
        public string business_phone { get; set; }
        public string member_country { get; set; }
        public string member_name { get; set; }
        public string location_Address1 { get; set; }
        public string location_Address2 { get; set; }
        public string location_city { get; set; }
        public string location_state { get; set; }
        public string location_zip { get; set; }
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public int region_id { get; set; }
        public int member_id { get; set; }
        public TimeSpan event_start_time { get; set; }
        public DateTime? cancel_by_date { get; set; }
        public string cancel_message { get; set; }
    }

    public class ReservationPaymentLogModel
    {
        public PayCard pay_card { get; set; }
        public List<PaymentModel> reservation_payments_log { get; set; }
    }

    public class ReservationNoteLogModel
    {
        public string tags { get; set; }
        public string user_tags { get; set; }
        public string InternalNote { get; set; }
        public string Notes { get; set; }
        public string account_notes { get; set; }
        public string ConciergeNote { get; set; }
        public List<Additional_guests> additional_guests { get; set; }
        public int total_guests { get; set; }
        public bool CovidSurveyEnabled { get; set; }
        public bool WaiverEnabled { get; set; }
        public List<AttendeeQuestion> attendee_questions { get; set; }
        public int? event_id { get; set; }
        public int reservation_id { get; set; }
        public string ipBrowserDetails { get; set; }
    }

    public class ReservationTransactionDetailModel
    {
        public decimal feedue { get; set; }
        public decimal TransactionFee { get; set; }
        public bool RefundFees { get; set; }
        public int Status { get; set; }
    }

    public class ReservationDataModel
    {
        public int reservation_id { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime event_end_date { get; set; }
        public short total_guests { get; set; }
        public string guest_note { get; set; }
        public string booking_code { get; set; }
        public byte status { get; set; }
        public string concierge_note { get; set; }
        public string internal_note { get; set; }
        public ReservationDataHolder reservation_holder { get; set; }
        public int reservation_seated_status { get; set; }
        public int payment_status { get; set; }
        public int? pre_assign_server_id { get; set; }
        public List<int> pre_assign_table_ids { get; set; }
        public DateTime? seating_start_time { get; set; }
        public DateTime? seating_end_time { get; set; }
        public int assigned_server_id { get; set; }
        public string floor_plan_name { get; set; }
        public int assigned_floor_plan_id { get; set; }
        public int ticket_order_id { get; set; }
        public string country { get; set; }
        public List<int> assign_table_ids { get; set; }
    }

    public class ReservationDataHolder
    {
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int customer_type { get; set; }
        public string phone { get; set; }
    }

    public class ReservationDataEvent
    {
        public int event_id { get; set; }
        public DateTime event_date { get; set; }
        public DateTime event_date_end { get; set; }
        public string event_name { get; set; }
        public List<ReservationDataEventSchedule> event_times { get; set; }
    }

    public class ReservationDataEventSchedule
    {
        public List<ReservationDataModel> reservations { get; set; }
    }

    public class RSVPReviewModel
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
        public int recommend { get; set; }
        public int review_value { get; set; }
    }

    public class GetLocationMapViewModel
    {
        public string map_html { get; set; }
    }
}

