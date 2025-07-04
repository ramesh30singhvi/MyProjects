using CPReservationApi.Common;
using CPReservationApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using static CPReservationApi.Common.Email;
using uc = CPReservationApi.Common;
using Address = CPReservationApi.Model.Address;
using static CPReservationApi.Common.Common;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;

namespace CPReservationApi.DAL
{
    public class TicketDAL : BaseDataAccess
    {
        public TicketDAL(string connectionString) : base(connectionString)
        {
        }

        public List<TicketEventModel> GetTicketEventsByWineryId(int wineryId, int event_type)
        {
            string sp = "GetTicketEventsByWineryId";

            var eventS = new List<TicketEventModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@EventType", event_type));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tEvent = new TicketEventModel();
                        tEvent.id = Convert.ToInt32(dataReader["Id"]);
                        tEvent.status = (uc.Common.TicketsEventStatus)Convert.ToInt32(dataReader["Status"]);
                        tEvent.name = Convert.ToString(dataReader["EventTitle"]);
                        tEvent.start_date = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tEvent.end_date = Convert.ToDateTime(dataReader["EndDateTime"]);
                        tEvent.checkin_type = (uc.Common.AttendeeAppCheckInMode)Convert.ToInt32(dataReader["AttendeeAppCheckInMode"]);
                        tEvent.attendance_mode = (uc.Common.AttendanceModeStatus)Convert.ToInt32(dataReader["AttendanceMode"]);
                        //tEvent.location_id = Convert.ToInt32(dataReader["VenueLocationId"]);
                        tEvent.venue_address_1 = Convert.ToString(dataReader["VenueAddress1"]);
                        tEvent.venue_address_2 = Convert.ToString(dataReader["VenueAddress2"]);

                        string Regions = Convert.ToString(dataReader["Regions"]);
                        if (Regions.Length > 0)
                        {
                            Regions = "[" + Regions + "]";
                            tEvent.regions_ids = JsonConvert.DeserializeObject<List<int>>(Regions);
                        }

                        string tags = Convert.ToString(dataReader["Tags"]);
                        if (tags.Length > 0)
                        {
                            tEvent.tags = tags.Split(',').ToList();
                        }

                        tEvent.primary_category = Convert.ToInt32(dataReader["Category"]);
                        tEvent.secondary_category = Convert.ToInt32(dataReader["Category2"]);
                        tEvent.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        tEvent.venue_country = Convert.ToString(dataReader["VenueCounty"]);
                        tEvent.venue_latitude = Convert.ToString(dataReader["VenueLatitude"]);
                        tEvent.venue_longitude = Convert.ToString(dataReader["VenueLongitude"]);
                        tEvent.venue_state = Convert.ToString(dataReader["VenueState"]);
                        tEvent.venue_zip = Convert.ToString(dataReader["VenueZip"]);
                        tEvent.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        tEvent.timezone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        tEvent.timezone_offset = Times.GetOffsetMinutes(tEvent.timezone);
                        tEvent.checkin_allowed = CheckInAllowed(tEvent.start_date, tEvent.timezone);
                        tEvent.event_type = Convert.ToInt32(dataReader["event_type"]);
                        tEvent.total_tickets_sold = Convert.ToInt32(dataReader["total_tickets_sold"]);
                        tEvent.event_capacity = Convert.ToInt32(dataReader["event_capacity"]);
                        tEvent.guest_list_enabled = Convert.ToBoolean(dataReader["GuestListEnabled"]);

                        if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.standardSingle)
                            tEvent.checkin_type_description = "Guest List Mode, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.barcodeSingle)
                            tEvent.checkin_type_description = "Barcode Only, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.barcodeMulti)
                            tEvent.checkin_type_description = "Barcode Only, Multi Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.searchSingle)
                            tEvent.checkin_type_description = "Search Mode, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.searchMulti)
                            tEvent.checkin_type_description = "Search Mode, Multi Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.multiEvent)
                            tEvent.checkin_type_description = "Multi-Event Check-In";

                        tEvent.status_description = Enum.GetName(typeof(uc.Common.TicketsEventStatus), tEvent.status);

                        if (tEvent.end_date < Times.ToTimeZoneTime(DateTime.UtcNow, tEvent.timezone) || tEvent.status == TicketsEventStatus.CANCELLED || tEvent.status == TicketsEventStatus.ENDED)
                            tEvent.event_ended = true;

                        tEvent.city_url = Convert.ToString(dataReader["VenueCity"]);
                        //tEvent.locations = GetLocationByTicketEventID(tEvent.id);
                        tEvent.participating_members = GetParticipatingLocationByTicketEventID(tEvent.id);

                        tEvent.membership_start_date = Convert.ToDateTime(dataReader["MembershipStartDate"]);
                        tEvent.membership_end_date = Convert.ToDateTime(dataReader["MembershipExpiresDate"]);
                        tEvent.refund_policy = (uc.Common.TicketRefundPolicy)Convert.ToInt32(dataReader["TicketRefundPolicy"]);
                        tEvent.refund_policy_text = tEvent.refund_policy.GetEnumDescription();
                        tEvent.refund_service_fees_option = (uc.Common.RefundServiceFeesOption)Convert.ToInt32(dataReader["RefundServiceFeesOption"]);
                        tEvent.refund_service_fees_option_text = tEvent.refund_service_fees_option.GetEnumDescription();
                        tEvent.collect_tax = Convert.ToBoolean(dataReader["ChargeTax"]);

                        tEvent.order_special_instructions = Convert.ToString(dataReader["OrderSpecialInstructions"]);
                        tEvent.confirmation_page = Convert.ToString(dataReader["ConfirmationPage"]);
                        tEvent.business_message = Convert.ToString(dataReader["ConfirmationEmail"]);
                        tEvent.require_reservations = Convert.ToBoolean(dataReader["DisplayRsvpBookingOnEventPage"]);
                        tEvent.disable_travel_time_restrictions = Convert.ToBoolean(dataReader["DisableTravelTimeRestrictions"]);

                        if (!string.IsNullOrWhiteSpace(dataReader["InternalNotificationRecipient"].ToString()))
                        {
                            try
                            {
                                List<int> internalNotificationRecipient = dataReader["InternalNotificationRecipient"].ToString().Split(',').Select(int.Parse).ToList();
                                tEvent.internal_notification_recipient = internalNotificationRecipient;
                            }
                            catch { }
                        }

                        if (!string.IsNullOrWhiteSpace(dataReader["Tax_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["Tax_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                tEvent.tax_ticketlevels = ticketLevels;
                            }
                            catch { }
                        }
                        eventS.Add(tEvent);
                    }
                }
            }
            return eventS;
        }

        public List<ProfileTicketEventModel> GetProfileTicketEventsByWineryId(int wineryId)
        {
            string sp = "GetProfileTicketEventsByWineryId";

            var eventS = new List<ProfileTicketEventModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", wineryId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tEvent = new ProfileTicketEventModel();
                        tEvent.id = Convert.ToInt32(dataReader["Id"]);
                        tEvent.status = (uc.Common.TicketsEventStatus)Convert.ToInt32(dataReader["Status"]);
                        tEvent.name = Convert.ToString(dataReader["EventTitle"]);
                        tEvent.start_date = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tEvent.end_date = Convert.ToDateTime(dataReader["EndDateTime"]);
                        tEvent.membership_start_date = Convert.ToDateTime(dataReader["MembershipStartDate"]);
                        tEvent.membership_end_date = Convert.ToDateTime(dataReader["MembershipExpiresDate"]);
                        tEvent.checkin_type = (uc.Common.AttendeeAppCheckInMode)Convert.ToInt32(dataReader["AttendeeAppCheckInMode"]);
                        tEvent.attendance_mode = (uc.Common.AttendanceModeStatus)Convert.ToInt32(dataReader["AttendanceMode"]);
                        //tEvent.location_id = Convert.ToInt32(dataReader["VenueLocationId"]);
                        tEvent.venue_address_1 = Convert.ToString(dataReader["VenueAddress1"]);
                        tEvent.venue_address_2 = Convert.ToString(dataReader["VenueAddress2"]);
                        tEvent.event_organizer_url = Convert.ToString(dataReader["WebsiteURL"]);

                        string Regions = Convert.ToString(dataReader["Regions"]);
                        if (Regions.Length > 0)
                        {
                            Regions = "[" + Regions + "]";
                            tEvent.regions_ids = JsonConvert.DeserializeObject<List<int>>(Regions);
                        }
                        string tags = Convert.ToString(dataReader["Tags"]);
                        if (tags.Length > 0)
                        {
                            tEvent.tags = tags.Split(',').ToList();
                        }

                        tEvent.primary_category = Convert.ToInt32(dataReader["Category"]);
                        tEvent.secondary_category = Convert.ToInt32(dataReader["Category2"]);
                        tEvent.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        tEvent.venue_country = Convert.ToString(dataReader["VenueCounty"]);
                        tEvent.venue_latitude = Convert.ToString(dataReader["VenueLatitude"]);
                        tEvent.venue_longitude = Convert.ToString(dataReader["VenueLongitude"]);
                        tEvent.venue_state = Convert.ToString(dataReader["VenueState"]);
                        tEvent.venue_zip = Convert.ToString(dataReader["VenueZip"]);
                        tEvent.timezone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        tEvent.timezone_offset = Times.GetOffsetMinutes(tEvent.timezone);
                        tEvent.checkin_allowed = CheckInAllowed(tEvent.start_date, tEvent.timezone);
                        tEvent.event_type = Convert.ToInt32(dataReader["event_type"]);
                        tEvent.total_tickets_sold = Convert.ToInt32(dataReader["total_tickets_sold"]);
                        tEvent.event_capacity = Convert.ToInt32(dataReader["event_capacity"]);
                        tEvent.guest_list_enabled = Convert.ToBoolean(dataReader["GuestListEnabled"]);
                        tEvent.event_image = Convert.ToString(dataReader["EventImage"]);
                        tEvent.event_image_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        tEvent.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.standardSingle)
                            tEvent.checkin_type_description = "Guest List Mode, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.barcodeSingle)
                            tEvent.checkin_type_description = "Barcode Only, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.barcodeMulti)
                            tEvent.checkin_type_description = "Barcode Only, Multi Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.searchSingle)
                            tEvent.checkin_type_description = "Search Mode, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.searchMulti)
                            tEvent.checkin_type_description = "Search Mode, Multi Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.multiEvent)
                            tEvent.checkin_type_description = "Multi-Event Check-In";

                        tEvent.status_description = Enum.GetName(typeof(uc.Common.TicketsEventStatus), tEvent.status);

                        if (tEvent.end_date < Times.ToTimeZoneTime(DateTime.UtcNow, tEvent.timezone) || tEvent.status == TicketsEventStatus.CANCELLED || tEvent.status == TicketsEventStatus.ENDED)
                            tEvent.event_ended = true;

                        tEvent.city_url = Convert.ToString(dataReader["VenueCity"]);
                        //tEvent.locations = GetLocationByTicketEventID(tEvent.id);
                        //tEvent.participating_members = GetParticipatingLocationByTicketEventID(tEvent.id);
                        tEvent.short_description = Convert.ToString(dataReader["ShortDesc"]);
                        tEvent.long_description = Convert.ToString(dataReader["LongDesc"]);
                        string event_url = GetFriendlyURL(tEvent.name, tEvent.id);
                        tEvent.event_url = event_url;
                        tEvent.refund_policy = (uc.Common.TicketRefundPolicy)Convert.ToInt32(dataReader["TicketRefundPolicy"]);
                        tEvent.refund_policy_text = tEvent.refund_policy.GetEnumDescription();
                        tEvent.refund_service_fees_option = (uc.Common.RefundServiceFeesOption)Convert.ToInt32(dataReader["RefundServiceFeesOption"]);
                        tEvent.refund_service_fees_option_text = tEvent.refund_service_fees_option.GetEnumDescription();
                        tEvent.collect_tax = Convert.ToBoolean(dataReader["ChargeTax"]);

                        tEvent.order_special_instructions = Convert.ToString(dataReader["OrderSpecialInstructions"]);
                        tEvent.confirmation_page = Convert.ToString(dataReader["ConfirmationPage"]);
                        tEvent.business_message = Convert.ToString(dataReader["ConfirmationEmail"]);

                        tEvent.sold_out = Convert.ToBoolean(dataReader["SoldOut"]);
                        tEvent.waitlist_available = Convert.ToBoolean(dataReader["WaitlistAvl"]);

                        if (!string.IsNullOrWhiteSpace(dataReader["InternalNotificationRecipient"].ToString()))
                        {
                            try
                            {
                                List<int> internalNotificationRecipient = dataReader["InternalNotificationRecipient"].ToString().Split(',').Select(int.Parse).ToList();
                                tEvent.internal_notification_recipient = internalNotificationRecipient;
                            }
                            catch { }
                        }

                        if (!string.IsNullOrWhiteSpace(dataReader["Tax_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["Tax_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                tEvent.tax_ticketlevels = ticketLevels;
                            }
                            catch { }
                        }

                        tEvent.min_price = Convert.ToDecimal(dataReader["MinPrice"]);
                        tEvent.max_price = Convert.ToDecimal(dataReader["MaxPrice"]);

                        if (tEvent.min_price == tEvent.max_price && tEvent.min_price <= 0)
                            tEvent.price_range = "Complimentary";
                        else if (tEvent.min_price != tEvent.max_price)
                            tEvent.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", tEvent.min_price < 0 ? 0 : tEvent.min_price) + " - " + string.Format(new CultureInfo("en-US"), "{0:C}", tEvent.max_price) + "/pp";
                        else if (tEvent.min_price == tEvent.max_price)
                            tEvent.price_range = string.Format(new CultureInfo("en-US"), "{0:C}", tEvent.min_price) + "/pp";

                        eventS.Add(tEvent);
                    }
                }
            }
            return eventS;
        }

        public string GetFriendlyURL(string eventTitle, int eventId)
        {
            if (eventTitle == null || eventId == 0)
            {
                return "";
            }

            string url = "";
            url = string.Format("{0}-{1}", Regex.Replace(eventTitle, "[^A-Za-z0-9 ]+", "").Replace("  ", " ").Replace(" ", "-").TrimEnd('-'), eventId);
            return url.ToLower();
        }

        private string GetTimezoneNameById(Times.TimeZone timeZone)
        {
            string tzName = "Pacific";
            switch (timeZone)
            {
                case Times.TimeZone.ArizonaTimeZone:
                    tzName = "Arizona";
                    break;
                case Times.TimeZone.CentralTimeZone:
                    tzName = "Central";
                    break;
                case Times.TimeZone.MountainTimeZone:
                    tzName = "Mountain";
                    break;
                case Times.TimeZone.EasternTimeZone:
                    tzName = "East";
                    break;
                case Times.TimeZone.PacificTimeZone:
                    tzName = "Pacific";
                    break;

            }
            return tzName;
        }

        public string GetTicketEventUrlById(int eventId)
        {
            string eventUrl = "";
            string sql = "select EventURL from Tickets_Event where id = @EventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", eventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        eventUrl = Convert.ToString(dataReader["EventURL"]);
                    }
                }
            }
            return eventUrl;
        }
        public TicketEventDetailModel GetTicketEventDetailsById(int event_id, ref string eventPassword)
        {
            string sp = "GetTicketEventDetailsById";
            var listQuestions = new List<TicketQuestion>();
            var tEvent = new TicketEventDetailModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", event_id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        tEvent.id = Convert.ToInt32(dataReader["Id"]);
                        tEvent.status = (uc.Common.TicketsEventStatus)Convert.ToInt32(dataReader["Status"]);
                        tEvent.name = Convert.ToString(dataReader["EventTitle"]);
                        tEvent.short_description = Convert.ToString(dataReader["ShortDesc"]);
                        tEvent.long_description = Convert.ToString(dataReader["LongDesc"]);
                        tEvent.member_name = Convert.ToString(dataReader["WineryName"]);
                        tEvent.membership_start_date = Convert.ToDateTime(dataReader["MembershipStartDate"]);
                        tEvent.membership_end_date = Convert.ToDateTime(dataReader["MembershipExpiresDate"]);
                        tEvent.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        tEvent.start_date = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tEvent.end_date = Convert.ToDateTime(dataReader["EndDateTime"]);
                        tEvent.checkin_type = (uc.Common.AttendeeAppCheckInMode)Convert.ToInt32(dataReader["AttendeeAppCheckInMode"]);
                        tEvent.attendance_mode = (uc.Common.AttendanceModeStatus)Convert.ToInt32(dataReader["AttendanceMode"]);
                        //tEvent.location_id = Convert.ToInt32(dataReader["VenueLocationId"]);
                        tEvent.venue_address_1 = Convert.ToString(dataReader["VenueAddress1"]);
                        tEvent.venue_address_2 = Convert.ToString(dataReader["VenueAddress2"]);

                        tEvent.disable_travel_time_restrictions = Convert.ToBoolean(dataReader["DisableTravelTimeRestrictions"]);
                        tEvent.require_reservations = Convert.ToBoolean(dataReader["DisplayRsvpBookingOnEventPage"]);

                        string Regions = Convert.ToString(dataReader["Regions"]);
                        if (Regions.Length > 0)
                        {
                            Regions = "[" + Regions + "]";
                            tEvent.regions_ids = JsonConvert.DeserializeObject<List<int>>(Regions);
                        }
                        string tags = Convert.ToString(dataReader["Tags"]);
                        if (tags.Length > 0)
                        {
                            tEvent.tags = tags.Split(',').ToList();
                        }

                        tEvent.primary_category = Convert.ToInt32(dataReader["Category"]);
                        tEvent.secondary_category = Convert.ToInt32(dataReader["Category2"]);

                        tEvent.cta_button = Convert.ToInt32(dataReader["CTAButton"]);
                        tEvent.disable_book_itinerary_msg = Convert.ToBoolean(dataReader["DisableBookItineraryMsg"]);

                        tEvent.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        tEvent.venue_country = Convert.ToString(dataReader["VenueCounty"]);
                        tEvent.venue_latitude = Convert.ToString(dataReader["VenueLatitude"]);
                        tEvent.venue_longitude = Convert.ToString(dataReader["VenueLongitude"]);
                        tEvent.venue_state = Convert.ToString(dataReader["VenueState"]);
                        tEvent.venue_zip = Convert.ToString(dataReader["VenueZip"]);

                        tEvent.timezone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        tEvent.timezone_name = GetTimezoneNameById(tEvent.timezone);
                        tEvent.timezone_offset = Times.GetOffsetMinutes(tEvent.timezone);
                        tEvent.checkin_allowed = CheckInAllowed(tEvent.start_date, tEvent.timezone);
                        tEvent.event_type = Convert.ToInt32(dataReader["event_type"]);
                        tEvent.total_tickets_sold = Convert.ToInt32(dataReader["total_tickets_sold"]);
                        tEvent.event_capacity = Convert.ToInt32(dataReader["event_capacity"]);

                        tEvent.min_price = Convert.ToDecimal(dataReader["MinPrice"]);
                        tEvent.max_price = Convert.ToDecimal(dataReader["MaxPrice"]);

                        tEvent.requires_invite = Convert.ToBoolean(dataReader["RequiresInvite"]);
                        tEvent.requires_password = Convert.ToBoolean(dataReader["RequiresPassword"]);
                        tEvent.is_private = Convert.ToBoolean(dataReader["isPrivate"]);
                        tEvent.show_discount_code = Convert.ToBoolean(dataReader["ShowDiscountCode"]);
                        tEvent.is_automated_discounts = Convert.ToBoolean(dataReader["ShowAutomatedDiscount"]);
                        tEvent.show_access_code = Convert.ToBoolean(dataReader["ShowAccessCode"]);
                        tEvent.show_guest_list = Convert.ToBoolean(dataReader["ShowGuestList"]);
                        tEvent.guest_lists = Convert.ToString(dataReader["Guestlists"]);
                        tEvent.sold_out = Convert.ToBoolean(dataReader["SoldOut"]);
                        tEvent.waitlist_available = Convert.ToBoolean(dataReader["WaitlistAvl"]);
                        tEvent.available_tickets = Convert.ToBoolean(dataReader["AvailableTickets"]);
                        tEvent.ticket_cancel_policy = Convert.ToString(dataReader["TicketCancelPolicy"]);
                        tEvent.event_attendee_policy = Convert.ToString(dataReader["EventAttendeePolicy"]);
                        string EnabledCreditCards = Convert.ToString(dataReader["EnabledCreditCards"]);
                        tEvent.accepted_card_types = EnabledCreditCards.Replace("32", "20").Split(',').Select(Int32.Parse).ToList();

                        if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.standardSingle)
                            tEvent.checkin_type_description = "Guest List Mode, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.barcodeSingle)
                            tEvent.checkin_type_description = "Barcode Only, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.barcodeMulti)
                            tEvent.checkin_type_description = "Barcode Only, Multi Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.searchSingle)
                            tEvent.checkin_type_description = "Search Mode, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.searchMulti)
                            tEvent.checkin_type_description = "Search Mode, Multi Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.multiEvent)
                            tEvent.checkin_type_description = "Multi-Event Check-In";

                        tEvent.status_description = Enum.GetName(typeof(uc.Common.TicketsEventStatus), tEvent.status);

                        tEvent.require_hdyh = Convert.ToBoolean(dataReader["ReqHDYH"]);
                        tEvent.show_will_call_adress = Convert.ToBoolean(dataReader["ShowWillCallAddress"]);
                        tEvent.service_fee_option = (Common.Common.TicketsServiceFeesOption)Convert.ToInt32(dataReader["ServiceFeesOption"]);
                        tEvent.purchase_timeout = Convert.ToInt32(dataReader["PurchaseTimeout"]);
                        tEvent.waitlist_expiration = Convert.ToInt32(dataReader["WaitlistExpiration"]);
                        tEvent.email_template_id = Convert.ToInt32(dataReader["EmailReceiptTemplate"]);
                        tEvent.purchase_policy_id = Convert.ToInt32(dataReader["TicketPurchasePolicy"]);

                        tEvent.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        tEvent.event_image_big = Convert.ToString(dataReader["EventImageBig"]);
                        tEvent.event_image_big_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        tEvent.event_url = Convert.ToString(dataReader["EventURL"]);
                        tEvent.is_cp_payment_gateway = Convert.ToBoolean(dataReader["CPPaymentGateway"]);
                        if (tEvent.end_date < Times.ToTimeZoneTime(DateTime.UtcNow, tEvent.timezone) || tEvent.status == TicketsEventStatus.CANCELLED || tEvent.status == TicketsEventStatus.ENDED)
                            tEvent.event_ended = true;

                        if (tEvent.event_ended)
                        {
                            tEvent.show_upcoming_events = ShowUpcomingEvents(tEvent.member_id);
                        }

                        tEvent.city_url = Convert.ToString(dataReader["VenueCity"]);
                        //tEvent.locations = GetLocationByTicketEventID(tEvent.id, tEvent.location_id);

                        eventPassword = Convert.ToString(dataReader["EventPassword"]);
                        tEvent.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        tEvent.event_organizer_phone = Convert.ToString(dataReader["EventOrganizerPhone"]);
                        tEvent.event_organizer_email = Convert.ToString(dataReader["EventOrganizerEmail"]);
                        tEvent.event_organizer_url = Convert.ToString(dataReader["WebsiteURL"]);
                        tEvent.region_name = Convert.ToString(dataReader["RegionName"]);
                        tEvent.state = Convert.ToString(dataReader["VenueState"]);
                        tEvent.show_organizer_phone = Convert.ToBoolean(dataReader["DisplayPhone"]);
                        tEvent.show_map = Convert.ToBoolean(dataReader["DisplayMap"]);
                        tEvent.show_book_rsvp_btn = Convert.ToBoolean(dataReader["DisplayBookRSVPBtn"]);
                        tEvent.rsvp_booking_short_desc = Convert.ToString(dataReader["RsvpBookingShortDesc"]);
                        tEvent.rsvp_booking_long_desc = Convert.ToString(dataReader["RsvpBookingLongDesc"]);
                        tEvent.block_repeat_bookings = Convert.ToBoolean(dataReader["BlockRepeatBookings"]);
                        tEvent.limit_bookings_per_day = Convert.ToInt32(dataReader["LimitBookingsPerDay"]);
                        tEvent.show_availability_btn = Convert.ToBoolean(dataReader["DisableCheckAvailability"]);
                        tEvent.disable_activate_button = Convert.ToBoolean(dataReader["DisableActivateButton"]);
                        tEvent.refund_policy = (uc.Common.TicketRefundPolicy)Convert.ToInt32(dataReader["TicketRefundPolicy"]);
                        tEvent.refund_policy_text = tEvent.refund_policy.GetEnumDescription();
                        tEvent.refund_service_fees_option = (uc.Common.RefundServiceFeesOption)Convert.ToInt32(dataReader["RefundServiceFeesOption"]);
                        tEvent.refund_service_fees_option_text = tEvent.refund_service_fees_option.GetEnumDescription();
                        tEvent.collect_tax = Convert.ToBoolean(dataReader["ChargeTax"]);

                        tEvent.order_special_instructions = Convert.ToString(dataReader["OrderSpecialInstructions"]);
                        tEvent.confirmation_page = Convert.ToString(dataReader["ConfirmationPage"]);
                        tEvent.business_message = Convert.ToString(dataReader["ConfirmationEmail"]);
                        if (dataReader["LastTicketSalesEndIn"] != DBNull.Value)
                        {
                            try
                            {
                                tEvent.last_ticket_sales_end_in = Convert.ToDateTime(dataReader["LastTicketSalesEndIn"]);
                            }
                            catch { }
                        }

                        if (!string.IsNullOrWhiteSpace(dataReader["InternalNotificationRecipient"].ToString()))
                        {
                            try
                            {
                                List<int> internalNotificationRecipient = dataReader["InternalNotificationRecipient"].ToString().Split(',').Select(int.Parse).ToList();
                                tEvent.internal_notification_recipient = internalNotificationRecipient;
                            }
                            catch { }
                        }

                        if (!string.IsNullOrWhiteSpace(dataReader["Tax_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["Tax_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                tEvent.tax_ticketlevels = ticketLevels;
                            }
                            catch { }
                        }
                        //buyr flags
                        TicketHolderConfig thConfig = new TicketHolderConfig
                        {

                            require_address = Convert.ToBoolean(dataReader["TH_ReqAddress"]),
                            require_mem_num = Convert.ToBoolean(dataReader["ReqMembershipNum"]),
                            require_dob = Convert.ToBoolean(dataReader["TH_ReqDOB"]),
                            show_dob = Convert.ToBoolean(dataReader["TH_ReqDOB"]),
                            require_website = Convert.ToBoolean(dataReader["TH_ReqWebsite"]),
                            require_age = Convert.ToBoolean(dataReader["TH_ReqAge"]),
                            show_firstname = Convert.ToBoolean(dataReader["TH_ShowFirstName"]),
                            require_firstname = Convert.ToBoolean(dataReader["TH_ReqFirstName"]),
                            show_lastname = Convert.ToBoolean(dataReader["TH_ShowLastName"]),
                            require_lastname = Convert.ToBoolean(dataReader["TH_ReqLastName"]),
                            show_gender = Convert.ToBoolean(dataReader["TH_ShowGender"]),
                            require_gender = Convert.ToBoolean(dataReader["TH_ReqGender"]),
                            show_age_group = Convert.ToBoolean(dataReader["AgeGroup"]),
                            require_age_group = Convert.ToBoolean(dataReader["AgeGroup"]),
                            show_company = Convert.ToBoolean(dataReader["TH_ShowCompany"]),
                            require_company = Convert.ToBoolean(dataReader["TH_ReqCompany"]),
                            show_email = Convert.ToBoolean(dataReader["TH_ShowEmail"]),
                            require_email = Convert.ToBoolean(dataReader["TH_ReqEmail"]),
                            show_title = Convert.ToBoolean(dataReader["TH_ShowTitle"]),
                            require_title = Convert.ToBoolean(dataReader["TH_ReqTitle"]),
                            show_workphone = Convert.ToBoolean(dataReader["TH_ShowWorkPhone"]),
                            require_workphone = Convert.ToBoolean(dataReader["TH_ReqWorkPhone"]),
                            show_mobilephone = Convert.ToBoolean(dataReader["TH_ShowMobilePhone"]),
                            require_mobilephone = Convert.ToBoolean(dataReader["TH_ReqMobilePhone"]),
                            show_zipcode = Convert.ToBoolean(dataReader["TH_ShowZipCode"]),
                            require_zipcode = Convert.ToBoolean(dataReader["TH_ReqZipCode"]),
                            unique_email = Convert.ToBoolean(dataReader["TH_UniqueEmail"]),
                            requires_21 = Convert.ToBoolean(dataReader["TH_Req21Older"]),
                        };

                        if (!string.IsNullOrWhiteSpace(dataReader["TH_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["TH_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                thConfig.ticket_levels = ticketLevels;
                            }
                            catch { }
                        }
                        tEvent.ticket_holder_config = thConfig;


                        TicketPostCaptureConfig pcConfig = new TicketPostCaptureConfig();

                        var parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_FirstName"]));
                        pcConfig.require_firstname = parser.require_flag;
                        pcConfig.show_firstname = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_LastName"]));
                        pcConfig.require_lastname = parser.require_flag;
                        pcConfig.show_lastname = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Company"]));
                        pcConfig.require_company = parser.require_flag;
                        pcConfig.show_company = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Email"]));
                        pcConfig.require_email = parser.require_flag;
                        pcConfig.show_email = parser.show_flag;

                        pcConfig.unique_email = Convert.ToBoolean(dataReader["PC_UniqueEmail"]);

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Title"]));
                        pcConfig.require_title = parser.require_flag;
                        pcConfig.show_title = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Gender"]));
                        pcConfig.require_gender = parser.require_flag;
                        pcConfig.show_gender = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Age"]));
                        pcConfig.require_age = parser.require_flag;
                        //pcConfig.show_age_group = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Website"]));
                        pcConfig.require_website = parser.require_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_WorkPhone"]));
                        pcConfig.require_workphone = parser.require_flag;
                        pcConfig.show_workphone = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_MobilePhone"]));
                        pcConfig.require_mobilephone = parser.require_flag;
                        pcConfig.show_mobilephone = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_DOB"]));
                        pcConfig.require_dob = parser.require_flag;
                        pcConfig.show_dob = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Address"]));
                        pcConfig.require_address = parser.require_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_AgeGroup"]));
                        pcConfig.require_age_group = parser.require_flag;
                        pcConfig.show_age_group = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_ZipCode"]));
                        pcConfig.require_zipcode = parser.require_flag;
                        pcConfig.show_zipcode = parser.show_flag;
                        pcConfig.requires_21 = Convert.ToBoolean(dataReader["TH_Req21Older"]);

                        if (!string.IsNullOrWhiteSpace(dataReader["PC_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["PC_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                pcConfig.ticket_levels = ticketLevels;
                            }
                            catch { }
                        }
                        tEvent.post_capture_config = pcConfig;

                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                var ticketQuestion = new TicketQuestion();
                                List<string> lstChoices = null;
                                var choices = Convert.ToString(dataReader["Choices"]);
                                if (!string.IsNullOrEmpty(choices))
                                {
                                    lstChoices = choices.Split("@!@").ToList();
                                }
                                ticketQuestion.question_id = Convert.ToInt32(dataReader["Id"]);
                                ticketQuestion.question_type = Convert.ToString(dataReader["QuestionType"]);
                                ticketQuestion.question_text = Convert.ToString(dataReader["QuestionText"]);
                                ticketQuestion.is_required = Convert.ToBoolean(dataReader["IsRequired"]);
                                ticketQuestion.is_default_state = Convert.ToBoolean(dataReader["DefaultState"]);
                                ticketQuestion.question_show_to = Convert.ToInt32(dataReader["QuestionShowTo"]);
                                ticketQuestion.choices = lstChoices;
                                listQuestions.Add(ticketQuestion);
                            }
                        }

                        //tEvent.will_call_locations = GetWillLocationsByTicketEventID(tEvent.id);
                        //tEvent.will_call_location_details = GetTicketsEventWillCallLocationByTicketId(tEvent.id);

                        var listLocation = new List<LocationModel>();
                        if (dataReader.NextResult())
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

                                Address addr = new Address();

                                addr.address_1 = Convert.ToString(dataReader["Address1"]);
                                addr.address_2 = Convert.ToString(dataReader["Address2"]);
                                addr.city = Convert.ToString(dataReader["City"]);
                                addr.state = Convert.ToString(dataReader["State"]);
                                addr.zip_code = Convert.ToString(dataReader["Zip"]);
                                addr.country = Convert.ToString(dataReader["country"]);
                                locationModel.address = addr;

                                listLocation.Add(locationModel);
                            }
                        }

                        tEvent.will_call_locations = listLocation;

                        var listTicketsEventWillCallLocation = new List<TicketsEventWillCallLocation>();

                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                var item = new TicketsEventWillCallLocation();
                                item.ticket_id = Convert.ToInt32(dataReader["Tickets_Ticket_Id"]);
                                item.location_name = Convert.ToString(dataReader["LocationName"]);
                                item.location_id = Convert.ToInt32(dataReader["LocationId"]);
                                item.will_call_limit = Convert.ToInt32(dataReader["WillCallLimit"]);
                                item.order_qty = Convert.ToInt32(dataReader["OrderQty"]);
                                item.available_qty = Convert.ToInt32(dataReader["AvlQty"]);

                                if (item.available_qty > 0)
                                    listTicketsEventWillCallLocation.Add(item);
                            }
                        }

                        tEvent.will_call_location_details = listTicketsEventWillCallLocation;

                        var listAdditionalContent = new List<AdditionalContent>();

                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                var item = new AdditionalContent();
                                item.id = Convert.ToInt32(dataReader["Id"]);
                                item.content = Convert.ToString(dataReader["Content"]);
                                item.content_type = Convert.ToInt32(dataReader["ContentType"]);
                                item.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                                item.image_name = Convert.ToString(dataReader["ImageName"]);

                                if (item.content_type == 3)
                                {
                                    item.content = string.Format("<div class=\"text-center my-5\"><a href=\"{0}\" class=\"btn btn-primary\" target=\"_blank\">{1}</a>&nbsp;</div>", item.content, item.image_name);
                                    item.image_name = "";
                                }


                                if (item.content_type == 2)
                                {
                                    item.content = string.Format("<div class=\"text-center my-5\"><a href=\"{0}\" target=\"_blank\"><img src=\"{2}/{1}\"/></a>&nbsp;</div>", item.content, item.image_name, StringHelpers.GetImagePath(ImageType.ticketEventImage,ImagePathType.azure));
                                }

                                    listAdditionalContent.Add(item);
                            }
                        }

                        tEvent.additional_content = listAdditionalContent;
                    }


                }
            }
            tEvent.attendee_questions = listQuestions;
            return tEvent;
        }

        public WineryReviews GetTicketEventReviews(int TicketEventId)
        {
            WineryReviews model = new WineryReviews();
            string sp = "GetTicketEventReviews";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TicketEventId", TicketEventId));

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

        public TicketEventLandingPageModel GetTicketEventLandingPageData(int event_id, int user_id, ref string eventPassword)
        {
            string sp = "GetTicketEventLandingPageData";
            var listQuestions = new List<TicketQuestion>();
            var tEvent = new TicketEventLandingPageModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", event_id));
            parameterList.Add(GetParameter("@UserId", user_id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        tEvent.id = Convert.ToInt32(dataReader["Id"]);
                        tEvent.status = (uc.Common.TicketsEventStatus)Convert.ToInt32(dataReader["Status"]);
                        tEvent.name = Convert.ToString(dataReader["EventTitle"]);
                        tEvent.meta_keywords= Convert.ToString(dataReader["MetaKeywords"]);
                        tEvent.meta_description = Convert.ToString(dataReader["MetaDescription"]);
                        tEvent.short_description = Convert.ToString(dataReader["ShortDesc"]);
                        tEvent.long_description = Convert.ToString(dataReader["LongDesc"]);
                        tEvent.business_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        tEvent.instagram_url = Convert.ToString(dataReader["InstagramUrl"]);
                        tEvent.twitter_url = Convert.ToString(dataReader["TwitterUrl"]);
                        tEvent.facebook_url = Convert.ToString(dataReader["FacebookUrl"]);
                        tEvent.pinterest_url = Convert.ToString(dataReader["PintrestUrl"]);
                        tEvent.business_name = Convert.ToString(dataReader["WineryName"]);

                        tEvent.cta_button = Convert.ToInt32(dataReader["CTAButton"]);
                        tEvent.disable_book_itinerary_msg = Convert.ToBoolean(dataReader["DisableBookItineraryMsg"]);

                        tEvent.membership_start_date = Convert.ToDateTime(dataReader["MembershipStartDate"]);
                        tEvent.membership_end_date = Convert.ToDateTime(dataReader["MembershipExpiresDate"]);
                        tEvent.business_profile_url = Convert.ToString(dataReader["PurchaseURL"]);
                        tEvent.start_date = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tEvent.end_date = Convert.ToDateTime(dataReader["EndDateTime"]);
                         tEvent.attendance_mode = (uc.Common.AttendanceModeStatus)Convert.ToInt32(dataReader["AttendanceMode"]);
                        //tEvent.location_id = Convert.ToInt32(dataReader["VenueLocationId"]);
                        VenueLocation location = new VenueLocation
                        {
                            address_1 = Convert.ToString(dataReader["VenueAddress1"]),
                            address_2 = Convert.ToString(dataReader["VenueAddress2"]),
                            city = Convert.ToString(dataReader["VenueCity"]),
                            country = Convert.ToString(dataReader["VenueCounty"]),
                            state = Convert.ToString(dataReader["VenueState"]),
                            zip_code = Convert.ToString(dataReader["VenueZip"]),
                            geo_latitude = Convert.ToString(dataReader["VenueLatitude"]),
                            geo_longitude = Convert.ToString(dataReader["VenueLongitude"])

                        };
                        tEvent.location = location;

                       // tEvent.disable_travel_time_restrictions = Convert.ToBoolean(dataReader["DisableTravelTimeRestrictions"]);
                        tEvent.require_reservations = Convert.ToBoolean(dataReader["DisplayRsvpBookingOnEventPage"]);

                        string Regions = Convert.ToString(dataReader["Regions"]);
                        if (Regions.Length > 0)
                        {
                            Regions = "[" + Regions + "]";
                            tEvent.regions_ids = JsonConvert.DeserializeObject<List<int>>(Regions);
                        }
                        string tags = Convert.ToString(dataReader["Tags"]);
                        if (tags.Length > 0)
                        {
                            tEvent.tags = tags.Split(',').ToList();
                        }

                        tEvent.primary_category_id = Convert.ToInt32(dataReader["Category"]);
                        tEvent.secondary_category_id = Convert.ToInt32(dataReader["Category2"]);
                        tEvent.primary_category = Convert.ToString(dataReader["EventCategory1"]);
                        tEvent.secondary_category = Convert.ToString(dataReader["EventCategory2"]);

                        tEvent.timezone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        tEvent.timezone_name = GetTimezoneNameById(tEvent.timezone);
                        tEvent.timezone_offset = Times.GetOffsetMinutes(tEvent.timezone);
                        tEvent.event_type = Convert.ToInt32(dataReader["event_type"]);
                        tEvent.total_tickets_sold = Convert.ToInt32(dataReader["total_tickets_sold"]);
                        tEvent.event_capacity = Convert.ToInt32(dataReader["event_capacity"]);

                        tEvent.min_price = Convert.ToDecimal(dataReader["MinPrice"]);
                        tEvent.max_price = Convert.ToDecimal(dataReader["MaxPrice"]);

                        tEvent.requires_invite = Convert.ToBoolean(dataReader["RequiresInvite"]);
                        tEvent.requires_password = Convert.ToBoolean(dataReader["RequiresPassword"]);
                        tEvent.is_private = Convert.ToBoolean(dataReader["isPrivate"]);
                        tEvent.show_discount_code = Convert.ToBoolean(dataReader["ShowDiscountCode"]);
                        tEvent.is_automated_discounts = Convert.ToBoolean(dataReader["ShowAutomatedDiscount"]);
                        tEvent.show_access_code = Convert.ToBoolean(dataReader["ShowAccessCode"]);
                        tEvent.show_guest_list = Convert.ToBoolean(dataReader["ShowGuestList"]);
                        tEvent.guest_lists = Convert.ToString(dataReader["Guestlists"]);
                        tEvent.sold_out = Convert.ToBoolean(dataReader["SoldOut"]);
                        tEvent.waitlist_available = Convert.ToBoolean(dataReader["WaitlistAvl"]);
                        tEvent.available_tickets = Convert.ToBoolean(dataReader["AvailableTickets"]);
                        tEvent.ticket_cancel_policy = Convert.ToString(dataReader["TicketCancelPolicy"]);

                        tEvent.event_attendee_policy = Convert.ToString(dataReader["EventAttendeePolicy"]);
                        tEvent.country_name = Convert.ToString(dataReader["CountryName"]);
                        tEvent.state_name = Convert.ToString(dataReader["StateName"]);

                        tEvent.region_url = Convert.ToString(dataReader["RegionFriendlyURL"]);

                        tEvent.status_description = Enum.GetName(typeof(uc.Common.TicketsEventStatus), tEvent.status);

                        
                        tEvent.event_image_big = Convert.ToString(dataReader["EventImageBig"]);
                        tEvent.event_image = Convert.ToString(dataReader["EventImage"]);

                        tEvent.event_image_big_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        tEvent.event_image_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        tEvent.event_url = Convert.ToString(dataReader["EventURL"]);
                        tEvent.is_cp_payment_gateway = Convert.ToBoolean(dataReader["CPPaymentGateway"]);
                        if (tEvent.end_date < Times.ToTimeZoneTime(DateTime.UtcNow, tEvent.timezone) || tEvent.status == TicketsEventStatus.CANCELLED || tEvent.status == TicketsEventStatus.ENDED)
                            tEvent.event_ended = true;

                        bool upcomingEvents = ShowUpcomingEvents(tEvent.business_id);
                        if (tEvent.event_ended)
                        {
                            tEvent.show_upcoming_events = upcomingEvents;
                        }
                        tEvent.has_upcoming_events = upcomingEvents;

                        tEvent.city_url = Convert.ToString(dataReader["VenueCity"]);
                        //tEvent.locations = GetLocationByTicketEventID(tEvent.id, tEvent.location_id);

                        eventPassword = Convert.ToString(dataReader["EventPassword"]);
                        tEvent.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        tEvent.event_organizer_phone = Convert.ToString(dataReader["EventOrganizerPhone"]);
                        tEvent.event_organizer_email = Convert.ToString(dataReader["EventOrganizerEmail"]);
                        tEvent.event_organizer_url = Convert.ToString(dataReader["WebsiteURL"]);
                        tEvent.business_region_name = Convert.ToString(dataReader["BusinessRegionName"]);
                        tEvent.show_organizer_phone = Convert.ToBoolean(dataReader["DisplayPhone"]);
                        tEvent.show_map = Convert.ToBoolean(dataReader["DisplayMap"]);
                        tEvent.show_book_rsvp_btn = Convert.ToBoolean(dataReader["DisplayBookRSVPBtn"]);
                        tEvent.rsvp_booking_short_desc = Convert.ToString(dataReader["RsvpBookingShortDesc"]);
                        tEvent.rsvp_booking_long_desc = Convert.ToString(dataReader["RsvpBookingLongDesc"]);
                        tEvent.show_availability_btn = Convert.ToBoolean(dataReader["DisableCheckAvailability"]);
                        tEvent.disable_activate_button = Convert.ToBoolean(dataReader["DisableActivateButton"]);
                        tEvent.refund_policy = (uc.Common.TicketRefundPolicy)Convert.ToInt32(dataReader["TicketRefundPolicy"]);
                        tEvent.refund_policy_text = tEvent.refund_policy.GetEnumDescription();
                        tEvent.refund_service_fees_option = (uc.Common.RefundServiceFeesOption)Convert.ToInt32(dataReader["RefundServiceFeesOption"]);
                        tEvent.refund_service_fees_option_text = tEvent.refund_service_fees_option.GetEnumDescription();
 
                        if (dataReader["LastTicketSalesEndIn"] != DBNull.Value)
                        {
                            try
                            {
                                tEvent.last_ticket_sales_end_in = Convert.ToDateTime(dataReader["LastTicketSalesEndIn"]);
                            }
                            catch { }
                        }

                        TicketHolderConfig thConfig = new TicketHolderConfig
                        {

                            require_address = Convert.ToBoolean(dataReader["TH_ReqAddress"]),
                            require_mem_num = Convert.ToBoolean(dataReader["ReqMembershipNum"]),
                            require_dob = Convert.ToBoolean(dataReader["TH_ReqDOB"]),
                            require_website = Convert.ToBoolean(dataReader["TH_ReqWebsite"]),
                            require_age = Convert.ToBoolean(dataReader["TH_ReqAge"]),

                            show_firstname = Convert.ToBoolean(dataReader["TH_ShowFirstName"]),
                            require_firstname = Convert.ToBoolean(dataReader["TH_ReqFirstName"]),

                            show_zipcode = Convert.ToBoolean(dataReader["TH_ShowZipCode"]),
                            require_zipcode = Convert.ToBoolean(dataReader["TH_ReqZipCode"]),

                            show_lastname = Convert.ToBoolean(dataReader["TH_ShowLastName"]),
                            require_lastname = Convert.ToBoolean(dataReader["TH_ReqLastName"]),


                            show_gender = Convert.ToBoolean(dataReader["TH_ShowGender"]),
                            require_gender = Convert.ToBoolean(dataReader["TH_ReqGender"]),

                            show_age_group = Convert.ToBoolean(dataReader["AgeGroup"]),
                            require_age_group = Convert.ToBoolean(dataReader["AgeGroup"]),

                            show_company = Convert.ToBoolean(dataReader["TH_ShowCompany"]),
                            require_company = Convert.ToBoolean(dataReader["TH_ReqCompany"]),

                            show_email = Convert.ToBoolean(dataReader["TH_ShowEmail"]),
                            require_email = Convert.ToBoolean(dataReader["TH_ReqEmail"]),
                            unique_email = Convert.ToBoolean(dataReader["TH_UniqueEmail"]),

                            show_title = Convert.ToBoolean(dataReader["TH_ShowTitle"]),
                            require_title = Convert.ToBoolean(dataReader["TH_ReqTitle"]),

                            show_workphone = Convert.ToBoolean(dataReader["TH_ShowWorkPhone"]),
                            require_workphone = Convert.ToBoolean(dataReader["TH_ReqWorkPhone"]),

                            show_mobilephone = Convert.ToBoolean(dataReader["TH_ShowMobilePhone"]),
                            require_mobilephone = Convert.ToBoolean(dataReader["TH_ReqMobilePhone"]),
                        };

                        if (!string.IsNullOrWhiteSpace(dataReader["TH_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["TH_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                thConfig.ticket_levels = ticketLevels;
                            }
                            catch { }
                        }
                        tEvent.ticket_holder_config = thConfig;

                        TicketPostCaptureConfig pcConfig = new TicketPostCaptureConfig();

                        var parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_FirstName"]));
                        pcConfig.require_firstname = parser.require_flag;
                        pcConfig.show_firstname = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_LastName"]));
                        pcConfig.require_lastname = parser.require_flag;
                        pcConfig.show_lastname = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Company"]));
                        pcConfig.require_company = parser.require_flag;
                        pcConfig.show_company = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Email"]));
                        pcConfig.require_email = parser.require_flag;
                        pcConfig.show_email = parser.show_flag;

                        pcConfig.unique_email = Convert.ToBoolean(dataReader["PC_UniqueEmail"]);

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Title"]));
                        pcConfig.require_title = parser.require_flag;
                        pcConfig.show_title = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Gender"]));
                        pcConfig.require_gender = parser.require_flag;
                        pcConfig.show_gender = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Age"]));
                        pcConfig.require_age = parser.require_flag;
                        //pcConfig.show_age_group = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Website"]));
                        pcConfig.require_website = parser.require_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_WorkPhone"]));
                        pcConfig.require_workphone = parser.require_flag;
                        pcConfig.show_workphone = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_MobilePhone"]));
                        pcConfig.require_mobilephone = parser.require_flag;
                        pcConfig.show_mobilephone = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_DOB"]));
                        pcConfig.require_dob = parser.require_flag;
                        pcConfig.show_dob = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Address"]));
                        pcConfig.require_address = parser.require_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_AgeGroup"]));
                        pcConfig.require_age_group = parser.require_flag;
                        pcConfig.show_age_group = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_ZipCode"]));
                        pcConfig.require_zipcode = parser.require_flag;
                        pcConfig.show_zipcode = parser.show_flag;

                        if (!string.IsNullOrWhiteSpace(dataReader["PC_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["PC_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                pcConfig.ticket_levels = ticketLevels;
                            }
                            catch { }
                        }
                        tEvent.post_capture_config = pcConfig;

                        tEvent.has_post_config_ticket_levels = Convert.ToBoolean(dataReader["HasPostConfigTicketLevel"]); ;
                        tEvent.has_reviews = Convert.ToBoolean(dataReader["HasReviews"]);

                        tEvent.is_user_favorite = Convert.ToBoolean(dataReader["IsUserFavorite"]);
                        tEvent.show_broadcast = Convert.ToBoolean(dataReader["ShowBroadcast"]);
                        tEvent.broadcast_url = Convert.ToString(dataReader["BroadcastUrl"]);                        

                        string questionJson = Convert.ToString(dataReader["QuestionJson"]);
                        if (!string.IsNullOrWhiteSpace(questionJson))
                        {
                            try
                            {
                                tEvent.attendee_questions = JsonConvert.DeserializeObject<List<TicketQuestion>>(questionJson);
                            }
                            catch { }
                        }

                        string faqJson = Convert.ToString(dataReader["FAQJson"]);
                        if (!string.IsNullOrWhiteSpace(faqJson))
                        {
                            try
                            {
                                tEvent.faqs = JsonConvert.DeserializeObject<List<TicketFAQ>>(faqJson);
                            }
                            catch { }
                        }


                         var listAdditionalContent = new List<AdditionalContent>();

                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                var item = new AdditionalContent();
                                item.id = Convert.ToInt32(dataReader["Id"]);
                                item.content = Convert.ToString(dataReader["Content"]);
                                item.content_type = Convert.ToInt32(dataReader["ContentType"]);
                                item.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                                item.image_name = Convert.ToString(dataReader["ImageName"]);

                                if (item.content_type == 3)
                                {
                                    item.content = string.Format("<div class=\"text-center my-5\"><a href=\"{0}\" class=\"btn btn-primary\" target=\"_blank\">{1}</a>&nbsp;</div>", item.content, item.image_name);
                                    item.image_name = "";
                                }


                                if (item.content_type == 2)
                                {
                                    item.content = string.Format("<div class=\"text-center my-5\"><a href=\"{0}\" target=\"_blank\"><img src=\"https://cdncellarpass.blob.core.windows.net/photos/tickets/{1}\"/></a>&nbsp;</div>", item.content, item.image_name);
                                }

                                listAdditionalContent.Add(item);
                            }
                        }

                        tEvent.additional_content = listAdditionalContent;

                        if (tEvent.primary_category_id == (int)Common.Common.TicketCategory.Passport || tEvent.primary_category_id == (int)Common.Common.TicketCategory.Membership)
                        { 
                            tEvent.participating_locations = GetParticipatingLocationByTicketEventID(tEvent.id);
                        }

                    }


                }
            }

            return tEvent;
        }


        public List<UpcomingEventModel> GetTicketEventsComponent(TicketEventsComponentRequest req)
        {
            string sp = "GetTicketEventsComponentV3";

            var upcomingEvents = new List<UpcomingEventModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@topRecords", req.topRecords));
            parameterList.Add(GetParameter("@memberid", req.memberid));
            parameterList.Add(GetParameter("@regionId", req.regionId));
            parameterList.Add(GetParameter("@orderby", req.orderby));
            parameterList.Add(GetParameter("@onePerMember", req.onePerMember));
            parameterList.Add(GetParameter("@onePerRegion", req.onePerRegion));
            parameterList.Add(GetParameter("@daysAfter", req.daysAfter));
            parameterList.Add(GetParameter("@subregionId", req.subregionId));
            parameterList.Add(GetParameter("@isAdvancedFilter", req.isAdvancedFilter));
            parameterList.Add(GetParameter("@subRegionIds", req.subRegionIds));
            parameterList.Add(GetParameter("@eventTypes", req.eventTypes));
            parameterList.Add(GetParameter("@reviews", req.reviews));
            parameterList.Add(GetParameter("@populartags", req.populartags));
            parameterList.Add(GetParameter("@varietals", req.varietals));
            parameterList.Add(GetParameter("@notableFeatures", req.notableFeatures));
            parameterList.Add(GetParameter("@eventOrganizerName", req.eventOrganizerName));
            parameterList.Add(GetParameter("@excludeEventId", req.excludeEventId));
            parameterList.Add(GetParameter("@excludeWineryId", req.excludeWineryId));
            parameterList.Add(GetParameter("@excludeOrganizerName", req.excludeOrganizerName));
            parameterList.Add(GetParameter("@categoryId", req.categoryId));
            parameterList.Add(GetParameter("@status", req.status));
            parameterList.Add(GetParameter("@isPrivate", req.isPrivate));
            parameterList.Add(GetParameter("@searchTerm", req.searchTerm));
            parameterList.Add(GetParameter("@isUniqueRecords", req.isUniqueRecords));
            parameterList.Add(GetParameter("@userID", req.userID));
            parameterList.Add(GetParameter("@isPastEvents", req.isPastEvents));
            parameterList.Add(GetParameter("@arrivalDate", req.arrivalDate));
            parameterList.Add(GetParameter("@departureDate", req.departureDate));
            //parameterList.Add(GetParameter("@regionIds", req.regionIds));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new UpcomingEventModel();
                        model.event_id = Convert.ToInt32(dataReader["EventId"]);
                        model.event_title = Convert.ToString(dataReader["eventTitle"]);
                        model.organizer_name = Convert.ToString(dataReader["organizerName"]);
                        model.start_date_time = Convert.ToDateTime(dataReader["startDateTime"]);
                        model.end_date_time = Convert.ToDateTime(dataReader["endDateTime"]);
                        model.primary_category = Convert.ToString(dataReader["primaryCategory"]);
                        model.secondary_category = Convert.ToString(dataReader["secondaryCategory"]);
                        model.event_listing_URL = Convert.ToString(dataReader["EventImage"]);
                        model.event_banner_URL = Convert.ToString(dataReader["EventImageBig"]);

                        model.event_listing_url_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        model.event_banner_url_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));

                        model.venue_country = Convert.ToString(dataReader["venueCounty"]);
                        model.venue_city = Convert.ToString(dataReader["venueCity"]);
                        model.venue_state = Convert.ToString(dataReader["venueState"]);
                        model.state_name = Convert.ToString(dataReader["stateName"]);
                        model.tickets_sold = Convert.ToInt32(dataReader["ticketsSold"]);
                        model.max_capacity = Convert.ToInt32(dataReader["maxCapacity"]);
                        model.member_id = Convert.ToInt32(dataReader["offerId"]);
                        model.member_name = Convert.ToString(dataReader["offerWineryName"]);
                        model.time_zone = (Times.TimeZone)Convert.ToInt32(dataReader["timeZone"]);
                        model.time_zone_name = Convert.ToString(dataReader["TimeZoneAbbr"]);
                        model.sold_out = Convert.ToBoolean(dataReader["soldOut"]);
                        model.is_favorites = Convert.ToBoolean(dataReader["IsFavorites"]);
                        model.requires_invite = Convert.ToBoolean(dataReader["RequiresInvite"]);
                        model.friendly_url = GetFriendlyURL(model.event_title, model.event_id);

                        model.event_organizer_url = Convert.ToString(dataReader["WebsiteURL"]);

                        string Regions = Convert.ToString(dataReader["Regions"]);
                        if (Regions.Length > 0)
                        {
                            Regions = "[" + Regions + "]";
                            List<int> regions_ids = JsonConvert.DeserializeObject<List<int>>(Regions);

                            foreach (var item in regions_ids)
                            {
                                model.regions_id = item;
                            }
                        }
                        else
                        {
                            model.regions_id = Convert.ToInt32(dataReader["Appelation"]);
                        }

                        model.address1 = Convert.ToString(dataReader["VenueAddress1"]);
                        model.short_description = Convert.ToString(dataReader["ShortDesc"]);

                        if (model.start_date_time.Date == model.end_date_time.Date)
                            model.is_single_date = true;

                        model.end_date_date = model.end_date_time.Day.ToString();
                        model.start_date_date = model.start_date_time.Day.ToString();

                        model.event_date = model.start_date_time.ToString("dddd, MMM dd, hh:mm tt");

                        model.start_date_month = model.start_date_time.ToString("MMMM");
                        model.end_date_month = model.end_date_time.ToString("MMMM");

                        model.enable_map = Convert.ToBoolean(dataReader["EnableMap"]);

                        if (dataReader["LastTicketSalesEndIn"] != DBNull.Value)
                        {
                            try
                            {
                                model.last_ticket_sales_end_in = Convert.ToDateTime(dataReader["LastTicketSalesEndIn"]);
                            }
                            catch { }
                        }

                        model.region_name = Convert.ToString(dataReader["FriendlyName"]);
                        model.member_url = Convert.ToString(dataReader["PurchaseURL"]);

                        model.min_price = Convert.ToDecimal(dataReader["MinPrice"]);
                        model.max_price = Convert.ToDecimal(dataReader["MaxPrice"]);
                        model.total_events = Convert.ToInt32(dataReader["TotalEvents"]);
                        bool valid_sale = false;

                        if (req.valid_sale != null && req.valid_sale == true)
                            valid_sale = true;

                        if (valid_sale && model.last_ticket_sales_end_in != null)
                        {
                            DateTime currentEventDate = Times.ToTimeZoneTime(DateTime.UtcNow, model.time_zone);

                            if (model.last_ticket_sales_end_in > currentEventDate)
                                upcomingEvents.Add(model);
                        }
                        else
                            upcomingEvents.Add(model);
                    }
                }
            }
            return upcomingEvents;
        }

        public bool ShowUpcomingEvents(int member_id)
        {
            bool UpcomingEvents = false;

            string sp = "ShowUpcomingEventsByWineryId";

            var eventS = new List<TicketEventModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", member_id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        UpcomingEvents = true;
                    }
                }
            }

            return UpcomingEvents;
        }

        public PassportEventDetailModel GetPassportEventDetailsById(int event_id)
        {
            string sp = "GetTicketEventDetailsById";
            var listQuestions = new List<TicketQuestion>();
            var tEvent = new PassportEventDetailModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", event_id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        tEvent.id = Convert.ToInt32(dataReader["Id"]);
                        tEvent.status = (uc.Common.TicketsEventStatus)Convert.ToInt32(dataReader["Status"]);
                        tEvent.name = Convert.ToString(dataReader["EventTitle"]);
                        tEvent.short_description = Convert.ToString(dataReader["ShortDesc"]);
                        tEvent.long_description = Convert.ToString(dataReader["LongDesc"]);
                        tEvent.member_name = Convert.ToString(dataReader["WineryName"]);
                        tEvent.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        tEvent.start_date = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tEvent.end_date = Convert.ToDateTime(dataReader["EndDateTime"]);
                        tEvent.checkin_type = (uc.Common.AttendeeAppCheckInMode)Convert.ToInt32(dataReader["AttendeeAppCheckInMode"]);
                        //tEvent.location_id = Convert.ToInt32(dataReader["VenueLocationId"]);
                        tEvent.venue_address_1 = Convert.ToString(dataReader["VenueAddress1"]);
                        tEvent.venue_address_2 = Convert.ToString(dataReader["VenueAddress2"]);

                        string Regions = Convert.ToString(dataReader["Regions"]);
                        if (Regions.Length > 0)
                        {
                            Regions = "[" + Regions + "]";
                            tEvent.regions_ids = JsonConvert.DeserializeObject<List<int>>(Regions);
                        }
                        string tags = Convert.ToString(dataReader["Tags"]);
                        if (tags.Length > 0)
                        {
                            tEvent.tags = tags.Split(',').ToList();
                        }

                        tEvent.primary_category = Convert.ToInt32(dataReader["Category"]);
                        tEvent.secondary_category = Convert.ToInt32(dataReader["Category2"]);

                        tEvent.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        tEvent.venue_country = Convert.ToString(dataReader["VenueCounty"]);
                        tEvent.venue_latitude = Convert.ToString(dataReader["VenueLatitude"]);
                        tEvent.venue_longitude = Convert.ToString(dataReader["VenueLongitude"]);
                        tEvent.venue_state = Convert.ToString(dataReader["VenueState"]);
                        tEvent.venue_zip = Convert.ToString(dataReader["VenueZip"]);
                        tEvent.timezone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        tEvent.timezone_name = GetTimezoneNameById(tEvent.timezone);
                        tEvent.timezone_offset = Times.GetOffsetMinutes(tEvent.timezone);
                        tEvent.checkin_allowed = CheckInAllowed(tEvent.start_date, tEvent.timezone);
                        tEvent.event_type = Convert.ToInt32(dataReader["event_type"]);
                        tEvent.total_tickets_sold = Convert.ToInt32(dataReader["total_tickets_sold"]);
                        tEvent.event_capacity = Convert.ToInt32(dataReader["event_capacity"]);
                        tEvent.show_book_rsvp_btn = Convert.ToBoolean(dataReader["DisplayBookRSVPBtn"]);
                        tEvent.show_availability_btn = Convert.ToBoolean(dataReader["DisableCheckAvailability"]);
                        tEvent.disable_activate_button = Convert.ToBoolean(dataReader["DisableActivateButton"]);
                        tEvent.requires_invite = Convert.ToBoolean(dataReader["RequiresInvite"]);
                        tEvent.requires_password = Convert.ToBoolean(dataReader["RequiresPassword"]);
                        tEvent.is_private = Convert.ToBoolean(dataReader["isPrivate"]);
                        tEvent.show_discount_code = Convert.ToBoolean(dataReader["ShowDiscountCode"]);
                        tEvent.is_automated_discounts = Convert.ToBoolean(dataReader["ShowAutomatedDiscount"]);
                        tEvent.show_access_code = Convert.ToBoolean(dataReader["ShowAccessCode"]);
                        tEvent.show_guest_list = Convert.ToBoolean(dataReader["ShowGuestList"]);
                        tEvent.guest_lists = Convert.ToString(dataReader["Guestlists"]);
                        tEvent.sold_out = Convert.ToBoolean(dataReader["SoldOut"]);
                        tEvent.waitlist_available = Convert.ToBoolean(dataReader["WaitlistAvl"]);
                        tEvent.ticket_cancel_policy = Convert.ToString(dataReader["TicketCancelPolicy"]);
                        tEvent.available_tickets = Convert.ToBoolean(dataReader["AvailableTickets"]);
                        string EnabledCreditCards = Convert.ToString(dataReader["EnabledCreditCards"]);
                        tEvent.accepted_card_types = EnabledCreditCards.Replace("32", "20").Split(',').Select(Int32.Parse).ToList();

                        tEvent.min_price = Convert.ToDecimal(dataReader["MinPrice"]);
                        tEvent.max_price = Convert.ToDecimal(dataReader["MaxPrice"]);

                        tEvent.rsvp_booking_short_desc = Convert.ToString(dataReader["RsvpBookingShortDesc"]);
                        tEvent.rsvp_booking_long_desc = Convert.ToString(dataReader["RsvpBookingLongDesc"]);
                        tEvent.block_repeat_bookings = Convert.ToBoolean(dataReader["BlockRepeatBookings"]);

                        tEvent.limit_bookings_per_day = Convert.ToInt32(dataReader["LimitBookingsPerDay"]);
                        tEvent.is_cp_payment_gateway = Convert.ToBoolean(dataReader["CPPaymentGateway"]);
                        if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.standardSingle)
                            tEvent.checkin_type_description = "Guest List Mode, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.barcodeSingle)
                            tEvent.checkin_type_description = "Barcode Only, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.barcodeMulti)
                            tEvent.checkin_type_description = "Barcode Only, Multi Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.searchSingle)
                            tEvent.checkin_type_description = "Search Mode, Single Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.searchMulti)
                            tEvent.checkin_type_description = "Search Mode, Multi Check-In";
                        else if (tEvent.checkin_type == uc.Common.AttendeeAppCheckInMode.multiEvent)
                            tEvent.checkin_type_description = "Multi-Event Check-In";

                        tEvent.status_description = Enum.GetName(typeof(uc.Common.TicketsEventStatus), tEvent.status);

                        tEvent.membership_start_date = Convert.ToDateTime(dataReader["MembershipStartDate"]);
                        tEvent.membership_end_date = Convert.ToDateTime(dataReader["MembershipExpiresDate"]);
                        tEvent.require_hdyh = Convert.ToBoolean(dataReader["ReqHDYH"]);
                        tEvent.show_will_call_adress = Convert.ToBoolean(dataReader["ShowWillCallAddress"]);
                        tEvent.service_fee_option = (Common.Common.TicketsServiceFeesOption)Convert.ToInt32(dataReader["ServiceFeesOption"]);
                        tEvent.attendance_mode = (Common.Common.AttendanceModeStatus)Convert.ToInt32(dataReader["AttendanceMode"]);
                        tEvent.purchase_timeout = Convert.ToInt32(dataReader["PurchaseTimeout"]);
                        tEvent.waitlist_expiration = Convert.ToInt32(dataReader["WaitlistExpiration"]);
                        tEvent.email_template_id = Convert.ToInt32(dataReader["EmailReceiptTemplate"]);
                        tEvent.purchase_policy_id = Convert.ToInt32(dataReader["TicketPurchasePolicy"]);

                        tEvent.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        tEvent.event_image_big = Convert.ToString(dataReader["EventImageBig"]);

                        tEvent.event_image_big_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        tEvent.event_url = Convert.ToString(dataReader["EventURL"]);
                        tEvent.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        tEvent.region_name = Convert.ToString(dataReader["RegionName"]);
                        tEvent.state = Convert.ToString(dataReader["VenueState"]);
                        if (tEvent.end_date < Times.ToTimeZoneTime(DateTime.UtcNow, tEvent.timezone) || tEvent.status == TicketsEventStatus.CANCELLED || tEvent.status == TicketsEventStatus.ENDED)
                            tEvent.event_ended = true;

                        //tEvent.locations = GetLocationByTicketEventID(tEvent.id, tEvent.location_id);
                        tEvent.will_call_locations = GetWillLocationsByTicketEventID(tEvent.id);
                        tEvent.passport_members = GetPassportWineryMembers(tEvent.id);
                        tEvent.city_url = Convert.ToString(dataReader["VenueCity"]);
                        tEvent.show_organizer_phone = Convert.ToBoolean(dataReader["DisplayPhone"]);
                        tEvent.show_map = Convert.ToBoolean(dataReader["DisplayMap"]);
                        tEvent.refund_policy = (uc.Common.TicketRefundPolicy)Convert.ToInt32(dataReader["TicketRefundPolicy"]);
                        tEvent.refund_policy_text = tEvent.refund_policy.GetEnumDescription();
                        tEvent.refund_service_fees_option = (uc.Common.RefundServiceFeesOption)Convert.ToInt32(dataReader["RefundServiceFeesOption"]);
                        tEvent.refund_service_fees_option_text = tEvent.refund_service_fees_option.GetEnumDescription();
                        tEvent.collect_tax = Convert.ToBoolean(dataReader["ChargeTax"]);

                        tEvent.order_special_instructions = Convert.ToString(dataReader["OrderSpecialInstructions"]);
                        tEvent.confirmation_page = Convert.ToString(dataReader["ConfirmationPage"]);
                        tEvent.business_message = Convert.ToString(dataReader["ConfirmationEmail"]);
                        tEvent.require_reservations = Convert.ToBoolean(dataReader["DisplayRsvpBookingOnEventPage"]);
                        tEvent.disable_travel_time_restrictions = Convert.ToBoolean(dataReader["DisableTravelTimeRestrictions"]);

                        if (!string.IsNullOrWhiteSpace(dataReader["InternalNotificationRecipient"].ToString()))
                        {
                            try
                            {
                                List<int> internalNotificationRecipient = dataReader["InternalNotificationRecipient"].ToString().Split(',').Select(int.Parse).ToList();
                                tEvent.internal_notification_recipient = internalNotificationRecipient;
                            }
                            catch { }
                        }

                        if (!string.IsNullOrWhiteSpace(dataReader["Tax_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["Tax_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                tEvent.tax_ticketlevels = ticketLevels;
                            }
                            catch { }
                        }

                        //buyr flags
                        TicketHolderConfig thConfig = new TicketHolderConfig
                        {

                            require_address = Convert.ToBoolean(dataReader["TH_ReqAddress"]),
                            require_mem_num = Convert.ToBoolean(dataReader["ReqMembershipNum"]),
                            require_dob = Convert.ToBoolean(dataReader["TH_ReqDOB"]),
                            require_website = Convert.ToBoolean(dataReader["TH_ReqWebsite"]),
                            require_age = Convert.ToBoolean(dataReader["TH_ReqAge"]),

                            show_firstname = Convert.ToBoolean(dataReader["TH_ShowFirstName"]),
                            require_firstname = Convert.ToBoolean(dataReader["TH_ReqFirstName"]),

                            show_zipcode = Convert.ToBoolean(dataReader["TH_ShowZipCode"]),
                            require_zipcode = Convert.ToBoolean(dataReader["TH_ReqZipCode"]),

                            show_lastname = Convert.ToBoolean(dataReader["TH_ShowLastName"]),
                            require_lastname = Convert.ToBoolean(dataReader["TH_ReqLastName"]),


                            show_gender = Convert.ToBoolean(dataReader["TH_ShowGender"]),
                            require_gender = Convert.ToBoolean(dataReader["TH_ReqGender"]),

                            show_age_group = Convert.ToBoolean(dataReader["AgeGroup"]),
                            require_age_group = Convert.ToBoolean(dataReader["AgeGroup"]),

                            show_company = Convert.ToBoolean(dataReader["TH_ShowCompany"]),
                            require_company = Convert.ToBoolean(dataReader["TH_ReqCompany"]),

                            show_email = Convert.ToBoolean(dataReader["TH_ShowEmail"]),
                            require_email = Convert.ToBoolean(dataReader["TH_ReqEmail"]),
                            unique_email = Convert.ToBoolean(dataReader["TH_UniqueEmail"]),

                            show_title = Convert.ToBoolean(dataReader["TH_ShowTitle"]),
                            require_title = Convert.ToBoolean(dataReader["TH_ReqTitle"]),

                            show_workphone = Convert.ToBoolean(dataReader["TH_ShowWorkPhone"]),
                            require_workphone = Convert.ToBoolean(dataReader["TH_ReqWorkPhone"]),

                            show_mobilephone = Convert.ToBoolean(dataReader["TH_ShowMobilePhone"]),
                            require_mobilephone = Convert.ToBoolean(dataReader["TH_ReqMobilePhone"]),
                        };

                        if (!string.IsNullOrWhiteSpace(dataReader["TH_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["TH_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                thConfig.ticket_levels = ticketLevels;
                            }
                            catch { }
                        }
                        tEvent.ticket_holder_config = thConfig;


                        TicketPostCaptureConfig pcConfig = new TicketPostCaptureConfig();

                        var parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_FirstName"]));
                        pcConfig.require_firstname = parser.require_flag;
                        pcConfig.show_firstname = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_LastName"]));
                        pcConfig.require_lastname = parser.require_flag;
                        pcConfig.show_lastname = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Company"]));
                        pcConfig.require_company = parser.require_flag;
                        pcConfig.show_company = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Email"]));
                        pcConfig.require_email = parser.require_flag;
                        pcConfig.show_email = parser.show_flag;

                        pcConfig.unique_email = Convert.ToBoolean(dataReader["PC_UniqueEmail"]);

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Title"]));
                        pcConfig.require_title = parser.require_flag;
                        pcConfig.show_title = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Gender"]));
                        pcConfig.require_gender = parser.require_flag;
                        pcConfig.show_gender = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Age"]));
                        pcConfig.require_age = parser.require_flag;
                        //pcConfig.show_age_group = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Website"]));
                        pcConfig.require_website = parser.require_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_WorkPhone"]));
                        pcConfig.require_workphone = parser.require_flag;
                        pcConfig.show_workphone = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_MobilePhone"]));
                        pcConfig.require_mobilephone = parser.require_flag;
                        pcConfig.show_mobilephone = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_DOB"]));
                        pcConfig.require_dob = parser.require_flag;
                        pcConfig.show_dob = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_Address"]));
                        pcConfig.require_address = parser.require_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_AgeGroup"]));
                        pcConfig.require_age_group = parser.require_flag;
                        pcConfig.show_age_group = parser.show_flag;

                        parser = ParseShowRequiredFlags(Convert.ToInt32(dataReader["PC_ZipCode"]));
                        pcConfig.require_zipcode = parser.require_flag;
                        pcConfig.show_zipcode = parser.show_flag;

                        if (!string.IsNullOrWhiteSpace(dataReader["PC_TicketLevels"].ToString()))
                        {
                            try
                            {
                                List<int> ticketLevels = dataReader["PC_TicketLevels"].ToString().Split(',').Select(int.Parse).ToList();
                                pcConfig.ticket_levels = ticketLevels;
                            }
                            catch { }
                        }
                        tEvent.post_capture_config = pcConfig;

                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                var ticketQuestion = new TicketQuestion();
                                List<string> lstChoices = null;
                                var choices = Convert.ToString(dataReader["Choices"]);
                                if (!string.IsNullOrEmpty(choices))
                                {
                                    lstChoices = choices.Split("@!@").ToList();
                                }
                                ticketQuestion.question_id = Convert.ToInt32(dataReader["Id"]);
                                ticketQuestion.question_type = Convert.ToString(dataReader["QuestionType"]);
                                ticketQuestion.question_text = Convert.ToString(dataReader["QuestionText"]);
                                ticketQuestion.is_required = Convert.ToBoolean(dataReader["IsRequired"]);
                                ticketQuestion.is_default_state = Convert.ToBoolean(dataReader["DefaultState"]);
                                ticketQuestion.choices = lstChoices;
                                listQuestions.Add(ticketQuestion);
                            }
                        }
                    }
                }

            }
            tEvent.attendee_questions = listQuestions;
            return tEvent;
        }
        private PostCaptureParser ParseShowRequiredFlags(int value)
        {
            return new PostCaptureParser
            {
                require_flag = (value == 2),
                show_flag = (value == 1)
            };
        }
        public List<TicketModel> GetTicketsByEventId(int EventId)
        {
            string sp = "GetTicketsByEventId";

            var tictModel = new List<TicketModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tict = new TicketModel();
                        tict.ticket_id = Convert.ToInt32(dataReader["TicketId"]);
                        tict.ticket_number = tict.ticket_id.ToString().PadLeft(8, '0');
                        tict.ticket_barcode = GetBarcode(Convert.ToInt32(dataReader["Winery_Id"]), Convert.ToInt32(dataReader["OrderId"]), Convert.ToInt32(dataReader["TicketId"]));
                        tict.ticket_level_id = Convert.ToInt32(dataReader["Tickets_Ticket_Id"]);
                        tict.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        tict.ticket_price = Convert.ToDecimal(dataReader["TicketPrice"]);
                        tict.ticket_status = (uc.Common.TicketStatus)Convert.ToInt32(dataReader["Status"]);
                        tict.ticket_post_capture_status = (uc.Common.TicketPostCaptureStatus)Convert.ToInt32(dataReader["PostCaptureStatus"]);
                        tict.ticket_status_description = Enum.GetName(typeof(uc.Common.TicketStatus), tict.ticket_status);
                        tict.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        tict.valid_end_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        tict.event_id = EventId;
                        tict.event_title = Convert.ToString(dataReader["EventTitle"]);
                        tict.user_id = Convert.ToInt32(dataReader["User_Id"]);

                        bool ticketIsValidForDate = false;

                        Times.TimeZone eventTimeZone = GetEventTimeZone(EventId);

                        //Set Event Time
                        DateTime currentEventDate = Times.ToTimeZoneTime(DateTime.UtcNow, eventTimeZone);

                        //Make Sure Date on Ticket is Valid
                        if (currentEventDate.Date >= tict.valid_start_date.Date && currentEventDate.Date <= tict.valid_end_date.Date)
                            ticketIsValidForDate = true;

                        if (ticketIsValidForDate == true)
                            tict.checkin_status = uc.Common.CheckInStatus.NA;
                        else
                            tict.checkin_status = uc.Common.CheckInStatus.NOT_ALLOWED_BAD_DATE;

                        if (tict.ticket_status == uc.Common.TicketStatus.CLAIMED)
                        {
                            tict.checkin_status = uc.Common.CheckInStatus.SUCCESS;
                        }
                        else if (tict.ticket_status == uc.Common.TicketStatus.INVALID)
                        {
                            tict.checkin_status = uc.Common.CheckInStatus.FAILED;
                        }

                        var ticketHolder = new TicketHolder();
                        ticketHolder.first_name = Convert.ToString(dataReader["TicketHolderFirstName"]);
                        ticketHolder.last_name = Convert.ToString(dataReader["TicketHolderLastName"]);
                        ticketHolder.email = Convert.ToString(dataReader["TicketHolderEmail"]);
                        ticketHolder.company = Convert.ToString(dataReader["TicketHolderCompany"]);
                        ticketHolder.title = Convert.ToString(dataReader["TicketHolderTitle"]);
                        ticketHolder.gender = Convert.ToString(dataReader["TicketHolderGender"]);
                        ticketHolder.age = Convert.ToInt32(dataReader["TicketHolderAge"]);
                        ticketHolder.age_group = Convert.ToString(dataReader["TicketHolderAgeGroup"]);
                        ticketHolder.website = Convert.ToString(dataReader["TicketHolderWebsite"]);
                        ticketHolder.work_phone = Convert.ToString(dataReader["TicketHolderWorkPhone"]);
                        ticketHolder.mobile_phone = Convert.ToString(dataReader["TicketHolderMobilePhone"]);
                        ticketHolder.dob = Convert.ToString(dataReader["TicketHolderBirthDate"]);
                        ticketHolder.country = Convert.ToString(dataReader["TicketHolderCountry"]);
                        ticketHolder.address1 = Convert.ToString(dataReader["TicketHolderAddress1"]);
                        ticketHolder.address2 = Convert.ToString(dataReader["TicketHolderAddress2"]);
                        ticketHolder.city = Convert.ToString(dataReader["TicketHolderCity"]);
                        ticketHolder.state = Convert.ToString(dataReader["TicketHolderState"]);
                        ticketHolder.postal_code = Convert.ToString(dataReader["TicketHolderZipCode"]);
                        ticketHolder.will_call_location = Convert.ToString(dataReader["TicketHolderWCLocationName"]);
                        ticketHolder.will_call_location_id = Convert.ToString(dataReader["TicketHolderWCLocationId"]);
                        ticketHolder.delivery_type = Convert.ToInt32(dataReader["DeliveryType"]);

                        tict.ticket_holder = ticketHolder;

                        if (Convert.ToInt32(dataReader["lastCheckInId"]) > 0)
                        {
                            var ticketCheckIn = new TicketCheckIn();

                            ticketCheckIn.id = Convert.ToInt32(dataReader["lastCheckInId"]);
                            //ticketCheckIn.location_id = Convert.ToInt32(dataReader["Location_Id"]);
                            //ticketCheckIn.location_name = Convert.ToString(dataReader["LocationName"]);
                            ticketCheckIn.geo_latitude = Convert.ToString(dataReader["GeoLatitude"]);
                            ticketCheckIn.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                            ticketCheckIn.checkin_date = GetOffsetDate(Convert.ToDateTime(dataReader["CheckInDateTime"]), Convert.ToInt32(dataReader["offsetTime"])).ToString();

                            tict.previous_checkin = ticketCheckIn;
                        }
                        tictModel.Add(tict);
                    }
                }
            }
            return tictModel;
        }

        public List<TicketModel> GetTicketsByPassportEventIdAndMember(int eventId, int memberId)
        {
            string sp = "GetPassportEventTicketsByMember";

            var tictModel = new List<TicketModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", eventId));
            parameterList.Add(GetParameter("@MemberId", memberId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tict = new TicketModel();
                        tict.ticket_id = Convert.ToInt32(dataReader["TicketId"]);
                        tict.ticket_number = tict.ticket_id.ToString().PadLeft(8, '0');
                        tict.ticket_barcode = GetBarcode(Convert.ToInt32(dataReader["Winery_Id"]), Convert.ToInt32(dataReader["OrderId"]), Convert.ToInt32(dataReader["TicketId"]));
                        tict.ticket_level_id = Convert.ToInt32(dataReader["Tickets_Ticket_Id"]);
                        tict.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        tict.ticket_price = Convert.ToDecimal(dataReader["TicketPrice"]);
                        tict.ticket_status = (uc.Common.TicketStatus)Convert.ToInt32(dataReader["Status"]);
                        tict.ticket_post_capture_status = (uc.Common.TicketPostCaptureStatus)Convert.ToInt32(dataReader["PostCaptureStatus"]);
                        tict.attendance_mode = (uc.Common.AttendanceModeStatus)Convert.ToInt32(dataReader["AttendanceMode"]);
                        tict.ticket_status_description = Enum.GetName(typeof(uc.Common.TicketStatus), tict.ticket_status);
                        tict.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        tict.valid_end_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        tict.event_id = eventId;
                        tict.event_title = Convert.ToString(dataReader["EventTitle"]);
                        tict.user_id = Convert.ToInt32(dataReader["User_Id"]);

                        bool ticketIsValidForDate = false;

                        Times.TimeZone eventTimeZone = GetEventTimeZone(eventId);

                        //Set Event Time
                        DateTime currentEventDate = Times.ToTimeZoneTime(DateTime.UtcNow, eventTimeZone);

                        //Make Sure Date on Ticket is Valid
                        if (currentEventDate.Date >= tict.valid_start_date.Date && currentEventDate.Date <= tict.valid_end_date.Date)
                            ticketIsValidForDate = true;

                        if (ticketIsValidForDate == true)
                            tict.checkin_status = uc.Common.CheckInStatus.NA;
                        else
                            tict.checkin_status = uc.Common.CheckInStatus.NOT_ALLOWED_BAD_DATE;

                        if (tict.ticket_status == uc.Common.TicketStatus.CLAIMED)
                        {
                            tict.checkin_status = uc.Common.CheckInStatus.SUCCESS;
                        }
                        else if (tict.ticket_status == uc.Common.TicketStatus.INVALID)
                        {
                            tict.checkin_status = uc.Common.CheckInStatus.FAILED;
                        }

                        var ticketHolder = new TicketHolder();
                        ticketHolder.first_name = Convert.ToString(dataReader["TicketHolderFirstName"]);
                        ticketHolder.last_name = Convert.ToString(dataReader["TicketHolderLastName"]);
                        ticketHolder.email = Convert.ToString(dataReader["TicketHolderEmail"]);
                        ticketHolder.company = Convert.ToString(dataReader["TicketHolderCompany"]);
                        ticketHolder.title = Convert.ToString(dataReader["TicketHolderTitle"]);
                        ticketHolder.gender = Convert.ToString(dataReader["TicketHolderGender"]);
                        ticketHolder.age = Convert.ToInt32(dataReader["TicketHolderAge"]);
                        ticketHolder.age_group = Convert.ToString(dataReader["TicketHolderAgeGroup"]);
                        ticketHolder.website = Convert.ToString(dataReader["TicketHolderWebsite"]);
                        ticketHolder.work_phone = Convert.ToString(dataReader["TicketHolderWorkPhone"]);
                        ticketHolder.mobile_phone = Convert.ToString(dataReader["TicketHolderMobilePhone"]);
                        ticketHolder.dob = Convert.ToString(dataReader["TicketHolderBirthDate"]);
                        ticketHolder.country = Convert.ToString(dataReader["TicketHolderCountry"]);
                        ticketHolder.address1 = Convert.ToString(dataReader["TicketHolderAddress1"]);
                        ticketHolder.address2 = Convert.ToString(dataReader["TicketHolderAddress2"]);
                        ticketHolder.city = Convert.ToString(dataReader["TicketHolderCity"]);
                        ticketHolder.state = Convert.ToString(dataReader["TicketHolderState"]);
                        ticketHolder.postal_code = Convert.ToString(dataReader["TicketHolderZipCode"]);
                        ticketHolder.will_call_location = Convert.ToString(dataReader["TicketHolderWCLocationName"]);
                        ticketHolder.will_call_location_id = Convert.ToString(dataReader["TicketHolderWCLocationId"]);
                        ticketHolder.delivery_type = Convert.ToInt32(dataReader["DeliveryType"]);

                        tict.ticket_holder = ticketHolder;

                        if (Convert.ToInt32(dataReader["lastCheckInId"]) > 0)
                        {
                            var ticketCheckIn = new TicketCheckIn();

                            ticketCheckIn.id = Convert.ToInt32(dataReader["lastCheckInId"]);
                            //ticketCheckIn.location_id = Convert.ToInt32(dataReader["Location_Id"]);
                            //ticketCheckIn.location_name = Convert.ToString(dataReader["LocationName"]);
                            ticketCheckIn.geo_latitude = Convert.ToString(dataReader["GeoLatitude"]);
                            ticketCheckIn.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                            ticketCheckIn.checkin_date = GetOffsetDate(Convert.ToDateTime(dataReader["CheckInDateTime"]), Convert.ToInt32(dataReader["offsetTime"])).ToString();

                            tict.previous_checkin = ticketCheckIn;
                        }
                        tictModel.Add(tict);
                    }
                }
            }
            return tictModel;
        }

        public List<TicketLevelModel> GetTicketLevelsByEventId(int eventId, bool IsAdmin, string accessCode, ref int EventRemainingQty)
        {
            string sp = "Tickets_GetTicketLevelsWithSoldCountByEvent";

            var tictModel = new List<TicketLevelModel>();
            var parameterList = new List<DbParameter>();


            int SaleStatus = (int)TicketsSaleStatus.Any;

            if (!IsAdmin)
                SaleStatus = (int)TicketsSaleStatus.OnSale;

            parameterList.Add(GetParameter("@EventId", eventId));
            parameterList.Add(GetParameter("@SaleStatus", SaleStatus));
            parameterList.Add(GetParameter("@AccessCode", accessCode));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tict = new TicketLevelModel();
                        tict.ticket_id = Convert.ToInt32(dataReader["Id"]);
                        tict.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        tict.ticket_name_on_badge = Convert.ToString(dataReader["TicketNameOnBadge"]);
                        tict.price = Convert.ToDecimal(dataReader["Price"]);
                        tict.original_price = Convert.ToDecimal(dataReader["Price"]);
                        tict.ticket_desc = Convert.ToString(dataReader["TicketDesc"]);
                        tict.tickets_event_id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                        tict.sale_status = (uc.Common.TicketsSaleStatus)Convert.ToInt32(dataReader["SaleStatus"]);
                        tict.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        tict.valid_end_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        tict.date_time_available = Convert.ToDateTime(dataReader["DateTimeAvailable"]);
                        tict.date_time_unavailable = Convert.ToDateTime(dataReader["DateTimeUnavailable"]);
                        tict.max_available = Convert.ToInt32(dataReader["MaxAvailable"]);
                        tict.min_per_order = Convert.ToInt32(dataReader["MinPerOrder"]);
                        tict.max_per_order = Convert.ToInt32(dataReader["MaxPerOrder"]);
                        tict.show_remaining = Convert.ToInt32(dataReader["ShowRemaining"]);
                        tict.waitlist_enabled = Convert.ToBoolean(dataReader["WaitlistEnabled"]);
                        tict.waitlist_limit_enabled = Convert.ToBoolean(dataReader["WaitlistLimitEnabled"]);
                        tict.waitlist_limit = Convert.ToInt32(dataReader["WaitlistLimit"]);
                        tict.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                        tict.ticket_type = (uc.Common.TicketType)Convert.ToInt32(dataReader["TicketType"]);
                        //tict.charge_tax = Convert.ToBoolean(dataReader["ChargeTax"]);
                        tict.self_print_enabled = Convert.ToBoolean(dataReader["SelfPrint"]);
                        tict.will_call_enabled = Convert.ToBoolean(dataReader["WillCall"]);
                        tict.shipped_enabled = Convert.ToBoolean(dataReader["Shippable"]);
                        tict.gratuity_percentage = Convert.ToDecimal(dataReader["GratuityPercentage"]);
                        tict.tax_gratuity = Convert.ToBoolean(dataReader["TaxGratuity"]);
                        tict.time_zone_id = (uc.Common.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        tict.qty_sold = Convert.ToInt32(dataReader["SoldCount"]);
                        tict.waitlist_qty_sold = Convert.ToInt32(dataReader["WaitlistSoldCount"]);

                        tict.ticket_event_date = Convert.ToDateTime(dataReader["TicketEventDate"]);
                        tict.ticket_fee = Convert.ToDecimal(dataReader["TicketFee"]);
                        tict.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        tict.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        tict.remaining_qty = Convert.ToInt32(dataReader["QtyAvailable"]);
                        tict.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        EventRemainingQty = Convert.ToInt32(dataReader["EventRemainingQty"]);
                        tict.rsvp_access_code = Convert.ToString(dataReader["RSVPAccessCode"]);
                        tict.max_tickets_per_order = Convert.ToInt32(dataReader["MaxTicketsPerOrder"]);
                        tict.is_private_event = Convert.ToBoolean(dataReader["isPrivate"]);
                        tict.fulfillment_lead_time = Convert.ToInt32(dataReader["FulfillmentLeadTime"]);
                        tict.remaining_qty_text = "";
                        if (tict.remaining_qty < 1 || tict.min_per_order > tict.remaining_qty)
                            tict.remaining_qty_text = "";
                        else if (tict.remaining_qty <= tict.show_remaining)
                            tict.remaining_qty_text = tict.remaining_qty + " remaining";

                        if (tict.remaining_qty <= 0)
                            tict.sale_status = TicketsSaleStatus.SoldOut;

                        bool showWaitlist = false;
                        if (tict.waitlist_enabled)
                        {
                            showWaitlist = true;

                            if (tict.waitlist_limit_enabled)
                            {
                                if (tict.waitlist_limit <= tict.waitlist_qty_sold)
                                {
                                    showWaitlist = false;
                                }
                            }
                        }

                        tict.show_waitlist = showWaitlist;

                        Times.TimeZone timeZone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);

                        tict.is_valid = CheckIfTicketStillValid(tict, timeZone);

                        DateTime currDateTime = Times.ToTimeZoneTime(DateTime.UtcNow, timeZone);

                        if (currDateTime < tict.date_time_available)
                            tict.sale_status = TicketsSaleStatus.NotStarted;
                        else if (currDateTime > tict.date_time_unavailable)
                            tict.sale_status = TicketsSaleStatus.Ended;

                        if (tict.will_call_enabled)
                        {
                            string ids = ",";
                            var list = new List<EventWillCallLocation>();

                            list = GetEventWillCallLocationByEventId(eventId);
                            foreach (var item in list)
                            {
                                if (item.available_qty <= 0)
                                    ids = ids + item.location_id.ToString() + ",";
                            }

                            tict.will_call_location_details = GetTicketsEventWillCallLocationByTicketId(eventId, tict.ticket_id, ids);
                            
                        }
                            

                        tictModel.Add(tict);
                    }
                }
            }

            var newtictModel = new List<TicketLevelModel>();
            if (tictModel != null && tictModel.Count > 0)
            {
                newtictModel = tictModel.Where(f => f.sale_status != TicketsSaleStatus.SoldOut).ToList();
                var tictSoldOut = tictModel.Where(f => f.sale_status == TicketsSaleStatus.SoldOut).ToList();
                newtictModel.AddRange(tictSoldOut);
            }


            return newtictModel;
        }

        public List<TicketsEventWillCallLocation> GetTicketsEventWillCallLocationByTicketId(int TicketEventId, int TicketId = 0,string skipIds = "")
        {
            var list = new List<TicketsEventWillCallLocation>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TicketEventId", TicketEventId));
            parameterList.Add(GetParameter("@TicketId", TicketId));

            using (DbDataReader dataReader = GetDataReader("GetTicketsEventWillCallLocationByTicketId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var item = new TicketsEventWillCallLocation();
                        item.ticket_id = Convert.ToInt32(dataReader["Tickets_Ticket_Id"]);
                        item.location_name = Convert.ToString(dataReader["LocationName"]);
                        item.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        item.will_call_limit = Convert.ToInt32(dataReader["WillCallLimit"]);
                        item.order_qty = Convert.ToInt32(dataReader["OrderQty"]);
                        item.available_qty = Convert.ToInt32(dataReader["AvlQty"]);

                        if (skipIds.IndexOf(item.location_id.ToString()) == -1)
                            list.Add(item);
                    }
                }
            }
            return list;
        }

        public List<EventWillCallLocation> GetEventWillCallLocationByEventId(int TicketEventId)
        {
            var list = new List<EventWillCallLocation>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TicketEventId", TicketEventId));

            using (DbDataReader dataReader = GetDataReader("GetTicketsEventWillCallLocationByEventId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var item = new EventWillCallLocation();
                        item.location_name = Convert.ToString(dataReader["LocationName"]);
                        item.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        item.will_call_limit = Convert.ToInt32(dataReader["WillCallLimit"]);
                        item.order_qty = Convert.ToInt32(dataReader["OrderQty"]);
                        item.available_qty = Convert.ToInt32(dataReader["AvlQty"]);

                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public bool CheckIfTicketStillValid(TicketLevelModel tict, Times.TimeZone timezone)
        {
            DateTime currDateTime = Times.ToTimeZoneTime(DateTime.UtcNow, timezone);
            bool ret = true;
            if (currDateTime < tict.date_time_available || currDateTime > tict.date_time_unavailable)
                ret = false;
            return ret;
        }

        public TicketDiscount GetTicketDiscountByCodeWithUseCount(string promocode, int eventId,int discountid,int discounttype)
        {
            string sp = "Tickets_GetDiscountByCodeWithUseCount";

            var ticketDiscount = new TicketDiscount();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@promoCode", promocode));
            parameterList.Add(GetParameter("@eventId", eventId));
            parameterList.Add(GetParameter("@discountid", discountid));
            parameterList.Add(GetParameter("@discounttype", discounttype));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        ticketDiscount.id = Convert.ToInt32(dataReader["Id"]);
                        ticketDiscount.ticket_event_id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                        ticketDiscount.discount_code = Convert.ToString(dataReader["DiscountCode"]);
                        ticketDiscount.discount_amount = Convert.ToDecimal(dataReader["DiscountAmount"]);
                        ticketDiscount.discount_percent = Convert.ToDecimal(dataReader["DiscountPercent"]);
                        ticketDiscount.number_of_uses = Convert.ToInt32(dataReader["NumberOfUses"]);

                        ticketDiscount.use_count = Convert.ToInt32(dataReader["NumberUsed"]);
                        ticketDiscount.start_datetime = Convert.ToDateTime(dataReader["StartDateTime"]);
                        ticketDiscount.end_datetime = Convert.ToDateTime(dataReader["EndDateTime"]);
                        ticketDiscount.min_qty_reqd = Convert.ToInt32(dataReader["RequiredMinimum"]);
                        ticketDiscount.max_per_order = Convert.ToInt32(dataReader["MaximumPerOrder"]);

                        ticketDiscount.discount_type = Convert.ToInt32(dataReader["DiscountType"]);
                        ticketDiscount.guest_type = Convert.ToInt32(dataReader["GuestType"]);
                        ticketDiscount.wineryId = Convert.ToInt32(dataReader["wineryId"]);
                        string[] assigned_lists = Convert.ToString(dataReader["AssignedLists"]).Split(',', StringSplitOptions.RemoveEmptyEntries);

                        List<int> assignedlistT = new List<int>();

                        foreach (string alist in assigned_lists)
                        {
                            assignedlistT.Add(Convert.ToInt32(alist));
                        }
                        ticketDiscount.assigned_lists = assignedlistT;

                        string[] levels = Convert.ToString(dataReader["TicketLevels"]).Split(',', StringSplitOptions.RemoveEmptyEntries);
                        List<DiscountTicketLevel> listT = new List<DiscountTicketLevel>();
                        foreach (string level in levels)
                        {
                            DiscountTicketLevel ticketLevel = new DiscountTicketLevel();

                            int ticketId = 0;
                            Int32.TryParse(level, out ticketId);
                            ticketLevel.ticket_id = ticketId;
                            listT.Add(ticketLevel);
                        }
                        ticketDiscount.discount_ticket_levels = listT;

                    }
                }
            }
            return ticketDiscount;
        }

        public List<TicketDiscount> GetTicketDiscountByEventId(int eventId)
        {
            string sp = "Tickets_GetDiscountByCodeWithUseCount";

            var list = new List<TicketDiscount>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@promoCode", ""));
            parameterList.Add(GetParameter("@eventId", eventId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var ticketDiscount = new TicketDiscount();
                        ticketDiscount.id = Convert.ToInt32(dataReader["Id"]);
                        ticketDiscount.ticket_event_id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                        ticketDiscount.discount_code = Convert.ToString(dataReader["DiscountCode"]);
                        ticketDiscount.discount_amount = Convert.ToDecimal(dataReader["DiscountAmount"]);
                        ticketDiscount.discount_percent = Convert.ToDecimal(dataReader["DiscountPercent"]);
                        ticketDiscount.number_of_uses = Convert.ToInt32(dataReader["NumberOfUses"]);

                        ticketDiscount.use_count = Convert.ToInt32(dataReader["NumberUsed"]);
                        ticketDiscount.start_datetime = Convert.ToDateTime(dataReader["StartDateTime"]);
                        ticketDiscount.end_datetime = Convert.ToDateTime(dataReader["EndDateTime"]);
                        ticketDiscount.min_qty_reqd = Convert.ToInt32(dataReader["RequiredMinimum"]);
                        ticketDiscount.max_per_order = Convert.ToInt32(dataReader["MaximumPerOrder"]);

                        ticketDiscount.discount_type = Convert.ToInt32(dataReader["DiscountType"]);
                        ticketDiscount.guest_type = Convert.ToInt32(dataReader["GuestType"]);
                        ticketDiscount.wineryId = Convert.ToInt32(dataReader["wineryId"]);
                        string[] assigned_lists = Convert.ToString(dataReader["AssignedLists"]).Split(',', StringSplitOptions.RemoveEmptyEntries);

                        List<int> assignedlistT = new List<int>();

                        foreach (string alist in assigned_lists)
                        {
                            assignedlistT.Add(Convert.ToInt32(alist));
                        }
                        ticketDiscount.assigned_lists = assignedlistT;

                        string[] levels = Convert.ToString(dataReader["TicketLevels"]).Split(',', StringSplitOptions.RemoveEmptyEntries);
                        List<DiscountTicketLevel> listT = new List<DiscountTicketLevel>();
                        foreach (string level in levels)
                        {
                            DiscountTicketLevel ticketLevel = new DiscountTicketLevel();

                            int ticketId = 0;
                            Int32.TryParse(level, out ticketId);
                            ticketLevel.ticket_id = ticketId;
                            listT.Add(ticketLevel);
                        }
                        ticketDiscount.discount_ticket_levels = listT;

                        list.Add(ticketDiscount);
                    }
                }
            }
            return list;
        }

        public TicketModel GetTicketById(int Id)
        {
            string sp = "GetTicketById";

            var tict = new TicketModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@TicketId", Id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        tict.ticket_id = Convert.ToInt32(dataReader["TicketId"]);
                        tict.ticket_order_id = Convert.ToInt32(dataReader["OrderId"]);
                        tict.ticket_number = tict.ticket_id.ToString().PadLeft(8, '0');
                        tict.ticket_barcode = GetBarcode(Convert.ToInt32(dataReader["Winery_Id"]), Convert.ToInt32(dataReader["OrderId"]), Convert.ToInt32(dataReader["TicketId"]));
                        tict.ticket_level_id = Convert.ToInt32(dataReader["Tickets_Ticket_Id"]);
                        tict.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        tict.ticket_price = Convert.ToDecimal(dataReader["TicketPrice"]);
                        tict.ticket_status = (uc.Common.TicketStatus)Convert.ToInt32(dataReader["Status"]);
                        tict.ticket_status_description = Enum.GetName(typeof(uc.Common.TicketStatus), tict.ticket_status);
                        tict.ticket_post_capture_status = (uc.Common.TicketPostCaptureStatus)Convert.ToInt32(dataReader["PostCaptureStatus"]);
                        tict.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        tict.valid_end_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        tict.event_id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                        tict.event_title = Convert.ToString(dataReader["EventTitle"]);
                        tict.user_id = Convert.ToInt32(dataReader["User_Id"]);
                        tict.checkin_status = uc.Common.CheckInStatus.NA;
                        //tict.checkin_status_desc = Enum.GetName(typeof(uc.Common.CheckInStatus), tict.checkin_status);

                        //if (tict.checkin_status == uc.Common.CheckInStatus.NA)
                        //    tict.checkin_status_message = "NA";
                        //else if (tict.checkin_status == uc.Common.CheckInStatus.SUCCESS)
                        //    tict.checkin_status_message = "Check-in Successful";
                        //else if (tict.checkin_status == uc.Common.CheckInStatus.NOT_ALLOWED)
                        //    tict.checkin_status_message = "Ticket already checked-in or not allowed";
                        //else if (tict.checkin_status == uc.Common.CheckInStatus.FAILED)
                        //    tict.checkin_status_message = "Error with Check-in";
                        //else if (tict.checkin_status == uc.Common.CheckInStatus.NOT_ALLOWED_BAD_EVENT)
                        //    tict.checkin_status_message = "Ticket not valid for this event";
                        //else if (tict.checkin_status == uc.Common.CheckInStatus.NOT_ALLOWED_BAD_DATE)
                        //    tict.checkin_status_message = "Ticket not valid on this date";

                        var ticketHolder = new TicketHolder();
                        ticketHolder.first_name = Convert.ToString(dataReader["TicketHolderFirstName"]);
                        ticketHolder.last_name = Convert.ToString(dataReader["TicketHolderLastName"]);
                        ticketHolder.email = Convert.ToString(dataReader["TicketHolderEmail"]);
                        ticketHolder.company = Convert.ToString(dataReader["TicketHolderCompany"]);
                        ticketHolder.title = Convert.ToString(dataReader["TicketHolderTitle"]);
                        ticketHolder.gender = Convert.ToString(dataReader["TicketHolderGender"]);
                        ticketHolder.age = Convert.ToInt32(dataReader["TicketHolderAge"]);
                        ticketHolder.age_group = Convert.ToString(dataReader["TicketHolderAgeGroup"]);
                        ticketHolder.website = Convert.ToString(dataReader["TicketHolderWebsite"]);
                        ticketHolder.work_phone = Convert.ToString(dataReader["TicketHolderWorkPhone"]);
                        ticketHolder.mobile_phone = Convert.ToString(dataReader["TicketHolderMobilePhone"]);
                        ticketHolder.dob = Convert.ToString(dataReader["TicketHolderBirthDate"]);
                        ticketHolder.country = Convert.ToString(dataReader["TicketHolderCountry"]);
                        ticketHolder.address1 = Convert.ToString(dataReader["TicketHolderAddress1"]);
                        ticketHolder.address2 = Convert.ToString(dataReader["TicketHolderAddress2"]);
                        ticketHolder.city = Convert.ToString(dataReader["TicketHolderCity"]);
                        ticketHolder.state = Convert.ToString(dataReader["TicketHolderState"]);
                        ticketHolder.postal_code = Convert.ToString(dataReader["TicketHolderZipCode"]);
                        ticketHolder.will_call_location = Convert.ToString(dataReader["TicketHolderWCLocationName"]);
                        ticketHolder.will_call_location_id = Convert.ToString(dataReader["TicketHolderWCLocationId"]);
                        ticketHolder.delivery_type = Convert.ToInt32(dataReader["DeliveryType"]);

                        tict.ticket_holder = ticketHolder;

                        if (Convert.ToInt32(dataReader["lastCheckInId"]) > 0)
                        {
                            var ticketCheckIn = new TicketCheckIn();

                            ticketCheckIn.id = Convert.ToInt32(dataReader["lastCheckInId"]);
                            //ticketCheckIn.location_id = Convert.ToInt32(dataReader["Location_Id"]);
                            //ticketCheckIn.location_name = Convert.ToString(dataReader["LocationName"]);
                            ticketCheckIn.geo_latitude = Convert.ToString(dataReader["GeoLatitude"]);
                            ticketCheckIn.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                            ticketCheckIn.checkin_date = GetOffsetDate(Convert.ToDateTime(dataReader["CheckInDateTime"]), Convert.ToInt32(dataReader["offsetTime"])).ToString();

                            tict.previous_checkin = ticketCheckIn;
                        }
                    }
                }
            }
            return tict;
        }

        public uc.Common.AttendeeAppCheckInMode GetEventCheckInMode(int EventId)
        {
            uc.Common.AttendeeAppCheckInMode mode = uc.Common.AttendeeAppCheckInMode.barcodeMulti;

            string sql = "select AttendeeAppCheckInMode from Tickets_Event where id = @EventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        mode = (uc.Common.AttendeeAppCheckInMode)Convert.ToInt32(dataReader["AttendeeAppCheckInMode"]);
                    }
                }
            }
            return mode;
        }

        public uc.Common.TicketsServiceFeesOption GetEventServiceFeeMode(int EventId)
        {
            uc.Common.TicketsServiceFeesOption mode = uc.Common.TicketsServiceFeesOption.Organizer;

            string sql = "select ServiceFeesOption from Tickets_Event where id = @EventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        mode = (uc.Common.TicketsServiceFeesOption)Convert.ToInt32(dataReader["ServiceFeesOption"]);
                    }
                }
            }
            return mode;
        }
        public uc.Common.TicketsPaymentProcessor GetEventPaymentProcessor(int EventId)
        {
            uc.Common.TicketsPaymentProcessor paymentProcessor = uc.Common.TicketsPaymentProcessor.CellarPassProcessor;

            string sql = "select PaymentProcessingOption from Tickets_Event where id = @EventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        paymentProcessor = (uc.Common.TicketsPaymentProcessor)Convert.ToInt32(dataReader["PaymentProcessingOption"]);
                    }
                }
            }
            return paymentProcessor;
        }


        public List<HDYH> GetHDYHByEventId(int eventId)
        {
            List<HDYH> hdyhOptions = new List<HDYH>();
            string sp = "Select HDYHOption from Tickets_HDYHOptions where [Tickets_Event_Id]=@eventId";

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@eventId", eventId));
            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        hdyhOptions.Add(new HDYH
                        {
                            choice = Convert.ToString(dataReader["HDYHOption"])
                        });
                    }

                }
            }
            return hdyhOptions;

        }
        public bool CheckDuplicateOrder(int eventId, int userId, decimal orderTotal, int orderType = 0)
        {
            bool isDuplicate = false;
            string sp = "CheckDuplicateTicketOrder";
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@eventId", eventId));
            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@orderTotal", orderTotal));
            parameterList.Add(GetParameter("@orderType", orderType));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    isDuplicate = true;

                }
            }
            return isDuplicate;

        }


        public int GetTotalAvailableTicketQty(int ticketEventId)
        {
            int qtyAvailable = 0;
            string sp = "GetTotalAvailableTicketQty";
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Ticket_Event_Id", ticketEventId));
            qtyAvailable = (int)ExecuteScalar(sp, parameterList);

            return qtyAvailable;
        }

        public int SaveTicketOrder(SaveTicketRequest reqModel, TixOrderCalculationModel tixOrderCalculationModel, TicketPlan ticketPlan, uc.Common.TicketsPaymentProcessor PaymentProcessor, int mobilePhoneStatus, TicketEventDetailModel ticketEvent, Guid orderGuid)
        {
            int Id = 0;
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@OrderDate", DateTime.UtcNow));
            parameterList.Add(GetParameter("@OrderStatus", TicketOrderStatus.Draft));
            parameterList.Add(GetParameter("@OrderTotal", tixOrderCalculationModel.grand_total));
            parameterList.Add(GetParameter("@OrderPaymentTranId", ""));
            parameterList.Add(GetParameter("@OrderPaymentDetail", ""));
            parameterList.Add(GetParameter("@Winery_Id", reqModel.member_id));
            parameterList.Add(GetParameter("@User_Id", reqModel.user_id));
            parameterList.Add(GetParameter("@Tickets_Event_Id", reqModel.event_id));
            parameterList.Add(GetParameter("@EventTitle", ticketEvent.name));
            parameterList.Add(GetParameter("@EventVenueName", ""));
            parameterList.Add(GetParameter("@EventVenueAddress", ticketEvent.venue_address_1));
            parameterList.Add(GetParameter("@EventStartDateTime", ticketEvent.start_date));
            parameterList.Add(GetParameter("@EventEndDateTime", ticketEvent.end_date));

            decimal perTicketFee = 0;
            decimal serviceFee = 0;
            decimal processingFee = 0;
            decimal maxTicketFee = 0;

            if (ticketPlan != null)
            {
                perTicketFee = ticketPlan.per_ticket_fee;
                serviceFee = ticketPlan.service_fee;

                if (tixOrderCalculationModel.card_type.ToLower().Contains("discover"))
                {
                    processingFee = ticketPlan.discover_processing_fee;
                }
                else if (tixOrderCalculationModel.card_type.ToLower().Contains("american") || tixOrderCalculationModel.card_type.ToLower().Contains("amex"))
                {
                    processingFee = ticketPlan.amex_processing_fee;
                }
                else if (tixOrderCalculationModel.card_type.ToLower().Contains("master") || tixOrderCalculationModel.card_type.ToLower().Contains("mc"))
                {
                    processingFee = ticketPlan.mastercard_processing_fee;
                }
                else
                {
                    processingFee = ticketPlan.visa_processing_fee;
                }

                maxTicketFee = ticketPlan.max_ticket_fee;
            }

            string phoneNum = "0";
            if (!string.IsNullOrWhiteSpace(reqModel.home_phone))
            {
                phoneNum = ExtractNumbers(reqModel.home_phone);
            }
            if (phoneNum.Trim().Length == 0)
                phoneNum = "0";

            parameterList.Add(GetParameter("@PerTicketFee", perTicketFee));
            parameterList.Add(GetParameter("@ServiceFee", serviceFee));

            parameterList.Add(GetParameter("@DeliveryOption", (int)TicketDelivery.SelfPrint));
            parameterList.Add(GetParameter("@BillFirstName", reqModel.first_name.Trim()));
            parameterList.Add(GetParameter("@BillLastName", reqModel.last_name.Trim()));
            parameterList.Add(GetParameter("@BillAddress1", reqModel.address_1));
            parameterList.Add(GetParameter("@BillAddress2", reqModel.address_2));
            parameterList.Add(GetParameter("@BillCity", reqModel.city));
            parameterList.Add(GetParameter("@BillState", reqModel.state));
            parameterList.Add(GetParameter("@BillCountry", reqModel.country));
            parameterList.Add(GetParameter("@BillZip", reqModel.zip));
            parameterList.Add(GetParameter("@BillHomePhone", phoneNum));
            parameterList.Add(GetParameter("@BillEmail", reqModel.email_address.Trim()));
            parameterList.Add(GetParameter("@OrderGUID", orderGuid));
            parameterList.Add(GetParameter("@EventVenueCounty", ticketEvent.venue_country));
            parameterList.Add(GetParameter("@EventVenueLatitude", ticketEvent.venue_latitude));
            parameterList.Add(GetParameter("@EventVenueLongitude", ticketEvent.venue_longitude));

            if (reqModel.referral_type == ReferralType.BackOffice)
            {
                parameterList.Add(GetParameter("@OrderType", (int)TicketOrderType.Backoffice));
            }
            else
            {
                parameterList.Add(GetParameter("@OrderType", (int)TicketOrderType.Online));
            }
            
            parameterList.Add(GetParameter("@ServiceFeesOption", (int)ticketEvent.service_fee_option));
            parameterList.Add(GetParameter("@ReferralID", 0));
            parameterList.Add(GetParameter("@ReminderSent", false));
            parameterList.Add(GetParameter("@OrderNote", reqModel.order_note));

            string membership = string.Empty;

            try
            {
                membership = reqModel.ticket_holders[0].membership;
            }
            catch { }
            parameterList.Add(GetParameter("@MembershipNum", membership));
            parameterList.Add(GetParameter("@HDYH", reqModel.hdyh));
            parameterList.Add(GetParameter("@ZohoOrderRefId", ""));
            parameterList.Add(GetParameter("@MaxTicketFee", maxTicketFee));

            parameterList.Add(GetParameter("@InternalNote", ""));
            parameterList.Add(GetParameter("@IsChargeback", false));
            parameterList.Add(GetParameter("@MobilePhoneStatus", mobilePhoneStatus));
            parameterList.Add(GetParameter("@PaymentProcessor", PaymentProcessor));

            //if (PaymentProcessor == TicketsPaymentProcessor.Stripe)
            //{
            //    parameterList.Add(GetParameter("@ProcessingFeeTotal", 0));
            //    parameterList.Add(GetParameter("@ProcessingFee", 0));
            //}
            //else
            //{
            //    parameterList.Add(GetParameter("@ProcessingFeeTotal", tixOrderCalculationModel.processing_fees));
            //    parameterList.Add(GetParameter("@ProcessingFee", processingFee));
            //}

            parameterList.Add(GetParameter("@ProcessingFeeTotal", tixOrderCalculationModel.processing_fees));
            parameterList.Add(GetParameter("@ProcessingFee", processingFee));
            parameterList.Add(GetParameter("@BookedById", reqModel.booked_by_id));
            parameterList.Add(GetParameter("@BookedByName", reqModel.booked_by_login_name));

            Id = Convert.ToInt32(ExecuteScalar("TicketsOrderInsert", parameterList));


            return Id;

        }

        public string GetStripeUserId(int memberId)
        {
            string sql = "select top 1 stripe_user_id from stripe_connect_account where member_id = @memberId and active=1";
            string stripeUserId = string.Empty;

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@memberId", memberId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        stripeUserId = Convert.ToString(dataReader["stripe_user_id"]);
                    }
                }
            }

            return stripeUserId;
        }


        public bool CheckTicketAccessCode(int eventId, string accessCode, int userId, int Qty)
        {
            bool accessCodeValid = false;
            string sp = "CheckAccessCodeValid";
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Accesscode", accessCode));
            parameterList.Add(GetParameter("@EventId", eventId));
            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@Qty", Qty));

            accessCodeValid = Convert.ToBoolean(ExecuteScalar(sp, parameterList));
            //using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            //{
            //    if (dataReader != null && dataReader.HasRows)
            //    {
            //        while (dataReader.Read())
            //        {
            //            accessCodeValid = Convert.ToBoolean(dataReader["AccessCodeValid"]);
            //        }

            //    }
            //}

            return accessCodeValid;
        }

        public bool CheckDiscountCodeValid(int eventId, string discountCode, int userId,int DiscountId)
        {
            bool discountCodeValid = false;
            string sp = "CheckDiscountCodeValid";
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Discountcode", discountCode));
            parameterList.Add(GetParameter("@EventId", eventId));
            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@DiscountId", DiscountId));

            discountCodeValid = Convert.ToBoolean(ExecuteScalar(sp, parameterList));

            return discountCodeValid;
        }

        public PassportCheckIn GetPassportCheckIn(int ticketId, int memberId)
        {
            PassportCheckIn passportCheckIn = new PassportCheckIn();

            string sql = "select top 1 w.DisplayName,pc.id,pc.Winery_Id,pc.CheckinDate From Tickets_Order_Tickets_PassportCheckin pc Join winery w On w.Id = pc.Winery_Id Where pc.Tickets_Order_Tickets_Id = @ticketId And pc.Winery_Id = @memberId Order By pc.CheckinDate desc";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ticketId", ticketId));
            parameterList.Add(GetParameter("@memberId", memberId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        passportCheckIn.id = Convert.ToInt32(dataReader["Id"]);
                        passportCheckIn.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        passportCheckIn.member_name = Convert.ToString(dataReader["DisplayName"]);
                        passportCheckIn.checkin_date = Convert.ToDateTime(dataReader["CheckinDate"]);
                    }
                }
            }
            return passportCheckIn;
        }

        public TicketsOrderPaymentDetail GetTicketsOrderPaymentDetail(int Id)
        {
            TicketsOrderPaymentDetail model = new TicketsOrderPaymentDetail();

            string sql = "select Amount,PayCardToken,TransactionId,PayCardNumber,PayCardCustName,PayCardExpMonth,PayCardExpYear,PaymentGateway,PayCardType from Tickets_Order_Payment where Tickets_Order_Id=@Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.ticket_order_id = Id;
                        model.amount = Convert.ToDecimal(dataReader["Amount"]);
                        model.card_token = Convert.ToString(dataReader["PayCardToken"]);
                        model.transaction_id = Convert.ToString(dataReader["TransactionId"]);

                        model.pay_card_number = StringHelpers.Decryption(Convert.ToString(dataReader["PayCardNumber"]));
                        if (!string.IsNullOrEmpty(model.pay_card_number))
                        {
                            model.pay_card_last_four_digits = Common.Common.Right(model.pay_card_number, 4);
                            //model.pay_card_first_four_digits = Common.Common.Left(model.pay_card_number, 4);
                        }

                        model.pay_card_custName = Convert.ToString(dataReader["PayCardCustName"]);
                        model.pay_card_exp_month = Convert.ToString(dataReader["PayCardExpMonth"]);
                        model.pay_card_exp_year = Convert.ToString(dataReader["PayCardExpYear"]);
                        model.payment_gateway = (Common.Payments.Configuration.Gateway)Convert.ToInt32(dataReader["PaymentGateway"]);
                        model.pay_card_type = Convert.ToString(dataReader["PayCardType"]);
                    }
                }
            }
            return model;
        }

        public Times.TimeZone GetEventTimeZone(int EventId)
        {
            Times.TimeZone TimeZone = Times.TimeZone.PacificTimeZone;

            string sql = "select TimeZoneId from Tickets_Event where id = @EventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TimeZone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                    }
                }
            }
            return TimeZone;
        }

        public static bool MultiCheckInAllowed(uc.Common.AttendeeAppCheckInMode mode)
        {
            bool multiAllowed = false;

            if (mode == uc.Common.AttendeeAppCheckInMode.standardMulti || mode == uc.Common.AttendeeAppCheckInMode.barcodeMulti || mode == uc.Common.AttendeeAppCheckInMode.searchMulti || mode == uc.Common.AttendeeAppCheckInMode.multiEvent)
                multiAllowed = true;
            else
                multiAllowed = false;

            return multiAllowed;
        }

        private static string GetBarcode(int memberId, int ticketOrderId, int ticketOrderTicketId)
        {
            string barcode = "";

            barcode = "0000".Substring(0, 4 - memberId.ToString().Length) + memberId.ToString() + "00000000".Substring(0, 8 - ticketOrderId.ToString().Length) + ticketOrderId.ToString() + "00000000".Substring(0, 8 - ticketOrderTicketId.ToString().Length) + ticketOrderTicketId.ToString();

            return barcode;
        }

        private static DateTime GetOffsetDate(DateTime originalDate, int offsetTime)
        {
            return originalDate.AddMinutes(offsetTime);
        }

        public TicketEventMetrics GetTicketEventMetricsByEventId(int EventId)
        {
            string sp = "Tickets_MetricsByTicketLevel";

            var ticketMetrics = new List<TicketMetrics>();
            var ticketEventMetrics = new TicketEventMetrics();
            var parameterList = new List<DbParameter>();

            int total_qty_available = 0;
            int total_qty_checkedin = 0;
            int total_qty_sold = 0;
            int total_capacity = 0;
            decimal total_ticket_sales = 0;
            decimal revenue = 0;

            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tMetrics = new TicketMetrics();
                        tMetrics.qty_available = Convert.ToInt32(dataReader["MaxAvailable"]);
                        tMetrics.qty_checkedin = Convert.ToInt32(dataReader["QtyCheckedIn"]);
                        tMetrics.qty_sold = Convert.ToInt32(dataReader["QtySold"]);
                        tMetrics.ticket_id = Convert.ToInt32(dataReader["id"]);
                        tMetrics.ticket_name = Convert.ToString(dataReader["TicketName"]);

                        total_qty_available = total_qty_available + tMetrics.qty_available;
                        total_qty_checkedin = total_qty_checkedin + tMetrics.qty_checkedin;
                        total_qty_sold = total_qty_sold + tMetrics.qty_sold;
                        if (total_capacity == 0)
                            total_capacity = Convert.ToInt32(dataReader["MaxCapacity"]);
                        total_ticket_sales = total_ticket_sales + Convert.ToDecimal(dataReader["TicketPrice"]);
                        revenue = revenue + Convert.ToDecimal(dataReader["Revenue"]);
                        ticketMetrics.Add(tMetrics);
                    }
                }
            }

            if (ticketMetrics.Count > 0)
            {
                ticketEventMetrics.ticket_metrics = ticketMetrics;
                ticketEventMetrics.total_qty_available = total_qty_available;
                ticketEventMetrics.total_qty_checkedin = total_qty_checkedin;
                ticketEventMetrics.total_qty_sold = total_qty_sold;
                ticketEventMetrics.total_capacity = total_capacity;
                ticketEventMetrics.total_ticket_sales = total_ticket_sales;
                ticketEventMetrics.revenue = revenue;
            }
            return ticketEventMetrics;
        }

        public TicketPassportEventMetrics GetTicketPassportEventMetricsByEventId(int EventId)
        {
            string sp = "GetTicketPassportEventMetricsByEventId";

            var ticketMetrics = new List<TicketPassportMetrics>();
            var ticketEventMetrics = new TicketPassportEventMetrics();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tMetrics = new TicketPassportMetrics();
                        tMetrics.member_id = Convert.ToInt32(dataReader["Id"]);
                        tMetrics.qty_checkedin = Convert.ToInt32(dataReader["qty_checkedin"]);
                        tMetrics.last_checkin = Convert.ToDateTime(dataReader["last_checkin"]);
                        tMetrics.member_name = Convert.ToString(dataReader["DisplayName"]);

                        ticketMetrics.Add(tMetrics);
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            ticketEventMetrics.ticket_metrics = ticketMetrics;
                            ticketEventMetrics.total_qty_available = Convert.ToInt32(dataReader["TixAvailable"]);
                            ticketEventMetrics.total_qty_checkedin = Convert.ToInt32(dataReader["TixCheckedIn"]);
                            ticketEventMetrics.total_qty_sold = Convert.ToInt32(dataReader["TixSold"]);
                        }
                    }
                }
            }

            return ticketEventMetrics;
        }

        private static bool CheckInAllowed(DateTime eventStartDate, Times.TimeZone eventTimeZone)
        {
            bool isAllowed = false;
            DateTime currentEventDate = Times.ToTimeZoneTime(DateTime.UtcNow, eventTimeZone);

            try
            {
                if (eventStartDate.Date <= currentEventDate.Date)
                    isAllowed = true;
            }
            catch (Exception ex)
            {
            }

            return isAllowed;
        }

        public List<ParticipatingLocations> GetParticipatingLocationByTicketEventID(int TicketEventId)
        {
            string sp = "GetParticipatingLocationByTicketEventID";

            var model = new List<ParticipatingLocations>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@TicketEventId", TicketEventId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ParticipatingLocations participatingLocations = new ParticipatingLocations();
                        participatingLocations.member_id = Convert.ToInt32(dataReader["Winery_id"]);
                        participatingLocations.member_name = Convert.ToString(dataReader["DisplayName"]);

                        model.Add(participatingLocations);
                    }
                }
            }
            return model;
        }

        //public List<LocationModel> GetLocationByTicketEventID(int TicketEventId, int primaryLocationId = 0)
        //{
        //    string sp = "GetLocationByTicketEventID";

        //    var model = new List<LocationModel>();
        //    var parameterList = new List<DbParameter>();

        //    parameterList.Add(GetParameter("@TicketEventId", TicketEventId));

        //    using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
        //    {
        //        if (dataReader != null && dataReader.HasRows)
        //        {
        //            while (dataReader.Read())
        //            {
        //                LocationModel locationModel = new LocationModel();
        //                locationModel.location_id = Convert.ToInt32(dataReader["Id"]);
        //                locationModel.destination_name = Convert.ToString(dataReader["DestinationName"]);
        //                locationModel.location_name = Convert.ToString(dataReader["LocationName"]);
        //                locationModel.technical_name = Convert.ToString(dataReader["TechnicalName"]);
        //                locationModel.description = Convert.ToString(dataReader["Description"]);
        //                locationModel.geo_latitude = Convert.ToString(dataReader["GeoLatitude"]);
        //                locationModel.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
        //                locationModel.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
        //                locationModel.location_timezone = Convert.ToInt32(dataReader["TimeZoneId"]);
        //                locationModel.location_timezone_offset = Times.GetOffsetMinutes((Times.TimeZone)locationModel.location_timezone);
        //                locationModel.seating_reset_time = Convert.ToString(dataReader["SeatingResetTime"]);
        //                locationModel.room_height = Convert.ToInt32(dataReader["RoomHeight"]);
        //                locationModel.room_width = Convert.ToInt32(dataReader["RoomWidth"]);

        //                if (string.IsNullOrEmpty(locationModel.seating_reset_time))
        //                {
        //                    locationModel.seating_reset_time = "04:00:00";
        //                }

        //                Address addr = new Address();

        //                addr.address_1 = Convert.ToString(dataReader["Address1"]);
        //                addr.address_2 = Convert.ToString(dataReader["Address2"]);
        //                addr.city = Convert.ToString(dataReader["City"]);
        //                addr.state = Convert.ToString(dataReader["State"]);
        //                addr.zip_code = Convert.ToString(dataReader["Zip"]);
        //                addr.country = Convert.ToString(dataReader["country"]);
        //                locationModel.address = addr;

        //                if (primaryLocationId > 0 && locationModel.location_id == primaryLocationId)
        //                {
        //                    locationModel.is_primary = true;
        //                }
        //                else
        //                {
        //                    locationModel.is_primary = false;
        //                }
        //                model.Add(locationModel);
        //            }
        //        }
        //    }
        //    return model;
        //}

        public List<LocationModel> GetWillLocationsByTicketEventID(int TicketEventId)
        {
            var model = new List<LocationModel>();

            string sql = @"select l.Id,l.DestinationName,l.LocationName,l.TechnicalName,l.Description,l.GeoLatitude,l.GeoLongitude,wl.SortOrder,l.TimeZoneId,l.SeatingResetTime,
                            l.ServerMode,l.Address1,l.Address2,l.City,l.State,l.Zip,l.RoomWidth,l.RoomHeight,isnull(l.Country,'') as country 
                            from Location l
                            join  Tickets_Event_WillCallLocation wl on l.Id=wl.LocationId
                             where wl.Tickets_Event_Id = @TicketEventId order by wl.SortOrder";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TicketEventId", TicketEventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
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

                        Address addr = new Address();

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

        public List<LocationModel> GetWillLocationsByTicketLevelID(int ticketLevelId, int quantity)
        {
            var model = new List<LocationModel>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@TicketLevelId", ticketLevelId));
            parameterList.Add(GetParameter("@Quantity", quantity));

            using (DbDataReader dataReader = GetDataReader("GetWillLocationsByTicketLevelID", parameterList, CommandType.StoredProcedure))
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

                        Address addr = new Address();

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


        public int CheckInTicket(int ticketId)
        {
            int checkInId = 0;

            //EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
            //LocationModel locationModel = eventDAL.GetLocationByID(locationId);

            if (ticketId > 0)
            {
                var parameterList = new List<SqlParameter>();
                parameterList.Add(GetParameter("@TicketId", ticketId));
                //parameterList.Add(GetParameter("@Location_Id", locationId));
                //parameterList.Add(GetParameter("@LocationName", locationModel.location_name));
                //parameterList.Add(GetParameter("@GeoLatitude", locationModel.geo_latitude));
                //parameterList.Add(GetParameter("@GeoLongitude", locationModel.geo_longitude));

                checkInId = Convert.ToInt32(ExecuteScalar("CheckInTicket", parameterList));
            }


            return checkInId;
        }

        public int CheckInPassportTicket(int Tickets_Order_Tickets_Id, int Winery_Id)
        {
            int checkInId = 0;

            if (Tickets_Order_Tickets_Id > 0 && Winery_Id > 0)
            {
                var parameterList = new List<SqlParameter>();
                parameterList.Add(GetParameter("@Tickets_Order_Tickets_Id", Tickets_Order_Tickets_Id));
                parameterList.Add(GetParameter("@Winery_Id", Winery_Id));

                checkInId = Convert.ToInt32(ExecuteScalar("CheckInPassportTicket", parameterList));
            }


            return checkInId;
        }

        public TicketPlan GetTicketPlanForMember(int wineryId)
        {
            string sp = "GetTicketingPlanForMember";

            var ticketPlan = new TicketPlan();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@wineryId", wineryId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ticketPlan.max_ticket_fee = Convert.ToDecimal(dataReader["MaxTicketFee"]);
                        ticketPlan.monthly_fee = Convert.ToDecimal(dataReader["MonthlyFee"]);
                        ticketPlan.per_ticket_fee = Convert.ToDecimal(dataReader["PerTicketFee"]);
                        ticketPlan.visa_processing_fee = Convert.ToDecimal(dataReader["ProcessingFee"]);
                        ticketPlan.mastercard_processing_fee = Convert.ToDecimal(dataReader["MasterCardProcessingFee"]);
                        ticketPlan.discover_processing_fee = Convert.ToDecimal(dataReader["DiscoverProcessingFee"]);
                        ticketPlan.amex_processing_fee = Convert.ToDecimal(dataReader["AmexProcessingFee"]);
                        ticketPlan.service_fee = Convert.ToDecimal(dataReader["ServiceFee"]);

                        ticketPlan.name = Convert.ToString(dataReader["Name"]);
                        ticketPlan.description = Convert.ToString(dataReader["Description"]);
                        ticketPlan.plan_id = Convert.ToInt32(dataReader["Id"]);
                        ticketPlan.svc_agreement_id = Convert.ToInt32(dataReader["ServiceAgreement_Id"]);
                        ticketPlan.winery_name = Convert.ToString(dataReader["Name"]);
                    }
                }
            }


            return ticketPlan;
        }

        public TicketPostCaptureStatus GetTicketPostCaptureStatus(int ticket_id)
        {
            string sql = @"select PostCaptureStatus from Tickets_Order_Tickets where Id = @Id";
            TicketPostCaptureStatus status = TicketPostCaptureStatus.NA;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", ticket_id));
            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        status = (TicketPostCaptureStatus)Convert.ToInt32(dataReader["PostCaptureStatus"]);
                    }
                }
            }

            return status;
        }

        public TicketOrder GetTicketOrderById(int orderId)
        {
            string sp = "GetTicketOrderById";

            var ticketOrder = new TicketOrder();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@orderId", orderId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ticketOrder.Id = Convert.ToInt32(dataReader["Id"]);
                        ticketOrder.Winery_Id = Convert.ToInt32(dataReader["Winery_Id"]);
                        ticketOrder.User_Id = Convert.ToInt32(dataReader["User_Id"]);
                        ticketOrder.Tickets_Event_Id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                        ticketOrder.EventStartDateTime = Convert.ToDateTime(dataReader["EventStartDateTime"]);
                        ticketOrder.EventEndDateTime = Convert.ToDateTime(dataReader["EventEndDateTime"]);
                        ticketOrder.OrderTotal = Convert.ToDecimal(dataReader["OrderTotal"]);
                        ticketOrder.ServiceFee = Convert.ToDecimal(dataReader["ServiceFee"]);
                        ticketOrder.PaidAmt = Convert.ToDecimal(dataReader["PaidAmt"]);
                        ticketOrder.OrderDate = Convert.ToDateTime(dataReader["OrderDate"]);
                        ticketOrder.BillEmailAddress = Convert.ToString(dataReader["BillEmail"]);
                        ticketOrder.BillFirstName = Convert.ToString(dataReader["BillFirstName"]);
                        ticketOrder.BillLastName = Convert.ToString(dataReader["BillLastName"]);
                        ticketOrder.BillHomePhone = Convert.ToString(dataReader["BillHomePhone"]);
                        ticketOrder.BillCountry = Convert.ToString(dataReader["BillCountry"]);
                        ticketOrder.BillZip = Convert.ToString(dataReader["BillZip"]);
                        ticketOrder.EventTitle = Convert.ToString(dataReader["EventTitle"]);
                        //ticketOrder.EventVenueName = Convert.ToString(dataReader["EventVenueName"]);
                        ticketOrder.EventVenueAddress = Convert.ToString(dataReader["EventVenueAddress"]);
                        ticketOrder.OrderGUID = Convert.ToString(dataReader["OrderGUID"]);
                        ticketOrder.wineryName = Convert.ToString(dataReader["DisplayName"]);
                        ticketOrder.DynamicPaymentDesc = Convert.ToString(dataReader["DynamicPaymentDesc"]);
                        ticketOrder.OrderNote = Convert.ToString(dataReader["OrderNote"]);
                        ticketOrder.EventOrganizerEmail = Convert.ToString(dataReader["EventOrganizerEmail"]);
                        ticketOrder.EventOrganizerName = Convert.ToString(dataReader["EventOrganizerName"]);
                        ticketOrder.EventOrganizerPhone = Convert.ToString(dataReader["EventOrganizerPhone"]);
                        ticketOrder.PaymentType = (PaymentType)Convert.ToInt32(dataReader["PaymentType"]);
                        ticketOrder.EmailReceiptTemplate = Convert.ToInt32(dataReader["EmailReceiptTemplate"]);
                        ticketOrder.EmailBuyerReminderTemplate = Convert.ToInt32(dataReader["EmailBuyerReminderTemplate"]);
                        ticketOrder.PrimaryCategory = (TicketCategory)Convert.ToInt32(dataReader["Category"]);
                        ticketOrder.ItineraryGUID = Convert.ToString(dataReader["ItineraryGUID"]);
                        ticketOrder.EventBannerImage = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        ticketOrder.refund_policy = (uc.Common.TicketRefundPolicy)Convert.ToInt32(dataReader["TicketRefundPolicy"]);
                        ticketOrder.refund_policy_text = ticketOrder.refund_policy.GetEnumDescription();
                        ticketOrder.business_message = Convert.ToString(dataReader["ConfirmationEmail"]);
                        ticketOrder.geo_latitude = Convert.ToString(dataReader["VenueLatitude"]);
                        ticketOrder.geo_longitude = Convert.ToString(dataReader["VenueLongitude"]);

                        ticketOrder.ticket_event_policy = Convert.ToString(dataReader["EventAttendeePolicy"]);

                        ticketOrder.venue_full_address = Convert.ToString(dataReader["Fulladdress"]);
                        ticketOrder.event_url = Convert.ToString(dataReader["EventURL"]);
                        ticketOrder.region_id = Convert.ToInt32(dataReader["Regionid"]);
                        ticketOrder.refund_service_fees_option = (uc.Common.RefundServiceFeesOption)Convert.ToInt32(dataReader["RefundServiceFeesOption"]);
                        ticketOrder.refund_service_fees_option_text = ticketOrder.refund_service_fees_option.GetEnumDescription();
                        if (!string.IsNullOrWhiteSpace(dataReader["InternalNotificationRecipient"].ToString()))
                        {
                            try
                            {
                                List<int> internalNotificationRecipient = dataReader["InternalNotificationRecipient"].ToString().Split(',').Select(int.Parse).ToList();
                                ticketOrder.internal_notification_recipient = internalNotificationRecipient;
                            }
                            catch { }
                        }
                    }
                }
            }


            return ticketOrder;
        }

        public TicketOrderModel GetTicketOrder(int orderId, string TicketGUID)
        {
            string sp = "GetTicketOrderById";

            var ticketOrder = new TicketOrderModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@orderId", orderId));
            parameterList.Add(GetParameter("@TicketGUID", TicketGUID));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ticketOrder.id = Convert.ToInt32(dataReader["Id"]);
                        ticketOrder.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        ticketOrder.tickets_event_id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                        ticketOrder.event_start_date_time = Convert.ToDateTime(dataReader["EventStartDateTime"]);
                        ticketOrder.event_end_date_time = Convert.ToDateTime(dataReader["EventEndDateTime"]);
                        ticketOrder.order_total = Convert.ToDecimal(dataReader["OrderTotal"]);
                        ticketOrder.order_total = Convert.ToDecimal(dataReader["OrderTotal"]);
                        ticketOrder.paid_total = Convert.ToDecimal(dataReader["PaidAmt"]);
                        ticketOrder.bill_first_name = Convert.ToString(dataReader["BillFirstName"]);
                        ticketOrder.bill_last_name = Convert.ToString(dataReader["BillLastName"]);
                        ticketOrder.bill_email_address = Convert.ToString(dataReader["BillEmail"]);
                        ticketOrder.bill_home_phone = Convert.ToString(dataReader["BillHomePhone"]);
                        ticketOrder.bill_country = Convert.ToString(dataReader["BillCountry"]);
                        ticketOrder.event_title = Convert.ToString(dataReader["EventTitle"]);
                        //ticketOrder.event_venue_name = Convert.ToString(dataReader["EventVenueName"]);
                        ticketOrder.event_venue_address = Convert.ToString(dataReader["EventVenueAddress"]);
                        ticketOrder.order_guid = Convert.ToString(dataReader["OrderGUID"]);
                        ticketOrder.member_name = Convert.ToString(dataReader["DisplayName"]);
                        ticketOrder.dynamic_payment_desc = Convert.ToString(dataReader["DynamicPaymentDesc"]);
                        ticketOrder.order_note = Convert.ToString(dataReader["OrderNote"]);
                        ticketOrder.event_organizer_email = Convert.ToString(dataReader["EventOrganizerEmail"]);
                        ticketOrder.event_organizer_url = Convert.ToString(dataReader["WebsiteURL"]);
                        ticketOrder.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        ticketOrder.event_organizer_phone = Convert.ToString(dataReader["EventOrganizerPhone"]);
                        ticketOrder.payment_type = (PaymentType)Convert.ToInt32(dataReader["PaymentType"]);

                        ticketOrder.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        ticketOrder.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        ticketOrder.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        ticketOrder.timezone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        ticketOrder.timezone_name = GetTimezoneNameById(ticketOrder.timezone);
                        ticketOrder.timezone_offset = Times.GetOffsetMinutes(ticketOrder.timezone);
                        ticketOrder.geo_latitude = Convert.ToString(dataReader["VenueLatitude"]);
                        ticketOrder.geo_longitude = Convert.ToString(dataReader["VenueLongitude"]);
                        ticketOrder.venue_full_address = Convert.ToString(dataReader["Fulladdress"]);
                        ticketOrder.event_image = Convert.ToString(dataReader["EventImage"]);
                        ticketOrder.event_image_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        ticketOrder.ad_image = Convert.ToString(dataReader["AdImage"]);
                        ticketOrder.event_image_big = Convert.ToString(dataReader["EventImageBig"]);

                        ticketOrder.event_image_big_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));

                        ticketOrder.processing_fee = Convert.ToDecimal(dataReader["ProcessingFee"]);

                        ticketOrder.email_receipt_template = Convert.ToInt32(dataReader["EmailReceiptTemplate"]);

                        ticketOrder.primary_category = (TicketCategory)Convert.ToInt32(dataReader["Category"]);

                        ticketOrder.order_date = Convert.ToDateTime(dataReader["OrderDate"]);
                        ticketOrder.itinerary_guid = Convert.ToString(dataReader["ItineraryGUID"]);

                        ticketOrder.order_special_instructions = Convert.ToString(dataReader["OrderSpecialInstructions"]);
                        ticketOrder.confirmation_page = Convert.ToString(dataReader["ConfirmationPage"]);
                        ticketOrder.business_message = Convert.ToString(dataReader["ConfirmationEmail"]);
                        ticketOrder.region_id = Convert.ToInt32(dataReader["Regionid"]);
                        ticketOrder.event_url = Convert.ToString(dataReader["EventURL"]);
                        ticketOrder.ticket_event_policy = Convert.ToString(dataReader["EventAttendeePolicy"]);

                        if (!string.IsNullOrWhiteSpace(dataReader["InternalNotificationRecipient"].ToString()))
                        {
                            try
                            {
                                List<int> internalNotificationRecipient = dataReader["InternalNotificationRecipient"].ToString().Split(',').Select(int.Parse).ToList();
                                ticketOrder.internal_notification_recipient = internalNotificationRecipient;
                            }
                            catch { }
                        }

                        List<TixOrderHolder> tixholder = new List<TixOrderHolder>();
                        decimal Total = 0;
                        decimal GratuityTotal = 0;
                        decimal SalesTaxTotal = 0;
                        decimal TotalServiceFees = 0;

                        var tictorderticket = GetTicketOrderTicketsByOrderId(ticketOrder.id, Convert.ToString(dataReader["BillFirstName"]), Convert.ToString(dataReader["BillLastName"]), Convert.ToString(dataReader["BillEmail"]), out tixholder, out Total, out TotalServiceFees, out GratuityTotal, out SalesTaxTotal);

                        ticketOrder.ticket_holder = tixholder;
                        ticketOrder.ticket_order_ticket = tictorderticket;

                        if (tictorderticket != null && tictorderticket.Count > 0)
                        {
                            var tixOrderSummary = new List<TixOrderTicket>();
                            int ticket_id = 0;
                            TicketDelivery DeliveryType = TicketDelivery.None;
                            TixOrderTicket ticketOrderTicket = null;
                            foreach (TixOrderTicket tix in tictorderticket)
                            {
                                int newticket_id = tix.ticket_id;
                                TicketDelivery newDeliveryType = tix.delivery_type;

                                if (newticket_id != ticket_id || newDeliveryType != DeliveryType)
                                {
                                    ticketOrderTicket = new TixOrderTicket();
                                    ticketOrderTicket.id = tix.id;
                                    ticketOrderTicket.delivery_type = tix.delivery_type;
                                    ticketOrderTicket.ticket_name = tix.ticket_name;
                                    ticketOrderTicket.ticket_price = tix.ticket_price;
                                    ticketOrderTicket.validend_date = tix.valid_start_date;
                                    ticketOrderTicket.valid_start_date = tix.validend_date;
                                    ticketOrderTicket.first_name = ticketOrder.bill_first_name;
                                    ticketOrderTicket.last_name = ticketOrder.bill_last_name;
                                    ticketOrderTicket.email = ticketOrder.bill_email_address;

                                    ticketOrderTicket.gratuity = tix.gratuity;
                                    ticketOrderTicket.sales_tax = tix.sales_tax;
                                    ticketOrderTicket.ticket_fee = tix.ticket_fee;
                                    ticketOrderTicket.ticket_qty = tix.ticket_qty;
                                    ticketOrderTicket.post_capture_key = tix.post_capture_key;
                                    ticketOrderTicket.post_capture_status = tix.post_capture_status;

                                    ticketOrderTicket.will_call_location_id = tix.will_call_location_id;
                                    ticketOrderTicket.will_call_location_name = tix.will_call_location_name;
                                    ticketOrderTicket.will_call_location_address = tix.will_call_location_address;

                                    ticket_id = tix.ticket_id;
                                    DeliveryType = tix.delivery_type;

                                    ticketOrderTicket.include_confirmation_message = tix.include_confirmation_message;
                                    ticketOrderTicket.confirmation_message = tix.confirmation_message;

                                    tixOrderSummary.Add(ticketOrderTicket);
                                }
                                else
                                {
                                    ticketOrderTicket.gratuity += tix.gratuity;
                                    ticketOrderTicket.sales_tax += tix.sales_tax;
                                    ticketOrderTicket.ticket_fee += tix.ticket_fee;
                                    ticketOrderTicket.ticket_qty += tix.ticket_qty;
                                    ticketOrderTicket.ticket_price += tix.ticket_price;
                                }
                            }
                            ticketOrder.ticket_order_ticket_summary = tixOrderSummary;
                        }

                        ticketOrder.service_fees = TotalServiceFees;
                        ticketOrder.sub_total = Total;
                        ticketOrder.sales_tax = SalesTaxTotal;
                        ticketOrder.gratuity = GratuityTotal;
                        ticketOrder.refund_policy = (uc.Common.TicketRefundPolicy)Convert.ToInt32(dataReader["TicketRefundPolicy"]);
                        ticketOrder.refund_policy_text = ticketOrder.refund_policy.GetEnumDescription();
                    }
                }
            }


            return ticketOrder;
        }

        public TicketOrderV2Model GetTicketOrderV2(int orderId, string TicketGUID)
        {
            string sp = "GetTicketOrderById";

            var ticketOrder = new TicketOrderV2Model();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@orderId", orderId));
            parameterList.Add(GetParameter("@TicketGUID", TicketGUID));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ticketOrder.id = Convert.ToInt32(dataReader["Id"]);
                        ticketOrder.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        ticketOrder.tickets_event_id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                        ticketOrder.event_start_date_time = Convert.ToDateTime(dataReader["EventStartDateTime"]);
                        ticketOrder.event_end_date_time = Convert.ToDateTime(dataReader["EventEndDateTime"]);
                        ticketOrder.order_total = Convert.ToDecimal(dataReader["OrderTotal"]);
                        ticketOrder.order_total = Convert.ToDecimal(dataReader["OrderTotal"]);
                        ticketOrder.paid_total = Convert.ToDecimal(dataReader["PaidAmt"]);
                        ticketOrder.bill_first_name = Convert.ToString(dataReader["BillFirstName"]);
                        ticketOrder.bill_last_name = Convert.ToString(dataReader["BillLastName"]);
                        ticketOrder.bill_email_address = Convert.ToString(dataReader["BillEmail"]);
                        ticketOrder.bill_home_phone = Convert.ToString(dataReader["BillHomePhone"]);
                        ticketOrder.bill_country = Convert.ToString(dataReader["BillCountry"]);
                        ticketOrder.event_title = Convert.ToString(dataReader["EventTitle"]);
                        //ticketOrder.event_venue_name = Convert.ToString(dataReader["EventVenueName"]);
                        ticketOrder.event_venue_address = Convert.ToString(dataReader["EventVenueAddress"]);
                        ticketOrder.order_guid = Convert.ToString(dataReader["OrderGUID"]);
                        ticketOrder.member_name = Convert.ToString(dataReader["DisplayName"]);
                        ticketOrder.dynamic_payment_desc = Convert.ToString(dataReader["DynamicPaymentDesc"]);
                        ticketOrder.order_note = Convert.ToString(dataReader["OrderNote"]);
                        ticketOrder.event_organizer_email = Convert.ToString(dataReader["EventOrganizerEmail"]);
                        ticketOrder.event_organizer_url = Convert.ToString(dataReader["WebsiteURL"]);
                        ticketOrder.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        ticketOrder.event_organizer_phone = Convert.ToString(dataReader["EventOrganizerPhone"]);
                        ticketOrder.payment_type = (PaymentType)Convert.ToInt32(dataReader["PaymentType"]);

                        ticketOrder.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        ticketOrder.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        ticketOrder.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        ticketOrder.timezone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        ticketOrder.timezone_name = GetTimezoneNameById(ticketOrder.timezone);
                        ticketOrder.timezone_offset = Times.GetOffsetMinutes(ticketOrder.timezone);

                        ticketOrder.event_image = Convert.ToString(dataReader["EventImage"]);
                        ticketOrder.event_image_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        ticketOrder.ad_image = Convert.ToString(dataReader["AdImage"]);
                        ticketOrder.event_image_big = Convert.ToString(dataReader["EventImageBig"]);
                        ticketOrder.event_image_big_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        ticketOrder.processing_fee = Convert.ToDecimal(dataReader["ProcessingFee"]);

                        ticketOrder.email_receipt_template = Convert.ToInt32(dataReader["EmailReceiptTemplate"]);

                        ticketOrder.primary_category = (TicketCategory)Convert.ToInt32(dataReader["Category"]);
                        ticketOrder.region_id = Convert.ToInt32(dataReader["Regionid"]);

                        ticketOrder.order_date = Convert.ToDateTime(dataReader["OrderDate"]);
                        ticketOrder.itinerary_guid = Convert.ToString(dataReader["ItineraryGUID"]);

                        List<TixOrderHolder> tixholder = new List<TixOrderHolder>();
                        decimal Total = 0;
                        decimal GratuityTotal = 0;
                        decimal SalesTaxTotal = 0;
                        decimal TotalServiceFees = 0;

                        var tictorderticket = GetTicketOrderTicketsV2ByOrderId(ticketOrder.id, Convert.ToString(dataReader["BillFirstName"]), Convert.ToString(dataReader["BillLastName"]), Convert.ToString(dataReader["BillEmail"]), out Total, out TotalServiceFees, out GratuityTotal, out SalesTaxTotal);

                        ticketOrder.ticket_order_ticket = tictorderticket;

                        if (tictorderticket != null && tictorderticket.Count > 0)
                        {
                            var tixOrderSummary = new List<TixOrderTicket>();
                            int ticket_id = 0;
                            TicketDelivery DeliveryType = TicketDelivery.None;
                            TixOrderTicket ticketOrderTicket = null;
                            foreach (TixOrderTicketV2 tix in tictorderticket)
                            {
                                int newticket_id = tix.ticket_id;
                                TicketDelivery newDeliveryType = tix.delivery_type;

                                if (newticket_id != ticket_id || newDeliveryType != DeliveryType)
                                {
                                    ticketOrderTicket = new TixOrderTicket();
                                    ticketOrderTicket.id = tix.id;
                                    ticketOrderTicket.delivery_type = tix.delivery_type;
                                    ticketOrderTicket.ticket_name = tix.ticket_name;
                                    ticketOrderTicket.ticket_price = tix.ticket_price;
                                    ticketOrderTicket.validend_date = tix.valid_start_date;
                                    ticketOrderTicket.valid_start_date = tix.validend_date;
                                    ticketOrderTicket.first_name = ticketOrder.bill_first_name;
                                    ticketOrderTicket.last_name = ticketOrder.bill_last_name;
                                    ticketOrderTicket.email = ticketOrder.bill_email_address;

                                    ticketOrderTicket.gratuity = tix.gratuity;
                                    ticketOrderTicket.sales_tax = tix.sales_tax;
                                    ticketOrderTicket.ticket_fee = tix.ticket_fee;
                                    ticketOrderTicket.ticket_qty = tix.ticket_qty;
                                    ticketOrderTicket.post_capture_key = tix.post_capture_key;
                                    ticketOrderTicket.post_capture_status = tix.post_capture_status;

                                    ticketOrderTicket.will_call_location_id = tix.will_call_location_id;
                                    ticketOrderTicket.will_call_location_name = tix.will_call_location_name;
                                    ticketOrderTicket.will_call_location_address = tix.will_call_location_address;

                                    ticket_id = tix.ticket_id;
                                    DeliveryType = tix.delivery_type;

                                    ticketOrderTicket.include_confirmation_message = tix.include_confirmation_message;
                                    ticketOrderTicket.confirmation_message = tix.confirmation_message;

                                    tixOrderSummary.Add(ticketOrderTicket);
                                }
                                else
                                {
                                    ticketOrderTicket.gratuity += tix.gratuity;
                                    ticketOrderTicket.sales_tax += tix.sales_tax;
                                    ticketOrderTicket.ticket_fee += tix.ticket_fee;
                                    ticketOrderTicket.ticket_qty += tix.ticket_qty;
                                    ticketOrderTicket.ticket_price += tix.ticket_price;
                                }
                            }
                            ticketOrder.ticket_order_ticket_summary = tixOrderSummary;
                        }

                        ticketOrder.service_fees = TotalServiceFees;
                        ticketOrder.sub_total = Total;
                        ticketOrder.sales_tax = SalesTaxTotal;
                        ticketOrder.gratuity = GratuityTotal;
                    }
                }
            }


            return ticketOrder;
        }

        public List<TixOrderTicket> GetTicketOrderTicketsByOrderId(int orderId, string FirstName, string LastName, string Email, out List<TixOrderHolder> tixholder, out decimal SubTotal, out decimal ServiceFees, out decimal Gratuity, out decimal SalesTax)
        {

            decimal Total = 0;
            decimal GratuityTotal = 0;
            decimal SalesTaxTotal = 0;
            decimal TotalServiceFees = 0;
            string sp = "Tickets_GetTicketOrderTicketsByOrder";

            var ticketOrderTicket = new TixOrderTicket();
            var listticketOrderTicket = new List<TixOrderTicket>();
            List<TixOrderHolder> listholder = new List<TixOrderHolder>();
            var parameterList = new List<DbParameter>();


            parameterList.Add(GetParameter("@orderId", orderId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    int DeliveryType = -1;
                    while (dataReader.Read())
                    {
                        var holder = new TixOrderHolder();
                        ticketOrderTicket = new TixOrderTicket();
                        ticketOrderTicket.id = Convert.ToInt32(dataReader["Id"]);
                        ticketOrderTicket.delivery_type = (TicketDelivery)Convert.ToInt32(dataReader["DeliveryType"]);
                        ticketOrderTicket.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        ticketOrderTicket.ticket_price = Convert.ToDecimal(dataReader["TicketPrice"]);
                        ticketOrderTicket.validend_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        ticketOrderTicket.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        ticketOrderTicket.first_name = Convert.ToString(dataReader["TicketHolderFirstName"]);
                        ticketOrderTicket.last_name = Convert.ToString(dataReader["TicketHolderLastName"]);
                        ticketOrderTicket.email = Convert.ToString(dataReader["TicketHolderEmail"]);
                        ticketOrderTicket.fulfillment_lead_time = Convert.ToInt32(dataReader["FulfillmentLeadTime"]);

                        ticketOrderTicket.gratuity = Convert.ToDecimal(dataReader["Gratuity"]);
                        ticketOrderTicket.sales_tax = Convert.ToDecimal(dataReader["SalesTax"]);
                        ticketOrderTicket.ticket_fee = Convert.ToDecimal(dataReader["TicketFee"]);
                        ticketOrderTicket.ticket_qty = Convert.ToInt32(dataReader["TicketQty"]);
                        ticketOrderTicket.post_capture_key = Convert.ToString(dataReader["PostCaptureKey"]);
                        ticketOrderTicket.display_venue_on_ticket = Convert.ToBoolean(dataReader["DisplayVenueonTicket"]);
                        ticketOrderTicket.post_capture_status = (TicketPostCaptureStatus)Convert.ToInt32(dataReader["PostCaptureStatus"]);

                        ticketOrderTicket.will_call_location_id = Convert.ToInt32(dataReader["TicketHolderWCLocationId"]);
                        ticketOrderTicket.will_call_location_name = Convert.ToString(dataReader["TicketHolderWCLocationName"]);
                        ticketOrderTicket.will_call_location_address = Convert.ToString(dataReader["TicketHolderWCLocationAddress"]);

                        ticketOrderTicket.include_confirmation_message = Convert.ToBoolean(dataReader["IncludeConfirmationMessage"]);
                        ticketOrderTicket.confirmation_message = Convert.ToString(dataReader["ConfirmationMessage"]);

                        ticketOrderTicket.ticket_id = Convert.ToInt32(dataReader["Tickets_Ticket_Id"]);
                        ticketOrderTicket.ticket_barcode = GetBarcode(Convert.ToInt32(dataReader["Winery_Id"]), orderId, ticketOrderTicket.id);

                        DeliveryType = Convert.ToInt32(dataReader["DeliveryType"]);
                        ticketOrderTicket.ticket_type = (TicketType)Convert.ToInt32(dataReader["TicketType"]);

                        listticketOrderTicket.Add(ticketOrderTicket);

                        holder.first_name = Convert.ToString(dataReader["TicketHolderFirstName"]);
                        holder.last_name = Convert.ToString(dataReader["TicketHolderLastName"]);
                        holder.email = Convert.ToString(dataReader["TicketHolderEmail"]);

                        listholder.Add(holder);

                        Total += Convert.ToDecimal(dataReader["TicketPrice"]);
                        GratuityTotal += Convert.ToDecimal(dataReader["Gratuity"]);
                        SalesTaxTotal += Convert.ToDecimal(dataReader["SalesTax"]);
                        TotalServiceFees += Convert.ToDecimal(dataReader["TicketFee"]);
                    }
                }
            }

            tixholder = listholder;
            SubTotal = Total;
            Gratuity = GratuityTotal;
            SalesTax = SalesTaxTotal;
            ServiceFees = TotalServiceFees;

            return listticketOrderTicket;
        }

        public List<TixOrderTicketV2> GetTicketOrderTicketsV2ByOrderId(int orderId, string FirstName, string LastName, string Email, out decimal SubTotal, out decimal ServiceFees, out decimal Gratuity, out decimal SalesTax)
        {

            decimal Total = 0;
            decimal GratuityTotal = 0;
            decimal SalesTaxTotal = 0;
            decimal TotalServiceFees = 0;
            string sp = "Tickets_GetTicketOrderTicketsByOrder";

            var ticketOrderTicket = new TixOrderTicketV2();
            var listticketOrderTicket = new List<TixOrderTicketV2>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@orderId", orderId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    int DeliveryType = -1;
                    while (dataReader.Read())
                    {
                        ticketOrderTicket = new TixOrderTicketV2();
                        ticketOrderTicket.id = Convert.ToInt32(dataReader["Id"]);
                        ticketOrderTicket.delivery_type = (TicketDelivery)Convert.ToInt32(dataReader["DeliveryType"]);
                        ticketOrderTicket.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        ticketOrderTicket.ticket_desc = Convert.ToString(dataReader["TicketDesc"]);
                        ticketOrderTicket.ticket_price = Convert.ToDecimal(dataReader["TicketPrice"]);
                        ticketOrderTicket.validend_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        ticketOrderTicket.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        ticketOrderTicket.first_name = Convert.ToString(dataReader["TicketHolderFirstName"]);
                        ticketOrderTicket.last_name = Convert.ToString(dataReader["TicketHolderLastName"]);
                        ticketOrderTicket.email = Convert.ToString(dataReader["TicketHolderEmail"]);

                        ticketOrderTicket.gratuity = Convert.ToDecimal(dataReader["Gratuity"]);
                        ticketOrderTicket.sales_tax = Convert.ToDecimal(dataReader["SalesTax"]);
                        ticketOrderTicket.ticket_fee = Convert.ToDecimal(dataReader["TicketFee"]);
                        ticketOrderTicket.ticket_qty = Convert.ToInt32(dataReader["TicketQty"]);
                        ticketOrderTicket.post_capture_key = Convert.ToString(dataReader["PostCaptureKey"]);
                        ticketOrderTicket.post_capture_status = (TicketPostCaptureStatus)Convert.ToInt32(dataReader["PostCaptureStatus"]);

                        ticketOrderTicket.fulfillment_lead_time = Convert.ToInt32(dataReader["FulfillmentLeadTime"]);

                        ticketOrderTicket.will_call_location_id = Convert.ToInt32(dataReader["TicketHolderWCLocationId"]);
                        ticketOrderTicket.will_call_location_name = Convert.ToString(dataReader["TicketHolderWCLocationName"]);
                        ticketOrderTicket.will_call_location_address = Convert.ToString(dataReader["TicketHolderWCLocationAddress"]);

                        ticketOrderTicket.ticket_id = Convert.ToInt32(dataReader["Tickets_Ticket_Id"]);
                        ticketOrderTicket.ticket_barcode = GetBarcode(Convert.ToInt32(dataReader["Winery_Id"]), orderId, ticketOrderTicket.id);

                        DeliveryType = Convert.ToInt32(dataReader["DeliveryType"]);
                        ticketOrderTicket.ticket_type = (TicketType)Convert.ToInt32(dataReader["TicketType"]);

                        ticketOrderTicket.company = Convert.ToString(dataReader["TicketHolderCompany"]);
                        ticketOrderTicket.age_group = Convert.ToString(dataReader["TicketHolderAgeGroup"]);
                        ticketOrderTicket.title = Convert.ToString(dataReader["TicketHolderTitle"]);
                        ticketOrderTicket.gender = Convert.ToString(dataReader["TicketHolderGender"]);
                        if (!string.IsNullOrWhiteSpace(dataReader["TicketHolderAge"].ToString()))
                            ticketOrderTicket.age = Convert.ToInt32(dataReader["TicketHolderAge"]);
                        ticketOrderTicket.web_site = Convert.ToString(dataReader["TicketHolderWebsite"]);
                        ticketOrderTicket.work_phone = Convert.ToString(dataReader["TicketHolderWorkPhone"]);
                        ticketOrderTicket.mobile_phone = Convert.ToString(dataReader["TicketHolderMobilePhone"]);
                        if (!string.IsNullOrWhiteSpace(dataReader["TicketHolderBirthDate"].ToString()))
                            ticketOrderTicket.birth_date = Convert.ToDateTime(dataReader["TicketHolderBirthDate"]);

                        ticketOrderTicket.country = Convert.ToString(dataReader["TicketHolderCountry"]);
                        ticketOrderTicket.address1 = Convert.ToString(dataReader["TicketHolderAddress1"]);
                        ticketOrderTicket.address2 = Convert.ToString(dataReader["TicketHolderAddress2"]);
                        ticketOrderTicket.city = Convert.ToString(dataReader["TicketHolderCity"]);
                        ticketOrderTicket.state = Convert.ToString(dataReader["TicketHolderState"]);
                        ticketOrderTicket.zip_code = Convert.ToString(dataReader["TicketHolderZipCode"]);
                        ticketOrderTicket.include_confirmation_message = Convert.ToBoolean(dataReader["IncludeConfirmationMessage"]);
                        ticketOrderTicket.confirmation_message = Convert.ToString(dataReader["ConfirmationMessage"]);
                        listticketOrderTicket.Add(ticketOrderTicket);

                        Total += Convert.ToDecimal(dataReader["TicketPrice"]);
                        GratuityTotal += Convert.ToDecimal(dataReader["Gratuity"]);
                        SalesTaxTotal += Convert.ToDecimal(dataReader["SalesTax"]);
                        TotalServiceFees += Convert.ToDecimal(dataReader["TicketFee"]);
                    }
                }
            }

            SubTotal = Total;
            Gratuity = GratuityTotal;
            SalesTax = SalesTaxTotal;
            ServiceFees = TotalServiceFees;

            return listticketOrderTicket;
        }

        public List<TicketOrderTicket> Tickets_GetTicketOrderTicketsByOrder(int orderId)
        {
            string sp = "Tickets_GetTicketOrderTicketsByOrder";

            var listticketOrderTicket = new List<TicketOrderTicket>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@orderId", orderId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var ticketOrderTicket = new TicketOrderTicket();
                        ticketOrderTicket.DeliveryType = (TicketDelivery)Convert.ToInt32(dataReader["DeliveryType"]);
                        ticketOrderTicket.FirstName = Convert.ToString(dataReader["TicketHolderFirstName"]);
                        ticketOrderTicket.Gratuity = Convert.ToDecimal(dataReader["Gratuity"]);
                        ticketOrderTicket.Id = Convert.ToInt32(dataReader["Id"]);
                        ticketOrderTicket.Winery_Id = Convert.ToInt32(dataReader["Winery_Id"]);
                        ticketOrderTicket.LastName = Convert.ToString(dataReader["TicketHolderLastName"]);
                        ticketOrderTicket.SalesTax = Convert.ToDecimal(dataReader["SalesTax"]);
                        ticketOrderTicket.TicketFee = Convert.ToDecimal(dataReader["TicketFee"]);
                        ticketOrderTicket.TicketName = Convert.ToString(dataReader["TicketName"]);
                        ticketOrderTicket.TicketPrice = Convert.ToDecimal(dataReader["TicketPrice"]);
                        ticketOrderTicket.TicketQty = Convert.ToInt32(dataReader["TicketQty"]);
                        ticketOrderTicket.ValidEndDate = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        ticketOrderTicket.ValidStartDate = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        ticketOrderTicket.PostCaptureStatus = (TicketPostCaptureStatus)Convert.ToInt32(dataReader["PostCaptureStatus"]);
                        ticketOrderTicket.PostCaptureKey = Convert.ToString(dataReader["PostCaptureKey"]);
                        ticketOrderTicket.Email = Convert.ToString(dataReader["TicketHolderEmail"]);
                        ticketOrderTicket.WillCallLcationId = Convert.ToInt32(dataReader["TicketHolderWCLocationId"]);
                        ticketOrderTicket.WillCallLocationName = Convert.ToString(dataReader["TicketHolderWCLocationName"]);
                        ticketOrderTicket.WillCallLocationAddress = Convert.ToString(dataReader["TicketHolderWCLocationAddress"]);
                        ticketOrderTicket.include_confirmation_message = Convert.ToBoolean(dataReader["IncludeConfirmationMessage"]);
                        ticketOrderTicket.confirmation_message = Convert.ToString(dataReader["ConfirmationMessage"]);
                        ticketOrderTicket.fulfillment_lead_time = Convert.ToInt32(dataReader["FulfillmentLeadTime"]);

                        if (!string.IsNullOrWhiteSpace(dataReader["InternalNotificationRecipient"].ToString()))
                        {
                            try
                            {
                                List<int> internalNotificationRecipient = dataReader["InternalNotificationRecipient"].ToString().Split(',').Select(int.Parse).ToList();
                                ticketOrderTicket.internal_notification_recipient = internalNotificationRecipient;
                            }
                            catch { }
                        }

                        int status = Convert.ToInt32(dataReader["Status"]);

                        if (status == 0 || status == 1)
                            listticketOrderTicket.Add(ticketOrderTicket);
                    }
                }
            }
            return listticketOrderTicket;
        }

        public bool TicketsOrderTicketsInsert(TicketLevelForTax ticket_level, int OrderId, TicketHolder ticket_holder, TicketLevelModel tict, decimal itemGratuity, decimal sales_tax_percent, TicketPlan ticketPlan, decimal TicketFee, string discountcode, int PostCaptureStatus, string accessCode,int discountId=0)
        {
            var parameterList = new List<SqlParameter>();
            string attendeeQuestions = "";
            parameterList.Add(GetParameter("@Tickets_Order_Id", OrderId));
            parameterList.Add(GetParameter("@Tickets_Ticket_Id", ticket_level.ticket_id));
            parameterList.Add(GetParameter("@TicketName", tict.ticket_name));
            parameterList.Add(GetParameter("@TicketPrice", ticket_level.price));
            parameterList.Add(GetParameter("@TicketQty", 1));
            parameterList.Add(GetParameter("@TicketFee", TicketFee));
            parameterList.Add(GetParameter("@Status", 0));

            //if (ticket_level.price == tict.price)
            //    parameterList.Add(GetParameter("@PromoCode", ""));
            //else
            //    parameterList.Add(GetParameter("@PromoCode", discountcode));

            parameterList.Add(GetParameter("@PromoCode", discountcode));

            parameterList.Add(GetParameter("@TicketHolderFirstName", ticket_holder.first_name.Trim()));
            parameterList.Add(GetParameter("@TicketHolderLastName", ticket_holder.last_name.Trim()));
            parameterList.Add(GetParameter("@TicketHolderCompany", ticket_holder.company));
            parameterList.Add(GetParameter("@TicketHolderEmail", ticket_holder.email.Trim()));
            parameterList.Add(GetParameter("@ValidStartDate", tict.valid_start_date));
            parameterList.Add(GetParameter("@ValidEndDate", tict.valid_end_date));
            parameterList.Add(GetParameter("@ClaimDate", "1/1/1900"));
            parameterList.Add(GetParameter("@TicketHolderTitle", ticket_holder.title));

            int age = ticket_holder.age ?? 0;

            parameterList.Add(GetParameter("@TicketHolderAge", age));

            parameterList.Add(GetParameter("@TicketHolderWebsite", ticket_holder.website));
            parameterList.Add(GetParameter("@TicketHolderWorkPhone", ticket_holder.work_phone));

            DateTime dob = Convert.ToDateTime("1/1/1900");

            try
            {
                if (!string.IsNullOrEmpty(ticket_holder.dob))
                {
                    DateTime.TryParse(ticket_holder.dob, out dob);
                }
            }
            catch { dob = Convert.ToDateTime("1/1/1900"); }

            parameterList.Add(GetParameter("@TicketHolderBirthDate", dob));
            parameterList.Add(GetParameter("@TicketHolderCountry", ticket_holder.country));
            parameterList.Add(GetParameter("@TicketHolderAddress1", ticket_holder.address1));
            parameterList.Add(GetParameter("@TicketHolderAddress2", ticket_holder.address2));
            parameterList.Add(GetParameter("@TicketHolderCity", ticket_holder.city));
            parameterList.Add(GetParameter("@TicketHolderState", ticket_holder.state));
            parameterList.Add(GetParameter("@TicketHolderZipCode", ticket_holder.postal_code));
            parameterList.Add(GetParameter("@TicketHolderGender", ticket_holder.gender));
            parameterList.Add(GetParameter("@TicketHolderWCLocationId", ticket_holder.will_call_location_id));
            parameterList.Add(GetParameter("@TicketHolderWCLocationName", ticket_holder.will_call_location));
            parameterList.Add(GetParameter("@dateModified", DateTime.UtcNow));
            parameterList.Add(GetParameter("@ModifiedBy", 0));
            parameterList.Add(GetParameter("@AccessCode", accessCode));
            parameterList.Add(GetParameter("@TicketHolderMobilePhone", ticket_holder.mobile_phone));
            parameterList.Add(GetParameter("@PostCaptureKey", ""));
            parameterList.Add(GetParameter("@PostCaptureStatus", PostCaptureStatus));
            parameterList.Add(GetParameter("@TicketHolderAgeGroup", ticket_holder.age_group));
            parameterList.Add(GetParameter("@ZohoRefId", ""));
            parameterList.Add(GetParameter("@LastInviteDate", "1/1/1900"));
            parameterList.Add(GetParameter("@TicketNameOnBadge", tict.ticket_name_on_badge));
            parameterList.Add(GetParameter("@DeliveryType", ticket_holder.delivery_type));
            parameterList.Add(GetParameter("@TicketDesc", tict.ticket_desc));
            parameterList.Add(GetParameter("@PostCaptureDate", "1/1/1900"));
            parameterList.Add(GetParameter("@Gratuity", itemGratuity));
            parameterList.Add(GetParameter("@GratuityPercentage", ticket_level.gratuity_percentage));
            parameterList.Add(GetParameter("@TaxGratuity", ticket_level.tax_gratuity));
            //parameterList.Add(GetParameter("@DiscountId", discountId));

            if (ticket_level.charge_tax && sales_tax_percent > 0)
            {
                decimal taxableGratuityAmt = 0;

                if (ticket_level.tax_gratuity)
                    taxableGratuityAmt = itemGratuity;
                decimal taxperItem = (sales_tax_percent * (ticket_level.price + taxableGratuityAmt)) / 100;

                parameterList.Add(GetParameter("@SalesTax", taxperItem));
                parameterList.Add(GetParameter("@SalesTaxPercentage", sales_tax_percent));
            }
            else
            {
                parameterList.Add(GetParameter("@SalesTax", 0));
                parameterList.Add(GetParameter("@SalesTaxPercentage", 0));
            }

            if (ticket_level.price > 0)
            {

                parameterList.Add(GetParameter("@ExpectedPerTicketFee", ticketPlan.per_ticket_fee));

                decimal ExpectedServiceFee = 0;

                if (TicketFee > 0)
                {
                    ExpectedServiceFee = TicketFee - ticketPlan.per_ticket_fee;
                }
                else
                {
                    ExpectedServiceFee = Math.Round(ticket_level.price * ticketPlan.service_fee, 2, MidpointRounding.AwayFromZero);
                }

                parameterList.Add(GetParameter("@ExpectedServiceFee", ExpectedServiceFee));

            }
            else
            {
                parameterList.Add(GetParameter("@ExpectedPerTicketFee", 0));
                parameterList.Add(GetParameter("@ExpectedServiceFee", 0));
            }
            if (ticket_holder.attendee_questions != null && ticket_holder.attendee_questions.Count > 0)
            {
                attendeeQuestions = JsonConvert.SerializeObject(ticket_holder.attendee_questions);
            }
            parameterList.Add(GetParameter("@AttendeeQuestions", attendeeQuestions));
            ExecuteScalar("TicketsOrderTicketsInsert", parameterList);

            return true;
        }

        public bool TicketsOrderPaymentInsert(int Id, Payments.TransactionResult pr)
        {
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Tickets_Order_Id", Id));
            parameterList.Add(GetParameter("@PaymentDate", DateTime.UtcNow));
            parameterList.Add(GetParameter("@PayType", pr.PayType));
            parameterList.Add(GetParameter("@TransactionType", pr.TransactionType));
            parameterList.Add(GetParameter("@Status", pr.Status));
            parameterList.Add(GetParameter("@ResponseCode", pr.ResponseCode ?? ""));
            parameterList.Add(GetParameter("@ApprovalCode", pr.ApprovalCode ?? ""));
            parameterList.Add(GetParameter("@TransactionId", pr.TransactionID ?? ""));
            parameterList.Add(GetParameter("@AVSResponse", pr.AvsResponse ?? ""));
            parameterList.Add(GetParameter("@Detail", pr.Detail ?? ""));
            parameterList.Add(GetParameter("@Amount", pr.Amount));
            parameterList.Add(GetParameter("@PayCardType", pr.Card.Type ?? ""));

            string PayCardNumber = string.Empty;

            try
            {
                if (pr.Card != null)
                {
                    if (string.IsNullOrEmpty(pr.Card.Number))
                        PayCardNumber = StringHelpers.Encryption(pr.Card.CardLastFourDigits ?? "");
                    else
                        PayCardNumber = StringHelpers.Encryption(pr.Card.Number ?? "");
                }
            }
            catch { }

            //if ((pr.Card.Number ?? "").Trim().Length == 0)
            //    parameterList.Add(GetParameter("@PayCardNumber", StringHelpers.Encryption(pr.Card.CardLastFourDigits ?? "")));
            //else
            //    parameterList.Add(GetParameter("@PayCardNumber", StringHelpers.Encryption(pr.Card.Number ?? "")));

            parameterList.Add(GetParameter("@PayCardNumber", PayCardNumber));
            parameterList.Add(GetParameter("@PayCardCustName", pr.Card.CustName ?? ""));
            parameterList.Add(GetParameter("@PayCardExpMonth", pr.Card.ExpMonth ?? ""));
            parameterList.Add(GetParameter("@PayCardExpYear", pr.Card.ExpYear ?? ""));
            parameterList.Add(GetParameter("@CreatedBy", 0));
            parameterList.Add(GetParameter("@PaymentGateway", pr.PaymentGateway));
            parameterList.Add(GetParameter("@CheckNumber", pr.CheckOrRefNumber ?? ""));
            parameterList.Add(GetParameter("@Change", pr.Change));
            parameterList.Add(GetParameter("@PayCardToken", pr.Card.CardToken ?? ""));

            ExecuteScalar("TicketsOrderPaymentInsert", parameterList);

            return true;
        }

        public int SaveTicketsWaitlist(TicketsWaitlistRequest reqModel, decimal home_phone, Guid orderGuid)
        {
            int Id = 0;

            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Tickets_Event_Id", reqModel.ticket_event_id));
            parameterList.Add(GetParameter("@Tickets_Ticket_Id", reqModel.ticket_ticket_id));
            parameterList.Add(GetParameter("@QtyDesired", reqModel.qty));
            parameterList.Add(GetParameter("@FirstName", reqModel.first_name));
            parameterList.Add(GetParameter("@LastName", reqModel.last_name));
            parameterList.Add(GetParameter("@Email", reqModel.email));
            parameterList.Add(GetParameter("@HomePhone", home_phone));
            parameterList.Add(GetParameter("@WaitlistGUID", orderGuid));
            parameterList.Add(GetParameter("@Note", reqModel.note));

            Id = Convert.ToInt32(ExecuteScalar("InsertTicketsWaitlist", parameterList));

            return Id;

        }

        public int UpdateTicketsWaitlistStatus(TicketsWaitlistUpdateRequest reqModel)
        {
            int Id = 0;

            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Id", reqModel.id));
            parameterList.Add(GetParameter("@Status", reqModel.status));
            parameterList.Add(GetParameter("@WaitlistGUID", reqModel.waitlist_guid));

            ExecuteScalar("UpdateTicketsWaitlistStatus", parameterList);

            return Id;

        }

        public int UpdateTicketsWaitlistNote(TicketsWaitlistUpdateNoteRequest reqModel)
        {
            int Id = 0;

            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Id", reqModel.id));
            parameterList.Add(GetParameter("@Note", reqModel.note));
            parameterList.Add(GetParameter("@WaitlistGUID", reqModel.waitlist_guid));

            ExecuteScalar("UpdateTicketsWaitlistNote", parameterList);

            return Id;

        }

        public int ConvertWaitlistToTicket(string WaitlistGUID, int Tickets_Order_Id, int QtyPurchased, TicketWaitlistStatus status)
        {
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WaitlistGUID", @WaitlistGUID));
            parameterList.Add(GetParameter("@Tickets_Order_Id", Tickets_Order_Id));
            parameterList.Add(GetParameter("@QtyPurchased", QtyPurchased));
            parameterList.Add(GetParameter("@Status", status));

            string sqlQuery = "update Tickets_Waitlist set [Status]=@Status,QtyPurchased=@QtyPurchased,Tickets_Order_Id=@Tickets_Order_Id where cast(WaitlistGUID as varchar(50)) = @WaitlistGUID";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text);
        }


        public int UpdateTicketPostCaptureInvite(TicketPostCaptureInvite reqModel, string postCaptureKey)
        {
            int Id = 0;

            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Id", reqModel.ticket_order_ticket_id));
            parameterList.Add(GetParameter("@PostCaptureStatus", TicketPostCaptureStatus.Invited));
            parameterList.Add(GetParameter("@TicketHolderEmail", reqModel.email));
            parameterList.Add(GetParameter("@TicketHolderFirstName", reqModel.first_name));
            parameterList.Add(GetParameter("@TicketHolderLastName", reqModel.last_name));
            parameterList.Add(GetParameter("@PostCaptureKey", postCaptureKey));


            ExecuteScalar("UpdateTicketOrderTicketDetails", parameterList);

            return Id;

        }
        public int TicketPostCapture(TicketPostCaptureRequest reqModel)
        {
            int Id = 0;
            string attendeeQuestions = "";
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@Id", reqModel.ticket_order_ticket_id));
            parameterList.Add(GetParameter("@PostCaptureStatus", TicketPostCaptureStatus.Claimed));
            parameterList.Add(GetParameter("@TicketHolderAddress1", reqModel.ticket_holder.address1));
            parameterList.Add(GetParameter("@TicketHolderAddress2", reqModel.ticket_holder.address2));
            parameterList.Add(GetParameter("@TicketHolderAge", reqModel.ticket_holder.age));
            parameterList.Add(GetParameter("@TicketHolderAgeGroup", reqModel.ticket_holder.age_group));
            parameterList.Add(GetParameter("@TicketHolderCity", reqModel.ticket_holder.city));
            parameterList.Add(GetParameter("@TicketHolderCompany", reqModel.ticket_holder.company));
            parameterList.Add(GetParameter("@TicketHolderCountry", reqModel.ticket_holder.country));
            parameterList.Add(GetParameter("@DeliveryType", reqModel.ticket_holder.delivery_type));
            parameterList.Add(GetParameter("@TicketHolderBirthDate", reqModel.ticket_holder.dob));
            parameterList.Add(GetParameter("@TicketHolderEmail", reqModel.ticket_holder.email));
            parameterList.Add(GetParameter("@TicketHolderFirstName", reqModel.ticket_holder.first_name));
            parameterList.Add(GetParameter("@TicketHolderGender", reqModel.ticket_holder.gender));
            parameterList.Add(GetParameter("@TicketHolderLastName", reqModel.ticket_holder.last_name));
            parameterList.Add(GetParameter("@TicketHolderMobilePhone", reqModel.ticket_holder.mobile_phone));
            parameterList.Add(GetParameter("@TicketHolderZipCode", reqModel.ticket_holder.postal_code));
            parameterList.Add(GetParameter("@TicketHolderState", reqModel.ticket_holder.state));
            parameterList.Add(GetParameter("@TicketHolderTitle", reqModel.ticket_holder.title));
            parameterList.Add(GetParameter("@TicketHolderWebsite", reqModel.ticket_holder.website));
            parameterList.Add(GetParameter("@TicketHolderWCLocationName", reqModel.ticket_holder.will_call_location));
            parameterList.Add(GetParameter("@TicketHolderWCLocationId", reqModel.ticket_holder.will_call_location_id));
            parameterList.Add(GetParameter("@TicketHolderWorkPhone", reqModel.ticket_holder.work_phone));
            if (reqModel.attendee_questions != null && reqModel.attendee_questions.Count > 0)
            {
                attendeeQuestions = JsonConvert.SerializeObject(reqModel.attendee_questions);
            }
            parameterList.Add(GetParameter("@AttendeeQuestions", attendeeQuestions));
            ExecuteNonQuery("UpdateTicketOrderTicketDetails", parameterList);



            return Id;

        }

        public TicketWaitlistDetail GetTicketsEventByWaitlistId(int WaitlistId = 0, string WaitlistGUID = "")
        {
            string sp = "GetTicketsEventByWaitlistId";

            var waitListDetail = new TicketWaitlistDetail();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WaitlistId", WaitlistId));
            parameterList.Add(GetParameter("@WaitlistGUID", WaitlistGUID));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        waitListDetail.email = Convert.ToString(dataReader["Email"]);
                        waitListDetail.end_date_time = Convert.ToDateTime(dataReader["EndDateTime"]);
                        waitListDetail.event_id = Convert.ToInt32(dataReader["EventId"]);
                        waitListDetail.event_title = Convert.ToString(dataReader["EventTitle"]);
                        waitListDetail.first_name = Convert.ToString(dataReader["FirstName"]);
                        waitListDetail.home_phone = Convert.ToString(dataReader["HomePhone"]);
                        waitListDetail.last_name = Convert.ToString(dataReader["LastName"]);
                        waitListDetail.organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        waitListDetail.price = Convert.ToDecimal(dataReader["Price"]);
                        waitListDetail.qty_desired = Convert.ToInt32(dataReader["QtyDesired"]);
                        waitListDetail.qty_offered = Convert.ToInt32(dataReader["QtyOffered"]);
                        waitListDetail.start_date_time = Convert.ToDateTime(dataReader["StartDateTime"]);
                        waitListDetail.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        waitListDetail.ticket_id = Convert.ToInt32(dataReader["ticket_id"]);
                        //waitListDetail.venue_name = Convert.ToString(dataReader["VenueName"]);
                        waitListDetail.waitlist_expiration = Convert.ToInt32(dataReader["WaitlistExpiration"]);
                        waitListDetail.waitlist_guid = Convert.ToString(dataReader["WaitlistGUID"]);
                        waitListDetail.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        waitListDetail.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        waitListDetail.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        waitListDetail.venue_address = Convert.ToString(dataReader["VenueAddress"]);
                        waitListDetail.event_url = Convert.ToString(dataReader["EventURL"]);
                        waitListDetail.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        waitListDetail.valid_end_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        waitListDetail.organizer_phone = Convert.ToString(dataReader["EventOrganizerPhone"]);
                        waitListDetail.member_country = Convert.ToString(dataReader["Country"]);
                        waitListDetail.member_phone = Convert.ToString(dataReader["BusinessPhone"]);

                        if (!string.IsNullOrWhiteSpace(dataReader["DateOffered"].ToString()))
                        {
                            waitListDetail.date_offered = Convert.ToDateTime(dataReader["DateOffered"]);
                        }
                        waitListDetail.hours_left = Convert.ToInt32(dataReader["HoursLeft"]);
                        waitListDetail.status = (TicketWaitlistStatus)Convert.ToInt32(dataReader["WaitListStatus"]);

                        waitListDetail.note = Convert.ToString(dataReader["Note"]);
                    }
                }
            }


            return waitListDetail;
        }

        public PassportParticipatingMemberModel GetPassportParticipatingMember(int eventId, int member_id)
        {
            PassportParticipatingMemberModel passportParticipatingMemberModel = null;
            string sp = "GetPassportWineryMemberDetail";

            var list = new List<PassportParticipatingMemberModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", eventId));
            parameterList.Add(GetParameter("@WineryId", member_id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        passportParticipatingMemberModel = new PassportParticipatingMemberModel();
                        passportParticipatingMemberModel.benefit_desc = Convert.ToString(dataReader["BenefitDesc"]);
                        passportParticipatingMemberModel.member_appellation_name = Convert.ToString(dataReader["WineryAppellationName"]);
                        passportParticipatingMemberModel.member_city = Convert.ToString(dataReader["City"]);
                        passportParticipatingMemberModel.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        passportParticipatingMemberModel.member_name = Convert.ToString(dataReader["DisplayName"]);
                        passportParticipatingMemberModel.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        passportParticipatingMemberModel.review_count = Convert.ToInt32(dataReader["ReviewCount"]);
                        passportParticipatingMemberModel.review_stars = Convert.ToInt32(dataReader["ReviewStars"]);
                        passportParticipatingMemberModel.thumbnail_image = Convert.ToString(dataReader["thumbnailImage"]);
                        passportParticipatingMemberModel.visitation_rule = (Visitation_Rule)Convert.ToInt32(dataReader["VisitationRule"]);
                        passportParticipatingMemberModel.visitation_external_url = Convert.ToString(dataReader["ExternalURL"]);
                        passportParticipatingMemberModel.geo_latitude = Convert.ToString(dataReader["GeoLatitude"]);
                        passportParticipatingMemberModel.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                        passportParticipatingMemberModel.is_hidden_gem = Convert.ToBoolean(dataReader["IsHiddenGem"]);
                        passportParticipatingMemberModel.is_hidden = Convert.ToBoolean(dataReader["Hidden"]);
                        passportParticipatingMemberModel.is_active = Convert.ToBoolean(dataReader["Active"]);
                        passportParticipatingMemberModel.member_state = Convert.ToString(dataReader["State"]);
                        passportParticipatingMemberModel.passport_event_name = Convert.ToString(dataReader["EventTitle"]);

                        list.Add(passportParticipatingMemberModel);
                    }
                }
            }
            return passportParticipatingMemberModel;
        }

        public List<PassportParticipatingMemberModel> GetPassportWineryMembers(int eventId)
        {
            string sp = "GetPassportWineryMembers";

            var list = new List<PassportParticipatingMemberModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", eventId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var passportParticipatingMemberModel = new PassportParticipatingMemberModel();
                        passportParticipatingMemberModel.benefit_desc = Convert.ToString(dataReader["BenefitDesc"]);
                        passportParticipatingMemberModel.member_appellation_name = Convert.ToString(dataReader["WineryAppellationName"]);
                        passportParticipatingMemberModel.member_city = Convert.ToString(dataReader["City"]);
                        passportParticipatingMemberModel.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        passportParticipatingMemberModel.member_name = Convert.ToString(dataReader["DisplayName"]);
                        passportParticipatingMemberModel.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        passportParticipatingMemberModel.review_count = Convert.ToInt32(dataReader["ReviewCount"]);
                        passportParticipatingMemberModel.review_stars = Convert.ToInt32(dataReader["ReviewStars"]);
                        passportParticipatingMemberModel.thumbnail_image = Convert.ToString(dataReader["thumbnailImage"]);
                        passportParticipatingMemberModel.visitation_rule = (Visitation_Rule)Convert.ToInt32(dataReader["VisitationRule"]);
                        passportParticipatingMemberModel.geo_latitude = Convert.ToString(dataReader["GeoLatitude"]);
                        passportParticipatingMemberModel.geo_longitude = Convert.ToString(dataReader["GeoLongitude"]);
                        passportParticipatingMemberModel.is_hidden_gem = Convert.ToBoolean(dataReader["IsHiddenGem"]);
                        passportParticipatingMemberModel.is_hidden = Convert.ToBoolean(dataReader["Hidden"]);
                        passportParticipatingMemberModel.is_active = Convert.ToBoolean(dataReader["Active"]);
                        passportParticipatingMemberModel.member_state = Convert.ToString(dataReader["State"]);
                        passportParticipatingMemberModel.passport_event_name = Convert.ToString(dataReader["EventTitle"]);

                        list.Add(passportParticipatingMemberModel);
                    }
                }
            }
            return list;
        }

        public TicketLevelModel GetTicketLevelsById(int Id)
        {
            string sp = "Tickets_GetTicketLevelsWithSoldCountById";

            var tict = new TicketLevelModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@Id", Id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        tict.ticket_id = Convert.ToInt32(dataReader["Id"]);
                        tict.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        tict.ticket_name_on_badge = Convert.ToString(dataReader["TicketNameOnBadge"]);
                        tict.price = Convert.ToDecimal(dataReader["Price"]);
                        tict.ticket_desc = Convert.ToString(dataReader["TicketDesc"]);
                        tict.tickets_event_id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                        tict.sale_status = (uc.Common.TicketsSaleStatus)Convert.ToInt32(dataReader["SaleStatus"]);
                        tict.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        tict.valid_end_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        tict.date_time_available = Convert.ToDateTime(dataReader["DateTimeAvailable"]);
                        tict.date_time_unavailable = Convert.ToDateTime(dataReader["DateTimeUnavailable"]);
                        tict.max_available = Convert.ToInt32(dataReader["MaxAvailable"]);
                        tict.min_per_order = Convert.ToInt32(dataReader["MinPerOrder"]);
                        tict.max_per_order = Convert.ToInt32(dataReader["MaxPerOrder"]);
                        tict.show_remaining = Convert.ToInt32(dataReader["ShowRemaining"]);
                        tict.waitlist_enabled = Convert.ToBoolean(dataReader["WaitlistEnabled"]);
                        tict.waitlist_limit_enabled = Convert.ToBoolean(dataReader["WaitlistLimitEnabled"]);
                        tict.waitlist_limit = Convert.ToInt32(dataReader["WaitlistLimit"]);
                        tict.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                        tict.ticket_type = (uc.Common.TicketType)Convert.ToInt32(dataReader["TicketType"]);
                        //tict.charge_tax = Convert.ToBoolean(dataReader["ChargeTax"]);
                        tict.self_print_enabled = Convert.ToBoolean(dataReader["SelfPrint"]);
                        tict.will_call_enabled = Convert.ToBoolean(dataReader["WillCall"]);
                        tict.shipped_enabled = Convert.ToBoolean(dataReader["Shippable"]);
                        tict.gratuity_percentage = Convert.ToDecimal(dataReader["GratuityPercentage"]);
                        tict.tax_gratuity = Convert.ToBoolean(dataReader["TaxGratuity"]);
                        tict.time_zone_id = (uc.Common.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        tict.qty_sold = Convert.ToInt32(dataReader["SoldCount"]);
                        tict.waitlist_qty_sold = Convert.ToInt32(dataReader["WaitlistSoldCount"]);

                        tict.ticket_event_date = Convert.ToDateTime(dataReader["TicketEventDate"]);
                        tict.ticket_fee = Convert.ToDecimal(dataReader["TicketFee"]);
                        tict.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        tict.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        tict.remaining_qty = Convert.ToInt32(dataReader["QtyAvailable"]);

                        tict.remaining_qty_text = "";
                        if (tict.remaining_qty < 1 || tict.min_per_order > tict.remaining_qty)
                            tict.remaining_qty_text = "";
                        else if (tict.remaining_qty <= tict.show_remaining)
                            tict.remaining_qty_text = tict.remaining_qty + " remaining";

                        bool showWaitlist = false;
                        if (tict.waitlist_enabled)
                        {
                            showWaitlist = true;

                            if (tict.waitlist_limit_enabled)
                            {
                                if (tict.waitlist_limit <= tict.waitlist_qty_sold)
                                {
                                    showWaitlist = false;
                                }
                            }
                        }

                        tict.show_waitlist = showWaitlist;

                        Common.Times.TimeZone timeZone = (Common.Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);

                        tict.is_valid = CheckIfTicketStillValid(tict, timeZone);

                    }
                }
            }
            return tict;
        }

        public int UpdateTicketsOrder(int Id, string OrderPaymentDetail, string OrderPaymentTranId = "", int itineraryId = 0)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));
            parameterList.Add(GetParameter("@OrderPaymentDetail", OrderPaymentDetail));
            parameterList.Add(GetParameter("@OrderPaymentTranId", OrderPaymentTranId));
            parameterList.Add(GetParameter("@ItineraryId", itineraryId));

            string sqlQuery = "UpdateTicketOrder";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.StoredProcedure);
        }

        public int UpdateVoidTicketsOrder(int Id)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            string sqlQuery = "update Tickets_Order set OrderStatus = 3,OrderTotal=0,ProcessingFeeTotal=0,InternalNote = InternalNote + 'Order Voided' where Id = @Id";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text);
        }

        public int UpdateProblemTicketsOrder(int Id)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            string sqlQuery = "update Tickets_Order set OrderStatus = 2,OrderTotal=0,ProcessingFeeTotal=0,InternalNote = InternalNote + 'Problem Order' where Id = @Id";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text);
        }

        public List<FAQ> GetFAQByEventType(int eventType)
        {
            List<FAQ> faq = new List<FAQ>();
            string sp = "select [Question],[Answer] from [FAQ] where [Type] = @eventType or @eventType = 0";

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@eventType", eventType));
            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        faq.Add(new FAQ
                        {
                            question = Convert.ToString(dataReader["Question"]),
                            answer = Convert.ToString(dataReader["Answer"])
                        });
                    }

                }
            }
            return faq;

        }

        public List<MemberFaqModel> GetFAQByMemberId(int memberId)
        {
            List<MemberFaqModel> faq = new List<MemberFaqModel>();
            string sp = "GetMemberFaq";

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@memberId", memberId));
            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        faq.Add(new MemberFaqModel
                        {
                            question = Convert.ToString(dataReader["Question"]),
                            answer = Convert.ToString(dataReader["Answer"]),
                            business_id = Convert.ToInt32(dataReader["WineryId"])
                        });
                    }

                }
            }
            return faq;

        }

        public decimal GetAmountBytransactionId(string transactionId, int OrderId)
        {
            decimal Amount = 0;
            string sp = "select Amount from Tickets_Order_Payment where Tickets_Order_Id=@OrderId and TransactionId=@transactionId and TransactionType=2 and Status=1";

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@transactionId", transactionId));
            parameterList.Add(GetParameter("@OrderId", OrderId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Amount = Convert.ToDecimal(dataReader["Amount"]);
                    }
                }
            }
            return Amount;
        }

        public ContactReason GetContactReason()
        {
            ContactReason contactReason = new ContactReason();
            List<string> reason = new List<string>();
            string sp = "SELECT [Reason] FROM [dbo].[ContactReason]";

            using (DbDataReader dataReader = GetDataReader(sp, null, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        reason.Add(Convert.ToString(dataReader["Reason"]));
                    }

                }
            }
            contactReason.contactreason = reason;
            return contactReason;

        }

        public TicketOrderClaimModel GetTicketOrderTicketsByPostCaptureKeyandId(int id, string postCaptureKey)
        {

            decimal Total = 0;
            decimal GratuityTotal = 0;
            decimal SalesTaxTotal = 0;
            decimal TotalServiceFees = 0;
            int DeliveryType = -1;

            string sp = "GetTicketOrderTicketByPostCaptureKeyandId";

            var ticketOrder = new TicketOrderClaimModel();

            var ticketOrderTicket = new TixOrderTicket();
            var holder = new TixOrderHolder();
            var parameterList = new List<DbParameter>();
            var listQuestions = new List<TicketQuestion>();
            var willCallLocations = new List<WillCallLocation>();
            var questionAnswers = new List<TicketQandA>();


            parameterList.Add(GetParameter("@Id", id));

            parameterList.Add(GetParameter("@PostCaptureKey", postCaptureKey));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ticketOrder.id = Convert.ToInt32(dataReader["Id"]);
                        ticketOrder.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        ticketOrder.tickets_event_id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                        ticketOrder.event_start_date_time = Convert.ToDateTime(dataReader["EventStartDateTime"]);
                        ticketOrder.event_end_date_time = Convert.ToDateTime(dataReader["EventEndDateTime"]);
                        ticketOrder.order_total = Convert.ToDecimal(dataReader["OrderTotal"]);
                        ticketOrder.order_date = Convert.ToDateTime(dataReader["OrderDate"]);
                        ticketOrder.bill_first_name = Convert.ToString(dataReader["BillFirstName"]);
                        ticketOrder.bill_last_name = Convert.ToString(dataReader["BillLastName"]);
                        ticketOrder.bill_email_address = Convert.ToString(dataReader["BillEmail"]);
                        ticketOrder.bill_home_phone = Convert.ToString(dataReader["BillHomePhone"]);
                        ticketOrder.bill_country = Convert.ToString(dataReader["BillCountry"]);
                        ticketOrder.event_title = Convert.ToString(dataReader["EventTitle"]);
                        //ticketOrder.event_venue_name = Convert.ToString(dataReader["EventVenueName"]);
                        ticketOrder.event_venue_address = Convert.ToString(dataReader["EventVenueAddress"]);
                        ticketOrder.order_guid = Convert.ToString(dataReader["OrderGUID"]);
                        ticketOrder.member_name = Convert.ToString(dataReader["DisplayName"]);
                        ticketOrder.dynamic_payment_desc = Convert.ToString(dataReader["DynamicPaymentDesc"]);
                        ticketOrder.order_note = Convert.ToString(dataReader["OrderNote"]);
                        ticketOrder.event_organizer_email = Convert.ToString(dataReader["EventOrganizerEmail"]);
                        //tEvent.event_organizer_url = Convert.ToString(dataReader["WebsiteURL"]);
                        ticketOrder.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        ticketOrder.event_organizer_phone = Convert.ToString(dataReader["EventOrganizerPhone"]);
                        ticketOrder.payment_type = (PaymentType)Convert.ToInt32(dataReader["PaymentType"]);

                        ticketOrder.currency_symbol = Convert.ToString(dataReader["CurrencySymbol"]);
                        ticketOrder.currency_code = Convert.ToString(dataReader["CurrencyCode"]);
                        ticketOrder.member_url = Convert.ToString(dataReader["PurchaseURL"]);
                        ticketOrder.timezone = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                        ticketOrder.timezone_name = GetTimezoneNameById(ticketOrder.timezone);
                        ticketOrder.timezone_offset = Times.GetOffsetMinutes(ticketOrder.timezone);

                        ticketOrder.event_image = Convert.ToString(dataReader["EventImage"]);
                        ticketOrder.event_image_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        ticketOrder.ad_image = Convert.ToString(dataReader["AdImage"]);
                        ticketOrder.event_image_big = Convert.ToString(dataReader["EventImageBig"]);
                        ticketOrder.event_image_big_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        ticketOrder.processing_fee = Convert.ToDecimal(dataReader["ProcessingFee"]);
                        ticketOrder.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        ticketOrder.venue_state = Convert.ToString(dataReader["VenueState"]);
                        ticketOrder.venue_zip = Convert.ToString(dataReader["VenueZip"]);



                    }

                    if (dataReader.NextResult())
                    {

                        ticketOrderTicket = new TixOrderTicket();
                        while (dataReader.Read())
                        {
                            ticketOrderTicket.id = Convert.ToInt32(dataReader["Id"]);
                            ticketOrderTicket.delivery_type = (TicketDelivery)Convert.ToInt32(dataReader["DeliveryType"]);
                            ticketOrderTicket.ticket_name = Convert.ToString(dataReader["TicketName"]);
                            ticketOrderTicket.ticket_desc = Convert.ToString(dataReader["TicketDesc"]);
                            ticketOrderTicket.ticket_price = Convert.ToDecimal(dataReader["TicketPrice"]);
                            ticketOrderTicket.validend_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                            ticketOrderTicket.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                            ticketOrderTicket.first_name = Convert.ToString(dataReader["TicketHolderFirstName"]);
                            ticketOrderTicket.last_name = Convert.ToString(dataReader["TicketHolderLastName"]);
                            ticketOrderTicket.email = Convert.ToString(dataReader["TicketHolderEmail"]);

                            ticketOrderTicket.gratuity = Convert.ToDecimal(dataReader["Gratuity"]);
                            ticketOrderTicket.sales_tax = Convert.ToDecimal(dataReader["SalesTax"]);
                            ticketOrderTicket.ticket_fee = Convert.ToDecimal(dataReader["TicketFee"]);
                            ticketOrderTicket.ticket_qty = Convert.ToInt32(dataReader["TicketQty"]);
                            ticketOrderTicket.post_capture_key = Convert.ToString(dataReader["PostCaptureKey"]);
                            ticketOrderTicket.post_capture_status = (TicketPostCaptureStatus)Convert.ToInt32(dataReader["PostCaptureStatus"]);

                            ticketOrderTicket.will_call_location_id = Convert.ToInt32(dataReader["TicketHolderWCLocationId"]);
                            ticketOrderTicket.will_call_location_name = Convert.ToString(dataReader["TicketHolderWCLocationName"]);
                            ticketOrderTicket.fulfillment_lead_time = Convert.ToInt32(dataReader["FulfillmentLeadTime"]);

                            holder.first_name = Convert.ToString(dataReader["TicketHolderFirstName"]);
                            holder.last_name = Convert.ToString(dataReader["TicketHolderLastName"]);
                            holder.email = Convert.ToString(dataReader["TicketHolderEmail"]);
                            holder.company = Convert.ToString(dataReader["TicketHolderCompany"]);
                            holder.age_group = Convert.ToString(dataReader["TicketHolderAgeGroup"]);
                            holder.title = Convert.ToString(dataReader["TicketHolderTitle"]);
                            holder.gender = Convert.ToString(dataReader["TicketHolderGender"]);
                            if (!string.IsNullOrWhiteSpace(dataReader["TicketHolderAge"].ToString()))
                                holder.age = Convert.ToInt32(dataReader["TicketHolderAge"]);
                            holder.web_site = Convert.ToString(dataReader["TicketHolderWebsite"]);
                            holder.work_phone = Convert.ToString(dataReader["TicketHolderWorkPhone"]);
                            holder.mobile_phone = Convert.ToString(dataReader["TicketHolderMobilePhone"]);
                            if (!string.IsNullOrWhiteSpace(dataReader["TicketHolderBirthDate"].ToString()))
                                holder.birth_date = Convert.ToDateTime(dataReader["TicketHolderBirthDate"]);

                            holder.country = Convert.ToString(dataReader["TicketHolderCountry"]);
                            holder.address1 = Convert.ToString(dataReader["TicketHolderAddress1"]);
                            holder.address2 = Convert.ToString(dataReader["TicketHolderAddress2"]);
                            holder.city = Convert.ToString(dataReader["TicketHolderCity"]);
                            holder.state = Convert.ToString(dataReader["TicketHolderState"]);
                            holder.zip_code = Convert.ToString(dataReader["TicketHolderZipCode"]);

                            Total += Convert.ToDecimal(dataReader["TicketPrice"]);
                            GratuityTotal += Convert.ToDecimal(dataReader["Gratuity"]);
                            SalesTaxTotal += Convert.ToDecimal(dataReader["SalesTax"]);
                            TotalServiceFees += Convert.ToDecimal(dataReader["TicketFee"]);

                        }


                    }
                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var ticketQuestion = new TicketQuestion();
                            List<string> lstChoices = null;
                            var choices = Convert.ToString(dataReader["Choices"]);
                            if (!string.IsNullOrEmpty(choices))
                            {
                                lstChoices = choices.Split("@!@").ToList();
                            }
                            ticketQuestion.question_id = Convert.ToInt32(dataReader["Id"]);
                            ticketQuestion.question_type = Convert.ToString(dataReader["QuestionType"]);
                            ticketQuestion.question_text = Convert.ToString(dataReader["QuestionText"]);
                            ticketQuestion.is_required = Convert.ToBoolean(dataReader["IsRequired"]);
                            ticketQuestion.is_default_state = Convert.ToBoolean(dataReader["DefaultState"]);
                            ticketQuestion.choices = lstChoices;
                            listQuestions.Add(ticketQuestion);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var wcLocation = new WillCallLocation();

                            wcLocation.id = Convert.ToInt32(dataReader["Id"]);
                            wcLocation.will_call_location = Convert.ToString(dataReader["WCLocation"]);
                            willCallLocations.Add(wcLocation);
                        }
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var qandA = new TicketQandA();

                            qandA.question_text = Convert.ToString(dataReader["Question"]);
                            qandA.choice = Convert.ToString(dataReader["Choice"]);
                            questionAnswers.Add(qandA);
                        }
                    }
                    ticketOrder.ticket_details = ticketOrderTicket;
                    ticketOrder.attendee_details = holder;
                    ticketOrder.attendee_questions = listQuestions;
                    ticketOrder.will_call_locations = willCallLocations;
                    ticketOrder.question_answers = questionAnswers;
                }
            }

            return ticketOrder;
        }

        public bool CheckUserWaitListedForTicket(int eventId, int ticketId, string email)
        {
            string sp = "CheckUserWaitListedForTicket";
            bool isExists = false;
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@eventId", eventId));
            parameterList.Add(GetParameter("@ticketId", ticketId));
            parameterList.Add(GetParameter("@email", email));

            isExists = (bool)ExecuteScalar(sp, parameterList);

            return isExists;

        }

        public List<EventTicketHolder> GetEventTicketHoldersForReview(int ReminderHours, DateTime CurrentDate, int id)
        {
            string sp = "Task_GetEventTicketHoldersForReview";

            var eventS = new List<EventTicketHolder>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@ReminderInterval", ReminderHours));
            parameterList.Add(GetParameter("@CurrentDate", CurrentDate));
            parameterList.Add(GetParameter("@id", id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tEvent = new EventTicketHolder();
                        tEvent.Id = Convert.ToInt32(dataReader["Id"]);
                        tEvent.ClaimDate = Convert.ToDateTime(dataReader["ClaimDate"]);
                        tEvent.EventTitle = Convert.ToString(dataReader["EventTitle"]);
                        //tEvent.EventVenueName = Convert.ToString(dataReader["EventVenueName"]);
                        tEvent.TicketHolderEmail = Convert.ToString(dataReader["TicketHolderEmail"]);
                        tEvent.TicketHolderFirstName = Convert.ToString(dataReader["TicketHolderFirstName"]);
                        tEvent.TicketHolderLastName = Convert.ToString(dataReader["TicketHolderLastName"]);
                        tEvent.ValidStartDate = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        tEvent.TicketsOrderTicketGUID = Convert.ToString(dataReader["TicketsOrderTicketGUID"]);

                        eventS.Add(tEvent);
                    }
                }
            }
            return eventS;
        }

        public void UpdateReviewInviteSentForEvent(int Tickets_Order_Ticket_Id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Tickets_Order_Ticket_Id", Tickets_Order_Ticket_Id));
            parameterList.Add(GetParameter("@InviteDate", DateTime.Now));

            string sql = "INSERT INTO [dbo].[EventReviewInviteLog] ([Tickets_Order_Ticket_Id],[InviteDate]) VALUES (@Tickets_Order_Ticket_Id,@InviteDate)";
            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }

        public bool GetGuestlistIdById(Guid GuestGUID, string guest_lists)
        {
            guest_lists = "," + guest_lists + ",";
            bool guestlistAccepted = false;
            string sql = "select Guestlist_Id from Guestlist_Guests g join Guestlist l On g.Guestlist_Id = l.Id where g.GuestGUID = @GuestGUID";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@GuestGUID", GuestGUID));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        if (guest_lists.IndexOf(Convert.ToInt32(dataReader["Guestlist_Id"]).ToString()) > -1)
                            guestlistAccepted = true;
                    }
                }
            }
            return guestlistAccepted;
        }

        public bool CheckExistsGuestlist(int Guestlist_Id,string Email)
        {
            int Id = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Guestlist_Id", Guestlist_Id));
            parameterList.Add(GetParameter("@Email", Email));

            string sql = "select Id from Guestlist_Guests where Guestlist_Id = @Guestlist_Id and Email = @Email";

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Id = Convert.ToInt32(dataReader["Id"]);
                    }
                }
            }
            return Id > 0;
        }

        public string GetThirdParty_AccountTypeName(int Id)
        {
            string AccountTypeName = "";
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            string sql = "select ContactType from ThirdParty_AccountTypes where id = @Id";

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        AccountTypeName = Convert.ToString(dataReader["ContactType"]);
                    }
                }
            }
            return AccountTypeName;
        }

        public int SaveTicketsAbandoned(int User_Id, string Email, int Event_Id, int Member_Id, string AcccessCode, string Promo, Guid CartGUID)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@User_Id", User_Id));
            parameterList.Add(GetParameter("@Email", Email));
            parameterList.Add(GetParameter("@Member_Id", Member_Id));
            parameterList.Add(GetParameter("@Event_Id", Event_Id));
            parameterList.Add(GetParameter("@AcccessCode", AcccessCode));
            parameterList.Add(GetParameter("@Promo", Promo));
            parameterList.Add(GetParameter("@OrderGUID", CartGUID));

            int retvalue = Convert.ToInt32(ExecuteScalar("InsertTickets_Abandoned", parameterList));
            return retvalue;
        }

        public bool DeleteTicketsAbandoned(string Email, int Event_Id)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@Email", Email));
            parameterList.Add(GetParameter("@Event_Id", Event_Id));

            ExecuteScalar("DeleteTickets_Abandoned", parameterList);
            return true;
        }

        public bool UnsubscribeEvent(int userId, int eventId)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@eventId", eventId));

            ExecuteScalar("UnsubscribeEvent", parameterList);
            return true;
        }

        public int ConvertAbandonedCart(Guid CartGuid, int userId, string email, int orderId)
        {
            List<UserDetailModel> model = new List<UserDetailModel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@CartGuid", CartGuid));
            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@email", email));
            parameterList.Add(GetParameter("@orderId", orderId));

            string sqlQuery = "update Tickets_Abandoned set User_Id = @userId ,Email = @email,ConvertedOrderId = @orderId where OrderGUID = @CartGuid";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text);
        }

        public ActivatePassportModel GetInviteKeyByClaimCode(string claimCode, TicketPostCaptureStatus captureStatus)
        {
            ActivatePassportModel activatePassportModel = new ActivatePassportModel();
            string sql = "select PostCaptureKey,Tickets_Event_Id from Tickets_Order_Tickets tot join Tickets_Order o on o.id=tot.Tickets_Order_Id where PostCaptureStatus = @captureStatus and Substring(replace(PostCaptureKey,'-',''),17,16) = Right(replace(@claimCode,'-',''),16)";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@captureStatus", (int)captureStatus));
            parameterList.Add(GetParameter("@claimCode", claimCode));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        activatePassportModel.post_capture_key = Convert.ToString(dataReader["PostCaptureKey"]);
                        activatePassportModel.event_id = Convert.ToInt32(dataReader["Tickets_Event_Id"]);
                    }
                }
            }
            return activatePassportModel;
        }

        public List<TicketWillCallLocation> GetTicketWillCallLocations(int ticketId)
        {
            var ticketWillCallLocation = new List<TicketWillCallLocation>();
            string sql = "select Id,Tickets_Event_WillCallLocation_Id,WillCallLimit from Tickets_Ticket_WillCallLocation where Tickets_Ticket_Id = @ticketId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ticketId", ticketId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tWillCallLocation = new TicketWillCallLocation();
                        tWillCallLocation.Id = Convert.ToInt32(dataReader["Id"]);
                        tWillCallLocation.Tickets_Event_WillCallLocation_Id = Convert.ToInt32(dataReader["Tickets_Event_WillCallLocation_Id"]);
                        tWillCallLocation.WillCallLimit = Convert.ToInt32(dataReader["WillCallLimit"]);
                        tWillCallLocation.Id = ticketId;

                        ticketWillCallLocation.Add(tWillCallLocation);
                    }
                }
            }
            return ticketWillCallLocation;
        }

        public string GetTicketWillCallLocationsByLocationId(int locationId)
        {
            string ticketWillCallLocation = "";
            WillCallLocations tWillCallLocation = new WillCallLocations();
            string sql = "select Id,LocationName,Address1,Address2,City,State,Zip from Location where Id = @locationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@locationId", locationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        tWillCallLocation.Id = Convert.ToInt32(dataReader["Id"]);
                        tWillCallLocation.LocationName = Convert.ToString(dataReader["LocationName"] == null ? "" : dataReader["LocationName"]);
                        tWillCallLocation.Address1 = Convert.ToString(dataReader["Address1"] == null ? "" : dataReader["Address1"]);
                        tWillCallLocation.Address2 = Convert.ToString(dataReader["Address2"] == null ? "" : dataReader["Address2"]);
                        tWillCallLocation.City = Convert.ToString(dataReader["City"] == null ? "" : dataReader["City"]);
                        tWillCallLocation.State = Convert.ToString(dataReader["State"] == null ? "" : dataReader["State"]);
                        tWillCallLocation.Zip = Convert.ToString(dataReader["Zip"] == null ? "" : dataReader["Zip"]);
                    }
                }
            }

            if (tWillCallLocation.LocationName != "")
                ticketWillCallLocation += tWillCallLocation.LocationName + "<br />";

            if (tWillCallLocation.Address1 != "" && tWillCallLocation.Address1.Length > 0)
                ticketWillCallLocation += tWillCallLocation.Address1;

            if ((tWillCallLocation.Address2 != "" && tWillCallLocation.Address2.Length > 0) && (tWillCallLocation.Address1 != "" && tWillCallLocation.Address1.Length > 0))
                ticketWillCallLocation += ", " + tWillCallLocation.Address2;
            else
                ticketWillCallLocation += "" + tWillCallLocation.Address2;

            if (tWillCallLocation.City != "" && tWillCallLocation.City.Length > 0)
                ticketWillCallLocation += "<br />" + tWillCallLocation.City;

            if (tWillCallLocation.State != "" && tWillCallLocation.State.Length > 0)
                ticketWillCallLocation += ", " + tWillCallLocation.State;

            if (tWillCallLocation.Zip != "" && tWillCallLocation.Zip.Length > 0)
                ticketWillCallLocation += ", " + tWillCallLocation.Zip;

            return ticketWillCallLocation;
        }

        public string GetTicketWillCallLocationNameByLocationId(int locationId)
        {
            string ticketWillCallLocation = "";
            WillCallLocations tWillCallLocation = new WillCallLocations();
            string sql = "select Id,LocationName,Address1,Address2,City,State,Zip from Location where Id = @locationId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@locationId", locationId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        tWillCallLocation.Id = Convert.ToInt32(dataReader["Id"]);
                        tWillCallLocation.LocationName = Convert.ToString(dataReader["LocationName"] == null ? "" : dataReader["LocationName"]);
                        tWillCallLocation.Address1 = Convert.ToString(dataReader["Address1"] == null ? "" : dataReader["Address1"]);
                        tWillCallLocation.Address2 = Convert.ToString(dataReader["Address2"] == null ? "" : dataReader["Address2"]);
                        tWillCallLocation.City = Convert.ToString(dataReader["City"] == null ? "" : dataReader["City"]);
                        tWillCallLocation.State = Convert.ToString(dataReader["State"] == null ? "" : dataReader["State"]);
                        tWillCallLocation.Zip = Convert.ToString(dataReader["Zip"] == null ? "" : dataReader["Zip"]);
                    }
                }
            }

            if (tWillCallLocation.LocationName != "")
                ticketWillCallLocation += tWillCallLocation.LocationName;

            return ticketWillCallLocation;
        }

        public List<WillCallAllocation> GetTicketsAllocatedByWillCallLocation(int TicketsTicketId)
        {
            string sp = "GetTicketsAllocatedByWillCallLocation";

            var list = new List<WillCallAllocation>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@TicketsTicketId", TicketsTicketId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var willCallAllocation = new WillCallAllocation();

                        willCallAllocation.LocationId = Convert.ToInt32(dataReader["TicketHolderWCLocationId"]);
                        willCallAllocation.Allocated = Convert.ToInt32(dataReader["ticketCount"]);
                        willCallAllocation.TicketId = TicketsTicketId;

                        list.Add(willCallAllocation);
                    }
                }
            }
            return list;
        }

        public int UpdateUpcomingEventsData(int Id)
        {
            var parameterList = new List<SqlParameter>();

            parameterList.Add(GetParameter("@Id", Id));

            ExecuteScalar("UpdateUpcomingEventsData", parameterList);

            return Id;

        }

        public List<TicketEventReminderMOdel> GetTicketEventReminders()
        {
            string sp = "GetTicketEventReminderList";

            var tictModel = new List<TicketEventReminderMOdel>();
            var parameterList = new List<DbParameter>();


            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tict = new TicketEventReminderMOdel();
                        tict.queue_id = Convert.ToInt32(dataReader["QueueId"]);
                        tict.event_title = Convert.ToString(dataReader["EventTitle"]);
                        tict.event_id = Convert.ToInt32(dataReader["EventId"]);
                        tict.member_id = Convert.ToInt32(dataReader["Winery_Id"]);

                        tictModel.Add(tict);
                    }
                }
            }
            return tictModel;
        }

        public int UpdateReminderQueueStatus(int queueId, string errorMessage = "")
        {
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@QueueId", queueId));
            parameterList.Add(GetParameter("@ProcessDate", DateTime.UtcNow));
            parameterList.Add(GetParameter("@ErrorMessage", errorMessage));

            string sqlQuery = "update Tickets_Event_Reminder_Queue set [IsProcessed]=1,DateProcessed=@ProcessDate,ErrorMessage=@ErrorMessage where Id=@QueueId";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text);

        }

        public int InsertTicketFollowEventReminderLog(int eventId, int userId, string errorMessage = "")
        {
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@eventId", eventId));
            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@errorMessage", errorMessage));

            string sqlQuery = "InsertTicketsEventReminderLog";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.StoredProcedure);

        }
        public List<TicketEventUserNotificationMOdel> GetFollowingUsersForEventNotification(int eventId)
        {
            string sp = "GetUserListForReminderByMember";

            var tictModel = new List<TicketEventUserNotificationMOdel>();
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@eventId", eventId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tict = new TicketEventUserNotificationMOdel();
                        tict.user_id = Convert.ToInt32(dataReader["UserId"]);
                        tict.first_name = Convert.ToString(dataReader["FirstName"]);
                        tict.last_name = Convert.ToString(dataReader["LastName"]);
                        tict.email = Convert.ToString(dataReader["UserName"]);

                        tictModel.Add(tict);
                    }
                }
            }
            return tictModel;

        }
        //public int GetVenueLocationIdByEventId(int EventId)
        //{
        //    int LocationId = 0;
        //    string sql = "select VenueLocationId from Tickets_Event where id=@EventId";

        //    var parameterList = new List<DbParameter>();
        //    parameterList.Add(GetParameter("@EventId", EventId));

        //    using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
        //    {
        //        if (dataReader != null && dataReader.HasRows)
        //        {
        //            while (dataReader.Read())
        //            {
        //                LocationId = Convert.ToInt32(dataReader["VenueLocationId"]);
        //            }
        //        }
        //    }
        //    return LocationId;
        //}

        public TixOrderTicketV2 GetTicketDetailsByTicketGUID(string tickeGUID)
        {
            string sp = "GetTicketDetailsByGUID";

            var ticketOrderTicket = new TixOrderTicketV2();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@TicketGUID", tickeGUID));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    int DeliveryType = -1;
                    while (dataReader.Read())
                    {
                        ticketOrderTicket = new TixOrderTicketV2();
                        ticketOrderTicket.id = Convert.ToInt32(dataReader["Id"]);
                        ticketOrderTicket.tickets_order_id = Convert.ToInt32(dataReader["Tickets_Order_Id"]);
                        ticketOrderTicket.delivery_type = (TicketDelivery)Convert.ToInt32(dataReader["DeliveryType"]);
                        ticketOrderTicket.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        ticketOrderTicket.ticket_desc = Convert.ToString(dataReader["TicketDesc"]);
                        ticketOrderTicket.ticket_price = Convert.ToDecimal(dataReader["TicketPrice"]);
                        ticketOrderTicket.validend_date = Convert.ToDateTime(dataReader["ValidEndDate"]);
                        ticketOrderTicket.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        ticketOrderTicket.first_name = Convert.ToString(dataReader["TicketHolderFirstName"]);
                        ticketOrderTicket.last_name = Convert.ToString(dataReader["TicketHolderLastName"]);
                        ticketOrderTicket.email = Convert.ToString(dataReader["TicketHolderEmail"]);

                        ticketOrderTicket.gratuity = Convert.ToDecimal(dataReader["Gratuity"]);
                        ticketOrderTicket.sales_tax = Convert.ToDecimal(dataReader["SalesTax"]);
                        ticketOrderTicket.ticket_fee = Convert.ToDecimal(dataReader["TicketFee"]);
                        ticketOrderTicket.ticket_qty = Convert.ToInt32(dataReader["TicketQty"]);
                        ticketOrderTicket.post_capture_key = Convert.ToString(dataReader["PostCaptureKey"]);
                        ticketOrderTicket.post_capture_status = (TicketPostCaptureStatus)Convert.ToInt32(dataReader["PostCaptureStatus"]);

                        ticketOrderTicket.fulfillment_lead_time = Convert.ToInt32(dataReader["FulfillmentLeadTime"]);

                        ticketOrderTicket.will_call_location_id = Convert.ToInt32(dataReader["TicketHolderWCLocationId"]);
                        ticketOrderTicket.will_call_location_name = Convert.ToString(dataReader["TicketHolderWCLocationName"]);

                        ticketOrderTicket.ticket_id = Convert.ToInt32(dataReader["Tickets_Ticket_Id"]);
                        ticketOrderTicket.ticket_barcode = GetBarcode(Convert.ToInt32(dataReader["Winery_Id"]), ticketOrderTicket.tickets_order_id, ticketOrderTicket.id);

                        DeliveryType = Convert.ToInt32(dataReader["DeliveryType"]);
                        ticketOrderTicket.ticket_type = (TicketType)Convert.ToInt32(dataReader["TicketType"]);

                        ticketOrderTicket.company = Convert.ToString(dataReader["TicketHolderCompany"]);
                        ticketOrderTicket.age_group = Convert.ToString(dataReader["TicketHolderAgeGroup"]);
                        ticketOrderTicket.title = Convert.ToString(dataReader["TicketHolderTitle"]);
                        ticketOrderTicket.gender = Convert.ToString(dataReader["TicketHolderGender"]);
                        if (!string.IsNullOrWhiteSpace(dataReader["TicketHolderAge"].ToString()))
                            ticketOrderTicket.age = Convert.ToInt32(dataReader["TicketHolderAge"]);
                        ticketOrderTicket.web_site = Convert.ToString(dataReader["TicketHolderWebsite"]);
                        ticketOrderTicket.work_phone = Convert.ToString(dataReader["TicketHolderWorkPhone"]);
                        ticketOrderTicket.mobile_phone = Convert.ToString(dataReader["TicketHolderMobilePhone"]);
                        if (!string.IsNullOrWhiteSpace(dataReader["TicketHolderBirthDate"].ToString()))
                            ticketOrderTicket.birth_date = Convert.ToDateTime(dataReader["TicketHolderBirthDate"]);

                        ticketOrderTicket.country = Convert.ToString(dataReader["TicketHolderCountry"]);
                        ticketOrderTicket.address1 = Convert.ToString(dataReader["TicketHolderAddress1"]);
                        ticketOrderTicket.address2 = Convert.ToString(dataReader["TicketHolderAddress2"]);
                        ticketOrderTicket.city = Convert.ToString(dataReader["TicketHolderCity"]);
                        ticketOrderTicket.state = Convert.ToString(dataReader["TicketHolderState"]);
                        ticketOrderTicket.zip_code = Convert.ToString(dataReader["TicketHolderZipCode"]);
                        ticketOrderTicket.include_confirmation_message = Convert.ToBoolean(dataReader["IncludeConfirmationMessage"]);
                        ticketOrderTicket.confirmation_message = Convert.ToString(dataReader["ConfirmationMessage"]);
                        ticketOrderTicket.event_title = Convert.ToString(dataReader["EventTitle"]);
                        ticketOrderTicket.winery_name = Convert.ToString(dataReader["WineryName"]);
                        ticketOrderTicket.event_start_time = Convert.ToString(dataReader["EventStartTime"]);
                        ticketOrderTicket.member_url = Convert.ToString(dataReader["PurchaseURL"]);

                        string Regions = Convert.ToString(dataReader["Regions"]);
                        if (Regions.Length > 0)
                        {
                            Regions = "[" + Regions + "]";
                            ticketOrderTicket.regions_ids = JsonConvert.DeserializeObject<List<int>>(Regions);
                        }
                    }
                }
            }

            return ticketOrderTicket;
        }

        public bool SaveTicketEventReview(TicketEventReviewRequest request)
        {
            string sqlQuery = "InsertTicketEventReview";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Ticket_Guid", request.ticket_guid));
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
            parameterList.Add(GetParameter("@EmailId", request.email));
            parameterList.Add(GetParameter("@IsFavorite", request.is_favorite));
            int ret = ExecuteNonQuery(sqlQuery, parameterList, CommandType.StoredProcedure);

            return (ret > 0);
        }

        public List<EventReviewModel> GetEventReviews()
        {
            var eventReviewModelList = new List<EventReviewModel>();
            var parameterList = new List<DbParameter>();
            using (DbDataReader dataReader = GetDataReader("GetTopTicketEventReviews", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var eventReview = new EventReviewModel();
                        eventReview.id = Convert.ToInt32(dataReader["Id"]);
                        eventReview.active = Convert.ToBoolean(dataReader["Active"]);
                        eventReview.winery_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        eventReview.metric1 = Convert.ToInt32(dataReader["Metric1"]);
                        eventReview.metric2 = Convert.ToInt32(dataReader["Metric2"]);
                        eventReview.metric3 = Convert.ToInt32(dataReader["Metric3"]);
                        eventReview.metric4 = Convert.ToInt32(dataReader["Metric4"]);
                        eventReview.metric5 = Convert.ToInt32(dataReader["Metric5"]);
                        eventReview.metric6 = Convert.ToInt32(dataReader["Metric6"]);
                        eventReview.description = Convert.ToString(dataReader["Description"]);
                        eventReview.private_comment = Convert.ToString(dataReader["PrivateComment"]);
                        eventReview.date_of_review = Convert.ToDateTime(dataReader["DateOfReview"]);
                        eventReview.event_title = Convert.ToString(dataReader["EventTitle"]);
                        eventReview.user_first_name = Convert.ToString(dataReader["FirstName"]);
                        eventReview.user_last_name = Convert.ToString(dataReader["LastName"]);
                        eventReview.user_join_date = Convert.ToDateTime(dataReader["MemberSince"]);
                        eventReview.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        eventReviewModelList.Add(eventReview);
                    }
                }
            }
            return eventReviewModelList;
        }

        public List<EventReviewModel> GetTicketEventReviewsByEventId(int eventId)
        {
            var eventReviewModelList = new List<EventReviewModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", eventId));

            using (DbDataReader dataReader = GetDataReader("GetTicketEventReviewsByEventId", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var eventReview = new EventReviewModel();
                        eventReview.id = Convert.ToInt32(dataReader["Id"]);
                        eventReview.active = Convert.ToBoolean(dataReader["Active"]);
                        eventReview.winery_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        eventReview.metric1 = Convert.ToInt32(dataReader["Metric1"]);
                        eventReview.metric2 = Convert.ToInt32(dataReader["Metric2"]);
                        eventReview.metric3 = Convert.ToInt32(dataReader["Metric3"]);
                        eventReview.metric4 = Convert.ToInt32(dataReader["Metric4"]);
                        eventReview.metric5 = Convert.ToInt32(dataReader["Metric5"]);
                        eventReview.metric6 = Convert.ToInt32(dataReader["Metric6"]);
                        eventReview.description = Convert.ToString(dataReader["Description"]);
                        eventReview.private_comment = Convert.ToString(dataReader["PrivateComment"]);
                        eventReview.date_of_review = Convert.ToDateTime(dataReader["DateOfReview"]);
                        eventReview.event_title = Convert.ToString(dataReader["EventTitle"]);
                        eventReview.user_first_name = Convert.ToString(dataReader["FirstName"]);
                        eventReview.user_last_name = Convert.ToString(dataReader["LastName"]);
                        eventReview.user_join_date = Convert.ToDateTime(dataReader["MemberSince"]);
                        eventReview.purchase_url = Convert.ToString(dataReader["PurchaseURL"]);
                        eventReviewModelList.Add(eventReview);
                    }
                }
            }
            return eventReviewModelList;
        }

        public List<AdditionalTicketInfoModel> GetAdditionalTicketInfoByID(int Id)
        {
            var list = new List<AdditionalTicketInfoModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@ID", Id));

            using (DbDataReader dataReader = GetDataReader("GetAdditionalTicketInfoByID", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        var model = new AdditionalTicketInfoModel();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.TicketName = Convert.ToString(dataReader["TicketName"]);
                        model.ConfirmationMessage = Convert.ToString(dataReader["ConfirmationMessage"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<TicketEventDetail2Model> GetTicketEventDetail2ByWineryId(int wineryId, DateTime request_date)
        {
            string sp = "GetTicketEventDetail2ByWineryId";

            var eventS = new List<TicketEventDetail2Model>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@request_date", request_date));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tEvent = new TicketEventDetail2Model();
                        tEvent.id = Convert.ToInt32(dataReader["Id"]);
                        tEvent.name = Convert.ToString(dataReader["EventTitle"]);
                        tEvent.start_date = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tEvent.end_date = Convert.ToDateTime(dataReader["EndDateTime"]);
                        tEvent.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        tEvent.venue_country = Convert.ToString(dataReader["VenueCounty"]);
                        tEvent.venue_state = Convert.ToString(dataReader["VenueState"]);
                        tEvent.venue_zip = Convert.ToString(dataReader["VenueZip"]);
                        tEvent.event_image = Convert.ToString(dataReader["EventImage"]);
                        tEvent.event_image_full_path  = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        tEvent.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        tEvent.short_description = Convert.ToString(dataReader["ShortDesc"]);
                        string event_url = GetFriendlyURL(tEvent.name, tEvent.id);
                        tEvent.event_url = event_url;

                        eventS.Add(tEvent);
                    }
                }
            }
            return eventS;
        }

        public List<TicketEventDetail2Model> GetTicketEventDetail2ByRegionId(int wineryId, DateTime request_date)
        {
            string sp = "GetTicketEventDetail2ByRegionId";

            var eventS = new List<TicketEventDetail2Model>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", wineryId));
            parameterList.Add(GetParameter("@request_date", request_date));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tEvent = new TicketEventDetail2Model();
                        tEvent.id = Convert.ToInt32(dataReader["Id"]);
                        tEvent.name = Convert.ToString(dataReader["EventTitle"]);
                        tEvent.start_date = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tEvent.end_date = Convert.ToDateTime(dataReader["EndDateTime"]);
                        tEvent.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        tEvent.venue_country = Convert.ToString(dataReader["VenueCounty"]);
                        tEvent.venue_state = Convert.ToString(dataReader["VenueState"]);
                        tEvent.venue_zip = Convert.ToString(dataReader["VenueZip"]);
                        tEvent.event_image = Convert.ToString(dataReader["EventImage"]);
                        tEvent.event_image_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        tEvent.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        tEvent.short_description = Convert.ToString(dataReader["ShortDesc"]);
                        string event_url = GetFriendlyURL(tEvent.name, tEvent.id);
                        tEvent.event_url = event_url;

                        eventS.Add(tEvent);
                    }
                }
            }
            return eventS;
        }

        public List<TicketEventDetail2Model> GetTicketEventByRegion(string Str_Search, DateTime request_date)
        {
            string sp = "GetTicketEventByRegion";

            var eventS = new List<TicketEventDetail2Model>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@Str_Search", Str_Search));
            parameterList.Add(GetParameter("@request_date", request_date));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tEvent = new TicketEventDetail2Model();
                        tEvent.id = Convert.ToInt32(dataReader["Id"]);
                        tEvent.name = Convert.ToString(dataReader["EventTitle"]);
                        tEvent.start_date = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tEvent.end_date = Convert.ToDateTime(dataReader["EndDateTime"]);
                        tEvent.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        tEvent.venue_country = Convert.ToString(dataReader["VenueCounty"]);
                        tEvent.venue_state = Convert.ToString(dataReader["VenueState"]);
                        tEvent.venue_zip = Convert.ToString(dataReader["VenueZip"]);
                        tEvent.event_image = Convert.ToString(dataReader["EventImage"]);
                        tEvent.event_image_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        tEvent.event_organizer_name = Convert.ToString(dataReader["EventOrganizerName"]);
                        tEvent.short_description = Convert.ToString(dataReader["ShortDesc"]);
                        string event_url = GetFriendlyURL(tEvent.name, tEvent.id);
                        tEvent.event_url = event_url;

                        eventS.Add(tEvent);
                    }
                }
            }
            return eventS;
        }

        public bool CheckDisableTravelTimeRestrictionsByEventId(int EventId)
        {
            bool ret = false;

            string sql = "select DisableTravelTimeRestrictions from Tickets_Event where id = @EventId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = Convert.ToBoolean(dataReader["DisableTravelTimeRestrictions"]);
                    }
                }
            }
            return ret;
        }


        public List<TicketsFaqModel> GetTicketsFaqByEventId(int event_id)
        {
            string sp = "GetTicketsFAQ";

            var ticketsFaq = new List<TicketsFaqModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", event_id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tFaq = new TicketsFaqModel();
                        tFaq.id = Convert.ToInt32(dataReader["Id"]);
                        tFaq.winery_id = Convert.ToInt32(dataReader["WineryId"]);
                        tFaq.question = Convert.ToString(dataReader["Question"]);
                        tFaq.answer = Convert.ToString(dataReader["Answer"]);
                        ticketsFaq.Add(tFaq);
                    }
                }
            }
            return ticketsFaq;
        }

        public List<TicketsByEventExport> GetTicketsByEvent(int TicketEventId, int OffsetMinutes)
        {
            var list = new List<TicketsByEventExport>();
            var listQUESTION = new List<string>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", TicketEventId));
            parameterList.Add(GetParameter("@OffsetMinutes", OffsetMinutes));

            using (DbDataReader dataReader = GetDataReader("GetTicketsByEvent", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        listQUESTION.Add(Convert.ToString(dataReader["QUESTION"]));
                    }

                    if (dataReader.NextResult())
                    {
                        while (dataReader.Read())
                        {
                            var item = new TicketsByEventExport();
                            item.TicketId = Convert.ToInt32(dataReader["Ticket Id"]);
                            item.TicketLevel = Convert.ToString(dataReader["Ticket Level"]);
                            item.DeliveryMethod = Convert.ToString(dataReader["Delivery Method"]);
                            item.TicketStatus = Convert.ToString(dataReader["Ticket Status"]);
                            item.OrderDate = Convert.ToDateTime(dataReader["Order Date"]);
                            item.Order = Convert.ToInt32(dataReader["Order #"]);
                            item.TicketholderFirstName = Convert.ToString(dataReader["Ticketholder FirstName"]);
                            item.TicketholderLastName = Convert.ToString(dataReader["Ticketholder LastName"]);
                            item.TicketholderEmail = Convert.ToString(dataReader["Ticketholder Email"]);
                            item.TicketholderTitle = Convert.ToString(dataReader["Ticketholder Title"]);
                            item.TicketholderCompany = Convert.ToString(dataReader["Ticketholder Company"]);
                            item.TicketholderWPhone = Convert.ToString(dataReader["Ticketholder WPhone"]);
                            item.TicketholderMPhone = Convert.ToString(dataReader["Ticketholder MPhone"]);
                            item.TicketholderAddress1 = Convert.ToString(dataReader["Ticketholder Address1"]);
                            item.TicketholderAddress2 = Convert.ToString(dataReader["Ticketholder Address2"]);
                            item.TicketHolderCity = Convert.ToString(dataReader["TicketHolder City"]);
                            item.TicketHolderState = Convert.ToString(dataReader["TicketHolder State"]);
                            item.TicketHolderZip = Convert.ToString(dataReader["TicketHolder Zip"]);
                            item.TicketHolderCountry = Convert.ToString(dataReader["TicketHolder Country"]);
                            item.TicketHolderGender = Convert.ToString(dataReader["TicketHolder Gender"]);
                            item.TicketHolderAgeGroup = Convert.ToString(dataReader["TicketHolder Age Group"]);
                            item.TicketHolderAge = Convert.ToInt32(dataReader["TicketHolder Age"]);
                            item.TicketHolderDOB = Convert.ToString(dataReader["TicketHolder DOB"]);
                            item.TicketHolderWebsite = Convert.ToString(dataReader["TicketHolder Website"]);
                            item.Ticket = Convert.ToDecimal(dataReader["Ticket$"]);
                            item.Gratuity = Convert.ToDecimal(dataReader["Gratuity"]);
                            item.BuyerSvcFee = Convert.ToDecimal(dataReader["Buyer Svc Fee"]);
                            item.HostSvcFee = Convert.ToDecimal(dataReader["Host Svc Fee"]);
                            item.TotalSvcFee = Convert.ToDecimal(dataReader["Total Svc Fee"]);
                            item.SalesTax = Convert.ToDecimal(dataReader["Sales Tax"]);
                            item.TicketTotal = Convert.ToDecimal(dataReader["Ticket Total"]);
                            item.CCProcFee = Convert.ToDecimal(dataReader["CC Proc Fee"]);
                            item.PromoCode = Convert.ToString(dataReader["Promo Code"]);
                            item.AccessCode = Convert.ToString(dataReader["Access Code"]);
                            item.OrderNotes = Convert.ToString(dataReader["Order Notes"]);

                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }

        public List<TicketsByEventExport> GetPassportTicketsByEvent(int TicketEventId, int OffsetMinutes, int MemberId)
        {
            var list = new List<TicketsByEventExport>();

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", TicketEventId));
            parameterList.Add(GetParameter("@OffsetMinutes", OffsetMinutes));
            parameterList.Add(GetParameter("@MemberId", MemberId));

            using (DbDataReader dataReader = GetDataReader("GetPassportTicketsByEvent", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var item = new TicketsByEventExport();
                        item.TicketId = Convert.ToInt32(dataReader["Ticket Id"]);
                        item.TicketLevel = Convert.ToString(dataReader["Ticket Level"]);
                        item.DeliveryMethod = Convert.ToString(dataReader["Delivery Method"]);
                        item.TicketStatus = Convert.ToString(dataReader["Ticket Status"]);
                        item.OrderDate = Convert.ToDateTime(dataReader["Order Date"]);
                        item.Order = Convert.ToInt32(dataReader["Order #"]);
                        item.TicketholderFirstName = Convert.ToString(dataReader["Ticketholder FirstName"]);
                        item.TicketholderLastName = Convert.ToString(dataReader["Ticketholder LastName"]);
                        item.TicketholderEmail = Convert.ToString(dataReader["Ticketholder Email"]);
                        item.TicketholderTitle = Convert.ToString(dataReader["Ticketholder Title"]);
                        item.TicketholderCompany = Convert.ToString(dataReader["Ticketholder Company"]);
                        item.TicketholderWPhone = Convert.ToString(dataReader["Ticketholder WPhone"]);
                        item.TicketholderMPhone = Convert.ToString(dataReader["Ticketholder MPhone"]);
                        item.TicketholderAddress1 = Convert.ToString(dataReader["Ticketholder Address1"]);
                        item.TicketholderAddress2 = Convert.ToString(dataReader["Ticketholder Address2"]);
                        item.TicketHolderCity = Convert.ToString(dataReader["TicketHolder City"]);
                        item.TicketHolderState = Convert.ToString(dataReader["TicketHolder State"]);
                        item.TicketHolderZip = Convert.ToString(dataReader["TicketHolder Zip"]);
                        item.TicketHolderCountry = Convert.ToString(dataReader["TicketHolder Country"]);
                        item.TicketHolderGender = Convert.ToString(dataReader["TicketHolder Gender"]);
                        item.TicketHolderAgeGroup = Convert.ToString(dataReader["TicketHolder Age Group"]);
                        item.TicketHolderAge = Convert.ToInt32(dataReader["TicketHolder Age"]);
                        item.TicketHolderDOB = Convert.ToString(dataReader["TicketHolder DOB"]);
                        item.TicketHolderWebsite = Convert.ToString(dataReader["TicketHolder Website"]);
                        item.Ticket = Convert.ToDecimal(dataReader["Ticket$"]);
                        item.Gratuity = Convert.ToDecimal(dataReader["Gratuity"]);
                        item.BuyerSvcFee = Convert.ToDecimal(dataReader["Buyer Svc Fee"]);
                        item.HostSvcFee = Convert.ToDecimal(dataReader["Host Svc Fee"]);
                        item.TotalSvcFee = Convert.ToDecimal(dataReader["Total Svc Fee"]);
                        item.SalesTax = Convert.ToDecimal(dataReader["Sales Tax"]);
                        item.TicketTotal = Convert.ToDecimal(dataReader["Ticket Total"]);
                        item.CCProcFee = Convert.ToDecimal(dataReader["CC Proc Fee"]);
                        item.PromoCode = Convert.ToString(dataReader["Promo Code"]);
                        item.AccessCode = Convert.ToString(dataReader["Access Code"]);
                        item.OrderNotes = Convert.ToString(dataReader["Order Notes"]);

                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public List<ExportCheckInAttendees> GetTicketPassportCheckInUserHistory(int eventId, int wineryId)
        {
            string sp = "GetTicketPassportCheckInUserHistory";

            var list = new List<ExportCheckInAttendees>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@eventId", eventId));
            parameterList.Add(GetParameter("@wineryId", wineryId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ExportCheckInAttendees();
                        model.TicketId = Convert.ToInt32(dataReader["TicketId"]);
                        model.Email = Convert.ToString(dataReader["Email"]);
                        model.AccountType = Convert.ToString(dataReader["AccountType"]);
                        model.Address2 = Convert.ToString(dataReader["Address2"]);
                        model.Address1 = Convert.ToString(dataReader["Address1"]);
                        model.City = Convert.ToString(dataReader["City"]);
                        model.DateCheckedin = Convert.ToString(dataReader["DateCheckedin"]);
                        model.FirstName = Convert.ToString(dataReader["FirstName"]);
                        model.HomePhone = Convert.ToString(dataReader["HomePhone"]);
                        model.LastName = Convert.ToString(dataReader["LastName"]);
                        model.Location = Convert.ToString(dataReader["Location"]);
                        model.State = Convert.ToString(dataReader["State"]);
                        model.ZipCode = Convert.ToString(dataReader["ZipCode"]);
                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<ExportCheckInAttendees> GetTicketCheckInUserHistory(int eventId, int wineryId)
        {
            string sp = "GetTicketCheckInUserHistory";

            var list = new List<ExportCheckInAttendees>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@eventId", eventId));
            parameterList.Add(GetParameter("@wineryId", wineryId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ExportCheckInAttendees();
                        model.TicketId = Convert.ToInt32(dataReader["TicketId"]);
                        model.Email = Convert.ToString(dataReader["Email"]);
                        model.AccountType = Convert.ToString(dataReader["AccountType"]);
                        model.Address2 = Convert.ToString(dataReader["Address2"]);
                        model.Address1 = Convert.ToString(dataReader["Address1"]);
                        model.City = Convert.ToString(dataReader["City"]);
                        model.DateCheckedin = Convert.ToString(dataReader["DateCheckedin"]);
                        model.FirstName = Convert.ToString(dataReader["FirstName"]);
                        model.HomePhone = Convert.ToString(dataReader["HomePhone"]);
                        model.LastName = Convert.ToString(dataReader["LastName"]);
                        model.Location = Convert.ToString(dataReader["Location"]);
                        model.State = Convert.ToString(dataReader["State"]);
                        model.ZipCode = Convert.ToString(dataReader["ZipCode"]);
                        list.Add(model);
                    }
                }
            }
            return list;
        }

        public List<TicketOrderV2> GetTicketsByTimeAndkeyword(int winery_id, DateTime start_date, DateTime end_date, int offset_minutes, int mode, string keyword, string sort_by, int i_display_length, int i_display_start)
        {
            string sp = "GetTicketsByTimeAndkeyword";

            var list = new List<TicketOrderV2>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@WineryId", winery_id));
            parameterList.Add(GetParameter("@StartDate", start_date));
            parameterList.Add(GetParameter("@EndDate", end_date));
            parameterList.Add(GetParameter("@offsetMinutes", offset_minutes));
            parameterList.Add(GetParameter("@Mode", mode));
            parameterList.Add(GetParameter("@Keyword", keyword));
            parameterList.Add(GetParameter("@SortBy", sort_by));
            parameterList.Add(GetParameter("@iDisplayLength", i_display_length));
            parameterList.Add(GetParameter("@iDisplayStart", i_display_start));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new TicketOrderV2();
                        model.id = Convert.ToInt32(dataReader["Id"]);
                        model.order_guid = Convert.ToString(dataReader["OrderGUID"]);
                        model.order_status = Convert.ToInt32(dataReader["OrderStatus"]);
                        model.is_charge_back = Convert.ToBoolean(dataReader["isChargeback"]);
                        model.user_id = Convert.ToInt32(dataReader["UserId"]);
                        model.order_notes = Convert.ToString(dataReader["OrderNotes"]);
                        model.last_name = Convert.ToString(dataReader["LastName"]);
                        model.first_name = Convert.ToString(dataReader["FirstName"]);
                        model.event_id = Convert.ToInt32(dataReader["EventId"]);
                        model.event_title = Convert.ToString(dataReader["EventTitle"]);
                        model.order_date = Convert.ToDateTime(dataReader["OrderDate"]);
                        model.order_total = Convert.ToDecimal(dataReader["OrderTotal"]);
                        model.time_zone = Convert.ToInt32(dataReader["TimeZone"]);
                        model.ticket_fee = Convert.ToDecimal(dataReader["TicketFee"]);
                        model.svc_fees = Convert.ToDecimal(dataReader["SvcFees"]);
                        model.cc_fees = Convert.ToDecimal(dataReader["CcFees"]);
                        model.pay_type = Convert.ToInt32(dataReader["PayType"]);
                        model.ticket_count = Convert.ToInt32(dataReader["TicketCount"]);
                        model.promo_code = Convert.ToString(dataReader["PromoCode"]);
                        model.sales_tax = Convert.ToDecimal(dataReader["SalesTax"]);
                        model.visit_count = Convert.ToInt32(dataReader["VisitCount"]);
                        list.Add(model);
                    }
                }
                return list;
            }

            
        }

        public string CheckTicketEventInvite(int EventId, string Email)
        {
            string errorMsg = "";

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventId", EventId));
            parameterList.Add(GetParameter("@Email", Email));

            using (DbDataReader dataReader = GetDataReader("CheckTicketEventInvite", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        errorMsg = Convert.ToString(dataReader["errorMsg"]);
                    }
                }
            }

            return errorMsg;
        }

        public List<UserOrdersModel> GetUserOrders(int userId, DateTime? toDate, DateTime? fromDate, bool? isPastEvent = null, int? memberId = null)
        {
            string sp = "GetUserOrdersV3";

            var tictModel = new List<UserOrdersModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@fromDate", fromDate));
            parameterList.Add(GetParameter("@toDate", toDate));
            parameterList.Add(GetParameter("@wineryId", memberId));
            parameterList.Add(GetParameter("@isPastEvent", isPastEvent));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tict = new UserOrdersModel();
                        
                        tict.end_date_time= DateTimeOffset.Parse(Convert.ToString(dataReader["EndDateTime"]));
                        
                        tict.event_id = Convert.ToInt32(dataReader["EventId"]);
                        tict.event_title = Convert.ToString(dataReader["EventTitle"]);
                        tict.is_self_print = Convert.ToBoolean(dataReader["IsSelfPrint"]);
                        tict.member_name = Convert.ToString(dataReader["WineryName"]);
                        tict.no_of_tickets = Convert.ToInt32(dataReader["NoOfTickets"]);
                        tict.order_date = DateTimeOffset.Parse(Convert.ToString(dataReader["OrderDate"]));
                        tict.order_guid = Convert.ToString(dataReader["OrderGuid"]);
                        tict.order_id = Convert.ToInt32(dataReader["OrderId"]);
                        tict.order_total = Convert.ToDecimal(dataReader["OrderTotal"]);
                        tict.primary_category = Convert.ToInt32(dataReader["PrimaryCategory"]);
                        tict.purchase_url = Convert.ToString(dataReader["PurchaseUrl"]);
                        tict.start_date_time = DateTimeOffset.Parse(Convert.ToString(dataReader["StartDateTime"]));
                        tict.is_past_event = Convert.ToBoolean(dataReader["IsPastEvent"]);

                        tict.end_date_time_formated = tict.end_date_time.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                        tict.start_date_time_formated = tict.start_date_time.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                        tict.is_single_date = tict.start_date_time.Date == tict.end_date_time.Date;
                        tict.order_date_formated = tict.order_date.ToString("MMMM d, yyyy hh:mm tt", CultureInfo.InvariantCulture);
                        tict.order_total_text = tict.order_total.ToString("C", CultureInfo.GetCultureInfo("en-US"));

                        tictModel.Add(tict);
                    }
                }
            }
            return tictModel;
        }

        public List<UserTicketModel> GetUserTickets(string username, DateTime? toDate, DateTime? fromDate, bool? isPastEvent = null, int? memberId = null)
        {
            string sp = "GetUserTicketsV3";

            var tictModel = new List<UserTicketModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@ticketHolderEmail", username));
            parameterList.Add(GetParameter("@toDate", toDate));
            parameterList.Add(GetParameter("@fromDate", fromDate));
            parameterList.Add(GetParameter("@isPastEvent", isPastEvent));
            parameterList.Add(GetParameter("@wineryId", memberId));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tict = new UserTicketModel();

                        tict.event_title = Convert.ToString(dataReader["EventTitle"]);
                        tict.member_name = Convert.ToString(dataReader["WineryName"]);
                        tict.ticket_name = Convert.ToString(dataReader["TicketName"]);
                        tict.id = Convert.ToInt32(dataReader["Id"]);
                        tict.valid_start_date = Convert.ToDateTime(dataReader["ValidStartDate"]);
                        tict.delivery_type = Convert.ToString(dataReader["DeliveryType"]);
                        tict.is_past_event = Convert.ToBoolean(dataReader["IsPastEvent"]);
                        tict.star = Convert.ToInt32(dataReader["Star"]);
                        tict.event_id = Convert.ToInt32(dataReader["EventId"]);
                        tict.member_id = Convert.ToInt32(dataReader["WineryId"]);
                        tict.is_self_print = Convert.ToBoolean(dataReader["IsSelfPrint"]);
                        tict.event_end_date_time = Convert.ToDateTime(dataReader["EventEndDateTime"]);
                        tict.purchase_url = Convert.ToString(dataReader["PurchaseUrl"]);
                        tict.order_guid = Convert.ToString(dataReader["OrderGuid"]);
                        tict.primary_category = Convert.ToInt32(dataReader["PrimaryCategory"]);
                        tict.tickets_order_ticket_guid = Convert.ToString(dataReader["TicketsOrderTicketGUID"]);
                        tict.event_start_date_time = Convert.ToDateTime(dataReader["EventStartDateTime"]);

                        tict.valid_start_date_formated = tict.valid_start_date.ToString("dddd, MMM d", CultureInfo.InvariantCulture) + " at " + tict.valid_start_date.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                        tict.is_reviewed = tict.star > 0;
                        tict.is_single_date = tict.event_start_date_time.Date == tict.event_end_date_time;
                        tict.event_start_date_time_formated = tict.is_single_date ? tict.event_start_date_time.ToString("dddd, MMM d, yyyy", CultureInfo.InvariantCulture) + " " + tict.event_start_date_time.ToString("hh:mm tt", CultureInfo.InvariantCulture) : tict.event_start_date_time.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                        tict.event_end_date_time_formated = tict.event_end_date_time.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

                        tictModel.Add(tict);
                    }
                }
            }
            return tictModel;
        }

        public List<TicketEventByEventTypeModel> GetTicketeventsByEventType(string event_types, int? user_id)
        {
            string sp = "GetTicketeventsByEventType";

            var tictModel = new List<TicketEventByEventTypeModel>();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@EventType", event_types));
            parameterList.Add(GetParameter("@userID", user_id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var tict = new TicketEventByEventTypeModel();

                        tict.event_id = Convert.ToInt32(dataReader["EventId"]);
                        tict.event_title = Convert.ToString(dataReader["EventTitle"]);
                        tict.organizer_name = Convert.ToString(dataReader["OrganizerName"]);
                        tict.start_date_time = Convert.ToDateTime(dataReader["StartDateTime"]);
                        tict.end_date_time = Convert.ToDateTime(dataReader["EndDateTime"]);
                        tict.primary_category = Convert.ToString(dataReader["PrimaryCategory"]);
                        tict.secondary_category = Convert.ToString(dataReader["SecondaryCategory"]);
                        tict.event_image = Convert.ToString(dataReader["EventImage"]);
                        tict.event_image_big = Convert.ToString(dataReader["EventImageBig"]);

                        tict.event_image_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImage"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));
                        tict.event_image_big_full_path = string.Format("{1}/{0}", Convert.ToString(dataReader["EventImageBig"]), StringHelpers.GetImagePath(ImageType.ticketEventImage, ImagePathType.azure));

                        tict.venue_county = Convert.ToString(dataReader["VenueCounty"]);
                        tict.venue_city = Convert.ToString(dataReader["VenueCity"]);
                        tict.venue_state = Convert.ToString(dataReader["VenueState"]);
                        tict.location_id = Convert.ToInt32(dataReader["LocationId"]);
                        tict.state_name = Convert.ToString(dataReader["StateName"]);
                        tict.tickets_sold = Convert.ToInt32(dataReader["TicketsSold"]);
                        tict.max_capacity = Convert.ToInt32(dataReader["MaxCapacity"]);
                        tict.offer_business_id = Convert.ToInt32(dataReader["OfferId"]);
                        tict.offer_business_name = Convert.ToString(dataReader["OfferWineryName"]);
                        tict.is_favorites = Convert.ToBoolean(dataReader["IsFavorites"]);
                        tict.time_zone = Convert.ToString(dataReader["TimeZone"]);
                        tict.sold_out = Convert.ToBoolean(dataReader["SoldOut"]);

                        if (tict.start_date_time.Minute > 0)
                        {
                            tict.event_date = tict.start_date_time.ToString("dddd, MMM d, h:mmtt");
                        }
                        else
                        {
                            tict.event_date = tict.start_date_time.ToString("dddd, MMM d, htt");
                        }

                        tict.is_single_date = tict.start_date_time.Date == tict.end_date_time.Date;
                        tict.start_date_month = tict.start_date_time.ToString("MMM");
                        tict.start_date_date = tict.start_date_time.ToString("dd").TrimStart('0');
                        tict.end_date_month = tict.end_date_time.ToString("MMM");
                        tict.end_date_date = tict.end_date_time.ToString("dd").TrimStart('0');
                        tict.event_type = Convert.ToInt32(dataReader["Category"]);

                        tictModel.Add(tict);
                    }
                }
            }
            return tictModel;
        }

        public TicketReviewPostModel GetTicketReviewByTicketOrderTicketId(int ticket_order_ticket_id, int user_id)
        {
            string sp = "select metric1,description,event_id,TicketOrderTicket_Id,Winery_Id from EventReviews (nolock) where [user_id]=@user_id and TicketOrderTicket_Id=@ticket_order_ticket_id";

            var model = new TicketReviewPostModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@ticket_order_ticket_id", ticket_order_ticket_id));
            parameterList.Add(GetParameter("@user_id", user_id));

            using (DbDataReader dataReader = GetDataReader(sp, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.metric_1 = Convert.ToInt32(dataReader["metric1"]);
                        model.business_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        model.event_id = Convert.ToInt32(dataReader["event_id"]);
                        model.ticket_order_ticket_id = Convert.ToInt32(dataReader["TicketOrderTicket_Id"]);
                        model.description = Convert.ToString(dataReader["description"]);
                    }
                }
            }
            return model;
        }
    }
}

