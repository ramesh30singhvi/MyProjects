using CellarPassAppAdmin.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ICoversReportViewModel
    {
        public Task<List<TablesViewModel>> GetTablesReport(int floor_plan_ids);
        public Task<List<CoversReportLocations>> GetCoversReport(DateTime day, int member_id, int floor_plan_ids);
        public Task<ReservationDetailResponse> GetReservationDetails(int floor_plan_ids);
        public Task<WaitlistResponse> GetWaitLists(int waitlist_id);
        public Task<List<LocationDetailsModel>> GetTimeZoneOffSet(int member_id,bool active_only);
        public Task<List<MessageLogModel>> GetMessageLog(int member_id);
        public Task<List<WaitingListMessage>> GetWaitlistMessage(int member_id,int category);
        public Task<UpdateWaitlistStatusModel> UpdateWaitlistStatus(int id,int status);
        public Task<List<TableGroupModel>> GetTableStatusGroup(int member_id, bool include_default);
        public Task<List<TableGroupItemModel>> GetTableStatusGroupItem(int table_status_group_id);
        public Task<CreateReservationResponse> SaveReservation(ReservationDetailModel model);
    }
}
