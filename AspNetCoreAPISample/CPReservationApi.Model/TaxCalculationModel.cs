using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class TaxCalculationModel
    {
        public decimal subtotal { get; set; }
        public decimal subtotal_after_discount { get; set; }
        public decimal previous_deposit { get; set; }
        public decimal addon_total { get; set; }
        public decimal discount { get; set; }
        public decimal sales_tax { get; set; }
        public decimal balance_due { get; set; }
        public decimal sales_tax_percent { get; set; }
        public decimal tranasction_fee { get; set; }
        public decimal gratuity_total { get; set; }
        public decimal taxes_and_fees { get; set; }
        public decimal deposit_due_amount { get; set; }
        public decimal deposit_due_percentage { get; set; }
        public List<ActivationCodesModel> activation_codes { get; set; }
        public List<DiscountDetailsModel> club_discount_details { get; set; }
        public string discount_code { get; set; }
        //public bool discount_code_valid { get; set; } = true;
    }

    public class ActivationCodesModel
    {
        public string activation_code { get; set; }
        public string discount_desc { get; set; }
        public bool is_valid { get; set; } = false;
        public int ticket_id { get; set; }
    }

    public class DiscountDetailsModel
    {
        public string name { get; set; }
        public decimal discount_amount { get; set; }
        public string member_benefit_desc { get; set; }
    }
}
