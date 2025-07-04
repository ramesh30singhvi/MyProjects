using CPReservationApi.Common;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using static CPReservationApi.Common.Email;
using Settings = CPReservationApi.Common.Settings;

namespace CPReservationApi.WebApi.Services
{
    public class GoogleCalendar
    {
        public static bool CalendarAddEventV2(ReservationDetailModel rsvp)
        {
            bool ret = false;
            try
            {
                int addStatus = AddEventToGoogleCalendar(rsvp);
                if (addStatus == 602)
                {
                    addStatus = RefreshAccessTokenAndRetry(rsvp);
                }
                //if (addStatus == 602)
                //{
                //    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                //    SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                    //    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(rsvp.member_id, (int)Common.Common.SettingGroup.Google);

                    //    string clientId = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.GoogleClientID);
                    //    string clientSecret = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.GoogleClientSecret);
                    //    string refreshToken = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.GoogleRefreshToken);

                    //    var oauth1 = new Oauth();

                    //    oauth1.RuntimeLicense = "42474E3241414E58524638314544313433360000000000000000000000000000000000000000000048554A354638375600004659414A4352485A315653470000";
                    //    oauth1.ClientId = clientId;
                    //    oauth1.ClientSecret = clientSecret;
                    //    oauth1.ServerAuthURL = "https://accounts.google.com/o/oauth2/auth";
                    //    oauth1.ServerTokenURL = "https://accounts.google.com/o/oauth2/token";
                    //    oauth1.AuthorizationScope = "https://www.googleapis.com/auth/calendar";
                    //    oauth1.RefreshToken = refreshToken;

                    //    string Auth = oauth1.GetAuthorization();

                    //    if (!string.IsNullOrEmpty(Auth))
                    //    {
                    //        eventDAL.UpdateGoogleCalendarAuth(rsvp.member_id, Auth);

                    //        if (AddEventToGoogleCalendar(rsvp) == 1)
                    //        {
                    //            ret = true;
                    //        }
                    //    }
                    //}
            }
            catch (Exception ex)
            {
                ret = false;
            }

            return ret;
        }

        public static int RefreshAccessTokenAndRetry(ReservationDetailModel rsvp)
        {
            int ret = 0;
            string gUsername = "";
            string gPassword = "";
            string clientId = "";
            string secretKey = "";

            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            var winery = eventDAL.GetWineryById(rsvp.member_id);
            if (!(winery == null))
            {
                // Get Google Credentials
                if (winery.EnableGoogleSync)
                {
                    gUsername = winery.GoogleUsername;

                    SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(rsvp.member_id, (int)Common.Common.SettingGroup.Google).ToList();
                    if (settingsGroup != null && settingsGroup.Count > 0)
                    {
                        gPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.GoogleRefreshToken);
                    }

                    if (!string.IsNullOrWhiteSpace(gPassword))
                    {
                        logDAL.InsertLog("Google Calendar", String.Format("RefreshAccessTokenAndRetry:: rsvpId={0}, winery={1}, GuestEmail={2}", rsvp.reservation_id, rsvp.winery_name, (rsvp.user_detail != null ? rsvp.user_detail.email : "")), "", 3,rsvp.member_id);
                        //refresh the access token
                        var httpClient = new HttpClient
                        {
                            BaseAddress = new Uri("https://www.googleapis.com")
                        };


                        var requestUrl = $"oauth2/v4/token?client_id={clientId}&client_secret={secretKey}" +
                            $"&refresh_token={gPassword}&grant_type=refresh_token";
                        var dict = new Dictionary<string, string>
                        {
                            { "Content-Type", "application/x-www-form-urlencoded" }
                        };
                        var req = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, requestUrl) { Content = new FormUrlEncodedContent(dict) };
                        var response = httpClient.SendAsync(req).Result;
                        var jsonResp = response.Content.ReadAsStringAsync().Result;
                        logDAL.InsertLog("Google Calendar", String.Format("RefreshAccessTokenAndRetry:: Request sent:{0}, response from google{1}", requestUrl, jsonResp), "", 3, rsvp.member_id);

                        var token = JsonConvert.DeserializeObject<GmailToken>(jsonResp);

                        if (token != null && !string.IsNullOrWhiteSpace(token.AccessToken))
                        {
                            bool isSucess = settingsDAL.SaveSetting(new Settings.Setting
                            {
                                Group = Common.Common.SettingGroup.Google,
                                Key = Common.Common.SettingKey.GoogleCalendarAuth,
                                MemberId = rsvp.member_id,
                                Value = token.AccessToken
                            });

                            if (isSucess)
                            {
                                ret =  AddEventToGoogleCalendar(rsvp);
                            }
                        }
                        else
                        {
                            ret = 0;
                        }

                    }

                }

            }
            else
            {
                ret = 0;
            }

            return ret;
        }

        public static int AddEventToGoogleCalendar(ReservationDetailModel rsvp)
        {
            int status = 0;
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

            if (!(rsvp == null))
            {
                try
                {
                    logDAL.InsertLog("Google Calendar", String.Format("AddEventToGoogleCalendar:: rsvpId={0}, winery={1}, GuestEmail={2}", rsvp.reservation_id, rsvp.winery_name, (rsvp.user_detail != null ? rsvp.user_detail.email : "")), "", 3, rsvp.member_id);
                    bool isCalendarFeatureOn = false;
                    string gUsername = "";
                    string gPassword = "";

                    // Default Timezone
                    string timeZone = "America/Los_Angeles";
                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                    // Get Winery Details
                    var winery = eventDAL.GetWineryById(rsvp.member_id);
                    if (!(winery == null))
                    {
                        // Get Google Credentials
                        if (winery.EnableGoogleSync)
                        {
                            isCalendarFeatureOn = true;
                            gUsername = winery.GoogleUsername;

                            SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);
                            List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(rsvp.member_id, (int)Common.Common.SettingGroup.Google).ToList();
                            if (settingsGroup != null && settingsGroup.Count > 0)
                                gPassword = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.GoogleCalendarAuth);

                        }
                        // Set TimeZone
                        timeZone = Utility.GetGoogleTimeZoneNameById(winery.TimezoneId);
                    }
                    else
                    {
                        return 0;
                    }

                    // If credentials exist try to sync calendar with google
                    if (!string.IsNullOrEmpty(gUsername) & !string.IsNullOrEmpty(gPassword))
                    {
                        try
                        {
                            // Set Google Calendar
                            //var gcalendar1 = new Gcalendar();
                            var newEvent = new Google.Apis.Calendar.v3.Data.Event();
                            String calendarId = "primary";
                            //gcalendar1.RuntimeLicense = "42474E3241414E58524638314544313433360000000000000000000000000000000000000000000048554A354638375600004659414A4352485A315653470000";
                            //gcalendar1.CalendarId = gUsername;
                            GoogleCredential credential = GoogleCredential.FromAccessToken(gPassword)
                                .CreateScoped(new[] { CalendarService.Scope.Calendar, CalendarService.Scope.CalendarEvents });                            //gcalendar1.Authorization = gPassword; // "Bearer ya29.UgCxmttFH6oPbhwAAACGtZgYPHYQHYPwpV2yLkcMWVJHOR8I6D39z9-a37nxYA"

                            var calendarService = new CalendarService(new BaseClientService.Initializer()
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = "Google Calendar Integration"
                            });
                            // Not Cancel?
                            if (rsvp.status != (int)ReservationStatus.Cancelled)
                            {
                                DateTime EventStart = Times.ToUniversalTime(rsvp.event_start_date, (Times.TimeZone)winery.TimezoneId);

                                var gStartDate = new EventDateTime();
                                gStartDate.DateTime = EventStart;
                                gStartDate.TimeZone = "UTC";

                                newEvent.Start = gStartDate;

                                //gcalendar1.EventStartDate = gStartDate;
                                DateTime EventEnd = Times.ToUniversalTime(rsvp.event_end_date, (Times.TimeZone)winery.TimezoneId);
                                var gEndDate = new EventDateTime();
                                gEndDate.DateTime = EventEnd;
                                gEndDate.TimeZone = "UTC";
                                newEvent.End = gEndDate;
                                newEvent.Location = rsvp.location_name;
                                newEvent.Summary = string.Format("{0} - {1} - {2} {3}", rsvp.event_name, rsvp.user_detail.last_name, rsvp.total_guests, rsvp.total_guests > 1 ? "Guests" : "Guest");

                                // Status of RSVP
                                string rsvpStatus = Utility.GetGoogleStatusTextbyId(rsvp.status);

                                // Internal Note
                                string internalNotes = "";
                                if (!string.IsNullOrWhiteSpace(rsvp.internal_note))
                                {
                                    internalNotes = Regex.Replace(rsvp.internal_note.Replace("\"", ""), @"\r\n?|\n", " ");
                                }

                                // Guest Note
                                string guestNotes = "";
                                if (!string.IsNullOrWhiteSpace(rsvp.guest_note))
                                {
                                    guestNotes = Regex.Replace(rsvp.guest_note.Replace("\"", ""), @"\r\n?|\n", " ");
                                }

                                // Format Desc
                                string desc = "";
                                desc = "Status: " + rsvpStatus + @"\n";
                                desc += "Date Booked: " + rsvp.booking_date.Date + @"\n";
                                desc += "Confirmation Number: " + rsvp.booking_code + @"\n\n";
                                desc += "Reservation Holder: " + rsvp.user_detail.first_name + " " + rsvp.user_detail.last_name + @"\n";
                                desc += rsvp.user_detail.phone_number + @"\n";
                                desc += rsvp.user_detail.email + @"\n\n";

                                newEvent.Description = desc;

                                // Attendee
                                var gAttendee = new EventAttendee();
                                gAttendee.DisplayName = rsvp.user_detail.first_name + " " + rsvp.user_detail.last_name;
                                gAttendee.Email = rsvp.user_detail.email;
                                gAttendee.AdditionalGuests = rsvp.total_guests - 1;
                                gAttendee.ResponseStatus = "accepted";
                                gAttendee.Comment = guestNotes;
                                newEvent.Attendees = new List<EventAttendee>();
                                newEvent.Attendees.Add(gAttendee);
                                newEvent.Status = "confirmed";
                            }

                            bool CreateNew = false;

                            // If GoogleEvent exists update, else create
                            if (rsvp.google_calendar_event_url.Trim().Length > 0)
                            {
                                try
                                {
                                    // If Cancel Then Delete Event else Update
                                    if (rsvp.status == (int)ReservationStatus.Cancelled)
                                    {

                                        // Delete
                                        //newEvent.ETag = "*";
                                        //newEvent.Id = rsvp.google_calendar_event_url.Trim();
                                        //newEvent.DeleteEvent();
                                        status = 1;
                                        calendarService.Events.Delete(calendarId, rsvp.google_calendar_event_url.Trim()).Execute();
                                        eventDAL.ReservationV2StatusNote_Create(rsvp.reservation_id, rsvp.status, rsvp.member_id, "", false, 0, 0, 0, "SYNC - Reservation successfully removed from Google Calendar");
                                    }
                                    else
                                    {

                                        // Update
                                        newEvent.ETag = "*";
                                        newEvent.Id = rsvp.google_calendar_event_url.Trim();
                                        calendarService.Events.Update(newEvent, calendarId, rsvp.google_calendar_event_url.Trim()).Execute();
                                        status = 1;

                                        // Update RSVP Status Note
                                        eventDAL.ReservationV2StatusNote_Create(rsvp.reservation_id, rsvp.status, rsvp.member_id, "", false, 0, 0, 0, "SYNC - Reservation successfully updated in Google Calendar");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //if (ex is InGoogleException)
                                    //{
                                    //    var googleEx = new InGoogleException();
                                    //    googleEx = ex;
                                    //    if (googleEx.Code == 602)
                                    //    {
                                    status = 602;
                                    logDAL.InsertLog("Google Calendar", String.Format("AddEventToGoogleCalendar:: rsvpId={0}, winery={1}, GuestEmail={2}, Error:", rsvp.reservation_id, rsvp.winery_name, (rsvp.user_detail != null ? rsvp.user_detail.email : ""), ex.Message), "", 1, rsvp.member_id);
                                    //    }
                                    //}

                                    if ((ex.Message.Trim().ToLower() ?? "") == "no events found")
                                    {
                                        CreateNew = true;
                                    }
                                }
                            }
                            else
                            {
                                CreateNew = true;
                            }

                            if (CreateNew)
                            {

                                // Create
                                newEvent.ETag = "";

                                EventsResource.InsertRequest request = calendarService.Events.Insert(newEvent, calendarId);
                                var createdEvent = request.Execute();


                                // Save Calendar Event Id to CalendarURL property
                                rsvp.google_calendar_event_url = createdEvent.Id;

                                // Update Reservation
                                eventDAL.UpdateReservationGoogleCalendarEventURL(rsvp.reservation_id, rsvp.google_calendar_event_url);
                                status = 1;

                                // Update RSVP Status Note
                                eventDAL.ReservationV2StatusNote_Create(rsvp.reservation_id, rsvp.status, rsvp.member_id, "", false, 0, 0, 0, "SYNC - Reservation successfully added to Google Calendar");
                            }
                        }
                        catch (Exception ex)
                        {
                            logDAL.InsertLog("Google Calendar", String.Format("AddEventToGoogleCalendar:: rsvpId={0}, winery={1}, GuestEmail={2}, Error:", rsvp.reservation_id, rsvp.winery_name, (rsvp.user_detail != null ? rsvp.user_detail.email : ""), ex.Message), "", 1, rsvp.member_id);

                            // nsoftware.InGoogleWeb
                            //if (typeof ex is InGoogleException)
                            //{
                            //    var googleEx = new InGoogleException();
                            //    googleEx = ex1;
                            //    if (googleEx.Code == 602)
                            //    {
                            //        logDAL.InsertLog("GoogleCalendar", "AddEventToGoogleCalendar: error 602", "WebApi");
                            status = 602;
                            //    }
                            //}
                            //else
                            //{
                            //logDAL.InsertLog("GoogleCalendar", "NSoftware.CalendarAddEvent: " + ex.Message.ToString(), "WebApi");
                            //}
                        }
                    }
                    else
                    {
                        if (isCalendarFeatureOn == true)
                        {
                            logDAL.InsertLog("AddEventToGoogleCalendar", "ReservationId: " + rsvp.reservation_id.ToString() + " Google Username and/or Google Password not setup.", "WebApi",1, rsvp.member_id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logDAL.InsertLog("AddEventToGoogleCalendar", "ReservationId: " + rsvp.reservation_id.ToString() + "Message: " + ex.Message.ToString(), "WebApi",1, rsvp.member_id);
                }
            }

            return status;
        }


    }

    public class GmailToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
