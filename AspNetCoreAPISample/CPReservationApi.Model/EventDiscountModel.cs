using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class EventDiscount
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public int MemberId { get; set; }
        public string DiscountName { get; set; }
        public string DiscountCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public int NumberOfUses { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int RequiredMinimum { get; set; }
        public int RequiredMaximum { get; set; }
        public Common.Common.DiscountOption DiscountType { get; set; }
        public Common.Common.DateType DateType { get; set; }
    }
}
