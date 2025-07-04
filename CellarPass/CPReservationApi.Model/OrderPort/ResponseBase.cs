using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class ResponseBase
    {
        public string Status { get; set; }
        public int RequestId { get; set; }
        public List<ListMessages> Messages { get; set; } = new List<ListMessages>();
    }

    public class ListMessages
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public AdditionalInfo AdditionalInfo { get; set; } = new AdditionalInfo();
    }

    public class AdditionalInfo
    {
        public string AltOrderNumber { get; set; }
        public string Source { get; set; }
    }
}
