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

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/info")]
    public class InfoController : BaseController
    {

        public static IOptions<AppSettings> _appSetting;
        public InfoController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        /// <summary>
        /// This method gives list of active regions
        /// </summary>
        /// <returns></returns>
        [Route("region")]
        [HttpGet]
        public IActionResult GetActiveRegions(string state_code = "",string country_code = "")
        {
            var regionResponse = new RegionResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var regionModel = new List<RegionModel>();

                regionModel = eventDAL.GetRegions(state_code, country_code);
                if (regionModel != null)
                {
                    regionResponse.success = true;
                    regionResponse.data = regionModel;
                }
                else
                {
                    regionResponse.success = true;
                    regionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    regionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                regionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                regionResponse.error_info.extra_info = Common.Common.InternalServerError;
                regionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetActiveRegions:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(regionResponse);
        }

        [Route("activehomepageregions")]
        [HttpGet]
        public IActionResult GetActiveHomePageRegions(bool is_events_page_only = false)
        {
            var regionResponse = new RegionDetailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var regionModel = new List<RegionDetail2Model>();

                regionModel = eventDAL.GetActiveHomePageRegions(is_events_page_only);
                if (regionModel != null)
                {
                    regionResponse.success = true;
                    regionResponse.data = regionModel;
                }
                else
                {
                    regionResponse.success = true;
                    regionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    regionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                regionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                regionResponse.error_info.extra_info = Common.Common.InternalServerError;
                regionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetActiveHomePageRegions:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(regionResponse);
        }

        [Route("regionbyurl")]
        [HttpGet]
        public IActionResult GetRegionByURL(string url)
        {
            var regionResponse = new RegionDetailByURLResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var regionModel = new Region2Model();

                if (!string.IsNullOrEmpty(url))
                {
                    regionModel = eventDAL.GetRegionByURL(url);
                }
                
                if (regionModel != null && regionModel.id > 0)
                {
                    regionResponse.success = true;
                    regionResponse.data = regionModel;
                }
                else
                {
                    regionResponse.success = true;
                    regionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    regionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                regionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                regionResponse.error_info.extra_info = Common.Common.InternalServerError;
                regionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetRegionByURL:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(regionResponse);
        }

        [Route("regionbyid")]
        [HttpGet]
        public IActionResult GetRegionById(int region_id = 0)
        {
            var regionResponse = new RegionDetailByURLResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var regionModel = new Region2Model();

                if (region_id > 0)
                {
                    regionModel = eventDAL.GetRegionByURL("", region_id);
                }

                if (regionModel != null && regionModel.id > 0)
                {
                    regionResponse.success = true;
                    regionResponse.data = regionModel;
                }
                else
                {
                    regionResponse.success = true;
                    regionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    regionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                regionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                regionResponse.error_info.extra_info = Common.Common.InternalServerError;
                regionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetRegionById:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(regionResponse);
        }

        [Route("subregionbyregionid")]
        [HttpGet]
        public IActionResult GetSubRegionByRegionId(int region_id = 0)
        {
            var regionResponse = new SubRegionResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var regionModel = new List<SubRegionModel>();

                if (region_id > 0)
                {
                    regionModel = eventDAL.GetSubRegionByRegionId(region_id);
                }

                if (regionModel != null && regionModel.Count > 0)
                {
                    regionResponse.success = true;
                    regionResponse.data = regionModel;
                }
                else
                {
                    regionResponse.success = true;
                    regionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    regionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                regionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                regionResponse.error_info.extra_info = Common.Common.InternalServerError;
                regionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetSubRegionByRegionId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(regionResponse);
        }

        [Route("getregionpagebyurl")]
        [HttpGet]
        public IActionResult GetPageByURL(string url)
        {
            var regionResponse = new PageDetailByURLResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var regionModel = new PageModel();

                if (!string.IsNullOrEmpty(url))
                {
                    regionModel = eventDAL.GetPageByURL(url);
                }

                if (regionModel != null && regionModel.id > 0)
                {
                    regionResponse.success = true;
                    regionResponse.data = regionModel;
                }
                else
                {
                    regionResponse.success = true;
                    regionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    regionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                regionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                regionResponse.error_info.extra_info = Common.Common.InternalServerError;
                regionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPageByURL:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(regionResponse);
        }

        [Route("poicategories")]
        [HttpGet]
        public IActionResult GetPOICategoriesByRegionId(int region_id)
        {
            var pOICategoriesResponse = new POICategoriesResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var regionDetailModel = new RegionDetailModel();

                regionDetailModel = eventDAL.GetPOICategoriesByRegionId(region_id);
                if (regionDetailModel != null && regionDetailModel.id > 0)
                {
                    pOICategoriesResponse.success = true;
                    pOICategoriesResponse.data = regionDetailModel;
                }
                else
                {
                    pOICategoriesResponse.success = true;
                    pOICategoriesResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    pOICategoriesResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                pOICategoriesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                pOICategoriesResponse.error_info.extra_info = Common.Common.InternalServerError;
                pOICategoriesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPOICategoriesByRegionId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(pOICategoriesResponse);
        }

        /// <summary>
        /// This method gives list of options for "How did you hear?" while creating reservation
        /// </summary>
        /// <param name="member_id">Id of member (Required)</param>
        /// <returns></returns>
        [Route("hdyh")]
        [HttpGet]
        public IActionResult GetHDYH_OptionsByMemberId(int member_id)
        {
            var hDYHResponse = new HDYHResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var hdyh = new HDYHModel();
                var model = new HDYH();
                hdyh = eventDAL.GetHDYH_OptionsByWineryId(member_id);
                List<HDYH> strList = new List<HDYH>();

                if ((hdyh != null))
                {
                    if (!string.IsNullOrEmpty(hdyh.answer_1))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_1;
                        strList.Add(model);
                    }
                    if (!string.IsNullOrEmpty(hdyh.answer_2))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_2;
                        strList.Add(model);
                    }
                    if (!string.IsNullOrEmpty(hdyh.answer_3))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_3;
                        strList.Add(model);
                    }
                    if (!string.IsNullOrEmpty(hdyh.answer_4))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_4;
                        strList.Add(model);
                    }
                    if (!string.IsNullOrEmpty(hdyh.answer_5))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_5;
                        strList.Add(model);
                    }
                    if (!string.IsNullOrEmpty(hdyh.answer_6))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_6;
                        strList.Add(model);
                    }
                    if (!string.IsNullOrEmpty(hdyh.answer_7))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_7;
                        strList.Add(model);
                    }
                    if (!string.IsNullOrEmpty(hdyh.answer_8))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_8;
                        strList.Add(model);
                    }
                    if (!string.IsNullOrEmpty(hdyh.answer_9))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_9;
                        strList.Add(model);
                    }
                    if (!string.IsNullOrEmpty(hdyh.answer_10))
                    {
                        model = new HDYH();
                        model.choice = hdyh.answer_10;
                        strList.Add(model);
                    }
                }

                if (strList != null)
                {
                    hDYHResponse.success = true;
                    hDYHResponse.data = strList;
                }
                else
                {
                    hDYHResponse.success = true;
                    hDYHResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    hDYHResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                hDYHResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                hDYHResponse.error_info.extra_info = Common.Common.InternalServerError;
                hDYHResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetHDYH_OptionsBymemberId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(hDYHResponse);
        }

        /// <summary>
        /// This method gives list of affiliates for a member
        /// </summary>
        /// <param name="member_id">Id of member (Required)</param>
        /// <returns></returns>
        [Route("affiliate")]
        [HttpGet]
        public IActionResult GetAffiliatesByMemberId(int member_id)
        {
            var affiliateResponse = new AffiliateResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var affiliateModel = new List<AffiliateModel>();

                affiliateModel = eventDAL.GetAffiliatesByWineryId(member_id);
                if (affiliateModel != null)
                {
                    affiliateResponse.success = true;
                    affiliateResponse.data = affiliateModel;
                }
                else
                {
                    affiliateResponse.success = true;
                    affiliateResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    affiliateResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                affiliateResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                affiliateResponse.error_info.extra_info = Common.Common.InternalServerError;
                affiliateResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetAffiliatesBymemberId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(affiliateResponse);
        }

        /// <summary>
        /// This method gives list of deposit policies for a member
        /// </summary>
        /// <param name="member_id">Id of member  (Required)</param>
        /// <returns></returns>
        [Route("depositpolicy")]
        [HttpGet]
        public IActionResult GetDepositpolicyByMemberId(int member_id, int event_id)
        {
            var depositpolicyResponse = new DepositpolicyResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                EventModel eventModel = eventDAL.GetEventById(event_id);
                int ChargeFee = eventModel.ChargeFee;
                var config = new PaymentConfigModel();
                var depositpolicyModel = new List<DepositpolicyModel>();

                config = eventDAL.GetPaymentConfigByWineryId(member_id);

                if (config.MerchantLogin == null || config.MerchantPassword == null)
                {
                    depositpolicyModel = Utility.GetChargeFeeWithoutPayment(ChargeFee);
                }
                else
                {
                    if (config.PaymentGateway == Common.Payments.Configuration.Gateway.Offline || config.MerchantLogin.Length == 0)
                    {
                        depositpolicyModel = Utility.GetChargeFeeWithoutPayment(ChargeFee);
                    }
                    else
                    {
                        depositpolicyModel = Utility.GetChargeFeeRSVP(ChargeFee);
                    }
                }

                if (depositpolicyModel != null)
                {
                    depositpolicyResponse.success = true;
                    depositpolicyResponse.data = depositpolicyModel;
                }
                else
                {
                    depositpolicyResponse.success = true;
                    depositpolicyResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    depositpolicyResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                depositpolicyResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                depositpolicyResponse.error_info.extra_info = Common.Common.InternalServerError;
                depositpolicyResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetDepositpolicyBymemberId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(depositpolicyResponse);
        }

        /// <summary>
        /// This method gives list of rsvp confirmation email template for a member
        /// </summary>
        /// <param name="member_id">Id of member  (Required)</param>
        /// <returns></returns>
        [Route("confirmationtemplate")]
        [HttpGet]
        public IActionResult GetRsvpConfirmationEmailTemplates(int member_id, int event_id, bool active_only = false)
        {
            var rsvpConfirmationEmailTemplateResponse = new RsvpConfirmationEmailTemplateResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var rsvpConfirmationEmailTemplateModel = new List<RsvpConfirmationEmailTemplateModel>();

                rsvpConfirmationEmailTemplateModel = eventDAL.GetRsvpConfirmationEmailTemplates(member_id, event_id, active_only);

                if (rsvpConfirmationEmailTemplateModel != null)
                {
                    rsvpConfirmationEmailTemplateResponse.success = true;
                    rsvpConfirmationEmailTemplateResponse.data = rsvpConfirmationEmailTemplateModel;
                }
                else
                {
                    rsvpConfirmationEmailTemplateResponse.success = true;
                    rsvpConfirmationEmailTemplateResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    rsvpConfirmationEmailTemplateResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                rsvpConfirmationEmailTemplateResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                rsvpConfirmationEmailTemplateResponse.error_info.extra_info = Common.Common.InternalServerError;
                rsvpConfirmationEmailTemplateResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetRsvpConfirmationEmailTemplates:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(rsvpConfirmationEmailTemplateResponse);
        }

        [Route("faq")]
        [HttpGet]
        public IActionResult GetFAQByEventType(Common.AddOns.EventType event_type = Common.AddOns.EventType.none)
        {
            var fAQResponse = new FAQResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<FAQ> strList = new List<FAQ>();
                strList = ticketDAL.GetFAQByEventType((int)event_type);


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
                logDAL.InsertLog("WebApi", "GetFAQByEventType:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(fAQResponse);
        }

        [Route("contactreason")]
        [HttpGet]
        public IActionResult GetContactReason()
        {
            var contactReasonResponse = new ContactReasonResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                ContactReason strList = new ContactReason();
                strList = ticketDAL.GetContactReason();


                if (strList != null)
                {
                    contactReasonResponse.success = true;
                    contactReasonResponse.data = strList;
                }
                else
                {
                    contactReasonResponse.success = true;
                    contactReasonResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    contactReasonResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                contactReasonResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                contactReasonResponse.error_info.extra_info = Common.Common.InternalServerError;
                contactReasonResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetContactReason:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(contactReasonResponse);
        }

        [Route("getcontent")]
        [HttpGet]
        public IActionResult GetContentByType(Common.Common.SiteContentType content_id, int member_id = 0)
        {
            var contentResponse = new GetContentResponse();
            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                string content = "";

                if (content_id == Common.Common.SiteContentType.BusinessPagePromoModal)
                    content = settingsDAL.GetContent(content_id);
                else
                    content = settingsDAL.GetContent(content_id, member_id);


                if (!string.IsNullOrWhiteSpace(content))
                {
                    if (content_id == Common.Common.SiteContentType.BusinessPagePromoModal && member_id > 0)
                    {
                        //string strpath = "https://typhoon.cellarpass.com/";
                        //if (Common.Common.ConnectionString.IndexOf("live") > -1)
                        //    strpath = "https://www.cellarpass.com/";

                        EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                        //string profileUrl = string.Format("{1}profile/{0}?p=204", eventDAL.GetWineryById(member_id).MemberProfileUrl, strpath);
                       
                        int PromoId = eventDAL.GetActiveCellarScoutPromotionId(member_id);
                        string profileUrl = string.Format("[{0}]", PromoId);
                        content = content.Replace("[[Promotion]]", profileUrl);
                    }

                    contentResponse.success = true;
                    contentResponse.data = content;
                }
                else
                {
                    contentResponse.success = true;
                    contentResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    contentResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                contentResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                contentResponse.error_info.extra_info = Common.Common.InternalServerError;
                contentResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "getcontent:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(contentResponse);
        }

        [Route("preferredvisitduration")]
        [HttpGet]
        public IActionResult GetPreferredVisitDuration()
        {
            var preferredVisitDurationResponse = new PreferredVisitDurationResponse();
            try
            {
                var listModel = new List<PreferredVisitDurationModel>();

                preferredVisitDurationResponse.success = true;
                preferredVisitDurationResponse.data = Utility.GetPreferredVisitDuration();
            }
            catch (Exception ex)
            {
                preferredVisitDurationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                preferredVisitDurationResponse.error_info.extra_info = Common.Common.InternalServerError;
                preferredVisitDurationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetPreferredVisitDuration:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(preferredVisitDurationResponse);
        }

        [Route("reasonforvisit")]
        [HttpGet]
        public IActionResult GetReasonforVisit()
        {
            var reasonforVisitResponse = new ReasonforVisitResponse();
            try
            {
                var listModel = new List<ReasonforVisitModel>();

                reasonforVisitResponse.success = true;
                reasonforVisitResponse.data = Utility.GetReasonforVisit();
            }
            catch (Exception ex)
            {
                reasonforVisitResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reasonforVisitResponse.error_info.extra_info = Common.Common.InternalServerError;
                reasonforVisitResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetReasonforVisit:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(reasonforVisitResponse);
        }

        [Route("tags")]
        [HttpGet]
        public IActionResult GetMemberTags(int member_id)
        {
            var tagResponse = new TagResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var tag = new List<TagModel>();
                tag = eventDAL.GetMemberTags(member_id);
                
                if (tag != null)
                {
                    tagResponse.success = true;
                    tagResponse.data = tag;
                }
                else
                {
                    tagResponse.success = true;
                    tagResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    tagResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                tagResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                tagResponse.error_info.extra_info = Common.Common.InternalServerError;
                tagResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetMemberTags:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(tagResponse);
        }

        [Route("guesttags")]
        [HttpGet]
        public IActionResult GetGuestTags()
        {
            var tagResponse = new GuestTagResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var tag = new List<GuestTagModel>();
                tag = eventDAL.GetGuestTags();

                if (tag != null)
                {
                    tagResponse.success = true;
                    tagResponse.data = tag;
                }
                else
                {
                    tagResponse.success = true;
                    tagResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    tagResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                tagResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                tagResponse.error_info.extra_info = Common.Common.InternalServerError;
                tagResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetGuestTags:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(tagResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("welcomedashboarddata")]
        [HttpGet]
        public IActionResult GetWelcomeDashboardData()
        {
            var response = new WelcomeDashboardDataResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var model = new WelcomeDashboardDataModel();

                response.success = false;

                model = eventDAL.GetWelcomeDashboardData();
                if (model != null)
                {
                    response.success = true;
                    response.data = model;
                }
                else
                {
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
                logDAL.InsertLog("WebApi", "GetWelcomeDashboardData:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("businessprotipstricks")]
        [HttpGet]
        public IActionResult GetBusinessProtipsTricks()
        {
            var businessProtipsTricksResponse = new BusinessProtipsTricksResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var businessProtipsTricksModel = new List<BusinessProtipsTricksModel>();

                businessProtipsTricksModel = eventDAL.GetBusinessProtipsTricks();
                if (businessProtipsTricksModel != null)
                {
                    businessProtipsTricksResponse.success = true;
                    businessProtipsTricksResponse.data = businessProtipsTricksModel;
                }
                else
                {
                    businessProtipsTricksResponse.success = true;
                    businessProtipsTricksResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    businessProtipsTricksResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                businessProtipsTricksResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                businessProtipsTricksResponse.error_info.extra_info = Common.Common.InternalServerError;
                businessProtipsTricksResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetBusinessProtipsTricks:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(businessProtipsTricksResponse);
        }

        [Route("reviewvalue")]
        [HttpGet]
        public IActionResult GetReviewValue()
        {
            var preferredVisitDurationResponse = new PreferredVisitDurationResponse();
            try
            {
                var listModel = new List<PreferredVisitDurationModel>();

                preferredVisitDurationResponse.success = true;
                preferredVisitDurationResponse.data = Utility.GetReviewValue();
            }
            catch (Exception ex)
            {
                preferredVisitDurationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                preferredVisitDurationResponse.error_info.extra_info = Common.Common.InternalServerError;
                preferredVisitDurationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetReviewValue:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(preferredVisitDurationResponse);
        }

        [Route("getstatesbycountrycode")]
        [HttpGet]
        public IActionResult GetStatesByCountryCode(string country_code)
        {
            var response = new StatesResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var stateModel = new List<StateViewModel>();

                if (!string.IsNullOrEmpty(country_code))
                {
                    stateModel = eventDAL.GetStatesByCountryCode(country_code);
                }

                if (stateModel != null && stateModel.Count > 0)
                {
                    response.success = true;
                    response.data = stateModel;
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
                logDAL.InsertLog("WebApi", "GetStatesByCountryCode:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("getstatedata")]
        [HttpGet]
        public IActionResult GetStateInfoWithMetaData(string state)
        {
            var statesDataResponse = new StatesDataResponse();
            if (string.IsNullOrWhiteSpace(state))
            {
                statesDataResponse.success = true;
                statesDataResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                statesDataResponse.error_info.extra_info = "Please pass a vlaid state name or code";
                return new ObjectResult(statesDataResponse);
            }

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var stateDataModel = new StateDataModel();

                stateDataModel = eventDAL.GetStateData(state);
                if (stateDataModel != null)
                {
                    statesDataResponse.success = true;
                    statesDataResponse.data = stateDataModel;
                }
                else
                {
                    statesDataResponse.success = true;
                    statesDataResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    statesDataResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                statesDataResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                statesDataResponse.error_info.extra_info = Common.Common.InternalServerError;
                statesDataResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetStateInfoWithMetaData:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(statesDataResponse);
        }

        [Route("getspecialeventspagefilter")]
        [HttpGet]
        public IActionResult GetSpecialEventsPageFilter()
        {
            var specialEventsPageFilterResponse = new SpecialEventsPageFilterResponse();
            
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var specialEventsPageFilterModel = eventDAL.GetSpecialEventsPageFilter();
                if (specialEventsPageFilterModel != null)
                {
                    specialEventsPageFilterResponse.success = true;
                    specialEventsPageFilterResponse.data = specialEventsPageFilterModel;
                }
                else
                {
                    specialEventsPageFilterResponse.success = true;
                    specialEventsPageFilterResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    specialEventsPageFilterResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                specialEventsPageFilterResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                specialEventsPageFilterResponse.error_info.extra_info = Common.Common.InternalServerError;
                specialEventsPageFilterResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetSpecialEventsPageFilter:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(specialEventsPageFilterResponse);
        }

        [Route("businesstypes")]
        [HttpGet]
        public IActionResult GetWineryTypes()
        {
            var tagResponse = new WineryTypeResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var tag = new List<WineryTypesModel>();
                tag = eventDAL.GetWineryTypes();

                if (tag != null)
                {
                    tagResponse.success = true;
                    tagResponse.data = tag;
                }
                else
                {
                    tagResponse.success = true;
                    tagResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    tagResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                tagResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                tagResponse.error_info.extra_info = Common.Common.InternalServerError;
                tagResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetWineryTypes:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(tagResponse);
        }

        [Route("affiliatetypes")]
        [HttpGet]
        public IActionResult GetAffiliateTypes()
        {
            var affiliateTypesResponse = new AffiliateTypesResponse();
            try
            {
                List<KeyValueModel> pairs = new List<KeyValueModel>();

                foreach (var value in Enum.GetValues(typeof(AffiliateType)))
                {
                    pairs.Add(new KeyValueModel
                    {
                        text = ((AffiliateType)value).GetEnumDescription(),
                        value = ((int)value).ToString(),
                    });
                }

                affiliateTypesResponse.success = true;
                affiliateTypesResponse.data = pairs;
            }
            catch (Exception ex)
            {
                affiliateTypesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                affiliateTypesResponse.error_info.extra_info = Common.Common.InternalServerError;
                affiliateTypesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetAffiliateTypes:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(affiliateTypesResponse);
        }

        [Route("countries")]
        [HttpGet]
        public IActionResult GetCountries()
        {
            var countryResponse = new CountryResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var countries = new List<CountryModel>();
                countries = eventDAL.GetCountries();

                if (countries != null)
                {
                    countryResponse.success = true;
                    countryResponse.data = countries;
                }
                else
                {
                    countryResponse.success = true;
                    countryResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    countryResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                countryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                countryResponse.error_info.extra_info = Common.Common.InternalServerError;
                countryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCountries:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(countryResponse);
        }

        [Route("pagebyurl")]
        [HttpGet]
        public IActionResult GetPagesByUrl(string url)
        {
            var response = new CmsPageResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var model = new CmsPageModel();

                model = eventDAL.GetPagesByUrl(url);

                if (model != null && model.id > 0)
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
                logDAL.InsertLog("WebApi", "GetPagesByUrl:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("pagesectionbyid")]
        [HttpGet]
        public IActionResult GetPageSectionById(int id)
        {
            var response = new CmsPageSectionResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var model = new CmsPageSectionModel();

                model = eventDAL.GetPageSectionById(id);

                if (model != null && model.id > 0)
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
                logDAL.InsertLog("WebApi", "GetPageSectionById:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }
    }
}
