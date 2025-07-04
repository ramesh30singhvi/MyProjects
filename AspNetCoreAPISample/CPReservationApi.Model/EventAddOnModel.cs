using System;
using System.Collections.Generic;
using System.Text;
using uc = CPReservationApi.Common;

namespace CPReservationApi.Model
{
    public class EventAddOnModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public uc.AddOns.EventType EventType { get; set; }
        public int GroupId { get; set; }
    }

    public class AddOn_Group
    {
        public int id { get; set; }
        public int member_id { get; set; }
        public string name { get; set; }
        public int group_type { get; set; }
        public List<AddOn_Group_Items> addOn_group_items { get; set; }
    }

    public class AddOn_Group_Items
    {
        public int id { get; set; }
        public int addon_group_id { get; set; }
        public int addOn_item_id { get; set; }
        public decimal price { get; set; }
        public int sort_order { get; set; }
        public bool isdefault { get; set; }

        public int min_qty { get; set; }
        public int max_qty { get; set; }
        public bool included
        {
            get
            {
                bool _included = false;
                if (price == 0)
                    _included = true;
                return _included;
            }
        }
        public AddOn_Item addon_item { get; set; }
    }

    public class AddOn_Item
    {
        public int id { get; set; }
        public bool active { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public decimal retail_price { get; set; }
        public int category { get; set; }
        public int item_type { get; set; }
        public string image { get; set; }
        public decimal cost { get; set; }
        public bool taxable { get; set; }
        public int qty { get; set; } = 0;
        public bool calculate_gratuity { get; set; }
        public int department_id { get; set; }
    }

    public class AddOn_Item_Ext : AddOn_Item
    {
        public string category_desc { get; set; }
        public string item_type_desc { get; set; }
        public string department_desc { get; set; }

    }

    public class EventTypeModel
    {
        public int id { get; set; }
        public string event_type_name { get; set; }
        public string description { get; set; }
    }

    public class MemberStatsModel
    {
        public bool has_reviews { get; set; }
        public bool has_public_rsvp_events { get; set; }
        public bool has_public_ticket_events { get; set; }
        public bool has_trip_advisor_Id { get; set; }
        public bool has_nearby_events { get; set; }
        public int review_stars { get; set; }
        public decimal avg_review_value { get; set; }
        public int total_reviews { get; set; }
        public bool has_food_menu { get; set; }
        public bool has_product_menu { get; set; }
        public PromotionDetail profile_page_promo { get; set; }
    }

    public class MemberProductsModel
    {
        public int collection_id { get; set; }
        public string collection_name { get; set; }
        public List<ProductsModel> products { get; set; }
    }

    public class ProductsModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal RetailPrice { get; set; }
        public string sku { get; set; }
        public string description { get; set; }
        public string custom_field_1 { get; set; }
        public string custom_field_2 { get; set; }
    }

    public class MemberFoodMenuModel
    {
        public int group_id { get; set; }
        public string group_name { get; set; }
        public int group_type { get; set; }
        public List<FoodMenuModel> foodmenu { get; set; }
    }

    public class FoodMenuModel
    {
        public int item_id { get; set; }
        public string item_name { get; set; }
        public decimal price { get; set; }
        public string sku { get; set; }
        public string description { get; set; }
        public bool taxable { get; set; }
        public bool calculate_gratuity { get; set; }
        public int department_id { get; set; }
    }

    public class PriceRangeByRegionIdModel
    {
        public decimal min_price { get; set; }
        public decimal max_price { get; set; }
    }

    public class MembersByPriceRangeModel
    {
        public string member_name { get; set; }
        public string image { get; set; }
        public string region_name { get; set; }
        public int star { get; set; }
        public string url { get; set; }
        public decimal min_price { get; set; }
        public decimal max_price { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
        public int total_records { get; set; }
        public int member_id { get; set; }
        public string state { get; set; }
        public string city { get; set; }
    }

    public class Event_AddOns_AccountTypes
    {
        public int id { get; set; }
        public string contact_type_id { get; set; }
        public string contact_type { get; set; }
        public int member_benefit { get; set; }
        public bool member_benefit_req { get; set; }
        public decimal member_benefit_custom_value { get; set; }
    }

    public class EventAddOnsAccountTypesRequest
    {
        public int addon_group_id { get; set; }
        public int addon_group_items_id { get; set; }
        public int thirdparty_accounttypes_id { get; set; }
        public int member_benefit { get; set; }
        public bool member_benefit_req { get; set; }
        public decimal member_benefit_custom_value { get; set; }
    }

    public class DeleteEventAddOnsAccountTypesRequest
    {
        public int addon_group_id { get; set; }
        public int addon_group_items_id { get; set; }
        public int thirdparty_accounttypes_id { get; set; }
    }
}
