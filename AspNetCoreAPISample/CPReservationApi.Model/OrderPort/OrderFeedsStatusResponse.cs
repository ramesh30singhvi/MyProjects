using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class OrderFeedsStatusResponse : ResponseBase
    {
        public List<OrderFeedsInfo> Data { get; set; }

        public OrderFeedsStatusResponse()
        {
            this.Data = new List<OrderFeedsInfo>();
        }
    }

    public class OrderFeedsInfo
    {
        public string AltOrderNumber { get; set; }
        public string Status { get; set; }
        public List<object> Errors { get; set; }
        public string ResourceUri { get; set; }
        public OrderFeedsInfo()
        {
            this.Errors = new List<object>();
        }
    }
}
