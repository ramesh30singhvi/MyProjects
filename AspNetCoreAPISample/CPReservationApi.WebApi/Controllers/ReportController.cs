using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CPReservationApi.WebApi.ViewModels;
using CPReservationApi.WebApi.Services;
using CPReservationApi.DAL;
using CPReservationApi.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/report")]
    public class ReportController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public ReportController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        /// <summary>
        /// This report shows how many guests were assigned to each server on an hourly basis
        /// </summary>
        /// <param name="day">Day for which request is requested</param>
        /// <param name="member_id">Id of member</param>
        /// <param name="location_ids">Id of location</param>
        /// <returns></returns>
        [Route("shiftreport")]
        [HttpGet]
        public IActionResult ShiftReport(DateTime day, int member_id, int[] location_ids)
        {
            string strlocation_ids = string.Join(",", location_ids);
            ReportDAL reportDAL = new ReportDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var reportResponse = new ShiftReportResponse();
            try
            {
                var listmodel = new List<ShiftReport>();
                if (string.IsNullOrEmpty(strlocation_ids))
                {
                    var model = new List<LocationModel>();
                    model = eventDAL.GetLocationByWineryID(member_id);
                    strlocation_ids = string.Join(",", model.Select(a => a.location_id));
                }
                listmodel = reportDAL.GetShiftReport(day, member_id, strlocation_ids);
                if (listmodel != null)
                {
                    reportResponse.success = true;
                    reportResponse.data = listmodel;
                }
                else
                {
                    reportResponse.success = true;
                    reportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reportResponse.error_info.extra_info = Common.Common.InternalServerError;
                reportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ShiftReport:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(reportResponse);
        }

        [Route("shiftreportv2")]
        [HttpGet]
        public IActionResult ShiftReportV2(DateTime day, int member_id, int[] floor_plan_ids)
        {
            string strfloor_plan_ids = string.Join(",", floor_plan_ids);
            ReportDAL reportDAL = new ReportDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var reportResponse = new ShiftReportResponse();
            try
            {
                var listmodel = new List<ShiftReport>();
                //if (string.IsNullOrEmpty(strlocation_ids))
                //{
                //    var model = new List<LocationModel>();
                //    model = eventDAL.GetLocationByWineryID(member_id);
                //    strlocation_ids = string.Join(",", model.Select(a => a.location_id));
                //}
                listmodel = reportDAL.GetShiftReportV2(day, member_id, strfloor_plan_ids);
                if (listmodel != null)
                {
                    reportResponse.success = true;
                    reportResponse.data = listmodel;
                }
                else
                {
                    reportResponse.success = true;
                    reportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reportResponse.error_info.extra_info = Common.Common.InternalServerError;
                reportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ShiftReportV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(reportResponse);
        }

        /// <summary>
        /// This report is designed to help the manager understand the guests assigned to the tables at what time.
        /// </summary>
        /// <param name="day">Day for which request is requested</param>
        /// <param name="member_id">Id of member</param>
        /// <param name="location_ids">Id of location</param>
        /// <returns></returns>
        [Route("coversreport")]
        [HttpGet]
        public IActionResult CoversReport(DateTime day, int member_id, int[] location_ids)
        {
            string strlocation_ids = string.Join(",", location_ids);
            ReportDAL reportDAL = new ReportDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var reportResponse = new CoversReportResponse();
            try
            {
                var model = new List<CoversReportLocations>();
                if (string.IsNullOrEmpty(strlocation_ids))
                {
                    var locationmodel = new List<LocationModel>();
                    locationmodel = eventDAL.GetLocationByWineryID(member_id);
                    strlocation_ids = string.Join(",", locationmodel.Select(a => a.location_id));
                }
                model=reportDAL.GetCoversReport(day, member_id, strlocation_ids);
                if (model != null)
                {
                    reportResponse.success = true;
                    reportResponse.data = model;
                }
                else
                {
                    reportResponse.success = true;
                    reportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reportResponse.error_info.extra_info = Common.Common.InternalServerError;
                reportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CoversReport:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(reportResponse);
        }

        [Route("coversreportv2")]
        [HttpGet]
        public IActionResult CoversReportV2(DateTime day, int member_id, int[] floor_plan_ids)
        {
            string strfloorplan_ids = string.Join(",", floor_plan_ids);
            ReportDAL reportDAL = new ReportDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var reportResponse = new CoversReportResponseV2();
            try
            {
                var model = new List<CoversReportLocationsV2>();
                //if (string.IsNullOrEmpty(strlocation_ids))
                //{
                //    var locationmodel = new List<LocationModel>();
                //    locationmodel = eventDAL.GetLocationByWineryID(member_id);
                //    strlocation_ids = string.Join(",", locationmodel.Select(a => a.location_id));
                //}
                model = reportDAL.GetCoversReportV2(day, member_id, strfloorplan_ids);
                if (model != null)
                {
                    reportResponse.success = true;
                    reportResponse.data = model;
                }
                else
                {
                    reportResponse.success = true;
                    reportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reportResponse.error_info.extra_info = Common.Common.InternalServerError;
                reportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CoversReportV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(reportResponse);
        }

        [Route("coversreportv3")]
        [HttpGet]
        public IActionResult CoversReportV3(DateTime day, int member_id, int[] floor_plan_ids)
        {
            string strfloorplan_ids = string.Join(",", floor_plan_ids);
            ReportDAL reportDAL = new ReportDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var reportResponse = new CoversReportResponseV3();
            try
            {
                var model = new List<CoversReportLocationsV3>();
                
                model = reportDAL.GetCoversReportV3(day, member_id, strfloorplan_ids);
                if (model != null)
                {
                    reportResponse.success = true;
                    reportResponse.data = model;
                }
                else
                {
                    reportResponse.success = true;
                    reportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reportResponse.error_info.extra_info = Common.Common.InternalServerError;
                reportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CoversReportV3: " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(reportResponse);
        }

        [Route("dailyreport")]
        [HttpGet]
        public IActionResult DailyReport(DateTime date, int member_id, int[] location_ids)
        {
            string strlocation_ids = string.Join(",", location_ids);
            ReportDAL reportDAL = new ReportDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var reportResponse = new DailyReportResponse();
            try
            {
                var model = new DailyReport();
                if (string.IsNullOrEmpty(strlocation_ids))
                {
                    var locationmodel = new List<LocationModel>();
                    locationmodel = eventDAL.GetLocationByWineryID(member_id);
                    strlocation_ids = string.Join(",", locationmodel.Select(a => a.location_id));
                }
                model = reportDAL.GetDailyReport(date, member_id, strlocation_ids);
                model.notes = eventDAL.GetCalendarNotes(member_id, date);
                if (model != null)
                {
                    reportResponse.success = true;
                    reportResponse.data = model;
                }
                else
                {
                    reportResponse.success = true;
                    reportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reportResponse.error_info.extra_info = Common.Common.InternalServerError;
                reportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "DailyReport:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(reportResponse);
        }

        [Route("dailyreportv2")]
        [HttpGet]
        public IActionResult DailyReportV2(DateTime date, int member_id, int[] floor_plan_ids)
        {
            ReportDAL reportDAL = new ReportDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var reportResponse = new DailyReportResponse();
            try
            {
                var model = new DailyReport();

                var list = eventDAL.GetRsvpLocationIdByFloorPlanId(floor_plan_ids);
                //var list = new List<int>();
                //foreach (var item in floor_plan_ids)
                //{
                //    list.Add(eventDAL.GetLocationIdByFloorPlanId(item));
                //}

                string strlocation_ids = string.Join(",", list.Distinct());

                model = reportDAL.GetDailyReport(date, member_id, strlocation_ids);
                model.notes = eventDAL.GetCalendarNotes(member_id, date);
                if (model != null)
                {
                    reportResponse.success = true;
                    reportResponse.data = model;
                }
                else
                {
                    reportResponse.success = true;
                    reportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reportResponse.error_info.extra_info = Common.Common.InternalServerError;
                reportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "DailyReportV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(reportResponse);
        }

        [Route("exportreservationdetail")]
        [HttpGet]
        public IActionResult ExportReservationDetail(DateTime date, int member_id)
        {
            ReportDAL reportDAL = new ReportDAL(Common.Common.ConnectionString);
            var reportResponse = new ExportReservationDetailResponse();
            try
            {
                var model = new List<ExportReservationDetail>();
                model = reportDAL.ExportReservation(date, member_id);
                if (model != null)
                {
                    reportResponse.success = true;
                    reportResponse.data = model;
                }
                else
                {
                    reportResponse.success = true;
                    reportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reportResponse.error_info.extra_info = Common.Common.InternalServerError;
                reportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ExportReservationDetail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(reportResponse);
        }

        [Route("exportreservation")]
        [HttpGet]
        public IActionResult ExportReservation(DateTime date, int member_id)
        {
            ReportDAL reportDAL = new ReportDAL(Common.Common.ConnectionString);
            var reportResponse = new ExportReservationResponse();
            try
            {
                var model = new List<ExportReservation>();
                model = reportDAL.ExportReservationByDay(date, member_id);
                if (model != null)
                {
                    reportResponse.success = true;
                    reportResponse.data = model;
                }
                else
                {
                    reportResponse.success = true;
                    reportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reportResponse.error_info.extra_info = Common.Common.InternalServerError;
                reportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ExportReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(reportResponse);
        }
    }
}
