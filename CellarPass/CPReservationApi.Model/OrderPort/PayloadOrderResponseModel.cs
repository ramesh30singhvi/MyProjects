using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class PayloadOrderResponseModel : ResponseBase
    {
        public PayloadOrderData Data { get; set; }

        public PayloadOrderResponseModel()
        {
            this.Data = new PayloadOrderData();
        }
    }

    public class PayloadOrderData
    {
        public int BatchId { get; set; }
        public string ResourceUri { get; set; }
    }
}
