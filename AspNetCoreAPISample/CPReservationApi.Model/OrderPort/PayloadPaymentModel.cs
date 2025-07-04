using System;
using System.Collections.Generic;
using System.Text;
using CPReservationApi.Common;

namespace CPReservationApi.Model
{
    public class PayloadPaymentModel
    {
        public decimal Amount { get; set; } = 0;
        public string PaymentMethod { get; set; }
        public string CreditCardNumber { get; set; }
        public string CreditCardType { get; set; } //Use enumCreditCardType
        public string CreditCardName { get; set; }
        public short? CreditCardExpireMonth { get; set; } = null;
        public short? CreditCardExpireYear { get; set; } = null;
        public string PurchaseOrderNumber { get; set; }
        public string CheckNumber { get; set; }
    }
}
