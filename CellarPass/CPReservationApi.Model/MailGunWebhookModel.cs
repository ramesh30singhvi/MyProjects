using System;
using System.Collections.Generic;
using System.Text;
using CPReservationApi.Common;
using Newtonsoft.Json;

namespace CPReservationApi.Model
{
    public class MailGunWebhookRequest
    {
        public string Event { get; set; }
        public string Recipient { get; set; }
        public Email.EmailType CPMailType { get; set; }
        public string CPMailTypeDesc { get; set; }
        public int CPId { get; set; }
        public int CPEmailContentId { get; set; }
    }

    
}
