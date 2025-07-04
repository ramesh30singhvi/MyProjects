using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Model
{
    public class ItineraryPlanner
    {
        public int id { get; set; }
        public string itinerary_guid { get; set; }
        public string itinerary_name { get; set; }
        public Itinerary_Status itinerary_status { get; set; } = Itinerary_Status.Initial;
        public int user_id { get; set; }
        public int concierge_id { get; set; }
        public int party_adults { get; set; }
        public int party_children { get; set; }
        public int party_kids { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public List<ItineraryPlannerItem> items { get; set; }


    }

    public class ItineraryPlannerItemRequest : ItineraryPlannerItem
    {
        public int itinerary_id { get; set; }
    }

    public class RemoveItineraryItemRequest
    {
        //public int itinerary_id { get; set; }

        public int item_id { get; set; } = 0;

        public string item_guid { get; set; } = "";

    }

    public class ItineraryPlannerItem
    {
        public int id { get; set; }
        public string item_guid { get; set; }
        public ItineraryPlanner_Ota ota_info { get; set; }
        public int booking_type_id { get; set; }
        public string booking_type_name { get; set; } = "";
        public int cellarpass_member_id { get; set; } = 0;
        public int cellarpass_reservation_id { get; set; } = 0;
        public int cellarpass_ticket_orderid { get; set; } = 0;
        public string item_confirmation { get; set; } = "";
        public string business_name { get; set; } = "";
        public string address1 { get; set; } = "";
        public string address2 { get; set; } = "";
        public string city { get; set; } = "";
        public string state { get; set; } = "";
        public string zip { get; set; } = "";
        public string phone { get; set; } = "";
        public string latitude { get; set; } = "";
        public string longitude { get; set; } = "";
        public string party_first_name { get; set; } = "";
        public string party_last_name { get; set; } = "";
        public string party_size { get; set; } = "";
        public decimal item_amount { get; set; }
        public string item_notes { get; set; } = "";
        public int regionid { get; set; }
        public string item_start_date { get; set; }
        public string item_end_date { get; set; }
        public string member_url { get; set; }

        public string travel_time { get; set; } = "";

        public string distance { get; set; } = "";
    }

    public class ItineraryPlannerItemSorted : ItineraryPlannerItem
    {
        public int sort_order { get; set; }
    }
    public class ItineraryPlannerViewModel : ItineraryPlanner
    {
        public List<ItineraryPlanner_Region> regions { get; set; }

        public int items_count { get; set; }
    }


    public class ItineraryPlannerRequest : ItineraryPlanner
    {
        public List<int> regions { get; set; }



    }

    public class ConfirmItineraryRequest
    {
        public List<int> itinerary_ids { get; set; }

        public int user_id { get; set; }

        public ReferralType referral_type { get; set; } = ReferralType.CellarPass;
        public int booked_by_id { get; set; } = 0;

        public int ticket_order_id { get; set; } = 0;
    }

    public class AddReservationToItineraryRequest
    {
        public int reservation_id { get; set; }

        public int itinerary_id { get; set; }
    }
    public class ItineraryPlanner_Ota
    {
        public int id { get; set; }
        public string ota_name { get; set; }
        public short ota_type { get; set; }
        public string ota_account { get; set; }

    }
    public class ItineraryPlanner_Region
    {
        public int region_id { get; set; }
        public string region_name { get; set; }

    }

    public class ItineraryPlanner_BookingType
    {
        public int booking_type_id { get; set; }
        public string booking_type_name { get; set; }

    }


    public class AddPassportItinerary
    {
        public int ticket_event_id { get; set; }

        public int itinerary_id { get; set; } = 0;

        public string request_date { get; set; }

        public int guests { get; set; }

        public int user_id { get; set; }

        public int slot_id { get; set; }

        public int slot_type { get; set; }

        public int member_id { get; set; }

        public UserDetail user_info { get; set; }

    }

    public class UpdatePassportItinerary
    {

        public int itinerary_item_id { get; set; } 

        public string request_date { get; set; }
        public int slot_id { get; set; }

        public int slot_type { get; set; }

    }

    public class PassportItineraryResponseModel
    {
        public int itinerary_id { get; set; }
        public int itinerary_item_id { get; set; }
        public int sort_order { get; set; }

        public int user_id { get; set; }
        public string dist_prev_destination { get; set; }
        public string travel_time_prev_dest { get; set; }

    }

    public class CheckDistanceTravelTimeResponseModel
    {
        public string dist_prev_destination { get; set; }
        public string travel_time_prev_dest { get; set; }
        public bool is_valid { get; set; } = true;

    }
        
}
