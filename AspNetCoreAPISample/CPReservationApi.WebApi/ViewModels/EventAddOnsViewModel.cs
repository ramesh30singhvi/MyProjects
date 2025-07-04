using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPReservationApi.Model;

namespace CPReservationApi.WebApi.ViewModels
{
    public class EventAddOnsResponse : BaseResponse2
    {
        public EventAddOnsResponse()
        {
            data = new List<AddOn_Group>();
        }
        public List<AddOn_Group> data { get; set; }
    }

    public class MemberstatsResponse : BaseResponse2
    {
        public MemberstatsResponse()
        {
            data = new MemberStatsModel();
        }
        public MemberStatsModel data { get; set; }
    }

    public class GoogleSFTPEventResponse : BaseResponse2
    {
        public GoogleSFTPEventResponse()
        {
            data = new GoogleSFTPModel();
        }
        public GoogleSFTPModel data { get; set; }
    }

    public class MemberProductsResponse : BaseResponse2
    {
        public MemberProductsResponse()
        {
            data = new List<MemberProductsModel>();
        }
        public List<MemberProductsModel> data { get; set; }
    }

    public class MemberFoodMenuResponse : BaseResponse2
    {
        public MemberFoodMenuResponse()
        {
            data = new List<MemberFoodMenuModel>();
        }
        public List<MemberFoodMenuModel> data { get; set; }
    }

    public class PriceRangeResponse : BaseResponse2
    {
        public PriceRangeResponse()
        {
            data = new PriceRangeByRegionIdModel();
        }
        public PriceRangeByRegionIdModel data { get; set; }
    }

    public class MembersByPriceRangeResponse : BaseResponse2
    {
        public MembersByPriceRangeResponse()
        {
            data = new List<MembersByPriceRangeModel>();
        }
        public List<MembersByPriceRangeModel> data { get; set; }
    }

    public class EventAddOnsAccountTypesResponse : BaseResponse2
    {
        public EventAddOnsAccountTypesResponse()
        {
            data = new List<Event_AddOns_AccountTypes>();
        }
        public List<Event_AddOns_AccountTypes> data { get; set; }
    }

    public class BookedEventExperienceResponse : BaseResponse2
    {
        public BookedEventExperienceResponse()
        {
            data = new List<EventExperienceModel>();
        }
        public List<EventExperienceModel> data { get; set; }
    }
}
