using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{

    public class SaveItineraryResponseModel
    {
        public int itinerary_id { get; set; }
        public string itinerary_guid { get; set; }

    }


    public class SaveItineraryResponse : BaseResponse
    {

        public SaveItineraryResponse()
        {
            data = new SaveItineraryResponseModel();
        }
        public SaveItineraryResponseModel data { get; set; }

    }


    public class SaveItineraryItemResponseModel
    {
        public int item_id { get; set; }
        public string item_guid { get; set; }

    }


    public class SaveItineraryItemResponse : BaseResponse
    {

        public SaveItineraryItemResponse()
        {
            data = new SaveItineraryItemResponseModel();
        }
        public SaveItineraryItemResponseModel data { get; set; }

    }

    public class GetItineraryListResponse : BaseResponse
    {

        public GetItineraryListResponse()
        {
            data = new List<ItineraryPlannerViewModel>();
        }
        public List<ItineraryPlannerViewModel> data { get; set; }

    }

    public class GetItineraryUserReservationsResponse : BaseResponse
    {

        public GetItineraryUserReservationsResponse()
        {
            data = new List<ItineraryUserReservationsModel>();
        }
        public List<ItineraryUserReservationsModel> data { get; set; }

    }

    public class GetItineraryDetailResponse : BaseResponse
    {

        public GetItineraryDetailResponse()
        {
            data = new ItineraryPlannerViewModel();
        }
        public ItineraryPlannerViewModel data { get; set; }

    }

    public class GetOTAListResponse : BaseResponse
    {

        public GetOTAListResponse()
        {
            data = new List<ItineraryPlanner_Ota>();
        }
        public List<ItineraryPlanner_Ota> data { get; set; }

    }

    public class GetBookingTypeListResponse : BaseResponse
    {

        public GetBookingTypeListResponse()
        {
            data = new List<ItineraryPlanner_BookingType>();
        }
        public List<ItineraryPlanner_BookingType> data { get; set; }

    }

    public class CreateItineraryForPassportResponse : BaseResponse
    {
        public CreateItineraryForPassportResponse()
        {
            data = new PassportItineraryResponseModel();
        }
        public PassportItineraryResponseModel data { get; set; }

    }


    public class CheckIfDestinationValidResponse : BaseResponse
    {
        public CheckIfDestinationValidResponse()
        {
            data = new CheckDistanceTravelTimeResponseModel();
        }
        public CheckDistanceTravelTimeResponseModel data { get; set; }

    }

    public class ConfirmItineraryResponse : BaseResponse
    {
        public ConfirmItineraryResponse()
        {
            data = new List<CreateReservation>();
        }
        public List<CreateReservation> data { get; set; }
    }

}
