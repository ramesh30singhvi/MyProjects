using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class MetaData
    {
        public string id { get; set; }
        public string code { get; set; }
        public string value { get; set; }
    }

    public class EmailModel
    {
        public string id { get; set; }
        public string email { get; set; }
    }

    public class Phone
    {
        public string id { get; set; }
        public string phone { get; set; }
    }

    public class OrderInformation
    {
        public object lastOrderId { get; set; }
        public DateTime? lastOrderDate { get; set; }
        public int orderCount { get; set; }
        public int lifetimeValue { get; set; }
        public object currentClubTitle { get; set; }
        public object daysInCurrentClub { get; set; }
    }

    public class Club
    {
        public string clubId { get; set; }
        public string clubTitle { get; set; }
        public object cancelDate { get; set; }
        public DateTime signupDate { get; set; }
        public string clubMembershipId { get; set; }
    }

    public class CustomerModel
    {
        public string id { get; set; }
        //public string avatar { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        //public string birthDate { get; set; }
        public string city { get; set; }
        public string stateCode { get; set; }
        public string zipCode { get; set; }
        public string countryCode { get; set; }
        //public bool acceptsMarketing { get; set; }
        public DateTime lastActivityDate { get; set; }
        //public string facebookId { get; set; }
        //public List<MetaData> metaData { get; set; }
        //public DateTime createdAt { get; set; }
        //public DateTime updatedAt { get; set; }
        public List<EmailModel> emails { get; set; }
        public List<Phone> phones { get; set; }
        public List<string> groupIds { get; set; }
        public OrderInformation orderInformation { get; set; }
        public List<Club> clubs { get; set; }
        public List<Tags> tags { get; set; }
        public List<Groups> groups { get; set; }
    }
    public class Tags
    {
        public string id { get; set; }
        public string title { get; set; }
        public string objectType { get; set; }
        public string type { get; set; }
        public object appliesToCondition { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
    public class Groups
    {
        public string id { get; set; }
        public string title { get; set; }
        public string objectType { get; set; }
        public string type { get; set; }
        public object appliesToCondition { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
    public class Customers
    {
        public List<CustomerModel> customers { get; set; }
        public int total { get; set; }
    }

    public class CreatePhone
    {
        public string phone { get; set; }
    }

    public class CreateEmail
    {
        public string email { get; set; }
    }

    public class CreateCustomer
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDate { get; set; }
        public string city { get; set; }
        public string stateCode { get; set; }
        public string zipCode { get; set; }
        public string countryCode { get; set; }
        public string emailMarketingStatus { get; set; } = null;
        public List<CreatePhone> phones { get; set; }
        public List<CreateEmail> emails { get; set; }
        //public List<string> groupIds { get; set; }
    }

    public class CustomerAddress
    {
        public string id { get; set; }
        public string customerId { get; set; }
        public string birthDate { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string company { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string stateCode { get; set; }
        public string zipCode { get; set; }
        public string countryCode { get; set; }
        public bool isDefault { get; set; }
    }

    public class CustomerAddressList
    {
        public List<CustomerAddress> customerAddresses { get; set; }
        public int total { get; set; }
    }

    public class AddressInfo
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string company { get; set; }
        public string address { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string stateCode { get; set; }
        public string zipCode { get; set; }
        public string phone { get; set; }
        public string countryCode { get; set; }
        public string birthDate { get; set; }
    }

    public class Group
    {
        public string id { get; set; }
        public string title { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class Commerce7ContactType
    {
        public string id { get; set; }
        public string title { get; set; }
        public bool activeclub { get; set; } = false;
    }

    public class Commerce7ContactTypeList
    {
        public List<Commerce7ContactType> groups { get; set; }
    }

    public class GroupList
    {
        public List<Group> groups { get; set; }
        public int total { get; set; }
    }

    public class Seo
    {
        public string title { get; set; }
        public object description { get; set; }
    }

    public class Clubs
    {
        public string id { get; set; }
        public string title { get; set; }
        public object content { get; set; }
        public string slug { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public List<object> addOns { get; set; }
        public Seo seo { get; set; }
    }

    public class ClubList
    {
        public List<Clubs> clubs { get; set; }
        public int total { get; set; }
    }

    public class Note
    {
        public string id { get; set; }
        public string type { get; set; }
        public DateTime noteDate { get; set; }
        public string content { get; set; }
        public string customerId { get; set; }
        public object orderId { get; set; }
        public object dueDate { get; set; }
        public object accountId { get; set; }
        public object accountName { get; set; }
        public bool isComplete { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class NoteList
    {
        public List<Note> notes { get; set; }
        public int total { get; set; }
    }


    public class Tender
    {
        public string id { get; set; }
        public string tenderType { get; set; }
        public string chargeType { get; set; }
        public string chargeStatus { get; set; }
        public int amountTendered { get; set; }
        public int tip { get; set; }
        public string paymentDate { get; set; }
        public string creditCardBrand { get; set; }
        public string maskedCardNumber { get; set; }
        public int? creditCardExpiryMo { get; set; }
        public int? creditCardExpiryYr { get; set; }
        public string creditCardHolderName { get; set; }
    }

    public class Commerce7Item
    {
        public string id { get; set; }
        public string productTitle { get; set; }
        public string type { get; set; }
        public string inventoryLocationId { get; set; }
        public string productVariantTitle { get; set; }
        public string sku { get; set; }
        public int costOfGood { get; set; }
        public int price { get; set; }
        public int originalPrice { get; set; }
        public int comparePrice { get; set; }
        public int quantity { get; set; }
        public int quantityFulfilled { get; set; } = 0;
        public int tax { get; set; }
        public string taxType { get; set; }
        public int weight { get; set; }
        public bool hasShipping { get; set; }
        public int volumeInML { get; set; }
        public string departmentCode { get; set; } = null;
    }

    public class Tax
    {
        public string vendor { get; set; } = "Local";
        public string title { get; set; } = "Tax";
        public string countryCode { get; set; }
        public string stateCode { get; set; }
        public int freight { get; set; } = 0;
        public int food { get; set; } = 0;
        public int generalMerchandise { get; set; } = 0;
        public int cannabis { get; set; } = 0;
        public int wine { get; set; } = 0;
        public int price { get; set; }
        public bool isIncludedInPrice { get; set; }
    }

    public class Shipping
    {
        public string title { get; set; }
        public string code { get; set; }
        public string service { get; set; }
        public string shippingServiceId { get; set; }
        public int price { get; set; }
        public int originalPrice { get; set; }
        public bool isQualifiesForPrime { get; set; }
        public int tax { get; set; }
    }

    public class SelectedShippingOptions
    {
        public string shippingServiceId { get; set; }
    }

    public class Coupon
    {
        public string title { get; set; }
        public string code { get; set; }
        public double productValue { get; set; }
        public double shippingValue { get; set; }
        public double totalValue { get; set; }
    }

    public class Promotion
    {
        public string title { get; set; }
        public int productValue { get; set; }
        public int shippingValue { get; set; }
        public int totalValue { get; set; }
    }

    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
    public class Commerce7OrderModel
    {
        public string id { get; set; }
        public string orderSubmittedDate { get; set; }

        public string orderPaidDate { get; set; }
        public string externalOrderNumber { get; set; }
        public string externalOrderVendor { get; set; } = "CellarPass";

        [JsonIgnore]
        public string email { get; set; }
        public int? orderNumber { get; set; } = null;
        public string paymentStatus { get; set; } = "Paid";
        public string complianceStatus { get; set; } = "Compliant";
        public string fulfillmentStatus { get; set; } = "Fulfilled";
        public string orderFulfilledDate { get; set; }
        public string cartId { get; set; }
        public string channel { get; set; } = "Web";
        public  string posProfileId { get; set; }
        public string customerId { get; set; }
        public string orderDeliveryMethod { get; set; } = "Carry Out";
        public string ShippingInstructions { get; set; } = null;
        public string taxSaleType { get; set; } = "Onsite";
        public int subTotal { get; set; }
        public int shipTotal { get; set; }
        public int taxTotal { get; set; }
        public int tipTotal { get; set; }
        public int total { get; set; }
        public int totalAfterTip { get; set; }
        public string giftMessage { get; set; } = string.Empty;
        public MetaDataModel metaData { get; set; } = null;
        //public List<Coupon> coupons { get; set; }
        //public AddressInfo shipTo { get; set; }
        public AddressInfo billTo { get; set; }
        public List<Tender> tenders { get; set; }
        public List<Commerce7Item> items { get; set; }
        public List<Tax> taxes { get; set; }

        public Commerce7OrderModel()
        {
            billTo = new AddressInfo();
            //shipTo = new AddressInfo();
            tenders = new List<Tender>();
            taxes = new List<Tax>();
        }
    }

    public class MetaDataModel
    {
        public string ordersource { get; set; }
    }

    public class CreateCreditCard
    {
        public string cardBrand { get; set; }
        public string gateway { get; set; }
        public string maskedCardNumber { get; set; }
        public string cardNumber { get; set; }
        public string cvv2 { get; set; }
        public string tokenOnFile { get; set; }
        public int expiryMo { get; set; }
        public int expiryYr { get; set; }
        public string cardHolderName { get; set; }
        public bool isDefault { get; set; }
    }

    public class CustomerCreditCard
    {
        public string id { get; set; }
        public string cardBrand { get; set; }
        public string gateway { get; set; }
        public string maskedCardNumber { get; set; }
        public int expiryMo { get; set; }
        public int expiryYr { get; set; }
        public string cardHolderName { get; set; }
        public string tokenOnFile { get; set; }
        public string customerId { get; set; }
        public bool isDefault { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class CustomerCreditCardModel
    {
        public List<CustomerCreditCard> customerCreditCards { get; set; }
        public int total { get; set; }
    }

    public class CreateCreditCardResponce
    {
        public string cardBrand { get; set; }
        public string gateway { get; set; }
        public string maskedCardNumber { get; set; }
        public string tokenOnFile { get; set; }
        public int expiryMo { get; set; }
        public int expiryYr { get; set; }
        public string cardHolderName { get; set; }
        public bool isDefault { get; set; }
        public string customerId { get; set; }
        public string id { get; set; }
        public DateTime updatedAt { get; set; }
        public DateTime createdAt { get; set; }
    }

    public class MetaDataConfig
    {
        public string id { get; set; }
        public string title { get; set; }
        public string objectType { get; set; }
        public string code { get; set; }
        public string dataType { get; set; }
        public bool isRequired { get; set; }
        public List<string> options { get; set; }
        public int sortOrder { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class MetaDataConfigModel
    {
        public List<MetaDataConfig> metaDataConfigs { get; set; }
        public int total { get; set; }
    }
}
