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

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/location")]
    public class LocationController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public LocationController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }
        /// <summary>
        /// This method will give list of active lications for a member
        /// </summary>
        /// <param name="member_id">Id of member (Required)</param>
        /// <param name="active_only"></param>
        /// <returns></returns>
        [Route("list")]
        [HttpGet]
        public IActionResult GetLocations(int member_id, bool active_only = false)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            var locationResponse = new LocationResponse();

            try
            {
                var model = new List<LocationModel>();

                bool TablePro = false;
                if (HttpContext.Request.Headers["AuthenticateKey"] == Common.Common.GetEnumDescription(Common.Common.AurthorizedUserType.TablePro))
                    TablePro = true;

                model = eventDAL.GetLocationByWineryID(member_id, TablePro, active_only);

                if (model != null)
                {
                    locationResponse.success = true;
                    locationResponse.data = model;
                }
                else
                {
                    locationResponse.success = true;
                    locationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    locationResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                locationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                locationResponse.error_info.extra_info = Common.Common.InternalServerError;
                locationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetLocations:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(locationResponse);
        }

        [Route("floorplans")]
        [HttpGet]
        public IActionResult GetFloorplans(int member_id=0, int location_id=0, bool active_only=true)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            var response = new FloorPlanResponse();

            if (member_id <= 0 && location_id <= 0)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Member Id or Location Id is required.";
                response.error_info.description = "Member Id or Location Id is required.";
                return new ObjectResult(response);
            }

            try
            {
                var model = new List<FloorPlanModel>();


                model = eventDAL.GetFloorPlansByMemberOrLocation(member_id, location_id, active_only);

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
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetFloorPlans:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(response);
        }
    }
}