using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class ApiResponse
    {
        public int status { get; set; }
        public object response { get; set; }
        public object data { get; set; }
        public object meta { get; set; }
    }

    public class GetAllCustomersParams
    {
        public int? page { get; set; }
    }

    public class SearchCustomerParameters
    {
        public string name { get; set; }
        public string email { get; set; }
        public int? page { get; set; }
    }

    public class CreateCustomerAddress
    {
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address_type { get; set; }
        public string city { get; set; }
        public string company { get; set; }
        public string country_code { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string postal_code { get; set; }
        public string state_or_province { get; set; }
    }

    public class CreateCustomers
    {
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string phone { get; set; }
        public string notes { get; set; }
        public List<CreateCustomerAddress> addresses { get; set; }
    }

    public class SearchCustomerViewModel
    {
        public int status { get; set; }
        public object response { get; set; }
        public object meta { get; set; }
        public List<SearchCustomersData_VM> data { get; set; }
    }

    public class SearchCustomersData_VM
    {
        public object id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string notes { get; set; }
        public object addresses { get; set; }
        //public object lifetime_value { get; set; }
    }

    public class OrdersListParams
    {
        public int? page { get; set; }

        public int? customer_id { get; set; }
    }

    public class GetAllProductsParams
    {
        public string name { get; set; }
        public string keyword { get; set; }
        public int? page { get; set; }
    }

    public class CreateOrderBillingAddress
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string street_1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string email { get; set; }
    }

    public class CreateOrderShippingAddress
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string street_1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string email { get; set; }
    }

    public class CreateOrderProduct
    {
        public string name { get; set; }
        public int quantity { get; set; }
        public decimal price_inc_tax { get; set; }
        public decimal price_ex_tax { get; set; }
    }

    public class CreateOrderModel
    {
        public int customer_id { get; set; }
        public CreateOrderBillingAddress billing_address { get; set; }
        public List<CreateOrderShippingAddress> shipping_addresses { get; set; }
        public List<CreateOrderProduct> products { get; set; }
    }

    public class CustomUrl
    {
        public string url { get; set; }
        public bool is_customized { get; set; }
    }

    public class OptionValue
    {
        public int id { get; set; }
        public string label { get; set; }
        public int option_id { get; set; }
        public string option_display_name { get; set; }
        public int sort_order { get; set; }
        public ValueData value_data { get; set; }
        public bool is_default { get; set; }
    }

    public class Variant
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public string sku { get; set; }
        public int? sku_id { get; set; }
        public double? price { get; set; }
        public double calculated_price { get; set; }
        public double? sale_price { get; set; }
        public double? retail_price { get; set; }
        public double? map_price { get; set; }
        public double? weight { get; set; }
        public double calculated_weight { get; set; }
        public double? width { get; set; }
        public double? height { get; set; }
        public double? depth { get; set; }
        public bool is_free_shipping { get; set; }
        public double? fixed_cost_shipping_price { get; set; }
        public bool purchasing_disabled { get; set; }
        public string purchasing_disabled_message { get; set; }
        public string image_url { get; set; }
        public double cost_price { get; set; }
        public string upc { get; set; }
        public string mpn { get; set; }
        public string gtin { get; set; }
        public int inventory_level { get; set; }
        public int inventory_warning_level { get; set; }
        public string bin_picking_number { get; set; }
        public List<OptionValue> option_values { get; set; }
    }

    public class Image
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public bool is_thumbnail { get; set; }
        public int sort_order { get; set; }
        public string description { get; set; }
        public string image_file { get; set; }
        public string url_zoom { get; set; }
        public string url_standard { get; set; }
        public string url_thumbnail { get; set; }
        public string url_tiny { get; set; }
        public DateTime date_modified { get; set; }
    }

    public class PrimaryImage
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public bool is_thumbnail { get; set; }
        public int sort_order { get; set; }
        public string description { get; set; }
        public string image_file { get; set; }
        public string url_zoom { get; set; }
        public string url_standard { get; set; }
        public string url_thumbnail { get; set; }
        public string url_tiny { get; set; }
        public DateTime date_modified { get; set; }
    }

    public class ValueData
    {
        public List<string> colors { get; set; }
    }

    public class Option
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public string type { get; set; }
        public int sort_order { get; set; }
        public List<OptionValue> option_values { get; set; }
        public List<object> config { get; set; }
    }

    public class Datum
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string sku { get; set; }
        public string description { get; set; }
        public decimal weight { get; set; }
        public decimal width { get; set; }
        public decimal depth { get; set; }
        public decimal height { get; set; }
        public double price { get; set; }
        public double cost_price { get; set; }
        public double retail_price { get; set; }
        public double sale_price { get; set; }
        public double map_price { get; set; }
        public int tax_class_id { get; set; }
        public string product_tax_code { get; set; }
        public double calculated_price { get; set; }
        public List<int> categories { get; set; }
        public int brand_id { get; set; }
        public int? option_set_id { get; set; }
        public string option_set_display { get; set; }
        public int inventory_level { get; set; }
        public int inventory_warning_level { get; set; }
        public string inventory_tracking { get; set; }
        public int reviews_rating_sum { get; set; }
        public int reviews_count { get; set; }
        public object total_sold { get; set; }
        public double fixed_cost_shipping_price { get; set; }
        public bool is_free_shipping { get; set; }
        public bool is_visible { get; set; }
        public bool is_featured { get; set; }
        public List<int> related_products { get; set; }
        public string warranty { get; set; }
        public string bin_picking_number { get; set; }
        public string layout_file { get; set; }
        public string upc { get; set; }
        public string mpn { get; set; }
        public string gtin { get; set; }
        public string search_keywords { get; set; }
        public string availability { get; set; }
        public string availability_description { get; set; }
        public string gift_wrapping_options_type { get; set; }
        public List<object> gift_wrapping_options_list { get; set; }
        public int sort_order { get; set; }
        public string condition { get; set; }
        public bool is_condition_shown { get; set; }
        public int order_quantity_minimum { get; set; }
        public int order_quantity_maximum { get; set; }
        public string page_title { get; set; }
        public List<object> meta_keywords { get; set; }
        public string meta_description { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_modified { get; set; }
        public int view_count { get; set; }
        public object preorder_release_date { get; set; }
        public string preorder_message { get; set; }
        public bool is_preorder_only { get; set; }
        public bool is_price_hidden { get; set; }
        public string price_hidden_label { get; set; }
        public CustomUrl custom_url { get; set; }
        public int? base_variant_id { get; set; }
        public string open_graph_type { get; set; }
        public string open_graph_title { get; set; }
        public string open_graph_description { get; set; }
        public bool open_graph_use_meta_description { get; set; }
        public bool open_graph_use_product_name { get; set; }
        public bool open_graph_use_image { get; set; }
        public List<Variant> variants { get; set; }
        public List<Image> images { get; set; }
        public PrimaryImage primary_image { get; set; }
        public List<Option> options { get; set; }
    }

    public class Links
    {
        public string current { get; set; }
    }

    public class Pagination
    {
        public int total { get; set; }
        public int count { get; set; }
        public int per_page { get; set; }
        public int current_page { get; set; }
        public int total_pages { get; set; }
        public Links links { get; set; }
        public bool too_many { get; set; }
    }

    public class Meta
    {
        public Pagination pagination { get; set; }
    }

    public class ProductList
    {
        public List<Datum> data { get; set; }
        public Meta meta { get; set; }
    }

    public class BigCommerceProductsResponse : BaseResponse
    {
        public BigCommerceProductsResponse()
        {
            data = new List<Datum>();
        }
        public List<Datum> data { get; set; }
    }
}
