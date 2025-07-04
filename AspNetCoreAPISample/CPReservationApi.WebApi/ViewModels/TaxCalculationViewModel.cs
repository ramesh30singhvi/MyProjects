using CPReservationApi.Model;
using System;
using System.Collections.Generic;

namespace CPReservationApi.WebApi.ViewModels
{
    public class TaxCalculationResponse : BaseResponse
    {

        public TaxCalculationResponse()
        {
            data = new TaxCalculationModel();
        }
        public TaxCalculationModel data { get; set; }

    }

    public class TaxCalculationRequest
    {
        public int reservation_id { get; set; }
        public int event_id { get; set; }
        public int quantity { get; set; }
        public int fee_type { get; set; }
        public decimal fee_per_person { get; set; }
        public string discount_code { get; set; }
        public decimal discount_amount { get; set; }
        public int user_id { get; set; }
        public bool ignore_sales_tax { get; set; }
        public List<Addon_info> addon_info { get; set; }
        public int location_id { get; set; }
        public bool tax_gratuity { get; set; }
        public decimal gratuity_percentage { get; set; }
        public decimal gratuity_total { get; set; }
        public string email { get; set; }
        public  List<string> activation_codes  { get; set; }
        public string event_date { get; set; }
        public int deposit_policy_id { get; set; }
        public Common.ReferralType referral_type { get; set; } = Common.ReferralType.CellarPass;
        public int floor_plan_id { get; set; }
        public bool ignore_discount { get; set; } = false;
    }

    public class Addon_info
    {
        public int item_id { get; set; }
        public int group_id { get; set; }
        public int qty { get; set; }
        public decimal price { get; set; }
        public bool Taxable { get; set; }
        public bool calculate_gratuity { get; set; }
    }

    public class ReservationChangesResponse : BaseResponse2
    {
        public ReservationChangesResponse()
        {
            data = new ReservationChangesModel();
        }
        public ReservationChangesModel data { get; set; }

    }

    public class ReservationChangesModel
    {
        public decimal all_inclusive_price { get; set; }
    }
}
