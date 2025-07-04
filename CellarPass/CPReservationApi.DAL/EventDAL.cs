using CPReservationApi.Common;
using CPReservationApi.Model;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using static CPReservationApi.Common.Common;
using static CPReservationApi.Common.Email;
using static CPReservationApi.Common.Payments;
using uc = CPReservationApi.Common;
//using Stripe;

namespace CPReservationApi.DAL
{
    public class EventDAL : BaseDataAccess
    {
        public EventDAL(string connectionString) : base(connectionString)
        {
        }

        public EventSchedule GetConsumerEventScheduleV2(int wineryId, DateTime StartTime, int guestCount, int SlotId = 0, int SlotType = 0, int EventId = 0, bool ISAdmin = true, int reservationid = 0, int bookingtype = 0, bool hide_no_availability = false)
        {
            //int oldbookingtype = bookingtype;
            //var eventScheduleTime = new EventScheduleTime();

            if (bookingtype == 2)
            {
                //eventScheduleTime = GetSeatLeftEventScheduleV2(wineryId, StartTime, guestCount, SlotId, SlotType, EventId, ISAdmin, reservationid, bookingtype);
                bookingtype = 0;
            }

            string sp = "GetEventbyWineryIdAdminV2";

            var eventSchedule = new EventSchedule();
            var eventScheduleEvent = new List<EventScheduleEvent>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@GuestsCount", guestCount));
            parameterList.Add(GetParameter("@reservationid", reservationid));
            parameterList.Add(GetParameter("@bookingtype", bookingtype));

            if (!ISAdmin)
            {
                parameterList.Add(GetParameter("@SlotId", SlotId));
                parameterList.Add(GetParameter("@SlotType", SlotType));
                parameterList.Add(GetParameter("@callbtnSearch", false));
                parameterList.Add(GetParameter("@EventIdnum", EventId));

                sp = "GetEventsByWineryIdAndDateClientV2";
            }
            else
                parameterList.Add(GetParameter("@ShowAllEvent", !hide_no_availability));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    var winery = new WineryModel();
                    winery = GetWineryById(wineryId);
                    List<SettingModel> settingsGroup = null;
                    if (winery.EnableClubemember == false && winery.EnableClubSalesforce == false && winery.EnableClubVin65 == false)
                    {
                        //If none of the previous club feaures are enabled then check if bLoyal is enabled since we don't need to hit db again if one of these is already active
                        settingsGroup = GetSettingGroup(wineryId, uc.Common.SettingGroup.bLoyal);
                    }

                    while (dataReader.Read())
                    {
                        var sched = new EventScheduleEvent();
                        sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                        sched.event_name = Convert.ToString(dataReader["EventName"]);
                        sched.event_location = Convert.ToString(dataReader["EventLocation"]);
                        sched.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        //sched.EventType = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EventTypeId"])) ? 0 : Convert.ToInt32(dataReader["EventTypeId"]);
                        //sched.EventTypeName = Convert.ToString(dataReader["EventTypeName"]);
                        sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                        sched.duration_desc = Convert.ToString(dataReader["Duration"]);
                        sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                        sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        sched.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);
                        sched.lead_time_desc = Convert.ToString(dataReader["LeadTime"]);
                        sched.max_lead_time_desc = Convert.ToInt32(dataReader["MaximumLeadTime"]) + " Days";
                        sched.cancel_time_desc = Convert.ToString(dataReader["CancelTime"]);
                        sched.cancel_lead_time_in_minutes = Convert.ToInt32(dataReader["CancelTimeInMinutes"]);
                        string CancelTimeDesc = Convert.ToString(dataReader["CancelTime"]);

                        sched.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                        sched.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);

                        int MinPersons = 0;
                        int MaxPersons = 0;
                        //if (sp == "GetEventbyWineryIdAdminV2" && bookingtype == 0)
                        //{
                        //    MinPersons = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["MinPersons"])) ? 0 : Convert.ToInt32(dataReader["MinPersons"]);
                        //    MaxPersons = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["MaxPersons"])) ? 0 : Convert.ToInt32(dataReader["MaxPersons"]);
                        //}
                        //sched.min_persons = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["MinPersons"])) ? 0 : Convert.ToInt32(dataReader["MinPersons"]);
                        //sched.max_persons = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["MaxPersons"])) ? 0 : Convert.ToInt32(dataReader["MaxPersons"]);
                        //sched.MaxPerRsvp = Convert.ToInt32(dataReader["MaxPerRsvp"]);

                        //sched.RequestedGuests = guestCount;
                        sched.member_id = wineryId;
                        var ChargeFee = (uc.Common.ChargeFee)(string.IsNullOrWhiteSpace(Convert.ToString(dataReader["ChargeFee"])) ? 0 : Convert.ToInt32(dataReader["ChargeFee"]));
                        //sched.ChargeFee = ChargeFee;
                        //sched.QualifiedPurchaseAmount = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                        decimal QualifiedPurchaseAmount = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                        //sched.MemberBenefit = Convert.ToInt32(dataReader["MemberBenefit"]);
                        int MemberBenefit = Convert.ToInt32(dataReader["MemberBenefit"]);
                        //sched.HolidayName = Convert.ToString(dataReader["HolidayName"]);

                        //sched.ChargeSalesTax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        bool ChargeSalesTax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        string ShipcompliantAPIUserName = Convert.ToString(dataReader["ShipcompliantAPIUserName"]);
                        string ShipcompliantApiPassword = Convert.ToString(dataReader["ShipcompliantApiPassword"]);
                        //sched.EnableShipCompliant = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableShipCompliant"])) ? false : Convert.ToBoolean(dataReader["EnableShipCompliant"]);
                        bool EnableShipCompliant = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableShipCompliant"])) ? false : Convert.ToBoolean(dataReader["EnableShipCompliant"]);
                        sched.location_street1 = Convert.ToString(dataReader["LocationStreet1"]);
                        sched.location_street2 = Convert.ToString(dataReader["LocationStreet2"]);
                        sched.location_city = Convert.ToString(dataReader["LocationCity"]);
                        sched.location_state = Convert.ToString(dataReader["LocationState"]);
                        sched.location_zip = Convert.ToString(dataReader["LocationZip"]);
                        sched.show_additional_guests = Convert.ToBoolean(dataReader["ShowAddlGuests"]);
                        sched.show_guest_tags = Convert.ToBoolean(dataReader["ShowGuestTags"]);
                        sched.require_additional_guests = Convert.ToBoolean(dataReader["ReqAddlGuests"]);
                        sched.table_status_group_id = Convert.ToInt32(dataReader["Table_Status_Group_Id"]);

                        List<EventScheduleTime> eventTimes = new List<EventScheduleTime>();
                        int activeCount = 0;

                        AvlQtyForReservationIdModel qtymodel = new AvlQtyForReservationIdModel();
                        if (reservationid > 0 && ISAdmin)
                        {
                            qtymodel = GetAvlQtyForReservationId(reservationid);

                            if (qtymodel.StartDateTime.ToString("MM/dd/yyyy") != StartTime.ToString("MM/dd/yyyy"))
                                qtymodel = new AvlQtyForReservationIdModel();
                        }

                        if (sched.event_id == 0 && reservationid > 0)
                            sched.event_floor_plans = GetFloorPlanByReservationId(reservationid);
                        else
                            sched.event_floor_plans = GetEventFloorPlansByEventId(sched.event_id);

                        if (Convert.ToString(dataReader["EventTimings"]) != null)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EventTimings"])))
                            {
                                foreach (var t_loopVariable in Convert.ToString(dataReader["EventTimings"]).Split(','))
                                {
                                    string[] timeArray = t_loopVariable.Split('|');
                                    EventScheduleTime eTime = new EventScheduleTime();
                                    eTime.slot_id = Convert.ToInt32(timeArray[1]);
                                    eTime.time = Convert.ToString(timeArray[0]);
                                    if (ISAdmin)
                                    {
                                        eTime.is_available = true;
                                    }
                                    else
                                    {
                                        eTime.is_available = Convert.ToBoolean(Convert.ToInt32(timeArray[2]));
                                    }

                                    eTime.slot_type = Convert.ToInt32(timeArray[3]);

                                    eTime.seats_left = Convert.ToInt32(timeArray[4]);

                                    if (ISAdmin && winery.InventoryMode == 1)
                                    {
                                        eTime.seats_left = Convert.ToInt32(timeArray[4]) + guestCount;
                                    }

                                    if (sp == "GetEventbyWineryIdAdminV2")
                                    {
                                        MinPersons = Convert.ToInt32(timeArray[10]);
                                        MaxPersons = Convert.ToInt32(timeArray[11]);
                                    }
                                    else
                                    {
                                        MinPersons = Convert.ToInt32(timeArray[10]);
                                        MaxPersons = Convert.ToInt32(timeArray[11]);
                                    }

                                    string color = string.Empty;
                                    if (ISAdmin && bookingtype == 0 && sched.event_id > 0)
                                    {
                                        color = Convert.ToString(timeArray[5]);
                                        eTime.event_total_seats = Convert.ToInt32(timeArray[8]);
                                        eTime.event_duration_minutes = Convert.ToInt32(timeArray[9]);
                                        if (Convert.ToDateTime(timeArray[6]) == Convert.ToDateTime(timeArray[7]))
                                            eTime.is_recurring = false;
                                        else
                                            eTime.is_recurring = true;
                                    }
                                    else
                                    {
                                        color = Convert.ToString(timeArray[13]);
                                        eTime.event_total_seats = Convert.ToInt32(timeArray[8]);
                                        eTime.event_duration_minutes = Convert.ToInt32(timeArray[14]);
                                        if (Convert.ToDateTime(timeArray[5]) == Convert.ToDateTime(timeArray[6]))
                                            eTime.is_recurring = false;
                                        else
                                            eTime.is_recurring = true;
                                    }

                                    eTime.slot_color = 3;

                                    if (color.Contains("autoclosedevent"))
                                    {
                                        eTime.slot_color = 1;
                                    }
                                    else if (color.Contains("closedevent"))
                                    {
                                        eTime.slot_color = 0;
                                    }
                                    else if (color.Contains("privateevent"))
                                    {
                                        eTime.slot_color = 2;
                                    }
                                    else if (color.Contains("nobookings"))
                                    {
                                        eTime.slot_color = 3;
                                    }
                                    else if (color.Contains("oversold"))
                                    {
                                        eTime.slot_color = 4;
                                    }
                                    else if (color.Contains("soldout"))
                                    {
                                        eTime.slot_color = 5;
                                    }
                                    else if (color.Contains("eventbooked"))
                                    {
                                        eTime.slot_color = 6;
                                    }

                                    if (eTime.is_available && !eventSchedule.isAnyEventAvailable)
                                    {
                                        eventSchedule.isAnyEventAvailable = true;
                                    }

                                    bool addEvent = false;

                                    if (!ISAdmin)
                                        addEvent = true;
                                    else if (bookingtype == 1)
                                        addEvent = true;
                                    else
                                    {
                                        if (hide_no_availability)
                                        {
                                            if (guestCount <= eTime.seats_left && color.Contains("soldout") == false && color.Contains("oversold") == false && color.Contains("closedevent") == false && color.Contains("autoclosedevent") == false && guestCount >= MinPersons && guestCount <= MaxPersons)
                                                addEvent = true;
                                            //if (ISAdmin)
                                            //{
                                            //    if (guestCount <= eTime.seats_left && color.Contains("soldout") == false && color.Contains("oversold") == false && color.Contains("closedevent") == false && color.Contains("autoclosedevent") == false)
                                            //        addEvent = true;
                                            //}
                                            //else
                                            //{
                                            //    if (guestCount <= eTime.seats_left && color.Contains("soldout") == false && color.Contains("oversold") == false && color.Contains("closedevent") == false && color.Contains("autoclosedevent") == false && guestCount >= MinPersons && guestCount <= MaxPersons)
                                            //        addEvent = true;
                                            //}
                                        }
                                        else
                                            addEvent = true;
                                    }

                                    if (reservationid > 0 && qtymodel != null && qtymodel.FloorPlanId > 0)
                                    {
                                        //((StartTime >= r.StartTime And StartTime < r.EndTime) Or (EndTime > r.StartTime And EndTime <= r.EndTime) Or (EndTime > r.EndTime And StartTime < r.StartTime))

                                        DateTime STime = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + eTime.time);
                                        DateTime ETime = Convert.ToDateTime(StartTime.ToString("MM/dd/yyyy") + " " + eTime.time).AddMinutes(eTime.event_duration_minutes);

                                        if ((STime >= qtymodel.StartDateTime && STime < qtymodel.EndDatetime) || (ETime > qtymodel.StartDateTime && ETime <= qtymodel.EndDatetime) || (ETime > qtymodel.EndDatetime && STime < qtymodel.StartDateTime))
                                        {
                                            var floor_plan_id = sched.event_floor_plans.Where(f => f.floor_plan_id == qtymodel.FloorPlanId).Select(f => f.floor_plan_id).FirstOrDefault();

                                            if (floor_plan_id > 0)
                                            {
                                                if (eTime.slot_id == qtymodel.SlotId)
                                                {
                                                    eTime.event_total_seats = eTime.event_total_seats + qtymodel.Qty;
                                                    eTime.seats_left = eTime.event_total_seats - guestCount;
                                                }
                                                else
                                                {
                                                    eTime.seats_left = eTime.seats_left - guestCount;
                                                }
                                            }
                                        }
                                    }

                                    if (addEvent)
                                    {
                                        eventTimes.Add(eTime);
                                        if (eTime.is_available == true)
                                            activeCount += 1;
                                    }

                                }
                            }
                        }

                        sched.event_times = eventTimes;
                        //sched.EventTimesActiveCount = activeCount;
                        sched.min_persons = MinPersons;
                        sched.max_persons = MaxPersons;
                        if (sched.fee_Per_person > 0)
                        {
                            if (ChargeFee == uc.Common.ChargeFee.ComplimentaryWithPurchase)
                            {
                                sched.fee_per_person_desc = string.Format("{0}, Waived with Min. Qualified Purchase ({1} Per Person) ", string.Format(new CultureInfo("en-US"), "{0:C}", sched.fee_Per_person), string.Format(new CultureInfo("en-US"), "{0:C}", QualifiedPurchaseAmount));
                            }
                            else
                            {
                                if (EnableShipCompliant && ChargeSalesTax && !string.IsNullOrEmpty(ShipcompliantApiPassword) && !string.IsNullOrEmpty(ShipcompliantAPIUserName))
                                {
                                    sched.fee_per_person_desc = string.Format(new CultureInfo("en-US"), "{0:C}", sched.fee_Per_person) + " Per Person, plus tax";
                                }
                                else
                                {
                                    sched.fee_per_person_desc = string.Format(new CultureInfo("en-US"), "{0:C}", sched.fee_Per_person) + " Per Person";
                                }
                            }
                        }
                        else
                        {
                            sched.fee_per_person_desc = "Complimentary";
                        }

                        sched.club_member_benefit = uc.Common.GetMemberBenefit().Where(f => f.ID == MemberBenefit.ToString()).Select(f => f.Name).FirstOrDefault();

                        sched.show_club_member_benefits = false;
                        if (sched.member_id > 0)
                        {
                            bool bLoyalClubLookupEnabled = false;

                            if ((settingsGroup != null))
                            {
                                if (settingsGroup.Count > 0)
                                {
                                    bool ret = false;
                                    dynamic dbSettings = settingsGroup.Where(f => f.Key == uc.Common.SettingKey.bLoyalApiClubLookup).FirstOrDefault();

                                    if (dbSettings != null)
                                    {
                                        bool.TryParse(dbSettings.Value, out ret);
                                    }

                                    if (ret == true)
                                    {
                                        bLoyalClubLookupEnabled = true;
                                    }
                                }
                            }
                            if ((winery.EnableClubemember || winery.EnableClubCommerce7 || winery.EnableClubShopify || winery.EnableClubOrderPort || winery.EnableClubAMS || winery.EnableClubMicroworks || winery.EnableClubCoresense || winery.EnableClubVin65 || bLoyalClubLookupEnabled) && MemberBenefit > 0)
                            {
                                sched.show_club_member_benefits = true;
                            }
                        }

                        sched.deposit_policy = uc.Common.GetDepositPolicies(ISAdmin).Where(f => f.ID == Convert.ToString(dataReader["ChargeFee"])).Select(f => f.Name).FirstOrDefault();
                        if (CancelTimeDesc == "Non-changeable, Non-cancellable")
                        {
                            sched.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                        }
                        else if (CancelTimeDesc.IndexOf("Days") > -1)
                        {
                            try
                            {
                                int CancelTime = Convert.ToInt32(CancelTimeDesc.Replace("Days", "").Replace(" ", ""));

                                if (CancelTime > 20)
                                {
                                    sched.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                                }
                                else
                                {
                                    sched.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                                }
                            }
                            catch
                            {
                                sched.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                            }
                        }
                        else
                        {
                            sched.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                        }

                        sched.require_credit_card = requiresCreditCard(Convert.ToInt32(dataReader["ChargeFee"]));
                        sched.gratuity_percentage = Convert.ToDecimal(dataReader["GratuityPercentage"]);
                        sched.tax_gratuity = Convert.ToBoolean(dataReader["TaxGratuity"]);
                        sched.event_questions = GetEventQuestions(sched.event_id);

                        if (eventTimes.Count > 0)
                            eventScheduleEvent.Add(sched);
                    }
                }
            }

            eventSchedule.EventScheduleEvent = eventScheduleEvent;

            return eventSchedule;
        }

        public EventScheduleTime GetSeatLeftEventScheduleV2(int wineryId, DateTime StartTime, int guestCount, int SlotId = 0, int SlotType = 0, int EventId = 0, bool ISAdmin = true, int reservationid = 0, int bookingtype = 0)
        {
            string sp = "GetEventbyWineryIdAdminV2";

            var eventScheduleTime = new EventScheduleTime();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@GuestsCount", guestCount));
            parameterList.Add(GetParameter("@reservationid", reservationid));
            parameterList.Add(GetParameter("@bookingtype", bookingtype));

            if (!ISAdmin)
            {
                parameterList.Add(GetParameter("@SlotId", SlotId));
                parameterList.Add(GetParameter("@SlotType", SlotType));
                parameterList.Add(GetParameter("@callbtnSearch", false));
                parameterList.Add(GetParameter("@EventIdnum", EventId));

                sp = "GetEventsByWineryIdAndDateClientV2";
            }

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        if ((Convert.ToString(dataReader["EventTimings"]) != null))
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EventTimings"])))
                            {
                                foreach (var t_loopVariable in Convert.ToString(dataReader["EventTimings"]).Split(','))
                                {
                                    string[] timeArray = t_loopVariable.Split('|');

                                    eventScheduleTime.slot_id = Convert.ToInt32(timeArray[1]);
                                    eventScheduleTime.seats_left = Convert.ToInt32(timeArray[4]);
                                }
                            }
                        }
                    }
                }
            }
            return eventScheduleTime;
        }

        public List<Event> GetEvents(int wineryId)
        {
            string sp = "GetEventsByWineryId";

            var eventS = new List<Event>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", wineryId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var sched = new Event();
                        sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                        sched.event_name = Convert.ToString(dataReader["EventName"]);
                        sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                        sched.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                        sched.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                        sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        sched.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);
                        sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                        sched.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                        sched.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                        sched.duration = Convert.ToString(dataReader["Duration"]);
                        sched.week_days = Convert.ToString(dataReader["WeekDays"]);
                        decimal MinPrice = Convert.ToDecimal(dataReader["MinReservationChanges"]);
                        decimal MaxPrice = Convert.ToDecimal(dataReader["MaxReservationChanges"]);
                        sched.event_floor_plans = GetEventFloorPlansByEventId(sched.event_id);

                        //List<AddOn_Group> event_addOn = GetEventAddOnGroupByEventId(sched.event_id);

                        //decimal MinPrice = 0;
                        //decimal MaxPrice = 0;
                        //bool setPrice = false;

                        //List<AddOn_Group> addonlist1 = event_addOn.Where(c => c.group_type == 1).ToList();
                        //List<AddOn_Group> addonlist2 = event_addOn.Where(c => c.group_type == 2).ToList();

                        //if (addonlist1.Count > 0)
                        //{
                        //    var min = addonlist1[0].addOn_group_items.Min(o => o.price);
                        //    var max = addonlist1[0].addOn_group_items.Max(o => o.price);

                        //    if (!setPrice)
                        //    {
                        //        MinPrice = min;
                        //        MaxPrice = max;
                        //        setPrice = true;
                        //    }
                        //    else
                        //    {
                        //        if (MinPrice > min)
                        //            MinPrice = min;

                        //        if (MaxPrice < max)
                        //            MaxPrice = max;
                        //    }
                        //}

                        //if (addonlist2.Count > 0)
                        //{
                        //    var min = addonlist2[0].addOn_group_items.Min(o => o.price);
                        //    var max = addonlist2[0].addOn_group_items.Max(o => o.price);

                        //    if (!setPrice)
                        //    {
                        //        MinPrice = min;
                        //        MaxPrice = max;
                        //        setPrice = true;
                        //    }
                        //    else
                        //    {
                        //        if (MinPrice > min)
                        //            MinPrice = min;

                        //        if (MaxPrice < max)
                        //            MaxPrice = max;
                        //    }
                        //}

                        //MinPrice = MinPrice + sched.fee_Per_person;
                        //MaxPrice = MaxPrice + sched.fee_Per_person;

                        if (MinPrice == MaxPrice && MinPrice <= 0)
                            sched.price_range = "Complimentary";
                        else if (MinPrice != MaxPrice)
                            sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice < 0 ? 0 : MinPrice) + " - " + string.Format(new CultureInfo("en-US"), "{0:C}", MaxPrice) + "/pp";
                        else if (MinPrice == MaxPrice)
                            sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice) + "/pp";

                        sched.event_start_date = Convert.ToDateTime(dataReader["EventStartDate"]);
                        DateTime event_end_date = Convert.ToDateTime(dataReader["EventEndDate"]);
                        if (event_end_date >= sched.event_start_date)
                            sched.event_end_date = event_end_date;
                        sched.next_available_date = Convert.ToDateTime(dataReader["NextAvailableDate"]);

                        eventS.Add(sched);
                    }
                }
            }
            return eventS;
        }

        public List<EventData> GetPrivateEventsByAccessCode(int wineryId, string accessCode, DateTime? startDate = null, int guests = 0)
        {
            string sp = "GetPrivateEventsByAccessCode";

            var eventS = new List<EventData>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@AccessCode", accessCode));
            if (startDate != null && startDate.HasValue)
            {
                parameterList.Add(GetParameter("@StartDate", startDate.Value));
            }
            parameterList.Add(GetParameter("@Guests", guests));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var sched = new EventData();
                        sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                        sched.event_name = Convert.ToString(dataReader["EventName"]);
                        sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                        sched.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                        sched.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                        sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        sched.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);
                        sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                        sched.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                        sched.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                        sched.duration = Convert.ToString(dataReader["Duration"]);
                        sched.week_days = Convert.ToString(dataReader["WeekDays"]);
                        sched.event_floor_plans = GetEventFloorPlansByEventId(sched.event_id);
                        if (dataReader["FirstAvailableDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["FirstAvailableDate"].ToString()))
                            sched.first_available_date = Convert.ToDateTime(dataReader["FirstAvailableDate"]);
                        eventS.Add(sched);
                    }
                }
            }
            return eventS;
        }


        public EventAccess CheckEventAccessCode(int memberId, int eventId, string accessCode, int userId, int slotId = 0, int slotType = 0)
        {
            EventAccess access = null;
            string sp = "CheckRSVPEventAccessCode";
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@MemberId", memberId));
            parameterList.Add(GetParameter("@Accesscode", accessCode));
            parameterList.Add(GetParameter("@EventId", eventId));
            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@SlotId", slotId));
            parameterList.Add(GetParameter("@SlotType", slotType));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    access = new EventAccess();
                    while (dataReader.Read())
                    {
                        access.IsValid = Convert.ToBoolean(dataReader["AccessCodeValid"]);
                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["StartDate"])))
                        {
                            access.StartDate = Convert.ToDateTime(dataReader["StartDate"]);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EndDate"])))
                        {
                            access.EndDate = Convert.ToDateTime(dataReader["EndDate"]);
                        }
                    }
                }
            }
            return access;
        }

        public List<ScheduleV2> GetEventsByTimeAndGuests(string Str_Search, string time, int Guest, DateTime StartDate, bool include_waitlist, string PurchaseURL,int regionId)
        {
            var eventS = new List<ScheduleV2>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@Guest", Guest));
            parameterList.Add(GetParameter("@time", time));
            parameterList.Add(GetParameter("@Str_Search", Str_Search));
            parameterList.Add(GetParameter("@IncludeWaitlist", include_waitlist));
            parameterList.Add(GetParameter("@regionId", regionId));

            string OldDisplayName = string.Empty;
            var sched = new ScheduleV2();

            using (DbDataReader dataReader = GetDataReader("GetEventsByTimeAndGuests", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        string DisplayName = Convert.ToString(dataReader["displayname"]);

                        if (string.IsNullOrEmpty(OldDisplayName) || OldDisplayName != DisplayName)
                        {
                            sched = new ScheduleV2();

                            OldDisplayName = DisplayName;
                            sched.member_name = DisplayName;
                            sched.member_id = Convert.ToInt32(dataReader["Id"]);
                            sched.member_city = Convert.ToString(dataReader["city"]);
                            sched.member_state = Convert.ToString(dataReader["state"]);
                            sched.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                            sched.tag_1 = Convert.ToString(dataReader["TagName1"]);
                            sched.tag_2 = Convert.ToString(dataReader["TagName2"]);
                            sched.recommended_tag = Convert.ToString(dataReader["RecommendationName"]);
                            sched.booked_count = Convert.ToInt32(dataReader["BookedCount"]);
                            //sched.purchase_url = PurchaseURL + Convert.ToString(dataReader["PurchaseURL"]) + "-profile";
                            sched.listing_image_url = Convert.ToString(dataReader["ListingImageUrl"]);

                            WineryReviews wineryReviews = GetWineryReviews(sched.member_id);

                            sched.total_reviews = wineryReviews.ReviewCount;
                            sched.review_stars = wineryReviews.ReviewStars;
                            sched.avg_review_value = wineryReviews.avg_review_value;

                            sched.times = new List<string>();
                            eventS.Add(sched);
                        }

                        sched.times.Add(Convert.ToString(dataReader["starttime"]));
                    }
                }
            }
            return eventS;
        }

        public List<MostBookedEvent> GetMostBookedEvent(int event_type = 0, int region_id = 0)
        {
            var eventS = new List<MostBookedEvent>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@event_type", event_type));
            parameterList.Add(GetParameter("@region_id", region_id));



            using (DbDataReader dataReader = GetDataReader("GetMostBookedEvent", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var mostBookedEvent = new MostBookedEvent();

                        mostBookedEvent.event_id = Convert.ToInt32(dataReader["EventId"]);
                        mostBookedEvent.event_name = Convert.ToString(dataReader["EventName"]);
                        mostBookedEvent.event_desc = Convert.ToString(dataReader["Description"]);

                        eventS.Add(mostBookedEvent);
                    }
                }
            }
            return eventS;
        }

        public List<MostBookedEventType> GetMostBookedEventType(int regionId)
        {
            var eventS = new List<MostBookedEventType>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@region_id", regionId));

            using (DbDataReader dataReader = GetDataReader("GetMostBookedEventType", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var mostBookedEvent = new MostBookedEventType();

                        mostBookedEvent.event_type_id = Convert.ToInt32(dataReader["Id"]);
                        mostBookedEvent.event_type_name = Convert.ToString(dataReader["EventTypeName"]);
                        mostBookedEvent.event_type_desc = Convert.ToString(dataReader["Description"]);

                        eventS.Add(mostBookedEvent);
                    }
                }
            }
            return eventS;
        }

        public List<ScheduleV2> GetEventsByMemberId(int MemberId)
        {
            var eventS = new List<ScheduleV2>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@MemberId", MemberId));

            var sched = new ScheduleV2();

            using (DbDataReader dataReader = GetDataReader("GetEventsByMemberId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        sched = new ScheduleV2();

                        sched.member_name = Convert.ToString(dataReader["displayname"]);
                        sched.member_id = Convert.ToInt32(dataReader["Id"]);
                        sched.member_city = Convert.ToString(dataReader["city"]);
                        sched.member_state = Convert.ToString(dataReader["state"]);
                        sched.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        sched.tag_1 = Convert.ToString(dataReader["TagName1"]);
                        sched.tag_2 = Convert.ToString(dataReader["TagName2"]);
                        sched.recommended_tag = Convert.ToString(dataReader["RecommendationName"]);
                        sched.booked_count = Convert.ToInt32(dataReader["BookedCount"]);

                        WineryReviews wineryReviews = GetWineryReviews(sched.member_id);

                        sched.total_reviews = wineryReviews.ReviewCount;
                        sched.review_stars = wineryReviews.ReviewStars;

                        sched.times = new List<string>();
                        eventS.Add(sched);
                    }
                }
            }
            return eventS;
        }

        public MemberDetails GetMemberByKeyword(string search)
        {
            MemberDetails member = null;
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@Str_Search", search));
            using (DbDataReader dataReader = GetDataReader("GetMemberByKeyword", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        member = new MemberDetails
                        {
                            member_name = Convert.ToString(dataReader["DisplayName"]),
                            member_id = Convert.ToInt32(dataReader["Id"]),
                            member_url = Convert.ToString(dataReader["PurchaseURL"])
                        };
                    }
                }
            }

            return member;

        }

        public EventV3 GetEventsByWineryIdAndDate(int MemberId, string time, int Guest, DateTime StartDate, int event_id, int slot_id, int slot_type, bool include_waitlist, bool include_hidden_member, bool passport_event, bool show_image, string access_code, int reservationId, bool send_sold_out_dates = false)
        {
            var eventV3 = new EventV3();

            var eventS = new List<EventV2>();
            var parameterList = new List<DbParameter>();

            if (event_id == 0)
                event_id = -1;

            if (slot_id == 0)
            {
                slot_id = -1;
                slot_type = -1;
            }

            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@Guest", Guest));
            parameterList.Add(GetParameter("@time", time));
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@EventId", event_id));
            parameterList.Add(GetParameter("@SlotId", slot_id));
            parameterList.Add(GetParameter("@SlotType", slot_type));
            parameterList.Add(GetParameter("@IncludeWaitlist", include_waitlist));
            parameterList.Add(GetParameter("@IncludeHiddenMember", include_hidden_member));
            parameterList.Add(GetParameter("@AccessCode", access_code));
            parameterList.Add(GetParameter("@ReservationId", reservationId));

            string OldDisplayName = string.Empty;
            int MaxPersons = 0;
            int MinPersons = 0;
            var sched = new EventV2();

            using (DbDataReader dataReader = GetDataReader("GetEventsByWineryIdAndDate", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        eventV3.booked_count = Convert.ToInt32(dataReader["BookedCount"]);

                        string DisplayName = Convert.ToString(dataReader["EventName"]);


                        if (string.IsNullOrEmpty(OldDisplayName) || OldDisplayName != DisplayName)
                        {
                            sched = new EventV2();

                            MaxPersons = Convert.ToInt32(dataReader["MaxPersons"]);
                            MinPersons = Convert.ToInt32(dataReader["MinPersons"]);
                            OldDisplayName = DisplayName;
                            sched.event_name = DisplayName;
                            sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                            sched.event_floor_plans = GetEventFloorPlansByEventId(sched.event_id);
                            sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                            sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                            sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                            sched.event_location = Convert.ToString(dataReader["EventLocation"]);
                            sched.duration = Convert.ToString(dataReader["Duration"]);
                            sched.charge_fee = Convert.ToInt32(dataReader["ChargeFee"]);
                            sched.qualified_purchase = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                            sched.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                            sched.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);

                            int MemberBenefit = Convert.ToInt32(dataReader["MemberBenefit"]);

                            sched.member_benfeit = MemberBenefit > 0;
                            sched.member_benefit_desc = uc.Common.GetMemberBenefit().Where(f => f.ID == MemberBenefit.ToString()).Select(f => f.Name).FirstOrDefault();

                            sched.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                            sched.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                            sched.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                            sched.min_lead_time = Convert.ToString(dataReader["LeadTime"]);
                            sched.max_lead_time = Convert.ToInt32(dataReader["MaximumLeadTime"]) + " Days";
                            sched.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                            sched.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);

                            sched.region_id = Convert.ToInt32(dataReader["RegionId"]);

                            sched.event_start_date = Convert.ToDateTime(dataReader["EventStartDate"]);
                            DateTime event_end_date = Convert.ToDateTime(dataReader["EventEndDate"]);

                            if (event_end_date >= sched.event_start_date)
                                sched.event_end_date = event_end_date;

                            decimal MinPrice = Convert.ToDecimal(dataReader["MinReservationChanges"]);
                            decimal MaxPrice = Convert.ToDecimal(dataReader["MaxReservationChanges"]);

                            sched.total_reviews = Convert.ToInt32(dataReader["ReviewCount"]);
                            sched.review_stars = Convert.ToInt32(dataReader["ReviewStars"]);
                            sched.avg_review_value = Convert.ToDecimal(dataReader["AvgReviewValue"]);
                            sched.sold_out = Convert.ToBoolean(dataReader["soldOut"]);

                            //List<AddOn_Group> event_addOn = GetEventAddOnGroupByEventId(sched.event_id);

                            //decimal MinPrice = 0;
                            //decimal MaxPrice = 0;
                            //bool setPrice = false;

                            //List<AddOn_Group> addonlist1 = event_addOn.Where(c => c.group_type == 1).ToList();
                            //List<AddOn_Group> addonlist2 = event_addOn.Where(c => c.group_type == 2).ToList();

                            //if (addonlist1.Count > 0)
                            //{
                            //    var min = addonlist1[0].addOn_group_items.Min(o => o.price);
                            //    var max = addonlist1[0].addOn_group_items.Max(o => o.price);

                            //    if (!setPrice)
                            //    {
                            //        MinPrice = min;
                            //        MaxPrice = max;
                            //        setPrice = true;
                            //    }
                            //    else
                            //    {
                            //        if (MinPrice > min)
                            //            MinPrice = min;

                            //        if (MaxPrice < max)
                            //            MaxPrice = max;
                            //    }
                            //}

                            //if (addonlist2.Count > 0)
                            //{
                            //    var min = addonlist2[0].addOn_group_items.Min(o => o.price);
                            //    var max = addonlist2[0].addOn_group_items.Max(o => o.price);

                            //    if (!setPrice)
                            //    {
                            //        MinPrice = min;
                            //        MaxPrice = max;
                            //        setPrice = true;
                            //    }
                            //    else
                            //    {
                            //        if (MinPrice > min)
                            //            MinPrice = min;

                            //        if (MaxPrice < max)
                            //            MaxPrice = max;
                            //    }
                            //}

                            //MinPrice = MinPrice + sched.fee_Per_person;
                            //MaxPrice = MaxPrice + sched.fee_Per_person;

                            if (MinPrice == MaxPrice && MinPrice <= 0)
                                sched.price_range = "Complimentary";
                            else if (MinPrice != MaxPrice)
                                sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice < 0 ? 0 : MinPrice) + " - " + string.Format(new CultureInfo("en-US"), "{0:C}", MaxPrice) + "/pp";
                            else if (MinPrice == MaxPrice)
                                sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice) + "/pp";

                            if (show_image)
                                sched.event_image = Convert.ToString(dataReader["EventImage"]);
                            int requiredLogin = Convert.ToInt32(dataReader["RequiredLogin"]);

                            sched.required_login = (requiredLogin > 0);

                            if (requiredLogin == 1)
                            {
                                sched.discount_only = false;
                                sched.require_login_message = string.Format("To book this experience, we must first verify your {0} membership status.", Convert.ToString(dataReader["MemberName"]));
                            }
                            else if (requiredLogin == 2)
                            {
                                sched.require_login_message = string.Format("Select members of {0} may receive special incentives after verification.", sched.event_location);
                                sched.discount_only = true;
                            }
                            sched.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                            
                            sched.passport_promoted_event = false;
                            //sched.purchase_url = PurchaseURL + Convert.ToString(dataReader["PurchaseURL"]) + "-profile";
                            sched.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                            sched.week_days = Convert.ToString(dataReader["WeekDays"]);
                            sched.charge_sales_tax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                            if (passport_event)
                            {
                                sched.passport_promoted_event = PassportEvent(sched.event_id);
                            }

                            if (send_sold_out_dates && event_id > 0)
                            {
                                sched.sold_out_dates = GetSoldoutDates(sched.event_id, Guest, StartDate);
                            }

                            sched.is_prepaid = Convert.ToBoolean(dataReader["PrePaid"]);

                            sched.times = new List<EventTimeV2>();
                            eventS.Add(sched);
                        }
                        else
                        {
                            if (MaxPersons < Convert.ToInt32(dataReader["MaxPersons"]))
                            {
                                MaxPersons = Convert.ToInt32(dataReader["MaxPersons"]);
                                eventS.Last().max_persons = MaxPersons;
                            }

                            if (MinPersons > Convert.ToInt32(dataReader["MinPersons"]))
                            {
                                MinPersons = Convert.ToInt32(dataReader["MinPersons"]);
                                eventS.Last().min_persons = MinPersons;
                            }
                        }

                        var eventTimeV2 = new EventTimeV2();

                        eventTimeV2.start_time = Convert.ToString(dataReader["StartTime"]);
                        eventTimeV2.end_time = Convert.ToString(dataReader["EndTime"]);
                        eventTimeV2.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        eventTimeV2.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        eventTimeV2.wait_list = Convert.ToBoolean(dataReader["WaitList"]);
                        eventTimeV2.sold_out = false;
                        eventTimeV2.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                        eventTimeV2.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                        eventTimeV2.seats_left = Convert.ToInt32(dataReader["SeatsLeft"]);
                        eventTimeV2.wait_list_left = Convert.ToInt16(dataReader["WaitList"]);

                        sched.times.Add(eventTimeV2);
                    }

                    eventV3.events = eventS;
                }
            }
            return eventV3;
        }

        public RSVPEventLandingPage GetRSVPEventLandingPage(int MemberId, string time, int Guest, DateTime StartDate, int event_id, int slot_id, int slot_type, bool include_waitlist, bool include_hidden_member, bool passport_event, bool show_image, string access_code, int reservationId, bool send_sold_out_dates = false)
        {
            var eventV3 = new RSVPEventLandingPage();

            var eventS = new EventV2();
            var parameterList = new List<DbParameter>();

            if (event_id == 0)
                event_id = -1;

            if (slot_id == 0)
            {
                slot_id = -1;
                slot_type = -1;
            }

            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@Guest", Guest));
            parameterList.Add(GetParameter("@time", time));
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@EventId", event_id));
            parameterList.Add(GetParameter("@AccessCode", access_code));

            string OldDisplayName = string.Empty;
            int MaxPersons = 0;
            int MinPersons = 0;
            var sched = new EventV2();

            using (DbDataReader dataReader = GetDataReader("GetRSVPEventLandingPage", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        eventV3.has_other_events_at_member = Convert.ToBoolean(dataReader["has_other_events_at_member"]);
                        eventV3.has_other_events_nearby = Convert.ToBoolean(dataReader["has_other_events_nearby"]);
                        eventV3.has_ticket_events = Convert.ToBoolean(dataReader["has_ticket_events"]);
                        eventV3.has_reviews = Convert.ToBoolean(dataReader["has_TotalReviews"]);
                        eventV3.has_blog_articles = Convert.ToBoolean(dataReader["has_blog_articles"]);
                        eventV3.total_reviews = Convert.ToInt32(dataReader["ReviewCount"]);
                        eventV3.review_stars = Convert.ToInt32(dataReader["ReviewStars"]);
                        eventV3.avg_review_value = Convert.ToInt32(dataReader["avg_review_value"]);

                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                eventV3.booked_count = Convert.ToInt32(dataReader["BookedCount"]);

                                string DisplayName = Convert.ToString(dataReader["EventName"]);

                                string CancelTimeDesc = Convert.ToString(dataReader["CancelTime"]);
                                int chargeFee = Convert.ToInt32(dataReader["ChargeFee"]);
                                //sched.cancel_time_desc = CancelTimeDesc;

                                eventV3.deposit_policy = uc.Common.GetDepositPolicies(false).Where(f => f.ID == ((int)chargeFee).ToString()).Select(f => f.Name).FirstOrDefault();

                                if (CancelTimeDesc == "Non-changeable, Non-cancellable")
                                {
                                    eventV3.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                                }
                                else if (CancelTimeDesc.IndexOf("Days") > -1)
                                {
                                    try
                                    {
                                        int CancelTime = Convert.ToInt32(CancelTimeDesc.Replace("Days", "").Replace(" ", ""));

                                        if (CancelTime > 20)
                                        {
                                            eventV3.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                                        }
                                        else
                                        {
                                            eventV3.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                                        }
                                    }
                                    catch
                                    {
                                        eventV3.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                                    }
                                }
                                else
                                {
                                    eventV3.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                                }

                                if (string.IsNullOrEmpty(OldDisplayName) || OldDisplayName != DisplayName)
                                {
                                    sched = new EventV2();

                                    MaxPersons = Convert.ToInt32(dataReader["MaxPersons"]);
                                    MinPersons = Convert.ToInt32(dataReader["MinPersons"]);
                                    OldDisplayName = DisplayName;
                                    sched.event_name = DisplayName;
                                    sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                                    sched.event_floor_plans = GetEventFloorPlansByEventId(sched.event_id);
                                    sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                                    sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                                    sched.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);
                                    sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                                    sched.event_location = Convert.ToString(dataReader["EventLocation"]);
                                    sched.duration = Convert.ToString(dataReader["Duration"]);
                                    sched.charge_fee = Convert.ToInt32(dataReader["ChargeFee"]);
                                    sched.qualified_purchase = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                                    sched.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                                    sched.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                                    sched.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                                    sched.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                                    sched.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                                    sched.min_lead_time = Convert.ToString(dataReader["LeadTime"]);
                                    sched.max_lead_time = Convert.ToInt32(dataReader["MaximumLeadTime"]) + " Days";
                                    sched.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);

                                    sched.region_id = Convert.ToInt32(dataReader["RegionId"]);

                                    sched.event_start_date = Convert.ToDateTime(dataReader["EventStartDate"]);
                                    DateTime event_end_date = Convert.ToDateTime(dataReader["EventEndDate"]);

                                    if (event_end_date >= sched.event_start_date)
                                        sched.event_end_date = event_end_date;

                                    decimal MinPrice = Convert.ToDecimal(dataReader["MinReservationChanges"]);
                                    decimal MaxPrice = Convert.ToDecimal(dataReader["MaxReservationChanges"]);

                                    //List<AddOn_Group> event_addOn = GetEventAddOnGroupByEventId(sched.event_id);

                                    //decimal MinPrice = 0;
                                    //decimal MaxPrice = 0;
                                    //bool setPrice = false;

                                    //List<AddOn_Group> addonlist1 = event_addOn.Where(c => c.group_type == 1).ToList();
                                    //List<AddOn_Group> addonlist2 = event_addOn.Where(c => c.group_type == 2).ToList();

                                    //if (addonlist1.Count > 0)
                                    //{
                                    //    var min = addonlist1[0].addOn_group_items.Min(o => o.price);
                                    //    var max = addonlist1[0].addOn_group_items.Max(o => o.price);

                                    //    if (!setPrice)
                                    //    {
                                    //        MinPrice = min;
                                    //        MaxPrice = max;
                                    //        setPrice = true;
                                    //    }
                                    //    else
                                    //    {
                                    //        if (MinPrice > min)
                                    //            MinPrice = min;

                                    //        if (MaxPrice < max)
                                    //            MaxPrice = max;
                                    //    }
                                    //}

                                    //if (addonlist2.Count > 0)
                                    //{
                                    //    var min = addonlist2[0].addOn_group_items.Min(o => o.price);
                                    //    var max = addonlist2[0].addOn_group_items.Max(o => o.price);

                                    //    if (!setPrice)
                                    //    {
                                    //        MinPrice = min;
                                    //        MaxPrice = max;
                                    //        setPrice = true;
                                    //    }
                                    //    else
                                    //    {
                                    //        if (MinPrice > min)
                                    //            MinPrice = min;

                                    //        if (MaxPrice < max)
                                    //            MaxPrice = max;
                                    //    }
                                    //}

                                    //MinPrice = MinPrice + sched.fee_Per_person;
                                    //MaxPrice = MaxPrice + sched.fee_Per_person;

                                    if (MinPrice == MaxPrice && MinPrice <= 0)
                                        sched.price_range = "Complimentary";
                                    else if (MinPrice != MaxPrice)
                                        sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice < 0 ? 0 : MinPrice) + " - " + string.Format(new CultureInfo("en-US"), "{0:C}", MaxPrice) + "/pp";
                                    else if (MinPrice == MaxPrice)
                                        sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice) + "/pp";

                                    if (show_image)
                                        sched.event_image = Convert.ToString(dataReader["EventImage"]);
                                    int requiredLogin = Convert.ToInt32(dataReader["RequiredLogin"]);

                                    sched.required_login = (requiredLogin > 0);

                                    if (requiredLogin == 1)
                                    {
                                        sched.discount_only = false;
                                        sched.require_login_message = string.Format("To book this experience, we must first verify your {0} membership status.", Convert.ToString(dataReader["MemberName"]));
                                    }
                                    else if (requiredLogin == 2)
                                    {
                                        sched.require_login_message = string.Format("Select members of {0} may receive special incentives after verification.", sched.event_location);
                                        sched.discount_only = true;
                                    }
                                    sched.currency_code = Convert.ToString(dataReader["CurrencyCode"]);

                                    sched.passport_promoted_event = false;
                                    //sched.purchase_url = PurchaseURL + Convert.ToString(dataReader["PurchaseURL"]) + "-profile";
                                    sched.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                                    sched.week_days = Convert.ToString(dataReader["WeekDays"]);
                                    sched.charge_sales_tax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                                    if (passport_event)
                                    {
                                        sched.passport_promoted_event = PassportEvent(sched.event_id);
                                    }

                                    if (send_sold_out_dates && event_id > 0)
                                    {
                                        sched.sold_out_dates = GetSoldoutDates(sched.event_id, Guest, StartDate);
                                    }

                                    sched.is_prepaid = Convert.ToBoolean(dataReader["PrePaid"]);

                                    sched.times = new List<EventTimeV2>();
                                    eventS = sched;
                                }
                                else
                                {
                                    if (MaxPersons < Convert.ToInt32(dataReader["MaxPersons"]))
                                    {
                                        MaxPersons = Convert.ToInt32(dataReader["MaxPersons"]);
                                        eventS.max_persons = MaxPersons;
                                    }

                                    if (MinPersons > Convert.ToInt32(dataReader["MinPersons"]))
                                    {
                                        MinPersons = Convert.ToInt32(dataReader["MinPersons"]);
                                        eventS.min_persons = MinPersons;
                                    }
                                }

                                var eventTimeV2 = new EventTimeV2();

                                eventTimeV2.start_time = Convert.ToString(dataReader["StartTime"]);
                                eventTimeV2.end_time = Convert.ToString(dataReader["EndTime"]);
                                eventTimeV2.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                                eventTimeV2.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                                eventTimeV2.wait_list = Convert.ToBoolean(dataReader["WaitList"]);
                                eventTimeV2.sold_out = false;
                                eventTimeV2.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                                eventTimeV2.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                                eventTimeV2.seats_left = Convert.ToInt32(dataReader["SeatsLeft"]);
                                eventTimeV2.wait_list_left = Convert.ToInt16(dataReader["WaitList"]);

                                sched.times.Add(eventTimeV2);

                            }
                        }
                    }

                    eventV3.event_details = eventS;
                }
            }
            return eventV3;
        }

        public EventDateV3 GetEventsByEventIdAndDate(int MemberId, int Guest, DateTime StartDate, int event_id)
        {
            var eventV3 = new EventDateV3();

            var eventS = new List<EventDateTimeV2>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@Guest", Guest));
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@EventId", event_id));

            DateTime OldEventDate = Convert.ToDateTime("1/1/1900");
            var sched = new EventDateTimeV2();

            using (DbDataReader dataReader = GetDataReader("GetEventsByEventIdAndDate", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        DateTime EventDate = Convert.ToDateTime(dataReader["EventDate"]);


                        if (OldEventDate.Year == 1900 || OldEventDate.ToShortDateString() != EventDate.ToShortDateString())
                        {
                            sched = new EventDateTimeV2();

                            sched.event_date = EventDate;
                            OldEventDate = EventDate;

                            sched.times = new List<EventTimeV3>();
                            eventS.Add(sched);
                        }

                        var eventTimeV2 = new EventTimeV3();

                        eventTimeV2.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        eventTimeV2.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        eventTimeV2.start_time = Convert.ToString(dataReader["StartTime"]);
                        eventTimeV2.end_time = Convert.ToString(dataReader["EndTime"]);
                        eventTimeV2.wait_list_left = Convert.ToInt16(dataReader["WaitList"]);
                        eventTimeV2.sold_out = !Convert.ToBoolean(dataReader["IsAvailable"]);
                        eventTimeV2.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                        eventTimeV2.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                        eventTimeV2.seats_left = Convert.ToInt32(dataReader["SeatsLeft"]);

                        sched.times.Add(eventTimeV2);
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            eventV3.event_id = Convert.ToInt32(dataReader["EventId"]);
                            eventV3.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                            eventV3.event_name = Convert.ToString(dataReader["EventName"]);
                            eventV3.event_desc = Convert.ToString(dataReader["EventDesc"]);
                            eventV3.event_location = Convert.ToString(dataReader["EventLocation"]);
                            eventV3.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                            eventV3.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);
                            eventV3.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                            eventV3.charge_fee = Convert.ToInt32(dataReader["ChargeFee"]);
                            eventV3.qualified_purchase = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                            eventV3.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                            eventV3.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                            int requiredLogin = Convert.ToInt32(dataReader["RequiredLogin"]);
                            eventV3.required_login = (requiredLogin > 0);
                            if (requiredLogin == 1)
                            {
                                eventV3.discount_only = false;
                                eventV3.require_login_message = string.Format("To book this experience, we must first verify your {0} membership status.", Convert.ToString(dataReader["MemberName"]));
                            }
                            else if (requiredLogin == 2)
                            {
                                eventV3.require_login_message = string.Format("Select members of {0} may receive special incentives after verification.", eventV3.event_location);
                                eventV3.discount_only = true;
                            }
                            eventV3.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                            eventV3.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                            eventV3.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                            eventV3.week_days = Convert.ToString(dataReader["WeekDays"]);
                            eventV3.charge_sales_tax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                            eventV3.event_image = Convert.ToString(dataReader["EventImage"]);
                            eventV3.event_start_date = Convert.ToDateTime(dataReader["EventStartDate"]);
                            eventV3.is_prepaid = Convert.ToBoolean(dataReader["PrePaid"]);

                            DateTime event_end_date = Convert.ToDateTime(dataReader["EventEndDate"]);

                            if (event_end_date >= eventV3.event_start_date)
                                eventV3.event_end_date = event_end_date;

                            eventV3.duration = Convert.ToString(dataReader["Duration"]);
                            eventV3.min_persons = dataReader["MinPersons"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["MinPersons"]);
                            eventV3.max_persons = dataReader["MaxPersons"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["MaxPersons"]);
                        }
                    }

                    List<DateTime> soldoutDates = new List<DateTime>();
                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            soldoutDates.Add(Convert.ToDateTime(dataReader["EventDate"]));
                        }
                    }

                    eventV3.sold_out_dates = soldoutDates;
                    eventV3.event_dates = eventS;
                }
            }
            return eventV3;
        }

        public List<ProfileEvent> GetEventsForMemberProfile(int MemberId)
        {
            var eventS = new List<ProfileEvent>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@MemberId", MemberId));

            string OldDisplayName = string.Empty;
            int MaxPersons = 0;
            int MinPersons = 0;

            using (DbDataReader dataReader = GetDataReader("GetEventsForMemberProfile", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {


                        string DisplayName = Convert.ToString(dataReader["EventName"]);
                        ProfileEvent sched = new ProfileEvent();

                        MaxPersons = Convert.ToInt32(dataReader["MaxPersons"]);
                        MinPersons = Convert.ToInt32(dataReader["MinPersons"]);
                        OldDisplayName = DisplayName;
                        sched.event_name = DisplayName;
                        sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                        sched.event_floor_plans = GetEventFloorPlansByEventId(sched.event_id);
                        sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                        sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        sched.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);
                        sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                        sched.event_location = Convert.ToString(dataReader["EventLocation"]);
                        sched.duration = Convert.ToString(dataReader["Duration"]);
                        sched.charge_fee = Convert.ToInt32(dataReader["ChargeFee"]);
                        sched.qualified_purchase = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                        sched.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                        sched.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                        sched.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                        sched.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                        sched.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                        sched.is_prepaid = Convert.ToBoolean(dataReader["PrePaid"]);

                        sched.next_available_date = Convert.ToDateTime(dataReader["NextAvailableDate"]);

                        sched.address1 = Convert.ToString(dataReader["Address1"]);
                        sched.venue_city = Convert.ToString(dataReader["City"]);
                        sched.venue_country = Convert.ToString(dataReader["Country"]);
                        sched.venue_state = Convert.ToString(dataReader["State"]);

                        sched.event_organizer_url = Convert.ToString(dataReader["WebsiteURL"]);

                        decimal MinPrice = Convert.ToDecimal(dataReader["MinReservationChanges"]);
                        decimal MaxPrice = Convert.ToDecimal(dataReader["MaxReservationChanges"]);

                        //List<AddOn_Group> event_addOn = GetEventAddOnGroupByEventId(sched.event_id);

                        //decimal MinPrice = 0;
                        //decimal MaxPrice = 0;
                        //bool setPrice = false;

                        //List<AddOn_Group> addonlist1 = event_addOn.Where(c => c.group_type == 1).ToList();
                        //List<AddOn_Group> addonlist2 = event_addOn.Where(c => c.group_type == 2).ToList();

                        //if (addonlist1.Count > 0)
                        //{
                        //    var min = addonlist1[0].addOn_group_items.Min(o => o.price);
                        //    var max = addonlist1[0].addOn_group_items.Max(o => o.price);

                        //    if (!setPrice)
                        //    {
                        //        MinPrice = min;
                        //        MaxPrice = max;
                        //        setPrice = true;
                        //    }
                        //    else
                        //    {
                        //        if (MinPrice > min)
                        //            MinPrice = min;

                        //        if (MaxPrice < max)
                        //            MaxPrice = max;
                        //    }
                        //}

                        //if (addonlist2.Count > 0)
                        //{
                        //    var min = addonlist2[0].addOn_group_items.Min(o => o.price);
                        //    var max = addonlist2[0].addOn_group_items.Max(o => o.price);

                        //    if (!setPrice)
                        //    {
                        //        MinPrice = min;
                        //        MaxPrice = max;
                        //        setPrice = true;
                        //    }
                        //    else
                        //    {
                        //        if (MinPrice > min)
                        //            MinPrice = min;

                        //        if (MaxPrice < max)
                        //            MaxPrice = max;
                        //    }
                        //}

                        //MinPrice = MinPrice + sched.fee_Per_person;
                        //MaxPrice = MaxPrice + sched.fee_Per_person;

                        if (MinPrice == MaxPrice && MinPrice <= 0)
                            sched.price_range = "Complimentary";
                        else if (MinPrice != MaxPrice)
                            sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice < 0 ? 0 : MinPrice) + " - " + string.Format(new CultureInfo("en-US"), "{0:C}", MaxPrice) + "/pp";
                        else if (MinPrice == MaxPrice)
                            sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice) + "/pp";
                        sched.event_image = Convert.ToString(dataReader["EventImage"]);
                        int requiredLogin = Convert.ToInt32(dataReader["RequiredLogin"]);

                        sched.required_login = (requiredLogin > 0);

                        if (requiredLogin == 1)
                        {
                            sched.discount_only = false;
                            sched.require_login_message = string.Format("To book this experience, we must first verify your {0} membership status.", Convert.ToString(dataReader["MemberName"]));
                        }
                        else if (requiredLogin == 2)
                        {
                            sched.require_login_message = string.Format("Select members of {0} may receive special incentives after verification.", sched.event_location);
                            sched.discount_only = true;
                        }
                        sched.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        sched.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        sched.passport_promoted_event = false;
                        //sched.purchase_url = PurchaseURL + Convert.ToString(dataReader["PurchaseURL"]) + "-profile";
                        sched.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        sched.week_days = Convert.ToString(dataReader["WeekDays"]);
                        sched.charge_sales_tax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        sched.event_start_date = Convert.ToDateTime(dataReader["StartDate"]);
                        if (!System.DBNull.Value.Equals(dataReader["EndDate"]))
                        {
                            sched.event_end_date = Convert.ToDateTime(dataReader["EndDate"]);

                            if (Convert.ToDateTime(dataReader["StartDate"]).Date != Convert.ToDateTime(dataReader["EndDate"]).Date)
                                sched.is_multiple_dates = true;
                        }
                        else
                        {
                            sched.is_multiple_dates = true;
                        }

                        sched.total_reviews = Convert.ToInt32(dataReader["ReviewCount"]);
                        sched.review_stars = Convert.ToInt32(dataReader["ReviewStars"]);
                        sched.avg_review_value = Convert.ToDecimal(dataReader["AvgReviewValue"]);
                        sched.sold_out = Convert.ToBoolean(dataReader["soldOut"]);
                        DateTime currentDateTimeLocal = Convert.ToDateTime(dataReader["currentDateTimeLocal"]);
                        
                        if (sched.event_start_date.Date < currentDateTimeLocal.Date)
                            sched.is_past_date = true;

                        eventS.Add(sched);
                    }
                }
            }
            return eventS;
        }

        public EventV3 GetEventDetailsByEventId(int MemberId, int event_id, int slot_id, int slot_type, bool show_image)
        {
            var eventV3 = new EventV3();

            var eventS = new List<EventV2>();
            var parameterList = new List<DbParameter>();

            if (event_id == 0)
                event_id = -1;

            if (slot_id == 0)
            {
                slot_id = -1;
                slot_type = -1;
            }

            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@EventId", event_id));
            parameterList.Add(GetParameter("@SlotId", slot_id));
            parameterList.Add(GetParameter("@SlotType", slot_type));

            string OldDisplayName = string.Empty;
            var sched = new EventV2();

            using (DbDataReader dataReader = GetDataReader("GetEventDetailsByEventId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        eventV3.booked_count = Convert.ToInt32(dataReader["BookedCount"]);

                        string DisplayName = Convert.ToString(dataReader["EventName"]);

                        if (string.IsNullOrEmpty(OldDisplayName) || OldDisplayName != DisplayName)
                        {
                            sched = new EventV2();

                            OldDisplayName = DisplayName;
                            sched.event_name = DisplayName;
                            sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                            sched.event_floor_plans = GetEventFloorPlansByEventId(sched.event_id);
                            sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                            sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                            sched.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);
                            sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                            sched.event_location = Convert.ToString(dataReader["EventLocation"]);
                            //sched.duration = Convert.ToString(dataReader["Duration"]);
                            sched.charge_fee = Convert.ToInt32(dataReader["ChargeFee"]);
                            sched.qualified_purchase = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                            sched.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                            sched.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                            sched.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                            //sched.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                            //sched.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);

                            sched.duration = Convert.ToString(dataReader["Duration"]);
                            sched.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                            sched.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                            sched.min_lead_time = Convert.ToString(dataReader["LeadTime"]);
                            sched.max_lead_time = Convert.ToInt32(dataReader["MaximumLeadTime"]) + " Days";

                            sched.total_reviews = Convert.ToInt32(dataReader["ReviewCount"]);
                            sched.review_stars = Convert.ToInt32(dataReader["ReviewStars"]);
                            sched.avg_review_value = Convert.ToDecimal(dataReader["AvgReviewValue"]);

                            sched.event_start_date = Convert.ToDateTime(dataReader["EventStartDate"]);
                            DateTime event_end_date = Convert.ToDateTime(dataReader["EventEndDate"]);

                            if (event_end_date >= sched.event_start_date)
                                sched.event_end_date = event_end_date;

                            if (show_image)
                                sched.event_image = Convert.ToString(dataReader["EventImage"]);
                            int requiredLogin = Convert.ToInt32(dataReader["RequiredLogin"]);

                            sched.required_login = (requiredLogin > 0);

                            if (requiredLogin == 1)
                            {
                                sched.discount_only = false;
                                sched.require_login_message = string.Format("To book this experience, we must first verify your {0} membership status.", Convert.ToString(dataReader["MemberName"]));
                            }
                            else if (requiredLogin == 2)
                            {
                                sched.require_login_message = string.Format("Select members of {0} may receive special incentives after verification.", sched.event_location);
                                sched.discount_only = true;
                            }
                            sched.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                            sched.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                            sched.passport_promoted_event = false;
                            sched.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                            sched.week_days = Convert.ToString(dataReader["WeekDays"]);
                            sched.charge_sales_tax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);

                            sched.times = new List<EventTimeV2>();
                            eventS.Add(sched);
                        }

                        var eventTimeV2 = new EventTimeV2();

                        eventTimeV2.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        eventTimeV2.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        eventTimeV2.start_time = Convert.ToString(dataReader["StartTime"]);
                        eventTimeV2.end_time = Convert.ToString(dataReader["EndTime"]);
                        eventTimeV2.wait_list = Convert.ToBoolean(dataReader["WaitList"]);
                        sched.times.Add(eventTimeV2);
                    }

                    eventV3.events = eventS;
                }
            }
            return eventV3;
        }

        public List<EventTimeV2> GetSoldOutTimesByEventIdAndDate(DateTime StartDate, int event_id, List<int> slotIds)
        {
            var dateTimeObj = new List<EventTimeV2>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SlotIds", String.Join(",", slotIds.ToArray())));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EventId", event_id));
            parameterList.Add(GetParameter("@time", ""));

            using (DbDataReader dataReader = GetDataReader("GetSoldOutTimesByEventIdAndDate", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var eventTimeV2 = new EventTimeV2();
                        eventTimeV2.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        eventTimeV2.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        eventTimeV2.start_time = Convert.ToString(dataReader["StartTime"]);
                        eventTimeV2.end_time = Convert.ToString(dataReader["EndTime"]);
                        eventTimeV2.wait_list = Convert.ToBoolean(dataReader["WaitList"]);
                        eventTimeV2.sold_out = true;
                        dateTimeObj.Add(eventTimeV2);
                    }
                }
            }

            return dateTimeObj;
        }

        public AvailableDaysEvent GetAvailableDaysForEvent(int Guest, DateTime StartDate, int event_id, bool include_waitlist, bool include_hidden_member, bool passport_event)
        {
            var sched = new AvailableDaysEvent();

            var eventS = new List<EventV2>();
            var parameterList = new List<DbParameter>();

            if (event_id == 0)
                event_id = -1;

            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@Guest", Guest));
            parameterList.Add(GetParameter("@EventId", event_id));
            parameterList.Add(GetParameter("@IncludeWaitlist", include_waitlist));
            parameterList.Add(GetParameter("@IncludeHiddenMember", include_hidden_member));

            string OldDisplayName = string.Empty;
            DateTime prevDate = DateTime.MinValue;
            EventDateTimes dateTimeObj = null;
            using (DbDataReader dataReader = GetDataReader("GetNextAvailableDaysForEvent", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        sched.booked_count = Convert.ToInt32(dataReader["BookedCount"]);

                        string DisplayName = Convert.ToString(dataReader["EventName"]);
                        DateTime eventDate = Convert.ToDateTime(dataReader["EventDate"]);

                        if (string.IsNullOrEmpty(OldDisplayName) || OldDisplayName != DisplayName)
                        {

                            OldDisplayName = DisplayName;
                            sched.event_name = DisplayName;
                            sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                            sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                            sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                            sched.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);
                            sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                            sched.event_location = Convert.ToString(dataReader["EventLocation"]);
                            sched.duration = Convert.ToString(dataReader["Duration"]);
                            sched.charge_fee = Convert.ToInt32(dataReader["ChargeFee"]);
                            sched.qualified_purchase = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                            sched.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                            sched.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                            sched.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                            sched.min_persons = Convert.ToInt32(dataReader["MinPersons"]);
                            sched.max_persons = Convert.ToInt32(dataReader["MaxPersons"]);
                            sched.required_login = Convert.ToBoolean(dataReader["RequiredLogin"]);
                            sched.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                            sched.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                            sched.passport_promoted_event = false;
                            //sched.purchase_url = PurchaseURL + Convert.ToString(dataReader["PurchaseURL"]) + "-profile";
                            sched.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                            sched.week_days = Convert.ToString(dataReader["WeekDays"]);

                            if (passport_event)
                            {
                                sched.passport_promoted_event = PassportEvent(sched.event_id);
                            }
                            sched.event_date_times = new List<EventDateTimes>();
                        }

                        if (eventDate != prevDate)
                        {
                            prevDate = eventDate;
                            dateTimeObj = new EventDateTimes
                            {
                                event_date = eventDate,
                                times = new List<EventTimeV2>()
                            };
                            sched.event_date_times.Add(dateTimeObj);
                        };


                        var eventTimeV2 = new EventTimeV2();

                        eventTimeV2.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        eventTimeV2.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        eventTimeV2.start_time = Convert.ToString(dataReader["StartTime"]);
                        eventTimeV2.end_time = Convert.ToString(dataReader["EndTime"]);
                        eventTimeV2.wait_list = Convert.ToBoolean(dataReader["WaitList"]);
                        dateTimeObj.times.Add(eventTimeV2);



                    }
                    //sched.event_date_times.Add(dateTimeObj);

                    //eventV3.events = eventS;
                }
            }
            return sched;
        }


        public bool CheckEventAvailability(int MemberId, int Guest, DateTime StartDate, int slot_id, int slot_type, bool include_waitlist, bool include_hidden_member, string access_code, int reservation_id)
        {
            var parameterList = new List<DbParameter>();
            bool isAvailable = false;


            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@Guest", Guest));
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@SlotId", slot_id));
            parameterList.Add(GetParameter("@SlotType", slot_type));
            parameterList.Add(GetParameter("@IncludeWaitlist", include_waitlist));
            parameterList.Add(GetParameter("@IncludeHiddenMember", include_hidden_member));
            parameterList.Add(GetParameter("@AccessCode", access_code));
            parameterList.Add(GetParameter("@ReservationId", reservation_id));

            string OldDisplayName = string.Empty;
            var sched = new EventV2();

            using (DbDataReader dataReader = GetDataReader("CheckEventAvailability", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    isAvailable = true;
                }
            }
            return isAvailable;
        }

        public bool PassportEvent(int EventId)
        {
            bool IsPassportEvent = false;

            string sql = "select id from Tickets_Event_PassportMembers_Events where Event_Id = @EventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        IsPassportEvent = true;
                    }
                }
            }
            return IsPassportEvent;
        }

        public bool IsValidPayCardToken(int PaymentGatewayId,string PayCardToken,int WineryId,string PayCardCustName,string PayCardExpMonth,string PayCardExpYear)
        {
            bool IsValid = false;
             
            if (!string.IsNullOrEmpty(PayCardToken))
            {
                //string sql = "select top 1 Id from ReservationV2Payment (nolock) where PaymentGatewayId = @PaymentGatewayId and PayCardToken = @PayCardToken and [Status] = 1";

                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@PaymentGatewayId", PaymentGatewayId));
                parameterList.Add(GetParameter("@PayCardToken", PayCardToken));
                parameterList.Add(GetParameter("@WineryId", WineryId));
                parameterList.Add(GetParameter("@PayCardCustName", PayCardCustName));
                parameterList.Add(GetParameter("@PayCardExpMonth", PayCardExpMonth));
                parameterList.Add(GetParameter("@PayCardExpYear", PayCardExpYear));

                using (DbDataReader dataReader = GetDataReader("IsValidPayCardToken", parameterList, CommandType.StoredProcedure))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToBoolean(dataReader["IsValid"]);
                        }
                    }
                }
            }
            
            return IsValid;
        }

        //public bool IsValidPayCardToken2(int PaymentGatewayId, string PayCardToken, int WineryId)
        //{
        //    bool IsValid = false;

        //    if (!string.IsNullOrEmpty(PayCardToken))
        //    {
        //        string sql = "select top 1 reservationid from reservationv2 (nolock) r join PaymentConfig (nolock) p on p.winery_id = r.wineryid where PayCardToken = @PayCardToken and p.paymentgateway = @PaymentGatewayId and isactive=1 and WineryId = @WineryId";

        //        var parameterList = new List<DbParameter>();
        //        parameterList.Add(GetParameter("@PaymentGatewayId", PaymentGatewayId));
        //        parameterList.Add(GetParameter("@PayCardToken", PayCardToken));
        //        parameterList.Add(GetParameter("@WineryId", WineryId));

        //        using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
        //        {
        //            if (dataReader != null && dataReader.HasRows)
        //            {
        //                while (dataReader.Read())
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //    }

        //    return IsValid;
        //}

        //public bool IsValidPayCardToken3(string PayCardCustName, string PayCardExpMonth,string PayCardExpYear,int WineryId)
        //{
        //    bool IsValid = false;

        //    string sql = "select top 1 reservationid from reservationv2 (nolock) where PayCardCustName=@PayCardCustName and PayCardExpMonth=@PayCardExpMonth and PayCardExpYear = @PayCardExpYear and WineryId = @WineryId";

        //    var parameterList = new List<DbParameter>();
        //    parameterList.Add(GetParameter("@PayCardCustName", PayCardCustName));
        //    parameterList.Add(GetParameter("@PayCardExpMonth", PayCardExpMonth));
        //    parameterList.Add(GetParameter("@PayCardExpYear", PayCardExpYear));
        //    parameterList.Add(GetParameter("@WineryId", WineryId));

        //    using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
        //    {
        //        if (dataReader != null && dataReader.HasRows)
        //        {
        //            while (dataReader.Read())
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return IsValid;
        //}

        public List<DateTime> GetSoldoutDates(int eventId, int guests, DateTime eventDate)
        {
            List<DateTime> soldoutDates = new List<DateTime>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", eventId));
            parameterList.Add(GetParameter("@Guest", guests));
            parameterList.Add(GetParameter("@EventDate", eventDate));

            using (DbDataReader dataReader = GetDataReader("Get2MonthSoldoutDatesForEvent", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        soldoutDates.Add(Convert.ToDateTime(dataReader["EventDate"]));
                    }
                }
            }
            return soldoutDates;
        }
        public int GetWaitListIdByWaitListGuid(string WaitListGuid)
        {
            int WaitListId = 0;

            string sql = "select Id from ReservationV2Waitlist (nolock) where WaitListGuid = @WaitListGuid";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WaitListGuid", WaitListGuid));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        WaitListId = Convert.ToInt32(dataReader["Id"]);
                    }
                }
            }
            return WaitListId;
        }

        public string GetWaitListGuIdById(int Id)
        {
            string WaitListGuId = string.Empty;

            string sql = "select WaitListGuid from ReservationV2Waitlist (nolock) where Id = @Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        WaitListGuId = Convert.ToString(dataReader["WaitListGuId"]);
                    }
                }
            }
            return WaitListGuId;
        }


        public bool CheckPassportEvent(int event_id, ref int member_id)
        {
            bool isPassportEvent = false;
            string sql = "select winery_id from Tickets_Event where Id = @Id and Category=1";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", event_id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        member_id = Convert.ToInt32(dataReader["winery_id"]);
                        isPassportEvent = true;
                    }
                }
            }

            return isPassportEvent;
        }

        public AvailableEventsForFutureDate GetAvailableEventsForFutureDateV3(int MemberId, int Guest, DateTime StartDate, int event_id = -1, int slot_id = 0, int slot_type = 0, bool include_waitlist = false, bool include_hidden_member = false, string time = "", string access_code = "")
        {
            AvailableEventsForFutureDate availableEventsForFutureDate = new AvailableEventsForFutureDate();
            availableEventsForFutureDate.event_date = Convert.ToDateTime("1/1/1900");

            var parameterList = new List<DbParameter>();

            if (event_id == 0)
                event_id = -1;

            if (slot_id == 0)
            {
                slot_id = -1;
                slot_type = -1;
            }

            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@Guest", Guest));
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@EventId", event_id));
            parameterList.Add(GetParameter("@SlotId", slot_id));
            parameterList.Add(GetParameter("@SlotType", slot_type));
            parameterList.Add(GetParameter("@IncludeWaitlist", include_waitlist));
            parameterList.Add(GetParameter("@IncludeHiddenMember", include_hidden_member));
            parameterList.Add(GetParameter("@time", time));
            parameterList.Add(GetParameter("@AccessCode", access_code));

            using (DbDataReader dataReader = GetDataReader("GetAvailableEventsForFutureDateV3", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        availableEventsForFutureDate.event_date = Convert.ToDateTime(dataReader["eventdate"]);
                        availableEventsForFutureDate.start_time = Convert.ToDateTime(dataReader["starttime"]);
                    }
                }
            }
            return availableEventsForFutureDate;
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

        private bool RejectCreditCardOnDecline(int DepositPolicy)
        {
            bool reqCC = false;
            switch (DepositPolicy)
            {
                case 10:
                case 11:
                case 12:
                    reqCC = true;
                    break;
            }
            return reqCC;
        }





        public EventScheduleEvent GetEventDetail(DateTime StartTime, int EventId, bool IsAdmin, int slotId = 0, int slotType = 0)
        {
            string sp = "GetEventsByWineryIdAndDateClientV2";
            var eventScheduleEvent = new EventScheduleEvent();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", 0));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@GuestsCount", -1));
            parameterList.Add(GetParameter("@SlotId", slotId));
            parameterList.Add(GetParameter("@SlotType", slotType));
            parameterList.Add(GetParameter("@callbtnSearch", false));
            parameterList.Add(GetParameter("@EventIdnum", EventId));

            int wineryId = 0;
            var winery = new WineryModel();
            List<SettingModel> settingsGroup = null;
            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var sched = new EventScheduleEvent();
                        if (wineryId == 0)
                        {
                            wineryId = Convert.ToInt32(dataReader["wineryId"]);
                            winery = GetWineryById(wineryId);
                            if (winery.EnableClubemember == false && winery.EnableClubSalesforce == false && winery.EnableClubVin65 == false)
                            {
                                //If none of the previous club feaures are enabled then check if bLoyal is enabled since we don't need to hit db again if one of these is already active
                                settingsGroup = GetSettingGroup(wineryId, uc.Common.SettingGroup.bLoyal);
                            }
                        }

                        sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                        sched.event_name = Convert.ToString(dataReader["EventName"]);
                        sched.event_location = Convert.ToString(dataReader["EventLocation"]);
                        sched.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                        sched.duration_desc = Convert.ToString(dataReader["Duration"]);
                        sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                        sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        sched.all_inclusive_price = Convert.ToDecimal(dataReader["ReservationChanges"]);
                        sched.lead_time_desc = Convert.ToString(dataReader["LeadTime"]);
                        sched.max_lead_time_desc = Convert.ToInt32(dataReader["MaximumLeadTime"]) + " Days";
                        sched.cancel_lead_time_in_minutes = Convert.ToInt32(dataReader["CancelTimeInMinutes"]);

                        string CancelTimeDesc = Convert.ToString(dataReader["CancelTime"]);
                        sched.cancel_time_desc = CancelTimeDesc;
                        //sched.min_persons = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["MinPersons"])) ? 0 : Convert.ToInt32(dataReader["MinPersons"]);
                        //sched.max_persons = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["MaxPersons"])) ? 0 : Convert.ToInt32(dataReader["MaxPersons"]);
                        int MinPersons = 0;
                        int MaxPersons = 0;
                        sched.member_id = wineryId;
                        var ChargeFee = (uc.Common.ChargeFee)(string.IsNullOrWhiteSpace(Convert.ToString(dataReader["ChargeFee"])) ? 0 : Convert.ToInt32(dataReader["ChargeFee"]));
                        decimal QualifiedPurchaseAmount = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                        int MemberBenefit = Convert.ToInt32(dataReader["MemberBenefit"]);
                        bool ChargeSalesTax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        string ShipcompliantAPIUserName = Convert.ToString(dataReader["ShipcompliantAPIUserName"]);
                        string ShipcompliantApiPassword = Convert.ToString(dataReader["ShipcompliantApiPassword"]);
                        bool EnableShipCompliant = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableShipCompliant"])) ? false : Convert.ToBoolean(dataReader["EnableShipCompliant"]);
                        sched.show_additional_guests = Convert.ToBoolean(dataReader["ShowAddlGuests"]);
                        sched.show_guest_tags = Convert.ToBoolean(dataReader["ShowGuestTags"]);
                        sched.require_additional_guests = Convert.ToBoolean(dataReader["ReqAddlGuests"]);
                        sched.table_status_group_id = Convert.ToInt32(dataReader["Table_Status_Group_Id"]);
                        sched.member_url = Convert.ToString(dataReader["PurchaseUrl"]);
                        sched.email_content_id = Convert.ToInt32(dataReader["EmailContentId"].ToString());
                        sched.event_start_date = DateTime.Parse(dataReader["StartDate"].ToString());
                        sched.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                        sched.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                        DateTime dtEndDate = DateTime.MinValue;
                        if (DateTime.TryParse(dataReader["EndDate"].ToString(), out dtEndDate))
                        {
                            if (dtEndDate >= sched.event_start_date)
                            {
                                sched.event_end_date = dtEndDate;
                            }
                        }

                        List<EventScheduleTime> eventTimes = new List<EventScheduleTime>();
                        int activeCount = 0;
                        if ((Convert.ToString(dataReader["EventTimings"]) != null))
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataReader["EventTimings"])))
                            {
                                foreach (var t_loopVariable in Convert.ToString(dataReader["EventTimings"]).Split(','))
                                {
                                    string[] timeArray = t_loopVariable.Split('|');
                                    EventScheduleTime eTime = new EventScheduleTime();
                                    eTime.slot_id = Convert.ToInt32(timeArray[1]);
                                    eTime.time = Convert.ToString(timeArray[0]);
                                    eTime.end_time = Convert.ToString(timeArray[18]);
                                    eTime.is_available = Convert.ToBoolean(Convert.ToInt32(timeArray[2]));
                                    eTime.slot_type = Convert.ToInt32(timeArray[3]);
                                    eTime.seats_left = Convert.ToInt32(timeArray[4]);

                                    MinPersons = Convert.ToInt32(timeArray[10]);
                                    MaxPersons = Convert.ToInt32(timeArray[11]);

                                    string color = Convert.ToString(timeArray[13]);
                                    eTime.event_total_seats = Convert.ToInt32(timeArray[8]);
                                    eTime.event_duration_minutes = Convert.ToInt32(timeArray[14]);
                                    if (Convert.ToDateTime(timeArray[5]) == Convert.ToDateTime(timeArray[6]))
                                        eTime.is_recurring = false;
                                    else
                                        eTime.is_recurring = true;

                                    eTime.slot_color = 3;

                                    if (color.Contains("closedevent"))
                                    {
                                        eTime.slot_color = 0;
                                    }
                                    else if (color.Contains("autoclosedevent"))
                                    {
                                        eTime.slot_color = 1;
                                    }
                                    else if (color.Contains("privateevent"))
                                    {
                                        eTime.slot_color = 2;
                                    }
                                    else if (color.Contains("nobookings"))
                                    {
                                        eTime.slot_color = 3;
                                    }
                                    else if (color.Contains("oversold"))
                                    {
                                        eTime.slot_color = 4;
                                    }
                                    else if (color.Contains("soldout"))
                                    {
                                        eTime.slot_color = 5;
                                    }
                                    else if (color.Contains("eventbooked"))
                                    {
                                        eTime.slot_color = 6;
                                    }

                                    eventTimes.Add(eTime);
                                    if (eTime.is_available == true)
                                    {
                                        activeCount += 1;
                                    }
                                }
                            }
                        }

                        sched.event_times = eventTimes;

                        sched.min_persons = MinPersons;
                        sched.max_persons = MaxPersons;

                        if (sched.fee_Per_person > 0)
                        {
                            if (ChargeFee == uc.Common.ChargeFee.ComplimentaryWithPurchase)
                            {
                                sched.fee_per_person_desc = string.Format("{0}, Waived with Min. Qualified Purchase ({1} Per Person) ", string.Format(new CultureInfo("en-US"), "{0:C}", sched.fee_Per_person), string.Format(new CultureInfo("en-US"), "{0:C}", QualifiedPurchaseAmount));
                            }
                            else
                            {
                                if (EnableShipCompliant && ChargeSalesTax && !string.IsNullOrEmpty(ShipcompliantApiPassword) && !string.IsNullOrEmpty(ShipcompliantAPIUserName))
                                {
                                    sched.fee_per_person_desc = string.Format(new CultureInfo("en-US"), "{0:C}", sched.fee_Per_person) + " Per Person, plus tax";
                                }
                                else
                                {
                                    sched.fee_per_person_desc = string.Format(new CultureInfo("en-US"), "{0:C}", sched.fee_Per_person) + " Per Person";
                                }
                            }
                        }
                        else
                        {
                            sched.fee_per_person_desc = "Complimentary";
                        }

                        sched.club_member_benefit = uc.Common.GetMemberBenefit().Where(f => f.ID == MemberBenefit.ToString()).Select(f => f.Name).FirstOrDefault();

                        sched.show_club_member_benefits = false;
                        if (sched.member_id > 0)
                        {
                            bool bLoyalClubLookupEnabled = false;

                            if ((settingsGroup != null))
                            {
                                if (settingsGroup.Count > 0)
                                {
                                    bool ret = false;
                                    dynamic dbSettings = settingsGroup.Where(f => f.Key == uc.Common.SettingKey.bLoyalApiClubLookup).FirstOrDefault();

                                    if ((dbSettings != null))
                                    {
                                        bool.TryParse(dbSettings.Value, out ret);
                                    }

                                    if (ret == true)
                                    {
                                        bLoyalClubLookupEnabled = true;
                                    }
                                }
                            }
                            if ((winery.EnableClubemember || winery.EnableClubSalesforce || winery.EnableClubVin65 || bLoyalClubLookupEnabled) && MemberBenefit > 0)
                            {
                                sched.show_club_member_benefits = true;
                            }
                        }

                        sched.deposit_policy = uc.Common.GetDepositPolicies(IsAdmin).Where(f => f.ID == ((int)ChargeFee).ToString()).Select(f => f.Name).FirstOrDefault();

                        if (CancelTimeDesc == "Non-changeable, Non-cancellable")
                        {
                            sched.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                        }
                        else if (CancelTimeDesc.IndexOf("Days") > -1)
                        {
                            try
                            {
                                int CancelTime = Convert.ToInt32(CancelTimeDesc.Replace("Days", "").Replace(" ", ""));

                                if (CancelTime > 20)
                                {
                                    sched.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                                }
                                else
                                {
                                    sched.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                                }
                            }
                            catch
                            {
                                sched.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                            }
                        }
                        else
                        {
                            sched.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                        }

                        sched.require_credit_card = requiresCreditCard(Convert.ToInt32(dataReader["ChargeFee"]));
                        sched.event_addOn = GetEventAddOnGroupByEventId(sched.event_id);
                        sched.show_promo_code = GetActiveDiscountsByEventId(sched.event_id).Count > 0;
                        sched.show_passport_code = isRsvpEventInAnyPassports(sched.event_id, StartTime);
                        sched.reject_credit_card_on_decline = RejectCreditCardOnDecline(Convert.ToInt32(dataReader["ChargeFee"]));
                        sched.show_transportation = false;
                        int TransportationShowHideReq = Convert.ToInt32(dataReader["TransportationShowHideReq"]);
                        //Transportation - 0 = hide, 1 = Show, 2 = Require

                        if (TransportationShowHideReq == 1 || TransportationShowHideReq == 2)
                        {
                            sched.show_transportation = ShowTransportation(sched.event_id);
                        }
                        sched.gratuity_percentage = Convert.ToDecimal(dataReader["GratuityPercentage"]);
                        sched.tax_gratuity = Convert.ToBoolean(dataReader["TaxGratuity"]);
                        sched.event_questions = GetEventQuestions(sched.event_id);
                        sched.event_floor_plans = GetEventFloorPlansByEventId(sched.event_id);
                        eventScheduleEvent = sched;
                    }
                }
            }
            return eventScheduleEvent;
        }

        public List<EventQuestion> GetEventQuestions(int eventId)
        {
            List<EventQuestion> questions = new List<EventQuestion>();
            string sp = "GetEventQuestionsAndChoices";
            var sched = new ReservationCheckoutModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", eventId));
            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        List<string> lstChoices = null;
                        var choices = Convert.ToString(dataReader["Choices"]);
                        if (!string.IsNullOrEmpty(choices))
                        {
                            lstChoices = choices.Split("@!@").ToList();
                        }


                        var question = new EventQuestion
                        {
                            id = Convert.ToInt32(dataReader["Id"]),
                            is_required = Convert.ToBoolean(dataReader["IsRequired"]),
                            question_text = Convert.ToString(dataReader["QuestionText"]),
                            question_type = Convert.ToString(dataReader["QuestionType"]),
                            choices = lstChoices
                        };
                        questions.Add(question);
                    }
                }
            }
            return questions;
        }



        public ReservationCheckoutModel GetEventDataForRSVPCheckout(DateTime StartTime, bool IsAdmin, int guests, int memberId, int slotId, int slotType)
        {
            string sp = "GetCheckOutDataBySlotId";
            var sched = new ReservationCheckoutModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", memberId));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@GuestsCount", guests));
            parameterList.Add(GetParameter("@SlotId", slotId));
            parameterList.Add(GetParameter("@SlotType", slotType));

            int wineryId = 0;
            var winery = new WineryModel();
            List<SettingModel> settingsGroup = null;
            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        if (wineryId == 0)
                        {
                            wineryId = Convert.ToInt32(dataReader["wineryId"]);
                            winery = GetWineryById(wineryId);
                            if (winery.EnableClubemember == false && winery.EnableClubSalesforce == false && winery.EnableClubVin65 == false)
                            {
                                //If none of the previous club feaures are enabled then check if bLoyal is enabled since we don't need to hit db again if one of these is already active
                                settingsGroup = GetSettingGroup(wineryId, uc.Common.SettingGroup.bLoyal);
                            }
                        }

                        sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                        sched.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                        sched.event_name = Convert.ToString(dataReader["EventName"]);
                        sched.event_location = Convert.ToString(dataReader["EventLocation"]);
                        sched.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        sched.event_desc = Convert.ToString(dataReader["EventDesc"]);
                        sched.duration_desc = Convert.ToString(dataReader["Duration"]);
                        sched.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        sched.lead_time_desc = Convert.ToString(dataReader["LeadTime"]);
                        sched.max_lead_time_desc = Convert.ToInt32(dataReader["MaximumLeadTime"]) + " Days";
                        sched.cancel_lead_time_in_minutes = Convert.ToInt32(dataReader["CancelTimeInMinutes"]);
                        sched.required_hdyh = Convert.ToInt32(dataReader["HDYH_Requirement"]);

                        string CancelTimeDesc = Convert.ToString(dataReader["CancelTime"]);
                        sched.cancel_time_desc = CancelTimeDesc;
                        //sched.min_persons = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["MinPersons"])) ? 0 : Convert.ToInt32(dataReader["MinPersons"]);
                        //sched.max_persons = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["MaxPersons"])) ? 0 : Convert.ToInt32(dataReader["MaxPersons"]);
                        int MinPersons = 0;
                        int MaxPersons = 0;
                        sched.member_id = wineryId;
                        var ChargeFee = (uc.Common.ChargeFee)(string.IsNullOrWhiteSpace(Convert.ToString(dataReader["ChargeFee"])) ? 0 : Convert.ToInt32(dataReader["ChargeFee"]));
                        decimal QualifiedPurchaseAmount = Convert.ToDecimal(dataReader["QualifiedPurchase"]);
                        int MemberBenefit = Convert.ToInt32(dataReader["MemberBenefit"]);
                        bool ChargeSalesTax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        sched.ignore_sales_tax = !ChargeSalesTax;
                        string ShipcompliantAPIUserName = Convert.ToString(dataReader["ShipcompliantAPIUserName"]);
                        string ShipcompliantApiPassword = Convert.ToString(dataReader["ShipcompliantApiPassword"]);
                        bool EnableShipCompliant = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableShipCompliant"])) ? false : Convert.ToBoolean(dataReader["EnableShipCompliant"]);
                        sched.show_additional_guests = Convert.ToBoolean(dataReader["ShowAddlGuests"]);
                        sched.show_guest_tags = Convert.ToBoolean(dataReader["ShowGuestTags"]);
                        sched.require_additional_guests = Convert.ToBoolean(dataReader["ReqAddlGuests"]);
                        sched.table_status_group_id = Convert.ToInt32(dataReader["Table_Status_Group_Id"]);
                        sched.member_url = Convert.ToString(dataReader["PurchaseUrl"]);
                        sched.member_benefits_url = Convert.ToString(dataReader["MemberBenefitsURL"]);
                        sched.email_content_id = Convert.ToInt32(dataReader["EmailContentId"].ToString());
                        sched.event_start_date = DateTime.Parse(dataReader["EventStartDate"].ToString());
                        sched.location_street1 = Convert.ToString(dataReader["LocationStreet1"]);
                        sched.location_street2 = Convert.ToString(dataReader["LocationStreet2"]);
                        sched.location_city = Convert.ToString(dataReader["LocationCity"]);
                        sched.location_state = Convert.ToString(dataReader["LocationState"]);
                        sched.location_zip = Convert.ToString(dataReader["LocationZip"]);
                        sched.sms_provider = Convert.ToInt32(dataReader["SMSProvider"]);

                        string SmsNumber = Convert.ToString(dataReader["SmsNumber"]);

                        //if (string.IsNullOrEmpty(SmsNumber))
                        //    sched.sms_notification_available = false;
                        //else
                        //    sched.sms_notification_available = true;

                        DateTime dtEndDate = DateTime.MinValue;
                        if (DateTime.TryParse(dataReader["EventEndDate"].ToString(), out dtEndDate))
                        {
                            if (dtEndDate >= sched.event_start_date)
                            {
                                sched.event_end_date = dtEndDate;
                            }
                        }
                        sched.min_persons = MinPersons;
                        sched.max_persons = MaxPersons;

                        if (sched.fee_Per_person > 0)
                        {
                            if (ChargeFee == uc.Common.ChargeFee.ComplimentaryWithPurchase)
                            {
                                sched.fee_per_person_desc = string.Format("{0}, Waived with Min. Qualified Purchase ({1} Per Person) ", string.Format(new CultureInfo("en-US"), "{0:C}", sched.fee_Per_person), string.Format(new CultureInfo("en-US"), "{0:C}", QualifiedPurchaseAmount));
                            }
                            else
                            {
                                if (EnableShipCompliant && ChargeSalesTax && !string.IsNullOrEmpty(ShipcompliantApiPassword) && !string.IsNullOrEmpty(ShipcompliantAPIUserName))
                                {
                                    sched.fee_per_person_desc = string.Format(new CultureInfo("en-US"), "{0:C}", sched.fee_Per_person) + " Per Person, plus tax";
                                }
                                else
                                {
                                    sched.fee_per_person_desc = string.Format(new CultureInfo("en-US"), "{0:C}", sched.fee_Per_person) + " Per Person";
                                }
                            }
                        }
                        else
                        {
                            sched.fee_per_person_desc = "Complimentary";
                        }

                        sched.club_member_benefit = uc.Common.GetMemberBenefit().Where(f => f.ID == MemberBenefit.ToString()).Select(f => f.Name).FirstOrDefault();

                        sched.show_club_member_benefits = false;
                        if (sched.member_id > 0)
                        {
                            bool bLoyalClubLookupEnabled = false;

                            if ((settingsGroup != null))
                            {
                                if (settingsGroup.Count > 0)
                                {
                                    bool ret = false;
                                    dynamic dbSettings = settingsGroup.Where(f => f.Key == uc.Common.SettingKey.bLoyalApiClubLookup).FirstOrDefault();

                                    if ((dbSettings != null))
                                    {
                                        bool.TryParse(dbSettings.Value, out ret);
                                    }

                                    if (ret == true)
                                    {
                                        bLoyalClubLookupEnabled = true;
                                    }
                                }
                            }
                            if ((winery.EnableClubemember || winery.EnableClubCommerce7 || winery.EnableClubShopify || winery.EnableClubOrderPort || winery.EnableClubAMS || winery.EnableClubMicroworks || winery.EnableClubCoresense || winery.EnableClubVin65 || bLoyalClubLookupEnabled) && MemberBenefit > 0)
                            {
                                sched.show_club_member_benefits = true;
                            }
                        }

                        sched.charge_fee = (int)ChargeFee;
                        sched.deposit_policy = uc.Common.GetDepositPolicies(IsAdmin).Where(f => f.ID == ((int)ChargeFee).ToString()).Select(f => f.Name).FirstOrDefault();

                        if (CancelTimeDesc == "Non-changeable, Non-cancellable")
                        {
                            sched.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                        }
                        else if (CancelTimeDesc.IndexOf("Days") > -1)
                        {
                            try
                            {
                                int CancelTime = Convert.ToInt32(CancelTimeDesc.Replace("Days", "").Replace(" ", ""));

                                if (CancelTime > 20)
                                {
                                    sched.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                                }
                                else
                                {
                                    sched.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                                }
                            }
                            catch
                            {
                                sched.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                            }
                        }
                        else
                        {
                            sched.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                        }

                        sched.require_credit_card = requiresCreditCard(Convert.ToInt32(dataReader["ChargeFee"]));
                        sched.event_addOn = GetEventAddOnGroupByEventId(sched.event_id);

                        decimal MinPrice = Convert.ToDecimal(dataReader["MinReservationChanges"]);
                        decimal MaxPrice = Convert.ToDecimal(dataReader["MaxReservationChanges"]);

                        //decimal MinPrice = 0;
                        //decimal MaxPrice = 0;
                        //bool setPrice = false;

                        //List<AddOn_Group> addonlist1 = sched.event_addOn.Where(c => c.group_type == 1).ToList();
                        //List<AddOn_Group> addonlist2 = sched.event_addOn.Where(c => c.group_type == 2).ToList();

                        //if (addonlist1.Count > 0)
                        //{
                        //    var min = addonlist1[0].addOn_group_items.Min(o => o.price);
                        //    var max = addonlist1[0].addOn_group_items.Max(o => o.price);

                        //    if (!setPrice)
                        //    {
                        //        MinPrice = min;
                        //        MaxPrice = max;
                        //        setPrice = true;
                        //    }
                        //    else
                        //    {
                        //        if (MinPrice > min)
                        //            MinPrice = min;

                        //        if (MaxPrice < max)
                        //            MaxPrice = max;
                        //    }
                        //}

                        //if (addonlist2.Count > 0)
                        //{
                        //    var min = addonlist2[0].addOn_group_items.Min(o => o.price);
                        //    var max = addonlist2[0].addOn_group_items.Max(o => o.price);

                        //    if (!setPrice)
                        //    {
                        //        MinPrice = min;
                        //        MaxPrice = max;
                        //        setPrice = true;
                        //    }
                        //    else
                        //    {
                        //        if (MinPrice > min)
                        //            MinPrice = min;

                        //        if (MaxPrice < max)
                        //            MaxPrice = max;
                        //    }
                        //}

                        //MinPrice = MinPrice + sched.fee_Per_person;
                        //MaxPrice = MaxPrice + sched.fee_Per_person;

                        if (MinPrice == MaxPrice && MinPrice <= 0)
                            sched.price_range = "Complimentary";
                        else if (MinPrice != MaxPrice)
                            sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice < 0 ? 0 : MinPrice) + " - " + string.Format(new CultureInfo("en-US"), "{0:C}", MaxPrice) + "/pp";
                        else if (MinPrice == MaxPrice)
                            sched.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", MinPrice) + "/pp";

                        sched.show_promo_code = GetActiveDiscountsByEventId(sched.event_id).Count > 0;
                        sched.show_passport_code = isRsvpEventInAnyPassports(sched.event_id, StartTime);
                        sched.reject_credit_card_on_decline = RejectCreditCardOnDecline(Convert.ToInt32(dataReader["ChargeFee"]));
                        sched.show_transportation = false;
                        int TransportationShowHideReq = Convert.ToInt32(dataReader["TransportationShowHideReq"]);
                        //Transportation - 0 = hide, 1 = Show, 2 = Require

                        if (TransportationShowHideReq == 1 || TransportationShowHideReq == 2)
                        {
                            sched.show_transportation = ShowTransportation(sched.event_id);
                        }

                        sched.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        sched.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        sched.payment_enabled = Convert.ToBoolean(dataReader["EnablePayments"]);
                        string EnabledCreditCards = Convert.ToString(dataReader["EnabledCreditCards"]);
                        sched.accepted_card_types = EnabledCreditCards.Replace("32","20").Split(',').Select(Int32.Parse).ToList();
                        sched.member_name = Convert.ToString(dataReader["MemberName"]);
                        sched.transaction_fees = Convert.ToDecimal(dataReader["TransactionFee"]);
                        uc.Common.BillingPlanTransactionType tranType = (uc.Common.BillingPlanTransactionType)Convert.ToInt32(dataReader["TransactionType"].ToString());
                        sched.show_fees = (tranType == uc.Common.BillingPlanTransactionType.PerPersonPaidByGuest || tranType == uc.Common.BillingPlanTransactionType.PerRsvpPaidByGuest || tranType == uc.Common.BillingPlanTransactionType.PercentByGuest);
                        sched.gratuity_percentage = Convert.ToDecimal(dataReader["GratuityPercentage"]);
                        sched.tax_gratuity = Convert.ToBoolean(dataReader["TaxGratuity"]);
                        sched.ignore_deposit_cc_policy_on_zero_total = Convert.ToBoolean(dataReader["IgnoreDepositCCPolicyOnZeroTotal"]);
                        sched.event_questions = GetEventQuestions(sched.event_id);
                        sched.event_image = Convert.ToString(dataReader["EventImage"]);
                        sched.commerce7_enabled = Convert.ToBoolean(dataReader["EnableCommerce7"]);
                        sched.detailed_address_info_required = IsDetailedAddressInfoRequired(wineryId);
                        sched.event_type = Convert.ToString(dataReader["EventTypeName"]);
                        sched.event_sku = Convert.ToString(dataReader["RMSsku"]);
                        sched.member_website = Convert.ToString(dataReader["WebsiteURL"]);
                        sched.member_phone = Convert.ToString(dataReader["MemberPhone"]);

                        if (dataReader["CancelByDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["CancelByDate"].ToString()))
                            sched.cancel_by_date = Convert.ToDateTime(dataReader["CancelByDate"]);

                        sched.cancel_message = Convert.ToString(dataReader["CancelMessage"]);

                        sched.location_timezone = Convert.ToInt32(dataReader["TimeZoneId"]);
                        sched.location_timezone_offset = Times.GetOffsetMinutes((Times.TimeZone)sched.location_timezone);
                    }
                }
            }
            return sched;
        }

        public DateTime GetAvailableEventsForFutureDate(int wineryId, DateTime StartTime, int guestCount, int EventId = 0)
        {
            DateTime EventFutureDate = Convert.ToDateTime("1/1/1900");

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@GuestsCount", guestCount));
            parameterList.Add(GetParameter("@EventId", EventId));
            using (DbDataReader dataReader = GetDataReader("GetAvailableEventsForFutureDate", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        EventFutureDate = Convert.ToDateTime(dataReader["EventDate"]);
                    }
                }
            }
            return EventFutureDate;
        }

        public string IsHoliday(int wineryId, DateTime reqDate)
        {
            string HolidayName = string.Empty;

            string sql = "CheckHolidayByMemberIdDate";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", wineryId));
            parameterList.Add(GetParameter("@reqDate", reqDate));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        HolidayName = Convert.ToString(dataReader["HolidayName"]);
                    }
                }
            }
            return HolidayName;
        }

        public MaxSeatsLeft GetMaxSeatsLeftByWineryIdAndDate(int wineryId, DateTime StartTime)
        {
            MaxSeatsLeft maxSeatsLeft = new MaxSeatsLeft();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            using (DbDataReader dataReader = GetDataReader("GetMaxSeatsLeftByWineryIdAndDate", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        maxSeatsLeft.max_seats = Convert.ToInt32(dataReader["MaxSeatsLeft"]);
                        maxSeatsLeft.min_seats = Convert.ToInt32(dataReader["MinSeatsLeft"]);
                    }
                }
            }

            return maxSeatsLeft;
        }


        public MaxSeatsLeft GetMaxSeatsLeftByWineryIdAndDateV3(int wineryId, DateTime StartTime, int event_id = -1, int slot_id = -1, int slot_type = -1)
        {
            MaxSeatsLeft maxSeatsLeft = new MaxSeatsLeft();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", wineryId));
            parameterList.Add(GetParameter("@StartDate", StartTime));
            parameterList.Add(GetParameter("@EventId", event_id));
            parameterList.Add(GetParameter("@SlotId", slot_id));
            parameterList.Add(GetParameter("@SlotType", slot_type));
            using (DbDataReader dataReader = GetDataReader("GetMaxSeatsLeftByWineryIdAndDateV3", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        maxSeatsLeft.max_seats = Convert.ToInt32(dataReader["MaxSeatsLeft"]);
                        maxSeatsLeft.min_seats = Convert.ToInt32(dataReader["MinSeatsLeft"]);
                    }
                }
            }

            return maxSeatsLeft;
        }

        public DateTime GetMaxEndDateByWineryId(int wineryId, int event_id = 0, int slot_id = -1, int slot_type = -1)
        {
            DateTime MaxEndDate = Convert.ToDateTime("1/1/1900");

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@EventId", event_id));
            parameterList.Add(GetParameter("@SlotId", slot_id));
            parameterList.Add(GetParameter("@SlotType", slot_type));
            using (DbDataReader dataReader = GetDataReader("GetMaxEndDateByWineryId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        MaxEndDate = Convert.ToDateTime(dataReader["MaxEndDate"]);
                    }
                }
            }
            return MaxEndDate;
        }

        public WineryModel GetWinerySmsNumberById(int wineryId)
        {
            var winery = new WineryModel();

            string sql = "select SmsNumber,DisplayName from winery where Id = @wineryId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", wineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        winery.SmsNumber = Convert.ToString(dataReader["SmsNumber"]);
                        winery.DisplayName = Convert.ToString(dataReader["DisplayName"]);
                    }
                }
            }
            return winery;

        }
        public WineryModel GetWineryById(int wineryId)
        {
            var winery = new WineryModel();

            string sql = "select EnableClubeWinery,EnableClubSalesforce,EnableClubVin65,EnableVin65,Vin65Password,Vin65UserName,eWineryEnabled,eWineryUserNAme,eWineryPAssword,";
            sql += "SalesForceUserName,SalesForcePassword,VintegrateAPIUsername,SalesForceSecurityToken,EnableVintegrate,VintegrateAPIPassword,VintegratePartnerID,DirectionsURL,";
            sql += "DynamicPaymentDesc,DisplayName,BusinessPhone,EnableOrderPort,EnableClubOrderPort,OrderPortClientId,OrderPortApiKey,OrderPortApiToken,EnableCommerce7,EnableClubCommerce7,";
            sql += "Commerce7Username,Commerce7Password,Commerce7Tenant,Commerce7POSProfileId,ChargeSalesTaxOnPrivateEvents,SubscriptionPlan,AttendeeAppUsername,ReServeCloudSiteId,ReServeCloudApiUserName,";
            sql += "ReServeCloudApiPassword,Hidden,EnableClubeCellar,EnableClubAMS,EnableClubMicroworks,EnableClubCoresense,isnull(c.CurrencySymbol,'$') CurrencySymbol, ClubStatusField, UpsertOrderDateAs, UpsertShipDateAs, Address1, Address2, City,";
            sql += "w.State, Zip, EnableService,ThirdPartyTransaction, url,w.Country,EmailAddress, w.Active, BillingFirstName, BillingLastName, PurchaseURL, BillingEmail, Isnull(a.FriendlyName, '') as Appellation,DecryptKey,SALT,EnableShopify,EnableClubShopify,ShopifyPublishKey,ShopifySecretKey,ShopifyAppPassword,ShopifyAppStoreName,EnableCustomOrderType,CustomOrderType,";
            sql += "EnableGoogleSync,InventoryMode, GoogleUsername, GooglePassword, timezoneId,EnabledCreditCards,EnableUpsertCreditCard, UpsertFulfillmentDate, EnableBigCommerce, EnableClubBigCommerce, BigCommerceAceessToken, BigCommerceStoreId from Winery (nolock) w left join Country (nolock) c on c.iso = w.Country left join WineryAppellations (nolock) a on w.Appelation = a.Id where w.Id= @wineryId";


            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", wineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        winery.ChargeSalesTaxOnPrivateEvents = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["ChargeSalesTaxOnPrivateEvents"])) ? false : Convert.ToBoolean(dataReader["ChargeSalesTaxOnPrivateEvents"]);
                        winery.EnableClubemember = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableClubeWinery"])) ? false : Convert.ToBoolean(dataReader["EnableClubeWinery"]);
                        winery.EnableClubSalesforce = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableClubSalesforce"])) ? false : Convert.ToBoolean(dataReader["EnableClubSalesforce"]);
                        winery.EnableClubVin65 = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableClubVin65"])) ? false : Convert.ToBoolean(dataReader["EnableClubVin65"]);
                        winery.EnableVin65 = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableVin65"])) ? false : Convert.ToBoolean(dataReader["EnableVin65"]);
                        winery.Vin65Password = Convert.ToString(dataReader["Vin65Password"]);
                        winery.Vin65UserName = Convert.ToString(dataReader["Vin65UserName"]);
                        winery.eMemberEnabled = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["eWineryEnabled"])) ? false : Convert.ToBoolean(dataReader["eWineryEnabled"]);
                        winery.eMemberUserNAme = Convert.ToString(dataReader["eWineryUserNAme"]);
                        winery.eMemberPAssword = Convert.ToString(dataReader["eWineryPAssword"]);
                        winery.SalesForceUserName = Convert.ToString(dataReader["SalesForceUserName"]);
                        winery.SalesForcePassword = Convert.ToString(dataReader["SalesForcePassword"]);
                        winery.SalesForceSecurityToken = Convert.ToString(dataReader["SalesForceSecurityToken"]);
                        winery.EnableVintegrate = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableVintegrate"])) ? false : Convert.ToBoolean(dataReader["EnableVintegrate"]);
                        winery.VintegrateAPIUsername = Convert.ToString(dataReader["VintegrateAPIUsername"]);
                        winery.VintegrateAPIPassword = Convert.ToString(dataReader["VintegrateAPIPassword"]);
                        winery.VintegratePartnerID = Convert.ToString(dataReader["VintegratePartnerID"]);
                        winery.EnabledCreditCards = Convert.ToString(dataReader["EnabledCreditCards"]);
                        winery.DirectionsURL = Convert.ToString(dataReader["DirectionsURL"]);
                        winery.DynamicPaymentDesc = Convert.ToString(dataReader["DynamicPaymentDesc"]);
                        winery.DisplayName = Convert.ToString(dataReader["DisplayName"]);
                        winery.BusinessPhone = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["BusinessPhone"])) ? 0 : Convert.ToDecimal(dataReader["BusinessPhone"]);
                        winery.EnableOrderPort = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableOrderPort"])) ? false : Convert.ToBoolean(dataReader["EnableOrderPort"]);
                        winery.EnableClubOrderPort = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableClubOrderPort"])) ? false : Convert.ToBoolean(dataReader["EnableClubOrderPort"]);
                        winery.OrderPortClientId = Convert.ToString(dataReader["OrderPortClientId"]);
                        winery.OrderPortApiKey = Convert.ToString(dataReader["OrderPortApiKey"]);
                        winery.OrderPortApiToken = Convert.ToString(dataReader["OrderPortApiToken"]);
                        winery.EnableCommerce7 = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableCommerce7"])) ? false : Convert.ToBoolean(dataReader["EnableCommerce7"]);
                        winery.EnableClubCommerce7 = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableClubCommerce7"])) ? false : Convert.ToBoolean(dataReader["EnableClubCommerce7"]);
                        winery.Commerce7Username = Convert.ToString(dataReader["Commerce7Username"]);
                        winery.Commerce7Password = Convert.ToString(dataReader["Commerce7Password"]);
                        winery.Commerce7Tenant = Convert.ToString(dataReader["Commerce7Tenant"]);
                        winery.Commerce7POSProfileId = Convert.ToString(dataReader["Commerce7POSProfileId"]);

                        winery.EnableShopify = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableShopify"])) ? false : Convert.ToBoolean(dataReader["EnableShopify"]);
                        winery.EnableClubShopify = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableClubShopify"])) ? false : Convert.ToBoolean(dataReader["EnableClubShopify"]);
                        winery.ShopifyPublishKey = Convert.ToString(dataReader["ShopifyPublishKey"]);
                        winery.ShopifySecretKey = Convert.ToString(dataReader["ShopifySecretKey"]);
                        winery.ShopifyAppPassword = Convert.ToString(dataReader["ShopifyAppPassword"]);
                        winery.ShopifyAppStoreName = Convert.ToString(dataReader["ShopifyAppStoreName"]);

                        winery.SubscriptionPlan = Convert.ToInt32(dataReader["SubscriptionPlan"]);
                        winery.InventoryMode = Convert.ToInt32(dataReader["InventoryMode"]);
                        winery.AttendeeAppUsername = Convert.ToString(dataReader["AttendeeAppUsername"]);
                        winery.ReServeCloudApiPassword = Convert.ToString(dataReader["ReServeCloudApiPassword"]);
                        winery.ReServeCloudApiUserName = Convert.ToString(dataReader["ReServeCloudApiUserName"]);
                        winery.ReServeCloudSiteId = Convert.ToString(dataReader["ReServeCloudSiteId"]);
                        winery.HiddenMember = Convert.ToBoolean(dataReader["Hidden"]);
                        winery.EnableClubeCellar = Convert.ToBoolean(dataReader["EnableClubeCellar"]);
                        winery.EnableClubAMS = Convert.ToBoolean(dataReader["EnableClubAMS"]);
                        winery.EnableClubMicroworks = Convert.ToBoolean(dataReader["EnableClubMicroworks"]);
                        winery.EnableClubCoresense = Convert.ToBoolean(dataReader["EnableClubCoresense"]);
                        winery.CurrencySymbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        winery.ClubStatusField = Convert.ToString(dataReader["ClubStatusField"]);
                        winery.UpsertOrderDateAs = Convert.ToString(dataReader["UpsertOrderDateAs"]);
                        winery.UpsertShipDateAs = Convert.ToString(dataReader["UpsertShipDateAs"]);
                        winery.ThirdPartyTransaction = Convert.ToString(dataReader["ThirdPartyTransaction"]);
                        winery.WineryAddress = new Model.Address
                        {
                            address_1 = Convert.ToString(dataReader["Address1"]),
                            address_2 = Convert.ToString(dataReader["Address2"]),
                            city = Convert.ToString(dataReader["City"]),
                            state = Convert.ToString(dataReader["State"]),
                            zip_code = Convert.ToString(dataReader["Zip"]),
                            country = Convert.ToString(dataReader["Country"])
                        };
                        winery.EnableService = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableService"])) ? false : Convert.ToBoolean(dataReader["EnableService"]);
                        winery.WebSiteUrl = Convert.ToString(dataReader["url"]);
                        winery.EmailAddress = Convert.ToString(dataReader["EmailAddress"]);
                        winery.IsActive = Convert.ToBoolean(dataReader["Active"]);
                        winery.BillingFirstName = Convert.ToString(dataReader["BillingFirstName"]);
                        winery.BillingLastName = Convert.ToString(dataReader["BillingLastName"]);
                        winery.MemberProfileUrl = Convert.ToString(dataReader["PurchaseURL"]);
                        winery.AppellationName = Convert.ToString(dataReader["Appellation"]);
                        winery.BillingEmailAddress = Convert.ToString(dataReader["BillingEmail"]);
                        winery.EnableGoogleSync = Convert.ToBoolean(dataReader["EnableGoogleSync"]);
                        winery.GoogleUsername = Convert.ToString(dataReader["GoogleUsername"]);
                        winery.GooglePassword = Convert.ToString(dataReader["GooglePassword"]);
                        winery.TimezoneId = Convert.ToInt32(dataReader["timezoneId"]);
                        winery.SALT = Convert.ToString(dataReader["SALT"]);
                        winery.DecryptKey = Convert.ToString(dataReader["DecryptKey"]);
                        winery.EnableUpsertCreditCard = Convert.ToBoolean(dataReader["EnableUpsertCreditCard"]);
                        winery.UpsertFulfillmentCommerce7 = Convert.ToBoolean(dataReader["UpsertFulfillmentDate"]);
                        winery.EnableCustomOrderType = Convert.ToBoolean(dataReader["EnableCustomOrderType"]);
                        winery.CustomOrderType = Convert.ToString(dataReader["CustomOrderType"]);

                        winery.EnableBigCommerce = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableBigCommerce"])) ? false : Convert.ToBoolean(dataReader["EnableBigCommerce"]);
                        winery.EnableClubBigCommerce = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["EnableClubBigCommerce"])) ? false : Convert.ToBoolean(dataReader["EnableClubBigCommerce"]);
                        winery.BigCommerceStoreId = Convert.ToString(dataReader["BigCommerceStoreId"]);
                        winery.BigCommerceAceessToken = Convert.ToString(dataReader["BigCommerceAceessToken"]);

                    }
                }
            }
            return winery;
        }

        public WidgetModel GetWineryWidgetById(int wineryId)
        {
            var profile = new WidgetModel();

            string sql = "select DisplayName,PurchaseUrl,GoogleAnalyticsId,Url,CustomStyles,BusinessPhone,WebsiteUrl from winery (nolock) where id= @wineryId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", wineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        profile.display_name = Convert.ToString(dataReader["DisplayName"]);
                        profile.purchase_url = Convert.ToString(dataReader["PurchaseUrl"]);
                        profile.google_analytics_id = Convert.ToString(dataReader["GoogleAnalyticsId"]);
                        profile.url = Convert.ToString(dataReader["Url"]);
                        profile.custom_styles = Convert.ToString(dataReader["CustomStyles"]);
                        profile.business_phone = Convert.ToString(dataReader["BusinessPhone"]);
                        profile.website_url = Convert.ToString(dataReader["WebsiteUrl"]);

                        var styles = profile.custom_styles.Split('|').ToList();
                        profile.widget_cutom_styles = new WidgetCutomStyles();
                        for (var i = 0; i < styles.Count(); i++)
                        {
                            if (styles[i].Contains("WidgetBackgound:"))
                            {
                                profile.widget_cutom_styles.widget_backgound = styles[i].Replace("WidgetBackgound:", "");
                            }

                            if (styles[i].Contains("WidgetText:"))
                            {
                                profile.widget_cutom_styles.widget_text = styles[i].Replace("WidgetText:", "");
                            }
                            if (styles[i].Contains("WidgetFooterText:"))
                            {
                                profile.widget_cutom_styles.widget_footer_text = styles[i].Replace("WidgetFooterText:", "");
                            }
                            if (styles[i].Contains("WidgetSearchButton:"))
                            {
                                profile.widget_cutom_styles.widget_search_button = styles[i].Replace("WidgetSearchButton:", "");
                            }
                            if (styles[i].Contains("WidgetSearchButtonText:"))
                            {
                                profile.widget_cutom_styles.widget_search_button_text = styles[i].Replace("WidgetSearchButtonText:", "");
                            }
                            if (styles[i].Contains("WidgetTimeSlotButton:"))
                            {
                                profile.widget_cutom_styles.widget_time_slot_button = styles[i].Replace("WidgetTimeSlotButton:", "");
                            }
                            if (styles[i].Contains("WidgetTimeSlotButtonText:"))
                            {
                                profile.widget_cutom_styles.widget_time_slot_button_text = styles[i].Replace("WidgetTimeSlotButtonText:", "");
                            }
                            if (styles[i].Contains("WidgetHeaderBackground:"))
                            {
                                profile.widget_cutom_styles.widget_header_back_ground = styles[i].Replace("WidgetHeaderBackground:", "");
                            }
                            if (styles[i].Contains("WidgetHeaderText:"))
                            {
                                profile.widget_cutom_styles.widget_header_text = styles[i].Replace("WidgetHeaderText:", "");
                            }
                            if (styles[i].Contains("WidgetLinks:"))
                            {
                                profile.widget_cutom_styles.widget_links = styles[i].Replace("WidgetLinks:", "");
                            }
                            if (styles[i].Contains("WidgetEnabled:"))
                            {
                                profile.widget_cutom_styles.widget_enabled = Convert.ToBoolean(Convert.ToInt16(styles[i].Replace("WidgetEnabled:", "")));
                            }
                            if (styles[i].Contains("WidgetFont:"))
                            {
                                profile.widget_cutom_styles.widget_font = styles[i].Replace("WidgetFont:", "");
                            }
                            if (styles[i].Contains("WidgetFontSize:"))
                            {
                                profile.widget_cutom_styles.widget_font_size = styles[i].Replace("WidgetFontSize:", "");
                            }

                            if (styles[i].Contains("ClubBadgeText:"))
                            {
                                profile.widget_cutom_styles.club_badge_text = styles[i].Replace("ClubBadgeText:", "");
                            }

                            if (styles[i].Contains("ClubBadgeBackground:"))
                            {
                                profile.widget_cutom_styles.club_badge_background = styles[i].Replace("ClubBadgeBackground:", "");
                            }

                        }
                    }
                }
            }
            return profile;
        }

        public WineryModel GetWinery2ById(int wineryId)
        {
            var winery = new WineryModel();

            string sql = "select DisplayName,BusinessPhone,EmailAddress,Hidden,Country,timezoneId from Winery where Id= @wineryId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", wineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        winery.DisplayName = Convert.ToString(dataReader["DisplayName"]);
                        winery.BusinessPhone = string.IsNullOrWhiteSpace(Convert.ToString(dataReader["BusinessPhone"])) ? 0 : Convert.ToDecimal(dataReader["BusinessPhone"]);
                        winery.EmailAddress = Convert.ToString(dataReader["EmailAddress"]);
                        winery.HiddenMember = Convert.ToBoolean(dataReader["Hidden"]);
                        winery.TimezoneId = Convert.ToInt32(dataReader["timezoneId"]);
                        winery.WineryAddress = new Model.Address
                        {
                            country = Convert.ToString(dataReader["Country"])
                        };
                    }
                }
            }
            return winery;
        }

        public int GetTimeZonebyWineryId(int wineryId)
        {
            int timezoneId = 0;
            string sql = "select timezoneId from winery where Id= @wineryId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", wineryId));


            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        timezoneId = Convert.ToInt32(dataReader["timezoneId"]);
                    }
                }
            }

            return timezoneId;
        }

        public bool CheckPreAuthEnabledForWinery(int wineryId)
        {
            bool enablePreAuth = false;
            string sql = "select EnablePreAuthRSVP from winery where Id= @wineryId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", wineryId));


            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        enablePreAuth = Convert.ToBoolean(dataReader["EnablePreAuthRSVP"]);
                    }
                }
            }

            return enablePreAuth;
        }
        public int GetTimeZonebyEventId(int eventId)
        {
            int timezoneId = 5;
            string sql = "select top 1 TimeZoneId from events (nolock) e join winery (nolock) w on e.wineryid = w.id where Eventid=@Eventid";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Eventid", eventId));


            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        timezoneId = Convert.ToInt32(dataReader["TimeZoneId"]);
                    }
                }
            }

            return timezoneId;
        }

        public int GetTimeZonebyReservationId(int ReservationId)
        {
            int timezoneId = 5;
            string sql = "select top 1 TimeZoneId from Reservationv2 (nolock) r join winery (nolock) w on r.wineryid = w.id where ReservationId=@ReservationId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        timezoneId = Convert.ToInt32(dataReader["TimeZoneId"]);
                    }
                }
            }

            return timezoneId;
        }

        public List<SettingModel> GetSettingGroup(int wineryId, uc.Common.SettingGroup Group)
        {
            var settingModel = new List<SettingModel>();

            string sql = "select Id,Winery_Id,[Group],[Key],Value from Settings where Winery_Id = @wineryId and [Group]=@Group";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", wineryId));
            parameterList.Add(GetParameter("@Group", (int)Group));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new SettingModel();
                        model.Id = Convert.ToInt32(dataReader["Id"]);
                        model.MemberId = Convert.ToInt32(dataReader["Winery_Id"]);
                        model.Value = Convert.ToString(dataReader["Value"]);
                        model.Group = (uc.Common.SettingGroup)Convert.ToInt32(dataReader["Group"]);
                        model.Key = (uc.Common.SettingKey)Convert.ToInt32(dataReader["Key"]);

                        settingModel.Add(model);
                    }
                }
            }
            return settingModel;
        }

        public List<DiscountModel> GetActiveDiscountsByEventId(int EventId)
        {
            var discountModel = new List<DiscountModel>();

            string sql = "select d.Id,d.DiscountName,d.DiscountCode,d.DiscountType,d.DiscountAmount,d.DiscountPercent,d.StartDateTime,d.EndDateTime,d.RequiredMinimum,d.RequiredMaximum,d.DateType from Discounts d join Event_Discounts ed on d.Id=ed.Discounts_Id where Active=1 and StartDateTime <= GETDATE() And EndDateTime > GETDATE() and ed.Events_EventId =  @EventId order by d.StartDateTime desc";
            //string sql = "select d.Id,d.DiscountName,d.DiscountCode,d.DiscountType,d.DiscountAmount,d.DiscountPercent,d.StartDateTime,d.EndDateTime,d.RequiredMinimum,d.RequiredMaximum from Discounts d join Event_Discounts ed on d.Id=ed.Discounts_Id where Active=1 and StartDateTime <= GETDATE() And EndDateTime > GETDATE() and ed.Events_EventId =  @EventId order by d.StartDateTime desc";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new DiscountModel();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.discount_name = Convert.ToString(dataReader["DiscountName"]);
                        model.discount_code = Convert.ToString(dataReader["DiscountCode"]);
                        model.discount_amount = Convert.ToDecimal(dataReader["DiscountAmount"]);
                        model.discount_percent = Convert.ToDecimal(dataReader["DiscountPercent"]);
                        model.start_datetime = Convert.ToDateTime(dataReader["StartDateTime"]);
                        model.end_datetime = Convert.ToDateTime(dataReader["EndDateTime"]);
                        model.min_guest = Convert.ToInt32(dataReader["RequiredMinimum"]);
                        model.max_guest = Convert.ToInt32(dataReader["RequiredMaximum"]);
                        model.discount_type = (uc.Common.DiscountOption)Convert.ToInt32(dataReader["DiscountType"]);
                        model.date_type = (uc.Common.DateType)Convert.ToInt32(dataReader["DateType"]);
                        discountModel.Add(model);
                    }
                }
            }
            return discountModel;
        }

        public TicketsPaymentProcessor GetTicketPaymentProcessorByWinery(int memberId)
        {
            TicketsPaymentProcessor ticketProcessor = TicketsPaymentProcessor.CellarPassProcessor;
            string sql = "select Isnull(TicketsProcessor, 0) as TicketsProcessor from Winery (nolock) where Id=@memberId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@memberId", memberId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ticketProcessor = (TicketsPaymentProcessor)Convert.ToInt32(dataReader["TicketsProcessor"]);
                    }
                }
            }
            return ticketProcessor;
        }

        public bool isRsvpEventInAnyPassports(int EventId, DateTime rsvpEventDate)
        {
            bool ret = false;

            string sql = "select ppe.Id from Tickets_Event_PassportMembers_Events ppe join Tickets_Event_PassportMembers ppm on ppm.Id = ppe.Tickets_Event_PassportMembers_id Join Tickets_Event te on te.Id = ppm.Tickets_Event_Id And @rsvpEventDate >= te.StartDateTime And @rsvpEventDate <= te.EndDateTime where Event_Id = @EventId and te.Category = 1";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));
            parameterList.Add(GetParameter("@rsvpEventDate", rsvpEventDate));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        public bool ShowTransportation(int EventId)
        {
            bool ret = false;

            string sql = "select u.Id from [User] u  join Event_Transportation et on et.Transportation_Id = u.Id where Event_Id = @EventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        public List<RegionModel> GetRegions(string stateCode, string countryCode)
        {
            var regionModel = new List<RegionModel>();
           
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@stateCode", stateCode));
            parameterList.Add(GetParameter("@countryCode", countryCode));

            using (DbDataReader dataReader = GetDataReader("GetRegionsByStateCountry", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new RegionModel();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.friendly_name = Convert.ToString(dataReader["FriendlyName"]);
                        model.friendly_url = Convert.ToString(dataReader["FriendlyURL"]);
                        model.image_url = Convert.ToString(dataReader["ImageURL"]);

                        model.internal_name = Convert.ToString(dataReader["internalname"]);
                        model.state_name = Convert.ToString(dataReader["statename"]);
                        model.enable_1 = Convert.ToBoolean(dataReader["enable1"]);
                        model.enable_2 = Convert.ToBoolean(dataReader["enable2"]);
                        model.enable_3 = Convert.ToBoolean(dataReader["enable3"]);
                        model.enable_4 = Convert.ToBoolean(dataReader["enable4"]);
                        model.enable_5 = Convert.ToBoolean(dataReader["enable5"]);
                        model.enable_6 = Convert.ToBoolean(dataReader["enable6"]);

                        string content_json = Convert.ToString(dataReader["contentjson"]);

                        model.region_content_model = JsonConvert.DeserializeObject<RegionContentModel>(content_json);

                        if (model.region_content_model != null)
                        {
                            if (model.region_content_model.seasonitems != null && model.region_content_model.seasonitems.Count > 0)
                            {
                                model.region_content_model.seasonitems = model.region_content_model.seasonitems.OrderBy(x => x.sortorder).ToList();
                            }
                            if (model.region_content_model.travelitems != null && model.region_content_model.travelitems.Count > 0)
                            {
                                model.region_content_model.travelitems = model.region_content_model.travelitems.OrderBy(x => x.sortorder).ToList();
                            }
                            if (model.region_content_model.directoryitems != null && model.region_content_model.directoryitems.Count > 0)
                            {
                                model.region_content_model.directoryitems = model.region_content_model.directoryitems.OrderBy(x => x.sortorder).ToList();
                            }
                        }

                        regionModel.Add(model);
                    }
                }
            }
            return regionModel;
        }

        public List<BusinessProtipsTricksModel> GetBusinessProtipsTricks()
        {
            var businessProtipsTricksModel = new List<BusinessProtipsTricksModel>();

            using (DbDataReader dataReader = GetDataReader("GetBusinessProtipsTricks", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new BusinessProtipsTricksModel();

                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.title = Convert.ToString(dataReader["Title"]);
                        model.sub_title = Convert.ToString(dataReader["SubTitle"]);
                        model.content = Convert.ToString(dataReader["Content"]);
                        model.tags = Convert.ToString(dataReader["Tags"]);
                        model.article_date = Convert.ToDateTime(dataReader["ArticleDate"]);
                        model.view_count = Convert.ToInt32(dataReader["ViewCount"]);
                        model.custom_url = Convert.ToString(dataReader["CustomURL"]);
                        model.date_modified = Convert.ToDateTime(dataReader["DateModified"]);

                        businessProtipsTricksModel.Add(model);
                    }
                }
            }
            return businessProtipsTricksModel;
        }

        public HDYHModel GetHDYH_OptionsByWineryId(int WineryId)
        {
            var model = new HDYHModel();

            string sql = "select [Answer_1],[Answer_2],[Answer_3],[Answer_4],[Answer_5],[Answer_6],[Answer_7],[Answer_8],[Answer_9],[Answer_10] from [HDYH_Options] where [Member_Id] =  @WineryId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.answer_1 = Convert.ToString(dataReader["Answer_1"]);
                        model.answer_2 = Convert.ToString(dataReader["Answer_2"]);
                        model.answer_3 = Convert.ToString(dataReader["Answer_3"]);
                        model.answer_4 = Convert.ToString(dataReader["Answer_4"]);
                        model.answer_5 = Convert.ToString(dataReader["Answer_5"]);
                        model.answer_6 = Convert.ToString(dataReader["Answer_6"]);
                        model.answer_7 = Convert.ToString(dataReader["Answer_7"]);
                        model.answer_8 = Convert.ToString(dataReader["Answer_8"]);
                        model.answer_9 = Convert.ToString(dataReader["Answer_9"]);
                        model.answer_10 = Convert.ToString(dataReader["Answer_10"]);
                    }
                }
            }
            return model;
        }

        public List<TagModel> GetMemberTags(int WineryId)
        {
            var list = new List<TagModel>();

            string sql = "select ID,Tag,TagType from Tags where Member_Id = @WineryId order by Tag";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new TagModel();

                        model.id = Convert.ToInt32(dataReader["ID"]);
                        model.member_id = WineryId;
                        model.tag = Convert.ToString(dataReader["Tag"]);
                        model.tag_type = (TagType)Convert.ToInt32(dataReader["TagType"]);

                        if (model.tag_type == TagType.special_event)
                            model.tag_type_desc = "Special Event";
                        if (model.tag_type == TagType.special_guests)
                            model.tag_type_desc = "Special Guests";

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<GuestTagModel> GetGuestTags()
        {
            var list = new List<GuestTagModel>();

            string sql = "select Id,Tag from Guest_Tags order by SortOrder";

            using (DbDataReader dataReader = GetDataReader(sql, null, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new GuestTagModel();

                        model.id = Convert.ToInt32(dataReader["ID"]);
                        model.tag = Convert.ToString(dataReader["Tag"]);
                        
                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<WineryTypesModel> GetWineryTypes()
        {
            var list = new List<WineryTypesModel>();

            string sql = "select Id,InternalName,FriendlyName,FriendlyUrl,Active from WineryTypes where active=1 order by FriendlyName";

            using (DbDataReader dataReader = GetDataReader(sql, null, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new WineryTypesModel();

                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.internal_name = Convert.ToString(dataReader["InternalName"]);
                        model.friendly_name = Convert.ToString(dataReader["FriendlyName"]);
                        model.friendly_url = Convert.ToString(dataReader["FriendlyUrl"]);
                        model.active = Convert.ToBoolean(dataReader["Active"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<AffiliateModel> GetAffiliatesByWineryId(int WineryId)
        {
            var affiliateModel = new List<AffiliateModel>();

            string sql = "select u.Id,ISNULL(u.FirstName,'') as FirstName,u.LastName,ISNULL(u.CompanyName,'') as CompanyName from [User] u join UserWinery uw on u.Id = uw.UserId where uw.RoleId=6 and uw.WineryId = " + WineryId;

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new AffiliateModel();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.company_name = Convert.ToString(dataReader["CompanyName"]);
                        affiliateModel.Add(model);
                    }
                }
            }
            return affiliateModel;
        }

        public PaymentConfigModel GetPaymentConfigByWineryId(int WineryId,int SettingType = 1)
        {
            var paymentConfigModel = new PaymentConfigModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryId));
            parameterList.Add(GetParameter("@SettingType", SettingType));

            using (DbDataReader dataReader = GetDataReader("GetPaymentConfigByWineryId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        paymentConfigModel.MerchantLogin = Convert.ToString(dataReader["UserName"]);
                        paymentConfigModel.MerchantPassword = Convert.ToString(dataReader["Password"]);
                        paymentConfigModel.UserConfig1 = Convert.ToString(dataReader["UserConfig1"]);
                        paymentConfigModel.UserConfig2 = Convert.ToString(dataReader["UserConfig2"]);
                        paymentConfigModel.PaymentGateway = (Common.Payments.Configuration.Gateway)Convert.ToInt32(dataReader["PaymentGateway"]);
                        paymentConfigModel.GatewayMode = (Common.Payments.Configuration.Mode)Convert.ToInt32(dataReader["Mode"]);
                    }
                }
            }
            return paymentConfigModel;
        }

        public RegionDetailModel GetPOICategoriesByRegionId(int RegionId)
        {
            var poiCategoriesModel = new List<POICategoriesModel>();
            var regionDetailModel = new RegionDetailModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@RegionId", RegionId));

            using (DbDataReader dataReader = GetDataReader("GetPOICategoriesByRegionId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        regionDetailModel.id = RegionId;
                        regionDetailModel.friendly_name = Convert.ToString(dataReader["FriendlyName"]);
                        regionDetailModel.enable_getting_here = Convert.ToBoolean(dataReader["EnableGettingHere"]);
                        regionDetailModel.enable_getaway_ideas = Convert.ToBoolean(dataReader["EnableGetawayIdeas"]);
                        regionDetailModel.enable_where_to_drink = Convert.ToBoolean(dataReader["EnableWhereToDrink"]);
                        regionDetailModel.enable_where_to_eat = Convert.ToBoolean(dataReader["EnableWhereToEat"]);
                        regionDetailModel.enable_where_to_stay = Convert.ToBoolean(dataReader["EnableWhereToStay"]);
                        regionDetailModel.enable_things_to_do = Convert.ToBoolean(dataReader["EnableThingsToDo"]);
                        regionDetailModel.enable_on_homepage = Convert.ToBoolean(dataReader["EnableOnHomepage"]);
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var model = new POICategoriesModel();

                            model.attribute_id = Convert.ToInt32(dataReader["AttributeId"]);
                            model.attribute_name = Convert.ToString(dataReader["AttributeName"]);
                            model.attribute_type_id = Convert.ToInt32(dataReader["AttributeTypeId"]);
                            model.active = Convert.ToBoolean(dataReader["Active"]);

                            poiCategoriesModel.Add(model);
                        }
                    }

                    regionDetailModel.poicategories = poiCategoriesModel;
                }
            }
            return regionDetailModel;
        }

        public List<AddOn_Group> GetEventAddOnGroupByEventId(int EventId)
        {
            var addOn_GroupList = new List<AddOn_Group>();
            var eventAddOnModel = new List<EventAddOnModel>();

            eventAddOnModel = GetEventAddOnsByEventId(EventId);

            var reservation_addon = new List<Reservation_Addon>();

            if (eventAddOnModel != null)
            {
                foreach (var item in eventAddOnModel)
                {
                    var addOn_Group = new AddOn_Group();
                    addOn_Group = GetAddOnGroupItemsByGroupId(item.GroupId, reservation_addon);
                    if (addOn_Group.id > 0)
                    {
                        addOn_GroupList.Add(addOn_Group);
                    }
                }
            }
            return addOn_GroupList;
        }
        public List<EventAddOnModel> GetEventAddOnsByEventId(int EventId)
        {
            var eventAddOnModel = new List<EventAddOnModel>();

            string sql = "select Id,Event_Id,Event_Type,AddOn_Group_Id from AddOn_Event where Event_Type=1 and Event_Id = @EventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new EventAddOnModel();
                        model.Id = Convert.ToInt32(dataReader["Id"]);
                        model.EventId = Convert.ToInt32(dataReader["Event_Id"]);
                        //  model.EventType = (uc.AddOns)(dataReader["Event_Type"]);
                        model.GroupId = Convert.ToInt32(dataReader["AddOn_Group_Id"]);

                        eventAddOnModel.Add(model);
                    }
                }
            }
            return eventAddOnModel;
        }

        public List<AddOn_Group> GetAddOnGroupByWineryId(int MemberId)
        {
            var addOn_Group = new List<AddOn_Group>();

            string sql = "select Id,Name from AddOn_Group where winery_id = @MemberId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", MemberId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new AddOn_Group();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.name = Convert.ToString(dataReader["Name"]);

                        addOn_Group.Add(model);
                    }
                }
            }
            return addOn_Group;
        }

        public List<AddOn_Group> GetAddOnGroupByReservationId(int ReservationId)
        {
            var addOn_Group = new List<AddOn_Group>();

            string sql = "select distinct groupid from ReservationV2_AddOn (nolock) where reservation_id = @ReservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new AddOn_Group();
                        model.id = Convert.ToInt32(dataReader["groupid"]);

                        addOn_Group.Add(model);
                    }
                }
            }
            return addOn_Group;
        }

        public AddOn_Group GetAddOnGroupItemsByGroupId(int GroupId, List<Reservation_Addon> reservation_addon)
        {
            var addOn_Group_Items = new List<AddOn_Group_Items>();
            var addOn_Group = new AddOn_Group();
            string sql = "select g.Id as GroupId,g.Winery_Id as WineryId,g.Name,g.GroupType,gi.[Id],gi.[AddOn_Group_Id],gi.[AddOn_Item_Id],gi.[Price],gi.[SortOrder],gi.[IsDefault],";
            sql = sql + "i.[Id] as ItemId,i.[Active],i.[Sku],i.[Name] as AddOnItemName,i.[Description],i.[Price] as ItemPrice,i.[Winery_Id],i.[Category],i.[ItemType],i.[Image],i.[Cost],i.[Taxable], i.CalculateGratuity, gi.MinQty, gi.MaxQty,DepartmentId ";
            sql = sql + "from AddOn_Group g join AddOn_Group_Items gi on g.Id = gi.AddOn_Group_Id join AddOn_Item i on gi.AddOn_Item_Id = i.Id where gi.AddOn_Group_Id = @GroupId order by gi.SortOrder";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@GroupId", GroupId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        addOn_Group.id = Convert.ToInt32(dataReader["GroupId"]);
                        addOn_Group.name = Convert.ToString(dataReader["Name"]);
                        addOn_Group.group_type = Convert.ToInt32(dataReader["GroupType"]);
                        addOn_Group.member_id = Convert.ToInt32(dataReader["WineryId"]);

                        var model = new AddOn_Group_Items();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.isdefault = Convert.ToBoolean(dataReader["IsDefault"]);
                        model.price = Convert.ToDecimal(dataReader["Price"]);
                        model.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                        model.addOn_item_id = Convert.ToInt32(dataReader["AddOn_Item_Id"]);
                        model.addon_group_id = Convert.ToInt32(dataReader["AddOn_Group_Id"]);
                        model.max_qty = Convert.ToInt32(dataReader["MaxQty"]);
                        model.min_qty = Convert.ToInt32(dataReader["MinQty"]);

                        var addOn_Item = new AddOn_Item();
                        addOn_Item.active = Convert.ToBoolean(dataReader["Active"]);
                        addOn_Item.category = Convert.ToInt32(dataReader["Category"]);
                        addOn_Item.cost = Convert.ToDecimal(dataReader["Cost"]);
                        addOn_Item.description = Convert.ToString(dataReader["Description"]);
                        addOn_Item.id = Convert.ToInt32(dataReader["ItemId"]);
                        addOn_Item.image = Convert.ToString(dataReader["Image"]);
                        //addOn_Item.item_type = addOn_Group.group_type;
                        addOn_Item.name = Convert.ToString(dataReader["AddOnItemName"]);
                        addOn_Item.retail_price = Convert.ToDecimal(dataReader["ItemPrice"]);
                        //addOn_Item.name = Convert.ToString(dataReader["Name"]);
                        //addOn_Item.price = Convert.ToDecimal(dataReader["ItemPrice"]);
                        addOn_Item.sku = Convert.ToString(dataReader["Sku"]);
                        addOn_Item.taxable = Convert.ToBoolean(dataReader["Taxable"]);
                        addOn_Item.calculate_gratuity = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                        addOn_Item.department_id = Convert.ToInt32(dataReader["DepartmentId"]);
                        //addOn_Item.member_id = Convert.ToInt32(dataReader["Winery_Id"]);

                        //If the AddOn_Item item type is > 0 then we need to set it to that type, else we set it to the group type. We use this for the null item functionality
                        int addOnItemType = Convert.ToInt32(dataReader["ItemType"]);
                        if (addOnItemType > 0)
                        {
                            addOn_Item.item_type = addOnItemType;
                        }
                        else
                        {
                            addOn_Item.item_type = addOn_Group.group_type;
                        }

                        if (reservation_addon.Count > 0)
                        {
                            addOn_Item.qty = reservation_addon.Where(a => a.item_id == addOn_Item.id).Select(a => a.qty).FirstOrDefault();
                        }
                        else
                        {
                            addOn_Item.qty = -1;
                        }

                        model.addon_item = addOn_Item;
                        addOn_Group_Items.Add(model);
                    }
                    if (addOn_Group != null)
                    {
                        addOn_Group.addOn_group_items = addOn_Group_Items;
                    }
                }
            }
            return addOn_Group;
        }

        public List<AddOn_Item_Ext> GetAddonItemsByMemberId(int memberId)
        {
            var addOn_Items = new List<AddOn_Item_Ext>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", memberId));

            using (DbDataReader dataReader = GetDataReader("GetAddonItemsByMember", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var addOn_Item = new AddOn_Item_Ext();
                        addOn_Item.active = Convert.ToBoolean(dataReader["Active"]);
                        addOn_Item.category = Convert.ToInt32(dataReader["Category"]);
                        addOn_Item.category_desc = Convert.ToString(dataReader["CategoryDesc"]);
                        addOn_Item.cost = Convert.ToDecimal(dataReader["Cost"]);
                        addOn_Item.description = Convert.ToString(dataReader["Description"]);
                        addOn_Item.id = Convert.ToInt32(dataReader["ItemId"]);
                        addOn_Item.image = Convert.ToString(dataReader["ItemImage"]);
                        addOn_Item.item_type = Convert.ToInt32(dataReader["ItemType"]);
                        addOn_Item.item_type_desc = Convert.ToString(dataReader["ItemTypeDesc"]);
                        addOn_Item.name = Convert.ToString(dataReader["Name"]);
                        addOn_Item.retail_price = Convert.ToDecimal(dataReader["ItemPrice"]);
                        addOn_Item.sku = Convert.ToString(dataReader["Sku"]);
                        addOn_Item.taxable = Convert.ToBoolean(dataReader["Taxable"]);
                        addOn_Item.calculate_gratuity = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                        addOn_Item.department_id = Convert.ToInt32(dataReader["DepartmentId"]);
                        addOn_Item.department_desc = Convert.ToString(dataReader["DepartmentDesc"]);
                        addOn_Items.Add(addOn_Item);
                    }
                }
            }
            return addOn_Items;
        }

        public List<SFTPProduct> GetGoogleSFTPEvent()
        {
            var products = new List<SFTPProduct>();

            using (DbDataReader dataReader = GetDataReader("GetGoogleSFTPEvent", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var item = new SFTPProduct();
                        item.id = Convert.ToString(dataReader["eventid"]);
                        var title = new Title();
                        var localized_texts = new List<LocalizedText>();
                        var localized_text = new LocalizedText();

                        localized_text.text= Convert.ToString(dataReader["EventName"]);
                        localized_text.language_code = "en";

                        localized_texts.Add(localized_text);

                        title.localized_texts = localized_texts;

                        item.title = title;

                        var description = new Description();
                        var description_texts = new List<LocalizedText>();
                        var description_text = new LocalizedText();

                        var desclocalized_texts = new List<LocalizedText>();
                        var desclocalized_text = new LocalizedText();

                        description_text.language_code= "en";
                        description_text.text= Convert.ToString(dataReader["Description"]);

                        description_texts.Add(description_text);

                        description.localized_texts = description_texts;

                        item.description = description;

                        var rating = new Rating();

                        MemberStatsModel memberStatsModel = GetMemberstats(Convert.ToInt32(dataReader["WineryId"]));

                        rating.average_value = Convert.ToDouble(memberStatsModel.avg_review_value);
                        rating.rating_count = memberStatsModel.total_reviews;

                        item.rating = rating;

                        item.inventory_type = "INVENTORY_TYPE_DEFAULT";
                        item.confirmation_type = "CONFIRMATION_TYPE_INSTANT";

                        var fulfillmentType = new FulfillmentType();

                        fulfillmentType.mobile = true;
                        fulfillmentType.pickup = false;
                        fulfillmentType.print_at_home = false;

                        item.fulfillment_type = fulfillmentType;

                        var @operator= new Operator();

                        var googleBusinessProfileName = new GoogleBusinessProfileName();

                        var googleBusinesslocalized_text = new LocalizedText();

                        googleBusinesslocalized_text.language_code = "en";
                        googleBusinesslocalized_text.text = Convert.ToString(dataReader["googlebusinessprofile"]);

                        desclocalized_texts.Add(googleBusinesslocalized_text);

                        googleBusinessProfileName.localized_texts = desclocalized_texts;

                        @operator.google_business_profile_name = googleBusinessProfileName;

                        item.@operator = @operator;

                        var locations = new List<Location3>();
                        var location = new Location3();
                        var placeInfo = new PlaceInfo();
                        var coordinates = new Coordinates();

                        coordinates.latitude = Convert.ToDouble(dataReader["GeoLatitude"]);
                        coordinates.longitude = Convert.ToDouble(dataReader["GeoLongitude"]);

                        placeInfo.coordinates = coordinates;

                        location.place_id = Convert.ToString(dataReader["GooglePlaceID"]);
                        placeInfo.name= Convert.ToString(dataReader["LocationName"]);
                        placeInfo.website_url = "";
                        placeInfo.phone_number = "";
                        placeInfo.unstructured_address= Convert.ToString(dataReader["LocationAddress"]);
                        location.place_info = placeInfo;

                        locations.Add(location);

                        item.locations = locations;

                        var related_locations = new List<RelatedLocation>();
                        var related_location = new RelatedLocation();
                        var SFTPLocation = new SFTPLocation();

                        related_location.relation_type = "RELATION_TYPE_RELATED_NO_ADMISSION";
                        SFTPLocation.place_id= Convert.ToString(dataReader["GooglePlaceID"]);

                        related_location.location = SFTPLocation;
                        related_locations.Add(related_location);

                        item.related_locations = related_locations;

                        item.@operator= @operator;

                        var options = new List<Option>();
                        var option = new Option();

                        var optiontexts = new List<LocalizedText>();
                        var optiontitle = new Title();

                        var optiondescription = new Description();
                        var optiondescriptiontexts = new List<LocalizedText>();

                        var eventAddOnModel = new List<EventAddOnModel>();

                        eventAddOnModel = GetEventAddOnsByEventId(Convert.ToInt32(item.id));

                        string optionid = string.Empty;

                        if (eventAddOnModel != null && eventAddOnModel.Count>0)
                        {
                            var reservation_addon = new List<Model.Reservation_Addon>();
                            foreach (var item1 in eventAddOnModel)
                            {
                                var addOn_Group = new AddOn_Group();

                                addOn_Group = GetAddOnGroupItemsByGroupId(item1.GroupId, reservation_addon);
                                if (addOn_Group.id > 0)
                                {
                                    foreach (var item11 in addOn_Group.addOn_group_items)
                                    {
                                        var optiontext = new LocalizedText();
                                        var optiondescriptionLocalizedText = new LocalizedText();

                                        optiontext.text = item11.addon_item.name;
                                        optiontext.language_code = "en";

                                        optiontexts.Add(optiontext);

                                        optiondescriptionLocalizedText.text = item11.addon_item.description;
                                        optiondescriptionLocalizedText.language_code = "en";

                                        optiondescriptiontexts.Add(optiondescriptionLocalizedText);
                                    }
                                }
                                //else
                                //{
                                //    var optiontext = new LocalizedText();
                                //    var optiondescriptionLocalizedText = new LocalizedText();

                                //    optiontext.text = "";
                                //    optiontext.language_code = "en";

                                //    optiontexts.Add(optiontext);

                                //    optiondescriptionLocalizedText.text = "";
                                //    optiondescriptionLocalizedText.language_code = "en";

                                //    optiondescriptiontexts.Add(optiondescriptionLocalizedText);
                                //}
                            }
                        }
                        else
                        {
                            var optiontext = new LocalizedText();
                            var optiondescriptionLocalizedText = new LocalizedText();

                            optiontext.text = "";
                            optiontext.language_code = "en";

                            optiontexts.Add(optiontext);

                            optiondescriptionLocalizedText.text = "";
                            optiondescriptionLocalizedText.language_code = "en";

                            optiondescriptiontexts.Add(optiondescriptionLocalizedText);
                        }

                        option.id = "";

                        optiondescription.localized_texts = optiondescriptiontexts;

                        optiontitle.localized_texts = optiontexts;
                        option.description = optiondescription;
                        option.title = optiontitle;

                        var landingPageListView =new LandingPageListView();
                        landingPageListView.url= Convert.ToString(dataReader["LandingViewPageURL"]);
                        option.landing_page_list_view = landingPageListView;

                        
                        option.duration_sec = Convert.ToInt32(dataReader["DurationMinutes"]) * 60;

                        var landingPage = new LandingPage();

                        landingPage.url= Convert.ToString(dataReader["LandingPageURL"]);
                        option.landing_page = landingPage;

                        var priceOptions= new List<PriceOption>();
                        var priceOption = new PriceOption();

                        priceOption.id = "";
                        priceOption.title = "";

                        var price = new Price();

                        price.currency_code = "us";
                        price.units = Convert.ToInt32(dataReader["FeePerPerson"]);

                        priceOption.price = price;

                        var feesAndTaxes= new FeesAndTaxes();

                        var perTicketTax = new PerTicketTax();

                        perTicketTax.currency_code = "us";
                        perTicketTax.units = 0;
                        feesAndTaxes.per_ticket_tax = perTicketTax;

                        priceOption.fees_and_taxes = feesAndTaxes;
                        
                        priceOptions.Add(priceOption);

                        option.price_options = priceOptions;

                        var cancellationPolicy= new CancellationPolicy();
                        var refundConditions=new List<RefundCondition>();
                        var refundCondition = new RefundCondition();

                        refundCondition.min_duration_before_start_time_sec = Convert.ToInt32(dataReader["CancelLeadTime"]) * 60;
                        refundCondition.refund_percent = 100;

                        refundConditions.Add(refundCondition);

                        cancellationPolicy.refund_conditions = refundConditions;
                        option.cancellation_policy = cancellationPolicy;
                        options.Add(option);
                        item.options = options;
                        products.Add(item);
                    }
                }
            }
            return products;
        }


        public List<RegionDetail2Model> GetActiveHomePageRegions(bool isEventsPageOnly)
        {
            var list = new List<RegionDetail2Model>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@isEventsPageOnly", isEventsPageOnly));

            using (DbDataReader dataReader = GetDataReader("GetActiveHomePageRegions", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var Item = new RegionDetail2Model();
                        Item.id = Convert.ToInt32(dataReader["id"]);
                        Item.internal_name = Convert.ToString(dataReader["internalname"]);
                        Item.friendly_name = Convert.ToString(dataReader["friendlyname"]);
                        Item.friendly_url = Convert.ToString(dataReader["friendlyurl"]);
                        Item.state = Convert.ToString(dataReader["state"]);
                        Item.description = Convert.ToString(dataReader["description"]);
                        Item.image_url = Convert.ToString(dataReader["imageurl"]);
                        Item.state_name = Convert.ToString(dataReader["statename"]);
                        Item.country = Convert.ToString(dataReader["country"]);
                        Item.country_name = Convert.ToString(dataReader["countryname"]);
                        Item.geo_latitude = Convert.ToString(dataReader["geolatitude"]);
                        Item.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                        Item.go_to_landing = Convert.ToBoolean(dataReader["gotolanding"]);
                        Item.events_page_intro = Convert.ToString(dataReader["eventspageintro"]);
                        Item.events_page_desc = Convert.ToString(dataReader["eventspagedesc"]);
                        Item.events_page_banner_image = Convert.ToString(dataReader["eventspagebannerimage"]);
                        list.Add(Item);
                    }
                }
            }
            return list;
        }

        public PageModel GetPageByURL(string url)
        {
            var Item = new PageModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@url", url));
            var images = new List<string>();

            using (DbDataReader dataReader = GetDataReader("GetPageByURL", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Item.id = Convert.ToInt32(dataReader["id"]);
                        Item.friendly_name = Convert.ToString(dataReader["friendlyname"]);
                        Item.friendly_url = Convert.ToString(dataReader["friendlyurl"]);
                        Item.description = Convert.ToString(dataReader["description"]);
                        Item.geo_latitude = Convert.ToString(dataReader["geolatitude"]);
                        Item.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);

                        string content_json = Convert.ToString(dataReader["contentjson"]);

                        Item.region_content_model = JsonConvert.DeserializeObject<RegionContentModel>(content_json);

                        if (Item.region_content_model != null)
                        {
                            if (Item.region_content_model.seasonitems != null && Item.region_content_model.seasonitems.Count > 0)
                            {
                                Item.region_content_model.seasonitems = Item.region_content_model.seasonitems.OrderBy(x => x.sortorder).ToList();
                            }
                            if (Item.region_content_model.travelitems != null && Item.region_content_model.travelitems.Count > 0)
                            {
                                Item.region_content_model.travelitems = Item.region_content_model.travelitems.OrderBy(x => x.sortorder).ToList();
                            }
                            if (Item.region_content_model.directoryitems != null && Item.region_content_model.directoryitems.Count > 0)
                            {
                                Item.region_content_model.directoryitems = Item.region_content_model.directoryitems.OrderBy(x => x.sortorder).ToList();
                            }
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            images.Add(Convert.ToString(dataReader["ImagePath"]));
                        }
                    }

                    Item.image_url = images;

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            Item.meta_title = Convert.ToString(dataReader["title"]);
                            Item.meta_description = Convert.ToString(dataReader["metaDescription"]);
                            Item.meta_keywords = Convert.ToString(dataReader["metaKeywords"]);
                        }
                    }
                }
            }

            return Item;
        }

        public Region2Model GetRegionByURL(string url,int Id= 0)
        {
            var Item = new Region2Model();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@url", url));
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader("GetRegion", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Item.id = Convert.ToInt32(dataReader["id"]);
                        Item.internal_name = Convert.ToString(dataReader["internalname"]);
                        Item.friendly_name = Convert.ToString(dataReader["friendlyname"]);
                        Item.friendly_url = Convert.ToString(dataReader["friendlyurl"]);
                        Item.state = Convert.ToString(dataReader["state"]);
                        Item.description = Convert.ToString(dataReader["description"]);
                        Item.image_url = Convert.ToString(dataReader["imageurl"]);
                        Item.state_name = Convert.ToString(dataReader["statename"]);
                        Item.country = Convert.ToString(dataReader["country"]);
                        Item.country_name = Convert.ToString(dataReader["countryname"]);
                        Item.geo_latitude = Convert.ToString(dataReader["geolatitude"]);
                        Item.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                        Item.go_to_landing = Convert.ToBoolean(dataReader["gotolanding"]);
                        Item.events_page_intro = Convert.ToString(dataReader["eventspageintro"]);
                        Item.events_page_desc = Convert.ToString(dataReader["eventspagedesc"]);
                        Item.events_page_banner_image = Convert.ToString(dataReader["eventspagebannerimage"]);
                        Item.winery_type_id = Convert.ToInt32(dataReader["winerytypeid"]);
                        Item.enable_1 =Convert.ToBoolean(dataReader["enable1"]);
                        Item.enable_2 = Convert.ToBoolean(dataReader["enable2"]);
                        Item.enable_3 = Convert.ToBoolean(dataReader["enable3"]);
                        Item.enable_4 = Convert.ToBoolean(dataReader["enable4"]);
                        Item.enable_5 = Convert.ToBoolean(dataReader["enable5"]);
                        Item.enable_6 = Convert.ToBoolean(dataReader["enable6"]);
                        Item.content_json = Convert.ToString(dataReader["contentjson"]);
                        Item.enable_events_page = Convert.ToBoolean(dataReader["enableeventspage"]);
                    }
                }
            }

            if (Item != null && Item.id > 0)
            {
                Item.region_content_model = JsonConvert.DeserializeObject<RegionContentModel>(Item.content_json);

                if (Item.region_content_model != null)
                {
                    if (Item.region_content_model.seasonitems != null && Item.region_content_model.seasonitems.Count > 0)
                    {
                        Item.region_content_model.seasonitems = Item.region_content_model.seasonitems.OrderBy(x => x.sortorder).ToList();
                    }
                    if (Item.region_content_model.travelitems != null && Item.region_content_model.travelitems.Count > 0)
                    {
                        Item.region_content_model.travelitems = Item.region_content_model.travelitems.OrderBy(x => x.sortorder).ToList();
                    }
                    if (Item.region_content_model.directoryitems != null && Item.region_content_model.directoryitems.Count > 0)
                    {
                        Item.region_content_model.directoryitems = Item.region_content_model.directoryitems.OrderBy(x => x.sortorder).ToList();
                    }
                }
            }

            return Item;
        }

        public List<SubRegionModel> GetSubRegionByRegionId(int regionId)
        {
            var list = new List<SubRegionModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@regionId", regionId));

            using (DbDataReader dataReader = GetDataReader("GetSubRegionForHome", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var Item = new SubRegionModel();
                        Item.id = Convert.ToInt32(dataReader["Id"]);
                        Item.region_name = Convert.ToString(dataReader["RegionName"]);
                        Item.region_url = Convert.ToString(dataReader["RegionURL"]);
                        Item.sub_region_id = Convert.ToInt32(dataReader["SubRegionId"]);
                        Item.sub_region_image_name = Convert.ToString(dataReader["SubRegionImageName"]);
                        Item.sub_region_name = Convert.ToString(dataReader["SubRegionName"]);
                        Item.sub_region_url = Convert.ToString(dataReader["SubRegionURL"]);

                        list.Add(Item);
                    }
                }
            }

            return list;
        }

        public DateTime ConvertFromDateTimeOffset(DateTimeOffset dateTime)
        {
            if (dateTime.Offset.Equals(TimeSpan.Zero))
            {
                return dateTime.UtcDateTime;
            }
            else if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
            {
                return DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local);
            }
            else
            {
                return dateTime.DateTime;
            }
        }
        public List<ReservationEvent> GetReservationsByFilters(string WhereClause)
        {
            List<ReservationEvent> listRsvpEvents = new List<ReservationEvent>();
            var lstReservationModel = new List<ReservationModel>();
            //List<ReservationEventSchedule> listReservationEventSchedule = new List<ReservationEventSchedule>();
            ReservationEvent rEvent = new ReservationEvent();
            ReservationEventSchedule schedule = new ReservationEventSchedule();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@whereClause", WhereClause));

            string sql = "GetReservationsByFilter";

            int OldEventId = -1;
            int OldSlotId = -1;

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        int EventId = dataReader["EventId"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EventId"]);

                        //check if new event or san existing one
                        if (OldEventId == -1 || OldEventId != EventId || EventId == 0)
                        {
                            rEvent = new ReservationEvent();
                            rEvent.event_times = new List<ReservationEventSchedule>();
                            OldSlotId = -1;

                            OldEventId = EventId;

                            rEvent.event_id = EventId;

                            if (OldEventId == 0)
                            {
                                rEvent.event_date = Convert.ToDateTime(dataReader["EventDate"]);
                                if (TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])) > TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])))
                                    rEvent.event_date_end = Convert.ToDateTime(dataReader["EventDate"]).AddDays(1);
                                else
                                    rEvent.event_date_end = Convert.ToDateTime(dataReader["EventDate"]);
                            }
                            else
                            {
                                EventModel eventModel = GetEventById(EventId);
                                rEvent.event_date = eventModel.StartDate;
                                rEvent.event_date_end = eventModel.EndDate;
                            }

                            rEvent.event_name = Convert.ToString(dataReader["EventName"]);
                            rEvent.event_technical_location = Convert.ToString(dataReader["technicalname"]);
                            rEvent.table_status_group_id = Convert.ToInt32(dataReader["Table_Status_Group_Id"]);
                            rEvent.member_id = Convert.ToInt32(dataReader["WineryId"]);
                            listRsvpEvents.Add(rEvent); //added event to the response
                        }

                        //check if new slot object needs to be added to the event above
                        int SlotId = dataReader["SlotId"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SlotId"]);

                        if (OldSlotId == -1 || OldSlotId != SlotId)
                        {
                            schedule = new ReservationEventSchedule();
                            schedule.reservations = new List<ReservationModel>();

                            OldSlotId = SlotId;

                            schedule.slot_id = SlotId;
                            if (SlotId > 0)
                            {
                                schedule.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                            }
                            else
                            {
                                schedule.slot_type = 2;
                            }

                            schedule.time = Convert.ToString(dataReader["SlotTime"]);
                            schedule.event_duration_minutes = Convert.ToInt32(dataReader["event_duration_minutes"]);
                            rEvent.event_times.Add(schedule);
                        }

                        var model = new ReservationModel();
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        model.event_location = Convert.ToString(dataReader["EventLocation"]);

                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        if (TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])) > TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])))
                            model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).AddDays(1).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));
                        else
                            model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));

                        model.location_id = Convert.ToInt32(dataReader["LocationId"]);

                        model.fee_per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.total_guests = Convert.ToInt16(dataReader["TotalGuests"]);
                        model.booking_date = ConvertFromDateTimeOffset(DateTimeOffset.Parse(Convert.ToString(dataReader["BookingDate"])));
                        model.guest_note = Convert.ToString(dataReader["Note"]);
                        model.referral_type = Convert.ToByte(dataReader["ReferralType"]);
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.booking_guid = Convert.ToString(dataReader["BookingGUID"]);
                        model.status = Convert.ToByte(dataReader["Status"]);
                        model.concierge_note = Convert.ToString(dataReader["ConciergeNote"]);
                        model.fee_due = Convert.ToDecimal(dataReader["FeeDue"]);
                        model.amount_paid = Convert.ToDecimal(dataReader["AmountPaid"]);

                        model.full_paid = (model.amount_paid >= model.fee_due);

                        model.internal_note = Convert.ToString(dataReader["InternalNote"]);
                        model.country = Convert.ToString(dataReader["Country"]);
                        model.concierge_name = Convert.ToString(dataReader["ConciergeName"]);
                        model.referral_name = Convert.ToString(dataReader["ReferralName"]);
                        model.reservation_seated_status = Convert.ToInt32(dataReader["SeatedStatus"]);
                        model.convenience_fee = Convert.ToDecimal(dataReader["TransactionFee"]);
                        model.gratuity_amount = Convert.ToDecimal(dataReader["GratuityAmount"]);
                        model.reservation_seated_status = Convert.ToInt32(dataReader["SeatedStatus"]);
                        string PreAssign_Table_Id = Convert.ToString(dataReader["PreAssign_Table_Id"]);
                        if (PreAssign_Table_Id.Length > 0)
                        {
                            model.pre_assign_table_ids = JsonConvert.DeserializeObject<List<int>>(PreAssign_Table_Id);
                        }

                        model.soft_assigned_tables = Convert.ToBoolean(dataReader["TablesSoftAssigned"]);
                        model.floor_plan_name = Convert.ToString(dataReader["FloorPlanName"]);
                        model.floor_plan_technical_name = Convert.ToString(dataReader["TechnicalName"]);
                        model.floor_plan_id = Convert.ToInt32(dataReader["FloorPlanId"]);

                        model.pre_assign_server_id = Convert.ToInt32(dataReader["PreAssign_Server_Id"]);

                        var reservation_holder = new ReservationHolder();
                        reservation_holder.user_id = Convert.ToInt32(dataReader["UserId"]);
                        reservation_holder.email = Convert.ToString(dataReader["Email"]);
                        reservation_holder.first_name = Convert.ToString(dataReader["FirstName"]);
                        reservation_holder.last_name = Convert.ToString(dataReader["LastName"]);
                        reservation_holder.customer_type = Convert.ToInt32(dataReader["CustomerType"]);
                        reservation_holder.phone = Convert.ToString(dataReader["PhoneNumber"]);
                        reservation_holder.mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        reservation_holder.mobile_phone_status = (MobileNumberStatus)Convert.ToInt32(dataReader["MobilePhoneStatus"]);

                        var reservationUserAddress = new ReservationUserAddress();
                        reservationUserAddress.city = Convert.ToString(dataReader["City"]);
                        reservationUserAddress.country = Convert.ToString(dataReader["Country"]);
                        reservationUserAddress.state = Convert.ToString(dataReader["State"]);
                        reservationUserAddress.zip_code = Convert.ToString(dataReader["Zip"]);

                        reservation_holder.address = reservationUserAddress;

                        model.reservation_holder = reservation_holder;

                        UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                        model.reservation_holder.account_note = userDAL.GetAccountNote(Convert.ToInt32(dataReader["WineryId"]), reservation_holder.user_id);

                        model.delay_in_minutes = Convert.ToInt32(dataReader["DelayInMinutes"]);
                        var paymentStatus = GetReservationPaymentStatus(Convert.ToDecimal(dataReader["FeeDue"]), Convert.ToDecimal(dataReader["AmountPaid"]));
                        model.payment_status = (int)paymentStatus;

                        ReservationModel waitlistdatetime = GetWaitlistStartEndTime(model.reservation_id);

                        if (waitlistdatetime != null)
                        {
                            if (waitlistdatetime.seating_end_time != null)
                            {
                                model.seating_end_time = waitlistdatetime.seating_end_time;
                            }
                            model.seating_start_time = waitlistdatetime.seating_start_time;
                        }
                        model.cancel_lead_time = Convert.ToInt32(dataReader["CancelLeadTime"]);
                        model.allow_cancel = IsCancelAllowedOnReservation(model.location_id, model.cancel_lead_time, model.event_start_date);
                        model.assigned_server_id = Convert.ToInt32(dataReader["AssignedServerId"]);
                        model.assigned_floor_plan_id = Convert.ToInt32(dataReader["AssignedFloorPlanId"]);
                        model.ticket_order_id = Convert.ToInt32(dataReader["TicketOrderID"]);
                        string assignedTableIds = Convert.ToString(dataReader["AssignedTableIds"]);
                        if (assignedTableIds.Length > 0)
                        {
                            model.assign_table_ids = JsonConvert.DeserializeObject<List<int>>(assignedTableIds);
                        }

                        model.addons_available = GetAddOnGroupByReservationId(model.reservation_id).Count > 0;

                        schedule.reservations.Add(model);

                    }
                }
            }

            return listRsvpEvents;
        }

        public List<ReservationEvent> GetPassportGuestlistByEventandMember(int TicketEventId, int MemberId, DateTime StartDateTime, DateTime EndDateTime)
        {
            List<ReservationEvent> listRsvpEvents = new List<ReservationEvent>();
            var lstReservationModel = new List<ReservationModel>();
            //List<ReservationEventSchedule> listReservationEventSchedule = new List<ReservationEventSchedule>();
            ReservationEvent rEvent = new ReservationEvent();
            ReservationEventSchedule schedule = new ReservationEventSchedule();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TicketEventId", TicketEventId));
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@StartDateTime", StartDateTime));
            parameterList.Add(GetParameter("@EndDateTime", EndDateTime));

            string sql = "GetPassportGuestlistByEventandMember";

            int OldEventId = -1;
            int OldSlotId = -1;

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        int EventId = dataReader["EventId"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EventId"]);

                        //check if new event or san existing one
                        if (OldEventId == -1 || OldEventId != EventId || EventId == 0)
                        {
                            rEvent = new ReservationEvent();
                            rEvent.event_times = new List<ReservationEventSchedule>();
                            OldSlotId = -1;

                            OldEventId = EventId;

                            rEvent.event_id = EventId;

                            if (OldEventId == 0)
                            {
                                rEvent.event_date = Convert.ToDateTime(dataReader["EventDate"]);
                                if (TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])) > TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])))
                                    rEvent.event_date_end = Convert.ToDateTime(dataReader["EventDate"]).AddDays(1);
                                else
                                    rEvent.event_date_end = Convert.ToDateTime(dataReader["EventDate"]);
                            }
                            else
                            {
                                EventModel eventModel = GetEventById(EventId);
                                rEvent.event_date = eventModel.StartDate;
                                rEvent.event_date_end = eventModel.EndDate;
                            }

                            rEvent.event_name = Convert.ToString(dataReader["EventName"]);
                            rEvent.event_technical_location = Convert.ToString(dataReader["technicalname"]);
                            rEvent.table_status_group_id = Convert.ToInt32(dataReader["Table_Status_Group_Id"]);
                            rEvent.member_id = Convert.ToInt32(dataReader["WineryId"]);
                            listRsvpEvents.Add(rEvent); //added event to the response
                        }

                        //check if new slot object needs to be added to the event above
                        int SlotId = dataReader["SlotId"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SlotId"]);

                        if (OldSlotId == -1 || OldSlotId != SlotId)
                        {
                            schedule = new ReservationEventSchedule();
                            schedule.reservations = new List<ReservationModel>();

                            OldSlotId = SlotId;

                            schedule.slot_id = SlotId;
                            if (SlotId > 0)
                            {
                                schedule.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                            }
                            else
                            {
                                schedule.slot_type = 2;
                            }

                            schedule.time = Convert.ToString(dataReader["SlotTime"]);
                            schedule.event_duration_minutes = Convert.ToInt32(dataReader["event_duration_minutes"]);
                            rEvent.event_times.Add(schedule);
                        }

                        var model = new ReservationModel();
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        model.event_location = Convert.ToString(dataReader["EventLocation"]);

                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        if (TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])) > TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])))
                            model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).AddDays(1).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));
                        else
                            model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));

                        model.location_id = Convert.ToInt32(dataReader["LocationId"]);

                        model.fee_per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.total_guests = Convert.ToInt16(dataReader["TotalGuests"]);
                        model.booking_date = ConvertFromDateTimeOffset(DateTimeOffset.Parse(Convert.ToString(dataReader["BookingDate"])));
                        model.guest_note = Convert.ToString(dataReader["Note"]);
                        model.referral_type = Convert.ToByte(dataReader["ReferralType"]);
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.booking_guid = Convert.ToString(dataReader["BookingGUID"]);
                        model.status = Convert.ToByte(dataReader["Status"]);
                        model.concierge_note = Convert.ToString(dataReader["ConciergeNote"]);
                        model.fee_due = Convert.ToDecimal(dataReader["FeeDue"]);
                        model.amount_paid = Convert.ToDecimal(dataReader["AmountPaid"]);

                        model.full_paid = (model.amount_paid >= model.fee_due);

                        model.internal_note = Convert.ToString(dataReader["InternalNote"]);
                        model.country = Convert.ToString(dataReader["Country"]);
                        model.concierge_name = Convert.ToString(dataReader["ConciergeName"]);
                        model.referral_name = Convert.ToString(dataReader["ReferralName"]);
                        model.reservation_seated_status = Convert.ToInt32(dataReader["SeatedStatus"]);
                        model.convenience_fee = Convert.ToDecimal(dataReader["TransactionFee"]);
                        model.gratuity_amount = Convert.ToDecimal(dataReader["GratuityAmount"]);
                        model.reservation_seated_status = Convert.ToInt32(dataReader["SeatedStatus"]);
                        string PreAssign_Table_Id = Convert.ToString(dataReader["PreAssign_Table_Id"]);
                        if (PreAssign_Table_Id.Length > 0)
                        {
                            model.pre_assign_table_ids = JsonConvert.DeserializeObject<List<int>>(PreAssign_Table_Id);
                        }

                        model.soft_assigned_tables = Convert.ToBoolean(dataReader["TablesSoftAssigned"]);
                        model.floor_plan_name = Convert.ToString(dataReader["FloorPlanName"]);
                        model.floor_plan_technical_name = Convert.ToString(dataReader["TechnicalName"]);
                        model.floor_plan_id = Convert.ToInt32(dataReader["FloorPlanId"]);

                        model.pre_assign_server_id = Convert.ToInt32(dataReader["PreAssign_Server_Id"]);

                        var reservation_holder = new ReservationHolder();
                        reservation_holder.user_id = Convert.ToInt32(dataReader["UserId"]);
                        reservation_holder.email = Convert.ToString(dataReader["Email"]);
                        reservation_holder.first_name = Convert.ToString(dataReader["FirstName"]);
                        reservation_holder.last_name = Convert.ToString(dataReader["LastName"]);
                        reservation_holder.customer_type = Convert.ToInt32(dataReader["CustomerType"]);
                        reservation_holder.phone = Convert.ToString(dataReader["PhoneNumber"]);
                        reservation_holder.mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        reservation_holder.mobile_phone_status = (MobileNumberStatus)Convert.ToInt32(dataReader["MobilePhoneStatus"]);

                        var reservationUserAddress = new ReservationUserAddress();
                        reservationUserAddress.city = Convert.ToString(dataReader["City"]);
                        reservationUserAddress.country = Convert.ToString(dataReader["Country"]);
                        reservationUserAddress.state = Convert.ToString(dataReader["State"]);
                        reservationUserAddress.zip_code = Convert.ToString(dataReader["Zip"]);

                        reservation_holder.address = reservationUserAddress;

                        model.reservation_holder = reservation_holder;

                        UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                        model.reservation_holder.account_note = userDAL.GetAccountNote(Convert.ToInt32(dataReader["WineryId"]), reservation_holder.user_id);

                        model.delay_in_minutes = Convert.ToInt32(dataReader["DelayInMinutes"]);
                        var paymentStatus = GetReservationPaymentStatus(Convert.ToDecimal(dataReader["FeeDue"]), Convert.ToDecimal(dataReader["AmountPaid"]));
                        model.payment_status = (int)paymentStatus;

                        ReservationModel waitlistdatetime = GetWaitlistStartEndTime(model.reservation_id);

                        if (waitlistdatetime != null)
                        {
                            if (waitlistdatetime.seating_end_time != null)
                            {
                                model.seating_end_time = waitlistdatetime.seating_end_time;
                            }
                            model.seating_start_time = waitlistdatetime.seating_start_time;
                        }
                        model.cancel_lead_time = Convert.ToInt32(dataReader["CancelLeadTime"]);
                        model.allow_cancel = IsCancelAllowedOnReservation(model.location_id, model.cancel_lead_time, model.event_start_date);
                        model.assigned_server_id = Convert.ToInt32(dataReader["AssignedServerId"]);
                        model.assigned_floor_plan_id = Convert.ToInt32(dataReader["AssignedFloorPlanId"]);
                        string assignedTableIds = Convert.ToString(dataReader["AssignedTableIds"]);
                        if (assignedTableIds.Length > 0)
                        {
                            model.assign_table_ids = JsonConvert.DeserializeObject<List<int>>(assignedTableIds);
                        }
                        model.ticket_order_id = Convert.ToInt32(dataReader["TicketOrderID"]);
                        schedule.reservations.Add(model);

                    }
                }
            }

            return listRsvpEvents;
        }

        private ReservationPaymentStatus GetReservationPaymentStatus(decimal total, decimal paid)
        {
            ReservationPaymentStatus status = ReservationPaymentStatus.NOPAYMENT;
            if (total > 0 && (paid >= total))
            {
                status = ReservationPaymentStatus.PAIDFULL;
            }
            else if (total > 0 && paid == 0)
            {
                status = ReservationPaymentStatus.UNPAID;
            }
            else if (total > 0 && paid > 0 && (paid < total))
            {
                status = ReservationPaymentStatus.PAIDPARTIAL;
            }

            return status;
        }

        public ReservationStatusModel GetReservationDetailsbyId(int ReservationId, string booking_code = "", string booking_guid = "")
        {
            var model = new ReservationStatusModel();

            string sql = "select slotid,slottype,userid,locationid,CancelLeadTime,EventDate,StartTime,PreAssign_Server_Id,PreAssign_Table_Id,SeatedStatus from reservationv2 (nolock) where ReservationId = @reservationId or BookingCode = @bookingCode or cast(BookingGUID as varchar(50))= @bookingGUID";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", ReservationId));
            parameterList.Add(GetParameter("@bookingCode", booking_code));
            parameterList.Add(GetParameter("@bookingGUID", booking_guid));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        if (model.slot_id > 0)
                        {
                            model.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        }
                        else
                        {
                            model.slot_type = 2;
                        }

                        model.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        model.user_id = Convert.ToInt32(dataReader["userid"]);
                        model.cancel_lead_time = Convert.ToInt32(dataReader["CancelLeadTime"]);
                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));

                        model.pre_assign_server_id = Convert.ToInt32(dataReader["PreAssign_Server_Id"]);
                        string PreAssign_Table_Id = Convert.ToString(dataReader["PreAssign_Table_Id"]);
                        if (PreAssign_Table_Id.Length > 0)
                        {
                            model.pre_assign_table_ids = JsonConvert.DeserializeObject<List<int>>(PreAssign_Table_Id);
                        }

                        model.seated_status = Convert.ToInt32(dataReader["SeatedStatus"]);
                    }
                }
            }
            return model;
        }

        public ReservationDetailModel GetReservationDetailsForStatusChange(int ReservationId)
        {
            var model = new ReservationDetailModel();

            string sql = "select ReservationId, FeeDue, Status, BookingCode, BookingGUID, BookingDate, ReferralType,TotalGuests, ReferralID, SlotId,SlotType,userid,LocationId,CancelLeadTime,EventDate,StartTime,EndTime,CancelLeadTime,LeadTime,PreAssign_Server_Id,PreAssign_Table_Id,SeatedStatus from reservationv2 (nolock) where ReservationId = @reservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", ReservationId));
            //parameterList.Add(GetParameter("@bookingCode", booking_code));
            //parameterList.Add(GetParameter("@bookingGUID", booking_guid));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        model.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        if (model.slot_id > 0)
                        {
                            model.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        }
                        else
                        {
                            model.slot_type = 2;
                        }

                        model.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        model.cancel_lead_time = Convert.ToInt32(dataReader["CancelLeadTime"]);
                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));

                        model.referral_id = Convert.ToInt32(dataReader["ReferralID"]);

                        model.total_guests = Convert.ToInt16(dataReader["TotalGuests"]);
                        model.booking_date = ConvertFromDateTimeOffset(DateTimeOffset.Parse(Convert.ToString(dataReader["BookingDate"])));
                        model.referral_type = Convert.ToByte(dataReader["ReferralType"]);
                        model.reservation_type = Convert.ToInt32(dataReader["ReferralType"]);
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.booking_guid = Convert.ToString(dataReader["BookingGUID"]);
                        model.status = Convert.ToByte(dataReader["Status"]);
                        model.fee_due = Convert.ToDecimal(dataReader["FeeDue"]);

                        model.pre_assign_server_id = Convert.ToInt32(dataReader["PreAssign_Server_Id"]);
                        string PreAssign_Table_Id = Convert.ToString(dataReader["PreAssign_Table_Id"]);
                        if (PreAssign_Table_Id.Length > 0)
                        {
                            model.pre_assign_table_ids = JsonConvert.DeserializeObject<List<int>>(PreAssign_Table_Id);
                        }

                        model.seated_status = Convert.ToInt32(dataReader["SeatedStatus"]);
                    }
                }
            }
            return model;
        }


        public int GetFloorPlanIdForReservation(int reservationId)
        {
            int floorPlanId = 0;

            string sql = "select Isnull(floorplanId, 0) as FloorPlanId from reservationv2 (nolock) where ReservationId = @reservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", reservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        floorPlanId = Convert.ToInt32(dataReader["FloorPlanId"]);
                    }
                }
            }
            return floorPlanId;
        }

        public string GetDestinationNameForReservation(int reservationId)
        {
            string DestinationName = string.Empty;

            string sql = "select DestinationName from reservationv2 (nolock) r join location (nolock) l on r.locationid = l.id where reservationid = @reservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", reservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        DestinationName = Convert.ToString(dataReader["DestinationName"]);
                    }
                }
            }
            return DestinationName;
        }

        public ReservationDetailModel GetReservationDetailsbyReservationId(int ReservationId, string booking_code = "", bool IsAdmin = true, string booking_guid = "", bool IncludeReservationHolderMetrics = false, bool includeAllLogs = true, bool includeAddlGuets = true, bool includeCancelledReasons = false)
        {
            var model = new ReservationDetailModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", ReservationId));
            parameterList.Add(GetParameter("@bookingCode", booking_code));
            parameterList.Add(GetParameter("@bookingGUID", booking_guid));
            parameterList.Add(GetParameter("@IncludeReservationHolderMetrics", IncludeReservationHolderMetrics));

            using (DbDataReader dataReader = GetDataReader("GetReservationDetailByIDOrBookingCode", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        ReservationId = model.reservation_id;
                        model.event_id = dataReader["EventId"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EventId"]);
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.location_name = Convert.ToString(dataReader["EventLocation"]);
                        model.google_calendar_event_url = Convert.ToString(dataReader["GoogleCalendarEventEditURL"]);
                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));

                        if (model.event_start_date > model.event_end_date)
                            model.event_end_date = model.event_end_date.AddDays(1);

                        model.fee_per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.fee_type = Convert.ToInt32(dataReader["FeeType"]);

                        model.total_guests = Convert.ToInt16(dataReader["TotalGuests"]);
                        model.booking_date = ConvertFromDateTimeOffset(DateTimeOffset.Parse(Convert.ToString(dataReader["BookingDate"])));
                        model.referral_type = Convert.ToByte(dataReader["ReferralType"]);
                        model.reservation_type = Convert.ToInt32(dataReader["ReferralType"]);
                        model.referral_type_text = ((uc.Common.ReferralTypeText)Convert.ToInt32(dataReader["ReferralType"])).GetEnumDescription();
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.booking_guid = Convert.ToString(dataReader["BookingGUID"]);
                        model.status = Convert.ToByte(dataReader["Status"]);
                        model.fee_due = Convert.ToDecimal(dataReader["FeeDue"]);
                        model.amount_paid = Convert.ToDecimal(dataReader["AmountPaid"]);

                        model.full_paid = (model.amount_paid >= model.fee_due);

                        model.member_url = Convert.ToString(dataReader["PurchaseUrl"]);
                        model.member_website = Convert.ToString(dataReader["WebsiteURL"]);

                        model.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        if (model.slot_id > 0)
                        {
                            model.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        }
                        else
                        {
                            model.slot_type = 2;
                        }

                        model.concierge_name = Convert.ToString(dataReader["ConciergeName"]);
                        model.referral_name = Convert.ToString(dataReader["ReferralName"]);
                        model.lead_time = Convert.ToInt32(dataReader["LeadTime"]);
                        model.cancel_lead_time = Convert.ToInt32(dataReader["CancelLeadTime"]);
                        model.charge_fee = Convert.ToByte(dataReader["ChargeFee"]);

                        TicketOrderDetail ticketOrderDetail = new TicketOrderDetail();

                        ticketOrderDetail.ticket_order_id = Convert.ToInt32(dataReader["TicketOrderID"]);

                        if (ticketOrderDetail.ticket_order_id > 0)
                        {
                            ticketOrderDetail.ticketlevels = GetTicketLevelsbyTicketOrderId(ticketOrderDetail.ticket_order_id);
                        }

                        model.ticket_order = ticketOrderDetail;

                        model.booked_by_name = Convert.ToString(dataReader["BookedByName"]);

                        string CancelTimeDesc = Convert.ToString(dataReader["CancelTime"]);

                        if (CancelTimeDesc.IndexOf("Days") > -1)
                        {
                            try
                            {
                                int CancelTime = Convert.ToInt32(CancelTimeDesc.Replace("Days", "").Replace(" ", ""));

                                if (CancelTimeDesc == "Non-changeable, Non-cancellable")
                                {
                                    model.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                                }
                                else if (CancelTime > 20)
                                {
                                    model.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                                }
                                else
                                {
                                    model.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                                }
                            }
                            catch
                            {
                                model.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                            }
                        }
                        else
                        {
                            model.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                        }

                        model.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        model.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        model.guest_note = Convert.ToString(dataReader["Note"]);
                        model.booked_by_id = Convert.ToInt32(dataReader["BookedById"]);
                        model.concierge_note = Convert.ToString(dataReader["ConciergeNote"]);
                        model.affiliate_id = Convert.ToInt32(dataReader["AffiliateID"]);
                        model.referral_id = Convert.ToInt32(dataReader["ReferralID"]);
                        model.email_content_id = Convert.ToInt32(dataReader["EmailContentID"]);
                        model.hdyh = Convert.ToString(dataReader["HDYH"]);
                        model.internal_note = Convert.ToString(dataReader["InternalNote"]);
                        model.discount_code = Convert.ToString(dataReader["DiscountCode"]);
                        model.discount_amount = Convert.ToDecimal(dataReader["DiscountAmt"]);
                        model.rms_sku = Convert.ToString(dataReader["RMSsku"]);
                        model.region_id = Convert.ToInt32(dataReader["RegionId"]);
                        model.region_name = Convert.ToString(dataReader["RegionName"]);
                        model.region_friendly_url = Convert.ToString(dataReader["RegionFriendlyURL"]);
                        model.tags = Convert.ToString(dataReader["tags"]);
                        model.user_tags = Convert.ToString(dataReader["UserTags"]);

                        model.seated_status = Convert.ToInt32(dataReader["SeatedStatus"]);
                        model.delay_in_minutes = Convert.ToInt32(dataReader["DelayInMinutes"]);

                        var paymentStatus = GetReservationPaymentStatus(Convert.ToDecimal(dataReader["FeeDue"]), Convert.ToDecimal(dataReader["AmountPaid"]));
                        model.payment_status = (int)paymentStatus;

                        model.charge_fee_description = uc.Common.GetChargeFee().Where(f => f.ID == model.charge_fee.ToString()).Select(f => f.Name).FirstOrDefault();

                        var payCard = new PayCard();
                        payCard.cust_name = Convert.ToString(dataReader["PayCardCustName"]);
                        payCard.exp_month = Convert.ToString(dataReader["PayCardExpMonth"]);
                        payCard.exp_year = Convert.ToString(dataReader["PayCardExpYear"]);
                        payCard.number = Convert.ToString(dataReader["PayCardNumber"]);
                        payCard.card_token = Convert.ToString(dataReader["PayCardToken"]);
                        payCard.card_type = Convert.ToString(dataReader["PayCardType"]);

                        string cardNumber = StringHelpers.Decryption(payCard.number);
                        if (!string.IsNullOrEmpty(cardNumber))
                        {
                            payCard.card_last_four_digits = Common.Common.Right(cardNumber, 4);

                            if (cardNumber.Length>4)
                                payCard.card_first_four_digits = Common.Common.Left(cardNumber, 4);
                        }

                        model.pay_card = payCard;

                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString_readonly);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(model.member_id, (int)Common.Common.SettingGroup.member);
                        bool covidSurveyEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_covid_survey);
                        bool covidWaiverEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_covid_waiver);

                        var userDetail = new UserDetail();
                        userDetail.user_id = Convert.ToInt32(dataReader["UserId"]);
                        userDetail.email = Convert.ToString(dataReader["Email"]);
                        userDetail.first_name = Convert.ToString(dataReader["FirstName"]);
                        userDetail.last_name = Convert.ToString(dataReader["LastName"]);
                        userDetail.customer_type = Convert.ToInt32(dataReader["CustomerType"]);

                        List<string> listcontacttypes = new List<string>();

                        string[] contacttypes = Convert.ToString(dataReader["ContactTypes"]).Trim().Split(',');

                        foreach (string ct in contacttypes)
                        {
                            if (ct.Trim().Length > 0)
                            {
                                listcontacttypes.Add(ct);
                            }
                        }

                        userDetail.contact_types = listcontacttypes;

                        userDetail.phone_number = Convert.ToString(dataReader["PhoneNumber"]);
                        userDetail.mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        userDetail.mobile_phone_status = (MobileNumberStatus)Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        userDetail.region_most_visited = Convert.ToInt32(dataReader["RegionMostVisited"]);
                        if (covidSurveyEnabled || covidWaiverEnabled)
                        {
                            if (!string.IsNullOrEmpty(userDetail.email))
                            {
                                SurveyWaiverStatus surveyWaiverStatus = GetSurveyWaiverStatusByEmailAndMemberId(model.member_id, userDetail.email);

                                userDetail.survey_expire_date = surveyWaiverStatus.survey_expire_date;
                                userDetail.survey_status = surveyWaiverStatus.survey_status;
                                userDetail.waiver_status = surveyWaiverStatus.waiver_status;
                                userDetail.modified_date = surveyWaiverStatus.modified_date;
                            }
                            else
                            {
                                if (covidSurveyEnabled)
                                    userDetail.survey_status = RSVPPostCaptureStatus.Available;

                                if (covidWaiverEnabled)
                                    userDetail.waiver_status = RSVPPostCaptureStatus.Available;
                            }
                        }

                        var reservationUserAddress = new ReservationUserAddress();
                        reservationUserAddress.city = Convert.ToString(dataReader["City"]);
                        reservationUserAddress.country = Convert.ToString(dataReader["Country"]);
                        reservationUserAddress.state = Convert.ToString(dataReader["State"]);
                        reservationUserAddress.zip_code = Convert.ToString(dataReader["Zip"]);
                        reservationUserAddress.address_1 = Convert.ToString(dataReader["Address1"]);
                        reservationUserAddress.address_2 = Convert.ToString(dataReader["Address2"]);

                        userDetail.address = reservationUserAddress;

                        model.user_detail = userDetail;

                        var reservationHolderMetrics = new ReservationHolderMetrics();
                        reservationHolderMetrics.cancellations_count = Convert.ToInt32(dataReader["Cancellationscount"]);
                        reservationHolderMetrics.no_shows_count = Convert.ToInt32(dataReader["Noshowscount"]);
                        reservationHolderMetrics.visits_count = Convert.ToInt32(dataReader["Visitscount"]);

                        model.reservation_holder_metrics = reservationHolderMetrics;
                        if (includeAllLogs)
                        {
                            model.reservation_status_log = GetReservationChangeLogsbyReservationId(ReservationId, 0);
                            model.reservation_discounts_log = GetReservationChangeLogsbyReservationId(ReservationId, 1);
                            model.reservation_emails_log = GetReservationEmailLogsbyReservationId(ReservationId);
                            model.reservation_payments_log = GetPayments(ReservationId, payCard);

                            foreach (var item in model.reservation_payments_log)
                            {
                                if (item.card_last_four_digits == model.pay_card.card_last_four_digits && item.payment_card_exp_month == model.pay_card.exp_month && item.payment_card_exp_year == model.pay_card.exp_year)
                                {
                                    model.pay_card.card_first_four_digits = item.card_first_four_digits;
                                    model.pay_card.card_token = item.payment_card_token;
                                }
                            }
                        }

                        var add_guest = new Additional_guests();
                        add_guest.first_name = userDetail.first_name;
                        add_guest.last_name = userDetail.last_name;
                        add_guest.id = 0;
                        add_guest.email = userDetail.email;
                        add_guest.user_id = userDetail.user_id;
                        add_guest.survey_expire_date = userDetail.survey_expire_date;
                        add_guest.modified_date = userDetail.modified_date;
                        add_guest.survey_status = userDetail.survey_status;
                        add_guest.waiver_status = userDetail.waiver_status;

                        if(includeAddlGuets)
                            model.additional_guests = GetRsvpGuestsbyReservationId(ReservationId, model.member_id, covidSurveyEnabled, covidWaiverEnabled, add_guest);

                        model.reservation_addon = GetReservation_AddonbyReservationId(ReservationId);
                        UserDAL userDAL = new UserDAL(Common.Common.ConnectionString_readonly);
                        model.user_detail.account_note = userDAL.GetAccountNote(model.member_id, model.user_detail.user_id);
                        model.table_status_group_id = Convert.ToInt32(dataReader["Table_Status_Group_Id"]);
                        model.pre_assign_server_id = Convert.ToInt32(dataReader["PreAssign_Server_Id"]);

                        int pre_assign_server_id = model.pre_assign_server_id ?? 0;

                        if (pre_assign_server_id > 0)
                        {
                            UserDetailModel userDetailModel = userDAL.GetUserById(pre_assign_server_id);
                            model.pre_assigned_server_first_name = userDetailModel.first_name;
                            model.pre_assigned_server_last_name = userDetailModel.last_name;
                            model.pre_assigned_server_color = userDetailModel.color;
                        }

                        string PreAssign_Table_Id = Convert.ToString(dataReader["PreAssign_Table_Id"]);
                        if (PreAssign_Table_Id.Length > 0)
                        {
                            model.pre_assign_table_ids = JsonConvert.DeserializeObject<List<int>>(PreAssign_Table_Id);
                        }
                        model.soft_assigned_tables = Convert.ToBoolean(dataReader["TablesSoftAssigned"]);
                        model.ignore_discount = Convert.ToBoolean(dataReader["IgnoreDiscount"]);
                        model.floor_plan_name = Convert.ToString(dataReader["FloorPlanName"]);
                        model.floor_plan_technical_name = Convert.ToString(dataReader["TechnicalName"]);
                        model.floor_plan_id = Convert.ToInt32(dataReader["FloorPlanId"]);
                        model.assigned_floor_plan_id = Convert.ToInt32(dataReader["AssignedFloorPlanId"]);
                        string assignedTableIds = Convert.ToString(dataReader["AssignedTableIds"]);
                        if (assignedTableIds.Length > 0)
                        {
                            model.assign_table_ids = JsonConvert.DeserializeObject<List<int>>(assignedTableIds);
                        }
                        ReservationModel waitlistdatetime = GetWaitlistStartEndTime(model.reservation_id);

                        if (waitlistdatetime != null)
                        {
                            if (waitlistdatetime.seating_end_time != null)
                            {
                                model.seating_end_time = waitlistdatetime.seating_end_time;
                            }
                            model.seating_start_time = waitlistdatetime.seating_start_time;
                        }

                        model.status_changed_date = Convert.ToDateTime(dataReader["StatusChangeDate"]);

                        model.winery_name = Convert.ToString(dataReader["WineryName"]);
                        model.location_address1 = Convert.ToString(dataReader["LocationAddress1"]);
                        model.location_address2 = Convert.ToString(dataReader["LocationAddress2"]);
                        model.location_city = Convert.ToString(dataReader["LocationCity"]);
                        model.location_state = Convert.ToString(dataReader["LocationState"]);
                        model.location_zip = Convert.ToString(dataReader["LocationZip"]);
                        model.location_country = Convert.ToString(dataReader["LocationCountry"]);
                        model.location_phone = Convert.ToString(dataReader["BusinessPhone"]);
                        model.convenience_fee = Convert.ToInt32(dataReader["ConvenienceFee"]);
                        model.gratuity_amount = Convert.ToDecimal(dataReader["GratuityAmt"]);
                        model.transportation_id = Convert.ToInt32(dataReader["TransportationId"]);
                        model.transportation_name = Convert.ToString(dataReader["TransportationName"]);
                        model.destination_name = Convert.ToString(dataReader["DestinationName"]);
                        model.contact_types = Convert.ToString(dataReader["ContactTypes"]);

                        model.sales_tax = Convert.ToDecimal(dataReader["SalesTax"]);
                        model.sales_tax_percent = Convert.ToDecimal(dataReader["SalesTaxPercentage"]);
                        model.assigned_server_id = Convert.ToInt32(dataReader["AssignedServerId"]);

                        if (model.assigned_server_id > 0)
                        {
                            UserDetailModel userDetailModel = userDAL.GetUserById(model.assigned_server_id);
                            model.assigned_server_first_name = userDetailModel.first_name;
                            model.assigned_server_last_name = userDetailModel.last_name;
                            model.assigned_server_color = userDetailModel.color;
                        }

                        decimal addOnTotal = 0;

                        foreach (var item in model.reservation_addon)
                        {
                            addOnTotal += item.price * item.qty;
                        }

                        model.addon_total = addOnTotal;
                        model.taxes_and_fees = model.fee_due + model.discount_amount - model.addon_total - model.gratuity_amount - (model.fee_per_person * model.total_guests);

                        if (model.taxes_and_fees > model.sales_tax)
                            model.show_convenience_fee = true;

                        model.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        model.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        model.deposit_policy = uc.Common.GetDepositPolicies(IsAdmin).Where(f => f.ID == Convert.ToString(dataReader["ChargeFee"])).Select(f => f.Name).FirstOrDefault();
                        model.allow_cancel = IsCancelAllowedOnReservation(model.location_id, model.cancel_lead_time, model.event_start_date);

                        model.limit_my_account = Convert.ToBoolean(dataReader["LimitMyAccount"]);
                        //model.required_hdyh = Convert.ToInt32(dataReader["HDYH_Requirement"]);
                        //model.region_id = Convert.ToInt32(dataReader["Appelation"]);
                        try
                        {
                            model.event_description = Convert.ToString(dataReader["Description"]);
                        }
                        catch { }

                        var authPayment = GetReservationPreAuths(model.reservation_id);

                        if (authPayment != null && authPayment.Count > 0)
                        {
                            model.calculated_gratuity_amount = authPayment[0].original_amount - model.fee_due;
                        }
                        model.sms_opt_out = Convert.ToBoolean(dataReader["OptOutFromSMS"]);
                        model.cancellation_reason = Convert.ToString(dataReader["CancellationReason"]);
                        model.cancellation_reason_setting = (CancellationReasonSetting)Convert.ToInt32(dataReader["CancellationRequired"]);

                        if (includeCancelledReasons == true && (model.cancellation_reason_setting == CancellationReasonSetting.Required || model.cancellation_reason_setting == CancellationReasonSetting.Optional))
                        {
                            model.cancellation_reasons = GetCancellationReasonsByEventId(model.event_id);

                        }

                        if (dataReader["CancelByDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["CancelByDate"].ToString()))
                            model.cancel_by_date = Convert.ToDateTime(dataReader["CancelByDate"]);

                        model.cancel_message = Convert.ToString(dataReader["CancelMessage"]);
                        model.timezone_name = Convert.ToString(dataReader["TimeZoneName"]);
                    }
                    List<AttendeeQuestion> questions = new List<AttendeeQuestion>();
                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var question = new AttendeeQuestion
                            {
                                question = Convert.ToString(dataReader["QuestionText"]),
                                answer = Convert.ToString(dataReader["Choice"])
                            };
                            questions.Add(question);

                        }

                    }
                    model.attendee_questions = questions;
                    
                }
            }
            return model;
        }

        public ReservationDetail2Model GetReservationDetails2byReservationId(int ReservationId)
        {
            var model = new ReservationDetail2Model();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader("GetReservationDetailByID", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        model.referral_type = Convert.ToByte(dataReader["ReferralType"]);
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        var payCard = new PayCard();
                        payCard.cust_name = Convert.ToString(dataReader["PayCardCustName"]);
                        payCard.exp_month = Convert.ToString(dataReader["PayCardExpMonth"]);
                        payCard.exp_year = Convert.ToString(dataReader["PayCardExpYear"]);
                        payCard.number = Convert.ToString(dataReader["PayCardNumber"]);
                        payCard.card_token = Convert.ToString(dataReader["PayCardToken"]);
                        payCard.card_type = Convert.ToString(dataReader["PayCardType"]);

                        string cardNumber = StringHelpers.Decryption(payCard.number);
                        if (!string.IsNullOrEmpty(cardNumber))
                        {
                            payCard.card_last_four_digits = Common.Common.Right(cardNumber, 4);

                            if (cardNumber.Length > 4)
                                payCard.card_first_four_digits = Common.Common.Left(cardNumber, 4);
                        }

                        model.pay_card = payCard;

                        var userDetail = new UserDetail2();
                        userDetail.user_id = Convert.ToInt32(dataReader["UserId"]);
                        userDetail.email = Convert.ToString(dataReader["Email"]);
                        userDetail.first_name = Convert.ToString(dataReader["FirstName"]);
                        userDetail.last_name = Convert.ToString(dataReader["LastName"]);

                        userDetail.phone_number = Convert.ToString(dataReader["PhoneNumber"]);

                        var reservationUserAddress = new ReservationUserAddress();
                        reservationUserAddress.city = Convert.ToString(dataReader["City"]);
                        reservationUserAddress.country = Convert.ToString(dataReader["Country"]);
                        reservationUserAddress.state = Convert.ToString(dataReader["State"]);
                        reservationUserAddress.zip_code = Convert.ToString(dataReader["Zip"]);
                        reservationUserAddress.address_1 = Convert.ToString(dataReader["Address1"]);
                        reservationUserAddress.address_2 = Convert.ToString(dataReader["Address2"]);

                        userDetail.address = reservationUserAddress;

                        model.user_detail = userDetail;
                    }
                }
                return model;
            }
        }

        public ReservationDetailV3Model GetReservationDetailByGuid(string booking_guid)
        {
            var model = new ReservationDetailV3Model();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@bookingGUID", booking_guid));

            using (DbDataReader dataReader = GetDataReader("GetReservationDetailByGuid", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        model.user_id = Convert.ToInt32(dataReader["UserId"]);
                        model.email = Convert.ToString(dataReader["Email"]);
                        model.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        if (model.slot_id > 0)
                        {
                            model.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        }
                        else
                        {
                            model.slot_type = 2;
                        }
                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));
                        model.total_guests = Convert.ToInt16(dataReader["TotalGuests"]);
                        model.tags = Convert.ToString(dataReader["tags"]);
                        model.guest_note = Convert.ToString(dataReader["Note"]);
                        model.internal_note = Convert.ToString(dataReader["InternalNote"]);
                        model.personal_message = Convert.ToString(dataReader["PersonalMessage"]);

                        model.MemberBenefit = Convert.ToInt32(dataReader["MemberBenefit"]);
                        model.EventId = Convert.ToInt32(dataReader["EventId"]);
                        model.DiscountAmt = Convert.ToDecimal(dataReader["DiscountAmt"]);
                        model.FeePerPerson = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.FeeType = Convert.ToInt32(dataReader["FeeType"]);
                        model.EventName = Convert.ToString(dataReader["EventName"]);
                        model.GratuityAmount = Convert.ToDecimal(dataReader["GratuityAmount"]);
                        model.SalesTaxPercentage = Convert.ToDecimal(dataReader["SalesTaxPercentage"]);
                        model.SalesTax = Convert.ToDecimal(dataReader["SalesTax"]);
                        model.TransactionFee = Convert.ToDecimal(dataReader["TransactionFee"]);
                        model.TransactionType = Convert.ToInt32(dataReader["TransactionType"]);
                        model.ChargeFee = Convert.ToInt32(dataReader["ChargeFee"]);

                        model.FloorPlanId = Convert.ToInt32(dataReader["FloorPlanId"]);
                        model.firstName = Convert.ToString(dataReader["firstName"]);
                        model.lastName = Convert.ToString(dataReader["lastName"]);
                        model.CustomerType = Convert.ToInt32(dataReader["CustomerType"]);
                        model.PhoneNumber = Convert.ToString(dataReader["PhoneNumber"]);
                        model.City = Convert.ToString(dataReader["City"]);
                        model.Zip = Convert.ToString(dataReader["Zip"]);
                        model.Country = Convert.ToString(dataReader["Country"]);
                        model.State = Convert.ToString(dataReader["State"]);
                        model.Address1 = Convert.ToString(dataReader["Address1"]);
                        model.Address2 = Convert.ToString(dataReader["Address2"]);
                        model.TransportationId = Convert.ToInt32(dataReader["TransportationId"]);
                        model.TransportationName = Convert.ToString(dataReader["TransportationName"]);
                        model.GratuityPercentage = Convert.ToDecimal(dataReader["GratuityPercentage"]);
                        model.TaxGratuity = Convert.ToBoolean(dataReader["TaxGratuity"]);
                        model.LocationId = Convert.ToInt32(dataReader["LocationId"]);
                        model.EmailContentId = Convert.ToInt32(dataReader["EmailContentId"]);
                        model.CancelLeadTime = Convert.ToInt32(dataReader["CancelLeadTime"]);
                        model.EventLocation = Convert.ToString(dataReader["EventLocation"]);
                        model.ConciergeNote = Convert.ToString(dataReader["ConciergeNote"]);
                        model.AffiliateID = Convert.ToInt32(dataReader["AffiliateID"]);
                        model.ReferralId = Convert.ToInt32(dataReader["ReferralId"]);

                        var payCard = new PayCard();
                        payCard.cust_name = Convert.ToString(dataReader["PayCardCustName"]);
                        payCard.exp_month = Convert.ToString(dataReader["PayCardExpMonth"]);
                        payCard.exp_year = Convert.ToString(dataReader["PayCardExpYear"]);
                        payCard.number = Convert.ToString(dataReader["PayCardNumber"]);
                        payCard.card_token = Convert.ToString(dataReader["PayCardToken"]);
                        payCard.card_type = Convert.ToString(dataReader["PayCardType"]);

                        string cardNumber = StringHelpers.Decryption(payCard.number);
                        if (!string.IsNullOrEmpty(cardNumber))
                        {
                            payCard.card_last_four_digits = Common.Common.Right(cardNumber, 4);

                            if (cardNumber.Length > 4)
                                payCard.card_first_four_digits = Common.Common.Left(cardNumber, 4);
                        }

                        model.pay_card = payCard;

                        List<Reservation_Addon> listreservation_Addon = new List<Reservation_Addon>();
                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                var model1 = new Reservation_Addon();
                                model1.qty = Convert.ToInt32(dataReader["Qty"]);
                                model1.sku = Convert.ToString(dataReader["Sku"]);
                                model1.name = Convert.ToString(dataReader["Name"]);
                                model1.description = Convert.ToString(dataReader["Description"]);
                                model1.cost = Convert.ToDecimal(dataReader["Cost"]);
                                model1.price = Convert.ToDecimal(dataReader["Price"]);
                                model1.category = Convert.ToInt32(dataReader["Category"]);
                                model1.item_type = Convert.ToInt32(dataReader["ItemType"]);
                                model1.image = Convert.ToString(dataReader["Image"]);
                                model1.item_id = Convert.ToInt32(dataReader["ItemId"]);
                                model1.group_item_id = Convert.ToInt32(dataReader["group_item_id"]);
                                model1.group_id = Convert.ToInt32(dataReader["group_id"]);
                                model1.group_name = Convert.ToString(dataReader["group_name"]);
                                model1.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                                model1.taxable = Convert.ToBoolean(dataReader["Taxable"]);
                                model1.calculate_gratuity = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                                listreservation_Addon.Add(model1);
                            }
                        }
                        model.reservation_addon = listreservation_Addon;
                    }
                }
            }
            return model;
        }

        public ReservationPaymentLogModel GetReservationPaymentLogbyReservationId(int ReservationId)
        {
            var model = new ReservationPaymentLogModel();

            string sql = "select PayCardCustName,PayCardExpMonth,PayCardExpYear,PayCardNumber,PayCardToken,PayCardType from ReservationV2 (nolock) where ReservationId=@ReservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var payCard = new PayCard();
                        payCard.cust_name = Convert.ToString(dataReader["PayCardCustName"]);
                        payCard.exp_month = Convert.ToString(dataReader["PayCardExpMonth"]);
                        payCard.exp_year = Convert.ToString(dataReader["PayCardExpYear"]);
                        payCard.number = Convert.ToString(dataReader["PayCardNumber"]);
                        payCard.card_token = Convert.ToString(dataReader["PayCardToken"]);
                        payCard.card_type = Convert.ToString(dataReader["PayCardType"]);

                        string cardNumber = StringHelpers.Decryption(payCard.number);
                        if (!string.IsNullOrEmpty(cardNumber))
                        {
                            payCard.card_last_four_digits = Common.Common.Right(cardNumber, 4);
                            payCard.card_first_four_digits = Common.Common.Left(cardNumber, 4);
                        }

                        model.pay_card = payCard;

                        model.reservation_payments_log = GetPayments(ReservationId, payCard);
                    }
                }
            }

            return model;
        }

        public ReservationDetailV2Model GetReservationDetailV2byReservationId(int ReservationId)
        {
            var model = new ReservationDetailV2Model();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader("GetReservationDetailV2ByIDOrBookingCode", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        model.event_id = Convert.ToInt32(dataReader["EventId"]);
                        model.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        model.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        model.seated_status = Convert.ToInt32(dataReader["SeatedStatus"]);
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.location_name = Convert.ToString(dataReader["EventLocation"]);
                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));

                        model.fee_per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.fee_type = Convert.ToInt32(dataReader["FeeType"]);

                        model.total_guests = Convert.ToInt16(dataReader["TotalGuests"]);
                        model.booking_date = ConvertFromDateTimeOffset(DateTimeOffset.Parse(Convert.ToString(dataReader["BookingDate"])));
                        model.referral_type = Convert.ToByte(dataReader["ReferralType"]);
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.status = Convert.ToByte(dataReader["Status"]);
                        model.fee_due = Convert.ToDecimal(dataReader["FeeDue"]);
                        model.amount_paid = Convert.ToDecimal(dataReader["AmountPaid"]);
                        model.region_id = Convert.ToInt32(dataReader["Appelation"]);
                        model.full_paid = (model.amount_paid >= model.fee_due);

                        model.refund_paid = Convert.ToDecimal(dataReader["RefundAmount"]);
                        model.user_image = Convert.ToString(dataReader["UserImage"]);

                        model.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        if (model.slot_id > 0)
                        {
                            model.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        }
                        else
                        {
                            model.slot_type = 2;
                        }

                        model.concierge_name = Convert.ToString(dataReader["ConciergeName"]);
                        model.referral_name = Convert.ToString(dataReader["ReferralName"]);
                        model.cancel_lead_time = Convert.ToInt32(dataReader["CancelLeadTime"]);
                        model.charge_fee = Convert.ToByte(dataReader["ChargeFee"]);

                        model.booked_by_name = Convert.ToString(dataReader["BookedByName"]);

                        string CancelTimeDesc = Convert.ToString(dataReader["CancelTime"]);

                        if (CancelTimeDesc == "Non-changeable, Non-cancellable")
                        {
                            model.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                        }
                        else if (CancelTimeDesc.IndexOf("Days") > -1)
                        {
                            try
                            {
                                int CancelTime = Convert.ToInt32(CancelTimeDesc.Replace("Days", "").Replace(" ", ""));

                                if (CancelTime > 20)
                                {
                                    model.cancel_policy = "We’re sorry, but this reservation cannot be cancelled or scheduled.";
                                }
                                else
                                {
                                    model.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                                }
                            }
                            catch
                            {
                                model.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                            }
                        }
                        else
                        {
                            model.cancel_policy = (Convert.ToString(dataReader["CancelPolicy"]).Replace("\"", "'")).Replace("[[CancelLeadTime]]", CancelTimeDesc);
                        }

                        model.affiliate_id = Convert.ToInt32(dataReader["AffiliateID"]);
                        model.discount_amount = Convert.ToDecimal(dataReader["DiscountAmt"]);
                        model.gratuity_amount = Convert.ToDecimal(dataReader["GratuityAmt"]);

                        model.delay_in_minutes = Convert.ToInt32(dataReader["DelayInMinutes"]);

                        model.user_id = Convert.ToInt32(dataReader["UserId"]);
                        model.email = Convert.ToString(dataReader["Email"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.city = Convert.ToString(dataReader["City"]);
                        model.state = Convert.ToString(dataReader["State"]);
                        model.country = Convert.ToString(dataReader["Country"]);
                        model.customer_type = Convert.ToInt32(dataReader["CustomerType"]);

                        model.phone_number = Convert.ToString(dataReader["PhoneNumber"]);

                        model.pre_assign_server_id = Convert.ToInt32(dataReader["PreAssign_Server_Id"]);

                        ReservationModel waitlistdatetime = GetWaitlistStartEndTime(model.reservation_id);

                        model.transportation_id = Convert.ToInt32(dataReader["TransportationId"]);
                        model.transportation_name = Convert.ToString(dataReader["TransportationName"]);

                        model.sales_tax = Convert.ToDecimal(dataReader["SalesTax"]);

                        model.time_zone_id = Convert.ToInt32(dataReader["timezoneid"]);
                        model.has_review = Convert.ToBoolean(dataReader["HasReview"]);
                        model.exported = Convert.ToBoolean(dataReader["Exported"]);
                        model.has_payment_decline = Convert.ToBoolean(dataReader["HasPaymentDecline"]);
                        var tableIds = Convert.ToString(dataReader["PreAssign_Table_Id"]);
                        if (!string.IsNullOrWhiteSpace(tableIds))
                        {
                            model.pre_assign_table_ids = JsonConvert.DeserializeObject<List<int>>(tableIds);
                        }

                        model.soft_assigned_tables = Convert.ToBoolean(dataReader["TablesSoftAssigned"]);
                        model.floor_plan_name = Convert.ToString(dataReader["FloorPlanName"]);
                        model.floor_plan_id = Convert.ToInt32(dataReader["FloorPlanId"]);
                        model.table_names = Convert.ToString(dataReader["TableNames"]);
                        ReservationServer reservationServer = new ReservationServer();

                        reservationServer.user_id = Convert.ToInt32(dataReader["RserverUser_Id"]);
                        reservationServer.first_name = Convert.ToString(dataReader["RserverFirstName"]);
                        reservationServer.last_name = Convert.ToString(dataReader["RserverLastName"]);
                        reservationServer.reservation_id = ReservationId;

                        model.contact_types = Convert.ToString(dataReader["ContactTypes"]);

                        model.reservation_server = reservationServer;

                        if (dataReader["CancelByDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["CancelByDate"].ToString()))
                            model.cancel_by_date = Convert.ToDateTime(dataReader["CancelByDate"]);

                        model.cancel_message = Convert.ToString(dataReader["CancelMessage"]);
                        model.timezone_name = Convert.ToString(dataReader["TimeZoneName"]);

                        List<Reservation_Addon> listreservation_Addon = new List<Reservation_Addon>();
                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                var model1 = new Reservation_Addon();
                                model1.qty = Convert.ToInt32(dataReader["Qty"]);
                                model1.sku = Convert.ToString(dataReader["Sku"]);
                                model1.name = Convert.ToString(dataReader["Name"]);
                                model1.description = Convert.ToString(dataReader["Description"]);
                                model1.cost = Convert.ToDecimal(dataReader["Cost"]);
                                model1.price = Convert.ToDecimal(dataReader["Price"]);
                                model1.category = Convert.ToInt32(dataReader["Category"]);
                                model1.item_type = Convert.ToInt32(dataReader["ItemType"]);
                                model1.image = Convert.ToString(dataReader["Image"]);
                                model1.item_id = Convert.ToInt32(dataReader["ItemId"]);
                                model1.group_item_id = Convert.ToInt32(dataReader["group_item_id"]);
                                model1.group_id = Convert.ToInt32(dataReader["group_id"]);
                                model1.group_name = Convert.ToString(dataReader["group_name"]);
                                model1.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                                model1.taxable = Convert.ToBoolean(dataReader["Taxable"]);
                                model1.calculate_gratuity = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                                listreservation_Addon.Add(model1);
                            }
                        }
                        model.reservation_addon = listreservation_Addon;                        
                    }
                }
            }
            return model;
        }

        public ReservationDetailV4Model GetReservationDetailV4byReservationId(int ReservationId)
        {
            var model = new ReservationDetailV4Model();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader("GetReservationDetailV4ByID", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        model.total_guests = Convert.ToInt16(dataReader["TotalGuests"]);
                        model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        model.fee_per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.fee_type = Convert.ToInt32(dataReader["FeeType"]);
                        model.email = Convert.ToString(dataReader["Email"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.phone_number = Convert.ToString(dataReader["PhoneNumber"]);
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.booking_guid = Convert.ToString(dataReader["BookingGUID"]);
                        model.business_phone = Convert.ToString(dataReader["businessphone"]);
                        model.member_country = Convert.ToString(dataReader["country"]);
                        model.member_name = Convert.ToString(dataReader["displayname"]);
                        model.location_Address1 = Convert.ToString(dataReader["Address1"]);
                        model.location_Address2 = Convert.ToString(dataReader["Address2"]);
                        model.location_state = Convert.ToString(dataReader["State"]);
                        model.location_city = Convert.ToString(dataReader["City"]);
                        model.location_zip = Convert.ToString(dataReader["Zip"]);
                        model.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        model.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        model.region_id = Convert.ToInt32(dataReader["Appelation"]);
                        model.member_id = Convert.ToInt32(dataReader["wineryId"]);
                        model.event_start_time = TimeSpan.Parse(Convert.ToString(dataReader["StartTime"]));

                        if (dataReader["CancelByDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["CancelByDate"].ToString()))
                            model.cancel_by_date = Convert.ToDateTime(dataReader["CancelByDate"]);

                        model.cancel_message = Convert.ToString(dataReader["CancelMessage"]);
                    }
                }
            }
            return model;
        }

        public ReservationNoteLogModel GetReservationNoteLogbyReservationId(int ReservationId)
        {
            var model = new ReservationNoteLogModel();
            int member_id = 0;
            bool covidSurveyEnabled = false;
            bool covidWaiverEnabled = false;
            var add_guest = new Additional_guests();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader("GetReservationNoteDetailByID", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.reservation_id = ReservationId;
                        model.tags = Convert.ToString(dataReader["tags"]);
                        model.user_tags = Convert.ToString(dataReader["UserTags"]);
                        model.InternalNote = Convert.ToString(dataReader["InternalNote"]);
                        model.Notes = Convert.ToString(dataReader["Note"]);
                        model.account_notes = Convert.ToString(dataReader["AccountNotes"]);
                        model.ConciergeNote = Convert.ToString(dataReader["ConciergeNote"]);
                        model.total_guests = Convert.ToInt32(dataReader["TotalGuests"]);

                        member_id = Convert.ToInt32(dataReader["wineryid"]);
                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(member_id, (int)Common.Common.SettingGroup.member);
                        covidSurveyEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_covid_survey);
                        covidWaiverEnabled = Settings.GetBoolValue(settingsGroup, Common.Common.SettingKey.member_enable_covid_waiver);

                        model.CovidSurveyEnabled = covidSurveyEnabled;
                        model.WaiverEnabled = covidWaiverEnabled;
                        model.event_id = Convert.ToInt32(dataReader["EventId"]);

                        string email = Convert.ToString(dataReader["email"]);
                        DateTime? survey_expire_date = null;
                        DateTime? modified_date = null;
                        Common.Common.RSVPPostCaptureStatus survey_status = RSVPPostCaptureStatus.NA;
                        Common.Common.RSVPPostCaptureStatus waiver_status = RSVPPostCaptureStatus.NA;


                        if (covidSurveyEnabled || covidWaiverEnabled)
                        {
                            if (!string.IsNullOrEmpty(email))
                            {
                                SurveyWaiverStatus surveyWaiverStatus = GetSurveyWaiverStatusByEmailAndMemberId(member_id, email);

                                survey_expire_date = surveyWaiverStatus.survey_expire_date;
                                survey_status = surveyWaiverStatus.survey_status;
                                waiver_status = surveyWaiverStatus.waiver_status;
                                modified_date = surveyWaiverStatus.modified_date;
                            }
                            else
                            {
                                if (covidSurveyEnabled)
                                    survey_status = RSVPPostCaptureStatus.Available;

                                if (covidWaiverEnabled)
                                    waiver_status = RSVPPostCaptureStatus.Available;
                            }
                        }
                        add_guest.first_name = Convert.ToString(dataReader["firstname"]);
                        add_guest.last_name = Convert.ToString(dataReader["lastname"]);
                        add_guest.id = 0;
                        add_guest.email = email;
                        add_guest.user_id = Convert.ToInt32(dataReader["userid"]);
                        add_guest.survey_expire_date = survey_expire_date;
                        add_guest.modified_date = modified_date;
                        add_guest.survey_status = survey_status;
                        add_guest.waiver_status = waiver_status;
                        //model.ipBrowserDetails = Convert.ToString(dataReader["BookingCode"]);
                        //model.additional_guests
                    }

                    List<AttendeeQuestion> questions = new List<AttendeeQuestion>();
                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var question = new AttendeeQuestion
                            {
                                question = Convert.ToString(dataReader["QuestionText"]),
                                answer = Convert.ToString(dataReader["Choice"])
                            };
                            questions.Add(question);
                        }

                    }
                    model.attendee_questions = questions;

                    model.additional_guests = GetRsvpGuestsbyReservationId(ReservationId, member_id, covidSurveyEnabled, covidWaiverEnabled, add_guest);
                }
            }
            return model;
        }

        public List<ReservationChangeLog> GetReservationChangeLogsbyReservationId(int ReservationId, int StatusType)
        {
            List<ReservationChangeLog> listreservationChangeLog = new List<ReservationChangeLog>();

            string sql = "select Id, NoteDate, Note as LogNote,CurrentUser from ReservationV2StatusNotes(nolock) where RefId = @ReservationId and statustype = @StatusType order by NoteDate desc";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@StatusType", StatusType));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ReservationChangeLog();
                        model.change_log_id = Convert.ToInt32(dataReader["Id"]);
                        model.reservation_status_date = Convert.ToDateTime(dataReader["NoteDate"]);
                        model.log_note = Convert.ToString(dataReader["LogNote"]);
                        model.reservation_status_user = Convert.ToString(dataReader["CurrentUser"]);
                        model.cancellation_reason = GetReservationCancellationReason(ReservationId);

                        listreservationChangeLog.Add(model);
                    }
                }
            }
            return listreservationChangeLog;
        }

        public string GetReservationCancellationReason(int ReservationId)
        {
            string Reason = "";

            string sql = "select CancellationReason from ReservationV2(nolock) where ReservationId = @ReservationId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Reason = Convert.ToString(dataReader["CancellationReason"]);                     
                    }
                }
            }
            return Reason;
        }

        public List<TicketLevels> GetTicketLevelsbyTicketOrderId(int TicketOrderId)
        {
            List<TicketLevels> listticketLevels = new List<TicketLevels>();

            string sql = "select QTY,TicketName from (select count(id) QTY,TicketName from Tickets_Order_Tickets where tickets_order_id=@TicketOrderId group by TicketName) as temp order by TicketName";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TicketOrderId", TicketOrderId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new TicketLevels();
                        model.qty = Convert.ToInt32(dataReader["QTY"]);
                        model.ticket_level_name = Convert.ToString(dataReader["TicketName"]);

                        listticketLevels.Add(model);
                    }
                }
            }
            return listticketLevels;
        }

        public static string ExtractHTML(string expr)
        {
            return System.Text.RegularExpressions.Regex.Replace(expr, "<span(.*?)>(.*?)</span>", "").Trim();
        }

        public List<ReservationChangeLog> GetReservationEmailLogsbyReservationId(int ReservationId)
        {
            List<ReservationChangeLog> listreservationChangeLog = new List<ReservationChangeLog>();

            string sql = "select Id,LogDate,LogNote,EmailRecipient,EmailType,EmailStatus from EmailLog(nolock) where RefId = @ReservationId and EmailType = 4 order by LogDate desc";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ReservationChangeLog();
                        model.change_log_id = Convert.ToInt32(dataReader["Id"]);
                        model.reservation_status_date = Convert.ToDateTime(dataReader["LogDate"]);
                        model.log_note = string.Format("{0} Email to {1} {2}", GetEmailTypeDesc((EmailType)Convert.ToInt32(dataReader["EmailType"])), Convert.ToString(dataReader["EmailRecipient"]), GetEmailStatusDesc((EmailStatus)Convert.ToInt32(dataReader["EmailStatus"])));
                        model.reservation_status_user = Convert.ToString(dataReader["EmailRecipient"]);

                        listreservationChangeLog.Add(model);
                    }
                }
            }
            return listreservationChangeLog;
        }

        public List<ReservationChangeLog> GetReservationAllEmailLogsbyReservationId(int ReservationId)
        {
            List<ReservationChangeLog> listreservationChangeLog = new List<ReservationChangeLog>();

            string sql = "select Id,LogDate,LogNote,EmailRecipient,EmailType,EmailStatus from EmailLog where RefId = @ReservationId and EmailProvider = 1 order by LogDate desc";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ReservationChangeLog();
                        model.change_log_id = Convert.ToInt32(dataReader["Id"]);
                        model.reservation_status_date = Convert.ToDateTime(dataReader["LogDate"]);
                        model.log_note = string.Format("{0} Email to {1} {2}", GetEmailTypeDesc((EmailType)Convert.ToInt32(dataReader["EmailType"])), Convert.ToString(dataReader["EmailRecipient"]), GetEmailStatusDesc((EmailStatus)Convert.ToInt32(dataReader["EmailStatus"])));
                        model.reservation_status_user = Convert.ToString(dataReader["EmailRecipient"]);

                        listreservationChangeLog.Add(model);
                    }
                }
            }
            return listreservationChangeLog;
        }

        public List<ReservationChangeLog> GetReservationEmailLogs(int ReservationId)
        {
            List<ReservationChangeLog> listreservationChangeLog = new List<ReservationChangeLog>();

            string sql = "select Id,LogDate,LogNote,EmailRecipient,EmailType,EmailStatus from EmailLog where RefId = @ReservationId and EmailType = 4 and EmailStatus <> 12 and EmailProvider = 1 order by LogDate desc";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ReservationChangeLog();
                        model.change_log_id = Convert.ToInt32(dataReader["Id"]);
                        model.reservation_status_date = Convert.ToDateTime(dataReader["LogDate"]);
                        model.log_note = string.Format("{0} Email to {1} {2}", GetEmailTypeDesc((EmailType)Convert.ToInt32(dataReader["EmailType"])), Convert.ToString(dataReader["EmailRecipient"]), GetEmailStatusDesc((EmailStatus)Convert.ToInt32(dataReader["EmailStatus"])));
                        model.reservation_status_user = Convert.ToString(dataReader["EmailRecipient"]);

                        listreservationChangeLog.Add(model);
                    }
                }
            }
            return listreservationChangeLog;
        }

        public EventRuleModel GetEventcapacity(int SlotId, int SlotType)
        {
            EventRuleModel model = new EventRuleModel();
            string sql = string.Empty;

            if (SlotType == (int)Common.SlotType.Exception)
            {
                sql = "select ex.TotalSeats,ex.StartTime,ex.EndTime,ex.EventId,ex.LocationId,e.chargefee,eventname,EmailContentID,e.WineryID,e.Description,DATEDIFF(minute, StartTime, EndTime) Duration,MeetingBehavior,TimeZoneId from EventExceptions ex join events e on ex.eventid=e.eventid join winery w on w.id=e.wineryid where ExceptionId=@SlotId";
            }
            else
            {
                sql = "select er.TotalSeats,er.StartTime,er.EndTime,er.EventId,e.LocationId,e.chargefee,eventname,EmailContentID,e.WineryID,e.Description,DATEDIFF(minute, StartTime, EndTime) Duration,MeetingBehavior,TimeZoneId from EventRules er join events e on er.eventid=e.eventid join winery w on w.id=e.wineryid where EventRuleId=@SlotId";
            }

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SlotId", SlotId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.TotalSeats = Convert.ToInt32(dataReader["TotalSeats"]);
                        model.StartTime = TimeSpan.Parse(Convert.ToString(dataReader["StartTime"]));
                        model.EndTime = TimeSpan.Parse(Convert.ToString(dataReader["EndTime"]));
                        model.EventId = Convert.ToInt32(dataReader["EventId"]);
                        model.LocationId = Convert.ToInt32(dataReader["LocationId"]);
                        model.chargefee = Convert.ToInt32(dataReader["chargefee"]);
                        model.eventname = Convert.ToString(dataReader["eventname"]);
                        model.EmailContentID = Convert.ToInt32(dataReader["EmailContentID"]);
                        model.WineryID = Convert.ToInt32(dataReader["WineryID"]);
                        model.Description = Convert.ToString(dataReader["Description"]);
                        model.Duration = Convert.ToInt32(dataReader["Duration"]);
                        model.MeetingBehavior = Convert.ToInt32(dataReader["MeetingBehavior"]);
                        model.TimeZoneId = Convert.ToInt32(dataReader["TimeZoneId"]);
                    }
                }
            }
            return model;
        }

        public List<WineryReviewViewModel> GetWineryReviewsByMemberId(int MemberId)
        {
            List<WineryReviewViewModel> list = new List<WineryReviewViewModel>();
            string sp = "GetWineryReviews";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", MemberId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        WineryReviewViewModel model = new WineryReviewViewModel();
                                 
                        model.total_count = Convert.ToInt32(dataReader["TotalCount"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.member_since = Convert.ToDateTime(dataReader["MemberSince"]);
                        model.metric_1 = Convert.ToInt32(dataReader["Metric1"]);
                        model.metric_2 = Convert.ToInt32(dataReader["Metric2"]);
                        model.metric_3 = Convert.ToInt32(dataReader["Metric3"]);
                        model.metric_4 = Convert.ToInt32(dataReader["Metric4"]);
                        model.recommend  = Convert.ToString(dataReader["Recommend"]);
                        model.star = Convert.ToDecimal(dataReader["Star"]);
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.start_date = Convert.ToDateTime(dataReader["StartDate"]);
                        model.description = Convert.ToString(dataReader["Description"]);

                        if (model.member_since != null)
                        {
                            model.member_since_format = model.member_since.ToString("M/d/yy", CultureInfo.InvariantCulture).TrimStart('0');
                        }
                        if (model.start_date != null)
                        {
                            model.start_date_format = model.start_date.ToString("M/d/yy", CultureInfo.InvariantCulture).TrimStart('0');
                        }
                        if (!String.IsNullOrWhiteSpace(model.first_name) && !String.IsNullOrWhiteSpace(model.last_name))
                        {
                            model.user_name = model.first_name + " " + model.last_name.ToCharArray()[0] + ".";
                        }

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public WineryReviews GetWineryReviews(int MemberId)
        {
            WineryReviews model = new WineryReviews();
            string sp = "GetWineryReviews";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", MemberId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.ReviewCount = Convert.ToInt32(dataReader["ReviewCount"]);
                        model.ReviewStars = Convert.ToInt32(dataReader["ReviewStars"]);
                        model.avg_review_value = Convert.ToDecimal(dataReader["AvgReviewValue"]);
                    }
                }
            }
            return model;
        }

        public ReservationTransactionDetailModel GetReservationTransactionDetail(int Reservationid)
        {
            ReservationTransactionDetailModel model = new ReservationTransactionDetailModel();
            string sql = "select r.feedue,r.TransactionFee,RefundFees,[Status] from reservationv2 (nolock) r join winery (nolock) w on r.wineryid = w.id join BillingPlans (nolock) bp on  bp.Id=w.BillingPlan where r.TransactionType in (2,3,5) and r.reservationid = @Reservationid";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Reservationid", Reservationid));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.feedue = Convert.ToDecimal(dataReader["feedue"]);
                        model.TransactionFee = Convert.ToDecimal(dataReader["TransactionFee"]);
                        model.RefundFees = Convert.ToBoolean(dataReader["RefundFees"]);
                        model.Status = Convert.ToInt32(dataReader["Status"]);
                    }
                }
            }
            return model;
        }

        public int GetBookingCountByMemberId(int MemberId)
        {
            int BookingCount = 0;
            string sql = "select count(reservationid) as BookingCount FROM ReservationV2 (nolock) where WineryId=@MemberId and [status] not in (7,8) and dbo.dateonly(BookingDate) = dbo.dateonly(getutcdate())";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", MemberId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        BookingCount = Convert.ToInt32(dataReader["BookingCount"]);
                    }
                }
            }
            return BookingCount;
        }

        public int GetBookingCountByEventId(int EventId)
        {
            int BookingCount = 0;
            string sql = "select count(reservationid) as BookingCount FROM ReservationV2 (nolock) where EventId=@EventId and [status] not in (7,8) and dbo.dateonly(BookingDate) = dbo.dateonly(getutcdate())";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        BookingCount = Convert.ToInt32(dataReader["BookingCount"]);
                    }
                }
            }
            return BookingCount;
        }

        public int GetGuestSum(int SlotId, int SlotType, DateTime reqDate, int ReservationId)
        {
            int guestSum = 0;
            string sql = "select isnull(sum(TotalGuests),0) as guestSum  from ReservationV2 (nolock) where SlotId=@SlotId and SlotType =@SlotType and dbo.dateonly(EventDate)=dbo.dateonly(@reqDate) and Status not in (2,7,8) and (ReservationId <> @ReservationId or @ReservationId = 0)";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@SlotType", SlotType));
            parameterList.Add(GetParameter("@reqDate", reqDate));
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        guestSum = Convert.ToInt32(dataReader["guestSum"]);
                    }
                }
            }
            return guestSum;
        }

        public int GetRSVPBillingPlanFOrMmeber(int memberId)
        {
            int id = 0;
            string sql = @"Select bp.Id
                            from BillingPlans bp 
                            join Winery w(nolock) on bp.Id=w.BillingPlan 
                            where w.Id=@WineryId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", memberId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        id = Convert.ToInt32(dataReader[0]);
                    }
                }
            }
            return id;

        }


        public int GetBillingPlanTypeForMember(int memberId)
        {
            int id = 0;
            string sql = @"Select bp.PlanType
                            from BillingPlans bp 
                            join Winery w(nolock) on bp.Id=w.BillingPlan 
                            where w.Id=@WineryId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", memberId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        id = Convert.ToInt32(dataReader[0]);
                    }
                }
            }
            return id;

        }

        public bool IsMailChimpModuleAvailable(int memberId)
        {
            int planType = GetBillingPlanTypeForMember(memberId);
            return (planType == (int)BillingPlanType.Enterprise || planType == (int)BillingPlanType.TablePro);

        }
        public decimal GetServiceFeePaidByGuest(int memberId, int guestCount, decimal rsvpTotal = 0, ReferralType referral_type = ReferralType.CellarPass)
        {
            decimal svcFee = 0;

            string sql = @"Select bp.TransactionType, bp.TransactionFee, bp.TransactionFeeDirect, bp.TransactionFeeWidget, bp.TransactionFeeAffiliate
                            , bp.TransactionFeeMobile, bp.TransactionFeeReferrer, bp.WineryReferralFee
                            from BillingPlans bp 
                            join Winery w(nolock) on bp.Id=w.BillingPlan 
                            where w.Id=@WineryId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", memberId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        decimal txnFee = Convert.ToDecimal(dataReader["TransactionFeeDirect"]);

                        if (referral_type == ReferralType.CellarPass)
                            txnFee = Convert.ToDecimal(dataReader["TransactionFee"]);
                        else if (referral_type == ReferralType.BackOffice)
                            txnFee = Convert.ToDecimal(dataReader["TransactionFeeDirect"]);
                        else if (referral_type == ReferralType.Widget)
                            txnFee = Convert.ToDecimal(dataReader["TransactionFeeWidget"]);
                        else if (referral_type == ReferralType.Affiliate)
                            txnFee = Convert.ToDecimal(dataReader["TransactionFeeAffiliate"]);
                        else if (referral_type == ReferralType.Mobile)
                            txnFee = Convert.ToDecimal(dataReader["TransactionFeeMobile"]);
                        else if (referral_type == ReferralType.Referrer)
                            txnFee = Convert.ToDecimal(dataReader["TransactionFeeReferrer"]);
                        else if (referral_type == ReferralType.WineryReferral)
                            txnFee = Convert.ToDecimal(dataReader["WineryReferralFee"]);

                        uc.Common.BillingPlanTransactionType tranType = (uc.Common.BillingPlanTransactionType)Convert.ToInt32(dataReader["TransactionType"].ToString());

                        if (tranType == uc.Common.BillingPlanTransactionType.PerPersonPaidByGuest || tranType == uc.Common.BillingPlanTransactionType.PerRsvpPaidByGuest)
                        {
                            svcFee = txnFee;
                            if (tranType == uc.Common.BillingPlanTransactionType.PerPersonPaidByGuest)
                                svcFee = svcFee * guestCount;
                        }
                        else if (tranType == uc.Common.BillingPlanTransactionType.PercentByGuest)
                        {
                            svcFee = (txnFee * rsvpTotal) / 100;
                        }

                    }
                }
            }
            return svcFee;

        }
        public EventModel GetEventById(int EventId,int WineryId = 0)
        {
            var model = new EventModel();

            string sql = "GetEventDetailByEventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));
            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.EventID = Convert.ToInt32(dataReader["Eventid"]);
                        model.LeadTime = Convert.ToInt32(dataReader["LeadTime"]);
                        model.EventStatus = Convert.ToBoolean(dataReader["Status"]);
                        model.MemberID = Convert.ToInt32(dataReader["WineryID"]);
                        model.StartDate = Convert.ToDateTime(dataReader["StartDate"]);
                        model.EndDate = Convert.ToDateTime(dataReader["EndDate"]);
                        model.ChargeFee = Convert.ToInt32(dataReader["ChargeFee"]);
                        model.ChargeSalesTax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        model.TaxGratuity = Convert.ToBoolean(dataReader["TaxGratuity"]);
                        model.LocationId = Convert.ToInt32(dataReader["LocationId"]);
                        model.EventName = Convert.ToString(dataReader["EventName"]);
                        model.EmailContentID = Convert.ToInt32(dataReader["EmailContentID"]);
                        model.MemberBenefit = (uc.Common.DiscountType)Convert.ToInt32(dataReader["MemberBenefit"]);
                        model.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        model.FeeType = Convert.ToInt32(dataReader["FeeType"]);
                        model.Cost = Convert.ToDecimal(dataReader["Cost"]);
                        model.MaxLeadTime = Convert.ToInt32(dataReader["MaxLeadTimeInDays"]);
                        model.EventTypeId = Convert.ToInt32(dataReader["EventTypeId"]);
                        model.MeetingBehavior = Convert.ToInt32(dataReader["MeetingBehavior"]);
                        model.member_benefit_required = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                        model.account_type_required = Convert.ToBoolean(dataReader["AcountTypeReq"]);
                    }
                }
            }
            return model;
        }

        public ReservationDetailModel IsReservationConflict(int ReservationId, int UserId, DateTime EventDate, TimeSpan StartTime, TimeSpan EndTime, int SlotId = 0, int SlotType = 0)
        {
            var model = new ReservationDetailModel();

            string sql = "GetRSVPConflict";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@EventDate", EventDate));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@EndTime", EndTime));
            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@SlotType", SlotType));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        model.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.location_name = Convert.ToString(dataReader["WineryName"]);
                        model.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                    }
                }
            }
            return model;
        }

        public List<LocationModel> GetLocationByWineryID(int WineryID, bool TablePro = false, bool active_only = false)
        {
            var model = new List<LocationModel>();

            string sql = "GetLocationsByMember";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));
            parameterList.Add(GetParameter("@TablePro", TablePro));
            parameterList.Add(GetParameter("@active_only", active_only));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        LocationModel locationModel = new LocationModel();
                        locationModel.location_id = Convert.ToInt32(dataReader["Id"]);
                        locationModel.destination_name = Convert.ToString(dataReader["DestinationName"]);
                        locationModel.location_name = Convert.ToString(dataReader["LocationName"]);
                        locationModel.technical_name = Convert.ToString(dataReader["TechnicalName"]);
                        locationModel.description = Convert.ToString(dataReader["Description"]);
                        locationModel.geo_latitude = Convert.ToString(dataReader["GeoLatitude"]);
                        locationModel.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                        locationModel.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                        locationModel.location_timezone = Convert.ToInt32(dataReader["TimeZoneId"]);
                        locationModel.location_timezone_offset = Times.GetOffsetMinutes((Times.TimeZone)locationModel.location_timezone);
                        locationModel.seating_reset_time = Convert.ToString(dataReader["SeatingResetTime"]);
                        locationModel.room_height = Convert.ToInt32(dataReader["RoomHeight"]);
                        locationModel.room_width = Convert.ToInt32(dataReader["RoomWidth"]);

                        if (string.IsNullOrEmpty(locationModel.seating_reset_time))
                        {
                            locationModel.seating_reset_time = "04:00:00";
                        }

                        Model.Address addr = new Model.Address();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.zip_code = Convert.ToString(dataReader["Zip"]);
                        addr.country = Convert.ToString(dataReader["country"]);
                        locationModel.address = addr;
                        model.Add(locationModel);
                    }
                }
            }
            return model;
        }

        public int GetCalendarNoteLocationIdByWineryID(int WineryID)
        {
            int LocationId = 0;

            string sql = "select Id from location where wineryid=@WineryID and iscalendarnote=1";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        LocationId = Convert.ToInt32(dataReader["Id"]);
                    }
                }
            }
            return LocationId;
        }
        public LocationModel GetLocationByID(int ID)
        {
            LocationModel locationModel = new LocationModel();
            string sql = "select Id,DestinationName,LocationName,TechnicalName,Description,GeoLatitude,GeoLongitude,SortOrder,TimeZoneId,SeatingResetTime,";
            sql += "ServerMode,Address1,Address2,City,State,Zip,WineryID,isnull(Country,'') as country from Location (nolock) where ID = @ID";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ID", ID));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        locationModel.location_id = Convert.ToInt32(dataReader["Id"]);
                        locationModel.destination_name = Convert.ToString(dataReader["DestinationName"]);
                        locationModel.location_name = Convert.ToString(dataReader["LocationName"]);
                        locationModel.technical_name = Convert.ToString(dataReader["TechnicalName"]);
                        locationModel.description = Convert.ToString(dataReader["Description"]);
                        locationModel.geo_latitude = Convert.ToString(dataReader["GeoLatitude"]);
                        locationModel.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                        locationModel.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                        locationModel.location_timezone = Convert.ToInt32(dataReader["TimeZoneId"]);
                        locationModel.location_timezone_offset = Common.Times.GetOffsetMinutes((Times.TimeZone)locationModel.location_timezone);
                        locationModel.seating_reset_time = Convert.ToString(dataReader["SeatingResetTime"]);
                        locationModel.member_id = Convert.ToInt32(dataReader["WineryID"]);

                        if (string.IsNullOrEmpty(locationModel.seating_reset_time))
                        {
                            locationModel.seating_reset_time = "04:00:00";
                        }


                        Model.Address addr = new Model.Address();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.zip_code = Convert.ToString(dataReader["Zip"]);
                        addr.country = Convert.ToString(dataReader["country"]);
                        locationModel.address = addr;
                    }
                }
            }
            return locationModel;
        }

        public LocationMapModel GetLocationMapDataByID(int ID)
        {
            LocationMapModel locationModel = new LocationMapModel();
            string sql = "select Id,DestinationName,LocationName,GeoLatitude,GeoLongitude,MapAndDirectionsURL from Location where ID = @ID";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ID", ID));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        locationModel.location_id = Convert.ToInt32(dataReader["Id"]);
                        locationModel.destination_name = Convert.ToString(dataReader["DestinationName"]);
                        locationModel.location_name = Convert.ToString(dataReader["LocationName"]);

                        locationModel.geo_latitude = Convert.ToString(dataReader["GeoLatitude"]);
                        locationModel.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                        locationModel.map_and_directions_url = Convert.ToString(dataReader["MapAndDirectionsURL"]);

                    }
                }
            }
            return locationModel;
        }

        public LocationModel GetLocationAddressByReservationId(int ReservationId)
        {
            LocationModel locationModel = new LocationModel();
            Model.Address addr = new Model.Address();

            string sql = "select l.Address1,l.Address2,l.City,l.State,l.Zip,l.country from reservationv2 (nolock) r join location (nolock) l on r.locationid = l.Id where r.reservationid = @ReservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.zip_code = Convert.ToString(dataReader["Zip"]);
                        addr.country = Convert.ToString(dataReader["country"]);
                    }
                }
            }

            locationModel.address = addr;

            return locationModel;
        }

        public int GetOffsetMinutes(int LocationID)
        {
            int OffsetMinutes = 0;
            string sql = "select TimeZoneId from Location where Id = @LocationID";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@LocationID", LocationID));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        LocationModel locationModel = new LocationModel();

                        locationModel.location_timezone = Convert.ToInt32(dataReader["TimeZoneId"]);
                        OffsetMinutes = Convert.ToInt32(Common.Times.GetOffsetMinutes((Common.Times.TimeZone)locationModel.location_timezone));

                    }
                }
            }
            return OffsetMinutes;
        }

        public int GetOffsetMinutesByFloorPlanId(int FloorPlanId)
        {
            int OffsetMinutes = 0;
            string sql = "select top 1 TimeZoneId from Location (nolock) l join floor_plan (nolock) fp on l.Id = fp.locationid where fp.Id = @FloorPlanId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@FloorPlanId", FloorPlanId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        LocationModel locationModel = new LocationModel();

                        locationModel.location_timezone = Convert.ToInt32(dataReader["TimeZoneId"]);
                        OffsetMinutes = Convert.ToInt32(Common.Times.GetOffsetMinutes((Common.Times.TimeZone)locationModel.location_timezone));

                    }
                }
            }
            return OffsetMinutes;
        }

        public int GetLocationIdByFloorPlanId(int FloorPlanId)
        {
            int LocationId = 0;
            string sql = "select LocationID from floor_plan (nolock) where Id = @FloorPlanId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@FloorPlanId", FloorPlanId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        LocationId = Convert.ToInt32(dataReader["LocationID"]);
                    }
                }
            }
            return LocationId;
        }

        public List<int> GetRsvpLocationIdByFloorPlanId(int[] floor_plan_ids)
        {
            List<int> list = new List<int>();

            foreach (var item in floor_plan_ids)
            {
                string sql = "select distinct LocationID from reservationv2 (nolock) where FloorPlanId = @FloorPlanId";

                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@FloorPlanId", item));

                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            list.Add(Convert.ToInt32(dataReader["LocationID"]));
                        }
                    }
                }
            }

            return list;
        }

        public EmailReservationDetailsModel GetReservationEmailDataByReservationId(int ReservationId, int UserId, string bookingCode)
        {
            //Common.ReferralType RsvpType = Common.ReferralType.BackOffice;
            var emailReservationDetails = new EmailReservationDetailsModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@bookingCode", bookingCode));
            using (DbDataReader dataReader = GetDataReader("GetReservationEmailDataByReservationId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        emailReservationDetails.referralType = (ReferralType)Convert.ToInt32(dataReader["ReferralType"]);
                        emailReservationDetails.ReservationId = Convert.ToInt32(dataReader["ReservationId"]);
                        emailReservationDetails.TimeZoneId = Convert.ToInt32(dataReader["TimeZoneId"]);
                        emailReservationDetails.WineryID = Convert.ToInt32(dataReader["WineryId"]);
                        DateTimeOffset offsetDate = (DateTimeOffset)(dataReader["BookingDate"]);
                        DateTime regularDate;
                        //offsetDate = DateTimeOffset.Now;
                        regularDate = offsetDate.DateTime;
                        emailReservationDetails.BookingDate = regularDate;
                        emailReservationDetails.GuestName = Convert.ToString(dataReader["GuestName"]);
                        emailReservationDetails.GuestEmail = Convert.ToString(dataReader["GuestEmail"]);
                        emailReservationDetails.GuestPhone = Convert.ToString(dataReader["GuestPhone"]);
                        emailReservationDetails.GuestWPhone = Convert.ToString(dataReader["GuestWPhone"]);
                        emailReservationDetails.GuestAddress1 = Convert.ToString(dataReader["GuestAddress1"]);
                        emailReservationDetails.GuestAddress2 = Convert.ToString(dataReader["GuestAddress2"]);
                        emailReservationDetails.GuestCity = Convert.ToString(dataReader["GuestCity"]);
                        emailReservationDetails.GuestState = Convert.ToString(dataReader["GuestState"]);
                        emailReservationDetails.GuestZipCode = Convert.ToString(dataReader["GuestZipCode"]);
                        emailReservationDetails.Fee = Convert.ToDecimal(dataReader["Fee"]);
                        emailReservationDetails.ChargeFee = Convert.ToInt32(dataReader["ChargeFee"]);
                        emailReservationDetails.FeePaid = Convert.ToDecimal(dataReader["FeePaid"]);
                        emailReservationDetails.GuestCount = Convert.ToInt16(dataReader["GuestCount"]);
                        emailReservationDetails.MemberName = Convert.ToString(dataReader["MemberName"]);
                        emailReservationDetails.CancellationReason = Convert.ToString(dataReader["CancellationReason"]);

                        emailReservationDetails.MemberAddress1 = Convert.ToString(dataReader["MemberAddress1"]);
                        emailReservationDetails.MemberAddress2 = Convert.ToString(dataReader["MemberAddress2"]);
                        emailReservationDetails.MemberCity = Convert.ToString(dataReader["MemberCity"]);
                        emailReservationDetails.MemberState = Convert.ToString(dataReader["MemberState"]);
                        emailReservationDetails.MemberZipCode = Convert.ToString(dataReader["MemberZipCode"]);
                        emailReservationDetails.EventDate = Convert.ToDateTime(dataReader["EventDate"]);
                        emailReservationDetails.StartTime = TimeSpan.Parse(Convert.ToString(dataReader["StartTime"]));
                        emailReservationDetails.EndTime = TimeSpan.Parse(Convert.ToString(dataReader["EndTime"]));
                        emailReservationDetails.EventName = Convert.ToString(dataReader["EventName"]);
                        emailReservationDetails.EventLocation = Convert.ToString(dataReader["EventLocation"]);
                        emailReservationDetails.Notes = Convert.ToString(dataReader["Notes"]);
                        emailReservationDetails.Status = Convert.ToInt32(dataReader["Status"]);
                        emailReservationDetails.InternalNote = Convert.ToString(dataReader["InternalNote"]);
                        emailReservationDetails.ConciergeNote = Convert.ToString(dataReader["ConciergeNote"]);
                        emailReservationDetails.BookedById = Convert.ToInt32(dataReader["BookedById"]);
                        emailReservationDetails.CancelLeadTime = Convert.ToInt32(dataReader["CancelLeadTime"]);
                        emailReservationDetails.EmailContentID = Convert.ToInt32(dataReader["EmailContentID"]);
                        emailReservationDetails.EmailTemplateID = Convert.ToInt32(dataReader["EmailTemplateID"]);
                        emailReservationDetails.Host = Convert.ToString(dataReader["Host"]);
                        emailReservationDetails.AffiliateID = Convert.ToInt32(dataReader["AffiliateID"]);
                        emailReservationDetails.Content = Convert.ToString(dataReader["Content"]);
                        emailReservationDetails.DestinationName = Convert.ToString(dataReader["DestinationName"]);
                        emailReservationDetails.locAddress1 = Convert.ToString(dataReader["locAddress1"]);
                        emailReservationDetails.locAddress2 = Convert.ToString(dataReader["locAddress2"]);
                        emailReservationDetails.MapAndDirectionsURL = Convert.ToString(dataReader["MapAndDirectionsURL"]);
                        emailReservationDetails.locCity = Convert.ToString(dataReader["locCity"]);
                        emailReservationDetails.locState = Convert.ToString(dataReader["locState"]);
                        emailReservationDetails.locZip = Convert.ToString(dataReader["locZip"]);
                        emailReservationDetails.BookingCode = Convert.ToString(dataReader["BookingCode"]);
                        emailReservationDetails.ProfileUrl = Convert.ToString(dataReader["PurchaseURL"]);
                        emailReservationDetails.FeePerPerson = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        emailReservationDetails.SalesTax = Convert.ToDecimal(dataReader["SalesTax"]);
                        emailReservationDetails.GratuityAmount = Convert.ToDecimal(dataReader["GratuityAmount"]);
                        emailReservationDetails.BookingGUID = Convert.ToString(dataReader["BookingGUID"]);
                        emailReservationDetails.slotid = Convert.ToInt32(dataReader["slotid"]);
                        emailReservationDetails.slottype = Convert.ToInt32(dataReader["slottype"]);
                        emailReservationDetails.ItineraryGUID = Convert.ToString(dataReader["ItineraryGUID"]);
                        emailReservationDetails.EventId = Convert.ToInt32(dataReader["EventId"]);
                        emailReservationDetails.CancellationUsers = Convert.ToString(dataReader["CancellationUsers"]);
                        emailReservationDetails.ConfirmationUsers = Convert.ToString(dataReader["ConfirmationUsers"]);
                        emailReservationDetails.region_id = Convert.ToInt32(dataReader["Appelation"]);

                        if (dataReader["CancelByDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["CancelByDate"].ToString()))
                            emailReservationDetails.cancel_by_date = Convert.ToDateTime(dataReader["CancelByDate"]);

                        emailReservationDetails.cancel_message = Convert.ToString(dataReader["CancelMessage"]);

                        emailReservationDetails.EventTypeId = Convert.ToInt32(dataReader["EventTypeId"]);

                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(emailReservationDetails.WineryID, (int)Common.Common.SettingGroup.member);

                        emailReservationDetails.MemberEmail = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_rsvp_contact_email);
                        emailReservationDetails.MemberPhone = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_rsvp_contact_phone);

                        if (emailReservationDetails.MemberPhone.Length == 0)
                            emailReservationDetails.MemberPhone = Convert.ToString(dataReader["MemberPhone"]);

                        if (emailReservationDetails.MemberEmail.Length == 0)
                            emailReservationDetails.MemberEmail = Convert.ToString(dataReader["MemberEmail"]);

                        if (emailReservationDetails.EventId > 0)
                        {
                            emailReservationDetails.EventConfirmationMessage = Convert.ToString(dataReader["EventConfirmationMessage"]);
                            emailReservationDetails.EventCancellationMessage = Convert.ToString(dataReader["EventCancellationMessage"]);
                        }
                        else
                        {
                            if (emailReservationDetails.EmailContentID > 0)
                                emailReservationDetails.EventConfirmationMessage = Settings.GetStrValue(settingsGroup, (Common.Common.SettingKey)emailReservationDetails.EmailContentID);
                            
                            if (string.IsNullOrEmpty(emailReservationDetails.EventConfirmationMessage))
                                emailReservationDetails.EventConfirmationMessage = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_confirmation_message);

                            emailReservationDetails.EventCancellationMessage = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_Pvt_rsvp_cancellation_message);
                        }

                        emailReservationDetails.LocationId = Convert.ToInt32(dataReader["LocationId"]);
                        emailReservationDetails.ReservationInviteExpirationDateTime = Convert.ToDateTime(dataReader["ReservationInviteExpirationDateTime"]);
                        emailReservationDetails.HasInvite = Convert.ToBoolean(dataReader["HasInvite"]);
                        emailReservationDetails.timezone_name = Convert.ToString(dataReader["TimeZoneName"]);
                    }

                    List<AttendeeQuestion> questions = new List<AttendeeQuestion>();
                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var question = new AttendeeQuestion
                            {
                                question = Convert.ToString(dataReader["QuestionText"]),
                                answer = Convert.ToString(dataReader["Choice"])
                            };
                            questions.Add(question);

                        }

                    }
                    emailReservationDetails.attendee_questions = questions;
                }
            }
            return emailReservationDetails;
        }

        public List<AddOnModel> GetReservationAddOnItems(int ReservationId)
        {
            var addOnsList = new List<AddOnModel>();
            var parameterList = new List<DbParameter>();
            string sql = "select * from ReservationV2_AddOn (nolock) where Reservation_Id=@ReservationId order by Name asc";
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new AddOnModel();
                        model.Id = Convert.ToInt32(dataReader["ItemId"]);
                        model.Active = true;
                        model.Sku = Convert.ToString(dataReader["Sku"]);
                        model.Name = Convert.ToString(dataReader["Name"]);
                        model.Desc = Convert.ToString(dataReader["Description"]);
                        model.Cost = Convert.ToDecimal(dataReader["Cost"]);
                        model.Price = Convert.ToDecimal(dataReader["AddOnsPrice"]);
                        model.Taxable = Convert.ToBoolean(dataReader["Taxable"]);
                        model.Image = Convert.ToString(dataReader["Image"]);
                        model.Category = Convert.ToInt32(dataReader["Category"]);
                        model.ItemType = Convert.ToInt32(dataReader["ItemType"]);
                        model.Qty = Convert.ToInt32(dataReader["Qty"]);
                        model.PurchaseId = Convert.ToInt32(dataReader["Id"]);
                        model.Taxable = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                        addOnsList.Add(model);
                    }
                }
            }
            return addOnsList;
        }

        public string GetGuestAttending(int ReservationId,string rsvpGuestName)
        {
            string guestsAttending = rsvpGuestName + ", ";
            var parameterList = new List<DbParameter>();
            string sql = "select GuestName from RsvpGuestsDetail where ReservationId=@ReservationId order by id";
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        guestsAttending += Convert.ToString(dataReader["GuestName"]) + ", ";
                    }
                }

                if (guestsAttending.Length > 0)
                {
                    if (guestsAttending.EndsWith(","))
                    {
                        guestsAttending = guestsAttending.Remove(guestsAttending.Length - 1);
                    }
                }
            }
            return guestsAttending;
        }

        public UserModel GetUser(int UserId)
        {
            var user = new UserModel();
            var parameterList = new List<DbParameter>();
            string sql = "select UserName,FirstName,LastName,CompanyName from [User] where Id=@UserId";
            parameterList.Add(GetParameter("@UserId", UserId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        user.AffiliateEmail = Convert.ToString(dataReader["UserName"]);
                        user.AffiliateName = Convert.ToString(dataReader["FirstName"]) + " " + Convert.ToString(dataReader["LastName"]);
                        user.AffiliateCompany = Convert.ToString(dataReader["CompanyName"]);
                    }
                }
            }
            return user;
        }

        public List<string> GetPaymentStatusV2byReservationId(int ReservationId)
        {
            List<string> PaymentStatus = new List<string>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader("GetPaymentStatusV2byReservationId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        PaymentStatus.Add(Convert.ToString(dataReader["PayStatus"]));
                        //if (!string.IsNullOrEmpty(PaymentStatus))
                        //    PaymentStatus = PaymentStatus + "<br>";

                        //PaymentStatus = PaymentStatus + Convert.ToString(dataReader["PayStatus"]);
                    }
                }
            }
            return PaymentStatus;
        }

        public bool UpdateReservationV2Status(int ReservationId, int status, string username, int ActiveWineryId, int DelayInMinutes)
        {
            int retval = 0;
            //string sql = "update ReservationV2 set Status=@Status,StatusChangeDate=@StatusChangeDate,StatusChangeByUser=@StatusChangeByUser,DelayInMinutes=@DelayInMinutes";
            //sql += " where ReservationId = @ReservationId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@Status", status));
            parameterList.Add(GetParameter("@StatusChangeDate", DateTime.UtcNow));
            parameterList.Add(GetParameter("@StatusChangeByUser", username));
            parameterList.Add(GetParameter("@DelayInMinutes", DelayInMinutes));
            retval = ExecuteNonQuery("UpdateReservationV2Status", parameterList, CommandType.StoredProcedure);

            return retval > 0;
        }

        public Tuple<int, int, DateTime, int, int> GetReservationDataForStatusUpdate(int ReservationId)
        {
            int SlotId = 0, SlotType = 0;
            DateTime EventDate = default(DateTime);
            int LocationId = 0;
            int userid = 0;

            string sql = "select LocationId,SlotId,SlotType,EventDate,userid from ReservationV2 (nolock)";
            sql += " where ReservationId = @ReservationId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        SlotId = Convert.ToInt32(dataReader["SlotId"]);
                        SlotType = Convert.ToInt32(dataReader["SlotType"]);
                        EventDate = Convert.ToDateTime(dataReader["EventDate"]);
                        LocationId = Convert.ToInt32(dataReader["LocationId"]);
                        userid = Convert.ToInt32(dataReader["userid"]);
                    }
                }
            }
            return Tuple.Create(SlotId, SlotType, EventDate, LocationId, userid);
        }

        public int AutoCloseEventRule(int SlotId, int SlotType, int ReservationId, DateTime EventDate, string UserName, int userid)
        {
            int ret = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SlotId", SlotId));
            string sql = string.Empty;
            if (SlotType == 0)
            {
                sql = "select MaxOrders,LimitByMaxOrders from EventRules er join Events e on e.EventId = er.EventId where EventRuleId=@SlotId";

            }
            else
            {
                parameterList.Add(GetParameter("@EventDate", EventDate));
                sql = "select top 1 er.MaxOrders,LimitByMaxOrders from EventRules er join EventExceptions ex on er.EventRuleId=ex.EventRuleId join events e on e.EventId = er.EventId where ex.ExceptionId=@SlotId and dbo.DateOnly(ex.ExceptionDate) = dbo.DateOnly(@EventDate)";
            }

            Int32 MaxOrders = 0;
            bool LimitByMaxOrders = false;
            int rsvpCount = 0;
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        MaxOrders = Convert.ToInt32(dataReader["MaxOrders"]);
                        LimitByMaxOrders = Convert.ToBoolean(dataReader["LimitByMaxOrders"]);
                    }
                }
            }

            if (LimitByMaxOrders == true)
            {
                rsvpCount = GetReservationCountByslotId(SlotId, SlotType, EventDate);

                if (rsvpCount == MaxOrders)
                    ret = MoveEventRuleReservation(SlotId, SlotType, ReservationId, EventDate, UserName, userid);
            }

            return ret;
        }

        public int GetReservationCountByslotId(int SlotId, int SlotType, DateTime EventDate)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@SlotType", SlotType));
            parameterList.Add(GetParameter("@eventDate", EventDate));
            string sql = "select count(reservationid) rsvpCount from ReservationV2 (nolock) where EventDate = @eventDate and SlotId = @SlotId and SlotType = @SlotType and [Status] not in (2,7,8)";

            int rsvpCount = 0;
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        rsvpCount = Convert.ToInt32(dataReader["rsvpCount"]);
                    }
                }
            }

            return rsvpCount;
        }

        public int MoveEventRuleReservation(int SlotId, int SlotType, int ReservationId, DateTime EventDate, string UserName, int userid)
        {
            int TimeZoneId = GetTimeZonebyReservationId(ReservationId);
            DateTime currentDatetime = Times.ToTimeZoneTime(DateTime.UtcNow, (Times.TimeZone)TimeZoneId);

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@SlotType", SlotType));
            parameterList.Add(GetParameter("@eventDate", EventDate));
            parameterList.Add(GetParameter("@Note", "Event modified by " + UserName + " on " + currentDatetime.ToString("MM/dd/yyyy hh:mm tt")));
            parameterList.Add(GetParameter("@CreatedBy", userid));
            parameterList.Add(GetParameter("@ReservationID", ReservationId));

            int retvalue = Convert.ToInt32(ExecuteScalar("AutoCloseEventRule", parameterList));
            return retvalue;
        }

        public bool UndoAutoCloseEventRule(int SlotId, int SlotType, DateTime EventDate, string UserName, bool AutoClose = false, int userid = 0)
        {
            bool ret = false;
            //string sql = "select reservationid from ReservationV2";
            //sql += " where SlotId=@SlotId and SlotType=@SlotType and dbo.DateOnly(EventDate)=dbo.DateOnly(@EventDate) and Status not in (2,7,8)";
            //var parameterList = new List<DbParameter>();
            //parameterList.Add(GetParameter("@SlotType", SlotType));
            //parameterList.Add(GetParameter("@SlotId", SlotId));
            //parameterList.Add(GetParameter("@EventDate", EventDate));
            //using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            //{
            //    if (dataReader != null && dataReader.HasRows)
            //    {
            //        while (dataReader.Read())
            //        {
            //            return true;
            //        }
            //    }
            //}
            clsEventExceptions(SlotId, SlotType, EventDate, UserName, AutoClose, userid);
            return ret;
        }

        public bool clsEventExceptions(int SlotId, int SlotType, DateTime EventDate, string UserName, bool AutoClose = false, int userid = 0)
        {
            int ExceptionId = 0, Status = 0, EventId = 0, CreatedBy = 0, EventRuleId = 0;
            if (SlotType == 0)
            {
                string sql = "select ExceptionId,Status,CreatedBy,EventRuleId, EventId from EventExceptions";
                sql += " where EventRuleId=@EventRuleId and dbo.DateOnly(ExceptionDate)=dbo.DateOnly(@ExceptionDate)";
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@EventRuleId", SlotId));
                parameterList.Add(GetParameter("@ExceptionDate", EventDate));
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            ExceptionId = Convert.ToInt32(dataReader["ExceptionId"]);
                            Status = Convert.ToInt32(dataReader["Status"]);
                            EventRuleId = Convert.ToInt32(dataReader["EventRuleId"]);
                            EventId = Convert.ToInt32(dataReader["EventId"]);
                        }
                    }
                }
            }
            else
            {
                string sql = "select ExceptionId,Status,CreatedBy,EventRuleId, EventId from EventExceptions";
                sql += " where ExceptionId=@ExceptionId";
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@ExceptionId", SlotId));
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            ExceptionId = Convert.ToInt32(dataReader["ExceptionId"]);
                            Status = Convert.ToInt32(dataReader["Status"]);
                            CreatedBy = Convert.ToInt32(dataReader["CreatedBy"]);
                            EventRuleId = Convert.ToInt32(dataReader["EventRuleId"]);
                            EventId = Convert.ToInt32(dataReader["EventId"]);
                        }
                    }
                }
            }

            try
            {
                if (ExceptionId > 0 && Status == 2)
                {
                    UpdateEventException(ExceptionId, UserName, userid, EventRuleId, EventId);
                }
            }
            catch (Exception ex)
            { }
            return true;
        }

        public bool UpdateEventException(int ExceptionId, string UserName, int CreatedBy, int EventRuleId, int EventId)
        {
            int TimeZoneId = GetTimeZonebyEventId(EventId);
            DateTime currentDatetime = Times.ToTimeZoneTime(DateTime.UtcNow, (Times.TimeZone)TimeZoneId);

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ExceptionId", ExceptionId));
            parameterList.Add(GetParameter("@ExceptionType", "Open Event"));
            parameterList.Add(GetParameter("@Status", 1));
            parameterList.Add(GetParameter("@Note", "Event modified by " + UserName + " on " + currentDatetime.ToString("MM/dd/yyyy hh:mm tt")));
            int retvalue = ExecuteNonQuery("UpdateEventExceptions", parameterList, CommandType.StoredProcedure);

            return retvalue > 0;
        }

        //public bool MarkYelpOrderCanceled(string yelpOrderId, string reason, string YelpResult = "")
        //{
        //    bool cancelled = false;
        //    try
        //    {
        //        dynamic client = new RestClient("https://partner-api.yelp.com/checkout/v2/");
        //        client.Authenticator = new RestSharp.Authenticators.HttpBasicAuthenticator("hi_cellarpass", "y;6KiF1e'W7`ky{nq/N)>K~x");
        //        dynamic request = new RestRequest(string.Format("orders/{0}/cancels", yelpOrderId), Method.POST);
        //        request.RequestFormat = DataFormat.Json;

        //        request.AddParameter("application/json", "{}", ParameterType.RequestBody);

        //        //request.AddBody(json)

        //        IRestResponse response = client.Execute(request);

        //        string responseContent = response.Content;

        //        //Pass back to byRef
        //        YelpResult = responseContent;

        //        //Create A Dictionary of values
        //        Dictionary<string, object> jsonBody = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);

        //        //Get Order ID from Yelp
        //        if (jsonBody.ContainsKey("yelp_order_id"))
        //        {
        //            cancelled = true;
        //        }
        //        else
        //        {
        //            //Debug
        //            //Log.InsertLog(Log.LogType.Debug, "ThirdPartyLib.Yelp.MarkYelpOrderCanceled", string.Format("Yelp Order Id:{0}, Cancelled = False, {1}", responseContent));
        //        }

        //        //Debug
        //        //Log.InsertLog(Log.LogType.Debug, "ThirdPartyLib.Yelp.MarkYelpOrderCanceled", string.Format("Yelp Order Id:{0}, Reason:{1}", yelpOrderId, reason));

        //    }
        //    catch (Exception ex)
        //    {
        //        //Log.InsertLog(Log.LogType.Debug, "ThirdPartyLib.Yelp.MarkYelpOrderCanceled", ex.StackTrace.ToString);
        //    }

        //    return cancelled;

        //}

        public bool UpdateItineraryAndRSVPStatus(List<int> reservationIds, int ticketOrderId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationIds", String.Join(",", reservationIds.ToArray())));
            parameterList.Add(GetParameter("@TicketOrderId", ticketOrderId));

            int retvalue = ExecuteNonQuery("UpdateRSVPandItineraryStatus", parameterList, CommandType.StoredProcedure);

            return retvalue > 0;
        }

        public bool CancelRSVPForItinerary(List<int> reservationIds)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationIds", String.Join(",", reservationIds.ToArray())));

            int retvalue = ExecuteNonQuery("CancelRSVPForItinerary", parameterList, CommandType.StoredProcedure);

            return retvalue > 0;
        }

        public bool SaveReservationV2StatusNotes(int refid, int reservationstatus, string note, string currentuser, int wineryid, int statustype)
        {
            //string sql = "insert into ReservationV2StatusNotes(RefId, ReservationStatus, Note, NoteDate, CurrentUser, WineryID, StatusType) values(@RefId, @ReservationStatus, @Note, @NoteDate, @CurrentUser, @WineryID, @StatusType)";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@RefId", refid));
            parameterList.Add(GetParameter("@ReservationStatus", reservationstatus));
            parameterList.Add(GetParameter("@Note", note));
            parameterList.Add(GetParameter("@NoteDate", DateTime.UtcNow));
            parameterList.Add(GetParameter("@CurrentUser", currentuser));
            parameterList.Add(GetParameter("@WineryID", wineryid));
            parameterList.Add(GetParameter("@StatusType", statustype));

            int retvalue = ExecuteNonQuery("InsertReservationV2StatusNotes", parameterList, CommandType.StoredProcedure);

            return retvalue > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReservationID"></param>
        /// <param name="ReservationStatus"></param>
        /// <param name="WineryId"></param>
        /// <param name="username"></param>
        /// <param name="IsModifed"></param>
        /// <param name="DelayInMinutes"></param>
        /// <param name="OldTotalGuests"></param>
        /// <param name="NewTotalGuests"></param>
        /// <param name="customNote"></param>
        /// <param name="TaxVoided"></param>
        /// <returns></returns>
        public string ReservationV2StatusNote_Create(int ReservationID, int ReservationStatus, int WineryId = 0, string strUserName = "", bool IsModifed = false, int DelayInMinutes = 0, int OldTotalGuests = 0, int NewTotalGuests = 0, string customNote = "", bool TaxVoided = false, bool IsConcierge = false)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            var inotes = new ReservationStatusNotes();
            inotes.RefId = ReservationID;
            string InternalNotes = "";
            //string strUserName = "NA";

            //if (!object.ReferenceEquals(username.Trim(), string.Empty))
            //{
            //    strUserName = username.Trim();
            //}

            string rsvpStatusColor = "";

            try
            {
                rsvpStatusColor = Common.Common.GetRsvpStatusColor().Where(f => f.ID == ReservationStatus.ToString()).Select(f => f.Name).FirstOrDefault();

            }
            catch (Exception ex)
            {
            }

            if (ReservationStatus == (int)Email.ReservationStatus.Cancelled)
            {
                if (IsConcierge)
                    InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> Cancelled by {2} (CON)", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName);
                else
                    InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> CANCELLED - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName);
            }
            else if (ReservationStatus == (int)Email.ReservationStatus.Completed)
            {
                InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> CHECKED-IN - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName);
            }
            else if (ReservationStatus == (int)Email.ReservationStatus.GuestDelayed)
            {
                if (DelayInMinutes > 0)
                {
                    InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> Guest Delayed <strong>{3} Minutes</strong> - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName, DelayInMinutes);
                }
                else
                {
                    InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> Guest Delayed <strong>{3}</strong> - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName, "Don't know");
                }
            }
            else if (ReservationStatus == (int)Email.ReservationStatus.NoShow)
            {
                InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> NO SHOW - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName);
            }
            else if (ReservationStatus == (int)Email.ReservationStatus.Pending || ReservationStatus == (int)Email.ReservationStatus.Initiated)
            {
                if (IsModifed)
                {
                    InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> MODIFIED - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName);
                }
                else
                {
                    if (NewTotalGuests > 0)
                    {
                        InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> RESERVED {3} Guests - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName, NewTotalGuests.ToString());
                    }
                    else
                    {
                        InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> Reset to Confirmed - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName);
                    }
                }
            }
            else if (ReservationStatus == (int)Email.ReservationStatus.Rescheduled)
            {
                InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> RESCHEDULED - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName);
            }
            else if (ReservationStatus == (int)Email.ReservationStatus.Updated)
            {
                InternalNotes = string.Format("<span class=\"badge {0}\">&nbsp;</span> UPDATED - {2}", rsvpStatusColor, DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt"), strUserName);
            }

            if (IsModifed)
            {
                if (OldTotalGuests != NewTotalGuests)
                {
                    InternalNotes = InternalNotes + "<br>Guest qty changed from " + OldTotalGuests.ToString() + " to " + NewTotalGuests.ToString() + " guests.";
                }
            }



            //Allow logging of custom note
            if (!object.ReferenceEquals(customNote.Trim(), string.Empty))
            {
                InternalNotes = customNote;
            }

            if (TaxVoided)
            {
                InternalNotes = InternalNotes + "<br>Sales Tax Voided by " + strUserName;
            }

            inotes.Note = InternalNotes;
            inotes.NoteDate = DateTime.UtcNow;
            inotes.ReservationStatus = ReservationStatus;
            inotes.StatusType = 0;

            if (WineryId > 0)
            {
                inotes.WineryID = WineryId;
            }

            inotes.CurrentUser = strUserName;
            eventDAL.SaveReservationV2StatusNotes(inotes.RefId, inotes.ReservationStatus, inotes.Note, inotes.CurrentUser, inotes.WineryID, inotes.StatusType);
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReservationID"></param>
        /// <param name="status"></param>
        /// <param name="username"></param>
        /// <param name="ActiveWineryId"></param>
        /// <param name="DelayInMinutes"></param>
        /// <returns></returns>
        public bool SetReservationV2Status(ReservationDetailModel rsvp, int status, string username, int ActiveWineryId, int DelayInMinutes = 0, bool isAdmin = false, bool IsConcierge = false)
        {
            bool ret = false;
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            if (status == (int)ReservationStatus.Cancelled && !isAdmin)
            {
                bool cancelAllowed = IsCancelAllowedOnReservation(rsvp.location_id, rsvp.cancel_lead_time, rsvp.event_start_date);
                if (!cancelAllowed)
                    return false;

            }
            ret = eventDAL.UpdateReservationV2Status(rsvp.reservation_id, status, username, ActiveWineryId, DelayInMinutes);

            //If cancelled then undo auto close if applicable 
            if (status == (int)ReservationStatus.Cancelled)
            {
                eventDAL.UndoAutoCloseEventRule(rsvp.slot_id, rsvp.slot_type, rsvp.event_start_date, username, false, rsvp.user_detail.user_id);
            }

            //Insert Reservation Status Note
            ReservationV2StatusNote_Create(rsvp.reservation_id, status, ActiveWineryId, username, false, DelayInMinutes, 0, 0, "", false, IsConcierge);

            return ret;
        }

        public bool IsCancelAllowedOnReservation(int locationId, int cancelLeadTime, DateTime event_date)
        {
            bool ret = true;
            try
            {
                LocationModel locationModel = GetLocationByID(locationId);
                double timezone_offset = locationModel.location_timezone_offset;

                if (DateTime.UtcNow.AddMinutes(timezone_offset).AddMinutes(cancelLeadTime) > event_date)
                {
                    //lead time has passed cannot canel the reservation
                    ret = false;
                }
            }
            catch { }
            return ret;
        }

        public List<PaymentModel> GetReservationPayments(int reservationId)
        {
            var paymentList = new List<PaymentModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", reservationId));
            using (DbDataReader dataReader = GetDataReader("GetReservationPaymentsByReservationId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        PaymentModel p = new PaymentModel();
                        p.id = Convert.ToInt32(dataReader["Id"]);
                        p.reservation_id = Convert.ToInt32(dataReader["Reservation_Id"]);
                        p.payment_date = Convert.ToDateTime(dataReader["PaymentDate"]);
                        p.payment_type = Convert.ToInt32(dataReader["PayType"]);
                        p.transaction_type = Convert.ToInt32(dataReader["TransactionType"]);
                        p.status = Convert.ToInt32(dataReader["Status"]);
                        p.appoval_code = Convert.ToString(dataReader["ApprovalCode"]);
                        p.transaction_id = Convert.ToString(dataReader["TransactionId"]);
                        p.detail = Convert.ToString(dataReader["Detail"]);
                        p.amount = Convert.ToDecimal(dataReader["Amount"]);
                        p.payment_card_type = Convert.ToString(dataReader["PayCardType"]);
                        string payCardNum = StringHelpers.Decryption(Convert.ToString(dataReader["PayCardNumber"])).Trim();
                        payCardNum = StringHelpers.ExtractNumber(payCardNum);
                        if (payCardNum.Length > 4)
                            p.payment_card_number = payCardNum.Substring(payCardNum.Length - 4).PadLeft(payCardNum.Length, '*');
                        else
                            p.payment_card_number = payCardNum;

                        p.payment_card_exp_month = Convert.ToString(dataReader["PayCardExpMonth"]);
                        p.payment_card_exp_year = Convert.ToString(dataReader["PayCardExpYear"]);
                        p.payment_card_customer_name = Convert.ToString(dataReader["PayCardCustName"]);
                        p.processed_by = "N/A";
                        p.refund_amount = Convert.ToDecimal(dataReader["RefundAmount"]);
                        p.original_amount = Convert.ToDecimal(dataReader["OriginalAmount"]);
                        p.payment_card_token = System.DBNull.Value.Equals(dataReader["PayCardToken"]) ? "" : Convert.ToString(dataReader["PayCardToken"]);
                        p.payment_gateway = System.DBNull.Value.Equals(dataReader["PaymentGatewayId"]) ? Payments.Configuration.Gateway.Offline : (Payments.Configuration.Gateway)Convert.ToInt32(dataReader["PaymentGatewayId"]);
                        p.payment_version = System.DBNull.Value.Equals(dataReader["PaymentVersion"]) ? (short)0 : short.Parse(dataReader["PaymentVersion"].ToString());
                        p.card_last_four_digits = Convert.ToString(dataReader["CardLastFourDigits"]);
                        p.card_first_four_digits = Convert.ToString(dataReader["CardFirstFourDigits"]);
                        paymentList.Add(p);
                    }
                }
            }
            return paymentList;
        }

        public bool UpdateCancellationReason(int reservation_id, string cancellation_reason)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationID", reservation_id));
            parameterList.Add(GetParameter("@CancellationReason", cancellation_reason));

            string sqlQuery = "update ReservationV2 set CancellationReason=@CancellationReason where ReservationId=@ReservationID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdateReservationChangesByEventId(int EventId, decimal ReservationChanges,decimal MinReservationChanges,decimal MaxReservationChanges)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationChanges", ReservationChanges));
            parameterList.Add(GetParameter("@MinReservationChanges", MinReservationChanges));
            parameterList.Add(GetParameter("@MaxReservationChanges", MaxReservationChanges));
            parameterList.Add(GetParameter("@EventId", EventId));

            string sqlQuery = "update events set ReservationChanges = @ReservationChanges,MinReservationChanges = @MinReservationChanges,MaxReservationChanges = @MaxReservationChanges where EventId = @EventId";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public List<PaymentModel> GetReservationPreAuths(int reservationId)
        {
            var paymentList = new List<PaymentModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", reservationId));
            using (DbDataReader dataReader = GetDataReader("GetReservationAuthTransByReservationId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        PaymentModel p = new PaymentModel();
                        p.id = Convert.ToInt32(dataReader["Id"]);
                        p.reservation_id = Convert.ToInt32(dataReader["Reservation_Id"]);
                        p.payment_date = Convert.ToDateTime(dataReader["PaymentDate"]);
                        p.payment_type = Convert.ToInt32(dataReader["PayType"]);
                        p.transaction_type = Convert.ToInt32(dataReader["TransactionType"]);
                        p.status = Convert.ToInt32(dataReader["Status"]);
                        p.appoval_code = Convert.ToString(dataReader["ApprovalCode"]);
                        p.transaction_id = Convert.ToString(dataReader["TransactionId"]);
                        p.detail = Convert.ToString(dataReader["Detail"]);
                        p.amount = Convert.ToDecimal(dataReader["Amount"]);
                        p.payment_card_type = Convert.ToString(dataReader["PayCardType"]);
                        //p.payment_card_number = StringHelpers.Decryption(Convert.ToString(dataReader["PayCardNumber"]));
                        string payCardNum = StringHelpers.Decryption(Convert.ToString(dataReader["PayCardNumber"])).Trim();
                        payCardNum = StringHelpers.ExtractNumber(payCardNum);
                        if (payCardNum.Length > 4)
                            p.payment_card_number = payCardNum.Substring(payCardNum.Length - 4).PadLeft(payCardNum.Length, '*');
                        else
                            p.payment_card_number = payCardNum;
                        p.payment_card_exp_year = Convert.ToString(dataReader["PayCardExpYear"]);
                        p.payment_card_customer_name = Convert.ToString(dataReader["PayCardCustName"]);
                        p.processed_by = "N/A";
                        p.refund_amount = Convert.ToDecimal(dataReader["RefundAmount"]);
                        p.original_amount = Convert.ToDecimal(dataReader["OriginalAmount"]);
                        p.payment_card_token = System.DBNull.Value.Equals(dataReader["PayCardToken"]) ? "" : Convert.ToString(dataReader["PayCardToken"]);
                        p.payment_gateway = System.DBNull.Value.Equals(dataReader["PaymentGatewayId"]) ? Payments.Configuration.Gateway.Offline : (Payments.Configuration.Gateway)Convert.ToInt32(dataReader["PaymentGatewayId"]);
                        p.payment_version = System.DBNull.Value.Equals(dataReader["PaymentVersion"]) ? (short)0 : short.Parse(dataReader["PaymentVersion"].ToString());
                        p.card_last_four_digits = Convert.ToString(dataReader["CardLastFourDigits"]);
                        p.card_first_four_digits = Convert.ToString(dataReader["CardFirstFourDigits"]);
                        paymentList.Add(p);
                    }
                }
            }
            return paymentList;
        }
        public List<PaymentModel> GetPayments(int reservationId, PayCard payCard)
        {
            var paymentList = new List<PaymentModel>();
            string sql = "GetReservationPaymentLogByReservationId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", reservationId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        PaymentModel p = new PaymentModel();
                        p.id = Convert.ToInt32(dataReader["Id"]);
                        p.reservation_id = Convert.ToInt32(dataReader["Reservation_Id"]);
                        p.payment_date = Convert.ToDateTime(dataReader["PaymentDate"]);
                        p.payment_type = Convert.ToInt32(dataReader["PayType"]);
                        p.transaction_type = Convert.ToInt32(dataReader["TransactionType"]);
                        p.status = Convert.ToInt32(dataReader["Status"]);
                        p.appoval_code = Convert.ToString(dataReader["ApprovalCode"]);
                        p.transaction_id = Convert.ToString(dataReader["TransactionId"]);
                        p.detail = Convert.ToString(dataReader["Detail"]);
                        p.amount = Convert.ToDecimal(dataReader["Amount"]);
                        p.payment_card_type = Convert.ToString(dataReader["PayCardType"]);
                        string payCardNum = StringHelpers.Decryption(Convert.ToString(dataReader["PayCardNumber"])).Trim();
                        payCardNum = StringHelpers.ExtractNumber(payCardNum);
                        if (!string.IsNullOrEmpty(payCardNum) && payCardNum.Length > 4)
                            p.payment_card_number = payCardNum.Substring(payCardNum.Length - 4).PadLeft(payCardNum.Length, '*');
                        else
                            p.payment_card_number = payCardNum;
                        //p.payment_card_number = StringHelpers.Decryption(Convert.ToString(dataReader["PayCardNumber"]));
                        p.card_last_four_digits = Convert.ToString(dataReader["CardLastFourDigits"]);
                        p.card_first_four_digits = Convert.ToString(dataReader["CardFirstFourDigits"]);

                        p.payment_card_exp_month = Convert.ToString(dataReader["PayCardExpMonth"]);
                        p.payment_card_exp_year = Convert.ToString(dataReader["PayCardExpYear"]);
                        p.payment_card_customer_name = Convert.ToString(dataReader["PayCardCustName"]);
                        p.processed_by = Convert.ToString(dataReader["ProcessedBy"]);
                        p.timezone_id = Convert.ToInt32(dataReader["TimeZoneId"]);
                        p.payment_card_token = Convert.ToString(dataReader["PayCardToken"]);
                        p.card_entry = (CardEntry)Convert.ToInt32(dataReader["CardEntry"]);
                        p.application_type = (ApplicationType)Convert.ToInt32(dataReader["ApplicationType"]);
                        p.application_version = Convert.ToString(dataReader["ApplicationVersion"]);
                        p.terminal_id = Convert.ToString(dataReader["TerminalID"]);
                        p.card_reader = Convert.ToString(dataReader["CardReader"]);
                        paymentList.Add(p);
                    }
                }
            }

            if (paymentList.Count == 0)
            {
                PaymentModel p = new PaymentModel();
                p.payment_card_customer_name = payCard.cust_name;
                p.payment_card_exp_month = payCard.exp_month;
                p.payment_card_exp_year = payCard.exp_year;
                p.payment_card_token = payCard.card_token;
                p.payment_card_type = payCard.card_type;
                p.reservation_id = reservationId;
                //p.payment_card_number = StringHelpers.Decryption(payCard.number);
                string payCardNum = StringHelpers.Decryption(payCard.number);
                payCardNum = StringHelpers.ExtractNumber(payCardNum);
                if (!string.IsNullOrEmpty(payCardNum) && payCardNum.Length > 4)
                    p.payment_card_number = payCardNum.Substring(payCardNum.Length - 4).PadLeft(payCardNum.Length, '*');
                else
                    p.payment_card_number = payCardNum;
                if (!string.IsNullOrEmpty(p.payment_card_number))
                {
                    p.payment_card_number = Common.Common.Right(p.payment_card_number, 4);
                    p.card_first_four_digits = Common.Common.Left(p.payment_card_number, 4);
                    p.card_last_four_digits = p.payment_card_number;
                }

                paymentList.Add(p);
            }
            return paymentList;
        }

        public PromoEmail SendCheckInPromo(int Id, int type)
        {
            var emailPromo = new PromoEmail();
            var paymentList = new List<PaymentModel>();
            string sql = string.Empty;
            var parameterList = new List<DbParameter>();

            if (type == 0)
            {
                sql = "CheckReservationInPromo";
                parameterList.Add(GetParameter("@ReservationId", Id));
            }
            else
            {
                sql = "CheckTicketInPromo";
                parameterList.Add(GetParameter("@TicketId", Id));
            }

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        PromoModel promo = new PromoModel();
                        promo.Id = Convert.ToInt32(dataReader["Id"]);
                        promo.Active = Convert.ToBoolean(dataReader["Active"]);
                        promo.PromoName = Convert.ToString(dataReader["PromoName"]);
                        promo.PromoValue = (PromoValue)(dataReader["PromoValue"]);
                        promo.PromoFinePrint = Convert.ToString(dataReader["PromoFinePrint"]);
                        promo.PromoZone = (PromoZone)(dataReader["PromoZone"]);
                        promo.StartDate = Convert.ToDateTime(dataReader["StartDate"]);
                        promo.EndDate = Convert.ToDateTime(dataReader["EndDate"]);
                        promo.MemberId = Convert.ToInt32(dataReader["MemberId"]);
                        promo.MemberName = Convert.ToString(dataReader["DisplayName"]);
                        promo.ReferralCode = Convert.ToString(dataReader["ReferralCode"]);
                        promo.RedemptionInstructions = Convert.ToString(dataReader["RedemptionInstructions"]);
                        promo.EventId = Convert.ToInt32(dataReader["EventId"]);
                        promo.EventName = Convert.ToString(dataReader["EventName"]);
                        promo.MemberPhone = Convert.ToString(dataReader["MemberPhone"]);


                        emailPromo.ToEmail = Convert.ToString(dataReader["Email"]);
                        emailPromo.Promo = promo;
                    }
                }
            }
            return emailPromo;
        }

        public string GetBookingCodeByReservationID(int ReservationId)
        {
            string BookingCode = string.Empty;

            string sql = "select BookingCode from ReservationV2 (nolock) where ReservationId=@ReservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        BookingCode = Convert.ToString(dataReader["BookingCode"]);
                    }
                }
            }
            return BookingCode;
        }

        public int GetTotalGuestsByReservationID(int ReservationId)
        {
            int TotalGuests = 0;

            string sql = "select TotalGuests from ReservationV2 (nolock) where ReservationId=@ReservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TotalGuests = Convert.ToInt32(dataReader["TotalGuests"]);
                    }
                }
            }
            return TotalGuests;
        }

        public EventRuleModel GetEventExceptionDetail(int SlotId, int SlotType, DateTime ExceptionDate, TimeSpan StartTime, TimeSpan EndTime)
        {
            EventRuleModel model = new EventRuleModel();
            string sql = string.Empty;
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@ExceptionDate", ExceptionDate));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@EndTime", EndTime));

            if (SlotType == (int)Common.SlotType.Rule)
            {
                sql = "select ExceptionId,LocationId,Status from EventExceptions where EventRuleId=@SlotId and ExceptionDate = @ExceptionDate and StartTime = @StartTime and EndTime = @EndTime";
            }
            else
            {
                sql = "select ExceptionId,LocationId,Status from EventExceptions where ExceptionId=@SlotId and ExceptionDate = @ExceptionDate and StartTime = @StartTime and EndTime = @EndTime";

            }

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.ExceptionId = Convert.ToInt32(dataReader["ExceptionId"]);
                        model.LocationId = Convert.ToInt32(dataReader["LocationId"]);
                        model.Status = (EventStatus)Convert.ToInt32(dataReader["Status"]);
                    }
                }
            }
            return model;
        }

        public List<RsvpConfirmationEmailTemplateModel> GetRsvpConfirmationEmailTemplates(int member_id, int EventId, bool active_only)
        {
            var rsvpConfirmationEmailTemplateModel = new List<RsvpConfirmationEmailTemplateModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", member_id));
            parameterList.Add(GetParameter("@EventId", EventId));
            parameterList.Add(GetParameter("@active_only", active_only));

            string sql = "select EmailName,Id,SystemDefault,(case when Id = (select EmailContentId from Events where EventId = @EventId) then 1 else 0 end) as isdefault";
            sql += " from EmailContent where WineryID = @WineryID and (TemplateID = 3 or TemplateID = 15) and active = (case when @active_only = 1 then 1 else active end)";

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new RsvpConfirmationEmailTemplateModel();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.email_name = Convert.ToString(dataReader["EmailName"]);
                        model.system_default = Convert.ToBoolean(dataReader["SystemDefault"]);
                        model.is_default = Convert.ToBoolean(dataReader["isdefault"]);
                        rsvpConfirmationEmailTemplateModel.Add(model);
                    }
                }
            }
            return rsvpConfirmationEmailTemplateModel;
        }
        public bool EventExceptions_Check(int ExceptionId)
        {
            bool ret = false;
            string sql = "select ExceptionId from EventExceptions where ExceptionId=@ExceptionId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ExceptionId", ExceptionId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }
        public int GetEventRuleIdbyEventExceptionID(int ExceptionId)
        {
            int EventRuleId = 0;
            string sql = "select EventRuleId from EventExceptions_Log where ExceptionId=@ExceptionId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ExceptionId", ExceptionId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        EventRuleId = Convert.ToInt32(dataReader["EventRuleId"]);
                    }
                }
            }
            return EventRuleId;
        }

        public bool CheckIfTableAvailableForSlot(int slotId, int slotType, DateTime startTime, int noOfGuests)
        {
            bool ret = false;

            string sql = "CheckIfTableAvailableForSlot";
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@SlotId", slotId));
            parameterList.Add(GetParameter("@SlotType", slotType));
            parameterList.Add(GetParameter("@EventDate", startTime));
            parameterList.Add(GetParameter("@GuestsCount", noOfGuests));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int isSlotVailable = Convert.ToInt32(dataReader["SlotAvailable"]);
                        ret = (isSlotVailable > 0);
                    }
                }
            }
            return ret;

        }

        public bool CheckifTableAvailableCanFitGuestForRSVP(int reservationId, int guestCount)
        {
            bool ret = false;

            string sql = "CheckifTableAvailableCanFitGuestForRSVP";
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@ReservationId", reservationId));
            parameterList.Add(GetParameter("@GuestsCount", guestCount));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int isSlotVailable = Convert.ToInt32(dataReader["SlotAvailable"]);
                        ret = (isSlotVailable > 0);
                    }
                }
            }
            return ret;

        }

        public CreateReservation SaveReservation(CreateReservationModel reservation, bool IsAdmin, bool force_rsvp, bool IsModify, int OldTotalGuests, string currentuser, bool IsRescheduled = false, int CellarPassTicketOrderId = 0)
        {
            CreateReservation response = new CreateReservation();
            // bool createNew = true;
            //int OldTotalGuests = 0;
            int NewTotalGuests = 0;

            try
            {
                NewTotalGuests = reservation.TotalGuests;
                //Fix for wrong eventId in some reservations: Get the eventId from DB to ensure it is correct one and get it only for regular booking and not private reservations
                if (reservation.SlotId > 0)
                {
                    EventRuleModel eventRuleModel = GetEventcapacity(reservation.SlotId, reservation.SlotType);
                    reservation.EventId = eventRuleModel.EventId;
                    reservation.StartTime = eventRuleModel.StartTime;
                    reservation.EndTime = eventRuleModel.EndTime;
                    reservation.LocationId = eventRuleModel.LocationId;
                    reservation.WineryId = eventRuleModel.WineryID;
                }

                if (reservation.StartTime == reservation.EndTime)
                {
                    response.Id = -2;
                    response.Status = Common.ResponseStatus.Failed;
                    response.error_type = (int)Common.Common.ErrorType.EventError;
                    response.Message = "Reservation Event Error";
                    response.Description = "Reservation SlotType: " + reservation.SlotType.ToString() + " ,SlotId: " + reservation.SlotId.ToString() + " not found";
                    return response;
                }

                if (reservation.SlotId > 0)
                {
                    DateTime EventDate = reservation.EventDate.Date;
                    DateTime EventStartDate = EventDate.Add(reservation.StartTime);
                    DateTime EventEndDate = EventDate.Add(reservation.EndTime);

                    if (reservation.SlotType == (int)SlotType.Rule)
                    {
                        EventRuleModel eventRuleModel = GetEventExceptionDetail(reservation.SlotId, reservation.SlotType, reservation.EventDate.Date, reservation.StartTime, reservation.EndTime);
                        if (eventRuleModel.ExceptionId > 0)
                        {
                            if (eventRuleModel.Status == EventStatus.Active)
                            {
                                reservation.SlotType = 1;
                                reservation.SlotId = eventRuleModel.ExceptionId;
                                reservation.LocationId = eventRuleModel.LocationId;
                            }
                            else
                            {
                                response.Id = -2;
                                response.Status = Common.ResponseStatus.Failed;
                                response.error_type = (int)Common.Common.ErrorType.EventError;
                                response.Message = "Reservation Error Winery: " + GetWineryById(reservation.WineryId).DisplayName;
                                response.Description = "Reservation SlotType: " + reservation.SlotType.ToString() + " SlotId: " + reservation.SlotId.ToString() + " Reservation StartTime: " + EventStartDate.ToString("MM/dd/yyyy hh:mm:ss tt") + " EndTime: " + EventEndDate.ToString("MM/dd/yyyy hh:mm:ss tt");

                                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                                logDAL.InsertLog("SaveReservation", JsonConvert.SerializeObject(reservation), "", 3, reservation.WineryId);

                                return response;
                            }
                        }
                    }
                    else if (reservation.SlotType == (int)SlotType.Exception)
                    {
                        EventRuleModel eventRuleModel = GetEventExceptionDetail(reservation.SlotId, reservation.SlotType, EventDate, reservation.StartTime, reservation.EndTime);

                        if (eventRuleModel.ExceptionId == 0)
                        {
                            response.Id = -2;
                            response.Status = Common.ResponseStatus.Failed;
                            response.error_type = (int)Common.Common.ErrorType.EventError;
                            response.Message = "Reservation Error Winery: " + GetWineryById(reservation.WineryId).DisplayName;
                            response.Description = "Reservation SlotType: " + reservation.SlotType.ToString() + " SlotId: " + reservation.SlotId.ToString() + " Reservation StartTime: " + EventStartDate.ToString("MM/dd/yyyy hh:mm:ss tt") + " EndTime: " + EventEndDate.ToString("MM/dd/yyyy hh:mm:ss tt");

                            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                            logDAL.InsertLog("SaveReservation", JsonConvert.SerializeObject(reservation), "", 3, reservation.WineryId);

                            return response;
                        }
                        else if (eventRuleModel.Status != EventStatus.Active && IsAdmin == false)
                        {
                            if (IsModify == false)
                            {
                                response.Id = -2;
                                response.Status = Common.ResponseStatus.Failed;
                                response.error_type = (int)Common.Common.ErrorType.EventError;
                                response.Message = "Reservation Error Winery: " + GetWineryById(reservation.WineryId).DisplayName;
                                response.Description = "Reservation SlotType: " + reservation.SlotType.ToString() + " SlotId: " + reservation.SlotId.ToString() + " Reservation StartTime: " + EventStartDate.ToString("MM/dd/yyyy hh:mm:ss tt") + " EndTime: " + EventEndDate.ToString("MM/dd/yyyy hh:mm:ss tt");

                                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                                logDAL.InsertLog("SaveReservation", JsonConvert.SerializeObject(reservation), "", 3, reservation.WineryId);

                                return response;
                            }
                            else if (IsRescheduled)
                            {
                                response.Id = -2;
                                response.Status = Common.ResponseStatus.Failed;
                                response.error_type = (int)Common.Common.ErrorType.EventError;
                                response.Message = "Reservation Error Winery: " + GetWineryById(reservation.WineryId).DisplayName;
                                response.Description = "Reservation SlotType: " + reservation.SlotType.ToString() + " SlotId: " + reservation.SlotId.ToString() + " Reservation StartTime: " + EventStartDate.ToString("MM/dd/yyyy hh:mm:ss tt") + " EndTime: " + EventEndDate.ToString("MM/dd/yyyy hh:mm:ss tt");

                                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                                logDAL.InsertLog("SaveReservation", JsonConvert.SerializeObject(reservation), "", 3, reservation.WineryId);

                                return response;
                            }
                        }

                        reservation.LocationId = eventRuleModel.LocationId;
                    }
                }
                else
                {
                    reservation.SlotType = 2;
                }


                reservation.BookingDate = new DateTimeOffset(DateTime.Now, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

                reservation.Note = reservation.Note == null ? "" : Common.Common.Left(reservation.Note, 1200);

                reservation.ConciergeNote = reservation.ConciergeNote == null ? "" : Common.Common.Left(reservation.ConciergeNote, 500);

                reservation.InternalNote = reservation.InternalNote == null ? "" : Common.Common.Left(reservation.InternalNote, 1200);

                reservation.PersonalMessage = reservation.PersonalMessage == null ? "" : Common.Common.Left(reservation.PersonalMessage, 1200);

                bool SkipConflictCheck = force_rsvp;

                if (SkipConflictCheck == false)
                    SkipConflictCheck = IsAdmin;

                if (SkipConflictCheck == false)
                {
                    ReservationConflicts reservationConflicts = new ReservationConflicts();
                    bool isWaitList = !string.IsNullOrWhiteSpace(reservation.WaitListGuid);
                    reservationConflicts = CheckReservationConflicts(reservation.SlotId, reservation.SlotType, reservation.EventDate, reservation.TotalGuests, reservation.ReservationId, reservation.UserId, 0, reservation.WineryId, IsAdmin, force_rsvp, isWaitList);

                    if (reservationConflicts.success == false)
                    {
                        response.Id = -2;
                        response.Status = Common.ResponseStatus.Failed;
                        response.error_type = reservationConflicts.error_type;
                        response.Message = reservationConflicts.extra_info;
                        response.Description = reservationConflicts.description;
                        return response;
                    }
                }
                else if (reservation.ReservationId == 0)
                {
                    ReservationConflicts reservationConflicts = new ReservationConflicts();
                    bool isWaitList = !string.IsNullOrWhiteSpace(reservation.WaitListGuid);
                    reservationConflicts = CheckAdminReservationConflicts(reservation.SlotId, reservation.SlotType, reservation.EventDate, reservation.TotalGuests, reservation.ReservationId, reservation.UserId, 0, reservation.WineryId, IsAdmin, force_rsvp, isWaitList);

                    if (reservationConflicts.success == false)
                    {
                        response.Id = -2;
                        response.Status = Common.ResponseStatus.Failed;
                        response.error_type = reservationConflicts.error_type;
                        response.Message = reservationConflicts.extra_info;
                        response.Description = reservationConflicts.description;
                        return response;
                    }


                }

                //if existing we update else we create
                int ReservationId = 0;
                if (IsModify)
                    ReservationId = reservation.ReservationId;

                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@EventId", reservation.EventId));
                parameterList.Add(GetParameter("@WineryId", reservation.WineryId));
                parameterList.Add(GetParameter("@EventName", reservation.EventName));
                parameterList.Add(GetParameter("@EventLocation", reservation.EventLocation));
                parameterList.Add(GetParameter("@EventDate", reservation.EventDate));
                parameterList.Add(GetParameter("@StartTime", reservation.StartTime));
                parameterList.Add(GetParameter("@EndTime", reservation.EndTime));
                parameterList.Add(GetParameter("@FeePerPerson", reservation.FeePerPerson));
                parameterList.Add(GetParameter("@ChargeFee", reservation.ChargeFee));
                parameterList.Add(GetParameter("@RequireCreditCard", reservation.RequireCreditCard));
                parameterList.Add(GetParameter("@UserId", reservation.UserId));
                parameterList.Add(GetParameter("@TotalGuests", reservation.TotalGuests));
                parameterList.Add(GetParameter("@BookingDate", reservation.BookingDate));
                parameterList.Add(GetParameter("@Note", reservation.Note));
                parameterList.Add(GetParameter("@ReferralType", reservation.ReferralType));
                parameterList.Add(GetParameter("@BookingCode", reservation.BookingCode));
                parameterList.Add(GetParameter("@PayType", reservation.PayType));
                parameterList.Add(GetParameter("@PayCardType", reservation.PayCardType));
                parameterList.Add(GetParameter("@PayCardNumber", reservation.PayCardNumber));
                parameterList.Add(GetParameter("@PayCardCustName", reservation.PayCardCustName));
                parameterList.Add(GetParameter("@PayCardExpMonth", reservation.PayCardExpMonth));
                parameterList.Add(GetParameter("@PayCardExpYear", reservation.PayCardExpYear));
                parameterList.Add(GetParameter("@Status", reservation.Status));
                parameterList.Add(GetParameter("@CompletedGuestCount", reservation.CompletedGuestCount));
                parameterList.Add(GetParameter("@BookedById", reservation.BookedById));
                parameterList.Add(GetParameter("@ConciergeNote", reservation.ConciergeNote));
                parameterList.Add(GetParameter("@WaiveFee", reservation.WaiveFee));
                parameterList.Add(GetParameter("@AffiliateID", reservation.AffiliateID));
                parameterList.Add(GetParameter("@ReferralID", reservation.ReferralID));
                parameterList.Add(GetParameter("@ReturningGuest", reservation.ReturningGuest));
                parameterList.Add(GetParameter("@EmailContentID", reservation.EmailContentID));
                parameterList.Add(GetParameter("@WineryReferralId", reservation.WineryReferralId));
                parameterList.Add(GetParameter("@FeeDue", reservation.FeeDue));
                parameterList.Add(GetParameter("@AmountPaid", reservation.AmountPaid));
                parameterList.Add(GetParameter("@PurchaseTotal", reservation.PurchaseTotal));
                parameterList.Add(GetParameter("@Email", reservation.Email.Trim()));
                parameterList.Add(GetParameter("@FirstName", reservation.FirstName.Trim()));
                parameterList.Add(GetParameter("@LastName", reservation.LastName.Trim()));
                parameterList.Add(GetParameter("@PhoneNumber", reservation.PhoneNumber));
                parameterList.Add(GetParameter("@Zip", reservation.Zip));
                parameterList.Add(GetParameter("@SlotId", reservation.SlotId));
                parameterList.Add(GetParameter("@SlotType", reservation.SlotType));
                parameterList.Add(GetParameter("@DiscountAmt", reservation.DiscountAmt));
                parameterList.Add(GetParameter("@CellarPassTicketOrderId", CellarPassTicketOrderId));

                if (!string.IsNullOrEmpty(reservation.DiscountDesc))
                    parameterList.Add(GetParameter("@DiscountDesc", "Discount Code Applied"));
                else
                    parameterList.Add(GetParameter("@DiscountDesc", ""));

                parameterList.Add(GetParameter("@InternalNote", reservation.InternalNote));
                parameterList.Add(GetParameter("@Country", reservation.Country));
                parameterList.Add(GetParameter("@CustomerType", reservation.CustomerType));
                parameterList.Add(GetParameter("@BookedByName", reservation.BookedByName));
                parameterList.Add(GetParameter("@CreditCardReferenceNumber", reservation.CreditCardReferenceNumber));
                parameterList.Add(GetParameter("@City", reservation.City));
                parameterList.Add(GetParameter("@State", reservation.State));
                parameterList.Add(GetParameter("@LocationId", reservation.LocationId));
                parameterList.Add(GetParameter("@SalesTax", reservation.SalesTax));
                parameterList.Add(GetParameter("@DiscountId", reservation.DiscountId));
                parameterList.Add(GetParameter("@DiscountCode", reservation.DiscountCode));
                parameterList.Add(GetParameter("@DiscountCodeAmount", reservation.DiscountCodeAmount));
                parameterList.Add(GetParameter("@SalesTaxPercentage", reservation.SalesTaxPercentage));
                parameterList.Add(GetParameter("@HDYH", reservation.HDYH));
                parameterList.Add(GetParameter("@MobilePhone", reservation.MobilePhone));
                parameterList.Add(GetParameter("@MobilePhoneStatus", reservation.MobilePhoneStatus));
                parameterList.Add(GetParameter("@ReservationId", ReservationId));
                parameterList.Add(GetParameter("@Tags", reservation.Tags));
                parameterList.Add(GetParameter("@PreAssign_Server_Id", reservation.PreAssign_Server_Id));
                parameterList.Add(GetParameter("@PreAssign_Table_Id", reservation.PreAssign_Table_Id));
                parameterList.Add(GetParameter("@privateaddongroup", reservation.privateaddongroup));
                parameterList.Add(GetParameter("@CancelLeadTime", reservation.CancelLeadTime));
                parameterList.Add(GetParameter("@TransportationId", reservation.TransportationId));
                parameterList.Add(GetParameter("@TransportationName", reservation.TransportationName));
                parameterList.Add(GetParameter("@PayCardToken", reservation.PayCardToken));
                parameterList.Add(GetParameter("@GratuityAmount", reservation.GratuityAmount));
                parameterList.Add(GetParameter("@ContactTypes", reservation.ContactTypes));
                parameterList.Add(GetParameter("@AccessCode", reservation.AccessCode));
                parameterList.Add(GetParameter("@FeeType", reservation.FeeType));
                parameterList.Add(GetParameter("@IgnoreDiscount", reservation.IgnoreDiscount));

                string questionsJSON = "";
                if (reservation.AttendeeQuestions != null && reservation.AttendeeQuestions.Count > 0)
                {
                    foreach (var item in reservation.AttendeeQuestions)
                    {
                        item.answer = Common.Common.Left(item.answer, 200) + "";
                        item.question = item.question + "";
                    }

                    questionsJSON = JsonConvert.SerializeObject(reservation.AttendeeQuestions);
                }
                parameterList.Add(GetParameter("@AttendeeQuestions", questionsJSON));
                parameterList.Add(GetParameter("@WaitListGuid", reservation.WaitListGuid));
                parameterList.Add(GetParameter("@FloorPlanId", reservation.FloorPlanId));
                parameterList.Add(GetParameter("@Address1", reservation.Address1));
                parameterList.Add(GetParameter("@Address2", reservation.Address2));
                parameterList.Add(GetParameter("@ForceRSVP", force_rsvp));

                parameterList.Add(GetParameter("@PersonalMessage", reservation.PersonalMessage));

                string bookingGuid = "";
                using (DbDataReader dataReader = GetDataReader("ReservationV2Insert", parameterList))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            ReservationId = Convert.ToInt32(dataReader["reservationid"]);
                            bookingGuid = Convert.ToString(dataReader["BookingGUID"]);
                        }
                    }

                }

                //ReservationId = Convert.ToInt32(ExecuteScalar("ReservationV2Insert", parameterList));

                if (ReservationId > 0)
                {
                    response.Status = Common.ResponseStatus.Success;
                    response.Id = ReservationId;
                    response.BookingGUID = bookingGuid;
                    response.BookingCode = reservation.BookingCode;

                    if (reservation.Status != (int)Email.ReservationStatus.Rescheduled && reservation.Status != (int)Email.ReservationStatus.Initiated)
                    {
                        ReservationV2StatusNote_Create(ReservationId, reservation.Status, reservation.WineryId, currentuser, IsModify == true, 0, OldTotalGuests, NewTotalGuests);
                    }

                    response.Message = "Reservation completed successfully.";

                    //check and add reservation questions if passed

                    if (reservation.Status != (int)Email.ReservationStatus.Initiated)
                    {
                        var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                        var model = new CreateDeltaRequest();
                        model.item_id = ReservationId;
                        model.item_type = (int)ItemType.Reservations;
                        model.location_id = reservation.LocationId;
                        model.member_id = reservation.WineryId;
                        model.action_date = reservation.EventDate;
                        notificationDAL.SaveDelta(model);
                    }
                        
                    if (!IsModify)
                    {
                        response.SaveType = SaveType.Saved;
                    }
                    else
                        response.SaveType = SaveType.Updated;
                }
                else
                {
                    response.Status = Common.ResponseStatus.Failed;
                    if (IsModify)
                    {
                        response.error_type = (int)Common.Common.ErrorType.ReservationUpdateError;
                        response.Id = reservation.ReservationId;
                        response.Message = "Update Error";
                    }
                    else
                    {
                        response.error_type = (int)Common.Common.ErrorType.ReservationSavingError;
                        response.Id = -2;
                        response.Message = "Saving Error";
                    }
                }

            }
            catch (Exception ex)
            {
                response.Id = -2;
                response.Status = Common.ResponseStatus.Failed;

                if (IsModify)
                    response.Message = "Update Error";
                else
                    response.Message = "Saving Error";

                if (ex.Message.IndexOf("IX_UC_ReservationV2") > -1)
                {
                    response.Description = "Duplicate reservation error. There is already a reservation created with exact same information.";
                    response.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                }
                else if (ex.Message.IndexOf("There are no seats available for") > -1)
                {
                    response.Description = ex.Message.ToString().Replace("\"", "'");
                    response.error_type = (int)Common.Common.ErrorType.AvailableSeats;
                }
                else if (ex.Message.IndexOf("no longer available.") > -1)
                {
                    response.Description = ex.Message.ToString().Replace("\"", "'");
                    response.error_type = (int)Common.Common.ErrorType.AvailableSeats;
                }
                else if (ex.Message.IndexOf("Please select a valid location") > -1)
                {
                    response.Description = ex.Message.ToString().Replace("\"", "'");
                    response.error_type = (int)Common.Common.ErrorType.EventError;
                }
                else
                {
                    response.Description = ex.Message.ToString().Replace("\"", "'");
                    response.error_type = (int)Common.Common.ErrorType.Exception;
                }
            }
            return response;
        }
        public void SaveReservation_Addon(int Category, decimal Cost, string Description, string Image, int ItemId, int ItemType, string Name, decimal Price, int Qty, string Sku, int reservation_id, int GroupId, int GroupItemId, bool Taxable,decimal AddOnsPrice)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Reservation_Id", reservation_id));
            parameterList.Add(GetParameter("@Qty", Qty));
            parameterList.Add(GetParameter("@Sku", Sku));
            parameterList.Add(GetParameter("@Name", Name));
            parameterList.Add(GetParameter("@Description", Description));
            parameterList.Add(GetParameter("@Cost", Cost));
            parameterList.Add(GetParameter("@Price", Price));
            parameterList.Add(GetParameter("@Taxable", Taxable));
            parameterList.Add(GetParameter("@Category", Category));
            parameterList.Add(GetParameter("@ItemType", ItemType));
            parameterList.Add(GetParameter("@Image", Image));
            parameterList.Add(GetParameter("@ItemId", ItemId));
            parameterList.Add(GetParameter("@SalesTax", 0));
            parameterList.Add(GetParameter("@GroupItemId", GroupItemId));
            parameterList.Add(GetParameter("@GroupId", GroupId));
            parameterList.Add(GetParameter("@AddOnsPrice", AddOnsPrice));

            ExecuteNonQuery("ReservationV2_AddOnInsert", parameterList, CommandType.StoredProcedure);
        }

        public ReservationConflicts CheckReservationConflicts(int slot_id, int slot_type, DateTime req_date, int no_of_guests, int reservation_id, int user_id, double timezone_offset = 0, int member_id = 0, bool IsAdmin = false, bool force_rsvp = false, bool isWaitlist = false)
        {
            ReservationConflicts reservationConflicts = new ReservationConflicts();
            int invMode = 0;

            reservationConflicts.success = true;
            //OVERBOOKING CHECK - START
            EventRuleModel eventRuleModel = GetEventcapacity(slot_id, slot_type);
            if (IsAdmin == false && slot_id > 0)
            {
                if (!isWaitlist)
                {
                    int GuestSum = GetGuestSum(slot_id, slot_type, req_date, reservation_id);

                    int availableSeats = (eventRuleModel.TotalSeats - GuestSum);

                    if (availableSeats < no_of_guests)
                    {
                        reservationConflicts.success = false;
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.AvailableSeats;
                        reservationConflicts.extra_info = "No Availability";
                        if (availableSeats > 0)
                        {
                            string seatMsg = "";
                            if (availableSeats > 1)
                            {
                                seatMsg = string.Format("There are only {0} seats available.", availableSeats);
                            }
                            else
                            {
                                seatMsg = "There is only 1 seat available.";
                            }
                            reservationConflicts.description = string.Format("Sorry, the capacity of the event has changed. {0}", seatMsg);
                        }
                        else
                        {
                            reservationConflicts.description = "Sorry, the capacity of the event has changed. There is no longer space available.";
                        }
                        return reservationConflicts;
                    }
                }
                //OVERBOOKING CHECK - END


                //LAST MINUTE CHECKS START
                EventModel eventModel = GetEventById(eventRuleModel.EventId);

                if (timezone_offset == 0)
                {
                    int LocationId = eventModel.LocationId;

                    if (slot_type > 0)
                    {
                        LocationId = eventRuleModel.LocationId;
                    }

                    LocationModel locationModel = GetLocationByID(LocationId);
                    timezone_offset = locationModel.location_timezone_offset;
                }

                if (eventModel.EventID == eventRuleModel.EventId)
                {
                    //Check Lead Time
                    if (DateTime.UtcNow.AddMinutes(timezone_offset).AddMinutes(eventModel.LeadTime) > req_date.Date.Add(eventRuleModel.StartTime))
                    {
                        string desc = "";
                        if (eventModel.LeadTime == 0)
                            desc = "0 Hours";
                        else if (eventModel.LeadTime < 60)
                            desc = eventModel.LeadTime.ToString() + " Minutes";
                        else if (eventModel.LeadTime == 60)
                            desc = "1 Hour";
                        else
                            desc = eventModel.LeadTime / 60 + " Hours";

                        reservationConflicts.success = false;
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.EventLeadTime;
                        reservationConflicts.extra_info = "No Availability";
                        if (eventModel.LeadTime > 0)
                            reservationConflicts.description = string.Format("Sorry, but this event must be reserved {0} prior to the event start time.", desc);
                        else
                            reservationConflicts.description = "Sorry, but this event is no longer available.";
                        return reservationConflicts;
                    }

                    if (DateTime.UtcNow.AddMinutes(timezone_offset).AddDays(eventModel.MaxLeadTime) < req_date.Date.Add(eventRuleModel.StartTime))
                    {
                        string desc = eventModel.MaxLeadTime.ToString() + " Days";

                        reservationConflicts.success = false;
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.EventLeadTime;
                        reservationConflicts.extra_info = "No Availability";
                        if (eventModel.MaxLeadTime > 0)
                            reservationConflicts.description = string.Format("Sorry, but this event must be reserved {0} prior to the event start time.", desc);
                        else
                            reservationConflicts.description = "Sorry, but this event is no longer available.";
                        return reservationConflicts;
                    }

                    //Check Status
                    if (!eventModel.EventStatus)
                    {
                        reservationConflicts.success = false;
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.EventInactive;
                        reservationConflicts.extra_info = "Sorry, but this event is no longer available.";
                        reservationConflicts.description = "Reservation Error Winery: " + GetWineryById(member_id).DisplayName + " Change Event Status : " + eventModel.EventStatus.ToString();
                        return reservationConflicts;
                    }

                    //Holiday Check
                    string Holiday = IsHoliday(eventModel.MemberID, req_date);
                    if (!string.IsNullOrEmpty(Holiday))
                    {
                        reservationConflicts.success = false;
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.IsHoliday;
                        reservationConflicts.extra_info = Holiday;
                        reservationConflicts.description = "WARNING! This property is closed on " + req_date.ToString("dddd, MMMM dd, yyyy");
                        return reservationConflicts;
                    }
                }

                UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                invMode = userDAL.GetInventoryModeForMember(eventModel.MemberID);

            }

            //LAST MINUTE CHECKS END
            bool SkipConflictCheck = force_rsvp;

            if (SkipConflictCheck == false)
                SkipConflictCheck = IsAdmin;

            var reservationDetailModel = new ReservationDetailModel();
            reservationDetailModel = IsReservationConflict(reservation_id, user_id, req_date, eventRuleModel.StartTime, eventRuleModel.EndTime, slot_id, slot_type);
            if (reservationDetailModel != null && reservationDetailModel.reservation_id > 0)
            {
                if (reservationDetailModel.event_start_date.TimeOfDay == eventRuleModel.StartTime && reservationDetailModel.event_end_date.TimeOfDay == eventRuleModel.EndTime)
                {
                    reservationConflicts.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                    reservationConflicts.extra_info = "Guest Booking Warning";
                    reservationConflicts.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationDetailModel.location_name, reservationDetailModel.event_start_date.ToString("hh:mm tt"), reservationDetailModel.event_end_date.ToString("hh:mm tt"), reservationDetailModel.event_name, reservationDetailModel.event_start_date.ToString("MMMM dd yyyy"));
                    reservationConflicts.error_data.event_name = reservationDetailModel.event_name;
                    reservationConflicts.error_data.location_name = reservationDetailModel.location_name;
                    reservationConflicts.error_data.start_date = reservationDetailModel.event_start_date;
                    reservationConflicts.error_data.end_date = reservationDetailModel.event_end_date;
                }
                else if (reservationDetailModel.member_id != member_id)
                {
                    if (reservationDetailModel.event_start_date.TimeOfDay != eventRuleModel.EndTime || reservationDetailModel.event_end_date.TimeOfDay != eventRuleModel.StartTime)
                    {
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                        reservationConflicts.extra_info = "Guest Booking Warning";
                        reservationConflicts.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationDetailModel.location_name, reservationDetailModel.event_start_date.ToString("hh:mm tt"), reservationDetailModel.event_end_date.ToString("hh:mm tt"), reservationDetailModel.event_name, reservationDetailModel.event_start_date.ToString("MMMM dd yyyy"));
                        reservationConflicts.error_data.event_name = reservationDetailModel.event_name;
                        reservationConflicts.error_data.location_name = reservationDetailModel.location_name;
                        reservationConflicts.error_data.start_date = reservationDetailModel.event_start_date;
                        reservationConflicts.error_data.end_date = reservationDetailModel.event_end_date;
                    }
                }
                else if (SkipConflictCheck == false)
                {
                    if (reservationDetailModel.event_start_date.TimeOfDay == eventRuleModel.EndTime || reservationDetailModel.event_end_date.TimeOfDay == eventRuleModel.StartTime)
                    {
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.RsvpBackToBack;
                        reservationConflicts.extra_info = "Reservation Back To Back Detected";
                        reservationConflicts.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationDetailModel.location_name, reservationDetailModel.event_start_date.ToString("hh:mm tt"), reservationDetailModel.event_end_date.ToString("hh:mm tt"), reservationDetailModel.event_name, reservationDetailModel.event_start_date.ToString("MMMM dd yyyy"));
                        reservationConflicts.error_data.event_name = reservationDetailModel.event_name;
                        reservationConflicts.error_data.location_name = reservationDetailModel.location_name;
                        reservationConflicts.error_data.start_date = reservationDetailModel.event_start_date;
                        reservationConflicts.error_data.end_date = reservationDetailModel.event_end_date;
                    }
                    else if (reservationDetailModel.member_id == member_id)
                    {
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.ReservationConflict;
                        reservationConflicts.extra_info = "Reservation Conflict Detected";
                        reservationConflicts.description = string.Format("WARNING! Guest has reservation confirmed for {3} at {0} from {1} to {2} on {4}. Proceeding with this reservation will create a schedule conflict.", reservationDetailModel.location_name, reservationDetailModel.event_start_date.ToString("hh:mm tt"), reservationDetailModel.event_end_date.ToString("hh:mm tt"), reservationDetailModel.event_name, reservationDetailModel.event_start_date.ToString("MMMM dd yyyy"));
                        reservationConflicts.error_data.event_name = reservationDetailModel.event_name;
                        reservationConflicts.error_data.location_name = reservationDetailModel.location_name;
                        reservationConflicts.error_data.start_date = reservationDetailModel.event_start_date;
                        reservationConflicts.error_data.end_date = reservationDetailModel.event_end_date;
                    }
                }
            }

            if (invMode == 1 & !isWaitlist)
            {
                if (reservation_id == 0 && slot_id > 0)
                {
                    //check if tables available
                    bool tablesAvailable = CheckIfTableAvailableForSlot(slot_id, slot_type, req_date, no_of_guests);
                    if (!tablesAvailable)
                    {
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.NoTablesAvailable;
                        reservationConflicts.extra_info = "Guest Booking Warning";
                        reservationConflicts.description = string.Format("WARNING! No tables available for party of {0} guests. Proceeding with this reservation will lead to over booking.", no_of_guests.ToString());
                    }

                }


            }
            return reservationConflicts;
        }

        public ReservationConflicts CheckAdminReservationConflicts(int slot_id, int slot_type, DateTime req_date, int no_of_guests, int reservation_id, int user_id, double timezone_offset = 0, int member_id = 0, bool IsAdmin = false, bool force_rsvp = false, bool isWaitlist = false)
        {
            ReservationConflicts reservationConflicts = new ReservationConflicts();

            reservationConflicts.success = true;
            EventRuleModel eventRuleModel = GetEventcapacity(slot_id, slot_type);
            if (slot_id > 0)
            {
                EventModel eventModel = GetEventById(eventRuleModel.EventId);

                if (timezone_offset == 0)
                {
                    int LocationId = eventModel.LocationId;

                    if (slot_type > 0)
                    {
                        LocationId = eventRuleModel.LocationId;
                    }

                    LocationModel locationModel = GetLocationByID(LocationId);
                    timezone_offset = locationModel.location_timezone_offset;
                }

                if (eventModel.EventID == eventRuleModel.EventId)
                {
                    //Check Lead Time
                    //if (DateTime.UtcNow.AddMinutes(timezone_offset) > req_date.Date.Add(eventRuleModel.EndTime))
                    //{
                    //    reservationConflicts.success = false;
                    //    reservationConflicts.error_type = (int)Common.Common.ErrorType.EventError;
                    //    reservationConflicts.extra_info = "No Availability";

                    //    reservationConflicts.description = "Sorry, but this event is no longer available.";

                    //    return reservationConflicts;
                    //}

                    //Check Status
                    if (!eventModel.EventStatus)
                    {
                        reservationConflicts.success = false;
                        reservationConflicts.error_type = (int)Common.Common.ErrorType.EventInactive;
                        reservationConflicts.extra_info = "Sorry, but this event is no longer available.";
                        reservationConflicts.description = "Reservation Error Winery: " + GetWineryById(member_id).DisplayName + " Change Event Status : " + eventModel.EventStatus.ToString();
                        return reservationConflicts;
                    }
                }
            }

            return reservationConflicts;
        }

        public void DeleteAllGuestsOnReservation(int reservation_id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Reservation_Id", reservation_id));

            string sql = "DELETE FROM RsvpGuestsDetail WHERE ReservationId = @Reservation_Id";

            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }
        public void DeleteAllAddonOnReservation(int reservation_id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Reservation_Id", reservation_id));

            string sql = "DELETE FROM ReservationV2_AddOn where Reservation_Id = @Reservation_Id";

            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }

        public void SaveGuestToReservation(int reservation_id, string firstName, string lastName, string email = "")
        {
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@GuestName", firstName.Trim() + " " + lastName.Trim()));
            parameterList.Add(GetParameter("@Email", email));
            parameterList.Add(GetParameter("@FirstName", firstName));
            parameterList.Add(GetParameter("@LastName", lastName));
            parameterList.Add(GetParameter("@ReservationId", reservation_id));

            ExecuteScalar("InsertRsvpGuestsDetail", parameterList);
        }

        public void SaveRsvpSurveyWaiver(int member_id, int reservation_id, int AdditionalGuestId, int UserId, string email, int InviteType)
        {
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Email", email));
            parameterList.Add(GetParameter("@MemberId", member_id));
            parameterList.Add(GetParameter("@ReservationId", reservation_id));
            parameterList.Add(GetParameter("@AdditionalGuestId", AdditionalGuestId));
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@InviteType", InviteType));

            ExecuteScalar("InsertRsvpSurveyWaiver", parameterList);
        }

        public List<Additional_guests> GetRsvpGuestsbyReservationId(int ReservationId, int memberId, bool covidSurveyEnabled, bool covidWaiverEnabled, Additional_guests addguest)
        {
            List<Additional_guests> listadditional_guests = new List<Additional_guests>();

            string sql = "GetRSVPGuestDetails";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            int totalGuests = 0;
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        totalGuests = Convert.ToInt32(dataReader["TotalGuests"]);
                        var model = new Additional_guests();
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.email = Convert.ToString(dataReader["Email"]);
                        model.user_id = Convert.ToInt32(dataReader["UserID"]);

                        if (covidSurveyEnabled || covidWaiverEnabled)
                        {
                            if (!string.IsNullOrEmpty(model.email))
                            {
                                SurveyWaiverStatus surveyWaiverStatus = GetSurveyWaiverStatusByEmailAndMemberId(memberId, model.email, model.id);

                                model.survey_expire_date = surveyWaiverStatus.survey_expire_date;
                                model.survey_status = surveyWaiverStatus.survey_status;
                                model.waiver_status = surveyWaiverStatus.waiver_status;
                                model.modified_date = surveyWaiverStatus.modified_date;
                            }
                            else
                            {
                                if (covidSurveyEnabled)
                                    model.survey_status = RSVPPostCaptureStatus.Available;

                                if (covidWaiverEnabled)
                                    model.waiver_status = RSVPPostCaptureStatus.Available;
                            }
                        }

                        listadditional_guests.Add(model);
                    }
                }
            }

            if (totalGuests == 0 || (addguest != null && addguest.user_id > 0 && listadditional_guests.Count() < totalGuests))
                listadditional_guests.Insert(0, addguest);

            return listadditional_guests;
        }
        public List<Reservation_Addon> GetReservation_AddonbyReservationId(int ReservationId)
        {
            List<Reservation_Addon> listreservation_Addon = new List<Reservation_Addon>();
            string sql = "select r.id,r.Qty,r.Sku,r.Name,r.Description,r.Cost,r.Price,r.Category,r.ItemType,r.Image,r.ItemId,isnull(GroupItemId,0) as group_item_id,isnull(g.id,0) as group_id, isnull(g.Name,'') as group_name,isnull(g.GroupType,0) as group_type,isnull(gi.SortOrder,0) as SortOrder, r.Taxable, r.CalculateGratuity from ReservationV2_AddOn (nolock) r  left join AddOn_Group_Items (nolock) gi on gi.Id=GroupItemId left join AddOn_Group (nolock) g on g.id=GroupId where Reservation_Id = @ReservationId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new Reservation_Addon();
                        model.qty = Convert.ToInt32(dataReader["Qty"]);
                        model.sku = Convert.ToString(dataReader["Sku"]);
                        model.name = Convert.ToString(dataReader["Name"]);
                        model.description = Convert.ToString(dataReader["Description"]);
                        model.cost = Convert.ToDecimal(dataReader["Cost"]);
                        model.price = Convert.ToDecimal(dataReader["Price"]);
                        model.category = Convert.ToInt32(dataReader["Category"]);
                        AddonItemCategory itemCategory = (AddonItemCategory)model.category;
                        model.item_category_name = itemCategory.ToString();
                        model.item_type = Convert.ToInt32(dataReader["ItemType"]);
                        model.image = Convert.ToString(dataReader["Image"]);
                        model.item_id = Convert.ToInt32(dataReader["ItemId"]);
                        model.group_item_id = Convert.ToInt32(dataReader["group_item_id"]);
                        model.group_id = Convert.ToInt32(dataReader["group_id"]);
                        model.group_name = Convert.ToString(dataReader["group_name"]);
                        model.group_type = Convert.ToInt32(dataReader["group_type"]);
                        model.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                        model.taxable = Convert.ToBoolean(dataReader["Taxable"]);
                        model.calculate_gratuity = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                        listreservation_Addon.Add(model);
                    }
                }
            }
            return listreservation_Addon;
        }

        public Payments.Configuration GetWineryPaymentConfiguration(int wineryID, int SettingType = 1)
        {
            try
            {
                Payments.Configuration pcfg = new Payments.Configuration();
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@WineryID", wineryID));
                parameterList.Add(GetParameter("@SettingType", SettingType));

                string sql = "select PaymentGateway, UserName, [Password], UserConfig1, UserConfig2, Mode from PaymentConfig (nolock) p  left join winery (nolock) w on w.id=p.Winery_ID where Winery_ID = @WineryID and isactive = 1 and p.SettingType = @SettingType and (EnablePayments = 1 or Winery_ID = @WineryID)";

                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            pcfg.MerchantLogin = Convert.ToString(dataReader["UserName"]);
                            pcfg.MerchantPassword = Convert.ToString(dataReader["Password"]);
                            pcfg.UserConfig1 = Convert.ToString(dataReader["UserConfig1"]);
                            pcfg.UserConfig2 = Convert.ToString(dataReader["UserConfig2"]);
                            pcfg.PaymentGateway = Convert.ToInt32(dataReader["PaymentGateway"]);
                            pcfg.GatewayMode = (Configuration.Mode)Convert.ToInt32(dataReader["Mode"]);
                        }
                    }
                }
                return pcfg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Payments.Configuration GetWineryPaymentConfiguration(int wineryID, int PaymentGateway, int SettingType = 1)
        {
            try
            {
                Payments.Configuration pcfg = new Payments.Configuration();
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@WineryID", wineryID));
                parameterList.Add(GetParameter("@PaymentGateway", PaymentGateway));
                parameterList.Add(GetParameter("@SettingType", SettingType));

                string sql = "select PaymentGateway, UserName, [Password], UserConfig1, UserConfig2, Mode from PaymentConfig p where p.Winery_ID = @WineryID and p.SettingType = @SettingType and p.PaymentGateway = @PaymentGateway";

                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            pcfg.MerchantLogin = Convert.ToString(dataReader["UserName"]);
                            pcfg.MerchantPassword = Convert.ToString(dataReader["Password"]);
                            pcfg.UserConfig1 = Convert.ToString(dataReader["UserConfig1"]);
                            pcfg.UserConfig2 = Convert.ToString(dataReader["UserConfig2"]);
                            pcfg.PaymentGateway = Convert.ToInt32(dataReader["PaymentGateway"]);
                            pcfg.GatewayMode = (Configuration.Mode)Convert.ToInt32(dataReader["Mode"]);
                        }
                    }
                }
                return pcfg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public int SaveReservationPaymentV2(int wineryID, int invoiceID, Payments.TransactionResult tr)
        {
            try
            {
                if (tr.Card.CustName == null)
                {
                    tr.Card.CustName = "";
                }

                if (!string.IsNullOrWhiteSpace(tr.Card.Number) && tr.Card.Number.Length > 4)
                {
                    tr.Card.Number = Common.Common.Right(tr.Card.Number, 4);
                }
                int retval = 0;
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@Reservation_Id", invoiceID));
                parameterList.Add(GetParameter("@PaymentDate", DateTime.UtcNow));
                parameterList.Add(GetParameter("@PayType", tr.PayType));
                parameterList.Add(GetParameter("@TransactionType", tr.TransactionType));
                parameterList.Add(GetParameter("@Status", tr.Status));
                parameterList.Add(GetParameter("@ResponseCode", tr.ResponseCode ?? ""));
                parameterList.Add(GetParameter("@Detail", tr.Detail ?? ""));
                parameterList.Add(GetParameter("@Amount", tr.Amount));
                parameterList.Add(GetParameter("@PayCardType", tr.Card.Type ?? ""));
                parameterList.Add(GetParameter("@PayCardNumber", StringHelpers.Encryption(tr.Card.Number ?? "")));
                parameterList.Add(GetParameter("@PayCardCustName", tr.Card.CustName ?? ""));
                parameterList.Add(GetParameter("@PayCardExpMonth", tr.Card.ExpMonth ?? ""));
                parameterList.Add(GetParameter("@PayCardExpYear", tr.Card.ExpYear ?? ""));
                parameterList.Add(GetParameter("@CreatedBy", tr.ProcessedBy));
                parameterList.Add(GetParameter("@ApprovalCode", tr.ApprovalCode ?? ""));
                parameterList.Add(GetParameter("@TransactionId", tr.TransactionID ?? ""));
                parameterList.Add(GetParameter("@AVSResponse", tr.AvsResponse ?? ""));
                parameterList.Add(GetParameter("@PayCardToken", tr.Card.CardToken ?? ""));
                parameterList.Add(GetParameter("@CardLastFourDigits", tr.Card.CardLastFourDigits ?? ""));
                parameterList.Add(GetParameter("@CardFirstFourDigits", tr.Card.CardFirstFourDigits ?? ""));
                parameterList.Add(GetParameter("@CardEntry", (int)tr.Card.CardEntry));
                parameterList.Add(GetParameter("@ApplicationType", (int)tr.Card.ApplicationType));
                parameterList.Add(GetParameter("@ApplicationVersion", tr.Card.ApplicationVersion));
                parameterList.Add(GetParameter("@TerminalID", tr.Card.TerminalId));
                parameterList.Add(GetParameter("@CardReader", tr.Card.CardReader));

                retval = ExecuteNonQuery("InsertReservationV2Payment", parameterList, CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                logDAL.InsertLog("SaveReservationPaymentV2", ex.Message, "", 1, wineryID);
            }
            return 0;
        }

        public int GetEventRulesBySlotId(int SlotId)
        {
            int EventId = 0;
            string sql = "select EventId from EventRules where EventRuleId=@EventRuleId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventRuleId", SlotId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        EventId = Convert.ToInt32(dataReader["EventId"]);
                    }
                }
            }
            return EventId;
        }

        public int GetExceptionBySlotId(int SlotId)
        {
            int EventId = 0;
            string sql = "select EventId from EventExceptions where ExceptionId=@ExceptionId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ExceptionId", SlotId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        EventId = Convert.ToInt32(dataReader["EventId"]);
                    }
                }
            }
            return EventId;
        }

        public bool UpdateReservation(int reservation_id, decimal amount_paid, decimal GratuityAmount = 0)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", reservation_id));
            parameterList.Add(GetParameter("@AmountPaid", amount_paid));
            parameterList.Add(GetParameter("@GratuityAmount", GratuityAmount));
            int retvalue = ExecuteNonQuery("Reservation_UPDATE", parameterList, CommandType.StoredProcedure);
            return retvalue > 0;
        }
        public EventRuleDetailsModel GetEventRulesbyId(int EventRuleId)
        {
            var model = new EventRuleDetailsModel();
            string FirstName = string.Empty, LastName = string.Empty, Company = string.Empty, HomePhoneStr = string.Empty;
            string sql = "select [DaysOfWeek],TotalSeats,MinPersons,StartTime,EndTime,Status from EventRules where EventRuleId =@EventRuleId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventRuleId", EventRuleId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.DaysOfWeek = Convert.ToInt16(dataReader["DaysOfWeek"]);
                        model.eventmodel.TotalSeats = Convert.ToInt32(dataReader["TotalSeats"]);
                        model.MinPersons = Convert.ToInt16(dataReader["MinPersons"]);
                        model.eventmodel.StartTime = TimeSpan.Parse(Convert.ToString(dataReader["StartTime"]));
                        model.eventmodel.EndTime = TimeSpan.Parse(Convert.ToString(dataReader["EndTime"]));
                        model.Status = Convert.ToBoolean(dataReader["Status"]);
                    }
                }
            }
            return model;
        }

        public int GetEventExceptionbyIdAndStatus(int ExceptionId, int Status)
        {
            int TotalSeats = 0;
            string sql = "select TotalSeats from [EventExceptions] Where EventId=@EventId and Status = @Status";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ExceptionId", ExceptionId));
            parameterList.Add(GetParameter("@Status", Status));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TotalSeats = Convert.ToInt32(dataReader["TotalSeats"]);
                    }
                }
            }
            return TotalSeats;
        }
        public int GetReservationGuestCount(int SlotId, int SlotType, DateTime EventDate)
        {
            int guestSum = 0;
            int cancelled = (int)Common.Email.ReservationStatus.Cancelled;
            int initiated = (int)Common.Email.ReservationStatus.Initiated;
            int yelpinitiated = (int)Common.Email.ReservationStatus.YelpInitiated;
            string sql = "select TotalGuests from [ReservationV2] Where SlotId=@SlotId and SlotType=@SlotType and EventDate=@EventDate and Status<>@Cancelled and Status<>@Initiated and Status<>@YelpInitiated";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@SlotType", SlotType));
            parameterList.Add(GetParameter("@EventDate", EventDate));
            parameterList.Add(GetParameter("@Cancelled", cancelled));
            parameterList.Add(GetParameter("@Initiated", initiated));
            parameterList.Add(GetParameter("@YelpInitiated", yelpinitiated));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        guestSum += Convert.ToInt32(dataReader["TotalGuests"]);
                    }
                }
            }
            return guestSum;
        }

        public decimal GetPreviousDepositByReservationID(int ReservationId)
        {
            decimal amountpaid = 0;

            string sql = "select dbo.GetRsvpBalanceDue(@ReservationId) as amountpaid";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        amountpaid = Convert.ToDecimal(dataReader["amountpaid"]);
                    }
                }
            }
            return amountpaid;
        }

        public bool UpdateReservationNotes(int ReservationID, string concierge_note, string internal_note, string guest_note)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationID", ReservationID));
            parameterList.Add(GetParameter("@concierge_note", concierge_note));
            parameterList.Add(GetParameter("@internal_note", internal_note));
            parameterList.Add(GetParameter("@guest_note", guest_note));

            string sqlQuery = "update ReservationV2 set ConciergeNote =@concierge_note,InternalNote=@internal_note,Note=@guest_note where ReservationId=@ReservationID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdateRsvpSurveyWaiverStatus(int memberId, int reservationID, string email, int InviteType, RSVPPostCaptureStatus inviteStatus)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@InviteStatus", (short)inviteStatus));
            parameterList.Add(GetParameter("@MemberId", memberId));
            parameterList.Add(GetParameter("@ReservationID", reservationID));
            parameterList.Add(GetParameter("@email", email));
            parameterList.Add(GetParameter("@InviteType", InviteType));
            //inviteStatus

            string sqlQuery = "UpdateRsvpSurveyWaiverStatus";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.StoredProcedure) > 0;
        }

        public bool UpdateReservationPaymentV2(int ID, decimal RefundAmount)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ID", ID));
            parameterList.Add(GetParameter("@RefundAmount", RefundAmount));

            string sqlQuery = "UPDATE [ReservationV2Payment] SET RefundAmount = RefundAmount + @RefundAmount  WHERE Id = @ID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdatePreAssignedServerTable(int ReservationID, int? PreAssign_Server_Id, string PreAssign_Table_Id)
        {
            var parameterList = new List<DbParameter>();
            bool isSucess = false;
            if (PreAssign_Server_Id.HasValue && PreAssign_Server_Id.Value > 0)
            {
                isSucess = UpdatePreAssignedServer(ReservationID, PreAssign_Server_Id.Value);
            }
            if (!string.IsNullOrWhiteSpace(PreAssign_Table_Id))
            {
                isSucess = UpdatePreAssignedTable(ReservationID, PreAssign_Table_Id);
            }
            //if (!PreAssign_Server_Id.HasValue && string.IsNullOrWhiteSpace(PreAssign_Table_Id))
            //{
            //    parameterList.Add(GetParameter("@ReservationID", ReservationID));

            //    string sqlQuery = "update ReservationV2 set PreAssign_Server_Id =@0, PreAssign_Table_Id='[]' where ReservationId=@ReservationID";

            //    isSucess= ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
            //}

            return isSucess;


        }

        public bool SwappingPreAssignTableIdByTranId(int TableId, int TranId, int tranType, bool forceAssign, string PreAssignTableIds)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@TableId", TableId));
            parameterList.Add(GetParameter("@TranId", TranId));
            parameterList.Add(GetParameter("@tranType", tranType));
            parameterList.Add(GetParameter("@forceAssign", forceAssign));
            parameterList.Add(GetParameter("@PreAssignTableIds", PreAssignTableIds));

            string sqlQuery = "SwappingPreAssignTableIdByTranId";

            bool sessionStarted = (bool)ExecuteScalar(sqlQuery, parameterList);

            return sessionStarted;
        }

        public bool ReassignTable(int transactionId, PreAssignServerTransactionType transType, string tableIds)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@TranId", transactionId));
            parameterList.Add(GetParameter("@TranType", (int)transType));
            parameterList.Add(GetParameter("@TableIds", tableIds));

            string sqlQuery = "ReAssignTables";

            bool sessionStarted = (bool)ExecuteScalar(sqlQuery, parameterList);

            return sessionStarted;
        }

        public bool UpdatePreAssignedServer(int ReservationID, int PreAssign_Server_Id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationID", ReservationID));
            parameterList.Add(GetParameter("@PreAssign_Server_Id", PreAssign_Server_Id));

            string sqlQuery = "update ReservationV2 set ServerSoftAssigned= case when PreAssign_Server_Id =@PreAssign_Server_Id then ServerSoftAssigned else 0 end, PreAssign_Server_Id =@PreAssign_Server_Id where ReservationId=@ReservationID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdatePreAssignedTable(int ReservationID, string PreAssign_Table_Id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationID", ReservationID));
            parameterList.Add(GetParameter("@PreAssign_Table_Id", PreAssign_Table_Id));

            string sqlQuery = "update ReservationV2 set TablesSoftAssigned=0, PreAssign_Table_Id=@PreAssign_Table_Id where ReservationId=@ReservationID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdateWaitlistPreAssignedServerTable(int waitListId, int durationMinutes, int PreAssign_Server_Id, string PreAssign_Table_Id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WaitListId", waitListId));
            parameterList.Add(GetParameter("@PreAssignServerId", PreAssign_Server_Id));
            parameterList.Add(GetParameter("@PreAssignTableId", PreAssign_Table_Id));
            parameterList.Add(GetParameter("@DurationMinutes", durationMinutes));

            string sqlQuery = "UpdateWaitList";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.StoredProcedure) > 0;
        }

        public bool IsEnableUpdate3rdParty(int memberId)
        {
            bool EnableUpdate3rdParty = false;
            string sql = "select EnableUpdate3rdParty from Winery where Id= @wineryId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@wineryId", memberId));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        EnableUpdate3rdParty = Convert.ToBoolean(dataReader["EnableUpdate3rdParty"]);
                    }
                }
            }
            return EnableUpdate3rdParty;
        }

        public bool UpdateReservationStatus(int ReservationID, int Status)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationID", ReservationID));
            parameterList.Add(GetParameter("@Status", Status));

            string sqlQuery = "update ReservationV2 set [Status] =@Status,[StatusChangeDate] =GETUTCDATE() where ReservationId=@ReservationID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public ReservationSummaryModel GetReservationSummary(int SlotId, int SlotType, DateTime EventDate)
        {
            var reservationSummaryModel = new ReservationSummaryModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@SlotType", SlotType));
            parameterList.Add(GetParameter("@EventDate", EventDate));
            using (DbDataReader dataReader = GetDataReader("GetReservationSummary", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        reservationSummaryModel.MemberName = Convert.ToString(dataReader["MemberName"]);
                        reservationSummaryModel.RegionName = Convert.ToString(dataReader["RegionName"]);
                        reservationSummaryModel.EventDate = Convert.ToDateTime(dataReader["EventDate"]);
                        reservationSummaryModel.EventDateEnd = Convert.ToDateTime(dataReader["EventDateEnd"]);
                        reservationSummaryModel.EventName = Convert.ToString(dataReader["EventName"]);
                        reservationSummaryModel.LocationName = Convert.ToString(dataReader["LocationName"]);
                        reservationSummaryModel.PurchaseURL = Convert.ToString(dataReader["PurchaseURL"]);
                        reservationSummaryModel.RegionUrl = Convert.ToString(dataReader["RegionUrl"]);
                        reservationSummaryModel.CancelPolicy = Convert.ToString(dataReader["CancelPolicy"]);
                        reservationSummaryModel.DestinationName = Convert.ToString(dataReader["DestinationName"]);
                    }
                }
            }
            return reservationSummaryModel;
        }

        public Event_AbandonedModel GetRSVPAbandonedCart(int CartId)
        {
            var event_AbandonedModel = new Event_AbandonedModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", CartId));
            using (DbDataReader dataReader = GetDataReader("GetRSVPAbandonedCart", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        event_AbandonedModel.Id = Convert.ToInt32(dataReader["Id"]);
                        event_AbandonedModel.User_Id = Convert.ToInt32(dataReader["user_id"]);
                        event_AbandonedModel.Email = Convert.ToString(dataReader["Email"]);
                        event_AbandonedModel.DateAdded = Convert.ToDateTime(dataReader["DateAdded"]);
                        event_AbandonedModel.Slot_Id = Convert.ToInt32(dataReader["Slot_Id"]);
                        event_AbandonedModel.Slot_Type = Convert.ToInt32(dataReader["Slot_Type"]);
                        event_AbandonedModel.GuestCount = Convert.ToInt32(dataReader["GuestCount"]);
                        event_AbandonedModel.DateRequested = Convert.ToDateTime(dataReader["DateRequested"]);
                        event_AbandonedModel.SendEmailNotice = Convert.ToBoolean(dataReader["SendEmailNotice"]);
                        event_AbandonedModel.EmailSent = Convert.ToBoolean(dataReader["EmailSent"]);
                        event_AbandonedModel.ConvertedReservationId = Convert.ToInt32(dataReader["ConvertedReservationId"]);
                        event_AbandonedModel.Member_Id = Convert.ToInt32(dataReader["Member_Id"]);
                        event_AbandonedModel.ReferralId = Convert.ToInt32(dataReader["ReferralId"]);
                        event_AbandonedModel.isWidget = Convert.ToBoolean(dataReader["isWidget"]);
                        event_AbandonedModel.FirstName = Convert.ToString(dataReader["FirstName"]);
                        event_AbandonedModel.LastName = Convert.ToString(dataReader["LastName"]);
                    }
                }
            }
            return event_AbandonedModel;
        }

        public Tickets_AbandonedModel GetTicketAbandonedCart(int CartId)
        {
            var tickets_AbandonedModel = new Tickets_AbandonedModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", CartId));
            using (DbDataReader dataReader = GetDataReader("GetTicketAbandonedCart", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        tickets_AbandonedModel.FirstName = Convert.ToString(dataReader["FirstName"]);
                        tickets_AbandonedModel.LastName = Convert.ToString(dataReader["LastName"]);
                        tickets_AbandonedModel.Id = Convert.ToInt32(dataReader["Id"]);
                        tickets_AbandonedModel.User_Id = Convert.ToInt32(dataReader["User_Id"]);
                        tickets_AbandonedModel.Email = Convert.ToString(dataReader["Email"]);
                        tickets_AbandonedModel.Event_Id = Convert.ToInt32(dataReader["Event_Id"]);
                        tickets_AbandonedModel.EventTitle = Convert.ToString(dataReader["EventTitle"]);
                        tickets_AbandonedModel.StartDateTime = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tickets_AbandonedModel.EndDateTime = Convert.ToDateTime(dataReader["EndDateTime"]);
                        tickets_AbandonedModel.EventOrganizerName = Convert.ToString(dataReader["EventOrganizerName"]);
                        tickets_AbandonedModel.VenueName = Convert.ToString(dataReader["VenueName"]);
                        tickets_AbandonedModel.VenueAddress = Convert.ToString(dataReader["VenueAddress"]);
                        tickets_AbandonedModel.TicketsEventId = Convert.ToInt32(dataReader["TicketsEventId"]);
                        tickets_AbandonedModel.DateAdded = Convert.ToDateTime(dataReader["DateAdded"]);
                        tickets_AbandonedModel.TimeZoneId = Convert.ToInt32(dataReader["TimeZoneId"]);
                        tickets_AbandonedModel.SendEmailNotice = Convert.ToBoolean(dataReader["SendEmailNotice"]);
                        tickets_AbandonedModel.EmailSent = Convert.ToBoolean(dataReader["EmailSent"]);
                        tickets_AbandonedModel.ConvertedOrderId = Convert.ToInt32(dataReader["ConvertedOrderId"]);
                        tickets_AbandonedModel.Member_Id = Convert.ToInt32(dataReader["Member_Id"]);
                        tickets_AbandonedModel.AcccessCode = Convert.ToString(dataReader["AcccessCode"]);
                        tickets_AbandonedModel.Promo = Convert.ToString(dataReader["Promo"]);
                        tickets_AbandonedModel.ReferralId = Convert.ToInt32(dataReader["ReferralId"]);
                        tickets_AbandonedModel.isWidget = Convert.ToBoolean(dataReader["isWidget"]);
                        tickets_AbandonedModel.MemberName = Convert.ToString(dataReader["MemberName"]);
                        tickets_AbandonedModel.RegionName = Convert.ToString(dataReader["RegionName"]);
                        tickets_AbandonedModel.PurchaseURL = Convert.ToString(dataReader["PurchaseURL"]);
                        tickets_AbandonedModel.RegionUrl = Convert.ToString(dataReader["RegionUrl"]);
                        tickets_AbandonedModel.EventURL = Convert.ToString(dataReader["EventURL"]);
                    }
                }
            }
            return tickets_AbandonedModel;
        }

        public ReservationModel GetWaitlistStartEndTime(int reservationid)
        {
            ReservationModel reservationModel = null;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationid", reservationid));
            using (DbDataReader dataReader = GetDataReader("GetWaitlistStartEndTime", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        reservationModel = new ReservationModel();
                        reservationModel.seating_start_time = Convert.ToDateTime(dataReader["SessionDateTime"]);
                        if (Convert.ToString(dataReader["StatusDateTime"]).Length > 0)
                        {
                            reservationModel.seating_end_time = Convert.ToDateTime(dataReader["StatusDateTime"]);
                        }
                    }
                }
            }
            return reservationModel;
        }

        public bool UpdatePartySize(int ReservationID, int party_size)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationID", ReservationID));
            parameterList.Add(GetParameter("@party_size", party_size));

            string sqlQuery = "update ReservationV2 set PreAssign_Server_Id =@PreAssign_Server_Id,PreAssign_Table_Id=@PreAssign_Table_Id where ReservationId=@ReservationID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public List<int> GetAvailableReservationsAbandonedForNotifications()
        {
            List<int> ids = new List<int>();
            using (DbDataReader dataReader = GetDataReader("GetAvailableReservationsAbandonedForNotifications", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    try
                    {
                        while (dataReader.Read())
                        {
                            int id = Convert.ToInt32(dataReader["id"]);
                            ids.Add(id);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                    }

                }
            }
            return ids;
        }

        public List<int> GetAvailableTicketsAbandonedForNotifications()
        {
            List<int> ids = new List<int>();

            using (DbDataReader dataReader = GetDataReader("GetAvailableTicketsAbandonedForNotifications", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int id = Convert.ToInt32(dataReader["id"]);
                        ids.Add(id);
                    }
                }
            }
            return ids;
        }

        public bool UpdateTicketAbandonedSent(int Id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            string sqlQuery = "update Tickets_Abandoned set emailsent =1 where Id=@ID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdateReservationAbandonedSent(int Id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            string sqlQuery = "update Event_Abandoned set emailsent =1 where Id=@ID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public List<CalendarModel> GetCalendarNotes(int WineryID, DateTime Notedate)
        {
            var model = new List<CalendarModel>();

            string sql = "select NoteId,Subject,Note,starttime,endtime,notedate from CalendarNote where wineryid=@WineryID and dbo.dateonly(notedate)=dbo.dateonly(@Notedate) order by starttime,Subject";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));
            parameterList.Add(GetParameter("@Notedate", Notedate));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        CalendarModel calendarModel = new CalendarModel();
                        calendarModel.id = Convert.ToInt32(dataReader["NoteId"]);
                        calendarModel.text = Convert.ToString(dataReader["Note"]);
                        calendarModel.title = Convert.ToString(dataReader["Subject"]);
                        calendarModel.start_date = Convert.ToDateTime(dataReader["notedate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["starttime"])));
                        calendarModel.end_date = Convert.ToDateTime(dataReader["notedate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["endtime"])));
                        model.Add(calendarModel);
                    }
                }
            }
            return model;
        }

        public int CreateUser(int NoteId, int WineryId, DateTime NoteDate, TimeSpan StartTime, TimeSpan EndTime, string Subject, string Note)
        {
            int id = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@NoteId", NoteId));
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@NoteDate", NoteDate));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@EndTime", EndTime));
            parameterList.Add(GetParameter("@Subject", Subject));
            parameterList.Add(GetParameter("@Note", Note));

            id = Convert.ToInt32(ExecuteScalar("UpdateCalendarNote", parameterList));

            return id;
        }

        public PayloadOrderModel GetOrderPortSendOrder(int ReservationId)
        {
            var orders = new PayloadOrderModel();
            List<ProductModel> list = new List<ProductModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader("GetOrderPortSendOrder", parameterList, CommandType.StoredProcedure))
            {
                double TaxAmt = 0;
                double OrgDiscountAmt = 0;
                double DiscountAmt = 0;
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        ProductModel product = new ProductModel();
                        product.Sku = Convert.ToString(dataReader["Sku"]);
                        product.Quantity = Convert.ToInt32(dataReader["Qty"]);
                        //product.UnitPrice = Convert.ToDouble(dataReader["Price"]);
                        OrgDiscountAmt = Convert.ToDouble(dataReader["DiscountAmt"]);
                        if (DiscountAmt == 0)
                        {
                            DiscountAmt = OrgDiscountAmt;
                        }

                        if (DiscountAmt > 0)
                        {
                            product.UnitPrice = Convert.ToDouble(dataReader["Price"]) - (DiscountAmt / Convert.ToInt32(dataReader["Qty"]));
                            DiscountAmt = DiscountAmt - (product.UnitPrice * product.Quantity);

                            if (product.UnitPrice < 0)
                            {
                                product.UnitPrice = 0;
                            }
                        }
                        else
                        {
                            product.UnitPrice = Convert.ToDouble(dataReader["Price"]);
                        }

                        product.UnitOriginalPrice = Convert.ToDouble(dataReader["Price"]);
                        product.UnitCostOfGood = 0.00;
                        product.IsTaxable = Convert.ToBoolean(dataReader["Taxable"]);
                        product.TaxAmount = Convert.ToDouble(dataReader["TaxAmt"]);

                        TaxAmt = TaxAmt + Convert.ToDouble(dataReader["TaxAmt"]);

                        list.Add(product);
                    }
                }

                if (dataReader.NextResult())
                {
                    while (dataReader.Read())
                    {
                        orders.AltOrderNumber = Convert.ToString(dataReader["bookingcode"]);
                        orders.CustomerUuid = Convert.ToString(dataReader["CustomerReferenceNumber"]);
                        orders.AltCustomerNumber = Convert.ToString(dataReader["UserId"]);

                        orders.OrderDateTime = Convert.ToDateTime(dataReader["BookingDate"]).AddMinutes(Convert.ToInt32(Common.Times.GetOffsetMinutes((Common.Times.TimeZone)Convert.ToInt32(dataReader["timezoneid"]))));
                        orders.SaleDateTime = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["starttime"])));

                        orders.GiftMessage = Convert.ToString(dataReader["Note"]);
                        orders.OrderNotes = Convert.ToString(dataReader["InternalNote"]);

                        orders.BillingAddress.FirstName = Convert.ToString(dataReader["BillingFirstName"]);
                        orders.BillingAddress.LastName = Convert.ToString(dataReader["BillingLastName"]);
                        orders.BillingAddress.Company = Convert.ToString(dataReader["BillingCompanyName"]);
                        orders.BillingAddress.Address1 = Convert.ToString(dataReader["BillingAddress1"]);
                        orders.BillingAddress.Address2 = Convert.ToString(dataReader["BillingAddress2"]);
                        orders.BillingAddress.City = Convert.ToString(dataReader["BillingCity"]);
                        orders.BillingAddress.State = Convert.ToString(dataReader["BillingState"]);
                        orders.BillingAddress.ZipCode = Convert.ToString(dataReader["BillingZip"]);
                        orders.BillingAddress.Country = Convert.ToString(dataReader["BillingCountry"]);
                        //orders.BillingAddress.BirthDate = Convert.ToDateTime("1982-02-16");
                        orders.BillingAddress.Email = Convert.ToString(dataReader["BillingEmail"]);
                        orders.BillingAddress.Phone = Convert.ToString(dataReader["BillingPhoneNumber"]);
                        orders.BillingAddress.Fax = null;

                        orders.ShippingAddress.FirstName = Convert.ToString(dataReader["ShippingFirstName"]);
                        orders.ShippingAddress.LastName = Convert.ToString(dataReader["ShippingLastName"]);
                        orders.ShippingAddress.Company = Convert.ToString(dataReader["ShippingCompanyName"]);
                        orders.ShippingAddress.Address1 = Convert.ToString(dataReader["ShippingAddress1"]);
                        orders.ShippingAddress.Address2 = Convert.ToString(dataReader["ShippingAddress2"]);
                        orders.ShippingAddress.City = Convert.ToString(dataReader["ShippingCity"]);
                        orders.ShippingAddress.State = Convert.ToString(dataReader["ShippingState"]);
                        orders.ShippingAddress.ZipCode = Convert.ToString(dataReader["ShippingZip"]);
                        orders.ShippingAddress.Country = Convert.ToString(dataReader["ShippingCountry"]);
                        //orders.ShippingAddress.BirthDate = Convert.ToDateTime("1982-02-16");
                        orders.ShippingAddress.Email = Convert.ToString(dataReader["ShippingEmail"]);
                        orders.ShippingAddress.Phone = Convert.ToString(dataReader["ShippingPhone"]);
                        orders.ShippingAddress.Fax = Convert.ToString(dataReader["ShippingFaxPhone"]);

                        orders.SubTotal = Convert.ToDouble(dataReader["SubTotal"]);
                        orders.Discount = Convert.ToDouble(dataReader["DiscountAmt"]);
                        orders.Shipping = 0;
                        orders.Handling = 0;
                        orders.TotalTax = Convert.ToDouble(dataReader["SalesTax"]);
                        orders.GrandTotal = Convert.ToDouble(dataReader["GrandTotal"]);

                        ProductModel product = new ProductModel();

                        product.Sku = Convert.ToString(dataReader["RMSsku"]);
                        product.Quantity = Convert.ToInt32(dataReader["TotalGuests"]);
                        product.UnitPrice = Convert.ToDouble(dataReader["FeePerPerson"]) - (orders.Discount / Convert.ToInt32(dataReader["TotalGuests"]));
                        if (product.UnitPrice < 0)
                        {
                            product.UnitPrice = 0;
                        }

                        product.UnitOriginalPrice = Convert.ToDouble(dataReader["FeePerPerson"]);
                        product.UnitCostOfGood = Convert.ToDouble(dataReader["Cost"]);

                        //if (product.UnitCostOfGood > product.UnitOriginalPrice)
                        //{
                        //    product.UnitCostOfGood = product.UnitOriginalPrice;
                        //}

                        product.IsTaxable = Convert.ToBoolean(dataReader["Taxable"]);
                        product.TaxAmount = (orders.TotalTax - TaxAmt);

                        list.Add(product);

                        //orders.ShippingService = null;
                        orders.Payment.Amount = Convert.ToDecimal(dataReader["amountpaid"]);
                        orders.Payment.PaymentMethod = enumPaymentMethod.Cash.ToString();
                        if (orders.Payment.Amount > 0)
                        {
                            orders.Payment.PaymentMethod = enumPaymentMethod.CreditCard.ToString();
                            orders.Payment.CreditCardNumber = StringHelpers.Decryption(Convert.ToString(dataReader["PayCardNumber"]));
                            if (!string.IsNullOrEmpty(orders.Payment.CreditCardNumber))
                            {
                                orders.Payment.CreditCardNumber = Common.Common.Right(orders.Payment.CreditCardNumber, 4);
                            }

                            orders.Payment.CreditCardType = Convert.ToString(dataReader["PayCardType"]);

                            if (orders.Payment.CreditCardType.ToLower().IndexOf("visa") > -1)
                            {
                                orders.Payment.CreditCardType = "Visa";
                            }
                            else if (orders.Payment.CreditCardType.ToLower().IndexOf("master") > -1)
                            {
                                orders.Payment.CreditCardType = "MasterCard";
                            }
                            else if (orders.Payment.CreditCardType.ToLower().IndexOf("discover") > -1)
                            {
                                orders.Payment.CreditCardType = "Discover";
                            }
                            else if (orders.Payment.CreditCardType.ToLower().IndexOf("diners") > -1)
                            {
                                orders.Payment.CreditCardType = "Diners";
                            }
                            else if (orders.Payment.CreditCardType.ToLower().IndexOf("american") > -1)
                            {
                                orders.Payment.CreditCardType = "AmericanExpress";
                            }

                            orders.Payment.CreditCardName = Convert.ToString(dataReader["PayCardCustName"]);
                            orders.Payment.CreditCardExpireMonth = Convert.ToInt16(Convert.ToString(dataReader["PayCardExpMonth"]).TrimStart('0'));
                            orders.Payment.CreditCardExpireYear = Convert.ToInt16(dataReader["PayCardExpYear"]);
                        }
                    }
                }

                orders.LineItems = list.ToArray();
            }
            return orders;
        }

        public Commerce7OrderModel GetOrderDataCommerce7V2(int ReservationId,bool isUpsertFulfillmentEnabled, ref string posProfileId)
        {
            var orders = new Commerce7OrderModel();
            List<Commerce7Item> list = new List<Commerce7Item>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader("GetOrderPortSendOrder", parameterList, CommandType.StoredProcedure))
            {
                double TaxAmt = 0;
                double OrgDiscountAmt = 0;
                double DiscountAmt = 0;
                int calcPrice = 0;
                if (dataReader != null)
                {
                    //addons
                    while (dataReader.Read())
                    {
                        Commerce7Item product = new Commerce7Item();
                        product.sku = Convert.ToString(dataReader["Sku"]);
                        product.quantity = product.quantityFulfilled = Convert.ToInt32(dataReader["Qty"]);

                        if (isUpsertFulfillmentEnabled)
                        {
                            product.quantityFulfilled = 0;
                        }

                        OrgDiscountAmt = Convert.ToDouble(dataReader["DiscountAmt"]);
                        double price = Convert.ToDouble(dataReader["Price"]);
                        if (DiscountAmt == 0)
                        {
                            DiscountAmt = OrgDiscountAmt;
                        }

                        if (DiscountAmt > 0)
                        {
                            double discountedPrice = price - (DiscountAmt / Convert.ToInt32(dataReader["Qty"]));
                            product.price = int.Parse(Math.Round((Math.Round(discountedPrice, 2) * 100),0).ToString());
                            DiscountAmt = DiscountAmt - (discountedPrice * product.quantity);

                            if (product.price < 0)
                            {
                                product.price = 0;
                            }
                        }
                        else
                        {
                            product.price = int.Parse(Math.Round((price * 100),0).ToString());
                        }
                        calcPrice += product.price * product.quantity;
                        product.comparePrice = product.price;

                        product.originalPrice = int.Parse(Math.Round((price * 100),0).ToString());

                        double Cost = Convert.ToDouble(dataReader["Cost"]);

                        product.costOfGood = int.Parse(Math.Round((Cost * 100),0).ToString());

                        product.productTitle = Convert.ToString(dataReader["Name"]).Trim();
                        if (product.productTitle.Length > 256)
                            product.productTitle = product.productTitle.Left(256);

                        product.productVariantTitle = product.productTitle;

                        double addonTax = Convert.ToDouble(dataReader["TaxAmt"]);
                        product.tax = int.Parse(Math.Round((addonTax * 100),0).ToString());

                        string ItemTypeName = Convert.ToString(dataReader["ItemTypeName"]);

                        if (!string.IsNullOrEmpty(ItemTypeName))
                            product.type = ItemTypeName;

                        string TaxClassName = Convert.ToString(dataReader["TaxClassName"]);

                        if (!string.IsNullOrEmpty(TaxClassName))
                            product.taxType = TaxClassName;

                        int volumeInML = Convert.ToInt32(dataReader["volumeInML"]);

                        if (product.taxType == "Wine")
                            product.volumeInML = volumeInML;

                        string DepartmentCode = Convert.ToString(dataReader["DepartmentCode"]);

                        if (!string.IsNullOrEmpty(DepartmentCode))
                            product.departmentCode = DepartmentCode;

                        TaxAmt = TaxAmt + addonTax;

                        list.Add(product);
                    }
                }

                if (dataReader.NextResult())
                {
                    while (dataReader.Read())
                    {
                        orders.channel = Convert.ToString(dataReader["SalesChannelMapping"]);
                        orders.externalOrderNumber = Convert.ToString(dataReader["bookingcode"]);
                        orders.customerId = Convert.ToString(dataReader["CustomerReferenceNumber"]);

                        DateTime dtPaymentDate = Convert.ToDateTime(dataReader["PaymentDate"]);
                        DateTime dtBookingate = Convert.ToDateTime(dataReader["BookingDate"]);
                        DateTime dtCurrentDate = System.DateTime.UtcNow;

                        double grandTotal = Convert.ToDouble(dataReader["GrandTotal"]);
                        int UpsertShipDateAs = Convert.ToInt32(dataReader["UpsertShipDateAs"]);
                        int UpsertOrderDateAs = Convert.ToInt32(dataReader["UpsertOrderDateAs"]);
                        DateTime EventDate = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        EventDate = EventDate.AddMinutes(Convert.ToInt32(Common.Times.GetOffsetMinutes((Common.Times.TimeZone)Convert.ToInt32(dataReader["timezoneid"]))) * -1);

                        if (grandTotal>0)
                            orders.orderPaidDate = dtPaymentDate.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z";
                        else
                            orders.orderPaidDate = EventDate.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z";

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["Commerce7POSProfileId"])))
                        {
                            posProfileId = Convert.ToString(dataReader["Commerce7POSProfileId"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["Commerce7OrderSource"])))
                        {
                            MetaDataModel metaData = new MetaDataModel();

                            metaData.ordersource = Convert.ToString(dataReader["Commerce7OrderSource"]);

                            orders.metaData = metaData;
                        }

                        switch (UpsertOrderDateAs)
                        {
                            case 0:
                                orders.orderSubmittedDate = dtCurrentDate.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z";
                                break;
                            case 1:
                                orders.orderSubmittedDate = dtBookingate.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z";
                                break;
                            case 2:
                                orders.orderSubmittedDate = EventDate.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z";
                                break;
                            case 3:
                                if (grandTotal > 0)
                                    orders.orderSubmittedDate = dtPaymentDate.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z";
                                else
                                    orders.orderSubmittedDate = EventDate.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z";

                                break;
                        }

                        if (UpsertShipDateAs == 1)
                        {
                            orders.orderFulfilledDate = dtBookingate.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z";
                        }
                        else
                        {
                            orders.orderFulfilledDate = EventDate.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z";
                        }
                        orders.giftMessage = Convert.ToString(dataReader["Note"]);
                        orders.email = Convert.ToString(dataReader["BillingEmail"]);
                        orders.posProfileId = posProfileId;

                        orders.billTo.firstName = Convert.ToString(dataReader["BillingFirstName"]);
                        orders.billTo.lastName = Convert.ToString(dataReader["BillingLastName"]);
                        orders.billTo.company = Convert.ToString(dataReader["BillingCompanyName"]);
                        orders.billTo.address = Convert.ToString(dataReader["BillingAddress1"]);
                        orders.billTo.address2 = Convert.ToString(dataReader["BillingAddress2"]);
                        orders.billTo.city = Convert.ToString(dataReader["BillingCity"]);
                        orders.billTo.stateCode = Convert.ToString(dataReader["BillingState"]);
                        orders.billTo.zipCode = Convert.ToString(dataReader["BillingZip"]);
                        orders.billTo.countryCode = Convert.ToString(dataReader["BillingCountry"]);
                        orders.billTo.phone = StringHelpers.FormatTelephoneCommerce7(Convert.ToString(dataReader["BillingPhoneNumber"]), orders.billTo.countryCode, true, PhoneNumbers.PhoneNumberFormat.E164, Convert.ToString(dataReader["ShippingPhone"]), Convert.ToString(dataReader["ShippingCountry"]));

                        double discountAmt = Convert.ToDouble(dataReader["DiscountAmt"]);

                        orders.shipTotal = 0;
                        double totalSalesTax = Convert.ToDouble(dataReader["SalesTax"]);
                        orders.taxTotal = int.Parse(Math.Round((totalSalesTax * 100),0).ToString());

                        //double subTotalAmt = Convert.ToDouble(dataReader["SubTotal"]);
                        double gratuityTotal = Convert.ToDouble(dataReader["GratuityAmount"]);
                        //orders.subTotal = int.Parse(((subTotalAmt - discountAmt - gratuityTotal) * 100).ToString());

                        orders.tipTotal = int.Parse(Math.Round((gratuityTotal * 100),0).ToString());

                        orders.taxes.Add(new Tax
                        {
                            countryCode = orders.billTo.countryCode,
                            stateCode = orders.billTo.stateCode,
                            vendor = "Local",
                            title = "Tax",
                            price = orders.taxTotal
                        });


                        Commerce7Item product = new Commerce7Item();

                        product.productTitle = Convert.ToString(dataReader["EventName"]);
                        if (product.productTitle.Length > 256)
                            product.productTitle = product.productTitle.Left(256);

                        product.productVariantTitle = product.productTitle;

                        if (!string.IsNullOrEmpty(Convert.ToString(dataReader["Commerce7InventoryLocationID"])))
                        {
                            product.inventoryLocationId = Convert.ToString(dataReader["Commerce7InventoryLocationID"]);
                        }

                        string ItemTypeName = Convert.ToString(dataReader["ItemTypeName"]);

                        if (!string.IsNullOrEmpty(ItemTypeName))
                            product.type = ItemTypeName;

                        string TaxClassName = Convert.ToString(dataReader["TaxClassName"]);

                        if (!string.IsNullOrEmpty(TaxClassName))
                            product.taxType = TaxClassName;

                        int volumeInML = Convert.ToInt32(dataReader["volumeInML"]);

                        if (product.taxType == "Wine")
                            product.volumeInML = volumeInML;

                        string DepartmentCode = Convert.ToString(dataReader["DepartmentCode"]);

                        if (!string.IsNullOrEmpty(DepartmentCode))
                            product.departmentCode = DepartmentCode;

                        product.sku = Convert.ToString(dataReader["RMSsku"]);
                        if (string.IsNullOrWhiteSpace(product.sku))
                            product.sku = product.productTitle;

                        int totGuests = Convert.ToInt32(dataReader["TotalGuests"]);

                        product.quantity = product.quantityFulfilled = totGuests;

                        if (isUpsertFulfillmentEnabled)
                        {
                            product.quantityFulfilled = 0;
                        }

                        double feePerPerson = Convert.ToDouble(dataReader["FeePerPerson"]);

                        product.price = int.Parse(Math.Round((Math.Round(Convert.ToDouble(feePerPerson - discountAmt / totGuests), 2) * 100),0).ToString());
                        if (product.price < 0)
                        {
                            product.price = 0;
                        }
                        calcPrice += product.price * totGuests;
                        product.comparePrice = product.price;
                        product.originalPrice = int.Parse(Math.Round((feePerPerson * 100),0).ToString());

                        double cost = Convert.ToDouble(dataReader["Cost"]);

                        if (cost > feePerPerson)
                        {
                            cost = feePerPerson;
                        }

                        product.costOfGood = int.Parse(Math.Round((cost * 100),0).ToString());

                        product.tax = (orders.taxTotal - int.Parse(Math.Round((TaxAmt * 100),0).ToString()));

                        product.quantity = product.quantityFulfilled = totGuests;

                        if (isUpsertFulfillmentEnabled)
                        {
                            product.quantityFulfilled = 0;
                        }

                        list.Add(product);

                        orders.subTotal = calcPrice;

                        orders.total = orders.subTotal + orders.taxTotal;

                        orders.totalAfterTip = orders.total + orders.tipTotal;

                        Tender payment = new Tender();

                        payment.tip = orders.tipTotal;
                        payment.amountTendered = orders.total;
                        payment.chargeType = "Sale";
                        payment.chargeStatus = "Success";
                        payment.paymentDate = orders.orderPaidDate;

                        if (payment.amountTendered > 0)
                        {
                            payment.tenderType = "Credit Card";
                            string cardNumber = StringHelpers.Decryption(Convert.ToString(dataReader["PayCardNumber"]));

                            if (!string.IsNullOrEmpty(cardNumber))
                            {

                                payment.creditCardExpiryMo = Convert.ToInt16(Convert.ToString(dataReader["PayCardExpMonth"]).TrimStart('0'));
                                payment.creditCardExpiryYr = Convert.ToInt16(dataReader["PayCardExpYear"]);
                                payment.creditCardHolderName = Convert.ToString(dataReader["PayCardCustName"]);

                                string maskedCCNum = Common.Common.Right(cardNumber, 4).PadLeft(16, '*');
                                payment.maskedCardNumber = maskedCCNum;
                                payment.creditCardBrand = Convert.ToString(dataReader["PayCardType"]).Replace(" ", "");

                                if (payment.creditCardBrand.ToLower().IndexOf("visa") > -1)
                                {
                                    payment.creditCardBrand = "Visa";
                                }
                                else if (payment.creditCardBrand.ToLower().IndexOf("master") > -1)
                                {
                                    payment.creditCardBrand = "MasterCard";
                                }
                                else if (payment.creditCardBrand.ToLower().IndexOf("discover") > -1)
                                {
                                    payment.creditCardBrand = "Discover";
                                }
                                else if (payment.creditCardBrand.ToLower().IndexOf("diners") > -1)
                                {
                                    payment.creditCardBrand = "Diners Club";
                                }
                                else if (payment.creditCardBrand.ToLower().IndexOf("american") > -1)
                                {
                                    payment.creditCardBrand = "American Express";
                                }
                            }
                            else
                            {
                                payment.tenderType = "Cash";
                            }
                            orders.tenders.Add(payment);
                        }
                    }
                }

                orders.items = list;
            }
            return orders;
        }

        public bool InsertExportLog(int ExportType, int Export_ID, string ExportDetails, int ExportStatus, decimal ExportAmount, string ProcessedBy)
        {
            int retval = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ExportType", ExportType));
            parameterList.Add(GetParameter("@Export_ID", Export_ID));
            parameterList.Add(GetParameter("@ExportDetails", ExportDetails));
            parameterList.Add(GetParameter("@ExportDate", DateTime.UtcNow));
            parameterList.Add(GetParameter("@ExportStatus", ExportStatus));
            parameterList.Add(GetParameter("@ExportAmount", ExportAmount));
            parameterList.Add(GetParameter("@ProcessedBy", ProcessedBy));
            retval = ExecuteNonQuery("InsertExportLog", parameterList, CommandType.StoredProcedure);

            return retval > 0;
        }

        public bool CheckIfOrderAlreadySynced(int reservationId, Common.Common.ExportType exportType, ref string externalOrder)
        {
            bool isSynced = false;
            string sql = "Select top 1 Id, detail from ExportLog where Export_ID=@reservationId and ExportStatus=1 and ExportType=@exportType";
            //sql += " where ReservationId = @ReservationId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", reservationId));
            parameterList.Add(GetParameter("@exportType", (int)exportType));
            var reader = GetDataReader(sql, parameterList, CommandType.Text);
            if (reader != null && reader.HasRows)
            {
                isSynced = true;
                while (reader.Read())
                {
                    externalOrder = Convert.ToString(reader["detail"]);
                    externalOrder = externalOrder.ToLower().Replace("success", "").Replace("-", "").Trim();
                }
            }

            return isSynced;
        }

        public int GetWineryIdByReservationId(int reservationId)
        {
            int member_Id = 0;
            string sql = "Select WineryId from reservationv2 (nolock) where reservationId=@reservationId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", reservationId));
            var reader = GetDataReader(sql, parameterList, CommandType.Text);
            if (reader != null && reader.HasRows)
            {
                while (reader.Read())
                {
                    member_Id = Convert.ToInt32(reader["WineryId"]);
                }
            }

            return member_Id;
        }

        public WineryDetailForEvent GetWineryIdByEventId(int EventId)
        {
            WineryDetailForEvent model = new WineryDetailForEvent();
            string sql = "Select WineryId,Appelation from Events e (nolock) join Winery w (nolock) on e.WineryId = w.Id where EventId=@EventId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));
            var reader = GetDataReader(sql, parameterList, CommandType.Text);
            if (reader != null && reader.HasRows)
            {
                while (reader.Read())
                {
                    model.MemberId = Convert.ToInt32(reader["WineryId"]);
                    model.RegionId = Convert.ToInt32(reader["Appelation"]);
                }
            }

            return model;
        }

        public bool IsUpsertFulfilmentEnableCommerce7(int memberId)
        {
            bool isFulfilmentEnabled = false;
            string sql = "Select UpsertFulfillmentDate from winery where Id=@memberId";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@memberId", memberId));
            var reader = GetDataReader(sql, parameterList, CommandType.Text);
            if (reader != null && reader.HasRows)
            {
                while (reader.Read())
                {
                    isFulfilmentEnabled = Convert.ToBoolean(reader["UpsertFulfillmentDate"]);
                }
            }

            return isFulfilmentEnabled;
        }

        public bool CloseReservation(int transaction_id, int transaction_category)
        {
            int retval = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", transaction_id));
            parameterList.Add(GetParameter("@TransactionCategory", transaction_category));
            retval = ExecuteNonQuery("CloseReservation", parameterList, CommandType.StoredProcedure);

            return retval > 0;
        }

        public bool UnseatReservation(int transaction_id, int transaction_category)
        {
            int retval = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", transaction_id));
            parameterList.Add(GetParameter("@TransactionCategory", transaction_category));
            retval = ExecuteNonQuery("UnseatReservation", parameterList, CommandType.StoredProcedure);

            return retval > 0;
        }

        public List<SeatingSessionModel> GetSeatingSession(int TransactionId, int TransactionCategory)
        {
            var seatingSessionModel = new List<SeatingSessionModel>();

            string sql = "select s.Id,s.Location_Id, Isnull(t.Floor_Plan_Id, 0) as FloorPlanId from Seating_Session s left join Table_layout t on s.Table_id = t.TableID where s.transactionid=@Id and s.TransactionCategory=@TransactionCategory";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", TransactionId));
            parameterList.Add(GetParameter("@TransactionCategory", TransactionCategory));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new SeatingSessionModel();
                        model.Id = Convert.ToInt32(dataReader["Id"]);
                        model.LocationId = Convert.ToInt32(dataReader["Location_Id"]);
                        model.FloorPlanId = Convert.ToInt32(dataReader["FloorPlanId"]);

                        seatingSessionModel.Add(model);
                    }
                }
            }
            return seatingSessionModel;
        }

        public List<SeatingSessionModel> GetSeatingSession(int TableId, DateTime StartDate, DateTime EndDate)
        {
            var seatingSessionModel = new List<SeatingSessionModel>();

            string sql = "select [Id],[SessionDateTime],[Location_Id],[Table_Id],[NumberSeated],[TransactionCategory],[TransactionId],[User_Id] from Seating_Session where table_id = @TableId and SessionDateTime >= @StartDate and SessionDateTime <= @EndDate";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableId", TableId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new SeatingSessionModel();
                        model.Id = Convert.ToInt32(dataReader["Id"]);
                        model.SessionDateTime = Convert.ToDateTime(dataReader["SessionDateTime"]);
                        model.LocationId = Convert.ToInt32(dataReader["Location_Id"]);
                        model.TableId = Convert.ToInt32(dataReader["Table_Id"]);
                        model.NumberSeated = Convert.ToInt32(dataReader["NumberSeated"]);
                        model.TransactionCategory = Convert.ToInt32(dataReader["TransactionCategory"]);
                        model.TransactionId = Convert.ToInt32(dataReader["TransactionId"]);
                        model.UserId = Convert.ToInt32(dataReader["User_Id"]);

                        seatingSessionModel.Add(model);
                    }
                }
            }
            return seatingSessionModel;
        }

        public WaitlistModel GetWaitlistById(int Id)
        {
            var model = new WaitlistModel();

            string sql = @"select w.Id,Location_Id,Member_Id,WaitStartDateTime,WaitlistStatus,PartySize,User_Id,isnull(PreAssign_Table_Id,'') PreAssign_Table_Id, isnull(f.Id,0) as FloorPlanId, Isnull(st.Floor_Plan_Id, 0) as AssignedFlorPlanId,
                    DATEADD(minute, WaitTimeMinutes + DurationMinutes, WaitStartDateTime) as WaitEndTime
                    from Waitlist w 
                    outer Apply (select top 1 f.Id, PlanName  
					from Floor_Plan f 
					join Table_Layout t on t.Floor_Plan_Id = f.Id 
					join dbo.StrSplit(Replace(Replace(Isnull(w.PreAssign_Table_Id, ''), '[', ''), ']', ''), ',') s on cast(t.TableID as varchar) = Isnull(s.SplitValue, '')
					 ) as f 
                    outer apply (
				        select top 1 t.Floor_Plan_Id
                        from Seating_Session(nolock) s
				        left join Table_Layout t on s.Table_Id = t.TableID
				        where s.TransactionId = w.Id and s.TransactionCategory=1 order by s.SessionDateTime desc) as st
                where w.id=@Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.Id = Convert.ToInt32(dataReader["Id"]);
                        model.MemberId = Convert.ToInt32(dataReader["Member_Id"]);
                        model.LocationId = Convert.ToInt32(dataReader["Location_Id"]);
                        model.WaitStartDateTime = Convert.ToDateTime(dataReader["WaitStartDateTime"]);
                        model.WaitlistStatus = Convert.ToInt32(dataReader["WaitlistStatus"]);
                        model.PartySize = Convert.ToInt32(dataReader["PartySize"]);
                        model.UserId = Convert.ToInt32(dataReader["User_Id"]);
                        model.PreAssign_Table_Id = Convert.ToString(dataReader["PreAssign_Table_Id"]);
                        model.FloorPlanId = Convert.ToInt32(dataReader["FloorPlanId"]);
                        model.AssignedFloorPlanId = Convert.ToInt32(dataReader["AssignedFlorPlanId"]);
                        model.WaitEndDateTime = Convert.ToDateTime(dataReader["WaitEndTime"]);
                    }
                }
            }
            return model;
        }

        public RsvpModel GetRsvpById(int Id)
        {
            var model = new RsvpModel();

            string sql = @"select r.LocationId, r.WineryId, r.EventDate, r.SeatedStatus, Isnull(r.FloorPlanId, 0) as FloorPlanId,  Isnull(st.Floor_Plan_Id, 0) as AssignedFloorPlanId
                           from ReservationV2 r(nolock) 
                            outer apply (
				                            select top 1 s.User_Id, t.Floor_Plan_Id
                                            from Seating_Session(nolock) s
				                            left join Table_Layout (nolock) t on s.Table_Id = t.TableID
				                            where s.TransactionId = r.ReservationId and s.TransactionCategory=2 order by s.SessionDateTime desc) as st
                           where r.ReservationId = @Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.MemberId = Convert.ToInt32(dataReader["WineryId"]);
                        model.LocationId = Convert.ToInt32(dataReader["LocationId"]);
                        model.EventDate = Convert.ToDateTime(dataReader["EventDate"]);
                        model.SeatedStatus = Convert.ToInt32(dataReader["SeatedStatus"]);
                        model.FloorPlanId = Convert.ToInt32(dataReader["FloorPlanId"]);
                        model.AssignedFloorPlanId = Convert.ToInt32(dataReader["AssignedFloorPlanId"]);
                    }
                }
            }
            return model;
        }

        public bool UpdateWaitlistPartySize(int Id, int PartySize)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));
            parameterList.Add(GetParameter("@PartySize", PartySize));

            string sqlQuery = "update Waitlist set PartySize = @PartySize where id=@Id";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool IsActiveServerSession(int Id)
        {
            bool ret = false;

            string sql = "select Id from [Server_Session] where User_Id = @Id and dbo.dateonly(SessionDateTime) = dbo.dateonly(getutcdate()) and SessionEndDateTime is null";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        public TableLayoutModel GetTableLayoutById(int Id)
        {
            var model = new TableLayoutModel();

            string sql = "select LocationId,MinParty,MaxParty from Table_Layout where TableID = @Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.LocationId = Convert.ToInt32(dataReader["LocationId"]);
                        model.MinParty = Convert.ToInt32(dataReader["MinParty"]);
                        model.MaxParty = Convert.ToInt32(dataReader["MaxParty"]);
                    }
                }
            }
            return model;
        }

        public List<TableDetailModel> SortTablesByMaxParty(List<int> tableIds)
        {
            var sortedTableIds = new List<TableDetailModel>();

            string sql = String.Format("select TableID, MinParty, MaxParty from Table_Layout where TableID in ({0}) order by MaxParty desc", String.Join(",", tableIds));

            var parameterList = new List<DbParameter>();

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        sortedTableIds.Add(new TableDetailModel
                        {
                            TableId = Convert.ToInt32(dataReader["TableID"]),
                            MaxParty = Convert.ToInt32(dataReader["MaxParty"]),
                            MinParty = Convert.ToInt32(dataReader["MinParty"])
                        });
                    }
                }
            }
            return sortedTableIds;
        }

        public TableAvailableModel GetTableAvailableById(int TableId)
        {
            TableLayoutModel tableLayoutModel = GetTableLayoutById(TableId);

            var model = new TableAvailableModel();
            int offsettime = GetOffsetMinutes(tableLayoutModel.LocationId);

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@offsettime", offsettime));
            parameterList.Add(GetParameter("@TableId", TableId));
            parameterList.Add(GetParameter("@LocationId", tableLayoutModel.LocationId));

            using (DbDataReader dataReader = GetDataReader("GetTableAvailableById", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.SessionId = Convert.ToInt32(dataReader["Session_Id"]);
                        model.status = Convert.ToInt32(dataReader["status"]);
                        model.TransactionId = Convert.ToInt32(dataReader["TransactionId"]);
                        model.TransactionCategory = Convert.ToInt32(dataReader["TransactionCategory"]);
                    }
                }
            }
            return model;
        }

        public bool CheckTableAvailableById(int TableId, DateTime StartDate, DateTime EndDate,string PreAssign_Table_Id = "")
        {
            bool TableAvailable = true;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableId", TableId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));
            parameterList.Add(GetParameter("@PreAssign_Table_Id", PreAssign_Table_Id));

            using (DbDataReader dataReader = GetDataReader("CheckTableAvailableById", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        if (Convert.ToInt32(dataReader["tableids"]) > 0)
                            TableAvailable = false;
                    }
                }
            }
            return TableAvailable;
        }

        public TableBlockedStatusModel CheckTableBlockedStatus(int tableId, DateTime startDate, DateTime endDate)
        {
            TableBlockedStatusModel model = new TableBlockedStatusModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableId", tableId));
            parameterList.Add(GetParameter("@StartDate", startDate));
            parameterList.Add(GetParameter("@EndDate", endDate));


            using (DbDataReader dataReader = GetDataReader("CheckTableBlocked", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.Id = Convert.ToInt32(dataReader["StatusId"]);
                        if (model.Id > 0)
                        {
                            model.BlockStartDate = Convert.ToDateTime(dataReader["BlockedStartDate"]);
                            model.BlockEndDate = Convert.ToDateTime(dataReader["BlockedEndDate"]);
                        }
                    }
                }
            }
            return model;
        }

        public string GetTableNameById(int TableId)
        {
            string TableName = string.Empty;
            string sql = "select TableName from Table_Layout where tableid=@TableId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableId", TableId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TableName = Convert.ToString(dataReader["TableName"]);
                    }
                }
            }
            return TableName;
        }

        public string GetBusinessPhone(string Commerce7Username, string Commerce7Password, string Commerce7Tenant)
        {
            string BusinessPhone = string.Empty;
            string sql = "select BusinessPhone from winery where Commerce7Username=@Commerce7Username and Commerce7Password = @Commerce7Password and Commerce7Tenant = @Commerce7Tenant";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Commerce7Username", Commerce7Username));
            parameterList.Add(GetParameter("@Commerce7Password", Commerce7Password));
            parameterList.Add(GetParameter("@Commerce7Tenant", Commerce7Tenant));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        BusinessPhone = Convert.ToString(dataReader["BusinessPhone"]);
                    }
                }
            }
            return BusinessPhone;
        }

        public void ForceTableSeat(int SessionId, int TransactionCategory, int TransactionId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TransactionCategory", TransactionCategory));
            parameterList.Add(GetParameter("@TransactionId", TransactionId));
            parameterList.Add(GetParameter("@SessionId", SessionId));

            ExecuteNonQuery("ForceTableSeat", parameterList);
        }

        public int CreateSeating(DateTime SessionDateTime, int Location_Id, int Table_Id, int NumberSeated, int TransactionCategory, int TransactionId, int User_Id)
        {
            int responseid = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@SessionDateTime", SessionDateTime));
            parameterList.Add(GetParameter("@Location_Id", Location_Id));
            parameterList.Add(GetParameter("@Table_Id", Table_Id));
            parameterList.Add(GetParameter("@NumberSeated", NumberSeated));
            parameterList.Add(GetParameter("@TransactionCategory", TransactionCategory));
            parameterList.Add(GetParameter("@TransactionId", TransactionId));
            parameterList.Add(GetParameter("@User_Id", User_Id));

            responseid = Convert.ToInt32(ExecuteScalar("CreateSeating", parameterList));

            return responseid;
        }

        public int CreateSeatingLog(int Session_Id, DateTime StatusDateTime, int Status)
        {
            int responseid = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@Session_Id", Session_Id));
            parameterList.Add(GetParameter("@StatusDateTime", StatusDateTime));
            parameterList.Add(GetParameter("@Status", Status));

            responseid = Convert.ToInt32(ExecuteScalar("CreateSeating_Log", parameterList));

            return responseid;
        }

        public int SwappingTableByRsvpId(int TableId, DateTime StartDate, DateTime EndDate, int reservationid)
        {
            int responseid = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@TableId", TableId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));
            parameterList.Add(GetParameter("@reservationid", reservationid));

            responseid = Convert.ToInt32(ExecuteScalar("SwappingTableByRsvpId", parameterList));

            return responseid;
        }

        public bool UpdateWaitlistStatus(int Id, int WaitlistStatus)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));
            parameterList.Add(GetParameter("@WaitlistStatus", WaitlistStatus));

            string sqlQuery = "update Waitlist set WaitlistStatus = @WaitlistStatus where Id = @Id";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdateReservationV2SeatedStatus(int Id, int SeatedStatus)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));
            parameterList.Add(GetParameter("@SeatedStatus", SeatedStatus));

            string sqlQuery = "update ReservationV2 set SeatedStatus = @SeatedStatus where ReservationId = @Id";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public string GetImageNameByWineryID(int WineryID)
        {
            string Image_Name = string.Empty;

            string sql = "select Image_Name from ImageGallery where Entity_Id = @WineryID and Entity_Type = 0 and Image_Type = 6 and Image_Index = 0";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Image_Name = Convert.ToString(dataReader["Image_Name"]);
                    }
                }
            }
            return Image_Name;
        }

        public List<WineryImageModel> GetWineryImages(int id, int image_type)
        {
            List<WineryImageModel> Images = new List<WineryImageModel>();

            string sql = "select top 10 image_name,image_index from ImageGallery where Entity_Id = @WineryID and Image_Type=@image_type and Entity_Type = 0 order by Image_Index";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", id));
            parameterList.Add(GetParameter("@image_type", image_type));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        WineryImageModel model = new WineryImageModel();

                        model.image_name = Convert.ToString(dataReader["image_name"]);
                        model.image_index = Convert.ToInt32(dataReader["image_index"]);

                        Images.Add(model);
                    }
                }
            }
            return Images;
        }

        public string SaveReservationV2Waitlist(CreateWaitlist waitlist, int mobilePhoneStatus)
        {
            string Id = string.Empty;

            try
            {
                var parameterList = new List<SqlParameter>();

                DateTime event_date_time = Convert.ToDateTime(Convert.ToDateTime(waitlist.event_date).Date.ToShortDateString() + ' ' + waitlist.start_time);
                parameterList.Add(GetParameter("@WaitlistStatus", 0));
                parameterList.Add(GetParameter("@WineryId", waitlist.member_id));
                parameterList.Add(GetParameter("@ReservationId", 0));
                parameterList.Add(GetParameter("@EventId", waitlist.event_id));
                parameterList.Add(GetParameter("@SlotId", waitlist.slot_id));
                parameterList.Add(GetParameter("@SlotType", waitlist.slot_type));
                parameterList.Add(GetParameter("@GuestCount", waitlist.guest_count));
                parameterList.Add(GetParameter("@User_Id", waitlist.user_id));
                parameterList.Add(GetParameter("@EventName", waitlist.event_name));
                parameterList.Add(GetParameter("@Email", waitlist.email));
                parameterList.Add(GetParameter("@FirstName", waitlist.first_name));
                parameterList.Add(GetParameter("@LastName", waitlist.last_name));
                parameterList.Add(GetParameter("@MobilePhone", waitlist.mobile_phone));
                parameterList.Add(GetParameter("@MobilePhoneStatus", mobilePhoneStatus));
                parameterList.Add(GetParameter("@Note", waitlist.note));
                parameterList.Add(GetParameter("@NoteInternal", waitlist.note_internal));
                parameterList.Add(GetParameter("@EventDateTime", event_date_time));
                parameterList.Add(GetParameter("@CreatedDateTime", DateTime.UtcNow));
                parameterList.Add(GetParameter("@StatusDateTime", DateTime.UtcNow));
                parameterList.Add(GetParameter("@NotificationPreference", waitlist.notification_preference));
                parameterList.Add(GetParameter("@ValidMinutes", 0));
                parameterList.Add(GetParameter("@AffiliateId", waitlist.affiliate_id));
                parameterList.Add(GetParameter("@ConciergeNote", waitlist.concierge_note));

                Id = Convert.ToString(ExecuteScalar("InsertReservationV2Waitlist", parameterList));
            }
            catch (Exception ex)
            {
                Id = string.Empty;
            }
            return Id;
        }

        public string UpdateReservationV2Waitlist(UpdateWaitlist waitlist)
        {
            string Id = string.Empty;

            try
            {
                var parameterList = new List<SqlParameter>();

                parameterList.Add(GetParameter("@Id", waitlist.id));
                parameterList.Add(GetParameter("@WaitlistStatus", waitlist.waitlist_status));
                parameterList.Add(GetParameter("@ReservationId", waitlist.reservation_id));
                parameterList.Add(GetParameter("@StatusDateTime", DateTime.UtcNow));

                DateTime? InvitedDateTime = null;
                if (waitlist.waitlist_status == uc.Common.Waitlist_Status.approved)
                {
                    InvitedDateTime = DateTime.UtcNow;
                }

                parameterList.Add(GetParameter("@InvitedDateTime", InvitedDateTime));
                parameterList.Add(GetParameter("@ValidMinutes", waitlist.valid_minutes));

                Id = Convert.ToString(ExecuteScalar("UpdateReservationV2Waitlist", parameterList));
            }
            catch (Exception ex)
            {
                Id = string.Empty;
            }
            return Id;
        }

        public Waitlist GetReservationV2WaitlistbyId(string guid, string PurchaseURL = "", int Id = 0)
        {
            var waitlist = new Waitlist();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@Guid", guid));
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader("GetReservationV2WaitlistbyId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        waitlist.id = Convert.ToString(dataReader["Id"]);
                        waitlist.waitlist_status = (uc.Common.Waitlist_Status)Convert.ToInt32(dataReader["WaitlistStatus"]);
                        waitlist.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        waitlist.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        waitlist.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        waitlist.event_id = Convert.ToInt32(dataReader["EventId"]);
                        waitlist.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        waitlist.guest_count = Convert.ToInt32(dataReader["GuestCount"]);
                        waitlist.event_name = Convert.ToString(dataReader["EventName"]);
                        waitlist.user_id = Convert.ToInt32(dataReader["User_Id"]);
                        waitlist.email = Convert.ToString(dataReader["Email"]);
                        waitlist.first_name = Convert.ToString(dataReader["FirstName"]);
                        waitlist.last_name = Convert.ToString(dataReader["LastName"]);
                        waitlist.mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        waitlist.mobile_phone_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        waitlist.note = Convert.ToString(dataReader["Note"]);
                        waitlist.note_internal = Convert.ToString(dataReader["NoteInternal"]);
                        waitlist.event_date_time = Convert.ToDateTime(dataReader["EventDateTime"]);
                        waitlist.notification_preference = (uc.Common.Notification_Preference)Convert.ToInt32(dataReader["NotificationPreference"]);
                        waitlist.created_date_time = Convert.ToDateTime(dataReader["CreatedDateTime"]);
                        waitlist.status_date_time = Convert.ToDateTime(dataReader["StatusDateTime"]);

                        if (!string.IsNullOrWhiteSpace(Convert.ToString(dataReader["InvitedDateTime"])))
                        {
                            waitlist.invited_date_time = Convert.ToDateTime(dataReader["InvitedDateTime"]);
                        }

                        waitlist.valid_minutes = Convert.ToInt32(dataReader["ValidMinutes"]);
                        waitlist.member_name = Convert.ToString(dataReader["displayname"]);
                        waitlist.start_time = Convert.ToString(dataReader["StartTime"]);
                        waitlist.end_time = Convert.ToString(dataReader["EndTime"]);
                        waitlist.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        waitlist.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        waitlist.destination_name = Convert.ToString(dataReader["DestinationName"]);
                        waitlist.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        waitlist.work_phone = Convert.ToString(dataReader["BusinessPhone"]);
                        //waitlist.purchase_url = PurchaseURL + Convert.ToString(dataReader["PurchaseURL"]) + "-profile";
                        waitlist.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        waitlist.location_timezone = Convert.ToInt32(dataReader["TimeZoneId"]);
                        waitlist.affiliate_id = Convert.ToInt32(dataReader["AffiliateId"]);
                        waitlist.concierge_note = Convert.ToString(dataReader["ConciergeNote"]);
                        waitlist.event_location = Convert.ToString(dataReader["EventLocation"]);
                        waitlist.location_notification_email = Convert.ToString(dataReader["NotificationEmail"]);

                        UserAddress addr = new UserAddress();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.zip_code = Convert.ToString(dataReader["Zip"]);
                        addr.country = Convert.ToString(dataReader["Country"]);
                        waitlist.address = addr;
                    }
                }
            }
            return waitlist;
        }

        public List<Waitlist> GetReservationV2Waitlist(int member_id, string Keyword, int status, DateTime? start_date = null, DateTime? end_date = null, string PurchaseURL = "")
        {
            var List = new List<Waitlist>();
            var parameterList = new List<DbParameter>();

            if (start_date == null || start_date == default(DateTime))
                start_date = Convert.ToDateTime("1/1/1900");

            if (end_date == null || end_date == default(DateTime))
                end_date = Convert.ToDateTime("1/1/1900");

            parameterList.Add(GetParameter("@MemberId", member_id));
            parameterList.Add(GetParameter("@Keyword", Keyword));
            parameterList.Add(GetParameter("@status", status));
            parameterList.Add(GetParameter("@startDate", start_date));
            parameterList.Add(GetParameter("@endDate", end_date));

            using (DbDataReader dataReader = GetDataReader("GetReservationV2Waitlist", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var waitlist = new Waitlist();
                        waitlist.id = Convert.ToString(dataReader["Id"]);
                        waitlist.waitlist_status = (uc.Common.Waitlist_Status)Convert.ToInt32(dataReader["WaitlistStatus"]);
                        waitlist.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        waitlist.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        waitlist.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        waitlist.event_id = Convert.ToInt32(dataReader["EventId"]);
                        waitlist.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        waitlist.guest_count = Convert.ToInt32(dataReader["GuestCount"]);
                        waitlist.event_name = Convert.ToString(dataReader["EventName"]);
                        waitlist.user_id = Convert.ToInt32(dataReader["User_Id"]);
                        waitlist.email = Convert.ToString(dataReader["Email"]);
                        waitlist.first_name = Convert.ToString(dataReader["FirstName"]);
                        waitlist.last_name = Convert.ToString(dataReader["LastName"]);
                        waitlist.mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        waitlist.mobile_phone_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        waitlist.note = Convert.ToString(dataReader["Note"]);
                        waitlist.note_internal = Convert.ToString(dataReader["NoteInternal"]);
                        waitlist.event_date_time = Convert.ToDateTime(dataReader["EventDateTime"]);
                        waitlist.notification_preference = (uc.Common.Notification_Preference)Convert.ToInt32(dataReader["NotificationPreference"]);
                        waitlist.created_date_time = Convert.ToDateTime(dataReader["CreatedDateTime"]);
                        waitlist.status_date_time = Convert.ToDateTime(dataReader["StatusDateTime"]);

                        if (!string.IsNullOrWhiteSpace(Convert.ToString(dataReader["InvitedDateTime"])))
                        {
                            waitlist.invited_date_time = Convert.ToDateTime(dataReader["InvitedDateTime"]);
                        }

                        waitlist.valid_minutes = Convert.ToInt32(dataReader["ValidMinutes"]);
                        waitlist.member_name = Convert.ToString(dataReader["displayname"]);
                        waitlist.start_time = Convert.ToString(dataReader["StartTime"]);
                        waitlist.end_time = Convert.ToString(dataReader["EndTime"]);
                        waitlist.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        waitlist.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        waitlist.destination_name = Convert.ToString(dataReader["DestinationName"]);
                        waitlist.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        waitlist.work_phone = Convert.ToString(dataReader["BusinessPhone"]);
                        //waitlist.purchase_url = PurchaseURL + Convert.ToString(dataReader["PurchaseURL"]) + "-profile";
                        waitlist.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        waitlist.location_timezone = Convert.ToInt32(dataReader["TimeZoneId"]);

                        UserAddress addr = new UserAddress();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.zip_code = Convert.ToString(dataReader["Zip"]);
                        addr.country = Convert.ToString(dataReader["Country"]);
                        waitlist.address = addr;

                        List.Add(waitlist);
                    }
                }
            }
            return List;
        }

        public List<Waitlist> GetReservationV2WaitlistByUserId(int user_id, int status, DateTime? start_date = null, DateTime? end_date = null, string PurchaseURL = "")
        {
            var List = new List<Waitlist>();
            var parameterList = new List<DbParameter>();

            if (start_date == null || start_date == default(DateTime))
                start_date = Convert.ToDateTime("1/1/1900");

            if (end_date == null || end_date == default(DateTime))
                end_date = Convert.ToDateTime("1/1/1900");

            parameterList.Add(GetParameter("@UserId", user_id));
            parameterList.Add(GetParameter("@status", status));
            parameterList.Add(GetParameter("@startDate", start_date));
            parameterList.Add(GetParameter("@endDate", end_date));

            using (DbDataReader dataReader = GetDataReader("GetReservationV2WaitlistByUserId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var waitlist = new Waitlist();
                        waitlist.id = Convert.ToString(dataReader["Id"]);
                        waitlist.waitlist_status = (uc.Common.Waitlist_Status)Convert.ToInt32(dataReader["WaitlistStatus"]);
                        waitlist.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        waitlist.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        waitlist.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        waitlist.event_id = Convert.ToInt32(dataReader["EventId"]);
                        waitlist.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        waitlist.guest_count = Convert.ToInt32(dataReader["GuestCount"]);
                        waitlist.event_name = Convert.ToString(dataReader["EventName"]);
                        waitlist.user_id = Convert.ToInt32(dataReader["User_Id"]);
                        waitlist.email = Convert.ToString(dataReader["Email"]);
                        waitlist.first_name = Convert.ToString(dataReader["FirstName"]);
                        waitlist.last_name = Convert.ToString(dataReader["LastName"]);
                        waitlist.mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        waitlist.mobile_phone_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        waitlist.note = Convert.ToString(dataReader["Note"]);
                        waitlist.note_internal = Convert.ToString(dataReader["NoteInternal"]);
                        waitlist.event_date_time = Convert.ToDateTime(dataReader["EventDateTime"]);
                        waitlist.notification_preference = (uc.Common.Notification_Preference)Convert.ToInt32(dataReader["NotificationPreference"]);
                        waitlist.created_date_time = Convert.ToDateTime(dataReader["CreatedDateTime"]);
                        waitlist.status_date_time = Convert.ToDateTime(dataReader["StatusDateTime"]);

                        if (!string.IsNullOrWhiteSpace(Convert.ToString(dataReader["InvitedDateTime"])))
                        {
                            waitlist.invited_date_time = Convert.ToDateTime(dataReader["InvitedDateTime"]);
                        }

                        waitlist.valid_minutes = Convert.ToInt32(dataReader["ValidMinutes"]);
                        waitlist.member_name = Convert.ToString(dataReader["displayname"]);
                        waitlist.start_time = Convert.ToString(dataReader["StartTime"]);
                        waitlist.end_time = Convert.ToString(dataReader["EndTime"]);
                        waitlist.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        waitlist.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        waitlist.destination_name = Convert.ToString(dataReader["DestinationName"]);
                        waitlist.fee_Per_person = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        waitlist.work_phone = Convert.ToString(dataReader["BusinessPhone"]);
                        //waitlist.purchase_url = PurchaseURL +  Convert.ToString(dataReader["PurchaseURL"]) + "-profile";
                        waitlist.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        waitlist.location_timezone = Convert.ToInt32(dataReader["TimeZoneId"]);

                        UserAddress addr = new UserAddress();

                        addr.address_1 = Convert.ToString(dataReader["Address1"]);
                        addr.address_2 = Convert.ToString(dataReader["Address2"]);
                        addr.city = Convert.ToString(dataReader["City"]);
                        addr.state = Convert.ToString(dataReader["State"]);
                        addr.zip_code = Convert.ToString(dataReader["Zip"]);
                        addr.country = Convert.ToString(dataReader["Country"]);
                        waitlist.address = addr;

                        List.Add(waitlist);
                    }
                }
            }
            return List;
        }

        public List<TransportationModel> GetEventTransportationList(int event_id)
        {
            var lstTransportation = new List<TransportationModel>();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@eventId", event_id));
            using (DbDataReader dataReader = GetDataReader("GetEventTransportationAffiliatesList", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var transport = new TransportationModel();
                        transport.id = Convert.ToInt32(dataReader["Id"]);
                        transport.company_name = Convert.ToString(dataReader["CompanyName"]);

                        lstTransportation.Add(transport);
                    }
                }
            }

            return lstTransportation;
        }

        public PassportAvailableStatus isPassportClaimAvailable(string claimKey, int memberId, string memberName, int rsvpEventId, DateTime rsvpEventDate)
        {
            var pstatus = new PassportAvailableStatus();

            pstatus.isAvailable = false;
            pstatus.Message = "Claim code not recognized.";

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@PostCaptureKey", claimKey));
            parameterList.Add(GetParameter("@RsvpEventId", rsvpEventId));
            parameterList.Add(GetParameter("@WineryId", memberId));

            using (DbDataReader dataReader = GetDataReader("Tickets_Check_Passport_Claim", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        pstatus.ticketId = Convert.ToInt32(dataReader["Id"]);

                        int validEventCount = Convert.ToInt32(dataReader["validEventCount"]);
                        int claimCount = Convert.ToInt32(dataReader["claimCount"]);
                        DateTime ValidStartDate = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        DateTime ValidEndDate = Convert.ToDateTime(dataReader["ValidEndDate"]);

                        if (validEventCount == 0)
                        {
                            pstatus.Message = "Claim code not applicable to selected event.";
                        }
                        else if (claimCount > 0)
                        {
                            string name = memberName;
                            if (string.IsNullOrEmpty(name))
                            {
                                name = "this participating member";
                            }
                            pstatus.Message = string.Format("Claim code already redeemed for {0}.", name);
                        }
                        else if (validEventCount > 0 && claimCount == 0)
                        {
                            if (rsvpEventDate.Date >= ValidStartDate.Date && rsvpEventDate.Date <= ValidEndDate.Date)
                            {
                                pstatus.isAvailable = true;
                                pstatus.Message = "";
                            }
                            else
                            {
                                pstatus.isAvailable = false;
                                pstatus.Message = string.Format("Claim code valid for evens between  {0} and {1}.", ValidStartDate.Date.ToShortDateString(), ValidEndDate.Date.ToShortDateString());
                            }
                        }
                    }
                }
            }
            return pstatus;
        }

        public int SavePassportRsvpClaim(int Status, int Tickets_Order_Tickets_Id, int Winery_Id, int Reservation_Id, DateTime ClaimDate)
        {
            int id = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@Status", Status));
            parameterList.Add(GetParameter("@Tickets_Order_Tickets_Id", Tickets_Order_Tickets_Id));
            parameterList.Add(GetParameter("@Winery_Id", Winery_Id));
            parameterList.Add(GetParameter("@Reservation_Id", Reservation_Id));
            parameterList.Add(GetParameter("@ClaimDate", ""));

            id = Convert.ToInt32(ExecuteScalar("PassportRsvpClaim_INSERT", parameterList));

            return id;
        }

        public bool RemovePassportRsvpClaim(int reservationId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", reservationId));

            string sqlQuery = "DELETE FROM [dbo].[Tickets_Order_Tickets_PassportClaim] WHERE Reservation_Id = @reservationId";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public int SaveDiscountToReservation(int discountType, string DiscountCode, decimal DicountAmount, string DiscountDesc, int ReservationId)
        {
            int id = 0;

            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@DiscountType", discountType));
            parameterList.Add(GetParameter("@DiscountCode", DiscountCode));
            parameterList.Add(GetParameter("@DicountAmount", DicountAmount));
            parameterList.Add(GetParameter("@DiscountDesc", DiscountDesc));
            parameterList.Add(GetParameter("@Reservation_Id", ReservationId));

            id = Convert.ToInt32(ExecuteScalar("ReervationV2Discounts_INSERT", parameterList));

            return id;
        }

        public DestinationDetails GetDestinationLandingPage(int region_id, string region_url)
        {
            DestinationDetails destinationDetails = new DestinationDetails();
            List<RSVPEventTypes> rsvp_event_types = new List<RSVPEventTypes>();
            List<BusinessDetails> listdetails = new List<BusinessDetails>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@regionId", region_id));
            parameterList.Add(GetParameter("@regionURL", region_url));

            using (DbDataReader dataReader = GetDataReader("GetDestinationLandingPage", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        RSVPEventTypes rsvpeventtype = new RSVPEventTypes();

                        rsvpeventtype.id = Convert.ToInt32(dataReader["id"]);
                        rsvpeventtype.description = Convert.ToString(dataReader["EventTypeName"]);
                        rsvpeventtype.event_type_name = Convert.ToString(dataReader["description"]);

                        rsvp_event_types.Add(rsvpeventtype);
                    }
                }

                destinationDetails.rsvp_event_types = rsvp_event_types;

                if (dataReader.NextResult())
                {
                    while (dataReader.Read())
                    {
                        destinationDetails.region_id = Convert.ToInt32(dataReader["RegionId"]);
                        destinationDetails.region_name = Convert.ToString(dataReader["RegionName"]);
                        destinationDetails.region_friendly_url = Convert.ToString(dataReader["RegionFriendlyURL"]);
                        destinationDetails.state_code = Convert.ToString(dataReader["state"]);
                        destinationDetails.state_name = Convert.ToString(dataReader["stateName"]);

                        destinationDetails.region_image_url = Convert.ToString(dataReader["ImageURL"]);
                        destinationDetails.region_page_intro = Convert.ToString(dataReader["EventsPageIntro"]);
                        destinationDetails.region_page_desc = Convert.ToString(dataReader["EventsPageDesc"]);
                        destinationDetails.region_banner_image = Convert.ToString(dataReader["EventsPageBannerImage"]);

                        BusinessDetails details = new BusinessDetails();

                        details.business_id = Convert.ToInt32(dataReader["WineryId"]);
                        details.listing_image_url = Convert.ToString(dataReader["ListingImageUrl"]);
                        details.business_name = Convert.ToString(dataReader["DisplayName"]);
                        details.business_page_url = Convert.ToString(dataReader["PurchaseURL"]);
                        details.business_city = Convert.ToString(dataReader["city"]);
                        details.business_type = Convert.ToString(dataReader["WineryTypeName"]);
                        details.business_sub_region = Convert.ToString(dataReader["SubRegionName"]);
                        details.is_favorites = Convert.ToBoolean(dataReader["IsFavorites"]);
                        details.reviews = Convert.ToInt32(dataReader["Reviews"]);
                        details.review_stars = Convert.ToInt32(dataReader["Star"]);
                        details.avg_review_value = Convert.ToDecimal(dataReader["avg_review_value"]);
                        details.business_state = Convert.ToString(dataReader["state"]);
                        details.business_region = Convert.ToString(dataReader["RegionName"]);
                        details.poi = JsonConvert.DeserializeObject<List<POI>>(Convert.ToString(dataReader["POIJSON"]));
                        //List<string> listnotablefeatures = new List<string>();
                        //List<string> listvarietals = new List<string>();
                        //string notable_features = Convert.ToString(dataReader["notable_features"]);

                        //if (!string.IsNullOrWhiteSpace(notable_features))
                        //{
                        //    listnotablefeatures = JsonConvert.DeserializeObject<List<string>>(notable_features);
                        //}

                        //string varietals = Convert.ToString(dataReader["varietals"]);

                        //if (!string.IsNullOrWhiteSpace(varietals))
                        //{
                        //    listvarietals = JsonConvert.DeserializeObject<List<string>>(varietals);
                        //}

                        //details.notable_features = listnotablefeatures;
                        //details.varietals = listvarietals;

                        listdetails.Add(details);
                    }
                }

                destinationDetails.business_details = listdetails;
            }
            return destinationDetails;
        }

        public GuestDashboardModel GetDashboardMetricsByMemberAndDays(int memberId, int days, DateTime dt, DateTime start_date, DateTime end_date)
        {
            GuestDashboardModel guestDashboard = null;
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@memberId", memberId));
            parameterList.Add(GetParameter("@days", days));
            parameterList.Add(GetParameter("@date", dt));
            parameterList.Add(GetParameter("@start_date", start_date));
            parameterList.Add(GetParameter("@end_date", end_date));

            using (DbDataReader dataReader = GetDataReader("GetGuestDashboardData", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        guestDashboard = new GuestDashboardModel();
                        List<DashboardMetrics> metricsListByDay = new List<DashboardMetrics>();
                        string graphData = Convert.ToString(dataReader["GraphDataByDate"]);
                        if (!string.IsNullOrWhiteSpace(graphData))
                        {
                            metricsListByDay = JsonConvert.DeserializeObject<List<DashboardMetrics>>(graphData);
                        }

                        int[] reservationHistory = null;
                        int[] ticketHistory = null;
                        decimal[] revenueHistory = null;
                        int ticketTotal = 0;
                        decimal revenueTotal = 0;
                        decimal rsvpRevenue = 0;
                        decimal ticketRevenue = 0;
                        int totalGuest = 0;
                        List<RSVPGraph> rsvpGraph = new List<RSVPGraph>();
                        //List<RevenueGraph> revenueGraph = new List<RevenueGraph>();
                        if (metricsListByDay.Count > 0)
                        {
                            reservationHistory = metricsListByDay.Select(m => m.rsvp_count).ToList().ToArray();
                            revenueHistory = metricsListByDay.Select(m => m.rsvp_revenue).ToList().ToArray();
                            ticketHistory = metricsListByDay.Select(m => m.ticket_count).ToList().ToArray();
                            ticketTotal = metricsListByDay.Sum(m => m.ticket_count);
                            rsvpRevenue = metricsListByDay.Sum(m => m.rsvp_revenue);
                            ticketRevenue = metricsListByDay.Sum(m => m.ticket_revenue);
                            revenueTotal = rsvpRevenue + ticketRevenue;

                            rsvpGraph = metricsListByDay.Select(m => new RSVPGraph { date = m.booking_date, value = m.rsvp_guest_count }).ToList();
                            //revenueGraph = metricsListByDay.Select(m => new RevenueGraph { date = m.booking_date, value = m.ticket_revenue + m.rsvp_revenue }).ToList();
                            guestDashboard.revenue_reservation_graph = metricsListByDay.Select(m => new RevenueGraph { date = m.booking_date, value = m.rsvp_revenue }).ToList();
                            guestDashboard.revenue_ticket_graph = metricsListByDay.Select(m => new RevenueGraph { date = m.booking_date, value = m.ticket_revenue }).ToList();
                        }
                        guestDashboard.summary = new RSVPSummary
                        {
                            experience_review_increase = Convert.ToDecimal(dataReader["RatingChangePercentage"]),
                            experience_review_total = Convert.ToInt32(dataReader["RatingsCount"]),
                            favorite_increase = Convert.ToDecimal(dataReader["FavoriteChangePercentage"]),
                            favorite_total = Convert.ToInt32(dataReader["FaveCount"]),
                            new_guest_increase = Convert.ToDecimal(dataReader["NewGuestChangePercentage"]),
                            new_guest_percentage = Convert.ToDecimal(dataReader["NewGuestPercentage"]),
                            region_ranking = Convert.ToInt32(dataReader["Ranking"]),
                            region_ranking_increase = Convert.ToDecimal(dataReader["RankingChangePercentage"]),
                            reservation_history = reservationHistory,
                            reservation_increase_percentage = Convert.ToDecimal(dataReader["ReservationPercentChange"]),
                            reservation_total_has_increased = Convert.ToDecimal(dataReader["ReservationPercentChange"]) > 0,
                            revenue_history = revenueHistory,
                            revenue_total = revenueTotal,
                            ticket_history = ticketHistory,
                            ticket_total = ticketTotal,
                            total_guests_checked_in = Convert.ToInt32(dataReader["TotalGuestsCheckedIn"]),
                            total_guests_not_checked_in = Convert.ToInt32(dataReader["GuestsNotCheckedIn"]),
                            total_guests_rsvp = Convert.ToInt32(dataReader["TotalGuestsViaRSVP"]),
                            total_guests_seated = Convert.ToInt32(dataReader["TotalSeatedGuests"]),
                            total_guests_walkins = Convert.ToInt32(dataReader["TotGuestsViaWalkins"])


                        };
                        totalGuest = Convert.ToInt32(dataReader["TotalGuestsViaRSVP"]);
                        guestDashboard.reservation = new List<ReservationData>();
                        guestDashboard.reservation.Add(new ReservationData
                        {
                            title = "Direct Reservations",
                            count = Convert.ToInt32(dataReader["DirectCount"]),
                            total = totalGuest,
                            color = "#4c4f53"
                        });
                        guestDashboard.reservation.Add(new ReservationData
                        {
                            title = "Referral Reservations",
                            count = Convert.ToInt32(dataReader["ReferralCount"]),
                            total = totalGuest,
                            color = "#57889c"
                        });
                        guestDashboard.reservation.Add(new ReservationData
                        {
                            title = "Cancelled Reservations",
                            count = Convert.ToInt32(dataReader["CancelledCount"]),
                            total = totalGuest,
                            color = "#C02631"
                        });
                        guestDashboard.reservation.Add(new ReservationData
                        {
                            title = "Rescheduled Reservations",
                            count = Convert.ToInt32(dataReader["RescheduledCount"]),
                            total = totalGuest,
                            color = "#c79121"
                        });
                        guestDashboard.reservation_graph = rsvpGraph;
                        guestDashboard.revenue = new List<RevenueData>();
                        guestDashboard.revenue.Add(new RevenueData
                        {
                            title = "Reservations",
                            count = rsvpRevenue,
                            total = revenueTotal,
                            color = "#57889c"
                        });
                        guestDashboard.revenue.Add(new RevenueData
                        {
                            title = "Tickets",
                            count = ticketRevenue,
                            total = revenueTotal,
                            color = "#91A1A8"
                        });

                        //guestDashboard.revenue_graph = revenueGraph;
                    }
                }
            }

            return guestDashboard;

        }

        public GuestDashboardModel GetDashboardMetricsByEventDateMemberAndDays(int memberId, int days, DateTime dt, DateTime start_date, DateTime end_date)
        {
            GuestDashboardModel guestDashboard = null;
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@memberId", memberId));
            parameterList.Add(GetParameter("@days", days));
            parameterList.Add(GetParameter("@date", dt));
            parameterList.Add(GetParameter("@start_date", start_date));
            parameterList.Add(GetParameter("@end_date", end_date));

            using (DbDataReader dataReader = GetDataReader("GetGuestDashboardDataByEventDate", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        guestDashboard = new GuestDashboardModel();
                        List<DashboardMetrics> metricsListByDay = new List<DashboardMetrics>();
                        string graphData = Convert.ToString(dataReader["GraphDataByDate"]);
                        if (!string.IsNullOrWhiteSpace(graphData))
                        {
                            metricsListByDay = JsonConvert.DeserializeObject<List<DashboardMetrics>>(graphData);
                        }

                        int[] reservationHistory = null;
                        int[] ticketHistory = null;
                        decimal[] revenueHistory = null;
                        int ticketTotal = 0;
                        decimal revenueTotal = 0;
                        decimal rsvpRevenue = 0;
                        decimal ticketRevenue = 0;
                        int totalGuest = 0;
                        List<RSVPGraph> rsvpGraph = new List<RSVPGraph>();
                        //List<RevenueGraph> revenueGraph = new List<RevenueGraph>();
                        if (metricsListByDay.Count > 0)
                        {
                            reservationHistory = metricsListByDay.Select(m => m.rsvp_count).ToList().ToArray();
                            revenueHistory = metricsListByDay.Select(m => m.rsvp_revenue).ToList().ToArray();
                            ticketHistory = metricsListByDay.Select(m => m.ticket_count).ToList().ToArray();
                            ticketTotal = metricsListByDay.Sum(m => m.ticket_count);
                            rsvpRevenue = metricsListByDay.Sum(m => m.rsvp_revenue);
                            ticketRevenue = metricsListByDay.Sum(m => m.ticket_revenue);
                            revenueTotal = rsvpRevenue + ticketRevenue;

                            rsvpGraph = metricsListByDay.Select(m => new RSVPGraph { date = m.booking_date, value = m.rsvp_guest_count }).ToList();
                            //revenueGraph = metricsListByDay.Select(m => new RevenueGraph { date = m.booking_date, value = m.ticket_revenue + m.rsvp_revenue }).ToList();
                            guestDashboard.revenue_reservation_graph = metricsListByDay.Select(m => new RevenueGraph { date = m.booking_date, value = m.rsvp_revenue }).ToList();
                            guestDashboard.revenue_ticket_graph = metricsListByDay.Select(m => new RevenueGraph { date = m.booking_date, value = m.ticket_revenue }).ToList();
                        }
                        guestDashboard.summary = new RSVPSummary
                        {
                            experience_review_increase = Convert.ToDecimal(dataReader["RatingChangePercentage"]),
                            experience_review_total = Convert.ToInt32(dataReader["RatingsCount"]),
                            favorite_increase = Convert.ToDecimal(dataReader["FavoriteChangePercentage"]),
                            favorite_total = Convert.ToInt32(dataReader["FaveCount"]),
                            new_guest_increase = Convert.ToDecimal(dataReader["NewGuestChangePercentage"]),
                            new_guest_percentage = Convert.ToDecimal(dataReader["NewGuestPercentage"]),
                            region_ranking = Convert.ToInt32(dataReader["Ranking"]),
                            region_ranking_increase = Convert.ToDecimal(dataReader["RankingChangePercentage"]),
                            reservation_history = reservationHistory,
                            reservation_increase_percentage = Convert.ToDecimal(dataReader["ReservationPercentChange"]),
                            reservation_total_has_increased = Convert.ToDecimal(dataReader["ReservationPercentChange"]) > 0,
                            revenue_history = revenueHistory,
                            revenue_total = revenueTotal,
                            ticket_history = ticketHistory,
                            ticket_total = ticketTotal,
                            total_guests_checked_in = Convert.ToInt32(dataReader["TotalGuestsCheckedIn"]),
                            total_guests_not_checked_in = Convert.ToInt32(dataReader["GuestsNotCheckedIn"]),
                            total_guests_rsvp = Convert.ToInt32(dataReader["TotalGuestsViaRSVP"]),
                            total_guests_seated = Convert.ToInt32(dataReader["TotalSeatedGuests"]),
                            total_guests_walkins = Convert.ToInt32(dataReader["TotGuestsViaWalkins"])


                        };
                        totalGuest = Convert.ToInt32(dataReader["TotalGuestsViaRSVP"]);
                        guestDashboard.reservation = new List<ReservationData>();
                        guestDashboard.reservation.Add(new ReservationData
                        {
                            title = "Direct Reservations",
                            count = Convert.ToInt32(dataReader["DirectCount"]),
                            total = totalGuest,
                            color = "#4c4f53"
                        });
                        guestDashboard.reservation.Add(new ReservationData
                        {
                            title = "Referral Reservations",
                            count = Convert.ToInt32(dataReader["ReferralCount"]),
                            total = totalGuest,
                            color = "#57889c"
                        });
                        guestDashboard.reservation.Add(new ReservationData
                        {
                            title = "Cancelled Reservations",
                            count = Convert.ToInt32(dataReader["CancelledCount"]),
                            total = totalGuest,
                            color = "#C02631"
                        });
                        guestDashboard.reservation.Add(new ReservationData
                        {
                            title = "Rescheduled Reservations",
                            count = Convert.ToInt32(dataReader["RescheduledCount"]),
                            total = totalGuest,
                            color = "#c79121"
                        });
                        guestDashboard.reservation_graph = rsvpGraph;
                        guestDashboard.revenue = new List<RevenueData>();
                        guestDashboard.revenue.Add(new RevenueData
                        {
                            title = "Reservations",
                            count = rsvpRevenue,
                            total = revenueTotal,
                            color = "#57889c"
                        });
                        guestDashboard.revenue.Add(new RevenueData
                        {
                            title = "Tickets",
                            count = ticketRevenue,
                            total = revenueTotal,
                            color = "#91A1A8"
                        });

                        //guestDashboard.revenue_graph = revenueGraph;
                    }
                }
            }

            return guestDashboard;

        }

        public OpenTableMemberModel GetOpenTableMemberData(int memberId, int refId)
        {
            OpenTableMemberModel openTableMember = null;
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@memberId", memberId));
            parameterList.Add(GetParameter("@refId", refId));

            using (DbDataReader dataReader = GetDataReader("GetOpenTableDataByMemberId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        openTableMember = new OpenTableMemberModel();
                        openTableMember.name = Convert.ToString(dataReader["MemberName"]);
                        openTableMember.rid = Convert.ToInt32(dataReader["RefId"]);
                        openTableMember.address = Convert.ToString(dataReader["Address1"]);
                        openTableMember.address2 = Convert.ToString(dataReader["Address2"]);
                        openTableMember.city = Convert.ToString(dataReader["City"]);
                        openTableMember.state = Convert.ToString(dataReader["State"]);
                        openTableMember.postal_code = Convert.ToString(dataReader["ZipCode"]);
                        openTableMember.latitude = Convert.ToString(dataReader["Latitude"]);
                        openTableMember.longitude = Convert.ToString(dataReader["Longitude"]);
                        openTableMember.country = Convert.ToString(dataReader["Country"]);
                        openTableMember.phone_number = Common.StringHelpers.FormatTelephoneNumber(Convert.ToString(dataReader["PhoneNumber"]), openTableMember.country);
                        openTableMember.price_quartile = Convert.ToString(dataReader["PriceQuartile"]);
                        openTableMember.metro_name = Convert.ToString(dataReader["MetroName"]);
                        openTableMember.profile_url = Convert.ToString(dataReader["ProfileURL"]);
                        openTableMember.natural_reservation_url = Convert.ToString(dataReader["NaturalReservationURL"]);
                        openTableMember.natural_profile_url = Convert.ToString(dataReader["NaturalProfileURL"]);
                        string categories = Convert.ToString(dataReader["Categories"]); //comma-delimited
                        List<string> lstCategories = categories.Split(',').ToList();

                        openTableMember.category = lstCategories;
                        openTableMember.review_count = Convert.ToInt32(dataReader["ReviewCount"]);
                        openTableMember.aggregate_score = Convert.ToString(dataReader["AggregateScore"]);
                        openTableMember.is_restaurant_in_group = Convert.ToBoolean(dataReader["IsReservationInGroup"]);

                        //guestDashboard.revenue_graph = revenueGraph;
                    }
                }
            }

            return openTableMember;

        }

        public int SaveEventAbandoned(int User_Id, string Email, int Member_Id, int Slot_Id, int Slot_Type, int GuestCount, DateTime DateRequested, Guid CartGUID)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@User_Id", User_Id));
            parameterList.Add(GetParameter("@Email", Email));
            parameterList.Add(GetParameter("@Member_Id", Member_Id));
            parameterList.Add(GetParameter("@Slot_Id", Slot_Id));
            parameterList.Add(GetParameter("@Slot_Type", Slot_Type));
            parameterList.Add(GetParameter("@GuestCount", GuestCount));
            parameterList.Add(GetParameter("@DateRequested", DateRequested));
            parameterList.Add(GetParameter("@OrderGUID", CartGUID));

            int retvalue = Convert.ToInt32(ExecuteScalar("InsertEvent_Abandoned", parameterList));
            return retvalue;
        }

        public bool DeleteEventAbandoned(string Email, int Slot_Id, int Slot_Type)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@Email", Email));
            parameterList.Add(GetParameter("@Slot_Id", Slot_Id));
            parameterList.Add(GetParameter("@Slot_Type", Slot_Type));

            ExecuteScalar("DeleteEvent_Abandoned", parameterList);
            return true;
        }

        public int ConvertAbandonedCart(Guid CartGuid, int userId, string email, int ReservationId)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@CartGuid", CartGuid));
            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@email", email));
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            string sqlQuery = "update Event_Abandoned set User_Id = @userId ,Email = @email,ConvertedReservationId = @ReservationId where OrderGUID = @CartGuid";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text);
        }

        //Create method for insert and update data from open table api to save in opentablemember table in DB.
        public void InsertAndUpdateOpenTableMember(string jsonData)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@jsonData", jsonData));
            ExecuteScalar("AddUpdateOpenTableMembers", parameterList);
        }

        public int ReconcileOpenTableData()
        {
            var parameterList = new List<DbParameter>();

            return ExecuteNonQuery("ReconcileOpenMemberData", parameterList, CommandType.StoredProcedure);
        }

        public bool UpdateReservationPaycardtoken(int reservationid, string paycardtoken)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@paycardtoken", paycardtoken));
            parameterList.Add(GetParameter("@reservationid", reservationid));

            string sqlQuery = "update reservationv2 set paycardtoken = @paycardtoken where reservationid = @reservationid";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdateReservationPaycardDetails(int reservationid, string paycardtoken, string PayCardType, string PayCardNumber, string PayCardCustName, string PayCardExpMonth, string PayCardExpYear)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@paycardtoken", paycardtoken));
            parameterList.Add(GetParameter("@reservationid", reservationid));
            parameterList.Add(GetParameter("@PayCardType", PayCardType));
            parameterList.Add(GetParameter("@PayCardNumber", PayCardNumber));
            parameterList.Add(GetParameter("@PayCardCustName", PayCardCustName));
            parameterList.Add(GetParameter("@PayCardExpMonth", PayCardExpMonth));
            parameterList.Add(GetParameter("@PayCardExpYear", PayCardExpYear));

            string sqlQuery = "update reservationv2 set paycardtoken = @paycardtoken,PayCardType = @PayCardType,PayCardNumber = @PayCardNumber,PayCardCustName = @PayCardCustName,PayCardExpMonth = @PayCardExpMonth,PayCardExpYear = @PayCardExpYear where reservationid = @reservationid";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public List<TripAdvisorMember> GetTripAdvisorActiveMembers()
        {
            List<TripAdvisorMember> tripAdvisorMembers = new List<TripAdvisorMember>();
            var parameterList = new List<DbParameter>();


            using (DbDataReader dataReader = GetDataReader("GetTripAdvisorMembers", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        TripAdvisorMember member = new TripAdvisorMember();
                        member.TripAdvisorId = Convert.ToString(dataReader["TripAdvisorId"]);
                        member.MemberId = Convert.ToInt32(dataReader["Id"]);
                        tripAdvisorMembers.Add(member);
                    }
                }
            }

            return tripAdvisorMembers;

        }
        public void UpdateTripAdvisorRating(int memberId, string jsonData, string rating)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@memberId", memberId));
            parameterList.Add(GetParameter("@jsonData", jsonData));
            parameterList.Add(GetParameter("@rating", rating));
            ExecuteScalar("UpdateTripAdvisorRating", parameterList);
        }

        public bool FinishMemberSignupProcess(int memberId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@memberId", memberId));

            string sqlQuery = "update Winery set Active = 1, AccountStatus=1 where Id = @memberId";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;

        }

        public bool UpdateReservationGoogleCalendarEventURL(int ID, string GoogleCalendarEventEditURL)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ID", ID));
            parameterList.Add(GetParameter("@GoogleCalendarEventEditURL", GoogleCalendarEventEditURL));

            string sqlQuery = "UPDATE [ReservationV2] SET GoogleCalendarEventEditURL = @GoogleCalendarEventEditURL  WHERE ReservationId = @ID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdateGoogleCalendarAuth(int wineryId, string Authorization)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@memberId", wineryId));
            parameterList.Add(GetParameter("@Authorization", Authorization));

            string sqlQuery = "update [Settings] set [Value] = @Authorization where [Winery_Id] = @memberId and [Group] = 10 and [Key] = 13";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public EmailValidationLogModel GetEmailValidationLogByEmail(string email)
        {
            var parameterList = new List<DbParameter>();
            EmailValidationLogModel logModel = null;

            parameterList.Add(GetParameter("@Email", email));
            using (DbDataReader dataReader = GetDataReader("GetEmailValidationLogByEmail", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        logModel = new EmailValidationLogModel();
                        logModel.Id = Convert.ToInt32(dataReader["Id"]);
                        logModel.Email = Convert.ToString(dataReader["Email"]);
                        logModel.UserId = Convert.ToInt32(dataReader["UserId"]);
                        logModel.EmailStatus = (EmailValidStatus)Convert.ToInt32(dataReader["EmailStatus"]);
                        logModel.StatusType = (EmailStatusType)Convert.ToInt32(dataReader["StatusType"]);
                        logModel.WebhookEvent = (EmailWebhookEvent)Convert.ToInt32(dataReader["WebhookEvent"]);
                        logModel.LogDate = Convert.ToDateTime(dataReader["LogDate"]);

                    }
                }
            }

            return logModel;

        }


        public EmailValidStatus IsValidEmail(string email)
        {
            EmailValidStatus status = EmailValidStatus.na;
            var logData = GetEmailValidationLogByEmail(email);
            if (logData != null)
            {
                status = logData.EmailStatus;
            }
            return status;
        }

        public void AddEmailValidationLog(EmailValidationLogModel data)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@Email", data.Email));
            parameterList.Add(GetParameter("@UserId", data.UserId));
            parameterList.Add(GetParameter("@EmailStatus", (int)data.EmailStatus));
            parameterList.Add(GetParameter("@StatusType", (int)data.StatusType));
            parameterList.Add(GetParameter("@WebhookEvent", (int)data.WebhookEvent));

            ExecuteScalar("AddEmailValidationLog", parameterList);

        }

        public List<string> GetGoogleIndexerList()
        {
            List<string> googleIndexerList = new List<string>();
            var parameterList = new List<DbParameter>();


            using (DbDataReader dataReader = GetDataReader("GetGoogleIndexerList", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        googleIndexerList.Add(Convert.ToString(dataReader["URL"]));
                    }
                }
            }

            return googleIndexerList;
        }

        public void UpdateGoogleIndexerStatus(string url, int status = 1)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@url", url));
            parameterList.Add(GetParameter("@status", status));

            ExecuteScalar("UpdateGoogleIndexerStatus", parameterList);

        }

        public ZoomToken GetZoomTokenByMember(int memberId)
        {
            var parameterList = new List<DbParameter>();
            ZoomToken token = null;

            parameterList.Add(GetParameter("@MemberId", memberId));
            using (DbDataReader dataReader = GetDataReader("GetZoomTokenByMember", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        token = new ZoomToken();
                        token.Id = Convert.ToInt32(dataReader["Id"]);
                        token.MemberId = memberId;
                        token.RefreshToken = Convert.ToString(dataReader["refresh_token"]);
                        token.AccessToken = Convert.ToString(dataReader["access_token"]);
                        token.Expires = Convert.ToDateTime(dataReader["Expires"]);
                        token.ExpiresIn = Convert.ToInt32(dataReader["expires_in"]);
                        token.TokenType = Convert.ToString(dataReader["token_type"]);

                    }
                }
            }

            return token;
        }

        public void UpdateZoomTokenOfMember(ZoomToken token)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@MemberId", token.MemberId));
            parameterList.Add(GetParameter("@AccessToken", token.AccessToken));
            parameterList.Add(GetParameter("@TokenType", token.TokenType));
            parameterList.Add(GetParameter("@RefreshToken", token.RefreshToken));
            parameterList.Add(GetParameter("@ExpiresIn", token.ExpiresIn));
            parameterList.Add(GetParameter("@Scope", token.Scope));

            ExecuteScalar("UpdateZoomTokenForMember", parameterList);

        }

        public void InsertZoomMeetingInfo(int ReservationId, int SlotId, int SlotType, long MeetingId, string MeetingURL, string RegistrantId, DateTime StartDate, int MeetingBehavior)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@SlotType", SlotType));
            parameterList.Add(GetParameter("@MeetingId", MeetingId));
            parameterList.Add(GetParameter("@MeetingURL", MeetingURL));
            parameterList.Add(GetParameter("@RegistrantId", RegistrantId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@MeetingBehavior", MeetingBehavior));

            ExecuteScalar("InsertZoomMeetingInfo", parameterList);
        }

        public ZoomMeetingInfo GetZoomMeetingInfo(int SlotId, int SlotType, DateTime StartDate, int ReservationId = 0)
        {
            var parameterList = new List<DbParameter>();
            ZoomMeetingInfo zoomMeetingInfo = new ZoomMeetingInfo();

            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@SlotType", SlotType));
            parameterList.Add(GetParameter("@StartDate", StartDate.Date));
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader("GetZoomMeetingInfo", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        zoomMeetingInfo.Id = Convert.ToInt32(dataReader["Id"]);
                        zoomMeetingInfo.MeetingId = Convert.ToInt64(dataReader["MeetingId"]);
                        zoomMeetingInfo.ReservationId = Convert.ToInt32(dataReader["ReservationId"]);
                        zoomMeetingInfo.MeetingURL = Convert.ToString(dataReader["MeetingURL"]);
                        zoomMeetingInfo.RegistrantId = Convert.ToString(dataReader["RegistrantId"]);
                        zoomMeetingInfo.MeetingBehavior = Convert.ToInt32(dataReader["MeetingBehavior"]);
                    }
                }
            }
            return zoomMeetingInfo;
        }

        public ZoomMeetingInfo GetZoomMeetingInfoByReservationId(int ReservationId)
        {
            var parameterList = new List<DbParameter>();
            ZoomMeetingInfo zoomMeetingInfo = new ZoomMeetingInfo();

            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader("GetZoomMeetingInfoByReservationId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        zoomMeetingInfo.Id = Convert.ToInt32(dataReader["Id"]);
                        zoomMeetingInfo.MeetingId = Convert.ToInt64(dataReader["MeetingId"]);
                        zoomMeetingInfo.ReservationId = Convert.ToInt32(dataReader["ReservationId"]);
                        zoomMeetingInfo.MeetingURL = Convert.ToString(dataReader["MeetingURL"]);
                        zoomMeetingInfo.RegistrantId = Convert.ToString(dataReader["RegistrantId"]);
                        zoomMeetingInfo.MeetingBehavior = Convert.ToInt32(dataReader["MeetingBehavior"]);
                        zoomMeetingInfo.SlotId = Convert.ToInt32(dataReader["SlotId"]);
                        zoomMeetingInfo.SlotType = Convert.ToInt32(dataReader["SlotType"]);
                        zoomMeetingInfo.StartDate = Convert.ToDateTime(dataReader["StartDate"]);
                    }
                }
            }
            return zoomMeetingInfo;
        }

        public long GetMeetingIdByReservationId(int ReservationId)
        {
            long MeetingId = 0;

            string sql = "SELECT [MeetingId] FROM [ZoomMeetingInfo] where ReservationId = @ReservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        MeetingId = Convert.ToInt64(dataReader["MeetingId"]);
                    }
                }
            }
            return MeetingId;
        }

        public void DeleteZoomMeetingInfoByMeetingId(long MeetingId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MeetingId", MeetingId));

            string sql = "DELETE FROM ZoomMeetingInfo WHERE MeetingId = @MeetingId";

            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }

        public void DeleteZoomMeetingInfoByReservationId(int ReservationId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            string sql = "DELETE FROM ZoomMeetingInfo WHERE ReservationId = @ReservationId";

            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }

        public void SaveAccountTypes(string ContactTypeId, string ContactType)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ContactTypeId", ContactTypeId));
            parameterList.Add(GetParameter("@ContactType", ContactType));

            string sql = "INSERT INTO [dbo].[ThirdParty_AccountTypes] ([WineryId],[ThirdPartyId],[ContactTypeId],[ContactType],[ContactTypeImageId],[DefaultAccountType],[IsAvailable],[IsDefaultAccountType],[ActiveClub]) ";
            sql += " VALUES ";
            sql += "(8529,2,@ContactTypeId,@ContactType,0,0,1,0,0 )";
            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }

        public VisitorStatistics GetVisitorStaticByMemberOrEvent(int memberId, DateTime startDate, DateTime endDate, int eventId = 0)
        {
            VisitorStatistics data = new VisitorStatistics();

            var parameterList = new List<DbParameter>();


            parameterList.Add(GetParameter("@MemberId", memberId));
            parameterList.Add(GetParameter("@StartDate", startDate));
            parameterList.Add(GetParameter("@EndDate", endDate));
            parameterList.Add(GetParameter("@EventId", eventId));
            using (DbDataReader dataReader = GetDataReader("GetVisitorStatisticsByMemberorEvent", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        data.member_id = Convert.ToInt32(dataReader["MemberId"]);
                        data.profile_page_views = Convert.ToInt32(dataReader["ProfilePageViews"]);
                        data.ticket_event_views = Convert.ToInt32(dataReader["TotTicketEventViews"]);
                        data.directory_profile_views = Convert.ToInt32(dataReader["DirectoryProfileViews"]);
                        data.profile_widget_views = Convert.ToInt32(dataReader["ProfileWidgetViews"]);
                        data.ticket_order_views = Convert.ToInt32(dataReader["TicketOrderViews"]);
                        data.rsvp_detail_views = Convert.ToInt32(dataReader["RSVPDetailViews"]);

                    }
                }
            }
            return data;

        }

        public void InsertTempCardDetail(string PayCardType, string PayCardNumber, string PayCardCustName, string PayCardExpMonth, string PayCardExpYear, string PayCardToken, int MemberId, string Vin65Username, string Vin65Password,string cvv)
        {
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@PayCardType", PayCardType));
            parameterList.Add(GetParameter("@PayCardNumber", PayCardNumber));
            parameterList.Add(GetParameter("@PayCardCustName", PayCardCustName));
            parameterList.Add(GetParameter("@PayCardExpMonth", PayCardExpMonth));
            parameterList.Add(GetParameter("@PayCardExpYear", PayCardExpYear));
            parameterList.Add(GetParameter("@PayCardToken", PayCardToken));
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@Vin65Username", Vin65Username));
            parameterList.Add(GetParameter("@Vin65Password", Vin65Password));
            parameterList.Add(GetParameter("@cvv", cvv));

            ExecuteScalar("InsertTempCardDetail", parameterList);
        }

        public void InsertCreditCardDetail(string PayCardType, string PayCardCustName, string PayCardExpMonth, string PayCardExpYear, string PayCardToken, int MemberId, string PayCardLastFourDigits, string PayCardFirstFourDigits,int Source,string UserName,string FirstName,string LastName,int UserId,int PaymentGateway)
        {
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@PayCardType", PayCardType));
            parameterList.Add(GetParameter("@PayCardLastFourDigits", PayCardLastFourDigits));
            parameterList.Add(GetParameter("@PayCardFirstFourDigits", PayCardFirstFourDigits));
            parameterList.Add(GetParameter("@PayCardCustName", PayCardCustName));
            parameterList.Add(GetParameter("@PayCardExpMonth", PayCardExpMonth));
            parameterList.Add(GetParameter("@PayCardExpYear", PayCardExpYear));
            parameterList.Add(GetParameter("@PayCardToken", PayCardToken));
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@Source", Source));
            parameterList.Add(GetParameter("@UserName", UserName));
            parameterList.Add(GetParameter("@FirstName", FirstName));
            parameterList.Add(GetParameter("@LastName", LastName));
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@PaymentGateway", PaymentGateway));

            ExecuteScalar("InsertCreditCardDetail", parameterList);
        }

        public bool UpdateReservationCreditCardReferenceNumber(int ReservationID, string CreditCardReferenceNumber)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationID", ReservationID));
            parameterList.Add(GetParameter("@CreditCardReferenceNumber", CreditCardReferenceNumber));

            string sqlQuery = "update ReservationV2 set CreditCardReferenceNumber =@CreditCardReferenceNumber where ReservationId=@ReservationID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool DeleteTempCardDetail(int MemberId, string PayCardToken)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@PayCardToken", PayCardToken));

            string sqlQuery = "DELETE FROM [TempCardDetail] WHERE PayCardToken = @PayCardToken and MemberId = @MemberId";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public TempCardDetail GetTempCardDetail(int MemberId, string PayCardToken)
        {
            TempCardDetail data = new TempCardDetail();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@PayCardToken", PayCardToken));
            using (DbDataReader dataReader = GetDataReader("GetTempCardDetail", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        data.MemberId = Convert.ToInt32(dataReader["MemberId"]);
                        data.PayCardCustName = Convert.ToString(dataReader["PayCardCustName"]);
                        data.PayCardExpMonth = Convert.ToString(dataReader["PayCardExpMonth"]);
                        data.PayCardExpYear = Convert.ToString(dataReader["PayCardExpYear"]);
                        data.PayCardNumber = Convert.ToString(dataReader["PayCardNumber"]);
                        data.PayCardToken = Convert.ToString(dataReader["PayCardToken"]);
                        data.PayCardType = Convert.ToString(dataReader["PayCardType"]);
                        data.Vin65Password = Convert.ToString(dataReader["Vin65Password"]);
                        data.Vin65Username = Convert.ToString(dataReader["Vin65Username"]);
                        data.cvv = Convert.ToString(dataReader["cvv"]);

                    }
                }
            }
            DeleteTempCardDetail(MemberId, PayCardToken);
            if(PayCardToken!= data.PayCardToken)
                 DeleteTempCardDetail(MemberId, data.PayCardToken);
            return data;
        }

        public TempCardDetail GetTempCardByNumberDetail(int MemberId,string PayCardNumber)
        {
            TempCardDetail data = new TempCardDetail();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@PayCardNumber", PayCardNumber));
            using (DbDataReader dataReader = GetDataReader("GetTempCardByNumberDetail", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        data.MemberId = Convert.ToInt32(dataReader["MemberId"]);
                        data.PayCardCustName = Convert.ToString(dataReader["PayCardCustName"]);
                        data.PayCardExpMonth = Convert.ToString(dataReader["PayCardExpMonth"]);
                        data.PayCardExpYear = Convert.ToString(dataReader["PayCardExpYear"]);
                        data.PayCardNumber = Convert.ToString(dataReader["PayCardNumber"]);
                        data.PayCardToken = Convert.ToString(dataReader["PayCardToken"]);
                        data.PayCardType = Convert.ToString(dataReader["PayCardType"]);
                        data.Vin65Password = Convert.ToString(dataReader["Vin65Password"]);
                        data.Vin65Username = Convert.ToString(dataReader["Vin65Username"]);
                        data.Vin65Username = Convert.ToString(dataReader["Vin65Username"]);
                        data.cvv = Convert.ToString(dataReader["cvv"]);
                    }
                }
            }
            DeleteTempCardDetail(data.MemberId, data.PayCardToken);
            return data;
        }

        public List<CreditCardDetail> GetCreditCardDetail(int member_id, Common.ModuleType source_module, int user_id, string email, Configuration.Gateway PaymentGateway)
        {
            List<CreditCardDetail> list = new List<CreditCardDetail>();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@MemberId", member_id));
            parameterList.Add(GetParameter("@Source", (int)source_module));
            parameterList.Add(GetParameter("@PaymentGateway", (int)PaymentGateway));
            parameterList.Add(GetParameter("@UserId", user_id));
            parameterList.Add(GetParameter("@UserName", email));
            using (DbDataReader dataReader = GetDataReader("GetCreditCardDetail", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        CreditCardDetail data = new CreditCardDetail();
                        
                        data.card_exp_month = Convert.ToString(dataReader["PayCardExpMonth"]);
                        data.card_exp_year = Convert.ToString(dataReader["PayCardExpYear"]);
                        data.card_token = Convert.ToString(dataReader["PayCardToken"]);
                        data.card_type = Convert.ToString(dataReader["PayCardType"]);
                        data.customer_name = Convert.ToString(dataReader["PayCardCustName"]);
                        data.first_four_digits = Convert.ToString(dataReader["PayCardFirstFourDigits"]);
                        data.cust_id = Convert.ToString(dataReader["Id"]);

                        bool is_expired = true;
                        int year = 0;
                        int.TryParse(data.card_exp_year, out year);

                        int month = 0;

                        if (data.card_exp_month.StartsWith("0"))
                            int.TryParse(data.card_exp_month.Replace("0",""), out month);
                        else
                            int.TryParse(data.card_exp_month, out month);

                        if (year > DateTime.Now.Year)
                        {
                            is_expired = false;
                        }
                        else if (year == DateTime.Now.Year && month >= DateTime.Now.Month)
                        {
                            is_expired = false;
                        }

                        data.is_expired = is_expired;
                        data.last_four_digits = Convert.ToString(dataReader["PayCardLastFourDigits"]);
                        data.card_expiration = data.card_exp_month + "/" + data.card_exp_year;

                        if (!is_expired)
                            list.Add(data);
                    }
                }
            }
            
            return list;

        }

        public string GetPayCardTokenByRsvpId(int Reservation_Id)
        {
            string PayCardToken = string.Empty;

            string sql = "select PayCardToken from ReservationV2 (nolock) where ReservationId = @Reservation_Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Reservation_Id", Reservation_Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        PayCardToken = Convert.ToString(dataReader["PayCardToken"]);
                    }
                }
            }

            if (string.IsNullOrEmpty(PayCardToken))
            {
                PayCardToken = GetReservationPayCardTokenByRsvpId(Reservation_Id);
            }

            return PayCardToken;
        }

        public string GetReservationPayCardTokenByRsvpId(int Reservation_Id)
        {
            string PayCardToken = string.Empty;

            string sql = "select PayCardToken from reservationv2 (nolock) where ReservationId = @Reservation_Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Reservation_Id", Reservation_Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        PayCardToken = Convert.ToString(dataReader["PayCardToken"]);
                    }
                }
            }
            return PayCardToken;
        }

        public List<FailedMailModel> GetFailedMailList()
        {
            List<FailedMailModel> list = new List<FailedMailModel>();

            using (DbDataReader dataReader = GetDataReader("GetFailedMailList", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        FailedMailModel data = new FailedMailModel();
                        data.AffiliateID = Convert.ToInt32(dataReader["AffiliateID"]);
                        data.BookingCode = Convert.ToString(dataReader["BookingCode"]);
                        data.ReferralType = Convert.ToInt32(dataReader["ReferralType"]);
                        data.ReservationId = Convert.ToInt32(dataReader["reservationid"]);
                        data.UserId = Convert.ToInt32(dataReader["UserId"]);

                        list.Add(data);
                    }
                }
            }
            return list;
        }

        public bool AlreadySentRsvpCancelEmail(int RsvpId)
        {
            bool IsSentRsvpCancelEmail = false;

            try
            {
                string sql = "select Id from emaillog where refid = @RsvpId and emailtype=9";

                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@RsvpId", RsvpId));

                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            IsSentRsvpCancelEmail = true;
                        }
                    }
                }
            }
            catch { }

            return IsSentRsvpCancelEmail;
        }

        public SurveyWaiverStatus GetSurveyWaiverStatusByEmailAndMemberId(int MemberId, string email, int guest_id = 0)
        {
            SurveyWaiverStatus surveyWaiverStatus = new SurveyWaiverStatus();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@Email", email));
            parameterList.Add(GetParameter("@guest_id", guest_id));

            using (DbDataReader dataReader = GetDataReader("GetSurveyWaiverStatusByEmailAndMemberId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        surveyWaiverStatus.survey_status = (RSVPPostCaptureStatus)Convert.ToInt32(dataReader["SurveyStatus"]);
                        surveyWaiverStatus.waiver_status = (RSVPPostCaptureStatus)Convert.ToInt32(dataReader["WaiverStatus"]);

                        if (surveyWaiverStatus.survey_status != RSVPPostCaptureStatus.NA)
                        {
                            DateTime expirydate = Convert.ToDateTime(dataReader["expirydate"]);
                            DateTime dateModified = Convert.ToDateTime(dataReader["dateModified"]);

                            if (expirydate.Year != 1900)
                                surveyWaiverStatus.survey_expire_date = expirydate;

                            if (dateModified.Year != 1900)
                                surveyWaiverStatus.modified_date = dateModified;
                        }
                    }
                }
            }
            return surveyWaiverStatus;
        }

        public List<TaskMemberDetails> GetActiveWineryForExport(DateTime localDateTime)
        {
            List<TaskMemberDetails> list = new List<TaskMemberDetails>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@LocalDateTime", localDateTime));
            using (DbDataReader dataReader = GetDataReader("GetActiveWineryForExport", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TaskMemberDetails data = new TaskMemberDetails();

                        data.Id = Convert.ToInt32(dataReader["Id"]);
                        data.EnableSyncDate = Convert.ToInt32(dataReader["EnableSyncDate"]);
                        data.BillingPlan = Convert.ToInt32(dataReader["BillingPlan"]);
                        data.OrderPortApiKey = Convert.ToString(dataReader["OrderPortApiKey"]);
                        data.OrderPortApiToken = Convert.ToString(dataReader["OrderPortApiToken"]);
                        data.OrderPortClientId = Convert.ToString(dataReader["OrderPortClientId"]);
                        data.EnablePayments = Convert.ToBoolean(dataReader["EnablePayments"]);
                        data.EnableVin65 = Convert.ToBoolean(dataReader["EnableVin65"]);
                        data.Vin65Username = Convert.ToString(dataReader["Vin65Username"]);
                        data.Vin65Password = Convert.ToString(dataReader["Vin65Password"]);
                        data.eWineryEnabled = Convert.ToBoolean(dataReader["eWineryEnabled"]);
                        data.eWineryUsername = Convert.ToString(dataReader["eWineryUsername"]);
                        data.eWineryPassword = Convert.ToString(dataReader["eWineryPassword"]);
                        data.EnableYelp = Convert.ToBoolean(dataReader["EnableYelp"]);
                        data.EnableOrderPort = Convert.ToBoolean(dataReader["EnableOrderPort"]);
                        data.EnableCommerce7 = Convert.ToBoolean(dataReader["EnableCommerce7"]);
                        data.Commerce7Username = Convert.ToString(dataReader["Commerce7Username"]);
                        data.Commerce7Password = Convert.ToString(dataReader["Commerce7Password"]);
                        data.Commerce7Tenant = Convert.ToString(dataReader["Commerce7Tenant"]);
                        data.Commerce7POSProfileId = Convert.ToString(dataReader["Commerce7POSProfileId"]);
                        data.TimeZoneId = Convert.ToInt32(dataReader["TimeZoneId"]);
                        data.UpsertFulfillmentDate = Convert.ToBoolean(dataReader["UpsertFulfillmentDate"]);

                        list.Add(data);
                    }
                }
            }

            return list;

        }

        public List<TransactionsForExport> GetTransactionsForExport(int WineryId, DateTime StartDate, DateTime EndDate, int rsvpStatus, bool ByBookedDate, int ExportType)
        {
            List<TransactionsForExport> list = new List<TransactionsForExport>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));
            parameterList.Add(GetParameter("@rsvpStatus", rsvpStatus));
            parameterList.Add(GetParameter("@ByBookedDate", ByBookedDate));
            parameterList.Add(GetParameter("@ExportType", ExportType));
            using (DbDataReader dataReader = GetDataReader("GetTransactionsForExportV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TransactionsForExport data = new TransactionsForExport();

                        data.Id = Convert.ToInt32(dataReader["Id"]);
                        data.PayStatus = Convert.ToInt32(dataReader["PayStatus"]);
                        data.ExportId = Convert.ToInt32(dataReader["ExportId"]);

                        list.Add(data);
                    }
                }
            }

            return list;

        }

        public List<int> GetActiveWineriesForUserSync()
        {
            List<int> ids = new List<int>();
            using (DbDataReader dataReader = GetDataReader("GetActiveWineriesForUserSync", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    try
                    {
                        while (dataReader.Read())
                        {
                            int id = Convert.ToInt32(dataReader["Id"]);
                            ids.Add(id);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                    }
                }
            }
            return ids;
        }

        public List<GuestTags> GetGuestTagsByMemberId(int MemberId, bool show_all_tags)
        {
            var list = new List<GuestTags>();

            string sql = "select Tag,IsPublic,TagType from tags where member_id=@MemberId and (@show_all_tags = 1 or ispublic = 1) order by Tag";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", MemberId));
            parameterList.Add(GetParameter("@show_all_tags", show_all_tags));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new GuestTags();
                        model.tag = Convert.ToString(dataReader["Tag"]);
                        model.is_public = Convert.ToBoolean(dataReader["IsPublic"]);
                        model.tag_type = (TagType)Convert.ToInt32(dataReader["TagType"]);
                        if (model.tag_type == TagType.special_event)
                            model.tag_type_desc = "Visit";
                        if (model.tag_type == TagType.special_guests)
                            model.tag_type_desc = "Guest";
                        list.Add(model);
                    }
                }
            }
            return list;
        }


        public List<Event_FloorPlans> GetEventFloorPlansByEventId(int EventId)
        {
            var list = new List<Event_FloorPlans>();

            string sql = "select FloorPlanId,efp.SortOrder,PlanName,TechnicalName from Event_FloorPlans efp join Floor_Plan fp on efp.FloorPlanId = fp.Id where EventId=@EventId order by efp.SortOrder";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new Event_FloorPlans();
                        model.floor_plan_id = Convert.ToInt32(dataReader["FloorPlanId"]);
                        model.floor_plan_name = Convert.ToString(dataReader["PlanName"]);
                        model.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                        model.technical_name = Convert.ToString(dataReader["TechnicalName"]);
                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<Event_FloorPlans> GetFloorPlanByReservationId(int ReservationId)
        {
            var list = new List<Event_FloorPlans>();

            string sql = "select id FloorPlanId,PlanName,TechnicalName from Floor_Plan (nolock) fp join reservationv2 (nolock) r on fp.Id = r.FloorPlanId where reservationid = @ReservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new Event_FloorPlans();
                        model.floor_plan_id = Convert.ToInt32(dataReader["FloorPlanId"]);
                        model.floor_plan_name = Convert.ToString(dataReader["PlanName"]);
                        model.technical_name = Convert.ToString(dataReader["TechnicalName"]);
                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<FloorPlanModel> GetFloorPlansByMemberOrLocation(int wineryID, int locationId, bool active_only = true)
        {
            var model = new List<FloorPlanModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", wineryID));
            parameterList.Add(GetParameter("@LocationId", locationId));
            parameterList.Add(GetParameter("@ActiveOnly", active_only));

            using (DbDataReader dataReader = GetDataReader("GetFloorPlansByMemberOrLocation", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        FloorPlanModel floorPlan = new FloorPlanModel();
                        floorPlan.floor_plan_id = Convert.ToInt32(dataReader["Id"]);
                        floorPlan.plan_name = Convert.ToString(dataReader["PlanName"]);
                        floorPlan.location_id = Convert.ToInt32(dataReader["LocationID"]);
                        floorPlan.member_id = Convert.ToInt32(dataReader["WineryID"]);
                        floorPlan.location_name = Convert.ToString(dataReader["LocationName"]);
                        floorPlan.seating_reset_time = Convert.ToString(dataReader["SeatingResetTime"]);
                        floorPlan.plan_height = Convert.ToInt32(dataReader["Height"]);
                        floorPlan.plan_width = Convert.ToInt32(dataReader["Width"]);
                        floorPlan.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                        floorPlan.is_active = Convert.ToBoolean(dataReader["Active"]);
                        floorPlan.technical_name = Convert.ToString(dataReader["TechnicalName"]);
                        if (string.IsNullOrEmpty(floorPlan.seating_reset_time))
                        {
                            floorPlan.seating_reset_time = "04:00:00";
                        }

                        model.Add(floorPlan);
                    }
                }
            }
            return model;
        }

        public int GetAvailableQtyForPrivateReservation(int WineryID, int FloorPlanId, DateTime start_date, DateTime end_date)
        {
            int id = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));
            parameterList.Add(GetParameter("@FloorPlanId", FloorPlanId));
            parameterList.Add(GetParameter("@StartDate", start_date));
            parameterList.Add(GetParameter("@EndDate", end_date));

            using (DbDataReader dataReader = GetDataReader("GetAvailableQtyForPrivateReservation", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    try
                    {
                        while (dataReader.Read())
                        {
                            id = Convert.ToInt32(dataReader["AvailableQty"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                    }
                }
            }
            return id;
        }

        public CheckAvailableQtyPrivatersvpModel CheckifTableAvailableCanFitGuestForPrivateRSVP(int ReservationId, int GuestsCount)
        {
            CheckAvailableQtyPrivatersvpModel model = new CheckAvailableQtyPrivatersvpModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@GuestsCount", GuestsCount));

            using (DbDataReader dataReader = GetDataReader("CheckifTableAvailableCanFitGuestForPrivateRSVP", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    try
                    {
                        while (dataReader.Read())
                        {
                            model.party_can_seated = Convert.ToBoolean(dataReader["SlotAvailable"]);
                            model.max_qty_available = Convert.ToInt32(dataReader["GuestCountAvl"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                    }
                }
            }
            return model;
        }

        public List<ReservationChargeModel> GetTransactionsForCCAutomationTaskV2()
        {
            var model = new List<ReservationChargeModel>();

            using (DbDataReader dataReader = GetDataReader("GetTransactionsForCCAutomationTaskV2", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ReservationChargeModel reservationChargeModel = new ReservationChargeModel();
                        reservationChargeModel.ReservationId = Convert.ToInt32(dataReader["ReservationId"]);
                        reservationChargeModel.BalanceDue = Convert.ToDecimal(dataReader["BalanceDue"]);

                        model.Add(reservationChargeModel);
                    }
                }
            }
            return model;
        }

        public List<ReservationDetailModel> GetReservationsV2ForReminderTask(DateTime CurrentDate, int ReminderHours)
        {
            var model = new List<ReservationDetailModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@CurrentDate", CurrentDate));
            parameterList.Add(GetParameter("@ReminderInterval", ReminderHours));

            using (DbDataReader dataReader = GetDataReader("GetReservationsV2ForReminderTask", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ReservationDetailModel reservationDetailModel = new ReservationDetailModel();
                        reservationDetailModel.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        reservationDetailModel.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        reservationDetailModel.member_id = Convert.ToInt32(dataReader["UserId"]);

                        model.Add(reservationDetailModel);
                    }
                }
            }
            return model;
        }

        public List<EventDetails> GetActiveEventId()
        {
            var model = new List<EventDetails>();

            string sql = "select distinct eventid from Events e (nolock) join winery w (nolock) on e.wineryid = w.id where [status]=1 and (EndDate >= getdate() or EndDate = '1/1/1900') and w.active=1 and w.billingplan > 0";

            using (DbDataReader dataReader = GetDataReader(sql, null, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        EventDetails eventDetails = new EventDetails();
                        eventDetails.event_id = Convert.ToInt32(dataReader["eventid"]);

                        model.Add(eventDetails);
                    }
                }
            }
            return model;
        }

        public bool IsDetailedAddressInfoRequired(int WineryId)
        {
            var ret = false;

            string sql = "select Id from PaymentConfig where Winery_ID = @WineryId and PaymentGateway = 9 and IsActive = 1";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", WineryId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        public int GetTableMaxPartybyTableID(int TableID)
        {
            int MaxParty = 0;
            string sql = "select MaxParty from Table_Layout where TableID = @TableID";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableID", TableID));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        MaxParty = Convert.ToInt32(dataReader["MaxParty"]);
                    }
                }
            }
            return MaxParty;
        }

        public AvlQtyForReservationIdModel GetAvlQtyForReservationId(int ReservationId)
        {
            var model = new AvlQtyForReservationIdModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            using (DbDataReader dataReader = GetDataReader("GetAvlQtyForReservationId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.Qty = Convert.ToInt32(dataReader["Qty"]);
                        model.FloorPlanId = Convert.ToInt32(dataReader["FloorPlanId"]);
                        model.StartDateTime = Convert.ToDateTime(dataReader["EventStartTime"]);
                        model.EndDatetime = Convert.ToDateTime(dataReader["EventEndTime"]);
                        model.SlotId = Convert.ToInt32(dataReader["SlotId"]);
                        model.RsvpQty = Convert.ToInt32(dataReader["RsvpQty"]);
                    }
                }
            }
            return model;
        }

        public List<PassportEventAvailabilityModel> GetPassortEventAvailability(int ticketEventId, int memberId, int guestCount, DateTime requestDate, int itineraryId, bool includeWaitList = false, int itemId = 0, string rsvp_access_codes = "")
        {
            List<PassportEventAvailabilityModel> availabilityList = new List<PassportEventAvailabilityModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", ticketEventId));
            parameterList.Add(GetParameter("@StartDate", requestDate));
            parameterList.Add(GetParameter("@Guest", guestCount));
            parameterList.Add(GetParameter("@MemberId", memberId));
            parameterList.Add(GetParameter("@ItineraryId", itineraryId));
            parameterList.Add(GetParameter("@IncludeWaitlist", includeWaitList));
            parameterList.Add(GetParameter("@ItineraryItemId", itemId));
            parameterList.Add(GetParameter("@RsvpAccessCodes", rsvp_access_codes));
            using (DbDataReader dataReader = GetDataReader("GetEventsListForPassportEvent", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new PassportEventAvailabilityModel();
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        model.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        model.end_time = Convert.ToString(dataReader["EndTime"]);
                        model.start_time = Convert.ToString(dataReader["StartTime"]);
                        model.start_date_time = Convert.ToDateTime(dataReader["StartDateTime"]);
                        model.end_date_time = Convert.ToDateTime(dataReader["EndDateTime"]);
                        availabilityList.Add(model);
                    }
                }
            }
            return availabilityList;
        }

        public List<PassportEventAvailabilityV2Model> GetPassortEventAvailabilityV2(int ticketEventId, int memberId, int guestCount, DateTime startDate, DateTime endDate, int itineraryId, bool includeWaitList = false, int itemId = 0, string rsvp_access_codes = "")
        {
            List<PassportEventAvailabilityV2Model> availabilityList = new List<PassportEventAvailabilityV2Model>();
            List<PassportEventDateV2> event_dates = new List<PassportEventDateV2>();
            List<PassportEventTimeV2> times = new List<PassportEventTimeV2>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", ticketEventId));
            parameterList.Add(GetParameter("@StartDate", startDate));
            parameterList.Add(GetParameter("@EndDate", endDate));
            parameterList.Add(GetParameter("@Guest", guestCount));
            parameterList.Add(GetParameter("@MemberId", memberId));
            parameterList.Add(GetParameter("@ItineraryId", itineraryId));
            parameterList.Add(GetParameter("@IncludeWaitlist", includeWaitList));
            parameterList.Add(GetParameter("@ItineraryItemId", itemId));
            parameterList.Add(GetParameter("@RsvpAccessCodes", rsvp_access_codes));

            string OldEventName = string.Empty;
            DateTime OldEventDate = Convert.ToDateTime("1/1/1900");
            PassportEventAvailabilityV2Model passportevent = new PassportEventAvailabilityV2Model();
            PassportEventDateV2 passportevent_dates = new PassportEventDateV2();


            using (DbDataReader dataReader = GetDataReader("GetEventsListForPassportEventV2", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        string EventName = Convert.ToString(dataReader["EventName"]);

                        if (string.IsNullOrEmpty(OldEventName) || OldEventName != EventName)
                        {
                            passportevent = new PassportEventAvailabilityV2Model();
                            OldEventDate = Convert.ToDateTime("1/1/1900");
                            OldEventName = EventName;

                            passportevent.event_name = EventName;
                            passportevent.event_dates = new List<PassportEventDateV2>();

                            availabilityList.Add(passportevent);
                        }

                        DateTime EventDate = Convert.ToDateTime(dataReader["StartDateTime"]);
                        if (OldEventDate.Year == 1900 || OldEventDate.ToString("MM/dd/yyyy") != EventDate.ToString("MM/dd/yyyy"))
                        {
                            passportevent_dates = new PassportEventDateV2();
                            OldEventDate = EventDate;
                            passportevent_dates.event_date = EventDate.ToString("MM/dd/yyyy");
                            passportevent_dates.times = new List<PassportEventTimeV2>();
                            passportevent.event_dates.Add(passportevent_dates);
                        }

                        PassportEventTimeV2 passporttimes = new PassportEventTimeV2();

                        passporttimes.slot_id = Convert.ToInt32(dataReader["SlotId"]);
                        passporttimes.slot_type = Convert.ToInt32(dataReader["SlotType"]);
                        passporttimes.start_time = Convert.ToString(dataReader["StartTime"]);
                        passporttimes.end_time = Convert.ToString(dataReader["EndTime"]);
                        passporttimes.start_date_time = Convert.ToDateTime(dataReader["StartDateTime"]);
                        passporttimes.end_date_time = Convert.ToDateTime(dataReader["EndDateTime"]);
                        passporttimes.availability_string = string.Format("{2} | {0} - {1}", passporttimes.start_time, passporttimes.end_time, EventName);

                        passportevent_dates.times.Add(passporttimes);
                    }
                }
            }

            return availabilityList;
        }

        public List<CreateReservationModel> GetItineraryDetailsForRSVPCreate(List<int> ItineraryIds, ReferralType referralType = ReferralType.CellarPass, int bookedById = 0)
        {
            List<CreateReservationModel> reservations = new List<CreateReservationModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ItineraryIds", string.Join(",", ItineraryIds.ToArray())));
            parameterList.Add(GetParameter("@ReferralType", (int)referralType));
            parameterList.Add(GetParameter("@BookedById", bookedById));
            using (DbDataReader dataReader = GetDataReader("GetItineraryEventDetailsForCreatingRSVP", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new CreateReservationModel();
                        model.EventId = Convert.ToInt32(dataReader["EventId"]);
                        model.EventName = Convert.ToString(dataReader["EventName"]);
                        model.LocationId = Convert.ToInt32(dataReader["LocationId"]);
                        model.EventLocation = Convert.ToString(dataReader["LocationName"]);
                        model.SlotId = Convert.ToInt32(dataReader["SlotId"]);
                        model.SlotType = Convert.ToInt32(dataReader["SlotType"]);
                        model.EndTime = (TimeSpan)(dataReader["EndTime"]);
                        model.StartTime = (TimeSpan)(dataReader["StartTime"]);
                        model.BookedById = Convert.ToInt32(dataReader["BookedById"]);
                        model.BookedByName = Convert.ToString(dataReader["BookedByName"]);
                        model.BookingCode = StringHelpers.GenerateRandomString(8, false);
                        model.ChargeFee = Convert.ToInt32(dataReader["ChargeFee"]);
                        model.TotalGuests = Convert.ToInt32(dataReader["Guests"]);
                        model.Status = Convert.ToInt32(dataReader["Status"]);
                        model.Email = Convert.ToString(dataReader["Email"]);
                        model.FirstName = Convert.ToString(dataReader["FirstName"]);
                        model.LastName = Convert.ToString(dataReader["LastName"]);
                        model.Address1 = Convert.ToString(dataReader["Address1"]);
                        model.Address2 = Convert.ToString(dataReader["Address2"]);
                        model.City = Convert.ToString(dataReader["City"]);
                        model.State = Convert.ToString(dataReader["UserState"]);
                        model.Zip = Convert.ToString(dataReader["Zip"]);
                        model.PhoneNumber = Convert.ToString(dataReader["HomePhoneStr"]);
                        model.MobilePhone = Convert.ToString(dataReader["MobilePhone"]);
                        model.MobilePhoneStatus = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        model.WineryId = Convert.ToInt32(dataReader["CellarPassMemberId"]);
                        model.RequireCreditCard = Convert.ToBoolean(dataReader["RequireCreditCard"]);
                        model.FeeDue = Convert.ToDecimal(dataReader["FeeDue"]);
                        model.AmountPaid = Convert.ToDecimal(dataReader["AmountPaid"]);
                        model.PurchaseTotal = Convert.ToDecimal(dataReader["PurchaseTotal"]);
                        model.SalesTax = Convert.ToDecimal(dataReader["SalesTax"]);
                        model.PayType = Convert.ToInt32(dataReader["PayType"]);
                        model.ReferralType = (int)referralType;
                        model.BookingDate = DateTime.UtcNow;
                        model.EventDate = Convert.ToDateTime(dataReader["EventDate"]);
                        model.FeePerPerson = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.CustomerType = Convert.ToInt32(dataReader["CustomerType"]);
                        model.EmailContentID = Convert.ToInt32(dataReader["EmailContentId"]);
                        model.Country = Convert.ToString(dataReader["Country"]);
                        model.UserId = Convert.ToInt32(dataReader["UserId"]);
                        model.CreditCardReferenceNumber = "";
                        model.DiscountCode = "";
                        model.HDYH = "";
                        model.TransportationName = "";
                        reservations.Add(model);
                    }
                }
            }
            return reservations;
        }
        public bool SaveReservationV2Transaction(int ReservationId, int WineryId, string BookingCode, DateTime EventDate, TimeSpan StartTime, TimeSpan EndTime,
            int SlotId, int SlotType, int UserId, string FirstName, string LastName, string Email, string ProductType, string SKU, string ProductName, int Qty, decimal Price,
            DateTime TxnDate, decimal SalesTax, decimal SalesTaxPercentage, decimal GratuityAmount, int PaymentId, string TransID, string TenderType, decimal TxnAmount,
            decimal DiscountAmt, int ChargeFee)
        {
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@BookingCode", BookingCode));
            parameterList.Add(GetParameter("@EventDate", EventDate));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@EndTime", EndTime));
            parameterList.Add(GetParameter("@SlotId", SlotId));
            parameterList.Add(GetParameter("@SlotType", SlotType));
            parameterList.Add(GetParameter("@UserId", UserId));
            parameterList.Add(GetParameter("@FirstName", FirstName));
            parameterList.Add(GetParameter("@LastName", LastName));
            parameterList.Add(GetParameter("@Email", Email));
            parameterList.Add(GetParameter("@ProductType", ProductType));
            parameterList.Add(GetParameter("@SKU", SKU));
            parameterList.Add(GetParameter("@ProductName", ProductName));
            parameterList.Add(GetParameter("@Qty", Qty));
            parameterList.Add(GetParameter("@Price", Price));
            parameterList.Add(GetParameter("@TxnDate", TxnDate));
            parameterList.Add(GetParameter("@SalesTax", SalesTax));
            parameterList.Add(GetParameter("@SalesTaxPercentage", SalesTaxPercentage));
            parameterList.Add(GetParameter("@GratuityAmount", GratuityAmount));
            parameterList.Add(GetParameter("@PaymentId", PaymentId));
            parameterList.Add(GetParameter("@TransID", TransID));
            parameterList.Add(GetParameter("@TenderType", TenderType));
            parameterList.Add(GetParameter("@TxnAmount", TxnAmount));
            parameterList.Add(GetParameter("@DiscountAmt", DiscountAmt));
            parameterList.Add(GetParameter("@ChargeFee", ChargeFee));

            int retvalue = ExecuteNonQuery("InsertReservationV2Transaction", parameterList, CommandType.StoredProcedure);

            return retvalue > 0;
        }

        public PrivateEventFormSubmittedModel SavePrivateEventRequest(PrivateEventRequest model)
        {
            PrivateEventFormSubmittedModel resp = new PrivateEventFormSubmittedModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@Country", model.country));
            parameterList.Add(GetParameter("@Details", model.details));
            parameterList.Add(GetParameter("@Email", model.email));
            parameterList.Add(GetParameter("@FirstName", model.first_name));
            parameterList.Add(GetParameter("@Guest", model.guest));
            parameterList.Add(GetParameter("@LastName", model.last_name));
            parameterList.Add(GetParameter("@WineryId", model.member_id));
            parameterList.Add(GetParameter("@PhoneNumber", model.phone_number));
            parameterList.Add(GetParameter("@PreferredDate", Convert.ToDateTime(model.preferred_date)));
            parameterList.Add(GetParameter("@PreferredStartTime", new TimeSpan(Convert.ToDateTime(model.preferred_start_time).TimeOfDay.Ticks)));
            parameterList.Add(GetParameter("@PreferredVisitDuration", model.preferred_visit_duration));
            parameterList.Add(GetParameter("@ReasonforVisit", model.reason_for_visit));
            parameterList.Add(GetParameter("@UserId", model.user_id));

            using (DbDataReader dataReader = GetDataReader("PrivateEventInsert", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        resp.id = Convert.ToInt32(dataReader["Id"]);
                        resp.private_event_guid = Convert.ToString(dataReader["PrivateEventGUID"]);
                    }
                }
            }

            return resp;
        }

        public PrivateEventRequestDetails GetPrivateEventRequestDetails(int id, string private_event_guid = "")
        {
            var model = new PrivateEventRequestDetails();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ID", id));
            parameterList.Add(GetParameter("@PrivateEventGUID", private_event_guid));

            using (DbDataReader dataReader = GetDataReader("GetPrivateEventRequestDetails", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.country = Convert.ToString(dataReader["Country"]);
                        model.details = Convert.ToString(dataReader["Details"]);
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.email = Convert.ToString(dataReader["Email"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.guest = Convert.ToInt32(dataReader["Guest"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        model.phone_number = Convert.ToString(dataReader["PhoneNumber"]);
                        model.preferred_date = Convert.ToDateTime(dataReader["PreferredDate"]);
                        model.preferred_start_time = TimeSpan.Parse(Convert.ToString(dataReader["PreferredStartTime"]));
                        model.preferred_visit_duration = Convert.ToInt32(dataReader["PreferredVisitDuration"]);
                        model.private_event_guid = Convert.ToString(dataReader["PrivateEventGUID"]);
                        model.reason_for_visit = Convert.ToInt32(dataReader["ReasonforVisit"]);
                        model.user_id = Convert.ToInt32(dataReader["UserId"]);
                        model.member_name = Convert.ToString(dataReader["displayname"]);
                    }
                }
            }
            return model;
        }

        public TableBlockedStatusModel CheckTableBlockedStatusByTranId(int tableId, int tranId, PreAssignServerTransactionType transactionType)
        {
            TableBlockedStatusModel model = new TableBlockedStatusModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableId", tableId));
            parameterList.Add(GetParameter("@TranId", tranId));
            parameterList.Add(GetParameter("@TransType", (int)transactionType));


            using (DbDataReader dataReader = GetDataReader("CheckTableBlockedByTranId", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.Id = Convert.ToInt32(dataReader["StatusId"]);
                        if (model.Id > 0)
                        {
                            model.BlockStartDate = Convert.ToDateTime(dataReader["BlockedStartDate"]);
                            model.BlockEndDate = Convert.ToDateTime(dataReader["BlockedEndDate"]);
                        }
                    }
                }
            }
            return model;
        }

        public TablePreassigendStatusModel CheckIfTablePreassignedAlready(int tableId, int tranId, PreAssignServerTransactionType transactionType)
        {
            TablePreassigendStatusModel model = new TablePreassigendStatusModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TableId", tableId));
            parameterList.Add(GetParameter("@TranId", tranId));
            parameterList.Add(GetParameter("@TransType", (int)transactionType));


            using (DbDataReader dataReader = GetDataReader("CheckIfTableAlreadyPreassignedByTranId", parameterList))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.ReservationCount = Convert.ToInt32(dataReader["ReservationCount"]);
                        model.WaitlistCount = Convert.ToInt32(dataReader["WaitListCount"]);
                    }
                }
            }
            return model;
        }

        public int GetWineryIdByCommerce7Data(string Commerce7Username, string Commerce7Password)
        {
            int member_Id = 0;
            string sql = "select Id from winery where Commerce7Username = @Commerce7Username and Commerce7Password = @Commerce7Password";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Commerce7Username", Commerce7Username));
            parameterList.Add(GetParameter("@Commerce7Password", Commerce7Password));
            var reader = GetDataReader(sql, parameterList, CommandType.Text);
            if (reader != null && reader.HasRows)
            {
                while (reader.Read())
                {
                    member_Id = Convert.ToInt32(reader["Id"]);
                }
            }

            return member_Id;
        }

        public int GetWineryIdByOrderPortData(string OrderPortApiKey)
        {
            int member_Id = 0;
            string sql = "select Id from winery where OrderPortApiKey = @OrderPortApiKey";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@OrderPortApiKey", OrderPortApiKey));
            var reader = GetDataReader(sql, parameterList, CommandType.Text);
            if (reader != null && reader.HasRows)
            {
                while (reader.Read())
                {
                    member_Id = Convert.ToInt32(reader["Id"]);
                }
            }

            return member_Id;
        }

        public bool RemoveReservationFromAutoSyncingQueue(int reservationId)
        {
            string sql = "update [ReservationV2] set ThirdPartyautosync=1 where reservationId=@reservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", reservationId));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            return (ret > 0);

        }

        public bool UpdateReminderSentForReservation(int reservationId)
        {
            string sql = "update [ReservationV2] set ReminderSent=1 where reservationId=@reservationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", reservationId));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);

            return (ret > 0);

        }

        public bool AlreadyRsvpExported(int reservationId)
        {
            bool ret = false;
            string sql = "select Id from ExportLog where Export_Id=@reservationId and exportstatus=1";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@reservationId", reservationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        public bool UpdateReservationSeatedStatus(int ReservationID, int reservationSeatedStatus)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationID", ReservationID));
            parameterList.Add(GetParameter("@SeatedStatus", reservationSeatedStatus));

            string sqlQuery = "update ReservationV2 set SeatedStatus =@SeatedStatus where ReservationId=@ReservationID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool AddCreditCardReservation(int ReservationId, string Zip, string PayCardType, string PayCardNumber, string PayCardCustName, string PayCardExpMonth, string PayCardExpYear, string PayCardToken)
        {
            int retval = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@Zip", Zip));
            parameterList.Add(GetParameter("@PayCardType", PayCardType));
            parameterList.Add(GetParameter("@PayCardNumber", PayCardNumber));
            parameterList.Add(GetParameter("@PayCardCustName", PayCardCustName));
            parameterList.Add(GetParameter("@PayCardExpMonth", PayCardExpMonth));
            parameterList.Add(GetParameter("@PayCardExpYear", PayCardExpYear));
            parameterList.Add(GetParameter("@PayCardToken", PayCardToken));
            retval = ExecuteNonQuery("AddCreditCardReservation", parameterList, CommandType.StoredProcedure);

            return retval > 0;
        }

        public List<EventScheduleModel> GetEventScheduleListV2(int wineryId, DateTime StartTime, int guestCount, int SlotId = 0, int SlotType = 0, int EventId = 0, bool ISAdmin = true, int reservationid = 0, int bookingtype = 0, bool hide_no_availability = false)
        {
            if (bookingtype == 2)
            {
                bookingtype = 0;
            }

            string sp = "GetEventSchdeuleListbyWineryIdAdminV2";
            var eventScheduleEvent = new List<EventScheduleModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@StartTime", StartTime));
            parameterList.Add(GetParameter("@GuestsCount", guestCount));
            parameterList.Add(GetParameter("@reservationid", reservationid));
            parameterList.Add(GetParameter("@bookingtype", bookingtype));

            if (!ISAdmin)
            {
                parameterList.Add(GetParameter("@SlotId", SlotId));
                parameterList.Add(GetParameter("@SlotType", SlotType));
                parameterList.Add(GetParameter("@callbtnSearch", false));
                parameterList.Add(GetParameter("@EventIdnum", EventId));

                sp = "GetEventScheduleListByWineryIdAndDateClientV2";
            }

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {

                    while (dataReader.Read())
                    {
                        var sched = new EventScheduleModel();
                        sched.event_id = Convert.ToInt32(dataReader["EventId"]);
                        sched.event_name = Convert.ToString(dataReader["EventName"]);
                        eventScheduleEvent.Add(sched);
                    }
                }
            }

            //eventSchedule.EventScheduleEventId = eventScheduleEvent;

            return eventScheduleEvent;
        }
        public List<ScoutPromotion> GetPromotionsByPromoType(int offer_type, int offer_schema_grp, string city, string state)
        {
            var ScoutOffers = new List<ScoutPromotion>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@OfferType", offer_type));
            parameterList.Add(GetParameter("@OfferSchemaGroupId", offer_schema_grp));
            parameterList.Add(GetParameter("@City", city));
            parameterList.Add(GetParameter("@State", state));
            using (DbDataReader dataReader = GetDataReader("GetPromotionsByType", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var offerData = new ScoutPromotion();
                        offerData.promo_id = Convert.ToInt32(dataReader["OfferId"]);
                        offerData.member_name = Convert.ToString(dataReader["DisplayName"]);
                        offerData.promo_name = Convert.ToString(dataReader["PromoName"]);
                        offerData.member_city = Convert.ToString(dataReader["City"]);
                        offerData.promo_used_count = Convert.ToInt32(dataReader["UserPromo"]);
                        offerData.member_banner_image = Convert.ToString(dataReader["MemberBannerImage"]);
                        offerData.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        offerData.favorite_count = Convert.ToInt32(dataReader["FavCount"]);
                        offerData.promotion_type = Convert.ToString(dataReader["GroupName"]);
                        int memberId = Convert.ToInt32(dataReader["MemberId"]);
                        offerData.member_id = memberId;
                        offerData.offer_name = Convert.ToString(dataReader["OfferName"]);
                        WineryReviews wineryReviews = GetWineryReviews(memberId);

                        offerData.total_reviews = wineryReviews.ReviewCount;
                        offerData.review_stars = wineryReviews.ReviewStars;
                        offerData.member_state = Convert.ToString(dataReader["State"]);
                        offerData.currency = Convert.ToString(dataReader["CurrencySymbol"]);
                        offerData.offer_price = Convert.ToString(dataReader["Price"]);
                        offerData.start_date = Convert.ToDateTime(dataReader["StartDate"]);
                        offerData.end_date = Convert.ToDateTime(dataReader["EndDate"]);

                        ScoutOffers.Add(offerData);
                    }
                }
            }
            return ScoutOffers;
        }

        public PromotionDetail GetPromotionDetail(int offerid)
        {
            var detail = new PromotionDetail();
            var offeraddressinfodata = new UserAddress();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@OfferId", offerid));
            using (DbDataReader dataReader = GetDataReader("GetCellarPromotionDetail", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        detail.promotions_Id = Convert.ToInt32(dataReader["Id"]);
                        detail.member_name = Convert.ToString(dataReader["MemberName"]);
                        detail.member_id = Convert.ToInt32(dataReader["MemberId"]);
                        detail.promo_Name = Convert.ToString(dataReader["PromoName"]);
                        detail.promo_zone = Convert.ToInt32(dataReader["PromoZone"]);
                        detail.promotions_promo_Value = Convert.ToInt32(dataReader["PromoValue"]);
                        detail.promo_value = Convert.ToString(dataReader["PromoValue"]);
                        detail.promo_end_date = Convert.ToDateTime(dataReader["EndDate"]);
                        detail.promo_fine_print = Convert.ToString(dataReader["PromoFinePrint"]);
                        detail.referral_code = Convert.ToString(dataReader["ReferralCode"]);
                        detail.redemption_instructions = Convert.ToString(dataReader["RedemptionInstructions"]);
                        detail.promo_value_detail = Convert.ToString(dataReader["PromoSchema"]);
                        detail.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        detail.member_business_phone = Convert.ToString(dataReader["BusinessPhone"]);
                        offeraddressinfodata.address_1 = Convert.ToString(dataReader["MemberAddress1"]);
                        offeraddressinfodata.address_2 = Convert.ToString(dataReader["MemberAddress2"]);
                        offeraddressinfodata.city = Convert.ToString(dataReader["City"]);
                        offeraddressinfodata.state = Convert.ToString(dataReader["State"]);
                        offeraddressinfodata.zip_code = Convert.ToString(dataReader["Zip"]);
                        offeraddressinfodata.country = Convert.ToString(dataReader["Country"]);
                        detail.member_address = offeraddressinfodata;
                        detail.promo_offer_value = Convert.ToDecimal(dataReader["PromoOfferValue"]);
                    }
                }
            }
            return detail;
        }

        public PromotionDetail GetProfilePagePromoByMemberId(int memberid)
        {
            var detail = new PromotionDetail();
            var offeraddressinfodata = new UserAddress();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", memberid));
            using (DbDataReader dataReader = GetDataReader("GetProfilePagePromoByMemberId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        detail.promotions_Id = Convert.ToInt32(dataReader["Id"]);
                        detail.member_name = Convert.ToString(dataReader["MemberName"]);
                        detail.promo_zone = Convert.ToInt32(dataReader["PromoZone"]);
                        detail.promotions_promo_Value = Convert.ToInt32(dataReader["PromoValue"]);
                        detail.promo_end_date = Convert.ToDateTime(dataReader["EndDate"]);
                        detail.promo_fine_print = Convert.ToString(dataReader["PromoFinePrint"]);
                        detail.referral_code = Convert.ToString(dataReader["ReferralCode"]);
                        detail.redemption_instructions = Convert.ToString(dataReader["RedemptionInstructions"]);
                        detail.promo_value_detail = Convert.ToString(dataReader["PromoSchema"]);
                        detail.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        detail.member_business_phone = Convert.ToString(dataReader["BusinessPhone"]);
                        offeraddressinfodata.address_1 = Convert.ToString(dataReader["MemberAddress1"]);
                        offeraddressinfodata.address_2 = Convert.ToString(dataReader["MemberAddress2"]);
                        offeraddressinfodata.city = Convert.ToString(dataReader["City"]);
                        offeraddressinfodata.state = Convert.ToString(dataReader["State"]);
                        offeraddressinfodata.zip_code = Convert.ToString(dataReader["Zip"]);
                        offeraddressinfodata.country = Convert.ToString(dataReader["Country"]);
                        detail.member_address = offeraddressinfodata;
                        detail.promo_offer_value = Convert.ToDecimal(dataReader["PromoOfferValue"]);
                    }
                }
            }
            return detail;
        }

        public int ClaimOffer(ClaimOfferRequest request)
        {
            int UserPromotionsId = 0;
            string sql = "InsertUserPromotions";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@User_Id", request.user_id));
            parameterList.Add(GetParameter("@Promotion_Id", request.promotion_id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        UserPromotionsId = Convert.ToInt32(dataReader["UserPromotionsId"]);
                    }
                }
            }

            return UserPromotionsId;
        }

        public List<PromotionDetailUser> GetPromotionsByUser(int user_id)
        {
            var PromotionsByUser = new List<PromotionDetailUser>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@User_Id", user_id));
            using (DbDataReader dataReader = GetDataReader("GetCellarUserPromotions", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var offerData = new PromotionDetailUser();
                        var offeraddressinfodata = new UserAddress();
                        offerData.id = Convert.ToInt32(dataReader["Id"]);
                        offerData.user_Id = Convert.ToInt32(dataReader["User_Id"]);
                        offerData.promotions_Id = Convert.ToInt32(dataReader["Promotions_Id"]);
                        offerData.promo_Name = Convert.ToString(dataReader["PromoName"]);
                        offerData.promo_fine_print = Convert.ToString(dataReader["PromoFinePrint"]);
                        offerData.promo_zone = Convert.ToInt32(dataReader["PromoZone"]);
                        offerData.promo_value = Convert.ToString(dataReader["PromoValue"]);
                        offerData.member_id = Convert.ToInt32(dataReader["MemberId"]);
                        offerData.member_name = Convert.ToString(dataReader["MemberName"]);
                        offerData.expiration_date = Convert.ToDateTime(dataReader["ExpirationDate"]);
                        offerData.referral_code = Convert.ToString(dataReader["ReferralCode"]);
                        offerData.redemption_instructions = Convert.ToString(dataReader["RedemptionInstructions"]);
                        offerData.promo_value_detail = Convert.ToString(dataReader["PromoValueDetail"]);
                        offerData.promotions_promo_Value = Convert.ToInt32(dataReader["Promotions_PromoValue"]);
                        offerData.claim_date = Convert.ToDateTime(dataReader["ClaimDate"]);
                        if (dataReader["RedemptionDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["RedemptionDate"].ToString()))
                            offerData.redemption_date = Convert.ToDateTime(dataReader["RedemptionDate"]);

                        offerData.promotion_code = Convert.ToString(dataReader["PromotionCode"]);
                        offerData.purchase_url = Convert.ToString(dataReader["MemberUrl"]);
                        offerData.member_business_phone = Convert.ToString(dataReader["BusinessPhone"]);
                        //offerData.member_mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        //offerData.member_fax_phone = Convert.ToString(dataReader["FaxPhone"]);
                        offeraddressinfodata.address_1 = Convert.ToString(dataReader["MemberAddress1"]);
                        offeraddressinfodata.address_2 = Convert.ToString(dataReader["MemberAddress2"]);
                        offeraddressinfodata.city = Convert.ToString(dataReader["City"]);
                        offeraddressinfodata.state = Convert.ToString(dataReader["State"]);
                        offeraddressinfodata.zip_code = Convert.ToString(dataReader["Zip"]);
                        offeraddressinfodata.country = Convert.ToString(dataReader["Country"]);
                        offerData.member_address = offeraddressinfodata;
                        PromotionsByUser.Add(offerData);
                    }
                }
            }
            return PromotionsByUser;
        }

        public bool RedeemPromotion(RedeemPromotionRequest request)
        {
            string sqlQuery = "ReedemPromotionForUser";

            var parameterList = new List<DbParameter>();            
            parameterList.Add(GetParameter("@Promotion_Id", request.promotion_id));
            parameterList.Add(GetParameter("@User_Id", request.user_id));
            parameterList.Add(GetParameter("@Promotion_Code", request.promotion_code));
            int ret = ExecuteNonQuery(sqlQuery, parameterList, CommandType.StoredProcedure);

            return (ret > 0);
        }

        public List<CellarScoutOfferTypesModel> GetCellarScoutOfferTypes()
        {
            var scoutOfferTypesList = new List<CellarScoutOfferTypesModel>();
            var parameterList = new List<DbParameter>();
            //parameterList.Add(GetParameter("@User_Id", user_id));
            using (DbDataReader dataReader = GetDataReader("GetCellarScoutOfferTypes", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var scoutOfferTypes = new CellarScoutOfferTypesModel();
                        scoutOfferTypes.id = Convert.ToInt32(dataReader["GroupId"]);
                        scoutOfferTypes.name = Convert.ToString(dataReader["GroupName"]);
                        scoutOfferTypesList.Add(scoutOfferTypes);
                    }
                }
            }
            return scoutOfferTypesList;
        }

        public List<PromotionDetail> GetPromotionsByMember(int member_id, int offer_type)
        {
            var promotionsByMember = new List<PromotionDetail>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Member_id", member_id));
            parameterList.Add(GetParameter("@OfferType", offer_type));
            using (DbDataReader dataReader = GetDataReader("GetPromotiuonsByMember", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var offerData = new PromotionDetail();
                        var offeraddressinfodata = new UserAddress();
                        offerData.promotions_Id = Convert.ToInt32(dataReader["Id"]);
                        offerData.promo_Name = Convert.ToString(dataReader["PromoName"]);
                        offerData.promo_fine_print = Convert.ToString(dataReader["PromoFinePrint"]);
                        offerData.promo_zone = Convert.ToInt32(dataReader["PromoZone"]);
                        offerData.promo_value = Convert.ToString(dataReader["PromoValue"]);
                        offerData.member_id = Convert.ToInt32(dataReader["MemberId"]);
                        offerData.member_name = Convert.ToString(dataReader["MemberName"]);
                        offerData.promo_end_date = Convert.ToDateTime(dataReader["EndDate"]);
                        offerData.referral_code = Convert.ToString(dataReader["ReferralCode"]);
                        offerData.redemption_instructions = Convert.ToString(dataReader["RedemptionInstructions"]);
                        offerData.promo_value_detail = Convert.ToString(dataReader["PromoValueDetail"]);
                        offerData.promotions_promo_Value = Convert.ToInt32(dataReader["PromoValue"]);
                        offerData.purchase_url = Convert.ToString(dataReader["MemberUrl"]);
                        offerData.member_business_phone = Convert.ToString(dataReader["BusinessPhone"]);
                        //offerData.member_mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        //offerData.member_fax_phone = Convert.ToString(dataReader["FaxPhone"]);
                        offeraddressinfodata.address_1 = Convert.ToString(dataReader["MemberAddress1"]);
                        offeraddressinfodata.address_2 = Convert.ToString(dataReader["MemberAddress2"]);
                        offeraddressinfodata.city = Convert.ToString(dataReader["City"]);
                        offeraddressinfodata.state = Convert.ToString(dataReader["State"]);
                        offeraddressinfodata.zip_code = Convert.ToString(dataReader["Zip"]);
                        offeraddressinfodata.country = Convert.ToString(dataReader["Country"]);
                        offerData.member_address = offeraddressinfodata;
                        promotionsByMember.Add(offerData);
                    }
                }
            }
            return promotionsByMember;
        }
        public List<CellarScoutLocationsModel> GetCellarScoutLocations()
        {
            var scoutLocationsList = new List<CellarScoutLocationsModel>();
            var parameterList = new List<DbParameter>();
            //parameterList.Add(GetParameter("@User_Id", user_id));
            using (DbDataReader dataReader = GetDataReader("GetCellarScoutLocations", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var scoutLocations = new CellarScoutLocationsModel();
                        scoutLocations.state_code = Convert.ToString(dataReader["StateCode"]);
                        scoutLocations.state = Convert.ToString(dataReader["State"]);                       
                        if (dataReader["City"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["City"].ToString()))
                            scoutLocations.city = dataReader["City"].ToString().Split(',').ToList();
                        scoutLocationsList.Add(scoutLocations);
                    }
                }
            }
            return scoutLocationsList;
        }

        public WelcomeDashboardDataModel GetWelcomeDashboardData()
        {
            var model = new WelcomeDashboardDataModel();
            List<LinkContent> Announcements = new List<LinkContent>();
            List<Article> PlatformUpdates = new List<Article>();
            List<Article> BecomeAPro = new List<Article>();
            List<Article> GettingStarted = new List<Article>();
            List<Article> BetterGuests = new List<Article>();          
            List<Article> SystemUpdates = new List<Article>();

            using (DbDataReader dataReader = GetDataReader("GetWelcomeDashboardData", null, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        LinkContent announcement = new LinkContent();
                        announcement.Id = Convert.ToInt32(dataReader["Id"]);
                        announcement.Active = Convert.ToBoolean(dataReader["Active"]);
                        announcement.Content = Convert.ToString(dataReader["Content"]);
                        announcement.DatePublished = Convert.ToDateTime(dataReader["DatePublished"]);
                        announcement.CreatedById = Convert.ToInt32(dataReader["CreatedBy"]);
                        announcement.DateModified = Convert.ToDateTime(dataReader["DateModified"]);
                        announcement.ModifiedById = Convert.ToInt32(dataReader["ModifiedBy"]);
                        announcement.LinkURL = Convert.ToString(dataReader["LinkURL"]);
                        announcement.ContentType = (Common.LinkContentType)Convert.ToInt32(dataReader["LinkContentType"]);   
                        Announcements.Add(announcement);
                    }
                }

                if (dataReader.NextResult())
                {
                    while (dataReader.Read())
                    {
                        Article platformupdates = new Article();
                        platformupdates.Id = Convert.ToInt32(dataReader["Id"]);
                        platformupdates.Title = Convert.ToString(dataReader["Title"]);
                        platformupdates.SubTitle = Convert.ToString(dataReader["SubTitle"]);
                        platformupdates.Content = Convert.ToString(dataReader["Content"]);
                        platformupdates.Tags = Convert.ToString(dataReader["Tags"]);
                        platformupdates.ArticleDate = Convert.ToDateTime(dataReader["ArticleDate"]);
                        platformupdates.ViewCount = Convert.ToInt32(dataReader["ViewCount"]);
                        platformupdates.SectionId = Convert.ToInt32(dataReader["Section_id"]);
                        platformupdates.CustomURL = Convert.ToString(dataReader["CustomURL"]);

                        PlatformUpdates.Add(platformupdates);
                    }
                }

                if (dataReader.NextResult())
                {
                    while (dataReader.Read())
                    {
                        Article becomeapro = new Article();
                        becomeapro.Id = Convert.ToInt32(dataReader["Id"]);
                        becomeapro.Title = Convert.ToString(dataReader["Title"]);
                        becomeapro.Content = Convert.ToString(dataReader["Content"]);
                        becomeapro.ArticleDate = Convert.ToDateTime(dataReader["ArticleDate"]);
                        becomeapro.FriendlyUrl = Convert.ToString(dataReader["FriendlyUrl"]);

                        BecomeAPro.Add(becomeapro);
                    }
                }

                if (dataReader.NextResult())
                {
                    while (dataReader.Read())
                    {
                        Article GettingStart = new Article();
                        GettingStart.Id = Convert.ToInt32(dataReader["Id"]);
                        GettingStart.Title = Convert.ToString(dataReader["Title"]);
                        GettingStart.SubTitle = Convert.ToString(dataReader["SubTitle"]);
                        GettingStart.Content = Convert.ToString(dataReader["Content"]);
                        GettingStart.Tags = Convert.ToString(dataReader["Tags"]);
                        GettingStart.ArticleDate = Convert.ToDateTime(dataReader["ArticleDate"]);
                        GettingStart.ViewCount = Convert.ToInt32(dataReader["ViewCount"]);
                        GettingStart.SectionId = Convert.ToInt32(dataReader["Section_id"]);
                        GettingStart.CustomURL = Convert.ToString(dataReader["CustomURL"]);

                        GettingStarted.Add(GettingStart);
                    }
                }

                if (dataReader.NextResult())
                {
                    while (dataReader.Read())
                    {
                        Article SystemUpdate = new Article();
                        SystemUpdate.Id = Convert.ToInt32(dataReader["Id"]);
                        SystemUpdate.Title = Convert.ToString(dataReader["Title"]);
                        SystemUpdate.SubTitle = Convert.ToString(dataReader["SubTitle"]);
                        SystemUpdate.Content = Convert.ToString(dataReader["Content"]);
                        SystemUpdate.Tags = Convert.ToString(dataReader["Tags"]);
                        SystemUpdate.ArticleDate = Convert.ToDateTime(dataReader["ArticleDate"]);
                        SystemUpdate.ViewCount = Convert.ToInt32(dataReader["ViewCount"]);
                        SystemUpdate.SectionId = Convert.ToInt32(dataReader["Section_id"]);
                        SystemUpdate.CustomURL = Convert.ToString(dataReader["CustomURL"]);

                        SystemUpdates.Add(SystemUpdate);
                    }
                }


                if (dataReader.NextResult())
                {
                    while (dataReader.Read())
                    {
                        Article BetterGuest = new Article();
                        BetterGuest.Id = Convert.ToInt32(dataReader["Id"]);
                        BetterGuest.Title = Convert.ToString(dataReader["Title"]);
                        BetterGuest.SubTitle = Convert.ToString(dataReader["SubTitle"]);
                        BetterGuest.Content = Convert.ToString(dataReader["Content"]);
                        BetterGuest.Tags = Convert.ToString(dataReader["Tags"]);
                        BetterGuest.ArticleDate = Convert.ToDateTime(dataReader["ArticleDate"]);
                        BetterGuest.ViewCount = Convert.ToInt32(dataReader["ViewCount"]);
                        BetterGuest.SectionId = Convert.ToInt32(dataReader["Section_id"]);
                        BetterGuest.CustomURL = Convert.ToString(dataReader["CustomURL"]);

                        BetterGuests.Add(BetterGuest);
                    }
                }               

                model.Announcements = Announcements;
                model.PlatformUpdates = PlatformUpdates;
                model.BecomeAPro = BecomeAPro;
                model.GettingStarted = GettingStarted;
                model.BetterGuests = BetterGuests;
                model.SystemUpdates = SystemUpdates;
            }
            return model;
        }

        public bool UpdateReservationV2CancellationReason(int ReservationId, int reasonId)
        {
            int retval = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@CancellationReasonId", reasonId));
            retval = ExecuteNonQuery("UpdateReservationV2CancellationReason", parameterList, CommandType.StoredProcedure);

            return retval > 0;
        }

        public List<CancellationReason> GetCancellationReasonsByEventId(int? eventid)
        {
            string sp = "GetCancellationReasonsByEventId";

            var Objreason = new List<CancellationReason>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", eventid));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var reason = new CancellationReason();
                        reason.id = Convert.ToInt32(dataReader["Id"]);
                        reason.reason = Convert.ToString(dataReader["Reason"]);
                        Objreason.Add(reason);
                    }
                }
            }
            return Objreason;
        }

        public bool EventsUpdate(EventsUpdateRequest eventsUpdateRequest)
        {
            var parameterList = new List<DbParameter>();
            bool isAvailable = false;

            parameterList.Add(GetParameter("@EventId", eventsUpdateRequest.EventId));
            parameterList.Add(GetParameter("@OldDepartmentId", eventsUpdateRequest.OldDepartmentId));
            parameterList.Add(GetParameter("@OldDurationMinutes", eventsUpdateRequest.OldDurationMinutes));
            parameterList.Add(GetParameter("@OldEndDate", eventsUpdateRequest.OldEndDate));
            parameterList.Add(GetParameter("@OldIgnoreHolidays", eventsUpdateRequest.OldIgnoreHolidays));
            parameterList.Add(GetParameter("@OldItemType", eventsUpdateRequest.OldItemType));
            parameterList.Add(GetParameter("@OldLimitByCapacity", eventsUpdateRequest.OldLimitByCapacity));
            parameterList.Add(GetParameter("@OldLimitByMaxOrders", eventsUpdateRequest.OldLimitByMaxOrders));
            parameterList.Add(GetParameter("@OldLocationId", eventsUpdateRequest.OldLocationId));
            parameterList.Add(GetParameter("@OldStartDate", eventsUpdateRequest.OldStartDate));
            parameterList.Add(GetParameter("@OldStatus", eventsUpdateRequest.OldStatus));
            parameterList.Add(GetParameter("@OldTaxClass", eventsUpdateRequest.OldTaxClass));

            using (DbDataReader dataReader = GetDataReader("EventsUpdate", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    isAvailable = true;
                }
            }
            return isAvailable;
        }


       public List<ReservationDataEvent> GetReservationsDataByFilters(string WhereClause)
        {
            List<ReservationDataEvent> listRsvpEvents = new List<ReservationDataEvent>();   
            ReservationDataEvent rEvent = new ReservationDataEvent();
            ReservationDataEventSchedule schedule = new ReservationDataEventSchedule();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@whereClause", WhereClause));

            string sql = "GetReservationsDataByFilter";

            int OldEventId = -1;
            int OldSlotId = -1;

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        int EventId = dataReader["EventId"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EventId"]);

                        //check if new event or san existing one
                        if (OldEventId == -1 || OldEventId != EventId || EventId == 0)
                        {
                            rEvent = new ReservationDataEvent();
                            rEvent.event_times = new List<ReservationDataEventSchedule>();

                            OldEventId = EventId;

                            rEvent.event_id = EventId;

                            if (OldEventId == 0)
                            {
                                rEvent.event_date = Convert.ToDateTime(dataReader["EventDate"]);
                                if (TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])) > TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])))
                                    rEvent.event_date_end = Convert.ToDateTime(dataReader["EventDate"]).AddDays(1);
                                else
                                    rEvent.event_date_end = Convert.ToDateTime(dataReader["EventDate"]);
                            }
                            else
                            {
                                EventModel eventModel = GetEventById(EventId);
                                rEvent.event_date = eventModel.StartDate;
                                rEvent.event_date_end = eventModel.EndDate;
                            }

                            rEvent.event_name = Convert.ToString(dataReader["EventName"]);
                            listRsvpEvents.Add(rEvent); //added event to the response
                        }

                        //check if new slot object needs to be added to the event above
                        int SlotId = dataReader["SlotId"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SlotId"]);

                        if (OldSlotId == -1 || OldSlotId != SlotId)
                        {
                            schedule = new ReservationDataEventSchedule();
                            schedule.reservations = new List<ReservationDataModel>();
                            OldSlotId = SlotId;                                 
                            rEvent.event_times.Add(schedule);
                        }


                        var model = new ReservationDataModel();
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);      
                        model.event_start_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])));
                        if (TimeSpan.Parse(Convert.ToString(dataReader["StartTime"])) > TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])))
                            model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).AddDays(1).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));
                        else
                            model.event_end_date = Convert.ToDateTime(dataReader["EventDate"]).Add(TimeSpan.Parse(Convert.ToString(dataReader["EndTime"])));
                      
                        var reservation_holder = new ReservationDataHolder();
                        reservation_holder.user_id = Convert.ToInt32(dataReader["UserId"]);                      
                        reservation_holder.first_name = Convert.ToString(dataReader["FirstName"]);
                        reservation_holder.last_name = Convert.ToString(dataReader["LastName"]);
                        reservation_holder.customer_type = Convert.ToInt32(dataReader["CustomerType"]);
                        reservation_holder.phone = Convert.ToString(dataReader["PhoneNumber"]);  
                        model.reservation_holder = reservation_holder;
                        string assignedTableIds = Convert.ToString(dataReader["AssignedTableIds"]);
                        if (assignedTableIds.Length > 0)
                        {
                            model.assign_table_ids = JsonConvert.DeserializeObject<List<int>>(assignedTableIds);
                        }
                        model.assigned_floor_plan_id = Convert.ToInt32(dataReader["AssignedFloorPlanId"]);
                        model.pre_assign_server_id = Convert.ToInt32(dataReader["PreAssign_Server_Id"]);
                        string PreAssign_Table_Id = Convert.ToString(dataReader["PreAssign_Table_Id"]);
                        if (PreAssign_Table_Id.Length > 0)
                        {
                            model.pre_assign_table_ids = JsonConvert.DeserializeObject<List<int>>(PreAssign_Table_Id);
                        }
                        model.assigned_server_id = Convert.ToInt32(dataReader["AssignedServerId"]);
                        var paymentStatus = GetReservationPaymentStatus(Convert.ToDecimal(dataReader["FeeDue"]), Convert.ToDecimal(dataReader["AmountPaid"]));
                        model.payment_status = (int)paymentStatus;
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.status = Convert.ToByte(dataReader["Status"]);
                        model.total_guests = Convert.ToInt16(dataReader["TotalGuests"]);
                        model.floor_plan_name = Convert.ToString(dataReader["FloorPlanName"]);
                        model.reservation_seated_status = Convert.ToInt32(dataReader["SeatedStatus"]);
                        ReservationModel waitlistdatetime = GetWaitlistStartEndTime(model.reservation_id);

                        if (waitlistdatetime != null)
                        {
                            if (waitlistdatetime.seating_end_time != null)
                            {
                                model.seating_end_time = waitlistdatetime.seating_end_time;
                            }
                            model.seating_start_time = waitlistdatetime.seating_start_time;
                        }
                        model.guest_note = Convert.ToString(dataReader["Note"]);
                        model.internal_note = Convert.ToString(dataReader["InternalNote"]);
                        model.concierge_note = Convert.ToString(dataReader["ConciergeNote"]);
                        model.country = Convert.ToString(dataReader["Country"]);

                        schedule.reservations.Add(model);

                    }
                }
            }

            return listRsvpEvents;
        }
		
        public List<RSVPReviewModel> GetRSVPReviews(int topRecords = 4, int regionId=0)
        {
            var rsvpReviewModelList = new List<RSVPReviewModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@TopCount", topRecords));
            parameterList.Add(GetParameter("@RegionId", regionId));
            using (DbDataReader dataReader = GetDataReader("GetTopRSVPReviews", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var rsvpReview = new RSVPReviewModel();
                        rsvpReview.id = Convert.ToInt32(dataReader["Id"]);
                        rsvpReview.active = Convert.ToBoolean(dataReader["Active"]);
                        rsvpReview.winery_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        rsvpReview.metric1 = Convert.ToInt32(dataReader["Metric1"]);
                        rsvpReview.metric2 = Convert.ToInt32(dataReader["Metric2"]);
                        rsvpReview.metric3 = Convert.ToInt32(dataReader["Metric3"]);
                        rsvpReview.metric4 = Convert.ToInt32(dataReader["Metric4"]);
                        rsvpReview.metric5 = Convert.ToInt32(dataReader["Metric5"]);
                        rsvpReview.metric6 = Convert.ToInt32(dataReader["Metric6"]);
                        rsvpReview.description = Convert.ToString(dataReader["Description"]);
                        rsvpReview.private_comment = Convert.ToString(dataReader["PrivateComment"]);
                        rsvpReview.date_of_review = Convert.ToDateTime(dataReader["DateOfReview"]);
                        rsvpReview.event_title = Convert.ToString(dataReader["EventName"]);
                        rsvpReview.user_first_name = Convert.ToString(dataReader["FirstName"]);
                        rsvpReview.user_last_name = Convert.ToString(dataReader["LastName"]);
                        rsvpReview.user_join_date = Convert.ToDateTime(dataReader["MemberSince"]);
                        rsvpReview.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        rsvpReview.recommend = Convert.ToInt32(dataReader["Recommend"]);
                        rsvpReview.review_value = Convert.ToInt32(dataReader["ReviewValue"]);
                        rsvpReviewModelList.Add(rsvpReview);
                    }
                }
            }
            return rsvpReviewModelList;
        }

        public List<ScoutPromotion> GetActiveCellarScoutPromotions(int offer_schema_grp, string city, string state)
        {
            var ScoutOffers = new List<ScoutPromotion>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@OfferSchemaGroupId", offer_schema_grp));
            parameterList.Add(GetParameter("@City", city));
            parameterList.Add(GetParameter("@State", state));
            using (DbDataReader dataReader = GetDataReader("GetActiveCellarScoutPromotions", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var offerData = new ScoutPromotion();
                        offerData.promo_id = Convert.ToInt32(dataReader["OfferId"]);
                        offerData.member_name = Convert.ToString(dataReader["DisplayName"]);
                        offerData.promo_name = Convert.ToString(dataReader["PromoName"]);
                        offerData.member_city = Convert.ToString(dataReader["City"]);
                        offerData.promo_used_count = Convert.ToInt32(dataReader["UserPromo"]);
                        offerData.member_banner_image = Convert.ToString(dataReader["MemberBannerImage"]);
                        offerData.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        offerData.favorite_count = Convert.ToInt32(dataReader["FavCount"]);
                        offerData.promotion_type = Convert.ToString(dataReader["GroupName"]);
                        int memberId = Convert.ToInt32(dataReader["MemberId"]);
                        offerData.member_id = memberId;
                        offerData.offer_name = Convert.ToString(dataReader["OfferName"]);
                        WineryReviews wineryReviews = GetWineryReviews(memberId);

                        offerData.total_reviews = wineryReviews.ReviewCount;
                        offerData.review_stars = wineryReviews.ReviewStars;
                        offerData.member_state = Convert.ToString(dataReader["State"]);
                        offerData.currency = Convert.ToString(dataReader["CurrencySymbol"]);
                        offerData.offer_price = Convert.ToString(dataReader["Price"]);
                        offerData.start_date = Convert.ToDateTime(dataReader["StartDate"]);
                        offerData.end_date = Convert.ToDateTime(dataReader["EndDate"]);

                        ScoutOffers.Add(offerData);
                    }
                }
            }
            return ScoutOffers;
        }

        public int GetActiveCellarScoutPromotionId(int memberId = 0)
        {
            string sp = "GetActiveCellarScoutPromotionByWinery";
            int PromoId = 0;
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@WineryId", memberId));

            try
            {
                PromoId = (int)ExecuteScalar(sp, parameterList);
            }
            catch {
                PromoId = 0;
            }
            return PromoId;
        }

        public MemberStatsModel GetMemberstats(int member_id)
        {
            var model = new MemberStatsModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", member_id));

            using (DbDataReader dataReader = GetDataReader("GetMemberstats", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        model.has_reviews = Convert.ToBoolean(dataReader["has_reviews"]);
                        model.has_public_rsvp_events = Convert.ToBoolean(dataReader["has_public_rsvp_events"]);
                        model.has_public_ticket_events = Convert.ToBoolean(dataReader["has_public_ticket_events"]);
                        model.has_trip_advisor_Id = Convert.ToBoolean(dataReader["has_trip_advisor_Id"]);
                        model.review_stars = Convert.ToInt32(dataReader["review_stars"]);
                        model.avg_review_value = Convert.ToDecimal(dataReader["avg_review_value"]);
                        model.total_reviews = Convert.ToInt32(dataReader["total_reviews"]);
                        model.has_food_menu = Convert.ToBoolean(dataReader["has_food_menu"]);
                        model.has_product_menu = Convert.ToBoolean(dataReader["has_product_menu"]);
                    }
                }
            }
            return model;
        }

        public List<MemberProductsModel> GetMemberProducts(int member_id)
        {
            string OldCollectionName = string.Empty;

            List<MemberProductsModel> list = new List<MemberProductsModel>();
            //List<ProductsModel> listProduct = new List<ProductsModel>();
            MemberProductsModel memberProductsModel = new MemberProductsModel();
            ProductsModel productsModel = new ProductsModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", member_id));

            using (DbDataReader dataReader = GetDataReader("GetMemberProducts", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        string CollectionName = Convert.ToString(dataReader["CollectionName"]);
                        if (string.IsNullOrEmpty(OldCollectionName) || OldCollectionName != CollectionName)
                        {
                            memberProductsModel = new MemberProductsModel();
                            memberProductsModel.products = new List<ProductsModel>();

                            OldCollectionName = CollectionName;

                            memberProductsModel.collection_name = CollectionName;
                            memberProductsModel.collection_id = Convert.ToInt32(dataReader["CollectionId"]);

                            list.Add(memberProductsModel);
                        }

                        productsModel = new ProductsModel();

                        productsModel.ProductId = Convert.ToInt32(dataReader["ProductId"]);

                        if (productsModel.ProductId > 0)
                        {
                            productsModel.custom_field_1 = Convert.ToString(dataReader["CustomField1"]);
                            productsModel.custom_field_2 = Convert.ToString(dataReader["CustomField2"]);
                            productsModel.description = Convert.ToString(dataReader["Description"]);
                            productsModel.ProductName = Convert.ToString(dataReader["ProductName"]);
                            productsModel.RetailPrice = Convert.ToDecimal(dataReader["RetailPrice"]);
                            productsModel.sku = Convert.ToString(dataReader["SKU"]);

                            memberProductsModel.products.Add(productsModel);
                        }
                    }
                }
            }
            return list;
        }

        public List<MemberFoodMenuModel> GetMemberFoodMenu(int member_id)
        {
            string OldGroupName = string.Empty;

            List<MemberFoodMenuModel> list = new List<MemberFoodMenuModel>();
            //List<ProductsModel> listProduct = new List<ProductsModel>();
            MemberFoodMenuModel memberProductsModel = new MemberFoodMenuModel();
            FoodMenuModel productsModel = new FoodMenuModel();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", member_id));

            using (DbDataReader dataReader = GetDataReader("GetMemberFoodMenu", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        string group_name = Convert.ToString(dataReader["GroupName"]);
                        if (string.IsNullOrEmpty(OldGroupName) || OldGroupName != group_name)
                        {
                            memberProductsModel = new MemberFoodMenuModel();
                            memberProductsModel.foodmenu = new List<FoodMenuModel>();

                            OldGroupName = group_name;

                            memberProductsModel.group_name = group_name;
                            memberProductsModel.group_id = Convert.ToInt32(dataReader["AddOn_Group_Id"]);
                            memberProductsModel.group_type = Convert.ToInt32(dataReader["GroupType"]);

                            list.Add(memberProductsModel);
                        }

                        productsModel = new FoodMenuModel();

                        productsModel.item_id = Convert.ToInt32(dataReader["ItemId"]);

                        if (productsModel.item_id > 0)
                        {
                            productsModel.calculate_gratuity = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                            productsModel.department_id = Convert.ToInt32(dataReader["DepartmentId"]);
                            productsModel.description = Convert.ToString(dataReader["Description"]);
                            productsModel.item_name = Convert.ToString(dataReader["ItemName"]);
                            productsModel.price = Convert.ToDecimal(dataReader["Price"]);
                            productsModel.sku = Convert.ToString(dataReader["Sku"]);
                            productsModel.taxable = Convert.ToBoolean(dataReader["Taxable"]);

                            memberProductsModel.foodmenu.Add(productsModel);
                        }
                    }
                }
            }
            return list;
        }

        public PriceRangeByRegionIdModel GetPriceRangeByRegionId(DateTime StartDate,int Guest,int RegionId)
        {
            var model = new PriceRangeByRegionIdModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@Guest", Guest));
            parameterList.Add(GetParameter("@RegionId", RegionId));

            using (DbDataReader dataReader = GetDataReader("GetPriceRangeByRegionId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        model.max_price = Convert.ToDecimal(dataReader["MaxPrice"]);
                        model.min_price = Convert.ToDecimal(dataReader["MinPrice"]);
                    }
                }
            }
            return model;
        }

        public List<MembersByPriceRangeModel> GetMembersByPriceRange(DateTime start_date, int guest, int region_id, decimal min_price, decimal max_price)
        {
            var list = new List<MembersByPriceRangeModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@StartDate", start_date));
            parameterList.Add(GetParameter("@Guest", guest));
            parameterList.Add(GetParameter("@RegionId", region_id));
            parameterList.Add(GetParameter("@MinPrice", min_price));
            parameterList.Add(GetParameter("@MaxPrice", max_price));

            using (DbDataReader dataReader = GetDataReader("GetMembersByPriceRange", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        var model = new MembersByPriceRangeModel();
                        model.geo_latitude = Convert.ToString(dataReader["geo_latitude"]);
                        model.geo_longitude = Convert.ToString(dataReader["geo_longitude"]);
                        model.image = Convert.ToString(dataReader["image"]);
                        model.max_price = Convert.ToDecimal(dataReader["max_price"]);
                        model.member_name = Convert.ToString(dataReader["member_name"]);
                        model.min_price = Convert.ToDecimal(dataReader["min_price"]);
                        model.region_name = Convert.ToString(dataReader["region_name"]);
                        model.star = Convert.ToInt32(dataReader["star"]);
                        model.total_records = Convert.ToInt32(dataReader["total_records"]);
                        model.url = Convert.ToString(dataReader["url"]);
                        model.state = Convert.ToString(dataReader["State"]);
                        model.city = Convert.ToString(dataReader["City"]);
                        model.member_id = Convert.ToInt32(dataReader["MemberId"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public bool SaveReservationInvite(CreateReservationInvite req)
        {
            var parameterList = new List<DbParameter>();
            bool ret = false;

            parameterList.Add(GetParameter("@ReservationId", req.reservation_id));
            parameterList.Add(GetParameter("@UserId", req.user_id));
            parameterList.Add(GetParameter("@ExpirationDateTime", req.expiration_date_time));

            using (DbDataReader dataReader = GetDataReader("SaveReservationInvite", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    ret = true;
                }
            }
            return ret;
        }

        public bool SetReservationInviteStatus(SetReservationInviteStatusRequest req)
        {
            var parameterList = new List<DbParameter>();
            bool ret = false;

            parameterList.Add(GetParameter("@ReservationId", req.reservation_id));
            parameterList.Add(GetParameter("@ReservationInviteGUID", req.reservation_invite_guid));
            parameterList.Add(GetParameter("@Status", req.status));

            using (DbDataReader dataReader = GetDataReader("SetReservationInviteStatus", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    ret = true;
                }
            }
            return ret;
        }

        public bool SetReservationInviteReminderStatus(int ReservationId=0,string ReservationInviteGUID="")
        {
            var parameterList = new List<DbParameter>();
            bool ret = false;

            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@ReservationInviteGUID", ReservationInviteGUID));

            using (DbDataReader dataReader = GetDataReader("SetReservationInviteReminderStatus", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    ret = true;
                }
            }
            return ret;
        }

        public ReservationInviteModel GetReservationInviteByRsvpId(int ReservationId = 0, string ReservationInviteGUID = "")
        {
            var parameterList = new List<DbParameter>();
            ReservationInviteModel model = new ReservationInviteModel();

            parameterList.Add(GetParameter("@ReservationId", ReservationId));
            parameterList.Add(GetParameter("@ReservationInviteGUID", ReservationInviteGUID));

            using (DbDataReader dataReader = GetDataReader("GetReservationInviteByRsvpId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        model.expiration_date_time = Convert.ToDateTime(dataReader["ExpirationdateTime"]);
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.invite_date_time = Convert.ToDateTime(dataReader["InviteDateTime"]);
                        model.reminder_sent = Convert.ToBoolean(dataReader["Reminder_sent"]);
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationID"]);
                        model.reservation_invite_guid = Convert.ToString(dataReader["ReservationInviteGUID"]);
                        model.status = (ReservationInviteStatus)Convert.ToInt32(dataReader["Status"]);
                        model.update_date_time = Convert.ToDateTime(dataReader["UpdateDateTime"]);
                        model.user_id = Convert.ToInt32(dataReader["UserId"]);

                        model.event_date = Convert.ToDateTime(dataReader["eventdate"]);
                        model.start_time = string.Format("{0:hh:mm tt}", Convert.ToDateTime(dataReader["starttime"]));
                        model.end_time = string.Format("{0:hh:mm tt}", Convert.ToDateTime(dataReader["EndTime"]));
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.member_id = Convert.ToInt32(dataReader["WineryId"]);

                        SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                        List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(model.member_id, (int)Common.Common.SettingGroup.member);

                        model.member_phone = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.member_rsvp_contact_phone);

                        if (model.member_phone.Length == 0)
                            model.member_phone = Convert.ToString(dataReader["MemberPhone"]);
                    }
                }
            }
            return model;
        }



        public List<int> GetReservationIdForInviteReminder()
        {
            List<int> ids = new List<int>();
            using (DbDataReader dataReader = GetDataReader("GetReservationIdForInviteReminder", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    try
                    {
                        while (dataReader.Read())
                        {
                            int id = Convert.ToInt32(dataReader["ReservationID"]);
                            ids.Add(id);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                    }

                }
            }
            return ids;
        }

        public bool UpdateReservationTags(int ReservationID, string tags)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ReservationID", ReservationID));
            parameterList.Add(GetParameter("@tags", tags));

            string sqlQuery = "update ReservationV2 set tags =@tags where ReservationId=@ReservationID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public void DeleteAllUserTags(int user_id, int member_id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@user_id", user_id));
            parameterList.Add(GetParameter("@member_id", member_id));

            string sql = "DELETE FROM User_Guest_Tags WHERE UserId = @user_id and MemberId = @member_id";

            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }

        public void SaveUserTags(int user_id, int member_id, int tag_id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@user_id", user_id));
            parameterList.Add(GetParameter("@member_id", member_id));
            parameterList.Add(GetParameter("@tag_id", tag_id));

            string sql = "INSERT INTO [dbo].[User_Guest_Tags] ([UserId],[MemberId],[TagId]) ";
            sql += " VALUES ";
            sql += "(@user_id,@member_id,@tag_id)";
            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }

        public bool DeleteEventAddOnsAccountTypesById(int AddOnGroupId, int AddOnGroupItemsId, int ThirdPartyAccountTypesId)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@AddOnGroupId", AddOnGroupId));
            parameterList.Add(GetParameter("@AddOnGroupItemsId", AddOnGroupItemsId));
            parameterList.Add(GetParameter("@ThirdPartyAccountTypesId", ThirdPartyAccountTypesId));

            ExecuteScalar("DeleteEventAddOnsAccountTypesById", parameterList);
            return true;
        }

        public bool InsertEventAddOnsAccountTypes(int AddOn_Group_Id, int AddOn_Group_Items_Id, int ThirdParty_AccountTypes_Id, int MemberBenefit, bool MemberBenefitReq, decimal MemberBenefitCustomValue)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@AddOn_Group_Id", AddOn_Group_Id));
            parameterList.Add(GetParameter("@AddOn_Group_Items_Id", AddOn_Group_Items_Id));
            parameterList.Add(GetParameter("@ThirdParty_AccountTypes_Id", ThirdParty_AccountTypes_Id));
            parameterList.Add(GetParameter("@MemberBenefit", MemberBenefit));
            parameterList.Add(GetParameter("@MemberBenefitReq", MemberBenefitReq));
            parameterList.Add(GetParameter("@MemberBenefitCustomValue", MemberBenefitCustomValue));

            int retvalue = ExecuteNonQuery("InsertEventAddOnsAccountTypes", parameterList, CommandType.StoredProcedure);

            return retvalue > 0;
        }

        public List<Event_AddOns_AccountTypes> GetEventAddOnsAccountTypesId(int AddOnGroupId, int AddOnGroupItemsId)
        {
            var list = new List<Event_AddOns_AccountTypes>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@AddOnGroupId", AddOnGroupId));
            parameterList.Add(GetParameter("@AddOnGroupItemsId", AddOnGroupItemsId));

            using (DbDataReader dataReader = GetDataReader("GetEventAddOnsAccountTypesId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        var model = new Event_AddOns_AccountTypes();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.contact_type_id = Convert.ToString(dataReader["ContactTypeId"]);
                        model.contact_type = Convert.ToString(dataReader["ContactType"]);
                        model.member_benefit = Convert.ToInt32(dataReader["MemberBenefit"]);
                        model.member_benefit_req = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                        model.member_benefit_custom_value = Convert.ToDecimal(dataReader["MemberBenefitCustomValue"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<GetReservationV2ByWineryId_DetailResult> GetReservationV2ByWineryId_Detail(int WineryId, DateTime StartDate)
        {
            var list = new List<GetReservationV2ByWineryId_DetailResult>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@StartDate", StartDate));

            using (DbDataReader dataReader = GetDataReader("GetReservationV2ByWineryId_Detail", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        var model = new GetReservationV2ByWineryId_DetailResult();
                        model.GuestName = Convert.ToString(dataReader["GuestName"]);
                        model.PhoneNum = Convert.ToString(dataReader["PhoneNum"]);
                        model.TotalGuests = Convert.ToInt32(dataReader["TotalGuests"]);
                        model.FeePerPerson = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.BalanceDue = Convert.ToDecimal(dataReader["BalanceDue"]);
                        model.AccountType = Convert.ToString(dataReader["AccountType"]);
                        model.ContactTypes = Convert.ToString(dataReader["ContactTypes"]);
                        model.ReferredBy = Convert.ToString(dataReader["ReferredBy"]);
                        model.InternalNote = Convert.ToString(dataReader["InternalNote"]);
                        model.GuestNote = Convert.ToString(dataReader["GuestNote"]);
                        model.MaxPersons = Convert.ToInt32(dataReader["MaxPersons"]);
                        model.EventName = Convert.ToString(dataReader["EventName"]);
                        model.EventTime = Convert.ToString(dataReader["EventTime"]);
                        model.Guests = Convert.ToInt32(dataReader["Guests"]);
                        model.sortorder = Convert.ToInt32(dataReader["sortorder"]);
                        model.start = Convert.ToDateTime(dataReader["start"]);
                        model.LocationName = Convert.ToString(dataReader["LocationName"]);
                        model.Email = Convert.ToString(dataReader["Email"]);
                        model.FeeDue = Convert.ToDecimal(dataReader["FeeDue"]);
                        
                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<GetReservationV2ByWineryIdResult> GetReservationV2ByWineryId(int WineryId, DateTime StartDate)
        {
            var list = new List<GetReservationV2ByWineryIdResult>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@StartDate", StartDate));

            using (DbDataReader dataReader = GetDataReader("GetReservationV2ByWineryId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        var model = new GetReservationV2ByWineryIdResult();

                        model.LastName = Convert.ToString(dataReader["LastName"]);
                        model.FirstName = Convert.ToString(dataReader["FirstName"]);
                        model.PhoneNum = Convert.ToString(dataReader["PhoneNum"]);
                        model.TotalGuests = Convert.ToInt32(dataReader["TotalGuests"]);
                        model.FeePerPerson = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        model.Discount = Convert.ToDecimal(dataReader["Discount"]);
                        model.PurchaseTotal = Convert.ToDecimal(dataReader["PurchaseTotal"]);
                        model.AccountType = Convert.ToString(dataReader["AccountType"]);
                        model.BookingDate = Convert.ToString(dataReader["BookingDate"]);
                        model.ReferredBy = Convert.ToString(dataReader["ReferredBy"]);
                        model.InternalNote = Convert.ToString(dataReader["InternalNote"]);
                        model.MaxPersons = Convert.ToInt32(dataReader["MaxPersons"]);
                        model.EventName = Convert.ToString(dataReader["EventName"]);
                        model.EventTime = Convert.ToString(dataReader["EventTime"]);
                        model.Guests = Convert.ToInt32(dataReader["Guests"]);
                        model.sortorder = Convert.ToInt32(dataReader["sortorder"]);
                        model.start = Convert.ToDateTime(dataReader["start"]);
                        model.HDYH = Convert.ToString(dataReader["HDYH"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<GetFinancialV2ReportByMonthResult> GetFinancialV2ReportByMonth(int WineryId, DateTime StartDate,DateTime EndDate,int offsetMinutes)
        {
            var list = new List<GetFinancialV2ReportByMonthResult>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));
            parameterList.Add(GetParameter("@offsetMinutes", offsetMinutes));

            using (DbDataReader dataReader = GetDataReader("GetFinancialV2ReportByMonth", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        var model = new GetFinancialV2ReportByMonthResult();

                        model.PaymentDate = Convert.ToDateTime(dataReader["PaymentDate"]);
                        model.DateBooked = Convert.ToDateTime(dataReader["DateBooked"]);
                        model.EventDate = Convert.ToDateTime(dataReader["EventDate"]);
                        model.ConfirmNum = Convert.ToString(dataReader["ConfirmNum"]);
                        model.UserName = Convert.ToString(dataReader["UserName"]);
                        model.Eventamt = Convert.ToDecimal(dataReader["Eventamt"]);
                        model.Paidamt = Convert.ToDecimal(dataReader["Paidamt"]);
                        model.BalanceDue = Convert.ToDecimal(dataReader["BalanceDue"]);
                        model.TXNDate = Convert.ToDateTime(dataReader["TXNDate"]);
                        model.TXNType = Convert.ToString(dataReader["TXNType"]);
                        model.PayCardType = Convert.ToString(dataReader["PayCardType"]);
                        model.PayCardNumber = Convert.ToString(dataReader["PayCardNumber"]);
                        model.Approval = Convert.ToString(dataReader["Approval"]);
                        model.TransactionID = Convert.ToString(dataReader["TransactionID"]);
                        model.ExportType = Convert.ToString(dataReader["ExportType"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<GetTransactionReportByMonthResult> GetTransactionReportByMonth(int WineryId, DateTime StartDate, DateTime EndDate, int offsetMinutes)
        {
            var list = new List<GetTransactionReportByMonthResult>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryId));
            parameterList.Add(GetParameter("@StartDate", StartDate));
            parameterList.Add(GetParameter("@EndDate", EndDate));
            parameterList.Add(GetParameter("@offsetMinutes", offsetMinutes));

            using (DbDataReader dataReader = GetDataReader("GetTransactionReportByMonth", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        var model = new GetTransactionReportByMonthResult();

                        model.Id = Convert.ToInt32(dataReader["Id"]);
                        model.ReservationId = Convert.ToInt32(dataReader["ReservationId"]);
                        model.BookingCode = Convert.ToString(dataReader["BookingCode"]);
                        model.EventDate = Convert.ToDateTime(dataReader["EventDate"]);
                        model.StartTime = TimeSpan.Parse(Convert.ToString(dataReader["StartTime"]));
                        model.EndTime = TimeSpan.Parse(Convert.ToString(dataReader["EndTime"]));
                        model.SlotId = Convert.ToInt32(dataReader["SlotId"]);
                        model.SlotType = Convert.ToInt32(dataReader["SlotType"]);
                        model.UserId = Convert.ToInt32(dataReader["UserId"]);
                        model.FirstName = Convert.ToString(dataReader["FirstName"]);
                        model.LastName = Convert.ToString(dataReader["LastName"]);
                        model.UserName = Convert.ToString(dataReader["UserName"]);
                        model.Email = Convert.ToString(dataReader["Email"]);
                        model.ProductType = Convert.ToString(dataReader["ProductType"]);
                        model.SKU = Convert.ToString(dataReader["SKU"]);
                        model.ProductName = Convert.ToString(dataReader["ProductName"]);
                        model.Qty = Convert.ToInt32(dataReader["Qty"]);
                        model.Price = Convert.ToDecimal(dataReader["Price"]);
                        model.TransactionDate = Convert.ToDateTime(dataReader["TransactionDate"]);
                        model.SalesTax = Convert.ToDecimal(dataReader["SalesTax"]);
                        model.SalesTaxPercentage = Convert.ToDecimal(dataReader["SalesTaxPercentage"]);
                        model.GratuityAmount = Convert.ToDecimal(dataReader["GratuityAmount"]);
                        model.PaymentId = Convert.ToInt32(dataReader["PaymentId"]);
                        model.TransID = Convert.ToString(dataReader["TransID"]);
                        model.TenderType = Convert.ToString(dataReader["TenderType"]);
                        model.Extended = Convert.ToDecimal(dataReader["Extended"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public DataView ReservationV2ByDate_Exp(int WineryId, DateTime StartDate)
        {
            DataTable dt = new DataTable("EventDetails");
            dt.Columns.Add("EventId", Type.GetType("System.String"));
            dt.Columns.Add("EventName", Type.GetType("System.String"));
            dt.Columns.Add("StartTime", Type.GetType("System.String"));
            dt.Columns.Add("Start", Type.GetType("System.DateTime"));
            dt.Columns.Add("GetTotalGuest", Type.GetType("System.String"));
            dt.Columns.Add("MaxPersons", Type.GetType("System.String"));
            dt.Columns.Add("LastName", Type.GetType("System.String"));
            dt.Columns.Add("GuestName", Type.GetType("System.String"));
            dt.Columns.Add("FirstName", Type.GetType("System.String"));
            dt.Columns.Add("TotalGuests", Type.GetType("System.String"));
            dt.Columns.Add("InternalNote", Type.GetType("System.String"));
            dt.Columns.Add("GuestNote", Type.GetType("System.String"));
            dt.Columns.Add("sortorder", Type.GetType("System.Int32"));
            dt.Columns.Add("PhoneNumber", Type.GetType("System.String"));
            dt.Columns.Add("AddOnDetails", Type.GetType("System.String"));
            dt.Columns.Add("LocationName", Type.GetType("System.String"));

            string EventId = "";
            int sortorder = 0;

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@StartDate", StartDate));

            using (DbDataReader dataReader = GetDataReader("GetReservationV2ByDate", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        DataRow dr = dt.NewRow();

                        dr["EventId"] = Convert.ToString(dataReader["EventId"]);
                        dr["EventName"] = Convert.ToString(dataReader["EventName"]);
                        dr["StartTime"] = Convert.ToDateTime(dataReader["Start"]).ToString("hh:mm tt");
                        dr["Start"] = Convert.ToDateTime(dataReader["Start"]);
                        dr["GetTotalGuest"] = Convert.ToString(dataReader["Get_TotalGuest"]);
                        dr["MaxPersons"] = Convert.ToString(dataReader["MaxPersons"]);
                        dr["GuestName"] = Convert.ToString(dataReader["LastName"]) + ", " + Convert.ToString(dataReader["FirstName"]);
                        dr["LastName"] = Convert.ToString(dataReader["LastName"]);
                        dr["FirstName"] = Convert.ToString(dataReader["FirstName"]);
                        dr["TotalGuests"] = Convert.ToString(dataReader["TotalGuests"]);
                        dr["InternalNote"] = Convert.ToString(dataReader["InternalNote"]);
                        dr["GuestNote"] = Convert.ToString(dataReader["Note"]);
                        dr["PhoneNumber"] = Convert.ToString(dataReader["PhoneNumber"]);
                        dr["AddOnDetails"] = Convert.ToString(dataReader["AddOnDetails"]);
                        dr["LocationName"] = Convert.ToString(dataReader["locationname"]);
                        sortorder = sortorder + 1;
                        dr["sortorder"] = sortorder;

                        dt.Rows.Add(dr);

                        EventId += "," + Convert.ToString(dataReader["EventId"]) + ",";
                    }
                }
            }

            DataView dv = dt.DefaultView;
            dv.Sort = "sortorder";

            return dv;
        }

        public DataView EventBookingsByDate_Exp(int WineryId, DateTime StartDate)
        {
            DataTable dt = new DataTable("EventDetails");
            dt.Columns.Add("EventId", Type.GetType("System.String"));
            dt.Columns.Add("EventName", Type.GetType("System.String"));
            dt.Columns.Add("StartTime", Type.GetType("System.String"));
            dt.Columns.Add("Start", Type.GetType("System.DateTime"));
            dt.Columns.Add("GetTotalGuest", Type.GetType("System.String"));
            dt.Columns.Add("MaxPersons", Type.GetType("System.String"));
            dt.Columns.Add("LastName", Type.GetType("System.String"));
            dt.Columns.Add("GuestName", Type.GetType("System.String"));
            dt.Columns.Add("FirstName", Type.GetType("System.String"));
            dt.Columns.Add("TotalGuests", Type.GetType("System.String"));
            dt.Columns.Add("HomePhone", Type.GetType("System.String"));
            dt.Columns.Add("EventAmt", Type.GetType("System.String"));
            dt.Columns.Add("PaidAmt", Type.GetType("System.String"));
            dt.Columns.Add("GuestNote", Type.GetType("System.String"));
            dt.Columns.Add("sortorder", Type.GetType("System.Int32"));
            dt.Columns.Add("LocationName", Type.GetType("System.String"));

            string EventId = "";
            int sortorder = 0;

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", WineryId));
            parameterList.Add(GetParameter("@StartDate", StartDate));

            using (DbDataReader dataReader = GetDataReader("GetEventBookingsV2ByDate", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        if (EventId.IndexOf(Convert.ToString(dataReader["EventId"]) + ",") < 0 || Convert.ToString(dataReader["EventId"]) == "0")
                        {
                            sortorder = 0;
                        }
                        DataRow dr = dt.NewRow();
                        dr["EventId"] = Convert.ToString(dataReader["EventId"]);
                        dr["EventName"] = Convert.ToString(dataReader["EventName"]);
                        dr["StartTime"] = Convert.ToDateTime(dataReader["Start"]).ToString("hh:mm tt");
                        dr["Start"] = Convert.ToString(dataReader["Start"]);
                        dr["GetTotalGuest"] = Convert.ToString(dataReader["Get_TotalGuest"]);
                        dr["MaxPersons"] = Convert.ToString(dataReader["MaxPersons"]);
                        dr["GuestName"] = Convert.ToString(dataReader["LastName"]) + ", " + Convert.ToString(dataReader["FirstName"]);
                        dr["LastName"] = Convert.ToString(dataReader["LastName"]);
                        dr["FirstName"] = Convert.ToString(dataReader["FirstName"]);
                        dr["TotalGuests"] = Convert.ToString(dataReader["TotalGuests"]);
                        dr["HomePhone"] = Convert.ToString(dataReader["HomePhone"]);

                        if (Convert.ToBoolean(dataReader["WaiveFee"]))
                        {
                            dr["EventAmt"] = "Waived";
                            dr["PaidAmt"] = "N/A";
                        }
                        else
                        {
                            dr["EventAmt"] = "$ " + Convert.ToString(dataReader["EventAmt"]);
                            dr["PaidAmt"] = "$ " + Convert.ToString(dataReader["PaidAmt"]);
                        }

                        dr["GuestNote"] = Convert.ToString(dataReader["GuestNote"]);
                        dr["LocationName"] = "";
                        sortorder = sortorder + 1;
                        dr["sortorder"] = sortorder;

                        if (Convert.ToString(dataReader["LastName"]) != "" & Convert.ToString(dataReader["FirstName"]) != "")
                            dt.Rows.Add(dr);

                        int rowNo = Convert.ToInt32(dataReader["rowNo"]);

                        for (int index = 1; index <= Convert.ToInt32(dataReader["MaxPersons"]) - Convert.ToInt32(dataReader["Get_TotalGuest"]); index++)
                        {
                            DataRow dr1 = dt.NewRow();

                            if (EventId.IndexOf(Convert.ToString(dataReader["EventId"]) + ",") > -1)
                            {
                                break;
                            }

                            dr1["EventId"] = Convert.ToString(dataReader["EventId"]);
                            dr1["EventName"] = Convert.ToString(dataReader["EventName"]);
                            dr1["StartTime"] = Convert.ToDateTime(dataReader["Start"]).ToString("hh:mm tt");
                            dr1["Start"] = Convert.ToDateTime(dataReader["Start"]);
                            dr1["GetTotalGuest"] = Convert.ToString(dataReader["Get_TotalGuest"]);
                            dr1["MaxPersons"] = Convert.ToString(dataReader["MaxPersons"]);
                            dr1["GuestName"] = "";
                            dr1["LastName"] = "";
                            dr1["FirstName"] = "";
                            dr1["TotalGuests"] = "";
                            dr1["HomePhone"] = "";
                            dr1["EventAmt"] = "";
                            dr1["PaidAmt"] = "";
                            dr1["GuestNote"] = "";
                            dr["LocationName"] = "";
                            rowNo = rowNo + 1;
                            dr1["sortorder"] = rowNo;
                            dt.Rows.Add(dr1);
                        }
                        EventId += Convert.ToString(dataReader["EventId"]) + ", ";
                    }
                }
            }

            DataView dv = dt.DefaultView;
            dv.Sort = "sortorder";

            return dv;
        }

        public bool SaveReservationReview(ReservationReviewRequest request)
        {
            string sqlQuery = "InsertReservationReview";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@BookingGUID", request.booking_guid));
            parameterList.Add(GetParameter("@Winery_Id", request.member_id));
            parameterList.Add(GetParameter("@Metric1", request.rating));
            parameterList.Add(GetParameter("@Metric2", request.first_impression_rating));
            parameterList.Add(GetParameter("@Metric3", request.ambiance_rating));
            parameterList.Add(GetParameter("@Metric4", request.hospitality_rating));
            parameterList.Add(GetParameter("@RecommendTo", request.recommend_to));
            parameterList.Add(GetParameter("@AnyPurchases", request.any_purchases));
            parameterList.Add(GetParameter("@PurchaseAmountRange", request.purchase_amount_range));
            parameterList.Add(GetParameter("@MembershipSignup", request.membership_signup));
            parameterList.Add(GetParameter("@Description", request.review));
            parameterList.Add(GetParameter("@PrivateComment", request.private_comment));
            parameterList.Add(GetParameter("@ReviewValue", request.review_value));
            int ret = ExecuteNonQuery(sqlQuery, parameterList, CommandType.StoredProcedure);

            return (ret > 0);
        }

        public WaitlistTPResponse GetWaitlistDetailById(int waitlistId)
        {
            WaitlistTPResponse data = null;

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WaitlistId", waitlistId));

            using (DbDataReader dataReader = GetDataReader("GetWaitlistById", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        data = new WaitlistTPResponse();
                        data.id = Convert.ToInt32(dataReader["Id"]);
                        data.status = (Common.Common.Waitlist_Status)Convert.ToInt32(dataReader["WaitlistStatus"]);
                        data.member_id = Convert.ToInt32(dataReader["Member_Id"]);
                        data.location_id = Convert.ToInt32(dataReader["Location_Id"]);
                        data.first_name = Convert.ToString(dataReader["FirstName"]);
                        data.last_name = Convert.ToString(dataReader["LastName"]);
                        data.duration_in_minutes = Convert.ToInt32(dataReader["DurationMinutes"]);
                        data.email = Convert.ToString(dataReader["Email"]);
                        data.mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        data.mobile_phone_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        data.wait_start_time = Convert.ToDateTime(dataReader["WaitStartDateTime"]);
                        data.party_size = Convert.ToInt32(dataReader["PartySize"]);
                        data.wait_time_minutes = Convert.ToInt32(dataReader["WaitTimeMinutes"]);
                        data.guest_notes = Convert.ToString(dataReader["GuestNotes"]);
                        data.waitlist_notes = Convert.ToString(dataReader["WaitlistNotes"]);
                        data.tags = Convert.ToString(dataReader["Tags"]);
                        data.visits_count = Convert.ToInt32(dataReader["VisitsCount"]);
                        data.cancellations_count = Convert.ToInt32(dataReader["CancelledCount"]);
                        data.no_shows_count = Convert.ToInt32(dataReader["NoShowCount"]);
                        data.waitlist_index = Convert.ToInt32(dataReader["Waitlist_Index"]);
                        if (dataReader["SeatingStartDateTime"] != null && dataReader["SeatingStartDateTime"] != DBNull.Value)
                            data.seating_start_time = Convert.ToDateTime(dataReader["SeatingStartDateTime"]);
                        if (dataReader["SeatingEndDateTime"] != null && dataReader["SeatingEndDateTime"] != DBNull.Value)
                            data.seating_end_time = Convert.ToDateTime(dataReader["SeatingEndDateTime"]);
                        data.is_walk_in = Convert.ToBoolean(dataReader["IsWalkIn"]);
                        if (!string.IsNullOrWhiteSpace(dataReader["PreAssign_Table_Id"].ToString()))
                        {
                            data.pre_assign_table_ids = JsonConvert.DeserializeObject<List<int>>(dataReader["PreAssign_Table_Id"].ToString());
                        }
                        if (!string.IsNullOrWhiteSpace(dataReader["AssignedTableIds"].ToString()))
                        {
                            data.assign_table_ids = JsonConvert.DeserializeObject<List<int>>(dataReader["AssignedTableIds"].ToString());
                        }

                        if (!string.IsNullOrWhiteSpace(dataReader["UserNotes"].ToString()))
                        {
                            data.account_note = JsonConvert.DeserializeObject<UserNote>(dataReader["UserNotes"].ToString());
                        }
                        data.pre_assign_server_id = Convert.ToInt32(dataReader["PreAssign_Server_Id"]);
                        data.pre_assigned_server_first_name = Convert.ToString(dataReader["PreAssignServerFirstName"]);
                        data.pre_assigned_server_last_name = Convert.ToString(dataReader["PreassignServerLastName"]);
                        data.pre_assigned_server_color = Convert.ToString(dataReader["PreAssignServerColor"]);

                        data.floor_plan_id = Convert.ToInt32(dataReader["PreAssignedFloorplanId"]);
                        data.floor_plan_name = Convert.ToString(dataReader["PreAssignedFloorPlanName"]);
                        data.floor_plan_technical_name = Convert.ToString(dataReader["PreAssignedFloorPlanTechName"]);


                        data.assigned_server_id = Convert.ToInt32(dataReader["AssignedServerId"]);
                        data.assigned_server_first_name = Convert.ToString(dataReader["AssignedServerFirstName"]);
                        data.assigned_server_last_name = Convert.ToString(dataReader["AssignedServerLastName"]);
                        data.assigned_server_color = Convert.ToString(dataReader["AssignedSererColor"]);


                        data.assigned_floor_plan_id = Convert.ToInt32(dataReader["AssignedFloorPlanId"]);
                        data.assigned_floor_plan_name = Convert.ToString(dataReader["AssignedFloorPlanName"]);
                        data.assigned_floor_plan_technical_name = Convert.ToString(dataReader["AssignedFloorPlanTechName"]);

                        data.user_id = Convert.ToInt32(dataReader["user_id"]);

                        data.customer_type = (Common.Common.CustomerType)Convert.ToInt32(dataReader["CustomerType"]);
                        data.event_id = Convert.ToInt32(dataReader["EventId"]);
                        data.event_name = Convert.ToString(dataReader["EventName"]);
                        data.referred_by_id = dataReader["ReferredById"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReferredById"]);
                        data.referred_by_first_name = dataReader["ReferredByFirstName"] == DBNull.Value ? "" : Convert.ToString(dataReader["ReferredByFirstName"]);
                        data.referred_by_last_name = dataReader["ReferredByLastName"] == DBNull.Value ? "" : Convert.ToString(dataReader["ReferredByLastName"]);
                    }
                }
            }

            return data;
        }

        public List<WaitlistTPResponse> GetWaitlistByMember(int memberId, DateTime? waitlistStart, DateTime? waitlistEnd, string locationIds)
        {
            List<WaitlistTPResponse> lstWaitlist = new List<WaitlistTPResponse>();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@MemberId", memberId));
            parameterList.Add(GetParameter("@LocationIds", locationIds));
            if (waitlistStart.HasValue && waitlistEnd.HasValue)
            {
                parameterList.Add(GetParameter("@StartDate", waitlistStart.Value));
                parameterList.Add(GetParameter("@EndDate", waitlistEnd.Value));
            }

            using (DbDataReader dataReader = GetDataReader("GetWaitlistForMember", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var data = new WaitlistTPResponse();
                        data.id = Convert.ToInt32(dataReader["Id"]);
                        data.status = (Common.Common.Waitlist_Status)Convert.ToInt32(dataReader["WaitlistStatus"]);
                        data.member_id = Convert.ToInt32(dataReader["Member_Id"]);
                        data.location_id = Convert.ToInt32(dataReader["Location_Id"]);
                        data.first_name = Convert.ToString(dataReader["FirstName"]);
                        data.last_name = Convert.ToString(dataReader["LastName"]);
                        data.duration_in_minutes = Convert.ToInt32(dataReader["DurationMinutes"]);
                        data.email = Convert.ToString(dataReader["Email"]);
                        data.mobile_phone = Convert.ToString(dataReader["MobilePhone"]);
                        data.mobile_phone_status = Convert.ToInt32(dataReader["MobilePhoneStatus"]);
                        data.wait_start_time = Convert.ToDateTime(dataReader["WaitStartDateTime"]);
                        data.party_size = Convert.ToInt32(dataReader["PartySize"]);
                        data.wait_time_minutes = Convert.ToInt32(dataReader["WaitTimeMinutes"]);
                        data.guest_notes = Convert.ToString(dataReader["GuestNotes"]);
                        data.waitlist_notes = Convert.ToString(dataReader["WaitlistNotes"]);
                        data.tags = Convert.ToString(dataReader["Tags"]);
                        data.visits_count = Convert.ToInt32(dataReader["VisitsCount"]);
                        data.cancellations_count = Convert.ToInt32(dataReader["CancelledCount"]);
                        data.no_shows_count = Convert.ToInt32(dataReader["NoShowCount"]);
                        data.waitlist_index = Convert.ToInt32(dataReader["Waitlist_Index"]);
                        if (dataReader["SeatingStartDateTime"] != null && dataReader["SeatingStartDateTime"] != DBNull.Value)
                            data.seating_start_time = Convert.ToDateTime(dataReader["SeatingStartDateTime"]);
                        if (dataReader["SeatingEndDateTime"] != null && dataReader["SeatingEndDateTime"] != DBNull.Value)
                            data.seating_end_time = Convert.ToDateTime(dataReader["SeatingEndDateTime"]);
                        data.is_walk_in = Convert.ToBoolean(dataReader["IsWalkIn"]);
                        if (!string.IsNullOrWhiteSpace(dataReader["PreAssign_Table_Id"].ToString()))
                        {
                            data.pre_assign_table_ids = JsonConvert.DeserializeObject<List<int>>(dataReader["PreAssign_Table_Id"].ToString());
                        }
                        if (!string.IsNullOrWhiteSpace(dataReader["AssignedTableIds"].ToString()))
                        {
                            data.assign_table_ids = JsonConvert.DeserializeObject<List<int>>(dataReader["AssignedTableIds"].ToString());
                        }

                        if (!string.IsNullOrWhiteSpace(dataReader["UserNotes"].ToString()))
                        {
                            data.account_note = JsonConvert.DeserializeObject<UserNote>(dataReader["UserNotes"].ToString());
                        }
                        data.pre_assign_server_id = Convert.ToInt32(dataReader["PreAssign_Server_Id"]);
                        data.pre_assigned_server_first_name = Convert.ToString(dataReader["PreAssignServerFirstName"]);
                        data.pre_assigned_server_last_name = Convert.ToString(dataReader["PreassignServerLastName"]);
                        data.pre_assigned_server_color = Convert.ToString(dataReader["PreAssignServerColor"]);

                        data.floor_plan_id = Convert.ToInt32(dataReader["PreAssignedFloorplanId"]);
                        data.floor_plan_name = Convert.ToString(dataReader["PreAssignedFloorPlanName"]);
                        data.floor_plan_technical_name = Convert.ToString(dataReader["PreAssignedFloorPlanTechName"]);


                        data.assigned_server_id = Convert.ToInt32(dataReader["AssignedServerId"]);
                        data.assigned_server_first_name = Convert.ToString(dataReader["AssignedServerFirstName"]);
                        data.assigned_server_last_name = Convert.ToString(dataReader["AssignedServerLastName"]);
                        data.assigned_server_color = Convert.ToString(dataReader["AssignedSererColor"]);


                        data.assigned_floor_plan_id = Convert.ToInt32(dataReader["AssignedFloorPlanId"]);
                        data.assigned_floor_plan_name = Convert.ToString(dataReader["AssignedFloorPlanName"]);
                        data.assigned_floor_plan_technical_name = Convert.ToString(dataReader["AssignedFloorPlanTechName"]);
                        data.customer_type = (Common.Common.CustomerType)Convert.ToInt32(dataReader["CustomerType"]);
                        data.event_id = Convert.ToInt32(dataReader["EventId"]);
                        data.event_name = Convert.ToString(dataReader["EventName"]);
                        data.referred_by_id = dataReader["ReferredById"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReferredById"]);
                        data.referred_by_first_name = dataReader["ReferredByFirstName"] == DBNull.Value ? "" : Convert.ToString(dataReader["ReferredByFirstName"]);
                        data.referred_by_last_name = dataReader["ReferredByLastName"] == DBNull.Value ? "" : Convert.ToString(dataReader["ReferredByLastName"]);
                        lstWaitlist.Add(data);

                    }
                }
            }

            return lstWaitlist;
        }

        public EventReservationChangesModel GetEventDetailForReservationChanges(int eventId)
        {
            EventReservationChangesModel data = new EventReservationChangesModel();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", eventId));

            using (DbDataReader dataReader = GetDataReader("GetEventDetailForReservationChanges", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        data.MemberID = Convert.ToInt32(dataReader["wineryid"]);
                        data.FeePerPerson = Convert.ToDecimal(dataReader["FeePerPerson"]);
                        data.ChargeSalesTax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        data.GratuityPercentage = Convert.ToDecimal(dataReader["GratuityPercentage"]);
                        data.TaxGratuity = Convert.ToBoolean(dataReader["TaxGratuity"]);
                        data.FeeType = Convert.ToInt32(dataReader["FeeType"]);
                        data.Zip = Convert.ToString(dataReader["Zip"]);
                        data.Address1 = Convert.ToString(dataReader["Address1"]);
                        data.city = Convert.ToString(dataReader["city"]);
                        data.state = Convert.ToString(dataReader["state"]);
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            data.item_id = Convert.ToInt32(dataReader["AddOn_Item_Id"]);
                            data.price = Convert.ToDecimal(dataReader["price"]);
                            data.Taxable = Convert.ToBoolean(dataReader["Taxable"]);
                            data.calculate_gratuity = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                            data.group_id = Convert.ToInt32(dataReader["AddOn_Group_Id"]);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            data.min_item_id = Convert.ToInt32(dataReader["AddOn_Item_Id"]);
                            data.min_price = Convert.ToDecimal(dataReader["price"]);
                            data.min_Taxable = Convert.ToBoolean(dataReader["Taxable"]);
                            data.min_calculate_gratuity = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                            data.min_group_id = Convert.ToInt32(dataReader["AddOn_Group_Id"]);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            data.max_item_id = Convert.ToInt32(dataReader["AddOn_Item_Id"]);
                            data.max_price = Convert.ToDecimal(dataReader["price"]);
                            data.max_Taxable = Convert.ToBoolean(dataReader["Taxable"]);
                            data.max_calculate_gratuity = Convert.ToBoolean(dataReader["CalculateGratuity"]);
                            data.max_group_id = Convert.ToInt32(dataReader["AddOn_Group_Id"]);
                        }
                    }
                }
            }

            return data;
        }

        public List<StateViewModel> GetStatesByCountryCode(string country_code)
        {
            List<StateViewModel> stateModelList = new List<StateViewModel>();
            List<StateRegionViewModel> StateRegions = new List<StateRegionViewModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@countryCode", country_code));

            string OldStateName = string.Empty;
            StateViewModel stateModel = new StateViewModel();
            StateRegionViewModel passportevent_dates = new StateRegionViewModel();


            using (DbDataReader dataReader = GetDataReader("GetStates", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        string StateName = Convert.ToString(dataReader["StateName"]);

                        if (string.IsNullOrEmpty(OldStateName) || OldStateName != StateName)
                        {
                            stateModel = new StateViewModel();
                            OldStateName = StateName;

                            stateModel.state_code = Convert.ToString(dataReader["StateCode"]);
                            stateModel.state_id = Convert.ToInt32(dataReader["StateId"]);

                            stateModel.state_name = StateName;
                            stateModel.state_regions = new List<StateRegionViewModel>();

                            stateModelList.Add(stateModel);
                        }

                        var stateRegionViewModel = new StateRegionViewModel();

                        stateRegionViewModel.region_name = Convert.ToString(dataReader["RegionName"]);
                        stateRegionViewModel.region_url = Convert.ToString(dataReader["RegionURL"]);
                        stateModel.state_regions.Add(stateRegionViewModel);
                    }
                }
            }

            return stateModelList;
        }


        public StateDataModel GetStateData(string state_name_or_url)
        {
            StateDataModel stateDataModel = new StateDataModel();                

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@stateName", state_name_or_url));

            using (DbDataReader dataReader = GetDataReader("GetStateWithMetaData", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        stateDataModel.id = Convert.ToInt32(dataReader["Id"]);
                        stateDataModel.friendly_url = Convert.ToString(dataReader["FriendlyUrl"]);
                        stateDataModel.title = Convert.ToString(dataReader["title"]);
                        stateDataModel.meta_description = Convert.ToString(dataReader["metaDescription"]);
                        stateDataModel.meta_keywords = Convert.ToString(dataReader["metaKeywords"]);
                        stateDataModel.state_name = Convert.ToString(dataReader["StateName"]);
                        stateDataModel.state_code = Convert.ToString(dataReader["StateCode"]);
                        stateDataModel.content = Convert.ToString(dataReader["Content"]);
                    }                   
                }
            }

            return stateDataModel;
        }

        public SpecialEventsPageFilterModel GetSpecialEventsPageFilter()
        {
            SpecialEventsPageFilterModel specialEventsPageFilterModel = new SpecialEventsPageFilterModel();

            List<DateModel> dates = new List<DateModel>();
            List<CategoriesModel> categories = new List<CategoriesModel>();
            List<LocationFilterModel> locations = new List<LocationFilterModel>();
            LocationFilterModel stateModel = new LocationFilterModel();

            string OldStateName = string.Empty;

            using (DbDataReader dataReader = GetDataReader("GetSpecialEventsPageFilter", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        string StateName = Convert.ToString(dataReader["State"]);

                        if (string.IsNullOrEmpty(OldStateName) || OldStateName != StateName)
                        {
                            stateModel = new LocationFilterModel();
                            OldStateName = StateName;

                            stateModel.state_name = StateName;
                            stateModel.cities = new List<CityModel>();

                            locations.Add(stateModel);
                        }

                        var cityModel = new CityModel();

                        cityModel.city = Convert.ToString(dataReader["City"]);
                        stateModel.cities.Add(cityModel);
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var categoriesModel = new CategoriesModel();

                            categoriesModel.id = Convert.ToInt32(dataReader["Id"]);
                            categoriesModel.name = Convert.ToString(dataReader["Name"]);

                            categories.Add(categoriesModel);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var dateModel = new DateModel();

                            dateModel.start_date = Convert.ToDateTime(dataReader["StartDate"]);
                            dateModel.end_date = Convert.ToDateTime(dataReader["EndDate"]);
                            dateModel.display_name = Convert.ToString(dataReader["DisplayName"]);

                            dates.Add(dateModel);
                        }
                    }
                }
            }

            specialEventsPageFilterModel.locations = locations;
            specialEventsPageFilterModel.dates = dates;
            specialEventsPageFilterModel.categories = categories;

            return specialEventsPageFilterModel;
        }

        public List<UserReservationsModel> GetUserReservations(int user_id, DateTime? to_date = null, DateTime? from_date = null)
        {
            var list = new List<UserReservationsModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@userId", user_id));
            parameterList.Add(GetParameter("@fromDate", from_date));
            parameterList.Add(GetParameter("@toDate", to_date));
            //parameterList.Add(GetParameter("@wineryId", member_id));
            //parameterList.Add(GetParameter("@isPastEvent", is_past_event));

            using (DbDataReader dataReader = GetDataReader("GetUserReservationsByUserId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        var model = new UserReservationsModel();

                        model.member_name = Convert.ToString(dataReader["WineryName"]);
                        model.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.booking_code = Convert.ToString(dataReader["BookingCode"]);
                        model.event_date = Convert.ToDateTime(dataReader["EventDate"]);
                        model.status = (ReservationStatus)(Convert.ToInt32(dataReader["Status"]));
                        model.total_guests = Convert.ToInt16(dataReader["TotalGuests"]);
                        model.booking_date = DateTimeOffset.Parse(Convert.ToString(dataReader["BookingDate"]));
                        model.is_past_event = Convert.ToBoolean(dataReader["IsPastEvent"]);
                        model.guest_last_name = Convert.ToString(dataReader["GuestLastName"]);
                        model.reservation_id = Convert.ToInt32(dataReader["ReservationId"]);
                        model.purchase_url = Convert.ToString(dataReader["PurchaseUrl"]);
                        model.booking_guid = Convert.ToString(dataReader["BookingGuid"]);
                        model.allow_cancel = Convert.ToBoolean(dataReader["AllowCancel"]);
                        model.destination_name = Convert.ToString(dataReader["DestinationName"]);
                        model.limit_my_account = Convert.ToBoolean(dataReader["LimitMyAccount"]);
                        model.event_end_time = Convert.ToDateTime(dataReader["EventEndTime"]);
                        model.cancel_policy = Convert.ToString(dataReader["CancelPolicy"]);
                        model.cancel_time = Convert.ToString(dataReader["CancelTime"]);
                        model.member_business_phone = Convert.ToString(dataReader["MemberBusinessPhone"]);
                        model.star = Convert.ToInt32(dataReader["Star"]);

                        model.is_reviewed = model.star > 0;

                        if (model.status == ReservationStatus.Pending)
                        {
                            model.status_text = "Confirmed";
                            model.status_css_class = "badge-rsvp-confirmed";
                        }
                        else if (model.status == ReservationStatus.Completed)
                        {
                            model.status_text = "Checked-in";
                            model.status_css_class = "badge-rsvp-checked-in";
                        }
                        else if (model.status == ReservationStatus.Cancelled)
                        {
                            model.status_text = "Cancelled";
                            model.status_css_class = "badge-rsvp-cancelled";
                        }
                        else if (model.status == ReservationStatus.NoShow)
                        {
                            model.status_text = "No Show";
                            model.status_css_class = "badge-rsvp-no-show";
                        }
                        else if (model.status == ReservationStatus.Rescheduled)
                        {
                            model.status_text = "Rescheduled";
                            model.status_css_class = "badge-rsvp-rescheduled";
                        }
                        else if (model.status == ReservationStatus.GuestDelayed)
                        {
                            model.status_text = "Guest Delayed";
                            model.status_css_class = "badge-rsvp-guest-delayed";
                        }
                        else if (model.status == ReservationStatus.Updated)
                        {
                            model.status_text = "Updated";
                            model.status_css_class = "badge-rsvp-updated";
                        }
                        else if (model.status == ReservationStatus.YelpInitiated)
                        {
                            model.status_text = "Yelp Temporary";
                            model.status_css_class = "badge-rsvp-yelp-initiated";
                        }
                        else if (model.status == ReservationStatus.Initiated)
                        {
                            model.status_text = "Initiated";
                            model.status_css_class = "badge-rsvp-initiated";
                        }

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<CountryModel> GetCountries()
        {
            var list = new List<CountryModel>();

            using (DbDataReader dataReader = GetDataReader("GetCountries", null, CommandType.StoredProcedure))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        var model = new CountryModel();

                        model.name = Convert.ToString(dataReader["Name"]);
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.country_code = Convert.ToString(dataReader["CountryCode"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public CmsPageModel GetPagesByUrl(string url)
        {
            var Item = new CmsPageModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@url", url));
            var cms_page_sections = new List<CmsPageSectionModel>();
            var cms_page_images = new List<CmsPageImageModel>();

            using (DbDataReader dataReader = GetDataReader("GetPagesByUrl", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Item.id = Convert.ToInt32(dataReader["Id"]);
                        Item.friendly_url = Convert.ToString(dataReader["FriendlyUrl"]);
                        Item.title = Convert.ToString(dataReader["Title"]);
                        Item.meta_description = Convert.ToString(dataReader["MetaDescription"]);
                        Item.meta_keywords = Convert.ToString(dataReader["MetaKeywords"]);
                        Item.content = Convert.ToString(dataReader["Content"]);
                        Item.config_values = Convert.ToString(dataReader["ConfigValues"]);
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var cms_page_section = new CmsPageSectionModel();

                            cms_page_section.id = Convert.ToInt32(dataReader["id"]);
                            cms_page_section.type = (CmsPageSectionType)Convert.ToInt32(dataReader["Type"]);

                            if (dataReader["StartDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["StartDate"].ToString()))
                                cms_page_section.start_date = Convert.ToDateTime(dataReader["StartDate"]);

                            if (dataReader["EndDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["EndDate"].ToString()))
                                cms_page_section.end_date = Convert.ToDateTime(dataReader["EndDate"]);

                            cms_page_section.filter = Convert.ToString(dataReader["Filter"]);
                            cms_page_section.sort_position = Convert.ToInt32(dataReader["SortPosition"]);
                            cms_page_section.title = Convert.ToString(dataReader["Title"]);
                            cms_page_section.desc = Convert.ToString(dataReader["Desc"]);
                            cms_page_section.total_cols = Convert.ToInt32(dataReader["TotalCols"]);

                            cms_page_section.cms_page_section_cols = GetCmsPageSectionColByCMSPageSectionId(cms_page_section.id);

                            cms_page_sections.Add(cms_page_section);
                        }
                    }

                    Item.cms_page_sections = cms_page_sections;

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var cms_page_image = new CmsPageImageModel();

                            cms_page_image.id = Convert.ToInt32(dataReader["Id"]);
                            cms_page_image.cms_page_entity_type = (CMSPageEntityType)Convert.ToInt32(dataReader["CMSPageEntityType"]);
                            cms_page_image.entity_id = Convert.ToInt32(dataReader["EntityId"]);
                            cms_page_image.image_path = Convert.ToString(dataReader["ImagePath"]);

                            cms_page_images.Add(cms_page_image);
                        }
                    }

                    Item.cms_page_images = cms_page_images;
                }
            }

            return Item;
        }

        public CmsPageSectionModel GetPageSectionById(int id)
        {
            var cms_page_section = new CmsPageSectionModel();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", id));

            using (DbDataReader dataReader = GetDataReader("GetPageSectionById", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        cms_page_section.id = Convert.ToInt32(dataReader["id"]);
                        cms_page_section.type = (CmsPageSectionType)Convert.ToInt32(dataReader["Type"]);

                        if (dataReader["StartDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["StartDate"].ToString()))
                            cms_page_section.start_date = Convert.ToDateTime(dataReader["StartDate"]);

                        if (dataReader["EndDate"] != DBNull.Value && !string.IsNullOrWhiteSpace(dataReader["EndDate"].ToString()))
                            cms_page_section.end_date = Convert.ToDateTime(dataReader["EndDate"]);

                        cms_page_section.filter = Convert.ToString(dataReader["Filter"]);
                        cms_page_section.sort_position = Convert.ToInt32(dataReader["SortPosition"]);
                        cms_page_section.title = Convert.ToString(dataReader["Title"]);
                        cms_page_section.desc = Convert.ToString(dataReader["Desc"]);
                        cms_page_section.total_cols = Convert.ToInt32(dataReader["TotalCols"]);

                        cms_page_section.cms_page_section_cols = GetCmsPageSectionColByCMSPageSectionId(cms_page_section.id);
                    }
                }
            }

            return cms_page_section;
        }

        public List<EventTypes> GetEventsTypes(EventTypeGroupFilterModel model)
        {
            var listEventTypes = new List<EventTypes>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@topRecords", model.NoOfRecord));
            parameterList.Add(GetParameter("@eventTypeGroupId", model.EventTypeGroupId));
            parameterList.Add(GetParameter("@sortByPopularity", model.SortByPopularity));
            parameterList.Add(GetParameter("@regionId", model.Regionid));
            parameterList.Add(GetParameter("@subregionId", model.SubRegionId));
            parameterList.Add(GetParameter("@isAdvancedFilter", model.IsAdvancedFilter));
            parameterList.Add(GetParameter("@subRegionIds", model.SubRegionIds));
            parameterList.Add(GetParameter("@eventTypes", model.EventTypes));
            parameterList.Add(GetParameter("@reviews", model.Reviews));
            parameterList.Add(GetParameter("@populartags", model.PopularTags));
            parameterList.Add(GetParameter("@varietals", model.Varietals));
            parameterList.Add(GetParameter("@notableFeatures", model.NotableFeatures));

            using (DbDataReader dataReader = GetDataReader("GetEventsTypes", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var eventTypes = new EventTypes();

                        eventTypes.id = Convert.ToInt32(dataReader["EventTypeId"]);
                        eventTypes.event_type_name = Convert.ToString(dataReader["EventTypeName"]);

                        listEventTypes.Add(eventTypes);
                    }
                }
            }

            return listEventTypes;
        }

        public List<EventTypeDestinationModel> GetDestinationByEventTypeName(string eventType, int? regionId)
        {
            var listEventTypes = new List<EventTypeDestinationModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@eventTypeName", eventType));
            parameterList.Add(GetParameter("@appleationId", regionId));

            using (DbDataReader dataReader = GetDataReader("GetDestinationByEventTypeName", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var eventTypes = new EventTypeDestinationModel();

                        eventTypes.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        eventTypes.display_name = Convert.ToString(dataReader["DisplayName"]);
                        eventTypes.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        eventTypes.city = Convert.ToString(dataReader["City"]);
                        eventTypes.reviews = Convert.ToInt32(dataReader["Reviews"]);
                        eventTypes.star = Convert.ToDecimal(dataReader["Star"]);
                        eventTypes.recommendation_tag = Convert.ToString(dataReader["RecommendationTag"]);
                        eventTypes.tag_1 = Convert.ToString(dataReader["Tag1"]);
                        eventTypes.tag_2 = Convert.ToString(dataReader["Tag2"]);
                        eventTypes.member_ava = Convert.ToInt32(dataReader["WineryAva"]);
                        eventTypes.attributes = Convert.ToString(dataReader["Attributes"]);
                        eventTypes.region_id = Convert.ToInt32(dataReader["RegionId"]);
                        eventTypes.member_type = Convert.ToInt32(dataReader["WineryType"]);
                        eventTypes.featured_count = Convert.ToInt32(dataReader["FeaturedCount"]);
                        eventTypes.billing_plan = Convert.ToInt32(dataReader["BillingPlan"]);
                        eventTypes.event_id = Convert.ToInt32(dataReader["EventId"]);

                        listEventTypes.Add(eventTypes);
                    }
                }
            }

            return listEventTypes;
        }

        public List<CmsPageSectionColModel> GetCmsPageSectionColByCMSPageSectionId(int CMSPageSectionId)
        {
            var cms_page_section_cols = new List<CmsPageSectionColModel>();

            string sql = "select [Type],ColNo,ConfigValues,Active from  CMSPageSectionCols (nolock) where CMSPageSectionId = @CMSPageSectionId and Active =1";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@CMSPageSectionId", CMSPageSectionId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var cms_page_section_col = new CmsPageSectionColModel();

                        cms_page_section_col.type = (CmsPageSectionType)Convert.ToInt32(dataReader["Type"]);
                        cms_page_section_col.colno = Convert.ToInt32(dataReader["ColNo"]);
                        cms_page_section_col.config_values = Convert.ToString(dataReader["ConfigValues"]);
                        cms_page_section_col.active = Convert.ToBoolean(dataReader["Active"]);

                        cms_page_section_cols.Add(cms_page_section_col);
                    }
                }
            }
            return cms_page_section_cols;
        }

        public string GetCmsPageSectionFilterById(int Id)
        {
            string Filter = string.Empty;

            string sql = "select [Filter] from CmsPageSections where id=@Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Filter = Convert.ToString(dataReader["Filter"]);
                    }
                }
            }
            return Filter;
        }

        public BookedExperincesCache GetBookedEventExperiences(int? regionId, int? wineryId, int? subRegionId)
        {
            BookedExperincesCache eventExperinces = new BookedExperincesCache();
            string sql = string.Empty;
            var parameterList = new List<DbParameter>();

            if (wineryId != null)
            {
                sql = "select top 1 LastFetchDate,JsonResult,Id from BookedExperincesCache where WineryId = @wineryId";
                parameterList.Add(GetParameter("@wineryId", wineryId));
            }
            else if (regionId != null && subRegionId != null)
            {
                sql = "select top 1 LastFetchDate,JsonResult,Id from BookedExperincesCache where SubRegionId = @subRegionId and RegionId = @regionId";
                parameterList.Add(GetParameter("@subRegionId", subRegionId));
                parameterList.Add(GetParameter("@regionId", regionId));
            }
            else if (regionId != null)
            {
                sql = "select top 1 LastFetchDate,JsonResult,Id from BookedExperincesCache where RegionId = @regionId";
                parameterList.Add(GetParameter("@regionId", regionId));
            }

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        eventExperinces.LastFetchDate = Convert.ToDateTime(dataReader["LastFetchDate"]);
                        eventExperinces.Id = Convert.ToInt32(dataReader["Id"]);
                        string jsonresult = Convert.ToString(dataReader["JsonResult"]);
                        eventExperinces.JsonResult = jsonresult;
                    }
                }
            }

            return eventExperinces;
        }

        public List<EventExperienceModel> GetEventExperiences(EventExperienceFilterModel filterModel)
        {
            var experiences = new List<EventExperienceModel>();

            string sql = "GetEventExperience";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@noOfRecord", filterModel.NoOfRecord));
            parameterList.Add(GetParameter("@appleationId", filterModel.AppleationId));
            parameterList.Add(GetParameter("@subregionId", filterModel.SubRegionId));
            parameterList.Add(GetParameter("@orderBy", filterModel.OrderBy));
            parameterList.Add(GetParameter("@daysAfter", filterModel.DaysAfter));
            parameterList.Add(GetParameter("@isAdvancedFilter", filterModel.IsAdvancedFilter));
            parameterList.Add(GetParameter("@subRegionIds", filterModel.SubRegionIds));
            parameterList.Add(GetParameter("@eventTypes", filterModel.EventTypes));
            parameterList.Add(GetParameter("@reviews", filterModel.Reviews));
            parameterList.Add(GetParameter("@populartags", filterModel.PopularTags));
            parameterList.Add(GetParameter("@varietals", filterModel.Varietals));
            parameterList.Add(GetParameter("@notableFeatures", filterModel.NotableFeatures));
            parameterList.Add(GetParameter("@wineryId", filterModel.WineryId));
            parameterList.Add(GetParameter("@isUniqueRecords", filterModel.isUniqueRecords));


            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new EventExperienceModel();
                                             
                        model.acount_type_req= Convert.ToBoolean(dataReader["AcountTypeReq"]);
                        model.business_id = Convert.ToInt32(dataReader["WineryId"]);
                        model.business_name = Convert.ToString(dataReader["WineryName"]);
                        model.city = Convert.ToString(dataReader["City"]);
                        model.event_id = Convert.ToInt32(dataReader["EventId"]);
                        model.event_image = Convert.ToString(dataReader["EventImage"]);
                        model.event_name = Convert.ToString(dataReader["EventName"]);
                        model.member_benefit_req = Convert.ToBoolean(dataReader["MemberBenefitReq"]);
                        model.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        model.region_name = Convert.ToString(dataReader["RegionName"]);
                        model.reviews = Convert.ToInt32(dataReader["Reviews"]);
                        model.star = Convert.ToDecimal(dataReader["Star"]);

                        experiences.Add(model);
                    }
                }
            }
            return experiences;
        }

        public bool SaveBookedEventExperiences(int? regionId, int? wineryId, int? subRegionId, string jsonResult)
        {
            var eventExperinces = GetBookedEventExperiences(regionId, wineryId, subRegionId);

            string sql = string.Empty;
            var parameterList = new List<DbParameter>();

            if (eventExperinces.Id == 0)
            {
                sql = "INSERT INTO [dbo].[BookedExperincesCache] ([RegionId],[LastFetchDate],[JsonResult],[SubRegionId],[WineryId]) VALUES (@RegionId,GETUTCDATE(),@JsonResult,@SubRegionId,@WineryId)";

                parameterList.Add(GetParameter("@RegionId", regionId));
                parameterList.Add(GetParameter("@JsonResult", jsonResult));
                parameterList.Add(GetParameter("@SubRegionId", subRegionId));
                parameterList.Add(GetParameter("@WineryId", wineryId));

            }
            else
            {
                sql = "UPDATE [dbo].[BookedExperincesCache] SET [LastFetchDate] = GETUTCDATE(),[JsonResult] = @JsonResult WHERE Id = @Id";

                parameterList.Add(GetParameter("@JsonResult", jsonResult));
                parameterList.Add(GetParameter("@Id", eventExperinces.Id));
            }

            return ExecuteNonQuery(sql, parameterList, CommandType.Text) > 0;
        }

        public List<BusinessSearchResultModel> GetBusinessSearchResult(string searchTerm)
        {
            var wineries = GetBusinessWinerySearchResult(searchTerm);
            var eventsModel = GetBusinessEventSearchResult(searchTerm);
            List<BusinessSearchResultModel> lstBusinessSearchResultModel = new List<BusinessSearchResultModel>();

            for (int i = 0; i < eventsModel.Count; i++)
            {
                BusinessSearchResultModel BusinessSearchResultModel = new BusinessSearchResultModel
                {
                    business_search_result_type = BusinessSearchResultType.Event,
                    event_title = eventsModel[i].EventTitle,
                    event_start_date = eventsModel[i].EventStartDate,
                    event_city = eventsModel[i].City,
                    event_state = eventsModel[i].State,
                    friendly_url = eventsModel[i].FriendlyURL,
                    event_organizer_name = eventsModel[i].EventOrganizerName
                };
                lstBusinessSearchResultModel.Add(BusinessSearchResultModel);
            }

            for (int i = 0; i < wineries.Count; i++)
            {
                if (lstBusinessSearchResultModel.Any(x => x.display_name != null && x.display_name.Equals(wineries[i].DisplayName, StringComparison.OrdinalIgnoreCase) && x.business_search_result_type == BusinessSearchResultType.Property))
                    continue;

                BusinessSearchResultModel BusinessSearchResultModel = new BusinessSearchResultModel
                {
                    business_search_result_type = BusinessSearchResultType.Property,
                    display_name = wineries[i].DisplayName,
                    city = wineries[i].City,
                    state = wineries[i].State,
                    purchase_url = wineries[i].PurchaseURL,
                    business_type = wineries[i].WineryType,
                    billing_plan = wineries[i].BillingPlan
                };
                lstBusinessSearchResultModel.Add(BusinessSearchResultModel);

            }

            for (int i = 0; i < wineries.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(wineries[i].State) || string.IsNullOrWhiteSpace(wineries[i].City))
                    continue;

                if (lstBusinessSearchResultModel.Any(x => x.city != null && x.state != null && x.city.Equals(wineries[i].City, StringComparison.OrdinalIgnoreCase) && x.state.Equals(wineries[i].State, StringComparison.OrdinalIgnoreCase)
                && x.business_search_result_type == BusinessSearchResultType.Location))
                    continue;

                BusinessSearchResultModel BusinessSearchResultModel = new BusinessSearchResultModel
                {
                    business_search_result_type = BusinessSearchResultType.Location,
                    city = StringHelpers.ToTitleCase(wineries[i].City),
                    state = wineries[i].State,
                    business_type = wineries[i].WineryType,
                    region = wineries[i].Region,
                    region_url = wineries[i].RegionURL,
                    billing_plan = wineries[i].BillingPlan
                };
                lstBusinessSearchResultModel.Add(BusinessSearchResultModel);
            }

            for (int i = 0; i < wineries.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(wineries[i].Region))
                    continue;

                if (lstBusinessSearchResultModel.Any(x => x.region != null && x.region.Equals(wineries[i].Region, StringComparison.OrdinalIgnoreCase) && x.business_search_result_type == BusinessSearchResultType.Region))
                    continue;

                BusinessSearchResultModel BusinessSearchResultModel = new BusinessSearchResultModel
                {
                    business_search_result_type = BusinessSearchResultType.Region,
                    region = StringHelpers.ToTitleCase(wineries[i].Region),
                    region_url = wineries[i].RegionURL,
                    business_type = wineries[i].WineryType,
                    go_to_landing = wineries[i].GoToLanding
                };
                lstBusinessSearchResultModel.Add(BusinessSearchResultModel);
            }

            return lstBusinessSearchResultModel;
        }

        public List<BusinessWinerySearchResultModel> GetBusinessWinerySearchResult(string searchTerm)
        {
            List<BusinessWinerySearchResultModel> lstBusinessSearchResultModel = new List<BusinessWinerySearchResultModel>();

            string sql = "BusinessWinerySearch";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@searchTerm", searchTerm));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new BusinessWinerySearchResultModel();

                        model.BillingPlan = Convert.ToInt32(dataReader["BillingPlan"]);
                        model.City = Convert.ToString(dataReader["City"]);
                        model.DisplayName = Convert.ToString(dataReader["DisplayName"]);
                        model.GoToLanding = Convert.ToBoolean(dataReader["GoToLanding"]);
                        model.PurchaseURL = Convert.ToString(dataReader["PurchaseURL"]);
                        model.Region = Convert.ToString(dataReader["Region"]);
                        model.RegionURL = Convert.ToString(dataReader["RegionURL"]);
                        model.State = Convert.ToString(dataReader["State"]);
                        model.WineryType = Convert.ToInt32(dataReader["WineryType"]);

                        lstBusinessSearchResultModel.Add(model);
                    }
                }
            }

            return lstBusinessSearchResultModel;
        }

        public List<BusinessEventSearchResultViewModel> GetBusinessEventSearchResult(string searchTerm)
        {
            List<BusinessEventSearchResultViewModel> lstBusinessSearchResultModel = new List<BusinessEventSearchResultViewModel>();

            string sql = "BusinessEventSearch";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@searchTerm", searchTerm));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new BusinessEventSearchResultViewModel();

                        model.City = Convert.ToString(dataReader["City"]);
                        model.EventId = Convert.ToInt32(dataReader["EventId"]);
                        model.EventOrganizerName = Convert.ToString(dataReader["EventOrganizerName"]);
                        model.EventStartDate = Convert.ToDateTime(dataReader["EventStartDate"]);
                        model.EventTitle = Convert.ToString(dataReader["EventTitle"]);
                        model.State = Convert.ToString(dataReader["State"]);

                        lstBusinessSearchResultModel.Add(model);
                    }
                }
            }

            return lstBusinessSearchResultModel;
        }

        public List<EventTypeModel> GetEventType()
        {
            var list = new List<EventTypeModel>();

            string sql = "select Id,EventTypeName,[description] from EventType where Active=1 order by EventTypeName";

            using (DbDataReader dataReader = GetDataReader(sql, null, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new EventTypeModel();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.event_type_name = Convert.ToString(dataReader["EventTypeName"]);
                        model.description = Convert.ToString(dataReader["description"]);

                        list.Add(model);
                    }
                }
            }

            return list;
        }
    }
}
