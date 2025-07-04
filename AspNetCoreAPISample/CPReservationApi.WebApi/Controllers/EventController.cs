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
using System.Net;
using static CPReservationApi.Common.Common;
using Newtonsoft.Json;
using CPReservationApi.Common;
using static CPReservationApi.Common.Email;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.IO;

namespace CPReservationApi.WebApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/event")]
    public class EventController : BaseController
    {

        public static IOptions<AppSettings> _appSetting;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appSetting"></param>
        public EventController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        long LongRandom(long min, long max, Random rand)
        {
            long result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
            result = (result << 32);
            result = result | (long)rand.Next((Int32)min, (Int32)max);
            return result;
        }

        /// <summary>
        /// This method gives list of event addons by eventid
        /// </summary>
        /// <param name="event_id">Id of Event (Required)</param>
        /// <param name="reservation_id">Id of Reservation (optional)</param>
        /// <returns></returns>
        [Route("eventaddons")]
        [HttpGet]
        public IActionResult GetEventAddons(int event_id, int reservation_id = 0)
        {
            var eventAddOnsResponse = new EventAddOnsResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var eventAddOnModel = new List<EventAddOnModel>();

                eventAddOnModel = eventDAL.GetEventAddOnsByEventId(event_id);

                var reservation_addon = new List<Model.Reservation_Addon>();
                if (reservation_id > 0)
                {
                    reservation_addon = eventDAL.GetReservation_AddonbyReservationId(reservation_id);
                }

                if (eventAddOnModel != null)
                {
                    var addOn_Grouplist = new List<AddOn_Group>();
                    foreach (var item in eventAddOnModel)
                    {
                        var addOn_Group = new AddOn_Group();
                        addOn_Group = eventDAL.GetAddOnGroupItemsByGroupId(item.GroupId, reservation_addon);
                        if (addOn_Group.id > 0)
                        {
                            addOn_Grouplist.Add(addOn_Group);
                        }
                    }

                    if (addOn_Grouplist != null && addOn_Grouplist.Count > 0)
                    {
                        eventAddOnsResponse.success = true;
                        eventAddOnsResponse.data = addOn_Grouplist;
                    }
                    else
                    {
                        eventAddOnsResponse.success = true;
                        eventAddOnsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        eventAddOnsResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    eventAddOnsResponse.success = true;
                    eventAddOnsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    eventAddOnsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                eventAddOnsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventAddOnsResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                eventAddOnsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventAddOns:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(eventAddOnsResponse);
        }

        [Route("googlesftpevent")]
        [HttpGet]
        public IActionResult GetGoogleSFTPEvent()
        {
            var googleSFTPEventResponse = new GoogleSFTPEventResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var model = new GoogleSFTPModel();
                var feedMetadata = new FeedMetadata();

                feedMetadata.shard_id = 0;
                feedMetadata.total_shards_count = 1;
                feedMetadata.processing_instruction = "PROCESS_AS_SNAPSHOT";
                feedMetadata.nonce = LongRandom(1000, 100000000000000050, new Random());

                model.feed_metadata = feedMetadata;
                model.products = eventDAL.GetGoogleSFTPEvent();


                string GoogleSFTPJson = JsonConvert.SerializeObject(model);

                googleSFTPEventResponse.success = true;
                googleSFTPEventResponse.data = model;
            }
            catch (Exception ex)
            {
                googleSFTPEventResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                googleSFTPEventResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                googleSFTPEventResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetGoogleSFTPEvent:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(googleSFTPEventResponse);
        }

        /// <summary>
        /// This method gives list of event addons by memberid
        /// </summary>
        /// <param name="member_id">Id of Member (Required)</param>
        /// <param name="reservation_id">Id of Reservation (optional)</param>
        /// <returns></returns>
        [Route("eventaddonsbymember")]
        [HttpGet]
        public IActionResult GetEventAddonsByMember(int member_id, int reservation_id = 0)
        {
            var eventAddOnsResponse = new EventAddOnsResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var addOn_GroupModel = new List<AddOn_Group>();

                if (reservation_id > 0)
                {
                    addOn_GroupModel = eventDAL.GetAddOnGroupByReservationId(reservation_id);
                }
                else
                {
                    addOn_GroupModel = eventDAL.GetAddOnGroupByWineryId(member_id);
                }

                var reservation_addon = new List<Model.Reservation_Addon>();
                if (reservation_id > 0)
                {
                    reservation_addon = eventDAL.GetReservation_AddonbyReservationId(reservation_id);
                }

                if (addOn_GroupModel != null)
                {
                    var addOn_Grouplist = new List<AddOn_Group>();
                    foreach (var item in addOn_GroupModel)
                    {
                        var addOn_Group = new AddOn_Group();
                        addOn_Group = eventDAL.GetAddOnGroupItemsByGroupId(item.id, reservation_addon);
                        addOn_Grouplist.Add(addOn_Group);
                    }

                    if (addOn_Grouplist != null)
                    {
                        eventAddOnsResponse.success = true;
                        eventAddOnsResponse.data = addOn_Grouplist;
                    }
                    else
                    {
                        eventAddOnsResponse.success = true;
                        eventAddOnsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        eventAddOnsResponse.error_info.extra_info = "no record found";
                    }
                }
                else
                {
                    eventAddOnsResponse.success = true;
                    eventAddOnsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    eventAddOnsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                eventAddOnsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventAddOnsResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                eventAddOnsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventAddonsByMember:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(eventAddOnsResponse);
        }

        /// <summary>
        /// This method gives list of active discounts by eventid
        /// </summary>
        /// <param name="event_id">Id of Event (Required)</param>
        /// <returns></returns>
        [Route("discount")]
        [HttpGet]
        public IActionResult GetActiveDiscounts(int event_id)
        {
            var discountResponse = new DiscountResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var discountModel = new List<DiscountModel>();

                discountModel = eventDAL.GetActiveDiscountsByEventId(event_id);
                if (discountModel != null)
                {
                    discountResponse.success = true;
                    discountResponse.data = discountModel;
                }
                else
                {
                    discountResponse.success = true;
                    discountResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    discountResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                discountResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                discountResponse.error_info.extra_info = Common.Common.InternalServerError;
                discountResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetActiveDiscounts:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(discountResponse);
        }

        /// <summary>
        /// This method would search for events by date, guest count etc for a member
        /// </summary>
        /// <param name="model">Load Schedule Request Model</param>
        /// <returns></returns>

        [Route("schedule")]
        [HttpGet]
        public IActionResult LoadSchedule(LoadScheduleRequest model)
        {
            var loadScheduleResponse = new LoadScheduleResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var eventSchedule = new EventSchedule();
                var eventScheduleEvent = new List<EventScheduleEvent>();
                bool IsAdmin = Convert.ToBoolean(Request.Headers["IsAdmin"]);

                eventSchedule = eventDAL.GetConsumerEventScheduleV2(model.member_id, model.req_date, model.guest_count, model.slot_id, model.slot_type, model.event_id, IsAdmin, model.rsvp_id, model.booking_type, model.hide_no_availability);

                eventScheduleEvent = eventSchedule.EventScheduleEvent;
                if (eventScheduleEvent.Count > 0 && eventSchedule.isAnyEventAvailable)
                {
                    if (IsAdmin)
                    {
                        string HolidayName = eventDAL.IsHoliday(model.member_id, model.req_date);
                        if (!string.IsNullOrEmpty(HolidayName))
                        {
                            loadScheduleResponse.error_info.error_type = (int)Common.Common.ErrorType.IsHoliday;
                            loadScheduleResponse.error_info.holiday_name = HolidayName;
                            loadScheduleResponse.error_info.holiday_description = "WARNING! This property is closed on " + model.req_date.ToString("dddd, MMMM dd, yyyy");
                        }
                    }
                    loadScheduleResponse.success = true;
                    loadScheduleResponse.data = eventScheduleEvent;
                }
                else if (model.booking_type == 0)
                {
                    AvailableEventsForFutureDate availableEventsForFutureDate = eventDAL.GetAvailableEventsForFutureDateV3(model.member_id, model.guest_count, model.req_date, model.event_id);
                    if (availableEventsForFutureDate.event_date >= DateTime.UtcNow.Date)
                    {
                        loadScheduleResponse.error_info.error_type = (int)Common.Common.ErrorType.EventFutureDate;
                        loadScheduleResponse.error_info.extra_info = availableEventsForFutureDate.event_date.ToString();
                        loadScheduleResponse.error_info.description = "The next date with availability for " + model.guest_count + " guests is " + availableEventsForFutureDate.event_date.ToString("dddd, MMMM dd, yyyy") + " at " + availableEventsForFutureDate.start_time.ToString("hh:mm tt") + ".";
                    }
                    else
                    {
                        string HolidayName = eventDAL.IsHoliday(model.member_id, model.req_date);
                        if (!string.IsNullOrEmpty(HolidayName))
                        {
                            loadScheduleResponse.error_info.error_type = (int)Common.Common.ErrorType.IsHoliday;
                            loadScheduleResponse.error_info.extra_info = HolidayName;
                            loadScheduleResponse.error_info.description = "WARNING! This property is closed on " + model.req_date.ToString("dddd, MMMM dd, yyyy");
                        }
                        else
                        {
                            MaxSeatsLeft maxSeatsLeft = eventDAL.GetMaxSeatsLeftByWineryIdAndDateV3(model.member_id, model.req_date);

                            if (maxSeatsLeft.min_seats > 0 || maxSeatsLeft.max_seats > 0)
                            {
                                if (maxSeatsLeft.min_seats > model.guest_count)
                                {
                                    loadScheduleResponse.error_info.error_type = (int)Common.Common.ErrorType.MinSeats;
                                    loadScheduleResponse.error_info.extra_info = maxSeatsLeft.min_seats.ToString();
                                    loadScheduleResponse.error_info.description = "Sorry, the minimum guest count of the event has changed. Minimum guest count required is " + maxSeatsLeft.min_seats.ToString();
                                }
                                else
                                {
                                    loadScheduleResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxSeats;
                                    loadScheduleResponse.error_info.extra_info = maxSeatsLeft.max_seats.ToString();
                                    loadScheduleResponse.error_info.description = "Sorry, the max group size of the event has changed. Max group size allowed is " + maxSeatsLeft.max_seats.ToString();
                                }
                            }
                            else
                            {
                                DateTime MaxEndDate = eventDAL.GetMaxEndDateByWineryId(model.member_id);
                                if (MaxEndDate.Date < model.req_date.Date && MaxEndDate >= DateTime.UtcNow.Date)
                                {
                                    var winery = new Model.WineryModel();
                                    winery = eventDAL.GetWineryById(model.member_id);

                                    loadScheduleResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxEndDate;
                                    loadScheduleResponse.error_info.extra_info = MaxEndDate.ToString();
                                    loadScheduleResponse.error_info.description = winery.DisplayName + " accepts reservations up until " + MaxEndDate.ToString("dddd, MMMM dd, yyyy") + ". Contact " + Utility.FormatTelephoneNumber(winery.BusinessPhone.ToString(), winery.WineryAddress.country) + " or via " + winery.EmailAddress + " for more information.";
                                }
                                else
                                {
                                    loadScheduleResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                                    if (model.event_id > 0 || model.slot_id > 0)
                                        loadScheduleResponse.error_info.description = "There is no availability for this experience on " + model.req_date.ToString("MM/dd/yyyy");
                                    else
                                        loadScheduleResponse.error_info.description = "We’re sorry, but there are no experiences available on " + model.req_date.ToString("MM/dd/yyyy");
                                }
                            }

                        }
                    }
                }
                else
                {
                    loadScheduleResponse.error_info.error_type = (int)Common.Common.ErrorType.EventInactive;
                    loadScheduleResponse.error_info.extra_info = "Event Inactive";
                    loadScheduleResponse.error_info.description = "Sorry, this event is not available";
                }
            }
            catch (Exception ex)
            {
                loadScheduleResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                loadScheduleResponse.error_info.extra_info = Common.Common.InternalServerError;
                loadScheduleResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "LoadSchedule:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
            }
            return new ObjectResult(loadScheduleResponse);
        }

        [Route("eventdetail")]
        [HttpGet]
        public IActionResult EventDetail(EventDetailRequest model)
        {
            var eventDetailResponse = new EventDetailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var eventScheduleEvent = new EventScheduleEvent();

                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                eventScheduleEvent = eventDAL.GetEventDetail(model.req_date, model.event_id, IsAdmin);

                if (eventScheduleEvent != null && eventScheduleEvent.event_id > 0)
                {
                    eventDetailResponse.success = true;
                    eventDetailResponse.data = eventScheduleEvent;
                }
            }
            catch (Exception ex)
            {
                eventDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                eventDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "EventDetail:  event_id-" + model.event_id.ToString() + ",req_date-" + model.req_date.ToString() + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(eventDetailResponse);
        }

        [Route("reservationcheckout")]
        [HttpGet]
        public async Task<IActionResult> ReservationCheckout(DateTime request_date, int request_guest, int member_id, int slot_id, int slot_type, bool include_waitlist = true, bool include_hidden_member = false, int user_id = 0, int reservation_id = 0, string wl = "", string access_code = "")
        {
            var rsvpResponse = new ReservationCheckoutResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var rsvpCheckoutData = new ReservationCheckoutModel();
                rsvpResponse.error_info = new ErrorInfo();

                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                bool isEventAvailable = true;
                if (!IsAdmin)
                {
                    //check if there is valid waitlist guid
                    if (!string.IsNullOrWhiteSpace(wl) && wl != "null")
                    {
                        var waitList = eventDAL.GetReservationV2WaitlistbyId(wl, "", 0);

                        if (waitList != null)
                        {
                            if ((user_id > 0 && waitList.user_id != user_id) || waitList.status_date_time.AddMinutes(waitList.valid_minutes) < DateTime.UtcNow || waitList.waitlist_status == Waitlist_Status.canceled || waitList.waitlist_status == Waitlist_Status.converted || waitList.slot_id != slot_id || waitList.slot_type != slot_type)
                            {

                                var error_data = new ViewModels.ErrorData();

                                error_data.event_name = waitList.event_name;
                                error_data.location_name = waitList.event_location;
                                error_data.member_url = waitList.member_url;

                                rsvpResponse.success = false;
                                isEventAvailable = false;
                                rsvpResponse.error_info = new ErrorInfo
                                {
                                    error_type = (int)Common.Common.ErrorType.WaitListExists,
                                    description = "We're sorry, but the wait list is no longer available.",
                                    extra_info = string.Empty,
                                    error_data = error_data
                                };
                                return new ObjectResult(rsvpResponse);
                            }
                            else
                            {
                                isEventAvailable = true;
                            }
                        }
                    }
                    else
                    {
                        //check availability first if available then only send
                        isEventAvailable = eventDAL.CheckEventAvailability(member_id, request_guest, request_date, slot_id, slot_type, include_waitlist, include_hidden_member, access_code, reservation_id);
                    }
                }


                if (isEventAvailable)
                {

                    if (!string.IsNullOrWhiteSpace(access_code))
                    {

                        var eventAccess = eventDAL.CheckEventAccessCode(member_id, 0, access_code, user_id, slot_id, slot_type);

                        if (eventAccess == null || !eventAccess.IsValid)
                        {
                            rsvpResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                            rsvpResponse.error_info.extra_info = "Invalid Acess Code";
                            if (user_id > 0 && eventAccess.StartDate.HasValue)
                            {
                                rsvpResponse.error_info.description = "Access code has reached it's limit for this user and cannot be used.";
                            }
                            else
                            {
                                rsvpResponse.error_info.description = "Invalid access code. Acess code does not exist.";
                            }

                            return new ObjectResult(rsvpResponse);
                        }
                        else if (eventAccess.IsValid && eventAccess.StartDate == null)
                        {
                            rsvpResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                            rsvpResponse.error_info.extra_info = "Invalid Acess Code";
                            rsvpResponse.error_info.description = string.Format("The '{0}' access code is no longer available", access_code);
                            return new ObjectResult(rsvpResponse);
                        }
                    }

                    rsvpCheckoutData = eventDAL.GetEventDataForRSVPCheckout(request_date, IsAdmin, request_guest, member_id, slot_id, slot_type);

                    if (rsvpCheckoutData != null && rsvpCheckoutData.event_id > 0)
                    {
                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.member);
                        string MarketingOptin = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_Marketing_Opt_in);

                        if (user_id > 0)
                        {
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            string emailMarketingStatus = userDAL.GetEmailMarketingStatus(member_id, user_id);

                            if (MarketingOptin.Length > 0 && emailMarketingStatus.Length == 0)
                            {
                                rsvpCheckoutData.marketing_optin_text = MarketingOptin;
                                rsvpCheckoutData.show_marketing_optin = MarketingOptin.Length > 0;
                            }
                        }
                        else
                        {
                            rsvpCheckoutData.marketing_optin_text = MarketingOptin;
                            rsvpCheckoutData.show_marketing_optin = MarketingOptin.Length > 0;
                        }

                        var eventModel = eventDAL.GetEventDetail(request_date, rsvpCheckoutData.event_id, IsAdmin, slot_id, slot_type);

                        rsvpCheckoutData.member_benefit_required = eventModel.member_benefit_required;
                        rsvpCheckoutData.account_type_required = eventModel.account_type_required;


                        if (user_id > 0)
                        {
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            UserDetail userDetail = userDAL.GetUserDetailsbyId(user_id);

                            if (userDetail != null && userDetail.user_id > 0)
                            {
                                if ((MobileNumberStatus)userDetail.mobile_number_status == MobileNumberStatus.unverified)
                                {
                                    rsvpCheckoutData.mobile_number_status = (int)Utility.SMSVerified_System(userDetail.mobile_phone);

                                    if ((MobileNumberStatus)userDetail.mobile_number_status != MobileNumberStatus.unverified)
                                        userDAL.UpdateMobilePhoneStatusById(userDetail.mobile_phone, rsvpCheckoutData.mobile_number_status);
                                }
                                else
                                    rsvpCheckoutData.mobile_number_status = userDetail.mobile_number_status;
                            }

                            var reservationConflictCheck = new ReservationDetailModel();
                            reservationConflictCheck = eventDAL.IsReservationConflict(reservation_id, user_id, request_date, DateTime.Now.TimeOfDay, DateTime.Now.TimeOfDay, slot_id, slot_type);
                            if (reservationConflictCheck != null && reservationConflictCheck.reservation_id > 0)
                            {
                                //get event details
                                //eventDAL.GetE
                                if (reservationConflictCheck.event_start_date.TimeOfDay == rsvpCheckoutData.event_start_date.TimeOfDay && reservationConflictCheck.event_end_date.TimeOfDay == rsvpCheckoutData.event_end_date.TimeOfDay)
                                {
                                    rsvpResponse.success = false;
                                    rsvpResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                                    rsvpResponse.error_info.extra_info = "Reservation Conflict Error";
                                    //rsvpResponse.error_info.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                    rsvpResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} on {4} from {1} to {2}.<br>Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));

                                    var error_data = new ViewModels.ErrorData();

                                    error_data.event_name = reservationConflictCheck.event_name;
                                    error_data.location_name = reservationConflictCheck.location_name;
                                    error_data.start_date = reservationConflictCheck.event_start_date;
                                    error_data.end_date = reservationConflictCheck.event_end_date;
                                    error_data.member_url = rsvpCheckoutData.member_url;

                                    rsvpResponse.error_info.error_data = error_data;
                                    return new ObjectResult(rsvpResponse);
                                }
                                else if (reservationConflictCheck.member_id != rsvpCheckoutData.member_id)
                                {
                                    if (reservationConflictCheck.event_start_date.TimeOfDay != rsvpCheckoutData.event_end_date.TimeOfDay || reservationConflictCheck.event_end_date.TimeOfDay != rsvpCheckoutData.event_start_date.TimeOfDay)
                                    {
                                        rsvpResponse.success = false;
                                        rsvpResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                                        rsvpResponse.error_info.extra_info = "Reservation Conflict Error";
                                        rsvpResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} on {4} from {1} to {2}.<br>Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                        //rsvpResponse.error_info.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                        var error_data = new ViewModels.ErrorData();

                                        error_data.event_name = reservationConflictCheck.event_name;
                                        error_data.location_name = reservationConflictCheck.location_name;
                                        error_data.start_date = reservationConflictCheck.event_start_date;
                                        error_data.end_date = reservationConflictCheck.event_end_date;
                                        error_data.member_url = rsvpCheckoutData.member_url;

                                        rsvpResponse.error_info.error_data = error_data;
                                        return new ObjectResult(rsvpResponse);
                                    }
                                }
                                else if (IsAdmin == false)
                                {
                                    if (reservationConflictCheck.event_start_date.TimeOfDay == rsvpCheckoutData.event_end_date.TimeOfDay || reservationConflictCheck.event_end_date.TimeOfDay == rsvpCheckoutData.event_start_date.TimeOfDay)
                                    {
                                        rsvpResponse.success = false;
                                        rsvpResponse.error_info.error_type = (int)Common.Common.ErrorType.RsvpBackToBack;
                                        rsvpResponse.error_info.extra_info = "Reservation Conflict Error";
                                        //rsvpResponse.error_info.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                        rsvpResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} on {4} from {1} to {2}.<br>Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                        var error_data = new ViewModels.ErrorData();

                                        error_data.event_name = reservationConflictCheck.event_name;
                                        error_data.location_name = reservationConflictCheck.location_name;
                                        error_data.start_date = reservationConflictCheck.event_start_date;
                                        error_data.end_date = reservationConflictCheck.event_end_date;
                                        error_data.member_url = rsvpCheckoutData.member_url;

                                        rsvpResponse.error_info.error_data = error_data;
                                        return new ObjectResult(rsvpResponse);
                                    }
                                    else if (reservationConflictCheck.member_id == rsvpCheckoutData.member_id)
                                    {
                                        rsvpResponse.success = false;
                                        rsvpResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                                        rsvpResponse.error_info.extra_info = "Reservation Conflict Error";
                                        //rsvpResponse.error_info.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                        rsvpResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} on {4} from {1} to {2}.<br>Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                        var error_data = new ViewModels.ErrorData();

                                        error_data.event_name = reservationConflictCheck.event_name;
                                        error_data.location_name = reservationConflictCheck.location_name;
                                        error_data.start_date = reservationConflictCheck.event_start_date;
                                        error_data.end_date = reservationConflictCheck.event_end_date;
                                        error_data.member_url = rsvpCheckoutData.member_url;

                                        rsvpResponse.error_info.error_data = error_data;
                                        return new ObjectResult(rsvpResponse);
                                    }
                                }
                            }

                            
                            if (eventModel.member_benefit_required || eventModel.account_type_required || (!string.IsNullOrWhiteSpace(eventModel.club_member_benefit) && eventModel.club_member_benefit.ToLower() != "none"))
                            {
                                var error_data = new ViewModels.ErrorData();

                                error_data.event_name = eventModel.event_name;
                                error_data.location_name = eventModel.event_location;
                                error_data.member_url = eventModel.member_url;
                                error_data.member_benefits_url = rsvpCheckoutData.member_benefits_url;

                                if (!string.IsNullOrEmpty(userDetail.email))
                                {
                                    EventModel eventModelValidate = eventDAL.GetEventById(rsvpCheckoutData.event_id,member_id);
                                    List<string> activationCodes = new List<string>();
                                    var result = await Services.Discount.CheckAndApplyEventDiscount(eventModelValidate, request_guest, rsvpCheckoutData.fee_Per_person, "", member_id, userDetail.email, activationCodes, request_date, rsvpCheckoutData.fee_type, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);


                                    if (result.DiscountValid)
                                    {
                                        rsvpCheckoutData.member_benefits_desc = result.DiscountDesc;
                                    }
                                    else if (result.AccessOnly)
                                    {
                                        rsvpCheckoutData.member_benefits_desc = "Thank you for being a valued member.";
                                    }
                                    else if (eventModel.member_benefit_required || eventModel.account_type_required)
                                    {
                                        if (result.ClubMember)
                                        {
                                            rsvpCheckoutData.member_benefits_desc = "Thank you for being a valued member.";
                                        }
                                        else
                                        {
                                            rsvpResponse.success = false;
                                            rsvpResponse.error_info = new ErrorInfo
                                            {
                                                error_type = (int)Common.Common.ErrorType.InvalidClubMember,
                                                description = "Sorry, you must be an active club member to reserve this event.",
                                                extra_info = string.Empty,
                                                error_data = error_data
                                            };
                                            return new ObjectResult(rsvpResponse);
                                        }
                                    }
                                }
                                else if (eventModel.member_benefit_required || eventModel.account_type_required)
                                {
                                    //error
                                    rsvpResponse.success = false;
                                    rsvpResponse.error_info = new ErrorInfo
                                    {
                                        error_type = (int)Common.Common.ErrorType.InvalidClubMember,
                                        description = "Sorry, you must be an active club member to reserve this event.",
                                        extra_info = string.Empty,
                                        error_data = error_data
                                    };
                                    return new ObjectResult(rsvpResponse);
                                }
                            }
                            else if (rsvpCheckoutData.fee_Per_person > 0)
                            {
                                var error_data = new ViewModels.ErrorData();

                                error_data.event_name = eventModel.event_name;
                                error_data.location_name = eventModel.event_location;
                                error_data.member_url = eventModel.member_url;
                                error_data.member_benefits_url = rsvpCheckoutData.member_benefits_url;

                                if (!string.IsNullOrEmpty(userDetail.email))
                                {
                                    EventModel eventModelValidate = eventDAL.GetEventById(rsvpCheckoutData.event_id,member_id);
                                    List<string> activationCodes = new List<string>();
                                    var result = await Services.Discount.CheckAndApplyEventDiscount(eventModelValidate, request_guest, rsvpCheckoutData.fee_Per_person, "", member_id, userDetail.email, activationCodes, request_date, rsvpCheckoutData.fee_type, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);


                                    if (result.DiscountValid)
                                    {
                                        rsvpCheckoutData.member_benefits_desc = result.DiscountDesc;
                                    }
                                    else if (result.AccessOnly)
                                    {
                                        rsvpCheckoutData.member_benefits_desc = "Thank you for being a valued member.";
                                    }
                                    else if (eventModel.member_benefit_required || eventModel.account_type_required)
                                    {
                                        //error
                                        rsvpResponse.success = false;
                                        rsvpResponse.error_info = new ErrorInfo
                                        {
                                            error_type = (int)Common.Common.ErrorType.InvalidClubMember,
                                            description = "Sorry, you must be an active club member to reserve this event.",
                                            extra_info = string.Empty,
                                            error_data = error_data
                                        };
                                        return new ObjectResult(rsvpResponse);
                                    }
                                }
                                else if (eventModel.member_benefit_required || eventModel.account_type_required)
                                {
                                    //error
                                    rsvpResponse.success = false;
                                    rsvpResponse.error_info = new ErrorInfo
                                    {
                                        error_type = (int)Common.Common.ErrorType.InvalidClubMember,
                                        description = "Sorry, you must be an active club member to reserve this event.",
                                        extra_info = string.Empty,
                                        error_data = error_data
                                    };
                                    return new ObjectResult(rsvpResponse);
                                }
                            }

                            Guid CartGUID = Guid.NewGuid();

                            if (!string.IsNullOrEmpty(userDetail.email))
                                eventDAL.SaveEventAbandoned(user_id, userDetail.email, member_id, slot_id, slot_type, request_guest, request_date, CartGUID);

                            rsvpCheckoutData.cart_guid = CartGUID;

                            //get user's favorite region
                            rsvpCheckoutData.favorite_region_id = userDAL.GetUserFavRegionById(user_id);
                            rsvpCheckoutData.show_winery_visit_question = !userDAL.IsReturningUser(user_id, member_id);

                        }
                        rsvpResponse.success = true;

                        List<Settings.Setting> twiliosettingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.twilio).ToList();

                        if (twiliosettingsGroup != null)
                            rsvpCheckoutData.twilio_disable_verification_service = Settings.GetBoolValue(twiliosettingsGroup, Common.Common.SettingKey.twilio_DisableVerificationService);

                        rsvpResponse.data = rsvpCheckoutData;
                    }
                }
                else
                {
                    rsvpResponse.success = false;

                    var reservationConflictCheck = new ReservationDetailModel();
                    reservationConflictCheck = eventDAL.IsReservationConflict(reservation_id, user_id, request_date, DateTime.Now.TimeOfDay, DateTime.Now.TimeOfDay, slot_id, slot_type);
                    if (reservationConflictCheck != null && reservationConflictCheck.reservation_id > 0)
                    {
                        rsvpResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                        rsvpResponse.error_info.extra_info = "Reservation Conflict Error";
                        //rsvpResponse.error_info.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                        rsvpResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} on {4} from {1} to {2}.<br>Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));

                        var error_data = new ViewModels.ErrorData();

                        error_data.event_name = reservationConflictCheck.event_name;
                        error_data.location_name = reservationConflictCheck.location_name;
                        error_data.start_date = reservationConflictCheck.event_start_date;
                        error_data.end_date = reservationConflictCheck.event_end_date;
                        error_data.member_url = rsvpCheckoutData.member_url;

                        rsvpResponse.error_info.error_data = error_data;
                        return new ObjectResult(rsvpResponse);
                    }

                    rsvpResponse.error_info = new ErrorInfo
                    {
                        error_type = (int)Common.Common.ErrorType.AvailableSeats,
                        description = "Sorry, the capacity of the event has changed. There is no longer space available.",
                        extra_info = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                rsvpResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                rsvpResponse.error_info.extra_info = Common.Common.InternalServerError;
                rsvpResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "ReservationCheckout:  request_date-" + request_date.ToString() + ",request_guest" + request_guest.ToString() + ",member_id" + member_id.ToString() + ",slot_id" + slot_id.ToString() + ",slot_type" + slot_type.ToString() + ",include_waitlist" + include_waitlist.ToString() + ",include_hidden_member" + include_hidden_member.ToString() + ",user_id" + user_id.ToString() + ",reservation_id" + reservation_id.ToString() + ",wl" + wl + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(rsvpResponse);
        }

        [Route("list")]
        [HttpGet]
        public IActionResult LoadEvents(int member_id)
        {
            var loadEventsResponse = new LoadEventsResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var events = new List<Event>();
                events = eventDAL.GetEvents(member_id);

                if (events != null && events.Count > 0)
                {
                    loadEventsResponse.success = true;
                    loadEventsResponse.data = events;
                }
                else
                {
                    loadEventsResponse.success = true;
                    loadEventsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    loadEventsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                loadEventsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                loadEventsResponse.error_info.extra_info = Common.Common.InternalServerError;
                loadEventsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "LoadEvents:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(loadEventsResponse);
        }

        [Route("eventaccess")]
        [HttpGet]
        public IActionResult LoadPrivateEventsByAccessCode(int member_id, string access_code, DateTime? request_date = null, int guests = 0)
        {
            var loadEventsResponse = new LoadPrivateEventsResponse();

            if (member_id <= 0)
            {
                loadEventsResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                loadEventsResponse.error_info.extra_info = "Invalid Member Id. Please pass a valid one.";
                loadEventsResponse.error_info.description = "Invalid Member Id. Please pass a valid one.";
                return new ObjectResult(loadEventsResponse);
            }
            else if (string.IsNullOrWhiteSpace(access_code))
            {
                loadEventsResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                loadEventsResponse.error_info.extra_info = "Invalid Acess Code";
                loadEventsResponse.error_info.description = "Invalid access code. Acess code is required";
                return new ObjectResult(loadEventsResponse);
            }
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                //step 1 check if this is a valid access code
                var eventAccess = eventDAL.CheckEventAccessCode(member_id, 0, access_code, 0);

                if (eventAccess == null || !eventAccess.IsValid)
                {
                    loadEventsResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                    loadEventsResponse.error_info.extra_info = "Invalid Acess Code";
                    loadEventsResponse.error_info.description = "Invalid access code. Acess code does not exist.";
                    return new ObjectResult(loadEventsResponse);
                }
                else if (eventAccess.IsValid && eventAccess.StartDate == null)
                {
                    loadEventsResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                    loadEventsResponse.error_info.extra_info = "Invalid Acess Code";
                    loadEventsResponse.error_info.description = string.Format("The '{0}' access code is no longer available", access_code);
                    return new ObjectResult(loadEventsResponse);
                }

                var events = new List<EventData>();
                events = eventDAL.GetPrivateEventsByAccessCode(member_id, access_code, request_date, guests);

                if (events != null && events.Count > 0)
                {
                    foreach (var item in events)
                    {
                        DateTime first_available_date = string.IsNullOrWhiteSpace(Convert.ToString(item.first_available_date)) ? Convert.ToDateTime("1/1/1900") : Convert.ToDateTime(item.first_available_date);

                        if (first_available_date.Year > 2000)
                        {
                            var eventtimes = new EventV3();
                            eventtimes = eventDAL.GetEventsByWineryIdAndDate(member_id, "", guests, first_available_date, item.event_id, -1, -1, false, false, false, false, access_code, 0, false);

                            if (eventtimes != null && eventtimes.events != null)
                                item.next_available_times = eventtimes.events[0].times;
                        }
                    }

                    loadEventsResponse.success = true;
                    loadEventsResponse.data = events;
                }
                else
                {
                    loadEventsResponse.success = true;
                    loadEventsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    loadEventsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                loadEventsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                loadEventsResponse.error_info.extra_info = Common.Common.InternalServerError;
                loadEventsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "LoadEvents:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(loadEventsResponse);
        }

        [Route("searcheventsbytimeandguests")]
        [HttpGet]
        public IActionResult ScheduleByRegion(DateTime request_date, int request_guest, string request_time, string request_str_search, bool include_waitlist = true, int region_id = 0)
        {
            var schedulev2sResponse = new Schedulev2sResponse();
            try
            {
                string PurchaseURL = "https://dev.cellarpass.com/";
                if (_appSetting.Value.QueueName == "emailqueue")
                    PurchaseURL = "https://www.cellarpass.com/";

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var events = new List<ScheduleV2>();

                if (request_date.Year > DateTime.Now.AddYears(-2).Year)
                {
                    events = eventDAL.GetEventsByTimeAndGuests(request_str_search, request_time, request_guest, request_date, include_waitlist, PurchaseURL, region_id);
                }

                if (events != null && events.Count > 0)
                {
                    schedulev2sResponse.success = true;
                    schedulev2sResponse.data = events;
                }
                else
                {
                    schedulev2sResponse.success = true;
                    schedulev2sResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    schedulev2sResponse.error_info.extra_info = "no record found";
                    schedulev2sResponse.error_info.description = "No availability. Select a different date or search criteria.";

                    // see if the search keyword belongs to any winery
                    var memberDetail = eventDAL.GetMemberByKeyword(request_str_search);

                    if (memberDetail != null)
                    {
                        string memberDetails = JsonConvert.SerializeObject(memberDetail);

                        var eventmember = new List<ScheduleV2>();
                        eventmember = eventDAL.GetEventsByMemberId(memberDetail.member_id);

                        if (eventmember != null && eventmember.Count > 0)
                            schedulev2sResponse.data = eventmember;

                        //get the next available date for the same member and guest count
                        AvailableEventsForFutureDate availableEventsForFutureDate = eventDAL.GetAvailableEventsForFutureDateV3(memberDetail.member_id, request_guest, request_date, -1, 0, 0, false, false, request_time, "");

                        if (availableEventsForFutureDate.event_date >= DateTime.UtcNow.Date)
                        {
                            string message = string.Format("The next date with availability for {0} guests is {1} at {2}.", request_guest, availableEventsForFutureDate.event_date.ToString("dddd, MMMM dd, yyyy"), availableEventsForFutureDate.start_time.ToString("hh:mm tt"));
                            schedulev2sResponse.error_info.error_type = (int)Common.Common.ErrorType.EventFutureDate;
                            schedulev2sResponse.error_info.extra_info = memberDetails;
                            schedulev2sResponse.error_info.description = message;

                        }
                        else
                        {
                            bool maxMinConditionFormed = false;

                            for (int i = 0; i < 30; i++)
                            {
                                MaxSeatsLeft maxSeatsLeft = eventDAL.GetMaxSeatsLeftByWineryIdAndDateV3(memberDetail.member_id, request_date.AddDays(i));

                                if (maxSeatsLeft.min_seats > 0 || maxSeatsLeft.max_seats > 0)
                                {
                                    if (maxSeatsLeft.min_seats > request_guest)
                                    {
                                        schedulev2sResponse.error_info.error_type = (int)Common.Common.ErrorType.MinSeats;
                                        schedulev2sResponse.error_info.extra_info = memberDetails;
                                        schedulev2sResponse.error_info.description = "The minimum number of " + maxSeatsLeft.min_seats.ToString() + " guests can be booked for this event.";
                                        maxMinConditionFormed = true;
                                        break;
                                    }
                                    else if (maxSeatsLeft.max_seats < request_guest)
                                    {
                                        schedulev2sResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxSeats;
                                        schedulev2sResponse.error_info.extra_info = memberDetails;
                                        schedulev2sResponse.error_info.description = "The maximum number of  " + maxSeatsLeft.max_seats.ToString() + " guests can be booked for this event.";
                                        maxMinConditionFormed = true;
                                        break;
                                    }
                                }

                            }
                            if (!maxMinConditionFormed)
                            {
                                DateTime MaxEndDate = eventDAL.GetMaxEndDateByWineryId(memberDetail.member_id);
                                if (MaxEndDate.Date < request_date.Date && MaxEndDate >= DateTime.UtcNow.Date)
                                {
                                    var winery = new Model.WineryModel();
                                    winery = eventDAL.GetWineryById(memberDetail.member_id);

                                    schedulev2sResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxEndDate;
                                    schedulev2sResponse.error_info.extra_info = memberDetails;
                                    schedulev2sResponse.error_info.description = winery.DisplayName + " accepts reservations up until " + MaxEndDate.ToString("dddd, MMMM dd, yyyy") + ". Contact " + Utility.FormatTelephoneNumber(winery.BusinessPhone.ToString(), winery.WineryAddress.country) + " or via " + winery.EmailAddress + " for more information.";
                                }
                                else
                                {
                                    schedulev2sResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                                    schedulev2sResponse.error_info.description = "We’re sorry, but there are no experiences available on " + request_date.ToString("MM/dd/yyyy");
                                    //if (model.event_id > 0 || model.slot_id > 0)
                                    //    loadScheduleResponse.error_info.description = "There is no availability for this experience on " + request_date.ToString("MM/dd/yyyy");
                                    //else
                                    //    schedulev2sResponse.error_info.description = "We’re sorry, but there are no experiences available on " + request_date.ToString("MM/dd/yyyy");
                                }
                            }

                        }

                    }

                }
            }
            catch (Exception ex)
            {
                schedulev2sResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                schedulev2sResponse.error_info.extra_info = Common.Common.InternalServerError;
                schedulev2sResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "searcheventsbytimeandguests:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(schedulev2sResponse);
        }

        [Route("searcheventsbymemberidanddate")]
        [HttpGet]
        public IActionResult GetEventsByWineryIdAndDate(DateTime request_date, int request_guest, int member_id = 0, string request_time = "", int event_id = -1, int slot_id = -1, int slot_type = -1, bool include_waitlist = true, bool include_hidden_member = false, bool show_images = false, int passport_event_id = 0, string access_code = "", int reservation_id = 0, bool show_other_ticket_events = true)
        {
            var eventv2sResponse = new Eventv2sResponse();

            string org_request_time = request_time;

            bool passport_event = false;
            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var events = new EventV3();

                if (request_date.Year < 2010)
                {
                    eventv2sResponse.success = true;
                    eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    eventv2sResponse.error_info.extra_info = "no record found";
                }
                else
                {
                    string HolidayName = eventDAL.IsHoliday(member_id, request_date);

                    if (!string.IsNullOrEmpty(HolidayName))
                    {
                        eventv2sResponse.error_info.holiday_name = HolidayName;
                        eventv2sResponse.error_info.holiday_description = "This property is closed on " + request_date.ToString("dddd, MMMM dd, yyyy");
                    }

                    if (include_hidden_member == false && member_id > 0)
                    {
                        if (eventDAL.GetWinery2ById(member_id).HiddenMember)
                        {
                            eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.HiddenMember;
                            eventv2sResponse.error_info.extra_info = HolidayName;
                            eventv2sResponse.error_info.description = "This member is hidden";
                            return new ObjectResult(eventv2sResponse);
                        }
                    }
                    passport_event = (passport_event_id > 0);

                    if (event_id > 0 || slot_id > 0)
                        request_time = "";

                    if (!string.IsNullOrWhiteSpace(access_code))
                    {
                        //check if event access code is valid
                        var eventAccess = eventDAL.CheckEventAccessCode(member_id, event_id, access_code, 0, slot_id, slot_type);

                        if (eventAccess == null || !eventAccess.IsValid)
                        {
                            eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                            eventv2sResponse.error_info.extra_info = "Invalid Acess Code";
                            eventv2sResponse.error_info.description = "Invalid access code. Acess code does not exist.";
                            return new ObjectResult(eventv2sResponse);
                        }
                        else if (eventAccess.IsValid && eventAccess.StartDate == null)
                        {
                            eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                            eventv2sResponse.error_info.extra_info = "Invalid Acess Code";
                            eventv2sResponse.error_info.description = string.Format("The '{0}' access code is no longer available", access_code);
                            return new ObjectResult(eventv2sResponse);
                        }
                    }

                    if (event_id > 0)
                        events = eventDAL.GetEventsByWineryIdAndDate(member_id, "", request_guest, request_date, event_id, slot_id, slot_type, include_waitlist, include_hidden_member, passport_event, show_images, access_code, reservation_id);
                    else
                        events = eventDAL.GetEventsByWineryIdAndDate(member_id, request_time, request_guest, request_date, event_id, slot_id, slot_type, include_waitlist, include_hidden_member, passport_event, show_images, access_code, reservation_id);

                    if (events != null)
                    {
                        eventv2sResponse.success = true;

                        if (passport_event)
                        {
                            //populate benefit desc and visitation rule

                            var passportMember = ticketDAL.GetPassportParticipatingMember(passport_event_id, member_id);

                            if (passportMember != null)
                            {
                                events.passport_benefit_desc = passportMember.benefit_desc;
                                events.visitation_rule = passportMember.visitation_rule;
                                events.visitation_rule_desc = passportMember.visitation_rule_desc;
                                events.visitation_external_url = passportMember.visitation_external_url;
                                events.passport_event_name = passportMember.passport_event_name;

                                if (events.visitation_rule > 0)
                                    events.show_book_button = true;

                                if (events.events != null)
                                {
                                    events.show_complementary_msg = (events.events.Where(f => f.passport_promoted_event == true).ToList().Count > 0);
                                }
                            }
                        }

                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.member).ToList();

                        if (settingsGroup != null && settingsGroup.Count > 0)
                        {
                            events.show_private_request_content = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_private_booking_requests);
                            events.private_booking_request_email = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_private_booking_request_email);
                        }

                        eventv2sResponse.data = events;

                        if (events.events == null)
                        {
                            AvailableEventsForFutureDate availableEventsForFutureDate = eventDAL.GetAvailableEventsForFutureDateV3(member_id, request_guest, request_date, event_id, slot_id, slot_type, include_waitlist, include_hidden_member, request_time, access_code);
                            if (availableEventsForFutureDate.event_date >= request_date.Date)
                            {
                                eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.EventFutureDate;
                                eventv2sResponse.error_info.extra_info = availableEventsForFutureDate.event_date.ToString();
                                //eventv2sResponse.error_info.description = "The next date with availability for " + request_guest + " guests is " + availableEventsForFutureDate.event_date.ToString("dddd, MMMM dd, yyyy") + " at " + availableEventsForFutureDate.start_time.ToString("hh:mm tt") + ".";
                                if (event_id > 0 || slot_id > 0)
                                    eventv2sResponse.error_info.description = "There is no availability for this experience on " + request_date.ToString("MM/dd/yyyy");
                                else
                                    eventv2sResponse.error_info.description = "We’re sorry, but there are no experiences available on " + request_date.ToString("MM/dd/yyyy");

                            }
                            else
                            {
                                MaxSeatsLeft maxSeatsLeft = new MaxSeatsLeft();
                                bool maxMinConditionFormed = false;

                                for (int i = 0; i < 30; i++)
                                {
                                    maxSeatsLeft = new MaxSeatsLeft();
                                    maxSeatsLeft = eventDAL.GetMaxSeatsLeftByWineryIdAndDateV3(member_id, request_date.AddDays(i), event_id, slot_id, slot_type);
                                    if (maxSeatsLeft.min_seats > 0 || maxSeatsLeft.max_seats > 0 || maxSeatsLeft.min_seats == -1 || maxSeatsLeft.max_seats == -1)
                                        break;
                                }

                                if (maxSeatsLeft.min_seats > 0 || maxSeatsLeft.max_seats > 0)
                                {
                                    if (maxSeatsLeft.min_seats > request_guest)
                                    {
                                        var winery = new Model.WineryModel();
                                        winery = eventDAL.GetWinery2ById(member_id);

                                        maxMinConditionFormed = true;
                                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.MinSeats;
                                        eventv2sResponse.error_info.extra_info = maxSeatsLeft.min_seats.ToString();
                                        eventv2sResponse.error_info.description = "We’re sorry, but your party is too small to book a reservation at " + winery.DisplayName + ".";
                                    }
                                    else if (maxSeatsLeft.max_seats < request_guest)
                                    {
                                        var winery = new Model.WineryModel();
                                        winery = eventDAL.GetWinery2ById(member_id);

                                        maxMinConditionFormed = true;
                                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxSeats;
                                        eventv2sResponse.error_info.extra_info = maxSeatsLeft.max_seats.ToString();
                                        eventv2sResponse.error_info.description = "We’re sorry, but your party is too large to book a reservation at " + winery.DisplayName + ".";
                                    }
                                }

                                if (!maxMinConditionFormed)
                                {
                                    DateTime MaxEndDate = eventDAL.GetMaxEndDateByWineryId(member_id, event_id, slot_id, slot_type);
                                    if (MaxEndDate.Date < request_date.Date && MaxEndDate >= DateTime.UtcNow.Date)
                                    {
                                        var winery = new Model.WineryModel();
                                        winery = eventDAL.GetWinery2ById(member_id);

                                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxEndDate;
                                        eventv2sResponse.error_info.extra_info = MaxEndDate.ToString();
                                        eventv2sResponse.error_info.description = winery.DisplayName + " accepts reservations up until " + MaxEndDate.ToString("dddd, MMMM dd, yyyy") + ". Contact " + Utility.FormatTelephoneNumber(winery.BusinessPhone.ToString(), winery.WineryAddress.country) + " or via " + winery.EmailAddress + " for more information.";
                                    }
                                    else
                                    {
                                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                                        if (event_id > 0 || slot_id > 0)
                                            eventv2sResponse.error_info.description = "There is no availability for this experience on " + request_date.ToString("MM/dd/yyyy");
                                        else
                                            eventv2sResponse.error_info.description = "We’re sorry, but there are no experiences available on " + request_date.ToString("MM/dd/yyyy");
                                    }
                                }

                            }

                            if (slot_id > 0 || event_id > 0)
                            {
                                events = eventDAL.GetEventDetailsByEventId(member_id, event_id, slot_id, slot_type, show_images);

                                if (events != null && events.events != null)
                                {
                                    MaxSeatsLeft maxSeatsLeft = new MaxSeatsLeft();
                                    maxSeatsLeft = eventDAL.GetMaxSeatsLeftByWineryIdAndDateV3(member_id, request_date, event_id, slot_id, slot_type);

                                    events.events[0].max_persons = maxSeatsLeft.max_seats;
                                    events.events[0].min_persons = maxSeatsLeft.min_seats;
                                }
                                
                                if (availableEventsForFutureDate.event_date > request_date.Date)
                                {
                                    var events2 = new EventV3();
                                    events2 = eventDAL.GetEventsByWineryIdAndDate(member_id, "", request_guest, availableEventsForFutureDate.event_date, event_id, slot_id, slot_type, include_waitlist, include_hidden_member, passport_event, show_images, access_code, reservation_id);

                                    if (events2 != null && events2.events != null)
                                    {
                                        events.events[0].next_available_times = events2.events[0].times;
                                        events.events[0].max_persons = events2.events[0].max_persons;
                                        events.events[0].min_persons = events2.events[0].min_persons;
                                        events.events[0].duration = events2.events[0].duration;
                                    }
                                }

                                eventv2sResponse.data = events;
                            }
                            else if (availableEventsForFutureDate.event_date > request_date.Date)
                            {
                                events = eventDAL.GetEventsByWineryIdAndDate(member_id, "", request_guest, availableEventsForFutureDate.event_date, event_id, slot_id, slot_type, include_waitlist, include_hidden_member, passport_event, show_images, access_code, reservation_id);

                                if (events != null && events.events != null)
                                {
                                    List<EventV2> eventsv2 = events.events;
                                    foreach (var item in eventsv2)
                                    {
                                        item.next_available_times = item.times;
                                        item.times = null;
                                    }

                                    eventv2sResponse.data = events;
                                }
                            }
                        }

                        List<TicketEventDetail2Model> listtict = new List<TicketEventDetail2Model>();
                        if (member_id > 0)
                        {
                            listtict = ticketDAL.GetTicketEventDetail2ByWineryId(member_id, request_date);

                            if (show_other_ticket_events && listtict.Count == 0)
                            {
                                listtict = ticketDAL.GetTicketEventDetail2ByRegionId(member_id, request_date);
                            }
                            eventv2sResponse.data.ticket_events = listtict;
                        }
                    }
                    else
                    {
                        eventv2sResponse.success = true;
                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        eventv2sResponse.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventv2sResponse.error_info.extra_info = Common.Common.InternalServerError;
                eventv2sResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                string req = string.Format("request_date-{0},request_guest-{1},member_id-{2},request_time-{3},event_id-{4},slot_id-{5},slot_type-{6},include_waitlist-{7},include_hidden_member-{8},show_images -{9},passport_event_id-{10},access_code-{11},reservation_id-{12},show_other_ticket_events-{13}", request_date, request_guest, member_id ,  request_time,  event_id ,  slot_id , slot_type ,  include_waitlist ,  include_hidden_member , show_images, passport_event_id , access_code ,  reservation_id , show_other_ticket_events);
                logDAL.InsertLog("WebApi", "GetEventsByWineryIdAndDate:  req:" + req + ",Message: " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(eventv2sResponse);
        }

        [Route("searcheventsbyeventidanddate")]
        [HttpGet]
        public IActionResult GetEventsByEventIdAndDate(DateTime request_date, int request_guest, int member_id, int event_id)
        {
            var eventv2sResponse = new EventDatev2sResponse();

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var events = new EventDateV3();

                if (request_date.Year < 2010)
                {
                    eventv2sResponse.success = true;
                    eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    eventv2sResponse.error_info.extra_info = "no record found";
                }
                else
                {
                    if (event_id > 0)
                        events = eventDAL.GetEventsByEventIdAndDate(member_id, request_guest, request_date, event_id);

                    if (events != null)
                    {
                        eventv2sResponse.success = true;

                        eventv2sResponse.data = events;
                    }
                    else
                    {
                        eventv2sResponse.success = true;
                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        eventv2sResponse.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventv2sResponse.error_info.extra_info = Common.Common.InternalServerError;
                eventv2sResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventsByEventIdAndDate:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(eventv2sResponse);
        }


        [Route("memberprofileeventlist")]
        [HttpGet]
        public IActionResult GetEventsForWineryProfile(int member_id)
        {
            var response = new ProfileEventsResponse();

            try
            {
                //string PurchaseURL = "https://dev.cellarpass.com/";
                //if (_appSetting.Value.QueueName == "emailqueue")
                //    PurchaseURL = "https://www.cellarpass.com/";

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var events = new List<ProfileEvent>();

                if (member_id <= 0)
                {
                    response.success = false;
                    response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    response.error_info.extra_info = "Invalid Member Id. Please pass a valid member Id";
                }
                else
                {

                    events = eventDAL.GetEventsForMemberProfile(member_id);

                    if (events != null && events.Count > 0)
                    {
                        response.success = true;
                        response.data = events;
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
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventsForWineryProfile:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(response);
        }

        [Route("getnextavailabledaysforevent")]
        [HttpGet]
        public IActionResult GetNextAvailableDaysForEvent(DateTime request_date, int request_guest, int event_id, bool include_waitlist = true, bool include_hidden_member = false, bool passport_event = false)
        {
            var eventv2sResponse = new AvailableEventsResponse();
            if (event_id <= 0)
            {
                eventv2sResponse.success = false;
                eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.EventError;
                eventv2sResponse.error_info.extra_info = "Invalid Event Id";
                return new ObjectResult(eventv2sResponse);
            }

            string debugInfo = string.Format("request_date: {0}, request_guest: {1}, event_id: {2}", request_date.ToShortDateString(), request_guest.ToString(), event_id.ToString());

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var events = new AvailableDaysEvent();
                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));

                events = eventDAL.GetAvailableDaysForEvent(request_guest, request_date, event_id, include_waitlist, include_hidden_member, passport_event);

                if (events != null)
                {
                    eventv2sResponse.success = true;
                    eventv2sResponse.data = events;
                }
                else
                {
                    eventv2sResponse.success = true;
                    eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    eventv2sResponse.error_info.extra_info = "no record found";
                }

            }
            catch (Exception ex)
            {
                eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventv2sResponse.error_info.extra_info = Common.Common.InternalServerError;
                eventv2sResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetNextAvailableDaysForEvent::  data = " + debugInfo + ", error:" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(eventv2sResponse);
        }


        [Route("eventtransportationlist")]
        [HttpGet]
        public IActionResult GetEventTransportationList(int event_id)
        {
            var evtTransportResponse = new EventTransportationResponse();
            try
            {

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var lst = eventDAL.GetEventTransportationList(event_id);

                if (lst != null)
                {
                    evtTransportResponse.success = true;
                    evtTransportResponse.data = lst;
                }
                else
                {
                    evtTransportResponse.success = true;
                    evtTransportResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    evtTransportResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                evtTransportResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                evtTransportResponse.error_info.extra_info = Common.Common.InternalServerError;
                evtTransportResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventTransportationList:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(evtTransportResponse);
        }

        [Route("opentablelookup")]
        [HttpGet]
        public IActionResult GetOpenTableDataForMember(int member_id = 0, int rid = 0)
        {
            var openTableLookupResponse = new OpenTableLookupResponse();
            if (member_id == 0 && rid == 0)
            {
                openTableLookupResponse.success = false;
                openTableLookupResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                openTableLookupResponse.error_info.extra_info = "Member Id or Rid of Open table member is required.";
                return new ObjectResult(openTableLookupResponse);
            }
            try
            {

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var lst = eventDAL.GetOpenTableMemberData(member_id, rid);

                if (lst != null)
                {
                    openTableLookupResponse.success = true;
                    openTableLookupResponse.data = lst;
                }
                else
                {
                    openTableLookupResponse.success = true;
                    openTableLookupResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    openTableLookupResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                openTableLookupResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                openTableLookupResponse.error_info.extra_info = Common.Common.InternalServerError;
                openTableLookupResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetOpenTableDataForMember:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(openTableLookupResponse);
        }

        [Route("syncopentabledata")]
        [HttpGet]
        public IActionResult BatchAddUpdateOpenTableMemberData()
        {
            var openTableResponse = new BaseResponse();
            try
            {
                int offset = 0;
                int limit = 50;
                int otoffset = 0;
                int otlimit = 1000;
                int totalRecords = otlimit;
                int pageCount = 0;
                int pageCountApi = 0;
                pageCount = totalRecords / limit;
                pageCountApi = totalRecords / otlimit;
                List<OpenTableMemberModel> itemlist = new List<OpenTableMemberModel>();

                for (int i = 0; i < pageCountApi; i++)
                {
                    Services.Opentable objOpenTable = new Services.Opentable(_appSetting);
                    Task<string> transResult = Opentable.GetOpenTableList(otoffset, otlimit);

                    // Wait for the GetOpenTableList task to complete.
                    // ... Display its results.
                    transResult.Wait();
                    var result = transResult.Result;

                    if (!String.IsNullOrWhiteSpace(result))
                    {
                        OpenTableDirectoryListResponseModel resultToken = JsonConvert.DeserializeObject<OpenTableDirectoryListResponseModel>(result);
                        if (resultToken != null)
                        {
                            List<OpenTableMemberModel> itemListApi = resultToken.items;
                            itemlist = itemlist.Union(itemListApi).OrderBy(x => x.rid).ToList();
                            //parameter to set for loop data to insert in database 
                            totalRecords = resultToken.total_items;
                            double pCount = (double)totalRecords / limit;
                            if (!CPReservationApi.Common.StringHelpers.IsInteger(pCount))
                                pageCount = Convert.ToInt32(Math.Floor(pCount)) + 1;

                            //parameter to set for loop data to get from the opentable api
                            otoffset = otoffset + otlimit;
                            double pCountApi = (double)totalRecords / otlimit;
                            if (!CPReservationApi.Common.StringHelpers.IsInteger(pCountApi))
                                pageCountApi = Convert.ToInt32(Math.Floor(pCountApi)) + 1;
                            //each response success
                            openTableResponse.success = true;

                        }
                        else
                        {
                            openTableResponse.error_info.error_type = (int)Common.Common.ErrorType.OpenTableError;
                            openTableResponse.error_info.extra_info = Common.Common.InternalServerError;
                            openTableResponse.error_info.description = "Error while converting the data to model";

                        }
                    }
                    else
                    {
                        openTableResponse.error_info.error_type = (int)Common.Common.ErrorType.OpenTableError;
                        openTableResponse.error_info.extra_info = "";
                        openTableResponse.error_info.description = "Error while executing opentable api. API authentication failed";

                    }
                }


                //To check all data is added to the temporary list
                if (itemlist.Count() == totalRecords)
                {
                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                    pageCount = totalRecords / limit;
                    for (int i = 0; i <= pageCount; i++)
                    {
                        List<OpenTableMemberModel> pagedDbList = new List<OpenTableMemberModel>();
                        pagedDbList = itemlist.Skip((i - 1) * limit).Take(limit).ToList();
                        var serializeItems = JsonConvert.SerializeObject(pagedDbList);

                        eventDAL.InsertAndUpdateOpenTableMember(serializeItems);
                    }
                    eventDAL.ReconcileOpenTableData();
                }



            }
            catch (Exception ex)
            {
                openTableResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                openTableResponse.error_info.extra_info = Common.Common.InternalServerError;
                openTableResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "BatchAddUpdateOpenTableMemberData:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(openTableResponse);
        }

        [Route("fareharborlookup")]
        [HttpGet]
        public IActionResult GetFareharborCompanies()
        {
            var fareharborLookupResponse = new FareharborLookupResponse();
            try
            {

                Services.Fareharbor objFareharbor = new Services.Fareharbor(_appSetting);
                FareharborModel modelList = Fareharbor.GetFareharborCompanies();

                if (modelList != null)
                {
                    fareharborLookupResponse.success = true;
                    fareharborLookupResponse.data = modelList;
                }
                else
                {
                    fareharborLookupResponse.success = true;
                    fareharborLookupResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    fareharborLookupResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                fareharborLookupResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                fareharborLookupResponse.error_info.extra_info = Common.Common.InternalServerError;
                fareharborLookupResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetFareharborCompanies:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(fareharborLookupResponse);
        }

        [Route("fareharborcompanyitemlookup")]
        [HttpGet]
        public IActionResult GetFareharborCompanyItem(int member_id)
        {
            var fareharborCompanyItemLookupResponse = new FareharborCompanyItemLookupResponse();
            try
            {

                Services.Fareharbor objFareharbor = new Services.Fareharbor(_appSetting);
                FareharborCompanyItem modelList = Fareharbor.GetFareharborCompanyItem(member_id);

                if (modelList != null)
                {
                    fareharborCompanyItemLookupResponse.success = true;
                    fareharborCompanyItemLookupResponse.data = modelList;
                }
                else
                {
                    fareharborCompanyItemLookupResponse.success = true;
                    fareharborCompanyItemLookupResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    fareharborCompanyItemLookupResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                fareharborCompanyItemLookupResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                fareharborCompanyItemLookupResponse.error_info.extra_info = Common.Common.InternalServerError;
                fareharborCompanyItemLookupResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetFareharborCompanyItem:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(fareharborCompanyItemLookupResponse);
        }

        [Route("synctripadvisorratings")]
        [HttpGet]
        public IActionResult GetTripAdvisorRatingsForMembers()
        {
            var tripadvisorResponse = new BaseResponse();
            try
            {

                Services.TripAdvisor tripAdvisor = new Services.TripAdvisor(_appSetting);
                bool isSuccess = Task.Run(() => TripAdvisor.UpdateTripadvisorReviews()).Result;

                tripadvisorResponse.success = isSuccess;
            }
            catch (Exception ex)
            {
                tripadvisorResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                tripadvisorResponse.error_info.extra_info = Common.Common.InternalServerError;
                tripadvisorResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetTripAdvisorRatingsForMembers:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(tripadvisorResponse);
        }

        [Route("creategoogleindexer")]
        [HttpGet]
        public IActionResult CreateGoogleIndexer()
        {
            var googleIndexerResponse = new BaseResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                foreach (var item in eventDAL.GetGoogleIndexerList())
                {
                    bool ret = GoogleIndexer.AddUrl(item);

                    if (ret)
                    {
                        eventDAL.UpdateGoogleIndexerStatus(item);
                    }
                }
            }
            catch (Exception ex)
            {
                googleIndexerResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                googleIndexerResponse.error_info.extra_info = Common.Common.InternalServerError;
                googleIndexerResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "CreateGoogleIndexer:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(googleIndexerResponse);
        }

        [Route("zoomtest")]
        [HttpGet]
        public IActionResult ZoomTokenTest()
        {
            var zoomtokenResponse = new BaseResponse();
            try
            {
                var token = Task.Run(() => ZoomMeeting.GetZoomToken(26)).Result;
            }
            catch (Exception ex)
            {
                zoomtokenResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                zoomtokenResponse.error_info.extra_info = Common.Common.InternalServerError;
                zoomtokenResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "ZoomTokenTest:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(zoomtokenResponse);
        }

        [Route("visitorstatistics")]
        [HttpGet]
        public IActionResult GetVisitorTrackingStatistics(int member_id, DateTime start_date, DateTime end_date, int tix_event_id = 0)
        {
            var response = new VisitorTrackingResponse();
            if (member_id == 0 && tix_event_id == 0)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Member Id or ticket evcent Id of member is required.";
                return new ObjectResult(response);
            }
            try
            {

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var trakingData = eventDAL.GetVisitorStaticByMemberOrEvent(member_id, start_date, end_date, tix_event_id);

                if (trakingData != null)
                {
                    response.success = true;
                    response.data = trakingData;
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
                logDAL.InsertLog("WebApi", "visitorstatistics:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(response);
        }

        [Route("searchmosteventbooked")]
        [HttpGet]
        public IActionResult MostBookedEvent(int event_type = 0, int region_id = 0)
        {
            var mostBookedResponse = new MostBookedEventResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var events = new List<MostBookedEvent>();

                events = eventDAL.GetMostBookedEvent(event_type, region_id);

                if (events != null && events.Count > 0)
                {
                    mostBookedResponse.success = true;
                    mostBookedResponse.data = events;
                }
                else
                {
                    mostBookedResponse.success = true;
                    mostBookedResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    mostBookedResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                mostBookedResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                mostBookedResponse.error_info.extra_info = Common.Common.InternalServerError;
                mostBookedResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "searchmosteventbooked:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(mostBookedResponse);
        }

        [Route("searchmosteventtypebooked")]
        [HttpGet]
        public IActionResult MostBookedEventType(int region_id = 0)
        {
            var mostBookedResponse = new MostBookedEventTypeResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var events = new List<MostBookedEventType>();

                events = eventDAL.GetMostBookedEventType(region_id);

                if (events != null && events.Count > 0)
                {
                    mostBookedResponse.success = true;
                    mostBookedResponse.data = events;
                }
                else
                {
                    mostBookedResponse.success = true;
                    mostBookedResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    mostBookedResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                mostBookedResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                mostBookedResponse.error_info.extra_info = Common.Common.InternalServerError;
                mostBookedResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "searchmosteventtypebooked:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(mostBookedResponse);
        }

        [Route("guesttags")]
        [HttpGet]
        public IActionResult GetGuestTags(int member_id, bool show_all_tags = false)
        {
            var guestTagsResponse = new GuestTagsResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var list = new List<GuestTags>();

                list = eventDAL.GetGuestTagsByMemberId(member_id, show_all_tags);
                if (list != null)
                {
                    guestTagsResponse.success = true;
                    guestTagsResponse.data = list;
                }
                else
                {
                    guestTagsResponse.success = true;
                    guestTagsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    guestTagsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                guestTagsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                guestTagsResponse.error_info.extra_info = Common.Common.InternalServerError;
                guestTagsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetGuestTags:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(guestTagsResponse);
        }

        [Route("purgecdnasset")]
        [HttpPost]
        public IActionResult purcgecdnasset([FromBody]AddTocdnRequest req)
        {
            var resp = new BaseResponse();

            try
            {
                resp.success = Utility.purcgecdnasset(req.file_path);
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "AddTocdn:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(resp);
        }

        [Route("availableqtyforprivatereservation")]
        [HttpGet]
        public IActionResult GetAvailableQtyForPrivateReservation(int member_id, int floor_plan_id, DateTime start_date, DateTime end_date)
        {
            var availableQtyForPrivateReservationResponse = new AvailableQtyForPrivateReservationResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                int AvailableQty = eventDAL.GetAvailableQtyForPrivateReservation(member_id, floor_plan_id, start_date, end_date);

                availableQtyForPrivateReservationResponse.data.count = AvailableQty;
                availableQtyForPrivateReservationResponse.success = true;
            }
            catch (Exception ex)
            {
                availableQtyForPrivateReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                availableQtyForPrivateReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                availableQtyForPrivateReservationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetAvailableQtyForPrivateReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(availableQtyForPrivateReservationResponse);
        }

        [Route("checkavailableqtyprivatersvp")]
        [HttpGet]
        public IActionResult CheckAvailableQtyPrivatersvp(int reservation_id, int total_guests)
        {
            var checkAvailableQtyPrivatersvpResponse = new CheckAvailableQtyPrivatersvpResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                CheckAvailableQtyPrivatersvpModel model = eventDAL.CheckifTableAvailableCanFitGuestForPrivateRSVP(reservation_id, total_guests);

                checkAvailableQtyPrivatersvpResponse.data = model;
                checkAvailableQtyPrivatersvpResponse.success = true;
            }
            catch (Exception ex)
            {
                checkAvailableQtyPrivatersvpResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                checkAvailableQtyPrivatersvpResponse.error_info.extra_info = Common.Common.InternalServerError;
                checkAvailableQtyPrivatersvpResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "CheckAvailableQtyPrivatersvp:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(checkAvailableQtyPrivatersvpResponse);
        }

        [Route("getpassporteventavailability")]
        [HttpGet]
        public async Task<IActionResult> GetPassportEventsAvailability(int event_id, int guests, DateTime request_date, int user_id, int itinerary_id = 0, bool include_waitlist = false, int item_id = 0, string rsvp_access_codes = "")
        {
            var availabilityResponse = new GetPassportEventAvailabilityResponse();
            if (event_id == 0 || guests == 0)
            {
                availabilityResponse.success = false;
                availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                availabilityResponse.error_info.extra_info = "Event Id and number of guests are required.";
                return new ObjectResult(availabilityResponse);
            }
            try
            {
                if (request_date.Year < 2010)
                {
                    availabilityResponse.success = true;
                    availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    availabilityResponse.error_info.extra_info = "no record found";
                    return new ObjectResult(availabilityResponse);
                }

                int member_id = 0;
                //check if the event is a passport event
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                bool isPassportEvent = eventDAL.CheckPassportEvent(event_id, ref member_id);

                if (!isPassportEvent || member_id == 0)
                {
                    availabilityResponse.success = false;
                    availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    availabilityResponse.error_info.extra_info = "This event is not an passport event. Invalid Event Id";
                    return new ObjectResult(availabilityResponse);
                }
                //check if holiday on that date
                string HolidayName = eventDAL.IsHoliday(member_id, request_date);
                if (!string.IsNullOrEmpty(HolidayName))
                {
                    availabilityResponse.success = false;
                    availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.IsHoliday;
                    availabilityResponse.error_info.extra_info = HolidayName;
                    availabilityResponse.error_info.description = "This property is closed on " + request_date.ToString("dddd, MMMM dd, yyyy");
                    return new ObjectResult(availabilityResponse);
                }
                //get all the events for that member

                var availabilityList = eventDAL.GetPassortEventAvailability(event_id, member_id, guests, request_date, itinerary_id, include_waitlist, item_id, rsvp_access_codes);

                if (availabilityList != null && availabilityList.Count > 0)
                {
                    availabilityResponse.success = true;
                    availabilityResponse.data = availabilityList;
                }
                else
                {
                    availabilityResponse.success = true;
                    availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    availabilityResponse.error_info.extra_info = "no record found";

                }
            }
            catch (Exception ex)
            {
                availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                availabilityResponse.error_info.extra_info = Common.Common.InternalServerError;
                availabilityResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPassportEventsAvailability:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }

            return new ObjectResult(availabilityResponse);
        }

        [Route("getpassporteventavailabilityv2")]
        [HttpGet]
        public async Task<IActionResult> GetPassportEventsAvailabilityV2(int event_id, int guests, DateTime start_date, DateTime end_date, int user_id, int itinerary_id = 0, bool include_waitlist = false, int item_id = 0, string rsvp_access_codes = "")
        {
            var availabilityResponse = new GetPassportEventAvailabilityV2Response();
            if (event_id == 0 || guests == 0)
            {
                availabilityResponse.success = false;
                availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                availabilityResponse.error_info.extra_info = "Event Id and number of guests are required.";
                return new ObjectResult(availabilityResponse);
            }
            try
            {
                if (start_date.Year < 2010 || end_date.Year < 2010)
                {
                    availabilityResponse.success = true;
                    availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    availabilityResponse.error_info.extra_info = "no record found";
                    return new ObjectResult(availabilityResponse);
                }

                int member_id = 0;
                //check if the event is a passport event
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                bool isPassportEvent = eventDAL.CheckPassportEvent(event_id, ref member_id);

                if (!isPassportEvent || member_id == 0)
                {
                    availabilityResponse.success = false;
                    availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    availabilityResponse.error_info.extra_info = "This event is not an passport event. Invalid Event Id";
                    return new ObjectResult(availabilityResponse);
                }
                //check if holiday on that date
                //string HolidayName = eventDAL.IsHoliday(member_id, request_date);
                //if (!string.IsNullOrEmpty(HolidayName))
                //{
                //    availabilityResponse.success = false;
                //    availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.IsHoliday;
                //    availabilityResponse.error_info.extra_info = HolidayName;
                //    availabilityResponse.error_info.description = "This property is closed on " + request_date.ToString("dddd, MMMM dd, yyyy");
                //    return new ObjectResult(availabilityResponse);
                //}
                //get all the events for that member

                var availabilityList = eventDAL.GetPassortEventAvailabilityV2(event_id, member_id, guests, start_date, end_date, itinerary_id, include_waitlist, item_id, rsvp_access_codes);

                if (availabilityList != null && availabilityList.Count > 0)
                {
                    availabilityResponse.success = true;
                    availabilityResponse.data = availabilityList;
                }
                else
                {
                    availabilityResponse.success = true;
                    availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    availabilityResponse.error_info.extra_info = "no record found";

                }
            }
            catch (Exception ex)
            {
                availabilityResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                availabilityResponse.error_info.extra_info = Common.Common.InternalServerError;
                availabilityResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPassportEventsAvailability:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }

            return new ObjectResult(availabilityResponse);
        }

        public class ReCaptchaResponse
        {
            public bool Success;
            public string ChallengeTs;
            public string Hostname;
            public object[] ErrorCodes;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("captcha")]
        public bool Captcha([FromBody] string token)
        {
            bool isHuman = true;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            try
            {
                string secretKey = _appSetting.Value.GoogleReCaptchaSecretKey;
                Uri uri = new Uri("https://www.google.com/recaptcha/api/siteverify" +
                                  $"?secret={secretKey}&response={token}");

                logDAL.InsertLog("WebApi", "GoogleReCaptchaRequest:  " + uri.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3,0);

                HttpWebRequest request = WebRequest.CreateHttp(uri);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = 0;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                string result = streamReader.ReadToEnd();

                logDAL.InsertLog("WebApi", "GoogleReCaptchaResponse:  " + result, HttpContext.Request.Headers["AuthenticateKey"], 3, 0);

                ReCaptchaResponse reCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(result);
                isHuman = reCaptchaResponse.Success;
            }
            catch (Exception ex)
            {
                //Trace.WriteLine("reCaptcha error: " + ex);
            }

            return isHuman;
        }

        [Route("saveprivateeventrequest")]
        [HttpPost]
        public async Task<IActionResult> SavePrivateEventRequest([FromBody]PrivateEventRequest reqmodel)
        {
            var resp = new PrivateEventFormSubmittedResponse();

            try
            {
                if (string.IsNullOrEmpty(reqmodel.captcha_response))
                {
                    resp.success = false;
                    resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    resp.error_info.extra_info = "";
                    resp.error_info.description = "Captch response is required.";
                    return new ObjectResult(resp);
                }

                if (!Captcha(reqmodel.captcha_response))
                {
                    resp.success = false;
                    resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    resp.error_info.extra_info = "CAPTCHA validation failed.";
                    resp.error_info.description = "CAPTCHA validation failed. Please try again.";
                    return new ObjectResult(resp);
                }

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                resp.data = eventDAL.SavePrivateEventRequest(reqmodel);

                if (resp.data != null && resp.data.id > 0)
                {
                    resp.success = true;
                    QueueService getStarted = new QueueService(_appSetting);

                    var queueModel = new EmailQueue();
                    queueModel.EType = (int)EmailType.PrivateEventRequest;
                    queueModel.RsvpId = resp.data.id;

                    var qData = JsonConvert.SerializeObject(queueModel);

                    AppSettings _appsettings = _appSetting.Value;
                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                }
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SavePrivateEventRequest:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);
            }
            return new ObjectResult(resp);
        }

        [Route("privateeventrequestdetails")]
        [HttpGet]
        public async Task<IActionResult> GetPrivateEventRequestDetails(int id = 0, string private_event_guid = "")
        {
            var resp = new PrivateEventDetailsResponse();

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                PrivateEventRequestDetails model = eventDAL.GetPrivateEventRequestDetails(id, private_event_guid);
                if (model != null && model.id > 0)
                {
                    model.preferred_visit_duration_desc = Utility.GetPreferredVisitDuration().Where(a => a.id == model.preferred_visit_duration).FirstOrDefault().name;
                    model.reason_for_visit_desc = Utility.GetReasonforVisit().Where(a => a.id == model.reason_for_visit).FirstOrDefault().name;

                    SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(model.member_id, (int)Common.Common.SettingGroup.member).ToList();

                    if (settingsGroup != null && settingsGroup.Count > 0)
                    {
                        model.show_private_request_content = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_private_booking_requests);
                        model.private_booking_request_email = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_private_booking_request_email);
                    }

                    resp.success = true;
                    resp.data = model;
                }
                else
                {
                    resp.success = true;
                    resp.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    resp.error_info.extra_info = "no record found";

                }
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPrivateEventRequestDetails:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }

            return new ObjectResult(resp);
        }

        /// <summary>
        /// This method would search for events by date, guest count etc for a member
        /// </summary>
        /// <param name="model">Load Schedule Request Model</param>
        /// <returns></returns>

        [Route("geteventdataschedule")]
        [HttpGet]
        public IActionResult LoadScheduleEvent(LoadScheduleRequest model)
        {
            var loadScheduleEventResponse = new LoadScheduleEventIdResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                bool IsAdmin = Convert.ToBoolean(Request.Headers["IsAdmin"]);

                var eventSchedule = eventDAL.GetEventScheduleListV2(model.member_id, model.req_date, model.guest_count, model.slot_id, model.slot_type, model.event_id, IsAdmin, model.rsvp_id, model.booking_type, model.hide_no_availability);
                if (eventSchedule.Count > 0)
                {
                    loadScheduleEventResponse.success = true;
                    loadScheduleEventResponse.data = eventSchedule;
                }
                else if (model.booking_type == 0)
                {
                    AvailableEventsForFutureDate availableEventsForFutureDate = eventDAL.GetAvailableEventsForFutureDateV3(model.member_id, model.guest_count, model.req_date, model.event_id);
                    if (availableEventsForFutureDate.event_date >= DateTime.UtcNow.Date)
                    {
                        loadScheduleEventResponse.error_info.error_type = (int)Common.Common.ErrorType.EventFutureDate;
                        loadScheduleEventResponse.error_info.extra_info = availableEventsForFutureDate.event_date.ToString();
                        loadScheduleEventResponse.error_info.description = "The next date with availability for " + model.guest_count + " guests is " + availableEventsForFutureDate.event_date.ToString("dddd, MMMM dd, yyyy") + " at " + availableEventsForFutureDate.start_time.ToString("hh:mm tt") + ".";
                    }
                    else
                    {
                        string HolidayName = eventDAL.IsHoliday(model.member_id, model.req_date);
                        if (!string.IsNullOrEmpty(HolidayName))
                        {
                            loadScheduleEventResponse.error_info.error_type = (int)Common.Common.ErrorType.IsHoliday;
                            loadScheduleEventResponse.error_info.extra_info = HolidayName;
                            loadScheduleEventResponse.error_info.description = "WARNING! This property is closed on " + model.req_date.ToString("dddd, MMMM dd, yyyy");
                        }
                        else
                        {
                            MaxSeatsLeft maxSeatsLeft = eventDAL.GetMaxSeatsLeftByWineryIdAndDateV3(model.member_id, model.req_date);

                            if (maxSeatsLeft.min_seats > 0 || maxSeatsLeft.max_seats > 0)
                            {
                                if (maxSeatsLeft.min_seats > model.guest_count)
                                {
                                    loadScheduleEventResponse.error_info.error_type = (int)Common.Common.ErrorType.MinSeats;
                                    loadScheduleEventResponse.error_info.extra_info = maxSeatsLeft.min_seats.ToString();
                                    loadScheduleEventResponse.error_info.description = "Sorry, the minimum guest count of the event has changed. Minimum guest count required is " + maxSeatsLeft.min_seats.ToString();
                                }
                                else
                                {
                                    loadScheduleEventResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxSeats;
                                    loadScheduleEventResponse.error_info.extra_info = maxSeatsLeft.max_seats.ToString();
                                    loadScheduleEventResponse.error_info.description = "Sorry, the max group size of the event has changed. Max group size allowed is " + maxSeatsLeft.max_seats.ToString();
                                }
                            }
                            else
                            {
                                DateTime MaxEndDate = eventDAL.GetMaxEndDateByWineryId(model.member_id);
                                if (MaxEndDate.Date < model.req_date.Date && MaxEndDate >= DateTime.UtcNow.Date)
                                {
                                    var winery = new Model.WineryModel();
                                    winery = eventDAL.GetWineryById(model.member_id);

                                    loadScheduleEventResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxEndDate;
                                    loadScheduleEventResponse.error_info.extra_info = MaxEndDate.ToString();
                                    loadScheduleEventResponse.error_info.description = winery.DisplayName + " accepts reservations up until " + MaxEndDate.ToString("dddd, MMMM dd, yyyy") + ". Contact " + Utility.FormatTelephoneNumber(winery.BusinessPhone.ToString(), winery.WineryAddress.country) + " or via " + winery.EmailAddress + " for more information.";
                                }
                                else
                                {
                                    loadScheduleEventResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                                    if (model.event_id > 0 || model.slot_id > 0)
                                        loadScheduleEventResponse.error_info.description = "There is no availability for this experience on " + model.req_date.ToString("MM/dd/yyyy");
                                    else
                                        loadScheduleEventResponse.error_info.description = "We’re sorry, but there are no experiences available on " + model.req_date.ToString("MM/dd/yyyy");
                                }
                            }

                        }
                    }
                }
                else
                {
                    loadScheduleEventResponse.error_info.error_type = (int)Common.Common.ErrorType.EventInactive;
                    loadScheduleEventResponse.error_info.extra_info = "Event Inactive";
                    loadScheduleEventResponse.error_info.description = "Sorry, this event is not available";
                }
            }
            catch (Exception ex)
            {
                loadScheduleEventResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                loadScheduleEventResponse.error_info.extra_info = Common.Common.InternalServerError;
                loadScheduleEventResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "LoadScheduleEvent:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
            }
            return new ObjectResult(loadScheduleEventResponse);
        }


        [Route("addonitemsbymember")]
        [HttpGet]
        public IActionResult GetAddonItemsByMember(int member_id)
        {
            var addonItemsResponse = new AddonItemsByMemberResponse();
            if (member_id <= 0)
            {
                addonItemsResponse.success = false;
                addonItemsResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                addonItemsResponse.error_info.extra_info = "Invalid member Id.";
                return new ObjectResult(addonItemsResponse);
            }
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var addonItems = eventDAL.GetAddonItemsByMemberId(member_id);

                if (addonItems != null && addonItems.Count > 0)
                {
                    addonItemsResponse.data = addonItems;
                    addonItemsResponse.success = true;
                }
                else
                {
                    addonItemsResponse.success = true;
                    addonItemsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    addonItemsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                addonItemsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                addonItemsResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                addonItemsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetAddonItemsByMember:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(addonItemsResponse);
        }


        [Route("get_promotions_by_type")]
        [HttpGet]
        public IActionResult GetCellarScoutPromotions(int offer_type = -1, int offer_schema_grp = 0, string city = "", string state = "")
        {
            var ScoutOffersResponse = new ScoutOffersResponse();
            if(!String.IsNullOrEmpty(city) && String.IsNullOrEmpty(state))
            {
                ScoutOffersResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                //ScoutOffersResponse.error_info.extra_info = Common.Common.ErrorType.InvalidData;
                ScoutOffersResponse.error_info.description = "Please enter valid City and State";
                return new ObjectResult(ScoutOffersResponse);
            }
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var scoutPromotions = eventDAL.GetPromotionsByPromoType(offer_type, offer_schema_grp, city, state);

                if (scoutPromotions != null && scoutPromotions.Count > 0)
                {
                    ScoutOffersResponse.data = scoutPromotions;
                    ScoutOffersResponse.success = true;
                }
                else
                {
                    ScoutOffersResponse.success = true;
                    ScoutOffersResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ScoutOffersResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ScoutOffersResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ScoutOffersResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                ScoutOffersResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetCellarScoutPromotions:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(ScoutOffersResponse);
        }

        [Route("promotion_detail")]
        [HttpGet]
        public IActionResult GetPromotionDetail(int promo_id)
        {
            var PromotionDetailResponse = new PromotionDetailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var promoDetails = eventDAL.GetPromotionDetail(promo_id);

                if (promoDetails != null)
                {
                    PromotionDetailResponse.data = promoDetails;
                    if (!String.IsNullOrEmpty(promoDetails.member_business_phone))
                    {
                        PromotionDetailResponse.data.member_business_phone = Utility.FormatPhoneNumber(promoDetails.member_business_phone).Replace("+1 ","");
                    }
                    PromotionDetailResponse.success = true;
                }
                else
                {
                    PromotionDetailResponse.success = true;
                    PromotionDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    PromotionDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                PromotionDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                PromotionDetailResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                PromotionDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPromotionDetail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(PromotionDetailResponse);
        }


        [Route("claim_offer")]
        [HttpPost]
        public IActionResult ClaimOffer([FromBody] ClaimOfferRequest model)
        {
            var resp = new ClaimOfferResponse();
            if (model.user_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Invalid user Id. A valid User Id is required";
                resp.error_info.description = "Invalid user Id. A valid User Id is required";
                return new ObjectResult(resp);
            }
            if (model.promotion_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Invalid Promotion Id. A valid Promotion Id is required";
                resp.error_info.description = "Invalid Promotion Id. A valid Promotion Id is required";
                return new ObjectResult(resp);
            }

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                int userPromotionsId = eventDAL.ClaimOffer(model);

                if (userPromotionsId > 0)
                {
                    ClaimModel r = new ClaimModel();
                    r.id = userPromotionsId;
                    resp.data = r;
                    resp.success = true;
                }
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ClaimOffer:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(resp);
        }

        [Route("get_promotions_by_user")]
        [HttpGet]
        public IActionResult GetPromotionsByUser(int user_id)
        {
            var PromotionsByUserResponse = new PromotionsByUserResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var promotionsByUser = eventDAL.GetPromotionsByUser(user_id);

                if (promotionsByUser != null && promotionsByUser.Count > 0)
                {
                    PromotionsByUserResponse.data = promotionsByUser;                   
                    for (int i = 0; i < promotionsByUser.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(promotionsByUser[i].member_business_phone))
                        {
                            PromotionsByUserResponse.data[i].member_business_phone = Utility.FormatPhoneNumber(promotionsByUser[i].member_business_phone).Replace("+1 ", "");
                        }
                    }
                    PromotionsByUserResponse.success = true;
                }
                else
                {
                    PromotionsByUserResponse.success = true;
                    PromotionsByUserResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    PromotionsByUserResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                PromotionsByUserResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                PromotionsByUserResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                PromotionsByUserResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPromotionsByUser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(PromotionsByUserResponse);
        }

        [Route("redeem_promotion")]
        [HttpPost]
        public IActionResult RedeemPromotion([FromBody] RedeemPromotionRequest model)
        {
            var resp = new BaseResponse();
            if ((model.user_id <= 0 || model.promotion_id <= 0) && string.IsNullOrWhiteSpace(model.promotion_code))
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Invalid user Id/Promotion Id. A valid User Id and Promotion Id is required";
                resp.error_info.description = "Invalid user Id/Promotion Id. A valid User Id and Promotion Id is required";
                return new ObjectResult(resp);
            }
            else if (string.IsNullOrWhiteSpace(model.promotion_code) && model.user_id <= 0 && model.promotion_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Invalid promotion code. A valid promotion code is required";
                resp.error_info.description = "Invalid promotion code. A valid promotion code is required";
                return new ObjectResult(resp);
            }
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                bool isSuccess = eventDAL.RedeemPromotion(model);

                resp.success = isSuccess;

            }
            catch (Exception ex)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "RedeemPromotion:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(resp);
        }

        [Route("get_cellar_scout_offer_types")]
        [HttpGet]
        public IActionResult GetCellarScoutOfferTypes()
        {
            var response = new ScoutOffersTypesResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var cellarScoutOfferTypes = eventDAL.GetCellarScoutOfferTypes();

                if (cellarScoutOfferTypes != null && cellarScoutOfferTypes.Count > 0)
                {
                    response.data = cellarScoutOfferTypes;
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
                logDAL.InsertLog("WebApi", "GetCellarScoutOfferTypes:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }

        [Route("get_promotions_by_member")]
        [HttpGet]
        public IActionResult GetPromotionsByMember(int member_id, int offer_type = -1)
        {
            var promotionsByMemberResponse = new PromotionsByMemberResponse();
            if (member_id <= 0)
            {
                promotionsByMemberResponse.success = false;
                promotionsByMemberResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                promotionsByMemberResponse.error_info.extra_info = "Invalid Member Id";
                promotionsByMemberResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(promotionsByMemberResponse);
            }
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var promotionsByMember = eventDAL.GetPromotionsByMember(member_id, offer_type);

                if (promotionsByMember != null && promotionsByMember.Count > 0)
                {
                    promotionsByMemberResponse.data = promotionsByMember;
                    for (int i = 0; i < promotionsByMember.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(promotionsByMember[i].member_business_phone))
                        {
                            promotionsByMemberResponse.data[i].member_business_phone = Utility.FormatPhoneNumber(promotionsByMember[i].member_business_phone).Replace("+1 ", "");
                        }
                    }
                    promotionsByMemberResponse.success = true;
                }
                else
                {
                    promotionsByMemberResponse.success = true;
                    promotionsByMemberResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    promotionsByMemberResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                promotionsByMemberResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                promotionsByMemberResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                promotionsByMemberResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPromotionsByMember:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(promotionsByMemberResponse);
        }

        [Route("get_cellar_scout_locations")]
        [HttpGet]
        public IActionResult GetCellarScoutLocations()
        {
            var response = new CellarScoutLocationsResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var cellarScoutLocations = eventDAL.GetCellarScoutLocations();

                if (cellarScoutLocations != null && cellarScoutLocations.Count > 0)
                {
                    response.data = cellarScoutLocations;
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
                logDAL.InsertLog("WebApi", "GetCellarScoutLocations:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }

        [Route("eventsupdaterequest")]
        [HttpPost]
        public async Task<IActionResult> EventsUpdateRequest([FromBody]EventsUpdateRequest reqmodel)
        {
            var resp = new BaseResponse();

            try
            {
                if (reqmodel != null && reqmodel.EventId > 0)
                {
                    QueueService getStarted = new QueueService();

                    var queueModel = new EmailQueue();
                    queueModel.EType = (int)EmailType.EventsUpdateRequest;
                    queueModel.PerMsg = JsonConvert.SerializeObject(reqmodel);

                    var qData = JsonConvert.SerializeObject(queueModel);

                    AppSettings _appsettings = _appSetting.Value;
                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                }
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "EventsUpdateRequest:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(resp);
        }

        [Route("private_confirmation_messages")]
        [HttpGet]
        public IActionResult GetPrivateConfirmationMessages(int member_id)
        {
            var privateConfirmationMessageResponse = new PrivateConfirmationMessageResponse();
            if (member_id <= 0)
            {
                privateConfirmationMessageResponse.success = false;
                privateConfirmationMessageResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                privateConfirmationMessageResponse.error_info.extra_info = "Invalid Member Id";
                privateConfirmationMessageResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(privateConfirmationMessageResponse);
            }
            try
            {
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString_readonly);
                List<Settings.Setting> memberSettingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.member);
                privateConfirmationMessageResponse.success = true;

                string PvtConfirmationMessage = Settings.GetStrValue(memberSettingsGroup, SettingKey.member_Pvt_rsvp_confirmation_message);
                string PvtConfirmationMessage1 = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message1);
                string PvtConfirmationMessage2 = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message2);
                string PvtConfirmationMessage3 = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message3);
                string PvtConfirmationMessage4 = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message4);
                string PvtConfirmationMessage5 = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message5);
                string PvtConfirmationMessage6 = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message6);
                string PvtConfirmationMessage7 = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message7);
                string PvtConfirmationMessage8 = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message8);

                string PvtConfirmationMessage1Title = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message_title1);
                string PvtConfirmationMessage2Title = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message_title2);
                string PvtConfirmationMessage3Title = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message_title3);
                string PvtConfirmationMessage4Title = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message_title4);
                string PvtConfirmationMessage5Title = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message_title5);
                string PvtConfirmationMessage6Title = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message_title6);
                string PvtConfirmationMessage7Title = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message_title7);
                string PvtConfirmationMessage8Title = Settings.GetStrValue(memberSettingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message_title8);

                var list = new List<PrivateConfirmationMessageModel>();

                var model = new PrivateConfirmationMessageModel();

                model.confirmation_message = PvtConfirmationMessage;
                model.title = "Standard Private Confirmation Message";
                model.id = (int)SettingKey.member_Pvt_rsvp_confirmation_message;
                list.Add(model);

                if (PvtConfirmationMessage1.Length > 0 && PvtConfirmationMessage1Title.Length > 0)
                {
                    model = new PrivateConfirmationMessageModel();

                    model.confirmation_message = PvtConfirmationMessage1;
                    model.title = PvtConfirmationMessage1Title;
                    model.id = (int)SettingKey.member_Pvt_rsvp_confirmation_message1;
                    list.Add(model);
                }

                if (PvtConfirmationMessage2.Length > 0 && PvtConfirmationMessage2Title.Length > 0)
                {
                    model = new PrivateConfirmationMessageModel();

                    model.confirmation_message = PvtConfirmationMessage2;
                    model.title = PvtConfirmationMessage2Title;
                    model.id = (int)SettingKey.member_Pvt_rsvp_confirmation_message2;
                    list.Add(model);
                }

                if (PvtConfirmationMessage3.Length > 0 && PvtConfirmationMessage3Title.Length > 0)
                {
                    model = new PrivateConfirmationMessageModel();

                    model.confirmation_message = PvtConfirmationMessage3;
                    model.title = PvtConfirmationMessage3Title;
                    model.id = (int)SettingKey.member_Pvt_rsvp_confirmation_message3;
                    list.Add(model);
                }

                if (PvtConfirmationMessage4.Length > 0 && PvtConfirmationMessage4Title.Length > 0)
                {
                    model = new PrivateConfirmationMessageModel();

                    model.confirmation_message = PvtConfirmationMessage4;
                    model.title = PvtConfirmationMessage4Title;
                    model.id = (int)SettingKey.member_Pvt_rsvp_confirmation_message4;
                    list.Add(model);
                }

                if (PvtConfirmationMessage5.Length > 0 && PvtConfirmationMessage5Title.Length > 0)
                {
                    model = new PrivateConfirmationMessageModel();

                    model.confirmation_message = PvtConfirmationMessage5;
                    model.title = PvtConfirmationMessage5Title;
                    model.id = (int)SettingKey.member_Pvt_rsvp_confirmation_message5;
                    list.Add(model);
                }

                if (PvtConfirmationMessage6.Length > 0 && PvtConfirmationMessage6Title.Length > 0)
                {
                    model = new PrivateConfirmationMessageModel();

                    model.confirmation_message = PvtConfirmationMessage6;
                    model.title = PvtConfirmationMessage6Title;
                    model.id = (int)SettingKey.member_Pvt_rsvp_confirmation_message6;
                    list.Add(model);
                }

                if (PvtConfirmationMessage7.Length > 0 && PvtConfirmationMessage7Title.Length > 0)
                {
                    model = new PrivateConfirmationMessageModel();

                    model.confirmation_message = PvtConfirmationMessage7;
                    model.title = PvtConfirmationMessage7Title;
                    model.id = (int)SettingKey.member_Pvt_rsvp_confirmation_message7;
                    list.Add(model);
                }

                if (PvtConfirmationMessage8.Length > 0 && PvtConfirmationMessage8Title.Length > 0)
                {
                    model = new PrivateConfirmationMessageModel();

                    model.confirmation_message = PvtConfirmationMessage8;
                    model.title = PvtConfirmationMessage8Title;
                    model.id = (int)SettingKey.member_Pvt_rsvp_confirmation_message8;
                    list.Add(model);
                }


                privateConfirmationMessageResponse.data = list;
            }
            catch (Exception ex)
            {
                privateConfirmationMessageResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                privateConfirmationMessageResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                privateConfirmationMessageResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPrivateConfirmationMessages:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(privateConfirmationMessageResponse);
        }


        [Route("get_cellar_scout_promotions")]
        [HttpGet]
        public IActionResult GetActiveCellarScoutPromotions(int offer_schema_grp = 0, string city = "", string state = "")
        {
            var ScoutOffersResponse = new ScoutOffersResponse();
            if (!String.IsNullOrEmpty(city) && String.IsNullOrEmpty(state))
            {
                ScoutOffersResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                //ScoutOffersResponse.error_info.extra_info = Common.Common.ErrorType.InvalidData;
                ScoutOffersResponse.error_info.description = "Please enter valid City and State";
                return new ObjectResult(ScoutOffersResponse);
            }
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var scoutPromotions = eventDAL.GetActiveCellarScoutPromotions(offer_schema_grp, city, state);

                if (scoutPromotions != null && scoutPromotions.Count > 0)
                {
                    ScoutOffersResponse.data = scoutPromotions;
                    ScoutOffersResponse.success = true;
                }
                else
                {
                    ScoutOffersResponse.success = true;
                    ScoutOffersResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    ScoutOffersResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                ScoutOffersResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                ScoutOffersResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                ScoutOffersResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetActiveCellarScoutPromotions:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(ScoutOffersResponse);
        }

        [Route("getmemberstats")]
        [HttpGet]
        public IActionResult GetMemberstats(int member_id, int region_id = 0)
        {
            var memberstatsResponse = new MemberstatsResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var memberStatsModel = new MemberStatsModel();

                memberStatsModel = eventDAL.GetMemberstats(member_id);

                if (region_id > 0)
                {
                    TicketEventsComponentRequest model = new TicketEventsComponentRequest();
                    model.regionId = region_id;
                    model.topRecords = 4;

                    TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);
                    List<UpcomingEventModel> list = ticketDAL.GetTicketEventsComponent(model);

                    if (list != null && list.Count > 0)
                    {
                        memberStatsModel.has_nearby_events = true;
                    }
                }

                var promoDetails = eventDAL.GetProfilePagePromoByMemberId(member_id);

                if (promoDetails != null)
                {
                    if (!String.IsNullOrEmpty(promoDetails.member_business_phone))
                    {
                        promoDetails.member_business_phone = Utility.FormatPhoneNumber(promoDetails.member_business_phone).Replace("+1 ", "");
                    }

                    memberStatsModel.profile_page_promo = promoDetails;
                }

                memberstatsResponse.success = true;
                memberstatsResponse.data = memberStatsModel;
            }
            catch (Exception ex)
            {
                memberstatsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                memberstatsResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                memberstatsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetMemberstats:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(memberstatsResponse);
        }

        [Route("getmemberproducts")]
        [HttpGet]
        public IActionResult GetMemberProducts(int member_id)
        {
            var memberProductsResponse = new MemberProductsResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var list = new List<MemberProductsModel>();

                list = eventDAL.GetMemberProducts(member_id);

                if (list != null && list.Count > 0)
                {
                    memberProductsResponse.success = true;
                    memberProductsResponse.data = list;
                }
                else
                {
                    memberProductsResponse.success = true;
                    memberProductsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    memberProductsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                memberProductsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                memberProductsResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                memberProductsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetMemberProducts:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(memberProductsResponse);
        }

        [Route("getmemberfoodmenu")]
        [HttpGet]
        public IActionResult GetMemberFoodMenu(int member_id)
        {
            var memberProductsResponse = new MemberFoodMenuResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var list = new List<MemberFoodMenuModel>();

                list = eventDAL.GetMemberFoodMenu(member_id);

                if (list != null && list.Count > 0)
                {
                    memberProductsResponse.success = true;
                    memberProductsResponse.data = list;
                }
                else
                {
                    memberProductsResponse.success = true;
                    memberProductsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    memberProductsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                memberProductsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                memberProductsResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                memberProductsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetMemberProducts:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(memberProductsResponse);
        }

        [Route("getpricerange")]
        [HttpGet]
        public IActionResult GetPriceRangeByRegionId(int region_id, DateTime start_date, int guest = 0)
        {
            var priceRangeResponse = new PriceRangeResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var priceRangeByRegionIdModel = new PriceRangeByRegionIdModel();

                if (guest == 0)
                    start_date = DateTime.Now;

                priceRangeByRegionIdModel = eventDAL.GetPriceRangeByRegionId(start_date, guest, region_id);

                priceRangeResponse.success = true;
                priceRangeResponse.data = priceRangeByRegionIdModel;
            }
            catch (Exception ex)
            {
                priceRangeResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                priceRangeResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                priceRangeResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetPriceRangeByRegionId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(priceRangeResponse);
        }

        [Route("getmembersbypricerange")]
        [HttpGet]
        public IActionResult GetMembersByPriceRange(DateTime start_date, int guest, int region_id,decimal min_price,decimal max_price)
        {
            var priceRangeResponse = new MembersByPriceRangeResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var priceRangeByRegionIdModel = new List<MembersByPriceRangeModel>();

                priceRangeByRegionIdModel = eventDAL.GetMembersByPriceRange(start_date, guest, region_id, min_price,max_price);

                priceRangeResponse.success = true;
                priceRangeResponse.data = priceRangeByRegionIdModel;
            }
            catch (Exception ex)
            {
                priceRangeResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                priceRangeResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                priceRangeResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetMembersByPriceRange:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(priceRangeResponse);
        }

        [Route("eventslotdetail")]
        [HttpGet]
        public IActionResult EventSlotDetail(EventSlotDetailRequest model)
        {
            var eventDetailResponse = new EventSlotDetailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var eventScheduleEvent = new EventScheduleEvent();

                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                eventScheduleEvent = eventDAL.GetEventDetail(model.req_date, model.event_id, IsAdmin,model.slot_id,model.slot_type);

                if (eventScheduleEvent != null && eventScheduleEvent.event_id > 0)
                {
                    eventDetailResponse.success = true;
                    eventDetailResponse.data = eventScheduleEvent;
                }
            }
            catch (Exception ex)
            {
                eventDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                eventDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "EventSlotDetail:  event_id-" + model.event_id.ToString() + ",req_date-" + model.req_date.ToString() + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(eventDetailResponse);
        }

        [Route("eventaddonsaccounttypesbyid")]
        [HttpGet]
        public IActionResult GetEventAddOnsAccountTypesId(int addon_group_id, int addon_group_items_id)
        {
            var eventAddOnsAccountTypesResponse = new EventAddOnsAccountTypesResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var eventAddOnModel = new List<Event_AddOns_AccountTypes>();

                eventAddOnModel = eventDAL.GetEventAddOnsAccountTypesId(addon_group_id, addon_group_items_id);

                if (eventAddOnModel != null && eventAddOnModel.Count > 0)
                {
                    eventAddOnsAccountTypesResponse.success = true;
                    eventAddOnsAccountTypesResponse.data = eventAddOnModel;
                }
                else
                {
                    eventAddOnsAccountTypesResponse.success = true;
                    eventAddOnsAccountTypesResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    eventAddOnsAccountTypesResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                eventAddOnsAccountTypesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventAddOnsAccountTypesResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                eventAddOnsAccountTypesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventAddOnsAccountTypesId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(eventAddOnsAccountTypesResponse);
        }

        [Route("saveeventaddonsaccounttypes")]
        [HttpPost]
        public async Task<IActionResult> InsertEventAddOnsAccountTypes([FromBody] EventAddOnsAccountTypesRequest reqmodel)
        {
            var resp = new BaseResponse();

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                eventDAL.InsertEventAddOnsAccountTypes(reqmodel.addon_group_id, reqmodel.addon_group_items_id, reqmodel.thirdparty_accounttypes_id, reqmodel.member_benefit, reqmodel.member_benefit_req, reqmodel.member_benefit_custom_value);
                resp.success = true;
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "InsertEventAddOnsAccountTypes:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(resp);
        }

        [Route("deleteeventaddonsaccounttypesbyid")]
        [HttpPost]
        public async Task<IActionResult> DeleteEventAddOnsAccountTypesById([FromBody] DeleteEventAddOnsAccountTypesRequest request)
        {
            BaseResponse response = new BaseResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                eventDAL.DeleteEventAddOnsAccountTypesById(request.addon_group_id, request.addon_group_items_id, request.thirdparty_accounttypes_id);
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "DeleteEventAddOnsAccountTypesById:  error:-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }

            return new ObjectResult(response);
        }

        [Route("getrsvpeventlandingpage")]
        [HttpGet]
        public IActionResult GetRSVPEventLandingPage(DateTime request_date, int request_guest, string request_time, int event_id, bool show_images = true, string access_code = "")
        {
            var eventv2sResponse = new RSVPEventLandingPageResponse();
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
            int member_id = -1;
            int RegionId = 0;
            var wineryDetailForEvent = new WineryDetailForEvent();
            wineryDetailForEvent = eventDAL.GetWineryIdByEventId(event_id);
            member_id = wineryDetailForEvent.MemberId;
            RegionId = wineryDetailForEvent.RegionId;

            try
            {
                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString_readonly);

                var events = new EventV3();
                var rsvpEventLandingPage = new RSVPEventLandingPage();

                if (request_date.Year < 2010)
                {
                    eventv2sResponse.success = true;
                    eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    eventv2sResponse.error_info.extra_info = "no record found";
                }
                else
                {
                    //string HolidayName = eventDAL.IsHoliday(member_id, request_date);

                    //if (!string.IsNullOrEmpty(HolidayName))
                    //{
                    //    eventv2sResponse.error_info.holiday_name = HolidayName;
                    //    eventv2sResponse.error_info.holiday_description = "This property is closed on " + request_date.ToString("dddd, MMMM dd, yyyy");
                    //}

                    if (event_id > 0)
                        request_time = "";

                    if (!string.IsNullOrWhiteSpace(access_code))
                    {
                        //check if event access code is valid
                        var eventAccess = eventDAL.CheckEventAccessCode(member_id, event_id, access_code, 0);

                        if (eventAccess == null || !eventAccess.IsValid)
                        {
                            eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                            eventv2sResponse.error_info.extra_info = "Invalid Acess Code";
                            eventv2sResponse.error_info.description = "Invalid access code. Acess code does not exist.";
                            return new ObjectResult(eventv2sResponse);
                        }
                        else if (eventAccess.IsValid && eventAccess.StartDate == null)
                        {
                            eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                            eventv2sResponse.error_info.extra_info = "Invalid Acess Code";
                            eventv2sResponse.error_info.description = string.Format("The '{0}' access code is no longer available", access_code);
                            return new ObjectResult(eventv2sResponse);
                        }
                    }

                    rsvpEventLandingPage = eventDAL.GetRSVPEventLandingPage(member_id, "", request_guest, request_date, event_id, -1, -1, false, false, false, show_images, access_code, 0);

                    if (rsvpEventLandingPage != null)
                    {
                        eventv2sResponse.success = true;

                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString_readonly);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.member).ToList();

                        if (settingsGroup != null && settingsGroup.Count > 0)
                        {
                            rsvpEventLandingPage.show_private_request_content = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_private_booking_requests);
                            rsvpEventLandingPage.private_booking_request_email = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_private_booking_request_email);
                        }

                        eventv2sResponse.data = rsvpEventLandingPage;

                        if (rsvpEventLandingPage.event_details == null || rsvpEventLandingPage.event_details.event_id == 0)
                        {
                            AvailableEventsForFutureDate availableEventsForFutureDate = eventDAL.GetAvailableEventsForFutureDateV3(member_id, request_guest, request_date, event_id, -1, -1, false, false, request_time, access_code);
                            if (availableEventsForFutureDate.event_date >= request_date.Date)
                            {
                                eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.EventFutureDate;
                                eventv2sResponse.error_info.extra_info = availableEventsForFutureDate.event_date.ToString();

                                eventv2sResponse.error_info.description = "There is no availability for this experience on " + request_date.ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                MaxSeatsLeft maxSeatsLeft = new MaxSeatsLeft();
                                bool maxMinConditionFormed = false;

                                for (int i = 0; i < 30; i++)
                                {
                                    maxSeatsLeft = new MaxSeatsLeft();
                                    maxSeatsLeft = eventDAL.GetMaxSeatsLeftByWineryIdAndDateV3(member_id, request_date.AddDays(i), event_id, -1, -1);
                                    if (maxSeatsLeft.min_seats > 0 || maxSeatsLeft.max_seats > 0 || maxSeatsLeft.min_seats == -1 || maxSeatsLeft.max_seats == -1)
                                        break;
                                }

                                if (maxSeatsLeft.min_seats > 0 || maxSeatsLeft.max_seats > 0)
                                {
                                    if (maxSeatsLeft.min_seats > request_guest)
                                    {
                                        var winery = new Model.WineryModel();
                                        winery = eventDAL.GetWinery2ById(member_id);

                                        maxMinConditionFormed = true;
                                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.MinSeats;
                                        eventv2sResponse.error_info.extra_info = maxSeatsLeft.min_seats.ToString();
                                        eventv2sResponse.error_info.description = "We’re sorry, but your party is too small to book a reservation at " + winery.DisplayName + ".";
                                    }
                                    else if (maxSeatsLeft.max_seats < request_guest)
                                    {
                                        var winery = new Model.WineryModel();
                                        winery = eventDAL.GetWinery2ById(member_id);

                                        maxMinConditionFormed = true;
                                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxSeats;
                                        eventv2sResponse.error_info.extra_info = maxSeatsLeft.max_seats.ToString();
                                        eventv2sResponse.error_info.description = "We’re sorry, but your party is too large to book a reservation at " + winery.DisplayName + ".";
                                    }
                                }

                                if (!maxMinConditionFormed)
                                {
                                    DateTime MaxEndDate = eventDAL.GetMaxEndDateByWineryId(member_id, event_id, -1, -1);
                                    if (MaxEndDate.Date < request_date.Date && MaxEndDate >= DateTime.UtcNow.Date)
                                    {
                                        var winery = new Model.WineryModel();
                                        winery = eventDAL.GetWinery2ById(member_id);

                                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.MaxEndDate;
                                        eventv2sResponse.error_info.extra_info = MaxEndDate.ToString();
                                        eventv2sResponse.error_info.description = winery.DisplayName + " accepts reservations up until " + MaxEndDate.ToString("dddd, MMMM dd, yyyy") + ". Contact " + Utility.FormatTelephoneNumber(winery.BusinessPhone.ToString(), winery.WineryAddress.country) + " or via " + winery.EmailAddress + " for more information.";
                                    }
                                    else
                                    {
                                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                                        if (event_id > 0)
                                            eventv2sResponse.error_info.description = "There is no availability for this experience on " + request_date.ToString("MM/dd/yyyy");
                                        else
                                            eventv2sResponse.error_info.description = "We’re sorry, but there are no experiences available on " + request_date.ToString("MM/dd/yyyy");
                                    }
                                }

                            }

                            if (event_id > 0)
                            {
                                events = eventDAL.GetEventDetailsByEventId(member_id, event_id, -1, -1, show_images);

                                MaxSeatsLeft maxSeatsLeft = new MaxSeatsLeft();
                                maxSeatsLeft = eventDAL.GetMaxSeatsLeftByWineryIdAndDateV3(member_id, request_date, event_id, -1, -1);

                                events.events[0].max_persons = maxSeatsLeft.max_seats;
                                events.events[0].min_persons = maxSeatsLeft.min_seats;

                                if (availableEventsForFutureDate.event_date > request_date.Date)
                                {
                                    var events2 = new RSVPEventLandingPage();
                                    events2 = eventDAL.GetRSVPEventLandingPage(member_id, "", request_guest, availableEventsForFutureDate.event_date, event_id, -1, -1, false, false, false, show_images, access_code, 0);

                                    if (events2 != null && events2.event_details != null)
                                    {
                                        events.events[0].next_available_times = events2.event_details.times;
                                        events.events[0].max_persons = events2.event_details.max_persons;
                                        events.events[0].min_persons = events2.event_details.min_persons;
                                        events.events[0].duration = events2.event_details.duration;
                                    }
                                }

                                rsvpEventLandingPage.booked_count = events.booked_count;
                                rsvpEventLandingPage.event_details = events.events[0];
                                //rsvpEventLandingPage.visitation_rule = events.visitation_rule;
                                //rsvpEventLandingPage.visitation_rule_desc = events.visitation_rule_desc;
                                //rsvpEventLandingPage.passport_benefit_desc = events.passport_benefit_desc;
                                //rsvpEventLandingPage.passport_event_name = events.passport_event_name;
                                //rsvpEventLandingPage.show_complementary_msg = events.show_complementary_msg;
                                //rsvpEventLandingPage.visitation_external_url = events.visitation_external_url;
                                //rsvpEventLandingPage.show_book_button = events.show_book_button;
                                //rsvpEventLandingPage.show_private_request_content = events.show_private_request_content;
                                //rsvpEventLandingPage.private_booking_request_email = events.private_booking_request_email;
                                //rsvpEventLandingPage.ticket_events = events.ticket_events;

                                eventv2sResponse.data = rsvpEventLandingPage;
                            }
                            else if (availableEventsForFutureDate.event_date > request_date.Date)
                            {
                                var events2 = new RSVPEventLandingPage();
                                events2 = eventDAL.GetRSVPEventLandingPage(member_id, "", request_guest, availableEventsForFutureDate.event_date, event_id, -1, -1, false, false, false, show_images, access_code, 0);

                                if (events != null && events.events != null)
                                {
                                    List<EventV2> eventsv2 = events.events;
                                    foreach (var item in eventsv2)
                                    {
                                        item.next_available_times = item.times;
                                        item.times = null;
                                    }

                                    rsvpEventLandingPage.booked_count = events.booked_count;
                                    rsvpEventLandingPage.event_details = events2.event_details;
                                    //rsvpEventLandingPage.visitation_rule = events.visitation_rule;
                                    //rsvpEventLandingPage.visitation_rule_desc = events.visitation_rule_desc;
                                    //rsvpEventLandingPage.passport_benefit_desc = events.passport_benefit_desc;
                                    //rsvpEventLandingPage.passport_event_name = events.passport_event_name;
                                    //rsvpEventLandingPage.show_complementary_msg = events.show_complementary_msg;
                                    //rsvpEventLandingPage.visitation_external_url = events.visitation_external_url;
                                    //rsvpEventLandingPage.show_book_button = events.show_book_button;
                                    //rsvpEventLandingPage.show_private_request_content = events.show_private_request_content;
                                    //rsvpEventLandingPage.private_booking_request_email = events.private_booking_request_email;
                                    //rsvpEventLandingPage.ticket_events = events.ticket_events;

                                    eventv2sResponse.data = rsvpEventLandingPage;
                                }
                            }
                        }
                    }
                    else
                    {
                        eventv2sResponse.success = true;
                        eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        eventv2sResponse.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                eventv2sResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventv2sResponse.error_info.extra_info = Common.Common.InternalServerError;
                eventv2sResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetRSVPEventLandingPage:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(eventv2sResponse);
        }

        [Route("bookedeventexperiencebyid")]
        [HttpGet]
        public IActionResult GetBookedEventExperience(int id, int? region_id, int? sub_region_id, int? business_id)
        {
            var response = new BookedEventExperienceResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                string pageSectionFilter = eventDAL.GetCmsPageSectionFilterById(id);

                var filterModel = JsonConvert.DeserializeObject<EventExperienceFilterModel>(pageSectionFilter);
                if (business_id != null)
                    filterModel.WineryId = business_id;
                if (region_id != null && sub_region_id != null)
                    filterModel.SubRegionId = sub_region_id;
                if (region_id != null)
                    filterModel.AppleationId = region_id;

                var eventExperiences = eventDAL.GetBookedEventExperiences(filterModel.AppleationId, filterModel.WineryId, filterModel.SubRegionId);
                List<EventExperienceModel> experiences = new List<EventExperienceModel>();

                if (eventExperiences != null)
                {
                    if (eventExperiences.JsonResult != null)
                    {
                        DateTime lastFetchDate = eventExperiences.LastFetchDate;
                        DateTime endDate = DateTime.Now;
                        var timeDifference = endDate.Subtract(lastFetchDate).TotalHours;
                        if (timeDifference > 24)
                        {
                            experiences = eventDAL.GetEventExperiences(filterModel);
                            var jsonResult = JsonConvert.SerializeObject(experiences);
                            var result = eventDAL.SaveBookedEventExperiences(filterModel.AppleationId, filterModel.WineryId, filterModel.SubRegionId, jsonResult);
                        }
                        else
                        {
                            experiences = JsonConvert.DeserializeObject<List<EventExperienceModel>>(eventExperiences.JsonResult);
                            if (experiences.Count() <= 0)
                            {
                                experiences = eventDAL.GetEventExperiences(filterModel);
                                var jsonResult = JsonConvert.SerializeObject(experiences);
                                var result = eventDAL.SaveBookedEventExperiences(filterModel.AppleationId, filterModel.WineryId, filterModel.SubRegionId, jsonResult);
                            }
                        }
                    }
                    else
                    {
                        experiences = eventDAL.GetEventExperiences(filterModel);
                        var jsonResult = JsonConvert.SerializeObject(experiences);
                        var result = eventDAL.SaveBookedEventExperiences(filterModel.AppleationId, filterModel.WineryId, filterModel.SubRegionId, jsonResult);
                    }

                }
                else
                {
                    experiences = eventDAL.GetEventExperiences(filterModel);
                    var jsonResult = JsonConvert.SerializeObject(experiences);
                    var result = eventDAL.SaveBookedEventExperiences(filterModel.AppleationId, filterModel.WineryId, filterModel.SubRegionId, jsonResult);
                }

                if (experiences != null && experiences.Count > 0)
                {
                    response.success = true;
                    response.data = experiences;
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
                logDAL.InsertLog("WebApi", "GetBookedEventExperience:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("eventtypes")]
        [HttpGet]
        public IActionResult GetEventType()
        {
            var eventTypeResponse = new EventTypeResponse();
            
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var eventtypes = eventDAL.GetEventType();

                if (eventtypes != null && eventtypes.Count > 0)
                {
                    eventTypeResponse.data = eventtypes;
                    eventTypeResponse.success = true;
                }
                else
                {
                    eventTypeResponse.success = true;
                    eventTypeResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    eventTypeResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                eventTypeResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                eventTypeResponse.error_info.extra_info = Common.Common.InternalServerError.ToString();
                eventTypeResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetEventType:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(eventTypeResponse);
        }

        [Route("eventtypegroupbypagesection")]
        [HttpGet]
        public IActionResult GetEventTypeGroup(int id)
        {
            var response = new EventTypesResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var pageSection = new CmsPageSectionModel();

                pageSection = eventDAL.GetPageSectionById(id);

                if (pageSection != null && pageSection.id > 0)
                {
                    var filterModel = JsonConvert.DeserializeObject<EventTypeGroupFilterModel>(pageSection.filter);

                    response.success = true;
                    response.data = eventDAL.GetEventsTypes(filterModel);
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
                logDAL.InsertLog("WebApi", "GetEventTypeGroup:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }

        [Route("destinationbyeventtypename")]
        [HttpGet]
        public IActionResult GetDestinationByEventTypeName(string eventType, int? regionId)
        {
            var response = new EventTypeDestinationsResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var list = new List<EventTypeDestinationModel>();

                list = eventDAL.GetDestinationByEventTypeName(eventType, regionId);

                if (list != null && list.Count > 0)
                {
                    response.success = true;
                    response.data = eventDAL.GetDestinationByEventTypeName(eventType, regionId);
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
                logDAL.InsertLog("WebApi", "GetDestinationByEventTypeName:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(response);
        }
    }
}
