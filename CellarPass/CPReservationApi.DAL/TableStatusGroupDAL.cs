using CPReservationApi.Common;
using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Text;

namespace CPReservationApi.DAL
{
    public class TableStatusGroupDAL : BaseDataAccess
    {
        public TableStatusGroupDAL(string connectionString) : base(connectionString)
        {
        }

        public List<TableStatusGroupModel> GetTableStatusGroup(int member_id, bool include_default = false)
        {
            string sp = "GetTableStatusGroup";
            var tableStatusGroup = new List<TableStatusGroupModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@member_id", member_id));
            parameterList.Add(GetParameter("@include_default", include_default));
            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new TableStatusGroupModel
                        {
                            group_id = Convert.ToInt32(dataReader["Id"]),
                            group_name = Convert.ToString(dataReader["Name"]),
                            group_items = GetTableStatusGroupItemByTableStatusGroupId(Convert.ToInt32(dataReader["Id"]))
                        };
                        tableStatusGroup.Add(model);
                    }
                }
            }
            return tableStatusGroup;
        }

        public List<TableStatusGroupItem> GetTableStatusGroupItemByTableStatusGroupId(int tableStatusGroupId)
        {
            string sp = "GetTableStatusGroupItemByTableStatusGroupId";
            var tableStatusGroupItem = new List<TableStatusGroupItem>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Table_Status_Group_Id", tableStatusGroupId));
            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new TableStatusGroupItem
                        {
                            id = Convert.ToInt32(dataReader["Id"]),
                            color = GetColorById(Convert.ToInt32(dataReader["Id"])),
                            name = Convert.ToString(dataReader["Name"]),
                            //SortOrder = Convert.ToInt32(dataReader["SortOrder"])
                        };
                        tableStatusGroupItem.Add(model);
                    }
                }
            }
            return tableStatusGroupItem;
        }

        public string GetColorById(int TableStatusGroupItemId)
        {
            string color = string.Empty;
            switch (TableStatusGroupItemId)
            {
                case 2:
                    color = Common.Common.GetEnumDescription(Common.Common.Color.seated);
                    break;
                case 3:
                    color = Common.Common.GetEnumDescription(Common.Common.Color.partially_seated);
                    break;
                case 4:
                    color = Common.Common.GetEnumDescription(Common.Common.Color.first_course);
                    break;
                case 5:
                    color = Common.Common.GetEnumDescription(Common.Common.Color.check);
                    break;
                case 6:
                    color = Common.Common.GetEnumDescription(Common.Common.Color.bus);
                    break;
                case 9:
                    color = Common.Common.GetEnumDescription(Common.Common.Color.terminated);
                    break;
            }
            return color;
        }
        public int SaveTableStatusGroup(CreateTableStatusGroupRequest model)
        {
            int responseid = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@Winery_Id", model.member_id));
            parameterList.Add(GetParameter("@Name", model.name));
            parameterList.Add(GetParameter("@CreatedBy", model.user_id));

            responseid = Convert.ToInt32(ExecuteScalar("InsertTableStatusGroup", parameterList));

            return responseid;
        }

        public bool UpdateTableStatusGroup(CreateTableStatusGroupRequest model)
        {
            bool retval = false;
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@Id", model.id));
            parameterList.Add(GetParameter("@Name", model.name));
            parameterList.Add(GetParameter("@ModifiedBy", model.user_id));

            ExecuteScalar("UpdateTableStatusGroup", parameterList);
            retval = true;
            return retval;
        }

        public bool UpdateTableStatusGroupItem(UpdateTableStatusGroupItem model)
        {
            bool retval = false;
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@Id", model.Id));
            parameterList.Add(GetParameter("@Table_Status_Group_Id", model.TableStatusGroupId));
            parameterList.Add(GetParameter("@Name", model.Name));

            ExecuteScalar("UpdateTableStatusGroupItem", parameterList);
            retval = true;
            return retval;
        }

        public int ForceTableBlocked(int TableId, int MemberId, DateTime StartDate, DateTime EndDate)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@TableId", TableId));
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));

            int responseid = Convert.ToInt32(ExecuteScalar("ForceTableBlocked", parameterList));

            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            var model = new CreateDeltaRequest();
            model.item_id = responseid;
            model.item_type = (int)ItemType.TableBlockingIntervals;
            var statusDetails = GetTableStatusDetails(responseid);

            model.location_id = statusDetails.LocationId;
            model.member_id = MemberId;

            if (statusDetails.InventoryMode == 1)
            {
                model.floor_plan_id = statusDetails.FloorplanId;
            }
                model.action_date = DateTime.UtcNow;
            notificationDAL.SaveDelta(model);
            return responseid;
        }

        public int GetTableBlocked(int TableId, DateTime StartDate, DateTime EndDate, ref DateTime sdate, ref DateTime edate, ref string staus)
        {
            var parameterList = new List<DbParameter>();
            int isAvailable = 0;


            parameterList.Add(GetParameter("@TableId", TableId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));

            using (DbDataReader dataReader = GetDataReader("GetTableBlocked", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        isAvailable = 1;
                        staus = Convert.ToString(dataReader["Status"]);
                        sdate = Convert.ToDateTime(dataReader["StartDateTime"]);
                        edate = Convert.ToDateTime(dataReader["EndDateTime"]);
                    }
                }
            }
            return isAvailable;
        }


        public TableStatusDetails GetTableStatusDetails(int id)
        {
            var parameterList = new List<DbParameter>();
            TableStatusDetails statusDetails = new TableStatusDetails();

            parameterList.Add(GetParameter("@id", id));

            using (DbDataReader dataReader = GetDataReader("GetTableStatusDetailsById", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    statusDetails = new TableStatusDetails();
                    while (dataReader.Read())
                    {
                        statusDetails.MemberId = Convert.ToInt32(dataReader["MemberId"]);
                        statusDetails.FloorplanId = Convert.ToInt32(dataReader["Floor_Plan_Id"]);
                        statusDetails.InventoryMode = Convert.ToInt32(dataReader["InventoryMode"]);
                        statusDetails.TableId = Convert.ToInt32(dataReader["TableId"]);
                        statusDetails.LocationId = Convert.ToInt32(dataReader["LocationId"]);
                    }
                }
            }
            return statusDetails;
        }

        public List<BlockTables> GetTableBlockedByMemberId(int MemberId, DateTime StartDate, DateTime EndDate)
        {
            var parameterList = new List<DbParameter>();
            var blockTables = new List<BlockTables>();
            var blockTable = new BlockTables();

            int OldTableId = 0;

            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));

            try
            {
                using (DbDataReader dataReader = GetDataReader("GetTableBlockedByMemberId", parameterList, CommandType.StoredProcedure))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            int TableId = Convert.ToInt32(dataReader["TableId"]);
                            if (OldTableId == 0 || OldTableId != TableId)
                            {
                                blockTable = new BlockTables();
                                OldTableId = TableId;
                                blockTable.table_id = TableId;
                                blockTable.blocked = new List<BlockedTime>();
                                blockTables.Add(blockTable);
                            }

                            var blockedTime = new BlockedTime();
                            blockedTime.id = Convert.ToInt32(dataReader["Id"]);
                            blockedTime.start_date = Convert.ToDateTime(dataReader["StartDateTime"]);
                            blockedTime.end_date = Convert.ToDateTime(dataReader["EndDateTime"]);
                            blockTable.blocked.Add(blockedTime);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return blockTables;
        }

        public bool DeleteBlockTables(int id)
        {
            int retval = 0;

            var statusDetails = GetTableStatusDetails(id);

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", id));

            string sql = "DELETE FROM [TableStatus] WHERE Id = @Id";

            retval = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            if (retval > 0)
            {

                
                var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                var model = new CreateDeltaRequest();
                model.item_id = id;
                model.item_type = (int)ItemType.TableBlockingIntervals;
                model.location_id = statusDetails.LocationId;
                model.member_id = statusDetails.MemberId;
                if (statusDetails.InventoryMode == 1)
                {
                    model.floor_plan_id = statusDetails.FloorplanId;
                }
                model.action_date = DateTime.UtcNow;
                notificationDAL.SaveDelta(model);
            }
            return retval > 0;
        }

        public int GetLocationIdByTableStatusId(int Id)
        {
            var parameterList = new List<DbParameter>();
            int locationId = 0;

            string sql = "select locationid from Table_Layout where tableid = (select tableid from [TableStatus] where id=@Id)";
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        locationId = Convert.ToInt32(dataReader["locationid"]);
                    }
                }
            }
            return locationId;
        }

        public int GetFloorPlanIdByTableStatusId(int Id)
        {
            var parameterList = new List<DbParameter>();
            int floorplanId = 0;

            string sql = "select top 1 Floor_Plan_Id from Table_Layout where tableid = (select tableid from [TableStatus] where id=@Id)";
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        floorplanId = Convert.ToInt32(dataReader["Floor_Plan_Id"]);
                    }
                }
            }
            return floorplanId;
        }

        public TableDetails GetTableDetailsById(int TableId, DateTime StartDate, DateTime EndDate)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var parameterList = new List<DbParameter>();
            var tableDetails = new TableDetails();
            var assignedServer = new AssignedServer();

            tableDetails.table_id = TableId;

            //parameterList.Add(GetParameter("@TableId", TableId));
            //string sql = "select top 1 [User_Id] from Seating_Session where table_id = @TableId and [User_Id] > 0 order by id desc";

            //using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            //{
            //    if (dataReader != null && dataReader.HasRows)
            //    {
            //        while (dataReader.Read())
            //        {
            //            assignedServer.user_id = Convert.ToInt32(dataReader["User_Id"]);
            //        }
            //    }
            //}

            var parameterList1 = new List<DbParameter>();

            int current_guest_count = 0;
            int total_guest_count = 0;

            parameterList1.Add(GetParameter("@TableId", TableId));
            //parameterList1.Add(GetParameter("@UserId", assignedServer.user_id));
            parameterList1.Add(GetParameter("@StartDateTime", StartDate));

            using (DbDataReader dataReader = GetDataReader("Seating_StatusPerTable_ByTable", parameterList1, CommandType.StoredProcedure))
            {
                int status = 0;
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int logId = Convert.ToInt32(dataReader["LatestLogId"]);

                        if (logId > 0)
                        {

                            status = Convert.ToInt32(dataReader["Status"]);
                            if (status >= 2 && status <= 7)
                            {
                                assignedServer.user_id = Convert.ToInt32(dataReader["User_Id"]);
                            }
                            else
                            {
                                int serverMode = Convert.ToInt32(dataReader["ServerMode"]);
                                if (serverMode != 0 || serverMode != 3)
                                {
                                    assignedServer.user_id = Convert.ToInt32(dataReader["LastServerId"]);
                                }
                            }
                        }
                        else
                        {
                            //table is free
                            int serverMode = Convert.ToInt32(dataReader["ServerMode"]);
                            if (serverMode != 0 || serverMode != 3)
                            {
                                assignedServer.user_id= Convert.ToInt32(dataReader["LastServerId"]);
                            }
                        }

                            
                        current_guest_count = Convert.ToInt32(dataReader["NumberSeated"]);

                        total_guest_count = Convert.ToInt32(dataReader["TotalSeated"]);
                    }
                }
            }

            assignedServer.current_guest_count = current_guest_count;
            assignedServer.total_guest_count = total_guest_count;

            tableDetails.assigned_server = assignedServer;
            tableDetails.schedule = GetTableSchedulesByTableId(TableId, StartDate, EndDate);

            return tableDetails;
        }

        public List<Schedule> GetTableSchedulesByTableId(int TableId, DateTime StartDate, DateTime EndDate)
        {
            string sp = "GetTableSchedulesByTableId";
            var tableSchedule = new List<Schedule>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableId", TableId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));
            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new Schedule
                        {
                            start_date = Convert.ToDateTime(dataReader["startdate"]),
                            end_date = Convert.ToDateTime(dataReader["enddate"]),
                            status = Convert.ToInt32(dataReader["status"]),
                            transaction_id = Convert.ToInt32(dataReader["transactionid"]),
                            transaction_type = Convert.ToInt32(dataReader["transactiontype"]),
                            server_id = Convert.ToInt32(dataReader["serverid"])
                        };
                        tableSchedule.Add(model);
                    }
                }
            }
            return tableSchedule;
        }
    }
}
