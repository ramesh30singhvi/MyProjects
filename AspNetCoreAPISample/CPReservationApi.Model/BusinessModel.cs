using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CPReservationApi.Model
{
    public class SubscriptionPlans
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal monthly_fee { get; set; }
        public decimal startup_fee { get; set; }
        public decimal transaction_fee_widget { get; set; }

        public int plan_frequency { get; set; } = 0;
    }

    public class SubscriptionYearlyMonthlyPlans
    {
        public List<SubscriptionPlans> YearlySubscriptionPlans { get; set; }
        public List<SubscriptionPlans> MonthlySubscriptionPlans { get; set; }
    }

    public class DestinationDetails
    {
        public int region_id { get; set; }
        public string region_name { get; set; }
        public string region_friendly_url { get; set; }
        public string region_banner_image { get; set; }
        public string region_image_url { get; set; }
        public string region_page_intro { get; set; }
        public string region_page_desc { get; set; }
        public string state_code { get; set; }
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
        public List<BusinessDetails> business_details { get; set; }
        public List<RSVPEventTypes> rsvp_event_types { get; set; }
    }

    public class BusinessDetails
    {
        public int business_id { get; set; }
        public string business_name { get; set; }
        public string business_type { get; set; }
        public string business_city { get; set; }
        public string business_state { get; set; }
        public string listing_image_url { get; set; }
        public string business_page_url { get; set; }
        public string business_region { get; set; }
        public string business_sub_region { get; set; }
        public bool is_favorites { get; set; }
        public int reviews { get; set; }
        public int review_stars { get; set; }
        public decimal avg_review_value { get; set; }
        public List<POI> poi { get; set; }
        //public List<string> notable_features { get; set; }
        //public List<string> varietals { get; set; }
    }

    public class POI
    {
        [JsonPropertyName("attribute_type")]
        public string AttributeType { get; set; }
        [JsonPropertyName("attributes")]
        [JsonIgnore]
        public string Attributes { get; set; }
        [JsonPropertyName("attribute_values")]
        public string[] AttributeValues
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.Attributes))
                    return this.Attributes.Split(',');
                else
                    return "".Split(',');
            }
        }
    }

    public class RSVPEventTypes
    {
        public int id { get; set; }
        public string event_type_name { get; set; }
        public string description { get; set; }
    }

    public class EventTypes
    {
        public int id { get; set; }
        public string event_type_name { get; set; }
    }

    public class EventTypeDestinationModel
    {
        public int member_id { get; set; }
        public string display_name { get; set; }
        public string purchase_url { get; set; }
        public string city { get; set; }
        public int reviews { get; set; }
        public decimal star { get; set; }
        public string recommendation_tag { get; set; }
        public string tag_1 { get; set; }
        public string tag_2 { get; set; }
        public int member_ava { get; set; }
        public string attributes { get; set; }
        public int region_id { get; set; }
        //public List<MemberAreaModel> member_areas { get; set; }
        public int member_type { get; set; }
        public int featured_count { get; set; }
        public int billing_plan { get; set; }
        public int event_id { get; set; }
    }

    public class MemberAreaModel
    {
        public int member_id { get; set; }
        public int region_id { get; set; }
        public string region_name { get; set; }
    }

    //public class PointsofInterest
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}
}
