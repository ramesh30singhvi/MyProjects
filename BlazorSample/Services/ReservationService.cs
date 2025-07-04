using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.RequestModel;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IApiManager _apiManager;

        public ReservationService(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }
        public async Task<SetReservationModel> SetReservationStatusAsync(SetReservationRequestModel model)
        {
            try
            {
                SetReservationModel receiptSetting = await _apiManager.PostAsync<SetReservationRequestModel, SetReservationModel>("Reservation/setreservationstatus", model);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SetReservationModel();
            }
        }
        public async Task<SetReservationStatusArrivedResponse> SetReservationStatusArrivedAsync(int rsvp_id)
        {
            try
            {
                SetReservationStatusArrivedResponse receiptSetting = await _apiManager.GetAsync<SetReservationStatusArrivedResponse>("Reservation/setreservationstatusarrived?rsvp_id=" + rsvp_id);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SetReservationStatusArrivedResponse();
            }
        }

        public async Task<SeatLogModel> SetSeatingStatusUpdatev2Async(SeatLogModel model)
        {
            try
            {
                SeatLogModel receiptSetting = await _apiManager.PostAsync<SeatLogModel, SeatLogModel>("Reservation/setseatingstatusupdatev2", model);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new SeatLogModel();
            }
        }
        public async Task<CloseReservationResponse> SetCloseReservationv2Async(CloseReservationRequest model)
        {
            try
            {
                CloseReservationResponse receiptSetting = await _apiManager.PostAsync<CloseReservationRequest, CloseReservationResponse>("Reservation/setclosereservationv2", model);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CloseReservationResponse();
            }
        }
        public async Task<UsermemberResponse> GetUsersByRoleAsync(int member_id, int role_id)
        {
            try
            {
                UsermemberResponse receiptSetting = await _apiManager.GetAsync<UsermemberResponse>("Reservation/getusersbyrole?member_id=" + member_id + "&role_id=" + role_id);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UsermemberResponse();
            }
        }
        public async Task<PreAssignTableResponse> UpdatePreassignedServerTableAllAsync(PreAssignedServerTableRequestAll model)
        {
            try
            {
                PreAssignTableResponse receiptSetting = await _apiManager.PostAsync<PreAssignedServerTableRequestAll, PreAssignTableResponse>("Reservation/updatepreassignedservertableall", model);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new PreAssignTableResponse();
            }
        }
    }
}
