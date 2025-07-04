using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class ShopifyCustomerList
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string state { get; set; }
        public List<Address> addresses { get; set; }
        public DefaultAddress default_address { get; set; }
    }

    public class ShopifyCustomerListModel
    {
        public int status { get; set; }
        public Response response { get; set; }
        public object previous_page_info { get; set; }
        public object next_page_info { get; set; }
        public List<ShopifyCustomerList> customers { get; set; }
    }

    public class Response
    {
        public string success { get; set; }
    }

    public class ShopifyAddress
    {
        public object id { get; set; }
        public object customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
        public string province_code { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public bool @default { get; set; }
    }

    public class DefaultAddress
    {
        public long id { get; set; }
        public long customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public object company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
        public string province_code { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public bool @default { get; set; }
    }

    public class ShopifyCustomer
    {
        public long id { get; set; }
        public string email { get; set; }
        public bool accepts_marketing { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int orders_count { get; set; }
        public string state { get; set; }
        public string total_spent { get; set; }
        public object last_order_id { get; set; }
        public object note { get; set; }
        public bool verified_email { get; set; }
        public object multipass_identifier { get; set; }
        public bool tax_exempt { get; set; }
        public string phone { get; set; }
        public string tags { get; set; }
        public object last_order_name { get; set; }
        public string currency { get; set; }
        public List<Address> addresses { get; set; }
        public DateTime accepts_marketing_updated_at { get; set; }
        public object marketing_opt_in_level { get; set; }
        public List<object> tax_exemptions { get; set; }
        public string admin_graphql_api_id { get; set; }
        public DefaultAddress default_address { get; set; }
    }

    public class ShopifyCustomerModel
    {
        public int status { get; set; }
        public Response response { get; set; }
        public ShopifyCustomer customer { get; set; }
    }

    public class ShopifyAllCustomerAddress
    {
        public object id { get; set; }
        public object customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
        public string province_code { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public bool @default { get; set; }
    }

    public class ShopifyAllCustomerDefaultAddress
    {
        public object id { get; set; }
        public object customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
        public string province_code { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public bool @default { get; set; }
    }

    public class ShopifyAllCustomer
    {
        public string id { get; set; }
        public string email { get; set; }
        public bool accepts_marketing { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int orders_count { get; set; }
        public string state { get; set; }
        public string total_spent { get; set; }
        public long? last_order_id { get; set; }
        public string note { get; set; }
        public bool verified_email { get; set; }
        public object multipass_identifier { get; set; }
        public bool tax_exempt { get; set; }
        public string phone { get; set; }
        public string tags { get; set; }
        public string last_order_name { get; set; }
        public string currency { get; set; }
        public List<ShopifyAllCustomerAddress> addresses { get; set; }
        public DateTime accepts_marketing_updated_at { get; set; }
        public object marketing_opt_in_level { get; set; }
        public List<object> tax_exemptions { get; set; }
        public string admin_graphql_api_id { get; set; }
        public ShopifyAllCustomerDefaultAddress default_address { get; set; }
    }

    public class ShopifyAllCustomerModel
    {
        public int status { get; set; }
        public Response response { get; set; }
        public object previous_page_info { get; set; }
        public object next_page_info { get; set; }
        public List<ShopifyAllCustomer> customers { get; set; }
    }

    public class CreateShopifyAddress
    {
        public string address1 { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string phone { get; set; }
        public string zip { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
        public string country { get; set; }
    }

    public class CreateShopifyCustomer
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public bool verified_email { get; set; }
        public List<CreateShopifyAddress> addresses { get; set; }
    }

    public class CreateShopifyModel
    {
        public CreateShopifyCustomer customer { get; set; }
        public int member_id { get; set; }
    }

    public class CreateShopifyCustomerResponse
    {
        public int status { get; set; }
        public Response response { get; set; }
        public ShopifyAllCustomer customer { get; set; }
    }

}
