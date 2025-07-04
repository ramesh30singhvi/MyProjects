using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static CPReservationApi.Common.Email;

namespace CPReservationApi.Model
{
    public class ReservationModel
    {
        public int reservation_id { get; set; }  
        public string event_location { get; set; }
        public DateTime event_start_date { get; set; }
        public DateTime event_end_date { get; set; }
        //public TimeSpan start_time { get; set; } 
        //public TimeSpan end_time { get; set; } 
        public decimal fee_per_person { get; set; } 
        public short total_guests { get; set; } 
        public DateTime booking_date { get; set; } 
        public string guest_note { get; set; } 
        public byte referral_type { get; set; } 
        public string booking_code { get; set; } 
        public byte status { get; set; } 
        public string concierge_note { get; set; } 
        public decimal fee_due { get; set; } 
        public decimal amount_paid { get; set; }
        public bool full_paid { get; set; }
        //public int slot_id { get; set; }
        //public int slot_type { get; set; }
        public string internal_note { get; set; } 
        public string country { get; set; } 
        public string concierge_name { get; set; } 
        public string referral_name { get; set; }
        public ReservationHolder reservation_holder { get; set; }
        public int reservation_seated_status { get; set; }
        public int delay_in_minutes { get; set; }
        public int payment_status { get; set; }
        public int location_id { get; set; }
        public int? pre_assign_server_id { get; set; }
        public List<int> pre_assign_table_ids { get; set; }
        public DateTime? seating_start_time { get; set; }
        public DateTime? seating_end_time { get; set; }
        public string booking_guid { get; set; }
        public decimal convenience_fee { get; set; }

        public decimal gratuity_amount { get; set; }

        public int cancel_lead_time { get; set; }
        public bool allow_cancel { get; set; }

        public int assigned_server_id { get; set; }
        public bool soft_assigned_tables { get; set; }
        public int floor_plan_id { get; set; }
        public string floor_plan_name { get; set; }
        public string floor_plan_technical_name { get; set; }
        public int assigned_floor_plan_id { get; set; }
        public int ticket_order_id { get; set; }
        public List<int> assign_table_ids { get; set; }
        public bool addons_available { get; set; }
    }

    public class ReservationHolder
    {
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int customer_type { get; set; }
        public string phone { get; set; }
        public string mobile_phone { get; set; }
        public MobileNumberStatus mobile_phone_status { get; set; }
        public ReservationUserAddress address { get; set; }
        public List<string> contact_types { get; set; } = new List<string>();
        public AccountNote account_note { get; set; }
    }

    public class ReservationEvent
    {
        public int event_id { get; set; }
        public DateTime event_date { get; set; }
        public DateTime event_date_end { get; set; }
        public string event_name { get; set; }
        public string event_technical_location { get; set; }
        public int table_status_group_id { get; set; }
        public int member_id { get; set; }
        public List<ReservationEventSchedule> event_times { get; set; }        
    }

    public class ReservationEventSchedule
    {
        public int slot_id { get; set; }
        public int slot_type { get; set; }
        public string time { get; set; }
        public int event_duration_minutes { get; set; }
        
        public List<ReservationModel> reservations { get; set; }
    }

    public class ReservationChargeModel
    {
        public int ReservationId { get; set; }
        public decimal BalanceDue { get; set; }
    }

    public class AvlQtyForReservationIdModel
    {
        public int FloorPlanId { get; set; }
        public int Qty { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDatetime { get; set; }
        public int SlotId { get; set; }
        public int RsvpQty { get; set; }
    }

    public class UpdateReservationSeatedStatusModel
    {
        public int reservation_id { get; set; }
        public int reservation_seated_status { get; set; }
        public string reservation_seated_status_desc { get; set; } = "UNSEATED";
        public int update_status { get; set; }
        public string update_status_desc { get; set; }
        public string message { get; set; }
    }

    public class ReservationInviteModel
    {
        public int id { get; set; }
        public int reservation_id { get; set; }
        public int user_id { get; set; }
        public ReservationInviteStatus status { get; set; }
        public bool reminder_sent { get; set; }
        public DateTime expiration_date_time { get; set; }
        public DateTime invite_date_time { get; set; }
        public string reservation_invite_guid { get; set; }
        public DateTime update_date_time { get; set; }
        public DateTime event_date { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string booking_code { get; set; }
        public string event_name { get; set; }
        public int member_id { get; set; }
        public string member_phone { get; set; }
    }

    public class CreateReservationInvite
    {
        public int reservation_id { get; set; }
        public int user_id { get; set; }
        public string expiration_date_time { get; set; }
    }

    public class SetReservationInviteStatusRequest
    {
        public int reservation_id { get; set; } = 0;
        public string reservation_invite_guid { get; set; } = string.Empty;
        public int status { get; set; }
    }

    public class SetReservationInviteReminderStatusRequest
    {
        public int reservation_id { get; set; } = 0;
        public string reservation_invite_guid { get; set; } = string.Empty;
    }

    public class GetReservationV2ByWineryId_DetailResult
    {
        public string GuestName;

        public string PhoneNum;

        public int TotalGuests;

        public decimal FeePerPerson;

        public decimal BalanceDue;

        public string AccountType;

        public string ContactTypes;

        public string ReferredBy;

        public string InternalNote;

        public string GuestNote;

        public int MaxPersons;

        public string EventName;

        public string EventTime;

        public int Guests;

        public int sortorder;

        public DateTime start;

        public string LocationName;

        public string Email;

        public decimal FeeDue;
    }

    public class GetReservationV2ByWineryIdResult
    {
        public string LastName;

        public string FirstName;

        public string PhoneNum;

        public int TotalGuests;

        public decimal FeePerPerson;

        public decimal Discount;

        public decimal PurchaseTotal;

        public string AccountType;

        public string BookingDate;

        public string ReferredBy;

        public string InternalNote;

        public int MaxPersons;

        public string EventName;

        public string EventTime;

        public int Guests;

        public int sortorder;

        public DateTime start;

        public string HDYH;
    }

    public class GetFinancialV2ReportByMonthResult
    {
        public DateTime PaymentDate;

        public DateTime DateBooked;

        public DateTime EventDate;

        public string ConfirmNum;

        public string UserName;

        public decimal Eventamt;

        public decimal Paidamt;

        public decimal BalanceDue;

        public DateTime TXNDate;

        public string TXNType;

        public string PayCardType;

        public string PayCardNumber;

        public string Approval;

        public string TransactionID;

        public string ExportType;
    }

    public class GetTransactionReportByMonthResult
    {
        public int Id;

        public int ReservationId;

        public string BookingCode;

        public DateTime EventDate;

        public TimeSpan StartTime;

        public TimeSpan EndTime;

        public int SlotId;

        public int SlotType;

        public int UserId;

        public string FirstName;

        public string LastName;

        public string UserName;

        public string Email;

        public string ProductType;

        public string SKU;

        public string ProductName;

        public int Qty;

        public decimal Price;

        public DateTime TransactionDate;

        public decimal SalesTax;

        public decimal SalesTaxPercentage;

        public decimal GratuityAmount;

        public int PaymentId;

        public string TransID;

        public string TenderType;

        public decimal Extended;
    }

    public class ReservationReviewRequest
    {
        public string booking_guid { get; set; }
        public int member_id { get; set; }
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
        public int review_value { get; set; }
    }

    public class UserReservationsModel
    {
        public int reservation_id { get; set; }
        public int member_id { get; set; }
        public string member_name { get; set; }
        public string guest_last_name { get; set; }
        public string event_name { get; set; }
        public string booking_code { get; set; }
        public DateTime event_date { get; set; }
        public ReservationStatus status { get; set; }
        public bool is_past_event { get; set; }
        public DateTimeOffset booking_date { get; set; }
        public Int16 total_guests { get; set; }
        public string purchase_url { get; set; }
        public string booking_guid { get; set; }
        public bool allow_cancel { get; set; }
        public string destination_name { get; set; }
        public DateTime event_end_time { get; set; }
        public bool limit_my_account { get; set; }
        public string cancel_policy { get; set; }
        public string cancel_time { get; set; }
        public string member_business_phone { get; set; }
        public string status_css_class { get; set; }
        public string status_text { get; set; }
        public bool is_reviewed { get; set; }
        public int star { get; set; }
    }

    public class BusinessSearchResultModel
    {
        public BusinessSearchResultType business_search_result_type { get; set; }
        public int business_type { get; set; }
        public string display_name { get; set; }
        public string purchase_url { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string region_url { get; set; }
        public string event_title { get; set; }
        public DateTime event_start_date { get; set; }
        public string event_city { get; set; }
        public string event_state { get; set; }
        public string friendly_url { get; set; }
        public string event_organizer_name { get; set; }
        public int billing_plan { get; set; }
        public bool go_to_landing { get; set; }
    }

    public class BusinessWinerySearchResultModel
    {
        public string DisplayName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PurchaseURL { get; set; }
        public int BillingPlan { get; set; }
        public int WineryType { get; set; }
        public string Region { get; set; }
        public string RegionURL { get; set; }
        public bool GoToLanding { get; set; }

        //public string FriendlyURL { get; set; }
        //public string Country { get; set; }
        //public string Zip { get; set; }
        //public string FriendlyName { get; set; }
        //public int Appelation { get; set; }
        //public int WineryAva { get; set; }
    }

    public class BusinessEventSearchResultViewModel
    {
        public string EventTitle { get; set; }
        public DateTime EventStartDate { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string EventOrganizerName { get; set; }
        public int EventId { get; set; }
        public string FriendlyURL
        {
            get
            {
                return string.Format("{0}-{1}", Regex.Replace(EventTitle.ToLower(), "[^A-Za-z0-9 -]+", "").Replace("  ", " ").Replace(" ", "-").TrimEnd('-'), EventId);
            }
        }
    }
}
