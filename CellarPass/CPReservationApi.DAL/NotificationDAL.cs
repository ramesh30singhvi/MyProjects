using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace CPReservationApi.DAL
{
    public class NotificationDAL : BaseDataAccess
    {
        public NotificationDAL(string connectionString) : base(connectionString)
        {
        }
        public int SaveDelta(CreateDeltaRequest model)
        {
            int responseid = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@ItemType", model.item_type));
            parameterList.Add(GetParameter("@ItemId", model.item_id));
            parameterList.Add(GetParameter("@MemberId", model.member_id));
            parameterList.Add(GetParameter("@LocationId", model.location_id));
            parameterList.Add(GetParameter("@Status", model.status));
            parameterList.Add(GetParameter("@ActionDate", model.action_date));
            parameterList.Add(GetParameter("@FloorPlanId", model.floor_plan_id));

            responseid = Convert.ToInt32(ExecuteScalar("InsertDelta", parameterList));

            return responseid;
        }

        public int OpenDeviceSession(string user_id, string device_id, string locations,DateTime action_date,bool use_live_cert,int AppType)
        {
            int responseid = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@UserId", user_id));
            parameterList.Add(GetParameter("@DeviceId", device_id));
            parameterList.Add(GetParameter("@LocationIds", locations));
            parameterList.Add(GetParameter("@ActionDate", action_date));
            parameterList.Add(GetParameter("@Use_Live_Cert", use_live_cert));
            parameterList.Add(GetParameter("@AppType", AppType));

            responseid = Convert.ToInt32(ExecuteScalar("OpenDeviceSession", parameterList));

            return responseid;
        }

        public int OpenDeviceSessionFloorPlan(string user_id, string device_id, string floorPlans, DateTime action_date, bool use_live_cert, int AppType)
        {
            int responseid = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@UserId", user_id));
            parameterList.Add(GetParameter("@DeviceId", device_id));
            parameterList.Add(GetParameter("@FloorPlanIds", floorPlans));
            parameterList.Add(GetParameter("@ActionDate", action_date));
            parameterList.Add(GetParameter("@Use_Live_Cert", use_live_cert));
            parameterList.Add(GetParameter("@AppType", AppType));

            responseid = Convert.ToInt32(ExecuteScalar("OpenDeviceSessionFloorPlan", parameterList));

            return responseid;
        }
        public int CloseDeviceSession(string device_id)
        {
            int responseid = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@DeviceId", device_id));

            responseid = Convert.ToInt32(ExecuteScalar("CloseDeviceSession", parameterList));

            return responseid;
        }

        public List<DeviceSessionModel> GetOpenDeviceSession()
        {
            var deviceSessionModel = new List<DeviceSessionModel>();
            using (DbDataReader dataReader = GetDataReader("GetOpenDeviceSession", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new DeviceSessionModel
                        {
                            id = Convert.ToInt32(dataReader["Id"]),
                            user_id = Convert.ToString(dataReader["UserId"]),
                            device_id = Convert.ToString(dataReader["DeviceId"]),
                            date_updated = Convert.ToDateTime(dataReader["DateUpdated"]),
                            location_id = GetDeviceSessionLocationById(Convert.ToInt32(dataReader["Id"])),
                            action_date = Convert.ToDateTime(dataReader["ActionDate"]),
                            use_live_cert = Convert.ToBoolean(dataReader["Use_Live_Cert"])
                        };
                        deviceSessionModel.Add(model);
                    }
                }
            }
            return deviceSessionModel;
        }

        public List<DeviceSessionModelV2> GetOpenDeviceSessionV2()
        {
            var deviceSessionModel = new List<DeviceSessionModelV2>();
            using (DbDataReader dataReader = GetDataReader("GetOpenDeviceSession", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new DeviceSessionModelV2
                        {
                            id = Convert.ToInt32(dataReader["Id"]),
                            user_id = Convert.ToString(dataReader["UserId"]),
                            device_id = Convert.ToString(dataReader["DeviceId"]),
                            date_updated = Convert.ToDateTime(dataReader["DateUpdated"]),
                            floor_plan_id = GetDeviceSessionFloorPlanById(Convert.ToInt32(dataReader["Id"])),
                            action_date = Convert.ToDateTime(dataReader["ActionDate"]),
                            use_live_cert = Convert.ToBoolean(dataReader["Use_Live_Cert"])
                        };
                        deviceSessionModel.Add(model);
                    }
                }
            }
            return deviceSessionModel;
        }


        public List<int> GetDeviceSessionLocationById(int DeviceSessionId)
        {
            var LocationId = new List<int>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@DeviceSessionId", DeviceSessionId));
            using (DbDataReader dataReader = GetDataReader("GetDeviceSessionLocationById", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        LocationId.Add(Convert.ToInt32(dataReader["LocationId"]));
                    }
                }
            }
            return LocationId;
        }

        public List<int> GetDeviceSessionFloorPlanById(int DeviceSessionId)
        {
            var floorPlanIds = new List<int>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@DeviceSessionId", DeviceSessionId));
            using (DbDataReader dataReader = GetDataReader("GetDeviceSessionFloorPlanById", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        floorPlanIds.Add(Convert.ToInt32(dataReader["FloorPlanId"]));
                    }
                }
            }
            return floorPlanIds;
        }

        public List<DeltaModel> CheckAndSendAppleNotification()
        {
            var deltaModel = new List<DeltaModel>();
            using (DbDataReader dataReader = GetDataReader("CheckAndSendAppleNotification", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new DeltaModel
                        {
                            location_id = Convert.ToInt32(dataReader["LocationId"]),
                            item_type = Convert.ToInt32(dataReader["ItemType"]),
                            member_id = Convert.ToInt32(dataReader["MemberId"])
                        };
                        deltaModel.Add(model);
                    }
                }
            }
            return deltaModel;
        }

        public bool UpdateDelta(string device_id,int deltaid)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@deviceid", device_id));
            parameterList.Add(GetParameter("@deltaid", deltaid));

            ExecuteScalar("UpdateDelta", parameterList);

            return true;
        }

        public List<AvailableNotificationsModel> GetAvailableNotificationsForDevice()
        {
            var models = new List<AvailableNotificationsModel>();
            string OldDeviceId = "";
            AvailableNotificationsModel availableNotificationsModel = new AvailableNotificationsModel();
            string itemtypes = string.Empty;
            string rsvps = string.Empty;
            string waitlist = string.Empty;
            string chatrsvps = string.Empty;
            string chatwaitlist = string.Empty;
            string locationIds = string.Empty;

            using (DbDataReader dataReader = GetDataReader("GetAvailableNotificationsForDevice", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        string DeviceId = Convert.ToString(dataReader["DeviceId"]);

                        if (OldDeviceId == "" || OldDeviceId != DeviceId)
                        {
                            availableNotificationsModel = new AvailableNotificationsModel();
                            availableNotificationsModel.item_type = string.Empty;
                            itemtypes = string.Empty;
                            rsvps = string.Empty;
                            waitlist = string.Empty;
                            chatrsvps = string.Empty;
                            chatwaitlist = string.Empty;
                            locationIds = string.Empty;
                            availableNotificationsModel.device_id = DeviceId;
                            OldDeviceId = DeviceId;
                            models.Add(availableNotificationsModel);
                        }

                        string types = itemtypes;

                        if (types.Length > 0)
                        {
                            types = types + ",";
                        }

                        if (types.IndexOf(Convert.ToString(dataReader["itemtype"])) == -1)
                        {
                            itemtypes = types + Convert.ToInt32(dataReader["itemtype"]);
                        }

                        string rids = rsvps;

                        if (rids.Length > 0)
                        {
                            rids = rids + ",";
                        }

                        if (Convert.ToInt32(dataReader["rsvpid"]) > 0 && Convert.ToInt32(dataReader["itemtype"]) != 8)
                        {
                            rsvps = rids + Convert.ToInt32(dataReader["rsvpid"]);
                        }

                        string wids = waitlist;

                        if (wids.Length > 0)
                        {
                            wids = wids + ",";
                        }

                        if (Convert.ToInt32(dataReader["waitlistid"]) > 0 && Convert.ToInt32(dataReader["itemtype"]) != 9)
                        {
                            waitlist = wids + Convert.ToInt32(dataReader["waitlistid"]);
                        }

                        string chatrids = chatrsvps;

                        if (chatrids.Length > 0)
                        {
                            chatrids = chatrids + ",";
                        }

                        if (Convert.ToInt32(dataReader["rsvpid"]) > 0 && Convert.ToInt32(dataReader["itemtype"]) == 8)
                        {
                            chatrsvps = chatrids + Convert.ToInt32(dataReader["rsvpid"]);
                        }

                        string chatwids = chatwaitlist;

                        if (chatwids.Length > 0)
                        {
                            chatwids = chatwids + ",";
                        }

                        if (Convert.ToInt32(dataReader["waitlistid"]) > 0 && Convert.ToInt32(dataReader["itemtype"]) == 9)
                        {
                            chatwaitlist = chatwids + Convert.ToInt32(dataReader["waitlistid"]);
                        }

                        if (Convert.ToInt32(dataReader["LocationId"]) > 0)
                        {
                            if (!string.IsNullOrEmpty(locationIds))
                            {
                                locationIds += ",";
                            }
                            locationIds += Convert.ToString(dataReader["LocationId"]);
                        }

                        availableNotificationsModel.item_type = itemtypes;
                        availableNotificationsModel.rsvps = rsvps;
                        availableNotificationsModel.waitlist = waitlist;
                        availableNotificationsModel.use_live_cert = Convert.ToBoolean(dataReader["Use_Live_Cert"]);
                        availableNotificationsModel.max_delta_id = Convert.ToInt32(dataReader["deltaid"]);
                        availableNotificationsModel.app_type = Convert.ToInt32(dataReader["AppType"]);
                        availableNotificationsModel.chat_rsvps = chatrsvps;
                        availableNotificationsModel.chat_waitlists = chatwaitlist;
                        availableNotificationsModel.location_ids = locationIds;
                    }
                }
            }
            return models;
        }

        public List<AvailableNotificationsModel> GetAvailableNotificationsForDeviceV2()
        {
            var models = new List<AvailableNotificationsModel>();
            string OldDeviceId = "";
            AvailableNotificationsModel availableNotificationsModel = new AvailableNotificationsModel();
            string itemtypes = string.Empty;
            string rsvps = string.Empty;
            string waitlist = string.Empty;
            string chatrsvps = string.Empty;
            string chatwaitlist = string.Empty;
            string floorplanIds = string.Empty;

            using (DbDataReader dataReader = GetDataReader("GetAvailableNotificationsForDeviceV2", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        string DeviceId = Convert.ToString(dataReader["DeviceId"]);

                        if (OldDeviceId == "" || OldDeviceId != DeviceId)
                        {
                            availableNotificationsModel = new AvailableNotificationsModel();
                            availableNotificationsModel.item_type = string.Empty;
                            itemtypes = string.Empty;
                            rsvps = string.Empty;
                            waitlist = string.Empty;
                            chatrsvps = string.Empty;
                            chatwaitlist = string.Empty;
                            availableNotificationsModel.device_id = DeviceId;
                            OldDeviceId = DeviceId;
                            floorplanIds = string.Empty;
                            models.Add(availableNotificationsModel);
                        }

                        string types = itemtypes;

                        if (types.Length > 0)
                        {
                            types = types + ",";
                        }

                        if (types.IndexOf(Convert.ToString(dataReader["itemtype"])) == -1)
                        {
                            itemtypes = types + Convert.ToInt32(dataReader["itemtype"]);
                        }

                        string rids = rsvps;

                        if (rids.Length > 0)
                        {
                            rids = rids + ",";
                        }

                        if (Convert.ToInt32(dataReader["rsvpid"]) > 0 && Convert.ToInt32(dataReader["itemtype"]) != 8)
                        {
                            rsvps = rids + Convert.ToInt32(dataReader["rsvpid"]);
                        }

                        string wids = waitlist;

                        if (wids.Length > 0)
                        {
                            wids = wids + ",";
                        }

                        if (Convert.ToInt32(dataReader["waitlistid"]) > 0 && Convert.ToInt32(dataReader["itemtype"]) != 9)
                        {
                            waitlist = wids + Convert.ToInt32(dataReader["waitlistid"]);
                        }

                        string chatrids = chatrsvps;

                        if (chatrids.Length > 0)
                        {
                            chatrids = chatrids + ",";
                        }

                        if (Convert.ToInt32(dataReader["rsvpid"]) > 0 && Convert.ToInt32(dataReader["itemtype"]) == 8)
                        {
                            chatrsvps = chatrids + Convert.ToInt32(dataReader["rsvpid"]);
                        }

                        string chatwids = chatwaitlist;

                        if (chatwids.Length > 0)
                        {
                            chatwids = chatwids + ",";
                        }

                        if (Convert.ToInt32(dataReader["waitlistid"]) > 0 && Convert.ToInt32(dataReader["itemtype"]) == 9)
                        {
                            chatwaitlist = chatwids + Convert.ToInt32(dataReader["waitlistid"]);
                        }

                        if (Convert.ToInt32(dataReader["FloorPlanId"]) > 0)
                        {
                            if (!string.IsNullOrEmpty(floorplanIds))
                            {
                                floorplanIds += ",";
                            }
                            floorplanIds += Convert.ToString(dataReader["FloorPlanId"]);
                        }
                        availableNotificationsModel.item_type = itemtypes;
                        availableNotificationsModel.rsvps = rsvps;
                        availableNotificationsModel.waitlist = waitlist;
                        availableNotificationsModel.use_live_cert = Convert.ToBoolean(dataReader["Use_Live_Cert"]);
                        availableNotificationsModel.max_delta_id = Convert.ToInt32(dataReader["deltaid"]);
                        availableNotificationsModel.app_type = Convert.ToInt32(dataReader["AppType"]);
                        availableNotificationsModel.chat_rsvps = chatrsvps;
                        availableNotificationsModel.chat_waitlists = chatwaitlist;
                        availableNotificationsModel.floor_plan_ids = floorplanIds;
                    }
                }
            }
            return models;
        }
    }
}