using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class WineClubMembership
    {
        public string ClubName { get; set; }
        public string MemberSinceDate { get; set; }
        public bool OnHold { get; set; }
        public bool OnWaitingList { get; set; }
        public bool Pickup { get; set; }
        public bool IsDefault { get; set; }
        public bool IsGiftMembership { get; set; }
    }

 
    public class CustomerData
    {
        public string CustomerUuid { get; set; }
        public int CustomerNumber { get; set; }
        public AddressModel BillingAddress { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public List<AddressModel> AltShippingAddresses { get; set; }
        public List<AddressModel> AltBillingAddresses { get; set; }
        public DateTime? CreationDateTime { get; set; }
        public DateTime? LastUpdatedDateTime { get; set; }
        public double TotalCustomerValue { get; set; }
        public object CustomerClass { get; set; }
        public bool OptIn { get; set; }
        public bool TaxExempt { get; set; }
        public List<WineClubMembership> WineClubMemberships { get; set; }
        public string ResourceUri { get; set; }
        public CustomerData()
        {
            this.BillingAddress = new AddressModel();
            this.ShippingAddress = new AddressModel();
            this.AltBillingAddresses = new List<AddressModel>();
            this.AltShippingAddresses = new List<AddressModel>();
            this.WineClubMemberships = new List<WineClubMembership>();
        }
    }

    public class CustomerAcDetails
    {
        public string Status { get; set; }
        public int RequestId { get; set; }
        public List<object> Messages { get; set; }
        public CustomerData Data { get; set; }

        public CustomerAcDetails()
        {
            this.Messages = new List<object>();
            this.Data = new CustomerData();
        }
    }
}
