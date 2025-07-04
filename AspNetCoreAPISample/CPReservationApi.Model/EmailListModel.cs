using System;
using System.Collections.Generic;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Model
{
    public class AddToListRequest
    {
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }

    public class AddTagsRequest
    {
        public string email { get; set; }
        public string tag { get; set; }
    }
}
