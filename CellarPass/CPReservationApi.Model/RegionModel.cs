using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CPReservationApi.Model
{
    public class RegionModel
    {
        public int id { get; set; }
        public string friendly_name { get; set; }
        public string friendly_url { get; set; }
        public string image_url { get; set; }
        public string state_url
        {
            get
            {
                return string.Format("{0}", Regex.Replace((state_name + "").ToLower(), "  ", " ").Replace(" ", "-").TrimEnd('-'));
            }
        }
        public RegionContentModel region_content_model { get; set; }
        public string internal_name { get; set; }
        public bool enable_1 { get; set; }
        public bool enable_2 { get; set; }
        public bool enable_3 { get; set; }
        public bool enable_4 { get; set; }
        public bool enable_5 { get; set; }
        public bool enable_6 { get; set; }
        public string state_name { get; set; }
    }

    public class RegionDetailModel
    {
        public int id { get; set; }
        public string friendly_name { get; set; }
        public bool enable_getting_here { get; set; }
        public bool enable_getaway_ideas { get; set; }
        public bool enable_where_to_drink { get; set; }
        public bool enable_where_to_eat { get; set; }
        public bool enable_where_to_stay { get; set; }
        public bool enable_things_to_do { get; set; }
        public bool enable_on_homepage { get; set; }
        public List<POICategoriesModel> poicategories { get; set; }
    }

    public class RegionDetail2Model
    {
        public int id { get; set; }
        public string internal_name { get; set; }
        public string friendly_name { get; set; }
        public string friendly_url { get; set; }
        public string state { get; set; }
        public string description { get; set; }
        public string image_url { get; set; }
        public string state_name { get; set; }
        public string country { get; set; }
        public string country_name { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
        public bool go_to_landing { get; set; }
        public string events_page_intro { get; set; }
        public string events_page_desc { get; set; }
        public string events_page_banner_image { get; set; }
    }

    public class POICategoriesModel
    {
        public int attribute_id { get; set; }
        public string attribute_name { get; set; }
        public int attribute_type_id { get; set; }
        public bool active { get; set; }
    }

    public class BusinessProtipsTricksModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string sub_title { get; set; }
        public string content { get; set; }
        public string tags { get; set; }
        public DateTime article_date { get; set; }
        public int view_count { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public string custom_url { get; set; }
        public DateTime date_modified { get; set; }
    }

    public class Region2Model
    {
        public int id { get; set; }
        public string internal_name { get; set; }
        public string friendly_name { get; set; }
        public string friendly_url { get; set; }
        public int winery_type_id { get; set; }
        public string state { get; set; }
        public string description { get; set; }
        public string image_url { get; set; }
        public bool? enable_1 { get; set; }
        public bool? enable_2 { get; set; }
        public bool? enable_3 { get; set; }
        public bool? enable_4 { get; set; }
        public bool? enable_5 { get; set; }
        public bool? enable_6 { get; set; }
        public string state_name { get; set; }
        public string country { get; set; }
        public string country_name { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
        public string content_json { get; set; }
        public bool go_to_landing { get; set; }
        public bool enable_events_page { get; set; }
        public string events_page_intro { get; set; }
        public string events_page_desc { get; set; }
        public string events_page_banner_image { get; set; }
        public string state_url
        {
            get
            {
                return string.Format("{0}", Regex.Replace((state_name + "").ToLower(), "  ", " ").Replace(" ", "-").TrimEnd('-'));
            }
        }
        public RegionContentModel region_content_model { get; set; }
    }

    public class RegionSeasonsItem
    {
        public RegionSeasonsItemType type { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        public string image { get; set; }
        public int sortorder { get; set; }
    }

    public class RegionTravelItem
    {
        public RegionTravelItemType type { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        public int sortorder { get; set; }
    }

    public class RegionDerictoryItem
    {
        public RegionDirectorytemType type { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        public string image { get; set; }
        public int sortorder { get; set; }
    }

    public class RegionContentModel
    {
        public string description { get; set; }
        public List<RegionSeasonsItem> seasonitems { get; set; }
        public List<RegionTravelItem> travelitems { get; set; }
        public List<RegionDerictoryItem> directoryitems { get; set; }
    }

    public class PageModel
    {
        public int id { get; set; }
        public string friendly_name { get; set; }
        public string friendly_url { get; set; }
        public string description { get; set; }
        public string geo_latitude { get; set; }
        public string geo_longitude { get; set; }
        public List<string> image_url { get; set; }
        public string meta_description { get; set; }
        public string meta_title { get; set; }
        public string meta_keywords { get; set; }
        public RegionContentModel region_content_model { get; set; }
    }

    public class StateViewModel
    {
        public int state_id { get; set; }
        public string state_name { get; set; }
        public string state_code { get; set; }
        public string state_url
        {
            get
            {
                return string.Format("{0}", Regex.Replace((state_name + "").ToLower(), "  ", " ").Replace(" ", "-").TrimEnd('-'));
            }
        }
        public List<StateRegionViewModel> state_regions { get; set; }
    }

    public class StateRegionViewModel
    {
        public string region_name { get; set; }
        public string region_url { get; set; }
    }

    public class StateDataModel
    {
        public int id { get; set; }
        public string friendly_url { get; set; }
        public string title { get; set; }
        public string meta_description { get; set; }
        public string meta_keywords { get; set; }       
        public string state_name { get; set; }
        public string state_code { get; set; }
        public string content { get; set; }
    }

    public class SpecialEventsPageFilterModel
    {
        public List<DateModel> dates { get; set; }
        public List<CategoriesModel> categories { get; set; }
        public List<LocationFilterModel> locations { get; set; }
    }

    public class DateModel
    {
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public string display_name { get; set; }
    }

    public class CategoriesModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string friendly_url
        {
            get
            {
                return string.Format("{0}", Regex.Replace((name + "").ToLower(), "  ", " ").Replace(" ", "-").TrimEnd('-'));
            }
        }
    }

    public class LocationFilterModel
    {
        public string state_name { get; set; }
        public string state_url
        {
            get
            {
                return string.Format("{0}", Regex.Replace((state_name + "").ToLower(), "  ", " ").Replace(" ", "-").TrimEnd('-'));
            }
        }
        public List<CityModel> cities { get; set; }
    }

    public class CityModel
    {
        public string city { get; set; }
        public string friendly_url
        {
            get
            {
                return string.Format("{0}", Regex.Replace((city + "").ToLower(), "  ", " ").Replace(" ", "-").TrimEnd('-'));
            }
        }
    }

    public class SubRegionModel
    {
        public int id { get; set; }
        public int sub_region_id { get; set; }
        public string region_name { get; set; }
        public string region_url { get; set; }
        public string sub_region_name { get; set; }
        public string sub_region_url { get; set; }
        public string sub_region_image_name { get; set; }
    }

    public class CmsPageModel
    {
        public int id { get; set; }
        public string friendly_url { get; set; }
        public string title { get; set; }
        public string meta_description { get; set; }
        public string meta_keywords { get; set; }
        public string content { get; set; }
        public string config_values { get; set; }
        public List<CmsPageSectionModel> cms_page_sections { get; set; }
        public List<CmsPageImageModel> cms_page_images { get; set; }
    }

    public class CmsPageSectionColModel
    {
        public CmsPageSectionType type { get; set; }
        public int colno { get; set; }
        public string config_values { get; set; }
        public bool active { get; set; }
    }

    public class CmsPageSectionModel
    {
        public int id { get; set; }
        public CmsPageSectionType type { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public string filter { get; set; }
        public int sort_position { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public int total_cols { get; set; }
        public List<CmsPageSectionColModel> cms_page_section_cols { get; set; }
    }

    public class CmsPageImageModel
    {
        public int id { get; set; }
        public CMSPageEntityType cms_page_entity_type { get; set; }
        public int entity_id { get; set; }
        public string image_path { get; set; }
    }

    public class EventTypeGroupFilterModel
    {
        public int? NoOfRecord { get; set; }
        public int? EventTypeGroupId { get; set; }
        public bool? SortByPopularity { get; set; }
        public int? Regionid { get; set; }
        public int? SubRegionId { get; set; }
        public string SubRegionIds { get; set; }
        public string EventTypes { get; set; }
        public string Reviews { get; set; }
        public string PopularTags { get; set; }
        public string Varietals { get; set; }
        public string NotableFeatures { get; set; }
        public bool? IsAdvancedFilter { get; set; }
    }
}
