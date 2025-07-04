using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class Company
    {
        public string currency { get; set; }
        public string shortname { get; set; }
        public string name { get; set; }
    }

    public class FareharborModel
    {
        public List<Company> companies { get; set; }
    }

    public class FareharborAddress
    {
        public string province { get; set; }
        public string city { get; set; }
        public string street { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
    }

    public class FareharborLocation
    {
        public string google_place_id { get; set; }
        public string note_safe_html { get; set; }
        public double longitude { get; set; }
        public string note { get; set; }
        public string tripadvisor_url { get; set; }
        public FareharborAddress address { get; set; }
        public double latitude { get; set; }
        public int pk { get; set; }
        public string type { get; set; }
    }

    public class CustomerPrototype
    {
        public string note { get; set; }
        public int pk { get; set; }
        public int total { get; set; }
        public string display_name { get; set; }
        public int total_including_tax { get; set; }
    }

    public class Image
    {
        public string image_cdn_url { get; set; }
        public int pk { get; set; }
        public string gallery { get; set; }
    }

    public class Item
    {
        public string image_cdn_url { get; set; }
        public string description { get; set; }
        public string cancellation_policy_safe_html { get; set; }
        public string booking_notes_safe_html { get; set; }
        public string cancellation_policy { get; set; }
        public List<Image> images { get; set; }
        public List<object> description_bullets { get; set; }
        public List<FareharborLocation> locations { get; set; }
        public bool is_pickup_ever_available { get; set; }
        public string headline { get; set; }
        public string description_safe_html { get; set; }
        public string location { get; set; }
        public string booking_notes { get; set; }
        public int pk { get; set; }
        public string description_text { get; set; }
        public List<CustomerPrototype> customer_prototypes { get; set; }
        public double tax_percentage { get; set; }
        public string name { get; set; }
    }

    public class FareharborCompanyItem
    {
        public List<Item> items { get; set; }
    }


    public class CustomerType
    {
        public string note { get; set; }
        public int pk { get; set; }
        public string plural { get; set; }
        public string singular { get; set; }
    }

    //public class CustomerTypeRate
    //{
    //    public CustomerPrototype customer_prototype { get; set; }
    //    public int capacity { get; set; }
    //    public int? minimum_party_size { get; set; }
    //    public CustomerType customer_type { get; set; }
    //    public int? maximum_party_size { get; set; }
    //    public int pk { get; set; }
    //}

    public class Availability
    {
        public DateTime start_at { get; set; }
        public int capacity { get; set; }
        public List<CustomerTypeRate> customer_type_rates { get; set; }
        public int minimum_party_size { get; set; }
        public DateTime end_at { get; set; }
        public int? maximum_party_size { get; set; }
        public int pk { get; set; }
    }

    public class AvailabilityModel
    {
        public List<Availability> availabilities { get; set; }
    }


    public class Contact
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
    }

    public class FHCustomer
    {
        public int customer_type_rate { get; set; }
    }

    public class CreateFHBookingRequest
    {
        public string voucher_number { get; set; }
        public Contact contact { get; set; }
        public List<FHCustomer> customers { get; set; }
        public string note { get; set; }
    }


    public class ExtendedOption
    {
        public string name { get; set; }
        public bool is_taxable { get; set; }
        public string modifier_kind { get; set; }
        public string description_safe_html { get; set; }
        public int offset { get; set; }
        public int pk { get; set; }
        public double percentage { get; set; }
        public string modifier_type { get; set; }
        public bool is_always_per_customer { get; set; }
        public string description { get; set; }
    }

    public class CustomField
    {
        public string modifier_type { get; set; }
        public string description { get; set; }
        public string title { get; set; }
        public string booking_notes_safe_html { get; set; }
        public List<ExtendedOption> extended_options { get; set; }
        public bool is_taxable { get; set; }
        public string modifier_kind { get; set; }
        public string description_safe_html { get; set; }
        public string booking_notes { get; set; }
        public int offset { get; set; }
        public int pk { get; set; }
        public double percentage { get; set; }
        public bool is_required { get; set; }
        public string type { get; set; }
        public bool is_always_per_customer { get; set; }
        public string name { get; set; }
    }

    public class CustomFieldValue
    {
        public int pk { get; set; }
        public CustomField custom_field { get; set; }
        public string value { get; set; }
    }


    public class OrderCustomer
    {
        public string checkin_url { get; set; }
        public int pk { get; set; }
        public List<CustomFieldValue> custom_field_values { get; set; }
        public object checkin_status { get; set; }
        public CustomerTypeRate customer_type_rate { get; set; }
    }

    public class AffiliateCompany
    {
        public string currency { get; set; }
        public string shortname { get; set; }
        public string name { get; set; }
    }

    public class CustomerPrototype2
    {
        public string note { get; set; }
        public int pk { get; set; }
        public int total { get; set; }
        public string display_name { get; set; }
        public int total_including_tax { get; set; }
    }

    public class ExtendedOption2
    {
        public string name { get; set; }
        public bool is_taxable { get; set; }
        public string modifier_kind { get; set; }
        public string description_safe_html { get; set; }
        public int offset { get; set; }
        public int pk { get; set; }
        public double percentage { get; set; }
        public string modifier_type { get; set; }
        public bool is_always_per_customer { get; set; }
        public string description { get; set; }
    }

    public class CustomField2
    {
        public string modifier_type { get; set; }
        public string description { get; set; }
        public string title { get; set; }
        public string booking_notes_safe_html { get; set; }
        public bool is_taxable { get; set; }
        public string modifier_kind { get; set; }
        public string description_safe_html { get; set; }
        public string booking_notes { get; set; }
        public int offset { get; set; }
        public int pk { get; set; }
        public double percentage { get; set; }
        public bool is_required { get; set; }
        public string type { get; set; }
        public bool is_always_per_customer { get; set; }
        public string name { get; set; }
        public List<ExtendedOption2> extended_options { get; set; }
    }

    public class CustomFieldInstance
    {
        public int pk { get; set; }
        public CustomField2 custom_field { get; set; }
    }

    public class CustomerType2
    {
        public string note { get; set; }
        public int pk { get; set; }
        public string plural { get; set; }
        public string singular { get; set; }
    }

    public class CustomerTypeRate
    {
        public CustomerPrototype2 customer_prototype { get; set; }
        public int total { get; set; }
        public int capacity { get; set; }
        public int? minimum_party_size { get; set; }
        public List<CustomFieldInstance> custom_field_instances { get; set; }
        public CustomerType2 customer_type { get; set; }
        public int? maximum_party_size { get; set; }
        public int pk { get; set; }
        public int total_including_tax { get; set; }
    }



    public class FHOrderItem
    {
        public int pk { get; set; }
        public string name { get; set; }
    }

    public class Arrival
    {
        public string display_text { get; set; }
        public string notes { get; set; }
        public string notes_safe_html { get; set; }
        public DateTime time { get; set; }
    }


    public class EffectiveCancellationPolicy
    {
        public DateTime cutoff { get; set; }
        public string type { get; set; }
    }

   public class Booking
    {
        public string voucher_number { get; set; }
        public string display_id { get; set; }
        public object rebooked_to { get; set; }
        public string note_safe_html { get; set; }
        public List<OrderCustomer> customers { get; set; }
        public object agent { get; set; }
        public AffiliateCompany affiliate_company { get; set; }
        public Availability availability { get; set; }
        public string uuid { get; set; }
        public int receipt_subtotal { get; set; }
        public string confirmation_url { get; set; }
        public string note { get; set; }
        public object pickup { get; set; }
        public int pk { get; set; }
        public Company company { get; set; }
        public Arrival arrival { get; set; }
        public int receipt_total { get; set; }
        public List<CustomFieldValue> custom_field_values { get; set; }
        public EffectiveCancellationPolicy effective_cancellation_policy { get; set; }
        public int amount_paid { get; set; }
        public object desk { get; set; }
        public bool is_eligible_for_cancellation { get; set; }
        public int receipt_taxes { get; set; }
        public Contact contact { get; set; }
        public string status { get; set; }
        public int invoice_price { get; set; }
        public object rebooked_from { get; set; }
        public string external_id { get; set; }
        public object order { get; set; }
    }

    public class BookingModel
    {
        public Booking booking { get; set; }
    }

}
