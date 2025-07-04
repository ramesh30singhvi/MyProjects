using System;
using System.Collections.Generic;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Model
{
    public class DiscountModel
    {
        public int id { get; set; }
        public string discount_name { get; set; }
        public string discount_code { get; set; }
        public decimal discount_amount { get; set; }
        public decimal discount_percent { get; set; }
        public DateTime start_datetime { get; set; }
        public DateTime end_datetime { get; set; }
        public int min_guest { get; set; }
        public int max_guest { get; set; }
        public DiscountOption discount_type { get; set; }
        public DateType date_type { get; set; }
    }

    public class AccountTypeDiscountModel
    {
        public int Id { get; set; }
        public string ContactTypeId { get; set; }
        public string ContactType { get; set; }
        public int eventId { get; set; }
        public int MemberBenefitId { get; set; }
        public bool MemberBenefitReq { get; set; }
        public DiscountType MemberBenefit { get; set; }
        public decimal MemberBenefitCustomValue { get; set; }
    }

    public class PassportAvailableStatus
    {
        public bool isAvailable { get; set; }
        public string Message { get; set; }
        public int ticketId { get; set; }
    }
}
