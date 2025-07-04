using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using CPReservationApi.WebApi.Services;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/notification")]
    public class NotificationController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;

        public NotificationController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        [Route("getopendevices")]
        [HttpGet]
        public IActionResult GetOpenDevices()
        {
            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            var openDeviceResponse = new OpenDeviceResponse();

            try
            {
                var model = new List<DeviceSessionModel>();
                model = notificationDAL.GetOpenDeviceSession();

                if (model != null)
                {
                    openDeviceResponse.success = true;
                    openDeviceResponse.data = model;
                }
                else
                {
                    openDeviceResponse.success = true;
                    openDeviceResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    openDeviceResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                openDeviceResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                openDeviceResponse.error_info.extra_info = Common.Common.InternalServerError;
                openDeviceResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString_tablepro);

                logDAL.InsertLog("WebApi", "GetOpenDevices:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(openDeviceResponse);
        }

        [Route("getopendevicesv2")]
        [HttpGet]
        public IActionResult GetOpenDevicesV2()
        {
            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            var openDeviceResponse = new OpenDeviceResponsev2();

            try
            {
                var model = new List<DeviceSessionModelV2>();
                model = notificationDAL.GetOpenDeviceSessionV2();

                if (model != null)
                {
                    openDeviceResponse.success = true;
                    openDeviceResponse.data = model;
                }
                else
                {
                    openDeviceResponse.success = true;
                    openDeviceResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    openDeviceResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                openDeviceResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                openDeviceResponse.error_info.extra_info = Common.Common.InternalServerError;
                openDeviceResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString_tablepro);

                logDAL.InsertLog("WebApi", "GetOpenDevices:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(openDeviceResponse);
        }

        [Route("savedelta")]
        [HttpPost]
        public IActionResult SaveDelta([FromBody]CreateDeltaRequest model)
        {

            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            var response = new DeltaResponse();
            try
            {
                response.data.id = notificationDAL.SaveDelta(model);
                response.success = true;
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString_tablepro);

                logDAL.InsertLog("WebApi", "SaveDelta:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(response);
        }

        [Route("opendevicesession")]
        [HttpPost]
        public IActionResult OpenDeviceSession([FromBody]OpenDeviceSessionRequest model)
        {
            string strlocations = string.Join(",", model.locations);
            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            var response = new OpenDeviceSessionResponse();
            try
            {
                int AppType = 0;

                if (HttpContext.Request.Headers["AuthenticateKey"] == "Guest Link App User")
                {
                    AppType = 1;
                }

                response.data.id = notificationDAL.OpenDeviceSession(model.user_id, model.device_id, strlocations, Convert.ToDateTime(model.action_date), model.use_live_cert,AppType);
                response.success = true;
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString_tablepro);

                logDAL.InsertLog("WebApi", "OpenDeviceSession:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(response);
        }
        [Route("opendevicesessionv2")]
        [HttpPost]
        public IActionResult OpenDeviceSessionv2([FromBody]OpenDeviceSessionRequestv2 model)
        {
            string floorplanIds = string.Join(",", model.floor_plans);
            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            var response = new OpenDeviceSessionResponse();
            try
            {
                int AppType = 0;

                if (HttpContext.Request.Headers["AuthenticateKey"] == "Guest Link App User")
                {
                    AppType = 1;
                }

                response.data.id = notificationDAL.OpenDeviceSessionFloorPlan(model.user_id, model.device_id, floorplanIds,Convert.ToDateTime(model.action_date), model.use_live_cert, AppType);
                response.success = true;
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString_tablepro);

                logDAL.InsertLog("WebApi", "opendevicesessionv2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(response);
        }

        [Route("closedevicesession")]
        [HttpPost]
        public IActionResult CloseDeviceSession([FromBody]CloseDeviceSessionRequest model)
        {
            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            var response = new CloseDeviceSessionResponse();
            try
            {
                response.data.id = notificationDAL.CloseDeviceSession(model.device_id);
                response.success = true;
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString_tablepro);

                logDAL.InsertLog("WebApi", "CloseDeviceSession:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(response);
        }

        [Route("getavailablenotificationsfordevice")]
        [HttpGet]
        public IActionResult GetAvailableNotificationsForDevice()
        {
            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            var response = new AvailableNotificationsForDeviceResponse();

            try
            {
                var model = new List<AvailableNotificationsModel>();
                model = notificationDAL.GetAvailableNotificationsForDevice();

                if (model != null)
                {
                    response.success = true;
                    response.data = model;
                }
                else
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString_tablepro);

                logDAL.InsertLog("WebApi", "GetAvailableNotificationsForDevice:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(response);
        }

        [Route("getavailablenotificationsfordevicev2")]
        [HttpGet]
        public IActionResult GetAvailableNotificationsForDeviceV2()
        {
            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            var response = new AvailableNotificationsForDeviceResponse();

            try
            {
                var model = new List<AvailableNotificationsModel>();
                model = notificationDAL.GetAvailableNotificationsForDeviceV2();

                if (model != null)
                {
                    response.success = true;
                    response.data = model;
                }
                else
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString_tablepro);

                logDAL.InsertLog("WebApi", "GetAvailableNotificationsForDeviceV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(response);
        }

        [Route("updatedelta")]
        [HttpPost]
        public IActionResult UpdateDelta([FromBody]UpdateDeltaRequest model)
        {
            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString_tablepro);
            var response = new UpdateDeltaResponse();
            try
            {
                int logtype = 1;
                if (string.IsNullOrEmpty(model.device_id) == false && model.device_id.Trim().Length > 0)
                {
                    logtype = 3;
                    response.data.device_id = model.device_id;
                    response.success = notificationDAL.UpdateDelta(model.device_id, model.max_delta_id);
                }
                
                if (string.IsNullOrEmpty(model.log_msg) == false && model.log_msg.Trim().Length > 0)
                {
                    logDAL.InsertLog("APN", model.log_msg, "APN Service", logtype,0);
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "UpdateDelta:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(response);
        }
    }
}
