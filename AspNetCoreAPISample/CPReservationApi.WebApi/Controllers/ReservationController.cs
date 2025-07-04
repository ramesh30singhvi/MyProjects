using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.WebApi.ViewModels;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.Services;
using Microsoft.Extensions.Options;
using CPReservationApi.Common;
using static CPReservationApi.Common.Email;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq;
using static CPReservationApi.Common.Payments;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using eWineryWebServices;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/reservation")]
    public class ReservationController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public ReservationController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }
        /// <summary>
        /// This method is used to search reservations by various parameters like start and end date, booking code etc for a member
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("reservations")]
        [HttpGet]
        public async Task<IActionResult> GetReservationsByFilters(ReservationRequest model)
        {
            var reservationResponse = new ReservationResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationModel = new List<ReservationEvent>();

                string WhereClause = " where r.Status not in (7,8)";

                if (model.member_id > 0)
                {
                    WhereClause += " And r.WineryId=" + model.member_id;
                }

                if (model.location_ids.Length > 0)
                {
                    WhereClause += " And r.LocationId in (" + model.location_ids + ")";
                }

                if (model.user_id > 0)
                {
                    WhereClause += " And r.userId=" + model.user_id;
                }

                if (model.reservation_id > 0)
                {
                    WhereClause += " And r.reservationId=" + model.reservation_id;
                }

                if (model.email.Trim().Length > 0)
                {
                    WhereClause += " And r.email='" + model.email + "'";
                }

                if (model.last_name.Trim().Length > 0)
                {
                    WhereClause += " And r.LastName like '" + model.last_name + "%'";
                }

                if (model.phone_number.Trim().Length > 0)
                {
                    WhereClause += " And REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(PhoneNumber,'+1',''),'1 (',''),')',''),'(',''),'+',''),'-',''),' ','') like '%" + Utility.ExtractPhone(model.phone_number).ToString() + "%'";
                }

                if (model.booking_code.Trim().Length > 0)
                {
                    WhereClause += " And r.bookingCode='" + model.booking_code + "'";
                }

                if (model.event_name.Trim().Length > 0)
                {
                    WhereClause += " And r.eventName='" + model.event_name + "'";
                }

                if (model.start_date != null && model.end_date != null)
                {
                    DateTime starttime = Convert.ToDateTime(model.start_date);
                    DateTime endtime = Convert.ToDateTime(model.end_date);

                    if (model.mode == 0)
                    {
                        WhereClause += " And (CAST(r.eventdate AS datetime) + CAST(r.starttime AS datetime)) >='" + starttime.ToString("yyyy-MM-dd hh:mm tt") + "' And (CAST(r.eventdate AS datetime) + CAST(r.starttime AS datetime)) <='" + endtime.ToString("yyyy-MM-dd hh:mm tt") + "'";
                    }
                    else
                    {
                        WhereClause += " And r.BookingDate >='" + starttime.ToString("yyyy-MM-dd hh:mm tt") + "' And r.BookingDate <='" + endtime.ToString("yyyy-MM-dd hh:mm tt") + "'";
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.floor_plan_ids))
                {
                    WhereClause += " And (r.FloorPlanId in (" + model.floor_plan_ids.Trim() + ") or Isnull(ss.Floor_Plan_Id, 0) in (" + model.floor_plan_ids.Trim() + ")) ";
                }

                reservationModel = eventDAL.GetReservationsByFilters(WhereClause);

                if (reservationModel != null)
                {
                    foreach (var item in reservationModel)
                    {
                        foreach (var item1 in item.event_times)
                        {
                            foreach (var item2 in item1.reservations)
                            {
                                if (!string.IsNullOrWhiteSpace(item2.reservation_holder.mobile_phone))
                                    item2.reservation_holder.mobile_phone = Utility.FormatTelephoneNumber(item2.reservation_holder.mobile_phone, item2.country);
                                if (!string.IsNullOrWhiteSpace(item2.reservation_holder.phone))
                                    item2.reservation_holder.phone = Utility.FormatTelephoneNumber(item2.reservation_holder.phone, item2.country);
                            }
                        }
                    }

                    reservationResponse.success = true;
                    reservationResponse.data = reservationModel;
                }
                else
                {
                    reservationResponse.success = true;
                    reservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetReservationsByFilters:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
            }
            return new ObjectResult(reservationResponse);
        }
        /// <summary>
        /// This method gives details about an existing reservation
        /// </summary>
        /// <param name="reservation_id">Id of Reservation (optional)</param>
        /// <param name="booking_code">Id of Reservation (optional)</param>
        /// <returns></returns>
        [Route("reservationdetail")]
        [HttpGet]
        public async Task<IActionResult> GetReservationDetailsbyReservationId(int reservation_id, string booking_code, string booking_guid, bool show_cancel_reason)
        {
            var reservationDetailResponse = new ReservationDetailResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationDetailModel = new ReservationDetailModel();
                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(reservation_id, booking_code, IsAdmin, booking_guid, true, includeCancelledReasons: show_cancel_reason);

                if (reservationDetailModel != null && reservationDetailModel.reservation_id > 0)
                {
                    if (!string.IsNullOrEmpty(reservationDetailModel.pay_card.number))
                    {
                        string cardNumber = StringHelpers.Decryption(reservationDetailModel.pay_card.number);
                        if (!string.IsNullOrEmpty(cardNumber))
                        {
                            reservationDetailModel.pay_card.number = Common.Common.Right(cardNumber, 4);
                            reservationDetailModel.payment_details = string.Format("{0} -{1} {2}/{3}", reservationDetailModel.pay_card.card_type, reservationDetailModel.pay_card.number, reservationDetailModel.pay_card.exp_month, reservationDetailModel.pay_card.exp_year);
                        }
                    }
                    if (reservationDetailModel.user_detail != null && reservationDetailModel.user_detail.address != null)
                    {
                        if (!string.IsNullOrWhiteSpace(reservationDetailModel.user_detail.mobile_phone))
                            reservationDetailModel.user_detail.mobile_phone = Utility.FormatTelephoneNumber(reservationDetailModel.user_detail.mobile_phone, reservationDetailModel.user_detail.address.country);
                        if (!string.IsNullOrWhiteSpace(reservationDetailModel.user_detail.phone_number))
                            reservationDetailModel.user_detail.phone_number = Utility.FormatTelephoneNumber(reservationDetailModel.user_detail.phone_number, reservationDetailModel.user_detail.address.country);

                    }
                    if (!string.IsNullOrEmpty(reservationDetailModel.location_phone))
                    {
                        string country = "US";
                        if (!string.IsNullOrEmpty(reservationDetailModel.location_country))
                            country = reservationDetailModel.location_country;

                        reservationDetailModel.location_phone = Utility.FormatTelephoneNumber(reservationDetailModel.location_phone, country);

                    }

                    reservationDetailResponse.success = true;
                    reservationDetailResponse.data = reservationDetailModel;
                }
                else
                {
                    reservationDetailResponse.success = true;
                    reservationDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationDetailsbyReservationId: rsvpId-" + reservation_id.ToString() + ", BookingCode-" + booking_code + ", Booking_guid-" + booking_guid + ", Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationDetailResponse);
        }
        /// <summary>
        /// This method checks reservation conflicts
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("checkreservationconflicts")]
        [HttpGet]
        public IActionResult CheckReservationConflicts(ReservationConflictRequest model)
        {
            var reservationConflictResponse = new ReservationConflictResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                ReservationConflicts reservationConflicts = new ReservationConflicts();
                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                reservationConflicts = eventDAL.CheckReservationConflicts(model.slot_id, model.slot_type, model.req_date, model.no_of_guests, model.reservation_id, model.user_id, 0, model.member_id, IsAdmin);

                reservationConflictResponse.success = reservationConflicts.success;
                reservationConflictResponse.error_info.error_type = reservationConflicts.error_type;
                reservationConflictResponse.error_info.extra_info = reservationConflicts.extra_info;
                reservationConflictResponse.error_info.description = reservationConflicts.description;
            }
            catch (Exception ex)
            {
                reservationConflictResponse.success = false;
                reservationConflictResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationConflictResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationConflictResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CheckReservationConflicts:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
            }
            return new ObjectResult(reservationConflictResponse);
        }

        private bool requiresCreditCard(int DepositPolicy)
        {
            bool reqCC = false;
            switch (DepositPolicy)
            {
                case 1:
                case 4:
                case 5:
                case 6:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                    reqCC = true;
                    break;
            }
            return reqCC;
        }

        [Route("taxcalculation")]
        [HttpPost]
        public async Task<IActionResult> TaxCalculation([FromBody]TaxCalculationRequest reqmodel)
        {
            var taxCalculationResponse = new TaxCalculationResponse();
            try
            {
                Discount.EventDiscountResult eventDiscountResult = new Discount.EventDiscountResult();
                Discount.EventDiscountResult addOnsDiscountResult = new Discount.EventDiscountResult();

                List<DiscountDetailsModel> discount_details = new List<DiscountDetailsModel>();

                decimal discountAmount = 0;
                decimal addOnsDiscountAmount = 0;

                if (string.IsNullOrEmpty(reqmodel.email))
                    reqmodel.email = "";

                bool IsAdmin = Convert.ToBoolean(Request.Headers["IsAdmin"]);

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                EventModel eventModel = new EventModel();

                if (reqmodel.event_id > 0)
                    eventModel = eventDAL.GetEventById(reqmodel.event_id);

                if (eventModel.EventID == 0)
                    eventModel.FeeType = reqmodel.fee_type;

                if (reqmodel.location_id == 0 && reqmodel.floor_plan_id > 0)
                    reqmodel.location_id = eventDAL.GetLocationIdByFloorPlanId(reqmodel.floor_plan_id);

                List<ActivationCodesModel> ActivationCodes = new List<ActivationCodesModel>();
                var reservationDetailModel = new ReservationDetail2Model();

                if (reqmodel.reservation_id > 0 && string.IsNullOrEmpty(reqmodel.email))
                {
                    reservationDetailModel = eventDAL.GetReservationDetails2byReservationId(reqmodel.reservation_id);

                    if (reservationDetailModel.user_detail != null)
                    {
                        reqmodel.email = reservationDetailModel.user_detail.email;
                    }
                }

                if (!reqmodel.ignore_discount)
                {
                    if (reqmodel.discount_amount > 0)
                        discountAmount = reqmodel.discount_amount;
                    else if (reqmodel.event_id > 0)
                    {
                        string email = reqmodel.email;
                        if (string.IsNullOrEmpty(reqmodel.email) && reqmodel.user_id > 0)
                        {
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            email = userDAL.GetUserEmailById(reqmodel.user_id);
                        }

                        eventDiscountResult = await Discount.CheckAndApplyEventDiscount(eventModel, reqmodel.quantity, reqmodel.fee_per_person, reqmodel.discount_code, eventModel.MemberID, email, reqmodel.activation_codes,  Convert.ToDateTime(reqmodel.event_date), eventModel.FeeType, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                        if (reqmodel.addon_info != null && reqmodel.addon_info.Count > 0)
                            addOnsDiscountResult = await Discount.CheckAndApplyEventAddOnsDiscount(reqmodel.addon_info, eventModel.MemberID, email, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                        //result = Discount.DiscountCodeApply(reqmodel.event_id, reqmodel.quantity, reqmodel.fee_per_person, reqmodel.discount_code);
                        if (eventDiscountResult.DiscountValid)
                        {
                            discountAmount = eventDiscountResult.DiscountTotal;

                            if (eventDiscountResult.club_discount_details != null && eventDiscountResult.club_discount_details.Count>0)
                                discount_details.AddRange(eventDiscountResult.club_discount_details);

                            if (eventDiscountResult.ActivationCodes != null && eventDiscountResult.ActivationCodes.Count > 0)
                            {
                                foreach (var item in eventDiscountResult.ActivationCodes)
                                {
                                    ActivationCodesModel code = new ActivationCodesModel();
                                    code.activation_code = item.ActivationCode;
                                    code.discount_desc = item.DiscountDesc;
                                    code.is_valid = item.IsValid;
                                    code.ticket_id = item.TicketId;
                                    ActivationCodes.Add(code);
                                }
                            }
                        }

                        if (addOnsDiscountResult.DiscountValid)
                        {
                            discountAmount = discountAmount + addOnsDiscountResult.DiscountTotal;

                            addOnsDiscountAmount = addOnsDiscountResult.DiscountTotal;

                            if (addOnsDiscountResult.club_discount_details != null && addOnsDiscountResult.club_discount_details.Count > 0)
                                discount_details.AddRange(addOnsDiscountResult.club_discount_details);
                        }
                    }
                }

                decimal taxPercent = 0;

                try
                {
                    if (!reqmodel.ignore_sales_tax || reqmodel.tax_gratuity)
                    {
                        Utility objUtility = new Utility();
                        taxPercent = await objUtility.GetTax(reqmodel.event_id, 1, 100, reqmodel.location_id, false);
                    }
                }
                catch { }


                decimal addOnTotal = 0;
                decimal addOnTaxableTotal = 0;
                decimal gratuityTotal = 0;
                decimal gratuityPercent = reqmodel.gratuity_percentage;
                bool taxGratuity = reqmodel.tax_gratuity;
                decimal taxAmount = 0;
                decimal taxableGratuityAmt = 0;

                decimal rsvpTotal = (reqmodel.quantity * reqmodel.fee_per_person) - discountAmount;

                if (eventModel.FeeType == 1)
                    rsvpTotal = reqmodel.fee_per_person - discountAmount;

                foreach (var item in reqmodel.addon_info)
                {
                    if (item.Taxable)
                        addOnTaxableTotal += item.price * item.qty;
                    addOnTotal += item.price * item.qty;
                    if ((!IsAdmin || gratuityPercent > 0) && (rsvpTotal + addOnTotal) > 0)
                    {
                        if (item.calculate_gratuity && gratuityPercent > 0)
                        {
                            decimal itemGratuity = Utility.CalculateGratuity((item.price * item.qty), gratuityPercent);
                            gratuityTotal += itemGratuity;

                            if (itemGratuity > 0 && taxGratuity)
                            {
                                taxableGratuityAmt += itemGratuity;
                            }
                        }
                    }
                }

                if (reqmodel.gratuity_total != 0)
                {
                    if (reqmodel.tax_gratuity)
                        taxableGratuityAmt = reqmodel.gratuity_total;

                    gratuityTotal = reqmodel.gratuity_total;
                }
                else if (gratuityPercent > 0)
                {
                    decimal rsvpGratuity = Utility.CalculateGratuity((reqmodel.quantity * reqmodel.fee_per_person) - discountAmount + addOnsDiscountAmount, gratuityPercent);

                    if (reqmodel.fee_type == 1)
                        rsvpGratuity = Utility.CalculateGratuity(reqmodel.fee_per_person - discountAmount + addOnsDiscountAmount, gratuityPercent);

                    gratuityTotal += rsvpGratuity;
                    if (eventModel.TaxGratuity)
                        taxableGratuityAmt += rsvpGratuity;
                }

                decimal rsvpTaxTotal = addOnTaxableTotal + taxableGratuityAmt;


                decimal OrderTotalWithoutTax = (reqmodel.quantity * reqmodel.fee_per_person) + addOnTotal - discountAmount + gratuityTotal;

                if (reqmodel.fee_type == 1)
                    OrderTotalWithoutTax = reqmodel.fee_per_person + addOnTotal - discountAmount + gratuityTotal;
                
                //if (eventModel.FeeType == 1)
                //    OrderTotalWithoutTax = reqmodel.fee_per_person + addOnTotal - discountAmount + gratuityTotal;

                decimal svcFee = eventDAL.GetServiceFeePaidByGuest(eventModel.MemberID, reqmodel.quantity, OrderTotalWithoutTax, reqmodel.referral_type);

                bool ChargeSalesTax = true;


                if (reqmodel.event_id > 0)
                {
                    ChargeSalesTax = eventModel.ChargeSalesTax;
                }

                if (ChargeSalesTax && reqmodel.fee_per_person > 0)
                {
                    if (eventModel.FeeType == 1)
                        rsvpTaxTotal = rsvpTaxTotal + reqmodel.fee_per_person - discountAmount;
                    else
                        rsvpTaxTotal = rsvpTaxTotal + (reqmodel.quantity * reqmodel.fee_per_person) - discountAmount;
                }


                if ((!reqmodel.ignore_sales_tax) && rsvpTaxTotal > 0)
                {
                    decimal taxAmountRSVP = (rsvpTaxTotal * taxPercent) / 100;
                    string val = String.Format("{0:F2}", taxAmountRSVP);
                    taxAmount += Convert.ToDecimal(val);
                }

                decimal previousdeposit = 0;

                if (reqmodel.reservation_id > 0)
                {
                    previousdeposit = eventDAL.GetPreviousDepositByReservationID(reqmodel.reservation_id);
                }

                decimal balancedue = rsvpTotal + taxAmount + addOnTotal - previousdeposit + svcFee + gratuityTotal;

                //if (addOnTotal < 0 && discountAmount > 0)
                //{
                //    if ((addOnTotal * -1) >= discountAmount)
                //    {
                //        discountAmount = 0;
                //    }
                //}

                //if (addOnTotal < 0 && reqmodel.fee_per_person > 0)
                //{
                //    if ((addOnTotal * -1) > (reqmodel.quantity * reqmodel.fee_per_person))
                //    {
                //        addOnTotal = (reqmodel.quantity * reqmodel.fee_per_person * -1);
                //    }
                //}

                //if (addOnTotal < 0 && discountAmount > 0 && reqmodel.fee_per_person > 0)
                //{
                //    if ((reqmodel.quantity * reqmodel.fee_per_person) + addOnTotal < discountAmount)
                //    {
                //        discountAmount = (reqmodel.quantity * reqmodel.fee_per_person) + addOnTotal;
                //    }
                //}

                if (balancedue < 0 && previousdeposit == 0)
                {
                    balancedue = 0;
                }

                decimal deposit_due_amount = 0;
                decimal deposit_due_percentage = 0;

                if (reqmodel.deposit_policy_id == 11)
                {
                    deposit_due_percentage = Convert.ToDecimal(.25);
                    deposit_due_amount = Math.Round(balancedue / 4, 2);
                }
                else if (reqmodel.deposit_policy_id == 12)
                {
                    deposit_due_percentage = Convert.ToDecimal(.50);
                    deposit_due_amount = Math.Round(balancedue / 2, 2);
                }
                else if (reqmodel.deposit_policy_id == 10 || reqmodel.deposit_policy_id == 4)
                {
                    deposit_due_percentage = 1;
                    deposit_due_amount = balancedue;
                }

                decimal subtotal = (reqmodel.quantity * reqmodel.fee_per_person) + addOnTotal;

                decimal subtotal_after_discount = (reqmodel.quantity * reqmodel.fee_per_person) + addOnTotal - discountAmount;

                if (eventModel.FeeType == 1)
                {
                    subtotal = reqmodel.fee_per_person + addOnTotal;
                    subtotal_after_discount = reqmodel.fee_per_person + addOnTotal - discountAmount;
                }

                if (subtotal < 0)
                    subtotal = 0;

                if (subtotal_after_discount < 0)
                    subtotal_after_discount = 0;

                var model = new TaxCalculationModel
                {
                    subtotal = subtotal,
                    subtotal_after_discount = subtotal_after_discount,
                    sales_tax = taxAmount,
                    discount = discountAmount,
                    balance_due = balancedue,
                    addon_total = addOnTotal,
                    previous_deposit = previousdeposit,
                    sales_tax_percent = taxPercent,
                    tranasction_fee = svcFee,
                    gratuity_total = gratuityTotal,
                    activation_codes = ActivationCodes,
                    taxes_and_fees = svcFee + taxAmount,
                    deposit_due_amount = deposit_due_amount,
                    deposit_due_percentage = deposit_due_percentage,
                    club_discount_details = discount_details,
                    discount_code = (eventDiscountResult.DiscountId == 0 ? "" : reqmodel.discount_code)
                    //,discount_code_valid = (string.IsNullOrEmpty(eventDiscountResult.DiscountMsg) == false && eventDiscountResult.DiscountValid == false ? false : true)
                };
                taxCalculationResponse.success = true;
                taxCalculationResponse.data = model;

                if (!string.IsNullOrEmpty(eventDiscountResult.DiscountMsg))
                    taxCalculationResponse.error_info.description = eventDiscountResult.DiscountMsg;
            }
            catch (Exception ex)
            {
                taxCalculationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                taxCalculationResponse.error_info.extra_info = Common.Common.InternalServerError;
                taxCalculationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "TaxCalculation-:  Request-" + JsonConvert.SerializeObject(reqmodel) + ", Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(taxCalculationResponse);
        }

        [Route("reservationchangesbyeventid")]
        [HttpGet]
        public async Task<IActionResult> GetReservationChangesByEventId(int event_id)
        {
            var reservationChangesResponse = new ReservationChangesResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                EventReservationChangesModel m = eventDAL.GetEventDetailForReservationChanges(event_id);

                decimal EventFee = 0;
                decimal gratuityTotal = 0;

                decimal addOnTotal = 0;
                decimal taxAmount = 0;
                decimal rsvpTaxTotal = 0;
                decimal ServiceFees = 0;
                decimal SalesTaxpercentage = 0;
                decimal addOnTaxableTotal = 0;
                decimal taxableGratuityAmt = 0;

                EventFee = m.FeePerPerson;

                if (m.Taxable)
                    addOnTaxableTotal = m.price;

                addOnTotal = m.price;

                if ((m.GratuityPercentage > 0) && (m.FeePerPerson + addOnTotal) > 0)
                {
                    if (m.calculate_gratuity && m.GratuityPercentage > 0)
                    {
                        decimal itemGratuity = Utility.CalculateGratuity(m.price, m.GratuityPercentage);
                        gratuityTotal += itemGratuity;

                        if (itemGratuity > 0 && m.TaxGratuity)
                        {
                            taxableGratuityAmt += itemGratuity;
                        }
                    }
                }

                if (m.GratuityPercentage > 0)
                {
                    gratuityTotal += Utility.CalculateGratuity(m.FeePerPerson, m.GratuityPercentage);

                    if (m.TaxGratuity)
                        taxableGratuityAmt += Utility.CalculateGratuity(m.FeePerPerson, m.GratuityPercentage);
                }

                rsvpTaxTotal = addOnTaxableTotal + taxableGratuityAmt;

                decimal OrderTotalWithoutTax = m.FeePerPerson + gratuityTotal + addOnTotal;

                decimal svcFee = eventDAL.GetServiceFeePaidByGuest(m.MemberID, 1, OrderTotalWithoutTax);

                if (m.ChargeSalesTax || m.TaxGratuity)
                {
                    Utility objUtility = new Utility();
                    SalesTaxpercentage = await objUtility.GetTaxByEventId(m.MemberID, 100, m.Zip, m.Address1, m.city, m.state);
                }

                if (m.ChargeSalesTax)
                    rsvpTaxTotal = rsvpTaxTotal + m.FeePerPerson;

                if (rsvpTaxTotal > 0)
                    taxAmount = (rsvpTaxTotal * SalesTaxpercentage) / 100;

                string val = String.Format("{0:F2}", EventFee + addOnTotal + gratuityTotal + taxAmount + ServiceFees);

                var model = new ReservationChangesModel
                {
                    all_inclusive_price = Convert.ToDecimal(val)
                };

                reservationChangesResponse.success = true;
                reservationChangesResponse.data = model;
            }
            catch (Exception ex)
            {
                reservationChangesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationChangesResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationChangesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetReservationChangesByEventId:- EventId-" + event_id.ToString() + ", Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationChangesResponse);
        }

        [Route("sharewithfriend")]
        [HttpPost]
        public async Task<IActionResult> ShareWithFriend([FromBody] ShareFriendRequest reqmodel)
        {
            var shareFriendResponse = new ShareFriendResponse();

            if (reqmodel.reservation_id <= 0)
            {
                shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                shareFriendResponse.error_info.extra_info = "Invalid Reservation Id. Please pass a valid one.";
                shareFriendResponse.error_info.description = "Invalid Reservation Id. Please pass a valid one.";
                return new ObjectResult(shareFriendResponse);
            }
            else if (reqmodel.share_friends == null || reqmodel.share_friends.Count == 0)
            {
                shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                shareFriendResponse.error_info.extra_info = "No email addresses found in request. cannot send email";
                shareFriendResponse.error_info.description = "No email addresses found in request. cannot send email";
                return new ObjectResult(shareFriendResponse);
            }
            try

            {
                MailConfig config = new MailConfig
                {
                    Domain = _appSetting.Value.MailGunPostUrl,
                    ApiKey = _appSetting.Value.MainGunApiKey
                };
                ReservationEmailModel model = new ReservationEmailModel
                {
                    SendToFriendMode = true,

                    data = new ReservationEmailModel
                    {
                        RsvpId = reqmodel.reservation_id,
                        BCode = "",
                        UId = 0,
                        GuestEmail = true,
                        isRsvpType = 2
                    },
                    share_friends = reqmodel.share_friends,
                    ShareMessage = reqmodel.share_message,
                    SendCopyToGuest = reqmodel.send_copy_to_user

                };
                model.MailConfig = config;
                AuthMessageSender messageService = new AuthMessageSender();
                var response = await messageService.SendTRSVPEmail(model);
                shareFriendResponse.success = true;
                shareFriendResponse.data = response;

            }
            catch (Exception ex)
            {
                shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                shareFriendResponse.error_info.extra_info = Common.Common.InternalServerError;
                shareFriendResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "ShareWithFriend:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(shareFriendResponse);
        }

        [Route("sendsurveyinvite")]
        [HttpPost]
        public async Task<IActionResult> SurveyInvite([FromBody]RSVPInviteRequest reqmodel)
        {
            var shareFriendResponse = new BaseResponse();

            if (reqmodel.reservation_id <= 0 && reqmodel.member_id <= 0)
            {
                shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                shareFriendResponse.error_info.extra_info = "Either Reservation Id or memberId is required.";
                shareFriendResponse.error_info.description = "Either Reservation Id or memberId is required.";
                return new ObjectResult(shareFriendResponse);
            }
            else if (string.IsNullOrWhiteSpace(reqmodel.invite_email))
            {
                shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                shareFriendResponse.error_info.extra_info = "No email address found in request. cannot send email";
                shareFriendResponse.error_info.description = "No email address found in request. cannot send email";
                return new ObjectResult(shareFriendResponse);
            }
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                var surveyWaiverStatusmodel = eventDAL.GetSurveyWaiverStatusByEmailAndMemberId(reqmodel.member_id, reqmodel.invite_email, reqmodel.guest_id);

                bool sendInvite = false;
                if (reqmodel.invite_type == Common.Common.InviteType.Survey && (surveyWaiverStatusmodel.survey_status == Common.Common.RSVPPostCaptureStatus.Available || surveyWaiverStatusmodel.survey_status == Common.Common.RSVPPostCaptureStatus.Invited || surveyWaiverStatusmodel.survey_status == Common.Common.RSVPPostCaptureStatus.Expired))
                {
                    sendInvite = true;
                }
                else if (reqmodel.invite_type == Common.Common.InviteType.Waiver && (surveyWaiverStatusmodel.waiver_status == Common.Common.RSVPPostCaptureStatus.Available || surveyWaiverStatusmodel.waiver_status == Common.Common.RSVPPostCaptureStatus.Invited || surveyWaiverStatusmodel.waiver_status == Common.Common.RSVPPostCaptureStatus.Expired))
                {
                    sendInvite = true;
                }

                if (sendInvite == false)
                {
                    string errormsg = "Survey Invitation already sent.";
                    if (reqmodel.invite_type == Common.Common.InviteType.Waiver)
                        errormsg = "Waiver Invitation already sent.";

                    shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.AlreadyInvited;
                    shareFriendResponse.error_info.extra_info = errormsg;
                    shareFriendResponse.error_info.description = errormsg;
                    return new ObjectResult(shareFriendResponse);
                }

                MailConfig config = new MailConfig
                {
                    Domain = _appSetting.Value.MailGunPostUrl,
                    ApiKey = _appSetting.Value.MainGunApiKey
                };
                ReservationEmailModel model = new ReservationEmailModel
                {
                    RsvpId = reqmodel.reservation_id,
                    UId = reqmodel.guest_id,
                    InviteEmail = reqmodel.invite_email,
                    data = new ReservationEmailModel
                    {
                        RsvpId = reqmodel.reservation_id,
                        BCode = reqmodel.user_id.ToString(),
                        UId = reqmodel.guest_id,
                        GuestEmail = true,
                        InviteEmail = reqmodel.invite_email,
                        isRsvpType = (int)reqmodel.invite_type
                    }
                };
                model.MailConfig = config;
                AuthMessageSender messageService = new AuthMessageSender();
                var response = await messageService.SendWaverInviteEmail(model);
                shareFriendResponse.success = true;

                eventDAL.SaveRsvpSurveyWaiver(reqmodel.member_id, reqmodel.reservation_id, reqmodel.guest_id, reqmodel.user_id, reqmodel.invite_email, (int)reqmodel.invite_type);
            }
            catch (Exception ex)
            {
                shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                shareFriendResponse.error_info.extra_info = Common.Common.InternalServerError;
                shareFriendResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SurveyInvite:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);
            }
            return new ObjectResult(shareFriendResponse);
        }

        [Route("updatesurveyinvitestatus")]
        [HttpPost]
        public async Task<IActionResult> UpdateInviteStatus([FromBody]RSVPInviteRequest reqmodel)
        {
            var shareFriendResponse = new BaseResponse();

            if (reqmodel.reservation_id <= 0 && reqmodel.member_id <= 0)
            {
                shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                shareFriendResponse.error_info.extra_info = "Either Reservation Id or memberId is required.";
                shareFriendResponse.error_info.description = "Either Reservation Id or memberId is required.";
                return new ObjectResult(shareFriendResponse);
            }
            else if (string.IsNullOrWhiteSpace(reqmodel.invite_email))
            {
                shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                shareFriendResponse.error_info.extra_info = "No email address found in request. cannot send email";
                shareFriendResponse.error_info.description = "No email address found in request. cannot send email";
                return new ObjectResult(shareFriendResponse);
            }
            try

            {

                shareFriendResponse.success = true;

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                shareFriendResponse.success = eventDAL.UpdateRsvpSurveyWaiverStatus(reqmodel.member_id, reqmodel.reservation_id, reqmodel.invite_email, (int)reqmodel.invite_type, reqmodel.invite_status);
            }
            catch (Exception ex)
            {
                shareFriendResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                shareFriendResponse.error_info.extra_info = Common.Common.InternalServerError;
                shareFriendResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateInviteStatus:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, reqmodel.member_id);
            }
            return new ObjectResult(shareFriendResponse);
        }

        //[Route("ejgallotest")]
        //[HttpPost]
        //public async Task<IActionResult> TestEjGallo([FromBody] ViewModels.UserDetail user_detail)
        //{
        //    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
        //    SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
        //    var customSetting = settingsDAL.GetCustomSettingByMember(Common.Common.SettingType.EJGalloAPI, 26);

        //    if (customSetting != null)
        //    {
        //        try
        //        {
        //            EJGallo eJ = new EJGallo(_appSetting);
        //            bool isSuccess = await eJ.NewsletterSignup(26, user_detail, customSetting);
        //        }
        //        catch (Exception ex)
        //        {
        //            logDAL.InsertLog("WebApi", "SaveReservation- EJGallo:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"]);

        //        }
        //    }
        //    return new ObjectResult(new BaseResponse { success=true});
        //}

        private async Task<String> getRawPostData()
        {
            string content = "";
            using (StreamReader reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8))
            {
                if (reader.EndOfStream)
                {
                    reader.BaseStream.Position = 0;
                }
                content = await reader.ReadToEndAsync();
            }
            return content;
        }

        [Route("savereservation")]
        [HttpPost]
        public async Task<IActionResult> SaveReservation([FromBody]CreateReservationRequest model)
        {
            var createReservationResponse = new CreateReservationResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
            try
            {
                if (model == null)
                {
                    //string rawPostData = await getRawPostData();
                    //string debugData = "";
                    //if (!string.IsNullOrWhiteSpace(rawPostData))
                    //    debugData = " Raw Data:" + rawPostData;

                    //logDAL.InsertLog("WebApi", "SaveReservation Error:  No data or invalid data passed in request." + debugData, HttpContext.Request.Headers["AuthenticateKey"], 3, model.member_id);
                    createReservationResponse.success = false;
                    createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationSavingError;
                    createReservationResponse.error_info.extra_info = "No data or invalid data passed in request. ";
                    createReservationResponse.error_info.description = "There was problem processing your request. Kindly retry again or contact support.";
                    return new ObjectResult(createReservationResponse);
                }

                //if (_appSetting != null && _appSetting.Value != null && _appSetting.Value.QueueName.Contains("dev"))
                //{
                if (model.status != 8)
                    logDAL.InsertLog("WebApi", "SaveReservation:  request data:" + JsonConvert.SerializeObject(model), HttpContext.Request.Headers["AuthenticateKey"], 3, model.member_id);
                //}

                bool IsModify = model.reservation_id > 0;
                bool IsRescheduled = false;
                int actionType = 0;

                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));

                CreateReservationModel createReservationModel = new CreateReservationModel();

                var reservationDetailModel = new ReservationDetailModel();
                if (IsModify)
                {
                    reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(model.reservation_id, "", IsAdmin);
                    if (!(reservationDetailModel != null && reservationDetailModel.reservation_id > 0))
                        IsModify = false;
                }

                //Fix for wrong eventId in some reservations: Get the eventId from DB to ensure it is correct one and get it only for regular booking and not private reservations
                if (model.slot_id > 0)
                {
                    EventRuleModel eventRuleModel = eventDAL.GetEventcapacity(model.slot_id, model.slot_type);
                    model.event_id = eventRuleModel.EventId;
                    model.location_id = eventRuleModel.LocationId;
                    model.member_id = eventRuleModel.WineryID;
                }

                createReservationModel.EventId = model.event_id ?? 0;

                EventModel eventModel = eventDAL.GetEventById(createReservationModel.EventId,model.member_id);

                //if (model.slot_id > 0)
                //{
                //    model.member_id = eventModel.MemberID;
                //}

                //decimal amount_paid = reservationDetailModel.amount_paid;
                createReservationModel.TotalGuests = model.total_guests;
                createReservationModel.FeeType = model.fee_type;
                createReservationModel.ReferralType = model.referral_type;
                createReservationModel.PreAssign_Server_Id = model.pre_assign_server_id ?? 0;

                if (IsModify)
                {
                    if (model.pre_assign_table_ids != null && model.pre_assign_table_ids.Count > 0)
                        createReservationModel.PreAssign_Table_Id = JsonConvert.SerializeObject(model.pre_assign_table_ids);
                    else
                        createReservationModel.PreAssign_Table_Id = JsonConvert.SerializeObject(reservationDetailModel.pre_assign_table_ids);
                }
                else
                {
                    createReservationModel.PreAssign_Table_Id = JsonConvert.SerializeObject(model.pre_assign_table_ids);
                }

                createReservationModel.FloorPlanId = model.floor_plan_id;

                createReservationModel.CancelLeadTime = model.cancel_lead_time ?? 0;
                createReservationModel.AttendeeQuestions = model.attendee_questions;

                if (IsModify)
                {
                    model.referral_type = reservationDetailModel.referral_type;
                    createReservationModel.ReferralType = model.referral_type;

                    if (model.charge_fee == 14)
                    {
                        createReservationModel.ChargeFee = reservationDetailModel.charge_fee;
                        model.charge_fee = reservationDetailModel.charge_fee;
                    }

                    createReservationModel.BookingCode = reservationDetailModel.booking_code;
                    actionType = 2;
                    if (reservationDetailModel.event_start_date != Convert.ToDateTime(model.event_start_date) || reservationDetailModel.event_end_date != Convert.ToDateTime(model.event_end_date))
                    {
                        actionType = 1;
                    }
                }
                else
                    createReservationModel.BookingCode = StringHelpers.GenerateRandomString(8, false);

                createReservationModel.MobilePhoneStatus = (int)MobileNumberStatus.unverified;

                createReservationModel.SlotId = model.slot_id;
                createReservationModel.SlotType = model.slot_type;

                if (createReservationModel.SlotType == 1)
                {
                    if (eventDAL.EventExceptions_Check(createReservationModel.SlotId) == false)
                    {
                        createReservationModel.SlotType = 0;
                        createReservationModel.SlotId = eventDAL.GetEventRuleIdbyEventExceptionID(createReservationModel.SlotId);
                    }
                }

                createReservationModel.Email = model.user_detail.email;
                createReservationModel.Country = model.user_detail.address.country;
                createReservationModel.Zip = model.user_detail.address.zip_code;
                createReservationModel.Address1 = model.user_detail.address.address_1;
                createReservationModel.Address2 = model.user_detail.address.address_2;
                createReservationModel.CustomerType = model.user_detail.customer_type;
                createReservationModel.UserId = 0;
                createReservationModel.FirstName = model.user_detail.first_name;
                createReservationModel.LastName = model.user_detail.last_name;
                createReservationModel.PhoneNumber = Utility.FormatTelephoneNumber(model.user_detail.phone_number, model.user_detail.address.country);  //model.user_detail.phone_number;
                createReservationModel.City = model.user_detail.address.city;
                createReservationModel.State = model.user_detail.address.state;
                createReservationModel.WineryId = model.member_id;
                createReservationModel.Tags = model.tags;
                createReservationModel.WaitListGuid = model.waitlist_guid;

                if (model.transportation_id.HasValue)
                {
                    int transportId = 0;
                    int.TryParse(model.transportation_id.ToString(), out transportId);
                    createReservationModel.TransportationId = transportId;
                    createReservationModel.TransportationName = model.transportation_name;
                }
                else
                {
                    createReservationModel.TransportationId = 0;
                    createReservationModel.TransportationName = string.Empty;
                }

                bool existingUser = false;
                if (string.IsNullOrEmpty(createReservationModel.Email))
                {
                    createReservationModel.Email = Utility.GenerateUsername(createReservationModel.FirstName, createReservationModel.LastName);
                }
                else
                {
                    var userDetailModel = new UserDetailModel();
                    userDetailModel = userDAL.GetUserDetailsbyemail(createReservationModel.Email, 0);

                    if (userDetailModel.user_id > 0)
                    {
                        if (string.IsNullOrEmpty(createReservationModel.FirstName))
                            createReservationModel.FirstName = userDetailModel.first_name;

                        if (string.IsNullOrEmpty(createReservationModel.LastName))
                            createReservationModel.LastName = userDetailModel.last_name;

                        if (string.IsNullOrEmpty(createReservationModel.City))
                            createReservationModel.City = userDetailModel.address.city;

                        if (string.IsNullOrEmpty(createReservationModel.State))
                            createReservationModel.State = userDetailModel.address.state;

                        if (string.IsNullOrEmpty(createReservationModel.PhoneNumber))
                        {
                            createReservationModel.PhoneNumber = Utility.FormatTelephoneNumber(userDetailModel.phone_number, userDetailModel.address.country);  //userDetailModel.phone_number;
                        }

                        if (string.IsNullOrEmpty(createReservationModel.Country))
                            createReservationModel.Country = userDetailModel.address.country;

                        if (string.IsNullOrEmpty(createReservationModel.Zip))
                            createReservationModel.Zip = userDetailModel.address.zip_code;

                        if (string.IsNullOrEmpty(createReservationModel.Address1))
                        {
                            createReservationModel.Address1 = userDetailModel.address.address_1;

                            if (string.IsNullOrEmpty(createReservationModel.Address2))
                                createReservationModel.Address2 = userDetailModel.address.address_2;
                        }
                            
                        createReservationModel.UserId = userDetailModel.user_id;

                        existingUser = true;
                        //mobilephonestatus check
                    }
                }

                if (string.IsNullOrEmpty(createReservationModel.FirstName) || string.IsNullOrEmpty(createReservationModel.LastName) || string.IsNullOrEmpty(createReservationModel.PhoneNumber))
                {
                    createReservationResponse.success = false;
                    createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Guest;
                    createReservationResponse.error_info.extra_info = "Guest Error";
                    createReservationResponse.error_info.description = "Please fill out all required guest fields.";
                    return new ObjectResult(createReservationResponse);
                }

                createReservationModel.AccessCode = model.access_code; //assign access code
                //check if access code valid for new reservation
                if (!IsModify && !string.IsNullOrWhiteSpace(model.access_code))
                {
                    var eventAccess = eventDAL.CheckEventAccessCode(model.member_id, 0, model.access_code, createReservationModel.UserId, model.slot_id, model.slot_type);

                    if (eventAccess == null || !eventAccess.IsValid)
                    {
                        createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                        createReservationResponse.error_info.extra_info = "Invalid Acess Code";
                        if (createReservationModel.UserId > 0 && eventAccess.StartDate.HasValue)
                        {
                            createReservationResponse.error_info.description = "Access code has reached it's limit for this user and cannot be used.";
                        }
                        else
                        {
                            createReservationResponse.error_info.description = "Invalid access code. Acess code does not exist.";
                        }

                        return new ObjectResult(createReservationResponse);
                    }
                    else if (eventAccess.IsValid && eventAccess.StartDate == null)
                    {
                        createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidAccessCode;
                        createReservationResponse.error_info.extra_info = "Invalid Acess Code";
                        createReservationResponse.error_info.description = string.Format("The '{0}' access code is no longer available", model.access_code);
                        return new ObjectResult(createReservationResponse);
                    }
                }

                if (!string.IsNullOrEmpty(createReservationModel.WaitListGuid))
                {
                    //check if waitlist is valid
                    var waitList = eventDAL.GetReservationV2WaitlistbyId(createReservationModel.WaitListGuid, "", 0);

                    if (waitList != null)
                    {
                        if (waitList.waitlist_status == Common.Common.Waitlist_Status.converted || waitList.waitlist_status == Common.Common.Waitlist_Status.expired || waitList.waitlist_status == Common.Common.Waitlist_Status.pending
                            || waitList.waitlist_status == Common.Common.Waitlist_Status.canceled
                            || (createReservationModel.UserId > 0 && createReservationModel.UserId != waitList.user_id)
                            || (waitList.status_date_time.AddMinutes(waitList.valid_minutes) < DateTime.UtcNow)
                            || (waitList.slot_id != createReservationModel.SlotId)
                            || (waitList.slot_type != createReservationModel.SlotType)
                            )
                        {
                            createReservationResponse.success = false;
                            createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.WaitListExists;
                            createReservationResponse.error_info.extra_info = "Waitlist Invalid";
                            createReservationResponse.error_info.description = "We're sorry, but the wait list is no longer available.";

                            var error_data = new ViewModels.ErrorData();

                            error_data.event_name = waitList.event_name;
                            error_data.location_name = waitList.event_location;
                            error_data.member_url = waitList.member_url;

                            createReservationResponse.error_info.error_data = error_data;

                            return new ObjectResult(createReservationResponse);
                        }

                    }
                    else
                    {
                        createReservationResponse.success = false;
                        createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.WaitListExists;
                        createReservationResponse.error_info.extra_info = "Waitlist Invalid";
                        createReservationResponse.error_info.description = "Invalid Waitlist Id.";
                        return new ObjectResult(createReservationResponse);
                    }

                }

                createReservationModel.ChargeFee = model.charge_fee;

                if (model.pay_card == null)
                {
                    model.pay_card = new PayCard();
                }

                if (!string.IsNullOrEmpty(model.pay_card.card_last_four_digits))
                    createReservationModel.PayCardLastFourDigits = model.pay_card.card_last_four_digits;

                if (!string.IsNullOrEmpty(model.pay_card.card_first_four_digits))
                    createReservationModel.PayCardFirstFourDigits = model.pay_card.card_first_four_digits;

                if (string.IsNullOrEmpty(model.pay_card.card_type))
                    createReservationModel.PayCardType = Services.Payments.GetCardType(model.pay_card.number);
                else
                    createReservationModel.PayCardType = model.pay_card.card_type;

                createReservationModel.PayCardNumber = StringHelpers.Encryption(Common.Common.Right(model.pay_card.number, 4));
                createReservationModel.PayCardCustName = model.pay_card.cust_name;
                createReservationModel.PayCardExpYear = model.pay_card.exp_year;
                createReservationModel.PayCardExpMonth = model.pay_card.exp_month;
                createReservationModel.PayCardToken = model.pay_card.card_token;
                if (string.IsNullOrWhiteSpace(createReservationModel.PayCardToken) && IsModify && reservationDetailModel.pay_card != null)
                    createReservationModel.PayCardToken = reservationDetailModel.pay_card.card_token;

                var paymentConfig = eventDAL.GetPaymentConfigByWineryId(model.member_id);
                Services.Payments objPayments = new Services.Payments(_appSetting);

                Model.PaymentConfigModel secondaryPaymentConfig = new Model.PaymentConfigModel();

                //if (string.IsNullOrWhiteSpace(createReservationModel.PayCardToken))
                //{
                if (!string.IsNullOrEmpty(model.pay_card.number) && !string.IsNullOrEmpty(createReservationModel.PayCardCustName) && !string.IsNullOrEmpty(createReservationModel.PayCardExpYear) && !string.IsNullOrEmpty(createReservationModel.PayCardExpMonth) && model.pay_card.number.Length > 4)
                    {
                        if (paymentConfig != null && paymentConfig.PaymentGateway != Configuration.Gateway.Offline)
                        {
                            TokenizedCardRequest request = new TokenizedCardRequest();

                            request.card_type = createReservationModel.PayCardType;
                            request.cust_name = createReservationModel.PayCardCustName;
                            request.exp_month = createReservationModel.PayCardExpMonth;
                            request.exp_year = createReservationModel.PayCardExpYear;
                            request.member_id = model.member_id;
                            request.number = model.pay_card.number.Replace(" ","");
                            request.card_last_four_digits = createReservationModel.PayCardNumber;
                            request.card_first_four_digits = createReservationModel.PayCardNumber;
                            request.cvv2 = model.pay_card.cvv2;

                            if (string.IsNullOrWhiteSpace(request.cvv2) && paymentConfig.PaymentGateway == Configuration.Gateway.Commrece7Payments)
                            {
                                createReservationResponse.success = false;
                                createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                                createReservationResponse.error_info.extra_info = "Invalid credit card information. Card CVV2 is required";
                                createReservationResponse.error_info.description = "Invalid credit card information. Card CVV2 is required";
                                return new ObjectResult(createReservationResponse);
                            }

                            request.user_info = new UserDetailViewModel
                            {
                                first_name = createReservationModel.FirstName,
                                last_name = createReservationModel.LastName,
                                email = createReservationModel.Email + "",
                                phone_number = createReservationModel.PhoneNumber + ""
                            };
                            request.user_info.address = new ViewModels.UserAddress
                            {
                                zip_code = createReservationModel.Zip,
                                address_1 = createReservationModel.Address1 + "",
                                address_2 = createReservationModel.Address2 + "",
                                city = createReservationModel.City + "",
                                state = createReservationModel.State + "",
                                country = createReservationModel.Country + ""
                            };

                            request.ignore_avs_error = (model.charge_fee == 4);
                            TokenizedCard card = Services.Payments.TokenziedCard(request, paymentConfig);

                        int setting_type = 2;
                        secondaryPaymentConfig = eventDAL.GetPaymentConfigByWineryId(model.member_id, setting_type);
                        TokenizedCard secondarycard = null;

                        if (secondaryPaymentConfig != null && secondaryPaymentConfig.PaymentGateway != Configuration.Gateway.Offline)
                        {
                            secondarycard = Services.Payments.TokenziedCard(request, secondaryPaymentConfig);
                        }

                        if (card != null && !string.IsNullOrEmpty(card.card_token))
                        {
                            model.pay_card.card_token = card.card_token;
                            model.pay_card.card_first_four_digits = card.first_four_digits;
                            model.pay_card.card_last_four_digits = card.last_four_digits;
                            createReservationModel.PayCardFirstFourDigits = card.first_four_digits;
                            createReservationModel.PayCardLastFourDigits = card.last_four_digits;
                            createReservationModel.PayCardToken = card.card_token;

                            var winery = new Model.WineryModel();
                            winery = eventDAL.GetWineryById(request.member_id);

                            if ((winery.EnableVin65 || winery.EnableClubVin65) && !string.IsNullOrEmpty(card.card_token) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(winery.SALT) && !string.IsNullOrEmpty(winery.DecryptKey))
                            {
                                string cardtype2 = Services.Payments.GetCardType(request.number, "vin65");
                                string cardnumber = Common.StringHelpers.EncryptedCardNumber(request.number, winery.SALT, winery.DecryptKey);
                                eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, request.member_id, winery.Vin65UserName, winery.Vin65Password, request.cvv2);
                            }
                            else if ((winery.EnableCommerce7 || winery.EnableClubCommerce7) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(card.card_token))
                            {
                                string cardtype2 = Services.Payments.GetCardType(request.number, "commerce7");
                                string cardnumber = Common.Common.Right(request.number, 4).PadLeft(request.number.Length, '*');

                                string gateway = "No Gateway";
                                gateway = Utility.GetCommerce7PaymentGatewayName(paymentConfig.PaymentGateway);

                                eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, request.member_id, gateway, "", request.cvv2);
                            }

                            if ((winery.EnableVin65 || winery.EnableClubVin65) && !string.IsNullOrEmpty(card.card_token) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(winery.SALT) && !string.IsNullOrEmpty(winery.DecryptKey))
                            {
                                string cardtype2 = Services.Payments.GetCardType(request.number, "vin65");
                                string cardnumber = Common.StringHelpers.EncryptedCardNumber(request.number, winery.SALT, winery.DecryptKey);
                                string cardToken = card.card_token;
                                eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, cardToken, model.member_id, winery.Vin65UserName, winery.Vin65Password, request.cvv2);

                                if (secondarycard != null && !string.IsNullOrWhiteSpace(secondarycard.card_token))
                                {
                                    cardToken = secondarycard.card_token;
                                    eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, cardToken, model.member_id, winery.Vin65UserName, winery.Vin65Password, request.cvv2);

                                    if (request.user_info == null)
                                    {
                                        request.user_info = new UserDetailViewModel();

                                        request.user_info.email = "";
                                        request.user_info.first_name = "";
                                        request.user_info.last_name = "";
                                        request.user_id = 0;
                                    }

                                    if (card != null && !string.IsNullOrEmpty(card.card_token) && request.user_info != null)
                                        eventDAL.InsertCreditCardDetail(Services.Payments.GetCardType(request.number), request.cust_name, request.exp_month, request.exp_year, cardToken, model.member_id, Common.Common.Right(request.number, 4), Common.Common.Left(request.number, 4), (int)request.source_module, request.user_info.email, request.user_info.first_name, request.user_info.last_name, request.user_id, (int)secondaryPaymentConfig.PaymentGateway);
                                }

                            }
                            else if ((winery.EnableCommerce7 || winery.EnableClubCommerce7) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(card.card_token))
                            {
                                string cardtype2 = Services.Payments.GetCardType(request.number, "commerce7");
                                string cardnumber = Common.Common.Right(request.number, 4).PadLeft(request.number.Length, '*');

                                string gateway = "No Gateway";
                                string cardToken = card.card_token;
                                var paymentGateway = paymentConfig.PaymentGateway;
                                gateway = Utility.GetCommerce7PaymentGatewayName(paymentGateway);

                                eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, cardToken, model.member_id, gateway, "", request.cvv2);

                                if (secondarycard != null && !string.IsNullOrWhiteSpace(secondarycard.card_token))
                                {
                                    cardToken = secondarycard.card_token;
                                    paymentGateway = secondaryPaymentConfig.PaymentGateway;
                                    gateway = Utility.GetCommerce7PaymentGatewayName(paymentGateway);
                                    eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, cardToken, model.member_id, gateway, "", request.cvv2);

                                    if (request.user_info == null)
                                    {
                                        request.user_info = new UserDetailViewModel();

                                        request.user_info.email = "";
                                        request.user_info.first_name = "";
                                        request.user_info.last_name = "";
                                        request.user_id = 0;
                                    }

                                    if (card != null && !string.IsNullOrEmpty(card.card_token) && request.user_info != null)
                                        eventDAL.InsertCreditCardDetail(Services.Payments.GetCardType(request.number), request.cust_name, request.exp_month, request.exp_year, cardToken, model.member_id, Common.Common.Right(request.number, 4), Common.Common.Left(request.number, 4), (int)request.source_module, request.user_info.email, request.user_info.first_name, request.user_info.last_name, request.user_id, (int)secondaryPaymentConfig.PaymentGateway);
                                }
                            }
                        }
                        else if (card != null && !string.IsNullOrWhiteSpace(card.ErrorMessage) && string.IsNullOrWhiteSpace(card.card_token))
                        {
                            createReservationResponse.success = false;
                            createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                            createReservationResponse.error_info.extra_info = card.ErrorMessage;
                            createReservationResponse.error_info.description = card.ErrorMessage;
                            return new ObjectResult(createReservationResponse);
                        }

                    }
                }
                //}



                decimal OldrsvpTotal = 0;
                if (IsModify)
                {
                    if (reservationDetailModel.fee_type == 1)
                        OldrsvpTotal = (reservationDetailModel.fee_per_person) - reservationDetailModel.discount_amount;
                    else
                        OldrsvpTotal = (reservationDetailModel.fee_per_person * reservationDetailModel.total_guests) - reservationDetailModel.discount_amount;

                    if (reservationDetailModel.status == 8 && model.status == 0)
                        createReservationModel.Status = 0;
                    else
                    {
                        if (Convert.ToDateTime(model.event_end_date) != reservationDetailModel.event_end_date || Convert.ToDateTime(model.event_start_date) != reservationDetailModel.event_start_date || model.slot_id != reservationDetailModel.slot_id)
                        {
                            IsRescheduled = true;
                            createReservationModel.Status = (int)ReservationStatus.Rescheduled;
                        }
                        else
                            createReservationModel.Status = reservationDetailModel.status;
                    }
                   

                    int CardNumber = 0;
                    bool isNum = int.TryParse((model.pay_card.number + "").Trim(), out CardNumber);

                    if (!string.IsNullOrWhiteSpace(createReservationModel.PayCardToken))
                    {

                        string pay_cardnumber = model.pay_card.number + "";
                        if (isNum == false)
                        {
                            pay_cardnumber = Common.Common.Right(pay_cardnumber, 4);
                        }

                        if (pay_cardnumber.Trim().Length == 4 && Common.Common.Right(StringHelpers.Decryption(reservationDetailModel.pay_card.number), 4) == pay_cardnumber)
                        {
                            model.pay_card.number = StringHelpers.Decryption(reservationDetailModel.pay_card.number);
                            model.pay_card.cust_name = reservationDetailModel.pay_card.cust_name;
                            model.pay_card.exp_month = reservationDetailModel.pay_card.exp_month;
                            model.pay_card.exp_year = reservationDetailModel.pay_card.exp_year;
                            createReservationModel.PayCardNumber = reservationDetailModel.pay_card.number;
                            createReservationModel.PayCardExpMonth = reservationDetailModel.pay_card.exp_month;
                            createReservationModel.PayCardExpYear = reservationDetailModel.pay_card.exp_year;
                            createReservationModel.PayCardCustName = reservationDetailModel.pay_card.cust_name;
                            createReservationModel.PayCardType = reservationDetailModel.pay_card.card_type;
                        }
                    }
                    else
                    {
                        model.pay_card.number = String.Empty;
                        model.pay_card.cust_name = String.Empty;
                        model.pay_card.exp_month = String.Empty;
                        model.pay_card.exp_year = String.Empty;
                        createReservationModel.PayCardNumber = String.Empty;
                        createReservationModel.PayCardExpMonth = String.Empty;
                        createReservationModel.PayCardExpYear = String.Empty;
                        createReservationModel.PayCardCustName = String.Empty;
                        createReservationModel.PayCardType = String.Empty;
                    }

                }


                int regionMostVisited = 0;
                try
                {
                    regionMostVisited = model.user_detail.region_most_visited;
                }
                catch { }
                if (existingUser == false)
                {
                    string GuestPwd = StringHelpers.GenerateRandomString(8, false);
                    int mobilePhoneStatus = (int)Utility.SMSVerified_System(createReservationModel.PhoneNumber);

                    createReservationModel.MobilePhoneStatus = mobilePhoneStatus;
                    createReservationModel.UserId = userDAL.CreateUser(createReservationModel.Email, GuestPwd, createReservationModel.FirstName, createReservationModel.LastName, createReservationModel.Country, createReservationModel.Zip, createReservationModel.PhoneNumber, createReservationModel.CustomerType, createReservationModel.MobilePhoneStatus, regionMostVisited, model.cust_id, createReservationModel.City, createReservationModel.State, createReservationModel.Address1, createReservationModel.Address2, Common.Common.GetSource(HttpContext.Request.Headers["AuthenticateKey"]));

                    userDAL.UpdateUserWinery(createReservationModel.UserId, createReservationModel.WineryId, 4, "", "", "", createReservationModel.CustomerType);
                }
                else if (model.status != 8)
                {
                    if (!string.IsNullOrWhiteSpace(model.cust_id))
                    {
                        userDAL.UpdateGatewayCustId(createReservationModel.UserId, model.cust_id);
                    }

                    if (regionMostVisited > 0)
                    {
                        userDAL.UpdateFavoriteRegion(createReservationModel.UserId, regionMostVisited);
                    }
                    int mobilePhoneStatus = (int)Utility.SMSVerified_System(createReservationModel.PhoneNumber);
                    userDAL.UpdateUserSMSVerifiedbyId(createReservationModel.UserId, mobilePhoneStatus, createReservationModel.PhoneNumber);
                    createReservationModel.MobilePhoneStatus = mobilePhoneStatus;
                    //if ((userDAL.UpdateUserWinery(createReservationModel.WineryId, createReservationModel.UserId, "", "", createReservationModel.CustomerType)) == 0)
                    //{
                    userDAL.UpdateUserWinery(createReservationModel.UserId, createReservationModel.WineryId, 4, "", "", "", createReservationModel.CustomerType);
                    //}
                }


                if (model.affiliate_id > 0 && model.status != 8)
                {
                    userDAL.UpdateUserWinery(model.affiliate_id, createReservationModel.WineryId, 6);
                }

                model.user_detail.first_name = createReservationModel.FirstName;
                model.user_detail.last_name = createReservationModel.LastName;
                model.user_detail.phone_number = createReservationModel.PhoneNumber;
                model.user_detail.email = createReservationModel.Email;
                model.user_detail.address.zip_code = createReservationModel.Zip;
                model.user_detail.address.address_1 = createReservationModel.Address1;
                model.user_detail.address.address_2 = createReservationModel.Address2;

                if (createReservationModel.UserId == 0)
                {
                    createReservationResponse.success = false;
                    createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.CreatingUser;
                    createReservationResponse.error_info.extra_info = "Error Creating User";
                    createReservationResponse.error_info.description = "The user could not be loaded or created. Please try again or contact CellarPass.";
                    return new ObjectResult(createReservationResponse);
                }

                createReservationModel.EventDate = Convert.ToDateTime(model.event_start_date).Date;
                createReservationModel.StartTime = new TimeSpan(Convert.ToDateTime(model.event_start_date).TimeOfDay.Ticks);
                createReservationModel.EndTime = new TimeSpan(Convert.ToDateTime(model.event_end_date).TimeOfDay.Ticks);

                //EventModel eventModel = eventDAL.GetEventById(createReservationModel.EventId);

                if (existingUser)
                {
                    bool SkipConflictCheck = model.force_rsvp;

                    //if (SkipConflictCheck == false)
                    //    SkipConflictCheck = IsAdmin;

                    var reservationConflictCheck = new ReservationDetailModel();
                    reservationConflictCheck = eventDAL.IsReservationConflict(model.reservation_id, createReservationModel.UserId, createReservationModel.EventDate, createReservationModel.StartTime, createReservationModel.EndTime, createReservationModel.SlotId, createReservationModel.SlotType);
                    if (reservationConflictCheck != null && reservationConflictCheck.reservation_id > 0)
                    {
                        if (reservationConflictCheck.event_start_date.TimeOfDay == createReservationModel.StartTime && reservationConflictCheck.event_end_date.TimeOfDay == createReservationModel.EndTime)
                        {
                            createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                            createReservationResponse.error_info.extra_info = "Reservation Conflict Error";
                            //createReservationResponse.error_info.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd yyyy"));
                            createReservationResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} on {4} from {1} to {2}.<br>Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));

                            var error_data = new ViewModels.ErrorData();

                            error_data.event_name = reservationConflictCheck.event_name;
                            error_data.location_name = reservationConflictCheck.location_name;
                            error_data.start_date = reservationConflictCheck.event_start_date;
                            error_data.end_date = reservationConflictCheck.event_end_date;
                            error_data.member_url = eventModel.member_url;

                            createReservationResponse.error_info.error_data = error_data;
                            return new ObjectResult(createReservationResponse);
                        }
                        else if (reservationConflictCheck.member_id != createReservationModel.WineryId && SkipConflictCheck == false)
                        {
                            if (reservationConflictCheck.event_start_date.TimeOfDay != createReservationModel.EndTime || reservationConflictCheck.event_end_date.TimeOfDay != createReservationModel.StartTime)
                            {
                                createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                                createReservationResponse.error_info.extra_info = "Reservation Conflict Error";
                                createReservationResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} on {4} from {1} to {2}.<br>Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                //createReservationResponse.error_info.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd yyyy"));
                                var error_data = new ViewModels.ErrorData();

                                error_data.event_name = reservationConflictCheck.event_name;
                                error_data.location_name = reservationConflictCheck.location_name;
                                error_data.start_date = reservationConflictCheck.event_start_date;
                                error_data.end_date = reservationConflictCheck.event_end_date;
                                error_data.member_url = eventModel.member_url;

                                createReservationResponse.error_info.error_data = error_data;
                                return new ObjectResult(createReservationResponse);
                            }
                        }
                        else if (SkipConflictCheck == false)
                        {
                            if (reservationConflictCheck.event_start_date.TimeOfDay == createReservationModel.EndTime || reservationConflictCheck.event_end_date.TimeOfDay == createReservationModel.StartTime)
                            {
                                createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.RsvpBackToBack;
                                createReservationResponse.error_info.extra_info = "Reservation Conflict Error";
                                //createReservationResponse.error_info.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd yyyy"));
                                createReservationResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} on {4} from {1} to {2}.<br>Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                var error_data = new ViewModels.ErrorData();

                                error_data.event_name = reservationConflictCheck.event_name;
                                error_data.location_name = reservationConflictCheck.location_name;
                                error_data.start_date = reservationConflictCheck.event_start_date;
                                error_data.end_date = reservationConflictCheck.event_end_date;
                                error_data.member_url = reservationConflictCheck.member_url;

                                createReservationResponse.error_info.error_data = error_data;
                                return new ObjectResult(createReservationResponse);
                            }
                            else if (reservationConflictCheck.member_id == createReservationModel.WineryId)
                            {
                                createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                                createReservationResponse.error_info.extra_info = "Reservation Conflict Error";
                                //createReservationResponse.error_info.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd yyyy"));
                                createReservationResponse.error_info.description = string.Format("This guest is already booked for an adjacent reservation at {0} on {4} from {1} to {2}.<br>Please select a different date/time to continue.", reservationConflictCheck.location_name, reservationConflictCheck.event_start_date.ToString("hh:mm tt"), reservationConflictCheck.event_end_date.ToString("hh:mm tt"), reservationConflictCheck.event_name, reservationConflictCheck.event_start_date.ToString("MMMM dd, yyyy"));
                                var error_data = new ViewModels.ErrorData();

                                error_data.event_name = reservationConflictCheck.event_name;
                                error_data.location_name = reservationConflictCheck.location_name;
                                error_data.start_date = reservationConflictCheck.event_start_date;
                                error_data.end_date = reservationConflictCheck.event_end_date;
                                error_data.member_url = eventModel.member_url;

                                createReservationResponse.error_info.error_data = error_data;
                                return new ObjectResult(createReservationResponse);
                            }
                        }
                    }
                }

                bool CheckTableInventory = false;
                int invMode = userDAL.GetInventoryModeForMember(createReservationModel.WineryId);
                bool LoadDiscount = false;

                if (invMode == 1)
                    CheckTableInventory = true;

                Discount.EventDiscountResult result = new Discount.EventDiscountResult();
                if ((eventModel.member_benefit_required || eventModel.account_type_required) && model.user_detail.customer_type != 1 && model.ignore_club_member == false)
                {
                    result = await Discount.CheckAndApplyEventDiscount(eventModel, model.total_guests, model.fee_per_person, model.discount_code, model.member_id, model.user_detail.email, model.activation_codes, Convert.ToDateTime(model.event_start_date), model.fee_type, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);
                    LoadDiscount = true;
                    if (result.ClubMember == false)
                    {
                        createReservationResponse.success = false;
                        createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidClubMember;
                        createReservationResponse.error_info.extra_info = string.Empty;
                        createReservationResponse.error_info.description = "Sorry, you must be an active club member to reserve this event.";
                        return new ObjectResult(createReservationResponse);
                    }
                }

                if (!model.force_rsvp && CheckTableInventory)
                {
                    bool TableInventoryerror = false;
                    if (model.slot_id > 0 && model.reservation_id == 0)
                    {
                        TableInventoryerror = eventDAL.CheckIfTableAvailableForSlot(model.slot_id, model.slot_type, Convert.ToDateTime(model.event_start_date), model.total_guests) == false;
                    }
                    else if (model.slot_id > 0 && model.reservation_id > 0)
                    {
                        TableInventoryerror = eventDAL.CheckifTableAvailableCanFitGuestForRSVP(model.reservation_id, model.total_guests) == false;
                    }
                    else if (model.slot_id == 0 && model.reservation_id > 0)
                    {
                        CheckAvailableQtyPrivatersvpModel avlqty = eventDAL.CheckifTableAvailableCanFitGuestForPrivateRSVP(model.reservation_id, model.total_guests);
                        TableInventoryerror = avlqty.party_can_seated == false;
                    }

                    if (TableInventoryerror)
                    {
                        createReservationResponse.success = false;
                        createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.TableInventory;
                        createReservationResponse.error_info.extra_info = "Guest Booking Warning";
                        createReservationResponse.error_info.description = string.Format("No tables available for party of {0} guests.", model.total_guests);
                        return new ObjectResult(createReservationResponse);
                    }
                }

                createReservationModel.FeePerPerson = model.fee_per_person;

                if (IsModify == false)
                    createReservationModel.Status = model.status;

                decimal rsvpTotal = createReservationModel.FeePerPerson * model.total_guests;

                if (model.fee_type == 1)
                    rsvpTotal = createReservationModel.FeePerPerson;

                createReservationModel.EventName = model.event_name;
                createReservationModel.EventLocation = model.location_name;
                createReservationModel.LocationId = model.location_id;

                createReservationModel.AffiliateID = model.affiliate_id;
                createReservationModel.HDYH = model.hdyh;
                createReservationModel.PayType = 1;
                createReservationModel.BookedById = model.booked_by_id ?? 0;
                createReservationModel.BookedByName = model.booked_by_name;
                createReservationModel.ReservationId = model.reservation_id;

                createReservationModel.Note = model.guest_note;
                createReservationModel.InternalNote = model.internal_note;
                createReservationModel.PersonalMessage = model.personal_message;
                createReservationModel.ConciergeNote = model.concierge_note;
                createReservationModel.MobilePhone = "";

                createReservationModel.IgnoreDiscount = model.ignore_discount;

                decimal discountAmount = 0;
                createReservationModel.DiscountId = 0;
                createReservationModel.DiscountCode = "";
                createReservationModel.DiscountCodeAmount = 0;
                createReservationModel.DiscountDesc = "";

                

                if (!model.ignore_discount)
                {
                    DiscountDAL discountDAL = new DiscountDAL(Common.Common.ConnectionString);
                    var eventDiscount = new EventDiscount();

                    if (!string.IsNullOrEmpty(model.discount_code))
                    {
                        eventDiscount = discountDAL.GetDiscountDetail(createReservationModel.EventId, model.discount_code);

                        if (eventDiscount != null && eventDiscount.Id > 0)
                        {
                            createReservationModel.DiscountId = eventDiscount.Id;
                            createReservationModel.DiscountCode = model.discount_code;
                        }
                    }

                    if (model.discount_amount > 0 && model.is_club_discount == false)
                    {
                        discountAmount = model.discount_amount;
                    }
                    else if (model.discount_amount == -1)
                    {
                        discountAmount = 0;
                    }
                    else if (model.discount_amount == 0 || model.is_club_discount) //(ActionSource)model.referral_type != ActionSource.BackOffice || 
                    {
                        if (LoadDiscount == false)
                            result = await Discount.CheckAndApplyEventDiscount(eventModel, model.total_guests, model.fee_per_person, model.discount_code, model.member_id, model.user_detail.email, model.activation_codes, Convert.ToDateTime(model.event_start_date), model.fee_type, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                        //Discount.EventDiscountResult result = new Discount.EventDiscountResult();
                        //result = await Discount.CheckAndApplyEventDiscount(eventModel, model.total_guests, model.fee_per_person, model.discount_code, model.member_id, model.user_detail.email, model.activation_codes, Convert.ToDateTime(model.event_start_date), model.fee_type, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);
                        if (result.DiscountValid)
                        {
                            discountAmount = result.DiscountTotal;

                            createReservationModel.DiscountId = result.DiscountId;
                            createReservationModel.DiscountCode = model.discount_code;
                            createReservationModel.DiscountCodeAmount = discountAmount;
                            createReservationModel.DiscountAmt = discountAmount;
                            createReservationModel.DiscountDesc = result.DiscountMsg;

                            List<ActivationCodesModel> ActivationCodes = new List<ActivationCodesModel>();

                            if (result.ActivationCodes != null && result.ActivationCodes.Count > 0)
                            {
                                foreach (var item in result.ActivationCodes)
                                {
                                    if (item.IsValid)
                                    {
                                        ActivationCodesModel code = new ActivationCodesModel();
                                        code.activation_code = item.ActivationCode;
                                        code.discount_desc = item.DiscountDesc;
                                        code.is_valid = item.IsValid;
                                        code.ticket_id = item.TicketId;
                                        ActivationCodes.Add(code);
                                    }
                                }
                                createReservationModel.activation_codes = ActivationCodes;
                            }
                        }

                        Discount.EventDiscountResult addOnsresult = new Discount.EventDiscountResult();

                        List<Addon_info> listaddon_info = new List<Addon_info>();
                        foreach (var item in model.reservation_addon)
                        {
                            Addon_info addon_info = new Addon_info();
                            addon_info.group_id = item.group_id;
                            addon_info.item_id = item.item_id;
                            addon_info.price = item.price;
                            addon_info.qty = item.qty;

                            listaddon_info.Add(addon_info);
                        }

                        if (listaddon_info != null && listaddon_info.Count > 0)
                            addOnsresult = await Discount.CheckAndApplyEventAddOnsDiscount(listaddon_info, model.member_id, model.user_detail.email, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                        if (addOnsresult.DiscountValid)
                        {
                            discountAmount = discountAmount + addOnsresult.DiscountTotal;

                            createReservationModel.DiscountId = addOnsresult.DiscountId;
                            createReservationModel.DiscountCode = model.discount_code;
                            createReservationModel.DiscountCodeAmount = discountAmount;
                            createReservationModel.DiscountAmt = discountAmount;

                            if (!string.IsNullOrEmpty(createReservationModel.DiscountDesc))
                                createReservationModel.DiscountDesc = createReservationModel.DiscountDesc + ", ";

                            createReservationModel.DiscountDesc = createReservationModel.DiscountDesc + addOnsresult.DiscountMsg;

                        }
                    }
                }

                createReservationModel.DiscountAmt = discountAmount;

                //if (discountAmount > 0)
                //{
                //    createReservationModel.DiscountDesc = "Discount Code Applied";
                //}

                decimal addOnTotal = 0;
                decimal addOnTaxableTotal = 0;
                decimal taxPercent = 0;

                if (!model.ignore_sales_tax || model.tax_gratuity)
                {
                    Utility objUtility = new Utility();

                    if (model.location_id == 0 && model.floor_plan_id > 0)
                        createReservationModel.LocationId = eventDAL.GetLocationIdByFloorPlanId(model.floor_plan_id);

                    taxPercent = await objUtility.GetTax(createReservationModel.EventId, 1, 100, createReservationModel.LocationId, false);
                }
                decimal gratuityTotal = 0;
                decimal gratuityPercent = model.gratuity_percentage;
                bool taxGratuity = model.tax_gratuity;
                decimal taxAmount = 0;

                decimal taxableGratuityAmt = 0;

                if (model.reservation_id > 0 && model.status == 8)
                {
                    eventDAL.DeleteAllAddonOnReservation(model.reservation_id);
                }
                decimal addondiscount = 0;
                foreach (var item in model.reservation_addon)
                {
                    decimal addonPrice = item.price;
                    if (!item.addon_price_manual.HasValue)
                    {
                        item.addon_price_manual = item.price;
                    }
                    if (item.addon_price_manual != item.price)
                    {
                        addondiscount = addondiscount + ((item.price - item.addon_price_manual.Value) * item.qty);
                        addonPrice = item.addon_price_manual.Value;
                    }

                    if (item.Taxable)
                        addOnTaxableTotal += addonPrice * item.qty;
                    addOnTotal += addonPrice * item.qty;
                    if (!IsAdmin || gratuityPercent > 0)
                    {
                        if (item.calculate_gratuity && gratuityPercent > 0)
                        {
                            decimal itemGratuity = Utility.CalculateGratuity((addonPrice * item.qty), gratuityPercent);
                            gratuityTotal += itemGratuity;

                            if (itemGratuity > 0 && taxGratuity)
                            {
                                taxableGratuityAmt += itemGratuity;
                            }
                        }
                    }
                }


                if (IsAdmin && model.gratuity_total != 0)
                {
                    if (model.tax_gratuity)
                        taxableGratuityAmt = model.gratuity_total;

                    gratuityTotal = model.gratuity_total;
                }
                else if (gratuityPercent > 0 && (rsvpTotal + addOnTotal) > discountAmount)
                {
                    decimal rsvpGratuity = Utility.CalculateGratuity(rsvpTotal + addondiscount - discountAmount, gratuityPercent);
                    gratuityTotal += rsvpGratuity;
                    if (eventModel.TaxGratuity)
                        taxableGratuityAmt += rsvpGratuity;
                }
                else
                {
                    gratuityTotal = 0;
                    taxableGratuityAmt = 0;
                }

                if (model.ignore_sales_tax)
                {
                    createReservationModel.SalesTax = 0;
                    createReservationModel.SalesTaxPercentage = 0;
                    rsvpTotal = rsvpTotal + addondiscount - discountAmount;
                }
                else
                {
                    decimal rsvpTaxTotal = addOnTaxableTotal + taxableGratuityAmt;

                    if ((eventModel.ChargeSalesTax || eventModel.EventID == 0) && rsvpTotal > 0)
                    {
                        rsvpTaxTotal = rsvpTaxTotal + rsvpTotal + addondiscount - discountAmount;
                    }

                    rsvpTotal = rsvpTotal + addondiscount - discountAmount;
                    if (rsvpTaxTotal > 0)
                    {
                        //Utility objUtility = new Utility();
                        //taxAmount = await objUtility.GetTax(createReservationModel.EventId, 1, 100, 0, model.ignore_sales_tax);
                        taxAmount += (rsvpTaxTotal * taxPercent) / 100;
                    }

                    createReservationModel.SalesTax = Convert.ToDecimal(String.Format("{0:0.00}", taxAmount));
                    if (taxAmount > 0 && (rsvpTotal + addOnTaxableTotal) > 0)
                        createReservationModel.SalesTaxPercentage = taxPercent;
                    else
                        createReservationModel.SalesTaxPercentage = 0;
                }

                //createReservationModel.PayCardType = "";
                createReservationModel.WaiveFee = false;
                createReservationModel.ReferralID = model.referral_id;
                createReservationModel.CompletedGuestCount = 0;
                createReservationModel.ReturningGuest = existingUser;
                createReservationModel.EmailContentID = model.email_content_id;
                createReservationModel.WineryReferralId = 0;
                createReservationModel.CreditCardReferenceNumber = "";

                decimal OrderTotalWithoutTax = (createReservationModel.FeePerPerson * model.total_guests) + addOnTotal - createReservationModel.DiscountAmt + gratuityTotal;

                if (model.fee_type == 1)
                    OrderTotalWithoutTax = (createReservationModel.FeePerPerson) + addOnTotal - createReservationModel.DiscountAmt + gratuityTotal;

                decimal svcFee = eventDAL.GetServiceFeePaidByGuest(model.member_id, model.total_guests, OrderTotalWithoutTax, (ReferralType)model.referral_type);

                svcFee = Convert.ToDecimal(String.Format("{0:0.00}", svcFee));

                decimal FeeDue = rsvpTotal + createReservationModel.SalesTax + addOnTotal + gratuityTotal + svcFee;

                if (FeeDue < 0)
                {
                    FeeDue = 0;
                }



                if (requiresCreditCard(createReservationModel.ChargeFee) && FeeDue > 0 && paymentConfig != null && paymentConfig.PaymentGateway != Configuration.Gateway.Offline)
                {
                    createReservationModel.RequireCreditCard = true;
                    if (string.IsNullOrWhiteSpace(createReservationModel.PayCardToken) && model.status != 8)  // && (string.IsNullOrEmpty(createReservationModel.PayCardNumber) || string.IsNullOrEmpty(createReservationModel.PayCardCustName) || string.IsNullOrEmpty(createReservationModel.PayCardExpYear) || string.IsNullOrEmpty(createReservationModel.PayCardExpMonth))
                    {
                        createReservationResponse.success = false;
                        createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                        createReservationResponse.error_info.extra_info = "Credit Card Error!";
                        createReservationResponse.error_info.description = "A valid credit card is required for this transaction.\r\nPlease try again.";
                        return new ObjectResult(createReservationResponse);
                        //show Credit Card error
                    }
                }

                createReservationModel.FeeDue = FeeDue;

                if (IsModify)
                    createReservationModel.AmountPaid = reservationDetailModel.amount_paid;
                else
                    createReservationModel.AmountPaid = 0;

                createReservationModel.PurchaseTotal = 0;
                createReservationModel.GratuityAmount = gratuityTotal;

                if (createReservationModel.EventId == 0)
                {
                    foreach (var item in model.reservation_addon)
                    {
                        if (item.item_id > 0 && item.qty > 0)
                        {
                            createReservationModel.privateaddongroup = 1;
                            break;
                        }
                    }
                }

                try
                {
                    var userDetailModel = new List<UserDetailModel>();
                    userDetailModel = await Utility.GetUsersByEmail(createReservationModel.Email, createReservationModel.WineryId, false, _appSetting.Value.ShopifyUrl, _appSetting.Value.ShopifyAuthToken);

                    if (userDetailModel != null && userDetailModel.Count > 0)
                    {
                        if (userDetailModel[0].contact_types.Count > 0)
                        {
                            createReservationModel.ContactTypes = string.Join(',', userDetailModel[0].contact_types);

                            if ((ReferralType)createReservationModel.ReferralType != ReferralType.BackOffice)
                            {
                                createReservationModel.CustomerType = userDetailModel[0].customer_type;
                            }
                        }
                    }
                }
                catch { }

                string currentuser = HttpContext.Request.Headers["AuthenticateKey"];
                if (!string.IsNullOrEmpty(model.modified_by_name))
                {
                    currentuser = model.modified_by_name;
                }


                CreateReservation createReservation = new CreateReservation();
                createReservation = eventDAL.SaveReservation(createReservationModel, IsAdmin, model.force_rsvp, IsModify, reservationDetailModel.total_guests, currentuser, IsRescheduled);
                model.reservation_id = createReservation.Id;
                createReservationModel.ReservationId = model.reservation_id;

                if (createReservation.Status == ResponseStatus.Success)
                {
                    string paymentMsg = string.Empty;
                    if (model.status != 8)
                    {
                        if (model.over_book)
                        {
                            string noteMsg = "Overbooking Authorized- " + currentuser;
                            eventDAL.SaveReservationV2StatusNotes(createReservation.Id, 0, noteMsg, currentuser, createReservationModel.WineryId, 0);
                        }

                        if (model.cart_guid != default(Guid))
                        {
                            eventDAL.ConvertAbandonedCart(model.cart_guid, createReservationModel.UserId, createReservationModel.Email, model.reservation_id);
                        }
                        else
                        {
                            eventDAL.DeleteEventAbandoned(createReservationModel.Email, createReservationModel.SlotId, createReservationModel.SlotType);
                        }

                        //custom settigs
                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                        var customSetting = settingsDAL.GetCustomSettingByMember(Common.Common.SettingType.EJGalloAPI, model.member_id);

                        if (customSetting != null)
                        {
                            try
                            {
                                EJGallo eJ = new EJGallo(_appSetting);
                                bool isSuccess = await eJ.NewsletterSignup(model.member_id, model.user_detail, customSetting);
                            }
                            catch (Exception ex)
                            {
                                logDAL.InsertLog("WebApi", "SaveReservation- EJGallo:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);

                            }
                        }

                        Times.TimeZone timeZone = Times.TimeZone.PacificTimeZone;
                        bool enablePreAuth = false;
                        if (model.member_id > 0)
                        {
                            try
                            {

                                timeZone = (Times.TimeZone)eventDAL.GetTimeZonebyWineryId(model.member_id);

                            }
                            catch { timeZone = Times.TimeZone.PacificTimeZone; }

                            try
                            {
                                enablePreAuth = eventDAL.CheckPreAuthEnabledForWinery(model.member_id);
                            }
                            catch { enablePreAuth = false; }
                        }

                        if (IsRescheduled)
                        {
                            string noteMsg = String.Format("RESCHEDULED {0} Guests for {1} on {2} by {3} on {4}", reservationDetailModel.total_guests, reservationDetailModel.event_name, reservationDetailModel.event_start_date.ToString("MM/dd/yyyy hh:mm tt"), ((Common.Common.ReferralTypeFullText)Convert.ToInt32(createReservationModel.ReferralType)).GetEnumDescription(), Times.ToTimeZoneTime(reservationDetailModel.booking_date, timeZone).ToString("MM/dd/yyyy hh:mm:ss tt"));
                            eventDAL.SaveReservationV2StatusNotes(createReservationModel.ReservationId, createReservationModel.Status, noteMsg, HttpContext.Request.Headers["AuthenticateKey"], createReservationModel.WineryId, 0);
                        }

                        if (!IsModify)
                        {
                            if (model.subscribe_marketing_optin || model.cellarPass_marketing_optin)
                            {
                                //mailchimp integration
                                bool isMailChimpEnabled = eventDAL.IsMailChimpModuleAvailable(model.member_id);

                                if (isMailChimpEnabled)
                                {
                                    if (model.subscribe_marketing_optin)
                                    {
                                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(model.member_id, (int)Common.Common.SettingGroup.mailchimp);
                                        string mcAPIKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_key);
                                        string mcStore = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_store);
                                        string mcCampaign = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_listname);

                                        if (!string.IsNullOrWhiteSpace(mcAPIKey) && !string.IsNullOrWhiteSpace(mcStore))
                                        {
                                            try
                                            {
                                                QueueService getStarted = new QueueService();

                                                var queueModel = new EmailQueue();
                                                queueModel.EType = (int)EmailType.MailChimpOrder;
                                                queueModel.BCode = createReservation.BookingCode;
                                                queueModel.UId = createReservationModel.UserId;
                                                queueModel.RsvpId = createReservation.Id;
                                                queueModel.PerMsg = model.personal_message;
                                                queueModel.Src = reservationDetailModel.referral_type;
                                                var qData = JsonConvert.SerializeObject(queueModel);

                                                AppSettings _appsettings = _appSetting.Value;
                                                getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                                            }
                                            catch (Exception ex)
                                            {
                                                logDAL.InsertLog("WebApi", "SaveReservation Create Mail Chimp Order:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
                                            }
                                        }
                                    }
                                }

                                if (model.cellarPass_marketing_optin)
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
                                            queueModel.EType = (int)EmailType.MailChimpOrder;
                                            queueModel.BCode = createReservation.BookingCode;
                                            queueModel.UId = 0;
                                            queueModel.RsvpId = createReservation.Id;
                                            queueModel.PerMsg = model.personal_message;
                                            queueModel.Src = reservationDetailModel.referral_type;
                                            var qData = JsonConvert.SerializeObject(queueModel);

                                            AppSettings _appsettings = _appSetting.Value;
                                            getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                                        }
                                        catch (Exception ex)
                                        {
                                            logDAL.InsertLog("WebApi", "SaveReservation Create Mail Chimp Order:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
                                        }
                                    }
                                }
                            }

                        }

                        bool SaveDiscount = (createReservationModel.DiscountAmt != reservationDetailModel.discount_amount);
                        if (SaveDiscount)
                        {
                            if (createReservationModel.activation_codes != null && createReservationModel.activation_codes.Count > 0)
                            {
                                foreach (var item in createReservationModel.activation_codes)
                                {
                                    string noteMsg = string.Format("Activation Code {0}- discounted {1} by {2}", item.activation_code, model.fee_per_person, HttpContext.Request.Headers["AuthenticateKey"]);
                                    eventDAL.SavePassportRsvpClaim((int)Common.Common.RsvpClaimStatus.booked, item.ticket_id, createReservationModel.WineryId, createReservation.Id, DateTime.UtcNow);
                                    eventDAL.SaveReservationV2StatusNotes(createReservation.Id, 0, noteMsg, HttpContext.Request.Headers["AuthenticateKey"], createReservationModel.WineryId, 1);
                                    eventDAL.SaveDiscountToReservation((int)Common.Common.Discount.activationCode, item.activation_code, model.fee_per_person, item.discount_desc, createReservation.Id);
                                }
                            }
                            else
                            {
                                if (model.discount_amount > 0 && createReservationModel.DiscountAmt > 0 && model.is_club_discount == false)
                                {
                                    string DiscountAmt = string.Format(new CultureInfo("en-US"), "{0:C}", createReservationModel.DiscountAmt);
                                    string noteMsg = string.Format("Manual discount of {0} applied by {1}", DiscountAmt, HttpContext.Request.Headers["AuthenticateKey"]);
                                    eventDAL.SaveReservationV2StatusNotes(createReservation.Id, 0, noteMsg, HttpContext.Request.Headers["AuthenticateKey"], createReservationModel.WineryId, 1);
                                    eventDAL.SaveDiscountToReservation((int)Common.Common.Discount.manualDiscount, "Manual Discount", createReservationModel.DiscountAmt, noteMsg, createReservation.Id);
                                }
                                else if (createReservationModel.DiscountAmt > 0)
                                {
                                    if (!string.IsNullOrEmpty(createReservationModel.DiscountCode))
                                    {
                                        string DiscountAmt = string.Format(new CultureInfo("en-US"), "{0:C}", createReservationModel.DiscountAmt);
                                        string noteMsg = string.Format("Promo Code {0}- discounted {1} by {2}", createReservationModel.DiscountCode, DiscountAmt, HttpContext.Request.Headers["AuthenticateKey"]);
                                        eventDAL.SaveReservationV2StatusNotes(createReservation.Id, 0, noteMsg, HttpContext.Request.Headers["AuthenticateKey"], createReservationModel.WineryId, 1);
                                        eventDAL.SaveDiscountToReservation((int)Common.Common.Discount.discount, createReservationModel.DiscountCode, createReservationModel.DiscountAmt, noteMsg, createReservation.Id);
                                    }
                                    else
                                    {
                                        string DiscountAmt = string.Format(new CultureInfo("en-US"), "{0:C}", createReservationModel.DiscountAmt);
                                        //string noteMsg = string.Format("Promo Code {0}- discounted {1} by {2}", createReservationModel.DiscountCode, DiscountAmt, HttpContext.Request.Headers["AuthenticateKey"]);
                                        eventDAL.SaveReservationV2StatusNotes(createReservation.Id, 0, createReservationModel.DiscountDesc, HttpContext.Request.Headers["AuthenticateKey"], createReservationModel.WineryId, 1);
                                        eventDAL.SaveDiscountToReservation((int)Common.Common.Discount.discount, createReservationModel.DiscountCode, createReservationModel.DiscountAmt, createReservationModel.DiscountDesc, createReservation.Id);
                                    }
                                }
                            }
                        }

                        int BookedById = 0;
                        if (createReservationModel.BookedById > 0)
                            BookedById = createReservationModel.BookedById;
                        else
                            BookedById = createReservationModel.UserId;

                        
                        try
                        {
                            //check if inventory mode v3 is on then add a change for floorplan
                            var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                            var model1 = new CreateDeltaRequest();
                            model1.item_id = model.reservation_id;
                            model1.item_type = (int)ItemType.Reservations;
                            model1.location_id = createReservationModel.LocationId;
                            model1.member_id = createReservationModel.WineryId;
                            model1.action_date = createReservationModel.EventDate;
                            int floorPlanId = 0;
                            if (invMode == 1)
                            {
                                floorPlanId = eventDAL.GetFloorPlanIdForReservation(model.reservation_id);
                            }
                            model1.floor_plan_id = floorPlanId;
                            notificationDAL.SaveDelta(model1);

                            if (model.account_notes_have_changes && (!string.IsNullOrEmpty(model.account_notes)))
                            {
                                userDAL.UpdateUserAccountNotes(createReservationModel.UserId, createReservationModel.WineryId, model.account_notes, HttpContext.Request.Headers["AuthenticateKey"]);
                            }

                            int ExceptionId = eventDAL.AutoCloseEventRule(createReservationModel.SlotId, createReservationModel.SlotType, model.reservation_id, createReservationModel.EventDate, model.modified_by_name, BookedById);

                            if (ExceptionId > 0 && createReservationModel.SlotType == 0)
                            {
                                createReservationModel.SlotType = 1;
                                createReservationModel.SlotId = ExceptionId;
                            }

                            if (IsModify)
                            {
                                eventDAL.DeleteAllAddonOnReservation(model.reservation_id);
                                if (Convert.ToDateTime(model.event_end_date) != reservationDetailModel.event_end_date || Convert.ToDateTime(model.event_start_date) != reservationDetailModel.event_start_date || model.slot_id != reservationDetailModel.slot_id)
                                    eventDAL.UndoAutoCloseEventRule(reservationDetailModel.slot_id, reservationDetailModel.slot_type, reservationDetailModel.event_start_date, model.modified_by_name, false, BookedById);
                            }

                            foreach (var item in model.reservation_addon)
                            {
                                if (item.item_id > 0 && item.qty > 0)
                                    eventDAL.SaveReservation_Addon(item.category, item.cost, item.description, item.image, item.item_id, item.item_type, item.name, item.price, item.qty, item.sku, createReservation.Id, item.group_id, item.group_item_id, item.Taxable, item.addon_price_manual.Value);
                            }

                            if (IsModify)
                            {
                                eventDAL.DeleteAllGuestsOnReservation(model.reservation_id);
                            }

                            foreach (var item in model.additional_guests)
                            {
                                if (!string.IsNullOrEmpty(item.first_name) && !string.IsNullOrEmpty(item.last_name))
                                    eventDAL.SaveGuestToReservation(createReservation.Id, item.first_name, item.last_name, item.email + "");
                            }

                            //## PAYMENT CHARGING ##

                            bool modifyChargeNeeded = false;
                            decimal refundAmount = 0;

                            if (IsModify)
                            {
                                //PAYMENT - REFUND OR CHARGE?
                                decimal total = 0;
                                if (model.charge_fee == 11)
                                    total = Math.Round((createReservationModel.FeeDue / 4), 2);
                                else if (model.charge_fee == 12)
                                    total = Math.Round((createReservationModel.FeeDue / 2), 2);
                                else
                                    total = createReservationModel.FeeDue;

                                if (createReservationModel.FeeDue > 0 && reservationDetailModel.amount_paid < total && ((createReservationModel.FeeDue - reservationDetailModel.amount_paid) > 0))
                                    modifyChargeNeeded = true;
                                else if (createReservationModel.FeeDue >= 0 && reservationDetailModel.amount_paid > 0 && reservationDetailModel.amount_paid > total)
                                {
                                    refundAmount = (reservationDetailModel.amount_paid - total);

                                    Common.Payments.TransactionResult refundResult = new Common.Payments.TransactionResult();
                                    refundResult = await objPayments.RefundReservation(reservationDetailModel, refundAmount, BookedById);

                                    if (refundResult.Status == Common.Payments.TransactionResult.StatusType.Success)
                                    {
                                        reservationDetailModel.amount_paid = reservationDetailModel.amount_paid - refundResult.Amount;
                                        Services.Payments.UpdateReservation(reservationDetailModel);
                                        paymentMsg = string.Format("The 'Credit' for {0} was processed successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", refundAmount));
                                    }
                                    else
                                    {
                                        refundAmount = 0;
                                        paymentMsg = "Reservation completed, but we were unable to process the Credit/Void.";
                                    }
                                }
                            }

                            if (paymentConfig != null && paymentConfig.PaymentGateway > 0 && createReservationModel.FeeDue > 0 && (reservationDetailModel.amount_paid == 0 || modifyChargeNeeded || Math.Round(reservationDetailModel.amount_paid, 2) < Math.Round(createReservationModel.FeeDue, 2)))
                            {
                                decimal amountToCharge = createReservationModel.FeeDue;
                                if (checkChargeCC(model.charge_fee, Convert.ToDateTime(model.event_start_date), timeZone))
                                {
                                    Common.Payments.TransactionResult paymentresult = new Common.Payments.TransactionResult();
                                    string prevTranId = "";
                                    Common.Payments.Transaction.ChargeType chargeType = Transaction.ChargeType.Sale;

                                    var authPayment = eventDAL.GetReservationPreAuths(createReservation.Id);

                                    if (authPayment != null && authPayment.Count > 0 && (model.charge_fee == 1 || model.charge_fee == 5 || model.charge_fee == 6 || model.charge_fee == 8 || model.charge_fee == 9 || model.charge_fee == 10 || model.charge_fee == 4))
                                    {
                                        decimal amtAuthorized = authPayment[0].amount;
                                        if (amtAuthorized != amountToCharge)
                                        {
                                            //void the transaction
                                            Common.Payments.TransactionResult refundResult = new Common.Payments.TransactionResult();
                                            refundResult = await objPayments.VoidReservationPreAuth(authPayment[0], reservationDetailModel, amtAuthorized, BookedById);

                                        }
                                        else
                                        {
                                            chargeType = Transaction.ChargeType.Capture;
                                            prevTranId = authPayment[0].transaction_id;
                                        }
                                    }

                                    //if (enablePreAuth == false)
                                    //{
                                    if (model.charge_fee == 11)
                                        amountToCharge = Math.Round(amountToCharge / 4, 2) - Math.Round(reservationDetailModel.amount_paid, 2);
                                    else if (model.charge_fee == 12)
                                        amountToCharge = Math.Round((amountToCharge / 2), 2) - Math.Round(reservationDetailModel.amount_paid, 2);
                                    else
                                        amountToCharge = Math.Round(amountToCharge, 2) - Math.Round(reservationDetailModel.amount_paid, 2);
                                    //}

                                    if (amountToCharge > 0)
                                    {
                                        if (IsModify)
                                        {
                                            //var paymentConfig = eventDAL.GetPaymentConfigByWineryId(model.member_id);

                                            if (paymentConfig != null)
                                            {
                                                if (paymentConfig.PaymentGateway == Configuration.Gateway.Braintree)
                                                {
                                                    Configuration config = new Configuration();
                                                    config.MerchantLogin = paymentConfig.MerchantLogin;
                                                    config.MerchantPassword = paymentConfig.MerchantPassword;
                                                    config.UserConfig1 = paymentConfig.UserConfig1;
                                                    config.GatewayMode = (Configuration.Mode)paymentConfig.GatewayMode;

                                                    TokenizedCard card = null;
                                                    Services.Braintree objBraintree = new Services.Braintree(_appSetting);

                                                    TokenizedCardRequest request = new TokenizedCardRequest();

                                                    request.number = "";
                                                    request.card_last_four_digits = model.pay_card.card_last_four_digits;
                                                    request.card_first_four_digits = model.pay_card.card_first_four_digits;
                                                    request.cust_name = model.user_detail.first_name + " " + model.user_detail.last_name;
                                                    request.card_entry = model.pay_card.card_entry;
                                                    request.application_type = model.pay_card.application_type;
                                                    request.application_version = model.pay_card.application_version;
                                                    request.terminal_id = model.pay_card.terminal_id;
                                                    request.card_reader = model.pay_card.card_reader;

                                                    card = Services.Braintree.TokenziedCard(request, config);

                                                    if (card != null && !string.IsNullOrEmpty(card.card_token))
                                                    {
                                                        model.pay_card.card_token = card.card_token;
                                                        model.pay_card.card_first_four_digits = card.first_four_digits;
                                                        model.pay_card.card_last_four_digits = card.last_four_digits;
                                                        createReservationModel.PayCardFirstFourDigits = card.first_four_digits;
                                                        createReservationModel.PayCardLastFourDigits = card.last_four_digits;

                                                        var winery = new Model.WineryModel();
                                                        winery = eventDAL.GetWineryById(request.member_id);

                                                        if ((winery.EnableVin65 || winery.EnableClubVin65) && !string.IsNullOrEmpty(card.card_token) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(winery.SALT) && !string.IsNullOrEmpty(winery.DecryptKey))
                                                        {
                                                            string cardtype2 = Services.Payments.GetCardType(request.number, "vin65");
                                                            string cardnumber = Common.StringHelpers.EncryptedCardNumber(request.number, winery.SALT, winery.DecryptKey);
                                                            eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, request.member_id, winery.Vin65UserName, winery.Vin65Password, request.cvv2);
                                                        }
                                                        else if ((winery.EnableCommerce7 || winery.EnableClubCommerce7) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(card.card_token))
                                                        {
                                                            string cardtype2 = Services.Payments.GetCardType(request.number, "commerce7");
                                                            string cardnumber = Common.Common.Right(request.number, 4).PadLeft(request.number.Length, '*');

                                                            string gateway = "No Gateway";
                                                            gateway = Utility.GetCommerce7PaymentGatewayName(paymentConfig.PaymentGateway);

                                                            eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, request.member_id, gateway, "", request.cvv2);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        paymentresult = await objPayments.ChargeReservation(model, amountToCharge, createReservationModel.BookingCode, BookedById, chargeType, prevTranId);

                                        if (paymentresult.Status == Common.Payments.TransactionResult.StatusType.Failed && model.charge_fee == 10)
                                        {
                                            if (!IsModify)
                                            {
                                                eventDAL.UpdateReservationStatus(createReservation.Id, (int)ReservationStatus.Initiated);
                                                eventDAL.UndoAutoCloseEventRule(createReservationModel.SlotId, createReservationModel.SlotType, createReservationModel.EventDate, model.modified_by_name, false, BookedById);
                                            }
                                            else
                                                eventDAL.UpdateReservationStatus(createReservation.Id, createReservationModel.Status);

                                            createReservationResponse.success = false;
                                            createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationPaymentError;
                                            createReservationResponse.error_info.extra_info = "Payment Error " + paymentresult.Detail;
                                            createReservationResponse.error_info.description = "Problem processing your payment. Please review the credit card and try again.";

                                            if (string.IsNullOrEmpty(paymentresult.Detail))
                                            {
                                                paymentresult.Detail = "Problem processing your payment.";
                                            }

                                            eventDAL.SaveReservationPaymentV2(createReservationModel.WineryId, createReservation.Id, paymentresult);

                                            return new ObjectResult(createReservationResponse);
                                        }

                                        if (paymentresult.Status == Common.Payments.TransactionResult.StatusType.Success)
                                        {
                                            if (model.charge_fee == 10 || createReservationModel.Status == (int)ReservationStatus.Initiated)
                                                reservationDetailModel.status = (int)ReservationStatus.Pending;

                                            if (paymentresult.Amount > 0)
                                            {
                                                //Update Amount Paid on Rsvp Object
                                                reservationDetailModel.amount_paid = reservationDetailModel.amount_paid + amountToCharge;
                                                reservationDetailModel.reservation_id = createReservation.Id;
                                                Services.Payments.UpdateReservation(reservationDetailModel);

                                                if (model.charge_fee == 11 || model.charge_fee == 12)
                                                {
                                                    //If this was a 24/48 before event charge and it was charged because the booking date was already within this time then show this message
                                                    paymentMsg = string.Format("Required immediate payment. {0} was processed successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", amountToCharge));
                                                }
                                                else if (model.charge_fee == 11)
                                                {
                                                    paymentMsg = string.Format("25% Deposit ({0}) was charged successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", amountToCharge));
                                                }
                                                else if (model.charge_fee == 12)
                                                {
                                                    paymentMsg = string.Format("50% Deposit ({0}) was charged successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", amountToCharge));
                                                }
                                                else
                                                {
                                                    //Standard Payment charged message
                                                    paymentMsg = string.Format("{0} was charged successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", amountToCharge));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(paymentresult.Detail))
                                            {
                                                paymentMsg = paymentresult.Detail;
                                            }
                                            else
                                            {
                                                paymentresult.Detail = "Credit Card payment was DECLINED.";
                                                paymentMsg = "Declined credit card";
                                            }
                                            //****** Below line was commented on 07/28 to avoid duplicate rows***
                                            //eventDAL.SaveReservationPaymentV2(createReservationModel.WineryId, createReservation.Id, paymentresult);
                                        }
                                    }
                                }
                                else if (enablePreAuth && (model.charge_fee == 1 || model.charge_fee == 5 || model.charge_fee == 6 || model.charge_fee == 8 || model.charge_fee == 9))
                                {
                                    // do auth
                                    //if it is a mofify then void previous auth and issue a fresh auth
                                    if (amountToCharge > 0)
                                    {
                                        bool proceedTrans = true;
                                        if (IsModify)
                                        {
                                            //is there a previous auth?
                                            var authPayment = eventDAL.GetReservationPreAuths(createReservation.Id);

                                            if (authPayment != null && authPayment.Count > 0)
                                            {
                                                decimal amtAuthorized = authPayment[0].amount;
                                                if (amtAuthorized != amountToCharge)
                                                {
                                                    //void the transaction

                                                    Common.Payments.TransactionResult refundResult = new Common.Payments.TransactionResult();
                                                    refundResult = await objPayments.VoidReservationPreAuth(authPayment[0], reservationDetailModel, amtAuthorized, BookedById);
                                                    proceedTrans = (refundResult.Status == TransactionResult.StatusType.Success);
                                                }
                                                else
                                                {
                                                    proceedTrans = false; //no need to do anything
                                                }
                                            }


                                        }

                                        if (proceedTrans)
                                        {
                                            var paymentresult = await objPayments.ChargeReservation(model, amountToCharge, createReservationModel.BookingCode, BookedById, Transaction.ChargeType.AuthOnly);

                                            if (paymentresult.Status == TransactionResult.StatusType.Failed)
                                            {

                                                if (!IsModify)
                                                {
                                                    eventDAL.UpdateReservationStatus(createReservation.Id, (int)ReservationStatus.Initiated);
                                                    eventDAL.UndoAutoCloseEventRule(createReservationModel.SlotId, createReservationModel.SlotType, createReservationModel.EventDate, model.modified_by_name, false, BookedById);
                                                }
                                                else
                                                    eventDAL.UpdateReservationStatus(createReservation.Id, createReservationModel.Status);

                                                createReservationResponse.success = false;
                                                createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationPaymentError;
                                                createReservationResponse.error_info.extra_info = "Payment Authorization Error " + paymentresult.Detail;
                                                createReservationResponse.error_info.description = "Problem authorizing your payment. Please review the credit card and try again.";

                                                if (string.IsNullOrEmpty(paymentresult.Detail))
                                                {
                                                    paymentresult.Detail = "Problem authorizing your payment.";
                                                }


                                                //if (!string.IsNullOrEmpty(paymentresult.Detail))
                                                //{
                                                //    paymentMsg = paymentresult.Detail;
                                                //}
                                                //else
                                                //{
                                                //    paymentresult.Detail = "Credit Card pre authorization was FAILED.";
                                                //    paymentMsg = "Declined credit card";
                                                //}
                                                eventDAL.SaveReservationPaymentV2(createReservationModel.WineryId, createReservation.Id, paymentresult);
                                                return new ObjectResult(createReservationResponse);

                                            }
                                            else
                                            {
                                                if (createReservationModel.Status == (int)ReservationStatus.Initiated)
                                                    reservationDetailModel.status = (int)ReservationStatus.Pending;
                                                reservationDetailModel.reservation_id = createReservation.Id;
                                                Services.Payments.UpdateReservation(reservationDetailModel);
                                            }
                                        }
                                    }

                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            if (!IsModify)
                            {
                                eventDAL.UpdateReservationStatus(createReservation.Id, (int)ReservationStatus.Initiated);
                                eventDAL.UndoAutoCloseEventRule(createReservationModel.SlotId, createReservationModel.SlotType, createReservationModel.EventDate, model.modified_by_name, false, BookedById);
                            }

                            createReservationResponse.success = false;
                            createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                            createReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                            createReservationResponse.error_info.description = ex.Message.ToString();
                            logDAL.InsertLog("WebApi", "SaveReservation-:  Request-" + JsonConvert.SerializeObject(model) + ", Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
                            return new ObjectResult(createReservationResponse);
                        }

                        if (model.user_detail != null)
                        {
                            // add it to queue to create third party contact
                            try
                            {
                                string optin = "";
                                if (model.subscribe_marketing_optin)
                                {
                                    userDAL.UpdateUserWineryMarketingStatus(createReservationModel.WineryId, createReservationModel.UserId, "Subscribed");
                                    optin = "1";
                                }

                                QueueService getStarted = new QueueService();

                                var queueModel = new EmailQueue();
                                queueModel.EType = (int)EmailType.CreateThirdPartyContact;
                                queueModel.BCode = model.reservation_id.ToString();
                                queueModel.UId = createReservationModel.UserId;
                                queueModel.RsvpId = createReservationModel.WineryId;
                                queueModel.PerMsg = optin;
                                queueModel.Src = reservationDetailModel.referral_type;
                                var qData = JsonConvert.SerializeObject(queueModel);

                                AppSettings _appsettings = _appSetting.Value;
                                getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                            }
                            catch (Exception ex)
                            {
                                logDAL.InsertLog("WebApi", "SaveReservation Create Third Party Contact:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
                            }
                        }

                        try
                        {
                            bool CreateMeeting = false;

                            if (IsModify)
                            {
                                //TODO
                                ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfoByReservationId(model.reservation_id);
                                if (zoomMeetingInfo != null && zoomMeetingInfo.MeetingId > 0)
                                {
                                    if (zoomMeetingInfo.SlotId == createReservationModel.SlotId && zoomMeetingInfo.SlotType == createReservationModel.SlotType && zoomMeetingInfo.StartDate == createReservationModel.EventDate)
                                    {
                                        CreateMeeting = false;
                                        //DO Nothing
                                    }
                                    else
                                    {
                                        CreateMeeting = await DeleteMeeting(zoomMeetingInfo.MeetingBehavior, zoomMeetingInfo.MeetingId, createReservationModel.WineryId, model.reservation_id, createReservationModel.Status, zoomMeetingInfo.RegistrantId, model.user_detail.email, HttpContext.Request.Headers["AuthenticateKey"]);
                                    }
                                }
                                else
                                {
                                    CreateMeeting = true;
                                }
                            }
                            else
                            {
                                CreateMeeting = true;
                            }

                            if (CreateMeeting)
                            {
                                long MeetingId = await ZoomMeeting.CreateMeeting(createReservationModel.SlotId, createReservationModel.SlotType, createReservationModel.EventDate, model.reservation_id, model.user_detail, model.member_id);

                                if (MeetingId > 0)
                                {
                                    string noteMsg = "Zoom Meeting Created. Meeting Id:" + Utility.FormatZoomMeetingId(MeetingId.ToString());
                                    eventDAL.SaveReservationV2StatusNotes(model.reservation_id, createReservationModel.Status, noteMsg, HttpContext.Request.Headers["AuthenticateKey"], createReservationModel.WineryId, 0);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            logDAL.InsertLog("WebApi", "SaveReservation CreateMeeting:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
                        }

                        try
                        {
                            if (model.send_guest_sms)
                            {
                                if ((MobileNumberStatus)createReservationModel.MobilePhoneStatus == MobileNumberStatus.verified)
                                {
                                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(0, (int)Common.Common.SettingGroup.system);
                                    string SmsNumber = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.system_sms_master_number);

                                    WineryModel wineryModel = eventDAL.GetWinerySmsNumberById(model.member_id);

                                    EmailServiceDAL emailDAL = new EmailServiceDAL(Common.Common.ConnectionString);
                                    EmailContent ew = emailDAL.GetEmailContent((int)Email.EmailTemplates.SMSReservationConfirmation, 0);

                                    string smsbody = ew.EmailBody;
                                    smsbody = smsbody.Replace("[[MemberDisplayName]]", wineryModel.DisplayName);
                                    smsbody = smsbody.Replace("[[EventDate]]", createReservationModel.EventDate.ToString("MM/dd/yyyy"));
                                    smsbody = smsbody.Replace("[[EventStartTime]]", Convert.ToDateTime(model.event_start_date).ToString("hh:mm:ss tt"));
                                    smsbody = smsbody.Replace("[[Confirmation#]]", createReservationModel.BookingCode);
                                    smsbody = smsbody.Replace("[[RsvpDestination]]", eventDAL.GetDestinationNameForReservation(createReservationModel.ReservationId));

                                    string strpath = "https://typhoon.cellarpass.com/";
                                    if (Common.Common.ConnectionString.IndexOf("live") > -1)
                                        strpath = "https://www.cellarpass.com/";

                                    smsbody = smsbody.Replace("[[ConfirmationLink]]", string.Format("{1}rsvp-confirmation/{0}", createReservation.BookingGUID, strpath));

                                    Utility.SMSSend_System(smsbody, SmsNumber, createReservationModel.PhoneNumber);

                                    EmailLogModel emailLog = new EmailLogModel();
                                    emailLog.RefId = createReservationModel.ReservationId;
                                    emailLog.EmailType = (int)EmailType.Rsvp;
                                    emailLog.EmailProvider = (int)Common.Email.EmailProvider.NA;
                                    emailLog.EmailStatus = (int)EmailStatus.SmsSent;
                                    emailLog.EmailSender = "";
                                    emailLog.EmailRecipient = "";
                                    emailLog.LogNote = "Sms Sent";
                                    emailLog.LogDate = DateTime.UtcNow;
                                    emailLog.MemberId = model.member_id;
                                    emailLog.EmailContentId = 0;

                                    emailDAL.SaveEmailLog(emailLog);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logDAL.InsertLog("WebApi", "SaveReservation SMS:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
                        }

                        try
                        {
                            QueueService getStarted = new QueueService();

                            var queueModel = new EmailQueue();
                            queueModel.EType = (int)EmailType.Rsvp;
                            queueModel.BCode = createReservation.BookingCode;
                            queueModel.UId = createReservationModel.UserId;
                            queueModel.RsvpId = createReservation.Id;
                            queueModel.PerMsg = model.personal_message;
                            queueModel.Src = reservationDetailModel.referral_type;
                            queueModel.ActionType = actionType;

                            queueModel.AffiliateEmail = model.send_affiliate_email;
                            queueModel.GuestEmail = model.send_guest_email;

                            var qData = JsonConvert.SerializeObject(queueModel);

                            AppSettings _appsettings = _appSetting.Value;
                            getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                        }
                        catch (Exception ex)
                        {
                            logDAL.InsertLog("WebApi", "SaveReservation Email:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
                        }

                        try
                        {
                            //logDAL.InsertLog("test step 1 bLoyal", "SendReservation:  member_id-" + model.member_id.ToString() + ",Reservation.Id-" + createReservation.Id.ToString(), HttpContext.Request.Headers["AuthenticateKey"]);
                            createReservationModel.AmountPaid = reservationDetailModel.amount_paid;
                            await UpsertOrderTobLoyal(model.member_id, createReservationModel, createReservation.Id);
                        }
                        catch (Exception ex)
                        {
                            logDAL.InsertLog("WebApi", "SaveReservation::  Request-" + JsonConvert.SerializeObject(model) + ", Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
                        }

                        //try
                        //{
                        //    QueueService getStarted = new QueueService();

                        //    var queueModel = new EmailQueue();
                        //    queueModel.EType = (int)EmailType.GoogleCalendar;
                        //    queueModel.BCode = createReservation.BookingCode;
                        //    queueModel.UId = createReservationModel.UserId;
                        //    queueModel.RsvpId = createReservation.Id;
                        //    queueModel.PerMsg = model.personal_message;
                        //    queueModel.Src = reservationDetailModel.referral_type;
                        //    queueModel.ActionType = actionType;
                        //    var qData = JsonConvert.SerializeObject(queueModel);

                        //    AppSettings _appsettings = _appSetting.Value;
                        //    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                        //}
                        //catch { }
                    }
                    else
                    {
                        if (model.account_notes_have_changes && (!string.IsNullOrEmpty(model.account_notes)))
                        {
                            userDAL.UpdateUserAccountNotes(createReservationModel.UserId, createReservationModel.WineryId, model.account_notes, HttpContext.Request.Headers["AuthenticateKey"]);
                        }

                        if (IsModify)
                        {
                            eventDAL.DeleteAllAddonOnReservation(model.reservation_id);
                        }

                        foreach (var item in model.reservation_addon)
                        {
                            if (item.item_id > 0 && item.qty > 0)
                                eventDAL.SaveReservation_Addon(item.category, item.cost, item.description, item.image, item.item_id, item.item_type, item.name, item.price, item.qty, item.sku, createReservation.Id, item.group_id, item.group_item_id, item.Taxable, item.addon_price_manual.Value);
                        }

                        if (IsModify)
                        {
                            eventDAL.DeleteAllGuestsOnReservation(model.reservation_id);
                        }

                        foreach (var item in model.additional_guests)
                        {
                            if (!string.IsNullOrEmpty(item.first_name) && !string.IsNullOrEmpty(item.last_name))
                                eventDAL.SaveGuestToReservation(createReservation.Id, item.first_name, item.last_name, item.email + "");
                        }
                    }

                    CreateReservationResponseModel respData = new CreateReservationResponseModel
                    {
                        booking_code = createReservation.BookingCode,
                        booking_guid = createReservation.BookingGUID,
                        reservation_id = createReservation.Id,
                        save_type = createReservation.SaveType,
                        message = createReservation.Message,
                        payment_message = paymentMsg
                    };

                    if (model.ignore_club_member)
                    {
                        string adminuseremail = currentuser;

                        if (createReservationModel.BookedById > 0)
                            adminuseremail = userDAL.GetUsersbyId(createReservationModel.BookedById).email;

                        string noteMsg = adminuseremail + " approved override for non-club member booking";
                        eventDAL.SaveReservationV2StatusNotes(createReservation.Id, 0, noteMsg, currentuser, createReservationModel.WineryId, 0);
                    }

                    createReservationResponse.data = respData;
                    createReservationResponse.success = true;
                }
                else
                {
                    createReservationResponse.success = false;
                    createReservationResponse.error_info.error_type = createReservation.error_type;
                    createReservationResponse.error_info.extra_info = createReservation.Message;
                    createReservationResponse.error_info.description = createReservation.Description;
                    return new ObjectResult(createReservationResponse);
                }
            }
            catch (Exception ex)
            {
                createReservationResponse.success = false;
                createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                createReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                createReservationResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "SaveReservation::-  Request-" + JsonConvert.SerializeObject(model) + ", Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
            }
            return new ObjectResult(createReservationResponse);
        }


        [Route("testgooglecalendar")]
        [HttpGet]
        public async Task<IActionResult> TestGoogleCalendar(int rsvpId)
        {
            var createReservationResponse = new BaseResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(rsvpId);
                GoogleCalendar.CalendarAddEventV2(reservationDetailModel);
            }
            catch { }
            return new ObjectResult(createReservationResponse);
        }


        [Route("testAzureSericequeue")]
        [HttpGet]
        public async Task<IActionResult> TestAzureServiceBusQueue(int rsvpId)
        {
            var createReservationResponse = new BaseResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(rsvpId);
                ServiceBusQueueService getStarted = new ServiceBusQueueService();

                var queueModel = new EmailQueue();
                queueModel.EType = (int)EmailType.RsvpTicketSalesConfirmation;
                queueModel.BCode = reservationDetailModel.reservation_id.ToString();
                queueModel.UId = reservationDetailModel.user_detail.user_id;
                queueModel.RsvpId = reservationDetailModel.member_id;
                queueModel.PerMsg = "";
                queueModel.Src = reservationDetailModel.referral_type;
                var qData = JsonConvert.SerializeObject(queueModel);

                AppSettings _appsettings = _appSetting.Value;
                await getStarted.SendMessage(_appsettings, qData);
            }
            catch { }
            return new ObjectResult(createReservationResponse);
        }
        private async Task UpsertOrderTobLoyal(int member_id, CreateReservationModel createReservationModel, int reservationId)
        {

            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
            List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.bLoyal).ToList();

            if (settingsGroup != null && settingsGroup.Count > 0)
            {
                bool bloyalAPIEnabled = false;
                string bLoyalApiSyncOrderCriteria = "";
                bloyalAPIEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.bLoyalApiEnabled);
                if (bloyalAPIEnabled)
                {
                    bLoyalApiSyncOrderCriteria = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.bLoyalApiSyncOrderCriteria);
                    if (!string.IsNullOrWhiteSpace(bLoyalApiSyncOrderCriteria))
                    {
                        if ((bLoyalApiSyncOrderCriteria == "1" && createReservationModel.FeeDue != createReservationModel.AmountPaid) ||
                            (bLoyalApiSyncOrderCriteria == "3" && createReservationModel.FeeDue == createReservationModel.AmountPaid))
                        {
                            try
                            {
                                //LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                                //logDAL.InsertLog("test step 2 bLoyal", "SendReservation:  Reservation.Id-" + reservationId.ToString(), HttpContext.Request.Headers["AuthenticateKey"]);
                                QueueService getStarted = new QueueService();

                                var queueModel = new EmailQueue();
                                queueModel.EType = (int)EmailType.UploadOrderTobLoyal;
                                queueModel.BCode = createReservationModel.BookingCode;
                                queueModel.UId = createReservationModel.UserId;
                                queueModel.RsvpId = reservationId;
                                queueModel.PerMsg = "";
                                queueModel.Src = 0;
                                var qData = JsonConvert.SerializeObject(queueModel);

                                AppSettings _appsettings = _appSetting.Value;
                                getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                            }
                            catch (Exception ex)
                            {
                                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                                logDAL.InsertLog("WebApi", "SaveReservation UpsertOrderTobLoyal:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
                            }
                        }
                    }
                }

            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("taxPercentcalculation")]
        [HttpGet]
        public async Task<IActionResult> GetTaxPercentCalculation(int event_id)
        {
            var taxCalculationResponse = new TaxCalculationResponse();
            try
            {
                decimal taxAmount = 0;
                Utility objUtility = new Utility();
                taxAmount = await objUtility.GetTax(event_id, 1, 100);

                var model = new TaxCalculationModel
                {
                    subtotal = 100,
                    sales_tax = taxAmount,
                    discount = 0,
                    balance_due = 100 + taxAmount,
                    addon_total = 0,
                    previous_deposit = 0
                };
                taxCalculationResponse.success = true;
                taxCalculationResponse.data = model;
            }
            catch (Exception ex)
            {
                taxCalculationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                taxCalculationResponse.error_info.extra_info = Common.Common.InternalServerError;
                taxCalculationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTaxPercentCalculation:- EventId-" + event_id.ToString() + ", Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(taxCalculationResponse);
        }

        public static bool checkChargeCC(int ChargeFee, DateTime starttime, Times.TimeZone timeZone = Times.TimeZone.PacificTimeZone)
        {
            bool ret = false;
            DateTime currLocalTime = Times.ToTimeZoneTime(DateTime.UtcNow, timeZone);
            double datediff = (starttime - currLocalTime).TotalHours;
            //Upon Booking
            if (ChargeFee == 4 || ChargeFee == 10 || ChargeFee == 11 || ChargeFee == 12)
            {
                ret = true;
                //24 Hours Prior
            }
            else if (ChargeFee == 5 && datediff < 24)
            {
                ret = true;
                //48 Hours Prior
            }
            else if (ChargeFee == 6 && datediff < 48)
            {
                ret = true;
                //Charge on Arrival Date (AM)
            }
            else if (ChargeFee == 9 && starttime.Date == currLocalTime.Date)
            {
                ret = true;
            }
            return ret;
        }

        ///// <summary>
        ///// This method is used to update reservation status
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        [Route("setreservationstatus")]
        [HttpPost]
        public async Task<IActionResult> SetReservationStatus([FromBody]ReservationStatusRequest model)
        {
            int member_id = 0;
            var reservationStatusResponse = new ReservationStatusResponse();
            try
            {
                bool ret = false;
                bool refundAttempted = false, refundSuccess = false;
                AuthMessageSender messageService = new AuthMessageSender();
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                bool IsConcierge = false;
                var rsvp = eventDAL.GetReservationDetailsbyReservationId(model.reservation_id, "", IsAdmin, "", false, false, false);
                string currentuser = HttpContext.Request.Headers["AuthenticateKey"];

                if (!string.IsNullOrEmpty(model.modified_by_name))
                {
                    currentuser = model.modified_by_name;
                }

                if (rsvp != null && rsvp.reservation_id > 0)
                {
                    if (rsvp.referral_id > 0)
                        IsConcierge = rsvp.referral_id == model.modified_by_id;

                    member_id = rsvp.member_id;
                    if (model.status == (int)ReservationStatus.Pending)
                    {
                        ret = eventDAL.SetReservationV2Status(rsvp, model.status, currentuser, rsvp.member_id);
                    }
                    else if (model.status == (int)ReservationStatus.Completed)
                    {
                        ret = eventDAL.SetReservationV2Status(rsvp, model.status, currentuser, rsvp.member_id);
                        //Send Check In Promo Where Applicable
                        var emailPromo = eventDAL.SendCheckInPromo(model.reservation_id, 0);

                        try
                        {
                            if (emailPromo.ToEmail.Trim().Length > 0)
                            {
                                emailPromo.MailConfig.ApiKey = _appSetting.Value.MainGunApiKey;
                                emailPromo.MailConfig.Domain = _appSetting.Value.MailGunPostUrl;
                                messageService.ProcessCheckInPromoEmail(emailPromo, model.reservation_id);
                            }
                        }
                        catch { }

                    }
                    else if (model.status == (int)ReservationStatus.NoShow)
                    {
                        ret = eventDAL.SetReservationV2Status(rsvp, model.status, currentuser, rsvp.member_id);
                        QueueService getStarted = new QueueService();

                        var queueModel = new EmailQueue();
                        queueModel.EType = (int)EmailType.NoShowNotice;
                        queueModel.RsvpId = model.reservation_id;
                        queueModel.Src = (int)ActionSource.BackOffice;
                        queueModel.GuestEmail = model.send_mail;
                        var qData = JsonConvert.SerializeObject(queueModel);

                        AppSettings _appsettings = _appSetting.Value;
                        getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                    }
                    else if (model.status == (int)ReservationStatus.GuestDelayed)
                    {
                        ret = eventDAL.SetReservationV2Status(rsvp, model.status, currentuser, rsvp.member_id, model.delay_in_minutes);
                    }
                    else if (model.status == (int)ReservationStatus.Cancelled)
                    {
                        ret = eventDAL.SetReservationV2Status(rsvp, model.status, currentuser, rsvp.member_id, 0, IsAdmin, IsConcierge);
                        if (!ret && !IsAdmin)
                        {
                            reservationStatusResponse.success = false;
                            reservationStatusResponse.error_info = new ErrorInfo
                            {
                                description = "Cancel lead time has passed, reservation cannot be cancelled now.",
                                error_type = (int)Common.Common.ErrorType.CancelLeadTimeError
                            };
                            return new JsonResult(reservationStatusResponse);
                        }

                        try
                        {
                            ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfoByReservationId(model.reservation_id);
                            if (zoomMeetingInfo != null && zoomMeetingInfo.MeetingId > 0)
                            {
                                await DeleteMeeting(zoomMeetingInfo.MeetingBehavior, zoomMeetingInfo.MeetingId, rsvp.member_id, model.reservation_id, 2, zoomMeetingInfo.RegistrantId, rsvp.user_detail.email, HttpContext.Request.Headers["AuthenticateKey"]);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                            logDAL.InsertLog("WebApi", "DeleteMeeting api:  " + ex.Message.ToString() + ",reservation_id:-" + model.reservation_id.ToString(), "", 1, member_id);
                        }

                        if (ret)
                        {
                            eventDAL.RemovePassportRsvpClaim(model.reservation_id);
                            decimal RefundAmount = 0;
                            if (model.refund_deposit)
                            {
                                if (rsvp != null)
                                {
                                    if (rsvp.amount_paid > 0)
                                    {
                                        refundAttempted = true;
                                    }
                                }
                                if (rsvp != null)
                                {
                                    int modified_by_id = 0;
                                    if (model.modified_by_id > 0)
                                        modified_by_id = model.modified_by_id;
                                    else
                                        modified_by_id = rsvp.user_detail.user_id;
                                    Services.Payments objPayments = new Services.Payments(_appSetting);

                                    if (rsvp.amount_paid > 0)
                                    {
                                        refundAttempted = true;

                                        Common.Payments.TransactionResult refundResult = new Common.Payments.TransactionResult();

                                        refundResult = await objPayments.RefundReservation(rsvp, rsvp.amount_paid, modified_by_id);

                                        if (refundResult.Status == Common.Payments.TransactionResult.StatusType.Success)
                                        {
                                            RefundAmount = rsvp.amount_paid;
                                            refundSuccess = true;
                                            rsvp.amount_paid = 0;
                                            Services.Payments.UpdateReservation(rsvp);
                                        }
                                    }
                                    else
                                    {
                                        bool enablePreAuth = false;
                                        try
                                        {
                                            enablePreAuth = eventDAL.CheckPreAuthEnabledForWinery(rsvp.member_id);
                                        }
                                        catch { }
                                        if (enablePreAuth)
                                        {
                                            var authPayment = eventDAL.GetReservationPreAuths(rsvp.reservation_id);
                                            if (authPayment != null && authPayment.Count > 0)
                                            {
                                                decimal amtAuthorized = authPayment[0].amount;
                                                Common.Payments.TransactionResult refundResult = new Common.Payments.TransactionResult();
                                                refundResult = await objPayments.VoidReservationPreAuth(authPayment[0], rsvp, amtAuthorized, modified_by_id);
                                                if (refundResult.Status == Common.Payments.TransactionResult.StatusType.Success)
                                                {
                                                    RefundAmount = 0;
                                                    refundSuccess = true;
                                                    rsvp.amount_paid = 0;
                                                    Services.Payments.UpdateReservation(rsvp);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            QueueService getStarted = new QueueService();

                            var queueModel = new EmailQueue();
                            queueModel.EType = (int)EmailType.RsvpCancel;
                            queueModel.RsvpId = model.reservation_id;
                            queueModel.Src = (int)ActionSource.BackOffice;
                            queueModel.Ramt = RefundAmount;
                            queueModel.GuestEmail = model.send_mail;
                            var qData = JsonConvert.SerializeObject(queueModel);

                            AppSettings _appsettings = _appSetting.Value;
                            getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();

                            rsvp.status = (int)ReservationStatus.Cancelled;

                            if (!string.IsNullOrEmpty(model.cancellation_reason))
                            {
                                bool updateCancelReason = eventDAL.UpdateCancellationReason(rsvp.reservation_id, model.cancellation_reason);
                            }
                        }
                    }
                    if (ret)
                    {
                        int floorPlanId = rsvp.floor_plan_id;

                        reservationStatusResponse.data.reservation_id = model.reservation_id;
                        reservationStatusResponse.data.refund_attempted = refundAttempted;
                        reservationStatusResponse.data.refund_success = refundSuccess;
                        var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = model.reservation_id;
                        model1.item_type = (int)ItemType.Reservations;
                        model1.location_id = rsvp.location_id;
                        model1.member_id = rsvp.member_id;
                        model1.action_date = rsvp.event_start_date;
                        model1.floor_plan_id = floorPlanId;
                        notificationDAL.SaveDelta(model1);
                    }
                    reservationStatusResponse.success = true;
                }
                else
                {
                    reservationStatusResponse.success = false;
                }

            }
            catch (Exception ex)
            {
                reservationStatusResponse.success = false;

                if (ex.Message.IndexOf("IX_UC_ReservationV2") > -1)
                {
                    reservationStatusResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                    reservationStatusResponse.error_info.extra_info = "Guest Booking Warning";
                    reservationStatusResponse.error_info.description = "Duplicate reservation error. There is already a reservation created with exact same information.";
                }
                else
                {
                    reservationStatusResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                    reservationStatusResponse.error_info.extra_info = Common.Common.InternalServerError;
                    reservationStatusResponse.error_info.description = ex.Message.ToString();
                }

                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SetReservationStatus:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(reservationStatusResponse);
        }

        [Route("sendcheckinpromo")]
        [HttpPost]
        public async Task<IActionResult> SendCheckInPromo([FromBody]SendCheckInPromoRequest model)
        {
            AuthMessageSender messageService = new AuthMessageSender();
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            var resp = new BaseResponse();

            //Send Check In Promo Where Applicable
            var emailPromo = eventDAL.SendCheckInPromo(model.id, model.type);

            try
            {
                if (emailPromo.ToEmail.Trim().Length > 0)
                {
                    emailPromo.MailConfig.ApiKey = _appSetting.Value.MainGunApiKey;
                    emailPromo.MailConfig.Domain = _appSetting.Value.MailGunPostUrl;
                    messageService.ProcessCheckInPromoEmail(emailPromo, model.id);
                }

                resp.success = true;
            }
            catch { }

            return new JsonResult(resp);
        }

        [Route("updatereservationnotes")]
        [HttpPost]
        public async Task<IActionResult> UpdateReservationNotes([FromBody]ReservationNotesRequest model)
        {
            int member_id = 0;
            var reservationNotesResponse = new ReservationNotesResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                bool ret = eventDAL.UpdateReservationNotes(model.reservation_id, model.concierge_note, model.internal_note, model.guest_note);

                if (ret)
                {
                    reservationNotesResponse.data.reservation_id = model.reservation_id;
                    bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                    ReservationDetailModel reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(model.reservation_id, "", IsAdmin);
                    member_id = reservationDetailModel.member_id;
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var model1 = new CreateDeltaRequest();
                    model1.item_id = model.reservation_id;
                    model1.item_type = (int)ItemType.Reservations;
                    model1.location_id = reservationDetailModel.location_id;
                    model1.member_id = reservationDetailModel.member_id;
                    model1.action_date = reservationDetailModel.event_start_date;
                    //check if inventory mode v3 is on then add a change for floorplan
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    int invMode = userDAL.GetInventoryModeForMember(reservationDetailModel.member_id);
                    if (invMode == 1)
                    {
                        model1.floor_plan_id = reservationDetailModel.floor_plan_id;
                    }
                    notificationDAL.SaveDelta(model1);

                    reservationNotesResponse.success = true;
                }
                else
                    reservationNotesResponse.data.reservation_id = 0;
            }
            catch (Exception ex)
            {
                reservationNotesResponse.success = false;
                reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationNotesResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationNotesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateReservationNotes:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(reservationNotesResponse);
        }

        [Route("updatereservationnotesv2")]
        [HttpPost]
        public async Task<IActionResult> UpdateReservationNotesv2([FromBody]ReservationNotesRequestv2 model)
        {
            int member_id = 0;
            var reservationNotesResponse = new ReservationNotesResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                bool ret = eventDAL.UpdateReservationNotes(model.reservation_id, model.concierge_note, model.internal_note, model.guest_note);
                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                var rsvp = eventDAL.GetReservationDetailsbyReservationId(model.reservation_id, "", IsAdmin);

                member_id = rsvp.member_id;
                if (model.account_note_has_changes)
                {
                    if (rsvp != null && rsvp.reservation_id > 0 && (!string.IsNullOrEmpty(model.account_note)))
                    {
                        UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                        userDAL.UpdateUserAccountNotes(rsvp.user_detail.user_id, rsvp.member_id, model.account_note, HttpContext.Request.Headers["AuthenticateKey"]);
                    }
                }

                if (ret)
                {
                    reservationNotesResponse.data.reservation_id = model.reservation_id;

                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var model1 = new CreateDeltaRequest();
                    model1.item_id = model.reservation_id;
                    model1.item_type = (int)ItemType.Reservations;
                    model1.location_id = rsvp.location_id;
                    model1.member_id = rsvp.member_id;
                    model1.action_date = rsvp.event_start_date;
                    model1.floor_plan_id = rsvp.floor_plan_id;
                    notificationDAL.SaveDelta(model1);

                    reservationNotesResponse.success = true;
                }
                else
                    reservationNotesResponse.data.reservation_id = 0;
            }
            catch (Exception ex)
            {
                reservationNotesResponse.success = false;
                reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationNotesResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationNotesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateReservationNotes:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(reservationNotesResponse);
        }

        //This one only works for reservations
        [Route("updatepreassignedservertable")]
        [HttpPost]
        public async Task<IActionResult> UpdatePreAssignedServerTable([FromBody]PreAssignedServerTableRequest model)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            //string rawPostData = await getRawPostData();
            //string debugData = "";
            //if (!string.IsNullOrWhiteSpace(rawPostData))
            //    debugData = " Raw Data:" + rawPostData;

            //logDAL.InsertLog("WebApi", "UpdatePreAssignedServerTable." + debugData, HttpContext.Request.Headers["AuthenticateKey"], 3, 0);

            //logDAL.InsertLog("WebApi", "UpdatePreAssignedServerTable:" + JsonConvert.SerializeObject(model), HttpContext.Request.Headers["AuthenticateKey"], 3, -1);

            int member_id = 0;
            var reservationNotesResponse = new ReservationNotesResponse();
            try
            {
                bool ret = false;
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                if (model.pre_assign_server_id != null || model.pre_assign_table_ids != null)
                {
                    string PreAssign_Table_Id = JsonConvert.SerializeObject(model.pre_assign_table_ids);
                    ret = eventDAL.UpdatePreAssignedServerTable(model.reservation_id, model.pre_assign_server_id, PreAssign_Table_Id);
                }

                if (ret)
                {
                    bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                    ReservationDetailModel reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(model.reservation_id, "", IsAdmin);
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);

                    member_id = reservationDetailModel.member_id;
                    var model1 = new CreateDeltaRequest();
                    model1.item_id = model.reservation_id;
                    model1.item_type = (int)ItemType.Reservations;
                    model1.location_id = reservationDetailModel.location_id;
                    model1.member_id = reservationDetailModel.member_id;
                    model1.action_date = reservationDetailModel.event_start_date;
                    //check if inventory mode v3 is on then add a change for floorplan
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    int invMode = userDAL.GetInventoryModeForMember(reservationDetailModel.member_id);
                    if (invMode == 1)
                    {
                        model1.floor_plan_id = reservationDetailModel.floor_plan_id;
                    }
                    notificationDAL.SaveDelta(model1);

                    reservationNotesResponse.data.reservation_id = model.reservation_id;
                    reservationNotesResponse.success = true;
                }
                else
                    reservationNotesResponse.data.reservation_id = 0;
            }
            catch (Exception ex)
            {
                reservationNotesResponse.success = false;
                reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationNotesResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationNotesResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "UpdatePreAssignedServerTable:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(reservationNotesResponse);
        }

        //works for both waitlist and reservation table
        //works for both waitlist and reservation table
        [Route("updatepreassignedservertableall")]
        [HttpPost]
        public async Task<IActionResult> updatepreassignedservertableall([FromBody]PreAssignedServerTableRequestAll model)
        {
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            //string rawPostData = await getRawPostData();
            //string debugData = "";
            //if (!string.IsNullOrWhiteSpace(rawPostData))
            //    debugData = " Raw Data:" + rawPostData;

            //logDAL.InsertLog("WebApi", "updatepreassignedservertableall." + debugData, HttpContext.Request.Headers["AuthenticateKey"], 3, 0);

            logDAL.InsertLog("WebApi", "updatepreassignedservertableall:" + JsonConvert.SerializeObject(model), HttpContext.Request.Headers["AuthenticateKey"], 3, -1);

            int member_id = 0;
            var reservationNotesResponse = new PreAssignTableResponse();
            try
            {
                bool ret = false;
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                //if (model.pre_assign_server_id != null || model.pre_assign_table_ids != null)
                //{
                if (model.pre_assign_server_id == null && model.pre_assign_table_ids == null && model.transaction_type == PreAssignServerTransactionType.Reservation)
                {
                    reservationNotesResponse.success = false;
                    reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationSavingError;
                    reservationNotesResponse.error_info.extra_info = "Either preassigned server or preassigned table value is required for reservation";
                    reservationNotesResponse.error_info.description = "There was problem processing your request. Kindly retry again or contact support.";
                    return new ObjectResult(reservationNotesResponse);
                }
                string PreAssign_Table_Id = "";
                if (model.pre_assign_table_ids != null)
                {
                    PreAssign_Table_Id = JsonConvert.SerializeObject(model.pre_assign_table_ids);
                    foreach (var item in model.pre_assign_table_ids)
                    {
                        //do a check if the table is alreadty blocked
                        TableBlockedStatusModel blockedStatus = eventDAL.CheckTableBlockedStatusByTranId(item, model.transaction_id, model.transaction_type);
                        if (blockedStatus.Id > 0)
                        {
                            string TableName = eventDAL.GetTableNameById(item);
                            reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableBlocked;
                            reservationNotesResponse.error_info.extra_info = string.Format("Table {0} is blocked between {1} and {2}", TableName, blockedStatus.BlockStartDate.Value.ToString("hh:mm tt"), blockedStatus.BlockEndDate.Value.ToString("hh:mm tt"));
                            reservationNotesResponse.success = false;
                            return new ObjectResult(reservationNotesResponse);
                        }

                        //do a check if the table is alreadty preassigned to some other 
                        //TablePreassigendStatusModel preAssignStatus = eventDAL.CheckIfTablePreassignedAlready(item, model.transaction_id, model.transaction_type);
                        //if (preAssignStatus.ReservationCount > 0 || preAssignStatus.WaitlistCount > 0)
                        //{
                        //    string TableName = eventDAL.GetTableNameById(item);
                        //    reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                        //    reservationNotesResponse.error_info.extra_info = string.Format("Table {0} is already pre-assigned to a party", TableName);
                        //    reservationNotesResponse.success = false;
                        //    return new ObjectResult(reservationNotesResponse);

                        //}

                        if (!string.IsNullOrWhiteSpace(PreAssign_Table_Id))
                        {
                            eventDAL.SwappingPreAssignTableIdByTranId(item, model.transaction_id, (int)model.transaction_type, model.force_assign, PreAssign_Table_Id);
                        }
                    }
                }

                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                ReservationDetailModel reservationDetailModel = new ReservationDetailModel();

                if (model.transaction_type == PreAssignServerTransactionType.Reservation)
                {
                    reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(model.transaction_id, "", IsAdmin);
                    //if (model.pre_assign_table_ids != null)
                    //{
                    //    foreach (var item in model.pre_assign_table_ids)
                    //    {
                    //        if (eventDAL.CheckTableAvailableById(item, reservationDetailModel.event_start_date, reservationDetailModel.event_end_date, JsonConvert.SerializeObject(reservationDetailModel.pre_assign_table_ids)) == false)
                    //        {
                    //            reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                    //            reservationNotesResponse.error_info.extra_info = "The destination table(s) have been manually assigned and cannot be swapped.";

                    //            return new ObjectResult(reservationNotesResponse);
                    //        }
                    //    }
                    //}

                    ret = eventDAL.UpdatePreAssignedServerTable(model.transaction_id, model.pre_assign_server_id, PreAssign_Table_Id);
                }
                else
                {
                    if (model.pre_assign_table_ids != null)
                    {
                        WaitlistModel waitlistModel = eventDAL.GetWaitlistById(model.transaction_id);

                        int OffsetMinutes = Convert.ToInt32(Common.Times.GetOffsetMinutes((Common.Times.TimeZone)eventDAL.GetTimeZonebyWineryId(waitlistModel.MemberId)));

                        foreach (var item in model.pre_assign_table_ids)
                        {
                            if (eventDAL.CheckTableAvailableById(item, waitlistModel.WaitStartDateTime.AddMinutes(OffsetMinutes), waitlistModel.WaitEndDateTime.AddMinutes(OffsetMinutes), waitlistModel.PreAssign_Table_Id) == false)
                            {
                                reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                                reservationNotesResponse.error_info.extra_info = "The destination table(s) have been manually assigned and cannot be swapped.";

                                return new ObjectResult(reservationNotesResponse);
                            }
                        }
                    }
                        
                    ret = eventDAL.UpdateWaitlistPreAssignedServerTable(model.transaction_id, model.duration_in_minutes, model.pre_assign_server_id ?? -1, PreAssign_Table_Id);
                }

                //}

                if (ret)
                {
                    if (model.transaction_type == PreAssignServerTransactionType.Reservation)
                    {
                        member_id = reservationDetailModel.member_id;
                        var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.Reservations;
                        model1.location_id = reservationDetailModel.location_id;
                        model1.member_id = reservationDetailModel.member_id;
                        model1.action_date = reservationDetailModel.event_start_date;
                        //check if inventory mode v3 is on then add a change for floorplan
                        UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                        int invMode = userDAL.GetInventoryModeForMember(reservationDetailModel.member_id);
                        if (invMode == 1)
                        {
                            model1.floor_plan_id = reservationDetailModel.floor_plan_id;
                        }
                        notificationDAL.SaveDelta(model1);
                    }
                    else
                    {
                        var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                        WaitlistModel waitlistModel = new WaitlistModel();
                        waitlistModel = eventDAL.GetWaitlistById(model.transaction_id);
                        member_id = waitlistModel.MemberId;
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.Waitlists;
                        model1.location_id = waitlistModel.LocationId;
                        model1.member_id = waitlistModel.MemberId;
                        model1.action_date = waitlistModel.WaitStartDateTime;
                        UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                        int invMode = userDAL.GetInventoryModeForMember(waitlistModel.MemberId);

                        if (invMode == 1)
                        {
                            model1.floor_plan_id = waitlistModel.AssignedFloorPlanId > 0 ? waitlistModel.AssignedFloorPlanId : waitlistModel.FloorPlanId;
                        }
                        notificationDAL.SaveDelta(model1);
                    }
                }

                reservationNotesResponse.success = true;
                reservationNotesResponse.data.transaction_id = model.transaction_id;

            }
            catch (Exception ex)
            {
                reservationNotesResponse.success = false;

                reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationNotesResponse.error_info.description = ex.Message.ToString();

                if (ex.Message.ToString().IndexOf("hard assigned") > -1 || ex.Message.ToString().IndexOf("overbooking") > -1)
                {
                    reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.NoTablesAvailable;
                    reservationNotesResponse.error_info.extra_info = ex.Message.ToString();
                }
                else if (ex.Message.ToString().IndexOf("hard-assigned") > -1)
                {
                    reservationNotesResponse.error_info.error_type = (int)Common.Common.ErrorType.NoTablesAvailable;
                    reservationNotesResponse.error_info.extra_info = ex.Message.ToString();
                }
                else
                {
                    reservationNotesResponse.error_info.extra_info = Common.Common.InternalServerError;

                    logDAL.InsertLog("WebApi", "updatepreassignedservertableall: " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
                }
            }
            return new JsonResult(reservationNotesResponse);
        }


        [Route("reassigntables")]
        [HttpPost]
        public async Task<IActionResult> ReassignTables([FromBody]ReAssignedTableRequest model)
        {
            int member_id = 0;
            var reassignResponse = new ReassignTableResponse();
            try
            {
                bool ret = false;
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                //if (model.pre_assign_server_id != null || model.pre_assign_table_ids != null)
                //{
                if (model.transaction_id <= 0)
                {
                    reassignResponse.success = false;
                    reassignResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    reassignResponse.error_info.extra_info = "Transaction Id is required";
                    reassignResponse.error_info.description = "Transaction Id is required.";
                    return new ObjectResult(reassignResponse);
                }
                if (model.table_ids == null || model.table_ids.Count == 0)
                {
                    reassignResponse.success = false;
                    reassignResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    reassignResponse.error_info.extra_info = "Atleast one table is required for a seated session.";
                    reassignResponse.error_info.description = "Atleast one table is required for a seated session.";
                    return new ObjectResult(reassignResponse);

                }
                string tableIds = "";
                tableIds = String.Join(",", model.table_ids);

                ret = eventDAL.ReassignTable(model.transaction_id, model.transaction_type, tableIds);

                //}
                int floorplanId = 0;
                int invMode = 0;
                int locationId = 0;
                var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                if (model.transaction_type == PreAssignServerTransactionType.Reservation)
                {
                    bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                    ReservationDetailModel reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(model.transaction_id, "", IsAdmin);
                    member_id = reservationDetailModel.member_id;
                    var model1 = new CreateDeltaRequest();
                    model1.item_id = model.transaction_id;
                    model1.item_type = (int)ItemType.Reservations;
                    locationId = model1.location_id = reservationDetailModel.location_id;
                    model1.member_id = reservationDetailModel.member_id;
                    model1.action_date = reservationDetailModel.event_start_date;
                    //check if inventory mode v3 is on then add a change for floorplan
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    invMode = userDAL.GetInventoryModeForMember(reservationDetailModel.member_id);
                    if (invMode == 1)
                    {
                        floorplanId = reservationDetailModel.assigned_floor_plan_id > 0 ? reservationDetailModel.assigned_floor_plan_id : reservationDetailModel.floor_plan_id;
                        model1.floor_plan_id = floorplanId;
                    }

                    notificationDAL.SaveDelta(model1);

                    model1 = new CreateDeltaRequest();
                    model1.item_id = model.transaction_id;
                    model1.item_type = (int)ItemType.TableStatus;
                    model1.location_id = reservationDetailModel.location_id;
                    model1.member_id = reservationDetailModel.member_id;
                    if (invMode == 1)
                    {
                        model1.floor_plan_id = floorplanId;
                    }
                    model1.action_date = DateTime.UtcNow;
                    notificationDAL.SaveDelta(model1);

                }
                else
                {
                    WaitlistModel waitlistModel = new WaitlistModel();
                    waitlistModel = eventDAL.GetWaitlistById(model.transaction_id);
                    member_id = waitlistModel.MemberId;
                    var model1 = new CreateDeltaRequest();
                    model1.item_id = model.transaction_id;
                    model1.item_type = (int)ItemType.Waitlists;
                    locationId = model1.location_id = waitlistModel.LocationId;
                    model1.member_id = waitlistModel.MemberId;
                    model1.action_date = waitlistModel.WaitStartDateTime;
                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    invMode = userDAL.GetInventoryModeForMember(waitlistModel.MemberId);

                    if (invMode == 1)
                    {
                        floorplanId = waitlistModel.AssignedFloorPlanId > 0 ? waitlistModel.AssignedFloorPlanId : waitlistModel.FloorPlanId;
                        model1.floor_plan_id = floorplanId;
                    }
                    notificationDAL.SaveDelta(model1);

                    model1 = new CreateDeltaRequest();
                    model1.item_id = model.transaction_id;
                    model1.item_type = (int)ItemType.TableStatus;
                    model1.location_id = waitlistModel.LocationId;
                    model1.member_id = waitlistModel.MemberId;
                    model1.action_date = DateTime.UtcNow;
                    if (invMode == 1)
                    {
                        model1.floor_plan_id = floorplanId;
                    }
                    notificationDAL.SaveDelta(model1);
                }

                if (ret)
                {
                    var model11 = new CreateDeltaRequest();
                    model11.item_id = model.transaction_id;
                    model11.item_type = (int)ItemType.Servers;
                    model11.location_id = locationId;  //TODO
                    model11.member_id = 0;
                    model11.action_date = DateTime.UtcNow;
                    if (invMode == 1)
                    {
                        model11.floor_plan_id = floorplanId;
                    }
                    notificationDAL.SaveDelta(model11);
                }
                reassignResponse.success = true;
                reassignResponse.data.transaction_id = model.transaction_id;
            }
            catch (Exception ex)
            {
                reassignResponse.success = false;
                reassignResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reassignResponse.error_info.extra_info = Common.Common.InternalServerError;
                reassignResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "reassigntables:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(reassignResponse);
        }


        [Route("updatepartysize")]
        [HttpPost]
        public async Task<IActionResult> UpdatePartySize([FromBody]UpdatePartySizeRequest modelRequest)
        {
            var createReservationResponse = new CreateReservationResponse();
            createReservationResponse = await UpdateReservationPartySize(modelRequest);
            return new ObjectResult(createReservationResponse);
        }

        private async Task<CreateReservationResponse> UpdateReservationPartySize(UpdatePartySizeRequest modelRequest)
        {
            var createReservationResponse = new CreateReservationResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            logDAL.InsertLog("WebApi", "UpdateReservationPartySize:  request data:" + JsonConvert.SerializeObject(modelRequest), HttpContext.Request.Headers["AuthenticateKey"], 3);

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
            bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
            ReservationDetailModel reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(modelRequest.reservation_id, "", IsAdmin);
            CreateReservationModel createReservationModel = new CreateReservationModel();
            int member_id = 0;
            try
            {
                member_id = reservationDetailModel.member_id;
                createReservationModel.TotalGuests = modelRequest.party_size;
                createReservationModel.ReferralType = reservationDetailModel.referral_type;

                createReservationModel.BookingCode = reservationDetailModel.booking_code;

                createReservationModel.MobilePhoneStatus = (int)reservationDetailModel.user_detail.mobile_phone_status;
                createReservationModel.EventId = reservationDetailModel.event_id ?? 0;
                createReservationModel.SlotId = reservationDetailModel.slot_id;
                createReservationModel.SlotType = reservationDetailModel.slot_type;

                createReservationModel.Email = reservationDetailModel.user_detail.email;
                createReservationModel.Country = reservationDetailModel.user_detail.address.country;
                createReservationModel.Zip = reservationDetailModel.user_detail.address.zip_code;
                createReservationModel.CustomerType = reservationDetailModel.user_detail.customer_type;
                createReservationModel.UserId = reservationDetailModel.user_detail.user_id;
                createReservationModel.FirstName = reservationDetailModel.user_detail.first_name;
                createReservationModel.LastName = reservationDetailModel.user_detail.last_name;
                createReservationModel.PhoneNumber = reservationDetailModel.user_detail.phone_number;
                createReservationModel.City = reservationDetailModel.user_detail.address.city;
                createReservationModel.State = reservationDetailModel.user_detail.address.state;
                createReservationModel.WineryId = reservationDetailModel.member_id;
                createReservationModel.ContactTypes = reservationDetailModel.contact_types;

                createReservationModel.PreAssign_Server_Id = reservationDetailModel.pre_assign_server_id ?? 0;
                createReservationModel.PreAssign_Table_Id = JsonConvert.SerializeObject(reservationDetailModel.pre_assign_table_ids);

                createReservationModel.Email = reservationDetailModel.user_detail.email;

                createReservationModel.ChargeFee = reservationDetailModel.charge_fee;

                createReservationModel.PayCardNumber = reservationDetailModel.pay_card.number;
                createReservationModel.PayCardCustName = reservationDetailModel.pay_card.cust_name;
                createReservationModel.PayCardExpYear = reservationDetailModel.pay_card.exp_year;
                createReservationModel.PayCardExpMonth = reservationDetailModel.pay_card.exp_month;

                if (requiresCreditCard(createReservationModel.ChargeFee))
                {
                    createReservationModel.RequireCreditCard = true;
                }

                createReservationModel.EventDate = reservationDetailModel.event_start_date.Date;
                createReservationModel.StartTime = new TimeSpan(reservationDetailModel.event_start_date.TimeOfDay.Ticks);
                createReservationModel.EndTime = new TimeSpan(reservationDetailModel.event_end_date.TimeOfDay.Ticks);

                createReservationModel.FeePerPerson = reservationDetailModel.fee_per_person;
                createReservationModel.Status = reservationDetailModel.status;

                createReservationModel.FeeType = reservationDetailModel.fee_type;

                decimal rsvpTotal = 0;

                if (reservationDetailModel.fee_type == 1)
                    rsvpTotal = createReservationModel.FeePerPerson;
                else
                    rsvpTotal = createReservationModel.FeePerPerson * modelRequest.party_size;

                createReservationModel.EventName = reservationDetailModel.event_name;
                createReservationModel.EventLocation = reservationDetailModel.location_name;
                createReservationModel.LocationId = reservationDetailModel.location_id;

                createReservationModel.AffiliateID = reservationDetailModel.affiliate_id;
                createReservationModel.HDYH = reservationDetailModel.hdyh;
                createReservationModel.PayType = 1;
                createReservationModel.BookedById = reservationDetailModel.booked_by_id ?? 0;
                createReservationModel.BookedByName = reservationDetailModel.booked_by_name;
                createReservationModel.ReservationId = reservationDetailModel.reservation_id;

                createReservationModel.Note = reservationDetailModel.guest_note;
                createReservationModel.InternalNote = reservationDetailModel.internal_note;
                createReservationModel.ConciergeNote = reservationDetailModel.concierge_note;
                createReservationModel.MobilePhone = reservationDetailModel.user_detail.mobile_phone;

                createReservationModel.DiscountId = 0;
                createReservationModel.DiscountCode = reservationDetailModel.discount_code;
                createReservationModel.DiscountCodeAmount = 0;
                createReservationModel.DiscountAmt = reservationDetailModel.discount_amount;
                createReservationModel.DiscountDesc = "";

                decimal addOnTotal = 0;
                decimal addOnTaxableTotal = 0;
                foreach (var item in reservationDetailModel.reservation_addon)
                {
                    addOnTotal += item.price * item.qty;
                }

                string cardNumber = StringHelpers.Decryption(reservationDetailModel.pay_card.number);
                createReservationModel.PayCardType = reservationDetailModel.pay_card.card_type;
                createReservationModel.WaiveFee = false;
                createReservationModel.ReferralID = reservationDetailModel.referral_id;
                createReservationModel.CompletedGuestCount = 0;
                createReservationModel.ReturningGuest = true;
                createReservationModel.EmailContentID = reservationDetailModel.email_content_id;
                createReservationModel.WineryReferralId = 0;
                createReservationModel.CreditCardReferenceNumber = "";
                createReservationModel.SalesTax = reservationDetailModel.sales_tax;
                decimal FeeDue = rsvpTotal + createReservationModel.SalesTax + addOnTotal - reservationDetailModel.discount_amount;
                if (FeeDue < 0)
                {
                    FeeDue = 0;
                }
                createReservationModel.FeeDue = FeeDue;
                createReservationModel.AmountPaid = reservationDetailModel.amount_paid;
                createReservationModel.PurchaseTotal = 0;
                createReservationModel.Tags = reservationDetailModel.tags;
                createReservationModel.TransportationId = reservationDetailModel.transportation_id;
                createReservationModel.TransportationName = reservationDetailModel.transportation_name;


                string currentuser = HttpContext.Request.Headers["AuthenticateKey"];
                CreateReservation createReservation = new CreateReservation();
                createReservation = eventDAL.SaveReservation(createReservationModel, true, true, true, reservationDetailModel.total_guests, currentuser);

                if (createReservation.Status == ResponseStatus.Success)
                {
                    string paymentMsg = string.Empty;
                    try
                    {
                        var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                        var model1 = new CreateDeltaRequest();
                        model1.item_id = modelRequest.reservation_id;
                        model1.item_type = (int)ItemType.Reservations;
                        model1.location_id = createReservationModel.LocationId;
                        model1.member_id = createReservationModel.WineryId;
                        model1.action_date = createReservationModel.EventDate;
                        //check if inventory mode v3 is on then add a change for floorplan

                        int invMode = userDAL.GetInventoryModeForMember(reservationDetailModel.member_id);
                        if (invMode == 1)
                        {
                            if (reservationDetailModel.assigned_floor_plan_id > 0)
                            {
                                model1.floor_plan_id = reservationDetailModel.assigned_floor_plan_id;
                            }
                            else
                            {
                                model1.floor_plan_id = reservationDetailModel.floor_plan_id;
                            }

                        }
                        notificationDAL.SaveDelta(model1);


                        if (modelRequest.process_payment == true)
                        {
                            bool modifyChargeNeeded = false;
                            decimal refundAmount = 0;

                            //PAYMENT - REFUND OR CHARGE?
                            decimal total = createReservationModel.FeeDue;
                            if (reservationDetailModel.charge_fee == 11)
                                total = Math.Round((total / 4), 2);
                            else if (reservationDetailModel.charge_fee == 12)
                                total = Math.Round((total / 2), 2);

                            if (total > 0 && reservationDetailModel.amount_paid < total)
                                modifyChargeNeeded = true;
                            else if (total > 0 && reservationDetailModel.amount_paid > 0 && reservationDetailModel.amount_paid > total)
                            {
                                refundAmount = (reservationDetailModel.amount_paid - total);

                                Services.Payments objPayments = new Services.Payments(_appSetting);
                                Common.Payments.TransactionResult refundResult = new Common.Payments.TransactionResult();
                                refundResult = await objPayments.RefundReservation(reservationDetailModel, refundAmount, createReservationModel.BookedById);

                                if (refundResult.Status == Common.Payments.TransactionResult.StatusType.Success)
                                {
                                    reservationDetailModel.amount_paid = reservationDetailModel.amount_paid - refundResult.Amount;
                                    Services.Payments.UpdateReservation(reservationDetailModel);
                                    paymentMsg = string.Format("The 'Credit' for {0} was processed successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", refundAmount));
                                }
                                else
                                {
                                    refundAmount = 0;
                                    paymentMsg = "Reservation completed, but we were unable to process the Credit/Void.";
                                }
                            }
                            Times.TimeZone timeZone = Times.TimeZone.PacificTimeZone;
                            if (reservationDetailModel.member_id > 0)
                            {
                                try
                                {

                                    timeZone = (Times.TimeZone)eventDAL.GetTimeZonebyWineryId(reservationDetailModel.member_id);

                                }
                                catch { timeZone = Times.TimeZone.PacificTimeZone; }
                            }
                            if (checkChargeCC(reservationDetailModel.charge_fee, reservationDetailModel.event_start_date, timeZone) && createReservationModel.FeeDue > 0 && (reservationDetailModel.amount_paid == 0 || modifyChargeNeeded || reservationDetailModel.amount_paid < createReservationModel.FeeDue))
                            {
                                Services.Payments objPayments = new Services.Payments(_appSetting);
                                Common.Payments.TransactionResult result = new Common.Payments.TransactionResult();
                                decimal amountToCharge = createReservationModel.FeeDue;

                                if (reservationDetailModel.charge_fee == 11)
                                    amountToCharge = Math.Round((amountToCharge / 4), 2);
                                else if (reservationDetailModel.charge_fee == 12)
                                    amountToCharge = Math.Round((amountToCharge / 2), 2);

                                if (modifyChargeNeeded)
                                    amountToCharge = (amountToCharge - reservationDetailModel.amount_paid);

                                if (amountToCharge > 0)
                                {
                                    CreateReservationRequest model = new CreateReservationRequest();

                                    ViewModels.UserDetailViewModel user_detail = new ViewModels.UserDetailViewModel();
                                    user_detail.first_name = createReservationModel.FirstName;
                                    user_detail.last_name = createReservationModel.LastName;
                                    user_detail.phone_number = createReservationModel.PhoneNumber;
                                    user_detail.email = createReservationModel.Email;

                                    ViewModels.UserAddress address = new ViewModels.UserAddress();
                                    address.zip_code = createReservationModel.Zip;
                                    user_detail.address = address;

                                    model.user_detail = user_detail;

                                    model.reservation_id = createReservation.Id;

                                    cardNumber = (cardNumber + "").Replace(" ", "");

                                    PayCard paycard = new PayCard();
                                    paycard.number = cardNumber;
                                    paycard.cust_name = reservationDetailModel.pay_card.cust_name;
                                    paycard.exp_month = reservationDetailModel.pay_card.exp_month;
                                    paycard.exp_year = reservationDetailModel.pay_card.exp_year;
                                    paycard.cvv2 = "";
                                    paycard.card_entry = reservationDetailModel.pay_card.card_entry;
                                    paycard.application_type = reservationDetailModel.pay_card.application_type;
                                    paycard.application_version = reservationDetailModel.pay_card.application_version;
                                    paycard.terminal_id = reservationDetailModel.pay_card.terminal_id;
                                    paycard.card_reader = reservationDetailModel.pay_card.card_reader;

                                    model.pay_card = paycard;
                                    model.member_id = reservationDetailModel.member_id;

                                    result = await objPayments.ChargeReservation(model, amountToCharge, createReservationModel.BookingCode, createReservationModel.UserId);
                                    if (result.Status == Common.Payments.TransactionResult.StatusType.Failed && reservationDetailModel.charge_fee == 10)
                                    {
                                        createReservationResponse.success = false;
                                        createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationPaymentError;
                                        createReservationResponse.error_info.extra_info = "Payment Error " + result.Detail;
                                        createReservationResponse.error_info.description = "Problem processing your payment. Please review the credit card and try again.";
                                        return createReservationResponse;
                                    }

                                    if (result.Status == Common.Payments.TransactionResult.StatusType.Success)
                                    {
                                        if (reservationDetailModel.charge_fee == 10 || createReservationModel.Status == (int)ReservationStatus.Initiated)
                                            reservationDetailModel.status = (int)ReservationStatus.Pending;

                                        if (result.Amount > 0)
                                        {
                                            //Update Amount Paid on Rsvp Object
                                            reservationDetailModel.amount_paid = reservationDetailModel.amount_paid + amountToCharge;
                                            reservationDetailModel.reservation_id = createReservation.Id;
                                            Services.Payments.UpdateReservation(reservationDetailModel);
                                        }
                                    }

                                    if (reservationDetailModel.charge_fee == 11 || reservationDetailModel.charge_fee == 12)
                                    {
                                        //If this was a 24/48 before event charge and it was charged because the booking date was already within this time then show this message
                                        paymentMsg = string.Format("Required immediate payment. {1} was processed successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", amountToCharge));
                                    }
                                    else if (reservationDetailModel.charge_fee == 11)
                                    {
                                        paymentMsg = string.Format("25% Deposit ({0}) was charged successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", amountToCharge));
                                    }
                                    else if (reservationDetailModel.charge_fee == 12)
                                    {
                                        paymentMsg = string.Format("50% Deposit ({0}) was charged successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", amountToCharge));
                                    }
                                    else
                                    {
                                        //Standard Payment charged message
                                        paymentMsg = string.Format("{0} was charged successfully.", string.Format(new CultureInfo("en-US"), "{0:C}", amountToCharge));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        createReservationResponse.success = false;
                        createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                        createReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                        createReservationResponse.error_info.description = ex.Message.ToString();
                        logDAL.InsertLog("WebApi", "SaveReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
                        return createReservationResponse;
                    }

                    //try
                    //{
                    //    QueueService getStarted = new QueueService();

                    //    var queueModel = new EmailQueue();
                    //    queueModel.EType = (int)EmailType.Rsvp;
                    //    queueModel.BCode = createReservation.BookingCode;
                    //    queueModel.UId = createReservationModel.UserId;
                    //    queueModel.RsvpId = createReservation.Id;
                    //    queueModel.PerMsg = "";
                    //    queueModel.Src = reservationDetailModel.referral_type;
                    //    var qData = JsonConvert.SerializeObject(queueModel);

                    //    AppSettings _appsettings = _appSetting.Value;
                    //    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                    //}
                    //catch (Exception ex)
                    //{
                    //    logDAL.InsertLog("WebApi", "SaveReservation Email:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"]);
                    //}

                    CreateReservationResponseModel respData = new CreateReservationResponseModel
                    {
                        booking_code = createReservation.BookingCode,
                        reservation_id = createReservation.Id,
                        save_type = createReservation.SaveType,
                        message = createReservation.Message,
                        payment_message = paymentMsg
                    };
                    createReservationResponse.data = respData;
                    createReservationResponse.success = true;

                }
                else
                {
                    createReservationResponse.success = false;
                    createReservationResponse.error_info.error_type = createReservation.error_type;
                    createReservationResponse.error_info.extra_info = createReservation.Message;
                    createReservationResponse.error_info.description = createReservation.Description;
                    return createReservationResponse;
                }
            }
            catch (Exception ex)
            {
                createReservationResponse.success = false;
                createReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                createReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                createReservationResponse.error_info.description = ex.Message.ToString();

                logDAL.InsertLog("WebApi", "SaveReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return createReservationResponse;
        }

        [Route("sendabandonedcartrsvpemail")]
        [HttpPost]
        public async Task<IActionResult> SendAbandonedCartRsvpEmail([FromBody]AbandonedCartRsvpEmailRequest model)
        {
            var abandonedCartRsvpEmailResponse = new AbandonedCartRsvpEmailResponse();
            try
            {
                foreach (var item in model.ids)
                {
                    QueueService getStarted = new QueueService();

                    var queueModel = new EmailQueue();
                    queueModel.EType = (int)EmailType.AbanondedCartRsvp;
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

                logDAL.InsertLog("WebApi", "SendAbandonedCartRsvpEmail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(abandonedCartRsvpEmailResponse);
        }

        [Route("checkandsendreservationssabandonedfornotifications")]
        [HttpGet]
        public IActionResult CheckAndSendReservationsAbandonedForNotifications()
        {
            var abandonedCartRsvpEmailResponse = new AbandonedCartRsvpEmailResponse();
            try
            {

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                List<int> ids = eventDAL.GetAvailableReservationsAbandonedForNotifications();
                if (ids != null)
                {
                    foreach (var item in ids)
                    {
                        QueueService getStarted = new QueueService();

                        var queueModel = new EmailQueue();
                        queueModel.EType = (int)EmailType.AbanondedCartRsvp;
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

                logDAL.InsertLog("WebApi", "SendAbandonedCartRsvpEmail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(abandonedCartRsvpEmailResponse);
        }

        [Route("orderportsendorder")]
        [HttpPost]
        public async Task<IActionResult> OrderPortSendOrder([FromBody]OrderPortSendOrderRequest model)
        {
            var abandonedCartRsvpEmailResponse = new BaseResponse();
            int member_id = 0;
            int successCount = 0;
            int totalCount = model.reservation_id.Count;

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                string BookingCodes = string.Empty;
                

                foreach (var item in model.reservation_id)
                {
                    member_id = eventDAL.GetWineryIdByReservationId(item);

                    PayloadOrderModel orders = new PayloadOrderModel();

                    orders = eventDAL.GetOrderPortSendOrder(item);

                    PayloadOrderModel[] payloadModelArray = new PayloadOrderModel[1];

                    List<UserDetailModel> userDetailModel = new List<UserDetailModel>();
                    userDetailModel = await Task.Run(() => Utility.GetCustomersByNameOrEmail(orders.BillingAddress.Email, model.api_key, model.api_token, model.client_id));

                    if (userDetailModel != null && userDetailModel.Count > 0)
                    {
                        orders.CustomerUuid = userDetailModel[0].membership_number;
                    }
                    else
                    {
                        PayloadModel payloadModel = new PayloadModel();
                        payloadModel.CustomerUuid = "";

                        payloadModel.BillingAddress.FirstName = orders.BillingAddress.FirstName;
                        payloadModel.BillingAddress.LastName = orders.BillingAddress.LastName;
                        payloadModel.BillingAddress.Company = !string.IsNullOrEmpty(orders.BillingAddress.Company) ? orders.BillingAddress.Company : "";
                        payloadModel.BillingAddress.Address1 = orders.BillingAddress.Address1;
                        payloadModel.BillingAddress.Address2 = !string.IsNullOrEmpty(orders.BillingAddress.Address2) ? orders.BillingAddress.Address2 : "";
                        payloadModel.BillingAddress.City = !string.IsNullOrEmpty(orders.BillingAddress.City) ? orders.BillingAddress.City : "";
                        payloadModel.BillingAddress.State = !string.IsNullOrEmpty(orders.BillingAddress.State) ? orders.BillingAddress.State : "";
                        payloadModel.BillingAddress.ZipCode = !string.IsNullOrEmpty(orders.BillingAddress.ZipCode) ? orders.BillingAddress.ZipCode : "";
                        payloadModel.BillingAddress.Country = orders.BillingAddress.Country;
                        payloadModel.BillingAddress.Email = orders.BillingAddress.Email;
                        payloadModel.BillingAddress.Phone = orders.BillingAddress.Phone;
                        string CustId = await Utility.UpsertCustomerDetails(payloadModel, model.api_key, model.api_token, model.client_id, model.member_id);

                        orders.CustomerUuid = CustId;
                    }
                    payloadModelArray[0] = orders;

                    string val = await Utility.PushOrdersToOrderPort(payloadModelArray, model.api_key, model.api_token, model.client_id, item);

                    if (!string.IsNullOrEmpty(val))
                        successCount = successCount + 1;
                }


                //BookingCodes = BookingCodes + Interaction.IIf(BookingCodes.Length == 0, "", ", ") + BookingCode;
                abandonedCartRsvpEmailResponse.error_info.description = successCount.ToString() + " orders sent successfully, " + (totalCount-successCount).ToString() + " have errors. See Error view for details.";

                abandonedCartRsvpEmailResponse.success = true;
            }
            catch (Exception ex)
            {
                abandonedCartRsvpEmailResponse.success = false;
                abandonedCartRsvpEmailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                abandonedCartRsvpEmailResponse.error_info.extra_info = Common.Common.InternalServerError;
                abandonedCartRsvpEmailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "OrderPortSendOrder:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(abandonedCartRsvpEmailResponse);
        }

        [Route("commerce7sendorder")]
        [HttpPost]
        public async Task<IActionResult> Commerce7SendOrder([FromBody]Commerce7SendOrderRequest model)
        {
            var abandonedCartRsvpEmailResponse = new BaseResponse();
            int member_id = 0;
            int successCount = 0;
            //int errorCount = 0;
            int totalCount = model.reservation_id.Count;

            if (model == null || string.IsNullOrWhiteSpace(model.pos_profile_id) || string.IsNullOrWhiteSpace(model.password)
                || string.IsNullOrWhiteSpace(model.user_name) || string.IsNullOrWhiteSpace(model.tenant_name) || model.reservation_id.Count == 0)
            {
                abandonedCartRsvpEmailResponse.success = false;
                abandonedCartRsvpEmailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                abandonedCartRsvpEmailResponse.error_info.extra_info = Common.Common.InternalServerError;
                abandonedCartRsvpEmailResponse.error_info.description = "Invalid/missing data in request. Please make sure the profileId, username, password and tenant are correctly sent.";

                return new JsonResult(abandonedCartRsvpEmailResponse);
            }

            if (string.IsNullOrWhiteSpace(model.processed_by))
                model.processed_by = "CellarPass System";

            string bookingCodes = "";
            string authKey = HttpContext.Request.Headers["AuthenticateKey"];
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                string BookingCodes = string.Empty;
                bool isUpsertFulfillmentEnabled = false;

                foreach (var item in model.reservation_id)
                {
                    member_id = eventDAL.GetWineryIdByReservationId(item);
                    isUpsertFulfillmentEnabled = eventDAL.IsUpsertFulfilmentEnableCommerce7(member_id);

                    Commerce7OrderModel orders = new Commerce7OrderModel();
                    string posProfileId = model.pos_profile_id.Trim();
                    orders = eventDAL.GetOrderDataCommerce7V2(item, isUpsertFulfillmentEnabled, ref posProfileId);
                    if (!string.IsNullOrWhiteSpace(posProfileId))
                        model.pos_profile_id = posProfileId;

                    Commerce7CustomerModel commerce7CustomerModel = new Commerce7CustomerModel();

                    commerce7CustomerModel = await Utility.CheckAndUpdateCommerce7Customer(model.user_name.Trim(), model.password.Trim(), model.tenant_name.Trim(), orders.billTo.firstName, orders.billTo.lastName, orders.billTo.company, orders.billTo.address, orders.billTo.address2, orders.billTo.city,
                                                                            orders.billTo.stateCode, orders.billTo.zipCode, orders.billTo.countryCode, orders.email.Trim(), orders.billTo.phone, "", member_id, item);

                    if (commerce7CustomerModel.Exceeded)
                        break;

                    orders.customerId = commerce7CustomerModel.CustId;

                    if (string.IsNullOrWhiteSpace(orders.billTo.address))
                    {
                        orders.billTo = null;
                    }

                    if (!string.IsNullOrEmpty(orders.customerId))
                    {
                        string result = await Utility.PushOrdersToCommerce7(orders, model.user_name.Trim(), model.password.Trim(), model.tenant_name.Trim(), item, model.processed_by, isUpsertFulfillmentEnabled);

                        if (result == "exceeded")
                        {
                            break;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(bookingCodes))
                                successCount = successCount + 1;

                            bookingCodes = bookingCodes + (bookingCodes.Length == 0 ? "" : ", ") + result;
                        }
                    }
                }

                //logDAL.InsertLog("WebApi", "Commerce7SendOrder:  Commerce7 OrderIds:" + bookingCodes, authKey);

                abandonedCartRsvpEmailResponse.success = true;
                abandonedCartRsvpEmailResponse.error_info.description = successCount.ToString() + " orders sent successfully, " + (totalCount-successCount).ToString() + " have errors. See Error view for details.";
            }
            catch (Exception ex)
            {
                abandonedCartRsvpEmailResponse.success = false;
                abandonedCartRsvpEmailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                abandonedCartRsvpEmailResponse.error_info.extra_info = Common.Common.InternalServerError;
                abandonedCartRsvpEmailResponse.error_info.description = ex.Message.ToString();


                logDAL.InsertLog("WebApi", "Commerce7SendOrder:  " + ex.Message.ToString(), authKey, 1, member_id);
            }

            return new JsonResult(abandonedCartRsvpEmailResponse);
        }

        [Route("bigcommercesendorder")]
        [HttpPost]
        public async Task<IActionResult> BigCommerceSendOrder([FromBody]BigCommerceSendOrderRequest model)
        {
            var abandonedCartRsvpEmailResponse = new BaseResponse();
            int member_id = 0;
            int successCount = 0;
            int totalCount = model.reservation_id.Count;

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            WineryModel memberModel = eventDAL.GetWineryById(model.member_id);
            if (model == null || string.IsNullOrWhiteSpace(memberModel.BigCommerceAceessToken) || string.IsNullOrWhiteSpace(memberModel.BigCommerceStoreId)
                || model.reservation_id.Count == 0)
            {
                abandonedCartRsvpEmailResponse.success = false;
                abandonedCartRsvpEmailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                abandonedCartRsvpEmailResponse.error_info.extra_info = Common.Common.InternalServerError;
                abandonedCartRsvpEmailResponse.error_info.description = "Invalid/missing data in request.";

                return new JsonResult(abandonedCartRsvpEmailResponse);
            }

            if (string.IsNullOrWhiteSpace(model.processed_by))
                model.processed_by = "CellarPass System";

            string authKey = HttpContext.Request.Headers["AuthenticateKey"];
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try
            {
                string BookingCodes = string.Empty;

                foreach (var item in model.reservation_id)
                {
                    string externalOrderId = "";

                    bool IsAdmin = Convert.ToBoolean(Request.Headers["IsAdmin"]);
                    ReservationDetailModel reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(item, "", IsAdmin);

                    string CustId = await Task.Run(() => Utility.CheckAndUpdateBigCommerceCustomer(memberModel, reservationDetailModel.member_id, reservationDetailModel.user_detail.email));

                    if (!string.IsNullOrEmpty(CustId))
                    {
                        externalOrderId = await Task.Run(() => Utility.BigCommerceCreateOrder(memberModel.BigCommerceStoreId, memberModel.BigCommerceAceessToken, reservationDetailModel, CustId, model.processed_by));
                    }

                    if (!string.IsNullOrEmpty(CustId))
                        successCount = successCount + 1;
                }

                abandonedCartRsvpEmailResponse.success = true;
                abandonedCartRsvpEmailResponse.error_info.description = successCount.ToString() + " orders sent successfully, " + (totalCount-successCount).ToString() + " have errors. See Error view for details.";
            }
            catch (Exception ex)
            {
                abandonedCartRsvpEmailResponse.success = false;
                abandonedCartRsvpEmailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                abandonedCartRsvpEmailResponse.error_info.extra_info = Common.Common.InternalServerError;
                abandonedCartRsvpEmailResponse.error_info.description = ex.Message.ToString();


                logDAL.InsertLog("WebApi", "Commerce7SendOrder:  " + ex.Message.ToString(), authKey, 1, member_id);
            }

            return new JsonResult(abandonedCartRsvpEmailResponse);
        }

        [Route("closereservation")]
        [HttpPost]
        public async Task<IActionResult> CloseReservation([FromBody]CloseReservationRequest model)
        {
            int member_id = 0;
            var closeReservationResponse = new CloseReservationResponse();
            try
            {
                bool ret = false;
                bool alreadyClosed = false;

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                WaitlistModel waitlistModel = new WaitlistModel();
                RsvpModel rsvpModel = new RsvpModel();
                if (model.transaction_category == 1)
                {
                    waitlistModel = eventDAL.GetWaitlistById(model.transaction_id);
                    if (waitlistModel.WaitlistStatus == 7)
                    {
                        alreadyClosed = true;
                    }
                }
                else
                {
                    rsvpModel = eventDAL.GetRsvpById(model.transaction_id);
                    if (rsvpModel.SeatedStatus == 2)
                    {
                        alreadyClosed = true;
                    }
                }

                if (alreadyClosed)
                {
                    closeReservationResponse.success = false;
                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    if (model.transaction_category == 1)
                    {
                        closeReservationResponse.error_info.extra_info = "Waitlist already closed";
                    }
                    else
                    {
                        closeReservationResponse.error_info.extra_info = "Reservation already closed";
                    }
                }
                else
                {
                    ret = eventDAL.CloseReservation(model.transaction_id, model.transaction_category);

                    if (ret)
                    {
                        closeReservationResponse.data.transaction_id = model.transaction_id;
                        closeReservationResponse.success = true;

                        var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                        var model1 = new CreateDeltaRequest();

                        foreach (var item in eventDAL.GetSeatingSession(model.transaction_id, model.transaction_category))
                        {
                            model1.item_id = item.Id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = item.LocationId;
                            model1.member_id = 0;
                            model1.action_date = DateTime.UtcNow;
                            notificationDAL.SaveDelta(model1);
                        }

                        if (model.transaction_category == 1)
                        {
                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.Waitlists;
                            model1.location_id = waitlistModel.LocationId;
                            model1.member_id = waitlistModel.MemberId;
                            model1.action_date = DateTime.UtcNow;  //waitlistModel.WaitStartDateTime;
                            notificationDAL.SaveDelta(model1);

                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = waitlistModel.LocationId;
                            model1.member_id = waitlistModel.MemberId;
                            model1.action_date = DateTime.UtcNow;
                            notificationDAL.SaveDelta(model1);

                            member_id = waitlistModel.MemberId;
                        }
                        else
                        {
                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.Reservations;
                            model1.location_id = rsvpModel.LocationId;
                            model1.member_id = rsvpModel.MemberId;
                            model1.action_date = DateTime.UtcNow;  //rsvpModel.EventDate;
                            notificationDAL.SaveDelta(model1);

                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = rsvpModel.LocationId;
                            model1.member_id = rsvpModel.MemberId;
                            model1.action_date = DateTime.UtcNow;
                            notificationDAL.SaveDelta(model1);

                            member_id = rsvpModel.MemberId;
                        }
                    }
                    else
                    {
                        closeReservationResponse.success = true;
                        closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        closeReservationResponse.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                closeReservationResponse.success = false;
                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                closeReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                closeReservationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CloseReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(closeReservationResponse);
        }

        [Route("closereservationv2")]
        [HttpPost]
        public async Task<IActionResult> CloseReservationV2([FromBody]CloseReservationRequest model)
        {
            var closeReservationResponse = new CloseReservationResponse();
            int member_id = 0;
            try
            {
                bool ret = false;
                bool alreadyClosed = false;

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                WaitlistModel waitlistModel = new WaitlistModel();
                RsvpModel rsvpModel = new RsvpModel();
                if (model.transaction_category == 1)
                {
                    waitlistModel = eventDAL.GetWaitlistById(model.transaction_id);
                    member_id = waitlistModel.MemberId;
                    if (waitlistModel.WaitlistStatus == 7)
                    {
                        alreadyClosed = true;
                    }
                }
                else
                {
                    rsvpModel = eventDAL.GetRsvpById(model.transaction_id);
                    member_id = rsvpModel.MemberId;
                    if (rsvpModel.SeatedStatus == 2)
                    {
                        alreadyClosed = true;
                    }
                }

                if (alreadyClosed)
                {
                    closeReservationResponse.success = false;
                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    if (model.transaction_category == 1)
                    {
                        closeReservationResponse.error_info.extra_info = "Waitlist already closed";
                    }
                    else
                    {
                        closeReservationResponse.error_info.extra_info = "Reservation already closed";
                    }
                }
                else
                {
                    ret = eventDAL.CloseReservation(model.transaction_id, model.transaction_category);

                    if (ret)
                    {
                        closeReservationResponse.data.transaction_id = model.transaction_id;
                        closeReservationResponse.success = true;

                        var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                        var model1 = new CreateDeltaRequest();

                        foreach (var item in eventDAL.GetSeatingSession(model.transaction_id, model.transaction_category))
                        {
                            model1.item_id = item.Id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = 0;
                            model1.member_id = 0;
                            model1.action_date = DateTime.UtcNow;
                            model1.floor_plan_id = item.FloorPlanId;
                            notificationDAL.SaveDelta(model1);
                        }

                        if (model.transaction_category == 1)
                        {
                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.Waitlists;
                            model1.location_id = 0;
                            model1.member_id = waitlistModel.MemberId;
                            model1.action_date = DateTime.UtcNow;  //waitlistModel.WaitStartDateTime;
                            model1.floor_plan_id = waitlistModel.AssignedFloorPlanId > 0 ? waitlistModel.AssignedFloorPlanId : waitlistModel.FloorPlanId;
                            notificationDAL.SaveDelta(model1);

                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = 0;
                            model1.member_id = waitlistModel.MemberId;
                            model1.action_date = DateTime.UtcNow;
                            model1.floor_plan_id = waitlistModel.AssignedFloorPlanId > 0 ? waitlistModel.AssignedFloorPlanId : waitlistModel.FloorPlanId;
                            notificationDAL.SaveDelta(model1);
                        }
                        else
                        {
                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.Reservations;
                            model1.location_id = 0;
                            model1.member_id = rsvpModel.MemberId;
                            model1.action_date = DateTime.UtcNow;  //rsvpModel.EventDate;
                            model1.floor_plan_id = (rsvpModel.AssignedFloorPlanId > 0 ? rsvpModel.AssignedFloorPlanId : rsvpModel.FloorPlanId);
                            notificationDAL.SaveDelta(model1);

                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = 0;
                            model1.member_id = rsvpModel.MemberId;
                            model1.action_date = DateTime.UtcNow;
                            model1.floor_plan_id = (rsvpModel.AssignedFloorPlanId > 0 ? rsvpModel.AssignedFloorPlanId : rsvpModel.FloorPlanId);
                            notificationDAL.SaveDelta(model1);
                        }
                    }
                    else
                    {
                        closeReservationResponse.success = true;
                        closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                        closeReservationResponse.error_info.extra_info = "no record found";
                    }
                }
            }
            catch (Exception ex)
            {
                closeReservationResponse.success = false;
                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                closeReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                closeReservationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CloseReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(closeReservationResponse);
        }

        [Route("unseatreservation")]
        [HttpPost]
        public async Task<IActionResult> UnseatReservation([FromBody]CloseReservationRequest model)
        {
            int member_id = 0;
            var closeReservationResponse = new CloseReservationResponse();
            try
            {
                bool ret = false;
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                ret = eventDAL.UnseatReservation(model.transaction_id, model.transaction_category);
                if (ret)
                {
                    closeReservationResponse.data.transaction_id = model.transaction_id;
                    closeReservationResponse.success = true;

                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var model1 = new CreateDeltaRequest();

                    foreach (var item in eventDAL.GetSeatingSession(model.transaction_id, model.transaction_category))
                    {
                        model1.item_id = item.Id;
                        model1.item_type = (int)ItemType.TableStatus;
                        model1.location_id = item.LocationId;
                        model1.member_id = 0;
                        model1.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model1);
                    }

                    if (model.transaction_category == 1)
                    {
                        WaitlistModel waitlistModel = eventDAL.GetWaitlistById(model.transaction_id);
                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.Waitlists;
                        model1.location_id = waitlistModel.LocationId;
                        model1.member_id = waitlistModel.MemberId;
                        model1.action_date = DateTime.UtcNow;  //waitlistModel.WaitStartDateTime;
                        notificationDAL.SaveDelta(model1);

                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.TableStatus;
                        model1.location_id = waitlistModel.LocationId;
                        model1.member_id = waitlistModel.MemberId;
                        model1.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model1);

                        member_id = waitlistModel.MemberId;
                    }
                    else
                    {
                        RsvpModel rsvpModel = eventDAL.GetRsvpById(model.transaction_id);
                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.Reservations;
                        model1.location_id = rsvpModel.LocationId;
                        model1.member_id = rsvpModel.MemberId;
                        model1.action_date = DateTime.UtcNow;  //rsvpModel.EventDate;
                        notificationDAL.SaveDelta(model1);

                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.TableStatus;
                        model1.location_id = rsvpModel.LocationId;
                        model1.member_id = rsvpModel.MemberId;
                        model1.action_date = DateTime.UtcNow;
                        notificationDAL.SaveDelta(model1);

                        member_id = rsvpModel.MemberId;
                    }
                }
                else
                {
                    closeReservationResponse.success = true;
                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    closeReservationResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                closeReservationResponse.success = false;
                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                closeReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                closeReservationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UnseatReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(closeReservationResponse);
        }

        [Route("unseatreservationv2")]
        [HttpPost]
        public async Task<IActionResult> UnseatReservationV2([FromBody]CloseReservationRequest model)
        {
            int member_id = 0;
            var closeReservationResponse = new CloseReservationResponse();
            try
            {
                bool ret = false;
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WaitlistModel waitlistModel = null;
                RsvpModel rsvpModel = null;
                if (model.transaction_category == 1)
                {
                    waitlistModel = eventDAL.GetWaitlistById(model.transaction_id);
                    member_id = waitlistModel.MemberId;
                }
                else
                {
                    rsvpModel = eventDAL.GetRsvpById(model.transaction_id);
                    member_id = rsvpModel.MemberId;
                }

                ret = eventDAL.UnseatReservation(model.transaction_id, model.transaction_category);

                if (ret)
                {
                    closeReservationResponse.data.transaction_id = model.transaction_id;
                    closeReservationResponse.success = true;

                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var model1 = new CreateDeltaRequest();

                    foreach (var item in eventDAL.GetSeatingSession(model.transaction_id, model.transaction_category))
                    {
                        model1.item_id = item.Id;
                        model1.item_type = (int)ItemType.TableStatus;
                        model1.location_id = 0;
                        model1.member_id = 0;
                        model1.action_date = DateTime.UtcNow;
                        model1.floor_plan_id = item.FloorPlanId;
                        notificationDAL.SaveDelta(model1);
                    }

                    if (model.transaction_category == 1)
                    {
                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.Waitlists;
                        model1.location_id = 0;
                        model1.member_id = waitlistModel.MemberId;
                        model1.action_date = DateTime.UtcNow;  //waitlistModel.WaitStartDateTime;
                        model1.floor_plan_id = waitlistModel.AssignedFloorPlanId > 0 ? waitlistModel.AssignedFloorPlanId : waitlistModel.FloorPlanId;
                        notificationDAL.SaveDelta(model1);

                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.TableStatus;
                        model1.location_id = 0;
                        model1.member_id = waitlistModel.MemberId;
                        model1.action_date = DateTime.UtcNow;
                        model1.floor_plan_id = waitlistModel.AssignedFloorPlanId > 0 ? waitlistModel.AssignedFloorPlanId : waitlistModel.FloorPlanId;
                        notificationDAL.SaveDelta(model1);
                    }
                    else
                    {
                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.Reservations;
                        model1.location_id = 0;
                        model1.member_id = rsvpModel.MemberId;
                        model1.action_date = DateTime.UtcNow;  //rsvpModel.EventDate;
                        model1.floor_plan_id = rsvpModel.AssignedFloorPlanId > 0 ? rsvpModel.AssignedFloorPlanId : rsvpModel.FloorPlanId;
                        notificationDAL.SaveDelta(model1);

                        model1.item_id = model.transaction_id;
                        model1.item_type = (int)ItemType.TableStatus;
                        model1.location_id = 0;
                        model1.member_id = rsvpModel.MemberId;
                        model1.action_date = DateTime.UtcNow;
                        model1.floor_plan_id = (rsvpModel.AssignedFloorPlanId > 0 ? rsvpModel.AssignedFloorPlanId : rsvpModel.FloorPlanId);
                        notificationDAL.SaveDelta(model1);
                    }
                }
                else
                {
                    closeReservationResponse.success = true;
                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    closeReservationResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                closeReservationResponse.success = false;
                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                closeReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                closeReservationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UnseatReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(closeReservationResponse);
        }

        [Route("seatreservation")]
        [HttpPost]
        public async Task<IActionResult> SeatReservation([FromBody]SeatReservationRequest model)
        {
            int member_id = 0;
            var closeReservationResponse = new CloseReservationResponse();
            try
            {
                //seated = 2
                //partially_seated = 3
                if (model.table_status == 2 || model.table_status == 3)
                {
                    bool processed = false;
                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    WaitlistModel waitlistModel = new WaitlistModel();
                    ReservationDetailModel reservationDetailModel = new ReservationDetailModel();
                    int locationId = 0;
                    if (model.location_ids.Count > 0)
                        locationId = model.location_ids[0];

                    LocationModel locationModel = eventDAL.GetLocationByID(locationId);

                    if (model.transaction_category == 1) //Waitlists = 1
                    {
                        waitlistModel = eventDAL.GetWaitlistById(model.transaction_id);
                        member_id = waitlistModel.MemberId;
                        if (waitlistModel != null && waitlistModel.Id > 0)
                        {
                            if (locationModel.location_id == 0)
                                locationModel = eventDAL.GetLocationByID(waitlistModel.LocationId);

                            DateTime WaitLocalStartDateTime = waitlistModel.WaitStartDateTime.AddMinutes(locationModel.location_timezone_offset);

                            DateTime businessDay = DateTime.UtcNow.AddMinutes(locationModel.location_timezone_offset);

                            DateTime localdate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time);

                            if (businessDay < localdate)
                                businessDay = businessDay.AddDays(-1);

                            businessDay = businessDay.Date;

                            DateTime businessStartDate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time);

                            DateTime businessEndDate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time).AddHours(24).AddSeconds(-1);


                            if (WaitLocalStartDateTime >= businessStartDate && WaitLocalStartDateTime <= businessEndDate)
                            {
                                if (waitlistModel.WaitlistStatus == 2) //Seated = 2
                                {
                                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationAlreadySeated;
                                    closeReservationResponse.error_info.extra_info = "Already seated";
                                }
                                else
                                {
                                    if (model.force == false && waitlistModel.WaitlistStatus == 7)  //Closed = 7
                                    {
                                        closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationAlreadyTerminated;
                                        closeReservationResponse.error_info.extra_info = "Already closed";
                                    }
                                    else
                                    {
                                        if (waitlistModel.PartySize != model.party_size)
                                        {
                                            //Update PartySize
                                            eventDAL.UpdateWaitlistPartySize(model.transaction_id, model.party_size);
                                        }
                                        processed = true;
                                    }
                                }
                            }
                            else
                            {
                                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                                closeReservationResponse.error_info.extra_info = "The transaction isn't for the current business day";
                            }
                        }
                        else
                        {
                            closeReservationResponse.success = true;
                            closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                            closeReservationResponse.error_info.extra_info = "No record found";
                        }
                    }
                    else //Reservations = 2
                    {
                        bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                        reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(model.transaction_id, "", IsAdmin);
                        if (reservationDetailModel != null && reservationDetailModel.reservation_id > 0)
                        {
                            member_id = reservationDetailModel.member_id;
                            if (locationModel.location_id == 0)
                                locationModel = eventDAL.GetLocationByID(reservationDetailModel.location_id);
                            //DateTime businessDay = Times.ToTimeZoneTime(DateTime.UtcNow, (Times.TimeZone)locationModel.location_timezone);
                            DateTime businessDay = DateTime.UtcNow.AddMinutes(locationModel.location_timezone_offset);

                            DateTime localdate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time);

                            if (businessDay < localdate)
                                businessDay = businessDay.AddDays(-1);

                            businessDay = businessDay.Date;

                            DateTime businessStartDate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time);

                            DateTime businessEndDate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time).AddHours(24).AddSeconds(-1);

                            //if (reservationDetailModel.event_start_date.Date == DateTime.UtcNow.Date)
                            if (reservationDetailModel.event_start_date >= businessStartDate && reservationDetailModel.event_start_date <= businessEndDate)
                            {
                                //SEATED = 1
                                if (reservationDetailModel.seated_status == 1)
                                {
                                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationAlreadySeated;
                                    closeReservationResponse.error_info.extra_info = "Already seated";
                                }
                                else
                                {
                                    if (model.force == false && reservationDetailModel.seated_status == 2) //CLOSED = 2
                                    {
                                        closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationAlreadyTerminated;
                                        closeReservationResponse.error_info.extra_info = "Already closed";
                                    }
                                    else
                                    {
                                        if (reservationDetailModel.total_guests != model.party_size && model.table_status == 2)
                                        {
                                            //Update total_guests
                                            var updatePartySizeRequest = new UpdatePartySizeRequest();
                                            updatePartySizeRequest.reservation_id = model.transaction_id;
                                            updatePartySizeRequest.party_size = model.party_size;
                                            updatePartySizeRequest.process_payment = model.process_payment;
                                            var createReservationResponse = new CreateReservationResponse();
                                            createReservationResponse = await UpdateReservationPartySize(updatePartySizeRequest);
                                            if (createReservationResponse.success == false)
                                            {
                                                closeReservationResponse.error_info.error_type = createReservationResponse.error_info.error_type;
                                                closeReservationResponse.error_info.extra_info = createReservationResponse.error_info.extra_info;
                                                closeReservationResponse.error_info.description = createReservationResponse.error_info.description;

                                                return new ObjectResult(closeReservationResponse);
                                            }
                                        }
                                        processed = true;
                                    }
                                }
                            }
                            else
                            {
                                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                                closeReservationResponse.error_info.extra_info = "The transaction isn't for the current business day";
                            }
                        }
                        else
                        {
                            closeReservationResponse.success = true;
                            closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                            closeReservationResponse.error_info.extra_info = "No record found";
                        }
                    }

                    if (processed == true)
                    {
                        string tabelids = string.Empty;
                        foreach (var item in model.table_ids)
                        {
                            TableAvailableModel tableAvailableModel = eventDAL.GetTableAvailableById(item);
                            if (tableAvailableModel.status == 2 || tableAvailableModel.status == 3)
                            {
                                if (model.force == false)
                                {
                                    string TableName = eventDAL.GetTableNameById(item);

                                    if (TableName.Length > 0)
                                    {
                                        if (tabelids.Length > 0)
                                        {
                                            tabelids = tabelids + "," + TableName;
                                        }
                                        else
                                        {
                                            tabelids = TableName;
                                        }
                                    }
                                }
                                else
                                {
                                    eventDAL.ForceTableSeat(tableAvailableModel.SessionId, tableAvailableModel.TransactionCategory, tableAvailableModel.TransactionId);
                                }
                            }
                        }

                        if (tabelids.Length > 0)
                        {
                            closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                            closeReservationResponse.error_info.extra_info = "Table " + tabelids + " is no longer available";
                            processed = false;
                        }
                    }

                    if (processed == true)
                    {

                        int server_id = 0;
                        if (model.server_id != null)
                        {
                            server_id = model.server_id ?? 0;
                            if (server_id > 0)
                            {
                                bool IsActiveServerSession = eventDAL.IsActiveServerSession(server_id);
                                if (IsActiveServerSession == false)
                                {
                                    TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);
                                    SessionModel sessionModel = new SessionModel();

                                    sessionModel = tableLayoutDAL.StartServerSession(model.location_ids, server_id);

                                    foreach (var item in model.location_ids)
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
                            }
                        }

                        int remainingPeople = model.party_size;
                        foreach (var item in model.table_ids)
                        {
                            TableLayoutModel tableLayoutModel = new TableLayoutModel();
                            tableLayoutModel = eventDAL.GetTableLayoutById(item);
                            int numberSeated = 0;
                            if (remainingPeople > 0)
                            {
                                if (remainingPeople > tableLayoutModel.MaxParty)
                                {
                                    numberSeated = tableLayoutModel.MaxParty;
                                    remainingPeople = remainingPeople - tableLayoutModel.MaxParty;
                                }
                                else
                                {
                                    numberSeated = remainingPeople;
                                }
                            }
                            //CreateSeating
                            int SeatingId = eventDAL.CreateSeating(DateTime.UtcNow, tableLayoutModel.LocationId, item, numberSeated, model.transaction_category, model.transaction_id, model.server_id ?? 0);
                            if (SeatingId > 0)
                            {
                                eventDAL.CreateSeatingLog(SeatingId, DateTime.UtcNow, model.table_status);
                            }
                        }

                        //TODO
                        if (model.transaction_category == 1) //Waitlists = 1
                        {
                            eventDAL.UpdateWaitlistStatus(model.transaction_id, 2);
                            closeReservationResponse.success = true;
                            closeReservationResponse.data.transaction_id = model.transaction_id;

                            var model1 = new CreateDeltaRequest();
                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.Waitlists;
                            model1.location_id = waitlistModel.LocationId;
                            model1.member_id = waitlistModel.MemberId;
                            model1.action_date = waitlistModel.WaitStartDateTime;
                            notificationDAL.SaveDelta(model1);

                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = waitlistModel.LocationId;
                            model1.member_id = waitlistModel.MemberId;
                            model1.action_date = DateTime.UtcNow;
                            notificationDAL.SaveDelta(model1);
                        }
                        else
                        {
                            //ReservationStatus.Completed = 1
                            eventDAL.UpdateReservationV2Status(model.transaction_id, 1, "", 0, 0);
                            eventDAL.UpdateReservationV2SeatedStatus(model.transaction_id, 1);
                            closeReservationResponse.success = true;
                            closeReservationResponse.data.transaction_id = model.transaction_id;

                            var model1 = new CreateDeltaRequest();
                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.Reservations;
                            model1.location_id = reservationDetailModel.location_id;
                            model1.member_id = reservationDetailModel.member_id;
                            model1.action_date = reservationDetailModel.event_start_date;
                            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                            int invMode = userDAL.GetInventoryModeForMember(reservationDetailModel.member_id);
                            if (invMode == 1)
                            {
                                model1.floor_plan_id = reservationDetailModel.floor_plan_id;
                            }

                            notificationDAL.SaveDelta(model1);

                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = reservationDetailModel.location_id;
                            model1.member_id = reservationDetailModel.member_id;
                            model1.action_date = DateTime.UtcNow;
                            notificationDAL.SaveDelta(model1);
                        }
                    }
                }
                else
                {
                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    closeReservationResponse.error_info.extra_info = "Table status error";
                }
            }
            catch (Exception ex)
            {
                closeReservationResponse.success = false;
                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                closeReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                closeReservationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SeatReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(closeReservationResponse);
        }

        [Route("seatreservationv2")]
        [HttpPost]
        public async Task<IActionResult> SeatReservationV2([FromBody]SeatReservationRequestV2 model)
        {
            var closeReservationResponse = new CloseReservationResponse();
            DateTime startDate = System.DateTime.UtcNow;
            DateTime endDate = System.DateTime.UtcNow;
            int member_id = 0;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            logDAL.InsertLog("WebApi", "seatreservationv2:" + JsonConvert.SerializeObject(model), HttpContext.Request.Headers["AuthenticateKey"], 3, -1);

            try
            {
                //seated = 2
                //partially_seated = 3
                if (model.table_status == 2 || model.table_status == 3)
                {
                    bool processed = false;
                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    WaitlistModel waitlistModel = new WaitlistModel();
                    ReservationDetailModel reservationDetailModel = new ReservationDetailModel();
                    int locationId = 0;
                    if (model.table_ids.Count > 0)
                    {
                        TableLayoutModel tableLayoutModel = eventDAL.GetTableLayoutById(model.table_ids[0]);
                        locationId = tableLayoutModel.LocationId;
                    }

                    var lstTableDetails = eventDAL.SortTablesByMaxParty(model.table_ids);


                    LocationModel locationModel = eventDAL.GetLocationByID(locationId);

                    if (model.transaction_category == 1) //Waitlists = 1
                    {
                        waitlistModel = eventDAL.GetWaitlistById(model.transaction_id);
                        if (waitlistModel != null && waitlistModel.Id > 0)
                        {
                            member_id = waitlistModel.MemberId;
                            startDate = waitlistModel.WaitStartDateTime;
                            endDate = waitlistModel.WaitEndDateTime;
                            DateTime WaitLocalStartDateTime = waitlistModel.WaitStartDateTime.AddMinutes(locationModel.location_timezone_offset);

                            DateTime businessDay = DateTime.UtcNow.AddMinutes(locationModel.location_timezone_offset);

                            DateTime localdate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time);

                            if (businessDay < localdate)
                                businessDay = businessDay.AddDays(-1);

                            businessDay = businessDay.Date;

                            DateTime businessStartDate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time);

                            DateTime businessEndDate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time).AddHours(24).AddSeconds(-1);


                            if (WaitLocalStartDateTime >= businessStartDate && WaitLocalStartDateTime <= businessEndDate)
                            {
                                if (waitlistModel.WaitlistStatus == 2) //Seated = 2
                                {
                                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationAlreadySeated;
                                    closeReservationResponse.error_info.extra_info = "Already seated";
                                }
                                else
                                {
                                    if (model.force == false && waitlistModel.WaitlistStatus == 7)  //Closed = 7
                                    {
                                        closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationAlreadyTerminated;
                                        closeReservationResponse.error_info.extra_info = "Already closed";
                                    }
                                    else
                                    {
                                        if (waitlistModel.PartySize != model.party_size)
                                        {
                                            //Update PartySize
                                            eventDAL.UpdateWaitlistPartySize(model.transaction_id, model.party_size);
                                        }
                                        processed = true;
                                    }
                                }
                            }
                            else
                            {
                                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                                closeReservationResponse.error_info.extra_info = "The transaction isn't for the current business day";
                            }
                        }
                        else
                        {
                            closeReservationResponse.success = true;
                            closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                            closeReservationResponse.error_info.extra_info = "No record found";
                        }
                    }
                    else //Reservations = 2
                    {
                        bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                        reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(model.transaction_id, "", IsAdmin);
                        if (reservationDetailModel != null && reservationDetailModel.reservation_id > 0)
                        {
                            member_id = reservationDetailModel.member_id;
                            startDate = reservationDetailModel.event_start_date.AddMinutes(locationModel.location_timezone_offset * -1);
                            endDate = reservationDetailModel.event_end_date.AddMinutes(locationModel.location_timezone_offset * -1);
                            //DateTime businessDay = Times.ToTimeZoneTime(DateTime.UtcNow, (Times.TimeZone)locationModel.location_timezone);
                            DateTime businessDay = DateTime.UtcNow.AddMinutes(locationModel.location_timezone_offset);

                            DateTime localdate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time);

                            if (businessDay < localdate)
                                businessDay = businessDay.AddDays(-1);

                            businessDay = businessDay.Date;

                            DateTime businessStartDate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time);

                            DateTime businessEndDate = Convert.ToDateTime(businessDay.ToShortDateString() + " " + locationModel.seating_reset_time).AddHours(24).AddSeconds(-1);

                            //if (reservationDetailModel.event_start_date.Date == DateTime.UtcNow.Date)
                            if (reservationDetailModel.event_start_date >= businessStartDate && reservationDetailModel.event_start_date <= businessEndDate)
                            {
                                //SEATED = 1
                                if (reservationDetailModel.seated_status == 1)
                                {
                                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationAlreadySeated;
                                    closeReservationResponse.error_info.extra_info = "Already seated";
                                }
                                else
                                {
                                    if (model.force == false && reservationDetailModel.seated_status == 2) //CLOSED = 2
                                    {
                                        closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.ReservationAlreadyTerminated;
                                        closeReservationResponse.error_info.extra_info = "Already closed";
                                    }
                                    else
                                    {
                                        if (reservationDetailModel.total_guests != model.party_size)
                                        {
                                            if (model.party_size > reservationDetailModel.total_guests)
                                            {
                                                bool qtyerror = false;
                                                int TotalMaxParty = 0;

                                                foreach (var item in model.table_ids)
                                                {
                                                    TotalMaxParty = TotalMaxParty + eventDAL.GetTableMaxPartybyTableID(item);
                                                }

                                                if (model.party_size > TotalMaxParty)
                                                    qtyerror = true;

                                                if (qtyerror)
                                                {
                                                    if (model.force)
                                                    {
                                                        eventDAL.SaveReservationV2StatusNotes(reservationDetailModel.reservation_id, 0, "Reservation was forced seated.", model.user_name, reservationDetailModel.member_id, 0);
                                                    }
                                                    else
                                                    {
                                                        closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.TableInventory;
                                                        closeReservationResponse.error_info.extra_info = string.Format("party of {0} cannot be seated on this table. Max capacity of this table is {1}.", model.party_size, TotalMaxParty);

                                                        return new ObjectResult(closeReservationResponse);
                                                    }
                                                }
                                            }

                                            if (model.table_status == 2)
                                            {
                                                //Update total_guests
                                                var updatePartySizeRequest = new UpdatePartySizeRequest();
                                                updatePartySizeRequest.reservation_id = model.transaction_id;
                                                updatePartySizeRequest.party_size = model.party_size;
                                                updatePartySizeRequest.process_payment = model.process_payment;
                                                var createReservationResponse = new CreateReservationResponse();
                                                createReservationResponse = await UpdateReservationPartySize(updatePartySizeRequest);
                                                if (createReservationResponse.success == false)
                                                {
                                                    closeReservationResponse.error_info.error_type = createReservationResponse.error_info.error_type;
                                                    closeReservationResponse.error_info.extra_info = createReservationResponse.error_info.extra_info;
                                                    closeReservationResponse.error_info.description = createReservationResponse.error_info.description;

                                                    return new ObjectResult(closeReservationResponse);
                                                }
                                            }
                                        }

                                        foreach (var item in model.table_ids)
                                        {
                                            if (eventDAL.CheckTableAvailableById(item, reservationDetailModel.event_start_date, reservationDetailModel.event_end_date) == false)
                                            {
                                                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                                                closeReservationResponse.error_info.extra_info = "The destination table(s) have been manually assigned and cannot be swapped.";

                                                return new ObjectResult(closeReservationResponse);
                                            }
                                        }

                                        processed = true;
                                    }
                                }
                            }
                            else
                            {
                                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                                closeReservationResponse.error_info.extra_info = "The transaction isn't for the current business day";
                            }
                        }
                        else
                        {
                            closeReservationResponse.success = true;
                            closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                            closeReservationResponse.error_info.extra_info = "No record found";
                        }
                    }

                    if (processed == true)
                    {
                        string tabelids = string.Empty;
                        foreach (var item in model.table_ids)
                        {
                            TableAvailableModel tableAvailableModel = eventDAL.GetTableAvailableById(item);
                            if (tableAvailableModel.status == 2 || tableAvailableModel.status == 3)
                            {
                                if (model.force == false)
                                {
                                    string TableName = eventDAL.GetTableNameById(item);

                                    if (TableName.Length > 0)
                                    {
                                        if (tabelids.Length > 0)
                                        {
                                            tabelids = tabelids + "," + TableName;
                                        }
                                        else
                                        {
                                            tabelids = TableName;
                                        }
                                    }
                                }
                                else
                                {
                                    eventDAL.ForceTableSeat(tableAvailableModel.SessionId, tableAvailableModel.TransactionCategory, tableAvailableModel.TransactionId);
                                }
                            }
                        }

                        if (tabelids.Length > 0)
                        {
                            closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                            closeReservationResponse.error_info.extra_info = "Table " + tabelids + " is no longer available";
                            processed = false;
                        }
                    }

                    if (processed == true)
                    {
                        string tabelids = string.Empty;
                        foreach (var item in model.table_ids)
                        {
                            TableBlockedStatusModel blockedStatus = eventDAL.CheckTableBlockedStatus(item, startDate, endDate);
                            if (blockedStatus.Id > 0)
                            {
                                string TableName = eventDAL.GetTableNameById(item);
                                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.TableBlocked;
                                closeReservationResponse.error_info.extra_info = string.Format("Table {0} is blocked between {1} and {2}", TableName, blockedStatus.BlockStartDate.Value.ToString("hh:mm tt"), blockedStatus.BlockEndDate.Value.ToString("hh:mm tt"));
                                processed = false;
                            }
                        }
                    }
                    if (processed == true)
                    {

                        int server_id = 0;
                        if (model.server_id != null)
                        {
                            server_id = model.server_id ?? 0;
                            if (server_id > 0)
                            {
                                bool IsActiveServerSession = eventDAL.IsActiveServerSession(server_id);
                                if (IsActiveServerSession == false)
                                {
                                    TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);
                                    SessionModel sessionModel = new SessionModel();

                                    sessionModel = tableLayoutDAL.StartServerSessionV2(model.floor_plan_ids, server_id);

                                    foreach (var item in model.floor_plan_ids)
                                    {
                                        var model11 = new CreateDeltaRequest();
                                        model11.item_id = sessionModel.id;
                                        model11.item_type = (int)ItemType.Servers;
                                        model11.location_id = 0;  //TODO
                                        model11.member_id = 0;
                                        model11.action_date = DateTime.UtcNow;
                                        model11.floor_plan_id = item;
                                        notificationDAL.SaveDelta(model11);
                                    }
                                }
                            }
                        }

                        int remainingPeople = model.party_size;
                        int counter = 0;
                        List<int> selectedTables = new List<int>();
                        List<int> listTables = new List<int>();
                        while (counter < model.table_ids.Count)
                        {
                            int numberSeated = 0;
                            int item = 0;
                            if (remainingPeople > 0)
                            {
                                if (lstTableDetails[counter].MaxParty <= remainingPeople)
                                {
                                    numberSeated = lstTableDetails[counter].MaxParty;
                                    item = lstTableDetails[counter].TableId;

                                }
                                else if (lstTableDetails[counter].MaxParty > remainingPeople)
                                {
                                    int currTableId = lstTableDetails[counter].TableId;
                                    //find other table which can fit this party
                                    var table = lstTableDetails.Where(t => t.MaxParty >= remainingPeople && !selectedTables.Contains(t.TableId)).OrderBy(t => t.MaxParty).Select(t => t).FirstOrDefault();
                                    if (table == null)
                                    {
                                        item = currTableId;
                                        if (lstTableDetails[counter].MaxParty > remainingPeople)
                                        {
                                            numberSeated = remainingPeople;
                                        }
                                        else
                                            numberSeated = lstTableDetails[counter].MaxParty;
                                    }
                                    else
                                    {
                                        item = table.TableId;
                                        if (table.MaxParty > remainingPeople)
                                        {
                                            numberSeated = remainingPeople;
                                        }
                                        else
                                            numberSeated = table.MaxParty;

                                        if (item != currTableId)
                                        {
                                            int sId = eventDAL.CreateSeating(DateTime.UtcNow, locationId, currTableId, 0, model.transaction_category, model.transaction_id, model.server_id ?? 0);
                                            selectedTables.Add(currTableId);
                                        }

                                    }

                                }
                                selectedTables.Add(item);
                            }
                            else
                            {
                                var remainingTables = lstTableDetails.Where(t => !selectedTables.Contains(t.TableId)).Select(t => t).FirstOrDefault();
                                if (remainingTables != null)
                                {
                                    item = remainingTables.TableId;
                                    selectedTables.Add(item);
                                    numberSeated = 0;

                                }
                                else
                                {
                                    break;
                                }

                            }
                            //CreateSeating
                            if (item > 0)
                            {
                                int SeatingId = eventDAL.CreateSeating(DateTime.UtcNow, locationId, item, numberSeated, model.transaction_category, model.transaction_id, model.server_id ?? 0);
                                if (SeatingId > 0)
                                {
                                    eventDAL.CreateSeatingLog(SeatingId, DateTime.UtcNow, model.table_status);

                                    if (model.transaction_category == 2)
                                    {
                                        listTables.Add(item);
                                    }
                                }
                            }
                            remainingPeople = remainingPeople - numberSeated;
                            counter++;
                        }
                        int floorPlanIdForDelta = 0;
                        if (model.floor_plan_ids != null && model.floor_plan_ids.Count > 0)
                        {
                            floorPlanIdForDelta = model.floor_plan_ids[0];
                        }
                        else if (model.table_ids.Count > 0)
                        {
                            TableLayoutDAL tableLayoutDAL = new TableLayoutDAL(Common.Common.ConnectionString);
                            var floorPlans = tableLayoutDAL.GetFloorPlansByTableIds(string.Join(',', model.table_ids.ToArray()));
                            if (floorPlans != null && floorPlans.Count > 0)
                            {
                                floorPlanIdForDelta = floorPlans[0];
                            }
                        }

                        //TODO
                        if (model.transaction_category == 1) //Waitlists = 1
                        {
                            eventDAL.UpdateWaitlistStatus(model.transaction_id, 2);
                            closeReservationResponse.success = true;
                            closeReservationResponse.data.transaction_id = model.transaction_id;


                            var model1 = new CreateDeltaRequest();
                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.Waitlists;
                            model1.location_id = waitlistModel.LocationId;
                            model1.member_id = waitlistModel.MemberId;
                            model1.action_date = waitlistModel.WaitStartDateTime;
                            if (floorPlanIdForDelta == 0)
                                floorPlanIdForDelta = waitlistModel.AssignedFloorPlanId > 0 ? waitlistModel.AssignedFloorPlanId : waitlistModel.FloorPlanId;

                            model1.floor_plan_id = floorPlanIdForDelta;

                            notificationDAL.SaveDelta(model1);

                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = waitlistModel.LocationId;
                            model1.member_id = waitlistModel.MemberId;
                            model1.action_date = DateTime.UtcNow;
                            model1.floor_plan_id = floorPlanIdForDelta;
                            notificationDAL.SaveDelta(model1);
                        }
                        else
                        {
                            //ReservationStatus.Completed = 1
                            eventDAL.UpdateReservationV2Status(model.transaction_id, 1, "", 0, 0);
                            eventDAL.UpdateReservationV2SeatedStatus(model.transaction_id, 1);

                            foreach (var item in listTables)
                            {
                                eventDAL.SwappingTableByRsvpId(item, reservationDetailModel.event_start_date, reservationDetailModel.event_end_date, reservationDetailModel.reservation_id);
                            }

                            closeReservationResponse.success = true;
                            closeReservationResponse.data.transaction_id = model.transaction_id;

                            var model1 = new CreateDeltaRequest();
                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.Reservations;
                            model1.location_id = reservationDetailModel.location_id;
                            model1.member_id = reservationDetailModel.member_id;
                            model1.action_date = reservationDetailModel.event_start_date;
                            if (floorPlanIdForDelta == 0)
                                floorPlanIdForDelta = reservationDetailModel.assigned_floor_plan_id > 0 ? reservationDetailModel.assigned_floor_plan_id : reservationDetailModel.floor_plan_id;

                            model1.floor_plan_id = floorPlanIdForDelta;
                            notificationDAL.SaveDelta(model1);

                            model1.item_id = model.transaction_id;
                            model1.item_type = (int)ItemType.TableStatus;
                            model1.location_id = reservationDetailModel.location_id;
                            model1.member_id = reservationDetailModel.member_id;
                            model1.action_date = DateTime.UtcNow;
                            model1.floor_plan_id = floorPlanIdForDelta;
                            notificationDAL.SaveDelta(model1);
                        }
                    }
                }
                else
                {
                    closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    closeReservationResponse.error_info.extra_info = "Table status error";
                }
            }
            catch (Exception ex)
            {
                closeReservationResponse.success = false;
                closeReservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                closeReservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                closeReservationResponse.error_info.description = ex.Message.ToString();


                logDAL.InsertLog("WebApi", "SeatReservationV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(closeReservationResponse);
        }

        [Route("savereservationwaitlist")]
        [HttpPost]
        public async Task<IActionResult> SaveReservationV2Waitlist([FromBody]CreateWaitlist model)
        {
            int member_id = 0;
            var reservationV2WaitlistResponse = new ReservationV2WaitlistResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                int mobilePhoneStatus = (int)Utility.SMSVerified_System(model.mobile_phone);

                reservationV2WaitlistResponse.data.id = eventDAL.SaveReservationV2Waitlist(model, mobilePhoneStatus);

                int WaitListId = eventDAL.GetWaitListIdByWaitListGuid(reservationV2WaitlistResponse.data.id);

                if (WaitListId > 0)
                {
                    QueueService getStarted = new QueueService();

                    var queueModel = new EmailQueue();

                    queueModel.EType = (int)EmailType.WaitListNotification;

                    queueModel.RsvpId = WaitListId;
                    queueModel.Src = (int)ActionSource.BackOffice;
                    var qData = JsonConvert.SerializeObject(queueModel);

                    AppSettings _appsettings = _appSetting.Value;
                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                }

                reservationV2WaitlistResponse.success = true;
            }
            catch (Exception ex)
            {
                reservationV2WaitlistResponse.success = false;
                reservationV2WaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationV2WaitlistResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationV2WaitlistResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SaveReservationV2Waitlist:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(reservationV2WaitlistResponse);
        }

        [Route("updatereservationwaitliststatus")]
        [HttpPost]
        public async Task<IActionResult> UpdateReservationV2WaitlistStatus([FromBody]UpdateWaitlist model)
        {
            var reservationV2WaitlistResponse = new ReservationV2WaitlistResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                string id = eventDAL.UpdateReservationV2Waitlist(model);

                reservationV2WaitlistResponse.data.id = id;
                reservationV2WaitlistResponse.success = true;
                if (id.Length > 0 && model.send_notice)
                {
                    int WaitListId = eventDAL.GetWaitListIdByWaitListGuid(id);
                    if (WaitListId > 0)
                    {
                        QueueService getStarted = new QueueService();

                        var queueModel = new EmailQueue();

                        if (model.waitlist_status == Common.Common.Waitlist_Status.canceled)
                            queueModel.EType = (int)EmailType.WaitListCancellation;
                        else
                            queueModel.EType = (int)EmailType.WaitListNotification;

                        queueModel.RsvpId = WaitListId;
                        queueModel.Src = (int)ActionSource.BackOffice;
                        var qData = JsonConvert.SerializeObject(queueModel);

                        AppSettings _appsettings = _appSetting.Value;
                        getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                reservationV2WaitlistResponse.success = false;
                reservationV2WaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationV2WaitlistResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationV2WaitlistResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateReservationV2Waitlist:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(reservationV2WaitlistResponse);
        }

        [Route("reservationwaitlistdetail")]
        [HttpGet]
        public async Task<IActionResult> GetReservationV2WaitlistbyId(string reservation_waitlist_id)
        {
            int member_id = 0;
            var getReservationV2WaitlistResponse = new GetReservationV2WaitlistResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                string PurchaseURL = "https://dev.cellarpass.com/";
                if (_appSetting.Value.QueueName == "QueueName")
                    PurchaseURL = "https://cellarpass.com/";

                var waitlistModel = new Waitlist();
                waitlistModel = eventDAL.GetReservationV2WaitlistbyId(reservation_waitlist_id, PurchaseURL);

                if (waitlistModel != null)
                {
                    member_id = waitlistModel.member_id;
                    waitlistModel.work_phone = Utility.FormatTelephoneNumber(waitlistModel.work_phone, waitlistModel.address.country);
                    getReservationV2WaitlistResponse.success = true;
                    getReservationV2WaitlistResponse.data = waitlistModel;
                }
                else
                {
                    getReservationV2WaitlistResponse.success = true;
                    getReservationV2WaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    getReservationV2WaitlistResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                getReservationV2WaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                getReservationV2WaitlistResponse.error_info.extra_info = Common.Common.InternalServerError;
                getReservationV2WaitlistResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationV2WaitlistbyId: WaitlistId-" + reservation_waitlist_id + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(getReservationV2WaitlistResponse);
        }

        [Route("reservationwaitlist")]
        [HttpGet]
        public async Task<IActionResult> GetReservationV2Waitlist(int member_id, string Keyword = "", int status = -1, DateTime? start_date = null, DateTime? end_date = null)
        {
            var getReservationV2WaitlistResponse = new GetListReservationV2WaitlistResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                string PurchaseURL = "https://dev.cellarpass.com/";
                if (_appSetting.Value.QueueName == "QueueName")
                    PurchaseURL = "https://cellarpass.com/";

                var waitlistModel = new List<Waitlist>();
                waitlistModel = eventDAL.GetReservationV2Waitlist(member_id, Keyword, status, start_date, end_date, PurchaseURL);

                if (waitlistModel != null && waitlistModel.Count > 0)
                {
                    foreach (var item in waitlistModel)
                    {
                        item.work_phone = Utility.FormatTelephoneNumber(item.work_phone, item.address.country);
                    }

                    getReservationV2WaitlistResponse.success = true;
                    getReservationV2WaitlistResponse.data = waitlistModel;
                }
                else
                {
                    getReservationV2WaitlistResponse.success = true;
                    getReservationV2WaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    getReservationV2WaitlistResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                getReservationV2WaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                getReservationV2WaitlistResponse.error_info.extra_info = Common.Common.InternalServerError;
                getReservationV2WaitlistResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationV2Waitlist:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(getReservationV2WaitlistResponse);
        }

        [Route("reservationwaitlistbyuserid")]
        [HttpGet]
        public async Task<IActionResult> GetReservationV2WaitlistByUserId(int user_id, int status = -1, DateTime? start_date = null, DateTime? end_date = null)
        {
            var getReservationV2WaitlistResponse = new GetListReservationV2WaitlistResponse();
            try
            {
                //EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                string PurchaseURL = "https://dev.cellarpass.com/";
                if (_appSetting.Value.QueueName == "QueueName")
                    PurchaseURL = "https://cellarpass.com/";

                var waitlistModel = new List<Waitlist>();
                waitlistModel = eventDAL.GetReservationV2WaitlistByUserId(user_id, status, start_date, end_date, PurchaseURL);

                if (waitlistModel != null && waitlistModel.Count > 0)
                {
                    foreach (var item in waitlistModel)
                    {
                        item.work_phone = Utility.FormatTelephoneNumber(item.work_phone, item.address.country);
                    }
                    getReservationV2WaitlistResponse.success = true;
                    getReservationV2WaitlistResponse.data = waitlistModel;
                }
                else
                {
                    getReservationV2WaitlistResponse.success = true;
                    getReservationV2WaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    getReservationV2WaitlistResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                getReservationV2WaitlistResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                getReservationV2WaitlistResponse.error_info.extra_info = Common.Common.InternalServerError;
                getReservationV2WaitlistResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationV2WaitlistByUserId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(getReservationV2WaitlistResponse);
        }

        [Route("guestdashboard")]
        [HttpGet]
        public async Task<IActionResult> GetGuestDashboardMetricsByMember(int member_id, int days = 0, int mode = 0, string date = "", string start_date = "", string end_date = "")
        {
            var guestDashboard = new GuestDashboardResponse();

            if (member_id <= 0)
            {
                guestDashboard.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                guestDashboard.error_info.extra_info = "Invalid Member Id. Please pass a valid one.";
                guestDashboard.error_info.description = "Invalid Member Id. Please pass a valid one.";
                return new ObjectResult(guestDashboard);
            }
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                //check if member has RSVP plan or not
                int rsvpPlan = eventDAL.GetRSVPBillingPlanFOrMmeber(member_id);
                if (rsvpPlan == 0)
                {
                    guestDashboard.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    guestDashboard.error_info.extra_info = ".";
                    guestDashboard.error_info.description = "No active RSVP billing plan found for the member.";
                    return new ObjectResult(guestDashboard);
                }

                DateTime dt = Convert.ToDateTime("1/1/1900");

                DateTime.TryParse(date, out dt);

                if (dt.Year < 1900)
                    dt = Convert.ToDateTime("1/1/1900");

                DateTime sdt = Convert.ToDateTime("1/1/1900");

                DateTime.TryParse(start_date, out sdt);

                if (sdt.Year < 1900)
                    sdt = Convert.ToDateTime("1/1/1900");

                DateTime edt = Convert.ToDateTime("1/1/1900");

                DateTime.TryParse(end_date, out edt);

                if (edt.Year < 1900)
                    edt = Convert.ToDateTime("1/1/1900");

                var guestDashboardModel = new GuestDashboardModel();
                if (mode == 1)
                {
                    guestDashboardModel = eventDAL.GetDashboardMetricsByEventDateMemberAndDays(member_id, days, dt, sdt, edt);
                }
                else
                {
                    guestDashboardModel = eventDAL.GetDashboardMetricsByMemberAndDays(member_id, days, dt, sdt, edt);
                }
                if (guestDashboardModel != null)
                {
                    guestDashboard.success = true;
                    guestDashboard.data = guestDashboardModel;
                }
                else
                {
                    guestDashboard.success = true;
                    guestDashboard.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    guestDashboard.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                guestDashboard.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                guestDashboard.error_info.extra_info = Common.Common.InternalServerError;
                guestDashboard.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationV2WaitlistByUserId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(guestDashboard);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("refundreservation")]
        [HttpPost]
        public async Task<IActionResult> RefundReservation([FromBody]RefundReservationPaymentRequest request)
        {
            ReservationPaymentResponse response = new ReservationPaymentResponse();
            int member_id = 0;
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                var reservationDetailModel = new ReservationDetailModel();
                reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(request.reservation_id);
                member_id = reservationDetailModel.member_id;
                Services.Payments objPayments = new Services.Payments(_appSetting);
                response.data = await objPayments.RefundReservation(reservationDetailModel, request.amount, 0);
                response.success = true;
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "RefundReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

            }

            return new ObjectResult(response);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("chargereservation")]
        [HttpPost]
        public async Task<IActionResult> ChargeReservation([FromBody]ReservationPaymentRequest request)
        {
            ReservationPaymentResponse response = new ReservationPaymentResponse();
            int member_id = 0;
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                var reservationDetailModel = new ReservationDetailModel();
                reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(request.reservation_id);

                if (reservationDetailModel.member_id == 0)
                {
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "Invalid reservation Id. No reservation data found.";
                    response.error_info.description = "Invalid reservation Id. No reservation data found.";
                    return new ObjectResult(response);
                }

                member_id = reservationDetailModel.member_id;
                CreateReservationRequest rsvp = new CreateReservationRequest();

                ViewModels.UserDetailViewModel user_detail = new ViewModels.UserDetailViewModel();
                ViewModels.UserAddress address = new ViewModels.UserAddress();

                user_detail.first_name = reservationDetailModel.user_detail.first_name;
                user_detail.last_name = reservationDetailModel.user_detail.last_name;
                user_detail.phone_number = reservationDetailModel.user_detail.phone_number;
                user_detail.email = reservationDetailModel.user_detail.email;
                address.zip_code = reservationDetailModel.user_detail.address.zip_code;
                address.city = reservationDetailModel.user_detail.address.city;
                address.state = reservationDetailModel.user_detail.address.state;
                address.country = reservationDetailModel.user_detail.address.country;
                address.address_1 = reservationDetailModel.user_detail.address.address_1;
                address.address_2 = reservationDetailModel.user_detail.address.address_2;

                user_detail.address = address;
                rsvp.user_detail = user_detail;

                rsvp.pay_card = reservationDetailModel.pay_card;

                string cardNumber = StringHelpers.Decryption(reservationDetailModel.pay_card.number);
                rsvp.pay_card.card_last_four_digits = Common.Common.Right(cardNumber, 4);
                rsvp.pay_card.card_first_four_digits = Common.Common.Left(cardNumber, 4);
                rsvp.pay_card.number = Common.Common.Right(cardNumber, 4);  //cardNumber;

                rsvp.member_id = reservationDetailModel.member_id;

                rsvp.reservation_id = reservationDetailModel.reservation_id;

                Services.Payments objPayments = new Services.Payments(_appSetting);
                //bool enablePreAuth = false;
                //try
                //{
                //    enablePreAuth = eventDAL.CheckPreAuthEnabledForWinery(rsvp.member_id);
                //}
                //catch { enablePreAuth = false; }
                string prevTranId = "";
                Common.Payments.Transaction.ChargeType chargeType = Transaction.ChargeType.Sale;
                Common.Payments.TransactionResult refundResult = new Common.Payments.TransactionResult();

                var authPayment = eventDAL.GetReservationPreAuths(rsvp.reservation_id);

                if (authPayment != null && authPayment.Count > 0)
                {
                    if (authPayment[0].original_amount >= request.amount)
                    {
                        chargeType = Transaction.ChargeType.Capture;
                        prevTranId = authPayment[0].transaction_id;
                    }
                    else
                    {
                        refundResult = await objPayments.VoidReservationPreAuth(authPayment[0], reservationDetailModel, authPayment[0].amount, reservationDetailModel.booked_by_id ?? 0);
                    }
                }

                response.data = await objPayments.ChargeReservation(rsvp, request.amount, reservationDetailModel.booking_code, reservationDetailModel.user_detail.user_id, chargeType, prevTranId);

                if (chargeType == Transaction.ChargeType.Capture && response.data.Status == TransactionResult.StatusType.Failed)
                {
                    refundResult = await objPayments.VoidReservationPreAuth(authPayment[0], reservationDetailModel, authPayment[0].amount, reservationDetailModel.booked_by_id ?? 0);

                    response.data = await objPayments.ChargeReservation(rsvp, request.amount, reservationDetailModel.booking_code, reservationDetailModel.user_detail.user_id, Transaction.ChargeType.Sale);
                }

                if (response.data.Status == TransactionResult.StatusType.Success)
                {
                    var retval = eventDAL.UpdateReservation(reservationDetailModel.reservation_id, response.data.Amount, request.gratuity_amount);
                    response.success = true;
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "ChargeReservation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

            }

            return new ObjectResult(response);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("chargereservationv2")]
        [HttpGet]
        public async Task<IActionResult> ChargeReservationV2()
        {
            ReservationPaymentResponse response = new ReservationPaymentResponse();

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            int member_id = 0;
            List<ReservationChargeModel> list = eventDAL.GetTransactionsForCCAutomationTaskV2();

            foreach (var request in list)
            {
                try
                {
                    var reservationDetailModel = new ReservationDetail2Model();
                    reservationDetailModel = eventDAL.GetReservationDetails2byReservationId(request.ReservationId);
                    member_id = reservationDetailModel.member_id;
                    CreateReservationRequest rsvp = new CreateReservationRequest();

                    ViewModels.UserDetailViewModel user_detail = new ViewModels.UserDetailViewModel();
                    ViewModels.UserAddress address = new ViewModels.UserAddress();

                    user_detail.first_name = reservationDetailModel.user_detail.first_name;
                    user_detail.last_name = reservationDetailModel.user_detail.last_name;
                    user_detail.phone_number = reservationDetailModel.user_detail.phone_number;
                    user_detail.email = reservationDetailModel.user_detail.email;
                    address.zip_code = reservationDetailModel.user_detail.address.zip_code;
                    address.city = reservationDetailModel.user_detail.address.city;
                    address.state = reservationDetailModel.user_detail.address.state;
                    address.country = reservationDetailModel.user_detail.address.country;
                    address.address_1 = reservationDetailModel.user_detail.address.address_1;
                    address.address_2 = reservationDetailModel.user_detail.address.address_2;

                    user_detail.address = address;
                    rsvp.user_detail = user_detail;

                    rsvp.pay_card = reservationDetailModel.pay_card;

                    string cardNumber = StringHelpers.Decryption(reservationDetailModel.pay_card.number);
                    rsvp.pay_card.card_last_four_digits = Common.Common.Right(cardNumber, 4);
                    rsvp.pay_card.card_first_four_digits = Common.Common.Left(cardNumber, 4);
                    rsvp.pay_card.number = Common.Common.Right(cardNumber, 4);  //cardNumber;

                    rsvp.member_id = reservationDetailModel.member_id;

                    rsvp.reservation_id = reservationDetailModel.reservation_id;

                    Services.Payments objPayments = new Services.Payments(_appSetting);
                    var authPayment = eventDAL.GetReservationPreAuths(rsvp.reservation_id);

                    string prevTranId = "";
                    Common.Payments.Transaction.ChargeType chargeType = Transaction.ChargeType.Sale;
                    if (authPayment != null && authPayment.Count > 0)
                    {
                        chargeType = Transaction.ChargeType.Capture;
                        prevTranId = authPayment[0].transaction_id;
                    }
                    response.data = await objPayments.ChargeReservation(rsvp, request.BalanceDue, reservationDetailModel.booking_code, reservationDetailModel.user_detail.user_id, chargeType, prevTranId);

                    if (response.data.Status == TransactionResult.StatusType.Success)
                    {
                        var retval = eventDAL.UpdateReservation(reservationDetailModel.reservation_id, response.data.Amount);
                    }
                }
                catch (Exception ex)
                {
                    response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                    response.error_info.extra_info = Common.Common.InternalServerError;
                    response.error_info.description = ex.Message.ToString();
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "ChargeReservationv2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

                }
            }

            return new ObjectResult(response);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("finishreservationprocess")]
        [HttpPost]
        public async Task<IActionResult> FinishReservationProcess([FromBody]FinishReservationRequest request)
        {
            ReservationPaymentResponse response = new ReservationPaymentResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            int member_id = 0;
            try
            {
                var createReservationModel = new ReservationDetailModel();
                createReservationModel = eventDAL.GetReservationDetailsbyReservationId(request.reservation_id);

                if (createReservationModel.user_detail != null)
                {
                    member_id = createReservationModel.member_id;
                    // add it to queue to create third party contact
                    try
                    {
                        QueueService getStarted = new QueueService();

                        var queueModel = new EmailQueue();
                        queueModel.EType = (int)EmailType.CreateThirdPartyContact;
                        queueModel.BCode = "";
                        queueModel.UId = createReservationModel.user_detail.user_id;
                        queueModel.RsvpId = createReservationModel.member_id;
                        queueModel.PerMsg = "";
                        queueModel.Src = createReservationModel.referral_type;
                        var qData = JsonConvert.SerializeObject(queueModel);

                        AppSettings _appsettings = _appSetting.Value;
                        getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                    }
                    catch (Exception ex)
                    {
                        logDAL.InsertLog("WebApi", "FinishReservationProcess Create Third Party Contact:    RsvpId-" + request.reservation_id.ToString() + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
                    }
                }

                //custom settigs
                SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                var customSetting = settingsDAL.GetCustomSettingByMember(Common.Common.SettingType.EJGalloAPI, createReservationModel.member_id);

                if (customSetting != null && createReservationModel.user_detail != null && !string.IsNullOrWhiteSpace(createReservationModel.user_detail.email))
                {
                    try
                    {
                        EJGallo eJ = new EJGallo(_appSetting);
                        ViewModels.UserAddress userAddress = null;

                        if (createReservationModel.user_detail.address != null)
                        {
                            userAddress = new ViewModels.UserAddress
                            {
                                city = createReservationModel.user_detail.address.city,
                                country = createReservationModel.user_detail.address.country,
                                state = createReservationModel.user_detail.address.state,
                                zip_code = createReservationModel.user_detail.address.zip_code
                            };
                        }

                        var userDetail = new CPReservationApi.WebApi.ViewModels.UserDetailViewModel
                        {
                            address = userAddress,
                            customer_type = createReservationModel.user_detail.customer_type,
                            email = createReservationModel.user_detail.email,
                            first_name = createReservationModel.user_detail.first_name,
                            last_name = createReservationModel.user_detail.last_name,
                            phone_number = createReservationModel.user_detail.phone_number,

                        };

                        bool isSuccess = await eJ.NewsletterSignup(createReservationModel.member_id, userDetail, customSetting);
                    }
                    catch (Exception ex)
                    {
                        logDAL.InsertLog("WebApi", "FinishReservationProcess- EJGallo: RsvpId-" + request.reservation_id.ToString() + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

                    }
                }

                try
                {
                    bool CreateMeeting = false;

                    if (request.is_modify)
                    {
                        //TODO
                        ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfoByReservationId(createReservationModel.reservation_id);
                        if (zoomMeetingInfo != null && zoomMeetingInfo.MeetingId > 0)
                        {
                            if (zoomMeetingInfo.SlotId == createReservationModel.slot_id && zoomMeetingInfo.SlotType == createReservationModel.slot_type && zoomMeetingInfo.StartDate.Date == createReservationModel.event_start_date.Date)
                            {
                                CreateMeeting = false;
                                //DO Nothing
                            }
                            else
                            {
                                CreateMeeting = await DeleteMeeting(zoomMeetingInfo.MeetingBehavior, zoomMeetingInfo.MeetingId, createReservationModel.member_id, createReservationModel.reservation_id, createReservationModel.status, zoomMeetingInfo.RegistrantId, createReservationModel.user_detail.email, HttpContext.Request.Headers["AuthenticateKey"]);
                            }
                        }
                        else
                        {
                            CreateMeeting = true;
                        }
                    }
                    else
                    {
                        CreateMeeting = true;
                    }

                    if (CreateMeeting)
                    {
                        ViewModels.UserDetailViewModel user_detail = new ViewModels.UserDetailViewModel();

                        ViewModels.UserAddress address = new ViewModels.UserAddress();

                        user_detail.email = createReservationModel.user_detail.email;
                        user_detail.first_name = createReservationModel.user_detail.first_name;
                        user_detail.last_name = createReservationModel.user_detail.last_name;
                        address.city = createReservationModel.user_detail.address.city;
                        address.country = createReservationModel.user_detail.address.country;
                        address.zip_code = createReservationModel.user_detail.address.zip_code;
                        user_detail.phone_number = createReservationModel.user_detail.phone_number;
                        user_detail.address = address;

                        long MeetingId = await ZoomMeeting.CreateMeeting(createReservationModel.slot_id, createReservationModel.slot_type, createReservationModel.event_start_date.Date, createReservationModel.reservation_id, user_detail, createReservationModel.member_id);

                        if (MeetingId > 0)
                        {
                            string noteMsg = "Zoom Meeting Created. Meeting Id: " + Utility.FormatZoomMeetingId(MeetingId.ToString());
                            eventDAL.SaveReservationV2StatusNotes(createReservationModel.reservation_id, createReservationModel.status, noteMsg, HttpContext.Request.Headers["AuthenticateKey"], createReservationModel.member_id, 0);
                        }
                    }

                }
                catch (Exception ex)
                {
                    logDAL.InsertLog("WebApi", "SaveReservation CreateMeeting:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
                }

                if (!request.is_modify)
                {


                    //mailchimp integration
                    bool isMailChimpEnabled = eventDAL.IsMailChimpModuleAvailable(createReservationModel.member_id);

                    if (isMailChimpEnabled)
                    {

                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(createReservationModel.member_id, (int)Common.Common.SettingGroup.mailchimp);
                        string mcAPIKey = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_key);
                        string mcStore = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_store);
                        string mcCampaign = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_listname);

                        if (!string.IsNullOrWhiteSpace(mcAPIKey) && !string.IsNullOrWhiteSpace(mcStore))
                        {
                            try
                            {
                                QueueService getStarted = new QueueService();

                                var queueModel = new EmailQueue();
                                queueModel.EType = (int)EmailType.MailChimpOrder;
                                queueModel.BCode = createReservationModel.booking_code;
                                queueModel.UId = createReservationModel.user_detail.user_id;
                                queueModel.RsvpId = createReservationModel.reservation_id;
                                queueModel.PerMsg = request.personal_message;
                                queueModel.Src = createReservationModel.referral_type;
                                var qData = JsonConvert.SerializeObject(queueModel);

                                AppSettings _appsettings = _appSetting.Value;
                                getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                            }
                            catch (Exception ex)
                            {
                                logDAL.InsertLog("WebApi", "FinishReservationProcess Create Mail Chimp Order:  RsvpId-" + request.reservation_id.ToString() + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
                            }
                        }

                    }

                    try
                    {
                        CreateReservationModel model = new CreateReservationModel
                        {
                            FeeDue = createReservationModel.fee_due,
                            AmountPaid = createReservationModel.amount_paid,
                            UserId = createReservationModel.user_detail.user_id,
                            BookingCode = createReservationModel.booking_code
                        };
                        await UpsertOrderTobLoyal(createReservationModel.member_id, model, createReservationModel.reservation_id);

                    }
                    catch (Exception ex)
                    {
                        logDAL.InsertLog("WebApi", "FinishReservationProcess:    RsvpId-" + request.reservation_id.ToString() + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
                    }
                }
                if (request.send_guest_email || request.send_affiliate_email)
                {
                    try
                    {
                        QueueService getStarted = new QueueService();

                        var queueModel = new EmailQueue();
                        queueModel.EType = (int)EmailType.Rsvp;
                        queueModel.BCode = createReservationModel.booking_code;
                        queueModel.UId = createReservationModel.user_detail.user_id;
                        queueModel.RsvpId = createReservationModel.reservation_id;
                        queueModel.PerMsg = request.personal_message;
                        queueModel.Src = createReservationModel.referral_type;
                        queueModel.ActionType = request.action_type;
                        queueModel.GuestEmail = request.send_guest_email;
                        queueModel.AffiliateEmail = request.send_affiliate_email;
                        var qData = JsonConvert.SerializeObject(queueModel);

                        AppSettings _appsettings = _appSetting.Value;
                        getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                    }
                    catch (Exception ex)
                    {
                        logDAL.InsertLog("WebApi", "FinishReservationProcess Email:    RsvpId-" + request.reservation_id.ToString() + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
                    }
                }

                //GoogleCalendar.CalendarAddEventV2(createReservationModel);

            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("WebApi", "FinishReservationProcess:    RsvpId-" + request.reservation_id.ToString() + ",Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);

            }

            return new ObjectResult(response);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [Route("deletezoommeeting")]
        [HttpPost]
        public async Task<IActionResult> DeleteZoomMeeting([FromBody]DeleteMeetingRequest request)
        {
            BaseResponse response = new BaseResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                bool CreateMeeting = false;
                ZoomMeetingInfo zoomMeetingInfo = eventDAL.GetZoomMeetingInfoByReservationId(request.reservation_id);
                CreateMeeting = await DeleteMeeting(zoomMeetingInfo.MeetingBehavior, zoomMeetingInfo.MeetingId, request.member_id, request.reservation_id, request.status, zoomMeetingInfo.RegistrantId, request.email, HttpContext.Request.Headers["AuthenticateKey"]);
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "DeleteZoomMeeting:  ReservationId" + request.reservation_id.ToString() + ",error:-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, request.member_id);
            }

            return new ObjectResult(response);
        }

        private static async Task<bool> DeleteMeeting(int MeetingBehavior, long MeetingId, int member_id, int reservation_id, int status, string RegistrantId, string email, string currentuser)
        {
            bool DeleteMeeting = false;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            try
            {
                if (MeetingBehavior == 1)  //Unique Meeting
                {
                    DeleteMeeting = await ZoomMeeting.DeleteMeeting(MeetingId, member_id);
                    if (DeleteMeeting)
                    {
                        string noteMsg = "Zoom Meeting " + Utility.FormatZoomMeetingId(MeetingId.ToString()) + " was deleted.";
                        eventDAL.SaveReservationV2StatusNotes(reservation_id, status, noteMsg, currentuser, member_id, 0);
                    }
                }
                else if (MeetingBehavior == 2) //Add to Existing
                {
                    DeleteMeeting = await ZoomMeeting.DeleteMeetingRegistrant(member_id, MeetingId, RegistrantId, email, reservation_id);
                    if (DeleteMeeting)
                    {
                        string noteMsg = "Participant was removed from Zoom Meeting Id: " + Utility.FormatZoomMeetingId(MeetingId.ToString());
                        eventDAL.SaveReservationV2StatusNotes(reservation_id, status, noteMsg, currentuser, member_id, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "DeleteMeeting:    RsvpId-" + reservation_id.ToString() + ",Message-" + ex.Message.ToString(), currentuser, 1, member_id);

            }
            return DeleteMeeting;
        }


        [Route("getthirdpartynotes")]
        [HttpGet]
        public async Task<IActionResult> GetThirdPartyNotes(int member_id, string contact_id, string user_email = "")
        {
            ThirdPartyNoteResponse response = new ThirdPartyNoteResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            if (member_id <= 0)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Invalid Member Id. Please pass a valid one.";
                response.error_info.description = "Invalid Member Id. Please pass a valid one.";
                return new ObjectResult(response);
            }
            if (string.IsNullOrWhiteSpace(user_email) && string.IsNullOrWhiteSpace(contact_id))
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Either email or contact Id is required.";
                response.error_info.description = "Either email or contact Id is required.";
                return new ObjectResult(response);
            }

            try
            {
                var notes = await Utility.GetThirdPartyNotes(member_id, contact_id, user_email);

                if (notes != null)
                {
                    response.success = true;
                    response.data = notes;
                }

            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("WebApi", string.Format("GetThirdPartyNotes:  email:{0}, member_id:{1}, error:{2}", user_email, member_id, ex.Message.ToString()), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }

            return new ObjectResult(response);
        }

        [Route("addupdatethirdpartynotes")]
        [HttpPost]
        public async Task<IActionResult> AddUpdateThirdPartNotes([FromBody]UpdateNoteRequest request)
        {
            BaseResponse response = new BaseResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            if (request.member_id <= 0)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Invalid Member Id. Please pass a valid one.";
                response.error_info.description = "Invalid Member Id. Please pass a valid one.";
                return new ObjectResult(response);
            }
            if (string.IsNullOrWhiteSpace(request.contact_id))
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Contact Id is required.";
                response.error_info.description = "Contact Id is required.";
                return new ObjectResult(response);
            }

            try
            {
                AccountNote noteInfo = new AccountNote
                {
                    note = request.note,
                    subject = request.subject,
                    note_id = request.note_id + "",
                    note_date = request.note_date
                };

                if (!noteInfo.note_date.HasValue)
                    noteInfo.note_date = DateTime.UtcNow;

                bool isSuccess = await Utility.AddUpdateNoteToThirdParty(request.member_id, noteInfo, request.contact_id);

                response.success = isSuccess;


            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("WebApi", string.Format("addupdatethirdpartynotes:  member_id:{0}, error:{1}", request.member_id, ex.Message.ToString()), HttpContext.Request.Headers["AuthenticateKey"], 1, request.member_id);
            }

            return new ObjectResult(response);
        }

        [Route("resendfailedmail")]
        [HttpPost]
        public async Task<IActionResult> ResendFailedMail()
        {
            BaseResponse response = new BaseResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                List<FailedMailModel> list = eventDAL.GetFailedMailList();

                foreach (var item in list)
                {
                    try
                    {

                        QueueService getStarted = new QueueService();

                        var queueModel = new EmailQueue();
                        queueModel.EType = (int)EmailType.Rsvp;
                        queueModel.BCode = item.BookingCode;
                        queueModel.UId = item.UserId;
                        queueModel.RsvpId = item.ReservationId;
                        queueModel.PerMsg = "";
                        queueModel.Src = item.ReferralType;
                        queueModel.ActionType = 0;
                        queueModel.AffiliateEmail = item.AffiliateID > 0;

                        var qData = JsonConvert.SerializeObject(queueModel);

                        AppSettings _appsettings = _appSetting.Value;
                        getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                    }
                    catch (Exception ex)
                    {
                        logDAL.InsertLog("WebApi", "ResendFailedMail:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
                    }
                }
                response.success = true;
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("WebApi", string.Format("ResendFailedMail: " + ex.Message.ToString()), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }

            return new ObjectResult(response);
        }

        [Route("surveywaiverdetails")]
        [HttpGet]
        public async Task<IActionResult> GetSurveyWaiverDetailsByEmailAndMemberId(int member_id, string email)
        {
            SurveyWaiverResponse response = new SurveyWaiverResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            try
            {
                var model = eventDAL.GetSurveyWaiverStatusByEmailAndMemberId(member_id, email);

                if (model != null)
                {
                    response.success = true;
                    response.data = model;
                }

            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("WebApi", string.Format("GetSurveyWaiverDetailsByEmailAndMemberId:  email:{0}, member_id:{1}, error:{2}", email, member_id, ex.Message.ToString()), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }

            return new ObjectResult(response);
        }

        [Route("reservationdetailv2")]
        [HttpGet]
        public async Task<IActionResult> GetReservationDetailV2byReservationId(int reservation_id)
        {
            int member_id = 0;
            var reservationDetailResponse = new ReservationDetailV2Response();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationDetailModel = new ReservationDetailV2Model();
                reservationDetailModel = eventDAL.GetReservationDetailV2byReservationId(reservation_id);

                if (reservationDetailModel != null && reservationDetailModel.reservation_id > 0)
                {
                    reservationDetailResponse.success = true;
                    reservationDetailResponse.data = reservationDetailModel;
                }
                else
                {
                    reservationDetailResponse.success = true;
                    reservationDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationDetailV2byReservationId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationDetailResponse);
        }

        [Route("reservationdetailv4")]
        [HttpGet]
        public async Task<IActionResult> GetReservationDetailV4byReservationId(int reservation_id)
        {
            var reservationDetailResponse = new ReservationDetailV4Response();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationDetailModel = new ReservationDetailV4Model();
                reservationDetailModel = eventDAL.GetReservationDetailV4byReservationId(reservation_id);

                if (reservationDetailModel != null && reservationDetailModel.reservation_id > 0)
                {
                    reservationDetailModel.business_phone = Utility.FormatTelephoneNumber(reservationDetailModel.business_phone, reservationDetailModel.member_country);
                    reservationDetailResponse.success = true;
                    reservationDetailResponse.data = reservationDetailModel;
                }
                else
                {
                    reservationDetailResponse.success = true;
                    reservationDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationDetailV4byReservationId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationDetailResponse);
        }

        [Route("reservationdiscountlog")]
        [HttpGet]
        public async Task<IActionResult> GetReservationDiscountLogbyReservationId(int reservation_id)
        {
            var reservationDiscountLogResponse = new ReservationDiscountLogResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationDetailModel = new List<ReservationChangeLog>();
                reservationDetailModel = eventDAL.GetReservationChangeLogsbyReservationId(reservation_id, 1);

                if (reservationDetailModel != null)
                {
                    reservationDiscountLogResponse.success = true;
                    reservationDiscountLogResponse.data = reservationDetailModel;
                }
                else
                {
                    reservationDiscountLogResponse.success = true;
                    reservationDiscountLogResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationDiscountLogResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationDiscountLogResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationDiscountLogResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationDiscountLogResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationDiscountLogbyReservationId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationDiscountLogResponse);
        }

        [Route("reservationallemaillog")]
        [HttpGet]
        public async Task<IActionResult> GetReservationemailLogbyReservationId(int reservation_id)
        {
            var reservationDiscountLogResponse = new ReservationDiscountLogResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationDetailModel = new List<ReservationChangeLog>();
                reservationDetailModel = eventDAL.GetReservationAllEmailLogsbyReservationId(reservation_id);

                if (reservationDetailModel != null)
                {
                    reservationDiscountLogResponse.success = true;
                    reservationDiscountLogResponse.data = reservationDetailModel;
                }
                else
                {
                    reservationDiscountLogResponse.success = true;
                    reservationDiscountLogResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationDiscountLogResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationDiscountLogResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationDiscountLogResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationDiscountLogResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationemailLogbyReservationId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationDiscountLogResponse);
        }

        [Route("reservationchangeLog")]
        [HttpGet]
        public async Task<IActionResult> GetReservationchangeLogbyReservationId(int reservation_id)
        {
            var reservationDiscountLogResponse = new ReservationDiscountLogResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationDetailModel = new List<ReservationChangeLog>();
                reservationDetailModel = eventDAL.GetReservationChangeLogsbyReservationId(reservation_id, 0);

                if (reservationDetailModel != null)
                {
                    reservationDiscountLogResponse.success = true;
                    reservationDiscountLogResponse.data = reservationDetailModel;
                }
                else
                {
                    reservationDiscountLogResponse.success = true;
                    reservationDiscountLogResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationDiscountLogResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationDiscountLogResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationDiscountLogResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationDiscountLogResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationchangeLogbyReservationId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationDiscountLogResponse);
        }

        [Route("reservationpaymentLog")]
        [HttpGet]
        public async Task<IActionResult> GetReservationPaymentLogbyReservationId(int reservation_id)
        {
            var reservationPaymentLogResponse = new ReservationPaymentLogResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationDetailModel = new ReservationPaymentLogModel();
                reservationDetailModel = eventDAL.GetReservationPaymentLogbyReservationId(reservation_id);

                if (reservationDetailModel != null)
                {
                    reservationPaymentLogResponse.success = true;
                    reservationPaymentLogResponse.data = reservationDetailModel;
                }
                else
                {
                    reservationPaymentLogResponse.success = true;
                    reservationPaymentLogResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationPaymentLogResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationPaymentLogResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationPaymentLogResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationPaymentLogResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationPaymentLogbyReservationId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationPaymentLogResponse);
        }

        [Route("reservationnotesLog")]
        [HttpGet]
        public async Task<IActionResult> GetReservationNotesLogbyReservationId(int reservation_id)
        {
            var reservationnoteLogResponse = new ReservationNoteLogResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationDetailModel = new ReservationNoteLogModel();
                reservationDetailModel = eventDAL.GetReservationNoteLogbyReservationId(reservation_id);

                if (reservationDetailModel != null)
                {
                    reservationnoteLogResponse.success = true;
                    reservationnoteLogResponse.data = reservationDetailModel;
                }
                else
                {
                    reservationnoteLogResponse.success = true;
                    reservationnoteLogResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationnoteLogResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationnoteLogResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationnoteLogResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationnoteLogResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationNotesLogbyReservationId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationnoteLogResponse);
        }

        [Route("reservationsync")]
        [HttpGet]
        public async Task<IActionResult> TaskRsvpSync()
        {

            var baseResponse = new BaseResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try
            {
                DateTime localDateTime = Times.ToTimeZoneTime(DateTime.UtcNow);
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                List<TaskMemberDetails> listActiveWineryForExport = eventDAL.GetActiveWineryForExport(localDateTime);
                
                int count = 0;

                foreach (var item in listActiveWineryForExport)
                {
                    Common.Common.ExportType exportType = Common.Common.ExportType.None;
                    bool upsertFullfilmentC7 = item.UpsertFulfillmentDate;
                    bool featureEnabled = false;
                    bool bloyalAPIEnabled = false;
                    List<Settings.Setting> settingsGroup = new List<Settings.Setting>();
                    logDAL.InsertLog("TaskRsvpSync", "Start:- dbw.Id:" + item.Id.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, item.Id);

                    if (item.BillingPlan > 0)
                    {
                        if (item.EnableVin65)
                        {
                            if (item.Vin65Username.Trim().Length > 0 && item.Vin65Password.Trim().Length > 0)
                            {
                                featureEnabled = true;
                                exportType = Common.Common.ExportType.Vin65;
                            }
                        }
                        else if (item.eWineryEnabled)
                        {
                            if (item.eWineryUsername.Trim().Length > 0 && item.eWineryPassword.Trim().Length > 0)
                            {
                                featureEnabled = true;
                                exportType = Common.Common.ExportType.eWinery;
                            }
                        }
                        else if (item.EnableOrderPort)
                        {
                            if (item.OrderPortApiKey.Trim().Length > 0 && item.OrderPortClientId.Trim().Length > 0 && item.OrderPortApiToken.Trim().Length > 0)
                            {
                                featureEnabled = true;
                                exportType = Common.Common.ExportType.OrderPort;
                            }
                        }
                        else if (item.EnableCommerce7)
                        {
                            if (item.Commerce7Username.Trim().Length > 0 && item.Commerce7Password.Trim().Length > 0 && item.Commerce7Tenant.Trim().Length > 0 && item.Commerce7POSProfileId.Trim().Length > 0)
                            {
                                featureEnabled = true;
                                exportType = Common.Common.ExportType.Commerce7;
                                //upsertFullfilmentC7 = eventDAL.IsUpsertFulfilmentEnableCommerce7(item.Id);
                            }
                        }
                        else
                        {
                            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                            settingsGroup = settingsDAL.GetSettingGroup(item.Id, (int)Common.Common.SettingGroup.bLoyal).ToList();

                            bloyalAPIEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.bLoyalApiEnabled);

                            if (bloyalAPIEnabled)
                            {
                                featureEnabled = true;
                                exportType = Common.Common.ExportType.bLoyal;
                            }
                        }

                        if (featureEnabled)
                        {
                            DateTime StartDate = DateTime.UtcNow;
                            DateTime EndDate = DateTime.UtcNow;
                            //int Status = -1;
                            DateTime localCurrentDate = Times.ToTimeZoneTime(DateTime.UtcNow, (Times.TimeZone)item.TimeZoneId);

                            if (item.EnableSyncDate == 0)
                            {
                                StartDate = localCurrentDate.Date;
                                EndDate = localCurrentDate.Date;
                            }
                            else if (item.EnableSyncDate == 1)
                            {
                                StartDate = localCurrentDate.Date.AddDays(1);
                                EndDate = localCurrentDate.Date.AddDays(1);
                            }
                            else if (item.EnableSyncDate == -1)
                            {
                                StartDate = localCurrentDate.Date.AddDays(-1);
                                EndDate = localCurrentDate.Date.AddDays(-1);
                            }

                            logDAL.InsertLog("TaskRsvpSync", "dbw.Id:" + item.Id + ", StartDate:" + StartDate.ToString() + ", EndDate:" + EndDate.ToString() + ", exportType:" + exportType.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, item.Id);

                            bool UnpaidOrdersOnly = false;
                            bool PaidOrdersOnly = false;

                            if (bloyalAPIEnabled)
                            {
                                string bLoyalApiSyncOrderCriteria = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.bLoyalApiSyncOrderCriteria);

                                if (!string.IsNullOrWhiteSpace(bLoyalApiSyncOrderCriteria))
                                {
                                    if (bLoyalApiSyncOrderCriteria == "2")
                                    {
                                        UnpaidOrdersOnly = true;
                                    }
                                    else if (bLoyalApiSyncOrderCriteria == "4")
                                    {
                                        PaidOrdersOnly = true;
                                    }
                                }
                            }

                            List<TransactionsForExport> rList = eventDAL.GetTransactionsForExport(item.Id, StartDate, EndDate, -1, false, (int)exportType);
                            List<TransactionsForExport> rList1 = new List<TransactionsForExport>();
                            if (PaidOrdersOnly)
                            {
                                rList1 = rList.Where(c => c.ExportId == 0 && (c.PayStatus == 0 || c.PayStatus == 1)).ToList();
                            }
                            else if (UnpaidOrdersOnly)
                            {
                                rList1 = rList.Where(c => c.ExportId == 0 && (c.PayStatus == 2 || c.PayStatus == 3 || c.PayStatus == 4)).ToList();
                            }
                            else
                            {
                                rList1 = rList.Where(c => c.ExportId == 0).ToList();
                            }

                            foreach (var rsvp in rList1)
                            {
                                eventDAL.RemoveReservationFromAutoSyncingQueue(rsvp.Id);
                                string externalOrderId = "";

                                if (exportType == Common.Common.ExportType.Vin65)
                                {
                                    //TODO
                                    logDAL.InsertLog("TaskRsvpSync", "Start ExportType 'Vin65' RsvpId:" + rsvp.Id.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, item.Id);
                                }
                                else if (exportType == Common.Common.ExportType.eWinery)
                                {
                                    logDAL.InsertLog("TaskRsvpSync", "Start ExportType 'eWinery' RsvpId:" + rsvp.Id.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, item.Id);

                                    externalOrderId = await Utility.eWinerySendOrder(rsvp.Id, item.eWineryUsername, item.eWineryPassword);
                                }
                                else if (exportType == Common.Common.ExportType.bLoyal)
                                {
                                    logDAL.InsertLog("TaskRsvpSync", "Start ExportType 'bLoyal' RsvpId:" + rsvp.Id.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, item.Id);

                                    externalOrderId = await Utility.bLoyalSendOrder(settingsGroup, rsvp.Id);
                                }
                                else if (exportType == Common.Common.ExportType.Commerce7)
                                {
                                    count = count + 1;

                                    logDAL.InsertLog("TaskRsvpSync", "Start ExportType 'Commerce7' RsvpId:" + rsvp.Id.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, item.Id);

                                    Commerce7OrderModel orders = new Commerce7OrderModel();
                                    string posProfileId = item.Commerce7POSProfileId.Trim();
                                    orders = eventDAL.GetOrderDataCommerce7V2(rsvp.Id, upsertFullfilmentC7, ref posProfileId);
                                    if (!string.IsNullOrWhiteSpace(posProfileId))
                                        item.Commerce7POSProfileId = posProfileId;

                                    Commerce7CustomerModel commerce7CustomerModel = new Commerce7CustomerModel();

                                    commerce7CustomerModel = await Utility.CheckAndUpdateCommerce7Customer(item.Commerce7Username.Trim(), item.Commerce7Password.Trim(), item.Commerce7Tenant.Trim(), orders.billTo.firstName, orders.billTo.lastName, orders.billTo.company, orders.billTo.address, orders.billTo.address2, orders.billTo.city,
                                                                                            orders.billTo.stateCode, orders.billTo.zipCode, orders.billTo.countryCode, orders.email.Trim(), orders.billTo.phone, "", item.Id, rsvp.Id);

                                    if (commerce7CustomerModel.Exceeded)
                                    {
                                        Thread.Sleep(TimeSpan.FromMinutes(1));
                                        count = 0;
                                    }

                                    orders.customerId = commerce7CustomerModel.CustId;

                                    if (string.IsNullOrWhiteSpace(orders.billTo.address))
                                    {
                                        orders.billTo = null;
                                    }

                                    if (!string.IsNullOrEmpty(orders.customerId))
                                    {
                                        string result = await Utility.PushOrdersToCommerce7(orders, item.Commerce7Username.Trim(), item.Commerce7Password.Trim(), item.Commerce7Tenant.Trim(), rsvp.Id, "", upsertFullfilmentC7);

                                        if (result == "exceeded")
                                        {
                                            Thread.Sleep(TimeSpan.FromMinutes(1));
                                            count = 0;
                                        }
                                    }

                                    if (count == 15)
                                    {
                                        Thread.Sleep(TimeSpan.FromMinutes(1));
                                        count = 0;
                                    }
                                }
                                else if (exportType == Common.Common.ExportType.OrderPort)
                                {
                                    logDAL.InsertLog("TaskRsvpSync", "Start ExportType 'OrderPort' RsvpId:" + rsvp.Id.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, item.Id);

                                    PayloadOrderModel orders = new PayloadOrderModel();

                                    orders = eventDAL.GetOrderPortSendOrder(rsvp.Id);

                                    PayloadOrderModel[] payloadModelArray = new PayloadOrderModel[1];

                                    List<UserDetailModel> userDetailModel = new List<UserDetailModel>();
                                    userDetailModel = await Task.Run(() => Utility.GetCustomersByNameOrEmail(orders.BillingAddress.Email, item.OrderPortApiKey, item.OrderPortApiToken, item.OrderPortClientId));

                                    if (userDetailModel != null && userDetailModel.Count > 0)
                                    {
                                        orders.CustomerUuid = userDetailModel[0].membership_number;
                                    }
                                    else
                                    {
                                        PayloadModel payloadModel = new PayloadModel();
                                        payloadModel.CustomerUuid = "";

                                        payloadModel.BillingAddress.FirstName = orders.BillingAddress.FirstName;
                                        payloadModel.BillingAddress.LastName = orders.BillingAddress.LastName;
                                        payloadModel.BillingAddress.Company = !string.IsNullOrEmpty(orders.BillingAddress.Company) ? orders.BillingAddress.Company : "";
                                        payloadModel.BillingAddress.Address1 = orders.BillingAddress.Address1;
                                        payloadModel.BillingAddress.Address2 = !string.IsNullOrEmpty(orders.BillingAddress.Address2) ? orders.BillingAddress.Address2 : "";
                                        payloadModel.BillingAddress.City = !string.IsNullOrEmpty(orders.BillingAddress.City) ? orders.BillingAddress.City : "";
                                        payloadModel.BillingAddress.State = !string.IsNullOrEmpty(orders.BillingAddress.State) ? orders.BillingAddress.State : "";
                                        payloadModel.BillingAddress.ZipCode = !string.IsNullOrEmpty(orders.BillingAddress.ZipCode) ? orders.BillingAddress.ZipCode : "";
                                        payloadModel.BillingAddress.Country = orders.BillingAddress.Country;
                                        payloadModel.BillingAddress.Email = orders.BillingAddress.Email;
                                        payloadModel.BillingAddress.Phone = orders.BillingAddress.Phone;
                                        string CustId = await Utility.UpsertCustomerDetails(payloadModel, item.OrderPortApiKey, item.OrderPortApiToken, item.OrderPortClientId, item.Id);

                                        orders.CustomerUuid = CustId;
                                    }
                                    payloadModelArray[0] = orders;

                                    await Utility.PushOrdersToOrderPort(payloadModelArray, item.OrderPortApiKey, item.OrderPortApiToken, item.OrderPortClientId, rsvp.Id);
                                }
                                else if (exportType == Common.Common.ExportType.Shopify)
                                {
                                    logDAL.InsertLog("TaskRsvpSync", "Start ExportType 'Shopify' RsvpId:" + rsvp.Id.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, item.Id);
                                }
                                else if (exportType == Common.Common.ExportType.BigCommerce)
                                {
                                    logDAL.InsertLog("TaskRsvpSync", "Start ExportType 'BigCommerce' RsvpId:" + rsvp.Id.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, item.Id);

                                    bool IsAdmin = Convert.ToBoolean(Request.Headers["IsAdmin"]);
                                    ReservationDetailModel reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(rsvp.Id, "", IsAdmin);

                                    WineryModel memberModel = eventDAL.GetWineryById(reservationDetailModel.member_id);

                                    string CustId = await Task.Run(() => Utility.CheckAndUpdateBigCommerceCustomer(memberModel, reservationDetailModel.member_id, reservationDetailModel.user_detail.email));

                                    if (!string.IsNullOrEmpty(CustId))
                                        externalOrderId = await Task.Run(() => Utility.BigCommerceCreateOrder(memberModel.BigCommerceStoreId, memberModel.BigCommerceAceessToken, reservationDetailModel, CustId));
                                }
                            }
                        }

                    }
                }

                baseResponse.success = true;
            }
            catch (Exception ex)
            {
                baseResponse.success = false;
                baseResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                baseResponse.error_info.extra_info = Common.Common.InternalServerError;
                baseResponse.error_info.description = ex.Message.ToString();


                logDAL.InsertLog("WebApi", "TaskRsvpSync:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(baseResponse);
        }

        [Route("salestaxpercentbylocationid")]
        [HttpGet]
        public async Task<IActionResult> GetSalesTaxPercentByLocationId(int location_id, int floor_plan_id)
        {
            var salesTaxPercentResponse = new SalesTaxPercentResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                if (location_id == 0 && floor_plan_id > 0)
                    location_id = eventDAL.GetLocationIdByFloorPlanId(floor_plan_id);

                Utility objUtility = new Utility();
                decimal taxPercent = await objUtility.GetTaxByLocationId(location_id);

                salesTaxPercentResponse.data = taxPercent;
            }
            catch (Exception ex)
            {
                salesTaxPercentResponse.success = false;
                salesTaxPercentResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                salesTaxPercentResponse.error_info.extra_info = Common.Common.InternalServerError;
                salesTaxPercentResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetSalesTaxPercentByLocationId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(salesTaxPercentResponse);
        }

        [Route("verifyformobile")]
        [HttpGet]
        public async Task<IActionResult> VerifyForMobile(string mobile_number)
        {
            var verifyForMobileResponse = new VerifyForMobileResponse();
            try
            {
                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

                int UserId = 0;

                VerifyForMobileModel model = new VerifyForMobileModel();

                model.sms_Verified = false;

                MobileNumberStatus mobile_number_status = userDAL.GetMobilePhoneStatusByMobilePhoneNumber(Utility.ExtractPhone(mobile_number).ToString(), ref UserId);

                if (mobile_number_status == MobileNumberStatus.unverified)
                {
                    mobile_number_status = Utility.SMSVerified_System(mobile_number);

                    if (mobile_number_status != MobileNumberStatus.unverified)
                        userDAL.UpdateMobilePhoneStatusById(mobile_number, (int)mobile_number_status);
                }

                if (mobile_number_status == MobileNumberStatus.verified)
                    model.sms_Verified = true;

                verifyForMobileResponse.data = model;

            }
            catch (Exception ex)
            {
                verifyForMobileResponse.success = false;
                verifyForMobileResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                verifyForMobileResponse.error_info.extra_info = Common.Common.InternalServerError;
                verifyForMobileResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "VerifyForMobile:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(verifyForMobileResponse);
        }

        [Route("recalculatereservationtotal")]
        [HttpPost]
        public async Task<IActionResult> RecalculateReservationTotal([FromBody]RecalculateReservationTotalRequest request)
        {
            RecalculateReservationTotalResponse response = new RecalculateReservationTotalResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            if (request.reservation_id <= 0)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Invalid Reservation Id. Please pass a valid one.";
                response.error_info.description = "Invalid Reservation Id. Please pass a valid one.";
                return new ObjectResult(response);
            }

            if (request.party_size <= 0)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Invalid Member Id. Please pass a valid one.";
                response.error_info.description = "Invalid Member Id. Please pass a valid one.";
                return new ObjectResult(response);
            }

            int member_id = 0;
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                bool IsAdmin = Convert.ToBoolean(Request.Headers["IsAdmin"]);
                ReservationDetailModel reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(request.reservation_id, "", IsAdmin);

                if (reservationDetailModel != null && reservationDetailModel.reservation_id > 0)
                {
                    CreateReservationModel createReservationModel = new CreateReservationModel();

                    RecalculateReservationTotalModel model = new RecalculateReservationTotalModel();

                    if (request.party_size != reservationDetailModel.total_guests)
                    {
                        member_id = reservationDetailModel.member_id;

                        decimal rsvpTotal = reservationDetailModel.fee_per_person * request.party_size;

                        if (reservationDetailModel.fee_type == 1)
                            rsvpTotal = reservationDetailModel.fee_per_person;

                        decimal addOnTotal = 0;
                        foreach (var item in reservationDetailModel.reservation_addon)
                        {
                            addOnTotal += item.price * item.qty;
                        }

                        decimal FeeDue = rsvpTotal + reservationDetailModel.sales_tax + addOnTotal - reservationDetailModel.discount_amount;

                        if (FeeDue < 0)
                        {
                            FeeDue = 0;
                        }

                        decimal refundAmount = 0;
                        decimal amountToCharge = 0;

                        if (FeeDue > 0 && reservationDetailModel.amount_paid < FeeDue)
                            amountToCharge = (FeeDue - reservationDetailModel.amount_paid);
                        else if (FeeDue > 0 && reservationDetailModel.amount_paid > 0 && reservationDetailModel.amount_paid > FeeDue)
                            refundAmount = (reservationDetailModel.amount_paid - FeeDue);

                        model.additional_amount_due = amountToCharge;
                        model.new_balance_due = FeeDue - reservationDetailModel.amount_paid;
                        model.refund_due = refundAmount;
                    }

                    response.data = model;
                    response.success = true;
                }
                else
                {
                    response.success = false;
                    response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    response.error_info.extra_info = "Invalid Reservation Id. Please pass a valid one.";
                    response.error_info.description = "Invalid Reservation Id. Please pass a valid one.";
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("WebApi", string.Format("RecalculateReservationTotal:  reservation_id:{0}, party_size:{1}, error:{2}", request.reservation_id, request.party_size, ex.Message.ToString()), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }

            return new ObjectResult(response);
        }


        [Route("updatereservationseatedstatus")]
        [HttpPost]
        public async Task<IActionResult> UpdateReservationSeatedStatus([FromBody]UpdateReservationSeatedStatusRequest request)
        {
            UpdateReservationSeatedStatusResponse response = new UpdateReservationSeatedStatusResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);

            if (request.reservation_id <= 0)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Invalid Reservation Id. Please pass a valid one.";
                response.error_info.description = "Invalid Reservation Id. Please pass a valid one.";
                return new ObjectResult(response);
            }

            try
            {
                ReservationSeatedStatus originalSeatedStatus = ReservationSeatedStatus.UNSEATED;
                ReservationSeatedStatus currentSeatedStatus = (ReservationSeatedStatus)request.reservation_seated_status;

                UpdateReservationSeatedStatusModel model = new UpdateReservationSeatedStatusModel();

                model.reservation_id = request.reservation_id;
                model.reservation_seated_status = request.reservation_seated_status;

                if (currentSeatedStatus == ReservationSeatedStatus.CLOSED)
                    model.reservation_seated_status_desc = "CLOSED";
                else if (currentSeatedStatus == ReservationSeatedStatus.SEATED)
                    model.reservation_seated_status_desc = "SEATED";


                ReservationDetailV2Model rsvpModel = eventDAL.GetReservationDetailV2byReservationId(request.reservation_id);

                originalSeatedStatus = (ReservationSeatedStatus)rsvpModel.seated_status;

                bool ret = eventDAL.UpdateReservationSeatedStatus(request.reservation_id, request.reservation_seated_status);

                if (ret)
                {

                    int invMode = userDAL.GetInventoryModeForMember(rsvpModel.member_id);

                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var model1 = new CreateDeltaRequest();
                    model1.item_id = request.reservation_id;
                    model1.item_type = (int)ItemType.Reservations;
                    model1.location_id = rsvpModel.location_id;
                    model1.member_id = rsvpModel.member_id;
                    model1.action_date = rsvpModel.event_start_date.Date;
                    int floorPlanId = 0;
                    if (invMode == 1)
                    {
                        floorPlanId = eventDAL.GetFloorPlanIdForReservation(request.reservation_id);
                    }
                    model1.floor_plan_id = floorPlanId;
                    notificationDAL.SaveDelta(model1);

                    model.update_status = 1;

                    string originalSeated = "UNSEATED";

                    if (originalSeatedStatus == ReservationSeatedStatus.CLOSED)
                        originalSeated = "CLOSED";
                    else if (originalSeatedStatus == ReservationSeatedStatus.SEATED)
                        originalSeated = "SEATED";

                    string currentSeated = model.reservation_seated_status_desc;

                    model.message = string.Format("Status changed from {0} to {1}", originalSeated, model.reservation_seated_status_desc);
                    model.update_status_desc = "SUCCESS";
                    response.success = true;
                }
                else
                {
                    model.update_status = 0;
                    model.message = "Failed to update reservation status";
                    model.update_status_desc = "FAILED";
                    response.success = false;
                }

                response.data = model;

            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("WebApi", string.Format("UpdateReservationSeatedStatus:  reservation_id:{0}, error:{1}", request.reservation_id, ex.Message.ToString()), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }

            return new ObjectResult(response);
        }

        [Route("addupdatecreditcard")]
        [HttpPost]
        public async Task<IActionResult> AddUpdateCreditCardReservation([FromBody]ReservationAddCreditCardRequest request)
        {
            BaseResponse response = new BaseResponse();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            if (request.member_id <= 0)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Invalid Member Id. Please pass a valid one.";
                response.error_info.description = "Invalid Member Id. Please pass a valid one.";
                return new ObjectResult(response);
            }
            if (request.reservation_id <= 0)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                response.error_info.extra_info = "Reservation Id is required.";
                response.error_info.description = "Reservation Id is required.";
                return new ObjectResult(response);
            }

            if (request.member_id <= 0 || string.IsNullOrWhiteSpace(request.number) || ((request.number + "").Length < 12 && string.IsNullOrEmpty(request.card_token)))
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                response.error_info.extra_info = "";
                response.error_info.description = "Invalid credit card information";
                return new ObjectResult(response);
            }

            if (string.IsNullOrWhiteSpace(request.exp_year) || request.exp_year.Length > 4)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                response.error_info.extra_info = "";
                response.error_info.description = "Invalid credit card Year";
                return new ObjectResult(response);
            }
            if (string.IsNullOrWhiteSpace(request.exp_month) || request.exp_month.Length > 2)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                response.error_info.extra_info = "";
                response.error_info.description = "Invalid credit card month";
                return new ObjectResult(response);
            }

            if (Convert.ToDateTime(request.exp_month + "/" + "1/" + request.exp_year).AddMonths(1) < DateTime.UtcNow.Date)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                response.error_info.extra_info = "";
                response.error_info.description = "Expired date card";
                return new ObjectResult(response);
            }

            var winery = new Model.WineryModel();
            winery = eventDAL.GetWineryById(request.member_id);

            request.number = request.number.Replace(" ", "").Replace("-", "");

            string cardtype = request.card_type;

            if (string.IsNullOrEmpty(cardtype))
                cardtype = Services.Payments.GetCardType(request.number);

            string EnabledCreditCards = ',' + winery.EnabledCreditCards + ',';

            if (!(((EnabledCreditCards.IndexOf(",2,") > -1) && cardtype.ToLower().IndexOf("visa") > -1) || ((EnabledCreditCards.IndexOf(",4,") > -1) && cardtype.ToLower().IndexOf("american") > -1) || ((EnabledCreditCards.IndexOf(",1,") > -1) && cardtype.ToLower().IndexOf("master") > -1) || ((EnabledCreditCards.IndexOf(",8,") > -1) && cardtype.ToLower().IndexOf("diners") > -1) || ((EnabledCreditCards.IndexOf(",32,") > -1) && cardtype.ToLower().IndexOf("discover") > -1)))
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                response.error_info.extra_info = "";
                response.error_info.description = "Sorry, " + winery.DisplayName + " does not accept " + cardtype + ".";
                return new ObjectResult(response);
            }

            try
            {
                TokenizedCard card = null;
                Model.PaymentConfigModel paymentConfig = new Model.PaymentConfigModel();
                paymentConfig = eventDAL.GetPaymentConfigByWineryId(request.member_id);

                if (paymentConfig != null && paymentConfig.PaymentGateway != Configuration.Gateway.Offline)
                {
                    if (!string.IsNullOrEmpty(request.card_token))
                    {
                        card = new TokenizedCard
                        {
                            card_token = request.card_token,
                            last_four_digits = Common.Common.Right(request.number, 4),
                            first_four_digits = Common.Common.Left(request.number, 4),
                            is_expired = false,
                            customer_name = request.cust_name + "",
                            card_type = cardtype
                        };
                    }
                    else
                    {
                        TokenizedCardRequest tokenizedCardRequest = new TokenizedCardRequest();

                        tokenizedCardRequest.ignore_avs_error = true;
                        tokenizedCardRequest.member_id = request.member_id;
                        tokenizedCardRequest.card_type = cardtype;
                        tokenizedCardRequest.cust_name = request.cust_name;
                        tokenizedCardRequest.cvv2 = request.cvv2;
                        tokenizedCardRequest.exp_month = request.exp_month;
                        tokenizedCardRequest.exp_year = request.exp_year;
                        tokenizedCardRequest.number = request.number;
                        tokenizedCardRequest.source_module = ModuleType.Reservation;
                        //tokenizedCardRequest.user_info.address.zip_code = request.zip_code;

                        Services.Payments objPayments = new Services.Payments(_appSetting);
                        card = Services.Payments.TokenziedCard(tokenizedCardRequest, paymentConfig);
                    }
                    

                    if (card != null && !string.IsNullOrEmpty(card.card_token))
                    {
                        //model.pay_card.card_first_four_digits = card.first_four_digits;
                        //model.pay_card.card_last_four_digits = card.last_four_digits;
                        //createReservationModel.PayCardFirstFourDigits = card.first_four_digits;
                        //createReservationModel.PayCardLastFourDigits = card.last_four_digits;

                        bool isSuccess = eventDAL.AddCreditCardReservation(request.reservation_id, request.zip_code, cardtype, StringHelpers.Encryption(request.number), request.cust_name, request.exp_month, request.exp_year, card.card_token);

                        if (string.IsNullOrEmpty(request.card_token))
                        {
                            if ((winery.EnableVin65 || winery.EnableClubVin65) && !string.IsNullOrEmpty(card.card_token) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(winery.SALT) && !string.IsNullOrEmpty(winery.DecryptKey))
                            {
                                string cardtype2 = Services.Payments.GetCardType(request.number, "vin65");
                                string cardnumber = Common.StringHelpers.EncryptedCardNumber(request.number, winery.SALT, winery.DecryptKey);
                                eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, request.member_id, winery.Vin65UserName, winery.Vin65Password,request.cvv2);
                            }
                            else if ((winery.EnableCommerce7 || winery.EnableClubCommerce7) && winery.EnableUpsertCreditCard && !string.IsNullOrEmpty(card.card_token))
                            {
                                string cardtype2 = Services.Payments.GetCardType(request.number, "commerce7");
                                string cardnumber = Common.Common.Right(request.number, 4).PadLeft(request.number.Length, '*');

                                string gateway = "No Gateway";
                                gateway = Utility.GetCommerce7PaymentGatewayName(paymentConfig.PaymentGateway);

                                eventDAL.InsertTempCardDetail(cardtype2, cardnumber, request.cust_name, request.exp_month, request.exp_year, card.card_token, request.member_id, gateway, "", request.cvv2);

                                try
                                {
                                    var reservationDetailModel = new ReservationDetailModel();
                                    reservationDetailModel = eventDAL.GetReservationDetailsbyReservationId(request.reservation_id);

                                    QueueService getStarted = new QueueService();

                                    var queueModel = new EmailQueue();
                                    queueModel.EType = (int)EmailType.CreateThirdPartyContact;
                                    queueModel.BCode = request.reservation_id.ToString();
                                    queueModel.UId = reservationDetailModel.user_detail.user_id;
                                    queueModel.RsvpId = request.member_id;
                                    queueModel.PerMsg = "";
                                    queueModel.Src = reservationDetailModel.referral_type;
                                    var qData = JsonConvert.SerializeObject(queueModel);

                                    AppSettings _appsettings = _appSetting.Value;
                                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                                }
                                catch (Exception ex)
                                {
                                    logDAL.InsertLog("WebApi", "AddUpdateCreditCardReservation Create Third Party Contact:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, request.member_id);
                                }
                            }
                        }
                        
                        response.success = isSuccess;
                    }
                    else
                    {
                        response.error_info.error_type = (int)Common.Common.ErrorType.CreditCard;
                        response.error_info.extra_info = card.ErrorCode;
                        response.error_info.description = card.ErrorMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                logDAL.InsertLog("WebApi", string.Format("AddCreditCardReservation:  member_id:{0}, reservation_id:{1}, error:{2}", request.member_id, request.reservation_id, ex.Message.ToString()), HttpContext.Request.Headers["AuthenticateKey"], 1, request.member_id);
            }

            return new ObjectResult(response);
        }

        [Route("rsvpreminderv2")]
        [HttpGet]
        public async Task<IActionResult> TaskRsvpReminderV2()
        {
            BaseResponse response = new BaseResponse();

            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
            int ReminderHours = 24;
            DateTime CurrentDate = Times.ToTimeZoneTime(DateTime.UtcNow);
            int successCount = 0;

            List<ReservationDetailModel> list = eventDAL.GetReservationsV2ForReminderTask(CurrentDate, ReminderHours);

            if (list != null && list.Count > 0)
            {
                logDAL.InsertLog("WebApi", "TaskRsvpReminderV2: Begin Processing: Start Count: " + list.Count.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, 0);
                foreach (var request in list)
                {
                    try
                    {
                        QueueService getStarted = new QueueService();

                        var queueModel = new EmailQueue();
                        queueModel.EType = (int)EmailType.RsvpReminder;
                        queueModel.BCode = request.booking_code;
                        queueModel.UId = request.member_id;
                        queueModel.RsvpId = request.reservation_id;
                        queueModel.PerMsg = "";
                        queueModel.Src = 0;
                        var qData = JsonConvert.SerializeObject(queueModel);

                        AppSettings _appsettings = _appSetting.Value;
                        getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();

                        successCount += 1;
                    }
                    catch (Exception ex)
                    {
                        response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                        response.error_info.extra_info = Common.Common.InternalServerError;
                        response.error_info.description = ex.Message.ToString();

                        logDAL.InsertLog("WebApi", "TaskRsvpReminderV2:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
                    }
                }

                logDAL.InsertLog("WebApi", "TaskRsvpReminderV2: End Processing: Success Count: " + successCount.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 3, 0);
            }

            return new ObjectResult(response);
        }

        [Route("getpassportguestlistbyeventandmember")]
        [HttpGet]
        public async Task<IActionResult> GetPassportGuestlistByEventandMember(int ticket_event_id, int member_id, DateTime start_date_time, DateTime end_date_time)
        {
            var reservationResponse = new ReservationResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                var reservationModel = new List<ReservationEvent>();

                reservationModel = eventDAL.GetPassportGuestlistByEventandMember(ticket_event_id, member_id, start_date_time, end_date_time);

                if (reservationModel != null)
                {
                    reservationResponse.success = true;
                    reservationResponse.data = reservationModel;
                }
                else
                {
                    reservationResponse.success = true;
                    reservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetPassportGuestlistByEventandMember:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(reservationResponse);
        }

        [Route("setcancellationreason")]
        [HttpPost]
        public async Task<IActionResult> SetCancellationReason([FromBody] ReservationCancellationReasonRequest model)
        {
            bool ret = false;
            var reservationStatusResponse = new ReservationCancellationReasonResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                ret = eventDAL.UpdateReservationV2CancellationReason(model.reservation_id, model.reason_id);

            }
            catch (Exception ex)
            {
                reservationStatusResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationStatusResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationStatusResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SetCancellationReason:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
            }
            return new JsonResult(reservationStatusResponse);
        }

        [Route("get_rsvp_reviews")]
        [HttpGet]
        public IActionResult GetRSVPReviews(int topn = 4, int region_id = 0)
        {
            var response = new RSVPReviewsResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var rsvpReviews = eventDAL.GetRSVPReviews(topn, region_id);

                if (rsvpReviews != null && rsvpReviews.Count > 0)
                {
                    response.data = rsvpReviews;
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
                logDAL.InsertLog("WebApi", "GetRSVPReviews:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }

        [Route("reservationsdata")]
        [HttpGet]
        public async Task<IActionResult> GetReservationsDataByFilters(ReservationRequest model)
        {
            var reservationResponse = new ReservationDataResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationModel = new List<ReservationDataEvent>();

                string WhereClause = " where r.Status not in (7,8)";

                if (model.member_id > 0)
                {
                    WhereClause += " And r.WineryId=" + model.member_id;
                }

                if (model.location_ids.Length > 0)
                {
                    WhereClause += " And r.LocationId in (" + model.location_ids + ")";
                }

                if (model.user_id > 0)
                {
                    WhereClause += " And r.userId=" + model.user_id;
                }

                if (model.reservation_id > 0)
                {
                    WhereClause += " And r.reservationId=" + model.reservation_id;
                }

                if (model.email.Trim().Length > 0)
                {
                    WhereClause += " And r.email='" + model.email + "'";
                }

                if (model.last_name.Trim().Length > 0)
                {
                    WhereClause += " And r.LastName like '" + model.last_name + "%'";
                }

                if (model.phone_number.Trim().Length > 0)
                {
                    WhereClause += " And REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(PhoneNumber,'+1',''),'1 (',''),')',''),'(',''),'+',''),'-',''),' ','') like '%" + Utility.ExtractPhone(model.phone_number).ToString() + "%'";
                }

                if (model.booking_code.Trim().Length > 0)
                {
                    WhereClause += " And r.bookingCode='" + model.booking_code + "'";
                }

                if (model.event_name.Trim().Length > 0)
                {
                    WhereClause += " And r.eventName='" + model.event_name + "'";
                }

                if (model.start_date != null && model.end_date != null)
                {
                    DateTime starttime = Convert.ToDateTime(model.start_date);
                    DateTime endtime = Convert.ToDateTime(model.end_date);

                    if (model.mode == 0)
                    {
                        WhereClause += " And (CAST(r.eventdate AS datetime) + CAST(r.starttime AS datetime)) >='" + starttime.ToString("yyyy-MM-dd hh:mm tt") + "' And (CAST(r.eventdate AS datetime) + CAST(r.starttime AS datetime)) <='" + endtime.ToString("yyyy-MM-dd hh:mm tt") + "'";
                    }
                    else
                    {
                        WhereClause += " And r.BookingDate >='" + starttime.ToString("yyyy-MM-dd hh:mm tt") + "' And r.BookingDate <='" + endtime.ToString("yyyy-MM-dd hh:mm tt") + "'";
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.floor_plan_ids))
                {
                    WhereClause += " And (r.FloorPlanId in (" + model.floor_plan_ids.Trim() + ") or Isnull(ss.Floor_Plan_Id, 0) in (" + model.floor_plan_ids.Trim() + ")) ";
                }

                reservationModel = eventDAL.GetReservationsDataByFilters(WhereClause);

                if (reservationModel != null)
                {
                    foreach (var item in reservationModel)
                    {
                        foreach (var item1 in item.event_times)
                        {
                            foreach (var item2 in item1.reservations)
                            {
                                if (!string.IsNullOrWhiteSpace(item2.reservation_holder.phone))
                                    item2.reservation_holder.phone = Utility.FormatTelephoneNumber(item2.reservation_holder.phone, item2.country);
                            }
                        }
                    }

                    reservationResponse.success = true;
                    reservationResponse.data = reservationModel;
                }
                else
                {
                    reservationResponse.success = true;
                    reservationResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetReservationsDataByFilters:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
            }
            return new ObjectResult(reservationResponse);
        }

        [Route("getmaphtmlforlocation")]
        [HttpGet]
        public async Task<IActionResult> GenerateMapForLocation(int location_id)
        {
            var response = new GetLocationMapResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                var imageMapData = Task.Run(() => Utility.GetMapImageHtmlByLocation(location_id, _appSetting.Value.GoogleAPIKey)).Result;

                if (!string.IsNullOrWhiteSpace(imageMapData))
                {
                    response.success = true;
                    response.data = new GetLocationMapViewModel
                    {
                        map_html = imageMapData
                    };
                }
                else
                {
                    response.success = false;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no location data found";
                }
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GenerateMapForLocation:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }

        [Route("getreservationinvitebyrsvpid")]
        [HttpGet]
        public async Task<IActionResult> GetReservationInviteByRsvpId(string reservation_invite_guid = "", int member_id = 0)
        {
            var response = new ReservationInviteModelResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                ReservationInviteModel model = new ReservationInviteModel();
                model = eventDAL.GetReservationInviteByRsvpId(0, reservation_invite_guid);

                if (model != null && model.id > 0)
                {
                    response.success = true;

                    model.member_phone = Utility.FormatTelephoneNumber(model.member_phone, "US");

                    response.data = model;
                }
                else
                {
                    model.status = ReservationInviteStatus.Invalid;
                    if (member_id > 0)
                    {
                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(model.member_id, (int)Common.Common.SettingGroup.member);

                        model.member_phone = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_rsvp_contact_phone);

                        if (model.member_phone.Length == 0)
                        {
                            var winery = new WineryModel();
                            winery = eventDAL.GetWineryById(member_id);

                            model.member_phone = Utility.FormatTelephoneNumber(winery.BusinessPhone.ToString(), "US");
                        }
                    }
                    response.data = model;

                    response.success = false;
                    response.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    response.error_info.extra_info = "no location data found";
                }
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationInviteByRsvpId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }

        [Route("savereservationinvite")]
        [HttpPost]
        public async Task<IActionResult> SaveReservationInvite([FromBody] CreateReservationInvite model)
        {
            var saveReservationInviteResponse = new BaseResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                eventDAL.SaveReservationInvite(model);
                saveReservationInviteResponse.success = true;
            }
            catch (Exception ex)
            {
                saveReservationInviteResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                saveReservationInviteResponse.error_info.extra_info = Common.Common.InternalServerError;
                saveReservationInviteResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SaveReservationInvite:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(saveReservationInviteResponse);
        }

        [Route("setreservationinvitestatus")]
        [HttpPost]
        public async Task<IActionResult> SetReservationInviteStatus([FromBody] SetReservationInviteStatusRequest model)
        {
            var setReservationInviteStatusResponse = new BaseResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                eventDAL.SetReservationInviteStatus(model);
                setReservationInviteStatusResponse.success = true;

            }
            catch (Exception ex)
            {
                setReservationInviteStatusResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                setReservationInviteStatusResponse.error_info.extra_info = Common.Common.InternalServerError;
                setReservationInviteStatusResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SetReservationInviteStatus:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1,0);
            }
            return new JsonResult(setReservationInviteStatusResponse);
        }

        [Route("setreservationinvitereminderstatus")]
        [HttpPost]
        public async Task<IActionResult> SetReservationInviteReminderStatus([FromBody] SetReservationInviteReminderStatusRequest model)
        {
            var setReservationInviteStatusResponse = new BaseResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                eventDAL.SetReservationInviteReminderStatus(model.reservation_id, model.reservation_invite_guid);
                setReservationInviteStatusResponse.success = true;

            }
            catch (Exception ex)
            {
                setReservationInviteStatusResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                setReservationInviteStatusResponse.error_info.extra_info = Common.Common.InternalServerError;
                setReservationInviteStatusResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SetReservationInviteReminderStatus:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(setReservationInviteStatusResponse);
        }

        [Route("sendreservationinvitereminderemail")]
        [HttpPost]
        public async Task<IActionResult> SendReservationInvite()
        {
            var response = new BaseResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                foreach (var item in eventDAL.GetReservationIdForInviteReminder())
                {
                    QueueService getStarted = new QueueService();

                    var queueModel = new EmailQueue();
                    queueModel.EType = (int)EmailType.InviteReminder;
                    queueModel.RsvpId = item;
                    queueModel.Src = (int)ActionSource.BackOffice;
                    var qData = JsonConvert.SerializeObject(queueModel);

                    AppSettings _appsettings = _appSetting.Value;
                    getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                }

                response.success = true;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SendReservationInvite:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(response);
        }

        [Route("updatereservationtags")]
        [HttpPost]
        public async Task<IActionResult> UpdateReservationTags([FromBody]ReservationTagsRequest model)
        {
            int member_id = 0;
            var response = new BaseResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                bool ret = eventDAL.UpdateReservationTags(model.reservation_id, model.tags);
                response.success = true;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateReservationTags:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(response);
        }

        [Route("updateusertags")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserTags([FromBody]UserTagsRequest model)
        {
            int member_id = model.member_id;
            var response = new BaseResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                eventDAL.DeleteAllUserTags(model.user_id, model.member_id);

                foreach (var item in model.tags)
                {
                    eventDAL.SaveUserTags(model.user_id, model.member_id, item);
                }

                response.success = true;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateUserTags:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new JsonResult(response);
        }

        [Route("reservationdetailv3")]
        [HttpGet]
        public async Task<IActionResult> GetReservationDetailByGuid(string booking_guid)
        {
            var reservationDetailResponse = new ReservationDetailV3Response();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);

                var reservationDetailModel = new ReservationDetailV3Model();
                reservationDetailModel = eventDAL.GetReservationDetailByGuid(booking_guid);

                if (reservationDetailModel != null && reservationDetailModel.reservation_id > 0)
                {
                    reservationDetailResponse.success = true;
                    reservationDetailResponse.data = reservationDetailModel;
                }
                else
                {
                    reservationDetailResponse.success = true;
                    reservationDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    reservationDetailResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                reservationDetailResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                reservationDetailResponse.error_info.extra_info = Common.Common.InternalServerError;
                reservationDetailResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("WebApi", "GetReservationDetailByGuid: Booking_guid-" + booking_guid + ", Message-" + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(reservationDetailResponse);
        }

        [Route("deleteexportfiletostorage")]
        [HttpPost]
        public async Task<IActionResult> DeleteExportFileToStorage()
        {
            var response = new BaseResponse();
            try
            {
                int DeleteExportFileDays = _appSetting.Value.DeleteExportFileDays;

                Utility.DeleteExportFileToStorage(DeleteExportFileDays);

                response.success = true;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "DeleteExportFileToStorage:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new JsonResult(response);
        }

        [Route("savereservationreview")]
        [HttpPost]
        public IActionResult SaveReservationReview([FromBody] ReservationReviewRequest reqmodel)
        {
            var resp = new BaseResponse();
            if (string.IsNullOrWhiteSpace(reqmodel.booking_guid))
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Invalid Reservation Guid. A valid Reservation Guid is required";
                resp.error_info.description = "Invalid Reservation Guid. A valid Reservation Guid is required";
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
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            try
            {
                bool isSuccess = eventDAL.SaveReservationReview(reqmodel);
                resp.success = isSuccess;

            }
            catch (Exception ex)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SaveReservationReview:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(resp);
        }

        [Route("updatereservationchangesbyeventid")]
        [HttpPost]
        public async Task<IActionResult> UpdateReservationChangesByEventId([FromBody] UpdateReservationChangesByEventIdModel reqmodel)
        {
            var baseResponse = new BaseResponse2();

            try
            {
                baseResponse.success = true;

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                eventDAL.UpdateReservationChangesByEventId(reqmodel.event_id, reqmodel.all_inclusive_price, reqmodel.all_inclusive_min_price, reqmodel.all_inclusive_max_price);
            }
            catch (Exception ex)
            {
                baseResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                baseResponse.error_info.extra_info = Common.Common.InternalServerError;
                baseResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateReservationChangesByEventId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, 0);
            }
            return new ObjectResult(baseResponse);
        }

        [Route("updatereservationchanges")]
        [HttpGet]
        public async Task<IActionResult> UpdateReservationChanges()
        {
            BaseResponse2 response = new BaseResponse2();

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            List<EventDetails> list = eventDAL.GetActiveEventId();

            if (list != null && list.Count > 0)
            {
                foreach (var request in list)
                {
                    AuthMessageSender messageService = new AuthMessageSender();
                    ReservationEmailModel model = new ReservationEmailModel();

                    model.RsvpId = request.event_id;

                    await Task.Run(() => messageService.ProcessReservationChangesUpdate(model));
                    //QueueService getStarted = new QueueService();

                    //var queueModel = new EmailQueue();
                    //queueModel.EType = (int)EmailType.ReservationChangesUpdate;
                    //queueModel.BCode = "";
                    //queueModel.UId = request.event_id;
                    //queueModel.RsvpId = request.event_id;
                    //queueModel.PerMsg = "";
                    //queueModel.Src = 0;
                    //var qData = JsonConvert.SerializeObject(queueModel);

                    //AppSettings _appsettings = _appSetting.Value;
                    //getStarted.AddMessageIntoQueue(_appsettings, qData).Wait();
                }
            }

            return new ObjectResult(response);
        }

        [Route("userreservation")]
        [HttpGet]
        public IActionResult GetUserReservations(int user_id,DateTime? to_date = null, DateTime? from_date = null)
        {
            var response = new UserReservationsResponse();
            try
            {
                if (to_date == null || to_date == default(DateTime))
                    to_date = DateTime.Now.AddDays(1);

                if (from_date == null || from_date == default(DateTime))
                    from_date = DateTime.Now.AddDays(-365);

                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                List<UserReservationsModel> userReservations = eventDAL.GetUserReservations(user_id, to_date, from_date);

                if (userReservations != null && userReservations.Count > 0)
                {
                    response.data = userReservations;
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
                logDAL.InsertLog("WebApi", "GetUserReservations:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }

        [Route("businesssearch")]
        [HttpGet]
        public IActionResult GetBusinessSearch(string search_term)
        {
            var response = new BusinessSearchResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString_readonly);
                List<BusinessSearchResultModel> list = eventDAL.GetBusinessSearchResult(search_term);

                if (list != null && list.Count > 0)
                {
                    response.data = list;
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
                logDAL.InsertLog("WebApi", "GetBusinessSearch:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1);
            }
            return new ObjectResult(response);
        }
    }
}