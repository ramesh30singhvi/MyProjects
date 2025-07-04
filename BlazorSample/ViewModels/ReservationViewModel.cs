using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class ReservationViewModel:IReservationViewModel
    {
        private IReservationService _reservationService;
        public ReservationViewModel(IReservationService ReservationService)
        {
            _reservationService = ReservationService;
        }
        public async Task<SetReservationModel> SetReservationStatus(SetReservationRequestModel model)
        {
            SetReservationModel user = await _reservationService.SetReservationStatusAsync(model);
            return user;
        }
        public async Task<SetReservationStatusArrivedResponse> SetReservationStatusArrived(int rsvp_id)
        {
            SetReservationStatusArrivedResponse user = await _reservationService.SetReservationStatusArrivedAsync(rsvp_id);
            return user;
        }
        public async Task<SeatLogModel> SetSeatingStatusUpdatev2(SeatLogModel model)
        {
            SeatLogModel data = await _reservationService.SetSeatingStatusUpdatev2Async(model);
            return data;
        } 
        public async Task<CloseReservationResponse> SetCloseReservationv2(CloseReservationRequest model)
        {
            CloseReservationResponse data = await _reservationService.SetCloseReservationv2Async(model);
            return data;
        } 
        public async Task<UsermemberResponse> GetUsersByRole(int member_id, int role_id)
        {
            UsermemberResponse data = await _reservationService.GetUsersByRoleAsync(member_id, role_id);
            return data;
        }
        public async Task<PreAssignTableResponse> UpdatePreassignedServerTableAll(PreAssignedServerTableRequestAll model)
        {
            PreAssignTableResponse data = await _reservationService.UpdatePreassignedServerTableAllAsync(model);
            return data;
        }
    }
}
