using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{

    public class WineryModel
    {
        public bool ChargeSalesTaxOnPrivateEvents { get; set; }
        public bool EnableClubemember { get; set; }
        public bool EnableClubSalesforce { get; set; }
        public bool EnableClubVin65 { get; set; }
        public bool EnableVin65 { get; set; }
        public string Vin65Password { get; set; }
        public string Vin65UserName { get; set; }
        public bool eMemberEnabled { get; set; }
        public string eMemberUserNAme { get; set; }
        public string eMemberPAssword { get; set; }
        public string SalesForceUserName { get; set; }
        public string SalesForcePassword { get; set; }
        public string SalesForceSecurityToken { get; set; }
        public bool EnableVintegrate { get; set; }
        public string VintegrateAPIPassword { get; set; }
        public string VintegrateAPIUsername { get; set; }
        public string VintegratePartnerID{ get; set; }
        public string EnabledCreditCards { get; set; }
        public string DirectionsURL { get; set; }
        public string DynamicPaymentDesc { get; set; }
        public string DisplayName { get; set; }
        public string SmsNumber { get; set; }
        public decimal BusinessPhone { get; set; }
        public bool EnableOrderPort { get; set; }
        public bool EnableClubOrderPort { get; set; }
        public string OrderPortClientId { get; set; }
        public string OrderPortApiKey { get; set; }
        public string OrderPortApiToken { get; set; }
        public bool EnableCommerce7 { get; set; }
        public bool EnableClubCommerce7 { get; set; }
        public string Commerce7Username { get; set; }
        public string Commerce7Password { get; set; }
        public string Commerce7Tenant { get; set; }
        public string Commerce7POSProfileId { get; set; }
        public bool EnableShopify { get; set; }
        public bool EnableClubShopify { get; set; }
        public string ShopifyPublishKey { get; set; }
        public string ShopifySecretKey { get; set; }
        public string ShopifyAppPassword { get; set; }
        public string ShopifyAppStoreName { get; set; }
        public int SubscriptionPlan { get; set; }
        public string AttendeeAppUsername { get; set; }
        public string ReServeCloudSiteId { get; set; }
        public string ReServeCloudApiUserName { get; set; }
        public string ReServeCloudApiPassword { get; set; }
        public bool HiddenMember { get; set; }
        public bool EnableClubeCellar { get; set; }
        public bool EnableClubAMS { get; set; }
        public bool EnableClubMicroworks { get; set; }
        public bool EnableClubCoresense { get; set; }
        public string CurrencySymbol { get; set; }

        public string ClubStatusField { get; set; }
        public string UpsertOrderDateAs { get; set; }

        public string UpsertShipDateAs { get; set; }

        public Address WineryAddress { get; set; } = new Address();
        public bool EnableService { get; set; }
        public string ThirdPartyTransaction { get; set; }

        public string WebSiteUrl { get; set; }

        public string EmailAddress { get; set; }

        public bool IsActive { get; set; }

        public string BillingFirstName { get; set; }

        public string BillingLastName { get; set; }

        public string BillingEmailAddress { get; set; }

        public string MemberProfileUrl { get; set; }
        public string AppellationName { get; set; }
        //EnableGoogleSync, GoogleUsername, GooglePassword
        public bool EnableGoogleSync { get; set; }
        public string GoogleUsername { get; set; }
        public string GooglePassword { get; set; }

        public int TimezoneId { get; set; }
        public string DecryptKey { get; set; }
        public string SALT { get; set; }
        public bool EnableUpsertCreditCard { get; set; }
        public int InventoryMode { get; set; }

        public bool UpsertFulfillmentCommerce7 { get; set; }

        public bool EnableBigCommerce { get; set; }
        public bool EnableClubBigCommerce { get; set; }
        public string BigCommerceStoreId { get; set; }
        public string BigCommerceAceessToken{ get; set; }
        public bool EnableCustomOrderType { get; set; }
        public string CustomOrderType { get; set; }
    }

    public class WidgetModel
    {
        public string display_name { get; set; }
        public string purchase_url { get; set; }
        public string google_analytics_id { get; set; }
        public string url { get; set; }
        public string custom_styles { get; set; }
        public WidgetCutomStyles widget_cutom_styles { get; set; }
        public string business_phone { get; set; }
        public string website_url { get; set; }
    }
    
    public class WidgetCutomStyles
    {
        public string widget_backgound { get; set; }
        public string widget_text { get; set; }
        public string widget_footer_text { get; set; }
        public string widget_search_button { get; set; }
        public string widget_search_button_text { get; set; }
        public string widget_time_slot_button { get; set; }
        public string widget_time_slot_button_text { get; set; }
        public string widget_header_back_ground { get; set; }
        public string widget_header_text { get; set; }
        public string widget_links { get; set; }
        public bool widget_enabled { get; set; }
        public string widget_font { get; set; }
        public string widget_font_size { get; set; }
        public string club_badge_text { get; set; }
        public string club_badge_background { get; set; }
    }

    public class WineryReviewsModel
    {
        public int total_count { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public DateTime member_since { get; set; }
        public int metric_1 { get; set; }
        public int metric_2 { get; set; }
        public int metric_3 { get; set; }
        public int metric_4 { get; set; }
        public string recommend { get; set; }
        public decimal star { get; set; }
        public string event_name { get; set; }
        public DateTime start_date { get; set; }
        public string description { get; set; }
    }

    public class WineryReviewViewModel : WineryReviewsModel
    {
        public string member_since_format { get; set; }
        public string start_date_format { get; set; }
        public string user_name { get; set; }
    }

    public class WineryImageModel
    {
        public string image_name { get; set; }
        public int image_index { get; set; }
    }
}
