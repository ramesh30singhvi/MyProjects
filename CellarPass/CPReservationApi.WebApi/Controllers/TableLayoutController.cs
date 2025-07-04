using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.WebApi.ViewModels;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.Services;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Cryptography;
using CPReservationApi.Common;
using Newtonsoft.Json;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/tablelayout")]
    public class TableLayoutController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public TableLayoutController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        /// <summary>
        /// This method will be used to start server session
        /// </summary>
        /// <param name="location_id">Id of Location (Required)</param>
        /// <param name="user_id">Id of User (Required)</param>
        /// <returns></returns>
        [Route("startserversession")]
        [HttpPost]
        public IActionResult StartServerSession([FromBody]StartServerSessionRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var model = new StartServerSessionResponse();

            try
            {
                SessionModel sessionModel = new SessionModel();
                sessionModel = tableLayoutDAL.StartServerSession(req.location_id, req.user_id);

                if (sessionModel.id > 0)
                {
                    model.success = true;
                    model.data = sessionModel;

                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    foreach (var item in req.location_id)
                    {
                        var model11 = new CreateDeltaRequest();
                        model11.item_id = sessionModel.id;
                        model11.item_type = (int)ItemType.Servers;
                        model11.location_id = item;
                        model11.member_id = 0;
                        model11.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model11);
                    }
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = sessionModel.color;
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "StartServerSession:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        [Route("startserversessionv2")]
        [HttpPost]
        public IActionResult StartServerSessionV2([FromBody]StartServerSessionV2Request req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var model = new StartServerSessionResponse();

            try
            {

                if (req.session_start_date.HasValue && req.session_start_date.Value < System.DateTime.UtcNow)
                {
                    model.success = true;
                    model.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    model.error_info.extra_info = "Session start date cannot be in past";
                    return new ObjectResult(model);
                }
                SessionModel sessionModel = new SessionModel();
                sessionModel = tableLayoutDAL.StartServerSessionV2(req.floor_plan_id, req.user_id, req.session_start_date);

                if (sessionModel.id > 0)
                {
                    model.success = true;
                    model.data = sessionModel;

                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    foreach (var item in req.floor_plan_id)
                    {
                        var model11 = new CreateDeltaRequest();
                        model11.item_id = sessionModel.id;
                        model11.item_type = (int)ItemType.Servers;
                        model11.location_id = item; //TODO
                        model11.member_id = 0;
                        model11.action_date = DateTime.UtcNow;
                        model11.floor_plan_id = item;
                        notificationDAL.SaveDelta(model11);
                    }
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = sessionModel.color;
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "StartServerSessionV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        /// <summary>
        /// This method will be used to end server session
        /// </summary>
        /// <param name="server_session_id">Id of Server Session (Required)</param>
        /// <returns></returns>
        [Route("endserversession")]
        [HttpPost]
        public IActionResult EndServerSession([FromBody] EndServerSessionRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var model = new EndServerSessionResponse();

            try
            {
                bool ret = tableLayoutDAL.EndServerSession(req.server_session_id);

                if (ret)
                {
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var location_id = tableLayoutDAL.GetServerLocationByServerSessionId(req.server_session_id);
                    foreach (var item in location_id)
                    {
                        var model11 = new CreateDeltaRequest();
                        model11.item_id = req.server_session_id;
                        model11.item_type = (int)ItemType.Servers;
                        model11.location_id = item;
                        model11.member_id = 0;
                        model11.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model11);
                    }
                    model.success = true;
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = "End Server Session Error!";
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "EndServerSession:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        [Route("endserversessionv2")]
        [HttpPost]
        public IActionResult EndServerSessionV2([FromBody] EndServerSessionRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var model = new EndServerSessionResponse();

            try
            {
                bool ret = tableLayoutDAL.EndServerSession(req.server_session_id);

                if (ret)
                {
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var floorPlanIds = tableLayoutDAL.GetServerFloorPlanByServerSessionId(req.server_session_id);
                    foreach (var item in floorPlanIds)
                    {
                        var model11 = new CreateDeltaRequest();
                        model11.item_id = req.server_session_id;
                        model11.item_type = (int)ItemType.Servers;
                        model11.location_id = 0;
                        model11.member_id = 0;
                        model11.floor_plan_id = item;
                        model11.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model11);
                    }
                    model.success = true;
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = "End Server Session Error!";
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "EndServerSession:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        /// <summary>
        /// This method will be used to assign server session data to server table
        /// </summary>
        /// <param name="server_session_id">Id of Server Session (Required)</param>
        /// <param name="table_id">Id of Table (Required)</param>
        /// <returns></returns>
        [Route("assignservertable")]
        [HttpPost]
        public IActionResult AssignServerTable([FromBody] AssignServerTableRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var model = new AssignServerTableResponse();

            try
            {
                bool ret = tableLayoutDAL.AssignServerTable(req.server_session_id,req.table_id);

                if (ret)
                {
                    model.success = true;
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var location_id = tableLayoutDAL.GetServerLocationByServerSessionId(req.server_session_id);
                    foreach (var item in location_id)
                    {
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = req.server_session_id;
                        model1.item_type = (int)ItemType.Servers;
                        model1.location_id = item;
                        model1.member_id = 0;
                        model1.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model1);
                    }
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = "Assign Server Table Error!";
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AssignServerTable:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        [Route("assignservertablev2")]
        [HttpPost]
        public IActionResult AssignServerTableV2([FromBody] AssignServerTableRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var model = new AssignServerTableResponse();

            try
            {
                bool ret = tableLayoutDAL.AssignServerTable(req.server_session_id, req.table_id);

                if (ret)
                {
                    model.success = true;
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var floorplanIds = tableLayoutDAL.GetServerFloorPlanByServerSessionId(req.server_session_id);
                    foreach (var item in floorplanIds)
                    {
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = req.server_session_id;
                        model1.item_type = (int)ItemType.Servers;
                        model1.location_id = 0;
                        model1.member_id = 0;
                        model1.floor_plan_id = item;
                        model1.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model1);
                    }
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = "Assign Server Table Error!";
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AssignServerTable:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        /// <summary>
        /// This method will be used to unassign server session data to server table
        /// </summary>
        /// <param name="server_session_id">Id of Server Session  (Required)</param>
        /// <param name="table_id">Id of Table  (Required)</param>
        /// <returns></returns>
        [Route("unassignservertable")]
        [HttpPost]
        public IActionResult UnassignServerTable([FromBody] UnassignServerTableRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);
            var model = new UnassignServerTableResponse();
            try
            {
                bool ret = tableLayoutDAL.UnAssignServerTable(req.server_session_id, req.table_id);

                if (ret)
                {
                    model.success = true;
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var location_id = tableLayoutDAL.GetServerLocationByServerSessionId(req.server_session_id);
                    foreach (var item in location_id)
                    {
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = req.server_session_id;
                        model1.item_type = (int)ItemType.Servers;
                        model1.location_id = item;
                        model1.member_id = 0;
                        model1.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model1);
                    }
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = "Unassign Server Table Error!";
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UnassignServerTable:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        [Route("unassignservertablev2")]
        [HttpPost]
        public IActionResult UnassignServerTableV2([FromBody] UnassignServerTableRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);
            var model = new UnassignServerTableResponse();
            try
            {
                bool ret = tableLayoutDAL.UnAssignServerTable(req.server_session_id, req.table_id);

                if (ret)
                {
                    model.success = true;
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var florrplanIds = tableLayoutDAL.GetServerFloorPlanByServerSessionId(req.server_session_id);
                    foreach (var item in florrplanIds)
                    {
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = req.server_session_id;
                        model1.item_type = (int)ItemType.Servers;
                        model1.location_id = 0;
                        model1.member_id = 0;
                        model1.floor_plan_id = item;
                        model1.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model1);
                    }
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = "Unassign Server Table Error!";
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UnassignServerTable:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        /// <summary>
        /// This method will be used to assign server session data to server section
        /// </summary>
        /// <param name="server_session_id">Id of Server Session (Required)</param>
        /// <param name="server_section">Server Section (Required)</param>
        /// <returns></returns>
        [Route("assignserversection")]
        [HttpPost]
        public IActionResult AssignServerSection([FromBody] AssignServerSectionRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var model = new AssignServerSectionResponse();

            try
            {
                bool ret = tableLayoutDAL.AssignServerSection(req.server_session_id, req.server_section);

                if (ret)
                {
                    model.success = true;
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var floorPlanList = tableLayoutDAL.GetServerFloorPlavLocationByServerSessionId(req.server_session_id);
                    foreach (var item in floorPlanList)
                    {
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = req.server_session_id;
                        model1.item_type = (int)ItemType.Servers;
                        model1.floor_plan_id = item.floor_plan_id;
                        model1.location_id = item.location_id;                       
                        model1.member_id = 0;
                        model1.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model1);
                    }
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = "Assign Server Section Error!";
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AssignServerSection:  " + ex.Message.ToString() + ", request:" + JsonConvert.SerializeObject(req), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        /// <summary>
        /// This method will be used to unassign server session data to server section
        /// </summary>
        /// <param name="server_session_id">Id of Server Session (Required)</param>
        /// <param name="server_section">Server Section (Required)</param>
        /// <returns></returns>
        [Route("unassignserversection")]
        [HttpPost]
        public IActionResult UnassignServerSection([FromBody] UnassignServerSectionRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);
            var model = new UnassignServerSectionResponse();
            try
            {
                bool ret = tableLayoutDAL.UnAssignServerSection(req.server_session_id, req.server_section);

                if (ret)
                {
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var floorPlanList = tableLayoutDAL.GetServerFloorPlavLocationByServerSessionId(req.server_session_id);
                    foreach (var item in floorPlanList)
                    {
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = req.server_session_id;
                        model1.item_type = (int)ItemType.Servers;
                        model1.floor_plan_id = item.floor_plan_id;
                        model1.location_id = item.location_id;
                        model1.member_id = 0;
                        model1.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model1);
                    }

                    model.success = true;
                }
                else
                {
                    model.error_info.error_type = (int)Common.Common.ErrorType.None;
                    model.error_info.extra_info = "Unassign Server Section Error!";
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UnassignServerSection:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(model);
        }

        /// <summary>
        /// This method gives list of active session
        /// </summary>
        /// <param name="location_id">Id of Location (Required)</param>
        /// <returns></returns>
        [Route("activesessions")]
        [HttpGet]
        public IActionResult GetActivesessions(int location_id)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var sessionResponse = new SessionResponse();

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                int OffsetMinutes = eventDAL.GetOffsetMinutes(location_id);

                var model = new List<SessionModel>();
                model = tableLayoutDAL.GetActivesessions(location_id, OffsetMinutes);

                if (model != null)
                {
                    sessionResponse.success = true;
                    sessionResponse.data = model;
                }
                else
                {
                    sessionResponse.success = true;
                    sessionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    sessionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                sessionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                sessionResponse.error_info.extra_info = Common.Common.InternalServerError;
                sessionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetActivesessions:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(sessionResponse);
        }

        [Route("activesessionsv2")]
        [HttpGet]
        public IActionResult GetActivesessionsV2(int floorplan_id, DateTime? session_date)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var sessionResponse = new SessionResponse();

            try
            {
                if (session_date.HasValue && session_date.Value < System.DateTime.UtcNow)
                {
                    sessionResponse.success = true;
                    sessionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    sessionResponse.error_info.extra_info = "Session start date cannot be in past";
                    return new ObjectResult(sessionResponse);
                }
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                int OffsetMinutes = eventDAL.GetOffsetMinutesByFloorPlanId(floorplan_id);

                var model = new List<SessionModel>();
                model = tableLayoutDAL.GetActivesessionsV2(floorplan_id, OffsetMinutes, session_date);

                if (model != null)
                {
                    sessionResponse.success = true;
                    sessionResponse.data = model;
                }
                else
                {
                    sessionResponse.success = true;
                    sessionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    sessionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                sessionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                sessionResponse.error_info.extra_info = Common.Common.InternalServerError;
                sessionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetActivesessionsV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(sessionResponse);
        }

        /// <summary>
        /// This method gives list of active server section
        /// </summary>
        /// <param name="server_session_id">Id of Server Session (Required)</param>
        /// <returns></returns>
        [Route("activeserver_sections")]
        [HttpGet]
        public IActionResult GetActiveServer_Section( int server_session_id)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var server_SectionResponse = new Server_SectionResponse();

            try
            {
                var model = new List<Server_SectionModel>();
                model = tableLayoutDAL.GetActiveServer_Section(server_session_id);

                if (model != null)
                {
                    server_SectionResponse.success = true;
                    server_SectionResponse.data = model;
                }
                else
                {
                    server_SectionResponse.success = true;
                    server_SectionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    server_SectionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                server_SectionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                server_SectionResponse.error_info.extra_info = Common.Common.InternalServerError;
                server_SectionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetActiveServer_Section:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(server_SectionResponse);
        }

        /// <summary>
        /// This method gives list of active server table
        /// </summary>
        /// <param name="server_session_id">Id of Server Session (Required)</param>
        /// <returns></returns>
        [Route("activeserver_tables")]
        [HttpGet]
        public IActionResult GetActiveServer_Table( int server_session_id)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var server_TableResponse = new Server_TableResponse();

            try
            {
                var model = new List<Server_TableModel>();
                model = tableLayoutDAL.GetActiveServer_Table(server_session_id);

                if (model != null)
                {
                    server_TableResponse.success = true;
                    server_TableResponse.data = model;
                }
                else
                {
                    server_TableResponse.success = true;
                    server_TableResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    server_TableResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                server_TableResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                server_TableResponse.error_info.extra_info = Common.Common.InternalServerError;
                server_TableResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetActiveServer_Table:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(server_TableResponse);
        }

        [Route("activeserver_tablesandsessions_by_sessionids")]
        [HttpGet]
        public IActionResult GetActiveServerTablebySessionIds(int[] server_session_ids, bool active_only = true)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString_readonly);

            var serverTablebySessionIdsResponse = new ServerTablebySessionIdsResponse();

            try
            {
                var model = new List<ServerTablebySessionIdsModel>();
                model = tableLayoutDAL.GetActiveServerTablebySessionIds(server_session_ids, active_only);

                if (model != null)
                {
                    serverTablebySessionIdsResponse.success = true;
                    serverTablebySessionIdsResponse.data = model;
                }
                else
                {
                    serverTablebySessionIdsResponse.success = true;
                    serverTablebySessionIdsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    serverTablebySessionIdsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                serverTablebySessionIdsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                serverTablebySessionIdsResponse.error_info.extra_info = Common.Common.InternalServerError;
                serverTablebySessionIdsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetActiveServerTablebySessionIds:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(serverTablebySessionIdsResponse);
        }

        /// <summary>
        /// This method checks and end server session
        /// </summary>
        /// <param name="location_id"> Id of Location (Required)</param>
        /// <returns></returns>
        [Route("checkandendserversession")]
        [HttpPost]
        public IActionResult CheckEndServerSession([FromBody]CheckEndServerSessionRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var checkEndServerSessionResponse = new CheckEndServerSessionResponse();

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                int OffsetMinutes = eventDAL.GetOffsetMinutes(req.location_id);

                tableLayoutDAL.CheckEndServerSession(req.location_id, OffsetMinutes);

                checkEndServerSessionResponse.success = true;
            }
            catch (Exception ex)
            {
                checkEndServerSessionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                checkEndServerSessionResponse.error_info.extra_info = Common.Common.InternalServerError;
                checkEndServerSessionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CheckEndServerSession:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(checkEndServerSessionResponse);
        }

        [Route("clearsessionbytableid")]
        [HttpPost]
        public IActionResult ClearSessionByTableId([FromBody]ClearSessionRequest model)
        {

            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);
            var response = new ClearSessionResponse();
            try
            {
                response.data.id = tableLayoutDAL.ClearSessionByTableId(model.table_id);
                response.success = true;
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ClearSessionByTableId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(response);
        }

        [Route("startserversessions")]
        [HttpPost]
        public IActionResult StartServerSessions([FromBody] StartServerSessionsRequest req)
        {
            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);

            var model = new StartServerSessionsResponse();

            try
            {
                var sessionModel = new List<SessionModel>();
                sessionModel = tableLayoutDAL.StartServerSessions(req.floor_plan_id, req.user_ids,req.session_start_date);
                foreach (var serveritem in sessionModel)
                {
                    if (serveritem.id > 0)
                    {
                        model.success = true;
                        model.data = sessionModel;

                        var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                        foreach (var item in req.floor_plan_id)
                        {
                            var model11 = new CreateDeltaRequest();
                            model11.item_id = serveritem.id;
                            model11.item_type = (int)ItemType.Servers;
                            model11.location_id = item; //TODO
                            model11.member_id = 0;
                            model11.action_date = DateTime.UtcNow;
                            model11.floor_plan_id = item;
                            notificationDAL.SaveDelta(model11);
                        }
                    }
                    else
                    {
                        model.error_info.error_type = (int)Common.Common.ErrorType.None;
                        model.error_info.extra_info = serveritem.color;
                    }
                }
            }
            catch (Exception ex)
            {
                model.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                model.error_info.extra_info = Common.Common.InternalServerError;
                model.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "StartServerSessions:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(model);
        }
    }
}