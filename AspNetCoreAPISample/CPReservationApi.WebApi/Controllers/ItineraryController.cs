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
using System.IO;
using Newtonsoft.Json;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/itinerary")]
    public class ItineraryController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public ItineraryController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }


        [Route("saveitinerary")]
        [HttpPost]
        public  IActionResult SaveItinerary([FromBody]ItineraryPlannerRequest model)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            //string rawPostData = getRawPostData();
            //string debugData = "";
            //if (!string.IsNullOrWhiteSpace(rawPostData))
            //    debugData = " Raw Data:" + rawPostData;

            //logDAL.InsertLog("WebApi", "SaveItinerary request:" + debugData, HttpContext.Request.Headers["AuthenticateKey"], 3, 0);

            SaveItineraryResponse itineraryResponse = new SaveItineraryResponse();
            if (model == null)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No data sent in request";
                itineraryResponse.error_info.description = "request model cannot be null.";
                return new ObjectResult(itineraryResponse);
            }

            try
            {
                SaveItineraryResponseModel data = new SaveItineraryResponseModel();
                if (model.id == 0 && string.IsNullOrWhiteSpace(model.itinerary_guid))
                    model.itinerary_guid = Guid.NewGuid().ToString();

                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                int id = dal.SaveItinerary(model);
                data.itinerary_guid = model.itinerary_guid;
                data.itinerary_id = id;
                itineraryResponse.success = true;
                itineraryResponse.data = data;

            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "SaveItinerary:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("confirmitinerary")]
        [HttpPost]
        public async Task<IActionResult> ConfirmItinerary([FromBody]ConfirmItineraryRequest model)
        {
            ConfirmItineraryResponse itineraryResponse = new ConfirmItineraryResponse();
            if (model == null)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No data sent in request";
                itineraryResponse.error_info.description = "request model cannot be null.";
                return new ObjectResult(itineraryResponse);
            }
            if (model.itinerary_ids == null || model.itinerary_ids.Count == 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No itinerary info found in request";
                itineraryResponse.error_info.description = "No itinerary info found in request";
                return new ObjectResult(itineraryResponse);
            }
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

           //string rawPostData = getRawPostData();
           // string debugData = "";
           // if (!string.IsNullOrWhiteSpace(rawPostData))
           //     debugData = " Raw Data:" + rawPostData;

           // logDAL.InsertLog("WebApi", "ConfirmItinerary request:" + debugData, HttpContext.Request.Headers["AuthenticateKey"], 3, 0);
            
            try
            {
                //SaveItineraryResponseModel data = new SaveItineraryResponseModel();
                //if (model.id == 0 && string.IsNullOrWhiteSpace(model.itinerary_guid))
                //    model.itinerary_guid = Guid.NewGuid().ToString();

                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                List<CreateReservation> rsvpList = new List<CreateReservation>();

                //step1: get reservation related details for every thing
                var reservationData = eventDAL.GetItineraryDetailsForRSVPCreate(model.itinerary_ids, model.referral_type, model.booked_by_id);
                string currentuser = HttpContext.Request.Headers["AuthenticateKey"];
                //bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                List<CreateReservation> rsvpInfo = new List<CreateReservation>();

                if (reservationData != null && reservationData.Count > 0)
                {
                    logDAL.InsertLog("WebApi", "ConfirmItinerary RSVP data for final creation::" + JsonConvert.SerializeObject(reservationData), HttpContext.Request.Headers["AuthenticateKey"], 3, 0);
                    itineraryResponse.success = true;
                    foreach (CreateReservationModel createReservationModel in reservationData)
                    {
                        bool IsReservationConflict = false;
                        var reservationConflictCheck = new ReservationDetailModel();
                        var createReservationResponse = new CreateReservationResponse();
                        reservationConflictCheck = eventDAL.IsReservationConflict(createReservationModel.ReservationId, createReservationModel.UserId, createReservationModel.EventDate, createReservationModel.StartTime, createReservationModel.EndTime, createReservationModel.SlotId, createReservationModel.SlotType);
                        if (reservationConflictCheck != null && reservationConflictCheck.reservation_id > 0)
                        {
                            if (reservationConflictCheck.event_start_date.TimeOfDay == createReservationModel.StartTime && reservationConflictCheck.event_end_date.TimeOfDay == createReservationModel.EndTime)
                            {
                                createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                                createReservationResponse.error_info.extra_info = "Reservation Conflict Error";
                                createReservationResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} from {1} to {2}. Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd yyyy"));
                                
                                IsReservationConflict = true;
                            }
                            else if (reservationConflictCheck.member_id != createReservationModel.WineryId)
                            {
                                if (reservationConflictCheck.event_start_date.TimeOfDay != createReservationModel.EndTime || reservationConflictCheck.event_end_date.TimeOfDay != createReservationModel.StartTime)
                                {
                                    createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                                    createReservationResponse.error_info.extra_info = "Reservation Conflict Error";
                                    createReservationResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} from {1} to {2}. Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd yyyy"));
                                    
                                    IsReservationConflict = true;
                                }
                            }
                            else
                            {
                                if (reservationConflictCheck.event_start_date.TimeOfDay == createReservationModel.EndTime || reservationConflictCheck.event_end_date.TimeOfDay == createReservationModel.StartTime)
                                {
                                    createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.RsvpBackToBack;
                                    createReservationResponse.error_info.extra_info = "Reservation Conflict Error";
                                    createReservationResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} from {1} to {2}. Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd yyyy"));
                                    
                                    IsReservationConflict = true;
                                }
                                else if (reservationConflictCheck.member_id == createReservationModel.WineryId)
                                {
                                    createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                                    createReservationResponse.error_info.extra_info = "Reservation Conflict Error";
                                    createReservationResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} from {1} to {2}. Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd yyyy"));
                                    
                                    IsReservationConflict = true;
                                }
                            }
                        }

                        CreateReservation createRSVPResponse = new CreateReservation();

                        if (IsReservationConflict == true)
                        {
                            createRSVPResponse.Status = Common.ResponseStatus.Failed;
                            createRSVPResponse.error_type = createReservationResponse.error_info.error_type;
                            createRSVPResponse.Message = createReservationResponse.error_info.extra_info;
                            createRSVPResponse.Description = createReservationResponse.error_info.description;
                        }
                        else
                        {
                            createRSVPResponse = eventDAL.SaveReservation(createReservationModel, false, false, false, createReservationModel.TotalGuests, currentuser, false, model.ticket_order_id);
                        }
                        
                        if (createRSVPResponse.Status == Common.ResponseStatus.Success)
                        {
                            rsvpInfo.Add(createRSVPResponse);
                        }
                        else
                        {
                            itineraryResponse.success = false;
                            itineraryResponse.error_info.error_type = createRSVPResponse.error_type;
                            itineraryResponse.error_info.extra_info = createRSVPResponse.Message;
                            itineraryResponse.error_info.description = createRSVPResponse.Description;
                            //itineraryResponse.error_info.description = createRSVPResponse.Message;
                            itineraryResponse.error_info.error_data = new ViewModels.ErrorData
                            {
                                end_date = createReservationModel.EventDate.Add(createReservationModel.EndTime),
                                start_date = createReservationModel.EventDate.Add(createReservationModel.StartTime),
                                event_name = createReservationModel.EventName,
                                location_name = createReservationModel.EventLocation
                            };
                            logDAL.InsertLog("WebApi", "ConfirmItinerary RSVP creation failed::" + JsonConvert.SerializeObject(itineraryResponse), HttpContext.Request.Headers["AuthenticateKey"], 3, 0);
                            break;
                        }

                    }

                    if (itineraryResponse.success == true)
                    {
                        itineraryResponse.data = rsvpInfo;
                        //update rsvp status & Itinerary status

                        List<int> rsvpIds = rsvpInfo.Select(r => r.Id).ToList();

                        bool isSuccess = eventDAL.UpdateItineraryAndRSVPStatus(rsvpIds, model.ticket_order_id);

                        if (isSuccess)
                        {
                            QueueService getStarted = new QueueService();

                            var queueModel = new EmailQueue();
                            queueModel.EType = (int)Common.Email.EmailType.TicketSale;
                            queueModel.BCode = "";
                            queueModel.UId = 0;
                            queueModel.RsvpId = model.ticket_order_id;
                            queueModel.PerMsg = "";
                            queueModel.Src = 0;
                            var qData = JsonConvert.SerializeObject(queueModel);

                            AppSettings appsettings = _appSetting.Value;
                            getStarted.AddMessageIntoQueue(appsettings, qData).Wait();
                        }
                        itineraryResponse.success = isSuccess;
                    }

                    if (itineraryResponse.success == false)
                    {
                        List<int> rsvpIds = rsvpInfo.Select(r => r.Id).ToList();
                        logDAL.InsertLog("WebApi", "ConfirmItinerary Cancelling RSVPS::" + string.Join(",", rsvpIds.Select(i => i.ToString()).ToArray()), HttpContext.Request.Headers["AuthenticateKey"], 3, 0);
                        eventDAL.CancelRSVPForItinerary(rsvpIds);
                        CancelAndRefundTicketOrder(model.ticket_order_id);
                    }
                }
                else
                {
                    //no reservations booked, cancel the order
                    CancelAndRefundTicketOrder(model.ticket_order_id);
                }


            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("WebApi", "ConfirmItinerary:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        private async void CancelAndRefundTicketOrder(int ticketOrderId)
        {
            TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);

            TicketsOrderPaymentDetail ticketsOrderPaymentDetail = ticketDAL.GetTicketsOrderPaymentDetail(ticketOrderId);

            if (ticketsOrderPaymentDetail.ticket_order_id > 0)
            {
                if (ticketsOrderPaymentDetail.amount > 0)
                {
                    TicketRefundRequest request = new TicketRefundRequest();

                    request.amount = ticketsOrderPaymentDetail.amount;
                    request.card_token = ticketsOrderPaymentDetail.card_token;
                    request.charge_type = Common.Payments.Transaction.ChargeType.Void;

                    request.payment_gateway = ticketsOrderPaymentDetail.payment_gateway;

                    request.pay_card_custName = ticketsOrderPaymentDetail.pay_card_custName;
                    request.pay_card_exp_month = ticketsOrderPaymentDetail.pay_card_exp_month;
                    request.pay_card_exp_year = ticketsOrderPaymentDetail.pay_card_exp_year;
                    request.pay_card_last_four_digits = ticketsOrderPaymentDetail.pay_card_last_four_digits;
                    //request.pay_card_first_four_digits = ticketsOrderPaymentDetail.pay_card_first_four_digits;
                    request.pay_card_number = ticketsOrderPaymentDetail.pay_card_number;
                    request.pay_card_type = ticketsOrderPaymentDetail.pay_card_type;
                    request.ticket_order_id = ticketOrderId;
                    request.transaction_id = ticketsOrderPaymentDetail.transaction_id;

                    Services.Payments objPayments = new Services.Payments(_appSetting);
                    await Services.Payments.RefundTicket(request);
                }
            }
            
            ticketDAL.UpdateVoidTicketsOrder(ticketOrderId);
        }


        [Route("additemToitinerary")]
        [HttpPost]
        public IActionResult AddItemToItinerary([FromBody]ItineraryPlannerItemRequest model)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            //string rawPostData = getRawPostData();
            //string debugData = "";
            //if (!string.IsNullOrWhiteSpace(rawPostData))
            //    debugData = " Raw Data:" + rawPostData;

            //logDAL.InsertLog("WebApi", "AddItemToItinerary request:" + debugData, HttpContext.Request.Headers["AuthenticateKey"], 3, 0);

            SaveItineraryItemResponse itineraryResponse = new SaveItineraryItemResponse();
            if (model == null)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No data sent in request";
                itineraryResponse.error_info.description = "request model cannot be null.";
                return new ObjectResult(itineraryResponse);
            }
            if (model.itinerary_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Itineary Id not found";
                itineraryResponse.error_info.description = "Itineary Id not found";
                return new ObjectResult(itineraryResponse);
            }

            try
            {
                SaveItineraryItemResponseModel data = new SaveItineraryItemResponseModel();
                if (model.id == 0 && string.IsNullOrWhiteSpace(model.item_guid))
                    model.item_guid = Guid.NewGuid().ToString();

                ItineraryPlannerItem item = model as ItineraryPlannerItem;

                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                int id = dal.SaveItineraryItem(item, model.itinerary_id);
                data.item_guid = model.item_guid;
                data.item_id = id;

                itineraryResponse.success = true;
                itineraryResponse.data = data;

            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "additemToitinerary:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("deleteitineraryitem")]
        [HttpPost]
        public IActionResult DeleteItineraryItem([FromBody]RemoveItineraryItemRequest model)
        {
            SaveItineraryItemResponse itineraryResponse = new SaveItineraryItemResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            if (model == null)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No data sent in request";
                itineraryResponse.error_info.description = "request model cannot be null.";
                return new ObjectResult(itineraryResponse);
            }
            if (model.item_id <= 0 && string.IsNullOrWhiteSpace(model.item_guid))
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Itineary Item Id not found";
                itineraryResponse.error_info.description = "Itineary Item Id not found";
                return new ObjectResult(itineraryResponse);
            }

            try
            {

                logDAL.InsertLog("WebApi", "DeleteItineraryItem request:" + JsonConvert.SerializeObject(model), HttpContext.Request.Headers["AuthenticateKey"], 3, 0);
                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                bool success = dal.DeleteItineraryItem(model.item_id, model.item_guid);


                itineraryResponse.success = success;

            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "DeleteItineraryItem:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("addreservationtoitinerary")]
        [HttpPost]
        public IActionResult AddReservationToItinerary([FromBody]AddReservationToItineraryRequest model)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            //string rawPostData = getRawPostData();
            //string debugData = "";
            //if (!string.IsNullOrWhiteSpace(rawPostData))
            //    debugData = " Raw Data:" + rawPostData;

            //logDAL.InsertLog("WebApi", "AddReservationToItinerary request:" + debugData, HttpContext.Request.Headers["AuthenticateKey"], 3, 0);

            SaveItineraryItemResponse itineraryResponse = new SaveItineraryItemResponse();
            if (model == null)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No data sent in request";
                itineraryResponse.error_info.description = "request model cannot be null.";
                return new ObjectResult(itineraryResponse);
            }
            if (model.itinerary_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Itineary Id not found";
                itineraryResponse.error_info.description = "Itineary Id not found";
                return new ObjectResult(itineraryResponse);
            }
            if (model.reservation_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Invalid reservation Id";
                itineraryResponse.error_info.description = "Invalid reservation Id";
                return new ObjectResult(itineraryResponse);
            }

            try
            {
                SaveItineraryItemResponseModel data = new SaveItineraryItemResponseModel();


                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                string itemGUID = Guid.NewGuid().ToString();
                int id = dal.AddReservationToItinerary(model.reservation_id, model.itinerary_id, itemGUID);
                data.item_guid = itemGUID;
                data.item_id = id;

                itineraryResponse.success = true;
                itineraryResponse.data = data;

            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "additemToitinerary:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        private String getRawPostData()
        {
            string content = "";
            using (StreamReader reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8))
            {
                if (reader.EndOfStream)
                {
                    reader.BaseStream.Position = 0;
                }
                content = reader.ReadToEnd();
            }
            return content;
        }

        [Route("createitinerarypassport")]
        [HttpPost]
        public IActionResult CreateItineraryForPassport([FromBody]AddPassportItinerary model)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            //string rawPostData = getRawPostData();
            //string debugData = "";
            //if (!string.IsNullOrWhiteSpace(rawPostData))
            //    debugData = " Raw Data:" + rawPostData;

            //logDAL.InsertLog("WebApi", "CreateItineraryForPassport request:" + debugData, HttpContext.Request.Headers["AuthenticateKey"], 3, model.member_id);

            CreateItineraryForPassportResponse itineraryResponse = new CreateItineraryForPassportResponse();
            if (model == null)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No data sent in request";
                itineraryResponse.error_info.description = "request model cannot be null.";
                return new ObjectResult(itineraryResponse);
            }
            if (model.ticket_event_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "ticket Id not found";
                itineraryResponse.error_info.description = "ticket Id not found";
                return new ObjectResult(itineraryResponse);
            }
            if (model.slot_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Invalid slot Id";
                itineraryResponse.error_info.description = "Invalid slot Id";
                return new ObjectResult(itineraryResponse);
            }

            if (model.user_id <= 0 && (model.user_info == null || string.IsNullOrEmpty(model.user_info.first_name) || string.IsNullOrEmpty(model.user_info.last_name) || string.IsNullOrEmpty(model.user_info.phone_number)))
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Guest;
                itineraryResponse.error_info.extra_info = "Guest Error";
                itineraryResponse.error_info.description = "Please fill out all required guest fields.";
                return new ObjectResult(itineraryResponse);
            }
            if (model.user_id <= 0 && model.member_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Guest;
                itineraryResponse.error_info.extra_info = "Guest Error";
                itineraryResponse.error_info.description = "MemberId required for new guest";
                return new ObjectResult(itineraryResponse);
            }

            try
            {
                PassportItineraryResponseModel data = new PassportItineraryResponseModel();
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                bool existingUser = (model.user_id > 0);

                if (!existingUser)
                {
                    if (string.IsNullOrEmpty(model.user_info.email))
                    {
                        model.user_info.email = Utility.GenerateUsername(model.user_info.first_name, model.user_info.last_name);
                    }
                    else {
                        var userDetailModel = userDAL.GetUserDetailsbyemail(model.user_info.email, 0);
                        if (userDetailModel != null && userDetailModel.user_id > 0)
                        {
                            model.user_id = userDetailModel.user_id;
                            existingUser = true;
                        }

                    }

                }
                if (!existingUser)
                {
                    string GuestPwd = StringHelpers.GenerateRandomString(8, false);
                    int mobilePhoneStatus = (int)Utility.SMSVerified_System(model.user_info.phone_number);

                    model.user_info.mobile_phone_status = (MobileNumberStatus)mobilePhoneStatus;
                    if (model.user_info.address == null)
                    {
                        model.user_info.address = new ReservationUserAddress();
                    }
                    model.user_id = userDAL.CreateUser(model.user_info.email, GuestPwd, model.user_info.first_name, model.user_info.last_name, model.user_info.address.country, model.user_info.address.zip_code, model.user_info.phone_number, model.user_info.customer_type, (int)model.user_info.mobile_phone_status, 0, "", model.user_info.address.city, model.user_info.address.state, model.user_info.address.address_1, model.user_info.address.address_2, Common.Common.GetSource(HttpContext.Request.Headers["AuthenticateKey"]));

                    userDAL.UpdateUserWinery(model.user_id, model.member_id, 4, "", "", "", model.user_info.customer_type);
                }
                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                //string itemGUID = Guid.NewGuid().ToString();

                TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                bool DisableTravelTimeRestrictions = ticketDAL.CheckDisableTravelTimeRestrictionsByEventId(model.ticket_event_id);

                data = dal.CreatePassportInventory(model, _appSetting.Value.GoogleAPIKey, DisableTravelTimeRestrictions);

                data.user_id = model.user_id;
                if (data != null)
                {
                    itineraryResponse.success = true;
                    itineraryResponse.data = data;
                    //logDAL.InsertLog("WebApi", "CreateItineraryForPassport  " + debugData + ", response:" + JsonConvert.SerializeObject(itineraryResponse), HttpContext.Request.Headers["AuthenticateKey"], 3, model.member_id);
                }
                else
                {
                    itineraryResponse.success = false;
                    itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    itineraryResponse.error_info.extra_info = "Could not add the event to Itinerary. Please try again";
                    itineraryResponse.error_info.description = "Could not add the event to Itinerary. Please try again";

                    //logDAL.InsertLog("WebApi", "CreateItineraryForPassport  " + debugData + ", error: Could not add the event to Itinerary. Please try again" , HttpContext.Request.Headers["AuthenticateKey"], 3, model.member_id);
                }

            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                

                logDAL.InsertLog("WebApi", "CreateItineraryForPassport:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("updateitinerarypassport")]
        [HttpPost]
        public IActionResult UpdateItineraryForPassport([FromBody]UpdatePassportItinerary model)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            //string rawPostData = getRawPostData();
            //string debugData = "";
            //if (!string.IsNullOrWhiteSpace(rawPostData))
            //    debugData = " Raw Data:" + rawPostData;

            //logDAL.InsertLog("WebApi", "UpdateItineraryForPassport request:" + debugData, HttpContext.Request.Headers["AuthenticateKey"], 3, 0);

            CreateItineraryForPassportResponse itineraryResponse = new CreateItineraryForPassportResponse();
            if (model == null)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No data sent in request";
                itineraryResponse.error_info.description = "request model cannot be null.";
                return new ObjectResult(itineraryResponse);
            }
            if (model.itinerary_item_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Itinerary item id not found";
                itineraryResponse.error_info.description = "Itinerary item id not found";
                return new ObjectResult(itineraryResponse);
            }
            if (model.slot_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Invalid slot Id";
                itineraryResponse.error_info.description = "Invalid slot Id";
                return new ObjectResult(itineraryResponse);
            }

            try
            {
                PassportItineraryResponseModel data = new PassportItineraryResponseModel();

                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                //string itemGUID = Guid.NewGuid().ToString();
                data = dal.UpdatePassportInventory(model, _appSetting.Value.GoogleAPIKey);

                if (data != null)
                {
                    itineraryResponse.success = true;
                    itineraryResponse.data = data;
                    //logDAL.InsertLog("WebApi", "UpdateItineraryForPassport  " + debugData + ", response:" + JsonConvert.SerializeObject(itineraryResponse), HttpContext.Request.Headers["AuthenticateKey"], 3, 0);
                }
                else
                {
                    itineraryResponse.success = false;
                    itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    itineraryResponse.error_info.extra_info = "Could not update event in Itinerary. Please try again";
                    itineraryResponse.error_info.description = "Could not update event in Itinerary. Please try again";
                }

            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "UpdateItineraryForPassport:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("checkifdestinationvalid")]
        [HttpGet]
        public IActionResult CheckIfDestinationValid(int itinerary_id, int slot_id, int slot_type, DateTime request_date, int item_id=0)
        {
            CheckIfDestinationValidResponse itineraryResponse = new CheckIfDestinationValidResponse();

            if (itinerary_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Invalid Itinerary Id";
                itineraryResponse.error_info.description = "Invalid Itinerary Id";
                return new ObjectResult(itineraryResponse);
            }
            if (slot_id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Invalid slot Id";
                itineraryResponse.error_info.description = "Invalid slot Id";
                return new ObjectResult(itineraryResponse);
            }

            try
            {
                CheckDistanceTravelTimeResponseModel data = new CheckDistanceTravelTimeResponseModel();


                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                //string itemGUID = Guid.NewGuid().ToString();
                data = dal.CheckTravelTimeDistance(itinerary_id, slot_id, slot_type, request_date, _appSetting.Value.GoogleAPIKey, item_id);

                if (data != null)
                {
                    itineraryResponse.success = true;
                    itineraryResponse.data = data;
                }
                else
                {
                    itineraryResponse.success = false;
                    itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    itineraryResponse.error_info.extra_info = "Unknown error occured. Please try again";
                    itineraryResponse.error_info.description = "Unknown error occured. Please try again";
                }

            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CheckIfDestinationValid:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("getlistbyuser")]
        [HttpGet]
        public IActionResult GetItineraryListByUser(int userId)
        {
            GetItineraryListResponse itineraryResponse = new GetItineraryListResponse();

            if (userId <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "Itineary Id not found";
                itineraryResponse.error_info.description = "Itineary Id not found";
                return new ObjectResult(itineraryResponse);
            }

            try
            {
                List<ItineraryPlannerViewModel> data = new List<ItineraryPlannerViewModel>();
                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                //ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString_readonly);
                data = dal.GetItineraryListByUser(userId);
                if (data != null && data.Count > 0)
                {
                    itineraryResponse.success = true;
                    itineraryResponse.data = data;
                }
                else
                {
                    itineraryResponse.success = true;
                    itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    itineraryResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetItineraryListByUser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("itinerarydetail")]
        [HttpGet]
        public IActionResult GetItineraryDetails(int id = 0, string itinerary_guid = "")
        {
            GetItineraryDetailResponse itineraryResponse = new GetItineraryDetailResponse();

            if (id <= 0 && string.IsNullOrWhiteSpace(itinerary_guid))
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No data passed";
                itineraryResponse.error_info.description = "Itineary Id or GUID is required. No data found";
                return new ObjectResult(itineraryResponse);
            }

            try
            {
                ItineraryPlannerViewModel data = new ItineraryPlannerViewModel();
                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                //ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString_readonly);
                data = dal.GetItineraryDetails(id, itinerary_guid, _appSetting.Value.GoogleAPIKey);
                if (data != null)
                {
                    itineraryResponse.success = true;
                    itineraryResponse.data = data;
                }
                else
                {
                    itineraryResponse.success = true;
                    itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    itineraryResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetItineraryListByUser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("itinerarydetailmyaccount")]
        [HttpGet]
        public IActionResult GetItineraryDetailsMyAccount(int id = 0, string itinerary_guid = "")
        {
            GetItineraryDetailResponse itineraryResponse = new GetItineraryDetailResponse();

            if (id <= 0 && string.IsNullOrWhiteSpace(itinerary_guid))
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryResponse.error_info.extra_info = "No data passed";
                itineraryResponse.error_info.description = "Itineary Id or GUID is required. No data found";
                return new ObjectResult(itineraryResponse);
            }

            try
            {
                ItineraryPlannerViewModel data = new ItineraryPlannerViewModel();
                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                //ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString_readonly);
                data = dal.GetItineraryDetailsMyAccount(id, itinerary_guid, _appSetting.Value.GoogleAPIKey);
                if (data != null)
                {
                    itineraryResponse.success = true;
                    itineraryResponse.data = data;
                }
                else
                {
                    itineraryResponse.success = true;
                    itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    itineraryResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetItineraryListByUser:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("otalist")]
        [HttpGet]
        public IActionResult GetOTAList()
        {
            GetOTAListResponse itineraryResponse = new GetOTAListResponse();

            try
            {
                List<ItineraryPlanner_Ota> data = new List<ItineraryPlanner_Ota>();
                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                data = dal.GetOTAList();
                if (data != null)
                {
                    itineraryResponse.success = true;
                    itineraryResponse.data = data;
                }
                else
                {
                    itineraryResponse.success = true;
                    itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    itineraryResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetOTAList:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("bookingtypelist")]
        [HttpGet]
        public IActionResult GetBookingTypeList()
        {
            GetBookingTypeListResponse itineraryResponse = new GetBookingTypeListResponse();

            try
            {
                List<ItineraryPlanner_BookingType> data = new List<ItineraryPlanner_BookingType>();
                //ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString_readonly);
                data = dal.GetBookingTypeList();
                if (data != null)
                {
                    itineraryResponse.success = true;
                    itineraryResponse.data = data;
                }
                else
                {
                    itineraryResponse.success = true;
                    itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    itineraryResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetBookingTypeList:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }


        [Route("deleteitinerary")]
        [HttpPost]
        public IActionResult DeleteItinerary([FromBody]int id)
        {
            BaseResponse itineraryResponse = new BaseResponse();
            if (id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                itineraryResponse.error_info.extra_info = "Itineary Id not found";
                itineraryResponse.error_info.description = "Itineary Id not found";
                return new ObjectResult(itineraryResponse);
            }

            try
            {

                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                bool success = dal.DeleteItinerary(id);

                itineraryResponse.success = success;
            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "DeleteItinerary:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("updateinactive")]
        [HttpPost]
        public IActionResult UpdateItineraryInactive([FromBody]int id)
        {
            BaseResponse itineraryResponse = new BaseResponse();
            if (id <= 0)
            {
                itineraryResponse.success = false;
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                itineraryResponse.error_info.extra_info = "Itineary Id not found";
                itineraryResponse.error_info.description = "Itineary Id not found";
                return new ObjectResult(itineraryResponse);
            }

            try
            {

                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                bool success = dal.UpdateItineraryStatus(id, Common.Common.Itinerary_Status.InActive);

                itineraryResponse.success = success;
            }
            catch (Exception ex)
            {
                itineraryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "DeleteItinerary:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryResponse);
        }

        [Route("userreservationsforitinerary")]
        [HttpGet]
        public IActionResult GetUserReservationsForItinerary(int userId, DateTime toDate, DateTime fromDate)
        {
            GetItineraryUserReservationsResponse itineraryUserReservationsResponse = new GetItineraryUserReservationsResponse();

            if (userId <= 0 && toDate == null && fromDate == null)
            {
                itineraryUserReservationsResponse.success = false;
                itineraryUserReservationsResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                itineraryUserReservationsResponse.error_info.extra_info = "User Id not found";
                itineraryUserReservationsResponse.error_info.description = "User Id not found";
                return new ObjectResult(itineraryUserReservationsResponse);
            }

            try
            {
                List<ItineraryUserReservationsModel> data = new List<ItineraryUserReservationsModel>();
                ItineraryDAL dal = new ItineraryDAL(Common.Common.ConnectionString);
                data = dal.GetUserReservationsForItinerary(userId, toDate, fromDate);
                if (data != null && data.Count > 0)
                {
                    itineraryUserReservationsResponse.success = true;
                    itineraryUserReservationsResponse.data = data;
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(data[i].member_business_phone))
                        {
                            data[i].member_business_phone = Utility.FormatPhoneNumber(data[i].member_business_phone);
                        }
                    }
                }
                else
                {
                    itineraryUserReservationsResponse.success = true;
                    itineraryUserReservationsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    itineraryUserReservationsResponse.error_info.extra_info = "no record found";
                }


            }
            catch (Exception ex)
            {
                itineraryUserReservationsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                itineraryUserReservationsResponse.error_info.extra_info = Common.Common.InternalServerError;
                itineraryUserReservationsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetUserReservationsForItinerary:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(itineraryUserReservationsResponse);
        }
    }
}