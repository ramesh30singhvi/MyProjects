using CPReservationApi.Common;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.Services;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/business")]
    public class BusinessController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public BusinessController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        [Route("getsubscriptionplans")]
        [HttpGet]
        public IActionResult GetSubscriptionPlans()
        {
            GetSubscriptionPlansResponse businessResponse = new GetSubscriptionPlansResponse();

            try
            {
                SubscriptionYearlyMonthlyPlans plans = new SubscriptionYearlyMonthlyPlans();
                BusinessDAL dal = new BusinessDAL(Common.Common.ConnectionString);
                var data = dal.GetSubscriptionPlans();
                if (data != null)
                {
                    List<SubscriptionPlans> yearlyPlans = new List<SubscriptionPlans>();
                    List<SubscriptionPlans> monthlyPlans = new List<SubscriptionPlans>();
                    //for (var i = 0; i < data.Count; i++)
                    //{
                    //    if (data[i].id == 48 || data[i].id == 49 || data[i].id == 53 || data[i].id == 57 || data[i].id == 57)
                    //    {
                    //        monthlyPlans.Add(data[i]);
                    //    }
                    //    else if (data[i].id == 51 || data[i].id == 55)
                    //    {
                    //        yearlyPlans.Add(data[i]);
                    //    }
                    //}

                    if (data != null && data.Count > 0)
                    {
                        yearlyPlans = data.Where(p => p.plan_frequency == 2).ToList();
                        monthlyPlans = data.Where(p => p.plan_frequency == 1).ToList();
                    }
                    plans.YearlySubscriptionPlans = yearlyPlans;
                    plans.MonthlySubscriptionPlans = monthlyPlans;
                    businessResponse.success = true;
                    businessResponse.data = plans;
                }
                else
                {
                    businessResponse.success = true;
                    businessResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    businessResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                businessResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                businessResponse.error_info.extra_info = Common.Common.InternalServerError;
                businessResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetBusinessSubscriptionPlans:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(businessResponse);
        }

        [Route("getdestinationlandingpage")]
        [HttpGet]
        public IActionResult GetDestinationLandingPage(int region_id, string region_url = "")
        {
            var response = new GetDestinationLandingPageResponse();
            try
            {
                if (region_id <= 0 && string.IsNullOrEmpty(region_url))
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no record found";
                }
                else
                {
                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                    DestinationDetails destinationDetails = eventDAL.GetDestinationLandingPage(region_id, region_url);

                    if (destinationDetails != null)
                    {
                        response.success = true;
                        response.data = destinationDetails;
                    }
                    else
                    {
                        response.success = true;
                        response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        response.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetDestinationLandingPage:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(response);
        }

        [Route("profiledetailbybusinessid")]
        [HttpGet]
        public IActionResult GetProfileDetailByMemberId(int business_id)
        {
            var response = new ProfileDetailByMemberIdResponse();
            try
            {
                if (business_id <= 0)
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no record found";
                }
                else
                {
                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                    WidgetModel widgetModel = eventDAL.GetWineryWidgetById(business_id);

                    if (widgetModel != null)
                    {
                        widgetModel.business_phone = Utility.FormatPhoneNumber(widgetModel.business_phone);
                        response.success = true;
                        response.data = widgetModel;
                    }
                    else
                    {
                        response.success = true;
                        response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        response.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetProfileDetailByMemberId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, business_id);
            }
            return new JsonResult(response);
        }

        [Route("getsetting")]
        [HttpGet]
        public IActionResult GetSetting(int member_id, int group, int key)
        {
            var response = new GetSettingResponse();
            try
            {
                if (member_id <= 0 || group <= 0 || key <= 0)
                {
                    response.success = true;
                    response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    response.error_info.extra_info = "Need a valid memberId, GroupId and Key to get the setting";
                }
                else
                {
                    SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                    List<Settings.Setting> settingsDetails = settingsDAL.GetSetting(member_id,group,key);

                    if (settingsDetails != null)
                    {
                        response.success = true;
                        response.data = settingsDetails;
                    }
                    else
                    {
                        response.success = true;
                        response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        response.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetSetting:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(response);
        }

        [Route("businessreviews")]
        [HttpGet]
        public IActionResult GetBusinessReviews(int business_id)
        {
            var response = new WineryReviewResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                List<WineryReviewViewModel> list = eventDAL.GetWineryReviewsByMemberId(business_id);

                if (list != null && list.Count > 0)
                {
                    response.success = true;
                    response.data = list;
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
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetBusinessReviews:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(response);
        }

        [Route("faqbybusinessid")]
        [HttpGet]
        public IActionResult GetFaqByMemberId(int business_id)
        {
            var fAQResponse = new MemberFaqResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                List<MemberFaqModel> strList = new List<MemberFaqModel>();
                strList = ticketDAL.GetFAQByMemberId(business_id);

                if (strList != null)
                {
                    fAQResponse.success = true;
                    fAQResponse.data = strList;
                }
                else
                {
                    fAQResponse.success = true;
                    fAQResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    fAQResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                fAQResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                fAQResponse.error_info.extra_info = Common.Common.InternalServerError;
                fAQResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetFaqByMemberId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, business_id);
            }
            return new ObjectResult(fAQResponse);
        }

        [Route("images")]
        [HttpGet]
        public IActionResult GetWineryImages(int business_id, CPImageType image_type)
        {
            var response = new WineryImagesResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                List<WineryImageModel> strList = new List<WineryImageModel>();
                strList = eventDAL.GetWineryImages(business_id, (int)image_type);

                if (strList != null)
                {
                    response.success = true;
                    response.data = strList;
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
                logDAL.InsertLog("WebApi", "GetWineryImages:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, business_id);
            }
            return new ObjectResult(response);
        }
    }
}