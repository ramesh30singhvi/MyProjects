using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Model
{
    public class CollectionItem
    {
        public int id { get; set; }
        public string collection_name { get; set; }

        public int sort_order { get; set; }

        public List<Product> items { get; set; }
    }

    public class ItemDimensions
    {
        public decimal length { get; set; }
        public decimal width { get; set; }
        public decimal height { get; set; }
    }

    public class Product
    {
        public int id { get; set; }
        public string name { get; set; }
        public string sku { get; set; }
        public decimal retail_price { get; set; }
        public string image { get; set; }
        public bool is_active { get; set; }
        public bool is_public { get; set; }
        public bool charge_tax { get; set; }
        public string custom_field_1 { get; set; }
        public string custom_field_2 { get; set; }
        public List<string> tags { get; set; }
        public int status { get; set; }
        public string category { get; set; }
        public int department_id { get; set; }
    }

    public class StoreProduct
    {
        public int id { get; set; }
        public bool active { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public string variant { get; set; }
        public decimal retail_price { get; set; }
        public int status { get; set; }
    }

    public class ProductDetails
    {
        public int id { get; set; }
        public string item_name { get; set; }
        public string item_desc { get; set; }
        public string item_sku { get; set; }
        public decimal retail_price { get; set; }
        public decimal cost { get; set; }
        public decimal sale_price { get; set; }
        public decimal club_price { get; set; }
        public string item_image { get; set; }
        public bool is_active { get; set; }
        public bool is_public { get; set; }
        public bool charge_tax { get; set; }
        public int item_category { get; set; }
        public bool display_in_pos { get; set; }
        public int unit_type { get; set; }
        public string custom_field_1 { get; set; }
        public string custom_field_2 { get; set; }
        public List<string> collections { get; set; }
        public List<string> tags { get; set; }
        public decimal item_weight { get; set; }
        public int weight_option { get; set; }
        public ItemDimensions item_dimensions { get; set; }
        public bool requires_shipping { get; set; }
        public string volume { get; set; }
        public int inventory_mode { get; set; }
        public string fulfillment_service { get; set; }
        public int in_stock_quantity { get; set; }
        public List<Variants> item_variants { get; set; }
        public int status { get; set; }
        public bool exclude_from_discounts { get; set; }
        public string external_url { get; set; }
        public int department_id { get; set; }
        public int vendor_id { get; set; }
        public int product_template { get; set; }
        public int meta_tax_type { get; set; }
        public int meta_item_type { get; set; }
        public string meta_brand_key { get; set; }
        public string meta_product_key { get; set; }
        public int access_and_security { get; set; }
        public List<int> access_security_tags { get; set; }
        public List<int> access_security_clubs { get; set; }
        public string access_security_name { get; set; }
        public string department_name { get; set; }
        public string vendor_name { get; set; }
        public string template_name { get; set; }
        public string tax_class_name { get; set; }
        public string image_url { get; set; }
    }

    public class Variants
    {
        public int id { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public decimal retail_price { get; set; }
        public int variant_id { get; set; }
    }

    public class ProductCollection
    {
        public int id { get; set; }

        public string collection_name { get; set; }

        public int sort_order { get; set; } = 1;

        public int member_id { get; set; }
    }


    public class POSKey
    {
        public int id { get; set; }

        public int product_id { get; set; }

        public int key_num { get; set; } = 1;

        public int member_id { get; set; }

        public DateTime? date_added { get; set; }

        public DateTime? date_modified { get; set; }
    }

    public class POSKeyOut
    {
        public int id { get; set; }

        public int product_id { get; set; }

        public int key_num { get; set; } = 1;

        public int member_id { get; set; }

    }

    public class RemovePOSKeyRequest
    {
        public int id { get; set; }
    }

    public class RemovePOSKeyResp
    {
        public int id { get; set; }
    }

    public class RemoveCollectionRequest
    {
        public int id { get; set; }
        public int member_id { get; set; }
    }

    public class RemoveItemRequest
    {
        public int id { get; set; }
    }

    public class AddUpdateItemRequest
    {
        public int member_id { get; set; }
        public int id { get; set; }
        public string item_name { get; set; }
        public string item_desc { get; set; }
        public string item_sku { get; set; }
        public decimal retail_price { get; set; }
        public decimal cost { get; set; }
        public decimal sale_price { get; set; }
        public decimal club_price { get; set; }
        //public string item_image { get; set; }
        public int status { get; set; }
        public bool charge_tax { get; set; }
        public int item_category { get; set; }
        public bool display_in_pos { get; set; }
        public int unit_type { get; set; }
        public string custom_field_1 { get; set; }
        public string custom_field_2 { get; set; }
        public List<int> collections { get; set; }
        public List<string> tags { get; set; }
        public decimal item_weight { get; set; }
        public int weight_option { get; set; }
        public ItemDimensions item_dimensions { get; set; }
        public bool requires_shipping { get; set; }
        public string volume { get; set; }
        public int inventory_mode { get; set; }
        public string fulfillment_service { get; set; }
        public int in_stock_quantity { get; set; }
        public List<int> item_variant_ids { get; set; }
        public int current_user { get; set; }
        public bool exclude_from_discounts { get; set; }
        public string external_url { get; set; }
        public int department_id { get; set; }
        public int vendor_id { get; set; }
        public int product_template { get; set; }
        public int meta_tax_type { get; set; }
        public int meta_item_type { get; set; }
        public string meta_brand_key { get; set; }
        public string meta_product_key { get; set; }
        public int access_and_security { get; set; }
        public string access_security_tags_Ids { get; set; }
        public string access_security_clubs_Ids { get; set; }
    }

    public class RemoveItemFromCollectionRequest
    {
        public int item_id { get; set; }
        public int collection_id { get; set; }
    }

    public class AddItemImageResponseModel
    {
        public string item_image_url { get; set; }
    }

    public class Department
    {
        public int id { get; set; }

        public string department_title { get; set; }

        public string department_code { get; set; }

        public int member_id { get; set; }
    }
}
