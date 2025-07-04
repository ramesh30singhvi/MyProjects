using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class ErrorInfo
    {
        public int error_type { get; set; }
        public string extra_info { get; set; } = "";
        public string description { get; set; } = "";
        public ErrorData error_data { get; set; }
        public string holiday_name { get; set; } = "";
        public string holiday_description { get; set; } = "";
    }

    public class ErrorInfo2
    {
        public int error_type { get; set; }
        public string extra_info { get; set; } = "";
        public string description { get; set; } = "";
    }

    public class ErrorData
    {
        public string event_name { get; set; }
        public string location_name { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public string member_url { get; set; }
        public string member_benefits_url { get; set; }
        public string code { get; set; }
        public string status_code { get; set; }
    }
}
