using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class Vin65Model
    {
        public string Vin65ID { get; set; }
        public DateTime DateModified { get; set; }
        public string HomePhone { get; set; }
        public string BillingStreet { get; set; }
        public string BillingStreet2 { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingZip { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public decimal ltv { get; set; }
        public DateTime? last_order_date { get; set; }
        public bool member_status { get; set; } = false;
        public int order_count { get; set; }
        public List<string> contact_types { get; set; } = new List<string>();
    }
}
