using System;
using System.Collections.Generic;
using System.Text;
using static CPReservationApi.Common.Payments.Configuration;
using uc = CPReservationApi.Common;

namespace CPReservationApi.Model
{
    public class PaymentConfigModel
    {
        public uc.Payments.Configuration.Gateway PaymentGateway { get; set; } = Gateway.Offline;
        public string MerchantLogin { get; set; }
        public string MerchantPassword { get; set; }
        public string UserConfig1 { get; set; }
        public string UserConfig2 { get; set; }
        public Mode GatewayMode { get; set; }
    }
}
