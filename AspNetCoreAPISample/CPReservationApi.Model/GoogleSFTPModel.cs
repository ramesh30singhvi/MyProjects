using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class CancellationPolicy
    {
        public List<RefundCondition> refund_conditions { get; set; }
    }

    public class Coordinates
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class Description
    {
        public List<LocalizedText> localized_texts { get; set; }
    }

    public class FeedMetadata
    {
        public int shard_id { get; set; }
        public int total_shards_count { get; set; }
        public string processing_instruction { get; set; }
        public long nonce { get; set; }
    }

    public class FeesAndTaxes
    {
        public PerTicketTax per_ticket_tax { get; set; }
    }

    public class FulfillmentType
    {
        public bool mobile { get; set; }
        public bool print_at_home { get; set; }
        public bool pickup { get; set; }
    }

    public class GoogleBusinessProfileName
    {
        public List<LocalizedText> localized_texts { get; set; }
    }

    public class LandingPage
    {
        public string url { get; set; }
    }

    public class LandingPageListView
    {
        public string url { get; set; }
    }

    public class LocalizedText
    {
        public string language_code { get; set; }
        public string text { get; set; }
    }

    public class SFTPLocation
    {
        //public SFTPLocation location { get; set; }
        public string place_id { get; set; }
    }

    public class Location3
    {
        public string place_id { get; set; }
        public PlaceInfo place_info { get; set; }
    }

    public class Operator
    {
        public GoogleBusinessProfileName google_business_profile_name { get; set; }
    }

    public class Option
    {
        public string id { get; set; }
        public Title title { get; set; }
        public Description description { get; set; }
        public LandingPage landing_page { get; set; }
        public LandingPageListView landing_page_list_view { get; set; }
        public int duration_sec { get; set; }
        public CancellationPolicy cancellation_policy { get; set; }
        public List<PriceOption> price_options { get; set; }
    }

    public class PerTicketTax
    {
        public string currency_code { get; set; }
        public int units { get; set; }
    }

    public class PlaceInfo
    {
        public string name { get; set; }
        public Coordinates coordinates { get; set; }
        public string phone_number { get; set; }
        public string website_url { get; set; }
        public string unstructured_address { get; set; }
    }

    public class Price
    {
        public string currency_code { get; set; }
        public int units { get; set; }
    }

    public class PriceOption
    {
        public string id { get; set; }
        public string title { get; set; }
        public Price price { get; set; }
        public FeesAndTaxes fees_and_taxes { get; set; }
    }

    public class SFTPProduct
    {
        public string id { get; set; }
        public Title title { get; set; }
        public Description description { get; set; }
        public Rating rating { get; set; }
        public List<Option> options { get; set; }
        public Operator @operator { get; set; }
        public List<RelatedLocation> related_locations { get; set; }
        public List<Location3> locations { get; set; }
        public string inventory_type { get; set; } = "INVENTORY_TYPE_DEFAULT";
        public string confirmation_type { get; set; } = "CONFIRMATION_TYPE_INSTANT";
        public FulfillmentType fulfillment_type { get; set; }
    }

    public class Rating
    {
        public double average_value { get; set; }
        public int rating_count { get; set; }
    }

    public class RefundCondition
    {
        public int min_duration_before_start_time_sec { get; set; }
        public int refund_percent { get; set; }
    }

    public class RelatedLocation
    {
        public SFTPLocation location { get; set; }
        public string relation_type { get; set; } = "RELATION_TYPE_RELATED_NO_ADMISSION";
    }

    public class GoogleSFTPModel
    {
        public FeedMetadata feed_metadata { get; set; }
        public List<SFTPProduct> products { get; set; }
    }

    public class Title
    {
        public List<LocalizedText> localized_texts { get; set; }
    }

    //public class Attribution
    //{
    //    public List<LocalizedText> localized_texts { get; set; }
    //}

    //public class CancellationPolicy
    //{
    //    public List<RefundCondition> refund_conditions { get; set; }
    //}

    //public class Coordinates
    //{
    //    public double latitude { get; set; }
    //    public double longitude { get; set; }
    //}

    //public class Description
    //{
    //    public List<LocalizedText> localized_texts { get; set; }
    //}

    //public class FeedMetadata
    //{
    //    public int shard_id { get; set; }
    //    public int total_shards_count { get; set; }
    //    public string processing_instruction { get; set; }
    //    public long nonce { get; set; }
    //}

    //public class FeesAndTaxes
    //{
    //    public PerTicketFee per_ticket_fee { get; set; }
    //    public PerTicketTax per_ticket_tax { get; set; }
    //}

    //public class FulfillmentType
    //{
    //    public bool mobile { get; set; }
    //    public bool print_at_home { get; set; }
    //    public bool pickup { get; set; }
    //}

    //public class GoogleBusinessProfileName
    //{
    //    public List<LocalizedText> localized_texts { get; set; }
    //}

    //public class LandingPage
    //{
    //    public string url { get; set; }
    //}

    //public class LandingPageListView
    //{
    //    public string url { get; set; }
    //}

    //public class LocalizedText
    //{
    //    public string language_code { get; set; }
    //    public string text { get; set; }
    //}

    //public class SFTPLocation
    //{
    //    //public SFTPLocation location { get; set; }
    //    public string place_id { get; set; }
    //    public PlaceInfo place_info { get; set; }
    //}

    //public class Location4
    //{
    //    public SFTPLocation location { get; set; }
    //}

    //public class MeetingPoint
    //{
    //    public SFTPLocation location { get; set; }
    //    public Description description { get; set; }
    //}

    //public class Operator
    //{
    //    public GoogleBusinessProfileName google_business_profile_name { get; set; }
    //    public List<SFTPLocation> locations { get; set; }
    //}

    //public class Option
    //{
    //    public string id { get; set; }
    //    public Title title { get; set; }
    //    public LandingPage landing_page { get; set; }
    //    public LandingPageListView landing_page_list_view { get; set; }
    //    public int duration_sec { get; set; }
    //    public CancellationPolicy cancellation_policy { get; set; }
    //    public List<OptionCategory> option_categories { get; set; }
    //    public List<RelatedLocation> related_locations { get; set; }
    //    public List<PriceOption> price_options { get; set; }
    //    public MeetingPoint meeting_point { get; set; }
    //}

    //public class OptionCategory
    //{
    //    public string label { get; set; }
    //}

    //public class PerTicketFee
    //{
    //    public string currency_code { get; set; }
    //    public int units { get; set; }
    //}

    //public class PerTicketTax
    //{
    //    public string currency_code { get; set; }
    //    public int units { get; set; }
    //}

    //public class PlaceInfo
    //{
    //    public string name { get; set; }
    //    //public Coordinates coordinates { get; set; }
    //    public string phone_number { get; set; }
    //    public string website_url { get; set; }
    //    public string unstructured_address { get; set; }
    //}

    //public class Price
    //{
    //    public string currency_code { get; set; }
    //    public int units { get; set; }
    //    public int? nanos { get; set; }
    //}

    //public class PriceOption
    //{
    //    public string id { get; set; }
    //    public string title { get; set; }
    //    public Price price { get; set; }
    //    public FeesAndTaxes fees_and_taxes { get; set; }
    //}

    //public class SFTPProduct
    //{
    //    public string id { get; set; }
    //    public Title title { get; set; }
    //    public Description description { get; set; }
    //    public Rating rating { get; set; }
    //    public List<ProductFeature> product_features { get; set; }
    //    public List<Option> options { get; set; }
    //    public List<RelatedMedium> related_media { get; set; }
    //    public Operator @operator { get; set; }
    //    public string inventory_type { get; set; }
    //    public string confirmation_type { get; set; }
    //    public FulfillmentType fulfillment_type { get; set; }
    //}

    //public class ProductFeature
    //{
    //    public string feature_type { get; set; }
    //    public Value value { get; set; }
    //}

    //public class Rating
    //{
    //    public double average_value { get; set; }
    //    public int rating_count { get; set; }
    //}

    //public class RefundCondition
    //{
    //    public int min_duration_before_start_time_sec { get; set; }
    //    public int refund_percent { get; set; }
    //}

    //public class RelatedLocation
    //{
    //    public SFTPLocation location { get; set; }
    //    public string relation_type { get; set; }
    //}

    //public class RelatedMedium
    //{
    //    public string url { get; set; }
    //    public string type { get; set; }
    //    public Attribution attribution { get; set; }
    //}

    //public class GoogleSFTPModel
    //{
    //    public FeedMetadata feed_metadata { get; set; }
    //    public List<SFTPProduct> products { get; set; }
    //}

    //public class Title
    //{
    //    public List<LocalizedText> localized_texts { get; set; }
    //}

    //public class Value
    //{
    //    public List<LocalizedText> localized_texts { get; set; }
    //}
}
