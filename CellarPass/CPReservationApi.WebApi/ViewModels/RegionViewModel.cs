using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class RegionResponse : BaseResponse2
    {
        public RegionResponse()
        {
            data = new List<RegionModel>();
        }
        public List<RegionModel> data { get; set; }
    }

    public class RegionDetailResponse : BaseResponse2
    {
        public RegionDetailResponse()
        {
            data = new List<RegionDetail2Model>();
        }
        public List<RegionDetail2Model> data { get; set; }
    }

    public class RegionDetailByURLResponse : BaseResponse2
    {
        public RegionDetailByURLResponse()
        {
            data = new Region2Model();
        }
        public Region2Model data { get; set; }
    }

    public class SubRegionResponse : BaseResponse2
    {
        public SubRegionResponse()
        {
            data = new List<SubRegionModel>();
        }
        public List<SubRegionModel> data { get; set; }
    }

    public class PageDetailByURLResponse : BaseResponse2
    {
        public PageDetailByURLResponse()
        {
            data = new PageModel();
        }
        public PageModel data { get; set; }
    }

    public class POICategoriesResponse : BaseResponse2
    {
        public POICategoriesResponse()
        {
            data = new RegionDetailModel();
        }
        public RegionDetailModel data { get; set; }
    }

    public class WelcomeDashboardDataResponse : BaseResponse2
    {
        public WelcomeDashboardDataResponse()
        {
            data = new WelcomeDashboardDataModel();
        }
        public WelcomeDashboardDataModel data { get; set; }
    }

    public class BusinessProtipsTricksResponse : BaseResponse2
    {
        public BusinessProtipsTricksResponse()
        {
            data = new List<BusinessProtipsTricksModel>();
        }
        public List<BusinessProtipsTricksModel> data { get; set; }
    }

    public class StatesResponse : BaseResponse2
    {
        public StatesResponse()
        {
            data = new List<StateViewModel>();
        }
        public List<StateViewModel> data { get; set; }
    }

    public class StatesDataResponse : BaseResponse2
    {
        public StatesDataResponse()
        {
            data = new StateDataModel();
        }
        public StateDataModel data { get; set; }
    }

    public class SpecialEventsPageFilterResponse : BaseResponse2
    {
        public SpecialEventsPageFilterResponse()
        {
            data = new SpecialEventsPageFilterModel();
        }
        public SpecialEventsPageFilterModel data { get; set; }
    }

    public class CmsPageResponse : BaseResponse2
    {
        public CmsPageResponse()
        {
            data = new CmsPageModel();
        }
        public CmsPageModel data { get; set; }
    }

    public class CmsPageSectionResponse : BaseResponse2
    {
        public CmsPageSectionResponse()
        {
            data = new CmsPageSectionModel();
        }
        public CmsPageSectionModel data { get; set; }
    }

    public class EventTypesResponse : BaseResponse2
    {
        public EventTypesResponse()
        {
            data = new List<EventTypes>();
        }
        public List<EventTypes> data { get; set; }
    }

    public class EventTypeDestinationsResponse : BaseResponse2
    {
        public EventTypeDestinationsResponse()
        {
            data = new List<EventTypeDestinationModel>();
        }
        public List<EventTypeDestinationModel> data { get; set; }
    }
}
