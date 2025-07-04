using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class PaymentModel
    {
        public int id { get; set; }
        public int reservation_id { get; set; }
        public DateTime payment_date { get; set; }
        public int payment_type { get; set; }
        public int transaction_type { get; set; }
        public int status { get; set; }
        public string appoval_code { get; set; }
        public string transaction_id { get; set; }
        public string detail { get; set; }
        public decimal amount { get; set; }
        public string payment_card_type { get; set; }
        public string payment_card_number { get; set; }
        public string payment_card_exp_month { get; set; }
        public string payment_card_exp_year { get; set; }
        public string payment_card_customer_name { get; set; }
        public string payment_card_token { get; set; }
        public Common.Payments.Configuration.Gateway payment_gateway { get; set; }
        public short payment_version { get; set; }
        public string processed_by { get; set; }
        public decimal refund_amount { get; set; }
        public decimal original_amount { get; set; }
        public string card_last_four_digits { get; set; } = "";
        public string card_first_four_digits { get; set; } = "";
        public int timezone_id { get; set; } = 5;
        public Common.CardEntry card_entry { get; set; }
        public Common.ApplicationType application_type { get; set; }
        public string application_version { get; set; }
        public string terminal_id { get; set; }
        public string card_reader { get; set; }
    }
    public class TempCardDetail
    {
        public string PayCardType { get; set; }
        public string PayCardNumber { get; set; }

        public string PayCardCustName { get; set; }

        public string PayCardExpMonth { get; set; }
        public string PayCardExpYear { get; set; }
        public string PayCardToken { get; set; }
        public int MemberId { get; set; }
        public string Vin65Username { get; set; }
        public string Vin65Password { get; set; }
        public string cvv { get; set; }

    }

    public class CreditCardDetail
    {
        public string card_token { get; set; }
        public string last_four_digits { get; set; }
        public string first_four_digits { get; set; }
        public bool is_expired { get; set; }
        public string customer_name { get; set; }
        public string card_type { get; set; }
        public string cust_id { get; set; }
        public string card_expiration { get; set; }
        public string card_exp_month { get; set; }
        public string card_exp_year { get; set; }
    }
}
