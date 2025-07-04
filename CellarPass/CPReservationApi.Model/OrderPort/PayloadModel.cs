using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class PayloadModel
    {
        public string CustomerUuid { get; set; }
        public string AltCustomerNumber { get; set; }
        public AddressModel BillingAddress { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public int CustomerClassId { get; set; } = 1;
        public bool OptIn { get; set; }
        public bool TaxExempt { get; set; }
        public DriverLicenseModel DriverLicense { get; set; }
        public PayloadModel()
        {
            this.BillingAddress = new AddressModel();
            this.ShippingAddress = new AddressModel();
            this.DriverLicense = new DriverLicenseModel();
        }
    }

    public class DriverLicenseModel : AddressModel
    {
        public string ExpiryDate { get; set; }
        public string LicenseNo { get; set; }
    }
}
