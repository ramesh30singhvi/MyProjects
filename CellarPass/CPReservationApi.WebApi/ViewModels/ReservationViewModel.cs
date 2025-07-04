using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPReservationApi.Model;

namespace CPReservationApi.WebApi.ViewModels
{
    public class ReservationRequest
    {
        /// <summary>
        /// Id of Member
        /// </summary>
        public int member_id { get; set; } = 0;
        /// <summary>
        /// Start Search Date
        /// </summary>
        public DateTime? start_date { get; set; } = null;
        /// <summary>
        /// End Search Date
        /// </summary>
        public DateTime? end_date { get; set; } = null;
        /// <summary>
        /// Id of Reservation
        /// </summary>
        public int reservation_id { get; set; } = 0;
        /// <summary>
        /// Id of User
        /// </summary>
        public int user_id { get; set; } = 0;
        /// <summary>
        /// Search Mode (ByEventDate=0, ByBookDate=1)
        /// </summary>
        public int mode { get; set; } = 0;
        /// <summary>
        /// Booking Code of Reservation 
        /// </summary>
        public string booking_code { get; set; } = "";
        /// <summary>
        /// Email of User
        /// </summary>
        public string email { get; set; } = "";
        /// <summary>
        /// Name of Event
        /// </summary>
        public string event_name { get; set; } = "";
        /// <summary>
        /// Id's of Location
        /// </summary>
        public string location_ids { get; set; } ="";
        public string floor_plan_ids { get; set; } = "";
        /// <summary>
        /// Name of Last Name
        /// </summary>
        public string last_name { get; set; } = "";
        /// <summary>
        /// Name of Phone Number
        /// </summary>
        public string phone_number { get; set; } = "";
    }

    public class ReservationResponse : BaseResponse2
    {
        public ReservationResponse()
        {
            data = new List<ReservationEvent>();
        }
        public List<ReservationEvent> data { get; set; }
    }

    public class ReservationDetailResponse : BaseResponse2
    {
        public ReservationDetailResponse()
        {
            data = new ReservationDetailModel();
        }
        public ReservationDetailModel data { get; set; }
    }

    public class ReservationDetailV2Response : BaseResponse2
    {
        public ReservationDetailV2Response()
        {
            data = new ReservationDetailV2Model();
        }
        public ReservationDetailV2Model data { get; set; }
    }

    public class ReservationDetailV4Response : BaseResponse2
    {
        public ReservationDetailV4Response()
        {
            data = new ReservationDetailV4Model();
        }
        public ReservationDetailV4Model data { get; set; }
    }

    public class ReservationDiscountLogResponse : BaseResponse2
    {
        public ReservationDiscountLogResponse()
        {
            data = new List<ReservationChangeLog>();
        }
        public List<ReservationChangeLog> data { get; set; }
    }

    public class ReservationPaymentLogResponse : BaseResponse2
    {
        public ReservationPaymentLogResponse()
        {
            data = new ReservationPaymentLogModel();
        }
        public ReservationPaymentLogModel data { get; set; }
    }

    public class ReservationNoteLogResponse : BaseResponse2
    {
        public ReservationNoteLogResponse()
        {
            data = new ReservationNoteLogModel();
        }
        public ReservationNoteLogModel data { get; set; }
    }

    public class RefundReservationPaymentRequest
    {
        public int reservation_id { get; set; }
        public decimal amount { get; set; }
    }

    public class ReservationPaymentRequest
    {
        public int reservation_id { get; set; }
        public decimal amount { get; set; }
        public decimal gratuity_amount { get; set; }
    }

    public class ReservationAddCreditCardRequest
    {
        public int member_id { get; set; }
        public int reservation_id { get; set; }
        public string number { get; set; }
        public string cust_name { get; set; }
        public string exp_month { get; set; }
        public string exp_year { get; set; }
        public string cvv2 { get; set; }
        public string zip_code { get; set; }
        public string card_token { get; set; }
        public string card_type { get; set; }
    }

    public class FinishReservationRequest
    {
        public int reservation_id { get; set; }
        public bool is_modify { get; set; }

        public int action_type { get; set; } = 0;

        public bool send_guest_email { get; set; }

        public bool send_affiliate_email { get; set; }
        public string personal_message { get; set; }
    }

    public class DeleteMeetingRequest
    {
        public int reservation_id { get; set; }
        public int member_id { get; set; }
        public int status { get; set; }
        public string email { get; set; }
    }

    public class ReservationPaymentResponse : BaseResponse2
    {
        public ReservationPaymentResponse()
        {
            data = new Common.Payments.TransactionResult();
        }
        public Common.Payments.TransactionResult data { get; set; }
    }

    public class SalesTaxPercentResponse : BaseResponse2
    {
        public SalesTaxPercentResponse()
        {
            data = 0;
        }
        public decimal data { get; set; }
    }

    public class WillCallLocationCheckResponse : BaseResponse2
    {
        public WillCallLocationCheckResponse()
        {
            data = new List<WillCallLocationCheckModel>();
        }
        public List<WillCallLocationCheckModel> data { get; set; }
    }

    public class AvailablewclocationsfororderResponse : BaseResponse2
    {
        public AvailablewclocationsfororderResponse()
        {
            data = new List<WillCallLocationDetail>();
        }
        public List<WillCallLocationDetail> data { get; set; }
    }

    public class AvailablewclocationsfororderV2Response : BaseResponse2
    {
        public AvailablewclocationsfororderV2Response()
        {
            data = new List<TicketLevelWillCallLocationDetail>();
        }
        public List<TicketLevelWillCallLocationDetail> data { get; set; }
    }

    public class VerifyForMobileResponse : BaseResponse2
    {
        public VerifyForMobileResponse()
        {
            data = new VerifyForMobileModel();
        }
        public VerifyForMobileModel data { get; set; }
    }

    public class VerifyForMobileModel
    {
        public bool sms_Verified { get; set; }
    }

    public class RecalculateReservationTotalRequest
    {
        public int reservation_id { get; set; }
        public int party_size { get; set; }
    }

    public class RecalculateReservationTotalResponse : BaseResponse2
    {
        public RecalculateReservationTotalResponse()
        {
            data = new RecalculateReservationTotalModel();
        }
        public RecalculateReservationTotalModel data { get; set; }
    }

    public class RecalculateReservationTotalModel
    {
        public decimal refund_due { get; set; }
        public decimal additional_amount_due { get; set; }
        public decimal new_balance_due { get; set; }
    }

    public class ReservationDataResponse : BaseResponse2
    {
        public ReservationDataResponse()
        {
            data = new List<ReservationDataEvent>();
        }
        public List<ReservationDataEvent> data { get; set; }
    }
    public class RSVPReviewsResponse : BaseResponse2
    {
        public RSVPReviewsResponse()
        {
            data = new List<RSVPReviewModel>();
        }
        public List<RSVPReviewModel> data { get; set; }
    }

    public class GetLocationMapResponse : BaseResponse2
    {
        public GetLocationMapResponse()
        {
            data = new GetLocationMapViewModel();
        }
        public GetLocationMapViewModel data { get; set; }
    }

    public class ReservationInviteModelResponse : BaseResponse2
    {
        public ReservationInviteModelResponse()
        {
            data = new ReservationInviteModel();
        }
        public ReservationInviteModel data { get; set; }
    }

    public class ReservationDetailV3Response : BaseResponse2
    {
        public ReservationDetailV3Response()
        {
            data = new ReservationDetailV3Model();
        }
        public ReservationDetailV3Model data { get; set; }
    }

    public class UserReservationsResponse : BaseResponse2
    {
        public UserReservationsResponse()
        {
            data = new List<UserReservationsModel>();
        }
        public List<UserReservationsModel> data { get; set; }
    }

    public class BusinessSearchResponse : BaseResponse2
    {
        public BusinessSearchResponse()
        {
            data = new List<BusinessSearchResultModel>();
        }
        public List<BusinessSearchResultModel> data { get; set; }
    }
}
