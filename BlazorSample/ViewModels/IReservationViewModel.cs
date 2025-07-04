using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface IReservationViewModel
    {
        public Task<SetReservationModel> SetReservationStatus(SetReservationRequestModel model);
        public Task<SetReservationStatusArrivedResponse> SetReservationStatusArrived(int rsvp_id);
        public Task<SeatLogModel> SetSeatingStatusUpdatev2(SeatLogModel model);
        public Task<CloseReservationResponse> SetCloseReservationv2(CloseReservationRequest model);
        public Task<UsermemberResponse> GetUsersByRole(int member_id, int role_id);
        public Task<PreAssignTableResponse> UpdatePreassignedServerTableAll(PreAssignedServerTableRequestAll model);

    }
}
