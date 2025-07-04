using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using uc = CPReservationApi.Common;

namespace CPReservationApi.DAL
{
    public class TableLayoutDAL : BaseDataAccess
    {
        public TableLayoutDAL(string connectionString) : base(connectionString)
        {
        }

        public SessionModel StartServerSession(List<int> LocationId, int UserId)
        {
            SessionModel sessionModel = new SessionModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@LocationId", string.Join(",",LocationId)));
            parameterList.Add(GetParameter("@UserId", UserId));

            using (DbDataReader dataReader = GetDataReader("StartServerSession", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {

                    while (dataReader.Read())
                    {
                        sessionModel.id = -1;
                        sessionModel.color = Convert.ToString(dataReader["Error"]);
                    }

                    if (string.IsNullOrEmpty(sessionModel.color))
                    {
                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                sessionModel.id = Convert.ToInt32(dataReader["id"]);
                                sessionModel.color = Convert.ToString(dataReader["Color"]);
                                sessionModel.user_id = Convert.ToInt32(dataReader["User_Id"]);
                                sessionModel.session_datetime = Convert.ToDateTime(dataReader["SessionDateTime"]);
                                sessionModel.first_name = Convert.ToString(dataReader["FirstName"]);
                                sessionModel.last_name = Convert.ToString(dataReader["LastName"]);
                            }
                        }
                    }
                }
            }
            return sessionModel;
        }

        public SessionModel StartServerSessionV2(List<int> FloorPlanId, int UserId, DateTime? sessionStartDate=null)
        {
            SessionModel sessionModel = new SessionModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@FloorPlanId", string.Join(",", FloorPlanId)));
            parameterList.Add(GetParameter("@UserId", UserId));
            if(sessionStartDate != null && sessionStartDate.HasValue)
                parameterList.Add(GetParameter("@SessionStartDate", sessionStartDate.Value));

            using (DbDataReader dataReader = GetDataReader("StartServerSessionV2", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {

                    while (dataReader.Read())
                    {
                        sessionModel.id = -1;
                        sessionModel.color = Convert.ToString(dataReader["Error"]);
                    }

                    if (string.IsNullOrEmpty(sessionModel.color))
                    {
                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                sessionModel.id = Convert.ToInt32(dataReader["id"]);
                                sessionModel.color = Convert.ToString(dataReader["Color"]);
                                sessionModel.user_id = Convert.ToInt32(dataReader["User_Id"]);
                                sessionModel.session_datetime = Convert.ToDateTime(dataReader["SessionDateTime"]);
                                sessionModel.first_name = Convert.ToString(dataReader["FirstName"]);
                                sessionModel.last_name = Convert.ToString(dataReader["LastName"]);
                            }
                        }
                    }
                }
            }
            return sessionModel;
        }

        public bool EndServerSession(int Server_Session_Id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ServerSessionId", Server_Session_Id));

            int retvalue = ExecuteNonQuery("EndServerSession", parameterList);

            return retvalue > 0;
        }

        public bool AssignServerTable(int Server_Session_Id, List<string> TableId)
        {
            int retval = 0;
            foreach (var item in TableId)
            {
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));
                parameterList.Add(GetParameter("@TableId", item));
                retval += ExecuteNonQuery("AssignServerTable", parameterList);
            }
            return retval > 0;
        }

        public bool UnAssignServerTable(int Server_Session_Id, List<string> TableId)
        {
            int retval = 0;
            foreach (var item in TableId)
            {
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));
                parameterList.Add(GetParameter("@TableId", item));
                retval += ExecuteNonQuery("UnAssignServerTable", parameterList);
            }
            return retval > 0;
        }

        public bool AssignServerSection(int Server_Session_Id, List<string> ServerSection)
        {
            int retval = 0;
            foreach (var item in ServerSection)
            {
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));
                parameterList.Add(GetParameter("@ServerSection", item));
                retval += ExecuteNonQuery("AssignServerSection", parameterList);
            }
            return retval > 0;
        }

        public bool UnAssignServerSection(int Server_Session_Id, List<string> ServerSection)
        {
            int retval = 0;
            foreach (var item in ServerSection)
            {
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));
                parameterList.Add(GetParameter("@ServerSection", item));
                retval += ExecuteNonQuery("UnAssignServerSection", parameterList);
            }
            return retval > 0;
        }

        public List<SessionModel> GetActivesessions(int LocationID, int OffsetMinutes)
        {
            var model = new List<SessionModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@LocationID", LocationID));
            parameterList.Add(GetParameter("@offsettime", OffsetMinutes));

            using (DbDataReader dataReader = GetDataReader("GetActivesessions", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        SessionModel sessionModel = new SessionModel();
                        sessionModel.id = Convert.ToInt32(dataReader["id"]);
                        sessionModel.color = Convert.ToString(dataReader["Color"]);
                        sessionModel.user_id = Convert.ToInt32(dataReader["User_Id"]);
                        sessionModel.session_datetime = Convert.ToDateTime(dataReader["SessionDateTime"]);
                        sessionModel.first_name = Convert.ToString(dataReader["FirstName"]);
                        sessionModel.last_name = Convert.ToString(dataReader["LastName"]);
                        model.Add(sessionModel);
                    }
                }
            }
            return model;
        }

        public List<SessionModel> GetActivesessionsV2(int FloorPlanID, int OffsetMinutes, DateTime? sessionDate)
        {
            var model = new List<SessionModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@FloorPlanID", FloorPlanID));
            parameterList.Add(GetParameter("@offsettime", OffsetMinutes));
            if (sessionDate != null && sessionDate.HasValue)
                parameterList.Add(GetParameter("@SessionDateTime", sessionDate.Value));

            using (DbDataReader dataReader = GetDataReader("GetActivesessionsV2", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        SessionModel sessionModel = new SessionModel();
                        sessionModel.id = Convert.ToInt32(dataReader["id"]);
                        sessionModel.color = Convert.ToString(dataReader["Color"]);
                        sessionModel.user_id = Convert.ToInt32(dataReader["User_Id"]);
                        sessionModel.session_datetime = Convert.ToDateTime(dataReader["SessionDateTime"]);
                        sessionModel.first_name = Convert.ToString(dataReader["FirstName"]);
                        sessionModel.last_name = Convert.ToString(dataReader["LastName"]);
                        model.Add(sessionModel);
                    }
                }
            }
            return model;
        }

        public List<Server_TableModel> GetActiveServer_Table(int Server_Session_Id)
        {
            var model = new List<Server_TableModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));

            using (DbDataReader dataReader = GetDataReader("GetActiveServer_Table", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Server_TableModel server_TableModel = new Server_TableModel();
                        server_TableModel.id = Convert.ToInt32(dataReader["Id"]);
                        server_TableModel.table_id = Convert.ToString(dataReader["TableId"]);

                        model.Add(server_TableModel);
                    }
                }
            }
            return model;
        }

        public List<ServerTablebySessionIdsModel> GetActiveServerTablebySessionIds(int[] server_session_ids, bool active_only)
        {
            string strserver_session_ids = string.Join(",", server_session_ids);

            List<ServerTablebySessionIdsModel> list = new List<ServerTablebySessionIdsModel>();
            //List<Server_TableModel> servertablelist = new List<Server_TableModel>();
            //List<Server_SectionModel> serversectionlist = new List<Server_SectionModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Server_Session_Ids", strserver_session_ids));
            parameterList.Add(GetParameter("@ActiveOnly", active_only));

            int OldServer_Session_Id = 0;
            var serverTablebySessionIdsModel = new ServerTablebySessionIdsModel();

            using (DbDataReader dataReader = GetDataReader("GetActiveServerTablebySessionIds", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int Server_Session_Id = Convert.ToInt32(dataReader["Server_Session_Id"]);

                        if (Server_Session_Id == 0 || OldServer_Session_Id != Server_Session_Id)
                        {
                            OldServer_Session_Id = Server_Session_Id;
                            serverTablebySessionIdsModel = new ServerTablebySessionIdsModel();

                            serverTablebySessionIdsModel.server_session_id = Server_Session_Id;

                            serverTablebySessionIdsModel.server_table = new List<Server_TableModel>();
                            serverTablebySessionIdsModel.server_section = new List<Server_SectionModel>();
                            list.Add(serverTablebySessionIdsModel);
                        }

                        Server_TableModel server_TableModel = new Server_TableModel();
                        Server_SectionModel server_SectionModel = new Server_SectionModel();

                        if (Convert.ToInt32(dataReader["servertype"]) == 1)
                        {
                            server_TableModel.id = Convert.ToInt32(dataReader["Id"]);
                            server_TableModel.table_id = Convert.ToString(dataReader["TableId"]);
                            server_TableModel.status = Convert.ToInt32(dataReader["status"]);
                            serverTablebySessionIdsModel.server_table.Add(server_TableModel);
                        }
                        else
                        {
                            server_SectionModel.id = Convert.ToInt32(dataReader["Id"]);
                            server_SectionModel.server_section = Convert.ToString(dataReader["TableId"]);
                            server_SectionModel.status = Convert.ToInt32(dataReader["status"]);
                            serverTablebySessionIdsModel.server_section.Add(server_SectionModel);
                        }
                    }
                }
            }
            return list;
        }

        //public List<ServerSession_TableModel> GetActiveTablesByServerSessionIds(int[] server_session_ids)
        //{
        //    string strserver_session_ids = string.Join(",", server_session_ids);
        //    var model = new List<ServerSession_TableModel>();
        //    List<Server_TableModel> tablelist = new List<Server_TableModel>();

        //    var parameterList = new List<DbParameter>();
        //    parameterList.Add(GetParameter("@ServerSessionIds", strserver_session_ids));

        //    using (DbDataReader dataReader = GetDataReader("GetActiveTablesByServerSessionIds", parameterList))
        //    {
        //        int Oldserver_session_id = 0;
        //        var serverSession_TableModel = new ServerSession_TableModel();
        //        if (dataReader != null && dataReader.HasRows)
        //        {
        //            while (dataReader.Read())
        //            {
        //                int server_session_id = Convert.ToInt32(dataReader["Server_Session_Id"]);

        //                if (server_session_id == 0 || Oldserver_session_id != server_session_id)
        //                {
        //                    Oldserver_session_id = server_session_id;
        //                    serverSession_TableModel = new ServerSession_TableModel();

        //                    serverSession_TableModel.server_session_id = server_session_id;

        //                    serverSession_TableModel.tables = new List<Server_TableModel>();
        //                    model.Add(serverSession_TableModel);
        //                }
        //                Server_TableModel tbl = new Server_TableModel();

        //                tbl.id = Convert.ToInt32(dataReader["id"]);
        //                tbl.table_id = Convert.ToString(dataReader["table_id"]);

        //                serverSession_TableModel.tables.Add(tbl);
        //            }
        //        }
        //    }
        //    return model;
        //}
        public List<Server_SectionModel> GetActiveServer_Section(int Server_Session_Id)
        {
            var model = new List<Server_SectionModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));

            using (DbDataReader dataReader = GetDataReader("GetActiveServer_Section", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Server_SectionModel server_SectionModel = new Server_SectionModel();
                        server_SectionModel.id = Convert.ToInt32(dataReader["Id"]);
                        server_SectionModel.server_section = Convert.ToString(dataReader["ServerSection"]);
                        model.Add(server_SectionModel);
                    }
                }
            }
            return model;
        }

        public void CheckEndServerSession(int LocationID, int OffsetMinutes)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@LocationID", LocationID));
            parameterList.Add(GetParameter("@offsettime", OffsetMinutes));

            int retvalue = ExecuteNonQuery("CheckEndServerSession", parameterList);
        }

        public List<int> GetServerLocationByServerSessionId(int Server_Session_Id)
        {
            var model = new List<int>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));

            using (DbDataReader dataReader = GetDataReader("GetServerLocationByServerSessionId", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["locationid"]);

                        model.Add(id);
                    }
                }
            }
            return model;
        }

        public List<FloorPlanLocation> GetServerFloorPlavLocationByServerSessionId(int Server_Session_Id)
        {
            var model = new List<FloorPlanLocation>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));

            using (DbDataReader dataReader = GetDataReader("GetServerFloorplanLocationByServerSessionId", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int locationid = Convert.ToInt32(dataReader["locationid"]);
                        int floorplanId = Convert.ToInt32(dataReader["FloorplanId"]);

                        model.Add(new FloorPlanLocation {
                            floor_plan_id = floorplanId,
                            location_id = locationid
                        });
                    }
                }
            }
            return model;
        }

        public List<int> GetServerFloorPlanByServerSessionId(int Server_Session_Id)
        {
            var model = new List<int>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));

            using (DbDataReader dataReader = GetDataReader("GetServerFloorPlanByServerSessionId", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["FloorPlanId"]);

                        model.Add(id);
                    }
                }
            }
            return model;
        }

        public List<int> GetFloorPlansByTableIds(string tableIds)
        {
            var model = new List<int>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableIds", tableIds));

            using (DbDataReader dataReader = GetDataReader("GetFloorplansByTableIds", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["FloorPlanId"]);

                        model.Add(id);
                    }
                }
            }
            return model;
        }

        //public List<int> GetServerFloorPlanByServerSessionId(int Server_Session_Id)
        //{
        //    var model = new List<int>();

        //    var parameterList = new List<DbParameter>();
        //    parameterList.Add(GetParameter("@Server_Session_Id", Server_Session_Id));

        //    using (DbDataReader dataReader = GetDataReader("GetServerLocationByServerSessionId", parameterList))
        //    {
        //        if (dataReader != null && dataReader.HasRows)
        //        {
        //            while (dataReader.Read())
        //            {
        //                int id = Convert.ToInt32(dataReader["locationid"]);

        //                model.Add(id);
        //            }
        //        }
        //    }
        //    return model;
        //}
        public int ClearSessionByTableId(int TableId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableId", TableId));

            ExecuteNonQuery("ClearSessionByTableId", parameterList);

            return TableId;
        }

        public List<SessionModel> StartServerSessions(List<int> FloorPlanId, List<int> UserId, string sessionStartDate)
        {
            var Model = new List<SessionModel>();          
            var parameterList = new List<DbParameter>();
            int id = -1;
            string color = "";
            parameterList.Add(GetParameter("@FloorPlanId", string.Join(",", FloorPlanId)));
            parameterList.Add(GetParameter("@UserIds", string.Join(",", UserId)));
            if (!string.IsNullOrEmpty(sessionStartDate))
                parameterList.Add(GetParameter("@SessionStartDate", Convert.ToDateTime(sessionStartDate)));

            using (DbDataReader dataReader = GetDataReader("StartServerSessions", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        id = -1;
                        color = Convert.ToString(dataReader["Error"]);
                    }

                    if (string.IsNullOrEmpty(color))
                    {
                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                SessionModel sessionModel = new SessionModel();
                                sessionModel.id = Convert.ToInt32(dataReader["id"]);
                                sessionModel.color = Convert.ToString(dataReader["Color"]);
                                sessionModel.user_id = Convert.ToInt32(dataReader["User_Id"]);
                                sessionModel.session_datetime = Convert.ToDateTime(dataReader["SessionDateTime"]);
                                sessionModel.first_name = Convert.ToString(dataReader["FirstName"]);
                                sessionModel.last_name = Convert.ToString(dataReader["LastName"]);
                                Model.Add(sessionModel);
                            }
                        }
                    }
                }
            }
            return Model;
        }
    }
}
