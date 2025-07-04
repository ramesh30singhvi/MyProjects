using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CPReservationApi.Common.Payments;

namespace CPReservationApi.WebApi.ViewModels
{
    public class TicketEventResponse : BaseResponse2
    {
        public TicketEventResponse()
        {
            data = new List<TicketEventModel>();
        }
        public List<TicketEventModel> data { get; set; }
    }

    public class ProfileTicketEventResponse : BaseResponse2
    {
        public ProfileTicketEventResponse()
        {
            data = new List<ProfileTicketEventModel>();
        }
        public List<ProfileTicketEventModel> data { get; set; }
    }

    public class TicketEventDetailResponse : BaseResponse2
    {
        public TicketEventDetailResponse()
        {
            data = new TicketEventDetailModel();
        }
        public TicketEventDetailModel data { get; set; }
    }

    public class TicketEventLandingPageResponse : BaseResponse2
    {
        public TicketEventLandingPageResponse()
        {
            data = new TicketEventLandingPageModel();
        }
        public TicketEventLandingPageModel data { get; set; }
    }
    public class PassportEventDetailResponse : BaseResponse2
    {
        public PassportEventDetailResponse()
        {
            data = new PassportEventDetailModel();
        }
        public PassportEventDetailModel data { get; set; }
    }

    public class TicketOrderDetailResponse : BaseResponse2
    {
        public TicketOrderDetailResponse()
        {
            data = new TicketOrderModel();
        }
        public TicketOrderModel data { get; set; }
    }

    public class TicketOrderDetailV2Response : BaseResponse2
    {
        public TicketOrderDetailV2Response()
        {
            data = new TicketOrderV2Model();
        }
        public TicketOrderV2Model data { get; set; }
    }

    public class TicketDetailResponse : BaseResponse2
    {
        public TicketDetailResponse()
        {
            data = new TicketOrderClaimModel();
        }
        public TicketOrderClaimModel data { get; set; }
    }

    public class TicketWaitlistDetailResponse : BaseResponse2
    {
        public TicketWaitlistDetailResponse()
        {
            data = new TicketWaitlistDetail();
        }
        public TicketWaitlistDetail data { get; set; }
    }

    public class TicketEventMetricsResponse : BaseResponse2
    {
        public TicketEventMetricsResponse()
        {
            data = new TicketEventMetrics();
        }
        public TicketEventMetrics data { get; set; }
    }

    public class TicketEventPassportMetricsResponse : BaseResponse2
    {
        public TicketEventPassportMetricsResponse()
        {
            data = new TicketPassportEventMetrics();
        }
        public TicketPassportEventMetrics data { get; set; }
    }

    public class TicketResponse : BaseResponse2
    {
        public TicketResponse()
        {
            data = new List<TicketModel>();
        }
        public List<TicketModel> data { get; set; }
    }

    public class TicketLevelResponse : BaseResponse2
    {
        public TicketLevelResponse()
        {
            data = new TicketLevel();
        }
        public TicketLevel data { get; set; }
    }

    public class TicketLevel
    {
        public int event_remaining_qty { get; set; }
        //public List<EventWillCallLocation> event_will_call_location_details { get; set; }
        public List<TicketLevelModel> ticket_levels { get; set; }
    }

    public class CheckInTicketResponse : BaseResponse2
    {
        public CheckInTicketResponse()
        {
            data = new TicketModel();
        }
        public TicketModel data { get; set; }
    }

    public class CheckInTicketRequest
    {
        public string barcode { get; set; }
        public int event_id { get; set; }
        //public int location_id { get; set; }
        public bool is_multi_event { get; set; }
        public bool is_test { get; set; }
    }

    public class PassportCheckInTicketRequest
    {
        public string barcode { get; set; }
        public int event_id { get; set; }
        public int member_id { get; set; }
        public bool is_test { get; set; }
    }


    public class TixOrderCalculationRequest
    {
        public int member_id { get; set; }
        public int event_id { get; set; }
        public string discount_code { get; set; }
        public int user_id { get; set; }
        public Guid? cart_guid { get; set; } = default(Guid);
        public List<TicketLevelForTax> ticket_levels { get; set; }
        public string card_type { get; set; } = "";
        public bool bypass_check { get; set; } = false;
        public int discount_type { get; set; } = 0;
        public int discount_id { get; set; } = 0;
        public string email_address { get; set; } = string.Empty;
    }



    public class TixOrderCalculationResponse : BaseResponse2
    {

        public TixOrderCalculationResponse()
        {
            data = new TixOrderCalculationModel();
        }
        public TixOrderCalculationModel data { get; set; }

    }

    public class SaveTicketOrderResponse : BaseResponse2
    {

        public SaveTicketOrderResponse()
        {
            data = new SaveTicketnResponseModel();
        }
        public SaveTicketnResponseModel data { get; set; }

    }

    public class SaveTicketWaitlistResponse : BaseResponse2
    {

        public SaveTicketWaitlistResponse()
        {
            data = new SaveTicketWaitlistResponseModel();
        }
        public SaveTicketWaitlistResponseModel data { get; set; }

    }

    public class TicketPostCaptureResponse : BaseResponse2
    {

        public TicketPostCaptureResponse()
        {
            //data = new SaveTicketWaitlistResponseModel();
        }
        //public SaveTicketWaitlistResponseModel data { get; set; }

    }

    public class TicketPostCaptureInviteResponse : BaseResponse2
    {

        public TicketPostCaptureInviteResponse()
        {
            data = new TicketPostCaptureInviteResponseModel();
        }
        public TicketPostCaptureInviteResponseModel data { get; set; }

    }

    public class SaveTicketWaitlistResponseModel
    {
        public int waitlist_id { get; set; }
        public Guid waitlist_guid { get; set; }
        public string event_url { get; set; }
    }

    public class TicketPostCaptureInviteResponseModel
    {
        public int ticket_id { get; set; }
        public string ticket_guid { get; set; }
    }

    public class SaveTicketnResponseModel
    {
        public int order_id { get; set; }
        public Guid order_guid { get; set; }
        public Common.SaveType save_type { get; set; }
        public string message { get; set; }
        public string payment_message { get; set; }
    }

    public class PassportParticipatingMemberResponse : BaseResponse2
    {
        public PassportParticipatingMemberResponse()
        {
            data = new List<PassportParticipatingMemberModel>();
        }
        public List<PassportParticipatingMemberModel> data { get; set; }
    }

    public class ActivatePassportResponse : BaseResponse2
    {
        public ActivatePassportResponse()
        {
            data = new ActivatePassportModel();
        }
        public ActivatePassportModel data { get; set; }
    }

    public class TicketPaymentRequest
    {
        public int ticket_order_id { get; set; }
        public decimal amount { get; set; }
        public string pay_card_number { get; set; }
        public string pay_card_custName { get; set; }
        public string pay_card_exp_month { get; set; }
        public string pay_card_exp_year { get; set; }
        public string pay_card_last_four_digits { get; set; }
        //public string pay_card_first_four_digits { get; set; }
        public string pay_card_type { get; set; }
        public string card_token { get; set; }
        public decimal fee_total { get; set; }
    }

    public class TicketRefundRequest
    {
        public int ticket_order_id { get; set; }
        public decimal amount { get; set; }
        public string transaction_id { get; set; }
        public Common.Payments.Transaction.ChargeType charge_type { get; set; }
        public Configuration.Gateway payment_gateway { get; set; }
        public string pay_card_number { get; set; }
        public string pay_card_custName { get; set; }
        public string pay_card_exp_month { get; set; }
        public string pay_card_exp_year { get; set; }
        public string pay_card_last_four_digits { get; set; }
        public string pay_card_first_four_digits { get; set; }
        public string pay_card_type { get; set; }
        public string card_token { get; set; }
    }

    public class OrderPDFStrResponse : BaseResponse2
    {
        public OrderPDFStrResponse()
        {
            data = new OrderPDFStrModel();
        }
        public OrderPDFStrModel data { get; set; }
    }

    public class PassportItineraryInstructionResponse : BaseResponse2
    {
        public PassportItineraryInstructionResponse()
        {
            data = new PassportItineraryInstructionModel();
        }
        public PassportItineraryInstructionModel data { get; set; }
    }

    public class TicketEventsComponentResponse : BaseResponse2
    {
        public TicketEventsComponentResponse()
        {
            data = new List<UpcomingEventModel>();
        }
        public List<UpcomingEventModel> data { get; set; }
    }

    public class TicketOrderTicketDetailV2Response : BaseResponse2
    {
        public TicketOrderTicketDetailV2Response()
        {
            data = new TixOrderTicketV2();
        }
        public TixOrderTicketV2 data { get; set; }
    }

    public class EventReviewsResponse : BaseResponse2
    {
        public EventReviewsResponse()
        {
            data = new List<EventReviewModel>();
        }
        public List<EventReviewModel> data { get; set; }
    }

    public class TicketEventByRegionResponse : BaseResponse2
    {
        public TicketEventByRegionResponse()
        {
            data = new List<TicketEventDetail2Model>();
        }
        public List<TicketEventDetail2Model> data { get; set; }
    }   

    public class TicketsFaqByEventResponse : BaseResponse2
    {
        public TicketsFaqByEventResponse()
        {
            data = new List<TicketsFaqModel>();
        }
        public List<TicketsFaqModel> data { get; set; }
    }

    public class TicketOrderV2Response : BaseResponse2
    {
        public TicketOrderV2Response()
        {
            data = new List<TicketOrderV2>();
        }
        public List<TicketOrderV2> data { get; set; }
    }

    public class UserOrdersResponse : BaseResponse2
    {
        public UserOrdersResponse()
        {
            data = new List<UserOrdersModel>();
        }
        public List<UserOrdersModel> data { get; set; }
    }

    public class UserTicketResponse : BaseResponse2
    {
        public UserTicketResponse()
        {
            data = new List<UserTicketModel>();
        }
        public List<UserTicketModel> data { get; set; }
    }

    public class TicketeventsByEventTypeResponse : BaseResponse2
    {
        public TicketeventsByEventTypeResponse()
        {
            data = new List<TicketEventByEventTypeModel>();
        }
        public List<TicketEventByEventTypeModel> data { get; set; }
    }

    public class TicketReviewResponse : BaseResponse2
    {
        public TicketReviewResponse()
        {
            data = new TicketReviewPostModel();
        }
        public TicketReviewPostModel data { get; set; }
    }
}
