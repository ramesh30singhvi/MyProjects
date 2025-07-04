using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static CPReservationApi.Common.Email;

namespace CPReservationApi.Model
{
    public class UserDetailModel
    {
        public UserDetailModel()
        {
            address = new UserAddress();
        }
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public UserAddress address { get; set; }
        public string phone_number { get; set; } = "";
        public string mobile_number { get; set; }
        public int mobile_number_status { get; set; }
        public string membership_number { get; set; }
        public List<int> roles { get; set; }
        public int visits_count { get; set; }
        public int cancellations_count { get; set; }
        public int no_shows_count { get; set; }
        public int completed_count { get; set; }
        public string color { get; set; }
        public int customer_type { get; set; }
        public List<string> contact_types { get; set; } = new List<string>();
        public List<string> contact_types_id { get; set; } = new List<string>();
        //public int customer_type { get; set; }
        public string company_name { get; set; }
        public string gender { get; set; }
        public string title { get; set; }
        public string work_number { get; set; }
        //public int affiliate_type { get; set; }
        public string cell_phone { get; set; }
        public AccountNote account_note { get; set; }
        //public string title { get; set; }
        public decimal ltv { get; set; }
        public DateTime? last_order_date { get; set; }
        public bool member_status { get; set; }
        public bool is_restricted { get; set; }
        public int order_count { get; set; }
        public DateTime last_updated_date_time { get; set; }

        public string gateway_cust_id { get; set; }

        public string password_change_key { get; set; }

        public DateTime date_created { get; set; }
        public Common.Common.RSVPPostCaptureStatus survey_status { get; set; } = Common.Common.RSVPPostCaptureStatus.NA;
        public DateTime? survey_expire_date { get; set; } = null;
        public DateTime? survey_modified_date { get; set; } = null;
        public Common.Common.RSVPPostCaptureStatus waiver_status { get; set; } = Common.Common.RSVPPostCaptureStatus.NA;
        public int region_most_visited { get; set; }
        public DateTime? marketing_opt_in_date { get; set; } = null;
        public string email_marketing_status { get; set; } = null;
        public DateTime? birth_date { get; set; } = null;
        public bool sms_opt_out { get; set; } = false;
        public int last_reservation_id { get; set; }
        public DateTime? last_check_in_date { get; set; }
        public string user_tags { get; set; }
        public string user_image { get; set; }
    }

    public class UserDetail2Model
    {
        public UserDetail2Model()
        {
            address = new UserAddress();
        }
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public UserAddress address { get; set; }
        public string phone_number { get; set; } = "";
        public string mobile_number { get; set; }
        public int mobile_number_status { get; set; }
        public string color { get; set; }
        public int region_most_visited { get; set; }

        public string company_name { get; set; }
        public DateTime birth_date { get; set; }
        public string work_phone_str { get; set; }
        public int concierge_type { get; set; }
        public string title { get; set; }
        public string website { get; set; }
        public string gender { get; set; }
        public int age { get; set; }
        public int role_id { get; set; }
        public bool is_concierge { get; set; }
        public bool weekly_newsletter { get; set; }
}

    public class ConciergeUserDetailModel
    {
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string home_phone { get; set; }
        public string work_phone { get; set; }
        public string mobile_number { get; set; }
        public bool is_favorite { get; set; }
        public string company_name { get; set; }
        public string title { get; set; }
    }

    public class UpdateConciergeUserModel
    {
        public int user_id { get; set; }
        public int concierge_id { get; set; }
        public bool is_favorite { get; set; }
    }

    public class GuestDetailModel
    {
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone_number { get; set; }
        public string work_phone { get; set; }
        public string membership_number { get; set; }
        public string color { get; set; }
        public string company_name { get; set; }
        public string title { get; set; }
        public string affiliate_type { get; set; }
        public DateTime login_date { get; set; }
    }

    public class User2Model
    {
        public int user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public bool is_conceirge { get; set; }
        public List<int> roles { get; set; }
    }

    public class UserAddress
    {
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
        public string country { get; set; }
    }

    public class AccountNote
    {
        public string note_id { get; set; }
        public string subject { get; set; }
        public string note { get; set; } ="";
        public string modified_by { get; set; } = "";
        public DateTime? note_date { get; set; }
        public DateTime? date_added { get; set; }
        public DateTime? date_modified { get; set; }
    }

    public class GuestPerformance
    {
        public int user_id { get; set; }
        public List<int> roles { get; set; }
        public int visits_count { get; set; }
        public int cancellations_count { get; set; }
        public int no_shows_count { get; set; }
        public int completed_count { get; set; }
        public string color { get; set; }
        public AccountNote account_note { get; set; }
        public string mobile_number { get; set; }
        public int mobile_number_status { get; set; }
        public int region_most_visited { get; set; }
        public string zip_code { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string phone_number { get; set; }
        public bool sms_opt_out { get; set; } = false;
        public int last_reservation_id { get; set; }
        public DateTime? last_check_in_date { get; set; } = null;
        public DateTime? birth_date { get; set; } = null;
        public string user_tags { get; set; }
        public string user_image { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string company_name { get; set; }
        public string title { get; set; }
        public string work_number { get; set; }
        public string gender { get; set; }
    }

    public class ContactSalesRequest
    {
        public string name { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
        public string company_name { get; set; }
        public string message { get; set; }
        public string subject { get; set; }
        public Common.Common.MessageType message_type { get; set; } = Common.Common.MessageType.Sales;
    }

    public class VerificationEmailRequest
    {
        public string business_email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int member_id { get; set; }
    }

    public class VerifyAccountRequest
    {
        public string verification_code { get; set; }
    }

    public class SystemNewUserRequest
    {
        public string email { get; set; }
    }

    public class Commerce7CustomerModel
    {
        public string CustId { get; set; }
        //public string ValidPhoneNumber { get; set; }
        public bool Exceeded { get; set; }

        public bool AlreadyExists { get; set; } = false;
    }

    public class UsersForSync
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string UserName { get; set; }
    }

    public class MyAccountDetailsModel
    {
        public string user_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company_name { get; set; }
        public DateTime? birth_date { get; set; }
        public string country { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string cell_phone_str { get; set; }
        public string home_phone_str { get; set; }
        public string work_phone_str { get; set; }
        public int preferred_appellation { get; set; }
        public bool is_concierge { get; set; }
        public int? concierge_type { get; set; }
        public string title { get; set; }
        public string website { get; set; }
        public string gender { get; set; }
        public int age { get; set; }
        public int role_id { get; set; }
        public int favorites_count { get; set; }
        public int earned_points { get; set; }
        //public int ClaimedPoints { get; set; }
        public int next_reward_points { get; set; }
        //public int CalendarYearCheckInCount { get; set; }
        public int goal_percentage { get; set; }
        public string user_image { get; set; }
        public bool system_site_gated { get; set; }
        public string site_wide_message { get; set; }
        public List<UserReservationsViewModel> user_reservation { get; set; }
        public List<UserOrdersViewModel> user_orders { get; set; }
    }

    public class MyAccountDetailsV2Model
    {
        public string user_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company_name { get; set; }
        public DateTime? birth_date { get; set; }
        public string country { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string cell_phone_str { get; set; }
        public string home_phone_str { get; set; }
        public string work_phone_str { get; set; }
        public int preferred_appellation { get; set; }
        public bool is_concierge { get; set; }
        public int? concierge_type { get; set; }
        public string title { get; set; }
        public string website { get; set; }
        public string gender { get; set; }
        public int age { get; set; }
        public int role_id { get; set; }
        public int favorites_count { get; set; }
        public int earned_points { get; set; }
        //public int ClaimedPoints { get; set; }
        public int next_reward_points { get; set; }
        //public int CalendarYearCheckInCount { get; set; }
        public int goal_percentage { get; set; }
        public string user_image { get; set; }
        public bool system_site_gated { get; set; }
        public string site_wide_message { get; set; }
        public int reservation_count { get; set; }
        public int ticket_orders_count { get; set; }
        public int waitlist_count { get; set; }
        public int itinerary_count { get; set; }
    }

    public class UserOrdersViewModel
    {
        public string event_title { get; set; }
        public string member_name { get; set; }
        //public DateTimeOffset StartDateTime { get; set; }
        //public DateTimeOffset EndDateTime { get; set; }
        public int order_id { get; set; }
        //public DateTimeOffset OrderDate { get; set; }
        //public Decimal OrderTotal { get; set; }
        public int no_of_tickets { get; set; }
        public string order_guid { get; set; }
        //public int EventId { get; set; }
        public string purchase_url { get; set; }
        public bool is_self_print { get; set; }
        public int primary_category { get; set; }

        public string start_date_time_formated { get; set; }
        public string end_date_time_formated { get; set; }
        public string order_date_formated { get; set; }
        public bool is_single_date { get; set; }
        public string order_total_text { get; set; }
        public string event_url { get; set; }
    }

    public class UserReservationsViewModel
    {
        public int reservation_id { get; set; }
        public int member_id { get; set; }
        public string member_name { get; set; }
        public string guest_last_name { get; set; }
        public string event_name { get; set; }
        public string booking_code { get; set; }
        //public DateTime EventDate { get; set; }
        public ReservationStatus status { get; set; }
        //public bool? IsPastEvent { get; set; }
        //public DateTimeOffset? BookingDate { get; set; }
        public int total_guests { get; set; }
        public int star { get; set; }
        public string purchase_url { get; set; }
        public string booking_guid { get; set; }
        public bool allow_cancel { get; set; }
        public string destination_name { get; set; }
        //public DateTime EventEndTime { get; set; }
        public bool limit_my_account { get; set; }
        public string covid19_required { get; set; }
        public string waiver_required { get; set; }
        public DateTime? survey_date { get; set; }
        public DateTime? survey_expiry_date { get; set; }
        public bool? is_waiver { get; set; }
        //public string CancelPolicy { get; set; }
        //public string CancelTime { get; set; }
        //public string MemberBusinessPhone { get; set; }

        public string event_date_format { get; set; }
        public string status_text { get; set; }
        public string status_css_class { get; set; }
        public bool is_reviewed { get; set; }
        public string event_time_format { get; set; }
        //public string EventEndTimeFormat { get; set; }
    }

    public class MyAccountDataViewModel
    {
        public string user_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public bool is_concierge { get; set; }
        public int? concierge_type { get; set; }
        public int itinerary_count { get; set; }
        public int reservations_count { get; set; }
        public int ticket_order_count { get; set; }
        public int waitlist_count { get; set; }
        public int favorites_count { get; set; }
        public int special_offer_count { get; set; }
        public string image_url { get; set; }
    }

    public class UserFavoriteMemberViewModel : UserFavoriteMemberModel
    {
        public string address { get; set; }
    }

    public class UserFavoriteMemberModel
    {
        public int id { get; set; }
        public string appelation { get; set; }
        public string member_ava { get; set; }
        public string display_name { get; set; }
        public string purchase_url { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public int reviews { get; set; }
        public decimal star { get; set; }
    }

    public class FavoriteEventViewModel : FavoriteEventModel
    {
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
    }

    public class FavoriteEventModel
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
        public int? location_id { get; set; }
        public string state_name { get; set; }
        public int? tickets_sold { get; set; }
        public int max_capacity { get; set; }
        public int? Offer_id { get; set; }
        public string offer_member_name { get; set; }
        public bool is_favorites { get; set; }
        public string time_zone { get; set; }
    }

    //public class SaveUserImageModel
    //{
    //    public int user_id { get; set; }
    //    public IFormFile file { get; set; }
    //}

    public class UpdateAccountRequest
    {
        //public string user_id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company_name { get; set; }
        public DateTime? birth_date { get; set; }
        public string country { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string cell_phone_str { get; set; }
        public string home_phone_str { get; set; }
        //public string WorkPhoneStr { get; set; }
        public int preferred_appellation { get; set; }
        //public bool IsConcierge { get; set; }
        //public int? ConciergeType { get; set; }
        //public string Title { get; set; }
        //public string Website { get; set; }
        //public string Gender { get; set; }
        //public int Age { get; set; }
        //public int RoleId { get; set; }
    }

    public class ChangePasswordModel
    {
        public string email { get; set; }
        public string password { get; set; }
    }

    public class SaveEmailOptionModel
    {
        //public int user_id { get; set; }
        public string email { get; set; }
        //public string first_name { get; set; }
        //public string last_name { get; set; }
        //public bool opt_in_status { get; set; } = true;
        public bool is_subscribed { get; set; } = true;
    }

    public class CreateAccountModel
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string user_name { get; set; }
        public string zip_code { get; set; }
        public string password { get; set; }
        public int destination_id { get; set; }
        public bool is_concierge { get; set; }
        public int concierge_type { get; set; }
        public string phone_number { get; set; }
        public string country { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string company_name { get; set; }
    }

}
