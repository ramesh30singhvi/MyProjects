using System;
using System.Collections.Generic;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Model
{
    public class EventModel
    {
        public int EventID { get; set; }
        public int MemberID { get; set; }
        public int LeadTime { get; set; }
        public int MaxLeadTime { get; set; }
        public bool EventStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ChargeFee { get; set; }
        public bool ChargeSalesTax { get; set; }
        public bool TaxGratuity { get; set; }
        public int LocationId { get; set; }
        public string EventName { get; set; }
        public int EmailContentID { get; set; }
        public DiscountType MemberBenefit { get; set; }
        public string member_url { get; set; }
        public int FeeType { get; set; }
        public decimal Cost { get; set; }
        public int EventTypeId { get; set; }
        public int MeetingBehavior { get; set; }
        public bool account_type_required { get; set; }
        public bool member_benefit_required { get; set; }
    }

    public class EventReservationChangesModel
    {
        public int MemberID { get; set; }
        public decimal FeePerPerson { get; set; }
        public bool ChargeSalesTax { get; set; }
        public decimal GratuityPercentage { get; set; }
        public bool TaxGratuity { get; set; }
        public int FeeType { get; set; }
        public string Zip { get; set; }
        public string Address1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }

        public int item_id { get; set; }
        public int group_id { get; set; }
        public decimal price { get; set; }
        public bool Taxable { get; set; }
        public bool calculate_gratuity { get; set; }

        public int min_item_id { get; set; }
        public int min_group_id { get; set; }
        public decimal min_price { get; set; }
        public bool min_Taxable { get; set; }
        public bool min_calculate_gratuity { get; set; }

        public int max_item_id { get; set; }
        public int max_group_id { get; set; }
        public decimal max_price { get; set; }
        public bool max_Taxable { get; set; }
        public bool max_calculate_gratuity { get; set; }
    }

    public class UpdateReservationChangesByEventIdModel
    {
        public int event_id { get; set; }
        public decimal all_inclusive_price { get; set; }
        public decimal all_inclusive_min_price { get; set; }
        public decimal all_inclusive_max_price { get; set; }
    }

    public class EventRuleModel
    {
        public int TotalSeats { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int EventId { get; set; }
        public int ExceptionId { get; set; } = 0;
        public int LocationId { get; set; }
        public int chargefee { get; set; }
        public  string eventname { get; set; }
        public int EmailContentID { get; set; }
        public int WineryID { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public int MeetingBehavior { get; set; }
        public int TimeZoneId { get; set; }
        public Common.EventStatus Status { get; set; }
    }

    public class WineryReviews
    {
        public int ReviewCount { get; set; }
        public int ReviewStars { get; set; }
        public decimal avg_review_value { get; set; }
    }

    public class EventRuleDetailsModel
    {
        public EventRuleDetailsModel()
        {
            eventmodel = new EventRuleModel();
        }
        public EventRuleModel eventmodel { get; set; }
        public short DaysOfWeek { get; set; }
        public short MinPersons { get; set; }
        public short MaxPersons { get; set; }
        public bool Status { get; set; }
    }

    public class EventExceptionModel
    {
        public int ExceptionId { get; set; }
        public string ExceptionType { get; set; }
        public int EventId { get; set; }
        public DateTime ExceptionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MinPersons { get; set; }
        public int MaxPersons { get; set; }
        public int TotalSeats { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int EventRuleId { get; set; }
        public int LocationId { get; set; }
        public string Note { get; set; }
    }

    public class ClaimOfferRequest
    {
        public int user_id { get; set; }
        public int promotion_id { get; set; }
    }

    public class RedeemPromotionRequest
    {
        public int promotion_id { get; set; }
        public int user_id { get; set; }
        public string promotion_code { get; set; }
    }

    public class WelcomeDashboardDataModel
    {
        public List<LinkContent> Announcements { get; set; }
        public List<Article> PlatformUpdates { get; set; }
        public List<Article> BecomeAPro { get; set; }
        public List<Article> GettingStarted { get; set; }
        public List<Article> BetterGuests { get; set; }
        public List<Article> SystemUpdates { get; set; }
    }

    public class LinkContent
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public string Content { get; set; }
        public DateTime DatePublished { get; set; }
        public int CreatedById { get; set; }
        public DateTime DateModified { get; set; }
        public int ModifiedById { get; set; }
        public string LinkURL { get; set; }
        public Common.LinkContentType ContentType { get; set; }
    }

    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public DateTime ArticleDate { get; set; }
        public int ViewCount { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public string CustomURL { get; set; }
        public DateTime DateModified { get; set; }
        public string FriendlyUrl { get; set; }
    }

    public class EventExperienceModel
    {
        public int business_id { get; set; }
        public int event_id { get; set; }
        public string purchase_url { get; set; }
        public string event_name { get; set; }
        public string event_image { get; set; }
        public string city { get; set; }
        public int reviews { get; set; }
        public decimal star { get; set; }
        public string business_name { get; set; }
        public string region_name { get; set; }
        public bool member_benefit_req { get; set; }
        public bool acount_type_req { get; set; }
    }

    public class EventExperienceFilterModel
    {
        public int? NoOfRecord { get; set; }
        public int? AppleationId { get; set; }
        public int? SubRegionId { get; set; }
        public string OrderBy { get; set; }
        public int? DaysAfter { get; set; }
        public string SubRegionIds { get; set; }
        public string EventTypes { get; set; }
        public string Reviews { get; set; }
        public string PopularTags { get; set; }
        public string Varietals { get; set; }
        public string NotableFeatures { get; set; }
        public bool? IsAdvancedFilter { get; set; }
        public int? WineryId { get; set; }
        public bool isUniqueRecords { get; set; }
    }

    public class BookedExperincesCache 
    {
        public DateTime LastFetchDate { get; set; }
        public string JsonResult { get; set; }
        public int Id { get; set; }
    }
}
