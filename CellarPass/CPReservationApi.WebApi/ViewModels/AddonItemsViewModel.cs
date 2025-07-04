using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class AddonItemsByMemberResponse : BaseResponse2
    {
        public AddonItemsByMemberResponse()
        {
            data = new List<AddOn_Item_Ext>();
        }
        public List<AddOn_Item_Ext> data { get; set; }
    }

    public class EventTypeResponse : BaseResponse2
    {
        public EventTypeResponse()
        {
            data = new List<EventTypeModel>();
        }
        public List<EventTypeModel> data { get; set; }
    }
}
