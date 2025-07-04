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
using Twilio;
using CPReservationApi.Common;
using static CPReservationApi.Common.Email;
//using nsoftware.InPay;
using Newtonsoft.Json;
using System.Globalization;
using static CPReservationApi.Common.Payments;
using eWineryWebServices;
using System.IO;
using static CPReservationApi.Common.Common;
using System.Collections.Specialized;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/ticket")]
    public class TicketController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public TicketController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        [Route("sendabandonedcartticketemail")]
        [HttpPost]
        public async Task<IActionResult> SendAbandonedCartTicketEmail([FromBody] AbandonedCartRsvpEmailRequest model)
        {
            //EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            //SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
            ////mailchimp integration
            //bool isMailChimpEnabled = eventDAL.IsMailChimpModuleAvailable(26);

            //if (isMailChimpEnabled)
            //{
            //    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(26, (int)Common.Common.SettingGroup.mailchimp);
            //    string mcAPIKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_key);
            //    string mcStore = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_store);
            //    string mcCampaign = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_listname);
            //    string mcreservationstag = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_reservationstag);
            //    string mcticketingstag = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_ticketingstag);

            //    if (!string.IsNullOrWhiteSpace(mcAPIKey) && !string.IsNullOrWhiteSpace(mcStore))
            //    {
            //        //call routine and pass data
            //        MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, mcCampaign, mcreservationstag, mcticketingstag);
            //       await mailChimpAPI.CheckAndCreateTag(mcreservationstag,"ramessh@cellarpass.com");
            //        await mailChimpAPI.CheckAndCreateMember("ramessh@cellarpass.com");
            //    }
            //}

            var abandonedCartRsvpEmailResponse = new AbandonedCartRsvpEmailResponse();
            try
            {
                AuthMessageSender messageService = new AuthMessageSender();

                foreach (var item in model.ids)
                {
                    QueueService getStarted = new QueueService();

                    var queueModel = new EmailQueue();
                    queueModel.EType = (int)EmailType.AbanondedCartTickets;
                    queueModel.RsvpId = item;
                    queueModel.Src = (int)ActionSource.BackOffice;
                    var qData = JsonConvert.SerializeObject(queueModel);

                    AppSettings _appsettings = _appSetting.Value;
                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                }

                abandonedCartRsvpEmailResponse.success = true;
            }
            catch (Exception ex)
            {
                abandonedCartRsvpEmailResponse.success = false;
                abandonedCartRsvpEmailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                abandonedCartRsvpEmailResponse.error_info.extra_info = Common.Common.InternalServerError;
                abandonedCartRsvpEmailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SendAbandonedCartTicketEmail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(abandonedCartRsvpEmailResponse);
        }

        [Route("checkandsendticketsabandonedfornotifications")]
        [HttpGet]
        public IActionResult CheckAndSendTicketsAbandonedForNotifications()
        {
            var abandonedCartRsvpEmailResponse = new AbandonedCartRsvpEmailResponse();
            try
            {
                AuthMessageSender messageService = new AuthMessageSender();
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                List<int> ids = eventDAL.GetAvailableTicketsAbandonedForNotifications();
                if (ids != null)
                {
                    foreach (var item in ids)
                    {
                        QueueService getStarted = new QueueService();

                        var queueModel = new EmailQueue();
                        queueModel.EType = (int)EmailType.AbanondedCartTickets;
                        queueModel.RsvpId = item;
                        queueModel.Src = (int)ActionSource.BackOffice;
                        var qData = JsonConvert.SerializeObject(queueModel);

                        AppSettings _appsettings = _appSetting.Value;
                        getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                    }
                }

                abandonedCartRsvpEmailResponse.success = true;
            }
            catch (Exception ex)
            {
                abandonedCartRsvpEmailResponse.success = false;
                abandonedCartRsvpEmailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                abandonedCartRsvpEmailResponse.error_info.extra_info = Common.Common.InternalServerError;
                abandonedCartRsvpEmailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SendAbandonedCartTicketEmail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(abandonedCartRsvpEmailResponse);
        }

        [Route("ticketevents")]
        [HttpGet]
        public IActionResult GetTicketEventsByWineryId(int member_id, int event_type)
        {
            //event_type >> All: 0, TicketedEvent: 1, PassportEvent: 2
            var ticketEventResponse = new TicketEventResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<TicketEventModel> ticts = ticketDAL.GetTicketEventsByWineryId(member_id, event_type);

                if (ticts != null && ticts.Count > 0)
                {
                    ticketEventResponse.success = true;
                    ticketEventResponse.data = ticts;
                }
                else
                {
                    ticketEventResponse.success = true;
                    ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                ticketEventResponse.success = false;
                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketEventsByWineryId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(ticketEventResponse);
        }

        [Route("profileticketevents")]
        [HttpGet]
        public IActionResult GetProfileTicketEventsByWineryId(int member_id)
        {
            //event_type >> All: 0, TicketedEvent: 1, PassportEvent: 2
            var ticketEventResponse = new ProfileTicketEventResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<ProfileTicketEventModel> ticts = ticketDAL.GetProfileTicketEventsByWineryId(member_id);

                if (ticts != null && ticts.Count > 0)
                {
                    ticketEventResponse.success = true;
                    ticketEventResponse.data = ticts;
                }
                else
                {
                    ticketEventResponse.success = true;
                    ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                ticketEventResponse.success = false;
                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketEventsByWineryId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(ticketEventResponse);
        }

        [Route("geteventlandingpage")]
        [HttpGet]
        public IActionResult GetTicketEventsLandingPageDetails(int event_id, int user_id=0, string event_password = "", Guid guest_guid = default(Guid), bool bypass_check = false)
        {
            //event_type >> All: 0, TicketedEvent: 1, PassportEvent: 2
            var ticketEventResponse = new TicketEventLandingPageResponse();
            int member_id = 0;
            try
            {
                if (event_id <= 0)
                {
                    ticketEventResponse.success = true;
                    ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventResponse.error_info.extra_info = "no record found";
                }
                else
                {
                    string eventPassword = string.Empty;
                    bool guestlistAccepted = false;
                    bool passwordAccepted = false;
                    TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                    TicketEventLandingPageModel ticketEvent = ticketDAL.GetTicketEventLandingPageData(event_id, user_id, ref eventPassword);

                    if (ticketEvent != null)
                    {
                        member_id = ticketEvent.business_id;

                        EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                        WineryReviews wineryReviews = ticketDAL.GetTicketEventReviews(ticketEvent.id);

                        ticketEvent.total_reviews = wineryReviews.ReviewCount;
                        ticketEvent.review_stars = wineryReviews.ReviewStars;
                        ticketEvent.avg_review_value = wineryReviews.avg_review_value;

                        ticketEvent.event_organizer_phone = Utility.FormatTelephoneNumber(ticketEvent.event_organizer_phone, "US");
                        if (ticketEvent.requires_invite && !bypass_check)
                        {
                            if (guest_guid != default(Guid) && ticketEvent.guest_lists.Length > 0)
                            {
                                guestlistAccepted = ticketDAL.GetGuestlistIdById(guest_guid, ticketEvent.guest_lists);
                            }

                            if (!guestlistAccepted)
                            {
                                ticketEventResponse.success = false;
                                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.EventError;
                                ticketEventResponse.error_info.extra_info = "This event is invite-only.";
                                ticketEventResponse.error_info.description = "This event is invite-only.";
                            }
                        }
                        else
                            guestlistAccepted = true;

                        if (ticketEvent.requires_password && !bypass_check)
                        {
                            if (string.IsNullOrEmpty(event_password))
                            {
                                ticketEventResponse.success = false;
                                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.EventError;
                                ticketEventResponse.error_info.extra_info = "This event is password-protected.";
                                ticketEventResponse.error_info.description = "This event is password-protected.";
                            }
                            else if (eventPassword != event_password)
                            {
                                ticketEventResponse.success = false;
                                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.EventError;
                                ticketEventResponse.error_info.extra_info = "The password you entered is not valid for this event.";
                                ticketEventResponse.error_info.description = "The password you entered is not valid for this event.";
                            }
                            else
                                passwordAccepted = true;
                        }
                        else
                            passwordAccepted = true;


                        if (guestlistAccepted && passwordAccepted)
                        {
                            ticketEventResponse.success = true;
                            ticketEventResponse.data = ticketEvent;
                        }
                    }
                    else
                    {
                        ticketEventResponse.success = true;
                        ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        ticketEventResponse.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                ticketEventResponse.success = false;
                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketEventsByWineryId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(ticketEventResponse);
        }


        [Route("ticketeventdetails")]
        [HttpGet]
        public IActionResult GetTicketEventsDetails(int event_id, string event_password = "", Guid guest_guid = default(Guid), bool bypass_check = false)
        {
            //event_type >> All: 0, TicketedEvent: 1, PassportEvent: 2
            var ticketEventResponse = new TicketEventDetailResponse();
            int member_id = 0;
            try
            {
                if (event_id <= 0)
                {
                    ticketEventResponse.success = true;
                    ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventResponse.error_info.extra_info = "no record found";
                }
                else
                {
                    string eventPassword = string.Empty;
                    bool guestlistAccepted = false;
                    bool passwordAccepted = false;
                    TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                    TicketEventDetailModel ticketEvent = ticketDAL.GetTicketEventDetailsById(event_id, ref eventPassword);

                    if (ticketEvent != null)
                    {
                        member_id = ticketEvent.member_id;

                        WineryReviews wineryReviews = ticketDAL.GetTicketEventReviews(ticketEvent.id);

                        ticketEvent.total_reviews = wineryReviews.ReviewCount;
                        ticketEvent.review_stars = wineryReviews.ReviewStars;
                        ticketEvent.avg_review_value = wineryReviews.avg_review_value;

                        ticketEvent.event_organizer_phone = Utility.FormatTelephoneNumber(ticketEvent.event_organizer_phone, "US");

                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString_readonly);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.member);
                        string MarketingOptin = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_Marketing_Opt_in);

                        ticketEvent.marketing_optin_text = MarketingOptin;
                        ticketEvent.show_marketing_optin = MarketingOptin.Length > 0;

                        if (ticketEvent.requires_invite && !bypass_check)
                        {
                            if (guest_guid != default(Guid) && ticketEvent.guest_lists.Length > 0)
                            {
                                guestlistAccepted = ticketDAL.GetGuestlistIdById(guest_guid, ticketEvent.guest_lists);
                            }

                            if (!guestlistAccepted)
                            {
                                ticketEventResponse.success = false;
                                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.EventError;
                                ticketEventResponse.error_info.extra_info = "This event is invite-only.";
                                ticketEventResponse.error_info.description = "This event is invite-only.";
                            }
                        }
                        else
                            guestlistAccepted = true;

                        if (ticketEvent.requires_password && !bypass_check)
                        {
                            if (string.IsNullOrEmpty(event_password))
                            {
                                ticketEventResponse.success = false;
                                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.EventError;
                                ticketEventResponse.error_info.extra_info = "This event is password-protected.";
                                ticketEventResponse.error_info.description = "This event is password-protected.";
                            }
                            else if (eventPassword != event_password)
                            {
                                ticketEventResponse.success = false;
                                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.EventError;
                                ticketEventResponse.error_info.extra_info = "The password you entered is not valid for this event.";
                                ticketEventResponse.error_info.description = "The password you entered is not valid for this event.";
                            }
                            else
                                passwordAccepted = true;
                        }
                        else
                            passwordAccepted = true;


                        if (guestlistAccepted && passwordAccepted)
                        {
                            ticketEventResponse.success = true;
                            ticketEventResponse.data = ticketEvent;
                        }
                        else
                        {
                            var tEvent = new TicketEventDetailModel();
                            tEvent.requires_invite = ticketEvent.requires_invite;
                            tEvent.requires_password = ticketEvent.requires_password;

                            ticketEventResponse.data = tEvent;
                        }
                    }
                    else
                    {
                        ticketEventResponse.success = true;
                        ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        ticketEventResponse.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                ticketEventResponse.success = false;
                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketEventsByWineryId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(ticketEventResponse);
        }

        [Route("passporteventdetails")]
        [HttpGet]
        public IActionResult GetPassportEventsDetails(int event_id)
        {
            //event_type >> All: 0, TicketedEvent: 1, PassportEvent: 2
            var ticketEventResponse = new PassportEventDetailResponse();
            int member_id = 0;
            try
            {
                if (event_id <= 0)
                {
                    ticketEventResponse.success = true;
                    ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventResponse.error_info.extra_info = "no record found";
                }
                else
                {

                    TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                    PassportEventDetailModel ticketEvent = ticketDAL.GetPassportEventDetailsById(event_id);

                    if (ticketEvent != null)
                    {
                        member_id = ticketEvent.member_id;

                        ticketEventResponse.success = true;
                        ticketEventResponse.data = ticketEvent;
                    }
                    else
                    {
                        ticketEventResponse.success = true;
                        ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        ticketEventResponse.error_info.extra_info = "no record found";
                    }


                }
            }
            catch (Exception ex)
            {
                ticketEventResponse.success = false;
                ticketEventResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketEventsByWineryId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(ticketEventResponse);
        }

        [Route("ticketlevelsbyevent")]
        [HttpGet]
        public async Task<IActionResult> GetTicketLevelsByEventId(int event_id, string accessCode = "", string promo_code = "", int user_id = 0,int discount_type = 0, int discount_id=0,string email_address = "")
        {
            var ticketResponse = new TicketLevelResponse();
            int member_id = 0;
            int max_tickets_per_order = 0;
            try
            {
                int EventRemainingQty = 0;

                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<TicketLevelModel> ticts = ticketDAL.GetTicketLevelsByEventId(event_id, IsAdmin, accessCode, ref EventRemainingQty);

                if (ticts != null && ticts.Count > 0)
                {
                    ticketResponse.success = true;

                    List<TicketLevelModel> newticts = new List<TicketLevelModel>();
                    TicketDiscount tktDisc = null;
                    List<TicketDiscount> listtktDisc = null;

                    Common.Common.TicketsServiceFeesOption svcFeeOption = ticketDAL.GetEventServiceFeeMode(event_id);
                    member_id = ticts[0].member_id;
                    max_tickets_per_order = ticts[0].max_tickets_per_order;
                    var ticketPlan = ticketDAL.GetTicketPlanForMember(ticts[0].member_id);
                    decimal serviceFee = 0, perTicketFee = 0, maxTicketFee = 0;
                    serviceFee = ticketPlan.service_fee;
                    perTicketFee = ticketPlan.per_ticket_fee;
                    maxTicketFee = ticketPlan.max_ticket_fee;
                    //processingFee = ticketPlan.processing_fee;

                    if (user_id > 0 && string.IsNullOrWhiteSpace(email_address))
                    {
                        UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                        email_address = userDAL.GetUserEmailById(user_id);
                    }

                    if (!string.IsNullOrWhiteSpace(promo_code.Trim()) || (discount_id > 0 && (user_id > 0 || !string.IsNullOrWhiteSpace(email_address))))
                    {
                        tktDisc = ticketDAL.GetTicketDiscountByCodeWithUseCount(promo_code.Trim(), event_id, discount_id, discount_type);
                        foreach (var ticketLevel in ticts)
                        {
                            if (ticketLevel.ticket_id <= 0)
                                continue;

                            decimal price = ticketLevel.price;
                            decimal origPrice = ticketLevel.original_price;
                            bool promoUseExceeded = false;
                            bool promoQtyInvalid = false;
                            bool promoApplied = false;
                            bool promo_code_valid = false;
                            bool automated_discount_valid = false;
                            bool automated_discountApplied = false;
                            int qty = 1;

                            if (origPrice == 0 && price > 0)
                                origPrice = price;

                            if (discount_type == 0)
                            {
                                ticketLevel.promo_code_msg = string.Format("'{0}' Promo Code Invalid, Discount was not applied.", promo_code.Trim());
                            }
                            else
                            {
                                ticketLevel.promo_code_msg = string.Format("Automated discount Invalid, Discount was not applied.");
                            }
                            

                            if (qty > 0 && tktDisc != null && tktDisc.id > 0)
                            {
                                CalculateDiscountModel calculateDiscountModel = new CalculateDiscountModel();
                                calculateDiscountModel = await CalculateDiscount(tktDisc, qty, ticketLevel.original_price, price, ticketLevel.ticket_id, user_id,email_address);

                                price = calculateDiscountModel.discount_price;
                                promoUseExceeded = calculateDiscountModel.useExceeded;
                                promoQtyInvalid = calculateDiscountModel.qtyInvalid;
                                promoApplied = calculateDiscountModel.promoApplied;
                                promo_code_valid = calculateDiscountModel.promo_code_valid;
                                automated_discount_valid = calculateDiscountModel.automated_discount_valid;
                                automated_discountApplied = calculateDiscountModel.automated_discountApplied;

                                if (promo_code_valid)
                                {
                                    if (origPrice > price)
                                    {
                                        ticketLevel.promo_code_msg = string.Format("Promo Code '{0}' valid. Discount will be calculated during.", promo_code.Trim());

                                        //if (tktDisc.min_qty_reqd == tktDisc.max_per_order)
                                        //    ticketLevel.promo_code_msg = string.Format("'{0}' Promo Code Valid, Discount will be applied after ticket selection (limited to {1} guests).", promo_code.Trim(), tktDisc.min_qty_reqd, tktDisc.max_per_order);
                                        //else
                                        //    ticketLevel.promo_code_msg = string.Format("'{0}' Promo Code Valid, Discount will be applied after ticket selection (limited to {1}-{2} guests).", promo_code.Trim(), tktDisc.min_qty_reqd, tktDisc.max_per_order);
                                    }
                                    else
                                        ticketLevel.promo_code_msg = "";
                                }

                                if (automated_discount_valid)
                                {
                                    if (origPrice > price)
                                    {
                                        if (tktDisc.min_qty_reqd == tktDisc.max_per_order)
                                            ticketLevel.promo_code_msg = string.Format("Automated discount Valid, Discount will be applied after ticket selection (limited to {1} guests).", promo_code.Trim(), tktDisc.min_qty_reqd, tktDisc.max_per_order);
                                        else
                                            ticketLevel.promo_code_msg = string.Format("Automated discount Valid, Discount will be applied after ticket selection (limited to {1}-{2} guests).", promo_code.Trim(), tktDisc.min_qty_reqd, tktDisc.max_per_order);
                                    }
                                    else
                                        ticketLevel.promo_code_msg = "";
                                }
                            }

                            ticketLevel.promo_code_valid = (promo_code_valid || automated_discount_valid);

                            if (promoQtyInvalid && (!promoApplied || automated_discountApplied))
                            {
                                price = origPrice;
                            }

                            ticketLevel.price = price;
                            ticketLevel.ticket_fee = 0;

                            decimal gratuity = 0;

                            if (ticketLevel.gratuity_percentage > 0)
                                gratuity = Utility.CalculateGratuity(price, ticketLevel.gratuity_percentage);

                            if ((price > 0 || gratuity > 0) && ticketLevel.ticket_type == Common.Common.TicketType.Ticket)
                            {
                                if ((svcFeeOption == Common.Common.TicketsServiceFeesOption.Ticketholder) | (svcFeeOption == Common.Common.TicketsServiceFeesOption.TicketHolderPlusCCProcessing))
                                {
                                    ticketLevel.ticket_fee = Utility.CalculateFeeTotal(price, serviceFee, perTicketFee, maxTicketFee, gratuity);
                                }
                                else
                                    ticketLevel.ticket_fee = 0;
                            }
                            else
                                ticketLevel.ticket_fee = 0;

                            newticts.Add(ticketLevel);
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(promo_code.Trim()) && discount_id == 0 && (user_id > 0 || !string.IsNullOrWhiteSpace(email_address)))
                    {
                        listtktDisc = ticketDAL.GetTicketDiscountByEventId(event_id);
                        foreach (var ticketLevel in ticts)
                        {
                            if (ticketLevel.ticket_id <= 0)
                                continue;

                            decimal price = ticketLevel.price;
                            decimal origPrice = ticketLevel.original_price;
                            bool promoUseExceeded = false;
                            bool promoQtyInvalid = false;
                            bool promoApplied = false;
                            bool promo_code_valid = false;
                            bool automated_discount_valid = false;
                            bool automated_discountApplied = false;
                            int qty = 1;

                            if (origPrice == 0 && price > 0)
                                origPrice = price;

                            if (discount_type == 0)
                            {
                                ticketLevel.promo_code_msg = string.Format("'{0}' Promo Code Invalid, Discount was not applied.", promo_code.Trim());
                            }
                            else
                            {
                                ticketLevel.promo_code_msg = string.Format("Automated discount Invalid, Discount was not applied.");
                            }


                            if (qty > 0 && listtktDisc != null && listtktDisc.Count > 0)
                            {
                                foreach (var item in listtktDisc)
                                {
                                    decimal discount_price = 0;
                                    CalculateDiscountModel calculateDiscountModel = new CalculateDiscountModel();
                                    calculateDiscountModel = await CalculateDiscount(item, qty, ticketLevel.original_price, price, ticketLevel.ticket_id, user_id, email_address);

                                    if ((((int)(calculateDiscountModel.discount_price * 100)) > ((int)(discount_price * 100))) && calculateDiscountModel.automated_discount_valid)
                                    {
                                        discount_price = calculateDiscountModel.discount_price;

                                        price = calculateDiscountModel.discount_price;
                                        promoUseExceeded = calculateDiscountModel.useExceeded;
                                        promoQtyInvalid = calculateDiscountModel.qtyInvalid;
                                        promoApplied = calculateDiscountModel.promoApplied;
                                        promo_code_valid = calculateDiscountModel.promo_code_valid;
                                        automated_discount_valid = calculateDiscountModel.automated_discount_valid;
                                        automated_discountApplied = calculateDiscountModel.automated_discountApplied;

                                        if (promo_code_valid)
                                        {
                                            if (origPrice > price)
                                            {
                                                ticketLevel.promo_code_msg = string.Format("Promo Code '{0}' valid. Discount will be calculated during.", promo_code.Trim());
                                                //if (item.min_qty_reqd == item.max_per_order)
                                                //    ticketLevel.promo_code_msg = string.Format("'{0}' Promo Code Valid, Discount will be applied after ticket selection (limited to {1} guests).", promo_code.Trim(), item.min_qty_reqd, item.max_per_order);
                                                //else
                                                //    ticketLevel.promo_code_msg = string.Format("'{0}' Promo Code Valid, Discount will be applied after ticket selection (limited to {1}-{2} guests).", promo_code.Trim(), item.min_qty_reqd, item.max_per_order);
                                            }
                                            else
                                                ticketLevel.promo_code_msg = "";
                                        }

                                        if (automated_discount_valid)
                                        {
                                            if (origPrice > price)
                                            {
                                                if (item.min_qty_reqd == item.max_per_order)
                                                    ticketLevel.promo_code_msg = string.Format("Automated discount Valid, Discount will be applied after ticket selection (limited to {1} guests).", promo_code.Trim(), item.min_qty_reqd, item.max_per_order);
                                                else
                                                    ticketLevel.promo_code_msg = string.Format("Automated discount Valid, Discount will be applied after ticket selection (limited to {1}-{2} guests).", promo_code.Trim(), item.min_qty_reqd, item.max_per_order);
                                            }
                                            else
                                                ticketLevel.promo_code_msg = "";
                                        }
                                    }
                                }
                            }

                            ticketLevel.promo_code_valid = (promo_code_valid || automated_discount_valid);

                            if (promoQtyInvalid && (!promoApplied || automated_discountApplied))
                            {
                                price = origPrice;
                            }

                            ticketLevel.price = price;
                            ticketLevel.ticket_fee = 0;

                            decimal gratuity = 0;

                            if (ticketLevel.gratuity_percentage > 0)
                                gratuity = Utility.CalculateGratuity(price, ticketLevel.gratuity_percentage);

                            if ((price > 0 || gratuity > 0) && ticketLevel.ticket_type == Common.Common.TicketType.Ticket)
                            {
                                if ((svcFeeOption == Common.Common.TicketsServiceFeesOption.Ticketholder) | (svcFeeOption == Common.Common.TicketsServiceFeesOption.TicketHolderPlusCCProcessing))
                                {
                                    ticketLevel.ticket_fee = Utility.CalculateFeeTotal(price, serviceFee, perTicketFee, maxTicketFee, gratuity);
                                }
                                else
                                    ticketLevel.ticket_fee = 0;
                            }
                            else
                                ticketLevel.ticket_fee = 0;

                            newticts.Add(ticketLevel);
                        }
                    }
                    else
                        newticts = ticts;

                    ticketResponse.data.event_remaining_qty = EventRemainingQty;
                    //ticketResponse.data.event_will_call_location_details = ticketDAL.GetEventWillCallLocationByEventId(event_id);
                    ticketResponse.data.ticket_levels = newticts;
                }
                else
                {
                    ticketResponse.success = true;
                    ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                ticketResponse.success = false;
                ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketLevelsByEventId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(ticketResponse);
        }

        [Route("ticketeventmetrics")]
        [HttpGet]
        public IActionResult GetTicketEventMetricsByEventId(int event_id)
        {
            var ticketEventMetricsResponse = new TicketEventMetricsResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                TicketEventMetrics ticts = ticketDAL.GetTicketEventMetricsByEventId(event_id);

                if (ticts != null)
                {
                    ticketEventMetricsResponse.success = true;
                    ticketEventMetricsResponse.data = ticts;
                }
                else
                {
                    ticketEventMetricsResponse.success = true;
                    ticketEventMetricsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventMetricsResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                ticketEventMetricsResponse.success = false;
                ticketEventMetricsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventMetricsResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventMetricsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketEventMetricsByEventId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketEventMetricsResponse);
        }

        [Route("ticketpassporteventmetrics")]
        [HttpGet]
        public IActionResult GetTicketPassportEventMetricsByEventId(int event_id)
        {
            var ticketEventPassportMetricsResponse = new TicketEventPassportMetricsResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                TicketPassportEventMetrics ticts = ticketDAL.GetTicketPassportEventMetricsByEventId(event_id);

                if (ticts != null)
                {
                    ticketEventPassportMetricsResponse.success = true;
                    ticketEventPassportMetricsResponse.data = ticts;
                }
                else
                {
                    ticketEventPassportMetricsResponse.success = true;
                    ticketEventPassportMetricsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventPassportMetricsResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                ticketEventPassportMetricsResponse.success = false;
                ticketEventPassportMetricsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventPassportMetricsResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventPassportMetricsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketPassportEventMetricsByEventId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketEventPassportMetricsResponse);
        }

        [Route("ticketsbyevent")]
        [HttpGet]
        public IActionResult GetTicketsByEventId(int event_id)
        {
            var ticketResponse = new TicketResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<TicketModel> ticts = ticketDAL.GetTicketsByEventId(event_id);

                if (ticts != null && ticts.Count > 0)
                {
                    ticketResponse.success = true;
                    ticketResponse.data = ticts;
                }
                else
                {
                    ticketResponse.success = true;
                    ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                ticketResponse.success = false;
                ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketsByEventId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketResponse);
        }

        [Route("passporteventtickets")]
        [HttpGet]
        public IActionResult GetTicketsForPassportEvents(int event_id, int member_id)
        {
            var ticketResponse = new TicketResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<TicketModel> ticts = ticketDAL.GetTicketsByPassportEventIdAndMember(event_id, member_id);

                if (ticts != null && ticts.Count > 0)
                {
                    ticketResponse.success = true;
                    ticketResponse.data = ticts;
                }
                else
                {
                    ticketResponse.success = true;
                    ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                ticketResponse.success = false;
                ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketsByEventId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(ticketResponse);
        }

        [Route("checkintickets")]
        [HttpPost]
        public async Task<IActionResult> CheckInTicket([FromBody] CheckInTicketRequest reqmodel)
        {
            int member_id = 0;
            var checkInTicketResponse = new CheckInTicketResponse();
            try
            {

                if (reqmodel.event_id > 0 && reqmodel.barcode.Length > 0) //Validate parameters
                {
                    string strTicketId = Common.Common.Right(reqmodel.barcode.Trim(), 8).TrimStart('0');

                    int ticketId = 0;
                    Int32.TryParse(strTicketId, out ticketId);

                    TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                    //Get Ticket info
                    TicketModel ticket = ticketDAL.GetTicketById(ticketId);
                    if (ticket != null && ticket.ticket_id > 0)
                    {

                        //Do Check In
                        bool multiCheckInAllowed = false;
                        bool ticketIsValidForEvent = false;
                        bool ticketIsValidForDate = false;

                        Common.Common.AttendeeAppCheckInMode checkInMode = ticketDAL.GetEventCheckInMode(ticket.event_id);

                        if (checkInMode == Common.Common.AttendeeAppCheckInMode.standardMulti || checkInMode == Common.Common.AttendeeAppCheckInMode.barcodeMulti || checkInMode == Common.Common.AttendeeAppCheckInMode.searchMulti || checkInMode == Common.Common.AttendeeAppCheckInMode.multiEvent)
                            multiCheckInAllowed = true;
                        else
                            multiCheckInAllowed = false;

                        //Make Sure Ticket is Valid for Event being used at.
                        if (ticket.event_id == reqmodel.event_id) //Change this to passed event id on next push
                            ticketIsValidForEvent = true;
                        else if (reqmodel.is_multi_event == true)
                        {
                            //see if ticket is in group and validate
                        }

                        //reqmodel.is_multi_event = true;
                        //TODO
                        //        Else
                        //'see if ticket is in group and validate
                        //If Tickets.ValidateTicketForGroupedEvents(eventId, ticket.event_id) = True Then
                        //    ticketIsValidForEvent = True
                        //End If

                        //Get Event Time Zone
                        Times.TimeZone eventTimeZone = ticketDAL.GetEventTimeZone(reqmodel.event_id);

                        //Set Event Time
                        DateTime currentEventDate = Times.ToTimeZoneTime(DateTime.UtcNow, eventTimeZone);

                        //Make Sure Date on Ticket is Valid
                        if (ticket.valid_start_date.Date == ticket.valid_end_date.Date && currentEventDate.Date >= ticket.valid_start_date.Date && currentEventDate.Date.AddDays(-1) <= ticket.valid_end_date.Date)
                            ticketIsValidForDate = true;
                        else if (currentEventDate.Date >= ticket.valid_start_date.Date && currentEventDate.Date <= ticket.valid_end_date.Date)
                            ticketIsValidForDate = true;

                        if (((ticket.ticket_status == Common.Common.TicketStatus.ACTIVE) || (multiCheckInAllowed == true && ticket.ticket_status == Common.Common.TicketStatus.CLAIMED)) && ticketIsValidForEvent == true && ticketIsValidForDate == true && ticket.ticket_post_capture_status != Common.Common.TicketPostCaptureStatus.Invited)
                        {
                            if (reqmodel.is_test == false)
                            {
                                if (ticketDAL.CheckInTicket(ticket.ticket_id) > 0)
                                {
                                    ticket.checkin_status = Common.Common.CheckInStatus.SUCCESS;
                                    ticket.ticket_status = Common.Common.TicketStatus.CLAIMED;

                                    AuthMessageSender messageService = new AuthMessageSender();
                                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                                    var emailPromo = eventDAL.SendCheckInPromo(ticket.ticket_order_id, 1);

                                    try
                                    {
                                        if (emailPromo.ToEmail.Trim().Length > 0)
                                        {
                                            emailPromo.MailConfig.ApiKey = _appSetting.Value.MainGunApiKey;
                                            emailPromo.MailConfig.Domain = _appSetting.Value.MailGunPostUrl;
                                            messageService.ProcessCheckInPromoEmail(emailPromo, ticket.ticket_order_id);
                                        }
                                    }
                                    catch { }
                                }
                                else
                                {
                                    ticket.checkin_status = Common.Common.CheckInStatus.FAILED;
                                }
                            }
                            else
                            {
                                ticket.checkin_status = Common.Common.CheckInStatus.TEST;
                            }


                        }
                        else
                        {
                            if (ticket.ticket_post_capture_status == Common.Common.TicketPostCaptureStatus.Invited)
                                ticket.checkin_status = Common.Common.CheckInStatus.NOT_ALLOWED_INVITED_TICKET;
                            else if (ticketIsValidForEvent == false)
                                ticket.checkin_status = Common.Common.CheckInStatus.NOT_ALLOWED_BAD_EVENT;
                            else if (ticketIsValidForDate == false)
                                ticket.checkin_status = Common.Common.CheckInStatus.NOT_ALLOWED_BAD_DATE;
                            else
                                ticket.checkin_status = Common.Common.CheckInStatus.NOT_ALLOWED;
                        }
                        checkInTicketResponse.success = true;
                        checkInTicketResponse.data = ticket;
                    }
                    else
                    {
                        checkInTicketResponse.success = true;
                        checkInTicketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        checkInTicketResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    checkInTicketResponse.success = true;
                    checkInTicketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    checkInTicketResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                checkInTicketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                checkInTicketResponse.error_info.extra_info = Common.Common.InternalServerError;
                checkInTicketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CheckInTicket:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(checkInTicketResponse);
        }

        [Route("checkinpassportticket")]
        [HttpPost]
        public async Task<IActionResult> PassportCheckInTicket([FromBody] PassportCheckInTicketRequest reqmodel)
        {
            var checkInTicketResponse = new CheckInTicketResponse();
            try
            {

                if (reqmodel.event_id > 0 && reqmodel.member_id > 0 && reqmodel.barcode.Length > 0) //Validate parameters
                {
                    string strTicketId = Common.Common.Right(reqmodel.barcode.Trim(), 8).TrimStart('0');

                    int ticketId = 0;
                    Int32.TryParse(strTicketId, out ticketId);

                    TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                    //Get Ticket info
                    TicketModel ticket = ticketDAL.GetTicketById(ticketId);
                    if (ticket != null && ticket.ticket_id > 0)
                    {
                        //Do Check In
                        bool alreadyCheckedIn = false;
                        bool ticketIsValidForEvent = false;
                        bool ticketIsValidForDate = false;

                        //Check for previous checkin at this member
                        PassportCheckIn prevCheckIn = ticketDAL.GetPassportCheckIn(ticketId, reqmodel.member_id);

                        if (prevCheckIn != null && prevCheckIn.id > 0)
                        {
                            alreadyCheckedIn = true;
                        }

                        //Make Sure Ticket is Valid for Event being used at.
                        if (ticket.event_id == reqmodel.event_id) //Change this to passed event id on next push
                            ticketIsValidForEvent = true;


                        //Get Event Time Zone
                        Times.TimeZone eventTimeZone = ticketDAL.GetEventTimeZone(reqmodel.event_id);

                        //Set Event Time
                        DateTime currentEventDate = Times.ToTimeZoneTime(DateTime.UtcNow, eventTimeZone);

                        //Make Sure Date on Ticket is Valid
                        if (currentEventDate.Date >= ticket.valid_start_date.Date && currentEventDate.Date <= ticket.valid_end_date.Date)
                            ticketIsValidForDate = true;

                        if (ticket.ticket_status != Common.Common.TicketStatus.INVALID && alreadyCheckedIn == false && ticketIsValidForEvent == true && ticketIsValidForDate == true && ticket.ticket_post_capture_status != Common.Common.TicketPostCaptureStatus.Invited)
                        {
                            if (reqmodel.is_test == false)
                            {
                                if (ticketDAL.CheckInPassportTicket(ticket.ticket_id, reqmodel.member_id) > 0)
                                {
                                    ticket.checkin_status = Common.Common.CheckInStatus.SUCCESS;
                                    ticket.ticket_status = Common.Common.TicketStatus.CLAIMED;

                                    AuthMessageSender messageService = new AuthMessageSender();
                                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                                    var emailPromo = eventDAL.SendCheckInPromo(ticket.ticket_order_id, 1);

                                    try
                                    {
                                        if (emailPromo.ToEmail.Trim().Length > 0)
                                        {
                                            emailPromo.MailConfig.ApiKey = _appSetting.Value.MainGunApiKey;
                                            emailPromo.MailConfig.Domain = _appSetting.Value.MailGunPostUrl;
                                            messageService.ProcessCheckInPromoEmail(emailPromo, ticket.ticket_order_id);
                                        }
                                    }
                                    catch { }
                                }
                                else
                                {
                                    ticket.checkin_status = Common.Common.CheckInStatus.FAILED;
                                }
                            }
                            else
                            {
                                ticket.checkin_status = Common.Common.CheckInStatus.TEST;
                            }
                        }
                        else
                        {
                            if (ticket.ticket_post_capture_status == Common.Common.TicketPostCaptureStatus.Invited)
                                ticket.checkin_status = Common.Common.CheckInStatus.NOT_ALLOWED_INVITED_TICKET;
                            else if (ticketIsValidForEvent == false)
                                ticket.checkin_status = Common.Common.CheckInStatus.NOT_ALLOWED_BAD_EVENT;
                            else if (ticketIsValidForDate == false)
                                ticket.checkin_status = Common.Common.CheckInStatus.NOT_ALLOWED_BAD_DATE;
                            else
                                ticket.checkin_status = Common.Common.CheckInStatus.NOT_ALLOWED;
                        }

                        checkInTicketResponse.success = true;
                        checkInTicketResponse.data = ticket;
                    }
                    else
                    {
                        checkInTicketResponse.success = true;
                        checkInTicketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        checkInTicketResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    checkInTicketResponse.success = true;
                    checkInTicketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    checkInTicketResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                checkInTicketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                checkInTicketResponse.error_info.extra_info = Common.Common.InternalServerError;
                checkInTicketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "checkinpassportticket:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(checkInTicketResponse);
        }


        [Route("tixordercalculation")]
        [HttpPost]
        public async Task<IActionResult> TixOrderCalculation([FromBody] TixOrderCalculationRequest reqmodel)
        {
            var tixOrderCalculationResponse = new TixOrderCalculationResponse();
            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

            if (reqmodel.member_id <= 0 || reqmodel.event_id <= 0 || reqmodel.ticket_levels.Count == 0)
            {
                tixOrderCalculationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                tixOrderCalculationResponse.error_info.extra_info = "Invalid data sent in request";
                tixOrderCalculationResponse.error_info.description = "Ticket event Id and/or ticket Id invalid";
                return new ObjectResult(tixOrderCalculationResponse);
            }
            else if (reqmodel.bypass_check == false)
            {
                try
                {
                    string eventPassword = string.Empty;

                    TicketEventDetailModel ticketEvent = ticketDAL.GetTicketEventDetailsById(reqmodel.event_id, ref eventPassword);

                    DateTime localDateTime = Times.ToTimeZoneTime(DateTime.UtcNow, ticketEvent.timezone);

                    if (ticketEvent.status == TicketsEventStatus.DRAFT || ticketEvent.status == TicketsEventStatus.CANCELLED || ticketEvent.status == TicketsEventStatus.SOLDOUT || ticketEvent.end_date < localDateTime)
                    {
                        tixOrderCalculationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        tixOrderCalculationResponse.error_info.extra_info = "Event Unavailable";
                        tixOrderCalculationResponse.error_info.description = "Event is not active and cannot accept any booking.";
                        return new ObjectResult(tixOrderCalculationResponse);
                    }

                    int requestQty = reqmodel.ticket_levels.Sum(f => f.quantity);

                    if (requestQty > ticketEvent.event_remaining_qty)
                    {
                        tixOrderCalculationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        tixOrderCalculationResponse.error_info.extra_info = "The requested quantity is not available.";
                        tixOrderCalculationResponse.error_info.description = "The requested quantity is not available.";
                        return new ObjectResult(tixOrderCalculationResponse);
                    }
                }
                catch { }
            }
            try
            {
                //get winery plan
                var ticketPlan = ticketDAL.GetTicketPlanForMember(reqmodel.member_id);
                if (ticketPlan == null)
                {
                    tixOrderCalculationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoTicketingPlan;
                    tixOrderCalculationResponse.error_info.extra_info = "";
                    tixOrderCalculationResponse.error_info.description = "No active ticketing plan found for the member Id " + reqmodel.member_id.ToString();
                    return new ObjectResult(tixOrderCalculationResponse);
                }

                if (reqmodel.user_id > 0 && string.IsNullOrWhiteSpace(reqmodel.email_address))
                {
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                    reqmodel.email_address = userDAL.GetUserEmailById(reqmodel.user_id);
                }

                var model = await CalculateTax(reqmodel, ticketPlan);

                if ((reqmodel.cart_guid == null || reqmodel.cart_guid == default(Guid)) && !string.IsNullOrWhiteSpace(reqmodel.email_address))
                {
                    if (!string.IsNullOrEmpty(reqmodel.email_address))
                    {
                        Guid CartGUID = Guid.NewGuid();
                        ticketDAL.SaveTicketsAbandoned(reqmodel.user_id, reqmodel.email_address, reqmodel.event_id, reqmodel.member_id, "", reqmodel.discount_code, CartGUID);

                        model.cart_guid = CartGUID;
                    }
                }

                tixOrderCalculationResponse.success = true;
                tixOrderCalculationResponse.data = model;

            }
            catch (Exception ex)
            {
                tixOrderCalculationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                tixOrderCalculationResponse.error_info.extra_info = Common.Common.InternalServerError;
                tixOrderCalculationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "TixOrderCalculation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);
            }
            return new ObjectResult(tixOrderCalculationResponse);
        }

        [Route("saveticketorder")]
        [HttpPost]
        public async Task<IActionResult> SaveTicketOrder([FromBody] SaveTicketRequest reqmodel)
        {
            var saveTicketOrderResponse = new SaveTicketOrderResponse();

            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            if (reqmodel == null)
            {
                saveTicketOrderResponse.success = false;
                saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                saveTicketOrderResponse.error_info.extra_info = "No data or invalid data passed in request. ";
                saveTicketOrderResponse.error_info.description = "There was problem processing your request. Kindly retry again or contact support.";
                return new ObjectResult(saveTicketOrderResponse);
            }

            logDAL.InsertLog("WebApi", "SaveTicketOrder:  request data:" + JsonConvert.SerializeObject(reqmodel), HttpContext.Request.Headers["AuthenticateKey"], 3, reqmodel.member_id);

            if (reqmodel.member_id <= 0 || reqmodel.event_id <= 0 || reqmodel.ticket_levels.Count == 0)
            {
                saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                saveTicketOrderResponse.error_info.extra_info = "Invalid data sent in request";
                saveTicketOrderResponse.error_info.description = "Ticket event Id and/or ticket Id invalid";
                return new ObjectResult(saveTicketOrderResponse);
            }

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                string eventPassword = string.Empty;

                TicketEventDetailModel ticketEvent = ticketDAL.GetTicketEventDetailsById(reqmodel.event_id, ref eventPassword);

                DateTime localDateTime = Times.ToTimeZoneTime(DateTime.UtcNow, ticketEvent.timezone);

                if (ticketEvent.status == TicketsEventStatus.DRAFT || ticketEvent.status == TicketsEventStatus.CANCELLED || ticketEvent.status == TicketsEventStatus.SOLDOUT || ticketEvent.end_date < localDateTime)
                {
                    saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    saveTicketOrderResponse.error_info.extra_info = "Event Unavailable";
                    saveTicketOrderResponse.error_info.description = "Event is not active and cannot accept any booking.";
                    return new ObjectResult(saveTicketOrderResponse);
                }

                var willCallAllocationList = new List<WillCallAllocation>();

                int ticketholderscount = 0;
                foreach (var ticketlevels in reqmodel.ticket_levels)
                {
                    //Get Ticket Will Call Limitations (if any) with current allocation
                    List<TicketWillCallLocation> twc = ticketDAL.GetTicketWillCallLocations(ticketlevels.ticket_id);

                    //Get Allocation if Any
                    List<WillCallAllocation> allocatedList = ticketDAL.GetTicketsAllocatedByWillCallLocation(ticketlevels.ticket_id);

                    if (twc != null && twc.Count > 0)
                    {
                        bool unavailable = false;
                        int totalQty = 0;

                        List<TicketHolder> holders = new List<TicketHolder>();

                        holders = reqmodel.ticket_holders.Skip(ticketholderscount).Take(ticketlevels.quantity).ToList();

                        for (int i = 0; i < ticketlevels.quantity - 1; i++)
                        {
                            TicketHolder ticket_holder = new TicketHolder();
                            if (reqmodel.ticket_holders.Count > ticketholderscount)
                            {
                                ticket_holder = reqmodel.ticket_holders[ticketholderscount];
                            }

                            if (!string.IsNullOrEmpty(ticket_holder.will_call_location_id))
                            {
                                totalQty = holders.Where(f => f.will_call_location_id == ticket_holder.will_call_location_id).Count();

                                TicketWillCallLocation matched = twc.Where(f => f.Location_Id == Convert.ToInt32(ticket_holder.will_call_location_id)).FirstOrDefault();

                                if (matched != null && matched.Id > 0)
                                {
                                    int allocatedCount = 0;

                                    if (allocatedList != null && allocatedList.Count > 0)
                                    {
                                        allocatedCount = allocatedList.Where(f => f.LocationId == Convert.ToInt32(ticket_holder.will_call_location_id)).Select(f => f.Allocated).FirstOrDefault();
                                    }

                                    if (matched.WillCallLimit > 0)
                                    {
                                        int remainingCount = 0;

                                        remainingCount = matched.WillCallLimit - allocatedCount;

                                        if (totalQty > remainingCount)
                                        {
                                            unavailable = true;
                                            break;
                                        }
                                    }
                                    else if (matched.WillCallLimit == 0)
                                    {
                                        unavailable = true;
                                        break;
                                    }
                                }
                            }
                            ticketholderscount += 1;
                        }

                        if (unavailable)
                        {
                            saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.WillCallUnavailable;
                            saveTicketOrderResponse.error_info.extra_info = "Will Call Unavailable";
                            saveTicketOrderResponse.error_info.description = "Sorry the number of tickets available for the will call location selected are no longer available. Please select a different location.";
                            return new ObjectResult(saveTicketOrderResponse);
                        }
                    }
                }

                var listLocationId = reqmodel.ticket_holders.DistinctBy(p => p.will_call_location_id).Select(p => p.will_call_location_id).ToList();
                if (listLocationId != null)
                {
                    List<EventWillCallLocation> list = ticketDAL.GetEventWillCallLocationByEventId(reqmodel.event_id);
                    if (list.Count > 0)
                    {
                        foreach (var item in listLocationId)
                        {
                            if (!string.IsNullOrEmpty(item) && item != "0")
                            {
                                int totalQty = reqmodel.ticket_holders.Where(f => f.will_call_location_id == item).Count();

                                int avlQty = list.Where(f => f.location_id == Convert.ToInt32(item)).Select(f => f.available_qty).FirstOrDefault();

                                if (totalQty > avlQty)
                                {
                                    saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.WillCallUnavailable;
                                    saveTicketOrderResponse.error_info.extra_info = "Will Call Unavailable";
                                    saveTicketOrderResponse.error_info.description = "Sorry the number of tickets available for the will call location selected are no longer available. Please select a different location.";
                                    return new ObjectResult(saveTicketOrderResponse);
                                }
                            }
                            
                        }
                    }
                    
                }

                var listwcLocation = reqmodel.ticket_holders.Where(f => f.delivery_type == 0).ToList();
                if (listwcLocation != null)
                {
                    foreach (var item in listwcLocation)
                    {
                        if (string.IsNullOrEmpty(item.will_call_location_id) || item.will_call_location_id == "0")
                        {
                            saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.WillCallUnavailable;
                            saveTicketOrderResponse.error_info.extra_info = "Will Call Unavailable";
                            saveTicketOrderResponse.error_info.description = "Sorry the number of tickets available for the will call location selected are no longer available. Please select a different location.";
                            return new ObjectResult(saveTicketOrderResponse);
                        }
                    }
                }

                //get winery plan
                var ticketPlan = ticketDAL.GetTicketPlanForMember(reqmodel.member_id);
                if (ticketPlan == null)
                {
                    saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.NoTicketingPlan;
                    saveTicketOrderResponse.error_info.extra_info = "";
                    saveTicketOrderResponse.error_info.description = "No active ticketing plan found for the member Id " + reqmodel.member_id.ToString();
                    return new ObjectResult(saveTicketOrderResponse);
                }

                //step 1: check if a user exists
                bool userExists = CheckUserExists(ref reqmodel);
                var userDAL = new UserDAL(Common.Common.ConnectionString);
                int mobilePhoneStatus = 0;
                //if not create auser
                if (!userExists)
                {

                    string guestPwd = StringHelpers.GenerateRandomString(8, false);
                    if (!string.IsNullOrWhiteSpace(reqmodel.home_phone))
                        mobilePhoneStatus = (int)Utility.SMSVerified_System(reqmodel.home_phone);
                    else
                    {
                        mobilePhoneStatus = 0;
                        reqmodel.home_phone = "";
                    }
                    reqmodel.user_id = userDAL.CreateUser(reqmodel.email_address, guestPwd, reqmodel.first_name, reqmodel.last_name, reqmodel.country, reqmodel.zip, reqmodel.home_phone, (int)Common.Common.UserType.Guest, mobilePhoneStatus, 0, reqmodel.cust_id, reqmodel.city, reqmodel.state, reqmodel.address_1, reqmodel.address_2, Common.Common.GetSource(HttpContext.Request.Headers["AuthenticateKey"]));

                    userDAL.UpdateUserWinery(reqmodel.user_id, reqmodel.member_id, (int)Common.Common.UserRole.Guest, "", "", "", (int)Common.Common.UserType.Guest);

                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(reqmodel.cust_id))
                    {
                        userDAL.UpdateGatewayCustId(reqmodel.user_id, reqmodel.cust_id);
                    }
                    mobilePhoneStatus = (int)Utility.SMSVerified_System(reqmodel.home_phone);
                    //reqmodel.user_id = userDAL.CreateUser(reqmodel.email_address, guestPwd, reqmodel.first_name, reqmodel.last_name, reqmodel.country, reqmodel.zip, reqmodel.home_phone, (int)Common.Common.UserType.Guest, mobilePhoneStatus);
                    if ((userDAL.UpdateUserWinery(reqmodel.member_id, reqmodel.user_id, "", "", (int)Common.Common.UserType.Guest)) == 0)
                    {
                        userDAL.UpdateUserWinery(reqmodel.user_id, reqmodel.member_id, (int)Common.Common.UserRole.Guest, "", "", "", (int)Common.Common.UserType.Guest);
                    }

                }

                var userDetailModel = new UserDetailModel
                {
                    email = reqmodel.email_address,
                    first_name = reqmodel.first_name,
                    last_name = reqmodel.last_name,
                    user_id = reqmodel.user_id,
                    phone_number = Utility.FormatTelephoneNumber(reqmodel.home_phone + "", reqmodel.country + ""),
                    mobile_number_status = mobilePhoneStatus,
                    address = new Model.UserAddress
                    {
                        address_1 = reqmodel.address_1 + "",
                        address_2 = reqmodel.address_2 + "",
                        city = reqmodel.city + "",
                        state = reqmodel.state + "",
                        country = reqmodel.country + "",
                        zip_code = reqmodel.zip + ""
                    }


                };



                //if user could not be created for some reason then quit the process
                if (reqmodel.user_id <= 0)
                {
                    saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.CreatingUser;
                    saveTicketOrderResponse.error_info.extra_info = "Error Creating User";
                    saveTicketOrderResponse.error_info.description = "The user could not be loaded or created. Please try again or contact CellarPass.";
                    return new ObjectResult(saveTicketOrderResponse);
                }

                if (!string.IsNullOrEmpty(reqmodel.discount_code))
                {
                    bool DiscountCodeValid = ticketDAL.CheckDiscountCodeValid(reqmodel.event_id, reqmodel.discount_code, reqmodel.user_id,reqmodel.discount_id);

                    if (DiscountCodeValid == false)
                    {
                        saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidPromoCode;
                        saveTicketOrderResponse.error_info.extra_info = "Error Promo Code";
                        saveTicketOrderResponse.error_info.description = "Promo use exceeded";
                        return new ObjectResult(saveTicketOrderResponse);
                    }
                }

                if (!string.IsNullOrWhiteSpace(reqmodel.access_code))
                {
                    int qty = 0;
                    foreach (var item in reqmodel.ticket_levels)
                    {
                        TicketLevelModel tict = ticketDAL.GetTicketLevelsById(item.ticket_id);

                        if (tict.sale_status == Common.Common.TicketsSaleStatus.Hidden)
                            qty += item.quantity;
                    }

                    bool isAccessCodeValid = ticketDAL.CheckTicketAccessCode(reqmodel.event_id, reqmodel.access_code, reqmodel.user_id, qty);
                    if (!isAccessCodeValid)
                    {
                        saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.TicketAcessCodeInvalid;
                        saveTicketOrderResponse.error_info.extra_info = "Invalid Ticket Access code";
                        saveTicketOrderResponse.error_info.description = string.Format("The '{0}' access code is no longer available", reqmodel.access_code);
                        return new ObjectResult(saveTicketOrderResponse);
                    }
                }

                //check if qty still available
                int qtyAvailable = ticketDAL.GetTotalAvailableTicketQty(reqmodel.event_id);
                int waitListQty = 0;
                if (!string.IsNullOrWhiteSpace(reqmodel.wait_list_guid))
                {
                    //get waitlist details
                    var waitListDetails = ticketDAL.GetTicketsEventByWaitlistId(0, reqmodel.wait_list_guid);
                    waitListQty = waitListDetails.qty_offered;
                    if (waitListDetails.status == Common.Common.TicketWaitlistStatus.Expired)
                        waitListQty = 0;
                }
                if (qtyAvailable == 0 && waitListQty == 0)
                {
                    saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.QuantityNotAvailable;
                    saveTicketOrderResponse.error_info.extra_info = "Event Sold out";
                    saveTicketOrderResponse.error_info.description = "The tickets, ticket quantity or date and time you've requested are no longer available, due to previous sales. Please choose a different date, time or number of tickets and place your order again.";
                    return new ObjectResult(saveTicketOrderResponse);
                }

                // do the calculation of tax, discount and totals
                TixOrderCalculationRequest taxCalcReq = new TixOrderCalculationRequest
                {
                    discount_code = reqmodel.discount_code,
                    event_id = reqmodel.event_id,
                    member_id = reqmodel.member_id,
                    ticket_levels = reqmodel.ticket_levels,
                    discount_id=reqmodel.discount_id,
                    discount_type=reqmodel.discount_type,
                    email_address=reqmodel.email_address,
                    user_id=reqmodel.user_id
                };

                if (!string.IsNullOrEmpty(reqmodel.pay_card_number))
                {
                    string cardnumber = reqmodel.pay_card_number.Replace(" ", "").Replace("-", "");

                    string cardtype = Services.Payments.GetCardType(cardnumber);
                    taxCalcReq.card_type = cardtype;
                }

                var model = await CalculateTax(taxCalcReq, ticketPlan);

                //if (model.ticket_level_discounts.Count > 0)
                //{
                //    foreach (TicketLevelDiscount ticketleveldiscount in model.ticket_level_discounts)
                //    {
                //        foreach (TicketLevelForTax ticketLevel in reqmodel.ticket_levels)
                //        {
                //            if (ticketleveldiscount.ticket_id == ticketLevel.ticket_id && ticketLevel.price == ticketLevel.original_price)
                //            {
                //                ticketLevel.price = ((ticketLevel.original_price * ticketLevel.quantity) - ticketleveldiscount.discount_amount) / ticketLevel.quantity;
                //            }
                //        }
                //    }
                //}
                //get event data
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var paymentProcessor = eventDAL.GetTicketPaymentProcessorByWinery(reqmodel.member_id);

                string pay_type = "0";

                if (!string.IsNullOrEmpty(reqmodel.pay_type))
                    pay_type = reqmodel.pay_type;

                if (model.grand_total > 0 && pay_type == "0")
                {
                    if (paymentProcessor == Common.Common.TicketsPaymentProcessor.CellarPassProcessor && (string.IsNullOrEmpty(reqmodel.pay_card_number) || string.IsNullOrEmpty(reqmodel.pay_card_custName)) && string.IsNullOrEmpty(reqmodel.tokenized_card))
                    {
                        saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                        saveTicketOrderResponse.error_info.extra_info = "";
                        saveTicketOrderResponse.error_info.description = "Invalid credit card information";
                        return new ObjectResult(saveTicketOrderResponse);
                    }
                    else if (paymentProcessor == Common.Common.TicketsPaymentProcessor.Stripe && string.IsNullOrEmpty(reqmodel.tokenized_card))
                    {
                        saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                        saveTicketOrderResponse.error_info.extra_info = "";
                        saveTicketOrderResponse.error_info.description = "Invalid tokenized card";
                        return new ObjectResult(saveTicketOrderResponse);
                    }
                }


                //Check if duplicate order
                if (ticketDAL.CheckDuplicateOrder(reqmodel.event_id, reqmodel.user_id, model.grand_total))
                {
                    saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.DuplicateTicket;
                    saveTicketOrderResponse.error_info.extra_info = "Duplicate Order found";
                    saveTicketOrderResponse.error_info.description = "Duplicate ticket order found with same event, user and amount";
                    return new ObjectResult(saveTicketOrderResponse);
                }

                

                try
                {
                    int requestQty = reqmodel.ticket_levels.Sum(f => f.quantity);

                    if (requestQty > ticketEvent.event_remaining_qty || requestQty == 0)
                    {
                        saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        saveTicketOrderResponse.error_info.extra_info = "The requested quantity is not available.";
                        saveTicketOrderResponse.error_info.description = "The requested quantity is not available.";
                        return new ObjectResult(saveTicketOrderResponse);
                    }
                }
                catch { }

                Guid orderGuid = Guid.NewGuid();

                if (string.IsNullOrEmpty(reqmodel.booked_by_login_name))
                {
                    reqmodel.booked_by_id = reqmodel.user_id;
                    reqmodel.booked_by_login_name = reqmodel.email_address;
                }

                int Id = ticketDAL.SaveTicketOrder(reqmodel, model, ticketPlan, paymentProcessor, mobilePhoneStatus, ticketEvent, orderGuid);

                if (Id > 0)
                {
                    if (reqmodel.cart_guid != default(Guid))
                    {
                        ticketDAL.ConvertAbandonedCart(reqmodel.cart_guid, reqmodel.user_id, reqmodel.email_address, Id);
                    }
                    else
                    {
                        ticketDAL.DeleteTicketsAbandoned(reqmodel.email_address, reqmodel.event_id);
                    }

                    Common.Payments.TransactionResult result = new Common.Payments.TransactionResult();
                    Common.Payments.CreditCard card = new Common.Payments.CreditCard();

                    if (model.grand_total > 0)
                    {
                        Common.Payments.Transaction trans = new Common.Payments.Transaction();

                        trans.ProcessedBy = reqmodel.user_id;
                        trans.Amount = model.grand_total;
                        trans.Type = Common.Payments.Transaction.ChargeType.Sale;
                        trans.Transactions = Common.Payments.Transaction.TransactionType.TicketSale;

                        Common.Payments.UserDetails user = new Common.Payments.UserDetails();
                        user.FirstName = reqmodel.first_name;
                        user.LastName = reqmodel.last_name;
                        user.HomePhoneStr = reqmodel.home_phone;
                        user.Email = reqmodel.email_address;
                        user.ZipCode = reqmodel.zip;
                        user.City = reqmodel.city;
                        user.State = reqmodel.state;
                        user.Country = reqmodel.country;
                        user.Address1 = reqmodel.address_1;
                        user.Address2 = reqmodel.address_2;

                        trans.User = user;

                        card.Number = reqmodel.pay_card_number;
                        card.CustName = reqmodel.pay_card_custName;
                        card.ExpMonth = reqmodel.pay_card_exp_month;
                        card.ExpYear = reqmodel.pay_card_exp_year;
                        card.CVV = reqmodel.pay_card_cvv;
                        card.CardToken = reqmodel.tokenized_card;
                        card.CardLastFourDigits = reqmodel.pay_card_last_four_digits;
                        //card.CardFirstFourDigits = reqmodel.pay_card_first_four_digits;
                        card.Type = reqmodel.pay_card_type;
                        //Set Card
                        trans.Card = card;
                        trans.WineryId = reqmodel.member_id;

                        if (pay_type == "0")
                        {
                            int wid = -1;
                            if (paymentProcessor != Common.Common.TicketsPaymentProcessor.CellarPassProcessor)
                                wid = reqmodel.member_id;

                            if (paymentProcessor == Common.Common.TicketsPaymentProcessor.Stripe)
                            {
                                var order = ticketDAL.GetTicketOrderById(Id);
                                Services.Payments objPayments = new Services.Payments(_appSetting);
                                Services.Stripe obj = new Services.Stripe(_appSetting);
                                result = Services.Stripe.ChargeStripe(order, model, _appSetting.Value.StripeSecretKey, reqmodel.tokenized_card, trans);
                            }
                            else
                            {
                                Services.Payments objPayments = new Services.Payments(_appSetting);
                                result = await Services.Payments.ProcessPaymentV2(wid, Id, trans, reqmodel.save_card);
                            }
                        }
                        else
                        {
                            result.Amount = model.grand_total;
                            result.ApprovalCode = "";
                            result.AvsResponse = "";
                            result.Detail = "";

                            CreditCard Card = new CreditCard();
                            Card.CustName = "";
                            Card.ExpMonth = "";
                            Card.ExpYear = "";
                            Card.Number = "";
                            Card.Type = "";

                            result.Card = Card;
                            if ((reqmodel.pay_check_number + "").Length > 0 && pay_type == "2")
                            {
                                result.PayType = PaymentType.Check;
                                result.CheckOrRefNumber = reqmodel.pay_check_number;
                            }
                            else
                            {
                                decimal decChange =Convert.ToDecimal(reqmodel.pay_amount_received) - model.grand_total;
                                result.PayType = PaymentType.Cash;
                                result.Change = decChange;
                            }

                            result.ResponseCode = "0";
                            result.Status = Common.Payments.TransactionResult.StatusType.Success;
                            result.TransactionID = "";
                            result.TransactionType = Transaction.ChargeType.Sale;
                            result.PaymentGateway = 0;

                            ticketDAL.TicketsOrderPaymentInsert(Id, result);

                            result.Status = Common.Payments.TransactionResult.StatusType.Success;
                            result.ApprovalCode = "0";
                            result.TransactionID = "";
                            result.Detail = "NA";
                        }
                    }
                    else
                    {
                        result.Status = Common.Payments.TransactionResult.StatusType.Success;
                        result.ApprovalCode = "0";
                        result.TransactionID = "";
                        result.Detail = "NA";
                    }

                    if (result.Status == Common.Payments.TransactionResult.StatusType.Success)
                    {
                        if (!string.IsNullOrEmpty(reqmodel.wait_list_guid) && waitListQty > 0)
                        {
                            ticketDAL.ConvertWaitlistToTicket(reqmodel.wait_list_guid, Id, waitListQty, Common.Common.TicketWaitlistStatus.Purchased);
                        }

                        string OrderPaymentDetail = string.Format("Approval: {0} - {1}", result.ApprovalCode, result.Detail);
                        ticketDAL.UpdateTicketsOrder(Id, OrderPaymentDetail, result.TransactionID, reqmodel.itinerary_id);
                        int count = 0;

                        Common.Common.TicketPostCaptureStatus PostCaptureStatus = Common.Common.TicketPostCaptureStatus.NA;
                        if (ticketEvent.post_capture_config.show_age_group || ticketEvent.post_capture_config.show_company || ticketEvent.post_capture_config.show_dob || ticketEvent.post_capture_config.show_email || ticketEvent.post_capture_config.show_firstname || ticketEvent.post_capture_config.show_gender || ticketEvent.post_capture_config.show_lastname || ticketEvent.post_capture_config.show_mobilephone || ticketEvent.post_capture_config.show_title || ticketEvent.post_capture_config.show_workphone || ticketEvent.post_capture_config.show_zipcode || ticketEvent.post_capture_config.require_address || ticketEvent.post_capture_config.require_zipcode || ticketEvent.post_capture_config.require_age || ticketEvent.post_capture_config.require_age_group || ticketEvent.post_capture_config.require_company || ticketEvent.post_capture_config.require_dob || ticketEvent.post_capture_config.require_email || ticketEvent.post_capture_config.require_firstname || ticketEvent.post_capture_config.require_lastname || ticketEvent.post_capture_config.require_gender || ticketEvent.post_capture_config.require_mobilephone || ticketEvent.post_capture_config.require_title || ticketEvent.post_capture_config.require_website || ticketEvent.post_capture_config.require_workphone)
                        {
                            PostCaptureStatus = Common.Common.TicketPostCaptureStatus.Available;
                        }

                        try
                        {
                            List<TicketLevelForTax> ticket_levels = new List<TicketLevelForTax>();
                            List<string> tictIds = new List<string>();

                            if (model.ticket_level_discounts.Count > 0)
                            {
                                foreach (TicketLevelDiscount ticketleveldiscount in model.ticket_level_discounts)
                                {
                                    foreach (TicketLevelForTax ticketLevel in reqmodel.ticket_levels)
                                    {
                                        if (ticketleveldiscount.ticket_id == ticketLevel.ticket_id && ticketleveldiscount.applied_discount_qty > 0)
                                        {
                                            tictIds.Add(ticketleveldiscount.ticket_id.ToString());
                                            TicketLevelForTax newtl = new TicketLevelForTax();

                                            newtl.charge_tax = ticketLevel.charge_tax;
                                            newtl.gratuity_percentage = ticketLevel.gratuity_percentage;
                                            newtl.original_price = ticketLevel.original_price;
                                            newtl.price = (ticketLevel.original_price - (ticketleveldiscount.discount_amount / ticketleveldiscount.applied_discount_qty));
                                            newtl.quantity = ticketleveldiscount.applied_discount_qty;
                                            newtl.tax_gratuity = ticketLevel.tax_gratuity;
                                            newtl.ticket_id = ticketLevel.ticket_id;
                                            newtl.ticket_type = ticketLevel.ticket_type;

                                            ticket_levels.Add(newtl);

                                            if (ticketLevel.quantity > ticketleveldiscount.applied_discount_qty)
                                            {
                                                TicketLevelForTax newtl1 = new TicketLevelForTax();
                                                newtl1.charge_tax = ticketLevel.charge_tax;
                                                newtl1.gratuity_percentage = ticketLevel.gratuity_percentage;
                                                newtl1.original_price = ticketLevel.original_price;
                                                newtl1.price = ticketLevel.original_price;
                                                newtl1.quantity = ticketLevel.quantity - ticketleveldiscount.applied_discount_qty;
                                                newtl1.tax_gratuity = ticketLevel.tax_gratuity;
                                                newtl1.ticket_id = ticketLevel.ticket_id;
                                                newtl1.ticket_type = ticketLevel.ticket_type;

                                                ticket_levels.Add(newtl1);
                                            }
                                        }
                                    }
                                }

                                List<TicketLevelForTax> ticketlevels = reqmodel.ticket_levels.Where(c => !tictIds.Contains(c.ticket_id.ToString())).ToList();

                                if (ticketlevels != null && ticketlevels.Count > 0)
                                {
                                    ticket_levels.AddRange(ticketlevels);
                                }
                            }
                            else
                            {
                                ticket_levels = reqmodel.ticket_levels;
                            }

                            foreach (var item in ticket_levels)
                            {
                                TicketLevelModel tict = ticketDAL.GetTicketLevelsById(item.ticket_id);

                                //tict.price = item.price;

                                decimal itemGratuity = Utility.CalculateGratuity(item.price, item.gratuity_percentage);
                                decimal TicketFee = 0;

                                if ((item.price > 0 || itemGratuity > 0) && item.ticket_type == Common.Common.TicketType.Ticket && (ticketEvent.service_fee_option == Common.Common.TicketsServiceFeesOption.Ticketholder || ticketEvent.service_fee_option == Common.Common.TicketsServiceFeesOption.TicketHolderPlusCCProcessing))
                                {
                                    TicketFee = Utility.CalculateFeeTotal(item.price, ticketPlan.service_fee, ticketPlan.per_ticket_fee, ticketPlan.max_ticket_fee, itemGratuity);
                                }

                                if (item.ticket_type == Common.Common.TicketType.Donation)
                                {
                                    TicketHolder ticket_holder = new TicketHolder();
                                    ticket_holder.first_name = reqmodel.first_name;
                                    ticket_holder.last_name = reqmodel.last_name;
                                    ticket_holder.email = reqmodel.email_address;
                                    ticket_holder.country = reqmodel.country;

                                    ticketPlan.service_fee = 0;
                                    ticketPlan.per_ticket_fee = 0;

                                    ticketDAL.TicketsOrderTicketsInsert(item, Id, ticket_holder, tict, itemGratuity, model.sales_tax_percent, ticketPlan, TicketFee, model.discount_code, (int)PostCaptureStatus, reqmodel.access_code);
                                }
                                else
                                {
                                    TicketHolder ticket_holder = new TicketHolder();
                                    if (reqmodel.ticket_holders.Count > count)
                                    {
                                        ticket_holder = reqmodel.ticket_holders[count];
                                    }

                                    if (item.quantity > 0)
                                        ticketDAL.TicketsOrderTicketsInsert(item, Id, ticket_holder, tict, itemGratuity, model.sales_tax_percent, ticketPlan, TicketFee, model.discount_code, (int)PostCaptureStatus, reqmodel.access_code);

                                    count += 1;
                                    for (int i = 1; i < item.quantity; i++)
                                    {
                                        TicketHolder holder = new TicketHolder();
                                        if (reqmodel.ticket_holders.Count > count)
                                        {
                                            holder = reqmodel.ticket_holders[count];
                                        }
                                        ticketDAL.TicketsOrderTicketsInsert(item, Id, holder, tict, itemGratuity, model.sales_tax_percent, ticketPlan, TicketFee, model.discount_code, (int)PostCaptureStatus, reqmodel.access_code);
                                        count += 1;
                                    }
                                }
                            }

                            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                            //mailchimp integration
                            if (reqmodel.subscribe_marketing_optin || reqmodel.cellarPass_marketing_optin)
                            {
                                bool isMailChimpEnabled = eventDAL.IsMailChimpModuleAvailable(reqmodel.member_id);

                                if (isMailChimpEnabled)
                                {
                                    if (reqmodel.subscribe_marketing_optin)
                                    {
                                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(reqmodel.member_id, (int)Common.Common.SettingGroup.mailchimp);
                                        string mcAPIKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_key);
                                        string mcStore = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_store);
                                        string mcCampaign = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_listname);
                                        string mcreservationstag = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_reservationstag);
                                        string mcticketingstag = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_ticketingstag);
                                        string mcrsvpListId = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_rsvplistid);
                                        string mcticketListId = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_ticketlistid);

                                        if (!string.IsNullOrWhiteSpace(mcAPIKey) && !string.IsNullOrWhiteSpace(mcStore))
                                        {
                                            //call routine and pass data
                                            //MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, mcStore, mcCampaign, mcreservationstag, mcticketingstag, mcrsvpListId, mcticketListId);
                                            //mailChimpAPI.CreateTicketOrder(Id);

                                            try
                                            {
                                                QueueService getStarted = new QueueService();

                                                var queueModel = new EmailQueue();
                                                queueModel.EType = (int)EmailType.MailChimpTicketOrder;
                                                queueModel.BCode = "";
                                                queueModel.UId = reqmodel.user_id;
                                                queueModel.RsvpId = Id;
                                                queueModel.PerMsg = "";
                                                queueModel.Src = 0;
                                                var qData = JsonConvert.SerializeObject(queueModel);

                                                AppSettings _appsettings = _appSetting.Value;
                                                getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                                            }
                                            catch (Exception ex)
                                            {
                                                logDAL.InsertLog("WebApi", "SaveTicketOrder Create Mail Chimp Order:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);
                                            }
                                        }
                                    }
                                }

                                if (reqmodel.cellarPass_marketing_optin)
                                {
                                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.mailchimp_cp);
                                    string mcAPIKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_cp_key);
                                    string mcStore = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_cp_store);

                                    if (!string.IsNullOrWhiteSpace(mcAPIKey) && !string.IsNullOrWhiteSpace(mcStore))
                                    {
                                        try
                                        {
                                            QueueService getStarted = new QueueService();

                                            var queueModel = new EmailQueue();
                                            queueModel.EType = (int)EmailType.MailChimpTicketOrder;
                                            queueModel.BCode = "";
                                            queueModel.UId = 0;
                                            queueModel.RsvpId = Id;
                                            queueModel.PerMsg = "";
                                            queueModel.Src = 0;
                                            var qData = JsonConvert.SerializeObject(queueModel);

                                            AppSettings _appsettings = _appSetting.Value;
                                            getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                                        }
                                        catch (Exception ex)
                                        {
                                            logDAL.InsertLog("WebApi", "SaveTicketOrder Create Mail Chimp Order:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);
                                        }
                                    }
                                }
                            }

                            if (reqmodel.itinerary_id == 0)
                            {
                                QueueService getStarted = new QueueService();

                                var queueModel = new EmailQueue();
                                queueModel.EType = (int)EmailType.TicketSale;
                                queueModel.BCode = "";
                                queueModel.UId = 0;
                                queueModel.RsvpId = Id;
                                queueModel.PerMsg = "";
                                queueModel.Src = 0;
                                queueModel.AdminEmail = true;
                                var qData = JsonConvert.SerializeObject(queueModel);

                                AppSettings appsettings = _appSetting.Value;
                                getStarted.AddMessageIntoQueue(appsettings, qData).Wait();
                            }

                            saveTicketOrderResponse.success = true;
                            saveTicketOrderResponse.data.order_id = Id;
                            saveTicketOrderResponse.data.order_guid = orderGuid;
                            saveTicketOrderResponse.data.save_type = SaveType.Saved;

                            await Utility.SaveOrUpdateContactThirdParty(reqmodel.member_id, userDetailModel, reqmodel.referral_type, 0, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                            ticketDAL.UpdateUpcomingEventsData(reqmodel.event_id);
                        }
                        catch (Exception ex)
                        {
                            saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                            saveTicketOrderResponse.error_info.extra_info = Common.Common.InternalServerError;
                            saveTicketOrderResponse.error_info.description = ex.Message.ToString();


                            logDAL.InsertLog("WebApi", "SaveTicketOrderLevels:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);

                            if (model.grand_total > 0)
                            {
                                TicketRefundRequest request = new TicketRefundRequest();

                                request.amount = model.grand_total;
                                request.card_token = reqmodel.tokenized_card;
                                request.charge_type = Transaction.ChargeType.Void;

                                if (paymentProcessor == Common.Common.TicketsPaymentProcessor.Stripe)
                                    request.payment_gateway = Configuration.Gateway.Stripe;
                                else
                                    request.payment_gateway = Configuration.Gateway.Braintree;

                                request.pay_card_custName = reqmodel.pay_card_custName;
                                request.pay_card_exp_month = reqmodel.pay_card_exp_month;
                                request.pay_card_exp_year = reqmodel.pay_card_exp_year;
                                request.pay_card_last_four_digits = reqmodel.pay_card_last_four_digits;
                                //request.pay_card_first_four_digits = reqmodel.pay_card_first_four_digits;
                                request.pay_card_number = reqmodel.pay_card_number;
                                request.pay_card_type = reqmodel.pay_card_type;
                                request.ticket_order_id = Id;
                                request.transaction_id = result.TransactionID;

                                await RefundTicket(request);
                            }

                            ticketDAL.UpdateProblemTicketsOrder(Id);
                        }
                    }
                    else
                    {
                        ticketDAL.UpdateProblemTicketsOrder(Id);

                        string paymentMsg = string.Empty;
                        saveTicketOrderResponse.success = false;
                        saveTicketOrderResponse.error_info.extra_info = "Payment Error";
                        saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationPaymentError;

                        bool IsAdmin = Convert.ToBoolean(Request.Headers["IsAdmin"]);

                        if (IsAdmin)
                        {
                            if (!string.IsNullOrEmpty(result.Detail))
                                saveTicketOrderResponse.error_info.description = result.Detail;
                            else
                                saveTicketOrderResponse.error_info.description = "Declined credit card";
                        }
                        else
                        {
                            saveTicketOrderResponse.error_info.description = "Sorry, there was a problem processing your credit card<br>Please check your credit card and billing address information.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                saveTicketOrderResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                saveTicketOrderResponse.error_info.extra_info = Common.Common.InternalServerError;
                saveTicketOrderResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "SaveTicketOrder:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);


            }
            return new ObjectResult(saveTicketOrderResponse);
        }

        [Route("getavailablewillcalllocation")]
        [HttpPost]
        public IActionResult WillCallLocationCheck([FromBody] WillCallLocationCheckRequest reqmodel)
        {
            var willCallLocationCheckResponse = new WillCallLocationCheckResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                //var listLocationId = reqmodel.ticket_levels.DistinctBy(p => p.will_call_location_id).Select(p => p.will_call_location_id).ToList();

                //var listTicketId = reqmodel.ticket_levels.DistinctBy(p => p.ticket_id).Select(p => p.ticket_id).ToList();

                //List<WillCallAllocation> allocatedList = new List<WillCallAllocation>();

                //foreach (var itemLocationId in listLocationId)
                //{
                //    if (itemLocationId > 0)
                //    {
                //        WillCallAllocation allocatedmodel = new WillCallAllocation();

                //        int qty = reqmodel.ticket_levels.Where(f => f.will_call_location_id == itemLocationId).Count();

                //        allocatedmodel.Allocated = qty;
                //        allocatedmodel.LocationId = itemLocationId;

                //        allocatedList.Add(allocatedmodel);
                //    }
                //}

                string skipIds = ",";
                var listEventWillCallLocation = new List<EventWillCallLocation>();

                listEventWillCallLocation = ticketDAL.GetEventWillCallLocationByEventId(reqmodel.event_id);
                foreach (var item in listEventWillCallLocation)
                {
                    int qty = reqmodel.ticket_levels.Where(f => f.will_call_location_id == item.location_id).Count();

                    if (item.available_qty - qty <= 0)
                        skipIds = skipIds + item.location_id.ToString() + ",";
                }


                int EventRemainingQty = 0;

                List<TicketLevelModel> ticts = ticketDAL.GetTicketLevelsByEventId(reqmodel.event_id, false, "", ref EventRemainingQty);

                var listWillCallLocationCheck = new List<WillCallLocationCheckModel>();
                var list = new List<TicketsEventWillCallLocation>();
                foreach (var item in ticts)
                {
                    var willCallLocationCheckModel = new WillCallLocationCheckModel();
                    list = new List<TicketsEventWillCallLocation>();

                    willCallLocationCheckModel.ticket_id = item.ticket_id;
                    willCallLocationCheckModel.ticket_name = item.ticket_name;
                    if (item.will_call_location_details != null)
                    {
                        foreach (var item1 in item.will_call_location_details)
                        {
                            var model = new TicketsEventWillCallLocation();

                            int qty = reqmodel.ticket_levels.Where(f => f.will_call_location_id == item1.location_id && f.ticket_id == item1.ticket_id).Count();

                            model.available_qty = item1.available_qty - qty;
                            model.location_id = item1.location_id;
                            model.location_name = item1.location_name;
                            model.order_qty = item1.order_qty;
                            model.ticket_id = item1.ticket_id;
                            model.will_call_limit = item1.will_call_limit;

                            if (skipIds.IndexOf(item1.location_id.ToString()) == -1 && model.available_qty > 0)
                                list.Add(model);
                        }
                    }
                        
                    willCallLocationCheckModel.will_call_location_details = list;
                    listWillCallLocationCheck.Add(willCallLocationCheckModel);
                }

                willCallLocationCheckResponse.data = listWillCallLocationCheck;
                willCallLocationCheckResponse.success = true;
            }
            catch (Exception ex)
            {
                willCallLocationCheckResponse.success = false;
                willCallLocationCheckResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                willCallLocationCheckResponse.error_info.extra_info = Common.Common.InternalServerError;
                willCallLocationCheckResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "WillCallLocationCheck:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(willCallLocationCheckResponse);
        }

        [Route("getavailablewclocationsfororder")]
        [HttpGet]
        public IActionResult GetAvailablewclocationsfororder(int event_id,int ticket_qty)
        {
            var availablewclocationsfororderResponse = new AvailablewclocationsfororderResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                int EventRemainingQty = 0;

                List<TicketLevelModel> ticts = ticketDAL.GetTicketLevelsByEventId(event_id, false, "", ref EventRemainingQty);

                var list = new List<WillCallLocationDetail>();
                foreach (var item in ticts)
                {
                    //list = new List<WillCallLocationDetail>();

                    if (item.will_call_location_details != null)
                    {
                        foreach (var item1 in item.will_call_location_details)
                        {
                            var model = new WillCallLocationDetail();

                            model.location_id = item1.location_id;
                            model.location_name = item1.location_name;

                            if (item1.available_qty >= ticket_qty)
                                list.Add(model);
                        }
                    }
                }

                availablewclocationsfororderResponse.data = list.DistinctBy(p => p.location_id).ToList();
                availablewclocationsfororderResponse.success = true;
            }
            catch (Exception ex)
            {
                availablewclocationsfororderResponse.success = false;
                availablewclocationsfororderResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                availablewclocationsfororderResponse.error_info.extra_info = Common.Common.InternalServerError;
                availablewclocationsfororderResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetAvailablewclocationsfororder:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(availablewclocationsfororderResponse);
        }

        [Route("getavailablewclocationsfororderv2")]
        [HttpPost]
        public IActionResult GetAvailablewclocationsfororderv2([FromBody] Availablewclocationsfororderv2Request reqmodel)
        {
            var availablewclocationsfororderResponse = new AvailablewclocationsfororderV2Response();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);

                int EventRemainingQty = 0;

                List<TicketLevelModel> ticts = ticketDAL.GetTicketLevelsByEventId(reqmodel.event_id, true, "", ref EventRemainingQty);

                var list = new List<WillCallLocationDetail>();
                var listticketLevelWillCallLocationDetail = new List<TicketLevelWillCallLocationDetail>();

                var ticketLevelWillCallLocationDetail = new TicketLevelWillCallLocationDetail();

                if (ticts != null && ticts.Count > 0)
                {
                    foreach (var ticketleveldetails in reqmodel.ticket_level_details)
                    {
                        list = new List<WillCallLocationDetail>();
                        ticketLevelWillCallLocationDetail = new TicketLevelWillCallLocationDetail();

                        ticketLevelWillCallLocationDetail.ticket_id = ticketleveldetails.ticket_id;

                        var item = ticts.Where(t => t.ticket_id == ticketleveldetails.ticket_id).Select(t => t).FirstOrDefault();

                        if (item != null && item.will_call_location_details != null)
                        {
                            foreach (var item1 in item.will_call_location_details)
                            {
                                var model = new WillCallLocationDetail();

                                model.location_id = item1.location_id;
                                model.location_name = item1.location_name;

                                if (item1.available_qty >= ticketleveldetails.ticket_qty)
                                    list.Add(model);
                            }
                        }

                        ticketLevelWillCallLocationDetail.will_call_locations = list;
                        listticketLevelWillCallLocationDetail.Add(ticketLevelWillCallLocationDetail);
                    }
                }
                
                    
                availablewclocationsfororderResponse.data = listticketLevelWillCallLocationDetail;
                availablewclocationsfororderResponse.success = true;
            }
            catch (Exception ex)
            {
                availablewclocationsfororderResponse.success = false;
                availablewclocationsfororderResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                availablewclocationsfororderResponse.error_info.extra_info = Common.Common.InternalServerError;
                availablewclocationsfororderResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetAvailablewclocationsfororderv2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(availablewclocationsfororderResponse);
        }

        private bool CheckUserExists(ref SaveTicketRequest reqmodel)
        {
            bool existingUser = false;

            if (string.IsNullOrEmpty(reqmodel.email_address))
            {
                reqmodel.email_address = Utility.GenerateUsername(reqmodel.first_name, reqmodel.last_name);
            }
            else
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                var userDetailModel = new UserDetailModel();
                userDetailModel = userDAL.GetUserDetailsbyemail(reqmodel.email_address, 0);

                if (userDetailModel.user_id > 0)
                {
                    if (string.IsNullOrEmpty(reqmodel.first_name))
                        reqmodel.first_name = userDetailModel.first_name;

                    if (string.IsNullOrEmpty(reqmodel.last_name))
                        reqmodel.last_name = userDetailModel.last_name;

                    if (string.IsNullOrEmpty(reqmodel.city))
                        reqmodel.city = userDetailModel.address.city;

                    if (string.IsNullOrEmpty(reqmodel.state))
                        reqmodel.state = userDetailModel.address.state;

                    if (string.IsNullOrEmpty(reqmodel.home_phone))
                    {
                        reqmodel.home_phone = Utility.FormatTelephoneNumber(userDetailModel.phone_number, userDetailModel.address.country);  //userDetailModel.phone_number;
                    }

                    if (string.IsNullOrEmpty(reqmodel.country))
                        reqmodel.country = userDetailModel.address.country;

                    if (string.IsNullOrEmpty(reqmodel.zip))
                        reqmodel.zip = userDetailModel.address.zip_code;

                    reqmodel.user_id = userDetailModel.user_id;
                    existingUser = true;
                    //mobilephonestatus check
                }
            }

            return existingUser;
        }

        [Route("salestaxpercentbyeventid")]
        [HttpGet]
        public async Task<IActionResult> GetSalesTaxPercentByEventId(int event_id)
        {
            var salesTaxPercentResponse = new SalesTaxPercentResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                Utility objUtility = new Utility();
                decimal taxPercent = await objUtility.GetTicketTax(event_id, 1, 100);

                salesTaxPercentResponse.data = taxPercent;
            }
            catch (Exception ex)
            {
                salesTaxPercentResponse.success = false;
                salesTaxPercentResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                salesTaxPercentResponse.error_info.extra_info = Common.Common.InternalServerError;
                salesTaxPercentResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetSalesTaxPercentByEventId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(salesTaxPercentResponse);
        }

        private async Task<TixOrderCalculationModel> CalculateTax(TixOrderCalculationRequest reqmodel, TicketPlan ticketPlan)
        {
            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
            List<TicketLevelWillCall> willcallData = new List<TicketLevelWillCall>();

            decimal serviceFee = 0, perTicketFee = 0, maxTicketFee = 0, processingFee = 0;
            serviceFee = ticketPlan.service_fee;
            perTicketFee = ticketPlan.per_ticket_fee;
            maxTicketFee = ticketPlan.max_ticket_fee;
            

            if (reqmodel.card_type.ToLower().Contains("discover"))
            {
                processingFee = ticketPlan.discover_processing_fee;
            }
            else if (reqmodel.card_type.ToLower().Contains("american") || reqmodel.card_type.ToLower().Contains("amex"))
            {
                processingFee = ticketPlan.amex_processing_fee;
            }
            else if (reqmodel.card_type.ToLower().Contains("master") || reqmodel.card_type.ToLower().Contains("mc"))
            {
                processingFee = ticketPlan.mastercard_processing_fee;
            }
            else
            {
                processingFee = ticketPlan.visa_processing_fee;
            }

            //get tax percent
            decimal taxPercent = 0;
            int tktEventId = reqmodel.event_id;
            Utility objUtility = new Utility();
            taxPercent = await objUtility.GetTicketTax(reqmodel.event_id, 1, 100);

            //get serviceFeeMode
            Common.Common.TicketsServiceFeesOption svcFeeOption = ticketDAL.GetEventServiceFeeMode(tktEventId);

            TicketDiscount tktDisc = null;

            List<TicketDiscount> listtktDisc = null;

            if (reqmodel.discount_id != 1)
            {
                if (!string.IsNullOrWhiteSpace(reqmodel.discount_code.Trim()) || (reqmodel.discount_id > 0 && (reqmodel.user_id > 0 || !string.IsNullOrWhiteSpace(reqmodel.email_address))))
                //if (!string.IsNullOrWhiteSpace(reqmodel.discount_code.Trim()))
                {
                    tktDisc = ticketDAL.GetTicketDiscountByCodeWithUseCount(reqmodel.discount_code.Trim(), tktEventId, reqmodel.discount_id, reqmodel.discount_type);
                }
                else if (string.IsNullOrWhiteSpace(reqmodel.discount_code.Trim()) && reqmodel.discount_id == 0 && !string.IsNullOrWhiteSpace(reqmodel.email_address))
                {
                    listtktDisc = ticketDAL.GetTicketDiscountByEventId(tktEventId);
                }
            }
            
            decimal ticketTotal = 0, ticketOrigTotal = 0, feeTotal = 0, gratuityTotal = 0, taxTotal = 0;
            List<TicketLevelDiscount> ticketLevelDiscounts = new List<TicketLevelDiscount>();

            List<TicketLevelForTax> ticket_levels = reqmodel.ticket_levels.OrderByDescending(m => m.original_price).ToList();
            int applyqty = 0;

            foreach (TicketLevelForTax ticketLevel in ticket_levels)
            {
                if (ticketLevel.ticket_id <= 0)
                    continue;

                if (ticketLevel.ticket_type == Common.Common.TicketType.Donation && ticketLevel.price > 0)
                {
                    ticketLevel.quantity = 1;
                }

                var willCallLocations = ticketDAL.GetWillLocationsByTicketLevelID(ticketLevel.ticket_id, ticketLevel.quantity);
                if (willCallLocations != null && willCallLocations.Count > 0)
                {
                    willcallData.Add(new TicketLevelWillCall
                    {
                        ticket_id = ticketLevel.ticket_id,
                        will_call_locations = willCallLocations

                    });
                }

                decimal price = ticketLevel.price;
                decimal origPrice = ticketLevel.original_price;
                bool promoUseExceeded = false;
                bool promoQtyInvalid = false;
                bool promoApplied = false;
                bool promo_code_valid = false;
                bool automated_discount_valid = false;
                bool automated_discountApplied = false;  
                decimal discountamount = 0;
                int discounted_qty = 0;
                decimal discounteditemFee = 0;
                int qty = ticketLevel.quantity;

                if (origPrice == 0 && price > 0)
                    origPrice = price;

                if (qty > 0 && tktDisc != null)
                {
                    if (qty <= tktDisc.max_per_order)
                    {
                        CalculateDiscountModel calculateDiscountModel = new CalculateDiscountModel();
                        calculateDiscountModel = await CalculateDiscount(tktDisc, qty, ticketLevel.original_price, price, ticketLevel.ticket_id, reqmodel.user_id, reqmodel.email_address);

                        price = calculateDiscountModel.discount_price;
                        promoUseExceeded = calculateDiscountModel.useExceeded;
                        promoQtyInvalid = calculateDiscountModel.qtyInvalid;
                        promoApplied = calculateDiscountModel.promoApplied;
                        promo_code_valid = calculateDiscountModel.promo_code_valid;
                        automated_discount_valid = calculateDiscountModel.automated_discount_valid;
                        automated_discountApplied = calculateDiscountModel.automated_discountApplied;

                        if (price < origPrice)
                        {
                            applyqty = applyqty + qty;
                            discountamount = (origPrice - price) * qty;
                            discounted_qty = qty;
                        }

                        if (applyqty > tktDisc.max_per_order)
                        {
                            price = origPrice;
                            applyqty = applyqty - qty;
                            discountamount = 0;
                            discounted_qty = 0;
                        }
                    }
                    else
                    {
                        CalculateDiscountModel calculateDiscountModel = new CalculateDiscountModel();
                        calculateDiscountModel = await CalculateDiscount(tktDisc, 1, ticketLevel.original_price, price, ticketLevel.ticket_id, reqmodel.user_id, reqmodel.email_address);

                        price = calculateDiscountModel.discount_price;
                        promoUseExceeded = calculateDiscountModel.useExceeded;
                        promoQtyInvalid = calculateDiscountModel.qtyInvalid;
                        promoApplied = calculateDiscountModel.promoApplied;
                        promo_code_valid = calculateDiscountModel.promo_code_valid;
                        automated_discount_valid = calculateDiscountModel.automated_discount_valid;
                        automated_discountApplied = calculateDiscountModel.automated_discountApplied;

                        int disqty = 0;
                        if (price < origPrice)
                        {
                            if (tktDisc.max_per_order == 0)
                            {
                                discountamount = (origPrice - price) * qty;
                                discounted_qty = qty;
                            }
                            else
                            {
                                disqty = tktDisc.max_per_order - applyqty;
                                applyqty = applyqty + disqty;
                                discountamount = (origPrice - price) * disqty;
                                discounted_qty = disqty;
                            }
                        }

                        if (discountamount == 0)
                        {
                            price = origPrice;
                            applyqty = applyqty - disqty;
                            discountamount = 0;
                            discounted_qty = 0;
                        }
                    }
                }
                else if (qty > 0 && listtktDisc != null && listtktDisc.Count > 0)
                {
                    foreach (var item in listtktDisc)
                    {
                        decimal discount_price = 0;

                        if (qty <= item.max_per_order)
                        {
                            CalculateDiscountModel calculateDiscountModel = new CalculateDiscountModel();
                            calculateDiscountModel = await CalculateDiscount(item, qty, ticketLevel.original_price, price, ticketLevel.ticket_id, reqmodel.user_id, reqmodel.email_address);

                            if (((((int)(calculateDiscountModel.discount_price * 100)) > ((int)(discount_price * 100))) || (price > 0 && calculateDiscountModel.discount_price == 0)) && calculateDiscountModel.automated_discount_valid)
                            {
                                discount_price = calculateDiscountModel.discount_price;

                                price = calculateDiscountModel.discount_price;
                                promoUseExceeded = calculateDiscountModel.useExceeded;
                                promoQtyInvalid = calculateDiscountModel.qtyInvalid;
                                promoApplied = calculateDiscountModel.promoApplied;
                                promo_code_valid = calculateDiscountModel.promo_code_valid;
                                automated_discount_valid = calculateDiscountModel.automated_discount_valid;
                                automated_discountApplied = calculateDiscountModel.automated_discountApplied;

                                if (price < origPrice)
                                {
                                    applyqty = applyqty + qty;
                                    discountamount = (origPrice - price) * qty;
                                    discounted_qty = qty;
                                }

                                if (applyqty > item.max_per_order)
                                {
                                    price = origPrice;
                                    applyqty = applyqty - qty;
                                    discountamount = 0;
                                    discounted_qty = 0;
                                }
                            }
                        }
                        else
                        {
                            CalculateDiscountModel calculateDiscountModel = new CalculateDiscountModel();
                            calculateDiscountModel = await CalculateDiscount(item, 1, ticketLevel.original_price, price, ticketLevel.ticket_id, reqmodel.user_id, reqmodel.email_address);

                            if (((((int)(calculateDiscountModel.discount_price * 100)) > ((int)(discount_price * 100))) || (price > 0 && calculateDiscountModel.discount_price == 0)) && calculateDiscountModel.automated_discount_valid)
                            {
                                discount_price = calculateDiscountModel.discount_price;

                                price = calculateDiscountModel.discount_price;
                                promoUseExceeded = calculateDiscountModel.useExceeded;
                                promoQtyInvalid = calculateDiscountModel.qtyInvalid;
                                promoApplied = calculateDiscountModel.promoApplied;
                                promo_code_valid = calculateDiscountModel.promo_code_valid;
                                automated_discount_valid = calculateDiscountModel.automated_discount_valid;
                                automated_discountApplied = calculateDiscountModel.automated_discountApplied;

                                int disqty = 0;
                                if (price < origPrice)
                                {
                                    disqty = item.max_per_order - applyqty;
                                    applyqty = applyqty + disqty;
                                    discountamount = (origPrice - price) * disqty;
                                    discounted_qty = disqty;
                                }

                                if (discountamount == 0)
                                {
                                    price = origPrice;
                                    applyqty = applyqty - disqty;
                                    discountamount = 0;
                                    discounted_qty = 0;
                                }
                            }
                        }
                    }
                }
                //else
                //    price = origPrice;

                decimal gratuity = 0;

                if (ticketLevel.gratuity_percentage > 0)
                {
                    gratuity = Utility.CalculateGratuity((price * qty), ticketLevel.gratuity_percentage);
                }

                bool dicountapply = false;
                if (discounted_qty > 0)
                {
                    decimal discountedprice = ((discounted_qty * origPrice) - discountamount) / discounted_qty;

                    if (((discounted_qty * origPrice) == discountamount) && discountamount > 0)
                    {
                        discountedprice = origPrice;
                        dicountapply = true;
                    }

                    if (discountedprice > 0 && ticketLevel.ticket_type == Common.Common.TicketType.Ticket)
                    {
                        if ((svcFeeOption == Common.Common.TicketsServiceFeesOption.Ticketholder) | (svcFeeOption == Common.Common.TicketsServiceFeesOption.TicketHolderPlusCCProcessing))
                        {
                            discounteditemFee = (Utility.CalculateFeeTotal(discountedprice, serviceFee, perTicketFee, maxTicketFee, gratuity/ discounted_qty) * discounted_qty);
                        }
                        else
                            discounteditemFee = 0;
                    }
                    else
                        discounteditemFee = 0;

                }

                if (promoQtyInvalid && (!promoApplied || !automated_discountApplied))
                {
                    price = origPrice;
                }
                else if (promoApplied || automated_discountApplied)
                {
                    ticketLevelDiscounts.Add(new TicketLevelDiscount
                    {
                        ticket_id = ticketLevel.ticket_id,
                        discount_amount = discountamount,
                        applied_discount_qty = discounted_qty,
                        discounted_service_fees = discounteditemFee
                    });
                }

                if (discountamount == 0 && origPrice > price)
                {
                    discountamount = (origPrice - price) * qty;
                    ticketTotal += qty * price;
                    ticketOrigTotal += qty * origPrice;
                }
                else
                {
                    ticketTotal += ((qty * origPrice) - discountamount);
                    ticketOrigTotal += qty * origPrice;
                }
                //ticketTotal += ((qty * origPrice) - discountamount);
                //ticketTotal += qty * price;
                //ticketOrigTotal += qty * origPrice;

                if (qty > 0)
                    price = ((qty * origPrice) - discountamount) / qty;

                //get fees for the ticket
                decimal itemFee = 0;
                decimal itemGratuity = 0;
                if ((price > 0 || gratuity > 0) && ticketLevel.ticket_type == Common.Common.TicketType.Ticket && qty > 0)
                {
                    if ((svcFeeOption == Common.Common.TicketsServiceFeesOption.Ticketholder) | (svcFeeOption == Common.Common.TicketsServiceFeesOption.TicketHolderPlusCCProcessing))
                    {
                        if (dicountapply)
                        {
                            itemFee = (Utility.CalculateFeeTotal(origPrice, serviceFee, perTicketFee, maxTicketFee, gratuity/ qty) * qty);
                            feeTotal = feeTotal + itemFee - discounteditemFee;
                        }
                        else
                        {
                            itemFee = (Utility.CalculateFeeTotal(price, serviceFee, perTicketFee, maxTicketFee, gratuity/ qty) * qty);
                            feeTotal = feeTotal + itemFee;
                        }
                    }
                    else
                        feeTotal = 0;
                }
                else
                    feeTotal += 0;

                if (qty > 0)
                {
                    if (ticketLevel.gratuity_percentage > 0)
                    {
                        itemGratuity = Utility.CalculateGratuity((price * qty), ticketLevel.gratuity_percentage);
                        gratuityTotal += itemGratuity;
                    }
                    if (ticketLevel.charge_tax)
                    {
                        decimal taxableGratuityAmt = 0;
                        if (ticketLevel.tax_gratuity)
                            taxableGratuityAmt = itemGratuity;
                        if (taxableGratuityAmt > 0)
                            taxableGratuityAmt = (taxableGratuityAmt / qty);

                        // Tax Calculation
                        decimal taxperItem = (taxPercent * (price + taxableGratuityAmt)) / 100;

                        // Dim OneItemCalcTax As Decimal = (SalesTaxPercentage * ((price) + (ItemFee / qty) + (ItemgratuityTotal / qty))) / 100

                        //taxperItem = Math.Round(taxperItem, 2);

                        decimal calcTax = taxperItem * qty;

                        taxTotal = taxTotal + calcTax;
                    }
                    else if (ticketLevel.tax_gratuity)
                    {
                        decimal taxableGratuityAmt = itemGratuity;

                        if (taxableGratuityAmt > 0)
                            taxableGratuityAmt = (taxableGratuityAmt / qty);

                        // Tax Calculation
                        decimal taxperItem = (taxPercent * taxableGratuityAmt) / 100;

                        decimal calcTax = taxperItem * qty;

                        taxTotal = taxTotal + calcTax;
                    }
                }
            }

            taxTotal = Math.Round(taxTotal, 2, MidpointRounding.AwayFromZero);
            decimal totalWithoutProcessing = (ticketTotal + taxTotal + gratuityTotal + feeTotal); // + feeTotal

            // PROCESSING FEES - IF passed To Consumer
            decimal processingTotal = 0;
            if (svcFeeOption == Common.Common.TicketsServiceFeesOption.TicketHolderPlusCCProcessing)
            {
                // Get Total
                processingTotal = decimal.Round(totalWithoutProcessing * processingFee, 2, MidpointRounding.AwayFromZero);

            }


            // Orig Total could be less than new Total if user incresed price above normal original price
            decimal subTotal = ticketOrigTotal;
            decimal discountTotal = (ticketOrigTotal - ticketTotal);
            if (ticketOrigTotal < ticketTotal)
            {
                subTotal = ticketTotal;
                discountTotal = 0;
            }

            string discountcode = string.Empty;

            if (discountTotal > 0 || ticketLevelDiscounts.Count > 0)
            {
                discountcode = reqmodel.discount_code;
            }
            var model = new TixOrderCalculationModel
            {
                subtotal = subTotal,
                discount = discountTotal,
                service_fees = feeTotal,
                gratuity = gratuityTotal,
                sales_tax = taxTotal,
                processing_fees = processingTotal,
                grand_total = totalWithoutProcessing + processingTotal,
                sales_tax_percent = taxPercent,
                discount_code = discountcode,
                ticket_level_discounts = ticketLevelDiscounts,
                ticket_level_willcall_locations = willcallData
            };

            return model;

        }


        private async Task<CalculateDiscountModel> CalculateDiscount(TicketDiscount discount, int qty, decimal origPrice, decimal price, int ticketId, int userId, string email)
        {
            CalculateDiscountModel model = new CalculateDiscountModel();
            model.promoApplied = false;
            model.automated_discountApplied = false;

            decimal newPrice = price;

            if (discount != null)
            {
                if (discount.id > 0)
                {

                    // NumberOfUses = 0 is unlimited and is not checked.
                    //if (discount.number_of_uses > 0)
                    //{
                    //    if (discount.use_count >= discount.number_of_uses)
                    //        useExceeded = true;
                    //}
                    TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                    bool DiscountCodeValid = ticketDAL.CheckDiscountCodeValid(discount.ticket_event_id, discount.discount_code, userId,discount.id);

                    model.useExceeded = !DiscountCodeValid;
                    // Discounts
                    if (model.useExceeded == false)
                    {
                        //If discount.StartDateTime <= Date.Now And discount.EndDateTime > Date.Now And TicketId > 0 Then

                        //Times.TimeZone eventTimeZone = ticketDAL.GetEventTimeZone(reqmodel.event_id);

                        ////Set Event Time
                        //DateTime currentEventDate = Times.ToTimeZoneTime(DateTime.UtcNow, eventTimeZone);

                        DateTime localDateTime = Times.ToTimeZoneTime(DateTime.UtcNow);
                        if (discount.start_datetime <= localDateTime && discount.end_datetime >= localDateTime && ticketId > 0)
                        {
                            // if ticket id matches those available in discount then update price
                            if (discount.discount_ticket_levels.Where(f => f.ticket_id == ticketId).ToList().Count > 0)
                            {
                                if (discount.discount_type > 0)
                                {
                                    if (discount.assigned_lists.Count > 0)
                                    {
                                        if (discount.guest_type == 0)
                                        {
                                            //List Management
                                            foreach (var GuestlistId in discount.assigned_lists)
                                            {
                                                model.automated_discount_valid = ticketDAL.CheckExistsGuestlist(GuestlistId, email);

                                                if (model.automated_discount_valid)
                                                    break;
                                            }
                                        }
                                        else if(discount.guest_type == 1)
                                        {
                                            var userDetailModel = new List<UserDetailModel>();

                                            userDetailModel = await Utility.GetUsersByEmail(email, discount.wineryId);

                                            if (userDetailModel != null && userDetailModel.Count > 0)
                                            {
                                                if (userDetailModel[0].customer_type == 1)
                                                {
                                                    model.automated_discount_valid = true;
                                                }
                                            }
                                        }
                                        else if (discount.guest_type == 2)
                                        {
                                            //Integrated Partner
                                            var userDetailModel = new List<UserDetailModel>();

                                            userDetailModel = await Utility.GetUsersByEmail(email, discount.wineryId);

                                            if (userDetailModel != null && userDetailModel.Count > 0)
                                            {
                                                foreach (var GuestlistId in discount.assigned_lists)
                                                {
                                                    if (GuestlistId == 0)
                                                    {
                                                        if (userDetailModel[0].customer_type == 1)
                                                        {
                                                            model.automated_discount_valid = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        string GetThirdParty_AccountTypeName = ticketDAL.GetThirdParty_AccountTypeName(GuestlistId);

                                                        if (GetThirdParty_AccountTypeName.Length > 0)
                                                        {
                                                            foreach (var item in userDetailModel[0].contact_types)
                                                            {
                                                                if (GetThirdParty_AccountTypeName.ToLower() == item.ToLower())
                                                                {
                                                                    model.automated_discount_valid = true;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            
                                        }

                                        if (model.automated_discount_valid)
                                        {
                                            if (qty >= discount.min_qty_reqd & qty <= discount.max_per_order)
                                            {

                                                // amount based or percentage based discount
                                                if (discount.discount_amount > 0)
                                                    newPrice = (origPrice - discount.discount_amount);
                                                else
                                                    newPrice = (origPrice - (origPrice * discount.discount_percent));

                                                // When discount is more than the price set price to 0
                                                if (newPrice < 0)
                                                    newPrice = 0;

                                                // Set Applied
                                                model.automated_discountApplied = true;
                                            }
                                            else
                                                model.qtyInvalid = true;
                                        }
                                    }
                                    //else
                                    //    automated_discount_valid = false;
                                }
                                else
                                {
                                    model.promo_code_valid = true;

                                    if (qty >= discount.min_qty_reqd & qty <= discount.max_per_order)
                                    {

                                        // amount based or percentage based discount
                                        if (discount.discount_amount > 0)
                                            newPrice = (origPrice - discount.discount_amount);
                                        else
                                            newPrice = (origPrice - (origPrice * discount.discount_percent));

                                        // When discount is more than the price set price to 0
                                        if (newPrice < 0)
                                            newPrice = 0;

                                        // Set Applied
                                        model.promoApplied = true;
                                    }
                                    else
                                        model.qtyInvalid = true;
                                }
                            }
                        }
                    }
                }
            }

            model.discount_price = newPrice;
            return model;
        }

        [Route("saveticketswaitlist")]
        [HttpPost]
        public async Task<IActionResult> SaveTicketsWaitlist([FromBody] TicketsWaitlistRequest reqmodel)
        {
            var saveTicketWaitlistResponse = new SaveTicketWaitlistResponse();

            if (reqmodel.ticket_event_id <= 0 || reqmodel.ticket_ticket_id <= 0 || reqmodel.email.Length == 0)
            {
                saveTicketWaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                saveTicketWaitlistResponse.error_info.extra_info = "Invalid data sent in request";
                saveTicketWaitlistResponse.error_info.description = "Ticket event Id and/or ticket Id invalid";
                return new ObjectResult(saveTicketWaitlistResponse);
            }
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                bool isWaitlistExist = ticketDAL.CheckUserWaitListedForTicket(reqmodel.ticket_event_id, reqmodel.ticket_ticket_id, reqmodel.email);

                if (isWaitlistExist)
                {
                    saveTicketWaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.WaitListExists;
                    saveTicketWaitlistResponse.error_info.extra_info = "";
                    saveTicketWaitlistResponse.error_info.description = "You are already registered for the wait list for this ticket level.<br>Please contact the event organizer for further assistance.";
                    return new ObjectResult(saveTicketWaitlistResponse);
                }

                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                var userDetailModel = new UserDetailModel();
                userDetailModel = userDAL.GetUserDetailsbyemail(reqmodel.email, 0);

                if (userDetailModel.user_id == 0)
                {
                    string GuestPwd = StringHelpers.GenerateRandomString(8, false);
                    int UserId = userDAL.CreateUser(reqmodel.email, GuestPwd, reqmodel.first_name, reqmodel.last_name, "US", "", reqmodel.home_phone, 0, 0, 0, "", "", "", "", "", Common.Common.GetSource(HttpContext.Request.Headers["AuthenticateKey"]));
                    if (UserId > 0)
                    {
                        userDAL.UpdateUserWinery(UserId, reqmodel.member_id, 4, "", "", "", -1);
                    }
                }

                decimal home_phone = Utility.ExtractPhone(reqmodel.home_phone);

                if (home_phone == 0)
                    home_phone = Utility.ExtractPhone(userDetailModel.phone_number);

                if (home_phone == 0)
                    home_phone = Utility.ExtractPhone(userDetailModel.mobile_number);

                Guid orderGuid = Guid.NewGuid();

                int Id = ticketDAL.SaveTicketsWaitlist(reqmodel, home_phone, orderGuid);

                //if (Id > 0)
                //{
                //    QueueService getStarted = new QueueService();

                //    var queueModel = new EmailQueue();
                //    queueModel.EType = (int)EmailType.TicketWaitlistOffer;
                //    queueModel.BCode = "";
                //    queueModel.UId = 0;
                //    queueModel.RsvpId = Id;
                //    queueModel.PerMsg = "";
                //    queueModel.Src = 0;
                //    var qData = JsonConvert.SerializeObject(queueModel);

                //    AppSettings _appsettings = _appSetting.Value;
                //    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();

                //}

                saveTicketWaitlistResponse.success = true;
                string eventURL = ticketDAL.GetTicketEventUrlById(reqmodel.ticket_event_id);
                saveTicketWaitlistResponse.data.waitlist_id = Id;
                saveTicketWaitlistResponse.data.waitlist_guid = orderGuid;
                saveTicketWaitlistResponse.data.event_url = eventURL;

            }
            catch (Exception ex)
            {
                saveTicketWaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                saveTicketWaitlistResponse.error_info.extra_info = Common.Common.InternalServerError;
                saveTicketWaitlistResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SaveTicketsWaitlist:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);
            }
            return new ObjectResult(saveTicketWaitlistResponse);
        }

        [Route("updateticketswaitliststatus")]
        [HttpPost]
        public async Task<IActionResult> UpdateTicketsWaitlistStatus([FromBody] TicketsWaitlistUpdateRequest reqmodel)
        {
            var saveTicketWaitlistResponse = new SaveTicketWaitlistResponse();

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                ticketDAL.UpdateTicketsWaitlistStatus(reqmodel);

                if (reqmodel.status == Common.Common.TicketWaitlistStatus.Approved)
                {
                    QueueService getStarted = new QueueService();

                    var queueModel = new EmailQueue();
                    queueModel.EType = (int)EmailType.TicketWaitlistOffer;
                    queueModel.BCode = "";
                    queueModel.UId = 0;
                    queueModel.RsvpId = reqmodel.id;
                    queueModel.PerMsg = "";
                    queueModel.Src = 0;
                    var qData = JsonConvert.SerializeObject(queueModel);

                    AppSettings _appsettings = _appSetting.Value;
                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                }

                saveTicketWaitlistResponse.success = true;
                saveTicketWaitlistResponse.data.waitlist_id = reqmodel.id;
            }
            catch (Exception ex)
            {
                saveTicketWaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                saveTicketWaitlistResponse.error_info.extra_info = Common.Common.InternalServerError;
                saveTicketWaitlistResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateTicketsWaitlistStatus:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(saveTicketWaitlistResponse);
        }

        [Route("updateticketswaitlistnote")]
        [HttpPost]
        public async Task<IActionResult> UpdateTicketsWaitlistNote([FromBody] TicketsWaitlistUpdateNoteRequest reqmodel)
        {
            var saveTicketWaitlistResponse = new SaveTicketWaitlistResponse();

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                ticketDAL.UpdateTicketsWaitlistNote(reqmodel);

                saveTicketWaitlistResponse.success = true;
                saveTicketWaitlistResponse.data.waitlist_id = reqmodel.id;
            }
            catch (Exception ex)
            {
                saveTicketWaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                saveTicketWaitlistResponse.error_info.extra_info = Common.Common.InternalServerError;
                saveTicketWaitlistResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateTicketsWaitlistNote:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(saveTicketWaitlistResponse);
        }


        /// <summary>
        /// This method gives list of options for "How did you hear?" while creating reservation
        /// </summary>
        /// <param name="member_id">Id of member (Required)</param>
        /// <returns></returns>
        [Route("hdyh")]
        [HttpGet]
        public IActionResult GetHDYH_OptionsByEventId(int event_id)
        {
            var hDYHResponse = new HDYHResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<HDYH> strList = new List<HDYH>();
                var model = new HDYH();
                strList = ticketDAL.GetHDYHByEventId(event_id);


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
                logDAL.InsertLog("WebApi", "GetHDYH_OptionsByEventId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(hDYHResponse);
        }

        [Route("ticketpostcapture")]
        [HttpPost]
        public async Task<IActionResult> TicketPostCapture([FromBody] TicketPostCaptureRequest reqmodel)
        {
            var ticketResponse = new TicketPostCaptureResponse();

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                var status = ticketDAL.GetTicketPostCaptureStatus(reqmodel.ticket_order_ticket_id);

                if (status == Common.Common.TicketPostCaptureStatus.Claimed)
                {
                    ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.PostCaptureTicketInvalid;
                    ticketResponse.error_info.description = "This ticket is already claimed and not available.";
                    ticketResponse.success = false;

                }
                else
                {

                    ticketDAL.TicketPostCapture(reqmodel);

                    QueueService getStarted = new QueueService();

                    var queueModel = new EmailQueue();
                    queueModel.EType = (int)EmailType.RsvpTicketSalesConfirmation;
                    queueModel.BCode = "";
                    queueModel.UId = reqmodel.ticket_order_ticket_id;
                    queueModel.RsvpId = reqmodel.ticket_order_id;
                    queueModel.PerMsg = "";
                    queueModel.Src = 0;
                    var qData = JsonConvert.SerializeObject(queueModel);

                    AppSettings _appsettings = _appSetting.Value;
                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();

                    ticketResponse.success = true;
                    //ticketResponse.data.waitlist_id = reqmodel.ticket_order_ticket_id;

                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    var userDetailModel = new UserDetailModel();
                    userDetailModel = userDAL.GetUserDetailsbyemail(reqmodel.ticket_holder.email, 0);

                    if (userDetailModel.user_id == 0)
                    {
                        string GuestPwd = StringHelpers.GenerateRandomString(8, false);
                        int UserId = userDAL.CreateUser(reqmodel.ticket_holder.email, GuestPwd, reqmodel.ticket_holder.first_name, reqmodel.ticket_holder.last_name, reqmodel.ticket_holder.country, reqmodel.ticket_holder.postal_code, reqmodel.ticket_holder.work_phone, 0, 0, 0, "", reqmodel.ticket_holder.city, reqmodel.ticket_holder.state, reqmodel.ticket_holder.address1, reqmodel.ticket_holder.address2, Common.Common.GetSource(HttpContext.Request.Headers["AuthenticateKey"]));
                        if (UserId > 0)
                        {
                            userDAL.UpdateUserWinery(UserId, reqmodel.member_id, 4, "", "", "", -1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "TicketPostCapture:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);
            }
            return new ObjectResult(ticketResponse);
        }

        [Route("ticketpostcaptureinvite")]
        [HttpPost]
        public async Task<IActionResult> TicketPostCaptureInvite([FromBody] TicketPostCaptureInvite reqmodel)
        {
            var ticketResponse = new TicketPostCaptureInviteResponse();

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                var status = ticketDAL.GetTicketPostCaptureStatus(reqmodel.ticket_order_ticket_id);

                if (status != Common.Common.TicketPostCaptureStatus.Available)
                {
                    ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.PostCaptureTicketInvalid;
                    ticketResponse.error_info.description = "This ticket is not available for Post capture";
                    ticketResponse.success = false;

                }
                else
                {

                    string postCaptureKey = Guid.NewGuid().ToString();
                    ticketDAL.UpdateTicketPostCaptureInvite(reqmodel, postCaptureKey);

                    QueueService getStarted = new QueueService();

                    var queueModel = new EmailQueue();
                    queueModel.EType = (int)EmailType.TicketPostCapture;
                    queueModel.BCode = "";
                    queueModel.UId = reqmodel.ticket_order_ticket_id;
                    queueModel.RsvpId = reqmodel.ticket_order_id;
                    queueModel.PerMsg = "";
                    queueModel.Src = 0;
                    var qData = JsonConvert.SerializeObject(queueModel);

                    AppSettings _appsettings = _appSetting.Value;
                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();

                    ticketResponse.success = true;
                    ticketResponse.data.ticket_id = reqmodel.ticket_order_ticket_id;
                    ticketResponse.data.ticket_guid = postCaptureKey;
                }
            }
            catch (Exception ex)
            {
                ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "TicketPostCapture:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(ticketResponse);
        }

        [Route("passportparticipatingmember")]
        [HttpGet]
        public IActionResult GetPassportWineryMembers(int event_id)
        {
            var ticketResponse = new PassportParticipatingMemberResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<PassportParticipatingMemberModel> ticts = ticketDAL.GetPassportWineryMembers(event_id);

                if (ticts != null && ticts.Count > 0)
                {
                    ticketResponse.success = true;
                    ticketResponse.data = ticts;
                }
                else
                {
                    ticketResponse.success = true;
                    ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                ticketResponse.success = false;
                ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetPassportWineryMembers:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketResponse);
        }

        [Route("ticketorderdetail")]
        [HttpGet]
        public IActionResult GetTicketOrderDetail(int order_id, string order_guid)
        {
            //event_type >> All: 0, TicketedEvent: 1, PassportEvent: 2
            var ticketOrderDetailResponse = new TicketOrderDetailResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                TicketOrderModel ticketEvent = ticketDAL.GetTicketOrder(order_id, order_guid);

                if (ticketEvent != null)
                {
                    ticketEvent.bill_home_phone = Utility.FormatTelephoneNumber(ticketEvent.bill_home_phone, ticketEvent.bill_country);
                    ticketEvent.event_organizer_phone = Utility.FormatTelephoneNumber(ticketEvent.event_organizer_phone, ticketEvent.bill_country);
                    ticketOrderDetailResponse.success = true;
                    ticketOrderDetailResponse.data = ticketEvent;
                }
                else
                {
                    ticketOrderDetailResponse.success = true;
                    ticketOrderDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketOrderDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketOrderDetailResponse.success = false;
                ticketOrderDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketOrderDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketOrderDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketOrderDetail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketOrderDetailResponse);
        }

        [Route("ticketorderdetailv2")]
        [HttpGet]
        public IActionResult GetTicketOrderDetailV2(int order_id, string order_guid)
        {
            //event_type >> All: 0, TicketedEvent: 1, PassportEvent: 2
            var ticketOrderDetailResponse = new TicketOrderDetailV2Response();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                TicketOrderV2Model ticketEvent = ticketDAL.GetTicketOrderV2(order_id, order_guid);

                if (ticketEvent != null)
                {
                    ticketEvent.bill_home_phone = Utility.FormatTelephoneNumber(ticketEvent.bill_home_phone, ticketEvent.bill_country);
                    ticketEvent.event_organizer_phone = Utility.FormatTelephoneNumber(ticketEvent.event_organizer_phone, ticketEvent.bill_country);
                    ticketOrderDetailResponse.success = true;
                    ticketOrderDetailResponse.data = ticketEvent;
                }
                else
                {
                    ticketOrderDetailResponse.success = true;
                    ticketOrderDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketOrderDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketOrderDetailResponse.success = false;
                ticketOrderDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketOrderDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketOrderDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketOrderDetailV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketOrderDetailResponse);
        }

        [Route("ticketwaitlistdetail")]
        [HttpGet]
        public IActionResult GetTicketWaitlistDetail(int Waitlist_id, string Waitlist_guid)
        {
            var ticketOrderDetailResponse = new TicketWaitlistDetailResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                TicketWaitlistDetail ticketEvent = ticketDAL.GetTicketsEventByWaitlistId(Waitlist_id, Waitlist_guid);

                if (ticketEvent != null)
                {
                    ticketEvent.home_phone = Utility.FormatTelephoneNumber(ticketEvent.home_phone, ticketEvent.member_country);
                    ticketEvent.organizer_phone = Utility.FormatTelephoneNumber(ticketEvent.organizer_phone, ticketEvent.member_country);
                    ticketEvent.member_phone = Utility.FormatTelephoneNumber(ticketEvent.member_phone, ticketEvent.member_country);
                    ticketOrderDetailResponse.success = true;
                    ticketOrderDetailResponse.data = ticketEvent;
                }
                else
                {
                    ticketOrderDetailResponse.success = true;
                    ticketOrderDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketOrderDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketOrderDetailResponse.success = false;
                ticketOrderDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketOrderDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketOrderDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketWaitlistDetail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketOrderDetailResponse);
        }

        [Route("contacteventorganizer")]
        [HttpPost]
        public async Task<IActionResult> SendContactEventOrganizerEmail([FromBody] ContactEventOrganizerEmailRequest model)
        {
            var resp = new AbandonedCartRsvpEmailResponse();
            try
            {
                EmailServiceDAL emailDAL = new EmailServiceDAL(Common.Common.ConnectionString);

                int Id = emailDAL.SaveTempQueueData(model.event_id, model.guest_name, model.guest_email_address, model.contact_reason, model.contact_message);

                if (Id > 0)
                {
                    AuthMessageSender messageService = new AuthMessageSender();

                    QueueService getStarted = new QueueService();

                    var queueModel = new EmailQueue();
                    queueModel.EType = (int)EmailType.SysContactEventOrganizer;
                    queueModel.RsvpId = Id;
                    queueModel.Src = (int)ActionSource.BackOffice;
                    var qData = JsonConvert.SerializeObject(queueModel);

                    AppSettings _appsettings = _appSetting.Value;
                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();

                    resp.success = true;
                }
                else
                    resp.success = false;

            }
            catch (Exception ex)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SendContactEventOrganizerEmail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(resp);
        }


        [Route("ticketdetail")]
        [HttpGet]
        public IActionResult GetTicketDetail(int id, string guid)
        {
            //event_type >> All: 0, TicketedEvent: 1, PassportEvent: 2
            var ticketOrderDetailResponse = new TicketDetailResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                TicketOrderClaimModel ticketEvent = ticketDAL.GetTicketOrderTicketsByPostCaptureKeyandId(id, guid); //ticketDAL.GetTicketOrderByPostCaptureKeyandId(id, postcapturekey);

                if (ticketEvent != null)
                {
                    ticketEvent.bill_home_phone = Utility.FormatTelephoneNumber(ticketEvent.bill_home_phone, ticketEvent.bill_country);
                    ticketEvent.event_organizer_phone = Utility.FormatTelephoneNumber(ticketEvent.event_organizer_phone, ticketEvent.bill_country);
                    ticketOrderDetailResponse.success = true;
                    ticketOrderDetailResponse.data = ticketEvent;
                }
                else
                {
                    ticketOrderDetailResponse.success = true;
                    ticketOrderDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketOrderDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketOrderDetailResponse.success = false;
                ticketOrderDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketOrderDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketOrderDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketDetail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketOrderDetailResponse);
        }

        [Route("activatepassport")]
        [HttpGet]
        public IActionResult ActivatePassport(string activate_code)
        {
            var activatePassportResponse = new ActivatePassportResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                ActivatePassportModel activatePassportModel = new ActivatePassportModel();
                activatePassportModel = ticketDAL.GetInviteKeyByClaimCode(activate_code, Common.Common.TicketPostCaptureStatus.Invited);

                if (!string.IsNullOrEmpty(activatePassportModel.post_capture_key))
                {
                    activatePassportResponse.data = activatePassportModel;
                    activatePassportResponse.success = true;
                }
                else
                {
                    activatePassportModel = ticketDAL.GetInviteKeyByClaimCode(activate_code, Common.Common.TicketPostCaptureStatus.Claimed);

                    if (!string.IsNullOrEmpty(activatePassportModel.post_capture_key))
                    {
                        activatePassportResponse.success = false;
                        activatePassportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        activatePassportResponse.error_info.extra_info = "We're sorry, the claim code has already been activated.";
                    }
                    else
                    {
                        activatePassportResponse.success = false;
                        activatePassportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        activatePassportResponse.error_info.extra_info = "Sorry the claim code is not valid.";
                    }
                }
            }
            catch (Exception ex)
            {
                activatePassportResponse.success = false;
                activatePassportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                activatePassportResponse.error_info.extra_info = Common.Common.InternalServerError;
                activatePassportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ActivatePassport:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(activatePassportResponse);
        }

        [Route("refundticket")]
        [HttpPost]
        public async Task<IActionResult> RefundTicket([FromBody] TicketRefundRequest request)
        {
            ReservationPaymentResponse response = new ReservationPaymentResponse();

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            int member_id = 0;
            try
            {
                Services.Payments objPayments = new Services.Payments(_appSetting);
                response.data = await Services.Payments.RefundTicket(request);
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "RefundTicket:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

            }

            return new ObjectResult(response);
        }

        [Route("chargeticket")]
        [HttpPost]
        public async Task<IActionResult> ChargeTicket([FromBody] TicketPaymentRequest request)
        {
            ReservationPaymentResponse response = new ReservationPaymentResponse();

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            int member_id = 0;

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                TicketOrder ticketOrderModel = ticketDAL.GetTicketOrderById(request.ticket_order_id);

                if (request.amount > 0)
                {
                    member_id = ticketOrderModel.Winery_Id;

                    Common.Payments.Transaction trans = new Common.Payments.Transaction();
                    Common.Payments.CreditCard card = new Common.Payments.CreditCard();

                    trans.ProcessedBy = ticketOrderModel.User_Id;
                    trans.Amount = request.amount;
                    trans.Type = Common.Payments.Transaction.ChargeType.Sale;
                    trans.Transactions = Common.Payments.Transaction.TransactionType.TicketSale;

                    Common.Payments.UserDetails user = new Common.Payments.UserDetails();
                    user.FirstName = ticketOrderModel.BillFirstName;
                    user.LastName = ticketOrderModel.BillLastName;
                    user.HomePhoneStr = ticketOrderModel.BillHomePhone;
                    user.Email = ticketOrderModel.BillEmailAddress;
                    user.ZipCode = ticketOrderModel.BillZip;

                    trans.User = user;

                    card.Number = request.pay_card_number;
                    card.CustName = request.pay_card_custName;
                    card.ExpMonth = request.pay_card_exp_month;
                    card.ExpYear = request.pay_card_exp_year;
                    card.CardToken = request.card_token;
                    card.CardLastFourDigits = request.pay_card_last_four_digits;
                    //card.CardFirstFourDigits = request.pay_card_first_four_digits;
                    card.Type = request.pay_card_type;
                    //Set Card
                    trans.Card = card;
                    trans.WineryId = ticketOrderModel.Winery_Id;

                    //var paymentProcessor = ticketDAL.GetEventPaymentProcessor(ticketOrderModel.Tickets_Event_Id);
                    var paymentProcessor = eventDAL.GetTicketPaymentProcessorByWinery(ticketOrderModel.Winery_Id);
                    int wid = -1;
                    if (paymentProcessor != Common.Common.TicketsPaymentProcessor.CellarPassProcessor)
                        wid = ticketOrderModel.Winery_Id;

                    if (paymentProcessor == Common.Common.TicketsPaymentProcessor.Stripe)
                    {
                        TixOrderCalculationModel taxCalculationModel = new TixOrderCalculationModel();

                        taxCalculationModel.grand_total = request.amount;
                        taxCalculationModel.service_fees = request.fee_total;
                        Services.Payments objPayments = new Services.Payments(_appSetting);
                        Services.Stripe obj = new Services.Stripe(_appSetting);
                        response.data = Services.Stripe.ChargeStripe(ticketOrderModel, taxCalculationModel, _appSetting.Value.StripeSecretKey, request.card_token, trans);
                    }
                    else
                    {
                        Services.Payments objPayments = new Services.Payments(_appSetting);
                        response.data = await Services.Payments.ProcessPaymentV2(wid, request.ticket_order_id, trans);
                    }
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "ChargeTicket:  TicketId-" + request.ticket_order_id + ",Error-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

            }

            return new ObjectResult(response);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("generateorderpdfstr")]
        [HttpGet]
        public async Task<IActionResult> GenerateOrderPDFStr(string orderGUID, int ticketOrderTicketId = 0, bool SelfPrintOnly = true)
        {
            var orderPDFStrResponse = new OrderPDFStrResponse();
            try
            {
                AuthMessageSender messageService = new AuthMessageSender();

                orderPDFStrResponse.success = true;
                orderPDFStrResponse.data.OrderPDFStr = messageService.GenerateOrderPDFStr(orderGUID, ticketOrderTicketId, SelfPrintOnly);
            }
            catch (Exception ex)
            {
                orderPDFStrResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                orderPDFStrResponse.error_info.extra_info = Common.Common.InternalServerError;
                orderPDFStrResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GenerateOrderPDFStr:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(orderPDFStrResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("generateinvoicepdfstr")]
        [HttpGet]
        public async Task<IActionResult> GenerateInvoicePDFStr(int InvoiceId, int WineryId, DateTime Invoicedate)
        {
            var orderPDFStrResponse = new OrderPDFStrResponse();
            try
            {
                AuthMessageSender messageService = new AuthMessageSender();

                orderPDFStrResponse.success = true;
                orderPDFStrResponse.data.OrderPDFStr = messageService.GenerateInvoicePDFStr(InvoiceId, WineryId, Invoicedate);
            }
            catch (Exception ex)
            {
                orderPDFStrResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                orderPDFStrResponse.error_info.extra_info = Common.Common.InternalServerError;
                orderPDFStrResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GenerateInvoicePDFStr:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, WineryId);
            }
            return new ObjectResult(orderPDFStrResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("generategutprintBadgepdfstr")]
        [HttpGet]
        public async Task<IActionResult> GenerateGutPrintBadgePDFStr(string TicketOrderTicketId, int EventID, int BadgeLayoutId, int WineryId)
        {
            var orderPDFStrResponse = new OrderPDFStrResponse();
            try
            {
                AuthMessageSender messageService = new AuthMessageSender();

                orderPDFStrResponse.success = true;
                orderPDFStrResponse.data.OrderPDFStr = messageService.GenerateGutPrintBadgePDFStr(TicketOrderTicketId, EventID, BadgeLayoutId, WineryId);
            }
            catch (Exception ex)
            {
                orderPDFStrResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                orderPDFStrResponse.error_info.extra_info = Common.Common.InternalServerError;
                orderPDFStrResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GenerateGutPrintBadgePDFStr:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, WineryId);
            }
            return new ObjectResult(orderPDFStrResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("generateaddresslabelbadgepdfstr")]
        [HttpGet]
        public async Task<IActionResult> GenerateAddressLabelBadgePDFStr(string TicketOrderTicketId, int EventID, int BadgeLayoutId, int WineryId)
        {
            var orderPDFStrResponse = new OrderPDFStrResponse();
            try
            {
                AuthMessageSender messageService = new AuthMessageSender();

                orderPDFStrResponse.success = true;
                orderPDFStrResponse.data.OrderPDFStr = messageService.GenerateAddressLabelBadgePDFStr(TicketOrderTicketId, EventID, BadgeLayoutId, WineryId);
            }
            catch (Exception ex)
            {
                orderPDFStrResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                orderPDFStrResponse.error_info.extra_info = Common.Common.InternalServerError;
                orderPDFStrResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GenerateAddressLabelBadgePDFStr:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, WineryId);
            }
            return new ObjectResult(orderPDFStrResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("generatenamebadgepdfstr")]
        [HttpGet]
        public async Task<IActionResult> GenerateNameBadgePDFStr(string TicketOrderTicketId, int EventID, int BadgeLayoutId, int WineryId)
        {
            var orderPDFStrResponse = new OrderPDFStrResponse();
            try
            {
                AuthMessageSender messageService = new AuthMessageSender();

                orderPDFStrResponse.success = true;
                orderPDFStrResponse.data.OrderPDFStr = messageService.GenerateNameBadgePDFStr(TicketOrderTicketId, EventID, BadgeLayoutId, WineryId);
            }
            catch (Exception ex)
            {
                orderPDFStrResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                orderPDFStrResponse.error_info.extra_info = Common.Common.InternalServerError;
                orderPDFStrResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GenerateNameBadgePDFStr:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, WineryId);
            }
            return new ObjectResult(orderPDFStrResponse);
        }

        [Route("passportitineraryinstructionbyeventid")]
        [HttpGet]
        public IActionResult GetPassportItineraryInstructions(int ticket_event_id)
        {
            var passportItineraryInstructionResponse = new PassportItineraryInstructionResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                ActivatePassportModel activatePassportModel = new ActivatePassportModel();

                string eventPassword = string.Empty;
                TicketEventDetailModel ticketEvent = ticketDAL.GetTicketEventDetailsById(ticket_event_id, ref eventPassword);
                string content = "";

                if (ticketEvent != null && ticketEvent.member_id > 0)
                {
                    SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                    content = settingsDAL.GetContent(Common.Common.SiteContentType.PassportItineraryInstructions);

                    content = content.Replace("[[EventName]]", ticketEvent.name);
                    content = content.Replace("[[RegionName]]", ticketEvent.region_name);

                    string strpath = "https://typhoon.cellarpass.com/";
                    if (Common.Common.ConnectionString.IndexOf("live") > -1)
                        strpath = "https://www.cellarpass.com/";

                    //<a href="{1}events/{0}-reservations?sm=true" target="_new" style="color:#40C4F4; text-decoration: none;">View Availability</a>
                    content = content.Replace("[[ViewAvailability]]", string.Format("<a href=\"{1}events/{0}-reservations?sm=true\" target =\"_new\" style =\"color:#40C4F4; text-decoration: none;\" > View Availability</a>", ticketEvent.event_url, strpath));
                }


                //string event_url = string.Format("{1}events/{0}-reservations?sm=true", ticketEvent.event_url, strpath);
                //content = content.Replace("[[ViewAvailability]]", Utility.GenerateEmailButton("View Availability", event_url, "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));

                //string itinerarie_url = string.Format("{0}account/itineraries/all", strpath);
                //content = content.Replace("[[ViewItineraries]]", Utility.GenerateEmailButton("View Itineraries", itinerarie_url, "#47bf12", "#47bf12", "15px", "15px", "#ffffff"));

                PassportItineraryInstructionModel data = new PassportItineraryInstructionModel();
                data.passport_itinerary_instructions = content;

                passportItineraryInstructionResponse.data = data;
                passportItineraryInstructionResponse.success = true;
            }
            catch (Exception ex)
            {
                passportItineraryInstructionResponse.success = false;
                passportItineraryInstructionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                passportItineraryInstructionResponse.error_info.extra_info = Common.Common.InternalServerError;
                passportItineraryInstructionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetPassportItineraryInstructions:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(passportItineraryInstructionResponse);
        }

        [Route("upcoming_events")]
        [HttpGet]
        public IActionResult GetTicketEventsComponent(TicketEventsComponentRequest model)
        {
            var ticketEventsComponentResponse = new TicketEventsComponentResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<UpcomingEventModel> list = ticketDAL.GetTicketEventsComponent(model);

                if (list != null)
                {
                    ticketEventsComponentResponse.success = true;
                    ticketEventsComponentResponse.data = list;
                }
                else
                {
                    ticketEventsComponentResponse.success = true;
                    ticketEventsComponentResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventsComponentResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketEventsComponentResponse.success = false;
                ticketEventsComponentResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventsComponentResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventsComponentResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketEventsComponent:  request data:" + JsonConvert.SerializeObject(model) + ", Message" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketEventsComponentResponse);
        }

        [Route("send_event_reminders")]
        [HttpGet]
        public IActionResult SendTicketEventReminders()
        {
            var ticketEventsComponentResponse = new BaseResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                List<TicketEventReminderMOdel> list = ticketDAL.GetTicketEventReminders();

                if (list != null)
                {
                    ticketEventsComponentResponse.success = true;
                    QueueService getStarted = new QueueService();

                    foreach (TicketEventReminderMOdel reminder in list)
                    {


                        var queueModel = new EmailQueue();
                        queueModel.EType = (int)EmailType.TicketEventFollowingReminder;
                        queueModel.RsvpId = reminder.event_id;
                        queueModel.UId = reminder.queue_id;
                        var qData = JsonConvert.SerializeObject(queueModel);

                        AppSettings _appsettings = _appSetting.Value;
                        getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                    }
                }
                else
                {
                    ticketEventsComponentResponse.success = true;
                    ticketEventsComponentResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventsComponentResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketEventsComponentResponse.success = false;
                ticketEventsComponentResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventsComponentResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventsComponentResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SendTicketEventReminders:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketEventsComponentResponse);
        }

        [Route("send_event_review_invitation")]
        [HttpGet]
        public IActionResult SendEventReviewInvitation()
        {
            var ticketEventsComponentResponse = new BaseResponse();
            try
            {
                QueueService getStarted = new QueueService();
                var queueModel = new EmailQueue();
                queueModel.EType = (int)EmailType.EventReviewInvitation;
                queueModel.RsvpId = 0;
                queueModel.UId = 0;
                queueModel.BCode = "";
                var qData = JsonConvert.SerializeObject(queueModel);

                AppSettings _appsettings = _appSetting.Value;
                getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();

                ticketEventsComponentResponse.success = true;
            }
            catch (Exception ex)
            {
                ticketEventsComponentResponse.success = false;
                ticketEventsComponentResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventsComponentResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventsComponentResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SendEventReviewInvitation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketEventsComponentResponse);
        }


        [Route("ticket_order_ticket_details")]
        [HttpGet]
        public IActionResult GetTicketOrderTicketDetail(string guid)
        {
            var ticketOrderTicketDetailResponse = new TicketOrderTicketDetailV2Response();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                TixOrderTicketV2 ticketEvent = ticketDAL.GetTicketDetailsByTicketGUID(guid);

                if (ticketEvent != null)
                {
                    ticketOrderTicketDetailResponse.success = true;
                    ticketOrderTicketDetailResponse.data = ticketEvent;
                }
                else
                {
                    ticketOrderTicketDetailResponse.success = true;
                    ticketOrderTicketDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketOrderTicketDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketOrderTicketDetailResponse.success = false;
                ticketOrderTicketDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketOrderTicketDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketOrderTicketDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketOrderTicketDetail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketOrderTicketDetailResponse);
        }

        [Route("saveeventreview")]
        [HttpPost]
        public IActionResult SaveTicketEventReview([FromBody] TicketEventReviewRequest reqmodel)
        {
            var resp = new BaseResponse();
            if (string.IsNullOrWhiteSpace(reqmodel.ticket_guid))
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Invalid order Guid. A valid Order Guid is required";
                resp.error_info.description = "Invalid order Guid. A valid Order Guid is required";
                return new ObjectResult(resp);
            }
            else if (string.IsNullOrWhiteSpace(reqmodel.review))
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Review data is required";
                resp.error_info.description = "Review data is required";
                return new ObjectResult(resp);
            }
            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
            try
            {
                bool isSuccess = ticketDAL.SaveTicketEventReview(reqmodel);
                resp.success = isSuccess;

            }
            catch (Exception ex)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SaveTicketEventReview:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(resp);
        }

        [Route("get_event_reviews")]
        [HttpGet]
        public IActionResult GetEventReviews()
        {
            var response = new EventReviewsResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                var eventReviews = ticketDAL.GetEventReviews();

                if (eventReviews != null && eventReviews.Count > 0)
                {
                    response.data = eventReviews;
                    response.success = true;
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
                response.error_info.extra_info = Common.Common.InternalServerError.ToString();
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventReviews:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }

        [Route("geteventreviewsbyid")]
        [HttpGet]
        public IActionResult GetEventReviewsById(int ticket_event_id)
        {
            var response = new EventReviewsResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                var eventReviews = ticketDAL.GetTicketEventReviewsByEventId(ticket_event_id);

                if (eventReviews != null && eventReviews.Count > 0)
                {
                    response.data = eventReviews;
                    response.success = true;
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
                response.error_info.extra_info = Common.Common.InternalServerError.ToString();
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventReviewsById:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("GetAllMCList")]
        [HttpGet]
        public IActionResult GetAllMCList(int wineryId)
        {
            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
            List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(wineryId, (int)Common.Common.SettingGroup.mailchimp);
            string mcAPIKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_key);
            MailChimpAPI mailChimpAPI = new MailChimpAPI(mcAPIKey, "", "", "", "", "", "");
            return new ObjectResult(mailChimpAPI.GetAllList());
        }

        [Route("ticketeventsbyregion")]
        [HttpGet]
        public IActionResult GetTicketEventByRegion(string Str_Search, DateTime request_date)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            var ticketEventByRegionResponse = new TicketEventByRegionResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                List<TicketEventDetail2Model> list = new List<TicketEventDetail2Model>();

                if (request_date.Year > 2020)
                {
                    list = ticketDAL.GetTicketEventByRegion(Str_Search, request_date);
                }

                if (list != null && list.Count > 0)
                {
                    ticketEventByRegionResponse.success = true;
                    ticketEventByRegionResponse.data = list;
                }
                else
                {
                    ticketEventByRegionResponse.success = true;
                    ticketEventByRegionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketEventByRegionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "GetTicketEventByRegion:  Str_Search:" + Str_Search + ", request_date:" + request_date.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);

                ticketEventByRegionResponse.success = false;
                ticketEventByRegionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketEventByRegionResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketEventByRegionResponse.error_info.description = ex.Message.ToString();
                

                logDAL.InsertLog("WebApi", "GetTicketEventByRegion:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketEventByRegionResponse);
        }

        //[Route("generatetestticketpdf")]
        //[HttpGet]
        //public IActionResult GenerateTestTicketPDF()
        //{
        //    var bytes = new AuthMessageSender().GenerateOrderPDFTest();
        //    return File(bytes, "application/pdf", "MyTickets.pdf");
        //}



        [Route("ticketeventfaqbyid")]
        [HttpGet]
        public IActionResult GetTicketsFaqByEventId(int event_id)
        {
            var ticketsFaqByEventResponse = new TicketsFaqByEventResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                List<TicketsFaqModel> list = ticketDAL.GetTicketsFaqByEventId(event_id);

                if (list != null && list.Count > 0)
                {
                    ticketsFaqByEventResponse.success = true;
                    ticketsFaqByEventResponse.data = list;
                }
                else
                {
                    ticketsFaqByEventResponse.success = true;
                    ticketsFaqByEventResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketsFaqByEventResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketsFaqByEventResponse.success = false;
                ticketsFaqByEventResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketsFaqByEventResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketsFaqByEventResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketsFaqByEventId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketsFaqByEventResponse);
        }

        [Route("ticketsbytimeandkeyword")]
        [HttpGet]
        public IActionResult GetTicketsByTimeAndkeyword(int winery_id,DateTime start_date,DateTime end_date, int offset_minutes, int mode = 1, string keyword = "",string sort_by = "", int i_display_length = 100, int i_display_start = 1)
        {
            var ticketOrderV2Response = new TicketOrderV2Response();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                List<TicketOrderV2> list = ticketDAL.GetTicketsByTimeAndkeyword(winery_id, start_date, end_date, offset_minutes, mode, keyword, sort_by,  i_display_length, i_display_start);

                if (list != null && list.Count > 0)
                {
                    ticketOrderV2Response.success = true;
                    ticketOrderV2Response.data = list;
                }
                else
                {
                    ticketOrderV2Response.success = true;
                    ticketOrderV2Response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketOrderV2Response.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketOrderV2Response.success = false;
                ticketOrderV2Response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketOrderV2Response.error_info.extra_info = Common.Common.InternalServerError;
                ticketOrderV2Response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketsByTimeAndkeyword:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketOrderV2Response);
        }

        [Route("unsubscribeevent")]
        [HttpPost]
        public IActionResult UnsubscribeEvent([FromBody] UnsubscribeEventRequest model)
        {
            var resp = new BaseResponse2();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

                NameValueCollection decrypted = StringHelpers.DecryptQueryString(model.q);

                if (decrypted != null && decrypted.Count > 0)
                {
                    int event_id = Int32.Parse(decrypted["event_id"]);
                    int user_id = Int32.Parse(decrypted["user_id"]);

                    if (event_id > 0 && user_id > 0)
                    {
                        ticketDAL.UnsubscribeEvent(user_id, event_id);

                        resp.success = true;
                    }
                }
                //int eventId =Convert.ToInt32(StringHelpers.Decrypt(model.event_id));
                //int userId = Convert.ToInt32(StringHelpers.Decrypt(model.user_id));
            }
            catch (Exception ex)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UnsubscribeEvent:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(resp);
        }

        [Route("checkticketeventinvite")]
        [HttpGet]
        public IActionResult CheckTicketEventInvite(int event_id,string email)
        {
            var baseResponse = new BaseResponse2();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);

                string errorMsg = ticketDAL.CheckTicketEventInvite(event_id, email);

                if (string.IsNullOrEmpty(errorMsg) || errorMsg.IndexOf("require") > -1)
                    baseResponse.success = true;

                baseResponse.error_info.description = errorMsg;
            }
            catch (Exception ex)
            {
                baseResponse.success = false;
                baseResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                baseResponse.error_info.extra_info = Common.Common.InternalServerError;
                baseResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CheckTicketEventInvite:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(baseResponse);
        }

        [Route("userorders")]
        [HttpGet]
        public IActionResult GetUserOrders(int user_id, DateTime? to_date, DateTime? from_date, bool? is_past_event = null, int? member_id = null)
        {
            var ticketResponse = new UserOrdersResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                List<UserOrdersModel> ticts = ticketDAL.GetUserOrders(user_id, to_date, from_date, is_past_event, member_id);

                if (ticts != null && ticts.Count > 0)
                {
                    ticketResponse.success = true;
                    ticketResponse.data = ticts;
                }
                else
                {
                    ticketResponse.success = true;
                    ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketResponse.success = false;
                ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetUserOrders:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketResponse);
        }

        [Route("usertickets")]
        [HttpGet]
        public IActionResult GetUserTickets(string user_name, DateTime? to_date, DateTime? from_date, bool? is_past_event = null, int? member_id = null)
        {
            var ticketResponse = new UserTicketResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                List<UserTicketModel> ticts = ticketDAL.GetUserTickets(user_name, to_date, from_date, is_past_event, member_id);

                if (ticts != null && ticts.Count > 0)
                {
                    ticketResponse.success = true;
                    ticketResponse.data = ticts;
                }
                else
                {
                    ticketResponse.success = true;
                    ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ticketResponse.success = false;
                ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetUserTickets:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketResponse);
        }

        [Route("ticketeventsbyeventtype")]
        [HttpGet]
        public IActionResult GetTicketeventsByEventType(List<int> event_type,int? user_id)
        {
            var ticketResponse = new TicketeventsByEventTypeResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                List<TicketEventByEventTypeModel> ticts = ticketDAL.GetTicketeventsByEventType(String.Join(",", event_type), user_id);

                if (ticts != null && ticts.Count > 0)
                {
                    ticketResponse.success = true;
                    ticketResponse.data = ticts;
                }
                else
                {
                    ticketResponse.success = true;
                    ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ticketResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                ticketResponse.success = false;
                ticketResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ticketResponse.error_info.extra_info = Common.Common.InternalServerError;
                ticketResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTicketeventsByEventType:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(ticketResponse);
        }

        [Route("ticketreviewbyticketorderticketid")]
        [HttpGet]
        public IActionResult GetTicketReview(int ticket_order_ticket_id, int user_id)
        {
            var response = new TicketReviewResponse();
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                var eventReviews = ticketDAL.GetTicketReviewByTicketOrderTicketId(ticket_order_ticket_id, user_id);

                if (eventReviews != null && eventReviews.ticket_order_ticket_id > 0)
                {
                    response.data = eventReviews;
                    response.success = true;
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
                response.error_info.extra_info = Common.Common.InternalServerError.ToString();
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetTicketReview:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }
    }
}