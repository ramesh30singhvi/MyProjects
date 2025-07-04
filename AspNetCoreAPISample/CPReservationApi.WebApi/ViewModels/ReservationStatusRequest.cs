using CPReservationApi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    /// <summary>
    /// This class is used for taking request from server to update reservation status
    /// </summary>
    public class ReservationStatusRequest
    {
        public int reservation_id{ get; set; }
        public int status { get; set; }
        public bool send_mail { get; set; }
        public int delay_in_minutes { get; set; }
        public bool refund_deposit { get; set; }
        public string modified_by_name { get; set; }
        public int modified_by_id { get; set; } = 0;
        public string cancellation_reason { get; set; }
    }

    public class ReservationUpdateResponse : BaseResponse
    {
        public ReservationUpdateResponse()
        {
            data = new ReservationNotes();
        }
        public ReservationNotes data { get; set; }
    }

    public class ReservationStatusResponse : BaseResponse
    {
        public ReservationStatusResponse()
        {
            data = new ReservationUpdateStatus();
        }
        public ReservationUpdateStatus data { get; set; }
    }

    public class ReservationUpdateStatus
    {
        public int reservation_id { get; set; }
        public bool refund_attempted { get; set; } = false;
        public bool refund_success { get; set; } = false;
    }

    public class ReservationNotesRequest
    {
        public int reservation_id { get; set; }
        public string concierge_note { get; set; }
        public string internal_note { get; set; }
        public string guest_note { get; set; }
    }

    public class ReservationTagsRequest
    {
        public int reservation_id { get; set; }
        public string tags { get; set; }
    }

    public class UserTagsRequest
    {
        public int member_id { get; set; }
        public int user_id { get; set; }
        public List<int> tags { get; set; }
    }

    public class ReservationNotesRequestv2
    {
        public int reservation_id { get; set; }
        public string concierge_note { get; set; }
        public string internal_note { get; set; }
        public string guest_note { get; set; }
        public string account_note { get; set; }
        public bool account_note_has_changes { get; set; }
    }

    public class ReservationNotesResponse : BaseResponse
    {
        public ReservationNotesResponse()
        {
            data = new ReservationNotes();
        }
        public ReservationNotes data { get; set; }
    }

    public class ReservationNotes
    {
        public int reservation_id { get; set; }
    }

    public class PreAssignTable
    {
        public int transaction_id { get; set; }
    }

    public class PreAssignTableResponse : BaseResponse
    {
        public PreAssignTableResponse()
        {
            data = new PreAssignTable();
        }
        public PreAssignTable data { get; set; }
    }



    public class ReassignTableResponse : BaseResponse
    {
        public ReassignTableResponse()
        {
            data = new PreAssignTable();
        }
        public PreAssignTable data { get; set; }
    }
    public class PreAssignedServerTableRequest
    {
        public int reservation_id { get; set; }
        public int? pre_assign_server_id { get; set; }
        public List<int> pre_assign_table_ids { get; set; }
    }

    public class PreAssignedServerTableRequestAll
    {
        public int transaction_id { get; set; }
        public PreAssignServerTransactionType transaction_type { get; set; }
        public int? pre_assign_server_id { get; set; }
        public int duration_in_minutes { get; set; }
        public List<int> pre_assign_table_ids { get; set; }
        public bool force_assign { get; set; } = false;
    }

    public class ReAssignedTableRequest
    {
        public int transaction_id { get; set; }
        public PreAssignServerTransactionType transaction_type { get; set; }
        public List<int> table_ids { get; set; }
    }

    public class UpdatePartySizeRequest
    {
        public int reservation_id { get; set; }
        public int party_size { get; set; }
        public bool process_payment { get; set; } = false;
    }

    public class AbandonedCartRsvpEmailRequest
    {
        public List<int> ids { get; set; }
    }

    public class AbandonedCartRsvpEmailResponse : BaseResponse
    {
    }

    public class OrderPortSendOrderRequest
    {
        public List<int> reservation_id { get; set; }
        public string api_key { get; set; }
        public string api_token { get; set; }
        public string client_id { get; set; }
        public int member_id { get; set; }
    }
    public class Commerce7SendOrderRequest
    {
        public List<int> reservation_id { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }
        public string tenant_name { get; set; }
        public string pos_profile_id { get; set; }
        public string processed_by {get; set;}
    }
    public class BigCommerceSendOrderRequest
    {
        public List<int> reservation_id { get; set; }
        public int member_id { get; set; }
        public string processed_by { get; set; }
    }
    public class CloseReservationRequest
    {
        public int transaction_id { get; set; }
        public int transaction_category { get; set; }
    }

    public class CloseReservationResponse : BaseResponse
    {
        public CloseReservationResponse()
        {
            data = new CloseReservation();
        }
        public CloseReservation data { get; set; }
    }

    public class CloseReservation
    {
        public int transaction_id { get; set; }
    }

    public class SeatReservationRequest
    {
        public int transaction_id { get; set; }
        public int transaction_category { get; set; }
        public int member_id { get; set; }
        public int party_size { get; set; }
        public int table_status { get; set; }
        public List<int> table_ids { get; set; }
        public int? server_id { get; set; }
        public List<int> location_ids { get; set; }
        public bool force { get; set; }
        public bool process_payment { get; set; } = false;
    }

    public class SeatReservationRequestV2
    {
        public int transaction_id { get; set; }
        public int transaction_category { get; set; }
        public int member_id { get; set; }
        public int party_size { get; set; }
        public int table_status { get; set; }
        public List<int> table_ids { get; set; }
        public int? server_id { get; set; }
        public List<int> floor_plan_ids { get; set; }
        public bool force { get; set; }
        public bool process_payment { get; set; } = false;
        public string user_name { get; set; } = "";
    }

    public class UpdateReservationSeatedStatusRequest
    {
        public int reservation_id { get; set; }
        public int reservation_seated_status { get; set; }
    }

    public class UpdateReservationSeatedStatusResponse : BaseResponse
    {
        public UpdateReservationSeatedStatusResponse()
        {
            data = new Model.UpdateReservationSeatedStatusModel();
        }
        public Model.UpdateReservationSeatedStatusModel data { get; set; }
    }

    public class SendCheckInPromoRequest
    {
        public int id { get; set; }
        public int type { get; set; }
    }

    public class ReservationCancellationReasonResponse : BaseResponse
    {
        public ReservationCancellationReasonResponse()
        {
            data = new ReservationUpdateCancellationReason();
        }
        public ReservationUpdateCancellationReason data { get; set; }
    }

    public class ReservationUpdateCancellationReason
    {
        public int reservation_id { get; set; }
    }

    public class ReservationCancellationReasonRequest
    {
        public int reservation_id { get; set; }       
        public int reason_id { get; set; }
        public int member_id { get; set; }
    }
}
