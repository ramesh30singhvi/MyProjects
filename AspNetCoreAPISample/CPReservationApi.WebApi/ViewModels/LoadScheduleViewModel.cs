using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    /// <summary>
    /// Load Schedule Request
    /// </summary>
    public class LoadScheduleRequest
    {
        /// <summary>
        /// Id of Member (Required)
        /// </summary>
        public int member_id { get; set; }
        /// <summary>
        /// Date to be searched for the events (Required)
        /// </summary>
        public DateTime req_date { get; set; }
        /// <summary>
        /// Guest Count for which reservation needs to be done (Required)
        /// </summary>
        public int guest_count { get; set; }
        /// <summary>
        /// For search limiting to specific slot of an event, need to pass both slot Id and slot type (Optional)
        /// </summary>
        public int slot_id { get; set; } = 0;
        /// <summary>
        /// Slot type of the event slot (Optional)
        /// </summary>
        public int slot_type { get; set; } = 0;
        /// <summary>
        /// For search limiting to specific event. It is id of the event (Optional)
        /// </summary>
        public int event_id { get; set; } = 0;
        public int rsvp_id { get; set; } = 0;
        public int booking_type { get; set; } = 0;
         public bool hide_no_availability { get; set; } = false;
    }

    public class LoadScheduleResponse : BaseResponse
    {
        public LoadScheduleResponse()
        {
            data = new List<EventScheduleEvent>();
        }
        public List<EventScheduleEvent> data { get; set; }
    }

    public class LoadScheduleEventIdResponse : BaseResponse
    {
        public LoadScheduleEventIdResponse()
        {
            data = new List<EventScheduleModel>();
        }
        public List<EventScheduleModel> data { get; set; }
    }
    public class EventDetailRequest
    {
        /// <summary>
        /// Id of Event (Required)
        /// </summary>
        public int event_id { get; set; } = 0;
        /// <summary>
        /// Date to be searched for the events (Required)
        /// </summary>
        public DateTime req_date { get; set; } = DateTime.Now;
    }

    public class EventSlotDetailRequest
    {
        /// <summary>
        /// Id of Event (Required)
        /// </summary>
        public int event_id { get; set; } = 0;
        /// <summary>
        /// Id of Event (Required)
        /// </summary>
        public int slot_id { get; set; } = 0;
        /// <summary>
        /// Date to be searched for the events (Required)
        /// </summary>
        public DateTime req_date { get; set; } = DateTime.Now;

        public int slot_type { get; set; } = 0;
    }
    public class EventSlotDetailResponse : BaseResponse
    {
        public EventSlotDetailResponse()
        {
            data = new EventScheduleEvent();
        }
        public EventScheduleEvent data { get; set; }
    }



    public class EventDetailResponse : BaseResponse
    {
        public EventDetailResponse()
        {
            data = new EventScheduleEvent();
        }
        public EventScheduleEvent data { get; set; }
    }

    public class LoadEventsResponse : BaseResponse
    {
        public LoadEventsResponse()
        {
            data = new List<Event>();
        }
        public List<Event> data { get; set; }
    }

    public class LoadPrivateEventsResponse : BaseResponse
    {
        public LoadPrivateEventsResponse()
        {
            data = new List<EventData>();
        }
        public List<EventData> data { get; set; }
    }

    public class Schedulev2sResponse : BaseResponse
    {
        public Schedulev2sResponse()
        {
            data = new List<ScheduleV2>();
        }
        public List<ScheduleV2> data { get; set; }
    }

    public class MostBookedEventResponse : BaseResponse
    {
        public MostBookedEventResponse()
        {
            data = new List<MostBookedEvent>();
        }
        public List<MostBookedEvent> data { get; set; }
    }
    public class MostBookedEventTypeResponse : BaseResponse
    {
        public MostBookedEventTypeResponse()
        {
            data = new List<MostBookedEventType>();
        }
        public List<MostBookedEventType> data { get; set; }
    }

    public class Eventv2sResponse : BaseResponse
    {
        public Eventv2sResponse()
        {
            data = new EventV3();
        }
        public EventV3 data { get; set; }
    }

    public class RSVPEventLandingPageResponse : BaseResponse2
    {
        public RSVPEventLandingPageResponse()
        {
            data = new RSVPEventLandingPage();
        }
        public RSVPEventLandingPage data { get; set; }
    }

    public class EventDatev2sResponse : BaseResponse
    {
        public EventDatev2sResponse()
        {
            data = new EventDateV3();
        }
        public EventDateV3 data { get; set; }
    }

    public class AvailableEventsResponse : BaseResponse
    {
        public AvailableEventsResponse()
        {
            data = new AvailableDaysEvent();
        }
        public AvailableDaysEvent data { get; set; }
    }

    public class ReservationCheckoutResponse : BaseResponse
    {
        public ReservationCheckoutResponse()
        {
            data = new ReservationCheckoutModel();
        }
        public ReservationCheckoutModel data { get; set; }
    }

    public class EventTransportationResponse : BaseResponse
    {
        public EventTransportationResponse()
        {
            data = new List<TransportationModel>();
        }
        public List<TransportationModel> data { get; set; }
    }

    public class GuestTagsResponse : BaseResponse
    {
        public GuestTagsResponse()
        {
            data = new List<GuestTags>();
        }
        public List<GuestTags> data { get; set; }
    }

    public class AddTocdnRequest
    {
        public string file_path { get; set; }
    }

    public class AvailableQtyForPrivateReservationResponse : BaseResponse
    {
        public AvailableQtyForPrivateReservationResponse()
        {
            data = new AvailableQtyForPrivateReservationModel();
        }
        public AvailableQtyForPrivateReservationModel data { get; set; }
    }

    public class AvailableQtyForPrivateReservationModel
    {
        public int count { get; set; }
    }

    public class CheckAvailableQtyPrivatersvpResponse : BaseResponse
    {
        public CheckAvailableQtyPrivatersvpResponse()
        {
            data = new CheckAvailableQtyPrivatersvpModel();
        }
        public CheckAvailableQtyPrivatersvpModel data { get; set; }
    }

    public class GetPassportEventAvailabilityResponse : BaseResponse
    {
        public GetPassportEventAvailabilityResponse()
        {
            data = new List<PassportEventAvailabilityModel>();
        }
        public List<PassportEventAvailabilityModel> data { get; set; }
    }

    public class GetPassportEventAvailabilityV2Response : BaseResponse
    {
        public GetPassportEventAvailabilityV2Response()
        {
            data = new List<PassportEventAvailabilityV2Model>();
        }
        public List<PassportEventAvailabilityV2Model> data { get; set; }
    }

    public class ProfileEventsResponse : BaseResponse
    {
        public ProfileEventsResponse()
        {
            data = new List<ProfileEvent>();
        }
        public List<ProfileEvent> data { get; set; }
    }

    public class ScoutOffersResponse : BaseResponse
    {
        public ScoutOffersResponse()
        {
            data = new List<ScoutPromotion>();
        }
        public List<ScoutPromotion> data { get; set; }
    }

    public class PromotionDetailResponse : BaseResponse
    {
        public PromotionDetailResponse()
        {
            data = new PromotionDetail();
        }
        public PromotionDetail data { get; set; }
    }
    public class ClaimOfferResponse : BaseResponse
    {
        public ClaimOfferResponse()
        {
            data = new ClaimModel();
        }
        public ClaimModel data { get; set; }
    }
    public class PromotionsByUserResponse : BaseResponse
    {
        public PromotionsByUserResponse()
        {
            data = new List<PromotionDetailUser>();
        }
        public List<PromotionDetailUser> data { get; set; }
    }

    public class ScoutOffersTypesResponse : BaseResponse
    {
        public ScoutOffersTypesResponse()
        {
            data = new List<CellarScoutOfferTypesModel>();
        }
        public List<CellarScoutOfferTypesModel> data { get; set; }
    }

    public class PromotionsByMemberResponse : BaseResponse
    {
        public PromotionsByMemberResponse()
        {
            data = new List<PromotionDetail>();
        }
        public List<PromotionDetail> data { get; set; }
    }

    public class CellarScoutLocationsResponse : BaseResponse
    {
        public CellarScoutLocationsResponse()
        {
            data = new List<CellarScoutLocationsModel>();
        }
        public List<CellarScoutLocationsModel> data { get; set; }
    }

    public class PrivateConfirmationMessageResponse : BaseResponse
    {
        public PrivateConfirmationMessageResponse()
        {
            data = new List<PrivateConfirmationMessageModel>();
        }
        public List<PrivateConfirmationMessageModel> data { get; set; }
    }
}
