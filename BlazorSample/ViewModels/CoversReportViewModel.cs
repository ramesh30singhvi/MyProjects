using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class CoversReportViewModel : ICoversReportViewModel
    {
        private ICoversReportService _coversReportService;
        public CoversReportViewModel(ICoversReportService CoversReportService)
        {
            _coversReportService = CoversReportService;
        }
        public async Task<List<CoversReportLocations>> GetCoversReport(DateTime day, int member_id, int floor_plan_ids)
        {
            List<CoversReportLocations> user = await _coversReportService.GetCoversReportAsync(day, member_id, floor_plan_ids);
            return user;
        }
        public async Task<List<TablesViewModel>> GetTablesReport(int floor_plan_ids)
        {
            List<TablesViewModel> user = await _coversReportService.GetTablesReportAsync(floor_plan_ids);
            return user;
        }
        public async Task<ReservationDetailResponse> GetReservationDetails(int floor_plan_ids)
        {
            ReservationDetailResponse user = await _coversReportService.GetReservationDetailsAsync(floor_plan_ids);
            return user;
        }
        public async Task<WaitlistResponse> GetWaitLists(int waitlist_id)
        {
            WaitlistResponse user = await _coversReportService.GetWaitListsAsync(waitlist_id);
            return user;
        }
        public async Task<List<LocationDetailsModel>> GetTimeZoneOffSet(int member_id, bool active_only)
        {
            List<LocationDetailsModel> user = await _coversReportService.GetTimeZoneOffSetAsync(member_id, active_only);
            return user;
        }
        public async Task<List<MessageLogModel>> GetMessageLog(int member_id)
        {
            List<MessageLogModel> user = await _coversReportService.GetMessageLogAsync(member_id);
            return user;
        }
        public async Task<List<WaitingListMessage>> GetWaitlistMessage(int member_id, int category)
        {
            List<WaitingListMessage> user = await _coversReportService.GetWaitlistMessageAsync(member_id,category);
            return user;
        }
        public async Task<UpdateWaitlistStatusModel> UpdateWaitlistStatus(int id, int status)
        {
            UpdateWaitlistStatusModel user = await _coversReportService.UpdateWaitlistStatusAsync(id, status);
            return user;
        }
        public async Task<List<TableGroupModel>> GetTableStatusGroup(int member_id, bool include_default)
        {
            List<TableGroupModel> user = await _coversReportService.GetTableStatusGroupAsync(member_id, include_default);
            return user;
        }
        public async Task<List<TableGroupItemModel>> GetTableStatusGroupItem(int table_status_group_id)
        {
            List<TableGroupItemModel> user = await _coversReportService.GetTableStatusGroupItemAsync(table_status_group_id);
            return user;
        }
        public async Task<CreateReservationResponse> SaveReservation(ReservationDetailModel model)
        {
            CreateReservationResponse user = await _coversReportService.SaveReservationAsync(model);
            return user;
        }
    }
}
