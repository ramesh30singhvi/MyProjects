using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace CPReservationApi.DAL
{
    public class ReportDAL : BaseDataAccess
    {
        public ReportDAL(string connectionString) : base(connectionString)
        {
        }

        #region Shift Report

        public List<ShiftReport> GetShiftReport(DateTime day, int member_Id, string strlocation_ids)
        {
            var locationList = new List<ShiftReport>();
            if (!string.IsNullOrEmpty(strlocation_ids))
            {
                string[] locations = strlocation_ids.Split(',');
                if (locations.Length > 0)
                {
                    string sql = "select Id,TechnicalName,SeatingResetTime from Location where Id=@Id";
                    var parameterList = new List<DbParameter>();
                    parameterList.Add(GetParameter("@Id", locations[0]));
                    using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                    {
                        if (dataReader != null && dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                int defaultlocation_id = Convert.ToInt32(dataReader["Id"]);
                                locationList = GetShiftReportUsers(day, member_Id, strlocation_ids, Convert.ToString(dataReader["SeatingResetTime"]), defaultlocation_id);
                            }
                        }
                    }
                }
            }

            return locationList;
        }

        public List<ShiftReport> GetShiftReportV2(DateTime day, int member_Id, string strfloor_plan_ids)
        {
            var locationList = new List<ShiftReport>();
            if (!string.IsNullOrEmpty(strfloor_plan_ids))
            {
                string[] floorplans = strfloor_plan_ids.Split(',');
                if (floorplans.Length > 0)
                {
                    string sql = "select l.Id,fp.SeatingResetTime from Location l join floor_plan fp on l.Id = fp.locationId where fp.Id=@Id";
                    var parameterList = new List<DbParameter>();
                    parameterList.Add(GetParameter("@Id", floorplans[0]));
                    using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                    {
                        if (dataReader != null && dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                int defaultlocation_id = Convert.ToInt32(dataReader["Id"]);
                                locationList = GetShiftReportUsersV2(day, member_Id, strfloor_plan_ids, Convert.ToString(dataReader["SeatingResetTime"]), defaultlocation_id);
                            }
                        }
                    }
                }
            }

            return locationList;
        }

        public List<ShiftReport> GetShiftReportUsers(DateTime day, int member_Id, string strlocation_ids, string SeatingResetTime, int defaultlocation_id)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            LocationModel locationModel = eventDAL.GetLocationByID(defaultlocation_id);
            double offset = locationModel.location_timezone_offset;
            var modelList = new List<ShiftReport>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@MemberId", member_Id));
            parameterList.Add(GetParameter("@LocationId", strlocation_ids));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            using (DbDataReader dataReader = GetDataReader("GetShiftReport", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ShiftReport();
                        model.user_id = Convert.ToInt32(dataReader["Id"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.color = Convert.ToString(dataReader["Color"]);
                        //model.total_guests = Convert.ToInt32(dataReader["total_guests"]);
                        model.summary = GetShiftReportHoursSummary(day, member_Id, strlocation_ids, model.user_id, SeatingResetTime, offset);
                        modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public List<ShiftReport> GetShiftReportUsersV2(DateTime day, int member_Id, string strfloor_plan_ids, string SeatingResetTime, int defaultlocation_id)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            LocationModel locationModel = eventDAL.GetLocationByID(defaultlocation_id);
            double offset = locationModel.location_timezone_offset;
            var modelList = new List<ShiftReport>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@MemberId", member_Id));
            parameterList.Add(GetParameter("@FloorPlanId", strfloor_plan_ids));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            using (DbDataReader dataReader = GetDataReader("GetShiftReportV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ShiftReport();
                        model.user_id = Convert.ToInt32(dataReader["Id"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.color = Convert.ToString(dataReader["Color"]);
                        model.summary = GetShiftReportHoursSummaryV2(day, member_Id, strfloor_plan_ids, model.user_id, SeatingResetTime, offset);
                        modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public List<ShiftReportSummary> GetShiftReportHoursSummary(DateTime day, int member_Id, string location_id, int userId, string SeatingResetTime, double offset)
        {

            int[] hours = new int[24]; int[] guests = new int[24]; int i = 0;
            var modelList = new List<ShiftReportSummary>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@MemberId", member_Id));
            parameterList.Add(GetParameter("@LocationId", location_id));
            parameterList.Add(GetParameter("@UserId", userId));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            using (DbDataReader dataReader = GetDataReader("GetShiftReport24HoursSummary", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        hours[i] = Convert.ToInt32(dataReader["hour"]);
                        guests[i] = Convert.ToInt32(dataReader["total_guests"]);
                        i++;
                    }
                }
                modelList = SetShiftReport24HoursValue(hours, guests, SeatingResetTime, day, offset);
            }
            return modelList;
        }

        public List<ShiftReportSummary> GetShiftReportHoursSummaryV2(DateTime day, int member_Id, string strfloor_plan_ids, int userId, string SeatingResetTime, double offset)
        {

            int[] hours = new int[24]; int[] guests = new int[24]; int i = 0;
            var modelList = new List<ShiftReportSummary>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@MemberId", member_Id));
            parameterList.Add(GetParameter("@FloorPlanId", strfloor_plan_ids));
            parameterList.Add(GetParameter("@UserId", userId));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            using (DbDataReader dataReader = GetDataReader("GetShiftReport24HoursSummaryV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        hours[i] = Convert.ToInt32(dataReader["hour"]);
                        guests[i] = Convert.ToInt32(dataReader["total_guests"]);
                        i++;
                    }
                }
                modelList = SetShiftReport24HoursValue(hours, guests, SeatingResetTime, day, offset);
            }
            return modelList;
        }

        public List<ShiftReportSummary> SetShiftReport24HoursValue(int[] hours, int[] guests, string SeatingResetTime, DateTime day, double offset)
        {
            int currentHours = 0; bool isCurrentDate = false;
            if (DateTime.UtcNow.AddMinutes(offset).Date == day.Date)
            {
                currentHours = DateTime.UtcNow.AddMinutes(offset).Hour;
                isCurrentDate = true;
            }
            int startTime = 4;
            if (!string.IsNullOrEmpty(SeatingResetTime))
            {
                string[] str = SeatingResetTime.Split(':');
                startTime = Convert.ToInt32(str[0]);
            }
            int K = 0, j = 0, count = 0;
            var modelList = new List<ShiftReportSummary>();
            for (int i = startTime; i < 24 + startTime; i++)
            {
                if (i < 24)
                {
                    if (isCurrentDate && currentHours == i)
                        count = -1;
                }
                else
                {
                    j = i - 24;
                    if (isCurrentDate && currentHours == j)
                        count = -1;
                }
                var model = new ShiftReportSummary();
                int index = 0;

                if (i < 12)
                    model.title = Convert.ToString(i + " AM");
                else
                {
                    if (i < 24)
                    {
                        if (i == 12)
                            model.title = Convert.ToString(i + " PM");
                        else
                        {
                            K = i - 12;
                            model.title = Convert.ToString(K + " PM");
                        }
                    }
                    else
                    {
                        if (i == 24)
                        {
                            model.title = Convert.ToString(12 + " AM");
                        }
                        else
                        {
                            K = i - 24;
                            model.title = Convert.ToString(K + " AM");
                        }

                    }
                }
                if (i > 0)
                {
                    if (i < 24)
                    {
                        index = Array.IndexOf(hours, i);
                        index = hours.ToList().FindIndex(x => x == i);
                    }
                    else
                    {
                        j = i - 24;
                        if (j > 0)
                        {
                            index = Array.IndexOf(hours, j);
                            //index = hours.ToList().FindIndex(x => x == j);
                            if (index < 20)
                                index = -1;
                        }
                        else
                            index = -1;
                    }
                }
                if (index > -1)
                {
                    model.guest_count = Convert.ToInt32(guests[index]);
                }
                else
                {
                    model.guest_count = count;
                }
                modelList.Add(model);
            }

            return modelList;
        }

        #endregion

        #region Cover Report

        public List<CoversReportLocations> GetCoversReport(DateTime day, int member_Id, string strlocation_ids)
        {
            TimeSpan resetTime = new TimeSpan(0, 0, 0);
            var locationList = new List<CoversReportLocations>();
            if (!string.IsNullOrEmpty(strlocation_ids))
            {
                string[] locations = strlocation_ids.Split(',');
                if (locations.Length > 0)
                {
                    foreach (var item in locations)
                    {
                        string sql = "select Id,LocationName,SeatingResetTime from Location where Id=@Id";
                        var parameterList = new List<DbParameter>();
                        parameterList.Add(GetParameter("@Id", item));
                        using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                        {
                            if (dataReader != null && dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    var model = new CoversReportLocations();
                                    model.location_id = Convert.ToInt32(dataReader["Id"]);
                                    model.location_name = Convert.ToString(dataReader["LocationName"]);
                                    if (!string.IsNullOrEmpty(Convert.ToString(dataReader["SeatingResetTime"])))
                                    {
                                        resetTime = TimeSpan.Parse(Convert.ToString(dataReader["SeatingResetTime"]));
                                    }
                                    model.seating_reset_time = Convert.ToDouble(resetTime.TotalHours);
                                    model.tables = GetCoverReportTables(day, member_Id, model.location_id, resetTime);
                                    locationList.Add(model);
                                }
                            }
                        }
                    }
                }
            }

            return locationList;
        }



        public List<CoversReportLocationsV2> GetCoversReportV2(DateTime day, int member_Id, string strfloorplan_ids)
        {
            var locationList = new List<CoversReportLocationsV2>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@FloorPlanIds", strfloorplan_ids));
            parameterList.Add(GetParameter("@MemberId", member_Id));
            using (DbDataReader dataReader = GetDataReader("GetSeatingReport", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    int prevFloorPlanId = 0;
                    int currFloorPlanId = 0;
                    int prevTableId = 0;
                    int currTableId = 0;
                    CoversReportLocationsV2 model = new CoversReportLocationsV2();
                    List<CoversReport> tableList = new List<CoversReport>();
                    CoversReport tableData = new CoversReport();
                    TimeSpan resetTime = new TimeSpan(0, 0, 0);
                    int offset = 0;
                    while (dataReader.Read())
                    {
                        currFloorPlanId = Convert.ToInt32(dataReader["FloorplanId"]);
                        offset = Convert.ToInt32(dataReader["OffsetMinutes"]);
                        if (prevFloorPlanId != currFloorPlanId)
                        {
                            if (prevFloorPlanId > 0)
                            {
                                model.tables = tableList;
                                locationList.Add(model);
                            }
                            model = new CoversReportLocationsV2();
                            model.floor_plan_id = currFloorPlanId;
                            model.floor_plan_name = Convert.ToString(dataReader["FloorplanName"]);
                            if (!string.IsNullOrEmpty(Convert.ToString(dataReader["SeatingResetTime"])))
                            {
                                resetTime = TimeSpan.Parse(Convert.ToString(dataReader["SeatingResetTime"]));
                            }
                            model.seating_reset_time = Convert.ToDouble(resetTime.TotalHours);
                            tableList = new List<CoversReport>();
                            prevFloorPlanId = currFloorPlanId;
                        }
                        currTableId = Convert.ToInt32(dataReader["TableID"]);
                        if (prevTableId != currTableId)
                        {
                            if (prevTableId > 0)
                            {
                                tableList.Add(tableData);
                            }
                            tableData = new CoversReport();
                            tableData.table_id = currTableId;
                            tableData.table_name = Convert.ToString(dataReader["TableName"]);
                            tableData.rsvps = new List<ReportModel>();
                            tableData.waitlists = new List<ReportModel>();
                            tableData.pre_assign_rsvps = new List<ReportModel>();
                            tableData.pre_assign_waitlists = new List<ReportModel>();
                            prevTableId = currTableId;
                        }
                        int dataType = Convert.ToInt32(dataReader["DataType"]);
                        int reservationId = Convert.ToInt32(dataReader["ReservationId"]);
                        int waitListId = Convert.ToInt32(dataReader["Waitlist_Id"]);
                        int waitListIndex = Convert.ToInt32(dataReader["waitlist_index"]);
                        int numberSeated = Convert.ToInt32(dataReader["NumberSeated"]);
                        int partySize = Convert.ToInt32(dataReader["TotalGuests"]);
                        int serverId = Convert.ToInt32(dataReader["userid"]);
                        string colorCode = Convert.ToString(dataReader["Color"]);
                        string firstName = Convert.ToString(dataReader["FirstName"]);
                        string lastName = Convert.ToString(dataReader["LastName"]);
                        double startTime = 0, endTime = 0, rsvpEndTime = 0, waitlistEndTime = 0, rsvpStartTime = 0;
                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["StartTime"])))
                            startTime = Convert.ToDouble(dataReader["StartTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EndTime"])))
                            endTime = Convert.ToDouble(dataReader["EndTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["rsvpEndTime"])))
                            rsvpEndTime = Convert.ToDouble(dataReader["rsvpEndTime"]);
                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["rsvpStartTime"])))
                            rsvpStartTime = Convert.ToDouble(dataReader["rsvpStartTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["waitlistEndTime"])))
                            waitlistEndTime = Convert.ToDouble(dataReader["waitlistEndTime"]);
                        ReportModel data = new ReportModel();
                        if (serverId > 0)
                            data.server_id = serverId;

                        data.guest_first_name = Convert.ToString(dataReader["FirstName"]);
                        data.guest_last_name = Convert.ToString(dataReader["LastName"]);
                        data.index = waitListIndex;
                        if (reservationId > 0)
                            data.id = reservationId;
                        else
                            data.id = waitListId;
                        data.server_color = colorCode;
                        data.party_size = partySize;
                        data.number_seated = numberSeated;

                        switch (dataType)
                        {
                            case 1:
                                startTime = startTime - resetTime.TotalHours;
                                endTime = endTime - resetTime.TotalHours;
                                rsvpEndTime = rsvpEndTime - resetTime.TotalHours;
                                waitlistEndTime = waitlistEndTime - resetTime.TotalHours;
                                data.start_time = Math.Round(startTime, 2);
                                double currentTime = Math.Round(((DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute + offset) / 60.00,2);
                                currentTime = Math.Round(currentTime,2) - resetTime.TotalHours;

                                if (currentTime <= 0)
                                    currentTime = Math.Round(currentTime,2) + 24;

                                if (endTime <= 0 || endTime < startTime)
                                {
                                    endTime = currentTime;
                                    if (reservationId > 0)
                                    {
                                        if ((rsvpEndTime - endTime) < 0)
                                            data.end_time = Math.Round(endTime, 2);
                                        else if (rsvpEndTime < startTime)
                                            data.end_time = Math.Round(endTime, 2);
                                        else
                                            data.end_time = Math.Round(rsvpEndTime, 2);

                                        //if (rsvpEndTime > startTime)
                                        //{
                                        //    model.end_time = Math.Round(rsvpEndTime, 2);
                                        //}
                                    }
                                    else if (waitListId > 0)
                                    {
                                        if ((waitlistEndTime - endTime) < 0)
                                            data.end_time = Math.Round(endTime, 2);
                                        else if (waitlistEndTime < startTime)
                                            data.end_time = Math.Round(endTime, 2);
                                        else
                                            data.end_time = Math.Round(waitlistEndTime, 2);
                                        //if ((endTime - startTime) < 1)
                                        //    model.end_time = startTime + 1;
                                        //else if ((endTime - startTime) > 1)
                                        //    model.end_time = Math.Round(endTime, 2);

                                        //if ((endTime - startTime) > 12)
                                        //    model.end_time = startTime + 12;
                                        //else if ((endTime - startTime) < 1)
                                        //    model.end_time = startTime + 1;
                                        //else
                                        //    model.end_time = Math.Round(endTime, 2);
                                    }
                                }
                                else
                                    data.end_time = Math.Round(endTime, 2);
                                if (startTime > 0 && data.id > 0)
                                {
                                    if (reservationId > 0)
                                        tableData.rsvps.Add(data);
                                    else
                                        tableData.waitlists.Add(data);
                                }

                                break;
                            case 2:
                                rsvpStartTime = rsvpStartTime - resetTime.TotalHours;
                                rsvpEndTime = rsvpEndTime - resetTime.TotalHours;

                                data.start_time = Math.Round(rsvpStartTime, 2);
                                data.end_time = Math.Round(rsvpEndTime, 2);
                                if (data.start_time > 0)
                                {
                                    tableData.pre_assign_rsvps.Add(data);
                                }
                                    break;
                            case 3:
                                startTime = startTime - resetTime.TotalHours;
                                endTime = endTime - resetTime.TotalHours;
                                data.start_time = Math.Round(startTime, 2);
                                if (endTime <= 0)
                                {
                                    endTime = ((DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute) / 60;
                                    endTime = endTime - resetTime.TotalHours;
                                    if ((endTime - startTime) > 12)
                                        data.end_time = startTime + 12;
                                    else if ((endTime - startTime) < 1)
                                        data.end_time = startTime + 1;
                                    else
                                        data.end_time = Math.Round(endTime, 2);
                                }
                                else
                                    data.end_time = Math.Round(endTime, 2);
                                if (startTime > 0)
                                {
                                    tableData.pre_assign_waitlists.Add(data);
                                }
                                break;
                        }

                    }
                    tableList.Add(tableData);
                    model.tables = tableList;
                    locationList.Add(model);

                    var floorplansExist = locationList.Select(t => t.floor_plan_id.ToString()).Distinct().ToList();
                    var currenList = strfloorplan_ids.Split(',').ToList();

                    var notExists = currenList.Except(floorplansExist).ToList();

                    if (notExists != null && notExists.Count > 0)
                    {
                        string sql = "select fp.SeatingResetTime,fp.Id floor_plan_id,PlanName from floor_plan fp where fp.Id in (" + string.Join(",", notExists) + ")";
                        parameterList = new List<DbParameter>();

                        using (DbDataReader floordataReader = GetDataReader(sql, parameterList, CommandType.Text))
                        {
                            if (floordataReader != null && floordataReader.HasRows)
                            {
                                while (floordataReader.Read())
                                {
                                    var floorPlanData = new CoversReportLocationsV2();
                                    floorPlanData.floor_plan_id = Convert.ToInt32(floordataReader["floor_plan_id"]);
                                    floorPlanData.floor_plan_name = Convert.ToString(floordataReader["PlanName"]);
                                    if (!string.IsNullOrEmpty(Convert.ToString(floordataReader["SeatingResetTime"])))
                                    {
                                        resetTime = TimeSpan.Parse(Convert.ToString(floordataReader["SeatingResetTime"]));
                                    }
                                    floorPlanData.seating_reset_time = Convert.ToDouble(resetTime.TotalHours);
                                   
                                    locationList.Add(floorPlanData);
                                }
                            }
                        }
                    }
                    
                    locationList = locationList.OrderBy(s => s.floor_plan_id).ToList();


                }
            }

            return locationList;
        }

        public List<CoversReportLocationsV3> GetCoversReportV3(DateTime day, int member_Id, string strfloorplan_ids)
        {
            var locationList = new List<CoversReportLocationsV3>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@FloorPlanIds", strfloorplan_ids));
            parameterList.Add(GetParameter("@MemberId", member_Id));
            using (DbDataReader dataReader = GetDataReader("GetSeatingReportV3", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    int prevFloorPlanId = 0;
                    int currFloorPlanId = 0;
                    int prevTableId = 0;
                    int currTableId = 0;
                    CoversReportLocationsV3 model = new CoversReportLocationsV3();
                    List<CoversReportV3> tableList = new List<CoversReportV3>();
                    CoversReportV3 tableData = new CoversReportV3();
                    TimeSpan resetTime = new TimeSpan(0, 0, 0);
                    int offset = 0;
                    while (dataReader.Read())
                    {
                        currFloorPlanId = Convert.ToInt32(dataReader["FloorplanId"]);
                        offset = Convert.ToInt32(dataReader["OffsetMinutes"]);
                        if (prevFloorPlanId != currFloorPlanId)
                        {
                            if (prevFloorPlanId > 0)
                            {
                                model.tables = tableList;
                                locationList.Add(model);
                            }
                            model = new CoversReportLocationsV3();
                            model.floor_plan_id = currFloorPlanId;
                            model.floor_plan_name = Convert.ToString(dataReader["FloorplanName"]);
                            if (!string.IsNullOrEmpty(Convert.ToString(dataReader["SeatingResetTime"])))
                            {
                                resetTime = TimeSpan.Parse(Convert.ToString(dataReader["SeatingResetTime"]));
                            }
                            model.seating_reset_time = Convert.ToDouble(resetTime.TotalHours);
                            tableList = new List<CoversReportV3>();
                            prevFloorPlanId = currFloorPlanId;
                        }
                        currTableId = Convert.ToInt32(dataReader["TableID"]);
                        if (prevTableId != currTableId)
                        {
                            if (prevTableId > 0)
                            {
                                tableList.Add(tableData);
                            }
                            tableData = new CoversReportV3();
                            tableData.table_id = currTableId;
                            tableData.table_name = Convert.ToString(dataReader["TableName"]);
                            tableData.rsvps = new List<ReportModelV3>();
                            tableData.waitlists = new List<ReportModelV3>();
                            tableData.pre_assign_rsvps = new List<ReportModelV3>();
                            tableData.pre_assign_waitlists = new List<ReportModelV3>();
                            prevTableId = currTableId;
                        }
                        int dataType = Convert.ToInt32(dataReader["DataType"]);
                        int reservationId = Convert.ToInt32(dataReader["ReservationId"]);
                        int waitListId = Convert.ToInt32(dataReader["Waitlist_Id"]);
                        int waitListIndex = Convert.ToInt32(dataReader["waitlist_index"]);
                        int numberSeated = Convert.ToInt32(dataReader["NumberSeated"]);
                        int partySize = Convert.ToInt32(dataReader["TotalGuests"]);
                        int serverId = Convert.ToInt32(dataReader["userid"]);
                        string colorCode = Convert.ToString(dataReader["Color"]);
                        string firstName = Convert.ToString(dataReader["FirstName"]);
                        string lastName = Convert.ToString(dataReader["LastName"]);
                        bool IsWalkIn = Convert.ToBoolean(dataReader["IsWalkIn"]);
                        DateTime startTime = Convert.ToDateTime("1/1/1900"), endTime = Convert.ToDateTime("1/1/1900"), rsvpEndTime = Convert.ToDateTime("1/1/1900"), waitlistEndTime = Convert.ToDateTime("1/1/1900"), rsvpStartTime = Convert.ToDateTime("1/1/1900");

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["StartTime"])))
                            startTime = Convert.ToDateTime(dataReader["StartTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EndTime"])))
                            endTime = Convert.ToDateTime(dataReader["EndTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["rsvpEndTime"])))
                            rsvpEndTime = Convert.ToDateTime(dataReader["rsvpEndTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["rsvpStartTime"])))
                            rsvpStartTime = Convert.ToDateTime(dataReader["rsvpStartTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["waitlistEndTime"])))
                            waitlistEndTime = Convert.ToDateTime(dataReader["waitlistEndTime"]);

                        ReportModelV3 data = new ReportModelV3();
                        if (serverId > 0)
                            data.server_id = serverId;

                        data.guest_first_name = Convert.ToString(dataReader["FirstName"]);
                        data.guest_last_name = Convert.ToString(dataReader["LastName"]);
                        data.index = waitListIndex;
                        if (reservationId > 0)
                            data.id = reservationId;
                        else
                            data.id = waitListId;
                        data.server_color = colorCode;
                        data.party_size = partySize;
                        data.number_seated = numberSeated;
                        data.is_walk_in = IsWalkIn;

                        switch (dataType)
                        {
                            case 1:
                                //startTime = startTime - resetTime.TotalHours;
                                //endTime = endTime - resetTime.TotalHours;
                                //rsvpEndTime = rsvpEndTime - resetTime.TotalHours;
                                //waitlistEndTime = waitlistEndTime - resetTime.TotalHours;
                                data.start_time = startTime;
                                //double currentTime = ((DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute + offset) / 60;
                                //currentTime = currentTime - resetTime.TotalHours;

                                if (endTime.Year == 1900 || endTime < startTime)
                                {
                                    endTime = DateTime.UtcNow;
                                    if (reservationId > 0)
                                    {
                                        if (rsvpEndTime < endTime)
                                            data.end_time = endTime;
                                        else if (rsvpEndTime < startTime)
                                            data.end_time = endTime;
                                        else
                                            data.end_time = rsvpEndTime;
                                    }
                                    else if (waitListId > 0)
                                    {
                                        if (waitlistEndTime < endTime)
                                            data.end_time = endTime;
                                        else if (waitlistEndTime < startTime)
                                            data.end_time = endTime;
                                        else
                                            data.end_time = waitlistEndTime;
                                    }
                                }
                                else
                                    data.end_time = endTime;

                                if (startTime.Year > 1900 && data.id > 0)
                                {
                                    if (reservationId > 0)
                                        tableData.rsvps.Add(data);
                                    else
                                        tableData.waitlists.Add(data);
                                }

                                break;
                            case 2:
                                //rsvpStartTime = rsvpStartTime - resetTime.TotalHours;
                                //rsvpEndTime = rsvpEndTime - resetTime.TotalHours;

                                //data.start_time = Math.Round(rsvpStartTime, 2);
                                //data.end_time = Math.Round(rsvpEndTime, 2);

                                data.start_time = rsvpStartTime;
                                data.end_time = rsvpEndTime;
                                if (data.start_time.Year > 1900)
                                {
                                    tableData.pre_assign_rsvps.Add(data);
                                }
                                break;
                            case 3:
                                //startTime = startTime - resetTime.TotalHours;
                                //endTime = endTime - resetTime.TotalHours;
                                data.start_time = startTime;
                                if (endTime.Year == 1900)
                                {
                                    endTime = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour).AddMinutes(DateTime.UtcNow.Minute);
                                    endTime = endTime.AddHours(-resetTime.TotalHours);
                                    if ((endTime.Hour - startTime.Hour) > 12)
                                        data.end_time = startTime.AddHours(12);
                                    else if ((endTime.Hour - startTime.Hour) < 1)
                                        data.end_time = startTime.AddHours(1);
                                    else
                                        data.end_time = endTime;
                                }
                                else
                                    data.end_time = endTime;
                                if (startTime.Year > 1900)
                                {
                                    tableData.pre_assign_waitlists.Add(data);
                                }
                                break;
                        }

                    }
                    tableList.Add(tableData);
                    model.tables = tableList;
                    locationList.Add(model);

                    var floorplansExist = locationList.Select(t => t.floor_plan_id.ToString()).Distinct().ToList();
                    var currenList = strfloorplan_ids.Split(',').ToList();

                    var notExists = currenList.Except(floorplansExist).ToList();

                    if (notExists != null && notExists.Count > 0)
                    {
                        string sql = "select fp.SeatingResetTime,fp.Id floor_plan_id,PlanName from floor_plan fp where fp.Id in (" + string.Join(",", notExists) + ")";
                        parameterList = new List<DbParameter>();

                        using (DbDataReader floordataReader = GetDataReader(sql, parameterList, CommandType.Text))
                        {
                            if (floordataReader != null && floordataReader.HasRows)
                            {
                                while (floordataReader.Read())
                                {
                                    var floorPlanData = new CoversReportLocationsV3();
                                    floorPlanData.floor_plan_id = Convert.ToInt32(floordataReader["floor_plan_id"]);
                                    floorPlanData.floor_plan_name = Convert.ToString(floordataReader["PlanName"]);
                                    if (!string.IsNullOrEmpty(Convert.ToString(floordataReader["SeatingResetTime"])))
                                    {
                                        resetTime = TimeSpan.Parse(Convert.ToString(floordataReader["SeatingResetTime"]));
                                    }
                                    floorPlanData.seating_reset_time = Convert.ToDouble(resetTime.TotalHours);

                                    locationList.Add(floorPlanData);
                                }
                            }
                        }
                    }

                    locationList = locationList.OrderBy(s => s.floor_plan_id).ToList();
                }
            }

            return locationList;
        }

        public List<CoversReportLocationsV2> GetCoversReportV2Old(DateTime day, int member_Id, string strfloorplan_ids)
        {
            TimeSpan resetTime = new TimeSpan(0, 0, 0);
            var locationList = new List<CoversReportLocationsV2>();
            if (!string.IsNullOrEmpty(strfloorplan_ids))
            {
                string[] floorplans = strfloorplan_ids.Split(',');
                if (floorplans.Length > 0)
                {
                    foreach (var item in floorplans)
                    {
                        string sql = "select LocationID Id,fp.SeatingResetTime,fp.Id floor_plan_id,PlanName from floor_plan fp where fp.Id=@Id";
                        var parameterList = new List<DbParameter>();
                        parameterList.Add(GetParameter("@Id", item));
                        using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                        {
                            if (dataReader != null && dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    var model = new CoversReportLocationsV2();
                                    model.floor_plan_id = Convert.ToInt32(dataReader["floor_plan_id"]);
                                    model.floor_plan_name = Convert.ToString(dataReader["PlanName"]);
                                    if (!string.IsNullOrEmpty(Convert.ToString(dataReader["SeatingResetTime"])))
                                    {
                                        resetTime = TimeSpan.Parse(Convert.ToString(dataReader["SeatingResetTime"]));
                                    }
                                    model.seating_reset_time = Convert.ToDouble(resetTime.TotalHours);
                                    model.tables = GetCoverReportTablesV2(day, member_Id, Convert.ToInt32(dataReader["Id"]), resetTime, Convert.ToInt32(item));
                                    locationList.Add(model);
                                }
                            }
                        }
                    }
                }
            }

            return locationList;
        }


        public List<CoversReport> GetCoverReportTables(DateTime day, int member_Id, int location_Id, TimeSpan SeatingResetTime)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            LocationModel locationModel = eventDAL.GetLocationByID(location_Id);
            double offset = locationModel.location_timezone_offset;
            var modelList = new List<CoversReport>();
            var parameterList = new List<DbParameter>();
            int startingHours = SeatingResetTime.Hours;
            int hourRange = 24 + startingHours;
            //string sql = "select distinct TableID,TableName from Table_Layout t";
            //sql += " join Seating_Session ss on t.TableID = ss.Table_Id";
            //sql += " where (DATEADD(MINUTE, @OffsetMinutes, ss.SessionDateTime)) between dateadd(HOUR, @StartingHours, @SessionDateTime) and dateadd(HOUR, @HourRange, @SessionDateTime)";
            //sql += " and t.LocationId in (@LocationId)";
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@LocationId", location_Id));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            parameterList.Add(GetParameter("@StartingHours", startingHours));
            parameterList.Add(GetParameter("@HourRange", hourRange));
            using (DbDataReader dataReader = GetDataReader("GetCoverReportTables", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new CoversReport();
                        model.table_id = Convert.ToInt32(dataReader["TableID"]);
                        model.table_name = Convert.ToString(dataReader["TableName"]);
                        model.rsvps = GetCoversReportList(day, location_Id, (int)Common.Common.TransactionCategory.rsvp, model.table_id, offset, SeatingResetTime);
                        model.waitlists = GetCoversReportList(day, location_Id, (int)Common.Common.TransactionCategory.waitlists, model.table_id, offset, SeatingResetTime);
                        model.pre_assign_rsvps = GetPreAssignRsvpCoversReportList(day, location_Id, model.table_id, offset, SeatingResetTime);
                        model.pre_assign_waitlists = GetPreAssignWaitlistCoversReportList(day, location_Id, model.table_id, offset, SeatingResetTime);
                        modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public List<CoversReport> GetCoverReportTablesV2(DateTime day, int member_Id, int location_Id, TimeSpan SeatingResetTime,int floorplan_Id)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            LocationModel locationModel = eventDAL.GetLocationByID(location_Id);
            double offset = locationModel.location_timezone_offset;
            var modelList = new List<CoversReport>();
            var parameterList = new List<DbParameter>();
            int startingHours = SeatingResetTime.Hours;
            int hourRange = 24 + startingHours;
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@FloorPlanId", floorplan_Id));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            parameterList.Add(GetParameter("@StartingHours", startingHours));
            parameterList.Add(GetParameter("@HourRange", hourRange));
            using (DbDataReader dataReader = GetDataReader("GetCoverReportTablesV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new CoversReport();
                        model.table_id = Convert.ToInt32(dataReader["TableID"]);
                        model.table_name = Convert.ToString(dataReader["TableName"]);
                        model.rsvps = GetCoversReportListV2(day, floorplan_Id, (int)Common.Common.TransactionCategory.rsvp, model.table_id, offset, SeatingResetTime);
                        model.waitlists = GetCoversReportListV2(day, floorplan_Id, (int)Common.Common.TransactionCategory.waitlists, model.table_id, offset, SeatingResetTime);
                        model.pre_assign_rsvps = GetPreAssignRsvpCoversReportListV2(day, floorplan_Id, model.table_id, offset, SeatingResetTime);
                        model.pre_assign_waitlists = GetPreAssignWaitlistCoversReportListV2(day, floorplan_Id, model.table_id, offset, SeatingResetTime);
                        modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public List<ReportModel> GetCoversReportList(DateTime day, int location_Id, int TransactionCategoryType, int TableID, double offset, TimeSpan SeatingResetTime)
        {
            double startTime = 0, endTime = 0, rsvpEndTime = 0;
            var modelList = new List<ReportModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@LocationId", location_Id));
            parameterList.Add(GetParameter("@TransactionCategory", TransactionCategoryType));
            parameterList.Add(GetParameter("@TableID", TableID));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            parameterList.Add(GetParameter("@SeatingResetTime", SeatingResetTime.Hours));
            using (DbDataReader dataReader = GetDataReader("GetCoversReport", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ReportModel();
                        if (Convert.ToInt32(dataReader["User_Id"]) > 0)
                            model.server_id = Convert.ToInt32(dataReader["User_Id"]);

                        if (TransactionCategoryType == 2)
                            model.id = Convert.ToInt32(dataReader["Reservation_Id"]);
                        else
                            model.id = Convert.ToInt32(dataReader["Waitlist_Id"]);

                        model.guest_first_name = Convert.ToString(dataReader["FirstName"]);
                        model.guest_last_name = Convert.ToString(dataReader["LastName"]);
                        model.index = Convert.ToInt32(dataReader["waitlist_index"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["StartTime"])))
                            startTime = Convert.ToDouble(dataReader["StartTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EndTime"])))
                            endTime = Convert.ToDouble(dataReader["EndTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["rsvpEndTime"])))
                            rsvpEndTime = Convert.ToDouble(dataReader["rsvpEndTime"]);

                        startTime = startTime - SeatingResetTime.TotalHours;
                        endTime = endTime - SeatingResetTime.TotalHours;
                        rsvpEndTime = rsvpEndTime - SeatingResetTime.TotalHours;
                        model.start_time = Math.Round(startTime, 2);
                        if (endTime <= 0)
                        {
                            endTime = ((DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute + offset) / 60;
                            endTime = endTime  - SeatingResetTime.TotalHours;
                            if ((Common.Common.TransactionCategory)TransactionCategoryType == Common.Common.TransactionCategory.rsvp)
                            {
                                if ((rsvpEndTime - endTime) < 0)
                                    model.end_time = Math.Round(endTime, 2);
                                else
                                    model.end_time = Math.Round(rsvpEndTime, 2);

                                //if (rsvpEndTime > startTime)
                                //{
                                //    model.end_time = Math.Round(rsvpEndTime, 2);
                                //}
                            }
                            else if ((Common.Common.TransactionCategory)TransactionCategoryType == Common.Common.TransactionCategory.waitlists)
                            {
                                if ((endTime - startTime) < 1)
                                    model.end_time = startTime + 1;
                                else if ((endTime - startTime) > 1)
                                    model.end_time = Math.Round(endTime, 2);

                                //if ((endTime - startTime) > 12)
                                //    model.end_time = startTime + 12;
                                //else if ((endTime - startTime) < 1)
                                //    model.end_time = startTime + 1;
                                //else
                                //    model.end_time = Math.Round(endTime, 2);
                            }
                        }
                        else
                            model.end_time = Math.Round(endTime, 2);
                        model.server_color = Convert.ToString(dataReader["Color"]);
                        model.party_size = Convert.ToInt32(dataReader["PartySize"]);
                        if (startTime > 0)
                            modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public List<ReportModel> GetCoversReportListV2(DateTime day, int floorPlan_Id, int TransactionCategoryType, int TableID, double offset, TimeSpan SeatingResetTime)
        {
            var modelList = new List<ReportModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@FloorPlanId", floorPlan_Id));
            parameterList.Add(GetParameter("@TransactionCategory", TransactionCategoryType));
            parameterList.Add(GetParameter("@TableID", TableID));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            parameterList.Add(GetParameter("@SeatingResetTime", SeatingResetTime.Hours));
            using (DbDataReader dataReader = GetDataReader("GetCoversReportV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        double startTime = 0, endTime = 0, rsvpEndTime = 0, waitlistEndTime=0;

                        var model = new ReportModel();
                        if (Convert.ToInt32(dataReader["User_Id"]) > 0)
                            model.server_id = Convert.ToInt32(dataReader["User_Id"]);

                        if (TransactionCategoryType == 2)
                            model.id = Convert.ToInt32(dataReader["Reservation_Id"]);
                        else
                            model.id = Convert.ToInt32(dataReader["Waitlist_Id"]);

                        model.guest_first_name = Convert.ToString(dataReader["FirstName"]);
                        model.guest_last_name = Convert.ToString(dataReader["LastName"]);
                        model.index = Convert.ToInt32(dataReader["waitlist_index"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["StartTime"])))
                            startTime = Convert.ToDouble(dataReader["StartTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EndTime"])))
                            endTime = Convert.ToDouble(dataReader["EndTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["rsvpEndTime"])))
                            rsvpEndTime = Convert.ToDouble(dataReader["rsvpEndTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["waitlistEndTime"])))
                            waitlistEndTime = Convert.ToDouble(dataReader["waitlistEndTime"]);

                        startTime = startTime - SeatingResetTime.TotalHours;
                        endTime = endTime - SeatingResetTime.TotalHours;
                        rsvpEndTime = rsvpEndTime - SeatingResetTime.TotalHours;
                        waitlistEndTime = waitlistEndTime - SeatingResetTime.TotalHours;
                        model.start_time = Math.Round(startTime, 2);
                        double currentTime = ((DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute + offset) / 60;
                        currentTime = currentTime - SeatingResetTime.TotalHours;
                        if (endTime <= 0 || endTime < startTime)
                        {
                            endTime = currentTime;
                            if ((Common.Common.TransactionCategory)TransactionCategoryType == Common.Common.TransactionCategory.rsvp)
                            {
                                if ((rsvpEndTime - endTime) < 0)
                                    model.end_time = Math.Round(endTime, 2);
                                else
                                    model.end_time = Math.Round(rsvpEndTime, 2);

                                //if (rsvpEndTime > startTime)
                                //{
                                //    model.end_time = Math.Round(rsvpEndTime, 2);
                                //}
                            }
                            else if ((Common.Common.TransactionCategory)TransactionCategoryType == Common.Common.TransactionCategory.waitlists)
                            {
                                if ((waitlistEndTime - endTime) < 0)
                                    model.end_time = Math.Round(endTime, 2);
                                else
                                    model.end_time = Math.Round(waitlistEndTime, 2);
                                //if ((endTime - startTime) < 1)
                                //    model.end_time = startTime + 1;
                                //else if ((endTime - startTime) > 1)
                                //    model.end_time = Math.Round(endTime, 2);

                                //if ((endTime - startTime) > 12)
                                //    model.end_time = startTime + 12;
                                //else if ((endTime - startTime) < 1)
                                //    model.end_time = startTime + 1;
                                //else
                                //    model.end_time = Math.Round(endTime, 2);
                            }
                        }
                        else
                            model.end_time = Math.Round(endTime, 2);
                        model.server_color = Convert.ToString(dataReader["Color"]);
                        model.party_size = Convert.ToInt32(dataReader["PartySize"]);
                        model.number_seated = Convert.ToInt32(dataReader["NumberSeated"]);
                        if (startTime > 0 && model.id > 0)
                            modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public List<ReportModel> GetPreAssignWaitlistCoversReportList(DateTime day, int location_Id, int TableID, double offset, TimeSpan SeatingResetTime)
        {
            double startTime = 0, endTime = 0;
            var modelList = new List<ReportModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@LocationId", location_Id));
            parameterList.Add(GetParameter("@TableID", TableID));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            parameterList.Add(GetParameter("@SeatingResetTime", SeatingResetTime.Hours));
            using (DbDataReader dataReader = GetDataReader("GetPreAssignWaitlistCoversReportList", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ReportModel();
                        model.server_id = Convert.ToInt32(dataReader["User_Id"]);
                        model.id = Convert.ToInt32(dataReader["Waitlist_Id"]);
                        model.guest_first_name = Convert.ToString(dataReader["FirstName"]);
                        model.guest_last_name = Convert.ToString(dataReader["LastName"]);
                        model.index = Convert.ToInt32(dataReader["waitlist_index"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["StartTime"])))
                            startTime = Convert.ToDouble(dataReader["StartTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EndTime"])))
                            endTime = Convert.ToDouble(dataReader["EndTime"]);

                        startTime = startTime - SeatingResetTime.TotalHours;
                        endTime = endTime - SeatingResetTime.TotalHours;
                        model.start_time = Math.Round(startTime, 2);
                        if (endTime <= 0)
                        {
                            endTime = ((DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute) / 60;
                            endTime = endTime - SeatingResetTime.TotalHours;
                            if ((endTime - startTime) > 12)
                                model.end_time = startTime + 12;
                            else if ((endTime - startTime) < 1)
                                model.end_time = startTime + 1;
                            else
                                model.end_time = Math.Round(endTime, 2);
                        }
                        else
                            model.end_time = Math.Round(endTime, 2);
                        model.server_color = Convert.ToString(dataReader["Color"]);
                        model.party_size = Convert.ToInt32(dataReader["PartySize"]);
                        if (startTime > 0)
                            modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public List<ReportModel> GetPreAssignWaitlistCoversReportListV2(DateTime day, int FloorPlanId, int TableID, double offset, TimeSpan SeatingResetTime)
        {
            double startTime = 0, endTime = 0;
            var modelList = new List<ReportModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@FloorPlanId", FloorPlanId));
            parameterList.Add(GetParameter("@TableID", TableID));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            parameterList.Add(GetParameter("@SeatingResetTime", SeatingResetTime.Hours));
            using (DbDataReader dataReader = GetDataReader("GetPreAssignWaitlistCoversReportListV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ReportModel();
                        model.server_id = Convert.ToInt32(dataReader["User_Id"]);
                        model.id = Convert.ToInt32(dataReader["Waitlist_Id"]);
                        model.guest_first_name = Convert.ToString(dataReader["FirstName"]);
                        model.guest_last_name = Convert.ToString(dataReader["LastName"]);
                        model.index = Convert.ToInt32(dataReader["waitlist_index"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["StartTime"])))
                            startTime = Convert.ToDouble(dataReader["StartTime"]);

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EndTime"])))
                            endTime = Convert.ToDouble(dataReader["EndTime"]);

                        startTime = startTime - SeatingResetTime.TotalHours;
                        endTime = endTime - SeatingResetTime.TotalHours;
                        model.start_time = Math.Round(startTime, 2);
                        if (endTime <= 0)
                        {
                            endTime = ((DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute) / 60;
                            endTime = endTime - SeatingResetTime.TotalHours;
                            if ((endTime - startTime) > 12)
                                model.end_time = startTime + 12;
                            else if ((endTime - startTime) < 1)
                                model.end_time = startTime + 1;
                            else
                                model.end_time = Math.Round(endTime, 2);
                        }
                        else
                            model.end_time = Math.Round(endTime, 2);
                        model.server_color = Convert.ToString(dataReader["Color"]);
                        model.party_size = Convert.ToInt32(dataReader["PartySize"]);
                        model.number_seated = Convert.ToInt32(dataReader["NumberSeated"]);

                        if (startTime > 0 && model.id > 0)
                            modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public List<ReportModel> GetPreAssignRsvpCoversReportList(DateTime day, int location_Id, int TableID, double offset, TimeSpan SeatingResetTime)
        {
            double rsvpStartTime = 0, rsvpEndTime = 0;
            var modelList = new List<ReportModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@LocationId", location_Id));
            parameterList.Add(GetParameter("@TableID", TableID));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            parameterList.Add(GetParameter("@SeatingResetTime", SeatingResetTime.Hours));
            using (DbDataReader dataReader = GetDataReader("GetPreAssignRsvpCoversReportList", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ReportModel();

                        model.server_id = Convert.ToInt32(dataReader["userid"]);
                        model.id = Convert.ToInt32(dataReader["ReservationId"]);

                        model.guest_first_name = Convert.ToString(dataReader["FirstName"]);
                        model.guest_last_name = Convert.ToString(dataReader["LastName"]);
                        model.index = 0;

                        rsvpStartTime = Convert.ToDouble(dataReader["rsvpStartTime"]);
                        rsvpEndTime = Convert.ToDouble(dataReader["rsvpEndTime"]);

                        rsvpStartTime = rsvpStartTime - SeatingResetTime.TotalHours;
                        rsvpEndTime = rsvpEndTime - SeatingResetTime.TotalHours;

                        model.start_time = Math.Round(rsvpStartTime, 2);
                        model.end_time = Math.Round(rsvpEndTime, 2);

                        model.server_color = Convert.ToString(dataReader["Color"]);
                        model.party_size = Convert.ToInt32(dataReader["TotalGuests"]);
                        if (model.start_time > 0)
                            modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public List<ReportModel> GetPreAssignRsvpCoversReportListV2(DateTime day, int FloorPlan_Id, int TableID, double offset, TimeSpan SeatingResetTime)
        {
            double rsvpStartTime = 0, rsvpEndTime = 0;
            var modelList = new List<ReportModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", day));
            parameterList.Add(GetParameter("@FloorPlanId", FloorPlan_Id));
            parameterList.Add(GetParameter("@TableID", TableID));
            parameterList.Add(GetParameter("@OffsetMinutes", offset));
            parameterList.Add(GetParameter("@SeatingResetTime", SeatingResetTime.Hours));
            using (DbDataReader dataReader = GetDataReader("GetPreAssignRsvpCoversReportListV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ReportModel();

                        model.server_id = Convert.ToInt32(dataReader["userid"]);
                        model.id = Convert.ToInt32(dataReader["ReservationId"]);

                        model.guest_first_name = Convert.ToString(dataReader["FirstName"]);
                        model.guest_last_name = Convert.ToString(dataReader["LastName"]);
                        model.index = 0;

                        rsvpStartTime = Convert.ToDouble(dataReader["rsvpStartTime"]);
                        rsvpEndTime = Convert.ToDouble(dataReader["rsvpEndTime"]);

                        rsvpStartTime = rsvpStartTime - SeatingResetTime.TotalHours;
                        rsvpEndTime = rsvpEndTime - SeatingResetTime.TotalHours;

                        model.start_time = Math.Round(rsvpStartTime, 2);
                        model.end_time = Math.Round(rsvpEndTime, 2);

                        model.server_color = Convert.ToString(dataReader["Color"]);
                        model.party_size = Convert.ToInt32(dataReader["TotalGuests"]);
                        model.number_seated = Convert.ToInt32(dataReader["NumberSeated"]);
                        if (model.start_time >= 0 && model.id > 0)
                            modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        public DailyReport GetDailyReport(DateTime day, int member_Id, string strlocation_ids)
        {
            var dailyReport = new DailyReport();
            var eventsModel = new List<EventsModel>();
            var guest_requests = new List<GuestRequestsModel>();
            var special_events = new List<SpecialEventsModel>();
            var special_guests = new List<SpecialGuestsModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@StartDate", day));
            parameterList.Add(GetParameter("@WineryId", member_Id));
            parameterList.Add(GetParameter("@LocationIds", strlocation_ids));
            using (DbDataReader dataReader = GetDataReader("GetDailyReport", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        dailyReport.total_guest_count = Convert.ToInt32(dataReader["TotalGuestCount"]);
                        dailyReport.total_party_count = Convert.ToInt32(dataReader["TotalPartyCount"]);
                        dailyReport.total_clubmember_guest_count = Convert.ToInt32(dataReader["ClubMemberGuestCount"]);
                        dailyReport.total_seated_count = Convert.ToInt32(dataReader["TotalSeatedCount"]);
                        dailyReport.total_seated_party_count = Convert.ToInt32(dataReader["TotalSeatedPartyCount"]);
                        dailyReport.total_reservation_guest_count = Convert.ToInt32(dataReader["TotalReservationGuest"]);
                        dailyReport.total_walkin_guest_number = Convert.ToInt32(dataReader["TotalWalkInGuest"]);
                        dailyReport.total_waitlist_guest_count = Convert.ToInt32(dataReader["TotalWaitListGuest"]);
                    }
                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            EventsModel model = new EventsModel();
                            model.id = Convert.ToInt32(dataReader["EventId"]);
                            model.name = Convert.ToString(dataReader["EventName"]);
                            model.guest_count = Convert.ToInt32(dataReader["Get_TotalGuest"]);
                            model.party_count = Convert.ToInt32(dataReader["BookedCount"]);
                            string Name = Convert.ToString(dataReader["Name"]);
                            string FirstName = string.Empty;
                            string LastName = string.Empty;
                            if (Name.Length > 0)
                            {
                                string[] timeArray = Name.Split('~');
                                LastName = Convert.ToString(timeArray[1]);
                                FirstName = Convert.ToString(timeArray[0]);
                            }

                            model.first_arrival_first_name = FirstName;
                            model.first_arrival_last_name = LastName;
                            model.event_start_time = Convert.ToDateTime(dataReader["Start"]);

                            model.customer_type = Convert.ToInt32(dataReader["CustomerType"]);
                            eventsModel.Add(model);
                        }
                    }
                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            GuestRequestsModel model = new GuestRequestsModel();
                            model.id = Convert.ToInt32(dataReader["userid"]);
                            model.reservation_id = Convert.ToInt32(dataReader["reservationid"]);
                            model.event_id = Convert.ToInt32(dataReader["eventid"]);
                            model.event_start_time = Convert.ToDateTime(dataReader["Start"]);
                            model.guest_first_name = Convert.ToString(dataReader["firstname"]);
                            model.guest_last_name = Convert.ToString(dataReader["lastname"]);
                            model.guest_notes = Convert.ToString(dataReader["note"]);
                            guest_requests.Add(model);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            SpecialEventsModel model = new SpecialEventsModel();
                            model.id = Convert.ToInt32(dataReader["userid"]);
                            model.reservation_id = Convert.ToInt32(dataReader["reservationid"]);
                            model.event_id = Convert.ToInt32(dataReader["eventid"]);
                            model.event_start_time = Convert.ToDateTime(dataReader["Start"]);
                            model.guest_first_name = Convert.ToString(dataReader["firstname"]);
                            model.guest_last_name = Convert.ToString(dataReader["lastname"]);
                            //model.guest_notes = Convert.ToString(dataReader["note"]);
                            model.guest_tags = GetTags(member_Id, Convert.ToString(dataReader["tags"]));
                            special_events.Add(model);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            SpecialGuestsModel model = new SpecialGuestsModel();
                            model.id = Convert.ToInt32(dataReader["userid"]);
                            model.reservation_id = Convert.ToInt32(dataReader["reservationid"]);
                            model.event_id = Convert.ToInt32(dataReader["eventid"]);
                            model.event_start_time = Convert.ToDateTime(dataReader["Start"]);
                            model.guest_first_name = Convert.ToString(dataReader["firstname"]);
                            model.guest_last_name = Convert.ToString(dataReader["lastname"]);
                            //model.guest_notes = Convert.ToString(dataReader["note"]);
                            model.guest_tags = GetTags(member_Id, Convert.ToString(dataReader["tags"]));
                            special_guests.Add(model);
                        }
                    }

                }
            }
            dailyReport.events = eventsModel;
            dailyReport.guest_requests = guest_requests;
            dailyReport.special_events = special_events;
            dailyReport.special_guests = special_guests;
            return dailyReport;
        }

        public DailyReport GetDailyReportV2(DateTime day, int member_Id, string strfloor_plan_ids)
        {
            var dailyReport = new DailyReport();
            var eventsModel = new List<EventsModel>();
            var guest_requests = new List<GuestRequestsModel>();
            var special_events = new List<SpecialEventsModel>();
            var special_guests = new List<SpecialGuestsModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@StartDate", day));
            parameterList.Add(GetParameter("@WineryId", member_Id));
            parameterList.Add(GetParameter("@FloorPlanIds", strfloor_plan_ids));
            using (DbDataReader dataReader = GetDataReader("GetDailyReportV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        dailyReport.total_guest_count = Convert.ToInt32(dataReader["TotalGuestCount"]);
                        dailyReport.total_party_count = Convert.ToInt32(dataReader["TotalPartyCount"]);

                    }
                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            EventsModel model = new EventsModel();
                            model.id = Convert.ToInt32(dataReader["EventId"]);
                            model.name = Convert.ToString(dataReader["EventName"]);
                            model.guest_count = Convert.ToInt32(dataReader["Get_TotalGuest"]);
                            model.party_count = Convert.ToInt32(dataReader["BookedCount"]);
                            string Name = Convert.ToString(dataReader["Name"]);
                            string FirstName = string.Empty;
                            string LastName = string.Empty;
                            if (Name.Length > 0)
                            {
                                string[] timeArray = Name.Split('~');
                                LastName = Convert.ToString(timeArray[1]);
                                FirstName = Convert.ToString(timeArray[0]);
                            }

                            model.first_arrival_first_name = FirstName;
                            model.first_arrival_last_name = LastName;
                            model.event_start_time = Convert.ToDateTime(dataReader["Start"]);

                            eventsModel.Add(model);
                        }
                    }
                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            GuestRequestsModel model = new GuestRequestsModel();
                            model.id = Convert.ToInt32(dataReader["userid"]);
                            model.reservation_id = Convert.ToInt32(dataReader["reservationid"]);
                            model.event_id = Convert.ToInt32(dataReader["eventid"]);
                            model.event_start_time = Convert.ToDateTime(dataReader["Start"]);
                            model.guest_first_name = Convert.ToString(dataReader["firstname"]);
                            model.guest_last_name = Convert.ToString(dataReader["lastname"]);
                            model.guest_notes = Convert.ToString(dataReader["note"]);
                            guest_requests.Add(model);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            SpecialEventsModel model = new SpecialEventsModel();
                            model.id = Convert.ToInt32(dataReader["userid"]);
                            model.reservation_id = Convert.ToInt32(dataReader["reservationid"]);
                            model.event_id = Convert.ToInt32(dataReader["eventid"]);
                            model.event_start_time = Convert.ToDateTime(dataReader["Start"]);
                            model.guest_first_name = Convert.ToString(dataReader["firstname"]);
                            model.guest_last_name = Convert.ToString(dataReader["lastname"]);
                            //model.guest_notes = Convert.ToString(dataReader["note"]);
                            model.guest_tags = GetTags(member_Id, Convert.ToString(dataReader["tags"]));
                            special_events.Add(model);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            SpecialGuestsModel model = new SpecialGuestsModel();
                            model.id = Convert.ToInt32(dataReader["userid"]);
                            model.reservation_id = Convert.ToInt32(dataReader["reservationid"]);
                            model.event_id = Convert.ToInt32(dataReader["eventid"]);
                            model.event_start_time = Convert.ToDateTime(dataReader["Start"]);
                            model.guest_first_name = Convert.ToString(dataReader["firstname"]);
                            model.guest_last_name = Convert.ToString(dataReader["lastname"]);
                            //model.guest_notes = Convert.ToString(dataReader["note"]);
                            model.guest_tags = GetTags(member_Id, Convert.ToString(dataReader["tags"]));
                            special_guests.Add(model);
                        }
                    }

                }
            }
            dailyReport.events = eventsModel;
            dailyReport.guest_requests = guest_requests;
            dailyReport.special_events = special_events;
            dailyReport.special_guests = special_guests;
            return dailyReport;
        }

        public List<ExportReservationDetail> ExportReservation(DateTime day, int member_Id)
        {
            var list = new List<ExportReservationDetail>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@StartDate", day));
            parameterList.Add(GetParameter("@WineryId", member_Id));
            using (DbDataReader dataReader = GetDataReader("GetReservationV2ByWineryId_Detail", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ExportReservationDetail();
                        model.guest_name = Convert.ToString(dataReader["GuestName"]);
                        model.phone_number = Convert.ToString(dataReader["PhoneNum"]);
                        model.total_guests = Convert.ToInt32(dataReader["TotalGuests"]);
                        model.fee_per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.account_type = Convert.ToString(dataReader["AccountType"]);
                        model.referred_by = Convert.ToString(dataReader["ReferredBy"]);
                        model.internal_note = Convert.ToString(dataReader["InternalNote"]);
                        model.guest_note = Convert.ToString(dataReader["GuestNote"]);
                        model.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.event_time = Convert.ToString(dataReader["EventTime"]);
                        model.guests = Convert.ToInt32(dataReader["Guests"]);
                        model.sort_order = Convert.ToInt32(dataReader["sortorder"]);
                        model.start = Convert.ToDateTime(dataReader["start"]);
                        model.location_name = Convert.ToString(dataReader["LocationName"]);
                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<ExportReservation> ExportReservationByDay(DateTime day, int member_Id)
        {
            var list = new List<ExportReservation>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@StartDate", day));
            parameterList.Add(GetParameter("@WineryId", member_Id));
            using (DbDataReader dataReader = GetDataReader("GetReservationV2ByWineryId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ExportReservation();

                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.phone_number = Convert.ToString(dataReader["PhoneNum"]);
                        model.total_guests = Convert.ToInt32(dataReader["TotalGuests"]);
                        model.fee_per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.discount = Convert.ToDecimal(dataReader["Discount"]);
                        model.purchase_total = Convert.ToDecimal(dataReader["PurchaseTotal"]);
                        model.account_type = Convert.ToString(dataReader["AccountType"]);
                        model.booking_date = Convert.ToString(dataReader["BookingDate"]);
                        model.referred_by = Convert.ToString(dataReader["ReferredBy"]);
                        model.internal_note = Convert.ToString(dataReader["InternalNote"]);
                        model.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.event_time = Convert.ToString(dataReader["EventTime"]);
                        model.guests = Convert.ToInt32(dataReader["Guests"]);
                        model.sort_order = Convert.ToInt32(dataReader["sortorder"]);
                        model.start = Convert.ToDateTime(dataReader["start"]);
                        model.hdyh = Convert.ToString(dataReader["HDYH"]);
                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<GuestTagsModel> GetTags(int WineryID, string tags)
        {
            var model = new List<GuestTagsModel>();

            string sql = "select Id,member_id,tag,tagtype from tags t join [dbo].[StrSplit](@tags, ',') l on l.SplitValue=t.tag where member_id=@WineryID";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));
            parameterList.Add(GetParameter("@tags", tags));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        GuestTagsModel guestTagsModel = new GuestTagsModel();
                        guestTagsModel.id = Convert.ToInt32(dataReader["Id"]);
                        guestTagsModel.member_id = Convert.ToInt32(dataReader["member_id"]);
                        guestTagsModel.tag = Convert.ToString(dataReader["tag"]);
                        guestTagsModel.tag_type = Convert.ToInt32(dataReader["tagtype"]);
                        model.Add(guestTagsModel);
                    }
                }
            }
            return model;
        }

        #endregion
    }
}
