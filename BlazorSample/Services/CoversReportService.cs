using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class CoversReportService : ICoversReportService
    {
        private readonly IApiManager _apiManager;

        public CoversReportService(IApiManager apiManager)
        {
            _apiManager = apiManager;
        }
        public async Task<List<CoversReportLocations>> GetCoversReportAsync(DateTime day, int member_id, int floor_plan_ids)
        {
            try
            {
                List<CoversReportLocations> receiptSetting = await _apiManager.GetAsync<List<CoversReportLocations>>("FloorPlan/getcoversreport?member_id=" + member_id + "&floor_plan_ids=" + floor_plan_ids + "&day=" + day);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<CoversReportLocations>();
            }
        }
        public async Task<List<TablesViewModel>> GetTablesReportAsync(int floor_plan_ids)
        {
            try
            {
                List<TablesViewModel> receiptSetting = await _apiManager.GetAsync<List<TablesViewModel>>("FloorPlan/gettablesreport?floor_plan_ids=" + floor_plan_ids);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<TablesViewModel>();
            }
        }
        public async Task<ReservationDetailResponse> GetReservationDetailsAsync(int reservation_id)
        {
            try
            {
                ReservationDetailResponse receiptSetting = await _apiManager.GetAsync<ReservationDetailResponse>("FloorPlan/getreservationdetails?reservation_id=" + reservation_id);
                return receiptSetting;
            }
            catch (Exception)
            {
                return new ReservationDetailResponse();
            }
        }
        public async Task<WaitlistResponse> GetWaitListsAsync(int waitlist_id)
        {
            try
            {
                WaitlistResponse receiptSetting = await _apiManager.GetAsync<WaitlistResponse>("FloorPlan/getwaitlists?waitlist_id=" + waitlist_id);
                return receiptSetting;
            }
            catch (Exception)
            {
                return new WaitlistResponse();
            }
        }
        public async Task<List<LocationDetailsModel>> GetTimeZoneOffSetAsync(int member_id, bool active_only)
        {
            try
            {
                List<LocationDetailsModel> receiptSetting = await _apiManager.GetAsync<List<LocationDetailsModel>>("FloorPlan/getlocationdetails?member_id=" + member_id + "&active_only=" + active_only);
                return receiptSetting;
            }
            catch (Exception)
            {
                return new List<LocationDetailsModel>();
            }
        }
        public async Task<List<MessageLogModel>> GetMessageLogAsync(int member_id)
        {
            try
            {
                List<MessageLogModel> receiptSetting = await _apiManager.GetAsync<List<MessageLogModel>>("FloorPlan/getmessagelog?member_id=" + member_id);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<MessageLogModel>();
            }
        }
        public async Task<List<WaitingListMessage>> GetWaitlistMessageAsync(int member_id, int category)
        {
            try
            {
                List<WaitingListMessage> receiptSetting = await _apiManager.GetAsync<List<WaitingListMessage>>("FloorPlan/getwaitlistmessage?member_id=" + member_id+ "&category="+category);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<WaitingListMessage>();
            }
        }
        public async Task<UpdateWaitlistStatusModel> UpdateWaitlistStatusAsync(int id, int status)
        {
            try
            {
                UpdateWaitlistStatusModel receiptSetting = await _apiManager.GetAsync<UpdateWaitlistStatusModel>("FloorPlan/updatewaitliststatus?id=" + id + "&status=" + status);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new UpdateWaitlistStatusModel();
            }
        }
        public async Task<List<TableGroupModel>> GetTableStatusGroupAsync(int member_id, bool include_default)
        {
            try
            {
                List<TableGroupModel> receiptSetting = await _apiManager.GetAsync<List<TableGroupModel>>("FloorPlan/gettablestatusgroup?member_id=" + member_id + "&include_default=" + include_default);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<TableGroupModel>();
            }
        }
        public async Task<List<TableGroupItemModel>> GetTableStatusGroupItemAsync(int table_status_group_id)
        {
            try
            {
                List<TableGroupItemModel> receiptSetting = await _apiManager.GetAsync<List<TableGroupItemModel>>("FloorPlan/gettablestatusgroupitem?table_status_group_id=" + table_status_group_id);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new List<TableGroupItemModel>();
            }
        }
        public async Task<CreateReservationResponse> SaveReservationAsync(ReservationDetailModel model)
        {
            try
            {
                CreateReservationResponse receiptSetting = await _apiManager.PostAsync<ReservationDetailModel, CreateReservationResponse>("FloorPlan/savereservation", model);
                return receiptSetting;
            }
            catch (HttpRequestExceptionEx e)
            {
                Debug.WriteLine(e.HttpCode);
                return new CreateReservationResponse();
            }
        }
    }
}
