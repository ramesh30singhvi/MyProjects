using System;
using System.Collections.Generic;
using System.Text;
using CPReservationApi.Common;

namespace CPReservationApi.Model
{
    public class PayloadOrderModel
    {
        public string AltOrderNumber { get; set; } = "";
        public string CustomerUuid { get; set; }
        public string AltCustomerNumber { get; set; }
        public DateTime OrderDateTime { get; set; } = DateTime.MinValue;
        public DateTime? SaleDateTime { get; set; } = null;
        public DateTime? RequestedShipDate { get; set; } = null;
        public AddressModel BillingAddress { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public string GiftMessage { get; set; }
        public string OrderNotes { get; set; }
        public double SubTotal { get; set; } = 0;
        public double Discount { get; set; } = 0;
        public double Shipping { get; set; } = 0;
        public double Handling { get; set; } = 0;
        public double TotalTax { get; set; } = 0;
        public double GrandTotal { get; set; } = 0;
        public string ShippingService { get; set; }// = enumShippingService.Standard.ToString(); 
        public PayloadPaymentModel Payment { get; set; }
        public ProductModel[] LineItems { get; set; }// = new ProductModel[1];

        public PayloadOrderModel()
        {
            this.BillingAddress = new AddressModel();
            this.ShippingAddress = new AddressModel();
            this.Payment = new PayloadPaymentModel();
        }
    }
}
