using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class CalendarResponse : BaseResponse
    {
        public CalendarResponse()
        {
            data = new List<CalendarModel>();
        }
        public List<CalendarModel> data { get; set; }
    }

    public class NoteRequest
    {
        public int id { get; set; } = 0;
        public int member_id { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
    }

    public class NoteResponse : BaseResponse
    {
        public NoteResponse()
        {
            data = new NoteResponseModel();
        }
        public NoteResponseModel data { get; set; }
    }

    public class NoteResponseModel
    {
        public int id { get; set; }
    }
}
